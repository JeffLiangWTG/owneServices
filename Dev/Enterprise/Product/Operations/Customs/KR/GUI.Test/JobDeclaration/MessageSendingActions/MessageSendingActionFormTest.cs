using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MessageSendingActionForm))]
	class MessageSendingActionFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MessageSendingActionForm(new JobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._929), new ImportOriginalMessageSendingFormBuilder());
		}

		public void TestOriginalColumns()
		{
			using (var form = new MessageSendingActionForm(new JobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._929), new ImportOriginalMessageSendingFormBuilder()))
			{
				form.Show();
				var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				Assert(grid.Columns.Contains(nameof(JobDeclarationMessageSendingObject.FormattedEntryNumber)));
			}
		}

		public void TestClickSendButtonWithMessageErrorsWithNoSecurityRights()
		{
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = "OAC";
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			declaration.RunPreSaveValidation();
			var messageSendingObjectParent = new AgreedRateMessageSendingObjectParent(declaration);
			var builder = new AgreedRateMessageSendingFormBuilder();
			using (var form = new MessageSendingActionForm(messageSendingObjectParent, builder))
			{
				form.Show();
				var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				grid.Select(0);
				var firstLine = (AgreedRateMessageSendingObject)grid.GetFirstSelectedRow();
				firstLine.ShouldSend = true;

				AssertHasErrorContaining(firstLine.ShouldSendInfo, "The selected entry has message errors. Unless you fix them, you will not be able to send a message as you don't have the security right to send with message errors.");
				AssertNoWarningContaining(firstLine.ShouldSendInfo, "The selected entry has message errors. Please review them. However you will be able to send a message as you have the security right to send with message errors.");
				grid.UnSelect(0);
			}
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
			using (var form = new MessageSendingActionForm(messageSendingObjectParent, builder))
			{
				form.Show();
				var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				grid.Select(0);
				var firstLine = (AgreedRateMessageSendingObject)grid.GetFirstSelectedRow();
				firstLine.ShouldSend = true;
				AssertNoErrorContaining(firstLine.ShouldSendInfo, "The selected entry has message errors. Unless you fix them, you will not be able to send a message as you don't have the security right to send with message errors.");
				AssertHasWarningContaining(firstLine.ShouldSendInfo, "The selected entry has message errors. Please review them. However you will be able to send a message as you have the security right to send with message errors.");
				grid.UnSelect(0);
			}
		}

		public void TestTabControlsInValidationErrors()
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = "OAC";
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var messageSendingObjectParent_0 = new AgreedRateMessageSendingObjectParent(declaration);
			var builder_0 = new AgreedRateMessageSendingFormBuilder();

			using (var form = new MessageSendingActionForm(messageSendingObjectParent_0, builder_0))
			{
				form.Show();
				var validationErrorsTextBox = form.FindSingle<ZTextBox>("ValidationErrorsTextBox");
				AssertType(typeof(ZGroupBox), validationErrorsTextBox.Parent);
			}

			var messageSendingObjectParent_1 = new JobDeclarationMiscMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5FN, MessageFunctions.MessageFunctionCode.Original);
			var builder_1 = new Import5FNMessageSendingFormBuilder();

			using (var form = new MessageSendingActionForm(messageSendingObjectParent_1, builder_1))
			{
				form.Show();
				var validationErrorsTextBox = form.FindSingle<ZTextBox>("ValidationErrorsTextBox");
				AssertType(typeof(ZTabPage), validationErrorsTextBox.Parent);
			}
		}

		public void TestCheckIsOKToSend5FN()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DutyReductionExemption);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A093000004", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제93조제4호 해당물품");
			helper.CreateTariffAttribute(Customs.KR.Messaging.Constants.ZZ.TariffAttributes.IsDutyExempt, YesNo.Yes, tariff1);
			Factory.Save();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = "OAC";
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SecondaryPreference = "A093000004";

			var messageSendingObjectParent = new JobDeclarationMiscMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5FN, MessageFunctions.MessageFunctionCode.Original);
			var builder = new Import5FNMessageSendingFormBuilder();

			using (var form = new MessageSendingActionFormForTest(messageSendingObjectParent, builder))
			{
				form.Show();

				var messageGrid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				messageGrid.Select(0);
				var firstObject = (JobDeclarationMiscMessageSendingObject)messageGrid.GetFirstSelectedRow();
				firstObject.ShouldSend = true;

				var entryLineGrid = form.FindSingle<ZGrid>("EntryLinesGrid");
				entryLineGrid.Select(0);
				var firstLineObject = (MessageSendingEntryLineObject)entryLineGrid.GetFirstSelectedRow();
				firstLineObject.ShouldSend = false;

				AssertEquals("Cant send", false, form.CheckIsOKToSend_Exposed());

				messageGrid.Select(0);
				firstObject.ShouldSend = false;

				entryLineGrid.Select(0);
				firstLineObject.ShouldSend = true;

				AssertEquals("Can send", true, form.CheckIsOKToSend_Exposed());
			}
		}

		public void TestValidationTextBox()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DutyReductionExemption);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A093000004", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제93조제4호 해당물품");
			helper.CreateTariffAttribute(Customs.KR.Messaging.Constants.ZZ.TariffAttributes.IsDutyExempt, YesNo.Yes, tariff1);
			Factory.Save();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = "OAC";
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SecondaryPreference = "A093000004";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;

			var messageSendingObjectParent = new JobDeclarationMiscMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5FN, MessageFunctions.MessageFunctionCode.Original);
			var builder = new Import5FNMessageSendingFormBuilder();

			using var form = new MessageSendingActionFormForTest(messageSendingObjectParent, builder);
			form.Show();

			var entryLineGrid = form.FindSingle<ZGrid>("EntryLinesGrid");
			entryLineGrid.Select(0);
			var firstLineObject = (MessageSendingEntryLineObject)entryLineGrid.GetFirstSelectedRow();
			firstLineObject.ShouldSend = true;

			var validationErrorTextBox = form.FindSingle<ZTextBox>("ValidationErrorsTextBox");
			AssertNotNullOrEmpty(validationErrorTextBox.Text);
			AssertHasNotifications("Customs Value (KRW): Customs Value (KRW) cannot be zero.", entryLine.CL_CustomsValueInfo);
			AssertContains("Customs Value (KRW): Customs Value (KRW) cannot be zero.", validationErrorTextBox.Text);

			AssertHasNotifications("Freight (KRW): Freight (KRW) cannot be zero.", entry.FreightInfo);
			AssertContains("Freight (KRW): Freight (KRW) cannot be zero.", validationErrorTextBox.Text);

			AssertHasNotifications("Total Gross Weight: Please enter a 'Total Gross Weight' greater than 0.", invoice.JZ_WeightInfo);
			AssertContains("Total Gross Weight: Please enter a 'Total Gross Weight' greater than 0.", validationErrorTextBox.Text);

			AssertHasNotifications("C/O Issued: You have not entered a C/O Issued.", invoiceLine.CertificateOfOriginIssueStatusInfo);
			AssertContains("C/O Issued: You have not entered a C/O Issued.", validationErrorTextBox.Text);

			AssertHasNotifications("Agreed Rate: You have not entered an Agreed Rate.", entryInstruction.CEI_AgreedRateAppInfo);
			AssertContains("Agreed Rate: You have not entered an Agreed Rate.", validationErrorTextBox.Text);

			AssertHasNotifications("Inspection Date: You have not entered an Inspection Date.", declaration.InspectionDateInfo);
			AssertContains("Inspection Date: You have not entered an Inspection Date.", validationErrorTextBox.Text);

			firstLineObject.ShouldSend = false;
			AssertNullOrEmpty(validationErrorTextBox.Text);
		}

		class MessageSendingActionFormForTest : MessageSendingActionForm
		{
			public MessageSendingActionFormForTest(IJobDeclarationMessageSendingObjectParent declarationWrapper, MessageSendingFormBuilder builder) : base(declarationWrapper, builder) { }
			public bool CheckIsOKToSend_Exposed() => base.CheckIsOKToSend();
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
	}
}
