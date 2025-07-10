using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class VoyageDetailsControlTest : TestCaseWithFactory
	{
		public void TestUserFriendlyStatusOnBill()
		{
			var oceanBill1 = transportHeader.OceanBills.AddNew();
			var oceanBill2 = transportHeader.OceanBills.AddNew();
			var message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
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
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_Status = "RCV";
			message.EM_MessageType = "CRS";
			message.EM_MessageSubType = "CRS";
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			oceanBill2.Messages.Add(message);
			using (var testForm = new ZForm(transportHeader))
			{
				using (var control = new VoyageDetailsControl())
				{
					testForm.Controls.Add(control);
					control.SetDataBinding(transportHeader, "");
					testForm.Show();
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
					var oceanBillGrid = control.FindSingle<ZGrid>("OceanBillGrid");
					oceanBillGrid.ListManager.Position = oceanBillGrid.List.IndexOf(oceanBill2);
					control.BillStatusDetailsButton_Click(null, new EventArgs());
					AssertEquals(statusResult, UnitTestUserNotification.Instance.LastMessage.Text);
					oceanBillGrid.ListManager.Position = oceanBillGrid.List.IndexOf(oceanBill1);
					control.BillStatusDetailsButton_Click(null, new EventArgs());
					AssertEquals(UserFriendlyStatusMessages.StatusNotAvailable, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestUserFriendlyStatusOnCargoLine()
		{
			transportHeader.OceanBills.AddNew();
			var arrival1 = transportHeader.Arrivals.AddNew();
			var cargoLine1 = arrival1.CargoLines.AddNew();
			var cargoLine2 = arrival1.CargoLines.AddNew();
			arrival1.CargoLines.AddNew();
			Factory.Save();
			var message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000004+CUSRES:D:99B:UN'
BGM+34:::CARST+2D4H 75J9 C4AI:1+8'
DTM+9:20180706133245212750:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+4365++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:A00000532/CMT1::1'
RFF+AAQ:CN002'
RFF+ACC:E'
DOC+1'
PAC+++FCL:67:95'
UNT+16+000004'".Replace("\r\n", "");
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_Status = "RCV";
			message.EM_MessageType = "CRS";
			message.EM_MessageSubType = "CRS";
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			cargoLine2.Messages.Add(message);
			Factory.Save();
			using (var form = new ZForm(transportHeader))
			{
				using (var control = new VoyageDetailsControl())
				{
					form.Controls.Add(control);
					control.SetDataBinding(transportHeader, "");
					form.Show();
					var tabControl = control.FindSingle<ZTemplateTabControl>("TabControl");
					var arrivalInformationTabPage = control.FindSingle<ZTabPage>("ArrivalInformationTabPage");
					tabControl.SelectedTab = arrivalInformationTabPage;
					var arrivalTabControl = control.FindSingle<ZTemplateTabControl>("ArrivalTabControl");
					var cargoLineTabPage = control.FindSingle<ZTabPage>("CargoLineTabPage");
					arrivalTabControl.SelectedTab = cargoLineTabPage;
					string statusResult = @"CONSOLIDATED STATUS : CLEAR
CARGO REPORT SAC : N/A
";
					var cargoLinesGrid = control.FindSingle<ZGrid>("CargoLinesGrid");
					cargoLinesGrid.ListManager.Position = cargoLinesGrid.List.IndexOf(cargoLine2);
					control.CargoLineStatusDetailsButton_Click(null, new EventArgs());
					AssertEquals(statusResult, UnitTestUserNotification.Instance.LastMessage.Text);
					cargoLinesGrid.ListManager.Position = cargoLinesGrid.List.IndexOf(cargoLine1);
					control.CargoLineStatusDetailsButton_Click(null, new EventArgs());
					AssertEquals(UserFriendlyStatusMessages.StatusNotAvailable, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestOceanBillMessagesTabBinding()
		{
			using (var form = new ZForm(transportHeader))
			{
				using (var control = new VoyageDetailsControl())
				{
					form.Controls.Add(control);
					form.Show();
					var oceanBillMessagesUserControl = control.FindSingle<EDIMessageUserControl>("OceanBillMessagesUserControl");
					AssertEquals("MessagesGrid.BindTo", "OceanBillsView.Messages", oceanBillMessagesUserControl.MessagesGrid.BindTo);
					AssertEquals("MessageTextTextBox.BindTo", "OceanBillsView.Messages.EM_FormattedMessageText", oceanBillMessagesUserControl.MessageTextTextBox.BindTo);
				}
			}
		}

		public void TestArrivalsMessagesTabBinding()
		{
			using (var form = new ZForm(transportHeader))
			{
				using (var control = new VoyageDetailsControl())
				{
					form.Controls.Add(control);
					form.Show();
					var arrivalMessagesUserControl = control.FindSingle<EDIMessageUserControl>("ArrivalMessagesUserControl");
					AssertEquals("MessagesGrid.BindTo", "Arrivals.ActualArrivalMessages", arrivalMessagesUserControl.MessagesGrid.BindTo);
					AssertEquals("MessageTextTextBox.BindTo", "Arrivals.ActualArrivalMessages.EM_FormattedMessageText", arrivalMessagesUserControl.MessageTextTextBox.BindTo);
				}
			}
		}

		public void TestCargoListMessagesTabBinding()
		{
			using (var form = new ZForm(transportHeader))
			{
				using (var control = new VoyageDetailsControl())
				{
					form.Controls.Add(control);
					form.Show();
					var cargoListMessageUserControl = control.FindSingle<EDIMessageUserControl>("CargoListMessageUserControl");
					AssertEquals("MessagesGrid.BindTo", "Arrivals.CargoListAndLineMessagesCombined", cargoListMessageUserControl.MessagesGrid.BindTo);
					AssertEquals("MessageTextTextBox.BindTo", "Arrivals.CargoListAndLineMessagesCombined.EM_FormattedMessageText", cargoListMessageUserControl.MessageTextTextBox.BindTo);
				}
			}
		}

		public void TestCargoListLinesGridHasShipmentStatusColumn()
		{
			using (var form = new ZForm(transportHeader))
			{
				using (var control = new VoyageDetailsControl())
				{
					form.Controls.Add(control);
					form.Show();
					var cargoLinesGrid = control.FindSingle<ZGrid>("CargoLinesGrid");
					AssertNotNull("Shipment Status exists", cargoLinesGrid.GetColumnStyle("ShipmentStatus+Description"));
				}
			}
		}

		CusSeaManTranHead transportHeader;
		protected override void SetUp()
		{
			base.SetUp();
			transportHeader = Factory.New<CusSeaManTranHead>();
		}
	}
}
