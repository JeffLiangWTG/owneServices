using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.GUI.Testing
{
	class LocalExportCancellationMessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle_5DP5DQ()
		{
			var builder = new LocalExportAmendmentMessageSendingFormBuilder(MessageFunctionCode.Cancellation);
			var list = builder.GetColumnStyles();

			AssertEquals(list.Length, 4);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.ShouldSend), list[0].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.CustomsReceiptNumber), list[1].ColumnName);
			AssertEquals(expected: true, list[1].IsReadOnly);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.ReasonCode), list[2].ColumnName);
			AssertEquals(expected: false, list[2].IsReadOnly);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.AmendmentReason), list[3].ColumnName);
			AssertEquals(expected: false, list[3].IsReadOnly);
		}

		public void TestGetUserControl()
		{
			var builder = new LocalExportAmendmentMessageSendingFormBuilder(MessageFunctionCode.Cancellation);

			using (var userControl = builder.GetUserControl())
			{
				AssertNull(userControl);
			}
		}

		public void TestGetAdditionalTabPages()
		{
			var builder = new LocalExportAmendmentMessageSendingFormBuilder(MessageFunctionCode.Cancellation);

			AssertNull(builder.GetAdditionalTabPages(null));
		}
	}

	[TestedType(typeof(MessageSendingActionForm))]
	class LocalExportCancellationMessageSendingActionForm5DRTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MessageSendingActionForm(new JobDeclarationAmendmentMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5DR), new LocalExportAmendmentMessageSendingFormBuilder(MessageFunctionCode.Cancellation));
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
	class LocalExportCancellationMessageSendingActionForm5DSTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MessageSendingActionForm(new JobDeclarationAmendmentMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5DS), new LocalExportAmendmentMessageSendingFormBuilder(MessageFunctionCode.Cancellation));
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
