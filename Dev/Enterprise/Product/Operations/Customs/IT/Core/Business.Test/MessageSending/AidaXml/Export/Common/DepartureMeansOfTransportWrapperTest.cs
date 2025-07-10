using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class DepartureMeansOfTransportWrapperTest : TestCaseWithFactory
{
	public void TestNewOrNull()
	{
		CombineAssertions("When Identification Number is null or empty, NewOrNull", () =>
		{
			AssertNull(DepartureMeansOfTransportWrapper.NewOrNull(null, null, null));
			AssertNull(DepartureMeansOfTransportWrapper.NewOrNull("", null, null));
			AssertNull(DepartureMeansOfTransportWrapper.NewOrNull("", "1", "IT"));
			AssertNull(DepartureMeansOfTransportWrapper.NewOrNull("   ", "1", "IT"));
		});

		CombineAssertions("When Identification Number is valid, NewOrNull", () =>
		{
			AssertNotNull(DepartureMeansOfTransportWrapper.NewOrNull("A", null, null));
			AssertNotNull(DepartureMeansOfTransportWrapper.NewOrNull("A", "1", "US"));
		});
	}

	public void TestProperties()
	{
		IMeansOfTransport departureMeansOfTransport = DepartureMeansOfTransportWrapper.NewOrNull("ID", "10", "IT");
		CombineAssertions(() =>
		{
			AssertEquals(nameof(IMeansOfTransport.IdentificationNumber), "ID", departureMeansOfTransport.IdentificationNumber);
			AssertEquals(nameof(IMeansOfTransport.TypeOfIdentification), 10, departureMeansOfTransport.TypeOfIdentification);
			AssertEquals(nameof(IMeansOfTransport.Nationality), "IT", departureMeansOfTransport.Nationality);
		});

		departureMeansOfTransport = DepartureMeansOfTransportWrapper.NewOrNull("ID", "10A", null);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(IMeansOfTransport.IdentificationNumber), "ID", departureMeansOfTransport.IdentificationNumber);
			AssertEquals(nameof(IMeansOfTransport.TypeOfIdentification), -1, departureMeansOfTransport.TypeOfIdentification);
			AssertNull(nameof(IMeansOfTransport.Nationality), departureMeansOfTransport.Nationality);
		});
	}
}
