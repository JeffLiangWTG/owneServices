using System.Linq;
using CargoWise.Customs.IN.MessageContracts.AirCgm;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.MessageSending.AirCgm.Testing;

[TestedType(typeof(AirCgmCMCHI01DataProvider))]
sealed class ConsoligmDataProviderTest : AirCgmIConsoligmDataProviderBase
{
	public override void TestHouseItems()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertNotNull(nameof(IConsoligmDataProvider.HouseItems), dataProvider.HouseItems);
			AssertEquals($"{nameof(IConsoligmDataProvider.HouseItems)} Count", 0, dataProvider.HouseItems.Count);

			var bill = header.Bills.AddNew();
			bill.ABL_BolType = ZString.Empty;
			dataProvider = CreateDataProvider();
			AssertNotNull(nameof(IConsoligmDataProvider.HouseItems), dataProvider.HouseItems);
			AssertEquals($"{nameof(IConsoligmDataProvider.HouseItems)} Count", 1, dataProvider.HouseItems.Count);
			AssertEquals($"{nameof(IConsoligmDataProvider.HouseItems)} Type", "ConsolHouseDataProvider", dataProvider.HouseItems.Single().GetType().Name);

			bill = header.Bills.AddNew();
			bill.ABL_BolType = "STD";
			dataProvider = CreateDataProvider();
			AssertNotNull(nameof(IConsoligmDataProvider.HouseItems), dataProvider.HouseItems);
			AssertEquals($"{nameof(IConsoligmDataProvider.HouseItems)} Count", 2, dataProvider.HouseItems.Count);
		});
	}

	public override void TestMaster()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertNotNull(nameof(IConsoligmDataProvider.Master), dataProvider.Master);
			AssertEquals($"{nameof(IConsoligmDataProvider.Master)} Type", "ConsolMasterDataProvider", dataProvider.Master.GetType().Name);
		});
	}

	protected override IConsoligmDataProvider CreateDataProvider()
		=> AirCgmCMCHI01DataProvider.CreateProvider(header, Mock.Of<IAirCgmCMCHI01AdditionalDataProvider>()).Consoligm;
}
