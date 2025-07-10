using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRSEQRMessage))]
	sealed class CMRSEQRMessageTest : CMRCUSRESMessageTest
	{
		public void TestGetWrappedObjectForOutturnHeader()
		{
			CusOutturnHeader header = CusOutturnHeader.New(Factory);
			header.C6_SendersMessageReference = "O00000007";
			CMRSEAOUTRMessage message = Factory.New<CMRSEAOUTRMessage>();
			message.EM_MessageText = outturnHeaderSEAOUTRMessageText;
			message.SetEM_LinkedObject();
			AssertEquals("LinkedObject", header, message.EM_LinkedObject);
		}

		readonly string outturnHeaderSEAOUTRMessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEQR+3460 D344 AJ59:001+11'
NAD+MR+AAA374M::95'
RFF+ACW:SEQ'
RFF+AFM:9'
RFF+ABO:O00000007/DAT4::001'
DTM+310:20090318015545:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5202:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITH ERRORS AND/OR WARNINGS'
ERP+1'
ERC+ADVICE:80:95'
ERC+CG5431:6:95'
FTX+AAO+++NO MATCHING RECORDS FOUND'
CNT+55:000'
UNT+17+000001'".Replace("\r\n", "");
	}
}
