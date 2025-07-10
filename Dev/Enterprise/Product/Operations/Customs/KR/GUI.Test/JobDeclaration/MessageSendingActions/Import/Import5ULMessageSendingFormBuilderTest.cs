using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	class Import5ULMessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle()
		{
			var builder = new Import5ULMessageSendingFormBuilder();
			var list = builder.GetColumnStyles();

			AssertEquals(11, list.Length);
			AssertEquals(nameof(PenaltyRefundRequestMessageSendingObject.ShouldSend), list[0].ColumnName);
			AssertEquals(nameof(PenaltyRefundRequestMessageSendingObject.FormattedEntryNumber), list[1].ColumnName);
			AssertEquals(nameof(PenaltyRefundRequestMessageSendingObject.FormattedCustomsDisbursementBillNumber), list[2].ColumnName);
			AssertEquals(nameof(PenaltyRefundRequestMessageSendingObject.Amendment5WNNumber), list[3].ColumnName);
			AssertEquals(nameof(PenaltyRefundRequestMessageSendingObject.SubmissionDate), list[4].ColumnName);
			AssertEquals(nameof(PenaltyRefundRequestMessageSendingObject.AmendmentVersionNoCustoms), list[5].ColumnName);
			AssertEquals(nameof(PenaltyRefundRequestMessageSendingObject.PaymentAmount), list[6].ColumnName);
			AssertEquals(nameof(PenaltyRefundRequestMessageSendingObject.RefundType), list[7].ColumnName);
			AssertEquals(nameof(PenaltyRefundRequestMessageSendingObject.RefundCause), list[8].ColumnName);
			AssertEquals(nameof(PenaltyRefundRequestMessageSendingObject.RefundReason), list[9].ColumnName);
			AssertEquals(nameof(PenaltyRefundRequestMessageSendingObject.TaxOffice), list[10].ColumnName);
		}

		[TestedType(typeof(MessageSendingActionForm))]
		class Import5ULMessageSendingActionFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
		{
			protected override Form GetFormToBashCore()
			{
				var parent = JobDeclarationMessageSendingObjectParent.GetJobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5UL, MessageFunctions.MessageFunctionCode.Original);
				return new MessageSendingActionForm(parent, new Import5ULMessageSendingFormBuilder());
			}
			protected override void SetUp()
			{
				base.SetUp();
				declaration = Factory.New<JobDeclaration>();
				declaration.Invoices.AddNew();
				declaration.InvoiceLines.AddNew();
				declaration.ActiveEntryHeaders.AddNew();
			}
			JobDeclaration declaration;
		}
	}
}
