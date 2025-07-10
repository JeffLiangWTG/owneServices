using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ExpeditionAmendmentHeaderWrapperTest : ExpeditionHeaderWrapperTest
	{
		public void ExpeditionAmendmentHeaderWrapperConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => GetWrapper(null));
		}

		public void TestExpeditionT2LReference()
		{
			entryHeader.MovementReferenceNumber = "Test";
			AssertEquals("Expected filled ExpeditionT2LReference", "Test", wrapper.ExpeditionT2LReference);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = (ExpeditionAmendmentHeaderWrapper)GetWrapper(entryHeader);
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		ExpeditionAmendmentHeaderWrapper wrapper;
		protected override ExpeditionHeaderWrapper GetWrapper(CusEntryHeader cusEntryHeader) => new ExpeditionAmendmentHeaderWrapper(cusEntryHeader);

		protected override ExpeditionHeaderWrapper GetProvider() => wrapper;
	}
}
