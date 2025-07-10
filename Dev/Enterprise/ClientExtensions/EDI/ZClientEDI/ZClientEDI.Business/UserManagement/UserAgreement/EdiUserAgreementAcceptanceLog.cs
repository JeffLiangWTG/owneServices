using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiUserAgreementAcceptanceLog : AutoEdiUserAgreementAcceptanceLog
	{
		protected override ZString HumanReadableNameCore => Res.GetString("94524530-8E54-4767-AC5F-E9F1E1EE57AA", "User Agreement Acceptance Log");

		public EdiUserAgreementAcceptanceLog(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		#region EUL_ERA

		[RelatedBusinessObject("UserAgreement")]
		public override ZGuid EUL_ERA
		{
			get => base.EUL_ERA;
			set => base.EUL_ERA = value;
		}

		public EdiUserAgreement UserAgreement => Factory.Load<EdiUserAgreement>(EUL_ERA);

		#endregion

		#region EUL_EUA

		[RelatedBusinessObject("UserAccount")]
		public override ZGuid EUL_EUA
		{
			get => base.EUL_EUA;
			set
			{
				if (base.EUL_EUA != value)
				{
					base.EUL_EUA = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateEUL_OH();
						Validation.ValidateEUL_GS();
					}
				}
			}
		}

		public EdiCustomerUserAccount UserAccount => Factory.Load<EdiCustomerUserAccount>(EUL_EUA);

		#endregion

		#region EUL_OH

		[RelatedBusinessObject("Organisation")]
		public override ZGuid EUL_OH
		{
			get => base.EUL_OH;
			set
			{
				if (base.EUL_OH != value)
				{
					base.EUL_OH = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateEUL_EUA();
						Validation.ValidateEUL_GS();
					}
				}
			}
		}

		public OrgHeader Organisation => Factory.Load<OrgHeader>(EUL_OH);

		#endregion

		#region EUL_GS

		[RelatedBusinessObject("StaffAcceptanceProxy")]
		public override ZGuid EUL_GS
		{
			get => base.EUL_GS;
			set
			{
				if (base.EUL_GS != value)
				{
					base.EUL_GS = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateEUL_EUA();
						Validation.ValidateEUL_OH();
					}
				}
			}
		}

		public GlbStaff StaffAcceptanceProxy => Factory.Load<GlbStaff>(EUL_GS);

		#endregion

		[List($"{nameof(Lookups)}.{nameof(Lookups.LicenceDatabaseList)}")]
		public override ZGuid EUL_LD { get => base.EUL_LD; set => base.EUL_LD = value; }

		[List($"{nameof(Lookups)}.{nameof(Lookups.EnterpriseList)}")]
		public override ZGuid EUL_LE { get => base.EUL_LE; set => base.EUL_LE = value; }

		public ZString OrgCode => UserAccount?.WebAccessContact?.OrgCode ?? Organisation?.OH_Code ?? ZString.Empty;

		public ZString OrgName
		{
			get
			{
				var webAccessContact = UserAccount?.WebAccessContact;
				var result = ZString.Empty;
				if (webAccessContact != null)
				{
					if (!string.IsNullOrEmpty(webAccessContact.BranchAddress?.OA_CompanyNameOverride))
					{
						result = webAccessContact.BranchAddress.OA_CompanyNameOverride;
					}
					else if (!string.IsNullOrEmpty(webAccessContact.ParentOrg?.MainAddress?.OA_CompanyNameOverride))
					{
						result = webAccessContact.ParentOrg.MainAddress.OA_CompanyNameOverride;
					}
					else if (!string.IsNullOrEmpty(webAccessContact.ParentOrg?.OH_FullName))
					{
						result = webAccessContact.ParentOrg.OH_FullName;
					}
				}
				else if (Organisation != null)
				{
					if (!string.IsNullOrEmpty(Organisation?.MainAddress?.OA_CompanyNameOverride))
					{
						result = Organisation.MainAddress.OA_CompanyNameOverride;
					}
					else if (!string.IsNullOrEmpty(Organisation?.OH_FullName))
					{
						result = Organisation.OH_FullName;
					}
				}

				return result;
			}
		}

		public LicenceEnterprise Enterprise
		{
			get
			{
				if (!EUL_LE.IsEmpty)
				{
					return Factory.Load<LicenceEnterprise>(EUL_LE);
				}
				else
				{
					return Database?.LicEnterprise;
				}
			}
		}

		public LicenceDatabase Database
		{
			get
			{
				if (!EUL_LD.IsEmpty)
				{
					return Factory.Load<LicenceDatabase>(EUL_LD);
				}
				else if (!EUL_EUA.IsEmpty)
				{
					return UserAccount?.Database;
				}
				return null;
			}
		}

		public ZString ContactName => UserAccount?.WebAccessContact?.OC_ContactName ?? ZString.Empty;

		public ZString Product => Database?.LD_Product ?? ZString.Empty;

		public ZString ProductName => ProductTypeList.GetDescriptionFromCode(Product);

		public ZString ServerCode => Database?.LD_ServerCode ?? ZString.Empty;

		public ZInt DatabaseNumber => Database?.LD_DatabaseNumber ?? ZInt.Zero;

		public ZString EnterpriseCode => Enterprise?.LE_EnterpriseCode ?? ZString.Empty;

		public ZString EnterpriseId => Enterprise?.LE_EnterpriseID ?? ZString.Empty;

		public ZString TenantID => Database?.LD_TenantID ?? ZString.Empty;

		public ZString AgreementVariant => UserAgreement?.ERA_VariantCode ?? ZString.Empty;

		public ZString AgreementVersion => FormattableString.Invariant($"{UserAgreement?.ERA_VersionNumber ?? 0}.{UserAgreement?.ERA_MinorVersion ?? 0}");

		public ZDateTime AcceptanceTimeLocal => EUL_AcceptanceTimeUtc.ToLocalBranchTime(Factory);

		#endregion

		public bool IsAddedManually { get; set; }

		CodeDescriptionPairList ProductTypeList
		{
			get => productTypeList ??= new ProductTypes(includeCargoWiseOne: true);
		}
		CodeDescriptionPairList productTypeList;
	}
}
