using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	using Enterprise.MasterFiles.Business;

	class C88CreationStrategyTest : TestCaseWithFactory
	{
		public void TestSupportingDocumentKeys()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MasterUCR = "HELLO";
			var inv1 = dec.Invoices.AddNew();
			var invLine1 = inv1.JobComInvoiceLines.AddNew();
			invLine1.JI_Tariff = "2203001010";
			var inv2 = dec.Invoices.AddNew();
			var invLine2 = inv2.JobComInvoiceLines.AddNew();
			invLine2.JI_Tariff = "2203001010";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			invLine1.JI_SupplementaryCode1 = "ABCD";
			invLine2.JI_SupplementaryCode1 = "ABCD";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			invLine2.JI_SupplementaryCode1 = "EFGH";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertEquals("Two entries should be created because they have different Supplementary Codes", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			invLine1.JI_SupplementaryCode1 = "ABCD";
			invLine2.JI_SupplementaryCode1 = "ABCD";
			invLine1.JI_SupplementaryCode2 = "EFGH";
			invLine2.JI_SupplementaryCode2 = "EFGH";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			invLine2.JI_SupplementaryCode2 = "IJKL";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertEquals("Two entries should be created because they have different Supplementary Codes", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			invLine2.JI_SupplementaryCode2 = "EFGH";
			invLine1.AdditionalSupplementaryCodes.AsString = "IJKL,MNOP";
			invLine2.AdditionalSupplementaryCodes.AsString = "IJKL,MNOP";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			invLine2.AdditionalSupplementaryCodes.AsString = "MNOP,IJKL";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertEquals("Two entries should be created because they have different Supplementary Codes", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);
		}

		public void TestSplitOnInvoiceHeadersCreatesTwoEntries()
		{
			var dec = Factory.New<JobDeclaration>();
			var inv1 = dec.Invoices.AddNew();
			var invLine1 = inv1.JobComInvoiceLines.AddNew();
			var inv2 = dec.Invoices.AddNew();
			var invLine2 = inv2.JobComInvoiceLines.AddNew();

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			inv1.ZG_HouseSplitReference = "01";
			inv2.ZG_HouseSplitReference = "01";
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			inv2.ZG_HouseSplitReference = "02";
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, dec.CustomsEntryHeaders.Count);
		}

		public void TestJI_OA_SupervisingOfficeOnInvoiceLineCreatesTwoEntries()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var inv1 = dec.Invoices.AddNew();
			var invLine1 = inv1.JobComInvoiceLines.AddNew();
			invLine1.JI_Tariff = "2203001010";
			var inv2 = dec.Invoices.AddNew();
			var invLine2 = inv2.JobComInvoiceLines.AddNew();
			invLine2.JI_Tariff = "2203001010";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			invLine1.JI_OA_SupervisingOffice = ZGuid.BrettsGuid;
			invLine2.JI_OA_SupervisingOffice = ZGuid.BrettsGuid;

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			invLine2.JI_OA_SupervisingOffice = ZGuid.NewZGuid();
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Two entries should be created because they have different Supervising Offices", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);
		}

		public void TestJZ_CustomsAuthorisationReferenceForExportFallbackOnInvoiceHeadersCreatesTwoEntries()
		{
			var dec = Factory.New<JobDeclaration>();
			var inv1 = dec.Invoices.AddNew();
			var invLine1 = inv1.JobComInvoiceLines.AddNew();
			var inv2 = dec.Invoices.AddNew();
			var invLine2 = inv2.JobComInvoiceLines.AddNew();

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			inv1.ZG_CustomsAuthorisationReferenceForExportFallback = "X";
			inv2.ZG_CustomsAuthorisationReferenceForExportFallback = "X";
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			inv2.ZG_CustomsAuthorisationReferenceForExportFallback = "Y";
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, dec.CustomsEntryHeaders.Count);
			inv2.ZG_CustomsAuthorisationReferenceForExportFallback = "X";
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
		}

		public void TestMergeKeyContainsCEI()
		{
			var dec = Factory.New<JobDeclaration>();
			var cei = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var inv1 = dec.Invoices.AddNew();
			var invLine = inv1.JobComInvoiceLines.AddNew();
			invLine.JI_CEI = cei.PK;
			var mergeStrategy = dec.CreateEntryCreationStrategy();
			Assert(mergeStrategy.GetKeyForLine(invLine).Contains(cei.PK));
		}

		public void TestMergeKeyContainsFiscalReferences()
		{
			var dec = Factory.New<JobDeclaration>();
			var cei = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var inv = dec.Invoices.AddNew();
			var invLine = inv.JobComInvoiceLines.AddNew();
			invLine.JI_CEI = cei.PK;
			var fiscalReference1 = invLine.FiscalReferences.AddNew();
			fiscalReference1.CFR_Code = EU.Business.FiscalReferenceCodeList.Codes.FR1_Importer;
			fiscalReference1.CFR_Reference = "FR1-2002";
			var fiscalReference2 = invLine.FiscalReferences.AddNew();
			fiscalReference2.CFR_Code = EU.Business.FiscalReferenceCodeList.Codes.FR1_Importer;
			fiscalReference2.CFR_Reference = "FR1-1001";
			var fiscalReference3 = invLine.FiscalReferences.AddNew();
			fiscalReference3.CFR_Code = EU.Business.FiscalReferenceCodeList.Codes.FR2_Customer;
			fiscalReference3.CFR_Reference = "fr2-abcd";

			var mergeStrategy = dec.CreateEntryCreationStrategy();
			var mergeKey = mergeStrategy.GetKeyForLine(invLine);

			Assert(mergeKey.Contains((ZString)"FR1-1001"));
			Assert(mergeKey.Contains((ZString)"FR1-2002"));
			Assert(mergeKey.Contains((ZString)"FR2-ABCD"));
			Assert(mergeKey.IndexOf((ZString)"FR1-1001") < mergeKey.IndexOf((ZString)"FR1-2002"));
			Assert(mergeKey.IndexOf((ZString)"FR1-2002") < mergeKey.IndexOf((ZString)"FR2-ABCD"));
		}
	}
}
