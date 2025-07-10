using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class Import5BDMessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle()
		{
			var builder = new Import5BDMessageSendingFormBuilder();
			var list = builder.GetColumnStyles();

			AssertEquals(list.Length, 9);
			AssertEquals(nameof(JobDeclarationMessageSendingObject.ShouldSend), list[0].ColumnName);
			AssertEquals(nameof(JobDeclarationMessageSendingObject.FormattedEntryNumber), list[1].ColumnName);
			AssertEquals(nameof(EarlyReleaseMiscMessageSendingObject.AmendmentReason), list[2].ColumnName);
			AssertEquals(nameof(EarlyReleaseMiscMessageSendingObject.SecurityType), list[3].ColumnName);
			AssertEquals(nameof(EarlyReleaseMiscMessageSendingObject.OtherSecurityType), list[4].ColumnName);
			AssertEquals(nameof(EarlyReleaseMiscMessageSendingObject.SecurityStartDate), list[5].ColumnName);
			AssertEquals(nameof(EarlyReleaseMiscMessageSendingObject.SecurityEndDate), list[6].ColumnName);
			AssertEquals(nameof(EarlyReleaseMiscMessageSendingObject.SecurityAmount), list[7].ColumnName);
			AssertEquals(nameof(EarlyReleaseMiscMessageSendingObject.ReasonForEarlyRemoval), list[8].ColumnName);
		}
	}

	[TestedType(typeof(MessageSendingActionForm))]
	sealed class Import5BDMessageSendingActionFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore() => new MessageSendingActionForm(new EarlyReleaseMiscMessageSendingObjectParent(declaration), new Import5BDMessageSendingFormBuilder());

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
