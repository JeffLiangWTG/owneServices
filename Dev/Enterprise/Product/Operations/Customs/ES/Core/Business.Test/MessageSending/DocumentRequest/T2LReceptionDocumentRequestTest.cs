using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class T2LReceptionDocumentRequestTest : EntryHeaderDocumentRequestTest<T2LReceptionDocumentRequest>
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
				AssertEquals("T2L Clearance Document Capture Request can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

				var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
				docManagerInfo.AddFileOrDocument(new byte[1], T2C_MRN + "_I_AEAT_ZZZ.pdf", "ZZZ");
				docManagerInfo.Save();

				businessObject.SetCSVClearanceNum("34YEUXEP8E7GE9XD");
				declaration.Factory.Save();

				documentRequestMessage = RequestMissingDocument();
				AssertEquals("Message created successfully", 1, documentRequestMessage);

				AssertDocumentRequestEDIMessage(businessObject, T2C_MRN + "_I_AEAT_T2LR_CLR.pdf", "34YEUXEP8E7GE9XD", certificate.CertificatePK);

				AssertNoDuplicatedDocumentRequests(T2C_MRN + "_I_AEAT_T2LR_CLR.pdf", "34YEUXEP8E7GE9XD", certificate.CertificatePK);
			});
		}

		public void TestRequestMissingDocuments_CLRDoc_WithoutBroker()
		{
			CombineAssertions(() =>
			{
				AssertEquals(0, businessObject.Messages.Count);

				var documentRequestMessage = RequestMissingDocument();
				AssertEquals("T2L Clearance Document Capture Request can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

				var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
				docManagerInfo.AddFileOrDocument(new byte[1], T2C_MRN + "_I_AEAT_ZZZ.pdf", "ZZZ");
				docManagerInfo.Save();

				businessObject.SetCSVClearanceNum("34YEUXEP8E7GE9XD");
				declaration.Factory.Save();

				documentRequestMessage = RequestMissingDocument();
				AssertEquals("Message created successfully", 1, documentRequestMessage);

				AssertDocumentRequestEDIMessage(businessObject, T2C_MRN + "_I_AEAT_T2LR_CLR.pdf", "34YEUXEP8E7GE9XD", ZGuid.Empty);

				AssertNoDuplicatedDocumentRequests(T2C_MRN + "_I_AEAT_T2LR_CLR.pdf", "34YEUXEP8E7GE9XD", ZGuid.Empty);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CreateOrUpdateCusEntryNumber(businessObject, CusEntryNumberTypes.Spain.T2CMovementReferenceNumber, T2C_MRN, ZString.Empty, ZDateTime.Today, ZDateTime.Empty);
			declaration.Factory.Save();
		}

		protected override T2LReceptionDocumentRequest GetDocumentRequestClass(CusEntryHeader businessObject, ZString certName) => new T2LReceptionDocumentRequest(businessObject, certName);

		const string T2C_MRN = "21ES009999M0000707";
	}
}
