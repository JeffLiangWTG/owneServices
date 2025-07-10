using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public abstract class ExportCommonDocumentRequestTest<T> : EntryHeaderDocumentRequestTest<T>
		where T : ExportCommonDocumentRequest
	{
		public void TestRequestMissingDocuments_CLRDoc_WithBroker()
		{
			declaration.JE_GS_NKCusAgent = staff.GS_Code;

			CombineAssertions(() =>
			{
				AssertEquals(0, businessObject.Messages.Count);

				var documentRequestMessage = RequestMissingDocument();
				AssertEquals("Export Document Capture Request can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

				var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
				var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_AAA.pdf", "AAA");
				docManagerInfo.Save();

				businessObject.EUH_EADPrintProcedure = "0";
				businessObject.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
				declaration.Factory.Save();

				documentRequestMessage = RequestMissingDocument();
				AssertEquals("Message created successfully", 1, documentRequestMessage);

				AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_E_AEAT_CLR.pdf", "ABCDEFGHIJKLMNOP", certificate.CertificatePK);

				AssertNoDuplicatedDocumentRequests(mrnCode + "_E_AEAT_CLR.pdf", "ABCDEFGHIJKLMNOP", certificate.CertificatePK);
			});
		}

		public void TestRequestMissingDocuments_T2LFDoc()
		{
			var csvT2L = "CSVT2L";

			CombineAssertions(() =>
			{
				AssertEquals(0, businessObject.Messages.Count);

				var documentRequestMessage = RequestMissingDocument();
				AssertEquals("Export Document Capture Request can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

				var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
				var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_CLR.pdf", "CLR");
				docManagerInfo.Save();

				businessObject.EUH_EADPrintProcedure = "0";
				businessObject.ZG_CSVT2L = csvT2L;
				declaration.ZG_CTStatusID = ZString.Empty;
				declaration.Factory.Save();

				documentRequestMessage = RequestMissingDocument();
				AssertEquals("Export Document Capture Request for T2LF document can not be sent when CT Status is not T2LF so nothing will be sent", 0, documentRequestMessage);

				declaration.ZG_CTStatusID = "T2LF";
				declaration.Factory.Save();

				documentRequestMessage = RequestMissingDocument();
				AssertEquals("Message created successfully", 1, documentRequestMessage);

				AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_E_AEAT_t2lf.pdf", csvT2L, ZGuid.Empty);

				AssertNoDuplicatedDocumentRequests(mrnCode + "_E_AEAT_t2lf.pdf", csvT2L, ZGuid.Empty);
			});
		}
	}
}
