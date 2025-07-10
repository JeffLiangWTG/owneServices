using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.GUI.Login
{
	public class OrgContactSupersededHelper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OrgContactSupersededHelper(OrgContact originalLoginContact) : base(originalLoginContact?.Factory ?? new BusinessObjectFactory())
		{
			OriginalLoginContact = originalLoginContact;
		}

		OrgContact OriginalLoginContact { get; }
		public bool HasValidContacts => OriginalLoginContact != null && OriginalLoginContact.WebAccessSuperseded && ContactsForLogin.Any();

		#region Contact Lists

		public OrgContactCollection ContactsForLogin
		{
			get
			{
				if (contactsForLogin == null && OriginalLoginContact != null)
				{
					var webContactsFilter = new ZQuery(OrgContactSchema.OC_PER, OriginalLoginContact.OC_PER);
					webContactsFilter.AddToFilter(OrgContactSchema.OC_IsActive, true);
					webContactsFilter.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, true);
					var lcontactsForLogin = new OrgContactCollection(Factory, webContactsFilter);
					lcontactsForLogin.Load();
					contactsForLogin = lcontactsForLogin;
				}

				return contactsForLogin;
			}
		}
		OrgContactCollection contactsForLogin;

		public OrgContactCollection ContactsForDeactivation
		{
			get
			{
				if (contactsForDeactivation == null && OriginalLoginContact != null)
				{
					var deactivationContactsFilter = new ZDBOnlyQuery(typeof(OrgContact));
					deactivationContactsFilter.AddToFilter(OrgContactSchema.OC_PER, OriginalLoginContact.OC_PER);
					deactivationContactsFilter.AddSubQuery(OrgContactSchema.PK, OrgContact.GetSupersededLogSubQuery(), JoinCondition.And);
					var lcontactsForDeactivation = new OrgContactCollection(Factory, deactivationContactsFilter);
					lcontactsForDeactivation.Load();
					contactsForDeactivation = lcontactsForDeactivation;
				}

				return contactsForDeactivation;
			}
		}
		OrgContactCollection contactsForDeactivation;

		#region Binding

		#region LoginContactList

		public List<LoginContact> LoginContacts
		{
			get
			{
				if (loginContacts == null)
				{
					loginContacts = new List<LoginContact>();

					if (OriginalLoginContact != null)
					{
						foreach (var contact in ContactsForLogin.Cast<OrgContact>())
						{
							loginContacts.Add(LoginContact.New(contact));
						}
					}
				}

				return loginContacts;
			}
		}

		List<LoginContact> loginContacts;

		#region Default Login Contact

		public LoginContact DefaultLoginContact => LoginContacts.OrderByDescending(x => x.PrimaryWorkplaceFlag).ThenByDescending(x => x.Email.EqualsIgnoringCase(OriginalLoginContact.OC_Email)).FirstOrDefault();

		#endregion

		#endregion

		#endregion

		#endregion

		#region GenerateSetMasterPasswordUrl

		public Uri GenerateSetMasterPasswordUrl(Uri passwordPageBaseUrl, Uri originalRequestUrl, string identityToken)
		{
			return SetMasterPasswordHelper.GenerateSetMasterPasswordUrl(OriginalLoginContact, passwordPageBaseUrl, originalRequestUrl, identityToken);
		}

		#endregion

		#region DeactivateRedirectionContacts

		public void DeactivateRedirectionContacts()
		{
			ContactsForDeactivation.Cast<OrgContact>().ForEach(x => x.CancelWebAccessSuperseded());
			Factory.Save();
		}

		#endregion
	}
}
