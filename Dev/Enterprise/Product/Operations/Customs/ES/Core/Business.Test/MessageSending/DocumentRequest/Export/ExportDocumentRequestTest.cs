using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ExportDocumentRequestTest : ExportCommonDocumentRequestTest<ExportDocumentRequest>
	{
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

				businessObject.EUH_EADPrintProcedure = "0";
				businessObject.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
				declaration.Factory.Save();

				documentRequestMessage = RequestMissingDocument();
				AssertEquals("Export Document Capture Request for AED document can not be sent when EAD Print Procedure is 0 so nothing will be sent", 0, documentRequestMessage);

				businessObject.EUH_EADPrintProcedure = "1";
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
		}

		protected override ExportDocumentRequest GetDocumentRequestClass(CusEntryHeader businessObject, ZString certName) => new ExportDocumentRequest(businessObject, certName);
	}
}
