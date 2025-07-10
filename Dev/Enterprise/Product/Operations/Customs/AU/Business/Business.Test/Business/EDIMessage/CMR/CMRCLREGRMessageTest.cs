using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCLREGRMessage))]
	sealed class CMRCLREGRMessageTest : CMRCUSRESMessageTest
	{
		public void TestSendersReference()
		{
			AssertEquals("DOCMSGNUM", Message.SendersReference);
		}

		public void TestGetErrorsArrayList()
		{
			AssertEquals(1, RejectionMessage.GetErrorsArrayList().Count);
		}

		public void TestGetStatus()
		{
			AssertEquals("ORIGINAL ACCEPTED", Message.GetStatus());
		}

		public void TestGetStatusDescription()
		{
			AssertEquals(@"THIS TRANSACTION WAS ACCEPTED WITH ERRORS AND/OR WARNINGS
CCID =AAA3366766M CREATED SUCCESSFULLY", Message.GetStatusDescription());
		}

		CMRCLREGRMessage Message
		{
			get
			{
				if (fMessage == null)
				{
					fMessage = Factory.New<CMRCLREGRMessage>();
					fMessage.EM_MessageText = cLREGSampleMessage;
				}
				return fMessage;
			}
		}
		CMRCLREGRMessage fMessage;

		CMRCLREGRMessage RejectionMessage
		{
			get
			{
				if (fRejectionMessage == null)
				{
					fRejectionMessage = Factory.New<CMRCLREGRMessage>();
					fRejectionMessage.EM_MessageText = cLREGRejectionMessage;
				}
				return fRejectionMessage;
			}
		}

		CMRCLREGRMessage fRejectionMessage;
		const string cLREGSampleMessage = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::CLREGR+2F8D 6599 866A:001+11'FTX+CCI++AAA3366766M'NAD+MR+AAA374M::95'RFF+ACW:CLREG'RFF+AFM:9'RFF+ABO:DOCMSGNUM::001'DTM+310:20101221232955:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5202:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITH ERRORS AND/OR WARNINGS'ERP+1'ERC+ADVICE:80:95'ERC+CL0378:6:95'FTX+AAO+++CCID =AAA3366766M CREATED SUCCESSFULLY'CNT+55:000'UNT+18+000001'";
		const string cLREGRejectionMessage = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::CLREGR+1JGB 5G6B I66A:001+11'NAD+MR+AAA374M::95'RFF+ACW:CLREG'RFF+AFM:9'RFF+ABO:DOCMSGNUM::001'DTM+310:20101221231549:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5201:6:95'FTX+AAO+++THIS TRANSACTION WAS REJECTED'ERP+1'ERC+ERROR:80:95'ERC+CL0498:6:95'FTX+AAO+++POSTAL ADDRESS SUPPLIED FOR CCID CREATION'CNT+55:001'UNT+17+000001'";
	}
}
