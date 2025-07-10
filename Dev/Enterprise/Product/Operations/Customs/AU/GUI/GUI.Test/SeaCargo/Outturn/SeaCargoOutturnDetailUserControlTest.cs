using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoOutturnDetailUserControlTest : TestCaseWithFactory
	{
		public void TestDetailsButtonWithoutDetails()
		{
			CusOutturn outturn = Factory.New<CusOutturn>();
			using (SeaCargoOutturnDetailUserControl testControl = new SeaCargoOutturnDetailUserControl())
			{
				testControl.CurrentOutturn = outturn;
				testControl.Show();
				testControl.DetailsButton_Click(null, new System.EventArgs());
				AssertMultilineASCIIEquals("Should Display specified message", "Information " + UserFriendlyStatusMessages.StatusNotAvailable, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestDetailsButtonWithDetails()
		{
			CusOutturn outturn = Factory.New<CusOutturn>();
			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_Status = "RCV";
			message.EM_MessageType = "CRS";
			message.EM_MessageSubType = "CRS";
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
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
			outturn.Messages.Add(message);
			using (SeaCargoOutturnDetailUserControl testControl = new SeaCargoOutturnDetailUserControl())
			{
				testControl.CurrentOutturn = outturn;
				testControl.Show();
				testControl.DetailsButton_Click(null, new System.EventArgs());
				string statusResult = @"Information CONSOLIDATED STATUS : HELD
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
				AssertMultilineASCIIEquals("Should Display simple details", statusResult, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}
	}
}
