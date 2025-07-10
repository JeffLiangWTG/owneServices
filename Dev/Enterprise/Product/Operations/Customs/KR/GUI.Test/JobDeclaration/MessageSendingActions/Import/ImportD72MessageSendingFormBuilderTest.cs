using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	class ImportD72MessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle()
		{
			var builder = new ImportD72MessageSendingFormBuilder();
			var list = builder.GetColumnStyles();

			AssertEquals(5, list.Length);
			AssertEquals(nameof(ExtendReExportDateMessageSendingObject.ShouldSend), list[0].ColumnName);
			AssertEquals(nameof(ExtendReExportDateMessageSendingObject.FormattedEntryNumber), list[1].ColumnName);
			AssertEquals(nameof(ExtendReExportDateMessageSendingObject.CurrentReExportScheduledDate), list[2].ColumnName);
			AssertEquals(nameof(ExtendReExportDateMessageSendingObject.NewReExportDate), list[3].ColumnName);
			AssertEquals(nameof(ExtendReExportDateMessageSendingObject.ReasonDescription), list[4].ColumnName);
		}

		public void TestGetUserControl()
		{
			var builder = new ImportD72MessageSendingFormBuilder();

			using (var userControl = builder.GetUserControl())
			{
				AssertEquals(typeof(ImportD72MessageDetailsUserControl), userControl.GetType());
			}
		}

		public void TestGetAdditionalTabPages()
		{
			var builder = new ImportD72MessageSendingFormBuilder();
			AssertNull(builder.GetAdditionalTabPages(null));
		}
	}

	[TestedType(typeof(MessageSendingActionForm))]
	class ImportD72MessageSendingActionFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var parent = JobDeclarationMessageSendingObjectParent.GetJobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._D72, MessageFunctions.MessageFunctionCode.Original);
			return new MessageSendingActionForm(parent, new ImportD72MessageSendingFormBuilder());
		}
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_ScheduledReExportDate = ZDateTime.Today;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
		}
		JobDeclaration declaration;
	}
}
