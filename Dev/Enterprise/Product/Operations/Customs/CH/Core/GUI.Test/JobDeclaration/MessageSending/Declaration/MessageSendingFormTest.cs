using System.Linq;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using CHJobMessageTypeList = Enterprise.Customs.Common.CH.CHJobMessageTypeList.Codes;
using PassarMessageTypeList = Enterprise.Customs.CH.Business.PassarMessageTypeList.Codes;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(MessageSendingForm))]
sealed class MessageSendingFormTest : MessageSendingFormWithValidationDetailsAbstractTest
{
	[CaptureMemoryDumpForDisposableLeak]
	protected override Form GetFormToBashCore()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		Factory.Save();
		return new MessageSendingForm(new ExportDeclarationMessageSendingObjectParent(declaration));
	}

	public void TestControls()
	{
		using (var form = new MessageSendingFormForTest(new ExportDeclarationMessageSendingObjectParent(Declaration)))
		{
			form.Show();
			var messageSendingObjectsGrid = form.MessageSendingObjectsGrid;
			var sendButton = form.SendButton;
			var cancelButton2 = form.CancelButton2;

			CombineAssertions(() =>
			{
				AssertEquals($"{messageSendingObjectsGrid.Name} visible", true, messageSendingObjectsGrid.Visible);
				AssertEquals($"{sendButton.Name} visible", true, sendButton.Visible);
				AssertEquals($"{cancelButton2.Name} visible", true, cancelButton2.Visible);
			});
		}
	}

	public void TestMessageSendingObjectsGrid_Import()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		using (var form = new MessageSendingFormForTest(new ImportDeclarationMessageSendingObjectParent(Declaration)))
		{
			form.Show();
			var messageSendingObjectsGrid = form.MessageSendingObjectsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				UserControlTestHelper.AssertColumnStyles(messageSendingObjectsGrid, DeclarationMessageSendingObject.SchemaShouldSend, 0);
				UserControlTestHelper.AssertColumnStyles(messageSendingObjectsGrid, DeclarationMessageSendingObject.Schema.MessageType, 1);
				UserControlTestHelper.AssertColumnStyles(messageSendingObjectsGrid, DeclarationMessageSendingObject.Schema.VOCReason, 2);
				UserControlTestHelper.AssertColumnStyles(messageSendingObjectsGrid, DeclarationMessageSendingObject.Schema.DeclarationType, 3);
				UserControlTestHelper.AssertColumnStyles(messageSendingObjectsGrid, DeclarationMessageSendingObject.Schema.SubStyle, 4);
				UserControlTestHelper.AssertColumnStyles(messageSendingObjectsGrid, DeclarationMessageSendingObject.Schema.Description, 5);
				UserControlTestHelper.AssertColumnStyles(messageSendingObjectsGrid, DeclarationMessageSendingObject.Schema.LocalReferenceNumber, 6);
				UserControlTestHelper.AssertColumnStyles(messageSendingObjectsGrid, DeclarationMessageSendingObject.Schema.EntryStatus, 7);
			});
		}
	}

	public void TestReasonText() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Export;
		using (var form = new MessageSendingFormForTest(new ExportDeclarationMessageSendingObjectParent(Declaration)))
		{
			form.Show();
			var reasonTextTextBox = form.ReasonTextTextBox;

			AssertEquals("Multiline", true, reasonTextTextBox.Multiline);
			AssertEquals("BindTo", $"{nameof(ExportDeclarationMessageSendingObjectParent.SendingObjectsCollection)}.{nameof(ExportDeclarationMessageSendingObject.ReasonText)}", reasonTextTextBox.BindTo);
			AssertEquals("Export: Visible", true, reasonTextTextBox.Visible);
		}
		Declaration.JE_MessageType = CHJobMessageTypeList.Import;
		using (var form = new MessageSendingFormForTest(new ImportDeclarationMessageSendingObjectParent(Declaration)))
		{
			form.Show();
			AssertEquals("Import: Visible", false, form.ReasonTextTextBox.Visible);
		}
	});

	public void TestImportVisibility() => CombineAssertions(() =>
	{
		Declaration.ActiveEntryHeaders.AddNew();
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = Declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		Declaration.CustomsEntryHeaders[0].CH_CEI_Instruction = entryInstruction.PK;

		var messageSendingObjectParent = new ImportDeclarationMessageSendingObjectParentForTest(Declaration);
		var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection[0];

		using var form = new MessageSendingFormForTest(messageSendingObjectParent);
		form.Show();

		AssertVisibility(CHJobMessageTypeList.Import, PassarMessageTypeList.NI013, hasErrors: true);
		AssertVisibility(CHJobMessageTypeList.Import, PassarMessageTypeList.NI013, hasErrors: false);
		AssertVisibility(CHJobMessageTypeList.Import, PassarMessageTypeList.NI015, hasErrors: true);
		AssertVisibility(CHJobMessageTypeList.Import, PassarMessageTypeList.NI015, hasErrors: false);

		void AssertVisibility(string shipmentType, string messageType, bool hasErrors)
		{
			messageSendingObjectParent.HasValidationErrors(hasErrors);
			Declaration.JE_MessageType = shipmentType;
			messageSendingObject.MessageType = messageType;
			messageSendingObject.ShouldSend = true;
			messageSendingObjectParent.RunPreSaveValidation();

			var info = $"ShipmentType={shipmentType} MessageType={messageType} Errors={hasErrors}";
			AssertEquals($"{info} - WarningSplitContainer", expected: true, form.WarningSplitContainer.Visible);
			AssertEquals($"{info} - SendWithValidationErrorsCheckBox", hasErrors, form.SendWithValidationErrorsCheckBox.Visible);
			AssertEquals($"{info} - ReasonTextGroupBox", expected: false, form.ReasonTextGroupBox.Visible);
			AssertEquals($"{info} - NC123DetailsUserControl", expected: false, form.NC123DetailsUserControl.Visible);
		}
	});

	public void TestExportVisibility() => CombineAssertions(() =>
	{
		Declaration.ActiveEntryHeaders.AddNew();
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = Declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		Declaration.CustomsEntryHeaders[0].CH_CEI_Instruction = entryInstruction.PK;

		var messageSendingObjectParent = new ExportDeclarationMessageSendingObjectParentForTest(Declaration);
		var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection[0];

		using var form = new MessageSendingFormForTest(messageSendingObjectParent);
		form.Show();

		AssertVisibility(CHJobMessageTypeList.Export, PassarMessageTypeList.NE013, hasErrors: true);
		AssertVisibility(CHJobMessageTypeList.Export, PassarMessageTypeList.NE013, hasErrors: false);
		AssertVisibility(CHJobMessageTypeList.Export, PassarMessageTypeList.NE015, hasErrors: true);
		AssertVisibility(CHJobMessageTypeList.Export, PassarMessageTypeList.NE015, hasErrors: false);
		AssertVisibility(CHJobMessageTypeList.Export, PassarMessageTypeList.NE069, hasErrors: true);
		AssertVisibility(CHJobMessageTypeList.Export, PassarMessageTypeList.NE069, hasErrors: false);

		AssertVisibility(CHJobMessageTypeList.ExportDeclarationActivation, PassarMessageTypeList.NC123, hasErrors: true);
		AssertVisibility(CHJobMessageTypeList.ExportDeclarationActivation, PassarMessageTypeList.NC123, hasErrors: false);
		AssertVisibility(CHJobMessageTypeList.ExportDeclarationActivation, PassarMessageTypeList.NE130, hasErrors: true);
		AssertVisibility(CHJobMessageTypeList.ExportDeclarationActivation, PassarMessageTypeList.NE130, hasErrors: false);

		void AssertVisibility(string shipmentType, string messageType, bool hasErrors)
		{
			messageSendingObjectParent.HasValidationErrors(hasErrors);
			Declaration.JE_MessageType = shipmentType;
			messageSendingObject.MessageType = messageType;
			messageSendingObject.ShouldSend = true;
			messageSendingObjectParent.RunPreSaveValidation();

			var info = $"ShipmentType={shipmentType} MessageType={messageType} Errors={hasErrors}";
			AssertEquals($"{info} - WarningSplitContainer", expected: true, form.WarningSplitContainer.Visible);
			AssertEquals($"{info} - SendWithValidationErrorsCheckBox", hasErrors, form.SendWithValidationErrorsCheckBox.Visible);
			AssertEquals($"{info} - ReasonTextGroupBox", expected: true, form.ReasonTextGroupBox.Visible);
			AssertEquals($"{info} - NC123DetailsUserControl", expected: false, form.NC123DetailsUserControl.Visible);
		}
	});

	public void TestImportVisibilityForCancellationOrDataRequest() => CombineAssertions(() =>
	{
		Declaration.ActiveEntryHeaders.AddNew();
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = Declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		Declaration.CustomsEntryHeaders[0].CH_CEI_Instruction = entryInstruction.PK;

		var messageSendingObjectParent = new ImportDeclarationMessageSendingObjectParentForTest(Declaration);
		var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection[0];

		using var form = new MessageSendingFormForTest(messageSendingObjectParent);
		form.Show();

		AssertVisibility(CHJobMessageTypeList.Import, PassarMessageTypeList.NI014, hasErrors: true);
		AssertVisibility(CHJobMessageTypeList.Import, PassarMessageTypeList.NI014, hasErrors: false);
		AssertVisibility(CHJobMessageTypeList.Import, PassarMessageTypeList.NI016, hasErrors: true);
		AssertVisibility(CHJobMessageTypeList.Import, PassarMessageTypeList.NI016, hasErrors: false);

		void AssertVisibility(string shipmentType, string messageType, bool hasErrors)
		{
			messageSendingObjectParent.HasValidationErrors(hasErrors);
			Declaration.JE_MessageType = shipmentType;
			messageSendingObject.MessageType = messageType;
			messageSendingObject.ShouldSend = true;
			messageSendingObjectParent.RunPreSaveValidation();

			var info = $"ShipmentType={shipmentType} MessageType={messageType}";
			AssertEquals($"{info} - WarningSplitContainer", expected: false, form.WarningSplitContainer.Visible);
			AssertEquals($"{info} - SendWithValidationErrorsCheckBox", expected: false, form.SendWithValidationErrorsCheckBox.Visible);
			AssertEquals($"{info} - ReasonTextGroupBox", expected: false, form.ReasonTextGroupBox.Visible);
			AssertEquals($"{info} - NC123DetailsUserControl", expected: false, form.NC123DetailsUserControl.Visible);
		}
	});

	public void TestExportVisibilityForCancellationOrDataRequest() => CombineAssertions(() =>
	{
		Declaration.ActiveEntryHeaders.AddNew();
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = Declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		Declaration.CustomsEntryHeaders[0].CH_CEI_Instruction = entryInstruction.PK;

		var messageSendingObjectParent = new ExportDeclarationMessageSendingObjectParentForTest(Declaration);
		var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection[0];

		using var form = new MessageSendingFormForTest(messageSendingObjectParent);
		form.Show();

		AssertVisibility(CHJobMessageTypeList.Export, PassarMessageTypeList.NE014, hasErrors: true);
		AssertVisibility(CHJobMessageTypeList.Export, PassarMessageTypeList.NE014, hasErrors: false);
		AssertVisibility(CHJobMessageTypeList.Export, PassarMessageTypeList.NC016, hasErrors: true);
		AssertVisibility(CHJobMessageTypeList.Export, PassarMessageTypeList.NC016, hasErrors: false);

		void AssertVisibility(string shipmentType, string messageType, bool hasErrors)
		{
			messageSendingObjectParent.HasValidationErrors(hasErrors);
			Declaration.JE_MessageType = shipmentType;
			messageSendingObject.MessageType = messageType;
			messageSendingObject.ShouldSend = true;
			messageSendingObjectParent.RunPreSaveValidation();

			var info = $"ShipmentType={shipmentType} MessageType={messageType}";
			AssertEquals($"{info} - WarningSplitContainer", expected: false, form.WarningSplitContainer.Visible);
			AssertEquals($"{info} - SendWithValidationErrorsCheckBox", expected: false, form.SendWithValidationErrorsCheckBox.Visible);
			AssertEquals($"{info} - ReasonTextGroupBox", expected: true, form.ReasonTextGroupBox.Visible);
			AssertEquals($"{info} - NC123DetailsUserControl", expected: false, form.NC123DetailsUserControl.Visible);
		}
	});

	public void TestExportVisibilityForNC123MessageType() => CombineAssertions(() =>
	{
		Declaration.ActiveEntryHeaders.AddNew();
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = Declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		Declaration.CustomsEntryHeaders[0].CH_CEI_Instruction = entryInstruction.PK;

		var messageSendingObjectParent = new ExportDeclarationMessageSendingObjectParentForTest(Declaration);
		var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
		messageSendingObject.MessageType = PassarMessageTypeList.NC123;
		messageSendingObject.ShouldSend = true;
		messageSendingObjectParent.RunPreSaveValidation();

		using var form = new MessageSendingFormForTest(messageSendingObjectParent);
		form.Show();

		AssertVisibility(CHJobMessageTypeList.Export, PassarMessageTypeList.NC123, hasErrors: true, nc123ControlVisible: true);
		AssertVisibility(CHJobMessageTypeList.Export, PassarMessageTypeList.NC123, hasErrors: false, nc123ControlVisible: true);

		AssertVisibility(CHJobMessageTypeList.ExportDeclarationActivation, PassarMessageTypeList.NC123, hasErrors: true, nc123ControlVisible: false);
		AssertVisibility(CHJobMessageTypeList.ExportDeclarationActivation, PassarMessageTypeList.NC123, hasErrors: false, nc123ControlVisible: false);

		void AssertVisibility(string shipmentType, string messageType, bool hasErrors, bool nc123ControlVisible)
		{
			messageSendingObjectParent.HasValidationErrors(hasErrors);
			Declaration.JE_MessageType = shipmentType;
			messageSendingObject.MessageType = messageType;
			messageSendingObject.ShouldSend = true;
			messageSendingObjectParent.RunPreSaveValidation();

			var info = $"ShipmentType={shipmentType} MessageType={messageType}";
			AssertEquals($"ShipmentType={shipmentType} MessageType=NC123 - WarningSplitContainer", expected: !nc123ControlVisible, form.WarningSplitContainer.Visible);
			AssertEquals($"ShipmentType={shipmentType} MessageType=NC123 - SendWithValidationErrorsCheckBox", expected: !nc123ControlVisible && hasErrors, form.SendWithValidationErrorsCheckBox.Visible);
			AssertEquals($"ShipmentType={shipmentType} MessageType=NC123 - ReasonTextGroupBox", expected: !nc123ControlVisible, form.ReasonTextGroupBox.Visible);
			AssertEquals($"ShipmentType={shipmentType} MessageType=NC123 - NC123DetailsUserControl", expected: nc123ControlVisible, form.NC123DetailsUserControl.Visible);
		}
	});

	public void TestResendReasonMessageTypeImport() => AssertResendReasonMessageAppears(Common.Shared.SharedJobMessageTypeList.Codes.Import, CHLogicalStatusList.Codes.Sent);

	public void TestResendReasonMessageTypeExport() => CombineAssertions(() =>
	{
		AssertResendReasonMessageAppears(Common.Shared.SharedJobMessageTypeList.Codes.Export, CHLogicalStatusList.Codes.Acknowledged);
		AssertResendReasonMessageAppears(Common.Shared.SharedJobMessageTypeList.Codes.Export, CHLogicalStatusList.Codes.Sent);
	});

	public void TestResendReasonMessageTypeExportDeclarationActivation() => CombineAssertions(() =>
	{
		AssertResendReasonMessageAppears(CHJobMessageTypeList.ExportDeclarationActivation, CHLogicalStatusList.Codes.Acknowledged);
		AssertResendReasonMessageAppears(CHJobMessageTypeList.ExportDeclarationActivation, CHLogicalStatusList.Codes.Sent);
	});

	void AssertResendReasonMessageAppears(string messageType, string entryHeaderStatus)
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_NextProcedure = RefCusCodeTestHelper.ValidNextProcedureCode;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		ZFormModaliser.ShowDialogsInTest = true;
		ZFormModaliser.SetDelegateToCallOnFormClosing(frm =>
		{
			if (frm is ResendReasonSendForm confirmSendForm)
			{
				confirmSendForm.Reason = "some reason";
			}
		});

		declaration.JE_MessageType = messageType;
		declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		((CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault()).CH_Status = entryHeaderStatus;
		((CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault()).CH_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var importDeclarationMessageSendingObject = new ImportDeclarationMessageSendingObjectParent(declaration);
		((ImportDeclarationMessageSendingObject)importDeclarationMessageSendingObject.SendingObjectsCollection.FirstOrDefault()).VOCReason = "abc";
		((ImportDeclarationMessageSendingObject)importDeclarationMessageSendingObject.SendingObjectsCollection.FirstOrDefault()).MessageType = PassarMessageTypeList.NI016;
		((ImportDeclarationMessageSendingObject)importDeclarationMessageSendingObject.SendingObjectsCollection.FirstOrDefault()).ShouldSend = true;

		var exportDeclarationMessageSendingObject = new ExportDeclarationMessageSendingObjectParent(declaration);
		((ExportDeclarationMessageSendingObject)exportDeclarationMessageSendingObject.SendingObjectsCollection.FirstOrDefault()).ShouldSend = true;

		Customs.Business.BaseMessageSendingObjectParent messageSendingObjectParent = messageType == Common.Shared.SharedJobMessageTypeList.Codes.Import ? importDeclarationMessageSendingObject : exportDeclarationMessageSendingObject;

		using var form = new MessageSendingFormForTest(messageSendingObjectParent);
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
		form.Show();
		form.SendButton.Enabled = true;
		form.SendButton.PerformClick();
		AssertType<ResendReasonSendForm>("Cancel: ResendReasonSendForm shown", ZFormModaliser.LastFormShownDialogForTest);
		EventsTestHelper.AssertEventNotAdded(declaration, Events.Authorised, withReference: ResendEventReference + "%", message: "Cancelled");
		AssertEquals("Cancelled: SendForm visible after confirmation canceled", true, form.Visible);

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		form.Show();
		form.SendButton.Enabled = true;
		form.SendButton.PerformClick();
		AssertType<ResendReasonSendForm>("Confirm: ResendReasonSendForm shown", ZFormModaliser.LastFormShownDialogForTest);
		EventsTestHelper.AssertEventAdded(declaration, Events.Authorised, withReference: ResendEventReference + "%", expectedReference: ResendEventReference + "|RES=some reason", message: "Confirmed");
		AssertEquals("Confirmed: SendForm closed after confirmation", false, form.Visible);
		AssertEquals("Confirmed: Sendform.DialogResult after confirmation", DialogResult.OK, form.DialogResult);
	}

	const string ResendEventReference = "Submitting when Message status is sent";

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	class MessageSendingFormForTest : MessageSendingForm
	{
		public MessageSendingFormForTest(Customs.Business.BaseMessageSendingObjectParent declarationWrapper)
			: base(declarationWrapper)
		{
		}

		public new ZGrid MessageSendingObjectsGrid => base.MessageSendingObjectsGrid;
		public new ZButton SendButton => base.SendButton;
		public new ZButton CancelButton2 => base.CancelButton2;
		public new KSplitContainer WarningSplitContainer => base.WarningSplitContainer;
		public new ZCheckBox SendWithValidationErrorsCheckBox => base.SendWithValidationErrorsCheckBox;
	}

	class ExportDeclarationMessageSendingObjectParentForTest : ExportDeclarationMessageSendingObjectParent
	{
		public ExportDeclarationMessageSendingObjectParentForTest(JobDeclaration declaration)
			: base(declaration)
		{
		}

		bool _hasErrors;

		public void HasValidationErrors(bool value)
		{
			_hasErrors = value;
			ResetValidationMessages();
		}

		protected override ZString GetBizObjValidationMessageErrors()
		{
			return _hasErrors ? "Error" : ZString.Empty;
		}
	}

	class ImportDeclarationMessageSendingObjectParentForTest : ImportDeclarationMessageSendingObjectParent
	{
		public ImportDeclarationMessageSendingObjectParentForTest(JobDeclaration declaration)
			: base(declaration)
		{
		}

		bool _hasErrors;

		public void HasValidationErrors(bool value)
		{
			_hasErrors = value;
			ResetValidationMessages();
		}

		protected override ZString GetBizObjValidationMessageErrors()
		{
			return _hasErrors ? "Error" : ZString.Empty;
		}
	}
}
