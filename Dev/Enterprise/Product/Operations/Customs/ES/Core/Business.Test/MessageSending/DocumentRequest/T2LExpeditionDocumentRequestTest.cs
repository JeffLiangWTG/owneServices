using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class T2LExpeditionDocumentRequestTest : EntryHeaderDocumentRequestTest<T2LExpeditionDocumentRequest>
	{
		public void TestConstructorMRNCode()
		{
			AssertExceptionThrown<ArgumentException>("mrnCode null", () => GetDocumentRequestClass(declaration.CustomsEntryHeaders.AddNew(), "A"));
		}

		public void TestRequestMissingDocuments_CLRDoc_WithBroker()
		{
			declaration.JE_GS_NKCusAgent = staff.GS_Code;

			CombineAssertions(() =>
			{
				AssertEquals(0, businessObject.Messages.Count);

				var documentRequestMessage = RequestMissingDocument();
				AssertEquals("T2L Expedition Document Capture Request can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

				var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
				var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_AAA.pdf", "AAA");
				docManagerInfo.Save();

				businessObject.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
				declaration.Factory.Save();

				documentRequestMessage = RequestMissingDocument();
				AssertEquals("Message created successfully", 1, documentRequestMessage);

				AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_E_AEAT_T2L_CLR.pdf", "ABCDEFGHIJKLMNOP", certificate.CertificatePK);

				AssertNoDuplicatedDocumentRequests(mrnCode + "_E_AEAT_T2L_CLR.pdf", "ABCDEFGHIJKLMNOP", certificate.CertificatePK);
			});
		}

		public void TestRequestMissingDocuments_CLRDoc_WithoutBroker()
		{
			CombineAssertions(() =>
			{
				AssertEquals(0, businessObject.Messages.Count);

				var documentRequestMessage = RequestMissingDocument();
				AssertEquals("T2L Expedition Document Capture Request can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

				var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
				var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_AAA.pdf", "AAA");
				docManagerInfo.Save();

				businessObject.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
				declaration.Factory.Save();

				documentRequestMessage = RequestMissingDocument();
				AssertEquals("Message created successfully", 1, documentRequestMessage);

				AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_E_AEAT_T2L_CLR.pdf", "ABCDEFGHIJKLMNOP", ZGuid.Empty);

				AssertNoDuplicatedDocumentRequests(mrnCode + "_E_AEAT_T2L_CLR.pdf", "ABCDEFGHIJKLMNOP", ZGuid.Empty);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		}

		protected override T2LExpeditionDocumentRequest GetDocumentRequestClass(CusEntryHeader businessObject, ZString certName) => new T2LExpeditionDocumentRequest(businessObject, certName);
	}
}
