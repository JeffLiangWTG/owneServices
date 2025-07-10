using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	class Import5SIMessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle()
		{
			var builder = new Import5SIMessageSendingFormBuilder();
			var list = builder.GetColumnStyles();

			AssertEquals(list.Length, 4);
			AssertEquals(nameof(MailItemIDsMessageSendingObject.ShouldSend), list[0].ColumnName);
			AssertEquals(nameof(MailItemIDsMessageSendingObject.FormattedEntryNumber), list[1].ColumnName);
			AssertEquals(nameof(MailItemIDsMessageSendingObject.DeclarationCustomsOffice), list[2].ColumnName);
			AssertEquals(nameof(MailItemIDsMessageSendingObject.DeclarationCustomsDivision), list[3].ColumnName);
		}

		public void TestGetAmendmentUserControl()
		{
			var builder = new Import5SIMessageSendingFormBuilder();

			using (var userControl = builder.GetUserControl())
			{
				AssertEquals("To be worked on in other WIs(WI00687308)", null, userControl);
			}
		}

		public void TestGetAdditionalTabPages()
		{
			var builder = new Import5SIMessageSendingFormBuilder();

			AssertNull(builder.GetAdditionalTabPages(null));
		}
	}

	[TestedType(typeof(MessageSendingActionForm))]
	sealed class Import5SIMessageSendingActionFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore() => new MessageSendingActionForm(new MailItemIDsMessageSendingObjectParent(declaration), new Import5SIMessageSendingFormBuilder());

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
