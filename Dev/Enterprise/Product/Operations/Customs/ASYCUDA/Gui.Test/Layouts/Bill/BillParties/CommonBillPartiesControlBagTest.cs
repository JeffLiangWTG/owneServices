using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(CommonBillPartiesControlBag))]
	sealed class CommonBillPartiesControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CommonBillPartiesControlBag.ShipperSeparatorUserControl);
				yield return nameof(CommonBillPartiesControlBag.ShipperAddressControl);
				yield return nameof(CommonBillPartiesControlBag.ConvertShipperToOrganizationButton);
				yield return nameof(CommonBillPartiesControlBag.ShipperNameTextBox);
				yield return nameof(CommonBillPartiesControlBag.ShipperStreet1TextBox);
				yield return nameof(CommonBillPartiesControlBag.ShipperStreet2TextBox);
				yield return nameof(CommonBillPartiesControlBag.ShipperCityTextBox);
				yield return nameof(CommonBillPartiesControlBag.ShipperCountryCodeFindBox);
				yield return nameof(CommonBillPartiesControlBag.ShipperStateDropEdit);
				yield return nameof(CommonBillPartiesControlBag.ShipperPhoneTextBox);
				yield return nameof(CommonBillPartiesControlBag.ShipperPostCodeTextBox);
				yield return nameof(CommonBillPartiesControlBag.ShipperRegNoTextBox);
				yield return nameof(CommonBillPartiesControlBag.ShipperRegNoTypeDropEdit);
				yield return nameof(CommonBillPartiesControlBag.ConsigneeSeparatorUserControl);
				yield return nameof(CommonBillPartiesControlBag.ConsigneeAddressControl);
				yield return nameof(CommonBillPartiesControlBag.ConvertConsigneeToOrganizationButton);
				yield return nameof(CommonBillPartiesControlBag.ConsigneeNameTextBox);
				yield return nameof(CommonBillPartiesControlBag.ConsigneeStreet1TextBox);
				yield return nameof(CommonBillPartiesControlBag.ConsigneeStreet2TextBox);
				yield return nameof(CommonBillPartiesControlBag.ConsigneeCityTextBox);
				yield return nameof(CommonBillPartiesControlBag.ConigneeCountryCodeFindBox);
				yield return nameof(CommonBillPartiesControlBag.ConsigneeStateDropEdit);
				yield return nameof(CommonBillPartiesControlBag.ConsigneePostcodeTextBox);
				yield return nameof(CommonBillPartiesControlBag.ConsigneePhoneTextBox);
				yield return nameof(CommonBillPartiesControlBag.ConsigneeEmailTextBox);
				yield return nameof(CommonBillPartiesControlBag.ConsigneeRegoNoTextBox);
				yield return nameof(CommonBillPartiesControlBag.ConsigneeRegNoTypeDropEdit);
				yield return nameof(CommonBillPartiesControlBag.NotifyPartySeparatorUserControl);
				yield return nameof(CommonBillPartiesControlBag.NotifyPartyAddressControl);
				yield return nameof(CommonBillPartiesControlBag.ConvertNotifyPartyToOrganizationButton);
				yield return nameof(CommonBillPartiesControlBag.NotifyPartyNameTextBox);
				yield return nameof(CommonBillPartiesControlBag.NotifyPartyStreet1TextBox);
				yield return nameof(CommonBillPartiesControlBag.NotifyPartyStreet2TextBox);
				yield return nameof(CommonBillPartiesControlBag.NotifyPartyCityTextBox);
				yield return nameof(CommonBillPartiesControlBag.NotifyPartyCountryCodeFindBox);
				yield return nameof(CommonBillPartiesControlBag.NotifyPartyStateDropEdit);
				yield return nameof(CommonBillPartiesControlBag.NotifyPartyPostcodeTextBox);
				yield return nameof(CommonBillPartiesControlBag.NotifyPartyPhoneTextBox);
				yield return nameof(CommonBillPartiesControlBag.NotifyPartyRegNoTextBox);
				yield return nameof(CommonBillPartiesControlBag.NotifyPartyRegNoTypeDropEdit);
				yield return nameof(CommonBillPartiesControlBag.BuyerSeparatorUserControl);
				yield return nameof(CommonBillPartiesControlBag.BuyerAddressControl);
				yield return nameof(CommonBillPartiesControlBag.ConvertBuyerToOrganizationButton);
				yield return nameof(CommonBillPartiesControlBag.BuyerNameTextBox);
				yield return nameof(CommonBillPartiesControlBag.BuyerStreet1TextBox);
				yield return nameof(CommonBillPartiesControlBag.BuyerStreet2TextBox);
				yield return nameof(CommonBillPartiesControlBag.BuyerCityTextBox);
				yield return nameof(CommonBillPartiesControlBag.BuyerCountryCodeFindBox);
				yield return nameof(CommonBillPartiesControlBag.BuyerStateDropEdit);
				yield return nameof(CommonBillPartiesControlBag.BuyerPostcodeTextBox);
				yield return nameof(CommonBillPartiesControlBag.BuyerPhoneTextBox);
				yield return nameof(CommonBillPartiesControlBag.BuyerRegNoTextBox);
				yield return nameof(CommonBillPartiesControlBag.BuyerRegNoTypeDropEdit);
				yield return nameof(CommonBillPartiesControlBag.SellerSeparatorUserControl);
				yield return nameof(CommonBillPartiesControlBag.SellerAddressControl);
				yield return nameof(CommonBillPartiesControlBag.ConvertSellerToOrganizationButton);
				yield return nameof(CommonBillPartiesControlBag.SellerNameTextBox);
				yield return nameof(CommonBillPartiesControlBag.SellerStreet1TextBox);
				yield return nameof(CommonBillPartiesControlBag.SellerStreet2TextBox);
				yield return nameof(CommonBillPartiesControlBag.SellerCityTextBox);
				yield return nameof(CommonBillPartiesControlBag.SellerCountryCodeFindBox);
				yield return nameof(CommonBillPartiesControlBag.SellerStateDropEdit);
				yield return nameof(CommonBillPartiesControlBag.SellerPostcodeTextBox);
				yield return nameof(CommonBillPartiesControlBag.SellerPhoneTextBox);
				yield return nameof(CommonBillPartiesControlBag.SellerRegoNoTextBox);
				yield return nameof(CommonBillPartiesControlBag.SellerRegNoTypeDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CommonBillPartiesControlBag.Instance;
	}
}
