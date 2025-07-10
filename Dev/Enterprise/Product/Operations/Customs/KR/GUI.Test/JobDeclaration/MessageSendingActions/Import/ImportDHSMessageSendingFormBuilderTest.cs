using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	class ImportDHSMessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle()
		{
			var builder = new ImportDHSMessageSendingFormBuilder();
			var list = builder.GetColumnStyles();

			AssertEquals(list.Length, 8);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.ShouldSend), list[0].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.FormattedEntryNumber), list[1].ColumnName);
			AssertEquals(nameof(JobDeclarationMiscMessageSendingObjectCore.Schema.AmendmentVersion), list[2].ColumnName);
			AssertEquals(nameof(JobDeclarationMiscMessageSendingObjectCore.Schema.AmendmentTypeDescription), list[3].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.AmendmentTypeForInvoiceLine), list[4].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.LawCodeDescription), list[5].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.CustomsDisbursementBillNumber), list[6].ColumnName);
			AssertEquals(nameof(JobDeclarationMiscMessageSendingObjectCore.Schema.AmendmentReason), list[7].ColumnName);
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
	class ImportDHSMessageSendingActionFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var parent = JobDeclarationMessageSendingObjectParent.GetJobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._DHS, MessageFunctions.MessageFunctionCode.Amendment);
			return new MessageSendingActionForm(parent, new ImportDHSMessageSendingFormBuilder());
		}
		protected override void SetUp()
		{
			base.SetUp();
			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.OH_FullName = "상호";
			TestOrgDataSetUpHelper.AddOrgContact(broker, "name", true);
			broker.MainAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.Branch.GB_OH_OrgProxy = broker.PK;
			declaration.JE_CustomsOffice = "130";
			declaration.JE_CustomsDivision = "10";
			declaration.JE_ExportDate = new ZDateTime(2023, 07, 18);
			var invoice = declaration.Invoices.AddNew();

			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_FTARelationArticleCode = "4";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_FTASequenceNumber = 1;
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.CusEntryLine.CL_LineNumber = 1;
			invoiceLine1.CusEntryLine.CL_FTASequenceNumber = 1;
			invoiceLine1.JI_RN_NKSecondCommercialInvoiceCountry = ZString.Empty;
			invoiceLine1.JI_COOSupportingDocType = "1";
			invoiceLine1.JI_SequenceNumber = 1;

			entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 2;
			entryLine.CL_FTASequenceNumber = 2;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.CusEntryLine.CL_FTASequenceNumber = 2;
			invoiceLine2.JI_RN_NKSecondCommercialInvoiceCountry = ZString.Empty;
			invoiceLine2.JI_COOSupportingDocType = "1";
			invoiceLine2.JI_SequenceNumber = 2;

			ImportDHRHeader header = new ImportDHRCreator().Create(entry);
			var stream = KRXmlObjectSerializer.Serialize(header);
			AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._DHR, stream);
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._DHR);
			invoiceLine1.JI_CustomsFifthQuantity = 999;
		}
		JobDeclaration declaration;
	}
}
