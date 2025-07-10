using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class CustomsDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestCustomsDetailsUserControl()
		{
			using (var control = new CustomsDetailsUserControl())
			{
				AssertEquals(true, control.FindSingle<ZGuidDropEdit>("BillZGuidDropEdit").Visible);
				AssertEquals(true, control.FindSingle<ZTextBox>("CargoManagementNoTextBox").Visible);
				AssertEquals(true, control.FindSingle<ZDropEdit>("COStatusDropEdit").Visible);
				AssertEquals(true, control.FindSingle<ZDropEdit>("ValuationDeclarationStatusDropEdit").Visible);
				AssertEquals(true, control.FindSingle<ZTextBox>("BlanketValuationDeclarationNoTextBox").Visible);
				AssertEquals(true, control.FindSingle<ZTextBox>("CustomsBrokerCommentMultiTextBox").Visible);
				AssertEquals(true, control.FindSingle<ZOrganisationFindBox>("SupplierZOrganisationFindBox").Visible);
				AssertEquals(true, control.FindSingle<ZAddressControl>("ShipperZAddressControl").Visible);
				AssertEquals(true, control.FindSingle<ZLabel>("EmptyLabel").Visible);
				AssertEquals(true, control.FindSingle<ZDropEdit>("OnlineTradeTypeDropEdit").Visible);
				AssertEquals(true, control.FindSingle<ZAddressControl>("OnlineTradeDistributorZAddressControl").Visible);
				AssertEquals(true, control.FindSingle<ZAddressControl>("OnlineTradeSellerZAddressControl").Visible);
				AssertEquals(true, control.FindSingle<ZOrganisationFindBox>("OnlineTradeSellingAgentZOrganisationFindBox").Visible);
			}
		}
	}
}
