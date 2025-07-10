using System;
using CargoWise.Customs.IN.MessageContracts.ExportSb;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]
sealed class ExportSbCACHE01DataProviderTest : ExportSbExportSbCACHE01DataProviderAbstractClassBase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When messageSendingObject is null", () => ExportSbCACHE01DataProvider.CreateProvider(null, Mock.Of<IExportSbCACHE01AdditionalDataProvider>()));
		AssertExceptionThrown<ArgumentNullException>("When additional dataprovider is null", () => ExportSbCACHE01DataProvider.CreateProvider(header, null));
		AssertNoExceptionThrown("When messageSendingObject is not null", () => ExportSbCACHE01DataProvider.CreateProvider(header, Mock.Of<IExportSbCACHE01AdditionalDataProvider>()));
	}

	public override void TestFooter()
	{
		var dataProvider = CreateDataProvider();
		AssertNotNull(nameof(ExportSbCACHE01DataProviderAbstractClass.Footer), dataProvider.Footer);
		AssertEquals("Type", "FooterDataProvider", dataProvider.Footer?.GetType().Name);
	}

	public override void TestHeader()
	{
		var dataProvider = CreateDataProvider();
		AssertNotNull(nameof(ExportSbCACHE01DataProviderAbstractClass.Header), dataProvider.Header);
		AssertEquals("Type", "HeaderDataProvider", dataProvider.Header?.GetType().Name);
	}

	public override void TestSb()
	{
		var dataProvider = CreateDataProvider();
		AssertNotNull(nameof(ExportSbCACHE01DataProviderAbstractClass.Sb), dataProvider.Sb);
		AssertEquals("Type", "SbDataProvider", dataProvider.Sb?.GetType().Name);
	}

	protected override ExportSbCACHE01DataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbCACHE01DataProvider.CreateProvider(header, Mock.Of<IExportSbCACHE01AdditionalDataProvider>());
	}
}
