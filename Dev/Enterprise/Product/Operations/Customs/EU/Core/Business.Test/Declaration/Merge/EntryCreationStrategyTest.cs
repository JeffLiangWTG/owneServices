using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class EntryCreationStrategyTest : Customs.Business.Testing.EntryCreationStrategyTest
	{
		public void TestSupportingDocumentKeys()
		{
			var dec = GetJobDeclarationForTest();
			var cei = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var inv1 = dec.Invoices.AddNew();
			var invLine = inv1.JobComInvoiceLines.AddNew();
			invLine.JI_CEI = cei.PK;
			var mergeStrategy = dec.CreateEntryCreationStrategy();
			var supportingDocumentKeys = mergeStrategy.GetSupportingDocumentKeys();
			AssertArrayEqualsByElements(supportingDocumentKeys, GetExpectedSupportingDocumentKeys());
		}

		public void TestGetCusAuthorizationUsageKeys()
		{
			var dec = GetJobDeclarationForTest();
			var cei = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var inv1 = dec.Invoices.AddNew();
			var invLine = inv1.JobComInvoiceLines.AddNew();
			invLine.JI_CEI = cei.PK;
			var mergeStrategy = dec.CreateEntryCreationStrategy();
			var keys = mergeStrategy.GetCusAuthorizationUsageKeys();
			AssertArrayEqualsByElements(keys, GetExpectedCusAuthorizationUsageKeys());
		}

		public void TestGetCusSupplyChainActorReferenceKeys()
		{
			var dec = GetJobDeclarationForTest();
			var cei = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var inv1 = dec.Invoices.AddNew();
			var invLine = inv1.JobComInvoiceLines.AddNew();
			invLine.JI_CEI = cei.PK;
			var mergeStrategy = dec.CreateEntryCreationStrategy();
			var keys = mergeStrategy.GetCusSupplyChainActorReferenceKeys();
			AssertArrayEqualsByElements(keys, GetExpectedCusSupplyChainActorReferenceKeys());
		}

		public void TestGetFiscalReferenceKeys()
		{
			var declaration = GetJobDeclarationForTest();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var mergeStrategy = declaration.CreateEntryCreationStrategy();
			var keys = mergeStrategy.GetFiscalReferenceKeys();
			AssertArrayEqualsByElements(keys, GetExpectedFiscalReferenceKeys());
		}

		public void TestMergeKeyContainsCEI()
		{
			var dec = GetJobDeclarationForTest();
			var cei = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var inv1 = dec.Invoices.AddNew();
			var invLine = inv1.JobComInvoiceLines.AddNew();
			invLine.JI_CEI = cei.PK;
			var mergeStrategy = dec.CreateEntryCreationStrategy();
			Assert(mergeStrategy.GetKeyForLine(invLine).Contains(cei.PK));
		}

		public override void TestGetKeyForHeader()
		{
			var declaration = GetJobDeclarationForTest();
			var provider = declaration.CustomsEntryInstructionProvider;
			var entryInstr = provider.CustomsEntryInstructions.AddNew();
			var invHead1 = declaration.Invoices.AddNew();
			var invLine1 = invHead1.JobComInvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstr.PK;

			var lineMerger = new LineMerger(declaration);
			var entryCreationStrategy = new Customs.Business.EntryCreationStrategy(declaration);
			AssertEquals("Entry Instruction in header", false, provider.IsNoEntryInstruction);
			Assert("Entry Instruction pk", entryCreationStrategy.GetKeyForHeader(invLine1).Contains(entryInstr.PK));

			var entryManager = new EntryManager(declaration, entryCreationStrategy);
			var entryLine = entryManager.GetOrCreateEntryLine(invLine1);
			AssertEquals(entryInstr.PK, entryLine.Header.CH_CEI_Instruction);
		}

		public void TestMergeKeyForLineContainsJI_CustomsThirdUnitQty()
		{
			var declaration = GetJobDeclarationForTest();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CustomsThirdUnitQty = "KLT";
			var entryCreationStrategy = declaration.CreateEntryCreationStrategy();
			Assert(entryCreationStrategy.GetKeyForLine(invoiceLine).Contains(invoiceLine.JI_CustomsThirdUnitQty));
		}

		public void TestMergeKeyForLineContainsZG_CommercialReference()
		{
			var dec = GetJobDeclarationForTest();
			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invLine.ZG_CommercialReference = "Commercial Reference";
			var entryCreationStrategy = dec.CreateEntryCreationStrategy();
			Assert(entryCreationStrategy.GetKeyForLine(invLine).Contains(invLine.ZG_CommercialReference));
		}

		public void TestMergeKeyContainsTransactionNature()
		{
			var dec = GetJobDeclarationForTest();
			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invLine.ZG_TransNature = "22";
			var mergeStrategy = dec.CreateEntryCreationStrategy();
			Assert(mergeStrategy.GetKeyForLine(invLine).Contains(invLine.ZG_TransNature));
		}

		protected virtual JobDeclaration GetJobDeclarationForTest()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			return dec;
		}

		protected virtual string[] GetExpectedSupportingDocumentKeys()
		{
			return new string[]
			{
				SupportingDocument.Schema.CSI_DateOfIssue,
				SupportingDocument.Schema.CSI_SubType,
				SupportingDocument.Schema.CSI_Description,
				SupportingDocument.Schema.CSI_ReferenceNumber,
				SupportingDocument.Schema.CSI_Code,
				SupportingDocument.Schema.CSI_Status,
				SupportingDocument.Schema.CSI_UnitOfQuantity,
				SupportingDocument.Schema.CSI_UnitOfQuantity2,
				SupportingDocument.Schema.CSI_UnitOfQuantity3,
				SupportingDocument.Schema.CSI_RX_NKCurrency,
				SupportingDocument.Schema.CSI_RN_NKCountryCode,
				SupportingDocument.Schema.CSI_ReferenceNumber2,
				SupportingDocument.Schema.CSI_DateOfExpiry,
				SupportingDocument.Schema.CSI_Procedure,
				SupportingDocument.Schema.CSI_Tariff
			};
		}

		protected virtual string[] GetExpectedCusAuthorizationUsageKeys()
		{
			return new string[]
			{
				CusAuthorizationUsage.Schema.AGC_Code,
				CusAuthorizationUsage.Schema.AGC_Number,
				CusAuthorizationUsage.Schema.AGC_OH_Owner
			};
		}

		protected virtual string[] GetExpectedCusSupplyChainActorReferenceKeys()
		{
			return new string[]
			{
				CusSupplyChainActorReference.Schema.CFR_Code,
				CusSupplyChainActorReference.Schema.CFR_Reference,
				CusSupplyChainActorReference.Schema.CFR_OA_Owner
			};
		}

		protected virtual string[] GetExpectedFiscalReferenceKeys()
		{
			return new string[]
			{
				CusFiscalReference.Schema.CFR_Code,
				CusFiscalReference.Schema.CFR_Reference,
			};
		}
	}
}
