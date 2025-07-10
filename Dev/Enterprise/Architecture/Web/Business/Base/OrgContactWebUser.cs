using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
#if NET
using System.Text.Json.Serialization;
#endif
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business
{
	/// <summary>
	/// Class to describe a user who logs into the system.
	/// </summary>
	/// <remarks>
	/// MULTITHREADING
	/// ==============
	/// Instances of this class are stored in the session state and so
	/// can be accessed from multiple threads simultaneously.
	/// This happens if there are multiple requests for resources that
	/// only require a reader lock on the session state.
	/// Example resources that only get reader locks:
	/// - IHttpHandler implementations that implement IReadOnlySessionState.
	/// - pages with the EnableSessionState="ReadOnly".
	/// </remarks>
	public class OrgContactWebUser : WebUser
	{
		#region Overrides

		IOrgContactLoginAttemptRecorder LoginAttemptRecorder => loginAttemptRecorder ?? (loginAttemptRecorder = ObjectFactory.Get<IOrgContactLoginAttemptRecorder>());
		IOrgContactLoginAttemptRecorder loginAttemptRecorder;

		protected override void BeginLogin()
		{
			orgRights.Clear();
			organisationRelatedOrgPKs = null;
			organisationRelatedOrgAddressPKs = null;
			allUserRelatedOrgPKs = null;
			contactRelatedAccounts = null;
			IsLockedOut = false;
		}

		protected override IContactable LoginCore(BusinessObjectFactory factory, string companyCode, string username, string password, byte[] loginHash, bool shouldRecordLoginFailAttempt = true)
		{
			return LoginOrg(factory, null, companyCode, username, password, loginHash, shouldRecordLoginFailAttempt);
		}

		/// <summary>
		/// Login
		/// </summary>
		/// <param name="org">If org is given, then only contacts from that org are considered.</param>
		protected IContactable LoginOrg(BusinessObjectFactory factory, OrgHeader org, string companyCode, string username, string password, byte[] loginHash, bool shouldRecordLoginFailAttempt = true)
		{
			if (password != User.WebTransientPassword && LoginAttemptRecorder.IsLockedOut(companyCode, username, loginHash))
			{
				IsLockedOut = true;
				return null;
			}

			OrgContact result = null;
			allUserRelatedOrgsRef?.SetTarget(null);
			var passwordFailures = new OrgContactCollection(factory);
			var allUserRelatedContacts = GetActiveWebAccessContacts(factory, org, username);

			if (allUserRelatedContacts.Count > 0)
			{
				foreach (var contact in allUserRelatedContacts.Cast<OrgContact>())
				{
					if (RecordAndVerifyLogin(contact, password, passwordFailures, companyCode, loginHash))
					{
						result = contact;
						break;
					}
				}
			}

			if (result == null && ShouldIncludeSupersededContacts)
			{
				result = GetSupersededContacts(username, password, passwordFailures, companyCode, loginHash, true).FirstOrDefault();
			}

			if (result == null && shouldRecordLoginFailAttempt)
			{
				RecordLoginAttempt(companyCode, username, loginHash);
			}

			RecordUserRelatedOrgPKs(allUserRelatedContacts);

			return result;
		}

		public bool IsLockedOut { get; private set; }

		OrgContactCollection GetActiveWebAccessContacts(BusinessObjectFactory factory, OrgHeader org, string username)
		{
			var filter = new ZQuery(OrgContactSchema.OC_Email, SQLComparisonOperator.Equal, username);
			filter.AddToFilter(JoinCondition.And, OrgContactSchema.OC_WebAccessEnabled, ZBool.True);
			filter.AddToFilter(JoinCondition.And, OrgContactSchema.OC_IsActive, ZBool.True);

			if (org != null)
			{
				filter.AddToFilter(JoinCondition.And, OrgContactSchema.OC_OH, org.PK);
			}

			var allUserRelatedContacts = new OrgContactCollection(factory, filter);
			allUserRelatedContacts.Load();

			return allUserRelatedContacts;
		}

		protected virtual void ApplyAdditionalContactFilter(List<OrgContact> contacts)
		{
		}

		void RecordLoginAttempt(string companyCode, string username, byte[] loginHash)
		{
			LoginAttemptRecorder.RecordLoginAttempt(companyCode, username, loginHash);
		}

		void RecordUserRelatedOrgPKs(OrgContactCollection allUserRelatedContacts)
		{
			allUserRelatedOrgPKs = allUserRelatedContacts
				.Cast<OrgContact>()
				.Where(c => c.Header.OH_IsActive)
				.Select(c => c.Header.PK)
				.ToArray();
		}

		(LoginContactsResult, IContactable[]) GetMatchingContacts(BusinessObjectFactory factory, string companyCode, string username, string password, bool shouldIncludeSupersededContacts, byte[] loginHash, bool shouldRecordLoginFailAttempt)
		{
			var result = new List<IContactable>();
			allUserRelatedOrgsRef?.SetTarget(null);

			var passwordFailures = new OrgContactCollection(factory);
			var allUserRelatedContacts = GetActiveWebAccessContacts(factory, null, username);
			OrgContactCollection supersededContacts = null;

			if (shouldIncludeSupersededContacts)
			{
				supersededContacts = GetSupersededContacts(factory, username);
			}

			var isAnySupersededContacts = supersededContacts != null && supersededContacts.Count > 0;

			if (LoginAttemptRecorder.IsLockedOut(companyCode, username, loginHash))
			{
				IsLockedOut = true;
			}
			else if (allUserRelatedContacts.Count > 0 || isAnySupersededContacts)
			{
				var userRelatedContactsArray = isAnySupersededContacts ? allUserRelatedContacts.Cast<OrgContact>().Concat(supersededContacts.Cast<OrgContact>()).ToList() : allUserRelatedContacts.Cast<OrgContact>().ToList();
				ApplyAdditionalContactFilter(userRelatedContactsArray);

				var personsWithPasswords = new HashSet<ZGuid>();
				var passwordCount = 0;

				foreach (var contact in userRelatedContactsArray)
				{
					if (!personsWithPasswords.Contains(contact.OC_PER))
					{
						passwordCount += 1;

						if (contact.Person.HasPassword)
						{
							personsWithPasswords.Add(contact.OC_PER);
						}
					}
				}

				if (passwordCount > 5)
				{
					result.AddRange(userRelatedContactsArray);
					return (LoginContactsResult.TooManyContacts, result.ToArray());
				}

				var processedPersons = new HashSet<ZGuid>();

				foreach (var contact in userRelatedContactsArray)
				{
					if (processedPersons.Contains(contact.OC_PER))
					{
						result.Add(contact);
					}
					else if (RecordAndVerifyLogin(contact, password, passwordFailures, string.Empty, loginHash))
					{
						result.Add(contact);
						if (contact.Person.HasPassword)
						{
							processedPersons.Add(contact.OC_PER);
						}
					}
				}
			}
			else
			{
				// Introduce additional verify to mitigate potential time-based enumeration attacks.
				var filter = new ZQuery(OrgContactSchema.OC_WebAccessEnabled, SQLComparisonOperator.Equal, ZBool.True);
				filter.AddToFilter(JoinCondition.And, OrgContactSchema.OC_IsActive, ZBool.True);
				var contact = factory.LoadTop1<OrgContact>(filter);
				if (contact != null)
				{
					contact.VerifyPassword(password);
				}
			}

			if (!result.Any() && shouldRecordLoginFailAttempt)
			{
				RecordLoginAttempt(companyCode, username, loginHash);
			}

			RecordUserRelatedOrgPKs(allUserRelatedContacts);

			return (result.Any() ? LoginContactsResult.Success : LoginContactsResult.Failure, result.ToArray());
		}

		protected virtual bool RecordAndVerifyLogin(OrgContact contact, string password, OrgContactCollection passwordFailures, string companyCode, byte[] loginHash)
		{
			if (!string.IsNullOrEmpty(companyCode) && !contact.Header.OH_Code.EqualsIgnoringCase(companyCode))
			{
				return false;
			}

			if (!contact.Header.OH_IsActive)
			{
				return false;
			}

			if (password == User.WebTransientPassword)
			{
				return true;
			}

			if (contact.VerifyPassword(password))
			{
				return true;
			}

			// We should record a failure if we got this far
			passwordFailures.Add(contact);

			return false;
		}

		IEnumerable<OrgContact> GetSupersededContacts(string username, string password, OrgContactCollection passwordFailures, string companyCode, byte[] loginHash, bool shouldReturnFirstContact = false)
		{
			var allUserRelatedContacts = GetSupersededContacts(WebUserFactory?.Factory ?? new WebFactory().Factory, username);

			var supersededContacts =
				from contact
				in allUserRelatedContacts.Cast<OrgContact>()
				where RecordAndVerifyLogin(contact, password, passwordFailures, companyCode, loginHash)
				select contact;

			return shouldReturnFirstContact ? new[] { supersededContacts.FirstOrDefault() } : supersededContacts;
		}

		OrgContactCollection GetSupersededContacts(BusinessObjectFactory factory, string username)
		{
			var supersededContactsFilter = new ZDBOnlyQuery(typeof(OrgContact));
			supersededContactsFilter.AddToFilter(OrgContactSchema.OC_Email, SQLComparisonOperator.Equal, username);
			supersededContactsFilter.AddToFilter(JoinCondition.And, OrgContactSchema.OC_WebAccessEnabled, true);
			supersededContactsFilter.AddToFilter(JoinCondition.And, OrgContactSchema.OC_IsActive, false);
			var otherActiveContactsSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_PER);
			otherActiveContactsSubQuery.AddToFilter(JoinCondition.And, OrgContactSchema.OC_WebAccessEnabled, true);
			otherActiveContactsSubQuery.AddToFilter(JoinCondition.And, OrgContactSchema.OC_IsActive, true);
			supersededContactsFilter.AddSubQuery(OrgContactSchema.OC_PER, otherActiveContactsSubQuery, JoinCondition.And);
			supersededContactsFilter.AddSubQuery(OrgContactSchema.PK, OrgContact.GetSupersededLogSubQuery(), JoinCondition.And);

			var allUserRelatedContacts = new OrgContactCollection(factory, supersededContactsFilter);
			allUserRelatedContacts.Load();
			return allUserRelatedContacts;
		}

		public bool IsSpecialLogin(string username, string password)
		{
			if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
			{
				if (username == WebDataRegistry.Instance.WebServiceUsername.Value && password == WebDataRegistry.Instance.WebServicePassword.Value)
				{
					return true;
				}
				if (username == User.WebUserName && password == User.WebTransientPassword)
				{
					return true;
				}
				if (username.Equals(User.SupportUserName, StringComparison.OrdinalIgnoreCase) && CWSupportLoginToken.IsValidToken(password))
				{
					return true;
				}
			}

			return false;
		}

		public bool IsSpecialUser => IsSuperUser || LoggedInUserName.EqualsIgnoringCase(User.WebUserName) || LoggedInUserName.EqualsIgnoringCase(WebServicesUserName);

		public IContactable GetLoginContact(string companyCode, string username, string password, byte[] loginHash = null, byte[] loginNonce = null)
		{
			if (string.IsNullOrEmpty(username))
			{
				return null;
			}

			BeginLogin();

			var webFactory = new WebFactory();
			var factory = webFactory.Factory;

			ShouldIncludeSupersededContacts = true;
			var contact = LoginCore(factory, companyCode, username, password, loginHash);
			ShouldIncludeSupersededContacts = false;
			return contact;
		}

		public (LoginContactsResult, IContactable[]) GetLoginContacts(string companyCode, string username, string password, byte[] loginHash = null, bool shouldRecordLoginFailAttempt = true)
		{
			if (string.IsNullOrEmpty(username))
			{
				return (LoginContactsResult.Failure, Array.Empty<IContactable>());
			}

			BeginLogin();

			var webFactory = new WebFactory();
			var factory = webFactory.Factory;

			var contactsResultTuple = GetMatchingContacts(factory, companyCode, username, password, true, loginHash, shouldRecordLoginFailAttempt);
			return contactsResultTuple;
		}

		bool ShouldIncludeSupersededContacts { get; set; }

#if DEBUG
		public void OnSecurityRightsChangedForTest()
		{
			orgRights.Clear();
		}
#endif

		ZGuid[] GetGuids<T>(BusinessObjectFactory factory, ZQuery query) where T : BusinessObject
		{
			var guids = new List<ZGuid>();
			var bizOArray = factory.Load<T>(query);
			foreach (var bizO in bizOArray)
			{
				guids.Add(bizO.PK);
			}
			return guids.ToArray();
		}

		public static ZQuery GetOrgAddressQuery(ZGuid orgPK, ZGuid[] organisationRelatedOrgPKs)
		{
			var orgAddressSubQuery = new ZQuery();
			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, orgPK);
			orgAddressSubQuery.AddToFilter(JoinCondition.Or, OrgAddressSchema.OA_OH, organisationRelatedOrgPKs);
			return orgAddressSubQuery;
		}

		public static ZQuery GetRelatedOrgQuery(ZGuid orgPK)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgHeader));
			result.AddToFilter(OrgHeaderSchema.PK, orgPK);
			if (WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.Value)
			{
				ZDBOnlySubQuery relatedOrgSubQuery = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent);
				relatedOrgSubQuery.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, orgPK);

				ZQuery partyFilter = new ZQuery(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ControllingCustomer);
				partyFilter.AddToFilter(JoinCondition.Or, OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ManagementGrouping);
				relatedOrgSubQuery.AddToFilter(partyFilter, JoinCondition.And);

				ZQuery relatedOrgGlobalCompanySubQuery = new ZQuery();
				relatedOrgGlobalCompanySubQuery.AddToFilter(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);
				relatedOrgGlobalCompanySubQuery.AddToFilter(JoinCondition.Or, OrgRelatedPartySchema.PR_GC, null);
				relatedOrgSubQuery.AddToFilter(relatedOrgGlobalCompanySubQuery, JoinCondition.And);

				result.AddSubQuery(relatedOrgSubQuery, JoinCondition.Or);
			}

			return result;
		}

		protected override IContactable GetNewContactableForSpecialUser(BusinessObjectFactory factory, string affiliationCode, string username)
		{
			OrgContact result = null;

			OrgHeader orgHeader = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, affiliationCode);
			if (orgHeader != null)
			{
				result = orgHeader.Contacts.AddNew();
				result.SuspendValidation();
				result.OC_ContactName = username;
			}

			return result;
		}

		public override string AffiliationCode => LoggedInWebOrganisation?.OH_Code.ToString() ?? string.Empty;

		public override string AffiliationName => CompanyName;

		public override bool AreSecurityRightsGranted(WebSecurityRight securityRights)
		{
			return CurrentContactSecurityRightsAreGranted(securityRights);
		}

		bool CurrentContactSecurityRightsAreGranted(WebSecurityRight securityRights)
		{
			return IsSuperUser || (IsLoggedIn && IsRightGranted(securityRights));
		}

		#endregion

		#region Security rights

		readonly ConcurrentDictionary<string, bool> orgRights = new ConcurrentDictionary<string, bool>();
		protected bool IsRightGranted(WebSecurityRight securityRight)
		{
			var contact = LoggedInWebContact;
			if (contact == null)
			{
				return false;
			}

			return orgRights.GetOrAdd(securityRight.Code, key => IsRightGrantedWithoutCache(contact.Factory, contact.PK, contact.OC_OH, securityRight));
		}

		public static bool IsRightGrantedWithoutCache(BusinessObjectFactory factory, ZGuid contactPK, ZGuid contactOrgPK, WebSecurityRight right)
		{
			return WebSecurityHelper.IsRightGranted(factory, contactPK, contactOrgPK, right);
		}

		public static bool IsRightGrantedWithoutCache(WebSecurityRight right, OrgContact contact)
		{
			return contact.IsRightGranted(right);
		}

		#region CanAddNewOrganisations

		public bool CanAddNewOrganisations
		{
			get
			{
				return IsSuperUser ||
					(WebDataRegistry.Instance.AllowToAddNewOrganisation.Value &&
					IsRightGranted(WebSecurityRightsList.WebAddNewOrganisations));
			}
		}

		#endregion

		#region Publish Layouts

		public virtual bool CanPublishLayouts
		{
			get { return IsSuperUser || (IsLoggedIn && IsRightGranted(WebSecurityRightsList.WebPublishLayouts)); }
		}

		public virtual bool CanPublishCompanyLayouts
		{
			get
			{
				return IsSuperUser
					|| (IsLoggedIn
						&& IsRightGranted(WebSecurityRightsList.WebPublishLayouts)
						&& LoggedInOrganisation.IsProxyOrg(EnvProxy.Instance.CurrentCompany as GlbCompany));
			}
		}

		#endregion

		#endregion

		protected override IContactable LoggedInUserCore
		{
			get => webContact?.GetContact();
			set => webContact = (value != null) ? new WebContact((WebFactory)WebUserFactory, (OrgContact)value) : null;
		}

		/// <summary>
		/// Return value is an OrgContact for compatibility with lots of old code that assumes it is.
		/// However, new code should use LoggedInWebContact for common properties,
		/// or LoggedInOrgContact if an OrgContact is actually needed.
		/// </summary>
		public new OrgContact LoggedInUser => LoggedInOrgContact;

		public override IContactable LoggedInContactable => LoggedInWebContact;

		/// <summary>
		/// Lightweight contact that doesn't keep hold of a factory reference.
		/// Use in preference to LoggedInOrgContact / LoggedInUser for fast access to common properties.
		/// </summary>
		public IWebContact LoggedInWebContact => webContact;
