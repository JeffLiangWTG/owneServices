using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.Manifest.GUI;

sealed class CGMBillPartiesLayout : IPanelLayoutProvider
{
	public CGMBillPartiesLayout()
	{
		BillPartiesLayout = CreateBillPartiesLayout();
	}

	PanelLayout BillPartiesLayout { get; }

	PanelLayout IPanelLayoutProvider.Layout => BillPartiesLayout;

	PanelLayout CreateBillPartiesLayout()
	{
		var builder = new BillPartiesLayoutBuilder<CGMAsycudaBill>();
		var common = builder.CommonBag;

		builder.AddColumn();
		builder.Add(common.ShipperSeparatorUserControl, ControlWidthClass.Long);
		builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
		builder.Add(common.ConvertShipperToOrganizationButton, ControlWidthClass.Long);
		builder.Add(common.ShipperNameTextBox, ControlWidthClass.Long);
		builder.Add(common.ShipperStreet1TextBox, ControlWidthClass.Long);
		builder.Add(common.ShipperStreet2TextBox, ControlWidthClass.Long);
		builder.Add(common.ShipperCityTextBox, ControlWidthClass.Long);
		builder.Add(common.ShipperCountryCodeFindBox, ControlWidthClass.Auto);
		builder.Add(common.ShipperStateDropEdit, ControlWidthClass.Auto);
		builder.Add(common.ShipperPostCodeTextBox, ControlWidthClass.Auto);
		builder.Add(common.ShipperPhoneTextBox, ControlWidthClass.Long);
		builder.Add(common.ShipperRegNoTextBox, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(common.ConsigneeSeparatorUserControl, ControlWidthClass.Long);
		builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
		builder.Add(common.ConvertConsigneeToOrganizationButton, ControlWidthClass.Long);
		builder.Add(common.ConsigneeNameTextBox, ControlWidthClass.Long);
		builder.Add(common.ConsigneeStreet1TextBox, ControlWidthClass.Long);
		builder.Add(common.ConsigneeStreet2TextBox, ControlWidthClass.Long);
		builder.Add(common.ConsigneeCityTextBox, ControlWidthClass.Long);
		builder.Add(common.ConigneeCountryCodeFindBox, ControlWidthClass.Auto);
		builder.Add(common.ConsigneeStateDropEdit, ControlWidthClass.Auto);
		builder.Add(common.ConsigneePostcodeTextBox, ControlWidthClass.Auto);
		builder.Add(common.ConsigneePhoneTextBox, ControlWidthClass.Long);
		builder.Add(common.ConsigneeRegoNoTextBox, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(common.BuyerSeparatorUserControl, ControlWidthClass.Long);
		builder.Add(common.BuyerAddressControl, ControlWidthClass.Long);
		builder.Add(common.ConvertBuyerToOrganizationButton, ControlWidthClass.Long);
		builder.Add(common.BuyerNameTextBox, ControlWidthClass.Long);
		builder.Add(common.BuyerStreet1TextBox, ControlWidthClass.Long);
		builder.Add(common.BuyerStreet2TextBox, ControlWidthClass.Long);
		builder.Add(common.BuyerCityTextBox, ControlWidthClass.Long);
		builder.Add(common.BuyerCountryCodeFindBox, ControlWidthClass.Auto);
		builder.Add(common.BuyerStateDropEdit, ControlWidthClass.Auto);
		builder.Add(common.BuyerPostcodeTextBox, ControlWidthClass.Auto);
		builder.Add(common.BuyerPhoneTextBox, ControlWidthClass.Long);
		builder.Add(common.BuyerRegNoTextBox, ControlWidthClass.Long);

		builder.SetCaption(common.BuyerSeparatorUserControl, _ => Res.GetData("FE3624C2-26CA-4F9F-8E96-929898DAE0F4", "Importer Details"));
		builder.SetCaption(common.ConvertBuyerToOrganizationButton, _ => Res.GetData("4FBA1084-14D2-4E4A-84B1-A96EF76F7D45", "Convert Importer To Org."));

		builder.SetVisibility(common.BuyerSeparatorUserControl, x => x.IsSea, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(common.BuyerAddressControl, x => x.IsSea, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(common.BuyerNameTextBox, x => x.IsSea, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(common.BuyerStreet1TextBox, x => x.IsSea, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(common.BuyerStreet2TextBox, x => x.IsSea, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(common.BuyerCityTextBox, x => x.IsSea, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(common.BuyerCountryCodeFindBox, x => x.IsSea, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(common.BuyerStateDropEdit, x => x.IsSea, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(common.BuyerPostcodeTextBox, x => x.IsSea, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(common.BuyerPhoneTextBox, x => x.IsSea, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(common.BuyerRegNoTextBox, x => x.IsSea, x => x.Header?.AMA_TransportModeInfo);
		builder.SetVisibility(common.ConvertBuyerToOrganizationButton, x => x.CanConvertBuyerToOrganization && x.IsSea, x => x.ABL_OA_BuyerInfo, x => x.ABL_BuyerNameInfo, x => x.ABL_BuyerStreet1Info,
				 x => x.ABL_BuyerStreet2Info, x => x.ABL_BuyerCityInfo, x => x.ABL_BuyerStateInfo, x => x.ABL_BuyerPostcodeInfo, x => x.ABL_RN_NKBuyerCountryInfo, x => x.ABL_BuyerPhoneInfo, x => x.Header?.AMA_TransportModeInfo);

		return builder.Build();
	}
}
