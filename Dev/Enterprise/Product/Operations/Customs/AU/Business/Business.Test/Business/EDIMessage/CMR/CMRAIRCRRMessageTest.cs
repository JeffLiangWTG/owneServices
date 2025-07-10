using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRAIRCRRMessage))]
	sealed class CMRAIRCRRMessageTest : CMRCUSRESMessageTest
	{
		public void TestGetWrappedObject()
		{
			CMRAIRCRRMessage testMessage = (CMRAIRCRRMessage)HAWB.Messages.AddNew(typeof(CMRAIRCRRMessage));
			testMessage.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIRCRR+418B H57G D3BE:001+11'NAD+MR+AAA374M:110:95'RFF+ACW:AIRCR'RFF+AFM:9'RFF+ABO:A00001000/1::001'DTM+310:20041210050258:204'ERP+1'ERC+ADVICE:80:95'ERC+XX9999:6:95'FTX+AAO+++THIS TRANSACTION WAS REJECTED.'ERP+1'ERC+ERROR:80:95'ERC+CG1457:6:95'FTX+AAO+++ORIGINAL LOADING PORT IS NOT VALID'ERP+1'ERC+ERROR:80:95'ERC+CG0038:6:95'FTX+AAO+++WAYBILL ORIGIN COUNTRY CODE MUST NOT BE \"AU\"'ERP+1'ERC+ERROR:80:95'ERC+CL0108:6:95'FTX+AAO+++ANOTHER PROCESS HAS CHANGED THE DATA. PLEASE REFRESH SCREEN (OR RETRY FOR EDI)'CNT+55:003'UNT+25+000001'";
			HAWB.CS_MessageReference = "A00001000";
			testMessage.SetEM_LinkedObject();
			AssertEquals("LinkedObject", HAWB, testMessage.EM_LinkedObject);
		}

		public void TestReport()
		{
			AssertEquals("Report", "MASTER BILL DETAILS:\r\nMAWB: 123\r\nLoad Port: NZAKL\r\nDischarge Port: AUSYD\r\n\r\nHOUSE BILL DETAILS:\r\nMessage Reference: 321\r\nHAWB: 246\r\nOrigin: NZAKL\r\nDestination: AUSYD\r\n\r\nStatus: ORIGINAL REJECTED\r\nStatus Description: THIS TRANSACTION WAS REJECTED.\r\n\r\n\r\nErrors:\r\n\tORIGINAL LOADING PORT IS NOT VALID\r\n\tWAYBILL ORIGIN COUNTRY CODE MUST NOT BE \"AU\"\r\n\tANOTHER PROCESS HAS CHANGED THE DATA. PLEASE REFRESH SCREEN (OR RETRY FOR EDI)\r\n", Message.Report);
		}

		public void TestReportForWithdrawalRejected()
		{
			Message.EM_MessageText = Message.EM_MessageText.Replace("RFF+AFM:9'", "RFF+AFM:50'");
			AssertEquals("Report", "MASTER BILL DETAILS:\r\nMAWB: 123\r\nLoad Port: NZAKL\r\nDischarge Port: AUSYD\r\n\r\nHOUSE BILL DETAILS:\r\nMessage Reference: 321\r\nHAWB: 246\r\nOrigin: NZAKL\r\nDestination: AUSYD\r\n\r\nStatus: WITHDRAW REJECTED\r\nStatus Description: THIS TRANSACTION WAS REJECTED.\r\n\r\n\r\nErrors:\r\n\tORIGINAL LOADING PORT IS NOT VALID\r\n\tWAYBILL ORIGIN COUNTRY CODE MUST NOT BE \"AU\"\r\n\tANOTHER PROCESS HAS CHANGED THE DATA. PLEASE REFRESH SCREEN (OR RETRY FOR EDI)\r\n", Message.Report);
		}

		public void TestReportForWithdrawalAccepted()
		{
			Message.EM_MessageText = WithdrawalAcceptedMessageText;
			AssertEquals("Report", "MASTER BILL DETAILS:\r\nMAWB: 123\r\nLoad Port: NZAKL\r\nDischarge Port: AUSYD\r\n\r\nHOUSE BILL DETAILS:\r\nMessage Reference: 321\r\nHAWB: 246\r\nOrigin: NZAKL\r\nDestination: AUSYD\r\n\r\nStatus: WITHDRAW ACCEPTED\r\nStatus Description: THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS\r\n", Message.Report);
		}

		const string WithdrawalAcceptedMessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIRCRR+17HG 75G7 DHF5:001+11'NAD+MR+AAA374M:110:95'RFF+ACW:AIRCR'RFF+AFM:50'RFF+ABO:S00039443/1::002'DTM+310:20050111024135:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5203:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'CNT+55:000'UNT+13+000001'";

		CusMAWB MAWB
		{
			get
			{
				if (fMAWB == null)
				{
					fMAWB = Factory.New<CusMAWB>();
					fMAWB.CM_MAWB = "123";
					fMAWB.CM_RL_NKLoadPort = "NZAKL";
					fMAWB.CM_RL_NKDischargePort = "AUSYD";
				}
				return fMAWB;
			}
		}
		CusMAWB fMAWB;

		CusHAWB HAWB
		{
			get
			{
				if (fHAWB == null)
				{
					fHAWB = MAWB.ChildBills.AddNew();
					fHAWB.CS_MessageReference = "321";
					fHAWB.CS_HAWB = "246";
					fHAWB.CS_RL_NKOrigin = "NZAKL";
					fHAWB.CS_RL_NKDestination = "AUSYD";
				}
				return fHAWB;
			}
		}
		CusHAWB fHAWB;

		CMRAIRCRRMessage Message
		{
			get
			{
				if (fMessage == null)
				{
					fMessage = (CMRAIRCRRMessage)HAWB.Messages.AddNew(typeof(CMRAIRCRRMessage));
					fMessage.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIRCRR+418B H57G D3BE:001+11'NAD+MR+AAA374M:110:95'RFF+ACW:AIRCR'RFF+AFM:9'RFF+ABO:S00001540/1::001'DTM+310:20041210050258:204'ERP+1'ERC+ADVICE:80:95'ERC+XX9999:6:95'FTX+AAO+++THIS TRANSACTION WAS REJECTED.'ERP+1'ERC+ERROR:80:95'ERC+CG1457:6:95'FTX+AAO+++ORIGINAL LOADING PORT IS NOT VALID'ERP+1'ERC+ERROR:80:95'ERC+CG0038:6:95'FTX+AAO+++WAYBILL ORIGIN COUNTRY CODE MUST NOT BE \"AU\"'ERP+1'ERC+ERROR:80:95'ERC+CL0108:6:95'FTX+AAO+++ANOTHER PROCESS HAS CHANGED THE DATA. PLEASE REFRESH SCREEN (OR RETRY FOR EDI)'CNT+55:003'UNT+25+000001'";
				}
				return fMessage;
			}
		}
		CMRAIRCRRMessage fMessage;
	}
}
