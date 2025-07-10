using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	sealed class IM483HeaderProviderTest : DataProviderTestCase<IM483HeaderProvider>
	{
		public void TestSendingAction()
		{
			SetUpTestData(true);
			AssertSame(sendingAction, Provider.SendingAction);
		}

		public void TestAdditionalInformations()
		{
			SetUpTestData();
			var requestedDocument = bill.RequestedDocuments.AddNew();
			requestedDocument.CSI_Code = "9002";
			requestedDocument.CSI_Description = "9002 Desc";
			requestedDocument.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;

			var resultingAdditionalInformation = Provider.AdditionalInformations.Single();
			AssertType<AdditionalInformationProvider>(resultingAdditionalInformation);
			AssertEquals("Code", "9002", resultingAdditionalInformation.Code);
			AssertEquals("Text", "9002 Desc", resultingAdditionalInformation.Text);
		}

		public void TestSupportingDocuments()
		{
			SetUpTestData();
			var supportingDocument = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "SuppDoc.pdf", "CIV");
			AssertEquals("Precondition", "Commercial Invoice", supportingDocument.Description);

			var requestedDocument = bill.RequestedDocuments.AddNew();
			requestedDocument.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;

			CreateSendingAction();

			var addInfo = sendingAction.AddInfoCollection[0];

			var docSendingObject = addInfo.EDocsCollection.AddNew();
			docSendingObject.EDoc = supportingDocument.UniqueKey;

			var resultingSupportingDocument = Provider.SupportingDocuments.Single();
			CombineAssertions(() =>
			{
				AssertType<SupportingDocumentWithImageProvider>(resultingSupportingDocument);
				AssertEquals("Description", "Commercial Invoice", resultingSupportingDocument.Description);
				AssertEquals("DocumentImage", "SuppDoc.pdf", resultingSupportingDocument.DocumentImage.Filename);
			});
		}

		public void TestEDocsToAttach()
		{
			SetUpTestData();
			var supportingDocument = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "SuppDoc.pdf", "CIV");
			var supportingDocument2 = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "SuppDoc2.pdf", "CIV");

			var requestedDocument = bill.RequestedDocuments.AddNew();
			requestedDocument.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;

			var requestedDocument2 = bill.RequestedDocuments.AddNew();
			requestedDocument2.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;

			CreateSendingAction();

			var docSendingObject = sendingAction.AddInfoCollection[0].EDocsCollection.AddNew();
			docSendingObject.EDoc = supportingDocument.UniqueKey;
			var duplicatedDocSendingObject = sendingAction.AddInfoCollection[0].EDocsCollection.AddNew();
			duplicatedDocSendingObject.EDoc = supportingDocument.UniqueKey;
			var docSendingObject2 = sendingAction.AddInfoCollection[1].EDocsCollection.AddNew();
			docSendingObject2.EDoc = supportingDocument2.UniqueKey;

			var eDocsToAttach = Provider.EDocsToAttach;
			var expectedEDocsToAttach = new Dictionary<ZGuid, string>()
			{
				{ supportingDocument.UniqueKey, "SuppDoc.pdf" },
				{ supportingDocument2.UniqueKey, "SuppDoc2.pdf" }
			};
			AssertContainsExactElementsInAnyOrder("Deduplicated documents from all AddInfoSendingObjects", expectedEDocsToAttach, eDocsToAttach);
		}

		public void TestMRN()
		{
			SetUpTestData();
			AssertEquals("", Provider.MRN);
			bill.MovementReferenceNumber = "TestMRN";
			AssertEquals("TestMRN", Provider.MRN);
		}

		public void TestLRN()
		{
			SetUpTestData();
			AssertEquals("", Provider.LRN);
			bill.LocalReferenceNumber = "TestLRN";
			AssertEquals("TestLRN", Provider.LRN);
		}

		public void TestDeclaration()
		{
			SetUpTestData();
			var declaration = Provider.Declaration;
			AssertSame(declaration, Provider);
		}

		protected override IM483HeaderProvider GetProvider()
		{
			SetUpTestData(createSendingActionIfNull: true);
			return new IM483HeaderProvider(sendingAction);
		}

		void CreateSendingAction()
		{
			sendingAction = new UploadDocumentsSendingAction(bill);
		}

		void SetUpTestData(bool createSendingActionIfNull = false)
		{
			if (bill == null)
			{
				var header = Factory.New<AsycudaManifestHeader>();
				bill = header.Bills.AddNew();
			}

			if (createSendingActionIfNull && sendingAction == null)
			{
				CreateSendingAction();
			}
		}

		AsycudaBill bill;
		UploadDocumentsSendingAction sendingAction;
	}
}
