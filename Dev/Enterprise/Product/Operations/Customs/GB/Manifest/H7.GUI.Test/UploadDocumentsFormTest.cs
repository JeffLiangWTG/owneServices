using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using AsycudaBill = Enterprise.Customs.GB.H7.Business.AsycudaBill;
using AsycudaManifestHeader = Enterprise.Customs.GB.H7.Business.AsycudaManifestHeader;
using UploadDocumentsSendingAction = Enterprise.Customs.GB.H7.Business.UploadDocumentsSendingAction;

namespace Enterprise.Customs.GB.H7.GUI.Testing
{
	[TestedType(typeof(UploadDocumentsForm))]
	public class UploadDocumentsFormTest : ZFormBasherTest
	{
		public void TestMessageSendingGridColumnLayoutProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var testingParent = new UploadDocumentsSendingActionParent(header);

			using (var form = new UploadDocumentsForm(testingParent))
			{
				var messageSendingGridColumnLayoutProviderPropertyInfo = form.GetType().GetProperty("MessageSendingGridColumnLayoutProvider", BindingFlags.Instance | BindingFlags.NonPublic);
				var messageSendingGridColumnLayoutProvider = messageSendingGridColumnLayoutProviderPropertyInfo.GetValue(form, null);
				AssertType<UploadDocumentsGridColumnLayout>(messageSendingGridColumnLayoutProvider);
			}
		}

		public void TestMessageSendingObjectsGroupBoxCaption()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var testingParent = new UploadDocumentsSendingActionParent(header);

