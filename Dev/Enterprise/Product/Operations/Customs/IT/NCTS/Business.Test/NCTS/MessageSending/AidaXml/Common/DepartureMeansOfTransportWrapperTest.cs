using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class DepartureMeansOfTransportWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertNull(
				DepartureMeansOfTransportWrapper
				.NewOrNull(
					typeOfIdentification: "",
					identificationNumber: "",
					nationality: ""));
			AssertNull(
				DepartureMeansOfTransportWrapper
				.NewOrNull(
					typeOfIdentification: " ",
					identificationNumber: " ",
					nationality: " "));
			AssertNull(
				DepartureMeansOfTransportWrapper
				.NewOrNull(
					typeOfIdentification: "A",
					identificationNumber: " ",
					nationality: " "));

			AssertNull(
				DepartureMeansOfTransportWrapper
				.NewOrNullForTrailer(
					identificationNumber: "",
					nationality: ""));
			AssertNull(
				DepartureMeansOfTransportWrapper
				.NewOrNullForTrailer(
					identificationNumber: " ",
					nationality: " "));
		});

		CombineAssertions(() =>
		{
			AssertNotNull(
				DepartureMeansOfTransportWrapper
				.NewOrNull(
					typeOfIdentification: "1",
					identificationNumber: "",
					nationality: ""));
			AssertNotNull(
				DepartureMeansOfTransportWrapper
				.NewOrNull(
					typeOfIdentification: "",
					identificationNumber: "1",
					nationality: ""));

			AssertNotNull(
				DepartureMeansOfTransportWrapper
				.NewOrNull(
					typeOfIdentification: "",
					identificationNumber: "",
					nationality: "1"));

			AssertNotNull(
				DepartureMeansOfTransportWrapper
				.NewOrNullForTrailer(
					identificationNumber: "1",
					nationality: " "));
			AssertNotNull(
				DepartureMeansOfTransportWrapper
				.NewOrNullForTrailer(
					identificationNumber: "",
					nationality: "1"));
		});
	}

	public void TestTypeOfIdentification()
	{
		var departureMeansOfTransport = DepartureMeansOfTransportWrapper.NewOrNull(
			typeOfIdentification: "10",
			identificationNumber: "",
			nationality: "");
		AssertEquals(nameof(IMeansOfTransport.TypeOfIdentification), 10, departureMeansOfTransport.TypeOfIdentification);

		departureMeansOfTransport = DepartureMeansOfTransportWrapper.NewOrNull(
			typeOfIdentification: "A",
			identificationNumber: "ID~",
			nationality: "");
		AssertEquals(nameof(IMeansOfTransport.TypeOfIdentification), -1, departureMeansOfTransport.TypeOfIdentification);
	}

	public void TestIdentificationNumber()
	{
		var departureMeansOfTransport = DepartureMeansOfTransportWrapper.NewOrNull(
			typeOfIdentification: "",
			identificationNumber: "     ID#  ",
			nationality: "");
		AssertEquals(nameof(IMeansOfTransport.IdentificationNumber), "ID#", departureMeansOfTransport.IdentificationNumber);
	}

	public void TestNationality()
	{
		var departureMeansOfTransport = DepartureMeansOfTransportWrapper.NewOrNull(
			typeOfIdentification: "",
			identificationNumber: "",
			nationality: "IT");
		AssertEquals(nameof(IMeansOfTransport.Nationality), "IT", departureMeansOfTransport.Nationality);
	}
}
