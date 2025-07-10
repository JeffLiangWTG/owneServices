using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class ActiveBorderMeansOfTransportWrapperTest : TestCaseWithFactory
{
	public void TestNewOrNull()
	{
		CombineAssertions("When Identification Number is null or empty, NewOrNull", () =>
		{
			AssertNull(ActiveBorderMeansOfTransportWrapper.NewOrNull(Factory.New<NctsDepartureMovementHeader>()));
			AssertNull("Movement Header with white spaces", ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateMovementHeader(" ", " ", " ", " ", " ")));

			AssertNull(ActiveBorderMeansOfTransportWrapper.NewOrNull(Factory.New<DepartureCusTransportMeans>()));
			AssertNull("Transport Means with white spaces", ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateDepartureCusTransportMeans("   ", "   ", "   ", "   ", "   ")));
		});

		CombineAssertions("When Identification Number is valid, NewOrNull", () =>
		{
			AssertNotNull("Movement Header", ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateMovementHeader("1   ", "   ", "   ", "   ", "   ")));
			AssertNotNull("Movement Header", ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateMovementHeader("   ", "1   ", "   ", "   ", "   ")));
			AssertNotNull("Movement Header", ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateMovementHeader("   ", "   ", "1   ", "   ", "   ")));
			AssertNotNull("Movement Header", ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateMovementHeader("   ", "   ", "   ", "1   ", "   ")));
			AssertNotNull("Movement Header", ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateMovementHeader("   ", "   ", "   ", "   ", "1   ")));

			AssertNotNull("Transport Means", ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateDepartureCusTransportMeans("1   ", "   ", "   ", "   ", "   ")));
			AssertNotNull("Transport Means", ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateDepartureCusTransportMeans("   ", "1   ", "   ", "   ", "   ")));
			AssertNotNull("Transport Means", ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateDepartureCusTransportMeans("   ", "   ", "1   ", "   ", "   ")));
			AssertNotNull("Transport Means", ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateDepartureCusTransportMeans("   ", "   ", "   ", "1   ", "   ")));
			AssertNotNull("Transport Means", ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateDepartureCusTransportMeans("   ", "   ", "   ", "   ", "1   ")));
		});
	}

	public void TestCustomsOfficeAtBorder()
	{
		var wrapper = ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateMovementHeader("IT444444", null, null, null, null));
		AssertEquals("IT444444", wrapper.CustomsOfficeAtBorder);

		wrapper = ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateDepartureCusTransportMeans("IT555555", null, null, null, null));
		AssertEquals("IT555555", wrapper.CustomsOfficeAtBorder);
	}

	public void TestConveyanceReferenceNumber()
	{
		var wrapper = ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateMovementHeader(null, "Conveyance1", null, null, null));
		AssertEquals("Conveyance1", wrapper.ConveyanceReferenceNumber);

		wrapper = ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateDepartureCusTransportMeans(null, "Conveyance2", null, null, null));
		AssertEquals("Conveyance2", wrapper.ConveyanceReferenceNumber);
	}

	public void TestTypeOfIdentification()
	{
		var wrapper = ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateMovementHeader(null, null, NctsTransportTypeOfIdList.Codes._21, null, null));
		AssertEquals(21, wrapper.TypeOfIdentification);

		wrapper = ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateDepartureCusTransportMeans(null, null, NctsTransportTypeOfIdList.Codes._31, null, null));
		AssertEquals(31, wrapper.TypeOfIdentification);
	}

	public void TestIdentificationNumber()
	{
		var wrapper = ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateMovementHeader(null, null, null, "TRANSID1", null));
		AssertEquals("TRANSID1", wrapper.IdentificationNumber);

		wrapper = ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateDepartureCusTransportMeans(null, null, null, "TRANSID2", null));
		AssertEquals("TRANSID2", wrapper.IdentificationNumber);
	}
	public void TestNationality()
	{
		var wrapper = ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateMovementHeader(null, null, null, null, "IT"));
		AssertEquals("IT", wrapper.Nationality);

		wrapper = ActiveBorderMeansOfTransportWrapper.NewOrNull(CreateDepartureCusTransportMeans(null, null, null, null, "IL"));
		AssertEquals("IL", wrapper.Nationality);
	}
	NctsDepartureMovementHeader CreateMovementHeader(string customsOfficeAtBorder, string conveyanceReferenceNumber, string typeOfIdentification, string identificationNumber, string nationality)
	{
		var movementHeader = Factory.NewDepartureNctsHeader().MovementHeader;
		movementHeader.BM_CustomsOfficeAtBorder = customsOfficeAtBorder;
		movementHeader.BM_ConveyanceNumber = conveyanceReferenceNumber;
		movementHeader.BM_ActiveBorderIdentificationType = typeOfIdentification;
		movementHeader.BM_TOLCarrierID = identificationNumber;
		movementHeader.BM_RN_NKTOLCarrierNationality = nationality;
		return movementHeader;
	}

	DepartureCusTransportMeans CreateDepartureCusTransportMeans(string customsOfficeAtBorder, string conveyanceReferenceNumber, string typeOfIdentification, string identificationNumber, string nationality)
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var departureCusTransportMeans = nctsHeader.MovementHeader.AdditionalTransportAtBorderList.AddNew();
		departureCusTransportMeans.TPM_CustomsOffice = customsOfficeAtBorder;
		departureCusTransportMeans.TPM_ReferenceNumber = conveyanceReferenceNumber;
		departureCusTransportMeans.TPM_TypeOfIdentification = typeOfIdentification;
		departureCusTransportMeans.TPM_IdentificationNumber = identificationNumber;
		departureCusTransportMeans.TPM_RN_NKTransportNationality = nationality;
		return departureCusTransportMeans;
	}
}
