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

namespace Enterprise.Customs.KR.GUI.Testing
{
	class Import105MessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle()
		{
			var builder = new Import105MessageSendingFormBuilder();
			var list = builder.GetColumnStyles();

			AssertEquals(list.Length, 7);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.ShouldSend), list[0].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.FormattedEntryNumber), list[1].ColumnName);
			AssertEquals(nameof(JobDeclarationMiscMessageSendingObjectCore.Schema.AmendmentVersion), list[2].ColumnName);
			AssertEquals(nameof(JobDeclarationMiscMessageSendingObjectCore.Schema.AmendmentTypeDescription), list[3].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.LawCodeDescription), list[4].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.CustomsDisbursementBillNumber), list[5].ColumnName);
			AssertEquals(nameof(JobDeclarationMiscMessageSendingObjectCore.Schema.AmendmentReason), list[6].ColumnName);
		}

		public void TestGetAmendmentUserControl()
		{
			var builder = new ImportAmendmentMessageSendingFormBuilder();

			using (var userControl = builder.GetUserControl())
			{
				AssertEquals(typeof(AmendedItemsUserControl), userControl.GetType());
			}
		}

		public void TestGetAdditionalTabPages()
		{
			var builder = new ImportAmendmentMessageSendingFormBuilder();

			AssertNull(builder.GetAdditionalTabPages(null));
		}
	}

	[TestedType(typeof(MessageSendingActionForm))]
	sealed class Import105MessageSendingActionFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore() => new MessageSendingActionForm(new JobDeclarationAmendmentMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._105), new Import105MessageSendingFormBuilder());

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_SequenceNumber = 1;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders[0];
			var entryLine = entry.MergedLines[0];
			entryLine.CL_FTASequenceNumber = 1;
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5SC);

			var ftaHeader = new ImportFTACreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(ftaHeader))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._5SC, stream);
				Factory.Save();
			}
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._5SC);
			Factory.Save();

			invoiceLine.JI_Tariff = "010102";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}
		JobDeclaration declaration;
	}
}
