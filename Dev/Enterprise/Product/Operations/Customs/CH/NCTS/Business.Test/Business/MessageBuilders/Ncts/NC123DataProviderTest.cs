using System;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class NC123DataProviderTest : BaseNctsDepartureMessageDataProviderTest<NC123DataProvider, NctsHeaderDepartureMessageSendingObject>
{
	protected override NC123DataProvider CreateDataProvider() => new NC123DataProvider(MessageSendingObject);

	public void TestConstructorNullArgument() => AssertExceptionThrown<ArgumentNullException>("Argument == null", () => new NC123DataProvider(null));

	public void TestExportOperation()
	{
		MessageSendingObject.IsTransitOperation = true;
		AssertNull(DataProvider.ExportOperation);
		MessageSendingObject.IsTransitOperation = false;
		AssertNull(DataProvider.ExportOperation);
	}

	public void TestTransitOperation()
	{
		CombineAssertions(() =>
		{
			MessageSendingObject.IsTransitOperation = false;
			AssertNull(DataProvider.TransitOperation);
			MessageSendingObject.IsTransitOperation = true;
			AssertNotNull(DataProvider.TransitOperation);
			AssertType<BaseTransitOperationDataProvider>("type", DataProvider.TransitOperation);
			AssertSame("cached", DataProvider.TransitOperation, DataProvider.TransitOperation);
		});
	}

	public void TestTraderAtDeparture()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.TraderAtDeparture);
			AssertType<TraderDataProvider>("type", DataProvider.TraderAtDeparture);
			AssertSame("cached", DataProvider.TraderAtDeparture, DataProvider.TraderAtDeparture);
		});
	}

	public void TestApprovedLocationIdentificationNumber()
	{
		var validApprovedLocationIdentificationNumber = "12345";

		MessageSendingObject.NctsHeader.MovementHeader.GoodsLocation.Address.AuthorisationNumber = validApprovedLocationIdentificationNumber;
		AssertEquals("Approved location identification number", validApprovedLocationIdentificationNumber, DataProvider.ApprovedLocationIdentificationNumber);
	}

	public void TestModeOfTransport()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.ModeOfTransport);
			AssertType<ModeOfTransportDataProvider>("type", DataProvider.ModeOfTransport);
			AssertSame("cached", DataProvider.ModeOfTransport, DataProvider.ModeOfTransport);
		});
	}
}
