using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ArrivalAtExitDocumentRequestTest : CommonDocumentRequestTest<ArrivalAtExitDocumentRequest, CusExitDetail>
	{
		public void TestConstructorDeclarationAndMRNCode()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("declaration null", () => GetDocumentRequestClass(Factory.New<CusExitDetail>(), "A"));

				businessObject.CED_MovementReferenceNumber = ZString.Empty;
				AssertExceptionThrown<ArgumentException>("mrnCode null", () => GetDocumentRequestClass(businessObject, "A"));
			});
		}

		public void TestRequestMissingDocuments_EAL_WithBroker()
		{
			var csvClearance = "ABCDEFGHIJKLMNOP";
			exitHeader.CEH_GS_NKCustomsAgent = staff.GS_Code;

			CombineAssertions(() =>
			{
				AssertEquals(0, businessObject.Messages.Count);

				var documentRequestMessage = RequestMissingDocument();
				AssertEquals("EAL Document Capture Request can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

				var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
				var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_EAL_AAA.pdf", "AAA");
				docManagerInfo.Save();

				businessObject.ZG_CSVClearance = csvClearance;
				businessObject.Factory.Save();

				documentRequestMessage = RequestMissingDocument();
				AssertEquals("Message created successfully", 1, documentRequestMessage);

				AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_E_AEAT_EAL_CLR.pdf", csvClearance, certificate.CertificatePK);

				AssertNoDuplicatedDocumentRequests(mrnCode + "_E_AEAT_EAL_CLR.pdf", csvClearance, certificate.CertificatePK);
			});
		}

		public void TestRequestMissingDocuments_EAL_WithoutBroker()
		{
			var csvClearance = "ABCDEFGHIJKLMNOP";

			CombineAssertions(() =>
			{
				AssertEquals(0, businessObject.Messages.Count);

				var documentRequestMessage = RequestMissingDocument();
				AssertEquals("EAL Document Capture Request can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

				var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
				var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_EAL_AAA.pdf", "AAA");
				docManagerInfo.Save();

				businessObject.ZG_CSVClearance = csvClearance;
				businessObject.Factory.Save();

				documentRequestMessage = RequestMissingDocument();
				AssertEquals("Message created successfully", 1, documentRequestMessage);

				AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_E_AEAT_EAL_CLR.pdf", csvClearance, ZGuid.Empty);

				AssertNoDuplicatedDocumentRequests(mrnCode + "_E_AEAT_EAL_CLR.pdf", csvClearance, ZGuid.Empty);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.ZG_IsTrainingDeclaration = true;

			exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CEH_Parent = declaration;
			exitHeader.CEH_ReferenceNumber = "Header Ref";

			businessObject = exitHeader.CusExitDetails.AddNew();
			businessObject.CED_MovementReferenceNumber = mrnCode;
			businessObject.Factory.Save();
		}

		CusExitControlHeader exitHeader;

		protected override ArrivalAtExitDocumentRequest GetDocumentRequestClass(CusExitDetail businessObject, ZString certName) => new ArrivalAtExitDocumentRequest(businessObject, certName);

		protected override EDIMessage GetLastMessage(CusExitDetail businessObject)
		{
			businessObject.Messages.Reload(true);
			return businessObject.Messages.LastMessage;
		}

		protected override ZBool IsTrain() => true;
	}
}
