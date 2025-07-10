using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.GB;
using DeclarationApplicationCodeList = Enterprise.Customs.GB.Registry.Business.DeclarationApplicationCodeList;

namespace Enterprise.Customs.GB.H7.Business
{
	public class AsycudaManifestHeader : EU.H7.Business.AsycudaManifestHeader,
		GBH7.IAsycudaManifestHeader
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : EU.H7.Business.AsycudaManifestHeader.Schema
		{
			public const string CSP = "CSP";
			public const int CSPMaxLength = 5;
		}

		protected override ZString GetDataGroupingCore()
		{
			return Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
		}

		public new IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> Bills => (IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>)base.Bills;

		protected override IAsycudaBillCollection<EU.H7.Business.AsycudaBill, EU.H7.Business.AsycudaManifestHeader> CreateNewEUH7AsycudaBillCollection() => new AsycudaBillCollection(this);

		public new AsycudaBill MasterBill => (AsycudaBill)base.MasterBill;

		protected override Type GetBillTypeCore() => typeof(AsycudaBill);

		public new ApplicationBusinessProvider ApplicationBusinessProvider => (ApplicationBusinessProvider)base.ApplicationBusinessProvider;

		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);

		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;

		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;

		public bool IsForBIRDS => PortOfDischarge?.IsInGreatBritain ?? false;

		public CurrencyConverter CurrencyConverter
		{
			get
			{
				if (currencyConverter == null)
				{
					currencyConverter = CurrencyConverter.New(Factory, ZDateTime.Today, ZArchitecture.Core.ExchangeRateType.Customs, true);
				}
				return currencyConverter;
			}
		}

		CurrencyConverter currencyConverter;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AMA_CustomsProfile = GetDefaultCustomsProfile();
		}

		protected override ZString GetDefaultCountryCode() => CountryCodes.UnitedKingdom;

		ZString GetDefaultCustomsProfile()
		{
			var profiles = Lookups.ProfileList;
			var collection = GBGlbCompanyWrapper.GetWrapper<GBGlbCompanyWrapper>(GlbCompany.CurrentCompany).GBBPasswordCollection;

			var badgeCodeWithValidToken = collection.OfType<GlbExternalPassword_GB>()
				.Where(x => x.Status == PasswordStatusList.Codes.Valid && profiles.ContainsCode(x.Badge))
				.OrderByDescending(x => x.GP_IssueDate)
				.FirstOrDefault();

			if (badgeCodeWithValidToken != null)
			{
				return badgeCodeWithValidToken.Badge;
			}

			var badgeCodeWithInvalidToken = collection.OfType<GlbExternalPassword_GB>()
				.Where(x => x.Status != PasswordStatusList.Codes.Valid && profiles.ContainsCode(x.Badge))
				.OrderByDescending(x => x.GP_ExpiryDate)
				.FirstOrDefault();

			if (badgeCodeWithInvalidToken != null)
			{
				return badgeCodeWithInvalidToken.Badge;
			}

			if (profiles.Count > 0)
			{
				return profiles[0].Code;
			}

			return ZString.Empty;
		}

		public ApplicationExtender ApplicationExtender => applicationExtender ?? (applicationExtender = ApplicationExtender.New(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services));
		ApplicationExtender applicationExtender;

		#region Properties

		public override ZGuid AMA_GB
		{
			get
			{
				return base.AMA_GB;
			}
			set
			{
				var oldValue = base.AMA_GB;
				base.AMA_GB = value;

				if (oldValue != base.AMA_GB)
				{
					AMA_CustomsProfile = GetDefaultCustomsProfile();
				}
			}
		}

		public override ZString AMA_RL_NKPortOfDischarge
		{
			get
			{
				return base.AMA_RL_NKPortOfDischarge;
			}
			set
			{
				var oldValue = base.AMA_RL_NKPortOfDischarge;
				base.AMA_RL_NKPortOfDischarge = value;

				if (oldValue != base.AMA_RL_NKPortOfDischarge)
				{
					AMA_CustomsProfile = GetDefaultCustomsProfile();
				}
			}
		}

		public override ZString AMA_RL_NKPortOfFirstArrival
		{
			get
			{
				return base.AMA_RL_NKPortOfFirstArrival;
			}
			set
			{
				var oldValue = base.AMA_RL_NKPortOfFirstArrival;
				base.AMA_RL_NKPortOfFirstArrival = value;

				if (oldValue != base.AMA_RL_NKPortOfFirstArrival)
				{
					AMA_CustomsProfile = GetDefaultCustomsProfile();
				}
			}
		}

		[ResourceStringData("f21e07b2-3039-49d1-a04b-d3f16b41ec33", Caption = "Rep. Status")]
		public override ZString AMA_AgentType
		{
			get { return base.AMA_AgentType; }
			set { base.AMA_AgentType = value; }
		}

		[ResourceStringData("Enterprise.Customs.GB.H7.Business.AsycudaManifestHeader.State", Caption = "State")]
		public override ZString AMA_RN_NKCountry
		{
			get => base.AMA_RN_NKCountry;
			set => base.AMA_RN_NKCountry = value;
		}

		[ResourceStringData("Enterprise.Customs.GB.H7.Business.AsycudaManifestHeader.CustomsProfile", Caption = "Profile")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.ProfileList))]
		public override ZString AMA_CustomsProfile
		{
			get => base.AMA_CustomsProfile;
			set
			{
				var oldValue = AMA_CustomsProfile;
				base.AMA_CustomsProfile = value;
				if (oldValue != value)
				{
					var badgeCode = AMA_CustomsProfile;
					var badgeCodeSetting = badgeCode.IsEmpty ? null : GBCustomsDataRegistry.Instance.BadgeCodes.Value.FindByBadgeCodeOnly(badgeCode);
					CSP = badgeCodeSetting?.CSPCode ?? ZString.Empty;
				}
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.GB.H7.Business.AsycudaManifestHeader.CSP", Caption = "CSP")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CSPList))]
		[MaxLength(Schema.CSPMaxLength)]
		public ZString CSP
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.CSP);
			set
			{
				var oldValue = CSP;
				if (oldValue != value)
				{
					CheckMaximumLength(CSPInfo, value);
					this.SetSystemDefinedValue(Schema.CSP, value);
				}

				CSPInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CSPInfo => GetZPropertyInfo(nameof(CSP));

		public ZString TargetPort => Factory.GetValue(ref targetPort, () => { return AMA_RL_NKPortOfFirstArrival.IsEmpty ? AMA_RL_NKPortOfDischarge : AMA_RL_NKPortOfFirstArrival; });
		CachedProperty<ZString> targetPort;

		public ZString CredentialsKey
		{
			get
			{
				var eori = Branch?.OrgProxy?.GetEuIdentificationNumber() ?? ZString.Empty;
				return GBExtensions.GetCredentialsKey(eori, badgeCode: AMA_CustomsProfile);
			}
		}

		public override ZGuid AMA_OA_Declarant
		{
			get
			{
				return base.AMA_OA_Declarant;
			}
			set
			{
				var oldValue = AMA_OA_Declarant;
				base.AMA_OA_Declarant = value;

				if (oldValue != AMA_OA_Declarant)
				{
					SupervisingOfficeAddressPKInfo.RefreshBinding();
				}
			}
		}

		public OrgHeader SupervisingOffice
		{
			get
			{
				if (Declarant != null)
				{
					var declarant = Declarant.Header;
					return declarant.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsOffice, RelatedPartyDirectionList.Codes.Forwarder);
				}

				return null;
			}
		}

		public OrgAddress SupervisingOfficeAddress => SupervisingOffice?.MainAddress;

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.GB.H7.Business.AsycudaManifestHeader.SupervisingOffice", Caption = "Supervising Office")]
		[RelatedBusinessObject(nameof(SupervisingOfficeAddress))]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.SupervisingOfficeList))]
		public ZGuid SupervisingOfficeAddressPK
		{
			get
			{
				var addressPK = SupervisingOfficeAddress?.PK ?? ZGuid.Empty;

				if (addressPK.IsEmpty)
				{
					SupervisingOfficeAddressPK_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
				}

				return addressPK;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress SupervisingOfficeAddressPK_ZAddress => supervisingOfficeAddressPK_ZAddress ??= new ZAddress(SupervisingOfficeAddressPKInfo);
		ZAddress supervisingOfficeAddressPK_ZAddress;

		public ZPropertyInfo SupervisingOfficeAddressPKInfo => GetZPropertyInfo(nameof(SupervisingOfficeAddressPK));

		public GB.Business.CusPermitHeader BIRDSPermitHeader => Factory.GetValue(ref birdsPermitHeaderCached, () =>
		{
			if (Declarant != null)
			{
				var query = new ZQuery(CusPermitHeaderSchema.CPH_OA_AppliesTo, AMA_OA_Declarant);
				query.AddToFilter(CusPermitHeaderSchema.CPH_Type, BulkImportReducedDataSetAuthorisationCode);
				return Factory.LoadTop1<GB.Business.CusPermitHeader>(query);
			}

			return null;
		});

		CachedProperty<GB.Business.CusPermitHeader> birdsPermitHeaderCached;

		const string BulkImportReducedDataSetAuthorisationCode = "BRD";

		public override ZString RegistrationStatus
		{
			get
			{
				var baseRegistrationStatus = base.RegistrationStatus;
				return baseRegistrationStatus == StatusCodeMultiple ? GBH7StatusCodeMultiple : baseRegistrationStatus;
			}
			set => base.RegistrationStatus = value;
		}

		public const string GBH7StatusCodeMultiple = "MLT";

		#endregion
	}
}
