using System;
using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using JobDeclaration = Enterprise.Customs.BE.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class ConsignmentProviderTest : Customs.Business.Testing.DataProviderTestCase<ConsignmentProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new ConsignmentProvider(null, null, null));
	}

	public void TestContainerIndicator_0()
	{
		var provider = GetProvider();
		declaration.JE_ContainerMode = Core.Constants.ContainerModes.Liquid;
		AssertEquals(false, provider.ContainerIndicator);
	}

	public void TestContainerIndicator_1()
	{
		var provider = GetProvider();
		declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
		AssertEquals(true, provider.ContainerIndicator);
	}

	public void TestInlandModeOfTransport()
	{
		AssertEquals("1", provider.InlandModeOfTransport);
	}

	public void TestTransportEquipments()
	{
		AssertEquals(1, provider.TransportEquipments.Count);
	}

	public void TestLocationOfGoods()
	{
		AssertNotNull(provider.LocationOfGoods);
	}

	public void TestDepartureTransportMeans()
	{
		AssertEquals(1, provider.DepartureTransportMeans.Count);
	}

	public void TestDepartureTransportMeans_Road()
	{
		declaration.JE_TransportModeInland = "ROA";
		declaration.JE_TransportMeans = "30";
		declaration.JE_TransportIDInland = "1ABC123";
		declaration.JE_RN_NKTransportNationalityInland = "BE";
		declaration.JE_Trailer1RegNo = "2DEF456";
		declaration.JE_RN_NKTrailer1Nationality = "NL";
		declaration.JE_Trailer2RegNo = "3GHI789";
		declaration.JE_RN_NKTrailer2Nationality = "FR";

		CombineAssertions(() =>
		{
			AssertEquals(3, provider.DepartureTransportMeans.Count);
			AssertEquals("ID of first DepartureTransportMean", "1ABC123", provider.DepartureTransportMeans.First().IdentificationNumber);
			AssertEquals("ID of second DepartureTransportMean", "2DEF456", provider.DepartureTransportMeans.Skip(1).First().IdentificationNumber);
			AssertEquals("ID of third DepartureTransportMean", "3GHI789", provider.DepartureTransportMeans.Skip(2).First().IdentificationNumber);
		});
	}

	public void TestActiveBorderTransportMeans()
	{
		CombineAssertions(() =>
		{
			AssertNull("Empty Active Border Transport Means", provider.ActiveBorderTransportMeans);

			declaration.ZG_BorderTransportMeans = "10";
			provider = new ConsignmentProvider(declaration, entryInstruction, entryHeader);
			AssertNotNull("Filled Active Border Transport Means", provider.ActiveBorderTransportMeans);
		});
	}

	public void TestCarrierIdentificationNumber_EORI()
	{
		var shippingLine = Factory.New<OrgHeader>();
		var carrierCusCode = shippingLine.CustomsCodes.AddNew();
		carrierCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		carrierCusCode.OK_CustomsRegNo = "123";
		carrierCusCode.OK_RN_NKCodeCountry = "BE";

		declaration.CarrierEUBorderDocAddress.OrganisationPK = shippingLine.PK;

		AssertEquals("CarrierIdentificationNumber in case of EORI", "BE123", GetProvider().CarrierIdentificationNumber);
	}

	public void TestCarrierIdentificationNumber_TCU()
	{
		var shippingLine = Factory.New<OrgHeader>();
		var carrierCusCode = shippingLine.CustomsCodes.AddNew();
		carrierCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator;
		carrierCusCode.OK_CustomsRegNo = "123";
		carrierCusCode.OK_RN_NKCodeCountry = "BE";

		var declarantCusCode = shippingLine.CustomsCodes.AddNew();
		declarantCusCode.OK_CodeType = OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU;
		declarantCusCode.OK_CustomsRegNo = "789";
		declarantCusCode.OK_RN_NKCodeCountry = "BE";

		declaration.CarrierEUBorderDocAddress.OrganisationPK = shippingLine.PK;

		AssertEquals("CarrierIdentificationNumber in case of TCU", "BE789", provider.CarrierIdentificationNumber);
	}

	public void TestConsignee()
	{
		AssertNotNull(provider.Consignee);
	}

	public void TestConsignor()
	{
		AssertNotNull(provider.Consignor);
	}

	public void TestCountryOfRouting()
	{
		declaration.ItineraryCountries.AddNew();
		AssertEquals(1, provider.CountryOfRouting.Count);
	}

	public void TestGrossMass()
	{
		declaration.JE_TotalWeight = 123;
		declaration.JE_TotalWeightUnit = "T";
		AssertEquals(new decimal(123000), provider.GrossMass);
	}

	public void TestModeOfTransportAtTheBorder()
	{
		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(ModeOfTransportList.Codes._4_AirTransport, provider.ModeOfTransportAtTheBorder);
			declaration.JE_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
			AssertEquals(ModeOfTransportList.Codes._7_FixedTransportInstallations, provider.ModeOfTransportAtTheBorder);
			declaration.JE_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			AssertEquals(ModeOfTransportList.Codes._8_InlandWaterwayTransport, provider.ModeOfTransportAtTheBorder);
			declaration.JE_TransportMode = Core.Constants.TransportModes.OwnPropulsion;
			AssertEquals(ModeOfTransportList.Codes._9_OwnPropulsion, provider.ModeOfTransportAtTheBorder);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			AssertEquals(ModeOfTransportList.Codes._5_PostalConsignment, provider.ModeOfTransportAtTheBorder);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals(ModeOfTransportList.Codes._2_RailTransport, provider.ModeOfTransportAtTheBorder);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals(ModeOfTransportList.Codes._3_RoadTransport, provider.ModeOfTransportAtTheBorder);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(ModeOfTransportList.Codes._1_SeaTransport, provider.ModeOfTransportAtTheBorder);
			declaration.JE_TransportMode = "ZZZ";
			AssertNull(provider.ModeOfTransportAtTheBorder);
		});
	}

	public void TestReferenceNumberUCR()
	{
		var invoiceHeader = entryHeader.InvoiceHeaders[0];

		invoiceHeader.JZ_UCR = "UCR456";
		AssertEquals("UCR456", provider.ReferenceNumberUCR);
	}

	public void TestTransportChargesMOP()
	{
		var invoiceHeader = entryHeader.InvoiceHeaders[0];

		invoiceHeader.ZG_TransportChargesMethodOfPayment = "C";
		AssertEquals("C", provider.TransportChargesMOP);
	}

	public void TestTransportDocuments()
	{
		var transportDocument = entryInstruction.AdditionalInfos.AddNew();
		transportDocument.CSI_SubType = BEAdditionalDocTypeList.Codes.TransportDocuments;
		AssertEquals("1 additional info", 1, provider.TransportDocuments.Count);
	}

	public void TestArrivalTransportMeansType()
	{
		AssertNull(provider.ArrivalTransportMeansType);
	}

	public void TestArrivalTransportMeansId()
	{
		AssertNull(provider.ArrivalTransportMeansId);
	}

	public void TestTransportDocumentSequenceNumber()
	{
		AssertEquals(0, provider.TransportDocumentSequenceNumber);
	}

	public void TestTransportDocumentType()
	{
		AssertNull(provider.TransportDocumentType);
	}

	public void TestTransportDocumentReferenceNumber()
	{
		AssertNull(provider.TransportDocumentReferenceNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_TransportModeInland = "SEA";
		declaration.JE_TransportMeans = "11";
		declaration.JE_TransportIDInland = "MSC ANITA";
		declaration.JE_RN_NKTransportNationalityInland = "BE";
		declaration.CusContainers.AddNew();

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_InvoiceNumber = "ABC123";
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = "H1";
		invoiceHeader.InvoiceLines.AddNew();
		invoiceHeader.InvoiceLines.AddNew();

		entryInstruction.CEI_ClusterKey = 1;
		entryInstruction.CEI_JE = declaration.PK;

		var lineMerger = new EU.Business.Declaration.LineMerger(declaration);
		lineMerger.DoMerge();

		entryHeader = declaration.CustomsEntryHeaders.Single();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		provider = new ConsignmentProvider(declaration, entryInstruction, entryHeader);
	}

	protected override ConsignmentProvider GetProvider() => provider;

	JobDeclaration declaration;
	Declaration.CusEntryHeader entryHeader;
	Declaration.CusEntryInstruction entryInstruction;
	ConsignmentProvider provider;
}
