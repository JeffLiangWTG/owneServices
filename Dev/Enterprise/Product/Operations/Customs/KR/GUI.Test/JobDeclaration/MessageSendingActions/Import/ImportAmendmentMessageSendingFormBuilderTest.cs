using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	class ImportAmendmentMessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle()
		{
			var builder = new ImportAmendmentMessageSendingFormBuilder();
			var list = builder.GetColumnStyles();

			AssertEquals(list.Length, 5);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.ShouldSend), list[0].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.FormattedEntryNumber), list[1].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.AmendmentVersion), list[2].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.AmendmentTypeDescription), list[3].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.AmendmentReason), list[4].ColumnName);
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
	sealed class ImportAmendmentMessageSendingActionFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore() => new MessageSendingActionForm(new JobDeclarationAmendmentMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5BB), new ImportAmendmentMessageSendingFormBuilder());
		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			var invoice = declaration.Invoices.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._5BA;
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "IMP";
			var entryNum5BA = entry.EntryNumbers.AddNew();
			entryNum5BA.CE_EntryType = ElectronicDocumentTypeList.Codes._5BA;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;

			var entryLine1 = entry.MergedLines.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_CEI = entryInstruction.PK;

			var entryLine2 = entry.MergedLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_CEI = entryInstruction.PK;

			var import5BA = new Import5BAHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(import5BA))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._5BA, stream);
				Factory.Save();
			}
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._5BA);
		}
		JobDeclaration declaration;
	}
}
