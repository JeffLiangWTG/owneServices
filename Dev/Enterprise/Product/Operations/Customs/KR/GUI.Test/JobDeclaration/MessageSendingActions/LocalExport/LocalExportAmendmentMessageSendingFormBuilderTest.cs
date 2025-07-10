using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.GUI.Testing
{
	class LocalExportAmendmentMessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle_5DP5DQ()
		{
			var builder = new LocalExportAmendmentMessageSendingFormBuilder(MessageFunctionCode.Amendment);
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
			var builder = new LocalExportAmendmentMessageSendingFormBuilder(MessageFunctionCode.Amendment);

			using (var userControl = builder.GetUserControl())
			{
				AssertEquals(typeof(AmendedItemsUserControl), userControl.GetType());
			}
		}

		public void TestGetAdditionalTabPages()
		{
			var builder = new LocalExportAmendmentMessageSendingFormBuilder(MessageFunctionCode.Amendment);

			AssertNull(builder.GetAdditionalTabPages(null));
		}
	}

	[TestedType(typeof(MessageSendingActionForm))]
	class LocalExportAmendmentMessageSendingActionForm5DRTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MessageSendingActionForm(new JobDeclarationAmendmentMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5DR), new LocalExportAmendmentMessageSendingFormBuilder(MessageFunctionCode.Amendment));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders[0];

			var header = new LocalExport5DPEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._5DP, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._5DP);
				Factory.Save();
			}

			invoiceLine.JI_Tariff = "1234990000";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}
		JobDeclaration declaration;
	}

	[TestedType(typeof(MessageSendingActionForm))]
	class LocalExportAmendmentMessageSendingActionForm5DSTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MessageSendingActionForm(new JobDeclarationAmendmentMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5DS), new LocalExportAmendmentMessageSendingFormBuilder(MessageFunctionCode.Amendment));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders[0];

			var header = new LocalExport5DQEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._5DQ, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._5DQ);
				Factory.Save();
			}

			invoiceLine.JI_Tariff = "1234990000";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}
		JobDeclaration declaration;
	}
}
