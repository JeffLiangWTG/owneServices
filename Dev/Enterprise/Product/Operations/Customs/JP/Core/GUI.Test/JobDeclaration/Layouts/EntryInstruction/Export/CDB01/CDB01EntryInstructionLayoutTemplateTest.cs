using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI.UserControls.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing;

[TestedType(typeof(CDB01EntryInstructionLayoutTemplate))]
sealed class CDB01EntryInstructionLayoutTemplateTest : TestCaseWithFactory
{
	public void TestControls()
	{
		using (var control = new CDB01EntryInstructionLayoutTemplate())
		{
			TestHelper.AssertControlExists(control, "DateForDutyDateEdit", "CEI_DateForDuty");
			TestHelper.AssertControlExists(control, "CDB01CargoTypeDropEdit", "CEI_CDB01CargoType");
			TestHelper.AssertControlExists(control, "CDB01PermitNumberTextBox", "CEI_CDB01PermitNo");
			TestHelper.AssertControlExists(control, "CDB01MoveInUserControl", ".");
			TestHelper.AssertControlExists(control, "CDB01BillNumberUserControl", ".");
			TestHelper.AssertControlExists(control, "FinalDestinationCodeFindBox", "JobDeclaration.JE_RL_NKFinalDestination");
			TestHelper.AssertControlExists(control, "FinalDestinationTextBox", "JobDeclaration.FinalDestinationIATACode");
			TestHelper.AssertControlExists(control, "CarrierGroupBox", "");
			TestHelper.AssertControlExists(control, "CarrierCodeCodeFindBox", "JobDeclaration.JE_CarrierCode");
			TestHelper.AssertControlExists(control, "ShippingOrAirLineOrganisationGuidFindBox", "JobDeclaration.JE_OH_ShippingLine");
			TestHelper.AssertControlExists(control, "ForwarderGroupBox", "");
			TestHelper.AssertControlExists(control, "ForWarderNACCSCodeTextBox", "JobDeclaration.ForwarderCode");
			TestHelper.AssertControlExists(control, "AirCargoAgentGroupBox", "");
			TestHelper.AssertControlExists(control, "AirCargoAgentDocAddressControl", "JobDeclaration.AirCargoAgent");
			TestHelper.AssertControlExists(control, "AirCargoAgentNACCSCodeTextBox", "JobDeclaration.AirCargoAgentNACCSCode");
			TestHelper.AssertControlExists(control, "AirCargoAgentLocationCodeTextBox", "JobDeclaration.AirCargoAgentLocationCode");
			TestHelper.AssertControlExists(control, "ExternalBrokerGroupBox", "");
			TestHelper.AssertControlExists(control, "ExternalBrokerDocAddressControl", "JobDeclaration.ExternalBrokerAddress");
			TestHelper.AssertControlExists(control, "ExternalBrokerCodeTextBox", "JobDeclaration.ExternalBrokerCode");
			TestHelper.AssertControlExists(control, "PortOfLoadingCodeFindBox", "JobDeclaration.JE_RL_NKPortOfLoading");
			TestHelper.AssertControlExists(control, "PortOfLoadingTextBox", "JobDeclaration.PortOfLoadingIATACode");
			TestHelper.AssertControlExists(control, "MAWBTextBox", "JobDeclaration.JE_MasterBill");
			TestHelper.AssertControlExists(control, "PortOfLoadingPanel", "");
			TestHelper.AssertControlExists(control, "FinalDestinationPanel", "");
			TestHelper.AssertControlExists(control, "ForwarderDocAddressControl", "JobDeclaration.ForwarderAddress");
		}
	}
}
