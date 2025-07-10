using System.Linq;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.ExitControl.Business.AES.Testing
{
	class EX583MessageProviderTest : DataProviderTestCase<EX583MessageProvider>
	{
		public void TestMRN()
		{
			AssertEquals("MRN", "MRN001", Provider.MRN);
		}

		public void TestLRN()
		{
			AssertEquals("LRN", "LRN001", Provider.LRN);
		}

		public void TestDeclaration()
		{
			AssertType<EX583MessageProvider>("Declaration", Provider.Declaration);
		}

		public void TestAdditionalInformations()
		{
			var sendingObj = new AdditionalInfoSendingObject("9002");

			((IDocumentSendingMapper)Provider).AddAdditionalInfomation(sendingObj);
			AssertType<EX583AdditionalInformationProvider>(Provider.AdditionalInformations.Single());
		}

		public void TestSupportingDocuments()
		{
			var sup1 = exitHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Sup1.pdf", "CIV");
			var sendingObj = new DocumentSendingObject(exitReport);
			sendingObj.EDoc = sup1.UniqueKey;

			((IDocumentSendingMapper)Provider).AddSupportingDocument(sendingObj);
			AssertType<EX583SupportingDocumentProvider>(Provider.SupportingDocuments.Single());
		}

		protected override EX583MessageProvider GetProvider() => new EX583MessageProvider(new DocumentsSendingAction(exitReport));

		protected override void SetUp()
		{
			base.SetUp();
			exitHeader = Factory.New<CusExitHeader>();
			var consignment = exitHeader.CusExitConsignments.AddNew();
			consignment.CXC_MovementReference = "MRN001";
			consignment.CXC_UniqueConsignmentReference = "LRN001";
			exitReport = exitHeader.CusExitReports.AddNew();
			exitReport.CER_CXC_Consignment = consignment.PK;
		}
		CusExitHeader exitHeader;
		CusExitReport exitReport;
	}
}
