using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI.UserControls.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(OrganisationsUserControl))]
	sealed class OrganisationsUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using var control = new OrganisationsUserControl();
			TestHelper.AssertControlExists(control, "DeclarationConsignorAddressControl", "DeclarationConsignorAddress");
			TestHelper.AssertControlExists(control, "DeclarationConsigneeAddressControl", "DeclarationConsigneeAddress");
			TestHelper.AssertControlExists(control, "PowerOfAttorneyTextBox", "JE_ACP_POA");
			TestHelper.AssertControlExists(control, "AttorneyForCustomsProcedureGroupBox", string.Empty);
			TestHelper.AssertControlExists(control, "AttorneyForCustomsProcedureDocAddressControl", "AttorneyForCustomsProceduresAddress");
			TestHelper.AssertControlExists(control, "InspectionWitnessDocAddressControl", "InspectionWitness");
			TestHelper.AssertControlExists(control, "InspectionWitnessCodeTextBox", "InspectionWitnessCode");
			TestHelper.AssertControlExists(control, "InspectionWitnessGroupBox", string.Empty);
			TestHelper.AssertControlExists(control, "ExternalBrokerDocAddressControl", "ExternalBrokerAddress");
			TestHelper.AssertControlExists(control, "ExternalBrokerCodeTextBox", "ExternalBrokerCode");
			TestHelper.AssertControlExists(control, "ExternalBrokerGroupBox", string.Empty);
			TestHelper.AssertControlExists(control, "AirCargoAgentDocAddressControl", "AirCargoAgent");
			TestHelper.AssertControlExists(control, "AirCargoAgentNACCSCodeTextBox", "AirCargoAgentNACCSCode");
			TestHelper.AssertControlExists(control, "AirCargoAgentLocationCodeTextBox", "AirCargoAgentLocationCode");
			TestHelper.AssertControlExists(control, "AirCargoAgentGroupBox", string.Empty);
			TestHelper.AssertControlExists(control, "CarrierCodeCodeFindBox", "JE_CarrierCode");
			TestHelper.AssertControlExists(control, "ShippingOrAirLineOrganisationGuidFindBox", "JE_OH_ShippingLine");
			TestHelper.AssertControlExists(control, "CarrierGroupBox", string.Empty);
			TestHelper.AssertControlExists(control, "ForwarderDocAddressControl", "ForwarderAddress");
			TestHelper.AssertControlExists(control, "ForwarderTextBox", "ForwarderCode");
			TestHelper.AssertControlExists(control, "ForwarderGroupBox", string.Empty);
		}
	}
}
