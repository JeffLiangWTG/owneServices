using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Licensing.Billing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiUserAgreementAssignment : AutoEdiUserAgreementAssignment
	{
		public EdiUserAgreementAssignment(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		public LicenceEnterprise EnterpriseParent { get; set; }

		[List("Lookups.AgreementTypes")]
		[ReadOnlyMember(nameof(EAE_AgreementTypeReadOnly))]
		[ResourceStringData("EdiUserAgreementAssignment|Type", Caption = "Type", FullDescription = "Type Code")]
		public override ZString EAE_AgreementType
		{
			get => base.EAE_AgreementType;
			set
			{
				var oldAgreementType = EAE_AgreementType;
				base.EAE_AgreementType = value;
				if (oldAgreementType != value)
				{
					EAE_VariantCode = ZString.Empty;
					ReloadRelatedAgreementsAndAcceptanceLogs();
					Validation.ValidateEAE_ParentID();
					Validation.ValidateEAE_AllowOnlineAcceptance();
				}
			}
		}

		public bool EAE_AgreementTypeReadOnly => IsInDatabase;

		[List("Lookups.Variants")]
		[ReadOnlyMember(nameof(EAE_VariantCodeReadOnly))]
		[ResourceStringData("EdiUserAgreementAssignment|VariantCode", Caption = "Variant", FullDescription = "Assigned Variant")]
		public override ZString EAE_VariantCode
		{
			get => base.EAE_VariantCode;
			set
			{
				var oldVariant = EAE_VariantCode;
				base.EAE_VariantCode = value;
				if (oldVariant != base.EAE_VariantCode)
				{
					ReloadRelatedAgreementsAndAcceptanceLogs();
					Validation.ValidateEAE_AllowOnlineAcceptance();
				}
			}
		}

		public bool EAE_VariantCodeReadOnly => !EdiUserAgreementTypesMapper.IsVariantEnabled(EAE_AgreementType);

		[List("Lookups.ParentTypes")]
		public override ZString EAE_ParentTableCode
		{
			get => base.EAE_ParentTableCode;
			set
			{
				if (value != base.EAE_ParentTableCode)
				{
					base.EAE_ParentTableCode = value;

					if (value.EqualsIgnoringCase(LicenceEnterpriseSchema.Constants.Prefix))
					{
						EAE_OH_ClientAgreementOrg = ZGuid.Empty;
						EAE_ParentID = EnterpriseParent?.PK ?? ZGuid.Empty;
					}
					else if (value.EqualsIgnoringCase(LicenceDatabaseSchema.Constants.Prefix))
					{
						EAE_ParentID = ZGuid.Empty;

						if (EnterpriseParent != null)
						{
							EAE_OH_ClientAgreementOrg = EnterpriseParent.LE_OH;
						}
					}
				}
			}
		}

		[List("Lookups.ParentTypes")]
		[ResourceStringData("EdiUserAgreementAssignment|ParentTableCodeDescription", Caption = "Agreement Scope")]
		public ZString ParentTableCodeDescription
		{
			get => Lookups.ParentTypes.GetDescriptionFromCode(EAE_ParentTableCode);
			set
			{
				var code = Lookups.ParentTypes.GetCodeFromDescription(value);

				if (!string.IsNullOrEmpty(code))
				{
					EAE_ParentTableCode = code;
				}
			}
		}

		public ZWrappedPropertyInfo ParentTableCodeDescriptionInfo => GetWrappedZPropertyInfo(nameof(ParentTableCodeDescription), x => EAE_ParentTableCodeInfo);

		[ReadOnlyMember(nameof(EAE_ParentIDReadOnly))]
		[List("Lookups.LicenceDatabaseParents")]
		[ResourceStringData("EdiUserAgreementAssignment|EAE_ParentID", Caption = "Parent", FullDescription = "Assignment Parent")]
		public override ZGuid EAE_ParentID
		{
			get => base.EAE_ParentID;
			set => base.EAE_ParentID = value;
		}

		public bool EAE_ParentIDReadOnly => EAE_ParentTableCode.EqualsIgnoringCase(LicenceEnterpriseSchema.Constants.Prefix);

		#endregion

		#region Related Properties

		[ResourceStringData("EdiUserAgreementAssignment|TypeDescription", Caption = "Type Desc.", FullDescription = "Type Description")]
		public ZString EAE_AgreementTypeDescription => Lookups.AgreementTypes.GetDescriptionFromCode(EAE_AgreementType);

		public ZPropertyInfo EAE_AgreementTypeDescriptionInfo => GetZPropertyInfo(nameof(EAE_AgreementTypeDescription));

		[ResourceStringData("EdiUserAgreementAssignment|VariantDescription", Caption = "Variant Desc.", FullDescription = "Variant Description")]
		public ZString EAE_VariantCodeDescription => Lookups.Variants.GetDescriptionFromCode(EAE_VariantCode);

		public ZPropertyInfo EAE_VariantCodeDescriptionInfo => GetZPropertyInfo(nameof(EAE_VariantCodeDescription));

		[ResourceStringData("EdiUserAgreementAssignment|CurrentVersion", Caption = "Current Version")]
		public ZString CurrentVersion => FormattableString.Invariant($"{CurrentAgreement?.ERA_VersionNumber ?? 0}.{CurrentAgreement?.ERA_MinorVersion ?? 0}");

		public ZPropertyInfo CurrentVersionInfo => GetZPropertyInfo(nameof(CurrentVersion));

		[ResourceStringData("EdiUserAgreementAssignment|LastAcceptedDateUtc", Caption = "Acceptance Date (UTC)")]
		public ZDateTime LastAcceptedDateUtc => (AcceptanceLogs?.Count ?? 0) == 0 ? ZDateTime.Empty : AcceptanceLogs.Select(x => x.EUL_AcceptanceTimeUtc).Max();

		public ZPropertyInfo LastAcceptedDateUtcInfo => GetZPropertyInfo(nameof(LastAcceptedDateUtc));

		[ResourceStringData("EdiUserAgreementAssignment|EAE_AllowOnlineAcceptance", Caption = "Allow online click-through")]
		public override ZBool EAE_AllowOnlineAcceptance { get => base.EAE_AllowOnlineAcceptance; set => base.EAE_AllowOnlineAcceptance = value; }

		#endregion

		#region Related Entities

		public BusinessObject Parent
		{
			get
			{
				if (EAE_ParentID.IsEmpty || EAE_ParentTableCode.IsEmpty)
				{
					return null;
				}

				var parent = Factory.Load(EAE_ParentTableCode, EAE_ParentID);
				if (parent is not AutoLicenceEnterprise && parent is not AutoLicenceDatabase)
				{
					throw new InvalidOperationException("Currently, only LicenceEnterprise and LicenceDatabase can be the parent object of EDIUserAgreementAssignment.");
				}

				return parent;
			}

			set
			{
				if (value == null)
				{
					EAE_ParentTableCode = ZString.Empty;
					EAE_ParentID = ZGuid.Empty;
				}
				else if (value is not AutoLicenceEnterprise && value is not AutoLicenceDatabase)
				{
					throw new InvalidOperationException("Currently, only LicenceEnterprise and LicenceDatabase can be the parent object of EDIUserAgreementAssignment.");
				}
				else
				{
					EAE_ParentTableCode = value.TablePrefix;
					EAE_ParentID = value.PK;
				}

				ReloadRelatedAgreementsAndAcceptanceLogs();
			}
		}

		public EdiUserAgreement CurrentAgreement => RelatedAgreements.FirstOrDefault();

		public EdiUserAgreementCollection RelatedAgreements
		{
			get
			{
				if (relatedAgreements == null)
				{
					relatedAgreements = new EdiUserAgreementCollection(Factory);
					relatedAgreements.ApplySort(EdiUserAgreementSchema.Constants.ERA_EffectiveTimeUtc, System.ComponentModel.ListSortDirection.Descending);
					ReloadRelatedAgreementsAndAcceptanceLogs();
				}

				return relatedAgreements;
			}
		}

		EdiUserAgreementCollection relatedAgreements;

		[ChildEditable]
		public ActiveBusinessObjectCollection<EdiUserAgreementAcceptanceLog> AcceptanceLogs
		{
			get
			{
				if (acceptanceLogs == null)
				{
					acceptanceLogs = new ActiveBusinessObjectCollection<EdiUserAgreementAcceptanceLog>(Factory);
					acceptanceLogs.AdditionalFilter = GetAcceptanceLogQuery();
					RegisterEditableChildObject(acceptanceLogs);
				}

				return acceptanceLogs;
			}
		}
		ActiveBusinessObjectCollection<EdiUserAgreementAcceptanceLog> acceptanceLogs;

		void ReloadRelatedAgreementsAndAcceptanceLogs()
		{
			var query = EdiUserAgreement.GetCurrentAgreementsQuery(EAE_AgreementType, string.Empty);
			query.AddToFilter(EdiUserAgreementSchema.ERA_VariantCode, EdiUserAgreementTypesMapper.IsVariantMandatory(EAE_AgreementType) ? EAE_VariantCode : new string[] { EAE_VariantCode, string.Empty });

			RelatedAgreements.AdditionalFilter = query;
			RelatedAgreements.RefreshFromDb();

			AcceptanceLogs.AdditionalFilter = GetAcceptanceLogQuery();
			AcceptanceLogs.RefreshFromDb();
		}

		[ReadOnlyMember(nameof(ClientAgreementOrgPKReadOnly))]
		public override ZGuid EAE_OH_ClientAgreementOrg { get => base.EAE_OH_ClientAgreementOrg; set => base.EAE_OH_ClientAgreementOrg = value; }

		public override OrgHeader ClientAgreementOrg => HasEnterpriseParent ? ((LicenceEnterprise)Parent).Organisation : base.ClientAgreementOrg;

		[ReadOnlyMember(nameof(ClientAgreementOrgPKReadOnly))]
		[List("Lookups.ClientAgreementOrgs")]
		[ResourceStringData("EdiUserAgreementAssignment|ClientAgreementOrgPK", Caption = "Client Agreement Org.", FullDescription = "Client Agreement Organization")]
		public ZGuid ClientAgreementOrgPK
		{
			get => HasEnterpriseParent ? ((LicenceEnterprise)Parent).LE_OH : base.EAE_OH_ClientAgreementOrg;
			set => base.EAE_OH_ClientAgreementOrg = value;
		}

		public ZWrappedPropertyInfo ClientAgreementOrgPKInfo => GetWrappedZPropertyInfo(nameof(ClientAgreementOrgPK), x => EAE_OH_ClientAgreementOrgInfo);

		public bool ClientAgreementOrgPKReadOnly => HasEnterpriseParent;

		bool HasEnterpriseParent => EAE_ParentTableCode.EqualsIgnoringCase(LicenceEnterpriseSchema.Constants.Prefix);

		#endregion

		ZQuery GetAcceptanceLogQuery()
		{
			if (Parent == null)
			{
				return new ZQuery(EdiUserAgreementAcceptanceLogSchema.PK, ZGuid.Empty);
			}

			var query = new ZQuery();
			switch (EAE_ParentTableCode)
			{
				case "":
					query.AddToFilter(EdiUserAgreementAcceptanceLogSchema.PK, ZGuid.Empty);
					break;
				case LicenceEnterpriseSchema.Constants.Prefix:
					query.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_LE, EAE_ParentID);
					break;
				case LicenceDatabaseSchema.Constants.Prefix:
					query.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_LD, EAE_ParentID);
					break;
				default:
					throw new Exception("Unsupported parent table code");
			}

			query.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_ERA, RelatedAgreements.Select(x => x.PK));
			return query;
		}

		public static EdiUserAgreementAssignment GetCurrentAssignment(BusinessObject parent, string agreementType)
		{
			if (parent == null)
			{
				return null;
			}

			var tableCode = parent.TablePrefix;
			var parentID = parent.PK;
			var factory = parent.Factory;

			if (string.IsNullOrEmpty(tableCode) || parentID.IsEmpty)
			{
				return null;
			}

			if (!EdiUserAgreementTypesMapper.GetBindingTableCodes(agreementType).Contains(tableCode))
			{
				return null;
			}

			var assignmentQuery = new ZQuery();
			assignmentQuery.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_AgreementType, agreementType);
			assignmentQuery.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_ParentTableCode, tableCode);
			assignmentQuery.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_ParentID, parentID);

			var result = factory.LoadTop1<EdiUserAgreementAssignment>(assignmentQuery);

			if (result != null)
			{
				return result;
			}

			switch (agreementType)
			{
				case EdiUserAgreementTypes.Codes.CargoWiseNext:
					if (parent is LicenceDatabase databaseParent)
					{
						var enterpriseAssignmentQuery = new ZQuery();
						enterpriseAssignmentQuery.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_AgreementType, agreementType);
						enterpriseAssignmentQuery.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_ParentTableCode, LicenceEnterpriseSchema.Constants.Prefix);
						enterpriseAssignmentQuery.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_ParentID, databaseParent.LD_LE);
						return factory.LoadTop1<EdiUserAgreementAssignment>(enterpriseAssignmentQuery);
					}

					break;
				default:
					break;
			}

			return null;
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new EdiUserAgreementAssignmentFetchStrategy(this);
		}
	}
}
