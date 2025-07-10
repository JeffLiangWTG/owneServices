using System.Linq;
using CargoWise.Customs.IN.MessageContracts.SeaCgm;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.MessageSending.SeaCgm.Testing;

[TestedType(typeof(SeaCgmCMCHI21DataProvider))]
sealed class ConsoligmDataProviderTest : SeaCgmIConsoligmDataProviderBase
{
	public override void TestCargos()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertNotNull(nameof(IConsoligmDataProvider.Cargos), dataProvider.Cargos);
			AssertEquals($"No bills, {nameof(IConsoligmDataProvider.Cargos)} Count", 0, dataProvider.Cargos.Count);

			var bill = header.Bills.AddNew();
			bill.ABL_BolType = ZString.Empty;
			dataProvider = CreateDataProvider();
			AssertNotNull(nameof(IConsoligmDataProvider.Cargos), dataProvider.Cargos);
			AssertEquals($"Cargo bill present, {nameof(IConsoligmDataProvider.Cargos)} Count", 1, dataProvider.Cargos.Count);
			AssertEquals($"{nameof(IConsoligmDataProvider.Cargos)} Type", "ConsolCargoDataProvider", dataProvider.Cargos.Single().GetType().Name);

			bill = header.Bills.AddNew();
			bill.ABL_BolType = "STD";
			dataProvider = CreateDataProvider();
			AssertNotNull(nameof(IConsoligmDataProvider.Cargos), dataProvider.Cargos);
			AssertEquals($"Cargo bill present, {nameof(IConsoligmDataProvider.Cargos)} Count", 2, dataProvider.Cargos.Count);
		});
	}

	public override void TestContainers()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertNotNull(nameof(IConsoligmDataProvider.Containers), dataProvider.Containers);
			AssertEquals($"No bills, {nameof(IConsoligmDataProvider.Containers)} Count", 0, dataProvider.Containers.Count);

			var bill = header.Bills.AddNew();
			bill.ABL_BolType = ZString.Empty;
			dataProvider = CreateDataProvider();
			AssertEquals($"No Standard bills, {nameof(IConsoligmDataProvider.Containers)} Count", 0, dataProvider.Containers.Count);

			bill.ABL_BolType = "STD";
			dataProvider = CreateDataProvider();
			AssertEquals($"Cargo bill without containers, {nameof(IConsoligmDataProvider.Containers)} Count", 0, dataProvider.Containers.Count);

			var container = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			bill.Packs.AddNew();
			pack.ContainerPK = container.PK;
			dataProvider = CreateDataProvider();
			AssertEquals($"Cargo bill with container, {nameof(IConsoligmDataProvider.Containers)} Count", 1, dataProvider.Containers.Count);
			AssertEquals($"{nameof(IConsoligmDataProvider.Containers)} Type", "ConsolContDataProvider", dataProvider.Containers.Single().GetType().Name);
		});
	}

	protected override IConsoligmDataProvider CreateDataProvider()
		=> SeaCgmCMCHI21DataProvider.CreateProvider(header, Mock.Of<ISeaCgmCMCHI21AdditionalDataProvider>()).Consoligm;
}
