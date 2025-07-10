using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public class CommonBillPartiesControlBag : ControlBag
	{
		public static CommonBillPartiesControlBag Instance => commonBillPartiesControlBag.Value;

		CommonBillPartiesControlBag()
		{
			ShipperSeparatorUserControl = RegisterControl(nameof(CommonBillPartiesUserControl.ShipperSeparatorUserControl));
			ShipperAddressControl = RegisterControl(nameof(CommonBillPartiesUserControl.ShipperAddressControl));
			ConvertShipperToOrganizationButton = RegisterControl(nameof(CommonBillPartiesUserControl.ConvertShipperToOrganizationButton));
			ShipperNameTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.ShipperNameTextBox));
			ShipperStreet1TextBox = RegisterControl(nameof(CommonBillPartiesUserControl.ShipperStreet1TextBox));
			ShipperStreet2TextBox = RegisterControl(nameof(CommonBillPartiesUserControl.ShipperStreet2TextBox));
			ShipperCityTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.ShipperCityTextBox));
			ShipperCountryCodeFindBox = RegisterControl(nameof(CommonBillPartiesUserControl.ShipperCountryCodeFindBox));
			ShipperStateDropEdit = RegisterControl(nameof(CommonBillPartiesUserControl.ShipperStateDropEdit));
			ShipperPhoneTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.ShipperPhoneTextBox));
			ShipperPostCodeTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.ShipperPostCodeTextBox));
			ShipperRegNoTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.ShipperRegNoTextBox));
			ShipperRegNoTypeDropEdit = RegisterControl(nameof(CommonBillPartiesUserControl.ShipperRegNoTypeDropEdit));

			ConsigneeSeparatorUserControl = RegisterControl(nameof(CommonBillPartiesUserControl.ConsigneeSeparatorUserControl));
			ConsigneeAddressControl = RegisterControl(nameof(CommonBillPartiesUserControl.ConsigneeAddressControl));
			ConvertConsigneeToOrganizationButton = RegisterControl(nameof(CommonBillPartiesUserControl.ConvertConsigneeToOrganizationButton));
			ConsigneeNameTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.ConsigneeNameTextBox));
			ConsigneeStreet1TextBox = RegisterControl(nameof(CommonBillPartiesUserControl.ConsigneeStreet1TextBox));
			ConsigneeStreet2TextBox = RegisterControl(nameof(CommonBillPartiesUserControl.ConsigneeStreet2TextBox));
			ConsigneeCityTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.ConsigneeCityTextBox));
			ConigneeCountryCodeFindBox = RegisterControl(nameof(CommonBillPartiesUserControl.ConigneeCountryCodeFindBox));
			ConsigneeStateDropEdit = RegisterControl(nameof(CommonBillPartiesUserControl.ConsigneeStateDropEdit));
			ConsigneePostcodeTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.ConsigneePostcodeTextBox));
			ConsigneePhoneTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.ConsigneePhoneTextBox));
			ConsigneeEmailTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.ConsigneeEmailTextBox));
			ConsigneeRegoNoTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.ConsigneeRegoNoTextBox));
			ConsigneeRegNoTypeDropEdit = RegisterControl(nameof(CommonBillPartiesUserControl.ConsigneeRegNoTypeDropEdit));

			NotifyPartySeparatorUserControl = RegisterControl(nameof(CommonBillPartiesUserControl.NotifyPartySeparatorUserControl));
			NotifyPartyAddressControl = RegisterControl(nameof(CommonBillPartiesUserControl.NotifyPartyAddressControl));
			ConvertNotifyPartyToOrganizationButton = RegisterControl(nameof(CommonBillPartiesUserControl.ConvertNotifyPartyToOrganizationButton));
			NotifyPartyNameTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.NotifyPartyNameTextBox));
			NotifyPartyStreet1TextBox = RegisterControl(nameof(CommonBillPartiesUserControl.NotifyPartyStreet1TextBox));
			NotifyPartyStreet2TextBox = RegisterControl(nameof(CommonBillPartiesUserControl.NotifyPartyStreet2TextBox));
			NotifyPartyCityTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.NotifyPartyCityTextBox));
			NotifyPartyCountryCodeFindBox = RegisterControl(nameof(CommonBillPartiesUserControl.NotifyPartyCountryCodeFindBox));
			NotifyPartyStateDropEdit = RegisterControl(nameof(CommonBillPartiesUserControl.NotifyPartyStateDropEdit));
			NotifyPartyPostcodeTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.NotifyPartyPostcodeTextBox));
			NotifyPartyPhoneTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.NotifyPartyPhoneTextBox));
			NotifyPartyRegNoTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.NotifyPartyRegNoTextBox));
			NotifyPartyRegNoTypeDropEdit = RegisterControl(nameof(CommonBillPartiesUserControl.NotifyPartyRegNoTypeDropEdit));

			BuyerSeparatorUserControl = RegisterControl(nameof(CommonBillPartiesUserControl.BuyerSeparatorUserControl));
			BuyerAddressControl = RegisterControl(nameof(CommonBillPartiesUserControl.BuyerAddressControl));
			ConvertBuyerToOrganizationButton = RegisterControl(nameof(CommonBillPartiesUserControl.ConvertBuyerToOrganizationButton));
			BuyerNameTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.BuyerNameTextBox));
			BuyerStreet1TextBox = RegisterControl(nameof(CommonBillPartiesUserControl.BuyerStreet1TextBox));
			BuyerStreet2TextBox = RegisterControl(nameof(CommonBillPartiesUserControl.BuyerStreet2TextBox));
			BuyerCityTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.BuyerCityTextBox));
			BuyerCountryCodeFindBox = RegisterControl(nameof(CommonBillPartiesUserControl.BuyerCountryCodeFindBox));
			BuyerStateDropEdit = RegisterControl(nameof(CommonBillPartiesUserControl.BuyerStateDropEdit));
			BuyerPostcodeTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.BuyerPostcodeTextBox));
			BuyerPhoneTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.BuyerPhoneTextBox));
			BuyerRegNoTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.BuyerRegNoTextBox));
			BuyerRegNoTypeDropEdit = RegisterControl(nameof(CommonBillPartiesUserControl.BuyerRegNoTypeDropEdit));

			SellerSeparatorUserControl = RegisterControl(nameof(CommonBillPartiesUserControl.SellerSeparatorUserControl));
			SellerAddressControl = RegisterControl(nameof(CommonBillPartiesUserControl.SellerAddressControl));
			ConvertSellerToOrganizationButton = RegisterControl(nameof(CommonBillPartiesUserControl.ConvertSellerToOrganizationButton));
			SellerNameTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.SellerNameTextBox));
			SellerStreet1TextBox = RegisterControl(nameof(CommonBillPartiesUserControl.SellerStreet1TextBox));
			SellerStreet2TextBox = RegisterControl(nameof(CommonBillPartiesUserControl.SellerStreet2TextBox));
			SellerCityTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.SellerCityTextBox));
			SellerCountryCodeFindBox = RegisterControl(nameof(CommonBillPartiesUserControl.SellerCountryCodeFindBox));
			SellerStateDropEdit = RegisterControl(nameof(CommonBillPartiesUserControl.SellerStateDropEdit));
			SellerPostcodeTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.SellerPostcodeTextBox));
			SellerPhoneTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.SellerPhoneTextBox));
			SellerRegoNoTextBox = RegisterControl(nameof(CommonBillPartiesUserControl.SellerRegoNoTextBox));
			SellerRegNoTypeDropEdit = RegisterControl(nameof(CommonBillPartiesUserControl.SellerRegNoTypeDropEdit));
		}

		public ControlReference ShipperSeparatorUserControl { get; }
		public ControlReference ShipperAddressControl { get; }
		public ControlReference ConvertShipperToOrganizationButton { get; }
		public ControlReference ShipperNameTextBox { get; }
		public ControlReference ShipperStreet1TextBox { get; }
		public ControlReference ShipperStreet2TextBox { get; }
		public ControlReference ShipperCityTextBox { get; }
		public ControlReference ShipperCountryCodeFindBox { get; }
		public ControlReference ShipperStateDropEdit { get; }
		public ControlReference ShipperPostCodeTextBox { get; }
		public ControlReference ShipperPhoneTextBox { get; }
		public ControlReference ShipperRegNoTextBox { get; }
		public ControlReference ShipperRegNoTypeDropEdit { get; }

		public ControlReference ConsigneeSeparatorUserControl { get; }
		public ControlReference ConsigneeAddressControl { get; }
		public ControlReference ConvertConsigneeToOrganizationButton { get; }
		public ControlReference ConsigneeNameTextBox { get; }
		public ControlReference ConsigneeStreet1TextBox { get; }
		public ControlReference ConsigneeStreet2TextBox { get; }
		public ControlReference ConsigneeCityTextBox { get; }
		public ControlReference ConigneeCountryCodeFindBox { get; }
		public ControlReference ConsigneeStateDropEdit { get; }
		public ControlReference ConsigneePostcodeTextBox { get; }
		public ControlReference ConsigneePhoneTextBox { get; }
		public ControlReference ConsigneeEmailTextBox { get; }
		public ControlReference ConsigneeRegoNoTextBox { get; }
		public ControlReference ConsigneeRegNoTypeDropEdit { get; }

		public ControlReference NotifyPartySeparatorUserControl { get; }
		public ControlReference NotifyPartyAddressControl { get; }
		public ControlReference ConvertNotifyPartyToOrganizationButton { get; }
		public ControlReference NotifyPartyNameTextBox { get; }
		public ControlReference NotifyPartyStreet1TextBox { get; }
		public ControlReference NotifyPartyStreet2TextBox { get; }
		public ControlReference NotifyPartyCityTextBox { get; }
		public ControlReference NotifyPartyCountryCodeFindBox { get; }
		public ControlReference NotifyPartyStateDropEdit { get; }
		public ControlReference NotifyPartyPostcodeTextBox { get; }
		public ControlReference NotifyPartyPhoneTextBox { get; }
		public ControlReference NotifyPartyRegNoTextBox { get; }
		public ControlReference NotifyPartyRegNoTypeDropEdit { get; }

		public ControlReference BuyerSeparatorUserControl { get; }
		public ControlReference BuyerAddressControl { get; }
		public ControlReference ConvertBuyerToOrganizationButton { get; }
		public ControlReference BuyerNameTextBox { get; }
		public ControlReference BuyerStreet1TextBox { get; }
		public ControlReference BuyerStreet2TextBox { get; }
		public ControlReference BuyerCityTextBox { get; }
		public ControlReference BuyerCountryCodeFindBox { get; }
		public ControlReference BuyerStateDropEdit { get; }
		public ControlReference BuyerPostcodeTextBox { get; }
		public ControlReference BuyerPhoneTextBox { get; }
		public ControlReference BuyerRegNoTextBox { get; }
		public ControlReference BuyerRegNoTypeDropEdit { get; }

		public ControlReference SellerSeparatorUserControl { get; }
		public ControlReference SellerAddressControl { get; }
		public ControlReference ConvertSellerToOrganizationButton { get; }
		public ControlReference SellerNameTextBox { get; }
		public ControlReference SellerStreet1TextBox { get; }
		public ControlReference SellerStreet2TextBox { get; }
		public ControlReference SellerCityTextBox { get; }
		public ControlReference SellerCountryCodeFindBox { get; }
		public ControlReference SellerStateDropEdit { get; }
		public ControlReference SellerPostcodeTextBox { get; }
		public ControlReference SellerPhoneTextBox { get; }
		public ControlReference SellerRegoNoTextBox { get; }
		public ControlReference SellerRegNoTypeDropEdit { get; }

		protected override Control CreateTemplate() => new CommonBillPartiesUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<CommonBillPartiesControlBag> commonBillPartiesControlBag = new Lazy<CommonBillPartiesControlBag>(() => new CommonBillPartiesControlBag());
	}
}