			using (var form = new UploadDocumentsForm(testingParent))
			{
				var messageSendingObjectsGroupBoxCaptionInfo = form.GetType().GetProperty("MessageSendingObjectsGroupBoxCaption", BindingFlags.Instance | BindingFlags.NonPublic);
				var messageSendingObjectsGroupBoxCaption = messageSendingObjectsGroupBoxCaptionInfo.GetValue(form, null);
				AssertType<ResourceStringData>(messageSendingObjectsGroupBoxCaption);

				var messageSendingObjectsGroupBoxCaptionStringData = messageSendingObjectsGroupBoxCaption as ResourceStringData;
				AssertEquals("Documents to be sent", messageSendingObjectsGroupBoxCaptionStringData.Caption);
			}
		}

		public void TestSendMessages_SomeBillsHaveNoDocument()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var branch = header.Branch.Company.Branches.AddNew();
			header.AMA_GB = branch.PK;

			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();

			bill1.ABL_BillNumber = "BILL1";
			bill2.ABL_BillNumber = "BILL2";
			bill3.ABL_BillNumber = "BILL3";

			var bill1EDoc = ((IDocManagerSupport)bill1).DocManagerInfo.AddFileOrDocument(new byte[1], "TestFile1", "LBL");

			var sendingObjectParent = new UploadDocumentsSendingActionParent(header);
			var sendingObject1 = sendingObjectParent.SendingObjectsCollection.Cast<UploadDocumentsSendingAction>().Single(x => x.Bill == bill1);
			var sendingObject2 = sendingObjectParent.SendingObjectsCollection.Cast<UploadDocumentsSendingAction>().Single(x => x.Bill == bill2);
			var sendingObject3 = sendingObjectParent.SendingObjectsCollection.Cast<UploadDocumentsSendingAction>().Single(x => x.Bill == bill3);

			var sendingDoc1 = sendingObject1.EDocsCollection.AddNew();
			sendingDoc1.DocumentType = "DC44I";
			sendingDoc1.EDoc = bill1EDoc.UniqueKey;

			using (var form = new UploadDocumentsForm(sendingObjectParent))
			{
				form.Show();
				sendingObject1.ShouldSend = true;
				sendingObject2.ShouldSend = true;
				sendingObject3.ShouldSend = true;

				var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
				sendButton.PerformClick();

				AssertEquals(@"There’s nothing selected to be sent to Customs for the following Bill Number(s).
Either untick the ‘Send?’ checkbox or add eDoc(s):

BILL2
BILL3", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendMessages()
		{
			CreateBadgeCode("PR1", "CDS");
			var header = Factory.New<AsycudaManifestHeader>();
			var branch = header.Branch.Company.Branches.AddNew();
			header.AMA_GB = branch.PK;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();
			header.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, EORI);
			header.AMA_CustomsProfile = "PR1";
			CreatePassword("PR1", PasswordStatusList.Codes.Valid, DateTime.Today.AddDays(-5));
			Factory.Save();

			var eDoc1 = bill1.DocManagerInfo().AddFileOrDocument(new byte[1], "AddInfo1.pdf", "CIV");
			bill1.DocManagerInfo().Save();
			var eDoc2 = bill2.DocManagerInfo().AddFileOrDocument(new byte[1], "AddInfo2.pdf", "CIV");
			bill2.DocManagerInfo().Save();
			var eDoc3 = bill3.DocManagerInfo().AddFileOrDocument(new byte[1], "AddInfo3.pdf", "CIV");
			var eDoc4 = bill3.DocManagerInfo().AddFileOrDocument(new byte[1], "AddInfo4.pdf", "CIV");
			var eDoc5 = bill3.DocManagerInfo().AddFileOrDocument(new byte[1], "AddInfo5.pdf", "CIV");
			bill3.DocManagerInfo().Save();

			var sendingObjectParent = new UploadDocumentsSendingActionParent(header);
			using (var form = new UploadDocumentsForm(sendingObjectParent))
			{
				form.Show();
				var sendingObject1 = sendingObjectParent.SendingObjectsCollection
					.Cast<UploadDocumentsSendingAction>().Single(x => x.Bill == bill1);
				var sendingDoc1 = sendingObject1.EDocsCollection.AddNew();
				sendingDoc1.EDoc = eDoc1.UniqueKey;
				sendingDoc1.DocumentType = "DC44I";
				var sendingObject2 = sendingObjectParent.SendingObjectsCollection
					.Cast<UploadDocumentsSendingAction>().Single(x => x.Bill == bill2);
				var sendingDoc2 = sendingObject2.EDocsCollection.AddNew();
				sendingDoc2.EDoc = eDoc2.UniqueKey;
				sendingDoc2.DocumentType = "DOC44";
				var sendingObject3 = sendingObjectParent.SendingObjectsCollection
					.Cast<UploadDocumentsSendingAction>().Single(x => x.Bill == bill3);
				var sendingDoc3 = sendingObject3.EDocsCollection.AddNew();
				sendingDoc3.EDoc = eDoc3.UniqueKey;
				sendingDoc3.DocumentType = "DC44I";
				var sendingDoc4 = sendingObject3.EDocsCollection.AddNew();
				sendingDoc4.EDoc = eDoc4.UniqueKey;
				sendingDoc4.DocumentType = "DC44I";
				var sendingDoc5 = sendingObject3.EDocsCollection.AddNew();
				sendingDoc5.EDoc = eDoc5.UniqueKey;
				sendingDoc5.DocumentType = "DC44I";

				sendingObject1.ShouldSend = true;
				sendingObject2.ShouldSend = false;
				sendingObject3.ShouldSend = true;

				_ = bill1.Messages; // initialize the Messages collection to test updating the collection
				_ = bill2.Messages;
				_ = bill3.Messages;

				var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
				sendButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("4 documents(s) queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertMessageSentResults(bill1, 1, "bill1");
					AssertMessageSentResults(bill2, 0, "bill2");
					AssertMessageSentResults(bill3, 3, "bill3");
				});
			}

			void AssertMessageSentResults(AsycudaBill bill, int expectedNumberOfMessages, string messagePrefix = "")
			{
				if (!string.IsNullOrWhiteSpace(messagePrefix))
				{
					messagePrefix += ": ";
				}
				AssertEquals($"{messagePrefix}Count", expectedNumberOfMessages, bill.Messages.Count);

				for (var i = 0; i < expectedNumberOfMessages; i++)
				{
					var message = bill.Messages[i];
					AssertEquals($"{messagePrefix} Message {i}, Message Type", "XUE", message.EM_MessageType);
					AssertEquals($"{messagePrefix} Message {i}, Direction", "TRX", message.EM_ReceiveTransmit);
					AssertEquals($"{messagePrefix} Message {i}, Status", "SNT", message.EM_Status);
				}

				AssertEquals($"{messagePrefix}DSN event", expectedNumberOfMessages, NumberOfDSNEvents(bill));
			}

			int NumberOfDSNEvents(AsycudaBill bill)
			{
				return bill.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.DocumentSent.Code).Count();
			}
		}

		public void TestSendMessages_SomeRecordsHaveErrors()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var branch = header.Branch.Company.Branches.AddNew();
			header.AMA_GB = branch.PK;

			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var bill1EDoc = ((IDocManagerSupport)bill1).DocManagerInfo.AddFileOrDocument(new byte[1], "TestFile1", "LBL");
			var bill2EDoc = ((IDocManagerSupport)bill2).DocManagerInfo.AddFileOrDocument(new byte[1], "TestFile2", "LBL");

			var sendingObjectParent = new UploadDocumentsSendingActionParent(header);
			var sendingObject1 = sendingObjectParent.SendingObjectsCollection.Cast<UploadDocumentsSendingAction>().Single(x => x.Bill == bill1);
			var sendingObject2 = sendingObjectParent.SendingObjectsCollection.Cast<UploadDocumentsSendingAction>().Single(x => x.Bill == bill2);
			sendingObjectParent.SendingObjectsCollection.Remove(sendingObject1);
			var sendingObject1WithValidationError = new UploadDocumentsSendingActionForTest(bill1)
			{
				HardCodeValidationErrorForTesting = true
			};
			sendingObjectParent.SendingObjectsCollection.Add(sendingObject1WithValidationError);

			sendingObject1WithValidationError.EDocsCollection.AddNew().EDoc = bill1EDoc.UniqueKey;
			sendingObject2.EDocsCollection.AddNew().EDoc = bill2EDoc.UniqueKey;

			using (var form = new UploadDocumentsForm(sendingObjectParent))
			{
				form.Show();
				sendingObject1WithValidationError.ShouldSend = true;
				sendingObject2.ShouldSend = true;

				var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
				sendButton.PerformClick();

				AssertEquals("Please fix the error(s) before sending any documents.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendMessages_Credentials()
		{
			CreateBadgeCode("PR1", "CDS");
			var header = Factory.New<AsycudaManifestHeader>();
			var branch = header.Branch.Company.Branches.AddNew();
			header.AMA_GB = branch.PK;
			var bill = header.Bills.AddNew();
			header.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, EORI);
			header.AMA_CustomsProfile = "PR1";

			var eDoc = ((IDocManagerSupport)bill).DocManagerInfo.AddFileOrDocument(new byte[1], "TestFile1", "LBL");

			Factory.Save();

			var sendingObjectParent = new UploadDocumentsSendingActionParent(header);
			var sendingObject = sendingObjectParent.SendingObjectsCollection.Single() as UploadDocumentsSendingAction;
			var sendingDoc = sendingObject.EDocsCollection.AddNew();
			sendingDoc.DocumentType = "DC44I";
			sendingDoc.EDoc = eDoc.UniqueKey;

			using (var form = new UploadDocumentsForm(sendingObjectParent))
			{
				form.Show();
				sendingObject.ShouldSend = true;

				var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
				sendButton.PerformClick();

				AssertEquals(@"No company-level CDS credentials exist.

EORI GBEORI0000001, badge PR1.
Please see eLearning unit 1BGB045 on My Account for assistance.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessages();

			CreatePassword("PR1", PasswordStatusList.Codes.Invalid, DateTime.Today.AddDays(-5));

			using (var form = new UploadDocumentsForm(sendingObjectParent))
			{
				form.Show();
				sendingObject.ShouldSend = true;

				var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
				sendButton.PerformClick();

				AssertEquals(@"Credentials are invalid, await further updates from CDS via eHub.

EORI GBEORI0000001, badge PR1.
Please see eLearning unit 1BGB045 on My Account for assistance.", UnitTestUserNotification.Instance.LastMessage.Text);
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
				AssertEquals("GBH7BillFilterBusinessObject", filterBusinessObject.GetType().Name);
				string[] expectedVisibleFilterDescriptions = ["Bill Number", "MRN", "LRN", "Customs Status"];
				var actualVisibleFilterDescriptions = filterBusinessObject.ModuleFilters.Where(f => f.Visible).Select(f => f.Description);
				AssertContainsExactElementsInAnyOrder(expectedVisibleFilterDescriptions, actualVisibleFilterDescriptions);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var requestedDocument = bill.RequestedDocuments.AddNew();
			requestedDocument.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			requestedDocument.CSI_Code = "9002";
			requestedDocument.CSI_Description = "9002 Desc";
			var testingParent = new UploadDocumentsSendingActionParent(header);
			return new UploadDocumentsForm(testingParent);
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override bool AllowSaveOnFormForTestHasChanges => false;

		public override Type FormToBashType => typeof(UploadDocumentsForm);

		void CreateBadgeCode(ZString badge, ZString cspCode)
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = badge;
			badgeCodeSetting.CSPCode = cspCode;

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);
		}

		void CreatePassword(ZString badge, ZString status, ZDateTime issueDate, ZDateTime? expiryDate = null)
		{
			var extPwd = Factory.New<GlbExternalPassword_GB>();
			extPwd.GP_GC = GlbCompany.CurrentCompany.PK.ToGuid();
			extPwd.EORI = $"{GlbCompany.CurrentCompany.Country.Code}{EORI}";
			extPwd.GP_UserID = $"{extPwd.EORI}.{badge}";
			extPwd.GP_PasswordType = "CDS";
			extPwd.GP_CurrentPassword = "NOWPWD";
			extPwd.GP_NextPassword = "NEXTPWD";
			extPwd.GP_IssueDate = issueDate;
			extPwd.GP_ExpiryDate = expiryDate ?? issueDate.AddDays(10);
			extPwd.GP_PasswordStatus = status;
			extPwd.IsTokenForCDS = true;
		}

		string EORI => "EORI0000001";
	}

	class UploadDocumentsSendingActionForTest : UploadDocumentsSendingAction
	{
		public UploadDocumentsSendingActionForTest(AsycudaBill bill) : base(bill)
		{
			Action = "ABC";
		}

		public bool HardCodeValidationErrorForTesting { get; set; }

		protected override EU.H7.Business.MessageSendingObjectValidation GetNewValidation() => new UploadDocumentsSendingActionValidationForTest(HardCodeValidationErrorForTesting, this);
	}

	class UploadDocumentsSendingActionValidationForTest : EU.H7.Business.MessageSendingObjectValidation
	{
		public UploadDocumentsSendingActionValidationForTest(bool hardcodeError, UploadDocumentsSendingActionForTest parent)
			: base(parent)
		{
			HardCodeValidationErrorForTesting = hardcodeError;
		}

		bool HardCodeValidationErrorForTesting { get; }

		protected override void CheckAction()
		{
			base.CheckAction();

			if (!Parent.ActionInfo.HasErrors() && HardCodeValidationErrorForTesting)
			{
				Parent.ActionInfo.AddError("Test Error");
			}
		}
	}
}

