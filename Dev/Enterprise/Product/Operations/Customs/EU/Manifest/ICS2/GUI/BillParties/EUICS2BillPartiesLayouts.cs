using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public class EUICS2BillPartiesLayouts : IPanelLayoutProvider
	{
		PanelLayout BillParties { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillParties;

		public EUICS2BillPartiesLayouts()
		{
			BillParties = CreateBillPartiesLayout();
		}

		PanelLayout CreateBillPartiesLayout()
		{
			var builder = new BillPartiesLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			var controlBag = EUICS2BillPartiesControlBag.Instance;
			builder.AddControlBag(controlBag);

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
			builder.Add(controlBag.ShipperPersonTypeDropEdit, ControlWidthClass.Auto);

			builder.Add(controlBag.SellerSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(controlBag.SellerAddressControl, ControlWidthClass.Long);
			builder.Add(controlBag.ConvertSellerToOrganizationButton, ControlWidthClass.Long);
			builder.Add(controlBag.SellerNameTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.SellerStreet1TextBox, ControlWidthClass.Long);
			builder.Add(controlBag.SellerStreet2TextBox, ControlWidthClass.Long);
			builder.Add(controlBag.SellerCityTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.SellerCountryCodeFindBox, ControlWidthClass.Auto);
			builder.Add(controlBag.SellerStateDropEdit, ControlWidthClass.Auto);
			builder.Add(controlBag.SellerPostCodeTextBox, ControlWidthClass.Auto);
			builder.Add(controlBag.SellerPhoneTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.SellerRegNoTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.SellerPersonTypeDropEdit, ControlWidthClass.Auto);

			builder.SetVisibility(controlBag.SellerSeparatorUserControl, b => !b.IsChildMasterBill);
			builder.SetVisibility(controlBag.SellerAddressControl, b => !b.IsChildMasterBill);
			builder.SetVisibility(controlBag.SellerNameTextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(controlBag.SellerStreet1TextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(controlBag.SellerStreet2TextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(controlBag.SellerCityTextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(controlBag.SellerCountryCodeFindBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(controlBag.SellerStateDropEdit, b => !b.IsChildMasterBill);
			builder.SetVisibility(controlBag.SellerPostCodeTextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(controlBag.SellerPhoneTextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(controlBag.SellerRegNoTextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(controlBag.SellerPersonTypeDropEdit, b => !b.IsChildMasterBill);

			builder.SetVisibility(controlBag.ConvertSellerToOrganizationButton, b => !b.IsChildMasterBill && b.CanConvertSellerToOrganization, b => b.ABL_OA_SellerInfo, b => b.ABL_SellerNameInfo, b => b.ABL_SellerStreet1Info,
				b => b.ABL_SellerStreet2Info, b => b.ABL_SellerCityInfo, b => b.ABL_SellerStateInfo, b => b.ABL_SellerPostcodeInfo, b => b.ABL_RN_NKSellerCountryInfo, b => b.ABL_SellerPhoneInfo, b => b.Header.AMA_TransportModeInfo, b => b.Header.SpecificCircumstanceIndicatorInfo);

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
			builder.Add(controlBag.ConsigneePersonTypeDropEdit, ControlWidthClass.Auto);

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
			builder.Add(controlBag.BuyerPersonTypeDropEdit, ControlWidthClass.Auto);

			builder.SetVisibility(common.BuyerSeparatorUserControl, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.BuyerAddressControl, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.BuyerNameTextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.BuyerStreet1TextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.BuyerStreet2TextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.BuyerCityTextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.BuyerCountryCodeFindBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.BuyerStateDropEdit, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.BuyerPostcodeTextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.BuyerPhoneTextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.BuyerRegNoTextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(controlBag.BuyerPersonTypeDropEdit, b => !b.IsChildMasterBill);

			builder.SetVisibility(common.ConvertBuyerToOrganizationButton, b => !b.IsChildMasterBill && b.CanConvertBuyerToOrganization, b => b.ABL_OA_BuyerInfo, b => b.ABL_BuyerNameInfo, b => b.ABL_BuyerStreet1Info,
				b => b.ABL_BuyerStreet2Info, b => b.ABL_BuyerCityInfo, b => b.ABL_BuyerStateInfo, b => b.ABL_BuyerPostcodeInfo, b => b.ABL_RN_NKBuyerCountryInfo, b => b.ABL_BuyerPhoneInfo, b => b.Header.AMA_TransportModeInfo, b => b.Header.SpecificCircumstanceIndicatorInfo);

			builder.AddColumn();
			builder.Add(common.NotifyPartySeparatorUserControl, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConvertNotifyPartyToOrganizationButton, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyNameTextBox, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyStreet1TextBox, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyStreet2TextBox, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyCityTextBox, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyCountryCodeFindBox, ControlWidthClass.Auto);
			builder.Add(common.NotifyPartyStateDropEdit, ControlWidthClass.Auto);
			builder.Add(common.NotifyPartyPostcodeTextBox, ControlWidthClass.Auto);
			builder.Add(common.NotifyPartyPhoneTextBox, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyRegNoTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.NotifyPartyPersonTypeDropEdit, ControlWidthClass.Auto);

			builder.SetVisibility(common.NotifyPartySeparatorUserControl, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.NotifyPartyAddressControl, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.NotifyPartyNameTextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.NotifyPartyStreet1TextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.NotifyPartyStreet2TextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.NotifyPartyCityTextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.NotifyPartyCountryCodeFindBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.NotifyPartyStateDropEdit, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.NotifyPartyPostcodeTextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.NotifyPartyPhoneTextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(common.NotifyPartyRegNoTextBox, b => !b.IsChildMasterBill);
			builder.SetVisibility(controlBag.NotifyPartyPersonTypeDropEdit, b => !b.IsChildMasterBill);

			builder.SetVisibility(common.ConvertNotifyPartyToOrganizationButton, b => !b.IsChildMasterBill && b.CanConvertNotifyPartyToOrganization, b => b.ABL_OA_NotifyPartyInfo, b => b.ABL_NotifyPartyNameInfo, b => b.ABL_NotifyPartyStreet1Info,
				b => b.ABL_NotifyPartyStreet2Info, b => b.ABL_NotifyPartyCityInfo, b => b.ABL_NotifyPartyStateInfo, b => b.ABL_NotifyPartyPostcodeInfo, b => b.ABL_RN_NKNotifyPartyCountryInfo, b => b.ABL_NotifyPartyPhoneInfo, b => b.Header.AMA_TransportModeInfo, b => b.Header.SpecificCircumstanceIndicatorInfo);

			return builder.Build();
		}
	}
}
