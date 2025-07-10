using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DangerousGoodsDataProvider))]
sealed class DangerousGoodsDataProviderTest : BasePassarDataProviderTest<DangerousGoodsDataProvider>
{
	public void TestNewCollection() => CombineAssertions(() =>
	{
		var invoiceLine1 = InvoiceLine;
		var invoiceLine2 = CreateInvoiceLine(invoiceLine1.InvoiceHeader);
		var invoiceLine3 = CreateInvoiceLine(invoiceLine1.InvoiceHeader);
		var invoiceLine4 = CreateInvoiceLine(invoiceLine1.InvoiceHeader);

		AddUNDGCodes(invoiceLine1, "H100");
		AddUNDGCodes(invoiceLine2, "H200");
		AddUNDGCodes(invoiceLine3, "H200");
		AddUNDGCodes(invoiceLine4, "H200", "H300");

		Declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
		Declaration.DoMergeForTesting();
		AssertEquals("Pre-condition: All InvoiceLines merged into single EntryLine", 4, EntryLine.InvoiceLines.Count);

		using (FuncsTestHelper.TemporarilySetFunctionality(FunctionalityTypes.CHNE015V3, true))
		{
			AssertEquals(AssertionInfo("Null arg"), 0, DangerousGoodsDataProvider.NewCollection(null)?.Count());
			var dataProviders = DangerousGoodsDataProvider.NewCollection(EntryLine).ToArray();
			AssertEquals(AssertionInfo("Single provider"), 1, dataProviders.Length);
			AssertNotNullOrEmpty(AssertionInfo("First UNNumber"), dataProviders.FirstOrDefault()?.UNNumber);
		}

		using (FuncsTestHelper.TemporarilySetFunctionality(FunctionalityTypes.CHNE015V3, false))
		{
			AssertEquals(AssertionInfo("Null arg"), 0, DangerousGoodsDataProvider.NewCollection(null)?.Count());
			var dataProviders = DangerousGoodsDataProvider.NewCollection(EntryLine).ToArray();
			AssertContainsExactElementsInAnyOrder(AssertionInfo("UNNumber's"), new[] { "H100", "H200", "H300" }, dataProviders.Select(x => x.UNNumber));
			AssertContainsExactElementsInAnyOrder(AssertionInfo("SequenceNumber's"), new[] { 1, 2, 3 }, dataProviders.Select(x => x.SequenceNumber));
		}

		string AssertionInfo(string info) => $"CHNE015V3={FuncsHelper.IsCHNE015V3Active} {info}";

		void AddUNDGCodes(JobComInvoiceLine invoiceLine, params string[] undgCodes)
		{
			foreach (var undgCode in undgCodes)
			{
				var undgSubstance = Factory.LoadTop1<UNDGSubstance>(new ZQuery(UNDGSubstanceSchema.DG_UNNO, undgCode));
				if (undgSubstance == null)
				{
					undgSubstance = Factory.New<UNDGSubstance>();
					undgSubstance.DG_UNNO = undgCode;
				}
				invoiceLine.UNDGs.AddNew().DI_DG = undgSubstance.PK;
			}
		}
	});

	public void TestProperties() => CombineAssertions(() =>
	{
		var undgSubstance = Factory.New<UNDGSubstance>();
		undgSubstance.DG_UNNO = "1234";
		var undgDataItem = EntryLine.RandomLine.UNDGs.AddNew();
		undgDataItem.DI_DG = undgSubstance.PK;

		AssertEquals("SequenceNumber", 1, DataProvider.SequenceNumber);
		AssertEquals("UNNumber", "1234", DataProvider.UNNumber);
	});

	protected override DangerousGoodsDataProvider CreateDataProvider() => DangerousGoodsDataProvider.NewCollection(EntryLine).First();
}
