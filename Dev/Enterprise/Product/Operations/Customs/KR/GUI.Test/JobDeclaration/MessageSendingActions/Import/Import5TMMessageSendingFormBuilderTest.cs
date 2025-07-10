using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	class Import5TMMessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle()
		{
			var builder = new Import5TMMessageSendingFormBuilder();
			var list = builder.GetColumnStyles();

			AssertEquals(list.Length, 3);
			AssertEquals(nameof(JobDeclarationMiscMessageSendingObject.ShouldSend), list[0].ColumnName);
			AssertEquals(nameof(JobDeclarationMiscMessageSendingObject.FormattedEntryNumber), list[1].ColumnName);
			AssertEquals(nameof(JobDeclarationMiscMessageSendingObject.PayerBusinessNumber), list[2].ColumnName);
		}

		public void TestGetAmendmentUserControl()
		{
			var builder = new Import5TMMessageSendingFormBuilder();
			using (var userControl = builder.GetUserControl())
			{
				AssertEquals(typeof(GoldVATDeclarationEntryLinesUserControl), userControl.GetType());
			}
		}

		public void GetUserControlGroupBoxCaption()
		{
			var builder = new Import5TMMessageSendingFormBuilder();
			AssertEquals("Entry Lines", builder.GetUserControlGroupBoxCaption().ToString());
		}
	}

	[TestedType(typeof(MessageSendingActionForm))]
	sealed class Import5TMMessageSendingActionFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore() => new MessageSendingActionForm(new JobDeclarationMiscMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5TM, MessageFunctions.MessageFunctionCode.Original), new Import5TMMessageSendingFormBuilder());

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.MergedLines.AddNew();
		}
		JobDeclaration declaration;
	}
}
