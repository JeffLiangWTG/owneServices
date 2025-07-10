using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.BE.NCTS.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.BE.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.BE.NCTS.GUI.Testing;

[TestedType(typeof(MessageSendingForm))]
sealed class MessageSendingFormTest : Customs.GUI.Testing.MessageSendingFormWithValidationDetailsAbstractTest
{
	protected override Form GetFormToBashCore() => new MessageSendingForm(new MessageSendingActionParent(NctsHeader));

	public void TestAvailableColumns()
	{
		form.Show();
		AssertSequencesEqual("Columns", new[]
			{
				NctsHeaderMessageSendingObject.Schema.ShouldSend,
				MessageSendingAction.Schema.IsTestDeclaration,
				MessageSendingAction.Schema.AdditionalDeclarationType,
				AutoNctsHeaderMessageSendingObject.Schema.LRN,
				AutoNctsHeaderMessageSendingObject.Schema.MRN,
				MessageSendingAction.Schema.EntryType,
				MessageSendingAction.Schema.EntryStatus
			},
			messageSendingActionGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
	}

	public void TestColumnsWidth()
	{
		CombineAssertions(() =>
		{
			AssertEquals(nameof(NctsHeaderMessageSendingObject.Schema.ShouldSend), 40, messageSendingActionGrid.GetColumnStyle(NctsHeaderMessageSendingObject.Schema.ShouldSend).Width);
			AssertEquals(nameof(MessageSendingAction.Schema.AdditionalDeclarationType), 100, messageSendingActionGrid.GetColumnStyle(MessageSendingAction.Schema.AdditionalDeclarationType).Width);
			AssertEquals(nameof(AutoNctsHeaderMessageSendingObject.Schema.LRN), 160, messageSendingActionGrid.GetColumnStyle(AutoNctsHeaderMessageSendingObject.Schema.LRN).Width);
			AssertEquals(nameof(AutoNctsHeaderMessageSendingObject.Schema.MRN), 160, messageSendingActionGrid.GetColumnStyle(AutoNctsHeaderMessageSendingObject.Schema.MRN).Width);
			AssertEquals(nameof(MessageSendingAction.Schema.EntryType), 100, messageSendingActionGrid.GetColumnStyle(MessageSendingAction.Schema.EntryType).Width);
			AssertEquals(nameof(MessageSendingAction.Schema.EntryStatus), 100, messageSendingActionGrid.GetColumnStyle(MessageSendingAction.Schema.EntryStatus).Width);
			AssertEquals(nameof(MessageSendingAction.Schema.IsTestDeclaration), 40, messageSendingActionGrid.GetColumnStyle(MessageSendingAction.Schema.IsTestDeclaration).Width);
		});
	}

	public void TestBottomSectionUserControl()
	{
		form.Show();
		var bottomSectionUserControl = form.Controls.Find("MessageSendingFormBottomSectionUserControl", true).Single();
		CombineAssertions(() =>
		{
			AssertType<MessageSendingFormBottomSectionUserControl>("Control Type", bottomSectionUserControl);
			AssertEquals("Binding", ".", bottomSectionUserControl.GetBindingMember());
		});
	}

	public void TestSurpressValidationsByEntryType_INV()
	{
		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
		movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.PreLodged;
		movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Cancellation;
		form.Show();
		var validationErrorsTextBox = form.FindSingle<ZTextBox>(x => x.Name == "ValidationErrorsTextBox");
		var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>(x => x.Name == "SendWithValidationErrorsCheckBox");
		var senderButton = form.FindSingle<ZButton>(x => x.Name == "SendButton");
		CombineAssertions(() =>
		{
			Assert("ValidationErrorsTextBox is visible", validationErrorsTextBox.Visible);
			Assert("SendWithValidationErrorsCheckBox is visible", sendWithValidationErrorsCheckBox.Visible);
			Assert("SendButton is disabled", !senderButton.Enabled);

			sendWithValidationErrorsCheckBox.Checked = true;
			Assert("SendButton is enabled after checked the SendWithValidationErrorsCheckBox", senderButton.Enabled);
		});

		action.EntryType = NctsMessageTypeList.Codes.InvalidationCancellation;
		CombineAssertions(() =>
		{
			Assert("ValidationErrorsTextBox is invisible", !validationErrorsTextBox.Visible);
			Assert("SendWithValidationErrorsCheckBox is invisible", !sendWithValidationErrorsCheckBox.Visible);
			Assert("SendButton is enabled", senderButton.Enabled);
		});
	}

	public void TestSurpressValidationsByEntryType_RRL()
	{
		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
		movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.DecisionToControl;
		movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
		form.Show();
		var validationErrorsTextBox = form.FindSingle<ZTextBox>(x => x.Name == "ValidationErrorsTextBox");
		var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>(x => x.Name == "SendWithValidationErrorsCheckBox");
		var senderButton = form.FindSingle<ZButton>(x => x.Name == "SendButton");
		CombineAssertions(() =>
		{
			Assert("ValidationErrorsTextBox is visible", validationErrorsTextBox.Visible);
			Assert("SendWithValidationErrorsCheckBox is visible", sendWithValidationErrorsCheckBox.Visible);
			Assert("SendButton is disabled", !senderButton.Enabled);

			sendWithValidationErrorsCheckBox.Checked = true;
			Assert("SendButton is enabled after checked the SendWithValidationErrorsCheckBox", senderButton.Enabled);
		});

		action.EntryType = NctsMessageTypeList.Codes.RequestARelease;
		CombineAssertions(() =>
		{
			Assert("ValidationErrorsTextBox is invisible", !validationErrorsTextBox.Visible);
			Assert("SendWithValidationErrorsCheckBox is invisible", !sendWithValidationErrorsCheckBox.Visible);
			Assert("SendButton is enabled", senderButton.Enabled);
		});
	}

	public void TestSurpressValidationsByEntryType_RNM()
	{
		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
		movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry;
		movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
		form.Show();
		var validationErrorsTextBox = form.FindSingle<ZTextBox>(x => x.Name == "ValidationErrorsTextBox");
		var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>(x => x.Name == "SendWithValidationErrorsCheckBox");
		var senderButton = form.FindSingle<ZButton>(x => x.Name == "SendButton");
		CombineAssertions(() =>
		{
			Assert("ValidationErrorsTextBox is visible", validationErrorsTextBox.Visible);
			Assert("SendWithValidationErrorsCheckBox is visible", sendWithValidationErrorsCheckBox.Visible);
			Assert("SendButton is disabled", !senderButton.Enabled);

			sendWithValidationErrorsCheckBox.Checked = true;
			Assert("SendButton is enabled after checked the SendWithValidationErrorsCheckBox", senderButton.Enabled);
		});

		action.EntryType = NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement;
		CombineAssertions(() =>
		{
			Assert("ValidationErrorsTextBox is invisible", !validationErrorsTextBox.Visible);
			Assert("SendWithValidationErrorsCheckBox is invisible", !sendWithValidationErrorsCheckBox.Visible);
			Assert("SendButton is enabled", senderButton.Enabled);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		messageSendingActionParent = new MessageSendingActionParent(NctsHeader);
		action = messageSendingActionParent.SendingObjectsCollection[0];
		action.ShouldSend = true;
	
		form = new MessageSendingForm(messageSendingActionParent);
		messageSendingActionGrid = form.FindSingle<ZGrid>();
	}
	MessageSendingForm form;
	ZGrid messageSendingActionGrid;
	MessageSendingActionParent messageSendingActionParent;
	MessageSendingAction action;

	protected override void TearDown()
	{
		base.TearDown();
		form.Dispose();
	}

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNtcsHeader());
	NctsHeader nctsHeader;

	NctsHeader CreateNtcsHeader()
	{
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		return nctsHeader;
	}
}
