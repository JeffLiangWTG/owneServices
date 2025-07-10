using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ExportAESDocumentRequestTest : ExportCommonDocumentRequestTest<ExportAESDocumentRequest>
	{
		public void TestRequestMissingDocuments_ExitCertificateDoc()
		{
			CombineAssertions(() =>
			{
				AssertEquals(0, businessObject.Messages.Count);

				var documentRequestMessage = RequestMissingDocument();
				AssertEquals("Export AES Document Capture Request can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

				var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
				var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_CLR.pdf", "CLR");
				docManagerInfo.Save();

				businessObject.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
				declaration.Factory.Save();

				documentRequestMessage = RequestMissingDocument();
				AssertEquals("Export AES Document Capture Request for Exit Certificate document can not be sent when ZG_CSVExitCertificate has no value so nothing will be sent", 0, documentRequestMessage);

				businessObject.ZG_CSVExitCertificate = CsvExitCertificate;
				declaration.Factory.Save();

				documentRequestMessage = RequestMissingDocument();
				AssertEquals("Message created successfully", 1, documentRequestMessage);

				AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_E_AEAT_CLR_EXT.pdf", CsvExitCertificate, ZGuid.Empty);

				AssertNoDuplicatedDocumentRequests(mrnCode + "_E_AEAT_CLR_EXT.pdf", CsvExitCertificate, ZGuid.Empty);
			});
		}

		public void TestRequestMissingDocuments_EADDoc()
		{
			CombineAssertions(() =>
			{
				AssertEquals(0, businessObject.Messages.Count);

				var documentRequestMessage = RequestMissingDocument();
				AssertEquals("Export Document Capture Request can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

				var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
				var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_CLR.pdf", "CLR");
				docManagerInfo.Save();

				businessObject.IndirectExport = false;
				businessObject.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
				declaration.Factory.Save();

				documentRequestMessage = RequestMissingDocument();
				AssertEquals("Export Document Capture Request for AED document can not be sent when Indirect Export is false so nothing will be sent", 0, documentRequestMessage);

				businessObject.IndirectExport = true;
				declaration.Factory.Save();

				documentRequestMessage = RequestMissingDocument();
				AssertEquals("Message created successfully", 1, documentRequestMessage);

				AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_E_AEAT_ead.pdf", mrnCode, ZGuid.Empty);

				AssertNoDuplicatedDocumentRequests(mrnCode + "_E_AEAT_ead.pdf", mrnCode, ZGuid.Empty);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			businessObject.ZG_UCC6Version = 1;
		}
		const string CsvExitCertificate = "A1234567890";

		protected override ExportAESDocumentRequest GetDocumentRequestClass(CusEntryHeader businessObject, ZString certName) => new ExportAESDocumentRequest(businessObject, certName);
	}
}
