using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	[SystemDefinedValues]
	public class ForeignOperator : BaseCusGoodsCatalogProductionInfo, Integration.Customs.BR.IForeignOperator
	{
		public ForeignOperator(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusGoodsCatalogProductionInfo.Schema
		{
			public const string IsKnow = nameof(ForeignOperator.IsKnow);
			public const string CountryCode = nameof(ForeignOperator.CountryCode);
			public const string AuthorityCode = nameof(ForeignOperator.AuthorityCode);
			public const string ForeignOperatorName = nameof(ForeignOperator.ForeignOperatorName);
			public const string CGI_CustomsStatusDescription = nameof(ForeignOperator.CGI_CustomsStatusDescription);
			public const int AuthorityCodeMaxLength = 35;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("4E4F8988-7426-466F-8CEB-57F393BA2C60", "Foreign Operator");

		protected override CusGoodsCatalogProductionInfoLookups GetNewLookups() => new ForeignOperatorLookups(this);

		protected override CusGoodsCatalogProductionInfoValidation GetNewValidation() => new ForeignOperatorValidation(this);

		public new ForeignOperatorValidation Validation => (ForeignOperatorValidation)base.Validation;

		public new CusGoodsCatalog GoodsCatalog => (CusGoodsCatalog)base.GoodsCatalog;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CGI_CustomsStatus = CustomsPostedStatusList.Codes.Active;
			CGI_Type = CusGoodsCatalogProductionInfoTypeList.Codes.FOR;
		}

		public override bool ReadOnly => base.ReadOnly || CGI_CustomsStatus.IsDeletePending() || CGI_CustomsStatus.IsAccepted();

		public override bool CanDelete => CGI_CustomsStatus != CustomsPostedStatusList.Codes.DeletePending;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("2c253495-23d5-4a49-9754-aaf287dcccc5", "Current Customs Status is Deletion Pending. Foreign Operator will only be deleted when message is sent to Customs.");

		public new ForeignOperatorLookups Lookups => (ForeignOperatorLookups)base.Lookups;

		[MaxLength(RefCountry.Schema.RN_CodeMaxLength)]
		public override ZString CGI_Reference { get => base.CGI_Reference; set => base.CGI_Reference = value; }

		[ReadOnlyMember(nameof(CountryCode_ReadOnly))]
		[MaxLength(RefCountry.Schema.RN_CodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(ForeignOperatorLookups.Countries))]
		[ResourceStringData("Enterprise.Customs.BR.Business.ForeignOperator|CountryCode", Caption = "Country of Origin")]
		public ZString CountryCode
		{
			get => CountryCode_ReadOnly ? BRForeignOperator.ForeignOperatorCountryCode : CGI_Reference;
			set
			{
				CGI_Reference = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCountryCode();
				}
				CountryCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CountryCodeInfo => GetZPropertyInfo(nameof(CountryCode));

		[ReadOnlyMember(nameof(IsForeignOperatorModuleEnabled))]
		[MaxLength(Schema.AuthorityCodeMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.ForeignOperator|AuthorityCode", Caption = "Authority Code")]
		public ZString AuthorityCode // TODO: Should not use GenAddOnColumn after registry Foreign Operator Enabled removed.
		{
			get => IsForeignOperatorModuleEnabled ? BRForeignOperator?.BFR_AuthorityIdentifier ?? ZString.Empty : this.GetSystemDefinedValue<ZString>(nameof(AuthorityCode));
			set
			{
				CheckMaximumLength(AuthorityCodeInfo, value);
				this.SetSystemDefinedValue(nameof(AuthorityCode), value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAuthorityCode();
				}
				AuthorityCodeInfo.RefreshBinding();
			}
		}

		[ResourceStringData("Enterprise.Customs.BR.Business.ForeignOperator|IsKnow", Caption = "Is Know?")]
		public ZBool IsKnow => CGI_BFR_ForeignOperator.IsValid || (!CountryCode.IsEmpty && !AuthorityCode.IsEmpty);

		public ZPropertyInfo AuthorityCodeInfo => GetZPropertyInfo(nameof(AuthorityCode));

		[ResourceStringData("Enterprise.Customs.BR.Business.ForeignOperator|CGI_CustomsStatusDescription", Caption = "Customs Status")]
		public ZString CGI_CustomsStatusDescription => Lookups.CustomsStatusList.GetDescriptionFromCode(CGI_CustomsStatus);

		public ZPropertyInfo CGI_CustomsStatusDescriptionInfo => GetZPropertyInfo(nameof(CGI_CustomsStatusDescription));

		public bool IsActive => CGI_CustomsStatus == CustomsPostedStatusList.Codes.Active;

		[List(nameof(Lookups) + "." + nameof(ForeignOperatorLookups.Manufacturers))]
		[ResourceStringData("Enterprise.Customs.BR.Business.ForeignOperator|CGI_BFR_ForeignOperator", Caption = "Manufacturer")]
		[RelatedBusinessObject(nameof(BRForeignOperator))]
		public override ZGuid CGI_BFR_ForeignOperator
		{
			get => base.CGI_BFR_ForeignOperator;
			set
			{
				var oldValue = base.CGI_BFR_ForeignOperator;
				base.CGI_BFR_ForeignOperator = value;
				if (!IsCopying && oldValue != value && value.IsValid)
				{
					CountryCode = ZString.Empty;
					AuthorityCode = ZString.Empty;
				}
			}
		}

		public CusBRForeignOperator BRForeignOperator => Factory.Load<CusBRForeignOperator>(CGI_BFR_ForeignOperator);

		[ResourceStringData("Enterprise.Customs.BR.Business.ForeignOperator|ForeignOperatorName", Caption = "Manufacturer Name")]
		public ZString ForeignOperatorName => BRForeignOperator?.ForeignOperatorName ?? ZString.Empty;

		public bool IsForeignOperatorModuleEnabled => BRCustomsDataRegistry.Instance.EnableForeignOperator.Value;

		bool CountryCode_ReadOnly => IsForeignOperatorModuleEnabled && BRForeignOperator != null;
	}
}
