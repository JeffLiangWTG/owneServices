using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

abstract class NctsSADHeaderCommonWrapperTest<THeaderWrapper> : TestCaseWithFactory
	where THeaderWrapper : NctsSADHeaderCommonWrapper
{
	public void TestHeaderDataDeclaredOnItems()
	{
		AssertEquals("Default value should be false", false, headerWrapper.HeaderDataDeclaredOnItems);
		nctsHeader.MovementHeader.ParticipantType = "STD";
		AssertEquals("When ParticipantType='STD', HeaderDataDeclaredOnItems should be false", false, headerWrapper.HeaderDataDeclaredOnItems);
		nctsHeader.MovementHeader.ParticipantType = "GRP";
		AssertEquals("When ParticipantType='GRP', HeaderDataDeclaredOnItems should be true", true, headerWrapper.HeaderDataDeclaredOnItems);
	}

	public void TestSecurityData()
	{
		nctsHeader.BH_FTZMove = false;
		AssertEquals(nameof(headerWrapper.SecurityData), false, headerWrapper.SecurityData);

		nctsHeader.BH_FTZMove = true;
		AssertEquals(nameof(headerWrapper.SecurityData), true, headerWrapper.SecurityData);
	}

	public void TestMeansOfTransportAtDeparture()
	{
		nctsMovementHeader.BM_RN_NKTransportAtDepartureCountry = "";
		nctsMovementHeader.BM_TransportAtDeparture = "";
		CombineAssertions("When input value are empty", () => AssertMeansOfTransportAtDeparture(ZString.Empty, ZString.Empty));

		nctsMovementHeader.BM_RN_NKTransportAtDepartureCountry = "KR";
		nctsMovementHeader.BM_TransportAtDeparture = "RX2839A";
		CombineAssertions("When input value are filled", () => AssertMeansOfTransportAtDeparture("KR", "RX2839A"));

		void AssertMeansOfTransportAtDeparture(ZString nationality, ZString identity)
		{
			var meansOfTransportAtDeparture = headerWrapper.MeansOfTransportAtDeparture;
			AssertType<SADMeansOfTransportWrapper>("MeansOfTransportAtDeparture type", meansOfTransportAtDeparture);
			AssertEquals(nameof(meansOfTransportAtDeparture.Nationality), nationality, meansOfTransportAtDeparture.Nationality);
			AssertEquals(nameof(meansOfTransportAtDeparture.Identity), identity, meansOfTransportAtDeparture.Identity);
		}
	}

	public void TestMeansOfTransportCrossingBorder()
	{
		var meansOfTransportCrossingBorder = headerWrapper.MeansOfTransportCrossingBorder;
		AssertNotNull(nameof(meansOfTransportCrossingBorder), meansOfTransportCrossingBorder);
		AssertEquals($"{nameof(meansOfTransportCrossingBorder)} type", ExpectedMeansOfTransportCrossingBorderType, meansOfTransportCrossingBorder.GetType());
	}

	protected abstract Type ExpectedMeansOfTransportCrossingBorderType { get; }

	public void TestDialogLanguageIndicatorAtDeparture()
	{
		AssertEquals(nameof(headerWrapper.DialogLanguageIndicatorAtDeparture), ZString.Empty, headerWrapper.DialogLanguageIndicatorAtDeparture);
	}

	public void TestSecurityBlockWhenSafetyAndSecurity()
	{
		nctsHeader.BH_FTZMove = true;
		AssertType<NctsSADHeaderSafetyAndSecurityBlockWrapper>(nameof(headerWrapper.SecurityBlock), headerWrapper.SecurityBlock);
	}

	public void TestSecurityBlockWhenNotSafetyAndSecurity()
	{
		nctsHeader.BH_FTZMove = false;
		AssertType<NctsSADHeaderSecurityBlockWrapper>(nameof(headerWrapper.SecurityBlock), headerWrapper.SecurityBlock);
	}

	public void TestExitCustomsOffice()
	{
		AssertEquals(nameof(headerWrapper.ExitCustomsOffice), ZString.Empty, headerWrapper.ExitCustomsOffice);
	}

	public void TestAgreedLocationOfGoods()
	{
		var agreedLocationOfGoods = headerWrapper.AgreedLocationOfGoods;
		AssertNotNull(nameof(agreedLocationOfGoods), agreedLocationOfGoods);
		AssertType<NctsSADHeaderAgreedLocationOfGoodsWrapper>($"{nameof(agreedLocationOfGoods)} type", agreedLocationOfGoods);
	}

	public void TestPrincipalTrader()
	{
		var principalTrader = headerWrapper.PrincipalTrader;
		AssertNotNull(nameof(principalTrader), principalTrader);
		AssertEquals($"{nameof(principalTrader)} type", ExpectedPrincipalTraderType, principalTrader.GetType());
	}

	protected abstract Type ExpectedPrincipalTraderType { get; }

	public abstract void TestTransitCustomsOffices();

	public abstract void TestGuarantees();

	public void TestDestinationCustomsOffice()
	{
		nctsHeader.DestinationCustomsOfficeCodeForDeparture = ZString.Empty;
		AssertEquals(nameof(headerWrapper.DestinationCustomsOffice), ZString.Empty, headerWrapper.DestinationCustomsOffice);

		nctsHeader.DestinationCustomsOfficeCodeForDeparture = "IT123456";
		AssertEquals(nameof(headerWrapper.DestinationCustomsOffice), "IT123456", headerWrapper.DestinationCustomsOffice);
	}

	public void TestSealsWhenSealTypeIsNotPAC()
	{
		AssertEquals($"When {nameof(nctsHeader.DepartureHeaderContainers)} collection is empty, Seals Count", 0, headerWrapper.Seals.Count());

		AddHeaderContainer("S1_1", "S1_2");
		AddHeaderContainer("S1_1", "");
		AddHeaderContainer("S2", "");
		AssertContainsExactElementsInAnyOrder($"When {nameof(nctsHeader.DepartureHeaderContainers)} collection is not empty, Seals", new ZString[] { "S1_1", "S1_2", "S2" }, headerWrapper.Seals.ToArray());

		void AddHeaderContainer(string seal1, string seal2)
		{
			var container1 = nctsHeader.DepartureHeaderContainers.AddNew();
			container1.BC_Seal1 = seal1;
			container1.BC_Seal2 = seal2;
		}
	}

	public void TestSealsWhenSealTypeIsPAC()
	{
		nctsMovementHeader.BM_SealType = SealTypeList.Codes.PackageSeal;

		AssertEquals($"When {nameof(nctsHeader.Seals)} collection is empty, Seals Count", 0, headerWrapper.Seals.Count());

		nctsHeader.Seals.AddNew().CY_Data = "S1_1";
		nctsHeader.Seals.AddNew().CY_Data = "S1_2";
		nctsHeader.Seals.AddNew().CY_Data = "";
		nctsHeader.Seals.AddNew().CY_Data = "S2";
		nctsHeader.Seals.AddNew().CY_Data = "S1_1";
		AssertContainsExactElementsInAnyOrder($"When {nameof(nctsHeader.Seals)} collection is not empty, Seals", new ZString[] { "S1_1", "S1_2", "S2" }, headerWrapper.Seals.ToArray());
	}

	public void TestControlResult()
	{
		var controlResult = headerWrapper.ControlResult;
		AssertNotNull(nameof(controlResult), controlResult);
		AssertType<NctsSADHeaderControlResultWrapper>($"{nameof(controlResult)} type", controlResult);
	}

	public void TestAnnualProgressiveNumber()
	{
		AssertEquals(nameof(headerWrapper.AnnualProgressiveNumber), "<<MSGNO PLACEHOLDER>>", headerWrapper.AnnualProgressiveNumber);
	}

	public abstract void TestAuthorizationNo();

	public abstract void TestAuthorizationCIN();

	public void TestTotalItems()
	{
		AssertEquals($"When {nameof(nctsHeader.MovementHeader.GoodsItems)} collection is empty", 0, headerWrapper.TotalItems);

		nctsMovementHeader.GoodsItems.AddNew();
		nctsMovementHeader.GoodsItems.AddNew();
		nctsMovementHeader.GoodsItems.AddNew();
		AssertEquals($"When {nameof(nctsHeader.MovementHeader.GoodsItems)} collection is not empty", 3, headerWrapper.TotalItems);
	}

	public void TestAcceptanceDate()
	{
		CombineAssertions(() =>
		{
			AssertEquals($"Default {nameof(headerWrapper.AcceptanceDate)}", ZDate.Empty, headerWrapper.AcceptanceDate);

			nctsMovementHeader.BM_EntryDate = new ZDate(2021, 01, 01);
			AssertEquals(nameof(headerWrapper.AcceptanceDate), new ZDate(2021, 01, 01), headerWrapper.AcceptanceDate);
		});
	}

	public void TestDeclaration()
	{
		var declaration = headerWrapper.Declaration;
		AssertNotNull(nameof(declaration), declaration);
		AssertEquals($"{nameof(declaration)} type", ExpectedDeclarationType, declaration.GetType());
	}

	protected abstract Type ExpectedDeclarationType { get; }

	public void TestConsignor()
	{
		var supplier = Factory.New<OrgHeader>();
		supplier.CustomsCodes.AddNew("EOR", "385040449", "IT");
		var address = supplier.Addresses.AddNew();
		nctsHeader.Consignor.E2_OA_Address = address.PK;
		address.CompanyName = "IKEA";
		address.Address1 = "MAIN";
		address.Address2 = "ADDRESS";
		address.Postcode = "4000";
		address.City = "ABCEXPMEL";
		address.OA_RN_NKCountryCode = "ZA";

		var consignor = headerWrapper.Consignor;
		AssertNotNull(nameof(consignor), consignor);
		AssertType<SADTraderWrapper>($"{nameof(consignor)} type", consignor);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(consignor.IdCountryCode), "IT", consignor.IdCountryCode);
			AssertEquals(nameof(consignor.ID), "385040449", consignor.ID);
			AssertEquals(nameof(consignor.Name), "IKEA", consignor.Name);
			AssertEquals(nameof(consignor.Address), "MAIN ADDRESS", consignor.Address);
			AssertEquals(nameof(consignor.Postcode), "4000", consignor.Postcode);
			AssertEquals(nameof(consignor.City), "ABCEXPMEL", consignor.City);
			AssertEquals(nameof(consignor.CountryCode), "ZA", consignor.CountryCode);
		});
	}

	public void TestConsignee()
	{
		var importer = Factory.New<OrgHeader>();
		importer.CustomsCodes.AddNew("EOR", "385040449", "IT");
		var address = importer.Addresses.AddNew();
		nctsHeader.Consignee.E2_OA_Address = address.PK;
		address.CompanyName = "YKK MEDITERRANEO SPA";
		address.Address1 = "ZONA IND. CAMPOLUNGO";
		address.Postcode = "63100";
		address.City = "ASCOLI PICENO";
		address.OA_RN_NKCountryCode = "IT";

		var consignee = headerWrapper.Consignee;
		AssertNotNull(nameof(consignee), consignee);
		AssertType<SADTraderWrapper>($"{nameof(consignee)} type", consignee);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(consignee.IdCountryCode), "IT", consignee.IdCountryCode);
			AssertEquals(nameof(consignee.ID), "385040449", consignee.ID);
			AssertEquals(nameof(consignee.Name), "YKK MEDITERRANEO SPA", consignee.Name);
			AssertEquals(nameof(consignee.Address), "ZONA IND. CAMPOLUNGO", consignee.Address);
			AssertEquals(nameof(consignee.Postcode), "63100", consignee.Postcode);
			AssertEquals(nameof(consignee.City), "ASCOLI PICENO", consignee.City);
			AssertEquals(nameof(consignee.CountryCode), "IT", consignee.CountryCode);
		});
	}

	public void TestDeclarantTrader()
	{
		var declarantTrader = headerWrapper.DeclarantTrader;
		AssertNotNull(nameof(declarantTrader), declarantTrader);
		AssertType<SADDeclarantTraderWrapper>($"{nameof(declarantTrader)} type", declarantTrader);
	}

	public void TestCountryOfDispatch()
	{
		nctsHeader.BH_RL_NKImportLoadPort = ZString.Empty;
		AssertEquals(nameof(headerWrapper.CountryOfDispatch), ZString.Empty, headerWrapper.CountryOfDispatch);

		nctsHeader.BH_RL_NKImportLoadPort = "CN";
		AssertEquals(nameof(headerWrapper.CountryOfDispatch), "CN", headerWrapper.CountryOfDispatch);
	}

	public void TestTermsOfDelivery()
	{
		var termsOfDelivery = headerWrapper.TermsOfDelivery;
		AssertNotNull(nameof(termsOfDelivery), termsOfDelivery);
		AssertType<SADEmptyTermsOfDeliveryWrapper>($"{nameof(termsOfDelivery)} type", termsOfDelivery);
	}

	public void TestTransactionData()
	{
		var transactionData = headerWrapper.TransactionData;
		AssertNotNull(nameof(transactionData), transactionData);
		AssertEquals($"{nameof(transactionData)} type", ExpectedTransactionDataType, transactionData.GetType());
	}

	protected abstract Type ExpectedTransactionDataType { get; }

	public void TestDeferredPayment()
	{
		var deferredPayment = headerWrapper.DeferredPayment;
		AssertNotNull(nameof(deferredPayment), deferredPayment);
		AssertType<NctsSADHeaderDeferredPaymentWrapper>($"{nameof(deferredPayment)} type", deferredPayment);
	}

	public void TestWarehouseIdentification()
	{
		var warehouseIdentification = headerWrapper.WarehouseIdentification;
		AssertNotNull(nameof(warehouseIdentification), warehouseIdentification);
		AssertType<NctsSADHeaderWarehouseIdentificationWrapper>($"{nameof(warehouseIdentification)} type", warehouseIdentification);
	}

	public void TestCountryOfDestination()
	{
		nctsMovementHeader.BM_RL_NKDestinationPort = ZString.Empty;
		AssertEquals(nameof(headerWrapper.CountryOfDestination), ZString.Empty, headerWrapper.CountryOfDestination);

		nctsMovementHeader.BM_RL_NKDestinationPort = "IT";
		AssertEquals(nameof(headerWrapper.CountryOfDestination), "IT", headerWrapper.CountryOfDestination);
	}

	public void TestIsContainerizedTransport()
	{
		AssertEquals($"When {nameof(nctsHeader.DepartureHeaderContainers)} collection is empty", false, headerWrapper.IsContainerizedTransport.Value);

		nctsHeader.DepartureHeaderContainers.AddNew();
		AssertEquals($"When {nameof(nctsHeader.DepartureHeaderContainers)} collection is not empty", true, headerWrapper.IsContainerizedTransport.Value);
	}

	public void TestTransportModeAtBorder()
	{
		nctsMovementHeader.BM_ExportTransportMode = ZString.Empty;
		AssertEquals(nameof(headerWrapper.TransportModeAtBorder), ZString.Empty, headerWrapper.TransportModeAtBorder);

		nctsMovementHeader.BM_ExportTransportMode = "1";
		AssertEquals(nameof(headerWrapper.TransportModeAtBorder), "1", headerWrapper.TransportModeAtBorder);
	}

	public void TestInlandTransportMode()
	{
		nctsMovementHeader.BM_InlandTransportMode = ZString.Empty;
		AssertEquals(nameof(headerWrapper.InlandTransportMode), ZString.Empty, headerWrapper.InlandTransportMode);

		nctsMovementHeader.BM_InlandTransportMode = "1";
		AssertEquals(nameof(headerWrapper.InlandTransportMode), "1", headerWrapper.InlandTransportMode);
	}

	public void TestDateLimitOfTemporaryOperation()
	{
		AssertEquals(nameof(headerWrapper.DateLimitOfTemporaryOperation), ZDate.Empty, headerWrapper.DateLimitOfTemporaryOperation);
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When nctsHeader is null", () => GetHeaderWrapper(null));
		AssertExceptionThrown<ArgumentNullException>("When nctsHeader is not departure job", () => GetHeaderWrapper(Factory.New<NctsHeader>()));
		AssertNoExceptionThrown("When nctsHeader is departure job", () => GetHeaderWrapper(nctsHeader));
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		nctsMovementHeader = nctsHeader.MovementHeader;
		headerWrapper = GetHeaderWrapper(nctsHeader);
	}

	protected abstract THeaderWrapper GetHeaderWrapper(NctsHeader nctsHeader);

	protected NctsHeader nctsHeader;
	protected NctsDepartureMovementHeader nctsMovementHeader;
	protected THeaderWrapper headerWrapper;
}
