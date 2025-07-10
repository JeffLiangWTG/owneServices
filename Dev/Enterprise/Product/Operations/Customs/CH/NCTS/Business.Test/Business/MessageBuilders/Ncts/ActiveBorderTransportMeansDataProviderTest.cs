using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

internal class ActiveBorderTransportMeansDataProviderTest : TestCaseWithFactory
{
	public void TestNewCollection()
	{
		CombineAssertions(() =>
		{
			AssertNull("null", ActiveBorderTransportMeansDataProvider.NewCollection(null));

			var nctsHeader = Factory.New<NctsHeader>();
			AssertNull("MovementHeader==null", ActiveBorderTransportMeansDataProvider.NewCollection(nctsHeader.MovementHeader));

			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			AssertNull("MovementHeader!=null, BM_ExportTransportMode is empty", ActiveBorderTransportMeansDataProvider.NewCollection(nctsHeader.MovementHeader));

			nctsHeader.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			AssertNotNull("MovementHeader != null, BM_ExportTransportMode not empty", ActiveBorderTransportMeansDataProvider.NewCollection(nctsHeader.MovementHeader));
		});
	}

	public void TestProvider()
	{
		const string activeBorderIdentificationType = "1";
		const string identificationNumber = "ABCD";
		const string nationality = Core.Constants.CountryCodes.Switzerland;
		const string conveyanceNumber = "456";
		const string customsOfficeAtBorder = "CH123";

		MovementHeader.BM_ActiveBorderIdentificationType = activeBorderIdentificationType;
		MovementHeader.BM_TOLCarrierID = identificationNumber;
		MovementHeader.BM_RN_NKTOLCarrierNationality = nationality;
		MovementHeader.BM_ConveyanceNumber = conveyanceNumber;
		MovementHeader.BM_CustomsOfficeAtBorder = customsOfficeAtBorder;

		var cusTransportMeans = MovementHeader.AdditionalTransportAtBorderList.AddNew();
		var tpmPrefix = "TPM_";
		cusTransportMeans.TPM_TypeOfIdentification = activeBorderIdentificationType;
		cusTransportMeans.TPM_ReferenceNumber = tpmPrefix + conveyanceNumber;
		cusTransportMeans.TPM_CustomsOffice = tpmPrefix + customsOfficeAtBorder;
		cusTransportMeans.TPM_RN_NKTransportNationality = nationality;
		cusTransportMeans.TPM_IdentificationNumber = tpmPrefix + identificationNumber;

		var bmActiveBorderTransportMeans = ActiveBorderTransportMeans.ElementAt(0);
		CombineAssertions("MovementHeader", () =>
		{
			AssertEquals("Sequence Number", 1, bmActiveBorderTransportMeans.SequenceNumber);
			AssertEquals("Type Of Identification", activeBorderIdentificationType, bmActiveBorderTransportMeans.TypeOfIdentification);
			AssertEquals("Identification Number", identificationNumber, bmActiveBorderTransportMeans.IdentificationNumber);
			AssertEquals("Nationality", nationality, bmActiveBorderTransportMeans.Nationality);
			AssertEquals("conveyanceNumber", conveyanceNumber, bmActiveBorderTransportMeans.ConveyanceReferenceNumber);
			AssertEquals("customsOfficeAtBorder", customsOfficeAtBorder, bmActiveBorderTransportMeans.CustomsOfficeAtBorderReferenceNumber);
		});

		var tpmActiveBorderTransportMeans = ActiveBorderTransportMeans.ElementAt(1);
		CombineAssertions("MovementHeader.AdditionalTransportAtBorderList", () =>
			{
			AssertEquals("Sequence Number", 2, tpmActiveBorderTransportMeans.SequenceNumber);
			AssertEquals("Type Of Identification", activeBorderIdentificationType, tpmActiveBorderTransportMeans.TypeOfIdentification);
			AssertEquals("Identification Number", tpmPrefix + identificationNumber, tpmActiveBorderTransportMeans.IdentificationNumber);
			AssertEquals("Nationality", nationality, tpmActiveBorderTransportMeans.Nationality);
			AssertEquals("conveyanceNumber", tpmPrefix + conveyanceNumber, tpmActiveBorderTransportMeans.ConveyanceReferenceNumber);
			AssertEquals("customsOfficeAtBorder", tpmPrefix + customsOfficeAtBorder, tpmActiveBorderTransportMeans.CustomsOfficeAtBorderReferenceNumber);
		});
	}

	public void TestSequenceNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Number Of Rows", 1, ActiveBorderTransportMeans.Count);
			AssertEquals("Sequence Nr Of Row 1", 1, ActiveBorderTransportMeans.ElementAt(0).SequenceNumber);
		});
	}

	public void TestTypeOfIdentification()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Type Of Identification 0 if BM_ActiveBorderIdentificationType empty", null, ActiveBorderTransportMeans.ElementAt(0).TypeOfIdentification);

			MovementHeader.BM_ActiveBorderIdentificationType = "1";
			var activeBorderTransportMeans = ActiveBorderTransportMeansDataProvider.NewCollection(MovementHeader).ElementAt(0);
			AssertEquals("Type Of Identification int 1 if BM_ActiveBorderIdentificationType 1", "1", activeBorderTransportMeans.TypeOfIdentification);
		});
	}

	public void TestIdentificationNumber()
	{
		const string identificationNumber = "ABCD";

		CombineAssertions(() =>
		{
			AssertNull("Identification Number empty", ActiveBorderTransportMeans.ElementAt(0).IdentificationNumber);

			MovementHeader.BM_TOLCarrierID = identificationNumber;
			var activeBorderTransportMeans = ActiveBorderTransportMeansDataProvider.NewCollection(MovementHeader).ElementAt(0);
			AssertEquals("Identification Number", identificationNumber, activeBorderTransportMeans.IdentificationNumber);
		});
	}

	public void TestNationality()
	{
		CombineAssertions(() =>
		{
			AssertNull("Nationality empty", ActiveBorderTransportMeans.ElementAt(0).Nationality);

			MovementHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.Switzerland;
			var activeBorderTransportMeans = ActiveBorderTransportMeansDataProvider.NewCollection(MovementHeader).ElementAt(0);
			AssertEquals("Nationality", Core.Constants.CountryCodes.Switzerland, activeBorderTransportMeans.Nationality);
		});
	}

	public void TestConveyanceReferenceNumber()
	{
		CombineAssertions(() =>
		{
			AssertNull("empty", ActiveBorderTransportMeans.ElementAt(0).ConveyanceReferenceNumber);

			MovementHeader.BM_ConveyanceNumber = "246";
			var activeBorderTransportMeans = ActiveBorderTransportMeansDataProvider.NewCollection(MovementHeader).ElementAt(0);
			AssertEquals("not empty", "246", activeBorderTransportMeans.ConveyanceReferenceNumber);
		});
	}

	NctsDepartureMovementHeader CreateNctsMovementHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		return nctsHeader.MovementHeader;
	}

	NctsDepartureMovementHeader MovementHeader => movementHeader ?? (movementHeader = CreateNctsMovementHeader());
	NctsDepartureMovementHeader movementHeader;

	public IReadOnlyCollection<IActiveBorderTransportMeans> ActiveBorderTransportMeans => activeBorderTransportMeans ?? (activeBorderTransportMeans = ActiveBorderTransportMeansDataProvider.NewCollection(MovementHeader).ToArray());
	IReadOnlyCollection<IActiveBorderTransportMeans> activeBorderTransportMeans;
}
