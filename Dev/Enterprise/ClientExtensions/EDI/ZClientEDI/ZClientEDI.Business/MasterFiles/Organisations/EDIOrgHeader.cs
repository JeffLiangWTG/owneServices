using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.eRouter.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Licensing;
using Enterprise.MasterData.Common.Deduplication.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgHeader : OrgHeader, IEDIOrgHeader
	{
		public EDIOrgHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoOrgHeader.Schema
		{
			public const string LicenceEnterpriseCode = "LicenceEnterpriseCode";
			public const string LicenceEnterpriseID = "LicenceEnterpriseID";
		}

		#region Delete

		public override void Delete()
		{
			Factory.Load<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_OH, this.PK)).DeleteAll();

			if (LicCompany != null)
			{
				LicCompany.Delete();
			}

			base.Delete();
		}

		#endregion

		#region Validation

		protected override OrgHeaderValidation GetNewValidation()
		{
			return new EDIOrgHeaderValidation(this);
		}

		public new EDIOrgHeaderValidation Validation
		{
			get { return (EDIOrgHeaderValidation)base.Validation; }
		}

		#endregion

		#region Licence Code Generation

		public void GenerateNewLicenceCode()
		{
			if (!OH_Code.IsEmpty)
			{
				var collection = new LicenceEnterpriseCollection(new BusinessObjectFactory());
				collection.Load();
				var existingCodes = new HashSet<ZString>(collection.OfType<LicenceEnterprise>().Select(x => x.LE_EnterpriseCode).Distinct());

				if (!existingCodes.Contains(OH_Code.SubstringSafe(0, 3)))
				{
					LicenceEnterpriseCode = OH_Code.SubstringSafe(0, 3);
					return;
				}

				ZString code = "";
				for (int i = 0; i < 10; i++)
				{
					code = OH_Code.SubstringSafe(0, 2) + i.ToString(CultureInfo.InvariantCulture);
					if (!existingCodes.Contains(code))
					{
						LicenceEnterpriseCode = code;
						return;
					}
				}

				for (int i = 10; i < 100; i++)
				{
					code = OH_Code.SubstringSafe(0, 1) + i.ToString(CultureInfo.InvariantCulture);
					if (!existingCodes.Contains(code))
					{
						LicenceEnterpriseCode = code;
						return;
					}
				}

				foreach (var char1 in CharactersToUse)
				{
					foreach (var char2 in CharactersToUse)
					{
						foreach (var char3 in CharactersToUse)
						{
							code = string.Concat(char1, char2, char3);
							if (!existingCodes.Contains(code))
							{
								LicenceEnterpriseCode = code;
								return;
							}
						}
					}
				}

				LicenceEnterpriseCodeGenerationFailure = true;
				try
				{
					Validation.ValidateLicenceEnterpriseCode();
				}
				finally
				{
					LicenceEnterpriseCodeGenerationFailure = false;
				}
			}
		}

		const string CharactersToUse = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

		internal bool LicenceEnterpriseCodeGenerationFailure { get; set; }

		void DeleteLicenceEnterpriseIfChanged()
		{
			if (LicEnterprise != null)
			{
				if (!LastSavedLicenceEnterprisePK.IsEmpty && LastSavedLicenceEnterprisePK != LicEnterprise.PK)
				{
					var licenceToDelete = Factory.Load<LicenceEnterprise>(LastSavedLicenceEnterprisePK);
					if (licenceToDelete != null)
					{
						licenceToDelete.Delete();
					}
				}
			}
		}

		#endregion

		#region Related Business Objects

		#region Contacts

		public new EDIFilteredContactsCollection FilteredContacts => (EDIFilteredContactsCollection)base.FilteredContacts;

		protected override FilteredContactsCollection CreateFilteredContactsCollection()
		{
			return new EDIFilteredContactsCollection(Contacts);
		}

		#endregion

		#region OrgMiscServ

		public new EDIOrgMiscServ MiscServ
		{
			get { return (EDIOrgMiscServ)base.MiscServ; }
		}

		protected override Type MiscServType
		{
			get { return typeof(EDIOrgMiscServ); }
		}

		#endregion

		#region OrgCompanyData

		public new EDIOrgCompanyData CompanyData
		{
			get { return (EDIOrgCompanyData)base.CompanyData; }
		}

		#endregion

		#region Licence Enterprise

		public LicenceEnterprise LicEnterprise
		{
			get
			{
				if ((fLicEnterprise == null || fLicEnterprise.IsDeleted) && LicCompany != null)
				{
					ReloadLicEnterprise();
				}
				return fLicEnterprise;
			}
		}

		public void ReloadLicEnterprise()
		{
			fLicEnterprise = Factory.Load<LicenceEnterprise>(LicCompany.LC_LE);
			RegisterEditableChildObject(fLicEnterprise);
		}

		LicenceEnterprise fLicEnterprise;

		#endregion

		#region Licence Company

		public LicenceCompany LicCompany
		{
			get
			{
				if (fLicCompany == null || (!IsDeleted && fLicCompany.IsDeleted))
				{
					fLicCompany = Factory.LoadTop1<LicenceCompany>(new ZQuery(LicenceCompanySchema.LC_OH, this.PK));
					if (fLicCompany != null)
					{
						RegisterEditableChildObject(fLicCompany);
						RegisterListChangedCalledRefreshBinding(fLicCompany);
					}
				}
				return fLicCompany;
			}
		}

		LicenceCompany fLicCompany;

		#endregion

		#region Changing

		public void MarkForSaving()
		{
			if (!((IBusinessObjectState)this).HasChangesNotIncludingChildren)
			{
				IsMarkedForSaving = true;
			}
		}

		public bool IsMarkedForSaving { get; private set; }

		public const string OrgForcedSaveCode = "~FS";

		void DoForceUpdateIfNeeded()
		{
			if (IsMarkedForSaving)
			{
				try
				{
					if (!((IBusinessObjectState)this).HasChangesNotIncludingChildren)
					{
						var sql = $@"
UPDATE {OrgHeaderSchema.Constants.SqlSchemaName}.{OrgHeaderSchema.Constants.TableName}
SET {OrgHeaderSchema.Constants.OH_SystemLastEditUser} = '{OrgForcedSaveCode}'
WHERE {OrgHeaderSchema.Constants.PK} = @OrgPk";

						using (var command = ((IDbConnected)Factory).Connection.Command(sql))
						{
							command.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, PK.ToGuid());
							command.ExecuteNonQuery();
						}
					}
				}
				finally
				{
					IsMarkedForSaving = false;
				}
			}
		}

		#endregion

		#region Enterprise Companies

		LicenceCompany[] EnterpriseCompaniesList
		{
			get
			{
				LicenceCompany[] result = Array.Empty<LicenceCompany>();
				if (LicEnterprise != null)
				{
					ZQuery referencingFilter = new ZQuery(LicenceCompanySchema.LC_LE, SQLComparisonOperator.Equal, LicEnterprise.PK);
					result = Factory.Load<LicenceCompany>(referencingFilter);
				}

				return result;
			}
		}

		#endregion

		#region Projects

		public ProjectCollection Projects
		{
			get
			{
				if (projects == null)
				{
					ZDBOnlySubQuery orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
					orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, PK);

					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(EDIProject));
					query.AddSubQuery(WorkProjectSchema.WKP_OA_ClientAddress, orgAddressQuery, JoinCondition.And);

					var localProjects = new ProjectCollection(Factory, query);
					localProjects.Load();
					projects = localProjects;
				}
				return projects;
			}
		}

		ProjectCollection projects;

		#endregion

		#region Contracting Party

		public OrgHeader ContractingParty
		{
			get { return base.GetRelatedParty(EDIOrgRelatedPartyLookups.ContractingPartyCode, ZString.Empty); }
		}

		#endregion

		#region Translogix related party

		const string TranslogixRelatedPartyCompanyCode = "TRACUSSYD1";

		bool IsHavingTranslogixRelatedParty()
		{
			ZQuery query = new ZQuery();
			ZDBOnlyQuery orgRelatedPartyQuery = new ZDBOnlyQuery(typeof(OrgRelatedParty));
			orgRelatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, PK);
			ZDBOnlySubQuery orgQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgRelatedPartySchema.PR_OH_RelatedParty);
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, TranslogixRelatedPartyCompanyCode);

			orgRelatedPartyQuery.AddSubQuery(orgQuery, JoinCondition.And);
			query.AddToFilter(orgRelatedPartyQuery);

			OrgRelatedParty relatedParty = Factory.LoadTop1<OrgRelatedParty>(query);
			return relatedParty != null;
		}

		#endregion

		#region Memberships

		[ChildEditable(true)]
		public EdiOrgMembershipCollection Memberships
		{
			get
			{
				if (memberships == null)
				{
					memberships = new EdiOrgMembershipCollection(this);
					RegisterEditableChildObject(memberships);
				}
				return memberships;
			}
		}
		EdiOrgMembershipCollection memberships;

		#endregion

		public eRouterEdiEnterpriseCommunicationCollection eRouterEdiEnterpriseCommunications
		{
			get { return feRouterEdiEnterpriseCommunications ?? (feRouterEdiEnterpriseCommunications = new eRouterEdiEnterpriseCommunicationCollection(this)); }
		}
		eRouterEdiEnterpriseCommunicationCollection feRouterEdiEnterpriseCommunications;

		#endregion

		#region Properties

		#region LastSavedLicenceEnterprisePK

		public ZGuid LastSavedLicenceEnterprisePK
		{
			get { return fLastSavedLicenceEnterprisePK; }
			set { fLastSavedLicenceEnterprisePK = value; }
		}

		ZGuid fLastSavedLicenceEnterprisePK;

		#endregion

		#region CompanyCode

		public ZString CompanyCode
		{
			get { return LicCompany?.LC_CompanyCode ?? ZString.Empty; }
		}

		#endregion

		#region ProductId

		public List<ZString> ProductId
		{
			get
			{
				var productIds = new List<ZString>();

				if (LicCompany != null)
				{
					foreach (LicenceDatabase db in LicCompany.LicDatabases)
					{
						productIds.Add(db.LD_Product);
					}
				}

				return productIds;
			}
		}

		#endregion

		#region LicenceEnterpriseCode

		[List("LicenceEnterpriseListForEntCodeFilter"), MaxLength(LicenceEnterprise.Schema.LE_EnterpriseCodeMaxLength)]
		public ZString LicenceEnterpriseCode
		{
			get { return LicEnterprise?.LE_EnterpriseCode ?? ZString.Empty; }
			set
			{
				var ent = LicEnterprise;
				if (ent != null && ent.LE_EnterpriseCode == value)
				{
					ValidateAndRefreshEnterpriseBinding();
					return;
				}

				CheckMaximumLength(LicenceEnterpriseCodeInfo, value);
				OnChangingEnterprise();

				var result = Factory.Load<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, value));
				if (result.Length == 1)
				{
					fLicEnterprise = result[0];
				}
				else
				{
					fLicEnterprise = Factory.New<LicenceEnterprise>();
					fLicEnterprise.LE_EnterpriseCode = value;
					fLicEnterprise.LE_OH = PK;
				}

				// Reassigning the company to a new enterprise code
				if (LicCompany != null)
				{
					LicCompany.LC_LE = fLicEnterprise.PK;
					LicCompany.HasChanges = true;
					LicCompany.LicDatabases.RemoveAll();
				}
				else
				{
					CreateLicenceCompany();
				}

				AllDatabasesInLicenceEnterpriseRequiresReload = true;

				RegisterEditableChildObject(fLicEnterprise);
				fLicEnterprise.HasChanges = true;
				ValidateAndRefreshEnterpriseBinding();
			}
		}

		void DeleteLicEnterpriseIfNotSaved()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ZQuery filter = new ZQuery(LicenceEnterpriseSchema.PK, SQLComparisonOperator.Equal, fLicEnterprise.PK);
			filter.AddToFilter(LicenceEnterpriseSchema.LE_OH, SQLComparisonOperator.Equal, PK);
			var licence = newFactory.LoadTop1<LicenceEnterprise>(filter);
			if (licence != null)
			{
				LastSavedLicenceEnterprisePK = fLicEnterprise.PK;
			}
			else
			{
				fLicEnterprise.Delete();
			}
		}

		public ZPropertyInfo LicenceEnterpriseCodeInfo
		{
			get { return GetZPropertyInfo(Schema.LicenceEnterpriseCode); }
		}

		protected bool LicenceEnterpriseCode_ReadOnly => LicenceEnterpriseID_ReadOnly;

		void OnChangingEnterprise()
		{
			if (fLicEnterprise != null)
			{
				if (EnterpriseCompaniesList.Length > 1)
				{
					if (LicEnterprise.LE_OH == PK)
					{
						var tempLicence = Factory.Load<LicenceEnterprise>(LicEnterprise.PK);

						// Set the new master company to the next company in the list by default
						if (tempLicence.LE_OH != EnterpriseCompaniesList[1].LC_OH)  //Hack: List not returning in expected order 
						{
							tempLicence.LE_OH = EnterpriseCompaniesList[1].LC_OH;
						}
						else
						{
							tempLicence.LE_OH = EnterpriseCompaniesList[0].LC_OH;
						}
					}
				}
				else
				{
					DeleteLicEnterpriseIfNotSaved();
				}
			}
		}

		void ValidateAndRefreshEnterpriseBinding()
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateLicenceEnterpriseCode();
			}
			LicenceEnterpriseCodeInfo.RefreshBinding();
			LicenceEnterpriseIDInfo.RefreshBinding();
		}

		[List("LicenceEnterpriseList"), MaxLength(LicenceEnterprise.Schema.LE_EnterpriseIDMaxLength)]
		public ZString LicenceEnterpriseID
		{
			get => LicEnterprise?.LE_EnterpriseID ?? ZString.Empty;
			set
			{
				CheckMaximumLength(LicenceEnterpriseIDInfo, value);

				//callback after F3
				if (LicenceEnterpriseID == value)
				{
					ValidateAndRefreshEnterpriseBinding();
					return;
				}

				OnChangingEnterprise();

				var enterprise = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseID, value));
				if (enterprise != null)
				{
					fLicEnterprise = enterprise;
				}
				else
				{
					fLicEnterprise = Factory.New<LicenceEnterprise>();
					fLicEnterprise.LE_OH = PK;
				}

				// Reassigning the company to a new enterprise
				if (LicCompany != null)
				{
					LicCompany.LC_LE = fLicEnterprise.PK;
					LicCompany.HasChanges = true;
					LicCompany.LicDatabases.RemoveAll();
				}

				AllDatabasesInLicenceEnterpriseRequiresReload = true;
				RegisterEditableChildObject(fLicEnterprise);
				fLicEnterprise.HasChanges = true;
				ValidateAndRefreshEnterpriseBinding();
			}
		}

		public ZPropertyInfo LicenceEnterpriseIDInfo
		{
			get { return GetZPropertyInfo(Schema.LicenceEnterpriseID); }
		}

		protected bool LicenceEnterpriseID_ReadOnly => LicCompany != null && LicCompany.IsInDatabase;

		#endregion

		#region LicenceTaxationRegNo

		public string LicenceTaxationRegNoType
		{
			get { return UNLOCO != null ? UNLOCO.Country.ConsumptionTaxRegistrationCode : ""; }
		}

		[MaxLength(50)]
		public ZString LicenceTaxationRegNo
		{
			get { return CustomsCodes?.GetCustomsRegNo(LicenceTaxationRegNoType, UNLOCO.Country) ?? ZString.Empty; }
		}

		public ZPropertyInfo LicenceTaxationRegNoInfo
		{
			get { return GetZPropertyInfo(nameof(LicenceTaxationRegNo)); }
		}

		#endregion

		#region LicenceBusinessRegNo

		public string LicenceBusinessRegNoType
		{
			get { return UNLOCO != null ? GlbCompany.LicenceBusinessRegNoType(UNLOCO.Country) : ""; }
		}

		[MaxLength(50)]
		public ZString LicenceBusinessRegNo
		{
			get { return CustomsCodes.GetCustomsRegNo(LicenceBusinessRegNoType, UNLOCO.Country); }
		}

		public ZPropertyInfo LicenceBusinessRegNoInfo
		{
			get { return GetZPropertyInfo(nameof(LicenceBusinessRegNo)); }
		}

		#endregion

		#region AllowAddingNewDatabases

		public bool AllowAddingNewDatabases
		{
			get { return LicEnterprise != null && LicEnterprise.IsInDatabase; }
		}

		#endregion

		#region Original Names and Codes

		public ZString OriginalNames
		{
			get
			{
				ZString result = ZString.Empty;
				if (BrandsOrRelatedNames.Count > 0)
				{
					IEnumerable<OrgBrandOrRelatedName> originalNamesCollection = BrandsOrRelatedNames.Cast<OrgBrandOrRelatedName>().Where(name => !IsOrgCode(name.P1_RelatedName));
					result = string.Join(", ", originalNamesCollection.Select(name => name.P1_RelatedName.ToString()).ToArray());
				}
				return result;
			}
		}

		public ZString OriginalCodes
		{
			get
			{
				ZString result = ZString.Empty;
				if (BrandsOrRelatedNames.Count > 0)
				{
					IEnumerable<OrgBrandOrRelatedName> originalCodesCollection = BrandsOrRelatedNames.Cast<OrgBrandOrRelatedName>().Where(name => IsOrgCode(name.P1_RelatedName));
					result = string.Join(", ", originalCodesCollection.Select(code => code.P1_RelatedName.ToString()).ToArray());
				}
				return result;
			}
		}

		bool IsOrgCode(ZString orgNameOrCode)
		{
			return Regex.IsMatch(orgNameOrCode, @"^[A-Z0-9]{3}[A-Z0-9]+$");
		}

		#endregion

		#region Ralationship Manager

		public ZString RelationshipManager
		{
			get
			{
				var assignments = new OrgStaffAssignmentsCollection(this);
				assignments.CompanySpecific = false;
				foreach (OrgStaffAssignments assignment in assignments)
				{
					if (assignment.O8_Role == "RM1")
					{
						return assignment.O8_GS_NKPersonResponsible;
					}
				}

				return ZString.Empty;
			}
		}

		#endregion

		#region Has Current Support Contract Or No Active Licence

		public bool HasCurrentSupportContractOrNoActiveLicence
		{
			get
			{
				bool result = true;
				if (LicCompany != null)
				{
					var activeLicences = LicCompany.LicHeadersForAllDatabases.Cast<LicenceHeader>().Where(header => header.LA_IsActive && header.Database.LD_IsActive);
					result = !activeLicences.Any() || activeLicences.Any(header => header.LA_ContractExpiryDate.IsEmpty || header.LA_ContractExpiryDate > ZDateTime.Today);
				}
				return result;
			}
		}

		public static bool HasCurrentSupport(Guid orgPk)
		{
			const string sql =
@"select top 1 LA_ContractExpiryDate
from dbo.LicenceCompany
join dbo.LicenceHeader on LA_LC = LC_PK and LA_IsActive = 1
join dbo.LicenceDatabase on LA_LD = LD_PK and LD_IsActive = 1
where LC_OH = @OrgPk
order by case when LA_ContractExpiryDate is null then 0 else 1 end, LA_ContractExpiryDate desc";

			bool result = true;
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, orgPk);
				using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
				{
					if (reader.Read())
					{
						var expiryDate = reader[0];
						result = expiryDate == DBNull.Value || (DateTime)expiryDate > ZDateTime.Today;
					}
				}
			}
			return result;
		}

		public static Guid[] GetOrgsWithCurrentSupport(Guid[] orgPks)
		{
			const string sql =
				@"
SELECT
	LC_OH,
	ExpiryDate =
		CASE
			WHEN MAX(CASE WHEN LA_ContractExpiryDate IS NULL THEN 1 ELSE 0 END) = 0
			THEN MAX(LA_ContractExpiryDate)
		END
FROM dbo.LicenceCompany
JOIN dbo.LicenceHeader ON LA_LC = LC_PK AND LA_IsActive = 1
JOIN dbo.LicenceDatabase ON LA_LD = LD_PK AND LD_IsActive = 1
WHERE LC_OH IN (SELECT Value FROM @OrgPks)
GROUP BY LC_OH";

			Dictionary<Guid, bool> resultDict;

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddTableValuedParameter("@OrgPks", TVPHelper.TVP_uniqueidentifier, orgPks);
				var cmdResult = DataUtils.GetDataTableFromCommand(cmd);
				resultDict = new Dictionary<Guid, bool>();
				var today = ZDateTime.Today;
				foreach (var row in cmdResult.Rows.Cast<DataRow>())
				{
					var expiryDate = row[1];
					var result = expiryDate == DBNull.Value || (DateTime)expiryDate > today;
					resultDict.Add((Guid)row[0], result);
				}

				orgPks.ForEach(x =>
				{
					if (!resultDict.ContainsKey(x))
					{
						resultDict.Add(x, true);
					}
				});
			}

			return resultDict.Where(x => x.Value).Select(x => x.Key).ToArray();
		}

		#endregion

		#region EnableLightValidationIfAvailable

		protected override bool EnableLightValidationIfAvailable => EnableLightValidationIfAvailableOverriden ?? base.EnableLightValidationIfAvailable;

		public bool? EnableLightValidationIfAvailableOverriden;

		#endregion

		#endregion

		#region Saving

		public SnapshotForEHubNativeOrg Snapshot { get; set; }

		protected override void OnFactorySaving()
		{
			DeleteLicenceEnterpriseIfChanged();
			ChangeLicenceEnterpriseIfAlreadySaved();
			base.OnFactorySaving();
			DoForceUpdateIfNeeded();
		}

		public override void OnSaving()
		{
			base.OnSaving();

			CheckForNameChange();
		}

		#region Check For Name Change

		void CheckForNameChange()
		{
			if (HasNameChanges)
			{
				string oldName = OH_FullNameInfo.OriginalValue.ToString();
				string oldCode = OH_CodeInfo.OriginalValue.ToString();
				if (!string.IsNullOrEmpty(oldName))
				{
					string reference = string.Format(CultureInfo.CurrentCulture, "Full Name was: {0}", oldName);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, reference);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

					if (!BrandsOrRelatedNames.Contains(oldName))
					{
						BrandsOrRelatedNames.AddNew().P1_RelatedName = oldName;
					}
					if (OH_CodeInfo.HasChanges && !BrandsOrRelatedNames.Contains(oldCode))
					{
						BrandsOrRelatedNames.AddNew().P1_RelatedName = oldCode;
					}

					if (!LicenceEnterpriseCode.IsEmpty)
					{
						SendNameChangeNotification(EDIDataRegistry.Instance.OrgNameChangeNotificationAddresses, ZString.Empty);
					}

					if (IsHavingTranslogixRelatedParty())
					{
						SendNameChangeNotification(EDIDataRegistry.Instance.TranslogixOrgNameChangeNotificationAddresses, new ZString("Translogix "));
					}
				}
			}
		}

		bool HasNameChanges
		{
			get
			{
				bool hasChanges = OH_FullNameInfo.HasChanges;
				if (hasChanges)
				{
					string[] oldNameWords = OH_FullNameInfo.OriginalValue.ToString().Split(new string[] { " ", "\t", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
					string[] newNameWords = OH_FullName.ToString().Split(new string[] { " ", "\t", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

					hasChanges = !NamesHaveSameWords(oldNameWords, newNameWords);
				}
				return hasChanges;
			}
		}

		bool NamesHaveSameWords(string[] oldNameWords, string[] newNameWords)
		{
			if (oldNameWords.Length != newNameWords.Length)
			{
				return false;
			}

			for (int i = 0; i < oldNameWords.Length; i++)
			{
				if (string.Compare(oldNameWords[i], newNameWords[i], true, CultureInfo.CurrentCulture) != 0)
				{
					return false;
				}
			}

			return true;
		}

		void SendNameChangeNotification(StringArrayRegistryItem notificationRecipients, ZString subjectPrefix)
		{
			if (notificationRecipients.Value.Length > 0)
			{
				HtmlEmailDef email = new HtmlEmailDef();
				string emailSubject = string.Format(CultureInfo.CurrentCulture, "Organization name change \"{0}\" -> \"{1}\" by {2} ({3})",
												OH_FullNameInfo.OriginalValue.ToString(),
												OH_FullNameTruncated,
												GlbStaff.CurrentUser.GS_FullName,
												GlbStaff.CurrentUser.GS_EmailAddress);
				email.Subject = !subjectPrefix.IsEmpty ? string.Concat(subjectPrefix, emailSubject) : emailSubject;
				email.FromAddress = SupportIncident.MailFromAddress;
				email.FromDisplayName = SupportIncident.MailFromName;

				string[] additionalRecipients = notificationRecipients.Value;

				foreach (string recipient in additionalRecipients)
				{
					if (!string.IsNullOrEmpty(recipient))
					{
						email.AddRecipientForUserCommunication(recipient);
					}
				}

				if (StaffAssignments.OverallSalesRepStaff != null && !StaffAssignments.OverallSalesRepStaff.GS_EmailAddress.IsEmpty)
				{
					email.AddRecipientForUserCommunication(StaffAssignments.OverallSalesRepStaff.GS_EmailAddress);
				}
				if (StaffAssignments.OverallAccountManagerStaff != null && !StaffAssignments.OverallAccountManagerStaff.GS_EmailAddress.IsEmpty)
				{
					email.AddRecipientForUserCommunication(StaffAssignments.OverallAccountManagerStaff.GS_EmailAddress);
				}

				string registryLocation = ((IRegistryItemInternals)notificationRecipients).Location;
				string footer = string.Format(CultureInfo.CurrentCulture, "This email is sent to the addresses in registry entries:</br>{0}</br>and the sales representative / account manager assigned to the organisation.",
					registryLocation);

				OrgTemplatedTextGenerator bodyTextGenarator = new OrgTemplatedTextGenerator(this);
				string bodyTemplate = IncidentConstants.GetTextFromResource("Enterprise.Client.EDI.MasterFiles.Organisations.Business.NameChangeEmailNotificationLetter.txt");
				email.Body = bodyTextGenarator.GenerateTemplatedText(
						bodyTemplate,
						HTMLLink,
						"", footer);
				Env.OutgoingMailManager.CreateAndSave(email);
			}
		}

		public ZString HTMLLink
		{
			get { return "<a href=\"" + ObjectFactory.Get<IShowEditFormUrlCreator>().CreateWithoutApplicationContext(ClientControllerRegistration.Organisations, PK) + "\">" + OH_Code + "</a>"; }
		}

		void ChangeLicenceEnterpriseIfAlreadySaved()
		{
			if (LicEnterprise != null && !LicEnterprise.LE_EnterpriseCode.IsEmpty)
			{
				var alreadySaved = new BusinessObjectFactory().LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, LicEnterprise.LE_EnterpriseCode));

				// If the newly specified LE is already in the DB, assign to it.
				if (alreadySaved != null && alreadySaved.PK != LicEnterprise.PK)
				{
					fLicEnterprise.Delete();
					fLicEnterprise = Factory.Load<LicenceEnterprise>(alreadySaved.PK);
					if (LicCompany != null && fLicEnterprise != null)
					{
						LicCompany.LC_LE = fLicEnterprise.PK;
					}
				}
			}
		}

		#endregion

		#endregion

		#region Lookups

		#region LicenceEnterpriseList

		public LicenceEnterpriseCollection LicenceEnterpriseList
		{
			get
			{
				if (fLicenceEnterpriseList == null)
				{
					fLicenceEnterpriseList = new LicenceEnterpriseCollection(Factory);
				}
				return fLicenceEnterpriseList;
			}
		}

		LicenceEnterpriseCollection fLicenceEnterpriseList;

		public LicenceEnterpriseCollectionForEntCodeFilter LicenceEnterpriseListForEntCodeFilter =>
				fLicenceEnterpriseListForEntCodeFilter ?? (fLicenceEnterpriseListForEntCodeFilter = new LicenceEnterpriseCollectionForEntCodeFilter(Factory));

		LicenceEnterpriseCollectionForEntCodeFilter fLicenceEnterpriseListForEntCodeFilter;

		#endregion

		#region AllDatabasesInLicenceEnterprise

		public LicenceDatabaseCollection AllDatabasesInLicenceEnterprise
		{
			get
			{
				if (fAllDatabasesInLicenceEnterprise == null || AllDatabasesInLicenceEnterpriseRequiresReload)
				{
					AllDatabasesInLicenceEnterpriseRequiresReload = false;
					if (LicCompany != null && LicCompany.LicEnterprise != null)
					{
						ZQuery filter = new ZQuery(LicenceDatabaseSchema.LD_LE, LicCompany.LC_LE);
						fAllDatabasesInLicenceEnterprise = new LicenceDatabaseCollection(LicCompany.LicEnterprise, filter);
					}
					else
					{
						fAllDatabasesInLicenceEnterprise = new LicenceDatabaseCollection(Factory);
					}
					fAllDatabasesInLicenceEnterprise.IsManagedForDataRefresh = true;
				}

				return fAllDatabasesInLicenceEnterprise;
			}
		}
		LicenceDatabaseCollection fAllDatabasesInLicenceEnterprise;
		bool AllDatabasesInLicenceEnterpriseRequiresReload;

		#endregion

		#endregion

		[List("FilterPartyTypeList")]
		public override ZString FilterPartyType
		{
			get
			{
				return base.FilterPartyType;
			}
			set
			{
				base.FilterPartyType = value;
			}
		}

		public CodeDescriptionPairList FilterPartyTypeList
		{
			get
			{
				if (filterPartyTypeList == null)
				{
					filterPartyTypeList = Lookups.FilterPartyTypeList;
					filterPartyTypeList.AddPair(EDIOrgRelatedPartyLookups.ContractingPartyCode, EDIOrgRelatedPartyLookups.ContractingPartyDescription);
					filterPartyTypeList.AddPair(EDIOrgRelatedPartyLookups.WARPConstant, "WARP Related Parties");
					filterPartyTypeList.AddPair(EDIOrgRelatedPartyLookups.ERequestVisibilityGroupCode, EDIOrgRelatedPartyLookups.ERequestVisibilityGroupDescription);
					filterPartyTypeList.Sort();
				}
				return filterPartyTypeList;
			}
		}
		CodeDescriptionPairList filterPartyTypeList;

		#region Create / Load Licence

		public void CreateAndLoadLicenceForOrg()
		{
			using (SuspendSettingHasChanges())
			{
				if (LicCompany == null)
				{
					CreateLicenceEnterprise();
					CreateLicenceCompany();
				}

				OnElementReset();
			}

			// Update fields which are not bound on the form, but could be changed by other values of the Org
			// Update LicCompany Country to match Org.UNLOCO.Country
			if (UNLOCO != null && LicCompany.LC_CompanyCountry != UNLOCO.RL_RN_NKCountryCode)
			{
				LicCompany.LC_CompanyCountry = UNLOCO.RL_RN_NKCountryCode;
			}
		}

		#region Sync Licence Company Country

		public bool ShouldSyncLicenceCompanyCountry
		{
			get { return UNLOCO != null && LicCompany != null && LicCompany.LC_CompanyCountry != UNLOCO.RL_RN_NKCountryCode; }
		}

		public ZString GetSyncLicenceCompanyCountryMessage()
		{
			var originalValue = LicCompany != null ? LicCompany.LC_CompanyCountry : ZString.Empty;
			var orgCountry = UNLOCO != null ? UNLOCO.RL_RN_NKCountryCode : ZString.Empty;
			return string.Format(CultureInfo.CurrentCulture, "The licence company country will be changed from '{0}' to '{1}' to match the country this organization locates. New licence key will be needed to update client system. Would you like to continue?", originalValue, orgCountry);
		}

		public void SyncLicenceCompanyCountry()
		{
			if (UNLOCO != null)
			{
				LicCompany.LC_CompanyCountry = UNLOCO.RL_RN_NKCountryCode;
				LicCompany.LC_RX_NKCurrency = UNLOCO.Country.RN_RX_NKLocalCurrency;
				LicCompany.LC_IsGSTRegistered = UNLOCO.Country.IsGSTRegistered;
			}
		}

		#endregion

		#region Create Licence Enterprise

		void CreateLicenceEnterprise()
		{
			fLicEnterprise = Factory.New<LicenceEnterprise>();
			using (fLicEnterprise.SuspendSettingHasChanges())
			{
				fLicEnterprise.LE_EnterpriseCode = ZString.Empty;
				fLicEnterprise.LE_OH = PK;
			}
			RegisterEditableChildObject(fLicEnterprise);
		}

		#endregion

		#region Create Licence Company

		internal void CreateLicenceCompany(LicenceEnterprise ent)
		{
			fLicCompany = Factory.New<LicenceCompany>();
			using (fLicCompany.SuspendSettingHasChanges())
			{
				fLicCompany.LC_OH = PK;
				fLicCompany.LC_LE = ent.PK;
				fLicCompany.LC_CompanyCode = OH_Code.Right(3);
				var unloco = UNLOCO;

				if (unloco != null)
				{
					fLicCompany.LC_CompanyCountry = unloco.RL_RN_NKCountryCode;
					fLicCompany.LC_RX_NKCurrency = unloco.Country.RN_RX_NKLocalCurrency;
					fLicCompany.LC_IsGSTRegistered = UNLOCO.Country.IsGSTRegistered;
					fLicCompany.LC_IsWHTRegistered = CompanyData.OB_ARWHTApplicable;
				}
			}

			RegisterEditableChildObject(fLicCompany);
		}

		void CreateLicenceCompany()
		{
			CreateLicenceCompany(LicEnterprise);
		}

		#endregion

		#endregion

		#region Logging

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				ArrayList objects = new ArrayList();

				objects.AddRange(base.BusinessObjectsWithRelatedEventsCore);
				objects.Add(LicEnterprise);
				objects.Add(LicCompany);
				if (LicCompany != null)
				{
					objects.AddRange(LicCompany.BusinessObjectsWithRelatedEvents);
				}

				return (BusinessObject[])objects.ToArray(typeof(BusinessObject));
			}
		}

		#endregion

		#region Licence And System Key Generation

		#region Generate Licence Key To File System

		public string GenerateLicenceKeyToFileSystem(string folderName, LicenceHeader licenceToGenerate)
		{
			return GenerateLicenceKeyToFileSystem(folderName, licenceToGenerate, true);
		}

		public string GenerateLicenceKeyToFileSystem(string folderName, LicenceHeader licenceToGenerate, bool logAndSave)
		{
			string keyFileName = GenerateKeyFileName(licenceToGenerate);
			string encryptedLicenceKey = GenerateLicenceKey(licenceToGenerate);
			SaveKeyToFile(folderName, keyFileName, encryptedLicenceKey);
			if (logAndSave)
			{
				RecordLicenceKeyGenerationEvent(licenceToGenerate.Database.LD_ServerCode);
				Factory.Save();
			}
			return folderName + Path.DirectorySeparatorChar + keyFileName;
		}

		void SaveKeyToFile(string folderName, string fileName, string key)
		{
			File.WriteAllText(Path.Combine(folderName, fileName), key);
		}

		string GenerateKeyFileName(LicenceHeader licenceToGenerate)
		{
			string result = "";

			if (CanGenerateLicenceKey(licenceToGenerate))
			{
				result = LicEnterprise.LE_EnterpriseCode + LicCompany.LC_CompanyCode + "-" + licenceToGenerate.Database.LD_ServerCode + LicenceKeyFileExtension;
			}

			return result;
		}

		#endregion

		#region Generate And Auto Deploy Licence Key

		public bool GenerateAndAutoDeployLicenceKey(LicenceHeader licenceToGenerate)
		{
			bool result = false;
			if (CanAutoDeployLicenceKey(licenceToGenerate))
			{
				new SystemUpdatePacketMailSender().Send(licenceToGenerate.Database, GetLicencePacket(licenceToGenerate));
				RecordLicenceKeyGenerationEvent(licenceToGenerate.Database.LD_ServerCode);
				Factory.Save();
				result = true;
			}

			return result;
		}

		SystemUpdatePacket GetLicencePacket(LicenceHeader licenceToGenerate)
		{
			SystemUpdatePacket packet = new SystemUpdatePacket();
			packet.EncryptedLicenceKey = GenerateLicenceKey(licenceToGenerate);
			return packet;
		}

		#endregion

		#region Generate Licence Key

		/// <summary>
		/// Generates a licence key for a specified database.
		/// </summary>
		/// <param name="DatabaseToGenerate"></param>
		/// <returns>The Encrypted Licence Key data</returns>
		public string GenerateLicenceKey(LicenceHeader licenceToGenerate)
		{
			string result = "";

			if (CanGenerateLicenceKey(licenceToGenerate))
			{
				result = licenceToGenerate.GenerateLicenceKey();
			}

			return result;
		}

		public bool CanGenerateLicenceKey(LicenceHeader licenceToGenerate)
		{
			return OrgIsInAllowedCountry() &&
				licenceToGenerate != null &&
				licenceToGenerate.Database != null &&
				licenceToGenerate.Modules.Count > 0;
		}

		#endregion

		#region CanAutoDeployLicenceKey

		public bool CanAutoDeployLicenceKey(LicenceHeader licenceToGenerate)
		{
			return CanGenerateLicenceKey(licenceToGenerate) && licenceToGenerate.Database.VersionIsDeployable && licenceToGenerate.Database.PublicEmailIsDeployable;
		}

		#endregion

		#region Org Is In Allowed Country

		public bool OrgIsInAllowedCountry()
		{
			return UNLOCO != null && RefCountry.IsSupportedForLicenceBuilder(UNLOCO.RL_RN_NKCountryCode);
		}

		#endregion

		#region Record Event Licence Key Generated

		void RecordLicenceKeyGenerationEvent(ZString serverCode)
		{
			Logs.AddNew(Events.DocumentSent, "Licence For " + LicenceEnterpriseCode + LicCompany.LC_CompanyCode + serverCode + " Generated");
		}

		#endregion

		public const string LicenceKeyFileExtension = ".key";

		#endregion

		#region EDI Org Security Provider

		protected override OrganisationSecurityProvider GetNewSecurityProvider()
		{
			return new EDIOrganisationSecurityProvider(this);
		}

		#endregion

		#region IReadOnlySecurity Members

		protected override bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool result = false;
			if (property.Name == Schema.LicenceEnterpriseCode || property.Name == Schema.LicenceEnterpriseID)
			{
				result = !EDISecurityCheckpoints.OrgLicenceModify.IsAllowed;
			}

			return result || base.GetReadOnlySecurity(property);
		}

		#endregion

		public ZInt LicenceDatabaseCount => LicCompany?.LicDatabases != null ? (ZInt)LicCompany.LicDatabases.Count : ZInt.Zero;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new EDIOrgHeaderFetchStrategy(this);

		public override bool IsWebSecuritySyncToNeoSupported => false;

		#region Staff Assignments

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public override OrgStaffAssignmentsCollection StaffAssignments
		{
			get
			{
				if (staffAssignments == null)
				{
					staffAssignments = new EDIOrgStaffAssignmentsCollection(this);
					staffAssignments.Load();
					RegisterEditableChildObject(staffAssignments);
					staffAssignments.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyDetailsStaffAssignmentsSecurity && !SecurityProvider.HasModifyDetailsStaffAssignmentsAnyRoleSecurity);
				}
				return staffAssignments;
			}
		}
		EDIOrgStaffAssignmentsCollection staffAssignments;

		protected override OrgStaffAssignmentsCollection GetStaffAssignments(GlbCompany company = null)
		{
			return new EDIOrgStaffAssignmentsCollection(this, company);
		}

		#endregion
	}
}
