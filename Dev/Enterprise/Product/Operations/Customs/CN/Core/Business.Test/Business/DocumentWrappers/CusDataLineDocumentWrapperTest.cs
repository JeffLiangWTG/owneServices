using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CusDataLineDocumentWrapper))]
	class CusDataLineDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCustomsInvoiceDocument()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var invoiceLine1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var wrapper = new CusDataLineDocumentWrapper(entryLine);
			invoiceLine.JI_NetWeight = 1000;
			invoiceLine.JI_NetWeightUQ = "G";
			AssertEquals(1m, wrapper.NetWeightInKG);
			invoiceLine.JI_NetWeightUQ = "KG";
			AssertEquals(1000m, wrapper.NetWeightInKG);
			invoiceLine1.JI_NetWeight = 1000;
			invoiceLine1.JI_NetWeightUQ = "KG";
			AssertEquals(2000m, wrapper.NetWeightInKG);
			invoiceLine.JI_Weight = 2000;
			invoiceLine.JI_WeightUQ = "G";
			AssertEquals(2m, wrapper.GrossWeightInKG);
			invoiceLine.JI_WeightUQ = "KG";
			AssertEquals(2000m, wrapper.GrossWeightInKG);
			invoiceLine1.JI_Weight = 2000;
			invoiceLine1.JI_WeightUQ = "KG";
			AssertEquals(4000m, wrapper.GrossWeightInKG);
		}

		public void TestUNDGPackingGroupAndUNDGNumber()
		{
			var testBondage = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var invoiceLine1 = testBondage.InvoiceLine;
			var wrapper = new CusDataLineDocumentWrapper(testBondage.EntryLine);
			AssertEquals("UNDGPackingGroup should be empty without UNDGs", ZString.Empty, wrapper.UNDGPackingGroup);
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "0000";
			subs.DG_Variant = "A";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_FlashPoint = "-4 cc";
			subs.DG_PG = "";
			subs.DG_PSN = "I am very dangerous";
			subs.DG_Class = "8";
			var undg = invoiceLine1.UNDGs.AddNew();
			undg.DI_DG = subs.PK;
			undg.LinkDefault(subs);
			AssertEquals("0000", wrapper.UNDGNumber);
			AssertEquals("UNDGPackingGroup should be Unknown for UNDGSubstance DG_PG is empty.", Constants.CNCustomsUNDGPackingGroups.Unknown, wrapper.UNDGPackingGroup);
			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_UNNO = "0001";
			subs2.DG_Variant = "A";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs2.DG_FlashPoint = "-4 cc";
			subs2.DG_PG = "III";
			subs2.DG_PSN = "I am very dangerous";
			subs2.DG_Class = "8";
			undg.DI_DG = subs2.PK;
			undg.LinkDefault(subs2);
			AssertEquals("UNDGPackingGroup should be 3 for UNDGSubstance DG_PG is III.", Constants.CNCustomsUNDGPackingGroups.LowDanger, wrapper.UNDGPackingGroup);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var entryLine = Factory.NewWithValidTestData<CusEntryLine>();
			return new CusDataLineDocumentWrapper(entryLine);
		}

		protected override void SetUp()
		{
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.ActiveEntryHeaders.AddNew();
			entryLine = header.AllEntryLines.AddNew();
			Factory.Save();
		}
		JobDeclaration declaration;
		CusEntryHeader header;
		CusEntryLine entryLine;
	}
}
