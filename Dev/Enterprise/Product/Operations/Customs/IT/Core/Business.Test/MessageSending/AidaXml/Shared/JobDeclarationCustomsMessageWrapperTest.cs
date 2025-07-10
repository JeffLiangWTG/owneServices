using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class JobDeclarationCustomsMessageWrapperTest : TestCaseWithFactory
{
	public void TestBorderMeansOfTransportNationality()
	{
		declaration.JE_RN_NKTransportNationality = ZString.Empty;
		var wrapper = GetNewWrapper();
		AssertNullOrEmpty(nameof(wrapper.BorderMeansOfTransportNationality), wrapper.BorderMeansOfTransportNationality);

		declaration.JE_RN_NKTransportNationality = "DE";
		wrapper = GetNewWrapper();
		AssertEquals(nameof(wrapper.BorderMeansOfTransportNationality), "DE", wrapper.BorderMeansOfTransportNationality);
	}

	public void TestCountryOfDestination()
	{
		declaration.JE_GoodsDestination = ZString.Empty;
		var wrapper = GetNewWrapper();
		AssertNullOrEmpty(nameof(wrapper.GoodsCountryOfDestination), wrapper.GoodsCountryOfDestination);

		declaration.JE_GoodsDestination = "DE";
		wrapper = GetNewWrapper();
		AssertEquals(nameof(wrapper.GoodsCountryOfDestination), "DE", wrapper.GoodsCountryOfDestination);
	}

	public void TestCountryOfDispatch()
	{
		declaration.JE_GoodsOrigin = ZString.Empty;
		var wrapper = GetNewWrapper();
		AssertNullOrEmpty(nameof(wrapper.GoodsCountryOfOrigin), wrapper.GoodsCountryOfOrigin);

		declaration.JE_GoodsOrigin = "FR";
		wrapper = GetNewWrapper();
		AssertEquals(nameof(wrapper.GoodsCountryOfOrigin), "FR", wrapper.GoodsCountryOfOrigin);
	}

	public void TestContainerModeForImportMessage()
	{
		declaration.JE_ContainerMode = ZString.Empty;
		var wrapper = GetNewWrapper();
		AssertEquals(nameof(wrapper.ContainerModeForImportMessage), 0, wrapper.ContainerModeForImportMessage);

		declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
		wrapper = GetNewWrapper();
		AssertEquals(nameof(wrapper.ContainerModeForImportMessage), 1, wrapper.ContainerModeForImportMessage);
	}

	public void TestUcr()
	{
		declaration.JE_UCR = ZString.Empty;
		var wrapper = GetNewWrapper();
		AssertNullOrEmpty(nameof(wrapper.Ucr), wrapper.Ucr);

		declaration.JE_UCR = "UCR123";
		wrapper = GetNewWrapper();
		AssertEquals(nameof(wrapper.Ucr), "UCR123", wrapper.Ucr);
	}

	public void TestArrivalMeansOfTransport()
	{
		var wrapper = GetNewWrapper();
		AssertNull(nameof(wrapper.ArrivalMeansOfTransport), wrapper.ArrivalMeansOfTransport);

		declaration.ZG_Box18TransportID = "123";
		declaration.JE_TransportMeans = "23";
		wrapper = GetNewWrapper();
		AssertType<ArrivalMeansOfTransportWrapper>(wrapper.ArrivalMeansOfTransport);
	}

	public void TestBorderTransportMode()
	{
		declaration.JE_TransportMode = ZString.Empty;
		var wrapper = GetNewWrapper();
		AssertEquals(nameof(wrapper.BorderTransportMode), 0, wrapper.BorderTransportMode);

		declaration.JE_TransportMode = "AIR";
		wrapper = GetNewWrapper();
		AssertEquals(nameof(wrapper.BorderTransportMode), 4, wrapper.BorderTransportMode);
	}

	public void TestImportDeclarant()
	{
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		var wrapper = GetNewWrapper();
		AssertNull(nameof(IJobDeclarationCustomsMessageWrapper.ImportDeclarant), wrapper.ImportDeclarant);

		var declarant = Factory.New<OrgHeader>();
		declarant.CustomsCodes.AddNew("EOR", "385040449", "IT");
		var declarantAddress = declarant.MainAddress;
		declarantAddress.CompanyName = "DECLARANT COMPANY NAME";
		declarantAddress.Address1 = "DECLARANT ADDRESS 1";
		declarantAddress.Address2 = "DECLARANT ADDRESS 2";
		declarantAddress.Postcode = "99999";
		declarantAddress.City = "DECLARANT CITY";
		declarantAddress.OA_RN_NKCountryCode = "DE";

		declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
		wrapper = GetNewWrapper();
		AssertType<TraderWrapper>(nameof(IJobDeclarationCustomsMessageWrapper.ImportDeclarant), wrapper.ImportDeclarant);
	}

	public void TestDeclarationCustomsOffice()
	{
		declaration.JE_CustomsOffice = ZString.Empty;
		var wrapper = GetNewWrapper();
		AssertNullOrEmpty(nameof(wrapper.DeclarationCustomsOffice), wrapper.DeclarationCustomsOffice);

		declaration.JE_CustomsOffice = "IT000000";
		wrapper = GetNewWrapper();
		AssertEquals(nameof(wrapper.DeclarationCustomsOffice), "000000", wrapper.DeclarationCustomsOffice);
	}

	public void TestImporter()
	{
		var wrapper = GetNewWrapper();
		AssertNull(nameof(wrapper.Importer), wrapper.Importer);

		var importer = Factory.New<OrgHeader>();
		importer.CustomsCodes.AddNew("EOR", "385040449", "IT");
		var importerAddress = importer.MainAddress;
		importerAddress.CompanyName = "IMPORTER COMPANY NAME";
		importerAddress.Address1 = "IMPORTER ADDRESS 1";
		importerAddress.Address2 = "IMPORTER ADDRESS 2";
		importerAddress.Postcode = "99999";
		importerAddress.City = "IMPORTER CITY";
		importerAddress.OA_RN_NKCountryCode = "IE";

		declaration.ImporterDocumentaryAddress.E2_OA_Address = importerAddress.PK;
		wrapper = GetNewWrapper();
		AssertType<TraderWrapper>(nameof(wrapper.Importer), wrapper.Importer);
	}

	public void TestInlandTransportMode()
	{
		declaration.JE_TransportModeInland = ZString.Empty;
		var wrapper = GetNewWrapper();
		AssertNull(nameof(wrapper.InlandTransportMode), wrapper.InlandTransportMode);

		declaration.JE_TransportModeInland = "SEA";
		wrapper = GetNewWrapper();
		AssertEquals(nameof(wrapper.InlandTransportMode), 1, wrapper.InlandTransportMode);
	}

	public void TestRegionOfDestination()
	{
		var wrapper = GetNewWrapper();
		AssertNullOrEmpty(nameof(wrapper.RegionOfDestination), wrapper.RegionOfDestination);

		var destinationUnloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "ITVCE");
		var refCountryState = Factory.New<RefCountryStates>();
		refCountryState.RW_Code = "XX";
		refCountryState.RW_RN_NKCountryCode = "IT";
		destinationUnloco.RL_RW = refCountryState.PK;

		declaration.JE_RL_NKFinalDestination = "ITVCE";
		wrapper = GetNewWrapper();
		AssertEquals(nameof(wrapper.RegionOfDestination), "XX", wrapper.RegionOfDestination);
	}

	public void TestRepresentative()
	{
		var wrapper = GetNewWrapper();
		declaration.JE_DeclarantType = ZString.Empty;
		AssertNull(nameof(wrapper.Representative), wrapper.Representative);

		var representative = Factory.New<OrgHeader>();
		representative.CustomsCodes.AddNew("EOR", "385040449", "IT");
		var representativeAddress = representative.MainAddress;
		representativeAddress.CompanyName = "REPRESENTATIVE COMPANY NAME";
		representativeAddress.Address1 = "REPRESENTATIVE ADDRESS 1";
		representativeAddress.Address2 = "REPRESENTATIVE ADDRESS 2";
		representativeAddress.Postcode = "99999";
		representativeAddress.City = "REPRESENTATIVE CITY";
		representativeAddress.OA_RN_NKCountryCode = "IT";

		declaration.JE_OA_Representative = representativeAddress.PK;
		declaration.JE_DeclarantType = "DIR";

		wrapper = GetNewWrapper();
		AssertNotNull(nameof(wrapper.Representative), wrapper.Representative);
		CombineAssertions(nameof(wrapper.Representative), () =>
		{
			AssertType<RepresentativeWrapper>(nameof(wrapper.Representative), wrapper.Representative);
			AssertEquals(nameof(IRepresentative.RepresentativeType), 2, wrapper.Representative.RepresentativeType);
		});
	}

	public void TestLocationOfGoods()
	{
		var wrapper = GetNewWrapper();
		AssertNull(nameof(wrapper.ImportLocationOfGoods), wrapper.ImportLocationOfGoods);

		declaration.JE_MessageType = "IMP";
		declaration.JE_LocationQualifier = "XY";
		wrapper = GetNewWrapper();
		AssertType<LocationOfGoodsWrapper>($"When LocationQualifier filled and MessageType=IMP, {nameof(wrapper.ImportLocationOfGoods)}", wrapper.ImportLocationOfGoods);
	}

	public void TestSupervisingCustomsOffice_WhenDeclarationIsImport()
	{
		declaration.JE_MessageType = "IMP";
		var wrapper = GetNewWrapper();
		AssertNullOrEmpty(nameof(wrapper.SupervisingCustomsOffice), wrapper.SupervisingCustomsOffice);

		declaration.CustomsOffices.AddNew("SCO", "");
		wrapper = GetNewWrapper();
		AssertEquals($"{nameof(wrapper.SupervisingCustomsOffice)} for empty SCO", ZString.Empty, wrapper.SupervisingCustomsOffice);

		declaration.CustomsOffices.RemoveAndDelete(declaration.CustomsOffices.GetFirstElementHaving("SCO"));
		declaration.CustomsOffices.AddNew("SCO", "LV002000");
		wrapper = GetNewWrapper();
		AssertEquals(nameof(wrapper.SupervisingCustomsOffice), "LV002000", wrapper.SupervisingCustomsOffice);
	}

	public void TestSupervisingCustomsOffice_WhenDeclarationIsExport()
	{
		declaration.JE_MessageType = "EXP";
		var wrapper = GetNewWrapper();
		AssertNullOrEmpty(nameof(wrapper.SupervisingCustomsOffice), wrapper.SupervisingCustomsOffice);

		declaration.CustomsOffices.AddNew("SVO", "");
		wrapper = GetNewWrapper();
		AssertEquals($"{nameof(wrapper.SupervisingCustomsOffice)} for empty SVO", ZString.Empty, wrapper.SupervisingCustomsOffice);

		declaration.CustomsOffices.RemoveAndDelete(declaration.CustomsOffices.GetFirstElementHaving("SVO"));
		declaration.CustomsOffices.AddNew("SVO", "LV003000");
		wrapper = GetNewWrapper();
		AssertEquals(nameof(wrapper.SupervisingCustomsOffice), "LV003000", wrapper.SupervisingCustomsOffice);
	}

	public void TestPresentationCustomsOffice()
	{
		var wrapper = GetNewWrapper();
		AssertNullOrEmpty(nameof(wrapper.PresentationCustomsOffice), wrapper.PresentationCustomsOffice);

		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation, "");
		wrapper = GetNewWrapper();
		AssertEquals($"{nameof(wrapper.PresentationCustomsOffice)} for empty PRE", ZString.Empty, wrapper.PresentationCustomsOffice);

		var euOfficeCodePre = declaration.CustomsOffices.GetFirstElementHaving(EuOfficeCodesTypes.Codes.OfficeOfPresentation);
		declaration.CustomsOffices.RemoveAndDelete(euOfficeCodePre);
		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation, "DE123123");
		wrapper = GetNewWrapper();
		AssertEquals(nameof(wrapper.PresentationCustomsOffice), "DE123123", wrapper.PresentationCustomsOffice);
	}

	public void TestBuyer()
	{
		var wrapper = GetNewWrapper();
		AssertNull(nameof(wrapper.Buyer), wrapper.Buyer);

		var buyer = Factory.New<OrgHeader>();
		buyer.CustomsCodes.AddNew("EOR", "385040449", "IT");
		var buyerAddress = buyer.MainAddress;
		buyerAddress.CompanyName = "BUYER COMPANY NAME";
		buyerAddress.Address1 = "BUYER ADDRESS 1";
		buyerAddress.Address2 = "BUYER ADDRESS 2";
		buyerAddress.Postcode = "99999";
		buyerAddress.City = "BUYER CITY";
		buyerAddress.OA_RN_NKCountryCode = "BY";

		declaration.JE_OH_Buyer = buyer.PK;
		wrapper = GetNewWrapper();
		AssertType<TraderWrapper>(nameof(wrapper.Buyer), wrapper.Buyer);
	}

	public void TestDutyPayerIdentificationNumber()
	{
		var wrapper = GetNewWrapper();
		AssertNullOrEmpty(nameof(wrapper.DutyPayerIdentificationNumber), wrapper.DutyPayerIdentificationNumber);

		declaration.JE_PaymentMethod = "C";
		declaration.JE_OH_Importer = ZGuid.Empty;
		wrapper = GetNewWrapper();
		AssertNullOrEmpty(nameof(wrapper.DutyPayerIdentificationNumber), wrapper.DutyPayerIdentificationNumber);

		var importer = Factory.New<OrgHeader>();
		importer.CustomsCodes.AddNew("EOR", "385040449", "IT");

		declaration.JE_OH_Importer = importer.PK;
		declaration.JE_PaymentMethod = "D";
		wrapper = GetNewWrapper();
		AssertEquals(nameof(wrapper.DutyPayerIdentificationNumber), "IT385040449", wrapper.DutyPayerIdentificationNumber);

		declaration.JE_PaymentMethod = "1";
		wrapper = GetNewWrapper();
		AssertNullOrEmpty($"When Importer is set but payment method is '1', {nameof(wrapper.DutyPayerIdentificationNumber)}", wrapper.DutyPayerIdentificationNumber);
	}

	public void TestSeller()
	{
		var wrapper = GetNewWrapper();
		AssertNull(nameof(wrapper.Seller), wrapper.Seller);

		var seller = Factory.New<OrgHeader>();
		seller.CustomsCodes.AddNew("EOR", "385040449", "IT");
		var sellerAddress = seller.MainAddress;
		sellerAddress.CompanyName = "SELLER COMPANY NAME";
		sellerAddress.Address1 = "SELLER ADDRESS 1";
		sellerAddress.Address2 = "SELLER ADDRESS 2";
		sellerAddress.Postcode = "99999";
		sellerAddress.City = "SELLER CITY";
		sellerAddress.OA_RN_NKCountryCode = "IT";

		declaration.JE_OA_SellerAddress = sellerAddress.PK;
		wrapper = GetNewWrapper();
		AssertType<TraderWrapper>(nameof(wrapper.Seller), wrapper.Seller);
	}

	public void TestSupplier()
	{
		var wrapper = GetNewWrapper();
		AssertNull(nameof(wrapper.Supplier), wrapper.Supplier);

		var supplier = GetNewSupplier();
		supplier.CustomsCodes.AddNew("EOR", "385040449", "IT");
		declaration.JE_OH_Supplier = supplier.PK;
		wrapper = GetNewWrapper();
		AssertType<TraderCustomsMessageWrapper>(nameof(wrapper.Supplier), wrapper.Supplier);
	}

	public void TestIsContainerizedTransport()
	{
		var wrapper = GetNewWrapper();
		AssertEquals(nameof(wrapper.IsContainerizedTransport), false, wrapper.IsContainerizedTransport);

		declaration.JE_ContainerMode = "FCL";
		wrapper = GetNewWrapper();
		AssertEquals("When ContainerMode is FCL, IsContainerizedTransport", true, wrapper.IsContainerizedTransport);

		declaration.JE_ContainerMode = "LSE";
		wrapper = GetNewWrapper();
		AssertEquals("When ContainerMode is LSE, IsContainerizedTransport", false, wrapper.IsContainerizedTransport);
	}

	public void TestEntryStyle()
	{
		var wrapper = GetNewWrapper();
		declaration.JE_MessageType = "IMP";
		AssertEquals(nameof(wrapper.EntryStyle), "IM", wrapper.EntryStyle);

		declaration.JE_EntryStyle = "CO";
		AssertEquals(nameof(wrapper.EntryStyle), "CO", wrapper.EntryStyle);
	}

	#region Export

	public void TestGetExportBorderMeansOfTransport()
	{
		var wrapper = GetNewWrapper();
		AssertNull(nameof(wrapper.GetExportBorderMeansOfTransport), wrapper.GetExportBorderMeansOfTransport(entryInstruction));

		entryInstruction.CEI_SubStyle = "A";
		entryInstruction.CEI_Procedure = "10";
		declaration.JE_EntryStyle = "EX";

		declaration.JE_TransportMode = "AIR";
		declaration.JE_VoyageFlightNo = "FLIGHTNO";
		declaration.ZG_BorderTransportMeans = "10";
		declaration.JE_RN_NKTransportNationality = "IT";
		wrapper = GetNewWrapper();
		CombineAssertions(() =>
		{
			var borderMeansOfTransport = wrapper.GetExportBorderMeansOfTransport(entryInstruction);
			AssertType<BorderMeansOfTransportWrapper>(nameof(borderMeansOfTransport), borderMeansOfTransport);
			AssertEquals(nameof(borderMeansOfTransport.IdentificationNumber), "FLIGHTNO", borderMeansOfTransport.IdentificationNumber);
			AssertEquals(nameof(borderMeansOfTransport.Nationality), "IT", borderMeansOfTransport.Nationality);
			AssertEquals(nameof(borderMeansOfTransport.TypeOfIdentification), 10, borderMeansOfTransport.TypeOfIdentification);
		});
	}

	public void TestGetExportBorderMeansOfTransport_WhenEntryStyleIsExportNormal()
	{
		entryInstruction.CEI_SubStyle = "A";
		declaration.JE_EntryStyle = "EX";
		declaration.JE_TransportMode = "AIR";
		declaration.JE_VoyageFlightNo = "FLIGHTNO";
		declaration.ZG_BorderTransportMeans = "10";
		declaration.JE_RN_NKTransportNationality = "IT";

		var allowedProcedureCode = new[] { "10", "11", "23", "31" };
		SetProcedureCodeAndAssertGetExportBorderMeansOfTransport(allowedProcedureCode);
	}

	public void TestGetExportBorderMeansOfTransport_WhenEntryStyleIsExportToSpecialTerritory()
	{
		entryInstruction.CEI_SubStyle = "A";
		declaration.JE_EntryStyle = "CO";
		declaration.JE_VoyageFlightNo = "FLIGHTNO";
		declaration.ZG_BorderTransportMeans = "10";
		declaration.JE_RN_NKTransportNationality = "IT";

		var allowedProcedureCode = new[] { "76", "77" };
		declaration.JE_TransportMode = "AIR";
		SetProcedureCodeAndAssertGetExportBorderMeansOfTransport(allowedProcedureCode);

		var transportModesToIgnore = new[] { "MAI", "FIX" };
		entryInstruction.CEI_Procedure = "76";
		SetTransportModeAndAssertGetExportBorderMeansOfTransport(transportModesToIgnore);
	}

	public void TestGetExportBorderMeansOfTransport_WithEntryInstructionSubStyles()
	{
		declaration.JE_EntryStyle = "EX";
		entryInstruction.CEI_Procedure = "10";
		declaration.JE_TransportMode = "AIR";
		declaration.JE_VoyageFlightNo = "FLIGHTNO";
		declaration.ZG_BorderTransportMeans = "10";
		declaration.JE_RN_NKTransportNationality = "IT";

		CombineAssertions(() =>
		{
			entryInstruction.CEI_SubStyle = "B";
			AssertNull("When CEI_SubStyle=B", GetExportBorderMeansOfTransport());

			entryInstruction.CEI_SubStyle = "C";
			AssertNull("When CEI_SubStyle=C", GetExportBorderMeansOfTransport());

			entryInstruction.CEI_SubStyle = "E";
			AssertNull("When CEI_SubStyle=E", GetExportBorderMeansOfTransport());

			entryInstruction.CEI_SubStyle = "F";
			AssertNull("When CEI_SubStyle=F", GetExportBorderMeansOfTransport());

			entryInstruction.CEI_SubStyle = "A";
			AssertNotNull("When CEI_SubStyle=A", GetExportBorderMeansOfTransport());
		});

		IMeansOfTransport GetExportBorderMeansOfTransport()
		{
			var wrapper = GetNewWrapper();
			var borderMeansOfTransport = wrapper.GetExportBorderMeansOfTransport(entryInstruction);
			return borderMeansOfTransport;
		}
	}

	public void TestGetExportBorderMeansOfTransport_TransitionPeriod()
	{
		declaration.MessageVersion = MessageVersionList.Codes.XML;
		declaration.JE_EntryStyle = "EX";
		declaration.JE_TransportMode = "AIR";
		declaration.JE_VoyageFlightNo = "FLIGHTNO";
		declaration.ZG_BorderTransportMeans = "10";
		declaration.JE_RN_NKTransportNationality = "IT";
		entryInstruction.CEI_Procedure = "10";
		entryInstruction.CEI_SubStyle = "A";

		using (TemporallySetTransitionPeriod(isActive: true))
		{
			var wrapper = GetNewWrapper();
			var exportBorderMeansOfTransport = wrapper.GetExportBorderMeansOfTransport(entryInstruction);
			AssertNull("When Transition Period is ON", exportBorderMeansOfTransport);
		}

		using (TemporallySetTransitionPeriod(isActive: false))
		{
			var wrapper = GetNewWrapper();
			var exportBorderMeansOfTransport = wrapper.GetExportBorderMeansOfTransport(entryInstruction);
			AssertNotNull("When Transition Period is OFF", exportBorderMeansOfTransport);
		}
	}

	public void TestCarrierIdentificationNumber()
	{
		CombineAssertions("Carrier different from Declarant", () =>
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.CustomsCodes.AddNew("MSC", "385040455", "IT");
			declarant.CustomsCodes.AddNew("EOR", "385040450", "IT");
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			var wrapper = GetNewWrapper();
			AssertNullOrEmpty("When carrier empty", wrapper.CarrierIdentificationNumber);

			var carrierWithoutEoriTuc = Factory.New<OrgHeader>();
			carrierWithoutEoriTuc.CustomsCodes.AddNew("MSC", "385040452", "IT");
			carrierWithoutEoriTuc.CustomsCodes.AddNew("IVA", "385040499", "IT");
			declaration.JE_OH_ShippingLine = carrierWithoutEoriTuc.PK;

			wrapper = GetNewWrapper();
			AssertNullOrEmpty("When carrier no EORI/TCU", wrapper.CarrierIdentificationNumber);

			var carrierWithEori = Factory.New<OrgHeader>();
			carrierWithEori.CustomsCodes.AddNew("TCU", "385040448", "IT");
			carrierWithEori.CustomsCodes.AddNew("MSC", "385040452", "IT");
			carrierWithEori.CustomsCodes.AddNew("EOR", "385040449", "IT");
			carrierWithEori.CustomsCodes.AddNew("IVA", "385040499", "IT");
			declaration.JE_OH_ShippingLine = carrierWithEori.PK;

			wrapper = GetNewWrapper();
			AssertEquals("When carrier EOR present", "IT385040449", wrapper.CarrierIdentificationNumber);

			var carrierWithTuc = Factory.New<OrgHeader>();
			carrierWithTuc.CustomsCodes.AddNew("MSC", "385040452", "IT");
			carrierWithTuc.CustomsCodes.AddNew("TCU", "385040451", "IT");
			carrierWithTuc.CustomsCodes.AddNew("IVA", "385040499", "IT");
			declaration.JE_OH_ShippingLine = carrierWithTuc.PK;

			wrapper = GetNewWrapper();
			AssertEquals("When carrier no EORI take TCU", "IT385040451", wrapper.CarrierIdentificationNumber);

			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			wrapper = GetNewWrapper();
			AssertEquals("When Declarant is empty", "IT385040451", wrapper.CarrierIdentificationNumber);
		});

		CombineAssertions("When Carrier is equal to Declarant", () =>
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.CustomsCodes.AddNew("MSC", "385040455", "IT");
			declarant.CustomsCodes.AddNew("EOR", "385040450", "IT");
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			declaration.JE_OH_ShippingLine = declarant.MainAddress.PK;
			var wrapper = GetNewWrapper();

			AssertNullOrEmpty(wrapper.CarrierIdentificationNumber);
		});
	}

	public void TestCountryOfExport()
	{
		var wrapper = GetNewWrapper();
		declaration.JE_GoodsOrigin = ZString.Empty;
		AssertNullOrEmpty(nameof(IJobDeclarationCustomsMessageWrapper.CountryOfExport), wrapper.CountryOfExport);

		declaration.JE_GoodsOrigin = "DE";
		AssertEquals(nameof(IJobDeclarationCustomsMessageWrapper.CountryOfExport), "DE", wrapper.CountryOfExport);
	}

	public void TestExitCustomsOffice()
	{
		var wrapper = GetNewWrapper();
		AssertNullOrEmpty(nameof(IJobDeclarationCustomsMessageWrapper.ExitCustomsOffice), wrapper.ExitCustomsOffice);

		declaration.CustomsOffices.AddNew("EXT", "");
		wrapper = GetNewWrapper();
		AssertEquals($"{nameof(IJobDeclarationCustomsMessageWrapper.ExitCustomsOffice)} for empty EXT", ZString.Empty, wrapper.ExitCustomsOffice);

		var exitCustomsOffice = declaration.CustomsOffices.GetFirstElementHaving("EXT");
		exitCustomsOffice.CY_Data = "LV003000";
		wrapper = GetNewWrapper();
		AssertEquals(nameof(IJobDeclarationCustomsMessageWrapper.ExitCustomsOffice), "LV003000", wrapper.ExitCustomsOffice);
	}

	public void TestExportCustomsOffice()
	{
		var wrapper = GetNewWrapper();
		AssertNullOrEmpty(nameof(IJobDeclarationCustomsMessageWrapper.ExportCustomsOffice), wrapper.ExportCustomsOffice);

		declaration.JE_CustomsOffice = "IT279100";
		wrapper = GetNewWrapper();
		AssertEquals(nameof(IJobDeclarationCustomsMessageWrapper.ExportCustomsOffice), "IT279100", wrapper.ExportCustomsOffice);
	}

	public void TestExporter()
	{
		var wrapper = GetNewWrapper();
		AssertNull(nameof(IJobDeclarationCustomsMessageWrapper.Exporter), wrapper.Exporter);

		var exporter = Factory.New<OrgHeader>();
		exporter.CustomsCodes.AddNew("EOR", "385040449", "IT");

		declaration.ExporterDocAddress.OrganisationPK = exporter.PK;
		wrapper = GetNewWrapper();
		AssertType<EoriOrTcuTraderWrapper>(nameof(IJobDeclarationCustomsMessageWrapper.Exporter), wrapper.Exporter);
	}

	public void TestIsSecurityDeclaration()
	{
		var wrapper = GetNewWrapper();
		AssertEquals(nameof(IJobDeclarationCustomsMessageWrapper.IsSecurityDeclaration), false, wrapper.IsSecurityDeclaration);

		declaration.ZG_IsSecurityDeclaration = true;
		AssertEquals(nameof(IJobDeclarationCustomsMessageWrapper.IsSecurityDeclaration), true, wrapper.IsSecurityDeclaration);
	}

	public void TestExportLocationOfGoods()
	{
		CombineAssertions(() =>
		{
			var goodsLocation = declaration.GoodsLocation;

			var wrapper = GetNewWrapper();
			AssertNull(nameof(IJobDeclarationCustomsMessageWrapper.ExportLocationOfGoods), wrapper.ExportLocationOfGoods);

			goodsLocation.CGL_Qualifier = "V";
			wrapper = GetNewWrapper();
			AssertType<CustomsOfficeQualifierLocationOfGoodsWrapper>(nameof(IJobDeclarationCustomsMessageWrapper.ExportLocationOfGoods), wrapper.ExportLocationOfGoods);

			goodsLocation.CGL_Qualifier = "Y";
			wrapper = GetNewWrapper();
			AssertType<AuthorizationNumberQualifierLocationOfGoodsWrapper>(nameof(IJobDeclarationCustomsMessageWrapper.ExportLocationOfGoods), wrapper.ExportLocationOfGoods);

			goodsLocation.CGL_Qualifier = "Z";
			wrapper = GetNewWrapper();
			AssertType<AddressQualifierLocationOfGoodsWrapper>(nameof(IJobDeclarationCustomsMessageWrapper.ExportLocationOfGoods), wrapper.ExportLocationOfGoods);
		});
	}

	public void TestSpecificCircumstanceIndicator()
	{
		var wrapper = GetNewWrapper();
		AssertNullOrEmpty(nameof(IJobDeclarationCustomsMessageWrapper.SpecificCircumstanceIndicator), wrapper.SpecificCircumstanceIndicator);

		declaration.ZG_SpecificCircumstanceIndicator = "A20";
		AssertEquals(nameof(IJobDeclarationCustomsMessageWrapper.SpecificCircumstanceIndicator), "A20", wrapper.SpecificCircumstanceIndicator);
	}

	public void TestExportDeclarant()
	{
		var wrapper = GetNewWrapper();
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		AssertNull(nameof(IJobDeclarationCustomsMessageWrapper.ExportDeclarant), wrapper.ExportDeclarant);

		var declarant = Factory.New<OrgHeader>();
		declarant.CustomsCodes.AddNew("EOR", "385040449", "IT");
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		wrapper = GetNewWrapper();
		AssertType<EoriOrTcuTraderWrapper>(nameof(IJobDeclarationCustomsMessageWrapper.ExportDeclarant), wrapper.ExportDeclarant);
	}

	public void TestConsignmentRoutings()
	{
		var wrapper = GetNewWrapper();
		var consignmentRoutings = wrapper.ConsignmentRoutings;

		AssertNotNull(nameof(IJobDeclarationCustomsMessageWrapper.ConsignmentRoutings), consignmentRoutings);
		AssertEquals($"{nameof(IJobDeclarationCustomsMessageWrapper.ConsignmentRoutings)} count", 0, consignmentRoutings.Count);

		declaration.ItineraryCountries.AddNew();
		declaration.ItineraryCountries.AddNew();
		wrapper = GetNewWrapper();
		consignmentRoutings = wrapper.ConsignmentRoutings;
		AssertEquals($"{nameof(IJobDeclarationCustomsMessageWrapper.ConsignmentRoutings)} count", 2, consignmentRoutings.Count);
		AssertEquals("all consignmentRoutings items are ConsignmentCountryRoutingWrapper", true, consignmentRoutings.All(x => x is ConsignmentCountryRoutingWrapper));
	}

	#endregion

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
	}

	OrgHeader GetNewSupplier()
	{
		var supplier = Factory.New<OrgHeader>();
		var supplierAddress = supplier.MainAddress;
		supplierAddress.CompanyName = "SUPPLIER COMPANY NAME";
		supplierAddress.Address1 = "SUPPLIER ADDRESS 1";
		supplierAddress.Address2 = "SUPPLIER ADDRESS 2";
		supplierAddress.Postcode = "99999";
		supplierAddress.City = "SUPPLIER CITY";
		supplierAddress.OA_RN_NKCountryCode = "IT";
		return supplier;
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;

	IJobDeclarationCustomsMessageWrapper GetNewWrapper() => new JobDeclarationCustomsMessageWrapper(declaration);

	IDisposable TemporallySetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isActive);

	void SetProcedureCodeAndAssertGetExportBorderMeansOfTransport(string[] allowedProcedureCode)
	{
		entryInstruction.CEI_Procedure = "34";
		var wrapper = GetNewWrapper();
		var borderMeansOfTransport = wrapper.GetExportBorderMeansOfTransport(entryInstruction);
		AssertNull(borderMeansOfTransport);

		foreach (var procedureCode in allowedProcedureCode)
		{
			entryInstruction.CEI_Procedure = procedureCode;
			wrapper = GetNewWrapper();
			borderMeansOfTransport = wrapper.GetExportBorderMeansOfTransport(entryInstruction);
			AssertNotNull($"When ProcedureCode={procedureCode}", borderMeansOfTransport);
		}
	}

	void SetTransportModeAndAssertGetExportBorderMeansOfTransport(string[] transportModesToIgnore)
	{
		foreach (var transportMode in transportModesToIgnore)
		{
			declaration.JE_TransportMode = transportMode;
			var wrapper = GetNewWrapper();
			var borderMeansOfTransport = wrapper.GetExportBorderMeansOfTransport(entryInstruction);
			AssertNull($"When TransportMode = {transportMode}", borderMeansOfTransport);
		}
	}
}
