using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	[TestedType(typeof(UploadDocumentsForm))]
	sealed class UploadDocumentsFormTest : MessageSendingFormWithValidationDetailsAbstractTest
	{
		public void TestSendCommonAnnexH7Message_WithMultipleDocuments()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;
			var bill = header.Bills.AddNew();
			bill.MovementReferenceNumber = "test";
			bill.DocumentationRequired = ESH7DocumentationRequiredList.Codes.Yes;

			var h7CusEntryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			h7CusEntryNum.CE_ParentTable = "AsycudaBill";
			h7CusEntryNum.CE_ParentID = bill.PK;
			h7CusEntryNum.CE_EntryType = "MRN";
			h7CusEntryNum.CE_EntryLineReference = "H7";
			h7CusEntryNum.CE_RN_NKCountryCode = "ES";
			h7CusEntryNum.CE_EntryNum = "H7123";

			var messageSendingObjectParent = new UploadDocumentsSendingActionParent<UploadDocumentsSendingAction>(header);
			foreach (var messageSendingObject in messageSendingObjectParent.SendingObjectsCollection)
			{
				var sendingObject = messageSendingObject as UploadDocumentsSendingAction;
				sendingObject.AddInfoCollection.AddNew();
				var docSendingObject1 = sendingObject.AddInfoCollection[0].EDocsCollection.AddNew();
				var docSendingObject2 = sendingObject.AddInfoCollection[0].EDocsCollection.AddNew();
				var supportingDocument1 = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "SuppDoc1.pdf", "CIV");
				var supportingDocument2 = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "SuppDoc2.pdf", "CIV");
				var requestedDocument1 = bill.RequestedDocuments.AddNew();
				var requestedDocument2 = bill.RequestedDocuments.AddNew();
				requestedDocument1.CSI_Status = "OPE";
				requestedDocument2.CSI_Status = "OPE";
				docSendingObject1.EDoc = supportingDocument1.UniqueKey;
				docSendingObject2.EDoc = supportingDocument2.UniqueKey;
			}

			using (var form = new UploadDocumentsForm(messageSendingObjectParent))
			{
				form.Show();

				var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
				sendWithValidationErrorsCheckBox.Checked = true;

				var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
				sendButton.Enabled = true;
				sendButton.PerformClick();

				CombineAssertions("New message created", () =>
				{
					AssertEquals(2, bill.Messages.Count);
					AssertEquals("Application code", "ESC", bill.Messages[0].EM_ApplicationCode);
					AssertEquals("Application code", "ESC", bill.Messages[1].EM_ApplicationCode);
					AssertEquals("Message Type", DeclarationMessageTypeList.Codes.H7Annexes, bill.Messages[0].EM_MessageType);
					AssertEquals("Message Type", DeclarationMessageTypeList.Codes.H7Annexes, bill.Messages[1].EM_MessageType);
					AssertEquals("Direction", "TRX", bill.Messages[0].EM_ReceiveTransmit);
					AssertEquals("Direction", "TRX", bill.Messages[1].EM_ReceiveTransmit);
					AssertEquals("Status", "QUE", bill.Messages[0].EM_Status);
					AssertEquals("Status", "QUE", bill.Messages[1].EM_Status);
				});
			}
		}

		public void TestSendCommonAnnexH7Message_WithMultipleBills()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			bill1.MovementReferenceNumber = "test1";
			bill2.MovementReferenceNumber = "test2";
			bill1.DocumentationRequired = ESH7DocumentationRequiredList.Codes.Yes;
			bill2.DocumentationRequired = ESH7DocumentationRequiredList.Codes.Yes;

			var h7CusEntryNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			h7CusEntryNum1.CE_ParentTable = "AsycudaBill";
			h7CusEntryNum1.CE_ParentID = bill1.PK;
			h7CusEntryNum1.CE_EntryType = "MRN";
			h7CusEntryNum1.CE_EntryLineReference = "H7";
			h7CusEntryNum1.CE_RN_NKCountryCode = "ES";
			h7CusEntryNum1.CE_EntryNum = "H7123";

			var h7CusEntryNum2 = Factory.NewWithValidTestData<CusEntryNumber>();
			h7CusEntryNum2.CE_ParentTable = "AsycudaBill";
			h7CusEntryNum2.CE_ParentID = bill2.PK;
			h7CusEntryNum2.CE_EntryType = "MRN";
			h7CusEntryNum2.CE_EntryLineReference = "H7";
			h7CusEntryNum2.CE_RN_NKCountryCode = "ES";
			h7CusEntryNum2.CE_EntryNum = "H7123";

			var messageSendingObjectParent = new UploadDocumentsSendingActionParent<UploadDocumentsSendingAction>(header);
			foreach (var messageSendingObject in messageSendingObjectParent.SendingObjectsCollection)
			{
				var sendingObject = messageSendingObject as UploadDocumentsSendingAction;
				sendingObject.ShouldSend = true;
				sendingObject.AddInfoCollection.AddNew();
				var docSendingObject = sendingObject.AddInfoCollection[0].EDocsCollection.AddNew();
				var supportingDocument = sendingObject.Bill.DocManagerInfo().AddFileOrDocument(new byte[1], "SuppDoc1.pdf", "CIV");
				var requestedDocument = sendingObject.Bill.RequestedDocuments.AddNew();
				requestedDocument.CSI_Status = "OPE";
				docSendingObject.EDoc = supportingDocument.UniqueKey;
			}

			using (var form = new UploadDocumentsForm(messageSendingObjectParent))
			{
				form.Show();

				var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
				sendWithValidationErrorsCheckBox.Checked = true;

				var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
				sendButton.PerformClick();

				CombineAssertions("New message created", () =>
				{
					AssertEquals(1, bill1.Messages.Count);
					AssertEquals(1, bill2.Messages.Count);
					AssertEquals("Application code", "ESC", bill1.Messages[0].EM_ApplicationCode);
					AssertEquals("Application code", "ESC", bill2.Messages[0].EM_ApplicationCode);
					AssertEquals("Message Type", DeclarationMessageTypeList.Codes.H7Annexes, bill1.Messages[0].EM_MessageType);
					AssertEquals("Message Type", DeclarationMessageTypeList.Codes.H7Annexes, bill2.Messages[0].EM_MessageType);
					AssertEquals("Direction", "TRX", bill1.Messages[0].EM_ReceiveTransmit);
					AssertEquals("Direction", "TRX", bill2.Messages[0].EM_ReceiveTransmit);
					AssertEquals("Status", "QUE", bill1.Messages[0].EM_Status);
					AssertEquals("Status", "QUE", bill2.Messages[0].EM_Status);
				});
			}
		}

		public void TestFilterControl()
		{
			using (var form = GetFormToBash())
			{
				var filterControl = form.Controls.Find("filterStripControl", true)[0] as ZFilterStripBaseControl;
				AssertNotNull(filterControl);
				var filterBusinessObject = filterControl.FilterBusinessObject;
				AssertNotNull(filterBusinessObject);
				AssertEquals("ESH7BillFilterBusinessObject", filterBusinessObject.GetType().Name);
				string[] expectedVisibleFilterDescriptions = ["Bill Number", "MRN (H7)", "LRN (G3)", "MRN (G3)", "Customs Status"];
				var actualVisibleFilterDescriptions = filterBusinessObject.ModuleFilters.Where(f => f.Visible).Select(f => f.Description);
				AssertContainsExactElementsInAnyOrder(expectedVisibleFilterDescriptions, actualVisibleFilterDescriptions);
			}
		}

		public override Type FormToBashType => typeof(UploadDocumentsForm);

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override bool AllowSaveOnFormForTestHasChanges => false;

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<Business.AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.DocumentationRequired = ESH7DocumentationRequiredList.Codes.Yes;
			var requestedDocument = bill.RequestedDocuments.AddNew();
			requestedDocument.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			requestedDocument.CSI_Code = "9002";
			requestedDocument.CSI_Description = "9002 Desc";
			var messageSendingObjectParent = new Business.UploadDocumentsSendingActionParent<Business.UploadDocumentsSendingAction>(header);
			return new UploadDocumentsForm(messageSendingObjectParent);
		}
	}
}
