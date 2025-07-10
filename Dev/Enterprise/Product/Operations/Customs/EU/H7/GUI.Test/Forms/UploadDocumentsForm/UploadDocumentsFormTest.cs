using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.EU.H7.Business.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(UploadDocumentsForm))]
	sealed class UploadDocumentsFormTest : MessageSendingFormWithValidationDetailsAbstractTest
	{
		public void TestMessageSendingGridColumns()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var messageSendingGrid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				CombineAssertions(() =>
				{
					AssertNotNull("MessageSendingObjectsGrid", messageSendingGrid);
					AssertEquals("MessageSendingObjectsGrid ColumnStyles defined by MessageSendingObjectProperties plus ShouldSend column", ((UploadDocumentsForm)form).MessageSendingObjectParent.MessageSendingObjectProperties.Count() + 1, messageSendingGrid.Columns.Count);
					AssertContainsExactElementsInAnyOrder(["ShouldSend", "Action", "BillNumber", "MRN", "LocalReferenceNumber", "CustomsStatus"], messageSendingGrid.Columns.Select(x => x.ColumnName));
				});
			}
		}

		public void TestSendWithValidationErrorsCheckBoxHidden()
		{
			using (var form = GetFormToBashCore())
			{
				var checkBox = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
				AssertEquals(false, checkBox.Visible);
			}
		}

		[RequiresSTA]
		public void TestWarningSplitContainerHidden()
		{
			using (var form = GetFormToBashCore())
			{
				var warningsSplitContainer = form.FindSingle<KSplitContainer>("WarningSplitContainer");
				AssertEquals(false, warningsSplitContainer.Visible);
			}
		}

		public void TestFormHeading()
		{
			using (var form = (UploadDocumentsForm)GetFormToBashCore())
			{
				AssertEquals("Form Heading", "Upload Documents", form.FormHeading);
			}
		}

		public void TestPreviewMessageCheckboxVisible()
		{
			using (var form = (UploadDocumentsForm)GetFormToBashCore())
			{
				form.Show();
				Assert("PreviewMessageCheckBox visibility", form.PreviewMessageCheckBox.Visible);
			}
		}

		public void TestSelectAllButton()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var messageSendingObject1 = new UploadDocumentsSendingAction(header.Bills.AddNew());
			var messageSendingObject2 = new UploadDocumentsSendingAction(header.Bills.AddNew());
			var messageSendingObject3 = new UploadDocumentsSendingAction(header.Bills.AddNew());
			var testingParent = new UploadDocumentsSendingActionParent<UploadDocumentsSendingAction>(header);
			testingParent.SendingObjectsCollection.Add(messageSendingObject1);
			testingParent.SendingObjectsCollection.Add(messageSendingObject2);
			testingParent.SendingObjectsCollection.Add(messageSendingObject3);
			messageSendingObject3.ShouldSend = false;

			var selectedMessageSendingObjectCount = testingParent.SendingObjectsCollection.OfType<BaseMessageSendingObject>().Count(mso => mso.ShouldSend);
			AssertEquals("Precondition: There is a non-selected object", 2, selectedMessageSendingObjectCount);

			using (var form = new UploadDocumentsForm(testingParent))
			{
				form.Show();
				var selectAllButton = form.Controls.Find("SelectAllButton", searchAllChildren: true).Single() as ZButton;
				AssertEquals("Button Text", "Select/Deselect All", selectAllButton.Text);

				selectAllButton.PerformClick();
				selectedMessageSendingObjectCount = testingParent.SendingObjectsCollection.OfType<BaseMessageSendingObject>().Count(mso => mso.ShouldSend);
				AssertEquals("Button should select all if there are any non-selected object", 3, selectedMessageSendingObjectCount);

				selectAllButton.PerformClick();
				selectedMessageSendingObjectCount = testingParent.SendingObjectsCollection.OfType<BaseMessageSendingObject>().Count(mso => mso.ShouldSend);
				AssertEquals("Button should deselect all if all objects are selected", 0, selectedMessageSendingObjectCount);
			}
		}

		public void TestSendMessages()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var branch = header.Branch.Company.Branches.AddNew();
			header.AMA_GB = branch.PK;
			var bill1 = header.Bills.AddNew();
			var requestedDoc1 = bill1.RequestedDocuments.AddNew();
			requestedDoc1.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			var bill2 = header.Bills.AddNew();
			var requestedDoc2 = bill2.RequestedDocuments.AddNew();
			requestedDoc2.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			var bill3 = header.Bills.AddNew();
			var requestedDoc3 = bill3.RequestedDocuments.AddNew();
			requestedDoc3.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			var sendingObjectParent = new UploadDocumentsSendingActionParent<UploadDocumentsSendingActionForTest>(header);
			using (var form = new UploadDocumentsForm(sendingObjectParent))
			{
				form.Show();
				var sendingObject1 = sendingObjectParent.SendingObjectsCollection
					.Cast<MessageSendingObject>().Single(x => x.Bill == bill1);
				var sendingObject2 = sendingObjectParent.SendingObjectsCollection
					.Cast<MessageSendingObject>().Single(x => x.Bill == bill2);
				var sendingObject3 = sendingObjectParent.SendingObjectsCollection
					.Cast<MessageSendingObject>().Single(x => x.Bill == bill3);
				sendingObject1.ShouldSend = true;
				sendingObject2.ShouldSend = false;
				sendingObject3.ShouldSend = true;

				var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
				sendButton.PerformClick();
				AssertEquals("2 message(s) queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertBillHasMessage(bill1, "ABC");
				AssertEquals("bill2.Messages.Count", 0, bill2.Messages.Count);
				AssertBillHasMessage(bill3, "ABC");

				void AssertBillHasMessage(AsycudaBill bill, ZString messageType)
				{
					CombineAssertions("New message created", () =>
					{
						AssertEquals(1, bill.Messages.Count);
						var message = bill.Messages[0];
						AssertEquals("Message Type", messageType, message.EM_MessageType);
						AssertEquals("Direction", "TRX", message.EM_ReceiveTransmit);
						AssertEquals("Status", "QUE", message.EM_Status);
					});
				}
			}
		}

		public void TestBottomSectionUserControl()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var bottomSectionUserControl = form.Controls.Find("UploadDocumentsAdditionalInfoUserControl", true).Single();
				CombineAssertions(() =>
				{
					AssertType<UploadDocumentsAdditionalInfoUserControl>("Control Type", bottomSectionUserControl);
					AssertEquals("Binding", "UploadDocumentsSendingActionFilteredCollection", bottomSectionUserControl.GetBindingMember());
				});
			}
		}

		public void TestMessageSendingGridColumnLayoutProvider()
		{
			using (var form = GetFormToBash())
			{
				var layoutProvider = typeof(UploadDocumentsForm).GetField("messageSendingGridColumnLayoutProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(form);
				AssertEquals(typeof(UploadDocumentsGridColumnLayout), layoutProvider?.GetType());
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
				AssertEquals("EUH7BillFilterBusinessObject", filterBusinessObject.GetType().Name);
				string[] expectedVisibleFilterDescriptions = ["Bill Number", "MRN", "LRN", "Customs Status"];
				var actualVisibleFilterDescriptions = filterBusinessObject.ModuleFilters.Where(f => f.Visible).Select(f => f.Description);
				AssertContainsExactElementsInAnyOrder(expectedVisibleFilterDescriptions, actualVisibleFilterDescriptions);
			}
		}

		public void TestMessageSendingGridDataSource()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var messageSendingGrid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				var messageSendingParent = messageSendingGrid.DataSource as BaseMessageSendingObjectParent;
				AssertNotNull(messageSendingParent);
				AssertEquals("UploadDocumentsSendingActionFilteredCollection", messageSendingGrid.DataMember);
			}
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override bool AllowSaveOnFormForTestHasChanges => false;

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var requestedDocument = bill.RequestedDocuments.AddNew();
			requestedDocument.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			requestedDocument.CSI_Code = "9002";
			requestedDocument.CSI_Description = "9002 Desc";
			var messageSendingObjectParent = new UploadDocumentsSendingActionParent<UploadDocumentsSendingAction>(header);
			return new UploadDocumentsForm(messageSendingObjectParent);
		}
	}

	class UploadDocumentsSendingActionForTest : UploadDocumentsSendingAction
	{
		public UploadDocumentsSendingActionForTest(AsycudaBill bill) : base(bill)
		{
			Action = "ABC";
		}

		public override Business.MessageSender CreateSender()
		{
			return new MessageSenderForTest(this);
		}
	}
}
