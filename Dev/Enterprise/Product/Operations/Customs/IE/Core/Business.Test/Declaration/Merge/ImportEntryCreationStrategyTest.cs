using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ImportEntryCreationStrategyTest : EU.Business.Declaration.Testing.EntryCreationStrategyTest
	{
		public override void TestGetKeyForHeader()
		{
			base.TestGetKeyForHeader();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_UCR = "UCR123";
			var invLine = invoice.JobComInvoiceLines.AddNew();
			var entryCreationStrategy = new ImportUCC5EntryCreationStrategy(declaration);
			var key = entryCreationStrategy.GetKeyForHeader(invLine);

			Assert("JZ_UCR", key.Contains(new ZString("UCR123")));
		}

		public void TestGetKeyForLineContainsJZ_ValuationCode()
		{
			var invoice = declaration.Invoices.AddNew();
			var invLine = invoice.JobComInvoiceLines.AddNew();
			invoice.JZ_ValuationCode = "10";
			var entryCreationStrategy = new ImportEntryCreationStrategy(declaration);
			Assert(entryCreationStrategy.GetKeyForLine(invLine).Contains(invoice.JZ_ValuationCode));
		}

		public void TestMergeKeyForInvoiceLinesWithAtLeastOneExciseTax()
		{
			var dec = GetJobDeclarationForTest();
			var inv = dec.Invoices.AddNew();
			var invLine1 = inv.JobComInvoiceLines.AddNew();
			inv.JobComInvoiceLines.AddNew();

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.ActiveEntryHeaders[0].MergedLines.Count);

			invLine1.CusLineTariffDetails.AddNew();
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, dec.ActiveEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergeKeyContainCusSupplyChainActors()
		{
			var org = Factory.New<OrgHeader>();
			var dec = GetJobDeclarationForTest();
			var cei = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var inv = dec.Invoices.AddNew();
			var invLine = inv.JobComInvoiceLines.AddNew();
			invLine.JI_CEI = cei.PK;
			var reference = invLine.CusSupplyChainActorReferences.AddNew();
			reference.CFR_Code = "FR1";
			reference.CFR_Reference = "REF1";
			reference.CFR_OA_Owner = org.MainAddress.PK;
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, true))
				{
					var mergeStrategy = dec.CreateEntryCreationStrategy();
					var mergeKey = mergeStrategy.GetKeyForLine(invLine);
					Assert("reference.CFR_Code", mergeKey.Contains(reference.CFR_Code));
					Assert("reference.CFR_Reference", mergeKey.Contains(reference.CFR_Reference));
					Assert("reference.CFR_OA_Owner", mergeKey.Contains(reference.CFR_OA_Owner));
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, false))
				{
					var mergeStrategy = dec.CreateEntryCreationStrategy();
					var mergeKey = mergeStrategy.GetKeyForLine(invLine);
					Assert("reference.CFR_Code", mergeKey.Contains(reference.CFR_Code));
					Assert("reference.CFR_Reference", mergeKey.Contains(reference.CFR_Reference));
					Assert("reference.CFR_OA_Owner", mergeKey.Contains(reference.CFR_OA_Owner));
				}
			});
		}

		public void TestMergeKeyContainFiscalReferences()
		{
			var dec = GetJobDeclarationForTest();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var fiscalReference = invoiceLine.FiscalReferences.AddNew();
			fiscalReference.CFR_Code = "FR1";
			fiscalReference.CFR_Reference = "REF1";

			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, true))
				{
					var mergeStrategy = declaration.CreateEntryCreationStrategy();
					var mergeKey = mergeStrategy.GetKeyForLine(invoiceLine);
					Assert("Contains FiscalReference.CFR_Code?", mergeKey.Contains(fiscalReference.CFR_Code));
					Assert("Contains FiscalReference.CFR_Reference?", mergeKey.Contains(fiscalReference.CFR_Reference));
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, false))
				{
					var mergeStrategy = declaration.CreateEntryCreationStrategy();
					var mergeKey = mergeStrategy.GetKeyForLine(invoiceLine);
					Assert("Contains FiscalReference.CFR_Code?", mergeKey.Contains(fiscalReference.CFR_Code));
					Assert("Contains FiscalReference.CFR_Reference?", mergeKey.Contains(fiscalReference.CFR_Reference));
				}
			});
		}

		protected override EU.Business.Declaration.JobDeclaration GetJobDeclarationForTest() => declaration;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
		}
		JobDeclaration declaration;
	}
}
