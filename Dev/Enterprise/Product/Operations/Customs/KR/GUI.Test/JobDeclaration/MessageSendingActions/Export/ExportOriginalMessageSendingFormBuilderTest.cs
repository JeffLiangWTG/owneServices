using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	class ExportOriginalMessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle()
		{
			var builder = new ExportOriginalMessageSendingFormBuilder();
			var list = builder.GetColumnStyles();

			AssertEquals(list.Length, 2);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.ShouldSend), list[0].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.FormattedEntryNumber), list[1].ColumnName);
		}

		public void TestGetAmendmentUserControl()
		{
			var builder = new ExportOriginalMessageSendingFormBuilder();

			using (var userControl = builder.GetUserControl())
			{
				AssertNull(userControl);
			}
		}
		public void TestGetAdditionalTabPages()
		{
			var builder = new ExportOriginalMessageSendingFormBuilder();

			AssertNull(builder.GetAdditionalTabPages(null));
		}
	}

	[TestedType(typeof(MessageSendingActionForm))]
	class ExportOriginalMessageSendingActionFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MessageSendingActionForm(new JobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._830), new ExportOriginalMessageSendingFormBuilder());
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();
		}
		JobDeclaration declaration;
	}
}
