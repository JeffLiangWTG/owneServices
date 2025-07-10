using System;
using CargoWise.Types;
using Enterprise.Customs.Business.CustomsLists;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NC123DataProvider))]
sealed class NC123DataProviderTest : BasePassarMessageDataProviderTest<NC123DataProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Argument == null", () => new NC123DataProvider(null));
	}

	public void TestProperties() => CombineAssertions(() =>
	{
		Declaration.JE_LocationOfGoods = "LOC123";

		AssertEquals("ApprovedLocationIdentificationNumber", "LOC123", DataProvider.ApprovedLocationIdentificationNumber);
	});

	public void TestUnusedProperties() => CombineAssertions(() =>
	{
		AssertNull("TransitOperation", DataProvider.TransitOperation);
	});

	public void TestExportOperation() => CombineAssertions(() =>
	{
		AssertNotNull(DataProvider.ExportOperation);
		AssertSame("cached", DataProvider.ExportOperation, DataProvider.ExportOperation);
	});

	public void TestTraderAtDeparture() => CombineAssertions(() =>
	{
		AssertNotNull(DataProvider.TraderAtDeparture);
		AssertSame("cached", DataProvider.TraderAtDeparture, DataProvider.TraderAtDeparture);
	});

	public void TestModeOfTransport() => CombineAssertions(() =>
	{
		AssertNotNull(DataProvider.ModeOfTransport);
		AssertSame("cached", DataProvider.ModeOfTransport, DataProvider.ModeOfTransport);
	});

	public void TestTransportDataProvider() => CombineAssertions(() =>
	{
		Declaration.JE_TransportMode = TransportTypeGenericList.Codes.Road;
		AssertNotNull("TransportDataProvider is null", DataProvider.ModeOfTransport);

		ResetDataProvider();

		Declaration.JE_TransportMode = ZString.Empty;
		AssertNull("TransportDataProvider not null", DataProvider.ModeOfTransport);
	});

	protected override NC123DataProvider CreateDataProvider() => new NC123DataProvider(SendingObject);
}
