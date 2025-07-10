using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCLNTDUPMessage))]
	sealed class CMRCLNTDUPMessageTest : CMRCUSRESMessageTest
	{
		public void TestSendersReference()
		{
			AssertEquals("53D175FC4D174C08B1375EF71CFB311F", Message.SendersReference);
		}

		public void TestGetStatus()
		{
			AssertEquals("ORIGINAL REJECTED", Message.GetStatus());
		}

		public void TestGetStatusDescription()
		{
			AssertEquals("THIS TRANSACTION WAS REJECTED", Message.GetStatusDescription());
		}

		CMRCLNTDUPMessage Message
		{
			get
			{
				if (fMessage == null)
				{
					fMessage = Factory.New<CMRCLNTDUPMessage>();
					fMessage.EM_MessageText = cLDUPSample;
				}
				return fMessage;
			}
		}
		CMRCLNTDUPMessage fMessage;

		readonly string cLDUPSample = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::CLREGR+18GH H148 DCF1:001+11'NAD+MR+AAA374M::95'RFF+ACW:CLREG'RFF+AFM:9'RFF+ABO:53D175FC4D174C08B1375EF71CFB311F::001'DTM+310:20110118211834:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5201:6:95'FTX+AAO+++THIS TRANSACTION WAS REJECTED'ERP+1'ERC+ERROR:80:95'ERC+CL0492:6:95'FTX+AAO+++THE NEW CCID CLIENT DETAILS MATCHED AN ALREADY REGISTERED CCID=AAA3366939P'ERP+1'ERC+ERROR:80:95'ERC+CL0501:6:95'FTX+AAO+++DUPLICATE RECORD(S) FOUND'CNT+55:002'UNT+21+000001'";
	}
}
