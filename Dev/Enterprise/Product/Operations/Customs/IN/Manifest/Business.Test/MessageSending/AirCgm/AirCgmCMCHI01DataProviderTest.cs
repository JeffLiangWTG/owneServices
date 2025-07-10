using System;
using CargoWise.Customs.IN.MessageContracts.AirCgm;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.MessageSending.AirCgm.Testing;

[TestedType(typeof(AirCgmCMCHI01DataProvider))]
sealed class AirCgmCMCHI01DataProviderTest : AirCgmIAirCgmCMCHI01DataProviderBase
{
	public override void TestConsoligm()
	{
		var dataProvider = CreateDataProvider();
		AssertNotNull(nameof(IAirCgmCMCHI01DataProvider.Consoligm), dataProvider.Consoligm);
		AssertEquals("Type", "ConsoligmDataProvider", dataProvider.Consoligm?.GetType().Name);
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When messageSendingObject is null", () => AirCgmCMCHI01DataProvider.CreateProvider(null, Mock.Of<IAirCgmCMCHI01AdditionalDataProvider>()));
		AssertExceptionThrown<ArgumentNullException>("When additional dataprovider is null", () => AirCgmCMCHI01DataProvider.CreateProvider(header, null));
		AssertNoExceptionThrown("When messageSendingObject is not null", () => AirCgmCMCHI01DataProvider.CreateProvider(header, Mock.Of<IAirCgmCMCHI01AdditionalDataProvider>()));
	}

	public override void TestFooter()
	{
		var dataProvider = CreateDataProvider();
		AssertNotNull(nameof(IAirCgmCMCHI01DataProvider.Footer), dataProvider.Footer);
		AssertEquals("Type", "FooterDataProvider", dataProvider.Footer?.GetType().Name);
	}

	public override void TestHeader()
	{
		var dataProvider = CreateDataProvider();
		AssertNotNull(nameof(IAirCgmCMCHI01DataProvider.Header), dataProvider.Header);
		AssertEquals("Type", "HeaderDataProvider", dataProvider.Header?.GetType().Name);
	}

	protected override IAirCgmCMCHI01DataProvider CreateDataProvider()
	{
		return AirCgmCMCHI01DataProvider.CreateProvider(header, Mock.Of<IAirCgmCMCHI01AdditionalDataProvider>());
	}
}
