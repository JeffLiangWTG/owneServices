using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	public class Import5UAMessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle()
		{
			var builder = new Import5UAMessageSendingFormBuilder();
			var list = builder.GetColumnStyles();

			AssertEquals(list.Length, 10);
			AssertEquals(nameof(JobDeclarationMessageSendingObject.ShouldSend), list[0].ColumnName);
			AssertEquals(nameof(JobDeclarationMessageSendingObject.FormattedEntryNumber), list[1].ColumnName);
			AssertEquals(nameof(PenaltyExemptionRequestMessageSendingObject.DutyPenaltyExemption5UASequenceNumber), list[2].ColumnName);
			AssertEquals(nameof(PenaltyExemptionRequestMessageSendingObject.PenaltyType), list[3].ColumnName);
			AssertEquals(nameof(PenaltyExemptionRequestMessageSendingObject.AmendmentDeclarationDate), list[4].ColumnName);
			var amendmentDeclarationDate = (ZDateEditColumnStyleInfo)list[4];
			AssertEquals(ZArchitecture.Core.ZDateTimePickerFormat.Short, amendmentDeclarationDate.DateTimeFormat);
			AssertEquals(nameof(PenaltyExemptionRequestMessageSendingObject.AmendmentVersion), list[5].ColumnName);
			AssertEquals(nameof(PenaltyExemptionRequestMessageSendingObject.PenaltyAmountFrom5FK), list[6].ColumnName);
			AssertEquals(nameof(PenaltyExemptionRequestMessageSendingObject.PenaltyExemptionReasonCode), list[7].ColumnName);
			AssertEquals(nameof(PenaltyExemptionRequestMessageSendingObject.PenaltyExemptionReason), list[8].ColumnName);
			AssertEquals(nameof(PenaltyExemptionRequestMessageSendingObject.PenaltyExemptionAmount), list[9].ColumnName);
		}
	}

	[TestedType(typeof(MessageSendingActionForm))]
	sealed class Import5UAMessageSendingActionFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore() => new MessageSendingActionForm(new PenaltyExemptionRequestMessageSendingObjectParent(declaration), new Import5UAMessageSendingFormBuilder());

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			
			var entry = declaration.ActiveEntryHeaders.AddNew();

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			new TestDataSetupHelper(Factory).SetUpAmendmentSessionalData(entry, "1001", "1", DutyTaxCorrectionCodeList.Codes.A);
		}
		JobDeclaration declaration;
	}
}
