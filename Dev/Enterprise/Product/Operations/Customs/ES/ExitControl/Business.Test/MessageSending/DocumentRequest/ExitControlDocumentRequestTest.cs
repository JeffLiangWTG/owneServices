using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	public class ExitControlDocumentRequestTest : CommonDocumentRequestTest<ExitControlDocumentRequest, CusExitReport>
	{
		public void TestConstructorDeclarationAndMRNCode()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("exitHeader null", () => GetDocumentRequestClass(Factory.New<CusExitReport>(), "A"));

				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				var report = exitHeader.CusExitReports.AddNew();
				AssertExceptionThrown<ArgumentNullException>("consignment null", () => GetDocumentRequestClass(report, "A"));

				consignment.CXC_MovementReference = ZString.Empty;
				AssertExceptionThrown<ArgumentException>("mrnCode null", () => GetDocumentRequestClass(businessObject, "A"));
			});
		}

		public void TestRequestMissingDocuments_EAL_WithBroker()
		{
			var csvClearance = "ABCDEFGHIJKLMNOP";

			exitHeader.CXH_GS_NKCustomsAgent = staff.GS_Code;

			CombineAssertions(() =>
			{
				AssertEquals(0, businessObject.Messages.Count);

				var documentRequestMessage = RequestMissingDocument();
				AssertEquals("EAL Document Capture Request can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

				var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
				var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_EAL_AAA.pdf", "AAA");
				docManagerInfo.Save();

				CreateOrUpdateCusEntryNumber(businessObject, CusEntryNumberTypes.Spain.ClearanceCSV, csvClearance, ZString.Empty, new ZDateTime(2021, 8, 1, 11, 0, 5), ZDateTime.Empty);
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

				CreateOrUpdateCusEntryNumber(businessObject, CusEntryNumberTypes.Spain.ClearanceCSV, csvClearance, ZString.Empty, new ZDateTime(2021, 8, 1, 11, 0, 5), ZDateTime.Empty);
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

			exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.TrainingEntry = true;
			consignment = exitHeader.CusExitConsignments.AddNew();
			consignment.CXC_MovementReference = mrnCode;
			businessObject = exitHeader.CusExitReports.AddNew();
			businessObject.CER_CXC_Consignment = consignment.PK;

			businessObject.Factory.Save();
		}
		CusExitHeader exitHeader;
		CusExitConsignment consignment;

		protected override ExitControlDocumentRequest GetDocumentRequestClass(CusExitReport businessObject, ZString certName) => new ExitControlDocumentRequest(businessObject, certName);

		protected override EDIMessage GetLastMessage(CusExitReport businessObject)
		{
			businessObject.Messages.Reload(true);
			return businessObject.Messages.LastMessage;
		}

		protected override ZBool IsTrain() => true;
	}
}
