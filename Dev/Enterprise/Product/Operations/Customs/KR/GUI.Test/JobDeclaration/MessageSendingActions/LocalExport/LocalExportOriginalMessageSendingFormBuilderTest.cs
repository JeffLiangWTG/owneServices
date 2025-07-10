using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	class LocalExportOriginalMessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle()
		{
			var builder = new LocalExportOriginalMessageSendingFormBuilder();
			var list = builder.GetColumnStyles();

			AssertEquals(list.Length, 2);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.ShouldSend), list[0].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.FormattedEntryNumber), list[1].ColumnName);
		}

		public void TestGetAmendmentUserControl()
		{
			var builder = new LocalExportOriginalMessageSendingFormBuilder();

			using (var userControl = builder.GetUserControl())
			{
				AssertNull(userControl);
			}
		}

		public void TestGetAdditionalTabPages()
		{
			var builder = new LocalExportOriginalMessageSendingFormBuilder();

			AssertNull(builder.GetAdditionalTabPages(null));
		}
	}

	[TestedType(typeof(MessageSendingActionForm))]
	class LocalExportOriginalMessageSendingActionForm5DPTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MessageSendingActionForm(new JobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5DP), new LocalExportOriginalMessageSendingFormBuilder());
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			declaration.ActiveEntryHeaders.AddNew();
		}
		JobDeclaration declaration;
	}

	[TestedType(typeof(MessageSendingActionForm))]
	class LocalExportOriginalMessageSendingActionForm5DQTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MessageSendingActionForm(new JobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5DQ), new LocalExportOriginalMessageSendingFormBuilder());
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			declaration.ActiveEntryHeaders.AddNew();
		}
		JobDeclaration declaration;
	}
}
