using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class TransitHeaderCustomsOfficeOfTransitTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When customsOffice is null", () => new TransitHeaderCustomsOfficeOfTransit(null));
		AssertNoExceptionThrown(() => new TransitHeaderCustomsOfficeOfTransit(Factory.New<NctsEuOfficeCode>()));
	}

	public void TestProperties()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var customsOffice = nctsHeader.MovementHeader.CustomsOffices.AddNew();
		customsOffice.CY_Data = "123";
		customsOffice.CY_Date = new ZDateTime(2021, 1, 1);
		CombineAssertions(() =>
		{
			var wrapper = new TransitHeaderCustomsOfficeOfTransit(customsOffice);
			AssertEquals(nameof(wrapper.EstimatedArrivalTime), new ZDateTime(2021, 1, 1), wrapper.EstimatedArrivalTime);
			AssertEquals(nameof(wrapper.ReferenceNumber), "123", wrapper.ReferenceNumber);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		customsOffice = Factory.New<NctsEuOfficeCode>();
		customsOffice.Parent = header.MovementHeader;
	}
	NctsEuOfficeCode customsOffice;
}
