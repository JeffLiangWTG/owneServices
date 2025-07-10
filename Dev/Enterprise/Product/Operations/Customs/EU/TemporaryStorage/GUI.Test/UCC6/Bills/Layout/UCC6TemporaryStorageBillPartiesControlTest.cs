using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	sealed class UCC6TemporaryStorageBillPartiesControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new UCC6TemporaryStorageBillPartiesControl())
			{
				CombineAssertions(() =>
				{
					AssertNotNull(control.FindSingle<ZAddressControl>("ShipperAddressControl"));
					AssertEquals("BindTo", "Bills.ABL_OA_Shipper", control.ShipperAddressControl.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("ShipperNameTextBox"));
					AssertEquals("BindTo", "Bills.ABL_ShipperName", control.ShipperNameTextBox.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("ShipperStreet1TextBox"));
					AssertEquals("BindTo", "Bills.ABL_ShipperStreet1", control.ShipperStreet1TextBox.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("ShipperStreet2TextBox"));
					AssertEquals("BindTo", "Bills.ABL_ShipperStreet2", control.ShipperStreet2TextBox.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("ShipperCityTextBox"));
					AssertEquals("BindTo", "Bills.ABL_ShipperCity", control.ShipperCityTextBox.BindTo);
					AssertNotNull(control.FindSingle<ZCodeFindBox>("ShipperCountryCodeFindBox"));
					AssertEquals("BindTo", "Bills.ABL_RN_NKShipperCountry", control.ShipperCountryCodeFindBox.BindTo);
					AssertNotNull(control.FindSingle<ZDropEdit>("ShipperStateDropEdit"));
					AssertEquals("BindTo", "Bills.ABL_ShipperState", control.ShipperStateDropEdit.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("ShipperPostCodeTextBox"));
					AssertEquals("BindTo", "Bills.ABL_ShipperPostcode", control.ShipperPostCodeTextBox.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("ShipperPhoneTextBox"));
					AssertEquals("BindTo", "Bills.ABL_ShipperPhone", control.ShipperPhoneTextBox.BindTo);
					AssertNotNull(control.FindSingle<ZDropEdit>("ShipperRegNoTypeDropEdit"));
					AssertEquals("BindTo", "Bills.ABL_ShipperRegNoType", control.ShipperRegNoTypeDropEdit.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("ShipperRegNoTextBox"));
					AssertEquals("BindTo", "Bills.ABL_ShipperRegNo", control.ShipperRegNoTextBox.BindTo);
					AssertNotNull(control.FindSingle<ZAddressControl>("ConsigneeAddressControl"));
					AssertEquals("BindTo", "Bills.ABL_OA_Consignee", control.ConsigneeAddressControl.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("ConsigneeNameTextBox"));
					AssertEquals("BindTo", "Bills.ABL_ConsigneeName", control.ConsigneeNameTextBox.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("ConsigneeStreet1TextBox"));
					AssertEquals("BindTo", "Bills.ABL_ConsigneeStreet1", control.ConsigneeStreet1TextBox.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("ConsigneeStreet2TextBox"));
					AssertEquals("BindTo", "Bills.ABL_ConsigneeStreet2", control.ConsigneeStreet2TextBox.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("ConsigneeCityTextBox"));
					AssertEquals("BindTo", "Bills.ABL_ConsigneeCity", control.ConsigneeCityTextBox.BindTo);
					AssertNotNull(control.FindSingle<ZCodeFindBox>("ConigneeCountryCodeFindBox"));
					AssertEquals("BindTo", "Bills.ABL_RN_NKConsigneeCountry", control.ConigneeCountryCodeFindBox.BindTo);
					AssertNotNull(control.FindSingle<ZDropEdit>("ConsigneeStateDropEdit"));
					AssertEquals("BindTo", "Bills.ABL_ConsigneeState", control.ConsigneeStateDropEdit.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("ConsigneePostcodeTextBox"));
					AssertEquals("BindTo", "Bills.ABL_ConsigneePostcode", control.ConsigneePostcodeTextBox.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("ConsigneePhoneTextBox"));
					AssertEquals("BindTo", "Bills.ABL_ConsigneePhone", control.ConsigneePhoneTextBox.BindTo);
					AssertNotNull(control.FindSingle<ZDropEdit>("ConsigneeRegNoTypeDropEdit"));
					AssertEquals("BindTo", "Bills.ABL_ConsigneeRegNoType", control.ConsigneeRegNoTypeDropEdit.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("ConsigneeRegoNoTextBox"));
					AssertEquals("BindTo", "Bills.ABL_ConsigneeRegNo", control.ConsigneeRegoNoTextBox.BindTo);
					AssertNotNull(control.FindSingle<ZAddressControl>("NotifyPartyAddressControl"));
					AssertEquals("BindTo", "Bills.ABL_OA_NotifyParty", control.NotifyPartyAddressControl.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("NotifyPartyNameTextBox"));
					AssertEquals("BindTo", "Bills.ABL_NotifyPartyName", control.NotifyPartyNameTextBox.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("NotifyPartyStreet1TextBox"));
					AssertEquals("BindTo", "Bills.ABL_NotifyPartyStreet1", control.NotifyPartyStreet1TextBox.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("NotifyPartyStreet2TextBox"));
					AssertEquals("BindTo", "Bills.ABL_NotifyPartyStreet2", control.NotifyPartyStreet2TextBox.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("NotifyPartyCityTextBox"));
					AssertEquals("BindTo", "Bills.ABL_NotifyPartyCity", control.NotifyPartyCityTextBox.BindTo);
					AssertNotNull(control.FindSingle<ZCodeFindBox>("NotifyPartyCountryCodeFindBox"));
					AssertEquals("BindTo", "Bills.ABL_RN_NKNotifyPartyCountry", control.NotifyPartyCountryCodeFindBox.BindTo);
					AssertNotNull(control.FindSingle<ZDropEdit>("NotifyPartyStateDropEdit"));
					AssertEquals("BindTo", "Bills.ABL_NotifyPartyState", control.NotifyPartyStateDropEdit.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("NotifyPartyPostcodeTextBox"));
					AssertEquals("BindTo", "Bills.ABL_NotifyPartyPostcode", control.NotifyPartyPostcodeTextBox.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("NotifyPartyPhoneTextBox"));
					AssertEquals("BindTo", "Bills.ABL_NotifyPartyPhone", control.NotifyPartyPhoneTextBox.BindTo);
					AssertNotNull(control.FindSingle<ZDropEdit>("NotifyPartyRegNoTypeDropEdit"));
					AssertEquals("BindTo", "Bills.ABL_NotifyPartyRegNoType", control.NotifyPartyRegNoTypeDropEdit.BindTo);
					AssertNotNull(control.FindSingle<ZTextBox>("NotifyPartyRegNoTextBox"));
					AssertEquals("BindTo", "Bills.ABL_NotifyPartyRegNo", control.NotifyPartyRegNoTextBox.BindTo);
				});
			}
		}
	}
}
