using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCMesageDecoderNoticesTest : TestCaseWithFactory
	{
		public void TestProcessNotices()
		{
			Assert("Line Number is empty", testEXDOCMesageDecoderNotices.lineNumber.IsEmpty);
			Assert("Error Message Identifier is empty", testEXDOCMesageDecoderNotices.errorMessageIdentifier.IsEmpty);
			Assert("Narritive Message is empty", testEXDOCMesageDecoderNotices.narrativeMessage.IsEmpty);
			testEXDOCMesageDecoderNotices.Process("L00 W0020 Consignee Name must be present before Certificates can be printed.");
			AssertEquals("Line Number", "L00", testEXDOCMesageDecoderNotices.lineNumber);
			AssertEquals("Error Message Identifier", "W0020", testEXDOCMesageDecoderNotices.errorMessageIdentifier);
			AssertEquals("Narritive Message", "Consignee Name must be present before Certificates can be printed.", testEXDOCMesageDecoderNotices.narrativeMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testEXDOCMesageDecoderNotices = new EXDOCMessageDecoderNotice();
		}

		EXDOCMessageDecoderNotice testEXDOCMesageDecoderNotices;
	}
}
