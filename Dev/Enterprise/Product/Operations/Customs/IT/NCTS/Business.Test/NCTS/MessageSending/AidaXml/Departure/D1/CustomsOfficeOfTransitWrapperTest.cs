using System;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class CustomsOfficeOfTransitWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new CustomsOfficeOfTransitWrapper(null));
	}

	public void TestReferenceNumber()
	{
		var wrapper = GetWrapper();
		AssertNullOrEmpty(nameof(ICustomsOfficeOfTransit.ReferenceNumber), wrapper.ReferenceNumber);

		customsOffice.CY_Data = "IT2345";
		wrapper = GetWrapper();
		AssertEquals(nameof(ICustomsOfficeOfTransit.ReferenceNumber), "IT2345", wrapper.ReferenceNumber);
	}

	public void TestArrivalDateAndTimeEstimated()
	{
		var arrivalDateAndTimeEstimated = new DateTime(2023, 9, 21, 11, 09, 30);
		var wrapper = GetWrapper();
		AssertNull(nameof(ICustomsOfficeOfTransit.ArrivalDateAndTimeEstimated), wrapper.ArrivalDateAndTimeEstimated);

		customsOffice.CY_Date = arrivalDateAndTimeEstimated;
		wrapper = GetWrapper();
		AssertEquals(nameof(ICustomsOfficeOfTransit.ArrivalDateAndTimeEstimated), arrivalDateAndTimeEstimated, wrapper.ArrivalDateAndTimeEstimated);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.NewDepartureNctsHeader();
		customsOffice = header.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfTransit);
	}

	NctsEuOfficeCode customsOffice;

	ICustomsOfficeOfTransit GetWrapper() => new CustomsOfficeOfTransitWrapper(customsOffice);
}
