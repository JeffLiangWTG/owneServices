using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(DangerousGoodsDataProvider))]
sealed class DangerousGoodsDataProviderTest : BaseDepartureDataProviderTest<DangerousGoodsDataProvider, NctsHeaderDepartureMessageSendingObject>
{
	public void TestNewCollection() => CombineAssertions(() =>
	{
		AssertEquals("When argument null", 0, DangerousGoodsDataProvider.NewCollection(null).Count());

		AssertEquals("When collection empty", 0, DangerousGoodsDataProvider.NewCollection(DepartureGoodsItem.UNDGs).Count());

		var undg = DepartureGoodsItem.UNDGs.AddNew();
		AssertEquals("When collection has empty element", 0, DangerousGoodsDataProvider.NewCollection(DepartureGoodsItem.UNDGs).Count());

		undg.DI_DG = Factory.New<UNDGSubstance>().PK;
		AssertEquals("When collection has element with empty DG_UNNO", 0, DangerousGoodsDataProvider.NewCollection(DepartureGoodsItem.UNDGs).Count());
	});

	public void TestSequenceNumber() => CombineAssertions(() =>
	{
		AddDangerousGood("0004", "a");
		AddDangerousGood("0005");

		var dataProviders = DangerousGoodsDataProvider.NewCollection(DepartureGoodsItem.UNDGs).ToArray();

		AssertEquals("SequenceNumber 1", 1, dataProviders.Where(d => d.UNNumber == "0004").FirstOrDefault()?.SequenceNumber);
		AssertEquals("SequenceNumber 2", 2, dataProviders.Where(d => d.UNNumber == "0005").FirstOrDefault()?.SequenceNumber);
	});

	public void TestUNNumber() => CombineAssertions(() =>
	{
		AddUNDGSubstance("3", "c");
		Factory.Save();

		AddDangerousGood("3", "c");
		AssertEquals("0003", DataProvider.UNNumber);
		ResetDataProviderAndDeleteList();

		AddDangerousGood("0004", "a");
		AssertEquals("0004", DataProvider.UNNumber);
		ResetDataProviderAndDeleteList();

		void AddUNDGSubstance(string unNo, string variant = "", string standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO)
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = unNo;
			substance.DG_Variant = variant;
			substance.DG_Standard = standard;
		}

		void ResetDataProviderAndDeleteList()
		{
			ResetDataProvider();
			DepartureGoodsItem.UNDGs.DeleteAll();
		}
	});

	void AddDangerousGood(string unNo, string variant = "") => DepartureGoodsItem.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, unNo, variant, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO).First().PK;

	protected override DangerousGoodsDataProvider CreateDataProvider() => DangerousGoodsDataProvider.NewCollection(DepartureGoodsItem.UNDGs).First();
}
