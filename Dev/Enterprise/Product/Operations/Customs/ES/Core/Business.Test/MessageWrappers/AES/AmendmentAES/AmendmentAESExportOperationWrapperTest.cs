using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AmendmentAESExportOperationWrapperTest : WrapperHelperTest<AmendmentAESExportOperationWrapper>
	{
		public void TestMRN()
		{
			AssertEquals("Expected filled MRN", "MRNNumber", wrapper.MRN);
		}

		public void TestDeclarationSubType()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled DeclarationSubType A when entry status not PDA and subStyle declared A", "A", wrapper.DeclarationSubType);

				entryHeader.CH_EntryStatus = "PDA";
				AssertEquals("Expected filled DeclarationSubType D when entry status PDA and subStyle declared A", "D", wrapper.DeclarationSubType);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
				AssertEquals("Expected filled DeclarationSubType E when entry status PDA and subStyle declared B", "E", wrapper.DeclarationSubType);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
				AssertEquals("Expected filled DeclarationSubType F when entry status PDA and subStyle declared C", "F", wrapper.DeclarationSubType);

				entryHeader.CH_EntryStatus = "CLR";
				AssertEquals("Expected filled DeclarationSubType C when entry status not PDA and subStyle declared C", "C", wrapper.DeclarationSubType);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.MovementReferenceNumber = "MRNNumber";

			wrapper = new AmendmentAESExportOperationWrapper(entryHeader, "2");
		}
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		AmendmentAESExportOperationWrapper wrapper;

		protected override AmendmentAESExportOperationWrapper GetProvider() => wrapper;
	}
}