#if NET
		[JsonInclude]
#endif
		WebContact webContact;

		public OrgContact LoggedInOrgContact => LoggedInWebContact?.GetContact();

		public IReadOnlyList<ZGuid> AllUserRelatedOrgPKs => allUserRelatedOrgPKs ?? Array.Empty<ZGuid>();

		public OrgHeaderCollection AllUserRelatedOrgs
		{
			get
			{
				OrgHeaderCollection result = null;

				lock (lockObject)
				{
					if (allUserRelatedOrgsRef == null)
					{
						allUserRelatedOrgsRef = new WeakReference<OrgHeaderCollection>(null);
					}
					else
					{
						allUserRelatedOrgsRef.TryGetTarget(out result);
					}

					if (result == null)
					{
						result = LoadAllUserRelatedOrgs();
						allUserRelatedOrgsRef.SetTarget(result);
					}
				}

				return result;
			}
		}
		WeakReference<OrgHeaderCollection> allUserRelatedOrgsRef;

		public OrgHeaderCollection LoadAllUserRelatedOrgs()
		{
			var factory = webContact?.Factory ?? new BusinessObjectFactory();
			var result = new OrgHeaderCollection(factory);
			if (allUserRelatedOrgPKs != null)
			{
				result.AddRange(factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, allUserRelatedOrgPKs)));
				result.Sort((OrgHeader x, OrgHeader y) => string.Compare(x.OH_FullName, y.OH_FullName, StringComparison.OrdinalIgnoreCase));
			}
			return result;
		}

		public ZGuid[] OrganisationRelatedOrgPKs
		{
			get
			{
				if (organisationRelatedOrgPKs == null)
				{
					lock (lockObject)
					{
						if (organisationRelatedOrgPKs == null)
						{
							organisationRelatedOrgPKs = GetGuids<OrgHeader>(new BusinessObjectFactory(), GetRelatedOrgQuery(CurrentOrg));
						}
					}
				}

				return organisationRelatedOrgPKs;
			}
		}

		public ZGuid[] OrganisationRelatedOrgAddressPKs
		{
			get
			{
				if (organisationRelatedOrgAddressPKs == null)
				{
					lock (lockObject)
					{
						if (organisationRelatedOrgAddressPKs == null)
						{
							organisationRelatedOrgAddressPKs = GetGuids<OrgAddress>(new BusinessObjectFactory(), GetOrgAddressQuery(CurrentOrg, OrganisationRelatedOrgPKs));
						}
					}
				}

				return organisationRelatedOrgAddressPKs;
			}
		}

		public OrgContactCollection ContactRelatedAccounts
		{
			get
			{
				if (contactRelatedAccounts == null && LoggedInOrgContact != null)
				{
					var contactQuery = new ZDBOnlyQuery(typeof(OrgContact));
					contactQuery.AddToFilter(OrgContactSchema.OC_IsActive, ZBool.True);
					contactQuery.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, ZBool.True);
					var contactOrQuery = new ZQuery(OrgContactSchema.OC_PER, LoggedInOrgContact.OC_PER);
					contactOrQuery.AddToFilter(JoinCondition.Or, OrgContactSchema.OC_Email, LoggedInOrgContact.OC_Email);
					contactQuery.AddToFilter(contactOrQuery);
					var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
					orgSubQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, ZBool.True);
					contactQuery.AddSubQuery(OrgContactSchema.OC_OH, orgSubQuery, JoinCondition.And);

					contactRelatedAccounts = new OrgContactCollection(webContact?.Factory ?? new BusinessObjectFactory(), contactQuery);
					contactRelatedAccounts.Load();
				}

				return contactRelatedAccounts;
			}
		}
		OrgContactCollection contactRelatedAccounts;

		/// <summary>
		/// Set contract signed date.
		/// Does nothing if IsSuperUser.
		/// </summary>
		public void SetUserWebContractSignedAndSaveToDb()
		{
			var user = (WebContact)LoggedInWebContact;
			if (user != null && !IsSuperUser)
			{
				user.SetUserWebContractSignedAndSaveToDb();
			}
		}

		public ZGuid CurrentOrg => LoggedInWebContact?.OC_OH ?? ZGuid.Invalid;

		/// <summary>
		/// Use LoggedInWebOrganisation for access to common properties.
		/// </summary>
		public OrgHeader LoggedInOrganisation => LoggedInWebOrganisation?.GetHeader();

		public IWebOrg LoggedInWebOrganisation => LoggedInWebContact?.ParentWebOrg;

		public ZString CompanyName => LoggedInWebOrganisation?.OH_FullNameTruncated ?? ZString.Empty;

		public ZString ContactAndCompanyReference
		{
			get
			{
				ZString result = "";
				var user = LoggedInWebContact;
				if (user != null)
				{
					result = user.Email;
					if (result.IsEmpty)
					{
						result = LoggedInUserName;
					}
					result += string.Format(CultureInfo.InvariantCulture, " ({0})", LoggedInWebOrganisation.OH_Code);
				}
				return result;
			}
		}

		public virtual bool CanViewDocument(BusinessObjectFactory factory, RefDocType docType)
		{
			return true;
		}

		readonly object lockObject = new object();
		ZGuid[] organisationRelatedOrgPKs;
		ZGuid[] organisationRelatedOrgAddressPKs;
		ZGuid[] allUserRelatedOrgPKs;

		protected override ZGuid GetBranchPKForLogin()
		{
			GlbBranch branchForLogin = null;
			var contact = LoggedInWebContact;
			if (contact != null)
			{
				var contactOrg = contact.ParentWebOrg.GetHeader();
				branchForLogin = contactOrg.GetBranchForLogin();
			}

			return branchForLogin?.PK ?? ZGuid.Empty;
		}
	}

	public static class ExtensionMethod
	{
		public static ZString GetCountryCode(this OrgContactWebUser currentUser)
		{
			var webContact = (WebContact)currentUser.LoggedInWebContact;
			if (webContact != null)
			{
				var branchAddress = webContact.GetContact()?.BranchAddress;
				if (branchAddress != null)
				{
					return branchAddress.RelatedCountry.Code;
				}
				else
				{
					var webOrg = (WebOrg)currentUser.LoggedInWebOrganisation;
					return webOrg.BranchOrOrgCountryCode;
				}
			}
			return ZString.Empty;
		}
	}

	public enum LoginContactsResult
	{
		Success,
		Failure,
		TooManyContacts
	}
}
