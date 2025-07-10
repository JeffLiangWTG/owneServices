using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	class LocalExportLoadingCompletionMessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle()
		{
			var builder = new LocalExportLoadingCompletionMessageSendingFormBuilder();
			var list = builder.GetColumnStyles();

			AssertEquals(list.Length, 6);
			AssertEquals(nameof(JobDeclarationLoadingCompletionMessageSendingObject.ShouldSend), list[0].ColumnName);
			AssertEquals(nameof(JobDeclarationLoadingCompletionMessageSendingObject.CustomsReceiptNumber), list[1].ColumnName);
			AssertEquals(nameof(JobDeclarationLoadingCompletionMessageSendingObject.CustomsOffice), list[2].ColumnName);
			AssertEquals(nameof(JobDeclarationLoadingCompletionMessageSendingObject.Department), list[3].ColumnName);
			AssertEquals(nameof(JobDeclarationLoadingCompletionMessageSendingObject.StevedoresCompany), list[4].ColumnName);
			AssertEquals(nameof(JobDeclarationLoadingCompletionMessageSendingObject.LoadingDate), list[5].ColumnName);
		}

		public void TestGetAmendmentUserControl()
		{
			var builder = new LocalExportLoadingCompletionMessageSendingFormBuilder();

			using (var userControl = builder.GetUserControl())
			{
				AssertEquals(typeof(StevedoresUserControl), userControl.GetType());
			}
		}

		public void TestGetAdditionalTabPages()
		{
			var builder = new LocalExportLoadingCompletionMessageSendingFormBuilder();

			AssertNull(builder.GetAdditionalTabPages(null));
		}
	}

	[TestedType(typeof(MessageSendingActionForm))]
	class LocalExportLoadingCompletionMessageSendingActionFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MessageSendingActionForm(new JobDeclarationLoadingCompletionMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._DF3), new LocalExportLoadingCompletionMessageSendingFormBuilder());
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;

			var person = declaration.Persons.AddNew();
			var glbPerson = Factory.New<GlbPerson>();
			glbPerson.PER_FullName = "Kenny G";
			person.CPN_PER_Person = glbPerson.PK;

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_BGMReference = "12345678901234";
		}
		JobDeclaration declaration;
	}
}
