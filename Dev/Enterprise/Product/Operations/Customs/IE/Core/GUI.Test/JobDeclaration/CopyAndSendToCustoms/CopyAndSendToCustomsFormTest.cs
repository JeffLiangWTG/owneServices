using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(CopyAndSendToCustomsForm))]
	class CopyAndSendToCustomsFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			using (var form = GetFormToBashCore() as ZForm)
			{
				AssertEquals("Copy and Submit To Customs", form.FormHeading);
				form.Show();
				AssertEquals("Copy and Submit To Customs", form.Text);
			}
		}

		public void TestSize()
		{
			using (var form = new CopyAndSendToCustomsForm(declaration, messageSendingObjectParent))
			{
				form.Show();
				AssertEquals("MinimumSize", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 175, true), form.MinimumSize);
			}
		}

		public void TestSendButton()
		{
			using (var form = new CopyAndSendToCustomsForm(declaration, messageSendingObjectParent))
			{
				form.Show();
				var sendButton = form.FindSingle<ZButton>("SendButton");
				AssertNotNull(sendButton);
				AssertEquals("SendButton.Caption", "Create", sendButton.CaptionResourceString.Caption);
			}
		}

		public void TestCancelButton()
		{
			using (var form = new CopyAndSendToCustomsForm(declaration, messageSendingObjectParent))
			{
				form.Show();
				var cancelButton = form.FindSingle<ZButton>("CancelButton2");
				AssertNotNull(cancelButton);
				AssertEquals("CancelButton2.Caption", "Cancel", cancelButton.CaptionResourceString.Caption);
			}
		}

		public void TestNumberOfCopiesTextBox()
		{
			using (var form = new CopyAndSendToCustomsForm(declaration, messageSendingObjectParent))
			{
				form.Show();
				var numberOfCopiesTextBox = form.FindSingle<ZTextBox>("NumberOfCopiesTextBox");
				AssertNotNull(numberOfCopiesTextBox);
			}
		}

		public void TestNumberOfInvoiceLinesCopiesTextBox()
		{
			using (var form = new CopyAndSendToCustomsForm(declaration, messageSendingObjectParent))
			{
				form.Show();
				var numberOfInvoiceLinesCopiesTextBox = form.FindSingle<ZTextBox>("NumberOfInvoiceLinesCopiesTextBox");
				AssertNotNull(numberOfInvoiceLinesCopiesTextBox);
			}
		}

		public void TestSendToCustomsCheckBox()
		{
			using (var form = new CopyAndSendToCustomsForm(declaration, messageSendingObjectParent))
			{
				form.Show();
				var numberOfCopiesTextBox = form.FindSingle<ZCheckBox>("SendToCustomsCheckBox");
				AssertNotNull(numberOfCopiesTextBox);
			}
		}

		public void TestMessageType()
		{
			using (var form = new CopyAndSendToCustomsForm(declaration, messageSendingObjectParent))
			{
				form.Show();
				var messageTypeDropEdit = form.FindSingle<ZDropEditWithFixedWidth>("MessageTypeDropEdit");
				AssertNotNull(messageTypeDropEdit);
			}
		}

		public void TestShowFormType()
		{
			CopyAndSendToCustomsForm.ShowForm(declaration);
			AssertType<CopyAndSendToCustomsForm>("Dialog form type = CopyAndSendToCustomsForm", ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestShowForm_CheckHasEntries()
		{
			var declarationWithNoEntry = Factory.New<JobDeclaration>();

			CopyAndSendToCustomsForm.ShowForm(declarationWithNoEntry);
			AssertEquals("No entries exist – Please generate entries before attempting to send a message to customs.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public override void TestBashingForm()
		{
			Assert(true);
		}

		public void TestCopyDeclarations()
		{
			using (var form = new CopyAndSendToCustomsForm(declaration, messageSendingObjectParent))
			{
				const int desiredInvoiceLinesCopies = 10;
				const int desiredNumberOfCopies = 3;
				messageSendingObjectParent.NumberOfCopies = desiredNumberOfCopies;
				messageSendingObjectParent.NumberOfInvoiceLinesCopies = desiredInvoiceLinesCopies;
				messageSendingObjectParent.SendToCustoms = false;
				var query = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.StartsWith, GlbStaff.CurrentUser.GS_Code);
				query.AddToFilter(JobDeclarationSchema.JE_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow);
				var copiedDeclarations = NewFactory().Load<JobDeclaration>(query);
				AssertEquals("copiedDeclarations.Length", 0, copiedDeclarations.Length);
				form.CopyAndSendToCustomsIfRequested();

				CombineAssertions(() =>
				{
					copiedDeclarations = NewFactory().Load<JobDeclaration>(query);
					AssertEquals("copiedDeclarations.Length", desiredNumberOfCopies, copiedDeclarations.Length);
					foreach (var copiedDeclaration in copiedDeclarations)
					{
						AssertEquals($"{copiedDeclaration.JE_DeclarationReference} - No declaration has been sent.", 0, copiedDeclaration.CustomsEntryHeaders.Count);

						AssertEquals("InvoiceLines Count", desiredInvoiceLinesCopies, copiedDeclaration.Invoices[0].InvoiceLines.Count);
					}
				});
			}
		}

		public void TestCopyAndSendToCustosmDeclarations()
		{
			using (var form = new CopyAndSendToCustomsForm(declaration, messageSendingObjectParent))
			{
				const int desiredNumberOfCopies = 3;
				messageSendingObjectParent.NumberOfCopies = desiredNumberOfCopies;
				messageSendingObjectParent.SendToCustoms = true;
				messageSendingObjectParent.MessageType = AESOutgoingMessageTypeList.Codes.ExportOriginal;
				var query = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.StartsWith, GlbStaff.CurrentUser.GS_Code);
				query.AddToFilter(JobDeclarationSchema.JE_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow);
				var copiedDeclarations = NewFactory().Load<JobDeclaration>(query);
				AssertEquals("copiedDeclarations.Length", 0, copiedDeclarations.Length);
				form.CopyAndSendToCustomsIfRequested();

				CombineAssertions(() =>
				{
					copiedDeclarations = NewFactory().Load<JobDeclaration>(query);
					AssertEquals("copiedDeclarations.Length", desiredNumberOfCopies, copiedDeclarations.Length);
					foreach (var copiedDeclaration in copiedDeclarations)
					{
						AssertEquals($"{copiedDeclaration.JE_DeclarationReference} - Declaration has been sent.", 1, copiedDeclaration.CustomsEntryHeaders[0].Messages.Count);
					}
				});
			}
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override bool AllowSaveOnFormForTestHasChanges => false;

		protected override Form GetFormToBashCore()
		{
			SetUp();
			return new CopyAndSendToCustomsForm(declaration, messageSendingObjectParent);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			_ = declaration.CustomsEntryHeaders.AddNew();
			_ = declaration.CustomsEntryHeaders.AddNew();
			messageSendingObjectParent = new CopyAndSendToCustomsFormData();
		}
		CopyAndSendToCustomsFormData messageSendingObjectParent;
		JobDeclaration declaration;
	}
}
