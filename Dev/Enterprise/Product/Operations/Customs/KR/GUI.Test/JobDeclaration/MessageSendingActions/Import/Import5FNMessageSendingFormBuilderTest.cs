using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	class Import5FNMessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle()
		{
			var builder = new Import5FNMessageSendingFormBuilder();
			var list = builder.GetColumnStyles();

			AssertEquals(list.Length, 1);
			AssertEquals(nameof(JobDeclarationMessageSendingObject.FormattedEntryNumber), list[0].ColumnName);
		}

		public void TestGetEntryLineUserControl()
		{
			var builder = new Import5FNMessageSendingFormBuilder();

			using (var userControl = builder.GetUserControl())
			{
				AssertEquals(typeof(Import5FNMessageEntryLinesUserControl), userControl.GetType());
			}
		}

		public void TestGetAdditionalTabPages()
		{
			var builder = new Import5FNMessageSendingFormBuilder();
			var tabPages = builder.GetAdditionalTabPages(null);
			AssertNotNull(tabPages);

			AssertEquals(1, tabPages.Length);
			AssertType(typeof(Import5FNMessageDetailsUserControl), tabPages[0].Controls[0]);

			foreach (var control in tabPages)
			{
				control.Dispose();
			}
		}
	}

	[TestedType(typeof(MessageSendingActionForm))]
	sealed class Import5FNMessageSendingActionFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore() => new MessageSendingActionForm(new JobDeclarationMiscMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5FN, MessageFunctions.MessageFunctionCode.Original), new Import5FNMessageSendingFormBuilder());

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_IsSpecificUseCode = true;
			invoiceLine.JI_CL = entryLine.PK;
		}
		JobDeclaration declaration;
	}
}
