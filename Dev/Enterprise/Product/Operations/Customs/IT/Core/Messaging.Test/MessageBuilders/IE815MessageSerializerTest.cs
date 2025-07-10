using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.MessageBuilders;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815MessageSerializerTest : BaseMessageSerializerTest
{
	[TestDate(2019, 4, 24)]
	public void TestAttributes()
	{
		AssertContains("IE815IT00BGA00169W     32569424              20190424", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestDetailPart()
	{
		AssertContains("A	0001	0001	00	001	2	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestHeaderEadSegment()
	{
		AssertContains("1	D	30	2	0	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestConsignorTrader()
	{
		AssertContains("IT00BGA001ZZZ     	CONSIGNOR	CONSIGNOR STREET	1	99990	CONSIGNOR CITY	EN	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestPlaceOfDispatcherTrader()
	{
		AssertContains("IT00BGA001XXX     	PLC DSPTCHR	PLC DSPTCHR STREET	1	99991	PLC DSPTCHR CITY	EN	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestDispatchImportOffice()
	{
		AssertContains("XXXXXXXX	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestCompetentAuthorityDispatchOffice()
	{
		AssertContains("IT276000	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestConsigneeTrader()
	{
		AssertContains("ES00028DB035M     	CONSIGNEE	CONSIGNEE STREET	1	99992	CONSIGNEE CITY	es	CONS EORI	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestComplementConsigneeTrader()
	{
		AssertContains("X1	SERIAL NUMBER OF CERTIFICATE OF EXEMPTION	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestDeliveryPlaceTrader()
	{
		AssertContains("DELIVERYPLACE     	DELIVERYPLACE	DELIVERYPLACE STREET	1	99993	DELIVERYPLACE CITY	es	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestDeliveryPlaceCustomsOffice()
	{
		AssertContains("DLOFFICE	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestTransportMode()
	{
		AssertContains("1	TRANSPORT MODE COMPLEMENTARY INFORMATION	EN	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestTransportArranger()
	{
		AssertContains("TRANSPORTARRANGER VAT              	TRANSPORTARRANGER	TRANSPORTARRANGER STREET	1	99994	TRANSPORTARRANGER CITY	EN	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestFirstTransportArranger()
	{
		AssertContains("FIRST TRANSPORTARRANGER VAT        	FIRST TRANSPORTARRANGER	FIRST TRANSPORTARRANGER STREET	1	99995	FIRST TRANSPORTARRANGER CITY	EN	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestEad()
	{
		AssertContains("REFERENCE DOCUMENT	20190423	2	20190423	165745	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestMovementGuarantee()
	{
		AssertContains("1	CONSIGNOR CODE G	1	1000.12	CONSIGNEE CODE G	2	2000.22	TRANSPORTER CODE G	3	3000.33	OWNER CODE G	4	4000.44	002	H	001	GUARANTOREXC1	GUARANTORVAT1	GUARANTOR1	GUARANTOR1 STREET	1	99996	GUARANTOR1 CITY	EN	H	002	GUARANTOREXC2	GUARANTORVAT2	GUARANTOR2	GUARANTOR2 STREET	2	99997	GUARANTOR2 CITY	EN	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestImportSad()
	{
		AssertContains("002	G	001	IMPORT SAD NUMBER 1	G	002	IMPORT SAD NUMBER 2	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestTransport()
	{
		AssertContains("002	I	001	02	IDENTITY1	COMMERCIAL SEAL ID 1	SEAL INFORMATION 1	EN	COMPLEMENTARY INFORMATION 1	EN	I	002	03	IDENTITY2	COMMERCIAL SEAL ID 2	SEAL INFORMATION 2	EN	COMPLEMENTARY INFORMATION 2	EN	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestCertificates()
	{
		AssertContains("002	L	001	DOCUMENT DESCRIPTION 1	EN	REFERENCE DOCUMENT 1	EN	L	002	DOCUMENT DESCRIPTION 2	EN	REFERENCE DOCUMENT 2	EN	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestBodyEad()
	{
		AssertContains("001	CPA0	88888888	12345.222	44444.44	333333.33	0.89	0.45	0.3	FISCAL MARK	EN	1	DESIGNATION OF ORIGIN 1	EN	1234567890	22	CADD	AAMS	COMMERCIAL DESCRIPTION 1	EN	BRAND NAME OF PRODUCT 1	EN	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestPackages()
	{
		AssertContains("002	M	001	CT	10	SEAL IDENTIFICATION 1	SEAL INFORMATION 1	EN	M	002	CT	20	SEAL IDENTIFICATION 2	SEAL INFORMATION 2	EN	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestWineProduct()
	{
		AssertContains("1	GROW1	IT	OTHER INFORMATION	EN	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestWineOperations()
	{
		AssertContains("002	O	001	1	O	002	2	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestDetailPartContinuation()
	{
		AssertContains("C	0000	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestBodyEadContinuation()
	{
		AssertContains("002	CPA2	88888888	12345.222	44444.44	333333.33	0.89	0.45	0.3	FISCAL MARK	EN	1	DESIGNATION OF ORIGIN 2	EN	1234567890	22	CADD	AAMS	COMMERCIAL DESCRIPTION 2	EN	BRAND NAME OF PRODUCT 2	EN	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestPackagesContinuation()
	{
		AssertContains("002	M	001	CT	10	SEAL IDENTIFICATION 3	SEAL INFORMATION 3	EN	M	002	CT	20	SEAL IDENTIFICATION 4	SEAL INFORMATION 4	EN	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestWineProductContinuation()
	{
		AssertContains("2	GROW2	IT	OTHER INFORMATION 2	EN	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	public void TestWineOperationsContinuation()
	{
		AssertContains("002	O	001	3	O	002	4	", flatFileMessageSerializer.Serialize(iE815Message));
	}

	[TestDate(2019, 4, 24)]
	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestCompleteMessage()
	{
		AssertASCIIFileSameAsString(BaseSourcePath + Messaging.Testing.TestConstants.ProjectRelativePath + @"TestFiles\TestIE815.txt", flatFileMessageSerializer.Serialize(iE815Message));
	}

	protected override void SetUp()
	{
		base.SetUp();

		var iE815EMCSMessageHeaderMock = new Mock<IIE815EMCSMessageHeader>();
		iE815EMCSMessageHeaderMock.Setup(m => m.Attributes).Returns(SetupFixedPart());
		iE815EMCSMessageHeaderMock.Setup(m => m.DetailPart).Returns(SetupDetailPart());
		iE815EMCSMessageHeaderMock.Setup(m => m.HeaderEad).Returns(SetupHeaderEad());
		iE815EMCSMessageHeaderMock.Setup(m => m.ConsignorTrader).Returns(SetupConsignor());
		iE815EMCSMessageHeaderMock.Setup(m => m.PlaceOfDispatchTrader).Returns(SetupPlaceOfDispatcher());
		iE815EMCSMessageHeaderMock.Setup(m => m.DispatchImportOffice).Returns(SetupDispatchImportOffice());
		iE815EMCSMessageHeaderMock.Setup(m => m.CompetentAuthorityDispatchOffice).Returns(SetupCompetentAuthorityDispatchOffice());
		iE815EMCSMessageHeaderMock.Setup(m => m.ConsigneeTrader).Returns(SetupConsignee());
		iE815EMCSMessageHeaderMock.Setup(m => m.ComplementConsigneeTrader).Returns(SetupComplementConsignee());
		iE815EMCSMessageHeaderMock.Setup(m => m.DeliveryPlaceTrader).Returns(SetupDeliveryPlace());
		iE815EMCSMessageHeaderMock.Setup(m => m.DeliveryPlaceCustomsOffice).Returns(SetupDeliveryCustomsOffice());
		iE815EMCSMessageHeaderMock.Setup(m => m.TransportMode).Returns(SetupTransportMode());
		iE815EMCSMessageHeaderMock.Setup(m => m.TransportArrangerTrader).Returns(SetupTransportArranger());
		iE815EMCSMessageHeaderMock.Setup(m => m.FirstTransporterTrader).Returns(SetupFirstTransportArranger());
		iE815EMCSMessageHeaderMock.Setup(m => m.EadDraft).Returns(SetupEadDraft());
		iE815EMCSMessageHeaderMock.Setup(m => m.MovementGuarantee).Returns(SetupMovementGuarantee());
		iE815EMCSMessageHeaderMock.Setup(m => m.ImportSads).Returns(SetupImportSad());
		iE815EMCSMessageHeaderMock.Setup(m => m.TransportDetailContainer).Returns(SetupTransportDetails());
		iE815EMCSMessageHeaderMock.Setup(m => m.Certificates).Returns(SetupCertificates());
		iE815EMCSMessageHeaderMock.Setup(m => m.BodyEad).Returns(SetupBodyEad());

		var iE815EMCSMessage = new Mock<IIE815EMCSMessage>();
		iE815EMCSMessage.Setup(m => m.Header).Returns(iE815EMCSMessageHeaderMock.Object);
		iE815EMCSMessage.Setup(m => m.Continuations).Returns(SetupContinuations());

		iE815Message = new IE815Message(iE815EMCSMessage.Object);

		flatFileMessageSerializer = new TabbedFlatFileMessageSerializer();
	}

	IEnumerable<IWineOperation> SetupWineOperationsContinuation()
	{
		var iE815WineOperationMock1 = new Mock<IWineOperation>();
		iE815WineOperationMock1.Setup(m => m.TreatmentCode).Returns(3);

		var iE815WineOperationMock2 = new Mock<IWineOperation>();
		iE815WineOperationMock2.Setup(m => m.TreatmentCode).Returns(4);

		return new IWineOperation[] { iE815WineOperationMock1.Object, iE815WineOperationMock2.Object };
	}

	IWineProduct SetupWineProductContinuation()
	{
		var iE815WineProductContinuationMock = new Mock<IWineProduct>();

		iE815WineProductContinuationMock.Setup(m => m.WineProductCategory).Returns(2);
		iE815WineProductContinuationMock.Setup(m => m.WineGrowingZoneCode).Returns("GROW2");
		iE815WineProductContinuationMock.Setup(m => m.ThirdCountryOfOrigin).Returns("IT");
		iE815WineProductContinuationMock.Setup(m => m.OtherInformation).Returns("OTHER INFORMATION 2");
		iE815WineProductContinuationMock.Setup(m => m.OtherInformationLanguage).Returns("EN");
		iE815WineProductContinuationMock.Setup(m => m.WineOperations).Returns(SetupWineOperationsContinuation());

		return iE815WineProductContinuationMock.Object;
	}

	IEnumerable<IPackage> SetupPackagesContinuation()
	{
		var iE815PackageMock1 = new Mock<IPackage>();
		iE815PackageMock1.Setup(m => m.KindOfPackages).Returns("CT");
		iE815PackageMock1.Setup(m => m.NumberOfPackages).Returns(10);
		iE815PackageMock1.Setup(m => m.CommercialSealIdentification).Returns("SEAL IDENTIFICATION 3");
		iE815PackageMock1.Setup(m => m.SealInformation).Returns("SEAL INFORMATION 3");
		iE815PackageMock1.Setup(m => m.SealInformationLanguage).Returns("EN");

		var iE815PackageMock2 = new Mock<IPackage>();
		iE815PackageMock2.Setup(m => m.KindOfPackages).Returns("CT");
		iE815PackageMock2.Setup(m => m.NumberOfPackages).Returns(20);
		iE815PackageMock2.Setup(m => m.CommercialSealIdentification).Returns("SEAL IDENTIFICATION 4");
		iE815PackageMock2.Setup(m => m.SealInformation).Returns("SEAL INFORMATION 4");
		iE815PackageMock2.Setup(m => m.SealInformationLanguage).Returns("EN");

		return new IPackage[] { iE815PackageMock1.Object, iE815PackageMock2.Object };
	}

	IIE815BodyEad SetupBodyEadContinuation()
	{
		var iE815BodyEadContinuationMock = new Mock<IIE815BodyEad>();

		iE815BodyEadContinuationMock.Setup(m => m.BodyRecordUniqueReference).Returns(2);
		iE815BodyEadContinuationMock.Setup(m => m.ExciseProductCode).Returns("CPA2");
		iE815BodyEadContinuationMock.Setup(m => m.CnCode).Returns("88888888");
		iE815BodyEadContinuationMock.Setup(m => m.Quantity).Returns(12345.222);
		iE815BodyEadContinuationMock.Setup(m => m.GrossWeight).Returns(44444.444);
		iE815BodyEadContinuationMock.Setup(m => m.NetWeight).Returns(333333.333);
		iE815BodyEadContinuationMock.Setup(m => m.Density).Returns(0.89);
		iE815BodyEadContinuationMock.Setup(m => m.AlcoholicStrength).Returns(0.45);
		iE815BodyEadContinuationMock.Setup(m => m.DegreePlato).Returns(0.3);
		iE815BodyEadContinuationMock.Setup(m => m.FiscalMark).Returns("FISCAL MARK");
		iE815BodyEadContinuationMock.Setup(m => m.FiscalMarkLanguage).Returns("EN");
		iE815BodyEadContinuationMock.Setup(m => m.FiscalMarkUsedFlag).Returns(true);
		iE815BodyEadContinuationMock.Setup(m => m.DesignationOfOrigin).Returns("DESIGNATION OF ORIGIN 2");
		iE815BodyEadContinuationMock.Setup(m => m.DesignationOfOriginLanguage).Returns("EN");
		iE815BodyEadContinuationMock.Setup(m => m.SizeOfProducer).Returns(1234567890);
		iE815BodyEadContinuationMock.Setup(m => m.TaricCode).Returns(22);
		iE815BodyEadContinuationMock.Setup(m => m.CaddCode).Returns("CADD");
		iE815BodyEadContinuationMock.Setup(m => m.AamsCode).Returns("AAMS");
		iE815BodyEadContinuationMock.Setup(m => m.CommercialDescription).Returns("COMMERCIAL DESCRIPTION 2");
		iE815BodyEadContinuationMock.Setup(m => m.CommercialDescriptionLanguage).Returns("EN");
		iE815BodyEadContinuationMock.Setup(m => m.BrandNameOfProducts).Returns("BRAND NAME OF PRODUCT 2");
		iE815BodyEadContinuationMock.Setup(m => m.BrandNameOfProductsLanguage).Returns("EN");
		iE815BodyEadContinuationMock.Setup(m => m.Packages).Returns(SetupPackagesContinuation());
		iE815BodyEadContinuationMock.Setup(m => m.WineProduct).Returns(SetupWineProductContinuation());

		return iE815BodyEadContinuationMock.Object;
	}

	IEnumerable<IIE815EMCSMessageContinuation> SetupContinuations()
	{
		var ie815MessageContinuation = new Mock<IIE815EMCSMessageContinuation>();
		ie815MessageContinuation.Setup(m => m.Attributes).Returns(SetupFixedPart());
		ie815MessageContinuation.Setup(m => m.BodyEad).Returns(SetupBodyEadContinuation());

		return new IIE815EMCSMessageContinuation[] { ie815MessageContinuation.Object };
	}

	IEnumerable<IWineOperation> SetupWineOperations()
	{
		var iE815WineOperationMock1 = new Mock<IWineOperation>();
		iE815WineOperationMock1.Setup(m => m.TreatmentCode).Returns(1);

		var iE815WineOperationMock2 = new Mock<IWineOperation>();
		iE815WineOperationMock2.Setup(m => m.TreatmentCode).Returns(2);

		return new IWineOperation[] { iE815WineOperationMock1.Object, iE815WineOperationMock2.Object };
	}

	IWineProduct SetupWineProduct()
	{
		var iE815WineProductMock = new Mock<IWineProduct>();

		iE815WineProductMock.Setup(m => m.WineProductCategory).Returns(1);
		iE815WineProductMock.Setup(m => m.WineGrowingZoneCode).Returns("GROW1");
		iE815WineProductMock.Setup(m => m.ThirdCountryOfOrigin).Returns("IT");
		iE815WineProductMock.Setup(m => m.OtherInformation).Returns("OTHER INFORMATION");
		iE815WineProductMock.Setup(m => m.OtherInformationLanguage).Returns("EN");
		iE815WineProductMock.Setup(m => m.WineOperations).Returns(SetupWineOperations());

		return iE815WineProductMock.Object;
	}

	IEnumerable<IPackage> SetupPackages()
	{
		var iE815PackageMock1 = new Mock<IPackage>();
		iE815PackageMock1.Setup(m => m.KindOfPackages).Returns("CT");
		iE815PackageMock1.Setup(m => m.NumberOfPackages).Returns(10);
		iE815PackageMock1.Setup(m => m.CommercialSealIdentification).Returns("SEAL IDENTIFICATION 1");
		iE815PackageMock1.Setup(m => m.SealInformation).Returns("SEAL INFORMATION 1");
		iE815PackageMock1.Setup(m => m.SealInformationLanguage).Returns("EN");

		var iE815PackageMock2 = new Mock<IPackage>();
		iE815PackageMock2.Setup(m => m.KindOfPackages).Returns("CT");
		iE815PackageMock2.Setup(m => m.NumberOfPackages).Returns(20);
		iE815PackageMock2.Setup(m => m.CommercialSealIdentification).Returns("SEAL IDENTIFICATION 2");
		iE815PackageMock2.Setup(m => m.SealInformation).Returns("SEAL INFORMATION 2");
		iE815PackageMock2.Setup(m => m.SealInformationLanguage).Returns("EN");

		return new IPackage[] { iE815PackageMock1.Object, iE815PackageMock2.Object };
	}

	IIE815BodyEad SetupBodyEad()
	{
		var iE815BodyEadMock = new Mock<IIE815BodyEad>();
		iE815BodyEadMock.Setup(m => m.BodyRecordUniqueReference).Returns(1);
		iE815BodyEadMock.Setup(m => m.ExciseProductCode).Returns("CPA0");
		iE815BodyEadMock.Setup(m => m.CnCode).Returns("88888888");
		iE815BodyEadMock.Setup(m => m.Quantity).Returns(12345.222);
		iE815BodyEadMock.Setup(m => m.GrossWeight).Returns(44444.444);
		iE815BodyEadMock.Setup(m => m.NetWeight).Returns(333333.333);
		iE815BodyEadMock.Setup(m => m.Density).Returns(0.89);
		iE815BodyEadMock.Setup(m => m.AlcoholicStrength).Returns(0.45);
		iE815BodyEadMock.Setup(m => m.DegreePlato).Returns(0.3);
		iE815BodyEadMock.Setup(m => m.FiscalMark).Returns("FISCAL MARK");
		iE815BodyEadMock.Setup(m => m.FiscalMarkLanguage).Returns("EN");
		iE815BodyEadMock.Setup(m => m.FiscalMarkUsedFlag).Returns(true);
		iE815BodyEadMock.Setup(m => m.DesignationOfOrigin).Returns("DESIGNATION OF ORIGIN 1");
		iE815BodyEadMock.Setup(m => m.DesignationOfOriginLanguage).Returns("EN");
		iE815BodyEadMock.Setup(m => m.SizeOfProducer).Returns(1234567890);
		iE815BodyEadMock.Setup(m => m.TaricCode).Returns(22);
		iE815BodyEadMock.Setup(m => m.CaddCode).Returns("CADD");
		iE815BodyEadMock.Setup(m => m.AamsCode).Returns("AAMS");
		iE815BodyEadMock.Setup(m => m.CommercialDescription).Returns("COMMERCIAL DESCRIPTION 1");
		iE815BodyEadMock.Setup(m => m.CommercialDescriptionLanguage).Returns("EN");
		iE815BodyEadMock.Setup(m => m.BrandNameOfProducts).Returns("BRAND NAME OF PRODUCT 1");
		iE815BodyEadMock.Setup(m => m.BrandNameOfProductsLanguage).Returns("EN");
		iE815BodyEadMock.Setup(m => m.Packages).Returns(SetupPackages());
		iE815BodyEadMock.Setup(m => m.WineProduct).Returns(SetupWineProduct());

		return iE815BodyEadMock.Object;
	}

	ICertificates SetupCertificates()
	{
		var iE815CertificatesMock = new Mock<ICertificates>();

		var iE815DocumentCertificateMock1 = new Mock<IDocumentCertificate>();
		iE815DocumentCertificateMock1.Setup(m => m.DocumentDescription).Returns("DOCUMENT DESCRIPTION 1");
		iE815DocumentCertificateMock1.Setup(m => m.DocumentDescriptionLanguage).Returns("EN");
		iE815DocumentCertificateMock1.Setup(m => m.ReferenceOfDocument).Returns("REFERENCE DOCUMENT 1");
		iE815DocumentCertificateMock1.Setup(m => m.ReferenceOfDocumentLanguage).Returns("EN");

		var iE815DocumentCertificateMock2 = new Mock<IDocumentCertificate>();
		iE815DocumentCertificateMock2.Setup(m => m.DocumentDescription).Returns("DOCUMENT DESCRIPTION 2");
		iE815DocumentCertificateMock2.Setup(m => m.DocumentDescriptionLanguage).Returns("EN");
		iE815DocumentCertificateMock2.Setup(m => m.ReferenceOfDocument).Returns("REFERENCE DOCUMENT 2");
		iE815DocumentCertificateMock2.Setup(m => m.ReferenceOfDocumentLanguage).Returns("EN");

		iE815CertificatesMock.Setup(m => m.DocumentCertificates).Returns(new IDocumentCertificate[] { iE815DocumentCertificateMock1.Object, iE815DocumentCertificateMock2.Object });
		return iE815CertificatesMock.Object;
	}

	ITransportDetailContainer SetupTransportDetails()
	{
		var iE815TransportMock = new Mock<ITransportDetailContainer>();

		var iE815TransportDetailMock1 = new Mock<ITransportDetails>();
		iE815TransportDetailMock1.Setup(m => m.TransportUnitCode).Returns(2);
		iE815TransportDetailMock1.Setup(m => m.IdentityOfTransportUnits).Returns("IDENTITY1");
		iE815TransportDetailMock1.Setup(m => m.CommercialSealIdentification).Returns("COMMERCIAL SEAL ID 1");
		iE815TransportDetailMock1.Setup(m => m.SealInformation).Returns("SEAL INFORMATION 1");
		iE815TransportDetailMock1.Setup(m => m.SealInformationLanguage).Returns("EN");
		iE815TransportDetailMock1.Setup(m => m.ComplementaryInformation).Returns("COMPLEMENTARY INFORMATION 1");
		iE815TransportDetailMock1.Setup(m => m.ComplementaryInformationLanguage).Returns("EN");

		var iE815TransportDetailMock2 = new Mock<ITransportDetails>();
		iE815TransportDetailMock2.Setup(m => m.TransportUnitCode).Returns(3);
		iE815TransportDetailMock2.Setup(m => m.IdentityOfTransportUnits).Returns("IDENTITY2");
		iE815TransportDetailMock2.Setup(m => m.CommercialSealIdentification).Returns("COMMERCIAL SEAL ID 2");
		iE815TransportDetailMock2.Setup(m => m.SealInformation).Returns("SEAL INFORMATION 2");
		iE815TransportDetailMock2.Setup(m => m.SealInformationLanguage).Returns("EN");
		iE815TransportDetailMock2.Setup(m => m.ComplementaryInformation).Returns("COMPLEMENTARY INFORMATION 2");
		iE815TransportDetailMock2.Setup(m => m.ComplementaryInformationLanguage).Returns("EN");

		iE815TransportMock.Setup(m => m.TransportDetails).Returns(new ITransportDetails[] { iE815TransportDetailMock1.Object, iE815TransportDetailMock2.Object });
		return iE815TransportMock.Object;
	}

	IImportSadContainer SetupImportSad()
	{
		var iE815MovementImportSadMock = new Mock<IImportSadContainer>();

		var iE815ImportSadMock1 = new Mock<IImportSad>();
		iE815ImportSadMock1.Setup(m => m.ImportSadNumber).Returns("IMPORT SAD NUMBER 1");

		var iE815ImportSadMock2 = new Mock<IImportSad>();
		iE815ImportSadMock2.Setup(m => m.ImportSadNumber).Returns("IMPORT SAD NUMBER 2");

		iE815MovementImportSadMock.Setup(m => m.ImportSads).Returns(new IImportSad[] { iE815ImportSadMock1.Object, iE815ImportSadMock2.Object });
		return iE815MovementImportSadMock.Object;
	}

	IMovementGuarantee SetupMovementGuarantee()
	{
		var iE815MovementGuaranteeMock = new Mock<IMovementGuarantee>();
		iE815MovementGuaranteeMock.Setup(m => m.GuarantorTypeCode).Returns(1);
		iE815MovementGuaranteeMock.Setup(m => m.ConsignorCodeGuarantee).Returns("CONSIGNOR CODE G");
		iE815MovementGuaranteeMock.Setup(m => m.ConsignorTypeGuarantee).Returns("1");
		iE815MovementGuaranteeMock.Setup(m => m.ConsignorDepositAmountCommitted).Returns(1000.12);
		iE815MovementGuaranteeMock.Setup(m => m.ConsigneeCodeGuarantee).Returns("CONSIGNEE CODE G");
		iE815MovementGuaranteeMock.Setup(m => m.ConsigneeTypeGuarantee).Returns("2");
		iE815MovementGuaranteeMock.Setup(m => m.ConsigneeDepositAmountCommitted).Returns(2000.22);
		iE815MovementGuaranteeMock.Setup(m => m.TransporterCodeGuarantee).Returns("TRANSPORTER CODE G");
		iE815MovementGuaranteeMock.Setup(m => m.TransporterTypeGuarantee).Returns("3");
		iE815MovementGuaranteeMock.Setup(m => m.TransporterDepositAmountCommitted).Returns(3000.33);
		iE815MovementGuaranteeMock.Setup(m => m.OwnerCodeGuarantee).Returns("OWNER CODE G");
		iE815MovementGuaranteeMock.Setup(m => m.OwnerTypeGuarantee).Returns("4");
		iE815MovementGuaranteeMock.Setup(m => m.OwnerDepositAmountCommitted).Returns(4000.44);

		var iE815GuarantorTraderMock1 = new Mock<IGuarantorTrader>();
		iE815GuarantorTraderMock1.Setup(m => m.TraderExciseNumber).Returns("GUARANTOREXC1");
		iE815GuarantorTraderMock1.Setup(m => m.VatNumber).Returns("GUARANTORVAT1");
		iE815GuarantorTraderMock1.Setup(m => m.TraderName).Returns("GUARANTOR1");
		iE815GuarantorTraderMock1.Setup(m => m.StreetName).Returns("GUARANTOR1 STREET");
		iE815GuarantorTraderMock1.Setup(m => m.StreetNumber).Returns("1");
		iE815GuarantorTraderMock1.Setup(m => m.Postcode).Returns("99996");
		iE815GuarantorTraderMock1.Setup(m => m.City).Returns("GUARANTOR1 CITY");
		iE815GuarantorTraderMock1.Setup(m => m.LanguageDescriptions).Returns("EN");

		var iE815GuarantorTraderMock2 = new Mock<IGuarantorTrader>();
		iE815GuarantorTraderMock2.Setup(m => m.TraderExciseNumber).Returns("GUARANTOREXC2");
		iE815GuarantorTraderMock2.Setup(m => m.VatNumber).Returns("GUARANTORVAT2");
		iE815GuarantorTraderMock2.Setup(m => m.TraderName).Returns("GUARANTOR2");
		iE815GuarantorTraderMock2.Setup(m => m.StreetName).Returns("GUARANTOR2 STREET");
		iE815GuarantorTraderMock2.Setup(m => m.StreetNumber).Returns("2");
		iE815GuarantorTraderMock2.Setup(m => m.Postcode).Returns("99997");
		iE815GuarantorTraderMock2.Setup(m => m.City).Returns("GUARANTOR2 CITY");
		iE815GuarantorTraderMock2.Setup(m => m.LanguageDescriptions).Returns("EN");

		iE815MovementGuaranteeMock.Setup(m => m.Guarantors).Returns(new IGuarantorTrader[] { iE815GuarantorTraderMock1.Object, iE815GuarantorTraderMock2.Object });
		return iE815MovementGuaranteeMock.Object;
	}

	IEadDraft SetupEadDraft()
	{
		var iE815EadDraftSegment = new Mock<IEadDraft>();
		iE815EadDraftSegment.Setup(m => m.InvoiceNumber).Returns("REFERENCE DOCUMENT");
		iE815EadDraftSegment.Setup(m => m.InvoiceDate).Returns((ZDate)new ZDateTime(2019, 04, 23));
		iE815EadDraftSegment.Setup(m => m.OriginTypeCode).Returns(2);
		iE815EadDraftSegment.Setup(m => m.DateOfDispatch).Returns((ZDate)new ZDateTime(2019, 04, 23));
		iE815EadDraftSegment.Setup(m => m.TimeOfDispatch).Returns(new ZDateTime(2019, 04, 23, 16, 57, 45));
		return iE815EadDraftSegment.Object;
	}

	ITransportTrader SetupFirstTransportArranger()
	{
		var iE815FirstTransporterMock = new Mock<ITransportTrader>();
		iE815FirstTransporterMock.Setup(m => m.VatNumber).Returns("FIRST TRANSPORTARRANGER VAT");
		iE815FirstTransporterMock.Setup(m => m.TraderName).Returns("FIRST TRANSPORTARRANGER");
		iE815FirstTransporterMock.Setup(m => m.StreetName).Returns("FIRST TRANSPORTARRANGER STREET");
		iE815FirstTransporterMock.Setup(m => m.StreetNumber).Returns("1");
		iE815FirstTransporterMock.Setup(m => m.Postcode).Returns("99995");
		iE815FirstTransporterMock.Setup(m => m.City).Returns("FIRST TRANSPORTARRANGER CITY");
		iE815FirstTransporterMock.Setup(m => m.LanguageDescriptions).Returns("EN");
		return iE815FirstTransporterMock.Object;
	}

	ITransportTrader SetupTransportArranger()
	{
		var iE815TransportArrangerTraderMock = new Mock<ITransportTrader>();
		iE815TransportArrangerTraderMock.Setup(m => m.VatNumber).Returns("TRANSPORTARRANGER VAT");
		iE815TransportArrangerTraderMock.Setup(m => m.TraderName).Returns("TRANSPORTARRANGER");
		iE815TransportArrangerTraderMock.Setup(m => m.StreetName).Returns("TRANSPORTARRANGER STREET");
		iE815TransportArrangerTraderMock.Setup(m => m.StreetNumber).Returns("1");
		iE815TransportArrangerTraderMock.Setup(m => m.Postcode).Returns("99994");
		iE815TransportArrangerTraderMock.Setup(m => m.City).Returns("TRANSPORTARRANGER CITY");
		iE815TransportArrangerTraderMock.Setup(m => m.LanguageDescriptions).Returns("EN");

		return iE815TransportArrangerTraderMock.Object;
	}

	ITransportMode SetupTransportMode()
	{
		var iE815TransportModeMock = new Mock<ITransportMode>();
		iE815TransportModeMock.Setup(m => m.TransportModeCode).Returns(1);
		iE815TransportModeMock.Setup(m => m.ComplementaryInformation).Returns("TRANSPORT MODE COMPLEMENTARY INFORMATION");
		iE815TransportModeMock.Setup(m => m.ComplementaryInformationLanguage).Returns("EN");
		return iE815TransportModeMock.Object;
	}

	IOffice SetupDeliveryCustomsOffice()
	{
		var iE815DeliveryPlaceCustomsOfficeMock = new Mock<IOffice>();
		iE815DeliveryPlaceCustomsOfficeMock.Setup(m => m.ReferenceNumber).Returns("DLOFFICE");
		return iE815DeliveryPlaceCustomsOfficeMock.Object;
	}

	IDeliveryPlaceTrader SetupDeliveryPlace()
	{
		var iE815DeliveryPlaceTraderMock = new Mock<IDeliveryPlaceTrader>();
		iE815DeliveryPlaceTraderMock.Setup(m => m.TraderId).Returns("DELIVERYPLACE");
		iE815DeliveryPlaceTraderMock.Setup(m => m.TraderName).Returns("DELIVERYPLACE");
		iE815DeliveryPlaceTraderMock.Setup(m => m.StreetName).Returns("DELIVERYPLACE STREET");
		iE815DeliveryPlaceTraderMock.Setup(m => m.StreetNumber).Returns("1");
		iE815DeliveryPlaceTraderMock.Setup(m => m.Postcode).Returns("99993");
		iE815DeliveryPlaceTraderMock.Setup(m => m.City).Returns("DELIVERYPLACE CITY");
		iE815DeliveryPlaceTraderMock.Setup(m => m.LanguageDescriptions).Returns("es");
		return iE815DeliveryPlaceTraderMock.Object;
	}

	IOffice SetupCompetentAuthorityDispatchOffice()
	{
		var iE815CompetentAuthorityDispatchOfficeMock = new Mock<IOffice>();
		iE815CompetentAuthorityDispatchOfficeMock.Setup(m => m.ReferenceNumber).Returns("IT276000");
		return iE815CompetentAuthorityDispatchOfficeMock.Object;
	}

	IOffice SetupDispatchImportOffice()
	{
		var iE815DispatchImportOfficeMock = new Mock<IOffice>();
		iE815DispatchImportOfficeMock.Setup(m => m.ReferenceNumber).Returns("XXXXXXXX");
		return iE815DispatchImportOfficeMock.Object;
	}

	IComplementConsigneeTrader SetupComplementConsignee()
	{
		var iE815ComplementConsigneeTraderMock = new Mock<IComplementConsigneeTrader>();
		iE815ComplementConsigneeTraderMock.Setup(m => m.MemberStateCode).Returns("X1");
		iE815ComplementConsigneeTraderMock.Setup(m => m.SerialNumberOfCertificateOfExemption).Returns("SERIAL NUMBER OF CERTIFICATE OF EXEMPTION");
		return iE815ComplementConsigneeTraderMock.Object;
	}

	IConsigneeTrader SetupConsignee()
	{
		var iE815ConsigneeMock = new Mock<IConsigneeTrader>();
		iE815ConsigneeMock.Setup(m => m.TraderId).Returns("ES00028DB035M");
		iE815ConsigneeMock.Setup(m => m.TraderName).Returns("CONSIGNEE");
		iE815ConsigneeMock.Setup(m => m.StreetName).Returns("CONSIGNEE STREET");
		iE815ConsigneeMock.Setup(m => m.StreetNumber).Returns("1");
		iE815ConsigneeMock.Setup(m => m.Postcode).Returns("99992");
		iE815ConsigneeMock.Setup(m => m.City).Returns("CONSIGNEE CITY");
		iE815ConsigneeMock.Setup(m => m.LanguageDescriptions).Returns("es");
		iE815ConsigneeMock.Setup(m => m.EoriNumber).Returns("CONS EORI");
		return iE815ConsigneeMock.Object;
	}

	IPlaceOfDispatchTrader SetupPlaceOfDispatcher()
	{
		var iiE815PlaceOfDispatchTraderMock = new Mock<IPlaceOfDispatchTrader>();
		iiE815PlaceOfDispatchTraderMock.Setup(m => m.ReferenceOfTaxWarehouse).Returns("IT00BGA001XXX");
		iiE815PlaceOfDispatchTraderMock.Setup(m => m.TraderName).Returns("PLC DSPTCHR");
		iiE815PlaceOfDispatchTraderMock.Setup(m => m.StreetName).Returns("PLC DSPTCHR STREET");
		iiE815PlaceOfDispatchTraderMock.Setup(m => m.StreetNumber).Returns("1");
		iiE815PlaceOfDispatchTraderMock.Setup(m => m.Postcode).Returns("99991");
		iiE815PlaceOfDispatchTraderMock.Setup(m => m.City).Returns("PLC DSPTCHR CITY");
		iiE815PlaceOfDispatchTraderMock.Setup(m => m.LanguageDescriptions).Returns("EN");
		return iiE815PlaceOfDispatchTraderMock.Object;
	}

	IConsignorTrader SetupConsignor()
	{
		var iE815ConsignorTraderMock = new Mock<IConsignorTrader>();
		iE815ConsignorTraderMock.Setup(m => m.TraderExciseNumber).Returns("IT00BGA001ZZZ");
		iE815ConsignorTraderMock.Setup(m => m.TraderName).Returns("CONSIGNOR");
		iE815ConsignorTraderMock.Setup(m => m.StreetName).Returns("CONSIGNOR STREET");
		iE815ConsignorTraderMock.Setup(m => m.StreetNumber).Returns("1");
		iE815ConsignorTraderMock.Setup(m => m.Postcode).Returns("99990");
		iE815ConsignorTraderMock.Setup(m => m.City).Returns("CONSIGNOR CITY");
		iE815ConsignorTraderMock.Setup(m => m.LanguageDescriptions).Returns("EN");
		return iE815ConsignorTraderMock.Object;
	}

	IHeaderEad SetupHeaderEad()
	{
		var result = new Mock<IHeaderEad>();
		result.Setup(m => m.DestinationTypeCode).Returns(1);
		result.Setup(m => m.DurationTransportUnitMeasure).Returns("D");
		result.Setup(m => m.JourneyTime).Returns(30);
		result.Setup(m => m.TransportArrangement).Returns(2);
		result.Setup(m => m.SendFlagDeferred).Returns(false);
		return result.Object;
	}

	IIE815MessageHeaderDetailPart SetupDetailPart()
	{
		var iE815MessageHeaderDetailPart = new Mock<IIE815MessageHeaderDetailPart>();
		iE815MessageHeaderDetailPart.Setup(m => m.MessageType).Returns(2);
		return iE815MessageHeaderDetailPart.Object;
	}

	IIE815Attributes SetupFixedPart()
	{
		var iE815AttributesMock = new Mock<IIE815Attributes>();
		iE815AttributesMock.Setup(m => m.IDRegistrantCode).Returns("IT00BGA00169W");
		iE815AttributesMock.Setup(m => m.LocalIdentityNumber).Returns("32569424");
		iE815AttributesMock.Setup(m => m.DateOfTrasmission).Returns((ZDate)ZDateTime.Now.ToDateTime());
		return iE815AttributesMock.Object;
	}

	TabbedFlatFileMessageSerializer flatFileMessageSerializer;
	IE815Message iE815Message;

	protected override Type MessageType => typeof(IE815Message);
}
