using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class HouseBillDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestCreateControl()
		{
			Env.Registry.ConsolPaymentTerm = Core.Constants.PaymentType.Collect;
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OB001";
			var house = oceanBill.FilteredHouseBills.AddNew();
			house.CA_HouseBill = "HB123";
			house.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			house.CA_IsMasterHouse = true;
			house.CA_MasterHouseBill = "MHB456";
			house.CA_RN_NKGoodsOrigin = "NZ";
			house.CA_RL_NK_PortOfOrigin = "USLAX";
			house.CA_RL_NK_PortOfDestination = "AUMEL";
			AssertEquals("Precondition", CMRMethodsOfPayment.Codes.Collect, house.CA_PrepaidCollectOther);
			house.Pivot.AddNew().CV_CargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			using (var testForm = new ZForm(house))
			using (var houseBillDetailsUserControl = new HouseBillDetailsUserControl())
			{
				testForm.Controls.Add(houseBillDetailsUserControl);
				houseBillDetailsUserControl.SetDataBinding(house, "");
				testForm.Show();
				AssertEquals("HB123", testForm.FindSingle<ZArchitecture.ZTextBox>("CA_HouseBillBoundTextBox").Text);
				AssertEquals(((ZString)CMRBaseStatuses.Descriptions.OriginalAccepted).ToUpper(), testForm.FindSingle<ZArchitecture.ZTextBox>("MessageStatusTextBox").Text);
				AssertEquals(true, testForm.FindSingle<ZCheckBox>("FreightForwarderIndicatorCheckBox").Checked);
				AssertEquals("MHB456", testForm.FindSingle<ZArchitecture.ZTextBox>("ParentBillTextBox").Text);
				AssertEquals("NZ", testForm.FindSingle<ZCodeFindBox>("CA_RN_NKGoodsOriginBoundCodeFindBox").Text);
				AssertEquals("USLAX", testForm.FindSingle<ZCodeFindBox>("CA_RL_NK_PortOfOriginBoundCodeFindBox").Text);
				AssertEquals("AUMEL", testForm.FindSingle<ZCodeFindBox>("CA_RL_NK_PortOfDestinationBoundCodeFindBox").Text);
				AssertEquals(CMRMethodsOfPayment.Codes.Collect, testForm.FindSingle<ZDropEdit>("CA_PrepaidCollectOtherBoundDropEdit").Text);
				AssertEquals(((ZString)CMRConsolidatedCargoStatuses.Descriptions.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased).ToUpper(), testForm.FindSingle<ZArchitecture.ZTextBox>("CA_ShipmentStatusBoundTextBox").Text);
			}
		}

		public void TestDetailsButton()
		{
			string carstMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2HF2 5CBC D7BF:1+8'
DTM+9:20051103122504817010:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++LCL UNDERBOND SATISFIED:NO'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+QA123++11++++9044748::11'
LOC+12+AUSYD::6'
LOC+4+9122P::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:H00001052/SYD1::1'
RFF+MB:1234564'
RFF+AAQ:LCLU99999999'
DOC+1'
PAC+++LCL:67:95'
UNT+34+000001'".Replace("\r\n", "");
			string statusResult = @"CONSOLIDATED STATUS : HELD
COMPLETE UNDERBOND SERIES APPROVED : N/A
LCL UNDERBOND SATISFIED : NO
CARGO REPORT ACS EVALUATED : NO
IMPORT DECLARATIONS MATCHED : N/A
IMPORT DECLARATION ACS EVALUATED : N/A
IMPORT DECLARATION AQIS EVALUATED : N/A
ACS IMPORT DECLARATION EVALUATION COMPLETE : N/A
AQIS IMPORT DECLARATION EVALUATION COMPLETE : N/A
IMPORT DECLARATION PAID : N/A
CARGO REPORT SAC : NO

========================================
Warning: Cargo is not a consolidation.
========================================";
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OB001";
			var container = oceanBill.Containers.AddNew();
			var house = oceanBill.FilteredHouseBills.AddNew();
			house.CA_HouseBill = "HB123";
			container.Pivots.Add(house.Pivot.AddNew());
			using (var testForm = new ZForm(house))
			using (var houseBillDetailsUserControl = new HouseBillDetailsUserControl())
			{
				testForm.Controls.Add(houseBillDetailsUserControl);
				houseBillDetailsUserControl.SetDataBinding(house, "");
				testForm.Show();
				testForm.FindSingle<ZButton>("DetailsButton").PerformClick();
				AssertEquals(UserFriendlyStatusMessages.StatusNotAvailable, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var message = Factory.New<CMRCARSTMessage>();
			message.EM_ReceiveTransmit = Messaging.Business.EDIInterchange.Direction.Receive;
			message.EM_Status = Messaging.Integration.EDIMessageStatusList.Codes.Received;
			message.EM_MessageText = carstMessage;
			house.Pivot[0].Messages.Add(message);
			using (var testForm = new ZForm(house))
			using (var houseBillDetailsUserControl = new HouseBillDetailsUserControl())
			{
				testForm.Controls.Add(houseBillDetailsUserControl);
				houseBillDetailsUserControl.SetDataBinding(house, "");
				testForm.Show();
				testForm.FindSingle<ZButton>("DetailsButton").PerformClick();
				AssertEquals(statusResult, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUpdateForOceanBillUnpack()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = oceanBill.FilteredHouseBills.AddNew();
			using (var testForm = new ZForm(house))
			using (var houseBillDetailsUserControl = new HouseBillDetailsUserControl())
			{
				testForm.Controls.Add(houseBillDetailsUserControl);
				houseBillDetailsUserControl.SetDataBinding(house, "");
				testForm.Show();
				AssertEquals("House Bill:", testForm.FindSingle<Control>("HouseBillLabel").Text);
				AssertEquals("House Bill", testForm.FindSingle<Control>("GroupBoxHouseBill").Text);
				oceanBill.CB_MultiOBLUnpack = true;
				houseBillDetailsUserControl.UpdateForOceanBillUnpack();
				AssertEquals("Ocean Bill:", testForm.FindSingle<Control>("HouseBillLabel").Text);
				AssertEquals("Ocean Bill", testForm.FindSingle<Control>("GroupBoxHouseBill").Text);
			}
		}
	}
}
