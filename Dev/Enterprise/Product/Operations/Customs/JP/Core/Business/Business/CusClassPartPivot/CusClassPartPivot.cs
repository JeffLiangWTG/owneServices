using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Business
{
	public class CusClassPartPivot : AutoJPCusClassPartPivot
	{
		public CusClassPartPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString CI_ChildType
		{
			get => base.CI_ChildType;
			set
			{
				base.CI_ChildType = value;
				if (IsExport)
				{
					CI_PrimaryPreference = string.Empty;
					CI_StorageType = string.Empty;
					CI_AdvanceRulingOnClassification = string.Empty;
					CI_AdvanceRulingOnOrigin = string.Empty;
					CI_DutyReductionAmount = ZDecimal.Zero;
				}
				else if (IsImport)
				{
					CI_FEFTAArticle48 = string.Empty;
					CI_DomesticConsumptionTaxExemptionIsPartial = ZBool.False;
					CI_DomesticConsumptionTaxExemptionCode = string.Empty;
				}
			}
		}

		public bool IsExport => CI_ChildType == ClassificationTypeList.Codes.HTE;

		public bool IsImport => CI_ChildType == ClassificationTypeList.Codes.HTI;

		[ResourceStringData("JP.Business.CusClassPartPivot|CI_DutyReductionExemptionRefundCode", Caption = "Duty Reduction Exemption Refund Code", ShortCaption = "Duty Refund")]
		public override ZString CI_DutyReductionExemptionRefundCode { get => base.CI_DutyReductionExemptionRefundCode; set => base.CI_DutyReductionExemptionRefundCode = value; }

		[ReadOnlyMember(nameof(IsExport))]
		[ResourceStringData("JP.Business.CusClassPartPivot|CI_PrimaryPreference", Caption = "Certificate of Origin Certifier", ShortCaption = "COO Certifier")]
		public override ZString CI_PrimaryPreference { get => base.CI_PrimaryPreference; set => base.CI_PrimaryPreference = value; }

		[ReadOnlyMember(nameof(IsExport))]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.StorageTypeList))]
		public override ZString CI_StorageType { get => base.CI_StorageType; set => base.CI_StorageType = value; }

		[ReadOnlyMember(nameof(IsExport))]
		[ResourceStringData("JP.Business.CusClassPartPivot|CI_DutyReductionAmount", Caption = "Duty Reduction Amount", ShortCaption = "Duty Reduction")]
		public override ZDecimal CI_DutyReductionAmount { get => base.CI_DutyReductionAmount; set => base.CI_DutyReductionAmount = value; }

		[ReadOnlyMember(nameof(IsImport))]
		public override ZBool CI_DomesticConsumptionTaxExemptionIsPartial { get => base.CI_DomesticConsumptionTaxExemptionIsPartial; set => base.CI_DomesticConsumptionTaxExemptionIsPartial = value; }

		[ReadOnlyMember(nameof(IsImport))]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.ConsumptionTaxExemptionIDList))]
		public override ZString CI_DomesticConsumptionTaxExemptionCode { get => base.CI_DomesticConsumptionTaxExemptionCode; set => base.CI_DomesticConsumptionTaxExemptionCode = value; }

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.TradeControlOrderAppendixList))]
		[ResourceStringData("JP.Business.CusClassPartPivot|CI_TradeControlOrderAppendix", Caption = "Trade Control Order Appendix", MediumCaption = "Trade Ctrl. Order Appx.", ShortCaption = "Trade Ctrl. Ord.", FullDescription = "Trade Control Order Appendix Code")]
		public override ZString CI_TradeControlOrderAppendix { get => base.CI_TradeControlOrderAppendix; set => base.CI_TradeControlOrderAppendix = value; }

		[ReadOnlyMember(nameof(IsImport))]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.FEFTAArticle48List))]
		[ResourceStringData("JP.Business.CusClassPartPivot|CI_FEFTAArticle48", Caption = "FEFTA Article 48", ShortCaption = "FEFTA", FullDescription = "Foreign Exchange and Foreign Trade Act (FEFTA) Article 48")]
		public override ZString CI_FEFTAArticle48 { get => base.CI_FEFTAArticle48; set => base.CI_FEFTAArticle48 = value; }

		[ReadOnlyMember(nameof(IsExport))]
		[ResourceStringData("JP.Business.CusClassPartPivot|CI_AdvanceRulingOnClassification", Caption = "Advance Ruling on Classification")]
		public override ZString CI_AdvanceRulingOnClassification
		{
			get { return base.CI_AdvanceRulingOnClassification; }
			set { base.CI_AdvanceRulingOnClassification = value; }
		}

		[ReadOnlyMember(nameof(IsExport))]
		[ResourceStringData("JP.Business.CusClassPartPivot|CI_AdvanceRulingOnOrigin", Caption = "Advance Ruling on Origin")]
		public override ZString CI_AdvanceRulingOnOrigin
		{
			get { return base.CI_AdvanceRulingOnOrigin; }
			set { base.CI_AdvanceRulingOnOrigin = value; }
		}

		protected override TariffFormatter GetTariffFormatter() => new TariffFormatter();

		public new CusClassPartPivotLookups Lookups => (CusClassPartPivotLookups)base.Lookups;

		protected override Customs.Business.CusClassPartPivotLookups GetNewLookups() => new CusClassPartPivotLookups(this);

		public new CusClassPartPivotValidation Validation => (CusClassPartPivotValidation)base.Validation;

		protected override Customs.Business.CusClassPartPivotValidation GetNewValidation() => new CusClassPartPivotValidation(this);
	}
}
