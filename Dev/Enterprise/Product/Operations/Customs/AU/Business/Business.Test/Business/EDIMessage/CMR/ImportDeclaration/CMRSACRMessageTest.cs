using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRSACRMessage))]
	sealed class CMRSACRMessageTest : CMRImportDeclarationMessageTest
	{
		public void TestAdviceOnSACWithLine()
		{
			CMRSACRMessage message = Factory.New<CMRSACRMessage>();
			message.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::SACR+7EHE E6EJ 2GF:1+11'NAD+MR+FGC673R::95'RFF+ABO:B00008570/1/BNE1::1'ERP+::0'ERC+ID0853::95'FTX+AAO+++LINE DETAILS ARE NOT REQUIRED TX MSG=2005-11-09-09.57.29.611875,SUB=000000000000003,LDGMT QST ID=000000000000018,ANSWR TYP=N'CNT+55:1'UNT+9+000001'";
			ZString report = message.GetReport();
			AssertEquals("Report", true, report.Contains(CMRSACRMessage.SACWithLineAdvice));
		}

		public void TestGetReportForRejectedResponse()
		{
			CMRSACRMessage message = Factory.New<CMRSACRMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.SACRTransactionRejected;
			ZString report = message.GetReport();
			AssertEquals("Report", true, report.Contains("Status: TRANSACTION REJECTED"));
			AssertEquals("Report", true, report.Contains("Errors:"));
			AssertEquals("Report", true, report.Contains("LOCALITY INVALID FOR POST CODE"));
		}

		public void TestIOutstandingPaymentInfoProvider()
		{
			var message = Factory.New<CMRSACRMessage>();
			message.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::SACR+7EHE E6EJ 2GF:1+11'NAD+MR+FGC673R::95'RFF+ABO:B00008570/1/BNE1::1'ERP+::0'ERC+ID0853::95'FTX+AAO+++LINE DETAILS ARE NOT REQUIRED TX MSG=2005-11-09-09.57.29.611875,SUB=000000000000003,LDGMT QST ID=000000000000018,ANSWR TYP=N'CNT+55:1'UNT+9+000001'";
			var provider = (IOutstandingPaymentInfoProvider)message;
			AssertType<SegmentGroup5MessageSection>(provider.Group5Section);
			AssertType<GISSegmentMessageSection>(provider.GISSection);
		}

		#region Implemenation

		protected override EDIMessage GetEDIMessage(string reference)
		{
			CMRSACRMessageTestClass message = Factory.New<CMRSACRMessageTestClass>();
			message.EM_MessageText = CMRImportDeclarationTestData.SACRHeld.Replace("B00122382", reference);
			return message;
		}

		protected override BusinessObject GetWrappedObject(string reference)
		{
			CMRSACRMessageTestClass message = (CMRSACRMessageTestClass)GetEDIMessage(reference);
			return message.GetWrappedObjectTestMethod();
		}

		#endregion
	}
}
