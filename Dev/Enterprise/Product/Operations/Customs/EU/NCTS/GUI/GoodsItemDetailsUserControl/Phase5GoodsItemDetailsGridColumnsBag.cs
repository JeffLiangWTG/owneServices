using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5GoodsItemDetailsGridColumnsBag
	{
		public Phase5GoodsItemDetailsGridColumnsBag()
		{
			LineNoTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_LineNo, 60);
			DeclarationGoodsItemNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_DeclarationGoodsItemNumber, 60);
			DescriptionTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_Description, 310);
			GrossWeightCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_GrossWeight, 110, c => c.BindToDecimalPlaces = null);
			GrossWeightUnitDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_GrossWeightUnit, 50);
			NetWeightCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_NetWeight, 110, c => c.BindToDecimalPlaces = null);
			NetWeightUnitDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_NetWeightUnit, 50);
			FormattedHarmonisedTariffColumn = new GridColumnReference<Universal.GUI.TariffColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_FormattedHarmonisedTariff, 85, c =>
			{
				c.SelectNomenclatureModes = null;
				c.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
				c.TariffType = "IMP";
			});
			TypeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_Type, 35,
				c => c.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper);
			CountryOfDispatchDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_RN_NKCountryOfDispatch, 90);
			CountryOfDestinationDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_RN_NKCountryOfDestination, 100);
			CountryOfOriginDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_RN_NKCountryOfOrigin, 75);
			CommercialReferenceNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_CommercialReferenceNumber, 140);
			TransportChargesMethodOfPaymentDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_TransportChargesMethodOfPayment, 100,
				c => c.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper);
			UNDGsGuidFindBoxColumn = new GridColumnReference<ZGuidFindBoxColumnStyleInfo>("UNDGs+UNDGSubstanceManagerGuid+Value", 80, c =>
			{
				c.BindToList = "UNDGs+AllUNDGSubstances";
				c.CaptionResourceString = Res.GetData("1216b53b-d345-49cd-a0fb-24c3dd62b9f4", "DG Substance");
			});
			CusC4NumberCodeFindBoxColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_CusC4Number, 65);
			ConsigneeOrganisationFindBoxColumn = new GridColumnReference<ZOrganisationFindBoxColumnStyleInfo>("Consignee+OrganisationPK", 80, c =>
			{
				c.BindToList = "Lookups.ConsigneeList";
				c.CaptionResourceString = Res.GetData("1C934853-CB79-4620-A8E9-A014DB3AE0F0", "Consignee Org.");
			});
			ConsigneeAddressDropEditColumn = new GridColumnReference<ZAddressDropEditColumnStyleInfo>("Consignee+E2_OA_Address", 160,
				c => c.CaptionResourceString = Res.GetData("E756FD11-0A60-436D-8ADD-53F74AE98C8E", "Consignee Address"));
			SupplementaryQuantityEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_CustomsSecondQuantity, 110, c =>
			{
				c.BindToDecimalPlaces = null;
				c.IsVisible = false;
			});
			SupplementaryQuantityUnitDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_CustomsSecondUnitQty, 110, c =>
			{
				c.IsVisible = false;
			});
			CustomsQuantityDropEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_CustomsQuantity, 110, c =>
			{
				c.CaptionResourceString = Res.GetData("71d36f9a-d96a-43ce-b693-50bd9ec4dd74", "Customs Qty");
				c.IsVisible = false;
			});
			ThirdQuantityEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_CustomsThirdQuantity, 110, c =>
			{
				c.BindToDecimalPlaces = null;
				c.IsVisible = false;
			});
			ThirdQuantityUnitDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_CustomsThirdUnitQty, 110, c =>
			{
				c.CaptionResourceString = Res.GetData("707fd63a-2493-494f-a7d6-94b970d89677", "Third Qty Unit");
				c.IsVisible = false;
			});
			FourthQuantityEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_CustomsFourthQuantity, 110, c =>
			{
				c.BindToDecimalPlaces = null;
				c.IsVisible = false;
			});
			FourthQuantityUnitDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_CustomsFourthUnitQty, 110, c =>
			{
				c.CaptionResourceString = Res.GetData("8ee82de1-2514-46ef-b6a7-0ed4c5ed7b73", "Fourth Qty Unit");
				c.IsVisible = false;
			});
			CustomsValueCalcDropEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_MonetaryValue, 110, c =>
			{
				c.IsVisible = false;
			});
			TaxOrFeeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_ZZF_NKTaxType, 75, c =>
			{
				c.IsVisible = false;
			});
			SupplementaryCodesFindBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_Supplements, 80, c =>
			{
				c.IsVisible = false;
			});
			LinePriceCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_LinePrice, 110);
			LinePriceCurrencyDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_RX_NKLinePriceCurrency, 50);
		}

		public static Phase5GoodsItemDetailsGridColumnsBag Instance => instance ?? (instance = new Phase5GoodsItemDetailsGridColumnsBag());

		public IGridColumnReference LineNoTextBoxColumn { get; }

		public IGridColumnReference DeclarationGoodsItemNumberTextBoxColumn { get; }

		public IGridColumnReference DescriptionTextBoxColumn { get; }

		public IGridColumnReference GrossWeightCalcEditColumn { get; }

		public IGridColumnReference GrossWeightUnitDropEditColumn { get; }

		public IGridColumnReference NetWeightCalcEditColumn { get; }

		public IGridColumnReference NetWeightUnitDropEditColumn { get; }

		public IGridColumnReference FormattedHarmonisedTariffColumn { get; }

		public IGridColumnReference TypeDropEditColumn { get; }

		public IGridColumnReference CountryOfDispatchDropEditColumn { get; }

		public IGridColumnReference CountryOfDestinationDropEditColumn { get; }

		public IGridColumnReference CountryOfOriginDropEditColumn { get; }

		public IGridColumnReference CommercialReferenceNumberTextBoxColumn { get; }

		public IGridColumnReference TransportChargesMethodOfPaymentDropEditColumn { get; }

		public IGridColumnReference UNDGsGuidFindBoxColumn { get; }

		public IGridColumnReference CusC4NumberCodeFindBoxColumn { get; }

		public IGridColumnReference ConsigneeOrganisationFindBoxColumn { get; }

		public IGridColumnReference ConsigneeAddressDropEditColumn { get; }

		public IGridColumnReference SupplementaryQuantityEditColumn { get; }

		public IGridColumnReference SupplementaryQuantityUnitDropEditColumn { get; }

		public IGridColumnReference CustomsQuantityDropEditColumn { get; }

		public IGridColumnReference ThirdQuantityEditColumn { get; }

		public IGridColumnReference ThirdQuantityUnitDropEditColumn { get; }

		public IGridColumnReference FourthQuantityEditColumn { get; }

		public IGridColumnReference FourthQuantityUnitDropEditColumn { get; }

		public IGridColumnReference TaxOrFeeDropEditColumn { get; }

		public IGridColumnReference CustomsValueCalcDropEditColumn { get; }

		public IGridColumnReference SupplementaryCodesFindBoxColumn { get; }

		public IGridColumnReference LinePriceCalcEditColumn { get; }

		public IGridColumnReference LinePriceCurrencyDropEditColumn { get; }

		[ThreadStatic]
		static Phase5GoodsItemDetailsGridColumnsBag instance;
	}
}
