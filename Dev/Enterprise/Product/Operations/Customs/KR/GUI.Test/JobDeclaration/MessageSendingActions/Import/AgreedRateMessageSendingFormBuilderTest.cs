using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	class AgreedRateMessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle()
		{
			var builder = new AgreedRateMessageSendingFormBuilder();
			var list = builder.GetColumnStyles();

			AssertEquals(list.Length, 5);
			AssertEquals(nameof(AgreedRateMessageSendingObject.ShouldSend), list[0].ColumnName);
			AssertEquals(nameof(AgreedRateMessageSendingObject.FormattedEntryNumber), list[1].ColumnName);
			AssertEquals(nameof(AgreedRateMessageSendingObject.DeclarationDate), list[2].ColumnName);
			AssertEquals(nameof(AgreedRateMessageSendingObject.PreferenceCodeDescription), list[3].ColumnName);
			AssertEquals(nameof(AgreedRateMessageSendingObject.DutyRate), list[4].ColumnName);
		}

		public void TestGetUserControl()
		{
			var builder = new AgreedRateMessageSendingFormBuilder();

			using (var userControl = builder.GetUserControl())
			{
				AssertEquals(typeof(AgreedRateMessageDetailsUserControl), userControl.GetType());
			}
		}

		public void TestGetAdditionalTabPages()
		{
			var builder = new AgreedRateMessageSendingFormBuilder();

			AssertNull(builder.GetAdditionalTabPages(null));
		}
	}

	[TestedType(typeof(MessageSendingActionForm))]
	sealed class AgreedRateMessageSendingActionFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore() => new MessageSendingActionForm(new AgreedRateMessageSendingObjectParent(declaration), new AgreedRateMessageSendingFormBuilder());

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.EntryNumber = "1234524000001M";
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MergedLines.AddNew();
		}
		JobDeclaration declaration;
	}
}
