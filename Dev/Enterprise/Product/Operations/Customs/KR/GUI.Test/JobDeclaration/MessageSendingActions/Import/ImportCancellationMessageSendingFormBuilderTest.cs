using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	class ImportCancellationMessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle_5BF()
		{
			var builder = new ImportCancellationMessageSendingFormBuilder();
			var list = builder.GetColumnStyles();

			AssertEquals(list.Length, 3);
			AssertEquals(nameof(JobDeclarationMessageSendingObject.ShouldSend), list[0].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.FormattedEntryNumber), list[1].ColumnName);
			AssertEquals(nameof(CancellationMessageSendingObject.CancellationReason), list[2].ColumnName);
		}

		public void TestGetUserControl()
		{
			var builder = new ImportCancellationMessageSendingFormBuilder();

			using (var userControl = builder.GetUserControl())
			{
				AssertNull(userControl);
			}
		}

		public void TestGetAdditionalTabPages()
		{
			var builder = new ImportCancellationMessageSendingFormBuilder();

			AssertNull(builder.GetAdditionalTabPages(null));
		}
	}

	[TestedType(typeof(MessageSendingActionForm))]
	sealed class ImportCancellationMessageSendingActionFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore() => new MessageSendingActionForm(new CancellationMessageSendingObjectParent(declaration), new ImportCancellationMessageSendingFormBuilder());
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
		}
		JobDeclaration declaration;
	}
}
