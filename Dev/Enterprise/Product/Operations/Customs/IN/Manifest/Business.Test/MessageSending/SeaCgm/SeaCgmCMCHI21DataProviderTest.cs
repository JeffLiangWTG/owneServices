using System;
using CargoWise.Customs.IN.MessageContracts.SeaCgm;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.MessageSending.SeaCgm.Testing;

[TestedType(typeof(SeaCgmCMCHI21DataProvider))]
sealed class SeaCgmCMCHI21DataProviderTest : SeaCgmISeaCgmCMCHI21DataProviderBase
{
	public override void TestConsoligm()
	{
		var dataProvider = CreateDataProvider();
		AssertNotNull(nameof(ISeaCgmCMCHI21DataProvider.Consoligm), dataProvider.Consoligm);
		AssertEquals("Type", "ConsoligmDataProvider", dataProvider.Consoligm?.GetType().Name);
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When messageSendingObject is null", () => SeaCgmCMCHI21DataProvider.CreateProvider(null, Mock.Of<ISeaCgmCMCHI21AdditionalDataProvider>()));
		AssertExceptionThrown<ArgumentNullException>("When additional dataprovider is null", () => SeaCgmCMCHI21DataProvider.CreateProvider(header, null));
		AssertNoExceptionThrown("When messageSendingObject and additional dataprovider is not null", () => SeaCgmCMCHI21DataProvider.CreateProvider(header, Mock.Of<ISeaCgmCMCHI21AdditionalDataProvider>()));
	}

	public override void TestFooter()
	{
		var dataProvider = CreateDataProvider();
		AssertNotNull(nameof(ISeaCgmCMCHI21DataProvider.Footer), dataProvider.Footer);
		AssertEquals("Type", "FooterDataProvider", dataProvider.Footer?.GetType().Name);
	}

	public override void TestHeader()
	{
		var dataProvider = CreateDataProvider();
		AssertNotNull(nameof(ISeaCgmCMCHI21DataProvider.Header), dataProvider.Header);
		AssertEquals("Type", "HeaderDataProvider", dataProvider.Header?.GetType().Name);
	}

	protected override ISeaCgmCMCHI21DataProvider CreateDataProvider()
		=> SeaCgmCMCHI21DataProvider.CreateProvider(header, Mock.Of<ISeaCgmCMCHI21AdditionalDataProvider>());
}
