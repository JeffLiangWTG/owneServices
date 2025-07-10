using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.MessageBuilders;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETMessageSerializerTest : BaseMessageSerializerTest
{
	#region Header

	public void TestHeaderFixedPart()
	{
		AssertContains("TET           01234500", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderDeclaration()
	{
		AssertContains("AAA	B	01234	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderConsignor()
	{
		AssertContains("CCCONSIGNORID	THISISTHECONSIGNORNAME	CONSIGNOR ADDRESS	11111	CONSIGNOR CITY	AA		", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderConsignee()
	{
		AssertContains("DDCONSIGNEEID	THISISTHECONSIGNEENAME	CONSIGNEE ADDRESS	22222	CONSIGNEE CITY	FF		", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderDeclarantTrader()
	{
		AssertContains("3	DDDECLARANTTRADER	DECLARANTTRADERNAME	DECLARANT TRADER ADDRESS	3333	DECLARANT TRADER CITY	GG		", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderMeansOfTransportAtDeparture()
	{
		AssertContains("IDENTITY		AA	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderTermsOfDelivery()
	{
		AssertContains("INC	1	COMPLEMENTOFINFO	COMPLEMENTOFINFOLNG	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderMeansOfTransportCrossingBorder()
	{
		AssertContains("TRANSPORT ON BORDER		NN	TYPE	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderTransactionData()
	{
		AssertContains("EUR	19876.00	1.12345	12	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderAgreedLocationOfGoods()
	{
		AssertContains("CODE	DESCRIPTION	CODEANDCIN	SUBPLACE	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderSecurityBlockItinerary()
	{
		AssertContains("2	IT	RU	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderSecurityBlockConsignor()
	{
		AssertContains("SSBLOCONSIGNORID	THISISTHECONSIGNORNAME	CONSIGNOR ADDRESS	11223	CONSIGNOR CITY	XX		", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderSecurityBlockConsignee()
	{
		AssertContains("AABLOCONSIGNEEID	THISISTHECONSIGNEENAME	CONSIGNEE ADDRESS	22222	CONSIGNEE CITY	QW		", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderSecurityBlockCarrier()
	{
		AssertContains("ZXBLOCARRIERID	THISISTHECARRIERNAME	CARRIER ADDRESS	12345	CARRIER CITY	CA		", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderDeferredPayment()
	{
		AssertContains("012345A", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderWarehouseIdentification()
	{
		AssertContains("W	IDWAREHOUSEZ	IT	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderPrincipalTrader()
	{
		AssertContains("FDPRINTRADID	THISISTHEPRINTRADERNAME	PRINTRADER ADDRESS	78945	PRINTRADER CITY	HK		TIN	TIRHOLID	GUATAXIDNUM	REPRESNAME	REPRESTYPE		", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderTransitCustomsOffices()
	{
		AssertContains("2	REF1	201901011010	REF2	201901021010	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderGuarantees()
	{
		AssertContains("1	TYPE1	GRN1	OTHREF1	ACC1	2312.00	AA	BB	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderSeals()
	{
		AssertContains("3	SEAL1		SEAL2		SEAL3		", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeaderControlResult()
	{
		AssertContains("	02012020	03012020			", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestHeader()
	{
		AssertContains("TET           01234500012345A	AAA	B	01234	02072019	1	1	A	12	CCCONSIGNORID	THISISTHECONSIGNORNAME	CONSIGNOR ADDRESS	11111	CONSIGNOR CITY	AA			DDCONSIGNEEID	THISISTHECONSIGNEENAME	CONSIGNEE ADDRESS	22222	CONSIGNEE CITY	FF		3	DDDECLARANTTRADER	DECLARANTTRADERNAME	DECLARANT TRADER ADDRESS	3333	DECLARANT TRADER CITY	GG		KK	LL	IDENTITY		AA	1	INC	1	COMPLEMENTOFINFO	COMPLEMENTOFINFOLNG	TRANSPORT ON BORDER		NN	TYPE	DE		EUR	19876.00	1.12345	12	1	2	PLLOADING	AA123456	1	COMMERCIALREF	CONVEYANCEREF	PLUNLOADING		CODE	DESCRIPTION	CODEANDCIN	SUBPLACE	2	IT	RU	SSBLOCONSIGNORID	THISISTHECONSIGNORNAME	CONSIGNOR ADDRESS	11223	CONSIGNOR CITY	XX		AABLOCONSIGNEEID	THISISTHECONSIGNEENAME	CONSIGNEE ADDRESS	22222	CONSIGNEE CITY	QW		ZXBLOCARRIERID	THISISTHECARRIERNAME	CARRIER ADDRESS	12345	CARRIER CITY	CA		012345A	W	IDWAREHOUSEZ	IT	FDPRINTRADID	THISISTHEPRINTRADERNAME	PRINTRADER ADDRESS	78945	PRINTRADER CITY	HK		TIN	TIRHOLID	GUATAXIDNUM	REPRESNAME	REPRESTYPE		2	REF1	201901011010	REF2	201901021010	1	TYPE1	GRN1	OTHREF1	ACC1	2312.00	AA	BB	AA654321	123	3	SEAL1		SEAL2		SEAL3		01012020		02012020	03012020			", flatFileMessageSerializer.Serialize(etMessage));
	}

	#endregion

	#region Line

	public void TestLineFixedPart()
	{
		AssertContains("?ET1          01234500", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestLineConsignor()
	{
		AssertContains("AQCONSIGNORID	THISISTHECONSIGNORNAME	CONSIGNOR ADDRESS	11111	CONSIGNOR CITY	PO		", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestLineConsignee()
	{
		AssertContains("TYCONSIGNEEID	THISISTHECONSIGNEENAME	CONSIGNEE ADDRESS	45678	CONSIGNEE CITY	RE		", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestLineSecurityBlockConsignor()
	{
		AssertContains("LNBLOCONSIGNORID	BLOCONSIGNORNAME	LN BLOCONSIGNOR ADDR	74185	LN BLOCONSIGNOR CITY	KJ		", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestLineSecurityBlockConsignee()
	{
		AssertContains("LNBLOCONSIGNEEID	BLOCONSIGNEENAME	LN BLOCONSIGNEE ADDR	74185	LN BLOCONSIGNEE CITY	QU		", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestLinePackages()
	{
		AssertContains("1	1	MARKS AND NUMBERS		BUL	2	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestLineContainers()
	{
		AssertContains("3	CNT1		CNT2		CNT3		", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestLineAdditionalCodes()
	{
		AssertContains("3	CA01	CA02	CA03	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestLineNationalProcedures()
	{
		AssertContains("2	NP1	NP2	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestLinePreviousAdministrativeDocument()
	{
		AssertContains("Z	ZZZ	APSN	12346578		Z	01012020	SE	78945612	1	IT123X	COMINF		", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestLineSpecialMentionsEori()
	{
		AssertContains("EO1	EO2	100.11	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestLineSpecialMentionsUnloadingData()
	{
		AssertContains("CODCOD	100.11	90.11	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestLineCertificates()
	{
		AssertContains("1	N380	IT	2020	AA123		3	AAA	1	1	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestLineSpecialMentionsAdditionalInformation()
	{
		AssertContains("ADD INFO 70 CHARS		ADD INFO ENCO	0	RU	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestLineTaxes()
	{
		AssertContains("2	TYP	12.00	1	1.000000	2	2	3	3	4	152.00	P	TYP	12.00	5	5.000000	6	6	7	7	8	153.00	A	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestLine()
	{
		AssertContains("?ET1          01234500DECTY	AQCONSIGNORID	THISISTHECONSIGNORNAME	CONSIGNOR ADDRESS	11111	CONSIGNOR CITY	PO		TYCONSIGNEEID	THISISTHECONSIGNEENAME	CONSIGNEE ADDRESS	45678	CONSIGNEE CITY	RE		WW	XC	LNBLOCONSIGNORID	BLOCONSIGNORNAME	LN BLOCONSIGNOR ADDR	74185	LN BLOCONSIGNOR CITY	KJ		LNBLOCONSIGNEEID	BLOCONSIGNEENAME	LN BLOCONSIGNEE ADDR	74185	LN BLOCONSIGNEE CITY	QU		1	LN COM REF NUM	1	1	MARKS AND NUMBERS		BUL	2	3	CNT1		CNT2		CNT3		THIS IS THE GOODS DESCRIPTION				0404	1	COMNOM		3	CA01	CA02	CA03	AA	15364.56	4000	2	NP1	NP2	12000.32	Z	ZZZ	APSN	12346578		Z	01012020	SE	78945612	1	IT123X	COMINF		150.1	EO1	EO2	100.11	CODCOD	100.11	90.11	1	N380	IT	2020	AA123		3	AAA	1	1	ADD INFO 70 CHARS		ADD INFO ENCO	0	RU	COMPL OF INFO	COMPL OF INFO LNG	NOTES 500 CHARS	260.00		2	TYP	12.00	1	1.000000	2	2	3	3	4	152.00	P	TYP	12.00	5	5.000000	6	6	7	7	8	153.00	A	600.56	1200.65	", flatFileMessageSerializer.Serialize(etMessage));
	}

	#endregion

	#region NB

	public void TestMessageNB1()
	{
		AssertContains("TNB           99999801IM	REF	R	120719	1	REA1	A1-REF	A	010719	A1	1	OFF-A1	MRNA	REB1	B1-REF	A	010719	B1	1	OFF-B1	1	3902.32	A123456789	3343.23	99.23	REA2	A2-REF	B	020719	A2	2	OFF-A2	MRNB	REB2	B2-REF	B	020719	B2	2	OFF-B2	2	2343.12	B123456789	33243.96	44.3	REA3	A3-REF	C	030719	A3	3	OFF-A3	MRNC	REB3	B3-REF	C	030719	B3	3	OFF-B3	3	3333.33	C123456789	33333.33	33.3	REA4	A4-REF	D	040719	A4	4	OFF-A4	MRND	REB4	B4-REF	D	040719	B4	4	OFF-B4	4	4444.44	D123456789	44444.44	44.4	1	", flatFileMessageSerializer.Serialize(etMessage));
		AssertContains("?NB1          99999801REA5	A5-REF	E	050719	A5	5	OFF-A5	MRNE	REB5	B5-REF	E	050719	B5	5	OFF-B5	5	5555.55	E123456789	55555.55	55.5	REA6	A6-REF	F	060719	A6	6	OFF-A6	MRNF	REB6	B6-REF	F	060719	B6	6	OFF-B6	6	6666.66	F123456789	66666.66	66.6																																									0	", flatFileMessageSerializer.Serialize(etMessage));
	}

	public void TestMessageNB2()
	{
		AssertContains("TNB           99999902IM	REF	R	120719	2	REA1	A1-REF	A	010719	A1	1	OFF-A1	MRNA	REB1	B1-REF	A	010719	B1	1	OFF-B1	1	3902.32	A123456789	3343.23	99.23																																																													0	", flatFileMessageSerializer.Serialize(etMessage));
	}

	#endregion

	protected override void SetUp()
	{
		base.SetUp();

		var etMessageMock = new Mock<IETMessageSendingObject>();
		etMessageMock.Setup(m => m.MessageHeader).Returns(SetupHeader());
		etMessageMock.Setup(m => m.MessageLines).Returns(SetupLines());
		etMessageMock.Setup(m => m.NBMessages).Returns(SetupNBMessages());
		etMessageMock.Setup(m => m.FallbackProcedure).Returns(false);
		etMessageMock.Setup(m => m.DeclarantTaxNumber).Returns("");
		etMessage = new ETMessage(etMessageMock.Object);

		var etMessageMockWithFallbackProcedure = new Mock<IETMessageSendingObject>();
		etMessageMockWithFallbackProcedure.Setup(m => m.MessageHeader).Returns(SetupHeader());
		etMessageMockWithFallbackProcedure.Setup(m => m.MessageLines).Returns(SetupLines());
		etMessageMockWithFallbackProcedure.Setup(m => m.NBMessages).Returns(SetupNBMessages());
		etMessageMockWithFallbackProcedure.Setup(m => m.FallbackProcedure).Returns(true);
		etMessageMockWithFallbackProcedure.Setup(m => m.DeclarantTaxNumber).Returns("DEC TAX NO");
		flatFileMessageSerializer = new TabbedFlatFileMessageSerializer();
	}

	#region Header Setup

	IETHeader SetupHeader()
	{
		var etMessageHeaderMock = new Mock<IETHeader>();
		etMessageHeaderMock.Setup(m => m.AnnualProgressiveNumber).Returns("012345");
		etMessageHeaderMock.Setup(m => m.AuthorizationNo).Returns("012345");
		etMessageHeaderMock.Setup(m => m.AuthorizationCIN).Returns("A");
		etMessageHeaderMock.Setup(m => m.Declaration).Returns(SetupHeaderDeclaration());
		etMessageHeaderMock.Setup(m => m.AcceptanceDate).Returns(new ZDate(2019, 07, 02));
		etMessageHeaderMock.Setup(m => m.HeaderDataDeclaredOnItems).Returns(true);
		etMessageHeaderMock.Setup(m => m.SecurityData).Returns(true);
		etMessageHeaderMock.Setup(m => m.SecurityBlock).Returns(SetupHeaderSecurityBlock());
		etMessageHeaderMock.Setup(m => m.TotalItems).Returns(12);
		etMessageHeaderMock.Setup(m => m.Consignor).Returns(SetupHeaderConsignor());
		etMessageHeaderMock.Setup(m => m.Consignee).Returns(SetupHeaderConsignee());
		etMessageHeaderMock.Setup(m => m.DeclarantTrader).Returns(SetupHeaderDeclarantTrader());
		etMessageHeaderMock.Setup(m => m.CountryOfDispatch).Returns("KK");
		etMessageHeaderMock.Setup(m => m.CountryOfDestination).Returns("LL");
		etMessageHeaderMock.Setup(m => m.MeansOfTransportAtDeparture).Returns(SetupHeaderMeansOfTransportAtDeparture());
		etMessageHeaderMock.Setup(m => m.IsContainerizedTransport).Returns(true);
		etMessageHeaderMock.Setup(m => m.TermsOfDelivery).Returns(SetupHeaderTermsOfDelivery());
		etMessageHeaderMock.Setup(m => m.MeansOfTransportCrossingBorder).Returns((IETHeaderMeansOfTransportCrossingBorder)SetupHeaderMeansOfTransportCrossingBorder());
		etMessageHeaderMock.Setup(m => m.DialogLanguageIndicatorAtDeparture).Returns("DE");
		etMessageHeaderMock.Setup(m => m.TransactionData).Returns(SetupHeaderTransactionData());
		etMessageHeaderMock.Setup(m => m.TransportModeAtBorder).Returns("1");
		etMessageHeaderMock.Setup(m => m.InlandTransportMode).Returns("2");
		etMessageHeaderMock.Setup(m => m.ExitCustomsOffice).Returns("AA123456");
		etMessageHeaderMock.Setup(m => m.AgreedLocationOfGoods).Returns(SetupHeaderAgreedLocationOfGoods());
		etMessageHeaderMock.Setup(m => m.DeferredPayment).Returns(SetupHeaderDeferredPayment());
		etMessageHeaderMock.Setup(m => m.WarehouseIdentification).Returns(SetupHeaderWarehouseIdentification());
		etMessageHeaderMock.Setup(m => m.PrincipalTrader).Returns(SetupHeaderPrincipalTrader());
		etMessageHeaderMock.Setup(m => m.TransitCustomsOffices).Returns(SetupHeaderTransitCustomsOffices());
		etMessageHeaderMock.Setup(m => m.Guarantees).Returns(SetupHeaderGuarantees());
		etMessageHeaderMock.Setup(m => m.DestinationCustomsOffice).Returns("AA654321");
		etMessageHeaderMock.Setup(m => m.Seals).Returns(new List<ZString>() { "SEAL1", "SEAL2", "SEAL3" });
		etMessageHeaderMock.Setup(m => m.DateLimitOfTemporaryOperation).Returns(new ZDate(2020, 01, 01));
		etMessageHeaderMock.Setup(m => m.ControlResult).Returns(SetupHeaderControlResult());
		return etMessageHeaderMock.Object;
	}

	IETHeaderControlResult SetupHeaderControlResult()
	{
		var controlResult = new Mock<IETHeaderControlResult>();
		controlResult.Setup(m => m.DateLimitOfArrivalNotification).Returns(new ZDate(2020, 01, 02));
		controlResult.Setup(m => m.DateLimitForTheExitFromEC).Returns(new ZDate(2020, 01, 03));
		return controlResult.Object;
	}

	IEnumerable<IETHeaderGuarantee> SetupHeaderGuarantees()
	{
		var guarantee1 = new Mock<IETHeaderGuarantee>();
		guarantee1.Setup(m => m.Type).Returns("TYPE1");
		guarantee1.Setup(m => m.Grn).Returns("GRN1");
		guarantee1.Setup(m => m.OtherReference).Returns("OTHREF1");
		guarantee1.Setup(m => m.AccessCode).Returns("ACC1");
		guarantee1.Setup(m => m.Amount).Returns(2312m);
		guarantee1.Setup(m => m.NotValidForEC).Returns("AA");
		guarantee1.Setup(m => m.NotValidForOtherContractingParties).Returns("BB");
		return new List<IETHeaderGuarantee>() { guarantee1.Object };
	}

	IEnumerable<IETHeaderTransitCustomsOffice> SetupHeaderTransitCustomsOffices()
	{
		var office1 = new Mock<IETHeaderTransitCustomsOffice>();
		office1.Setup(m => m.ReferenceNumber).Returns("REF1");
		office1.Setup(m => m.EstimatedArrivalTime).Returns(new ZDateTime(2019, 01, 01, 10, 10, 10));
		var office2 = new Mock<IETHeaderTransitCustomsOffice>();
		office2.Setup(m => m.ReferenceNumber).Returns("REF2");
		office2.Setup(m => m.EstimatedArrivalTime).Returns(new ZDateTime(2019, 01, 02, 10, 10, 10));
		return new List<IETHeaderTransitCustomsOffice>() { office1.Object, office2.Object };
	}

	IETHeaderPrincipalTrader SetupHeaderPrincipalTrader()
	{
		var principalTrader = new Mock<IETHeaderPrincipalTrader>();
		principalTrader.Setup(m => m.IdCountryCode).Returns("FD");
		principalTrader.Setup(m => m.ID).Returns("PRINTRADID");
		principalTrader.Setup(m => m.Name).Returns("THISISTHEPRINTRADERNAME");
		principalTrader.Setup(m => m.Address).Returns("PRINTRADER ADDRESS");
		principalTrader.Setup(m => m.Postcode).Returns("78945");
		principalTrader.Setup(m => m.City).Returns("PRINTRADER CITY");
		principalTrader.Setup(m => m.CountryCode).Returns("HK");
		principalTrader.Setup(m => m.TraderGuaranteeTaxIdentificationNumber).Returns("TIN");
		principalTrader.Setup(m => m.TIRHolderIdentification).Returns("TIRHOLID");
		principalTrader.Setup(m => m.RepresentativeGuaranteeTaxIdentificationNumber).Returns("GUATAXIDNUM");
		principalTrader.Setup(m => m.RepresentativeName).Returns("REPRESNAME");
		principalTrader.Setup(m => m.RepresentativeType).Returns("REPRESTYPE");
		return principalTrader.Object;
	}

	IWarehouseIdentification SetupHeaderWarehouseIdentification()
	{
		var iMMessageHeaderWarehouseIdentification = new Mock<IWarehouseIdentification>();
		iMMessageHeaderWarehouseIdentification.Setup(m => m.Type).Returns("W");
		iMMessageHeaderWarehouseIdentification.Setup(m => m.Identification).Returns("IDWAREHOUSE");
		iMMessageHeaderWarehouseIdentification.Setup(m => m.CinIdentification).Returns("Z");
		iMMessageHeaderWarehouseIdentification.Setup(m => m.AuthorizingCountry).Returns("IT");

		return iMMessageHeaderWarehouseIdentification.Object;
	}

	IDeferredPayment SetupHeaderDeferredPayment()
	{
		var iMMessageHeaderDeferredPayment = new Mock<IDeferredPayment>();
		iMMessageHeaderDeferredPayment.Setup(m => m.AuthorizationReference).Returns("012345");
		iMMessageHeaderDeferredPayment.Setup(m => m.CinOfAuthorizationReference).Returns("A");

		return iMMessageHeaderDeferredPayment.Object;
	}

	ITrader SetupHeaderSecurityBlockCarrier()
	{
		var securityBlockConsignee = new Mock<ITrader>();
		securityBlockConsignee.Setup(m => m.IdCountryCode).Returns("ZX");
		securityBlockConsignee.Setup(m => m.ID).Returns("BLOCARRIERID");
		securityBlockConsignee.Setup(m => m.Name).Returns("THISISTHECARRIERNAME");
		securityBlockConsignee.Setup(m => m.Address).Returns("CARRIER ADDRESS");
		securityBlockConsignee.Setup(m => m.Postcode).Returns("12345");
		securityBlockConsignee.Setup(m => m.City).Returns("CARRIER CITY");
		securityBlockConsignee.Setup(m => m.CountryCode).Returns("CA");
		return securityBlockConsignee.Object;
	}

	ITrader SetupHeaderSecurityBlockConsignee()
	{
		var securityBlockConsignee = new Mock<ITrader>();
		securityBlockConsignee.Setup(m => m.IdCountryCode).Returns("AA");
		securityBlockConsignee.Setup(m => m.ID).Returns("BLOCONSIGNEEID");
		securityBlockConsignee.Setup(m => m.Name).Returns("THISISTHECONSIGNEENAME");
		securityBlockConsignee.Setup(m => m.Address).Returns("CONSIGNEE ADDRESS");
		securityBlockConsignee.Setup(m => m.Postcode).Returns("22222");
		securityBlockConsignee.Setup(m => m.City).Returns("CONSIGNEE CITY");
		securityBlockConsignee.Setup(m => m.CountryCode).Returns("QW");
		return securityBlockConsignee.Object;
	}

	ITrader SetupHeaderSecurityBlockConsignor()
	{
		var securityBlockConsignor = new Mock<ITrader>();
		securityBlockConsignor.Setup(m => m.IdCountryCode).Returns("SS");
		securityBlockConsignor.Setup(m => m.ID).Returns("BLOCONSIGNORID");
		securityBlockConsignor.Setup(m => m.Name).Returns("THISISTHECONSIGNORNAME");
		securityBlockConsignor.Setup(m => m.Address).Returns("CONSIGNOR ADDRESS");
		securityBlockConsignor.Setup(m => m.Postcode).Returns("11223");
		securityBlockConsignor.Setup(m => m.City).Returns("CONSIGNOR CITY");
		securityBlockConsignor.Setup(m => m.CountryCode).Returns("XX");
		return securityBlockConsignor.Object;
	}

	IETHeaderAgreedLocationOfGoods SetupHeaderAgreedLocationOfGoods()
	{
		var etMessageHeaderTransactionDataMock = new Mock<IETHeaderAgreedLocationOfGoods>();
		etMessageHeaderTransactionDataMock.Setup(m => m.AgreedLocationOfGoodsCode).Returns("CODE");
		etMessageHeaderTransactionDataMock.Setup(m => m.AgreedLocationOfGoodsDescription).Returns("DESCRIPTION");
		etMessageHeaderTransactionDataMock.Setup(m => m.AuthorizedLocationOfGoodsCodeAndCin).Returns("CODEANDCIN");
		etMessageHeaderTransactionDataMock.Setup(m => m.CustomsSubPlace).Returns("SUBPLACE");
		return etMessageHeaderTransactionDataMock.Object;
	}

	ITransactionData SetupHeaderTransactionData()
	{
		var etMessageHeaderTransactionDataMock = new Mock<ITransactionData>();
		etMessageHeaderTransactionDataMock.Setup(m => m.CurrencyCode).Returns("EUR");
		etMessageHeaderTransactionDataMock.Setup(m => m.TotalAmountInvoiced).Returns(19876m);
		etMessageHeaderTransactionDataMock.Setup(m => m.ExchangeRate).Returns(1.123451m);
		etMessageHeaderTransactionDataMock.Setup(m => m.NatureOfTransactionCode).Returns("12");

		return etMessageHeaderTransactionDataMock.Object;
	}

	IMeansOfTransport SetupHeaderMeansOfTransportCrossingBorder()
	{
		var etMessageHeaderMeansOfTransportCrossingBorderMock = new Mock<IETHeaderMeansOfTransportCrossingBorder>();
		etMessageHeaderMeansOfTransportCrossingBorderMock.Setup(m => m.Nationality).Returns("NN");
		etMessageHeaderMeansOfTransportCrossingBorderMock.Setup(m => m.Identity).Returns("TRANSPORT ON BORDER");
		etMessageHeaderMeansOfTransportCrossingBorderMock.Setup(m => m.Type).Returns("TYPE");
		return etMessageHeaderMeansOfTransportCrossingBorderMock.Object;
	}

	ITermOfDeliveryGroup SetupHeaderTermsOfDelivery()
	{
		var etMessageHeaderTermsOfDeliveryMock = new Mock<ITermOfDeliveryGroup>();
		etMessageHeaderTermsOfDeliveryMock.Setup(m => m.IncotermCode).Returns("INC");
		etMessageHeaderTermsOfDeliveryMock.Setup(m => m.ComplementOfInfo).Returns("COMPLEMENTOFINFO");
		etMessageHeaderTermsOfDeliveryMock.Setup(m => m.ComplementOfInfoLng).Returns("COMPLEMENTOFINFOLNG");
		etMessageHeaderTermsOfDeliveryMock.Setup(m => m.ComplementaryCode).Returns("1");
		return etMessageHeaderTermsOfDeliveryMock.Object;
	}

	IMeansOfTransport SetupHeaderMeansOfTransportAtDeparture()
	{
		var etHeaderMeansOfTransportAtDeparture = new Mock<IMeansOfTransport>();
		etHeaderMeansOfTransportAtDeparture.Setup(m => m.Identity).Returns("IDENTITY");
		etHeaderMeansOfTransportAtDeparture.Setup(m => m.Nationality).Returns("AA");
		return etHeaderMeansOfTransportAtDeparture.Object;
	}

	IDeclarantTrader SetupHeaderDeclarantTrader()
	{
		var etHeaderDeclarantTrader = new Mock<IDeclarantTrader>();
		etHeaderDeclarantTrader.Setup(m => m.RepresentativeType).Returns("3");
		etHeaderDeclarantTrader.Setup(m => m.IdCountryCode).Returns("DD");
		etHeaderDeclarantTrader.Setup(m => m.ID).Returns("DECLARANTTRADER");
		etHeaderDeclarantTrader.Setup(m => m.Name).Returns("DECLARANTTRADERNAME");
		etHeaderDeclarantTrader.Setup(m => m.Address).Returns("DECLARANT TRADER ADDRESS");
		etHeaderDeclarantTrader.Setup(m => m.Postcode).Returns("3333");
		etHeaderDeclarantTrader.Setup(m => m.City).Returns("DECLARANT TRADER CITY");
		etHeaderDeclarantTrader.Setup(m => m.CountryCode).Returns("GG");
		return etHeaderDeclarantTrader.Object;
	}

	ITrader SetupHeaderConsignee()
	{
		var etHeaderConsignee = new Mock<ITrader>();
		etHeaderConsignee.Setup(m => m.IdCountryCode).Returns("DD");
		etHeaderConsignee.Setup(m => m.ID).Returns("CONSIGNEEID");
		etHeaderConsignee.Setup(m => m.Name).Returns("THISISTHECONSIGNEENAME");
		etHeaderConsignee.Setup(m => m.Address).Returns("CONSIGNEE ADDRESS");
		etHeaderConsignee.Setup(m => m.Postcode).Returns("22222");
		etHeaderConsignee.Setup(m => m.City).Returns("CONSIGNEE CITY");
		etHeaderConsignee.Setup(m => m.CountryCode).Returns("FF");
		return etHeaderConsignee.Object;
	}

	ITrader SetupHeaderConsignor()
	{
		var etHeaderConsignor = new Mock<ITrader>();
		etHeaderConsignor.Setup(m => m.IdCountryCode).Returns("CC");
		etHeaderConsignor.Setup(m => m.ID).Returns("CONSIGNORID");
		etHeaderConsignor.Setup(m => m.Name).Returns("THISISTHECONSIGNORNAME");
		etHeaderConsignor.Setup(m => m.Address).Returns("CONSIGNOR ADDRESS");
		etHeaderConsignor.Setup(m => m.Postcode).Returns("11111");
		etHeaderConsignor.Setup(m => m.City).Returns("CONSIGNOR CITY");
		etHeaderConsignor.Setup(m => m.CountryCode).Returns("AA");
		return etHeaderConsignor.Object;
	}

	IETHeaderSecurityBlock SetupHeaderSecurityBlock()
	{
		var securityBlock = new Mock<IETHeaderSecurityBlock>();
		securityBlock.Setup(m => m.SpecificCircumstanceIndicator).Returns("A");
		securityBlock.Setup(m => m.PlaceOfLoadingCode).Returns("PLLOADING");
		securityBlock.Setup(m => m.TransportChargesMethodOfPayment).Returns("1");
		securityBlock.Setup(m => m.CommercialReferenceNumber).Returns("COMMERCIALREF");
		securityBlock.Setup(m => m.ConveyanceReferenceNumber).Returns("CONVEYANCEREF");
		securityBlock.Setup(m => m.PlaceOfUnloadingCode).Returns("PLUNLOADING");
		securityBlock.Setup(m => m.TransitCountries).Returns(new List<ZString>() { "IT", "RU" });
		securityBlock.Setup(m => m.Consignor).Returns(SetupHeaderSecurityBlockConsignor());
		securityBlock.Setup(m => m.Consignee).Returns(SetupHeaderSecurityBlockConsignee());
		securityBlock.Setup(m => m.Carrier).Returns(SetupHeaderSecurityBlockCarrier());
		securityBlock.Setup(m => m.SealsNumber).Returns(123);
		return securityBlock.Object;
	}

	IDeclaration SetupHeaderDeclaration()
	{
		var etHeaderDeclaration = new Mock<IDeclaration>();
		etHeaderDeclaration.Setup(m => m.TypeDeclarationSubType1).Returns("AAA");
		etHeaderDeclaration.Setup(m => m.TypeDeclarationSubType2).Returns("B");
		etHeaderDeclaration.Setup(m => m.TypeDeclarationSubType3).Returns("01234");
		return etHeaderDeclaration.Object;
	}

	#endregion

	#region Line Setup

	IEnumerable<IETLine> SetupLines()
	{
		var etLine1 = SetupLine1();
		return new List<IETLine>() { etLine1 };
	}

	IETLine SetupLine1()
	{
		var line = new Mock<IETLine>();
		line.Setup(m => m.DeclarationType).Returns("DECTY");
		line.Setup(m => m.Consignor).Returns(SetupLineConsignor());
		line.Setup(m => m.Consignee).Returns(SetupLineConsignee());
		line.Setup(m => m.DispatchCountryCode).Returns("WW");
		line.Setup(m => m.DestinationCountryCode).Returns("XC");
		line.Setup(m => m.SecurityBlock).Returns(SetupLineSecurityBlock());
		line.Setup(m => m.Packages).Returns(SetupLinePackages());
		line.Setup(m => m.Containers).Returns(new List<ZString>() { "CNT1", "CNT2", "CNT3" });
		line.Setup(m => m.GoodsDescription).Returns("THIS IS THE GOODS DESCRIPTION");
		line.Setup(m => m.ItemNumber).Returns(1);
		line.Setup(m => m.CombinedNomenclature).Returns("COMNOM");
		line.Setup(m => m.AdditionalCodes).Returns(new List<ZString>() { "CA01", "CA02", "CA03" });
		line.Setup(m => m.CountryOfOrigin).Returns("AA");
		line.Setup(m => m.GrossMass).Returns(15364.56m);
		line.Setup(m => m.Procedure).Returns("4000");
		line.Setup(m => m.NationalProcedures).Returns(new List<ZString>() { "NP1", "NP2" });
		line.Setup(m => m.NetMass).Returns(12000.32m);
		line.Setup(m => m.PreviousAdministrativeDocument).Returns(SetupLinePreviousDocument());
		line.Setup(m => m.SupplementaryUnit).Returns(150.1);
		line.Setup(m => m.SpecialMentionGroup).Returns(SetupLineSpecialMentions());
		line.Setup(m => m.Certificates).Returns(SetupLineCertificates());
		line.Setup(m => m.ComplementOfInformation).Returns("COMPL OF INFO");
		line.Setup(m => m.ComplementOfInformationLng).Returns("COMPL OF INFO LNG");
		line.Setup(m => m.Notes).Returns("NOTES 500 CHARS");
		line.Setup(m => m.StatisticalValueAmount).Returns(260m);
		line.Setup(m => m.Duties).Returns(SetupLineDuties());
		line.Setup(m => m.TotalItemTaxedAmount).Returns(600.56);
		line.Setup(m => m.GrandTotalTaxedAmount).Returns(1200.65);
		return line.Object;
	}

	IEnumerable<IDutyTaxFee> SetupLineDuties()
	{
		var duty1 = new Mock<IDutyTaxFee>();
		duty1.Setup(m => m.Type).Returns("TYP");
		duty1.Setup(m => m.Base).Returns(12);
		duty1.Setup(m => m.CalculationFactor1).Returns("1");
		duty1.Setup(m => m.Rate1).Returns(1);
		duty1.Setup(m => m.CalculationFactor2).Returns("2");
		duty1.Setup(m => m.Rate2).Returns(2);
		duty1.Setup(m => m.CalculationFactor3).Returns("3");
		duty1.Setup(m => m.Rate3).Returns(3);
		duty1.Setup(m => m.CalculationFactor4).Returns("4");
		duty1.Setup(m => m.Amount).Returns(152);
		duty1.Setup(m => m.MethodOfPayment).Returns("P");
		var duty2 = new Mock<IDutyTaxFee>();
		duty2.Setup(m => m.Type).Returns("TYP");
		duty2.Setup(m => m.Base).Returns(12);
		duty2.Setup(m => m.CalculationFactor1).Returns("5");
		duty2.Setup(m => m.Rate1).Returns(5);
		duty2.Setup(m => m.CalculationFactor2).Returns("6");
		duty2.Setup(m => m.Rate2).Returns(6);
		duty2.Setup(m => m.CalculationFactor3).Returns("7");
		duty2.Setup(m => m.Rate3).Returns(7);
		duty2.Setup(m => m.CalculationFactor4).Returns("8");
		duty2.Setup(m => m.Amount).Returns(153);
		duty2.Setup(m => m.MethodOfPayment).Returns("A");
		return new List<IDutyTaxFee>() { duty1.Object, duty2.Object };
	}

	IEnumerable<ICertificate> SetupLineCertificates()
	{
		var certificate1 = new Mock<ICertificate>();
		certificate1.Setup(m => m.DocumentType).Returns("N380");
		certificate1.Setup(m => m.CountryOfIssue).Returns("IT");
		certificate1.Setup(m => m.IssuingYear).Returns("2020");
		certificate1.Setup(m => m.Reference).Returns("AA123");
		certificate1.Setup(m => m.Quantity).Returns(3m);
		certificate1.Setup(m => m.UnitOfMeasurement).Returns("AAA");
		certificate1.Setup(m => m.DerogationFlag).Returns(true);
		certificate1.Setup(m => m.RetrospectiveDerogationFlag).Returns(true);
		return new List<ICertificate>() { certificate1.Object };
	}

	IETLineSpecialMentionGroup SetupLineSpecialMentions()
	{
		var speciaMentions = new Mock<IETLineSpecialMentionGroup>();
		speciaMentions.Setup(m => m.Eori).Returns(SetupLineSpecialMentionsEori());
		speciaMentions.Setup(m => m.UnloadingData).Returns(SetupLineSpecialMentionsUnloadingData());
		speciaMentions.Setup(m => m.AdditionalInformation).Returns(SetupLineSpecialMentionsAdditionalInformation());
		return speciaMentions.Object;
	}

	IETLineSpecialMentionInfoAdditionalInformation SetupLineSpecialMentionsAdditionalInformation()
	{
		var additionalInformation = new Mock<IETLineSpecialMentionInfoAdditionalInformation>();
		additionalInformation.Setup(m => m.AdditionalInformation).Returns("ADD INFO 70 CHARS");
		additionalInformation.Setup(m => m.AdditionalInformationCoded).Returns("ADD INFO ENCO");
		additionalInformation.Setup(m => m.IsExportFromCE).Returns(false);
		additionalInformation.Setup(m => m.ExportCountry).Returns("RU");
		return additionalInformation.Object;
	}

	ISpecialMentionUnloadingDataInfo SetupLineSpecialMentionsUnloadingData()
	{
		var unloadingData = new Mock<ISpecialMentionUnloadingDataInfo>();
		unloadingData.Setup(m => m.CommodityCode).Returns("CODCOD");
		unloadingData.Setup(m => m.Quantity).Returns(100.11);
		unloadingData.Setup(m => m.SupplementaryUnit).Returns(90.11);
		return unloadingData.Object;
	}

	ISpecialMentionEoriInfo SetupLineSpecialMentionsEori()
	{
		var eori = new Mock<ISpecialMentionEoriInfo>();
		eori.Setup(m => m.FirstEoriCode).Returns("EO1");
		eori.Setup(m => m.PreviousInvoiceAmount).Returns(100.11);
		eori.Setup(m => m.SecondEoriCode).Returns("EO2");
		return eori.Object;
	}

	IPreviousDocument SetupLinePreviousDocument()
	{
		var previousDocument = new Mock<IPreviousDocument>();
		previousDocument.Setup(m => m.DocType).Returns("Z");
		previousDocument.Setup(m => m.Category).Returns("ZZZ");
		previousDocument.Setup(m => m.Register).Returns("APSN");
		previousDocument.Setup(m => m.ReferenceNumber).Returns("12346578");
		previousDocument.Setup(m => m.ReferenceCIN).Returns("Z");
		previousDocument.Setup(m => m.Date).Returns(new ZDate(2020, 01, 01));
		previousDocument.Setup(m => m.Series).Returns("SE");
		previousDocument.Setup(m => m.CustomsOffice).Returns("78945612");
		previousDocument.Setup(m => m.ItemNumber).Returns(1);
		previousDocument.Setup(m => m.Mrn).Returns("IT123X");
		previousDocument.Setup(m => m.ComplementOfInformation).Returns("COMINF");
		return previousDocument.Object;
	}

	IEnumerable<IPackage> SetupLinePackages()
	{
		var package1 = new Mock<IPackage>();
		package1.Setup(m => m.NumberOfPacks).Returns(1);
		package1.Setup(m => m.MarksAndNumbers).Returns("MARKS AND NUMBERS");
		package1.Setup(m => m.PackageType).Returns("BUL");
		package1.Setup(m => m.NumberOfPieces).Returns(2);
		return new List<IPackage>() { package1.Object };
	}

	IETLineSecurityBlock SetupLineSecurityBlock()
	{
		var lineSecurityBlock = new Mock<IETLineSecurityBlock>();
		lineSecurityBlock.Setup(m => m.Consignor).Returns(SetupLineSecurityBlockConsignor());
		lineSecurityBlock.Setup(m => m.Consignee).Returns(SetupLineSecurityBlockConsignee());
		lineSecurityBlock.Setup(m => m.TransportChargesMethodOfPayment).Returns("1");
		lineSecurityBlock.Setup(m => m.CommercialReferenceNumber).Returns("LN COM REF NUM");
		lineSecurityBlock.Setup(m => m.UNDangerousGoodsCode).Returns("0404");
		return lineSecurityBlock.Object;
	}

	ITrader SetupLineSecurityBlockConsignee()
	{
		var securityBlockConsignee = new Mock<ITrader>();
		securityBlockConsignee.Setup(m => m.IdCountryCode).Returns("LN");
		securityBlockConsignee.Setup(m => m.ID).Returns("BLOCONSIGNEEID");
		securityBlockConsignee.Setup(m => m.Name).Returns("BLOCONSIGNEENAME");
		securityBlockConsignee.Setup(m => m.Address).Returns("LN BLOCONSIGNEE ADDR");
		securityBlockConsignee.Setup(m => m.Postcode).Returns("74185");
		securityBlockConsignee.Setup(m => m.City).Returns("LN BLOCONSIGNEE CITY");
		securityBlockConsignee.Setup(m => m.CountryCode).Returns("QU");
		return securityBlockConsignee.Object;
	}

	ITrader SetupLineSecurityBlockConsignor()
	{
		var securityBlockConsignor = new Mock<ITrader>();
		securityBlockConsignor.Setup(m => m.IdCountryCode).Returns("LN");
		securityBlockConsignor.Setup(m => m.ID).Returns("BLOCONSIGNORID");
		securityBlockConsignor.Setup(m => m.Name).Returns("BLOCONSIGNORNAME");
		securityBlockConsignor.Setup(m => m.Address).Returns("LN BLOCONSIGNOR ADDR");
		securityBlockConsignor.Setup(m => m.Postcode).Returns("74185");
		securityBlockConsignor.Setup(m => m.City).Returns("LN BLOCONSIGNOR CITY");
		securityBlockConsignor.Setup(m => m.CountryCode).Returns("KJ");
		return securityBlockConsignor.Object;
	}

	ITrader SetupLineConsignor()
	{
		var lineConsignor = new Mock<ITrader>();
		lineConsignor.Setup(m => m.IdCountryCode).Returns("AQ");
		lineConsignor.Setup(m => m.ID).Returns("CONSIGNORID");
		lineConsignor.Setup(m => m.Name).Returns("THISISTHECONSIGNORNAME");
		lineConsignor.Setup(m => m.Address).Returns("CONSIGNOR ADDRESS");
		lineConsignor.Setup(m => m.Postcode).Returns("11111");
		lineConsignor.Setup(m => m.City).Returns("CONSIGNOR CITY");
		lineConsignor.Setup(m => m.CountryCode).Returns("PO");
		return lineConsignor.Object;
	}

	ITrader SetupLineConsignee()
	{
		var lineConsignee = new Mock<ITrader>();
		lineConsignee.Setup(m => m.IdCountryCode).Returns("TY");
		lineConsignee.Setup(m => m.ID).Returns("CONSIGNEEID");
		lineConsignee.Setup(m => m.Name).Returns("THISISTHECONSIGNEENAME");
		lineConsignee.Setup(m => m.Address).Returns("CONSIGNEE ADDRESS");
		lineConsignee.Setup(m => m.Postcode).Returns("45678");
		lineConsignee.Setup(m => m.City).Returns("CONSIGNEE CITY");
		lineConsignee.Setup(m => m.CountryCode).Returns("RE");
		return lineConsignee.Object;
	}

	#endregion

	#region NB Setup

	IEnumerable<INBMessageSendingObject> SetupNBMessages()
	{
		return new INBMessageSendingObject[] { SetupNBMessage1(), SetupNBMessage2() };
	}

	INBMessageSendingObject SetupNBMessage1()
	{
		var nBMessageMock = new Mock<INBMessageSendingObject>();
		nBMessageMock.Setup(m => m.Header).Returns(SetupNBHeaderMessage1());
		nBMessageMock.Setup(m => m.DataBlocks).Returns(SetupDataBlocksMessage1());
		nBMessageMock.Setup(m => m.AnnualProgressiveNumber).Returns("999998");

		return nBMessageMock.Object;
	}

	INBHeader SetupNBHeaderMessage1()
	{
		var nBMessageHeaderMock = new Mock<INBHeader>();
		nBMessageHeaderMock.Setup(m => m.MessageCodeEntry).Returns("IM");
		nBMessageHeaderMock.Setup(m => m.ReferenceNumber).Returns("REF");
		nBMessageHeaderMock.Setup(m => m.DeclarationCIN).Returns("R");
		nBMessageHeaderMock.Setup(m => m.DeclarationDate).Returns(new ZDate(2019, 07, 12));
		nBMessageHeaderMock.Setup(m => m.ItemNumber).Returns(1);

		return nBMessageHeaderMock.Object;
	}

	IEnumerable<IPreviousOperationInfo> SetupDataBlocksMessage1()
	{
		return new IPreviousOperationInfo[]
		{
			SetupOperation1(),
			SetupOperation2(),
			SetupOperation3(),
			SetupOperation4(),
			SetupOperation5(),
			SetupOperation6(),
		};
	}

	IPreviousOperationInfo SetupOperation1()
	{
		var nBPreviousOperation = new Mock<IPreviousOperationInfo>();

		var nBPreviousAllibrament = new Mock<IPreviousAdministrativeReference>();
		nBPreviousAllibrament.Setup(m => m.Register).Returns("REA1");
		nBPreviousAllibrament.Setup(m => m.ReferenceNumber).Returns("A1-REF");
		nBPreviousAllibrament.Setup(m => m.ReferenceCIN).Returns("A");
		nBPreviousAllibrament.Setup(m => m.Date).Returns(new ZDate(2019, 07, 01));
		nBPreviousAllibrament.Setup(m => m.Series).Returns("A1");
		nBPreviousAllibrament.Setup(m => m.CustomsOffice).Returns("OFF-A1");
		nBPreviousAllibrament.Setup(m => m.ItemNumber).Returns(1);
		nBPreviousOperation.Setup(m => m.PreviousAllibrament).Returns(nBPreviousAllibrament.Object);

		nBPreviousOperation.Setup(m => m.MRN).Returns("MRNA");

		var nBPreviousProcedureReference = new Mock<IPreviousAdministrativeReference>();
		nBPreviousProcedureReference.Setup(m => m.Register).Returns("REB1");
		nBPreviousProcedureReference.Setup(m => m.ReferenceNumber).Returns("B1-REF");
		nBPreviousProcedureReference.Setup(m => m.ReferenceCIN).Returns("A");
		nBPreviousProcedureReference.Setup(m => m.Date).Returns(new ZDate(2019, 07, 01));
		nBPreviousProcedureReference.Setup(m => m.Series).Returns("B1");
		nBPreviousProcedureReference.Setup(m => m.CustomsOffice).Returns("OFF-B1");
		nBPreviousProcedureReference.Setup(m => m.ItemNumber).Returns(1);
		nBPreviousOperation.Setup(m => m.PreviousProcedure).Returns(nBPreviousProcedureReference.Object);

		nBPreviousOperation.Setup(m => m.NumberOfPackages).Returns(1);
		nBPreviousOperation.Setup(m => m.GrossMass).Returns(3902.32);
		nBPreviousOperation.Setup(m => m.CombinedNomenclature).Returns("A123456789");
		nBPreviousOperation.Setup(m => m.NetMass).Returns(3343.23);
		nBPreviousOperation.Setup(m => m.SupplementaryUnit).Returns(99.23);

		return nBPreviousOperation.Object;
	}
	IPreviousOperationInfo SetupOperation2()
	{
		var nBPreviousOperation = new Mock<IPreviousOperationInfo>();

		var nBPreviousAllibrament = new Mock<IPreviousAdministrativeReference>();
		nBPreviousAllibrament.Setup(m => m.Register).Returns("REA2");
		nBPreviousAllibrament.Setup(m => m.ReferenceNumber).Returns("A2-REF");
		nBPreviousAllibrament.Setup(m => m.ReferenceCIN).Returns("B");
		nBPreviousAllibrament.Setup(m => m.Date).Returns(new ZDate(2019, 07, 02));
		nBPreviousAllibrament.Setup(m => m.Series).Returns("A2");
		nBPreviousAllibrament.Setup(m => m.CustomsOffice).Returns("OFF-A2");
		nBPreviousAllibrament.Setup(m => m.ItemNumber).Returns(2);
		nBPreviousOperation.Setup(m => m.PreviousAllibrament).Returns(nBPreviousAllibrament.Object);

		nBPreviousOperation.Setup(m => m.MRN).Returns("MRNB");

		var nBPreviousProcedureReference = new Mock<IPreviousAdministrativeReference>();
		nBPreviousProcedureReference.Setup(m => m.Register).Returns("REB2");
		nBPreviousProcedureReference.Setup(m => m.ReferenceNumber).Returns("B2-REF");
		nBPreviousProcedureReference.Setup(m => m.ReferenceCIN).Returns("B");
		nBPreviousProcedureReference.Setup(m => m.Date).Returns(new ZDate(2019, 07, 02));
		nBPreviousProcedureReference.Setup(m => m.Series).Returns("B2");
		nBPreviousProcedureReference.Setup(m => m.CustomsOffice).Returns("OFF-B2");
		nBPreviousProcedureReference.Setup(m => m.ItemNumber).Returns(2);
		nBPreviousOperation.Setup(m => m.PreviousProcedure).Returns(nBPreviousProcedureReference.Object);

		nBPreviousOperation.Setup(m => m.NumberOfPackages).Returns(2);
		nBPreviousOperation.Setup(m => m.GrossMass).Returns(2343.12);
		nBPreviousOperation.Setup(m => m.CombinedNomenclature).Returns("B123456789");
		nBPreviousOperation.Setup(m => m.NetMass).Returns(33243.96);
		nBPreviousOperation.Setup(m => m.SupplementaryUnit).Returns(44.3);

		return nBPreviousOperation.Object;
	}
	IPreviousOperationInfo SetupOperation3()
	{
		var nBPreviousOperation = new Mock<IPreviousOperationInfo>();

		var nBPreviousAllibrament = new Mock<IPreviousAdministrativeReference>();
		nBPreviousAllibrament.Setup(m => m.Register).Returns("REA3");
		nBPreviousAllibrament.Setup(m => m.ReferenceNumber).Returns("A3-REF");
		nBPreviousAllibrament.Setup(m => m.ReferenceCIN).Returns("C");
		nBPreviousAllibrament.Setup(m => m.Date).Returns(new ZDate(2019, 07, 03));
		nBPreviousAllibrament.Setup(m => m.Series).Returns("A3");
		nBPreviousAllibrament.Setup(m => m.CustomsOffice).Returns("OFF-A3");
		nBPreviousAllibrament.Setup(m => m.ItemNumber).Returns(3);
		nBPreviousOperation.Setup(m => m.PreviousAllibrament).Returns(nBPreviousAllibrament.Object);

		nBPreviousOperation.Setup(m => m.MRN).Returns("MRNC");

		var nBPreviousProcedureReference = new Mock<IPreviousAdministrativeReference>();
		nBPreviousProcedureReference.Setup(m => m.Register).Returns("REB3");
		nBPreviousProcedureReference.Setup(m => m.ReferenceNumber).Returns("B3-REF");
		nBPreviousProcedureReference.Setup(m => m.ReferenceCIN).Returns("C");
		nBPreviousProcedureReference.Setup(m => m.Date).Returns(new ZDate(2019, 07, 03));
		nBPreviousProcedureReference.Setup(m => m.Series).Returns("B3");
		nBPreviousProcedureReference.Setup(m => m.CustomsOffice).Returns("OFF-B3");
		nBPreviousProcedureReference.Setup(m => m.ItemNumber).Returns(3);
		nBPreviousOperation.Setup(m => m.PreviousProcedure).Returns(nBPreviousProcedureReference.Object);

		nBPreviousOperation.Setup(m => m.NumberOfPackages).Returns(3);
		nBPreviousOperation.Setup(m => m.GrossMass).Returns(3333.33);
		nBPreviousOperation.Setup(m => m.CombinedNomenclature).Returns("C123456789");
		nBPreviousOperation.Setup(m => m.NetMass).Returns(33333.33);
		nBPreviousOperation.Setup(m => m.SupplementaryUnit).Returns(33.3);

		return nBPreviousOperation.Object;
	}
	IPreviousOperationInfo SetupOperation4()
	{
		var nBPreviousOperation = new Mock<IPreviousOperationInfo>();

		var nBPreviousAllibrament = new Mock<IPreviousAdministrativeReference>();
		nBPreviousAllibrament.Setup(m => m.Register).Returns("REA4");
		nBPreviousAllibrament.Setup(m => m.ReferenceNumber).Returns("A4-REF");
		nBPreviousAllibrament.Setup(m => m.ReferenceCIN).Returns("D");
		nBPreviousAllibrament.Setup(m => m.Date).Returns(new ZDate(2019, 07, 04));
		nBPreviousAllibrament.Setup(m => m.Series).Returns("A4");
		nBPreviousAllibrament.Setup(m => m.CustomsOffice).Returns("OFF-A4");
		nBPreviousAllibrament.Setup(m => m.ItemNumber).Returns(4);
		nBPreviousOperation.Setup(m => m.PreviousAllibrament).Returns(nBPreviousAllibrament.Object);

		nBPreviousOperation.Setup(m => m.MRN).Returns("MRND");

		var nBPreviousProcedureReference = new Mock<IPreviousAdministrativeReference>();
		nBPreviousProcedureReference.Setup(m => m.Register).Returns("REB4");
		nBPreviousProcedureReference.Setup(m => m.ReferenceNumber).Returns("B4-REF");
		nBPreviousProcedureReference.Setup(m => m.ReferenceCIN).Returns("D");
		nBPreviousProcedureReference.Setup(m => m.Date).Returns(new ZDate(2019, 07, 04));
		nBPreviousProcedureReference.Setup(m => m.Series).Returns("B4");
		nBPreviousProcedureReference.Setup(m => m.CustomsOffice).Returns("OFF-B4");
		nBPreviousProcedureReference.Setup(m => m.ItemNumber).Returns(4);
		nBPreviousOperation.Setup(m => m.PreviousProcedure).Returns(nBPreviousProcedureReference.Object);

		nBPreviousOperation.Setup(m => m.NumberOfPackages).Returns(4);
		nBPreviousOperation.Setup(m => m.GrossMass).Returns(4444.44);
		nBPreviousOperation.Setup(m => m.CombinedNomenclature).Returns("D123456789");
		nBPreviousOperation.Setup(m => m.NetMass).Returns(44444.44);
		nBPreviousOperation.Setup(m => m.SupplementaryUnit).Returns(44.4);

		return nBPreviousOperation.Object;
	}
	IPreviousOperationInfo SetupOperation5()
	{
		var nBPreviousOperation = new Mock<IPreviousOperationInfo>();

		var nBPreviousAllibrament = new Mock<IPreviousAdministrativeReference>();
		nBPreviousAllibrament.Setup(m => m.Register).Returns("REA5");
		nBPreviousAllibrament.Setup(m => m.ReferenceNumber).Returns("A5-REF");
		nBPreviousAllibrament.Setup(m => m.ReferenceCIN).Returns("E");
		nBPreviousAllibrament.Setup(m => m.Date).Returns(new ZDate(2019, 07, 05));
		nBPreviousAllibrament.Setup(m => m.Series).Returns("A5");
		nBPreviousAllibrament.Setup(m => m.CustomsOffice).Returns("OFF-A5");
		nBPreviousAllibrament.Setup(m => m.ItemNumber).Returns(5);
		nBPreviousOperation.Setup(m => m.PreviousAllibrament).Returns(nBPreviousAllibrament.Object);

		nBPreviousOperation.Setup(m => m.MRN).Returns("MRNE");

		var nBPreviousProcedureReference = new Mock<IPreviousAdministrativeReference>();
		nBPreviousProcedureReference.Setup(m => m.Register).Returns("REB5");
		nBPreviousProcedureReference.Setup(m => m.ReferenceNumber).Returns("B5-REF");
		nBPreviousProcedureReference.Setup(m => m.ReferenceCIN).Returns("E");
		nBPreviousProcedureReference.Setup(m => m.Date).Returns(new ZDate(2019, 07, 05));
		nBPreviousProcedureReference.Setup(m => m.Series).Returns("B5");
		nBPreviousProcedureReference.Setup(m => m.CustomsOffice).Returns("OFF-B5");
		nBPreviousProcedureReference.Setup(m => m.ItemNumber).Returns(5);
		nBPreviousOperation.Setup(m => m.PreviousProcedure).Returns(nBPreviousProcedureReference.Object);

		nBPreviousOperation.Setup(m => m.NumberOfPackages).Returns(5);
		nBPreviousOperation.Setup(m => m.GrossMass).Returns(5555.55);
		nBPreviousOperation.Setup(m => m.CombinedNomenclature).Returns("E123456789");
		nBPreviousOperation.Setup(m => m.NetMass).Returns(55555.55);
		nBPreviousOperation.Setup(m => m.SupplementaryUnit).Returns(55.5);

		return nBPreviousOperation.Object;
	}
	IPreviousOperationInfo SetupOperation6()
	{
		var nBPreviousOperation = new Mock<IPreviousOperationInfo>();

		var nBPreviousAllibrament = new Mock<IPreviousAdministrativeReference>();
		nBPreviousAllibrament.Setup(m => m.Register).Returns("REA6");
		nBPreviousAllibrament.Setup(m => m.ReferenceNumber).Returns("A6-REF");
		nBPreviousAllibrament.Setup(m => m.ReferenceCIN).Returns("F");
		nBPreviousAllibrament.Setup(m => m.Date).Returns(new ZDate(2019, 07, 06));
		nBPreviousAllibrament.Setup(m => m.Series).Returns("A6");
		nBPreviousAllibrament.Setup(m => m.CustomsOffice).Returns("OFF-A6");
		nBPreviousAllibrament.Setup(m => m.ItemNumber).Returns(6);
		nBPreviousOperation.Setup(m => m.PreviousAllibrament).Returns(nBPreviousAllibrament.Object);

		nBPreviousOperation.Setup(m => m.MRN).Returns("MRNF");

		var nBPreviousProcedureReference = new Mock<IPreviousAdministrativeReference>();
		nBPreviousProcedureReference.Setup(m => m.Register).Returns("REB6");
		nBPreviousProcedureReference.Setup(m => m.ReferenceNumber).Returns("B6-REF");
		nBPreviousProcedureReference.Setup(m => m.ReferenceCIN).Returns("F");
		nBPreviousProcedureReference.Setup(m => m.Date).Returns(new ZDate(2019, 07, 06));
		nBPreviousProcedureReference.Setup(m => m.Series).Returns("B6");
		nBPreviousProcedureReference.Setup(m => m.CustomsOffice).Returns("OFF-B6");
		nBPreviousProcedureReference.Setup(m => m.ItemNumber).Returns(6);
		nBPreviousOperation.Setup(m => m.PreviousProcedure).Returns(nBPreviousProcedureReference.Object);

		nBPreviousOperation.Setup(m => m.NumberOfPackages).Returns(6);
		nBPreviousOperation.Setup(m => m.GrossMass).Returns(6666.66);
		nBPreviousOperation.Setup(m => m.CombinedNomenclature).Returns("F123456789");
		nBPreviousOperation.Setup(m => m.NetMass).Returns(66666.66);
		nBPreviousOperation.Setup(m => m.SupplementaryUnit).Returns(66.6);

		return nBPreviousOperation.Object;
	}

	INBMessageSendingObject SetupNBMessage2()
	{
		var nBMessageMock = new Mock<INBMessageSendingObject>();
		nBMessageMock.Setup(m => m.Header).Returns(SetupNBHeaderMessage2());
		nBMessageMock.Setup(m => m.DataBlocks).Returns(SetupDataBlocksMessage2());
		nBMessageMock.Setup(m => m.AnnualProgressiveNumber).Returns("999999");

		return nBMessageMock.Object;
	}

	INBHeader SetupNBHeaderMessage2()
	{
		var nBMessageHeaderMock = new Mock<INBHeader>();
		nBMessageHeaderMock.Setup(m => m.MessageCodeEntry).Returns("IM");
		nBMessageHeaderMock.Setup(m => m.ReferenceNumber).Returns("REF");
		nBMessageHeaderMock.Setup(m => m.DeclarationCIN).Returns("R");
		nBMessageHeaderMock.Setup(m => m.DeclarationDate).Returns(new ZDate(2019, 07, 12));
		nBMessageHeaderMock.Setup(m => m.ItemNumber).Returns(2);

		return nBMessageHeaderMock.Object;
	}

	IEnumerable<IPreviousOperationInfo> SetupDataBlocksMessage2()
	{
		return new IPreviousOperationInfo[]
		{
			SetupOperation1(),
		};
	}

	#endregion

	TabbedFlatFileMessageSerializer flatFileMessageSerializer;
	ETMessage etMessage;

	protected override Type MessageType => typeof(ETMessage);
}
