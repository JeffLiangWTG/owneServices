using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	sealed class H7DocumentRequestTest : CommonDocumentRequestTest<H7DocumentRequest, AsycudaBill>
	{
		public void TestRequestMissingDocuments()
		{
			SetUpTestData();
			var csvClearance = "123456789";

			CombineAssertions(() =>
			{
				AssertEquals(0, businessObject.Messages.Count);

				var documentRequestMessage = RequestMissingDocument();
				AssertEquals("Document Request can not be sent when CSV Clearance is empty", 0, documentRequestMessage);

				var clearanceReferenceNumber = businessObject.CustomsEntryNumbers.AddNew();
				clearanceReferenceNumber.CE_EntryType = "CLR";
				clearanceReferenceNumber.CE_EntryLineReference = "H7";
				clearanceReferenceNumber.CE_RN_NKCountryCode = "ES";
				clearanceReferenceNumber.CE_EntryNum = csvClearance;

				documentRequestMessage = RequestMissingDocument();
				AssertEquals("Message created successfully", 1, documentRequestMessage);

				AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_H7_AEAT_CLR.pdf", csvClearance, certificate.CertificatePK);

				AssertNoDuplicatedDocumentRequests(mrnCode + "_H7_AEAT_CLR.pdf", csvClearance, certificate.CertificatePK);
			});
		}

		public void TestRequestMissingDocuments_DocumentExists()
		{
			SetUpTestData();
			var csvClearance = "123456789";

			CombineAssertions(() =>
			{
				var clearanceReferenceNumber = businessObject.CustomsEntryNumbers.AddNew();
				clearanceReferenceNumber.CE_EntryType = "CLR";
				clearanceReferenceNumber.CE_EntryLineReference = "H7";
				clearanceReferenceNumber.CE_RN_NKCountryCode = "ES";
				clearanceReferenceNumber.CE_EntryNum = csvClearance;

				AssertEquals(0, businessObject.Messages.Count);

				var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
				var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_H7_AEAT_CLR.pdf", "AAA");
				docManagerInfo.Save();

				var documentRequestMessage = RequestMissingDocument();
				AssertEquals("Document Request can not be sent when document already exists", 0, documentRequestMessage);

				AssertEquals(0, businessObject.Messages.Count);
			});
		}

		void SetUpTestData()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_CustomsProfile = "aaa";
			staff.GS_Code = "STF";
			header.AMA_GS_NKCustomsAgent = "STF";

			businessObject = header.Bills.AddNew();
			businessObject.H7MovementReferenceNumber = mrnCode;
		}

		protected override H7DocumentRequest GetDocumentRequestClass(AsycudaBill businessObject, ZString certName)
		{
			return new H7DocumentRequest(businessObject, certName);
		}

		protected override EDIMessage GetLastMessage(AsycudaBill businessObject)
		{
			businessObject.Messages.Reload(true);
			return businessObject.Messages.LastMessage;
		}

		protected override ZBool IsTrain() => true;
	}
}
