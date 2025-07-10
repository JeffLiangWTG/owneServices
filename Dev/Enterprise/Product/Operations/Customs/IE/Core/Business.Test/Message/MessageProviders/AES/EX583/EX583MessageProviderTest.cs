using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class EX583MessageProviderTest : DataProviderTestCase<EX583MessageProvider>
	{
		protected override EX583MessageProvider GetProvider() => new EX583MessageProvider(new DocumentsSendingAction(entryHeader));

		public void TestLRN()
		{
			AssertEquals("LRN", "LRN2343234242", Provider.LRN);
		}

		public void TestMRN()
		{
			AssertEquals("MRN", "MRN001", Provider.MRN);
		}

		public void TestAdditionalInformations()
		{
			var requestedDocument = instruction.RequestedDocuments.AddNew();
			requestedDocument.CSI_Code = "9002";
			requestedDocument.CSI_Description = "9002 Desc";
			var sendingObj = new AdditionalInfoSendingObject(instruction.EntryHeader, null, requestedDocument);

			((IDocumentSendingMapper)Provider).AddAdditionalInfomation(sendingObj);
			AssertType<AdditionalInfoSendingObjectProvider>(Provider.AdditionalInformations.Single());
		}

		public void TestSupportingDocuments()
		{
			var suppDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "SuppDoc.pdf", "CIV");
			var sendingObj = new DocumentSendingObject(entryHeader, null);
			sendingObj.EDoc = suppDoc.UniqueKey;

			((IDocumentSendingMapper)Provider).AddSupportingDocument(sendingObj);
			AssertType<SupportingDocumentWithImageProvider>(Provider.SupportingDocuments.Single());
		}

		public void TestDeclaration()
		{
			AssertType<EX583MessageProvider>("Declaration", Provider.Declaration);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			entryHeaderWrapper = testBizObjs.entryHeaderWrapper;
			declaration = entryHeaderWrapper.Declaration;
			instruction = entryHeaderWrapper.Instruction;
			entryHeader = entryHeaderWrapper.EntryHeader;
			entryHeader.MovementReferenceNumberSetter("MRN001");
			entryHeader.CH_BGMReference = "LRN2343234242";
		}
		EntryHeaderWrapper entryHeaderWrapper;
		JobDeclaration declaration;
		CusEntryInstruction instruction;
		CusEntryHeader entryHeader;
	}
}
