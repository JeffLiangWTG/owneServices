using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.MessageBuilders;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMMessageSerializerTest : BaseMessageSerializerTest
{
	public void TestFixedPart()
	{
		AssertContains("TIM           01234500", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestHeader()
	{
		AssertContains("TIM           01234500012345	A	0123456	01	02072019				1	AAA	B	01234	02072019		1		CC	CONSIGNORID	THISISTHECONSIGNORNAME	CONSIGNOR ADDRESS	11111	CONSIGNOR CITY	AA		DD	CONSIGNEEID	THISISTHECONSIGNEENAME	CONSIGNEE ADDRESS	22222	CONSIGNEE CITY	FF	345.98	3	DD	DECLARANTTRADER	DECLARANTTRADERNAME	DECLARANT TRADER ADDRESS	3333	DECLARANT TRADER CITY	GG	KK	LL	RM	CN	TRANSPORT ON ARRIVAL	1	INC	COMPLEMENTOFINFO	1	NN	TRANSPORT ON BORDER	EUR	19876.98	1.12345	12	1	2		IT	012345	NAME OF ENTRY OFFICE	LOCOFGOODS1	PLCUNLD2	012345	A		W	IDWAREHOUSE	Z	IT		10072019																		", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestCompanyRegister()
	{
		AssertContains("0123456	01	02072019	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestDeclaration()
	{
		AssertContains("AAA	B	01234	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestConsignor()
	{
		AssertContains("CC	CONSIGNORID	THISISTHECONSIGNORNAME	CONSIGNOR ADDRESS	11111	CONSIGNOR CITY	AA	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestConsignee()
	{
		AssertContains("DD	CONSIGNEEID	THISISTHECONSIGNEENAME	CONSIGNEE ADDRESS	22222	CONSIGNEE CITY	FF	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestDeclarantTrader()
	{
		AssertContains("3	DD	DECLARANTTRADER	DECLARANTTRADERNAME	DECLARANT TRADER ADDRESS	3333	DECLARANT TRADER CITY	GG	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestMeansOfTransportOnArrival()
	{
		AssertContains("CN	TRANSPORT ON ARRIVAL	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestTermsOfDelivery()
	{
		AssertContains("INC	COMPLEMENTOFINFO	1	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestMeansOfTransportCrossingBorder()
	{
		AssertContains("NN	TRANSPORT ON BORDER	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestTransactionData()
	{
		AssertContains("EUR	19876.98	1.12345	12	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestCustomsEntryOffice()
	{
		AssertContains("IT	012345	NAME OF ENTRY OFFICE	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestLocationOfGoods()
	{
		AssertContains("LOCOFGOODS1	PLCUNLD2	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestDeferredPayment()
	{
		AssertContains("012345	A	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestWarehouseIdentification()
	{
		AssertContains("W	IDWAREHOUSE	Z	IT	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestLine()
	{
		AssertContains("?IM1          01234500																	3	2	ABCD012345A		BCDE123456B		GOODS DESCRIPTION	MARKS AND NUMBERS	CT	4			1	0123456789	2	A000	B111	CN	4444.1111	123	4000	2	50	60	4222.3829	4	1	2	3	4	A	ABC	REG1	01234567	Z	11072019	A1	IT123456	1	MRN0123456789ABCDE	COMPLEMENT OF INFORMATION		7832.32	1	FIRSTEORI	SECONDEORI	1000.34	0123456789	563.89	99.23	B111	01234567	X	11072019	MK	IT012345	3	1	4	TYPE0	IT	2018	REF1	100.1	KGM	1	1	TYPE1	IT	2019	REF2	200.2	KGM	0	0	TYPE3	IT	2019	REF3	999	KGM	0	0	TYPE4	IT	2019	REF4	1.12345	KGM	0	0			NOTES	1000.46	3000.90					2	A00	150.89	M	0.089000	Z	0.780000	L	0.830000	P	789.00	G	405	33.00	O	0.074000	I	0.550000	U	0.330000	Y	234.56	T	4039.00	83748.00	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestLineFixedPart()
	{
		AssertContains("?IM1          01234500", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestPackage()
	{
		AssertContains("MARKS AND NUMBERS	CT	4	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestContainers()
	{
		AssertContains("2	ABCD012345A		BCDE123456B		", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestAdditionalCodes()
	{
		AssertContains("2	A000	B111	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestNationalProcedures()
	{
		AssertContains("2	50	60	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestQuotas()
	{
		AssertContains("4	1	2	3	4	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestPreviousAdministrativeDocument()
	{
		AssertContains("A	ABC	REG1	01234567	Z	11072019	A1	IT123456	1	MRN0123456789ABCDE	COMPLEMENT OF INFORMATION	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestSpecialMentionsEori()
	{
		AssertContains("FIRSTEORI	SECONDEORI	1000.34	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestSpecialMentionsUnloadingData()
	{
		AssertContains("0123456789	563.89	99.23	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestSpecialMentionsPreviousProcedure()
	{
		AssertContains("B111	01234567	X	11072019	MK	IT012345	3	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestSpecialMentions()
	{
		AssertContains("FIRSTEORI	SECONDEORI	1000.34	0123456789	563.89	99.23	B111	01234567	X	11072019	MK	IT012345	3	1	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestCertificates()
	{
		AssertContains("4	TYPE0	IT	2018	REF1	100.1	KGM	1	1	TYPE1	IT	2019	REF2	200.2	KGM	0	0	TYPE3	IT	2019	REF3	999	KGM	0	0	TYPE4	IT	2019	REF4	1.12345	KGM	0	0	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestDuties()
	{
		AssertContains("2	A00	150.89	M	0.089000	Z	0.780000	L	0.830000	P	789.00	G	405	33.00	O	0.074000	I	0.550000	U	0.330000	Y	234.56	T	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestMessageNB1()
	{
		AssertContains("TNB           99999801IM	REF	R	120719	1	REA1	A1-REF	A	010719	A1	1	OFF-A1	MRNA	REB1	B1-REF	A	010719	B1	1	OFF-B1	1	3902.32	A123456789	3343.23	99.23	REA2	A2-REF	B	020719	A2	2	OFF-A2	MRNB	REB2	B2-REF	B	020719	B2	2	OFF-B2	2	2343.12	B123456789	33243.96	44.3	REA3	A3-REF	C	030719	A3	3	OFF-A3	MRNC	REB3	B3-REF	C	030719	B3	3	OFF-B3	3	3333.33	C123456789	33333.33	33.3	REA4	A4-REF	D	040719	A4	4	OFF-A4	MRND	REB4	B4-REF	D	040719	B4	4	OFF-B4	4	4444.44	D123456789	44444.44	44.4	1	", flatFileMessageSerializer.Serialize(iMMessage));
		AssertContains("?NB1          99999801REA5	A5-REF	E	050719	A5	5	OFF-A5	MRNE	REB5	B5-REF	E	050719	B5	5	OFF-B5	5	5555.55	E123456789	55555.55	55.5	REA6	A6-REF	F	060719	A6	6	OFF-A6	MRNF	REB6	B6-REF	F	060719	B6	6	OFF-B6	6	6666.66	F123456789	66666.66	66.6																																									0	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	public void TestMessageNB2()
	{
		AssertContains("TNB           99999902IM	REF	R	120719	2	REA1	A1-REF	A	010719	A1	1	OFF-A1	MRNA	REB1	B1-REF	A	010719	B1	1	OFF-B1	1	3902.32	A123456789	3343.23	99.23																																																													0	", flatFileMessageSerializer.Serialize(iMMessage));
	}

	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestCompleteMessage()
	{
		AssertASCIIFileSameAsString(BaseSourcePath + Messaging.Testing.TestConstants.ProjectRelativePath + @"TestFiles\TestIM_NB.txt", flatFileMessageSerializer.Serialize(iMMessage));
	}

	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestCompleteMessageWithFallbackProcedure()
	{
		AssertASCIIFileSameAsString(BaseSourcePath + Messaging.Testing.TestConstants.ProjectRelativePath + @"TestFiles\TestIM_NB_WithFallbackProcedure.txt", flatFileMessageSerializer.Serialize(iMMessageWithFallbackProcedure));
	}

	protected override void SetUp()
	{
		base.SetUp();

		var iMMessageMock = new Mock<IIMMessageSendingObject>();
		iMMessageMock.Setup(m => m.MessageHeader).Returns(SetupHeader());
		iMMessageMock.Setup(m => m.MessageLines).Returns(SetupLines());
		iMMessageMock.Setup(m => m.NBMessages).Returns(SetupNBMessages());
		iMMessageMock.Setup(m => m.FallbackProcedure).Returns(false);
		iMMessageMock.Setup(m => m.DeclarantTaxNumber).Returns("");
		iMMessage = new IMMessage(iMMessageMock.Object);

		var iMMessageMockWithFallbackProcedure = new Mock<IIMMessageSendingObject>();
		iMMessageMockWithFallbackProcedure.Setup(m => m.MessageHeader).Returns(SetupHeader());
		iMMessageMockWithFallbackProcedure.Setup(m => m.MessageLines).Returns(SetupLines());
		iMMessageMockWithFallbackProcedure.Setup(m => m.NBMessages).Returns(SetupNBMessages());
		iMMessageMockWithFallbackProcedure.Setup(m => m.FallbackProcedure).Returns(true);
		iMMessageMockWithFallbackProcedure.Setup(m => m.DeclarantTaxNumber).Returns("DEC TAX NO");
		iMMessageWithFallbackProcedure = new IMMessage(iMMessageMockWithFallbackProcedure.Object);

		flatFileMessageSerializer = new TabbedFlatFileMessageSerializer();
	}

	#region IM Line

	IEnumerable<IIMLine> SetupLines()
	{
		var iMMessageLineMock = new Mock<IIMLine>();
		iMMessageLineMock.Setup(m => m.Package).Returns(SetupPackage());
		iMMessageLineMock.Setup(m => m.Containers).Returns(SetupContainers());
		iMMessageLineMock.Setup(m => m.GoodsDescription).Returns("GOODS DESCRIPTION");
		iMMessageLineMock.Setup(m => m.ItemNumber).Returns(1);
		iMMessageLineMock.Setup(m => m.CombinedNomenclature).Returns("0123456789");
		iMMessageLineMock.Setup(m => m.AdditionalCodes).Returns(SetupAdditionalCodes());
		iMMessageLineMock.Setup(m => m.CountryOfOrigin).Returns("CN");
		iMMessageLineMock.Setup(m => m.GrossMass).Returns(4444.1111);
		iMMessageLineMock.Setup(m => m.Preferences).Returns("123");
		iMMessageLineMock.Setup(m => m.Procedure).Returns("4000");
		iMMessageLineMock.Setup(m => m.NationalProcedures).Returns(SetupNationalProcedure());
		iMMessageLineMock.Setup(m => m.NetMass).Returns(4222.3829);
		iMMessageLineMock.Setup(m => m.Quotas).Returns(SetupQuotas());
		iMMessageLineMock.Setup(m => m.PreviousAdministrativeDocument).Returns(SetupPreviousAdministrativeDocument());
		iMMessageLineMock.Setup(m => m.ItemPriceEuro).Returns(7832.32);
		iMMessageLineMock.Setup(m => m.EvaluationMethod).Returns("1");
		iMMessageLineMock.Setup(m => m.SpecialMentionGroup).Returns(SetupSpecialMentions());
		iMMessageLineMock.Setup(m => m.Certificates).Returns(SetupCertificates());
		iMMessageLineMock.Setup(m => m.Notes).Returns("NOTES");
		iMMessageLineMock.Setup(m => m.AdjustmentInEuro).Returns(1000.46);
		iMMessageLineMock.Setup(m => m.StatisticalValueAmount).Returns(3000.90);
		iMMessageLineMock.Setup(m => m.Duties).Returns(SetupDuties());
		iMMessageLineMock.Setup(m => m.TotalItemTaxedAmount).Returns(4039);
		iMMessageLineMock.Setup(m => m.GrandTotalTaxedAmount).Returns(83748);

		return new IIMLine[] { iMMessageLineMock.Object };
	}

	IEnumerable<IDutyTaxFee> SetupDuties()
	{
		var iMLineDutyMock1 = new Mock<IDutyTaxFee>();
		iMLineDutyMock1.Setup(m => m.Type).Returns("A00");
		iMLineDutyMock1.Setup(m => m.Base).Returns(150.89);
		iMLineDutyMock1.Setup(m => m.CalculationFactor1).Returns("M");
		iMLineDutyMock1.Setup(m => m.Rate1).Returns(0.089);
		iMLineDutyMock1.Setup(m => m.CalculationFactor2).Returns("Z");
		iMLineDutyMock1.Setup(m => m.Rate2).Returns(0.78);
		iMLineDutyMock1.Setup(m => m.CalculationFactor3).Returns("L");
		iMLineDutyMock1.Setup(m => m.Rate3).Returns(0.83);
		iMLineDutyMock1.Setup(m => m.CalculationFactor4).Returns("P");
		iMLineDutyMock1.Setup(m => m.Amount).Returns(789);
		iMLineDutyMock1.Setup(m => m.MethodOfPayment).Returns("G");

		var iMLineDutyMock2 = new Mock<IDutyTaxFee>();
		iMLineDutyMock2.Setup(m => m.Type).Returns("405");
		iMLineDutyMock2.Setup(m => m.Base).Returns(33);
		iMLineDutyMock2.Setup(m => m.CalculationFactor1).Returns("O");
		iMLineDutyMock2.Setup(m => m.Rate1).Returns(0.074);
		iMLineDutyMock2.Setup(m => m.CalculationFactor2).Returns("I");
		iMLineDutyMock2.Setup(m => m.Rate2).Returns(0.55);
		iMLineDutyMock2.Setup(m => m.CalculationFactor3).Returns("U");
		iMLineDutyMock2.Setup(m => m.Rate3).Returns(0.33);
		iMLineDutyMock2.Setup(m => m.CalculationFactor4).Returns("Y");
		iMLineDutyMock2.Setup(m => m.Amount).Returns(234.56);
		iMLineDutyMock2.Setup(m => m.MethodOfPayment).Returns("T");

		return new IDutyTaxFee[] { iMLineDutyMock1.Object, iMLineDutyMock2.Object };
	}

	IEnumerable<ICertificate> SetupCertificates()
	{
		var iMLineCertificate1 = new Mock<ICertificate>();
		iMLineCertificate1.Setup(m => m.DocumentType).Returns("TYPE0");
		iMLineCertificate1.Setup(m => m.CountryOfIssue).Returns("IT");
		iMLineCertificate1.Setup(m => m.IssuingYear).Returns("2018");
		iMLineCertificate1.Setup(m => m.Reference).Returns("REF1");
		iMLineCertificate1.Setup(m => m.Quantity).Returns(100.10m);
		iMLineCertificate1.Setup(m => m.UnitOfMeasurement).Returns("KGM");
		iMLineCertificate1.Setup(m => m.DerogationFlag).Returns(ZBool.True);
		iMLineCertificate1.Setup(m => m.RetrospectiveDerogationFlag).Returns(ZBool.True);

		var iMLineCertificate2 = new Mock<ICertificate>();
		iMLineCertificate2.Setup(m => m.DocumentType).Returns("TYPE1");
		iMLineCertificate2.Setup(m => m.CountryOfIssue).Returns("IT");
		iMLineCertificate2.Setup(m => m.IssuingYear).Returns("2019");
		iMLineCertificate2.Setup(m => m.Reference).Returns("REF2");
		iMLineCertificate2.Setup(m => m.Quantity).Returns(200.20m);
		iMLineCertificate2.Setup(m => m.UnitOfMeasurement).Returns("KGM");
		iMLineCertificate2.Setup(m => m.DerogationFlag).Returns(ZBool.False);
		iMLineCertificate2.Setup(m => m.RetrospectiveDerogationFlag).Returns(ZBool.False);

		var iMLineCertificate3 = new Mock<ICertificate>();
		iMLineCertificate3.Setup(m => m.DocumentType).Returns("TYPE3");
		iMLineCertificate3.Setup(m => m.CountryOfIssue).Returns("IT");
		iMLineCertificate3.Setup(m => m.IssuingYear).Returns("2019");
		iMLineCertificate3.Setup(m => m.Reference).Returns("REF3");
		iMLineCertificate3.Setup(m => m.Quantity).Returns(999m);
		iMLineCertificate3.Setup(m => m.UnitOfMeasurement).Returns("KGM");
		iMLineCertificate3.Setup(m => m.DerogationFlag).Returns(ZBool.False);
		iMLineCertificate3.Setup(m => m.RetrospectiveDerogationFlag).Returns(ZBool.False);

		var iMLineCertificate4 = new Mock<ICertificate>();
		iMLineCertificate4.Setup(m => m.DocumentType).Returns("TYPE4");
		iMLineCertificate4.Setup(m => m.CountryOfIssue).Returns("IT");
		iMLineCertificate4.Setup(m => m.IssuingYear).Returns("2019");
		iMLineCertificate4.Setup(m => m.Reference).Returns("REF4");
		iMLineCertificate4.Setup(m => m.Quantity).Returns(1.123451m);
		iMLineCertificate4.Setup(m => m.UnitOfMeasurement).Returns("KGM");
		iMLineCertificate4.Setup(m => m.DerogationFlag).Returns(ZBool.False);
		iMLineCertificate4.Setup(m => m.RetrospectiveDerogationFlag).Returns(ZBool.False);

		return new ICertificate[] { iMLineCertificate1.Object, iMLineCertificate2.Object, iMLineCertificate3.Object, iMLineCertificate4.Object };
	}

	IIMLineSpecialMentionGroup SetupSpecialMentions()
	{
		var iMLineSpecialMentionsMock = new Mock<IIMLineSpecialMentionGroup>();
		iMLineSpecialMentionsMock.Setup(m => m.Eori).Returns(SetupSpecialMentionsEori());
		iMLineSpecialMentionsMock.Setup(m => m.UnloadingData).Returns(SetupSpecialMentionsUnloadingData());
		iMLineSpecialMentionsMock.Setup(m => m.PreviousProcedure).Returns(SetupSpecialMentionsPreviousProcedure());
		iMLineSpecialMentionsMock.Setup(m => m.SteelType).Returns("1");

		return iMLineSpecialMentionsMock.Object;
	}

	IPreviousAdministrativeReference SetupSpecialMentionsPreviousProcedure()
	{
		var iMLineSpecialMentionsPreviousProcedure = new Mock<IPreviousAdministrativeReference>();
		iMLineSpecialMentionsPreviousProcedure.Setup(m => m.Register).Returns("B111");
		iMLineSpecialMentionsPreviousProcedure.Setup(m => m.ReferenceNumber).Returns("01234567");
		iMLineSpecialMentionsPreviousProcedure.Setup(m => m.ReferenceCIN).Returns("X");
		iMLineSpecialMentionsPreviousProcedure.Setup(m => m.Date).Returns(new ZDate(2019, 07, 11));
		iMLineSpecialMentionsPreviousProcedure.Setup(m => m.Series).Returns("MK");
		iMLineSpecialMentionsPreviousProcedure.Setup(m => m.CustomsOffice).Returns("IT012345");
		iMLineSpecialMentionsPreviousProcedure.Setup(m => m.ItemNumber).Returns(3);

		return iMLineSpecialMentionsPreviousProcedure.Object;
	}

	ISpecialMentionUnloadingDataInfo SetupSpecialMentionsUnloadingData()
	{
		var iMLineSpecialMentionsUnloadingDataMock = new Mock<ISpecialMentionUnloadingDataInfo>();
		iMLineSpecialMentionsUnloadingDataMock.Setup(m => m.CommodityCode).Returns("0123456789");
		iMLineSpecialMentionsUnloadingDataMock.Setup(m => m.Quantity).Returns(563.89);
		iMLineSpecialMentionsUnloadingDataMock.Setup(m => m.SupplementaryUnit).Returns(99.23);

		return iMLineSpecialMentionsUnloadingDataMock.Object;
	}

	ISpecialMentionEoriInfo SetupSpecialMentionsEori()
	{
		var iMLineSpecialMentionsEoriMock = new Mock<ISpecialMentionEoriInfo>();
		iMLineSpecialMentionsEoriMock.Setup(m => m.FirstEoriCode).Returns("FIRSTEORI");
		iMLineSpecialMentionsEoriMock.Setup(m => m.SecondEoriCode).Returns("SECONDEORI");
		iMLineSpecialMentionsEoriMock.Setup(m => m.PreviousInvoiceAmount).Returns(1000.34);

		return iMLineSpecialMentionsEoriMock.Object;
	}

	IPreviousDocument SetupPreviousAdministrativeDocument()
	{
		var iMLinePreviousAdministrativeDocumentMock = new Mock<IPreviousDocument>();
		iMLinePreviousAdministrativeDocumentMock.Setup(m => m.DocType).Returns("A");
		iMLinePreviousAdministrativeDocumentMock.Setup(m => m.Category).Returns("ABC");
		iMLinePreviousAdministrativeDocumentMock.Setup(m => m.Register).Returns("REG1");
		iMLinePreviousAdministrativeDocumentMock.Setup(m => m.ReferenceNumber).Returns("01234567");
		iMLinePreviousAdministrativeDocumentMock.Setup(m => m.ReferenceCIN).Returns("Z");
		iMLinePreviousAdministrativeDocumentMock.Setup(m => m.Date).Returns(new ZDate(2019, 7, 11));
		iMLinePreviousAdministrativeDocumentMock.Setup(m => m.Series).Returns("A1");
		iMLinePreviousAdministrativeDocumentMock.Setup(m => m.CustomsOffice).Returns("IT123456");
		iMLinePreviousAdministrativeDocumentMock.Setup(m => m.ItemNumber).Returns(1);
		iMLinePreviousAdministrativeDocumentMock.Setup(m => m.Mrn).Returns("MRN0123456789ABCDE");
		iMLinePreviousAdministrativeDocumentMock.Setup(m => m.ComplementOfInformation).Returns("COMPLEMENT OF INFORMATION");

		return iMLinePreviousAdministrativeDocumentMock.Object;
	}

	IEnumerable<ZString> SetupQuotas() => new ZString[] { "1", "2", "3", "4" };
	IEnumerable<ZString> SetupNationalProcedure() => new ZString[] { "50", "60" };
	IEnumerable<ZString> SetupAdditionalCodes() => new ZString[] { "A000", "B111", };
	IEnumerable<ZString> SetupContainers() => new ZString[] { "ABCD012345A", "BCDE123456B" };

	IPackage SetupPackage()
	{
		var iMLinePackageMock = new Mock<IPackage>();
		iMLinePackageMock.Setup(m => m.NumberOfPacks).Returns(3);
		iMLinePackageMock.Setup(m => m.PackageType).Returns("CT");
		iMLinePackageMock.Setup(m => m.NumberOfPieces).Returns(4);
		iMLinePackageMock.Setup(m => m.MarksAndNumbers).Returns("MARKS AND NUMBERS");

		return iMLinePackageMock.Object;
	}

	#endregion

	#region IM Header

	IIMHeader SetupHeader()
	{
		var iMMessageHeaderMock = new Mock<IIMHeader>();
		iMMessageHeaderMock.Setup(m => m.AnnualProgressiveNumber).Returns("012345");
		iMMessageHeaderMock.Setup(m => m.AuthorizationNo).Returns("012345");
		iMMessageHeaderMock.Setup(m => m.AuthorizationCIN).Returns("A");
		iMMessageHeaderMock.Setup(m => m.CompanyRegister).Returns(SetupCompanyRegister());
		iMMessageHeaderMock.Setup(m => m.PreClearing).Returns(ZBool.True);
		iMMessageHeaderMock.Setup(m => m.Declaration).Returns(SetupDeclaration());
		iMMessageHeaderMock.Setup(m => m.AcceptanceDate).Returns(new ZDate(2019, 07, 02));
		iMMessageHeaderMock.Setup(m => m.TotalItems).Returns(1);
		iMMessageHeaderMock.Setup(m => m.Consignor).Returns(SetupConsignor());
		iMMessageHeaderMock.Setup(m => m.Consignee).Returns(SetupConsignee());
		iMMessageHeaderMock.Setup(m => m.DeliveryCosts).Returns(345.98m);
		iMMessageHeaderMock.Setup(m => m.DeclarantTrader).Returns(SetupDeclarantTrader());
		iMMessageHeaderMock.Setup(m => m.CountryOfDispatch).Returns("KK");
		iMMessageHeaderMock.Setup(m => m.CountryOfDestination).Returns("LL");
		iMMessageHeaderMock.Setup(m => m.ProvinceOfDestination).Returns("RM");
		iMMessageHeaderMock.Setup(m => m.MeansOfTransportOnArrival).Returns(SetupMeansOfTransportOnArrival());
		iMMessageHeaderMock.Setup(m => m.IsContainerizedTransport).Returns(ZBool.True);
		iMMessageHeaderMock.Setup(m => m.TermsOfDelivery).Returns(SetupTermsOfDelivery());
		iMMessageHeaderMock.Setup(m => m.MeansOfTransportCrossingBorder).Returns(SetupMeansOfTransportCrossingBorder());
		iMMessageHeaderMock.Setup(m => m.TransactionData).Returns(SetupTransactionData());
		iMMessageHeaderMock.Setup(m => m.TransportModeAtBorder).Returns("1");
		iMMessageHeaderMock.Setup(m => m.InlandTransportMode).Returns("2");
		iMMessageHeaderMock.Setup(m => m.EntryCustomsOffice).Returns(SetupCustomsEntryOffice());
		iMMessageHeaderMock.Setup(m => m.LocationOfGoods).Returns(SetupLocationOfGoods());
		iMMessageHeaderMock.Setup(m => m.DeferredPayment).Returns(SetupDeferredPayment());
		iMMessageHeaderMock.Setup(m => m.WarehouseIdentification).Returns(SetupWarehouseIdentification());
		iMMessageHeaderMock.Setup(m => m.DateLimitOfTemporaryOperation).Returns(new ZDate(2019, 07, 10));

		return iMMessageHeaderMock.Object;
	}

	IWarehouseIdentification SetupWarehouseIdentification()
	{
		var iMMessageHeaderWarehouseIdentification = new Mock<IWarehouseIdentification>();
		iMMessageHeaderWarehouseIdentification.Setup(m => m.Type).Returns("W");
		iMMessageHeaderWarehouseIdentification.Setup(m => m.Identification).Returns("IDWAREHOUSE");
		iMMessageHeaderWarehouseIdentification.Setup(m => m.CinIdentification).Returns("Z");
		iMMessageHeaderWarehouseIdentification.Setup(m => m.AuthorizingCountry).Returns("IT");

		return iMMessageHeaderWarehouseIdentification.Object;
	}

	IDeferredPayment SetupDeferredPayment()
	{
		var iMMessageHeaderDeferredPayment = new Mock<IDeferredPayment>();
		iMMessageHeaderDeferredPayment.Setup(m => m.AuthorizationReference).Returns("012345");
		iMMessageHeaderDeferredPayment.Setup(m => m.CinOfAuthorizationReference).Returns("A");

		return iMMessageHeaderDeferredPayment.Object;
	}

	IIMHeaderLocationOfGoods SetupLocationOfGoods()
	{
		var iMMessageHeaderLocationOfGoods = new Mock<IIMHeaderLocationOfGoods>();
		iMMessageHeaderLocationOfGoods.Setup(m => m.CodeAndCinPlaceOfExamination).Returns("LOCOFGOODS1");
		iMMessageHeaderLocationOfGoods.Setup(m => m.CodeAndCinPlaceOfUnloading).Returns("PLCUNLD2");
		return iMMessageHeaderLocationOfGoods.Object;
	}

	IIMHeaderEntryCustomsOffice SetupCustomsEntryOffice()
	{
		var iMMessageHeaderEntryCustomsOffice = new Mock<IIMHeaderEntryCustomsOffice>();
		iMMessageHeaderEntryCustomsOffice.Setup(m => m.Nationality).Returns("IT");
		iMMessageHeaderEntryCustomsOffice.Setup(m => m.ReferenceNumber).Returns("012345");
		iMMessageHeaderEntryCustomsOffice.Setup(m => m.Name).Returns("NAME OF ENTRY OFFICE");
		return iMMessageHeaderEntryCustomsOffice.Object;
	}

	ITransactionData SetupTransactionData()
	{
		var iMMessageHeaderTransactionDataMock = new Mock<ITransactionData>();
		iMMessageHeaderTransactionDataMock.Setup(m => m.CurrencyCode).Returns("EUR");
		iMMessageHeaderTransactionDataMock.Setup(m => m.TotalAmountInvoiced).Returns(19876.98m);
		iMMessageHeaderTransactionDataMock.Setup(m => m.ExchangeRate).Returns(1.12345m);
		iMMessageHeaderTransactionDataMock.Setup(m => m.NatureOfTransactionCode).Returns("12");

		return iMMessageHeaderTransactionDataMock.Object;
	}

	IMeansOfTransport SetupMeansOfTransportCrossingBorder()
	{
		var iMMessageHeaderMeansOfTransportCrossingBorderMock = new Mock<IMeansOfTransport>();
		iMMessageHeaderMeansOfTransportCrossingBorderMock.Setup(m => m.Nationality).Returns("NN");
		iMMessageHeaderMeansOfTransportCrossingBorderMock.Setup(m => m.Identity).Returns("TRANSPORT ON BORDER");

		return iMMessageHeaderMeansOfTransportCrossingBorderMock.Object;
	}

	ITermOfDeliveryGroup SetupTermsOfDelivery()
	{
		var iMMessageHeaderTermsOfDeliveryMock = new Mock<ITermOfDeliveryGroup>();
		iMMessageHeaderTermsOfDeliveryMock.Setup(m => m.IncotermCode).Returns("INC");
		iMMessageHeaderTermsOfDeliveryMock.Setup(m => m.ComplementOfInfo).Returns("COMPLEMENTOFINFO");
		iMMessageHeaderTermsOfDeliveryMock.Setup(m => m.ComplementaryCode).Returns("1");

		return iMMessageHeaderTermsOfDeliveryMock.Object;
	}

	IMeansOfTransport SetupMeansOfTransportOnArrival()
	{
		var imHeaderMeansOfTransportOnArrival = new Mock<IMeansOfTransport>();
		imHeaderMeansOfTransportOnArrival.Setup(m => m.Nationality).Returns("CN");
		imHeaderMeansOfTransportOnArrival.Setup(m => m.Identity).Returns("TRANSPORT ON ARRIVAL");

		return imHeaderMeansOfTransportOnArrival.Object;
	}

	IDeclarantTrader SetupDeclarantTrader()
	{
		var iMHeaderDeclarantTrader = new Mock<IDeclarantTrader>();
		iMHeaderDeclarantTrader.Setup(m => m.RepresentativeType).Returns("3");
		iMHeaderDeclarantTrader.Setup(m => m.IdCountryCode).Returns("DD");
		iMHeaderDeclarantTrader.Setup(m => m.ID).Returns("DECLARANTTRADER");
		iMHeaderDeclarantTrader.Setup(m => m.Name).Returns("DECLARANTTRADERNAME");
		iMHeaderDeclarantTrader.Setup(m => m.Address).Returns("DECLARANT TRADER ADDRESS");
		iMHeaderDeclarantTrader.Setup(m => m.Postcode).Returns("3333");
		iMHeaderDeclarantTrader.Setup(m => m.City).Returns("DECLARANT TRADER CITY");
		iMHeaderDeclarantTrader.Setup(m => m.CountryCode).Returns("GG");

		return iMHeaderDeclarantTrader.Object;
	}

	ITrader SetupConsignee()
	{
		var iMHeaderConsignee = new Mock<ITrader>();
		iMHeaderConsignee.Setup(m => m.IdCountryCode).Returns("DD");
		iMHeaderConsignee.Setup(m => m.ID).Returns("CONSIGNEEID");
		iMHeaderConsignee.Setup(m => m.Name).Returns("THISISTHECONSIGNEENAME");
		iMHeaderConsignee.Setup(m => m.Address).Returns("CONSIGNEE ADDRESS");
		iMHeaderConsignee.Setup(m => m.Postcode).Returns("22222");
		iMHeaderConsignee.Setup(m => m.City).Returns("CONSIGNEE CITY");
		iMHeaderConsignee.Setup(m => m.CountryCode).Returns("FF");

		return iMHeaderConsignee.Object;
	}

	ITrader SetupConsignor()
	{
		var iMHeaderConsignor = new Mock<ITrader>();
		iMHeaderConsignor.Setup(m => m.IdCountryCode).Returns("CC");
		iMHeaderConsignor.Setup(m => m.ID).Returns("CONSIGNORID");
		iMHeaderConsignor.Setup(m => m.Name).Returns("THISISTHECONSIGNORNAME");
		iMHeaderConsignor.Setup(m => m.Address).Returns("CONSIGNOR ADDRESS");
		iMHeaderConsignor.Setup(m => m.Postcode).Returns("11111");
		iMHeaderConsignor.Setup(m => m.City).Returns("CONSIGNOR CITY");
		iMHeaderConsignor.Setup(m => m.CountryCode).Returns("AA");

		return iMHeaderConsignor.Object;
	}

	IDeclaration SetupDeclaration()
	{
		var iMHeaderDeclaration = new Mock<IDeclaration>();
		iMHeaderDeclaration.Setup(m => m.TypeDeclarationSubType1).Returns("AAA");
		iMHeaderDeclaration.Setup(m => m.TypeDeclarationSubType2).Returns("B");
		iMHeaderDeclaration.Setup(m => m.TypeDeclarationSubType3).Returns("01234");

		return iMHeaderDeclaration.Object;
	}

	IIMHeaderCompanyRegister SetupCompanyRegister()
	{
		var iMHeaderCompanyRegisterMock = new Mock<IIMHeaderCompanyRegister>();
		iMHeaderCompanyRegisterMock.Setup(m => m.Number).Returns("0123456");
		iMHeaderCompanyRegisterMock.Setup(m => m.Series).Returns("01");
		iMHeaderCompanyRegisterMock.Setup(m => m.Date).Returns(new ZDate(2019, 07, 02));

		return iMHeaderCompanyRegisterMock.Object;
	}

	#endregion

	#region Message NB

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
	IMMessage iMMessage;
	IMMessage iMMessageWithFallbackProcedure;

	protected override Type MessageType => typeof(IMMessage);
}
