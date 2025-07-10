using System.IO;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR830MessageBuilderTest : TestCaseWithFactory
	{
		Mock<IExportEntryHeader> exportHeaderMock;
		Mock<IExportEntryLine> entryLine1Mock;

		protected override void SetUp()
		{
			base.SetUp();

			#region exportHeader
			exportHeaderMock = new Mock<IExportEntryHeader>();
			exportHeaderMock.Setup(x => x.ExportDeclarationNumber).Returns("6N00221000025X");
			exportHeaderMock.Setup(x => x.TransactionType).Returns("11");
			exportHeaderMock.Setup(x => x.CountryOfDestination).Returns("HK");
			exportHeaderMock.Setup(x => x.GoodsLocationPostcode).Returns("08589");
			exportHeaderMock.Setup(x => x.GoodsLocationAddress).Returns("서울 금천구 가산디지털1로 119");
			exportHeaderMock.Setup(x => x.GoodsLocationAdditionalDetails).Returns("(SK)");
			exportHeaderMock.Setup(x => x.DeclarationProcedureType).Returns("H");
			exportHeaderMock.Setup(x => x.ExporterType).Returns("C");
			exportHeaderMock.Setup(x => x.ApplicationForSimpleDrawback).Returns("NO");
			exportHeaderMock.Setup(x => x.CargoManagement.ImportCargoManagementNumber).Returns("16HJSC0686I0008");
			exportHeaderMock.Setup(x => x.TotalCustomsValue).Returns(25987293m);
			exportHeaderMock.Setup(x => x.Incoterm).Returns("CFR");
			exportHeaderMock.Setup(x => x.Currency).Returns("USD");
			exportHeaderMock.Setup(x => x.TotalInvoiceAmount).Returns(27670m);
			exportHeaderMock.Setup(x => x.TotalPackQty).Returns(1);
			exportHeaderMock.Setup(x => x.PackType).Returns("OU");
			exportHeaderMock.Setup(x => x.TotalGrossWeightInKG).Returns(29600m);
			#endregion

			#region EntryLine
			entryLine1Mock = new Mock<IExportEntryLine>();
			entryLine1Mock.Setup(m => m.EntryLineNo).Returns("001");
			entryLine1Mock.Setup(m => m.HSCode).Returns("8429521022");
			entryLine1Mock.Setup(m => m.TradeName).Returns("HYUNDAI ROBEX3000LC-7A");
			entryLine1Mock.Setup(m => m.CountryOfOrigin).Returns("KR");
			entryLine1Mock.Setup(m => m.NetWeightInKG).Returns(360m);
			entryLine1Mock.Setup(m => m.NetWeightUQ).Returns("KG");
			entryLine1Mock.Setup(m => m.CustomsValue).Returns(25987293m);
			entryLine1Mock.Setup(m => m.PreApprovalType).Returns("A");
			entryLine1Mock.Setup(m => m.PreApprovalNo).Returns("KR00101010101");
			entryLine1Mock.Setup(m => m.PreApprovalEffectiveFromDate).Returns(new ZDate("2018-01-01"));
			entryLine1Mock.Setup(m => m.PreApprovalEffectiveToDate).Returns(new ZDate("2018-09-01"));
			entryLine1Mock.Setup(m => m.DocumentAttached).Returns("N");
			#endregion
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 03, 03)]
		public void TestGenerateDeclaration()
		{
			#region exportHeader
			exportHeaderMock.Setup(m => m.ExportTypeCode).Returns("B");
			exportHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("130");
			exportHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("10");
			exportHeaderMock.Setup(m => m.PortOfLoading).Returns("KRINC");
			exportHeaderMock.Setup(m => m.SouthNorthTradeIdentification).Returns("999999");
			exportHeaderMock.Setup(m => m.FinalLoadingPlace).Returns("9999999");
			exportHeaderMock.Setup(m => m.GoodsLocationBondedAreaCode).Returns("99999999");
			exportHeaderMock.Setup(m => m.PreferredInspectionDate).Returns(ZDate.Today);
			exportHeaderMock.Setup(m => m.BondedTransportationFromDate).Returns(new ZDate("2014-01-01"));
			exportHeaderMock.Setup(m => m.BondedTransportationToDate).Returns(new ZDate("2014-01-01"));
			exportHeaderMock.Setup(m => m.DepartureDate).Returns(new ZDate("2014-01-01"));
			exportHeaderMock.Setup(m => m.DrawbackApplicantType).Returns("1");
			exportHeaderMock.Setup(m => m.InvoicePaymentTerm).Returns("AA");
			exportHeaderMock.Setup(m => m.OutOfHoursDeclarationIndicator).Returns("B");
			exportHeaderMock.Setup(m => m.ReturnReason).Returns("ZZ");
			exportHeaderMock.Setup(m => m.ReturnType).Returns("A");
			exportHeaderMock.Setup(m => m.GoodsStatus).Returns("O");
			exportHeaderMock.Setup(m => m.ContainerizedIndicator).Returns(false);
			exportHeaderMock.Setup(m => m.SouthNorthTradeYN).Returns("N");
			exportHeaderMock.Setup(m => m.ContainerPackMode).Returns("AA");
			exportHeaderMock.Setup(m => m.LCNo).Returns("1234567");
			exportHeaderMock.Setup(m => m.TransportMode).Returns("10");
			exportHeaderMock.Setup(m => m.UCR).Returns("99999999999999999");
			exportHeaderMock.Setup(m => m.LocationIDInBondedArea).Returns("9999999999");
			exportHeaderMock.Setup(m => m.FreightForwarderContactName).Returns("나대표");
			exportHeaderMock.Setup(m => m.CarrierID).Returns("KE");
			exportHeaderMock.Setup(m => m.ShippingLineOrAirlineName).Returns("대한항공");
			exportHeaderMock.Setup(m => m.VesselNameOrFlightNo).Returns("AA9999");
			exportHeaderMock.Setup(m => m.Freight).Returns(899999999m);
			exportHeaderMock.Setup(m => m.Insurance).Returns(799999999m);
			exportHeaderMock.Setup(m => m.ExchangeRate).Returns(1084.25m);
			exportHeaderMock.Setup(m => m.DeclarantAdditionalDescription).Returns("신고인 기재란");
			exportHeaderMock.Setup(m => m.IndustrialParkCode).Returns("990");
			#endregion

			#region Containers
			var exportContainer1 = new Mock<IExportContainer>();
			exportContainer1.Setup(m => m.SequenceNo).Returns("01");
			exportContainer1.Setup(m => m.ContainerNo).Returns("999999999999999");
			var exportContainer2 = new Mock<IExportContainer>();
			exportContainer2.Setup(m => m.SequenceNo).Returns("02");
			exportContainer2.Setup(m => m.ContainerNo).Returns("899999999999999");
			exportHeaderMock.Setup(m => m.Containers).Returns(new IExportContainer[] { exportContainer1.Object, exportContainer2.Object });
			#endregion

			#region Declarant
			var declarantMock = new Mock<IOrganization>();
			declarantMock.Setup(m => m.CompanyName).Returns("레디코리아");
			declarantMock.Setup(m => m.RepresentativeName).Returns("김환태");

			exportHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			exportHeaderMock.Setup(m => m.UnipassDeclarantID).Returns("6N002");
			#endregion

			#region Exporter
			var exporterMock = new Mock<IOrganization>();
			exporterMock.Setup(m => m.CompanyName).Returns("레디코리아");
			exporterMock.Setup(m => m.OfficeID).Returns("899999999");
			exporterMock.Setup(m => m.UnipassIDForOrganization).Returns("레디코리1971018");
			exportHeaderMock.Setup(m => m.Exporter).Returns(exporterMock.Object);
			#endregion

			#region Supplier
			var supplierMock = new Mock<IOrganization>();
			supplierMock.Setup(m => m.CompanyName).Returns("레디코리아");
			supplierMock.Setup(m => m.RepresentativeName).Returns("김택윤");
			supplierMock.Setup(m => m.AddressLine1).Returns("서울특별시 서초구 동광로 41");
			supplierMock.Setup(m => m.AddressLine2).Returns("레디인빌딩");
			supplierMock.Setup(m => m.Postcode).Returns("06561");
			supplierMock.Setup(m => m.RoadNameCode).Returns("101010");
			supplierMock.Setup(m => m.BuildingNumber).Returns("020120");
			supplierMock.Setup(m => m.OfficeID).Returns("899999999");
			supplierMock.Setup(m => m.UnipassIDForOrganization).Returns("레디코리1971018");
			supplierMock.Setup(m => m.BusinessRegNo).Returns("1028142299");
			exportHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);
			#endregion

			#region Manufacturer
			var manufacturerMock = new Mock<IOrganization>();
			manufacturerMock.Setup(m => m.CompanyName).Returns("제조미상");
			manufacturerMock.Setup(m => m.Postcode).Returns("04784");
			manufacturerMock.Setup(m => m.OfficeID).Returns("899999999");
			manufacturerMock.Setup(m => m.UnipassIDForOrganization).Returns("제조미상9999000");

			exportHeaderMock.Setup(m => m.Manufacturer).Returns(manufacturerMock.Object);
			#endregion

			#region Importer
			var importerMock = new Mock<IOrganization>();
			importerMock.Setup(m => m.CompanyName).Returns("BOA RAM HONGKONG INTERNATIONAL TRADING CO LIMITED");
			importerMock.Setup(m => m.ForeignCompanyID).Returns("HKBOARAM0001A");
			exportHeaderMock.Setup(m => m.Importer).Returns(importerMock.Object);
			#endregion

			#region EntryLine
			entryLine1Mock.Setup(x => x.HSDescription).Returns("USED EXCAVATOR");
			entryLine1Mock.Setup(x => x.BrandName).Returns("상표명");
			entryLine1Mock.Setup(x => x.InvoiceNo).Returns("999999999");
			entryLine1Mock.Setup(x => x.CountryOfOriginDeterminationRule).Returns("A");
			entryLine1Mock.Setup(x => x.CountryOfOriginLabelLocation).Returns("N");
			entryLine1Mock.Setup(x => x.CertificateOfOriginIssued).Returns("Y");
			entryLine1Mock.Setup(x => x.QtyUnit).Returns("CT");
			entryLine1Mock.Setup(x => x.Qty).Returns(1292529.99m);
			entryLine1Mock.Setup(x => x.PackQty).Returns(3);
			entryLine1Mock.Setup(x => x.PackType).Returns("CT");
			entryLine1Mock.Setup(x => x.ImportDeclarationNumber).Returns("A");
			entryLine1Mock.Setup(x => x.ImportEntryLineNo).Returns("001");
			entryLine1Mock.Setup(x => x.SkipManifestReporting).Returns("N");
			entryLine1Mock.Setup(x => x.FTAType).Returns("106");
			entryLine1Mock.Setup(x => x.DocumentAttached).Returns("N");
			entryLine1Mock.Setup(x => x.NetWeightUQ).Returns("KG");

			var entryLine2Mock = new Mock<IExportEntryLine>();
			entryLine2Mock.Setup(x => x.EntryLineNo).Returns("002");
			entryLine2Mock.Setup(x => x.HSCode).Returns("899999999");
			entryLine2Mock.Setup(x => x.TradeName).Returns("거래품명2");
			entryLine2Mock.Setup(x => x.CountryOfOrigin).Returns("KR");
			entryLine2Mock.Setup(x => x.NetWeightInKG).Returns(1299999.99m);
			entryLine2Mock.Setup(x => x.NetWeightUQ).Returns("KG");
			entryLine2Mock.Setup(x => x.CustomsValue).Returns(8599999.99m);
			entryLine2Mock.Setup(x => x.PreApprovalType).Returns("A");
			entryLine2Mock.Setup(x => x.PreApprovalNo).Returns("KR00101010101");
			entryLine2Mock.Setup(x => x.PreApprovalEffectiveFromDate).Returns(new ZDate("2019-01-01"));
			entryLine2Mock.Setup(x => x.PreApprovalEffectiveToDate).Returns(new ZDate("2019-07-01"));
			entryLine2Mock.Setup(x => x.InvoiceLines).Returns(System.Array.Empty<IExportInvoiceLine>());
			entryLine2Mock.Setup(x => x.InvoiceLines).Returns(System.Array.Empty<IExportInvoiceLine>());
			entryLine2Mock.Setup(x => x.HSDescription).Returns("품명2");
			entryLine2Mock.Setup(x => x.BrandName).Returns("상표명2");
			entryLine2Mock.Setup(x => x.InvoiceNo).Returns("899999999");
			entryLine2Mock.Setup(x => x.CountryOfOriginDeterminationRule).Returns("A");
			entryLine2Mock.Setup(x => x.CountryOfOriginLabelLocation).Returns("N");
			entryLine2Mock.Setup(x => x.CertificateOfOriginIssued).Returns("Y");
			entryLine2Mock.Setup(x => x.QtyUnit).Returns("CT");
			entryLine2Mock.Setup(x => x.Qty).Returns(1292529.99m);
			entryLine2Mock.Setup(x => x.PackQty).Returns(3);
			entryLine2Mock.Setup(x => x.PackType).Returns("CT");
			entryLine2Mock.Setup(x => x.ImportDeclarationNumber).Returns("A");
			entryLine2Mock.Setup(x => x.ImportEntryLineNo).Returns("001");
			entryLine2Mock.Setup(x => x.SkipManifestReporting).Returns("N");
			entryLine2Mock.Setup(x => x.FTAType).Returns("105");
			entryLine2Mock.Setup(x => x.DocumentAttached).Returns("N");

			exportHeaderMock.Setup(m => m.EntryLines).Returns(new IExportEntryLine[] { entryLine1Mock.Object, entryLine2Mock.Object });
			#endregion

			#region IExportInvoiceLine (Line1)
			var exportInvoiceLine11Mock = new Mock<IExportInvoiceLine>();
			exportInvoiceLine11Mock.Setup(m => m.InvoiceLineNo).Returns("01");
			exportInvoiceLine11Mock.Setup(m => m.UnitPrice).Returns(27670.000000m);
			exportInvoiceLine11Mock.Setup(m => m.Amount).Returns(27670m);
			exportInvoiceLine11Mock.Setup(m => m.DetailDescription).Returns("2007 N81011141");

			exportInvoiceLine11Mock.Setup(m => m.QtyOrWeight).Returns(1m);
			exportInvoiceLine11Mock.Setup(m => m.QtyOrWeightUnit).Returns("U");
			exportInvoiceLine11Mock.Setup(m => m.Ingredient).Returns("dfewe");
			exportInvoiceLine11Mock.Setup(m => m.LotNumber).Returns("AbcdZZZ");

			var exportInvoiceLine12Mock = new Mock<IExportInvoiceLine>();
			exportInvoiceLine12Mock.Setup(m => m.InvoiceLineNo).Returns("02");
			exportInvoiceLine12Mock.Setup(m => m.UnitPrice).Returns(4564m);
			exportInvoiceLine12Mock.Setup(m => m.Amount).Returns(899999.25m);
			exportInvoiceLine12Mock.Setup(m => m.DetailDescription).Returns("모델규격2");

			exportInvoiceLine12Mock.Setup(m => m.QtyOrWeight).Returns(1234);
			exportInvoiceLine12Mock.Setup(m => m.QtyOrWeightUnit).Returns("KG");
			exportInvoiceLine12Mock.Setup(m => m.Ingredient).Returns("성분2");
			exportInvoiceLine12Mock.Setup(m => m.LotNumber).Returns("ZZZZEEE");
			exportInvoiceLine12Mock.Setup(m => m.GAApprovalDocuments).Returns(System.Array.Empty<IExportGAApprovalDocument>());

			entryLine1Mock.Setup(m => m.InvoiceLines).Returns(new IExportInvoiceLine[] { exportInvoiceLine11Mock.Object, exportInvoiceLine12Mock.Object });
			#endregion

			#region IExportGAApprovalDocument (Line1_exportInvoiceLine1)
			var exportGAApprovalDocument111Mock = new Mock<IExportGAApprovalDocument>();
			exportGAApprovalDocument111Mock.Setup(m => m.RegulationCategoryCode).Returns("05");
			exportGAApprovalDocument111Mock.Setup(m => m.SequenceNo).Returns("01");
			exportGAApprovalDocument111Mock.Setup(m => m.RequirementType).Returns("1");
			exportGAApprovalDocument111Mock.Setup(m => m.RequirementApprovalNumber).Returns("NO");

			exportGAApprovalDocument111Mock.Setup(x => x.ApprovalDate).Returns(new ZDate("2021-02-26"));
			exportGAApprovalDocument111Mock.Setup(x => x.RequirementDocumentType).Returns("A");
			exportGAApprovalDocument111Mock.Setup(x => x.DocumentName).Returns("식품등의 수입신고확인증");
			exportGAApprovalDocument111Mock.Setup(x => x.ReasonForMissingApprovalNumber).Returns("테스트");
			exportGAApprovalDocument111Mock.Setup(x => x.UniqueItemID).Returns("TEST");
			exportGAApprovalDocument111Mock.Setup(x => x.NonGAReasonType).Returns("05101");

			var exportGAApprovalDocument112Mock = new Mock<IExportGAApprovalDocument>();
			exportGAApprovalDocument112Mock.Setup(x => x.RegulationCategoryCode).Returns("06");
			exportGAApprovalDocument112Mock.Setup(x => x.SequenceNo).Returns("899999999");
			exportGAApprovalDocument112Mock.Setup(x => x.RequirementType).Returns("2");
			exportGAApprovalDocument112Mock.Setup(x => x.RequirementApprovalNumber).Returns("899999999");

			exportGAApprovalDocument112Mock.Setup(x => x.ApprovalDate).Returns(new ZDate("2015-01-01"));
			exportGAApprovalDocument112Mock.Setup(x => x.RequirementDocumentType).Returns("A");
			exportGAApprovalDocument112Mock.Setup(x => x.DocumentName).Returns("발급서류명2");
			exportGAApprovalDocument112Mock.Setup(x => x.ReasonForMissingApprovalNumber).Returns("사유2");
			exportGAApprovalDocument112Mock.Setup(x => x.UniqueItemID).Returns("586455632");
			exportGAApprovalDocument112Mock.Setup(x => x.NonGAReasonType).Returns("06202");
			exportInvoiceLine11Mock.Setup(x => x.GAApprovalDocuments).Returns(new IExportGAApprovalDocument[] { exportGAApprovalDocument111Mock.Object, exportGAApprovalDocument112Mock.Object });
			#endregion

			#region IExportVehicleNo (Line1_exportInvoiceLine1_exportVehicleNo1)
			var exportVehicleNo111Mock = new Mock<IExportVehicleNo>();
			exportVehicleNo111Mock.Setup(m => m.SequenceNo).Returns("001");
			exportVehicleNo111Mock.Setup(m => m.VIN).Returns("KN3HNP6N18K283119");

			var exportVehicleNo112Mock = new Mock<IExportVehicleNo>();
			exportVehicleNo112Mock.Setup(m => m.SequenceNo).Returns("002");
			exportVehicleNo112Mock.Setup(m => m.VIN).Returns("CCCCZZZ");
			exportInvoiceLine11Mock.Setup(m => m.VehicleNumbers).Returns(new IExportVehicleNo[] { exportVehicleNo111Mock.Object, exportVehicleNo112Mock.Object });
			#endregion

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ExportOutgoingTestFilePath, "GOVCBR830_0.xml"));
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(stream.WriteToString(), serialisedXml);
				}
			}

			//todo: XML 고쳐야함

			AssertEquals("13010", result.DeclarationOfficeId.Value);
			AssertEquals(2m, result.GoodsItemQuantity.Value);
			AssertEquals("6N00221000025X", result.Id.Value);
			AssertEquals(25987293m, result.InvoiceAmount.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals(29600m, result.TotalGrossMassMeasure.Value);
			AssertEquals("KG", result.TotalGrossMassMeasure.KcsUnitCode);
			AssertEquals(1m, result.TotalPackageQuantity.Value);
			AssertEquals("GOVCBR830", result.TypeCode.Value);
			AssertEquals("11", result.TransactionNatureCode.Value);
			AssertEquals("AB", result.ResponseTypeCode.Value);
			AssertEquals("ZZ", result.AdditionalCode.ReturnReasonCode.Value);
			AssertEquals("A", result.AdditionalCode.ReturnScopeCode.Value);
			AssertEquals("B", result.AdditionalInformation.StatementCode.Value);
			AssertEquals("신고인 기재란", result.AdditionalInformation.StatementDescription.Value);
			AssertEquals("2014010120140101", result.AdditionalInformation.PeriodDateTime);
			AssertEquals("레디코리1971018", result.Agent.Id[0].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Item380, result.Agent.Id[0].SchemeAgencyId);
			AssertEquals("899999999", result.Agent.Id[1].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Kts, result.Agent.Id[1].SchemeAgencyId);
			AssertEquals("레디코리아", result.Agent.Name.Value);
			AssertEquals("AA9999", result.BorderTransportMeans.Name.Value);
			AssertEquals("20140101", result.BorderTransportMeans.EstimatedDepartureDateTime);
			AssertEquals("01", result.BorderTransportMeans.TransportEquipment[0].SequenceNumeric.Value);
			AssertEquals("999999999999999", result.BorderTransportMeans.TransportEquipment[0].Id.Value);
			AssertEquals("KE", result.Carrier.Id.Value);
			AssertEquals("대한항공", result.Carrier.Name.Value);
			AssertEquals("나대표", result.Carrier.Contact.Name.Value);
			AssertEquals(1084.25m, result.CurrencyExchange.RateNumeric);
			AssertEquals("H", result.CustomsProcedure.ProcessTypeCode.Value);
			AssertEquals(Iso3AlphaCurrencyCodeContentType.Usd, result.Consignment.ConsignmentItem.Commodity.ValueAmount.CurrencyId);
			AssertEquals(27670m, result.Consignment.ConsignmentItem.Commodity.ValueAmount.Value);
			AssertEquals("9999999999", result.Consignment.ConsignmentItem.PreviousDocument.Id.Value);
			AssertEquals("99999999", result.Consignment.GoodsLocation.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.Control.InspectionStartDateTime);
			AssertEquals(AgencyIdentificationCodeContentType.Zzz, result.Exporter.Id[0].SchemeAgencyId);
			AssertEquals("1028142299", result.Exporter.Id[0].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Item380, result.Exporter.Id[1].SchemeAgencyId);
			AssertEquals("레디코리1971018", result.Exporter.Id[1].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Kts, result.Exporter.Id[2].SchemeAgencyId);
			AssertEquals("899999999", result.Exporter.Id[2].Value);
			AssertEquals("레디코리아", result.Exporter.Name.Value);
			AssertEquals("04", result.Exporter.RoleCode.Value);
			AssertEquals("C", result.Exporter.TypeCode.Value);
			AssertEquals("101010", result.Exporter.Address.CountrySubDivisionId.Value);
			AssertEquals("레디인빌딩", result.Exporter.Address.Line.Value);
			AssertEquals("06561", result.Exporter.Address.PostcodeId.Value);
			AssertEquals("020120", result.Exporter.Address.BuildingNumber.Value);
			AssertEquals("서울특별시 서초구 동광로 41", result.Exporter.Address.Description.Value);
			AssertEquals("김택윤", result.Exporter.Contact.RepresentativeName.Value);
			AssertEquals("9999999", result.FinalTransportMeansLoadingPlace.Id.Value);
			AssertEquals("HKBOARAM0001A", result.GoodsShipment.Buyer.Id.Value);
			AssertEquals("BOA RAM HONGKONG INTERNATIONAL TRADING CO LIMITED", result.GoodsShipment.Buyer.Name.Value);
			AssertEquals("HK", result.GoodsShipment.Buyer.Address.CountryCode.Value);
			AssertEquals("1234567", result.GoodsShipment.AdditionalDocument.Id.Value);
			AssertEquals("990", result.GoodsShipment.ApprovedEstablishmentPlace.Name.Value);
			AssertEquals(false, result.GoodsShipment.Consignment.ContainerIndicator);
			AssertEquals("O", result.GoodsShipment.Consignment.GoodsStatusCode.Value);
			AssertEquals("10", result.GoodsShipment.Consignment.BorderTransportMeans.TypeCode.Value);
			AssertEquals("AA", result.GoodsShipment.Consignment.BorderTransportMeans.TransportEquipment.CharacteristicCode.Value);
			AssertEquals("08589", result.GoodsShipment.Consignment.GoodsLocation.Id.Value);
			AssertEquals(799999999m, result.GoodsShipment.CustomsValuation.ExitToEntryChargeAmount.Value);
			AssertEquals(899999999m, result.GoodsShipment.CustomsValuation.FreightChargeAmount.Value);
			AssertEquals("1", result.GoodsShipment.DrawBack.RoleCode.Value);
			AssertEquals("NO", result.GoodsShipment.DrawBack.ApplicationTypeCode.Value);
			AssertEquals("001", result.GoodsShipment.GovernmentAgencyGoodsItem[0].SequenceNumeric.Value);
			AssertEquals("N", result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalCode.AttachmentIndicatorCode.Value);
			AssertEquals("Y", result.GoodsShipment.GovernmentAgencyGoodsItem[1].AdditionalInformation.StatementCode.Value);
			AssertEquals("N", result.GoodsShipment.GovernmentAgencyGoodsItem[1].AdditionalInformation.StatementTypeCode.Value);
			AssertEquals("거래품명2", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.CargoDescription.Value);
			AssertEquals(1292529.99m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.CountQuantity.Value);
			AssertEquals("CT", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.CountQuantity.KcsUnitCode);
			AssertEquals("품명2", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.Description.Value);
			AssertEquals("상표명2", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.Name.Value);
			AssertEquals(8599999.99m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.ValueAmount.Value);
			AssertEquals("KR00101010101", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.AdditionalInformation.Id.Value);
			AssertEquals("A", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.AdditionalInformation.StatementCode.Value);
			AssertEquals("20190101", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.AdditionalInformation.BeginningDateTime);
			AssertEquals("20190701", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.AdditionalInformation.EndingDateTime);
			AssertEquals("899999999", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.Classification.Id.Value);
			AssertEquals("899999999", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.Invoice.Id.Value);
			AssertEquals("A", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.PreviousDocument.Id.Value);
			AssertEquals("001", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.PreviousDocument.LineNumeric.Value);
			AssertEquals("106", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CriteriaConformanceCode.Value);
			AssertEquals("105", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.CriteriaConformanceCode.Value);
			AssertEquals("05101", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[0].NonObjectReasonCode.Value);
			AssertEquals("06202", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[1].NonObjectReasonCode.Value);
			AssertEquals("KG", result.GoodsShipment.GovernmentAgencyGoodsItem[1].GoodsMeasure.NetNetWeightMeasure.KcsUnitCode);
			AssertEquals(1299999.99m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].GoodsMeasure.NetNetWeightMeasure.Value);
			AssertEquals("KR", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Origin.CountryCode.Value);
			AssertEquals("A", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Origin.RuleCode.Value);
			AssertEquals("N", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Origin.OriginDescription.DisplayIndicatorCode.Value);
			AssertEquals(3m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].Packaging.QuantityQuantity.Value);
			AssertEquals("CT", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Packaging.TypeCode.Value);
			AssertEquals("CFR", result.GoodsShipment.TradeTerms.ConditionCode.Value);
			AssertEquals("AA", result.GoodsShipment.TradeTerms.SettlementConditionCode.Value);
			AssertEquals("(SK)", result.GoodsShipment.Warehouse.Name.Value);
			AssertEquals("서울 금천구 가산디지털1로 119", result.GoodsShipment.Consignment.GoodsLocation.Address.Description.Value);
			AssertEquals(AgencyIdentificationCodeContentType.Item380, result.Manufacturer.Id[0].SchemeAgencyId);
			AssertEquals("제조미상9999000", result.Manufacturer.Id[0].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Kts, result.Manufacturer.Id[1].SchemeAgencyId);
			AssertEquals("899999999", result.Manufacturer.Id[1].Value);
			AssertEquals("제조미상", result.Manufacturer.Name.Value);
			AssertEquals("04784", result.Manufacturer.Address.PostcodeId.Value);
			AssertEquals("KRINC", result.LoadingLocation.Id.Value);
			AssertEquals("10", result.LoadingLocation.TypeCode.Value);
			AssertEquals("OU", result.Packaging.TypeCode.Value);
			AssertEquals("16HJSC0686I0008", result.PreviousDocument.Id.Value);
			AssertEquals("999999", result.SouthNorthTrade.IdentificationId.Value);
			AssertEquals("N", result.SouthNorthTrade.TradeIndicatorCode.Value);
			AssertEquals("6N002", result.Submitter.Id.Value);
			AssertEquals("레디코리아", result.Submitter.Name.Value);
			AssertEquals("김환태", result.Submitter.Contact.RepresentativeName.Value);
			AssertEquals("99999999999999999", result.Ucr.Id.Value);
			exportHeaderMock.VerifyAll();
			entryLine1Mock.VerifyAll();
			exportContainer1.VerifyAll();
			declarantMock.VerifyAll();
			exporterMock.VerifyAll();
			supplierMock.VerifyAll();
			manufacturerMock.VerifyAll();
			importerMock.VerifyAll();
			entryLine2Mock.VerifyAll();
			exportInvoiceLine11Mock.VerifyAll();
			exportInvoiceLine12Mock.VerifyAll();
			exportGAApprovalDocument111Mock.VerifyAll();
			exportGAApprovalDocument112Mock.VerifyAll();
			exportVehicleNo111Mock.VerifyAll();
			exportVehicleNo112Mock.VerifyAll();
		}

		public void TestEmptyManufacturer()
		{
			AssertNull(exportHeaderMock.Object.Manufacturer);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.Manufacturer);
			AssertNotNull(result.Manufacturer.Name);
			AssertEquals(ZString.Empty, result.Manufacturer.Name.Value);

			AssertNotNull(result.Manufacturer.Address);
			AssertNotNull(result.Manufacturer.Address.PostcodeId);
			AssertEquals(ZString.Empty, result.Manufacturer.Address.PostcodeId.Value);

			AssertNull(result.Manufacturer.Id);

			var manufacturerMock = new Mock<IOrganization>();
			manufacturerMock.Setup(m => m.CompanyName).Returns("제조사");
			manufacturerMock.Setup(m => m.Postcode).Returns("04784");
			manufacturerMock.Setup(m => m.ForeignCompanyID).Returns("BXXXXXX");
			manufacturerMock.Setup(m => m.KoreanRegNoForForeigner).Returns("KXXXXXX");
			exportHeaderMock.Setup(m => m.Manufacturer).Returns(manufacturerMock.Object);

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("제조사", result.Manufacturer.Name.Value);
			AssertEquals("04784", result.Manufacturer.Address.PostcodeId.Value);
			AssertNull("If it is not '08', '06' ID type, it is not created.", result.Manufacturer.Id);

			manufacturerMock.Setup(m => m.OfficeID).Returns("8999999");
			manufacturerMock.Setup(m => m.UnipassIDForOrganization).Returns("제조사9999000");
			exportHeaderMock.Setup(m => m.Manufacturer).Returns(manufacturerMock.Object);

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("제조사9999000", result.Manufacturer.Id[0].Value);
			AssertEquals("8999999", result.Manufacturer.Id[1].Value);
		}

		public void TestEmptyLoadingLocation()
		{
			AssertEquals(ZString.Empty, exportHeaderMock.Object.PortOfLoading);
			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.LoadingLocation);
			AssertNull(result.LoadingLocation.Id);
			AssertNotNull(result.LoadingLocation.TypeCode);
			AssertEquals("If LoadingLocation.TypeCode is empty or length is not greater than 3, it is fixed to 40.", "40", result.LoadingLocation.TypeCode.Value);

			exportHeaderMock.Setup(m => m.PortOfLoading).Returns("KRINC");
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("KRINC", result.LoadingLocation.Id.Value);
			AssertEquals("10", result.LoadingLocation.TypeCode.Value);
		}

		public void TestEmptyPacking()
		{
			exportHeaderMock.Setup(x => x.PackType).Returns(ZString.Empty);
			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.Packaging.TypeCode);
			AssertEquals(ZString.Empty, result.Packaging.TypeCode.Value);

			exportHeaderMock.Setup(x => x.PackType).Returns("OU");
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("OU", result.Packaging.TypeCode.Value);
		}

		public void TestEmptyPreviousDocument()
		{
			exportHeaderMock.Setup(x => x.CargoManagement.ImportCargoManagementNumber).Returns(ZString.Empty);
			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.PreviousDocument);

			exportHeaderMock.Setup(x => x.CargoManagement.ImportCargoManagementNumber).Returns("16HJSC0686I0008");
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("16HJSC0686I0008", result.PreviousDocument.Id.Value);
		}

		public void TestEmptySouthNorthTrade()
		{
			AssertEquals(ZString.Empty, exportHeaderMock.Object.SouthNorthTradeIdentification);
			AssertEquals(ZString.Empty, exportHeaderMock.Object.SouthNorthTradeYN);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.SouthNorthTrade);

			exportHeaderMock.Setup(x => x.SouthNorthTradeIdentification).Returns("999999");
			exportHeaderMock.Setup(x => x.SouthNorthTradeYN).Returns(ZString.Empty);
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("999999", result.SouthNorthTrade.IdentificationId.Value);
			AssertNull(result.SouthNorthTrade.TradeIndicatorCode);

			exportHeaderMock.Setup(x => x.SouthNorthTradeIdentification).Returns(ZString.Empty);
			exportHeaderMock.Setup(x => x.SouthNorthTradeYN).Returns("N");
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.SouthNorthTrade.IdentificationId);
			AssertEquals("N", result.SouthNorthTrade.TradeIndicatorCode.Value);
		}

		public void TestEmptySubmitter()
		{
			AssertNull(exportHeaderMock.Object.Declarant);
			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.Submitter);
			AssertEquals(ZString.Empty, result.Submitter.Id.Value);
			AssertEquals(ZString.Empty, result.Submitter.Name.Value);
			AssertEquals(ZString.Empty, result.Submitter.Contact.RepresentativeName.Value);

			var declarantMock = new Mock<IOrganization>();
			declarantMock.Setup(m => m.CompanyName).Returns("레디코리아");
			declarantMock.Setup(m => m.RepresentativeName).Returns("대표자");
			exportHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			exportHeaderMock.Setup(m => m.UnipassDeclarantID).Returns("6N002");

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("6N002", result.Submitter.Id.Value);
			AssertEquals("레디코리아", result.Submitter.Name.Value);
			AssertEquals("대표자", result.Submitter.Contact.RepresentativeName.Value);
		}

		public void TestEmptyUCR()
		{
			exportHeaderMock.Setup(m => m.UCR).Returns(ZString.Empty);
			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.Ucr);

			exportHeaderMock.Setup(m => m.UCR).Returns("UCR");
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("UCR", result.Ucr.Id.Value);
		}

		public void TestEmptyBuyer()
		{
			AssertNull(exportHeaderMock.Object.Importer);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.GoodsShipment.Buyer);
			AssertNull(result.GoodsShipment.Buyer.Id);
			AssertNotNull(result.GoodsShipment.Buyer.Name);
			AssertNotNull(result.GoodsShipment.Buyer.Address);

			var buyerMock = new Mock<IOrganization>();
			buyerMock.Setup(m => m.CompanyName).Returns("구매자");
			exportHeaderMock.Setup(m => m.CountryOfDestination).Returns("KR");
			buyerMock.Setup(m => m.BusinessRegNo).Returns("XXXXXX");
			exportHeaderMock.Setup(m => m.Importer).Returns(buyerMock.Object);

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("구매자", result.GoodsShipment.Buyer.Name.Value);
			AssertEquals("KR", result.GoodsShipment.Buyer.Address.CountryCode.Value);
			AssertNull("If it is not '07' ID type, it is not created.", result.GoodsShipment.Buyer.Id);

			buyerMock.Setup(m => m.ForeignCompanyID).Returns("BuyerID");
			exportHeaderMock.Setup(m => m.Manufacturer).Returns(buyerMock.Object);

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("BuyerID", result.GoodsShipment.Buyer.Id.Value);
		}

		public void TestEmptyAdditionalDocument()
		{
			AssertEquals(ZString.Empty, exportHeaderMock.Object.LCNo);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.AdditionalDocument);

			exportHeaderMock.Setup(m => m.LCNo).Returns("LC No.");
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("LC No.", result.GoodsShipment.AdditionalDocument.Id.Value);
		}

		public void TestEmptyApprovedEstablishmentPlace()
		{
			AssertEquals(ZString.Empty, exportHeaderMock.Object.IndustrialParkCode);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.ApprovedEstablishmentPlace);

			exportHeaderMock.Setup(m => m.IndustrialParkCode).Returns("IPC");
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("IPC", result.GoodsShipment.ApprovedEstablishmentPlace.Name.Value);
		}

		public void TestEmptyConsignment()
		{
			exportHeaderMock.Setup(m => m.GoodsLocationPostcode).Returns(ZString.Empty);
			exportHeaderMock.Setup(m => m.GoodsLocationAddress).Returns(ZString.Empty);
			AssertEquals(false, exportHeaderMock.Object.ContainerizedIndicator);
			AssertEquals(ZString.Empty, exportHeaderMock.Object.GoodsStatus);
			AssertEquals(ZString.Empty, exportHeaderMock.Object.TransportMode);
			AssertEquals(ZString.Empty, exportHeaderMock.Object.ContainerPackMode);
			AssertEquals(ZString.Empty, exportHeaderMock.Object.GoodsLocationPostcode);
			AssertEquals(ZString.Empty, exportHeaderMock.Object.GoodsLocationAddress);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.GoodsShipment.Consignment);
			AssertNotNull(result.GoodsShipment.Consignment.ContainerIndicator);
			AssertNull(result.GoodsShipment.Consignment.GoodsStatusCode);
			AssertNull(result.GoodsShipment.Consignment.BorderTransportMeans);
			AssertNotNull(result.GoodsShipment.Consignment.GoodsLocation);
			AssertNotNull(result.GoodsShipment.Consignment.GoodsLocation.Id);
			AssertNotNull(result.GoodsShipment.Consignment.GoodsLocation.Address.Description);

			exportHeaderMock.Setup(m => m.GoodsStatus).Returns("A");
			exportHeaderMock.Setup(m => m.TransportMode).Returns("SEA");
			exportHeaderMock.Setup(m => m.ContainerPackMode).Returns("PL");
			exportHeaderMock.Setup(m => m.GoodsLocationPostcode).Returns("12345");
			exportHeaderMock.Setup(m => m.GoodsLocationAddress).Returns("GoodsLocationAddress");
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals(false, result.GoodsShipment.Consignment.ContainerIndicator.Value);
			AssertEquals("A", result.GoodsShipment.Consignment.GoodsStatusCode.Value);
			AssertEquals("SEA", result.GoodsShipment.Consignment.BorderTransportMeans.TypeCode.Value);
			AssertEquals("PL", result.GoodsShipment.Consignment.BorderTransportMeans.TransportEquipment.CharacteristicCode.Value);
			AssertEquals("12345", result.GoodsShipment.Consignment.GoodsLocation.Id.Value);
			AssertEquals("GoodsLocationAddress", result.GoodsShipment.Consignment.GoodsLocation.Address.Description.Value);
		}

		public void TestEmptyCustomsValuation()
		{
			AssertEquals(ZDecimal.Zero, exportHeaderMock.Object.Freight);
			AssertEquals(ZDecimal.Zero, exportHeaderMock.Object.Insurance);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.CustomsValuation);

			exportHeaderMock.Setup(m => m.Freight).Returns(1);
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals(1m, result.GoodsShipment.CustomsValuation.FreightChargeAmount.Value);
			AssertNull(result.GoodsShipment.CustomsValuation.ExitToEntryChargeAmount);

			exportHeaderMock.Setup(m => m.Freight).Returns(0);
			exportHeaderMock.Setup(m => m.Insurance).Returns(2);
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals(2m, result.GoodsShipment.CustomsValuation.ExitToEntryChargeAmount.Value);
			AssertNull(result.GoodsShipment.CustomsValuation.FreightChargeAmount);
		}

		public void TestEmptyDrawBack()
		{
			exportHeaderMock.Setup(m => m.ApplicationForSimpleDrawback).Returns(ZString.Empty);
			AssertEquals(ZString.Empty, exportHeaderMock.Object.DrawbackApplicantType);
			AssertEquals(ZString.Empty, exportHeaderMock.Object.ApplicationForSimpleDrawback);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.GoodsShipment.DrawBack);
			AssertNotNull(result.GoodsShipment.DrawBack.ApplicationTypeCode);
			AssertNull(result.GoodsShipment.DrawBack.RoleCode);

			exportHeaderMock.Setup(m => m.ApplicationForSimpleDrawback).Returns("AD");
			exportHeaderMock.Setup(m => m.DrawbackApplicantType).Returns("1");
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("1", result.GoodsShipment.DrawBack.RoleCode.Value);
			AssertEquals("AD", result.GoodsShipment.DrawBack.ApplicationTypeCode.Value);
		}

		public void TestEmptyTradeTerms()
		{
			exportHeaderMock.Setup(m => m.Incoterm).Returns(ZString.Empty);
			AssertEquals(ZString.Empty, exportHeaderMock.Object.Incoterm);
			AssertEquals(ZString.Empty, exportHeaderMock.Object.InvoicePaymentTerm);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.GoodsShipment.TradeTerms);
			AssertNotNull(result.GoodsShipment.TradeTerms.ConditionCode);
			AssertNull(result.GoodsShipment.TradeTerms.SettlementConditionCode);

			exportHeaderMock.Setup(m => m.Incoterm).Returns("FOB");
			exportHeaderMock.Setup(m => m.InvoicePaymentTerm).Returns("LC");
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("FOB", result.GoodsShipment.TradeTerms.ConditionCode.Value);
			AssertEquals("LC", result.GoodsShipment.TradeTerms.SettlementConditionCode.Value);
		}

		public void TestEmptyWarehouse()
		{
			exportHeaderMock.Setup(m => m.GoodsLocationAdditionalDetails).Returns(ZString.Empty);
			AssertEquals(ZString.Empty, exportHeaderMock.Object.GoodsLocationAdditionalDetails);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.GoodsShipment.Warehouse);

			exportHeaderMock.Setup(m => m.GoodsLocationAdditionalDetails).Returns("Warehouse");
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("Warehouse", result.GoodsShipment.Warehouse.Name.Value);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 03, 03)]
		public void TestEmptyBorderTransportMeans()
		{
			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.BorderTransportMeans);

			#region Containers
			var exportContainer1 = new Mock<IExportContainer>();
			exportContainer1.Setup(m => m.SequenceNo).Returns("01");
			var exportContainer2 = new Mock<IExportContainer>();
			exportContainer2.Setup(m => m.SequenceNo).Returns("02");
			exportContainer2.Setup(m => m.ContainerNo).Returns("B4");

			exportHeaderMock.Setup(m => m.Containers).Returns(new IExportContainer[] { exportContainer1.Object, exportContainer2.Object });
			#endregion

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals(2, result.BorderTransportMeans.TransportEquipment.Count);
			AssertEquals(null, result.BorderTransportMeans.TransportEquipment[0].Id);
			AssertEquals("01", result.BorderTransportMeans.TransportEquipment[0].SequenceNumeric.Value);
			AssertEquals("B4", result.BorderTransportMeans.TransportEquipment[1].Id.Value);
			AssertEquals("02", result.BorderTransportMeans.TransportEquipment[1].SequenceNumeric.Value);
		}

		public void TestEntryLineConditionalElementsNotCreated()
		{
			SetEmptyData_EntryLine();

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Description);
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Name);
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Invoice);
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation);
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Origin.RuleCode);
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Origin.OriginDescription);
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CriteriaConformanceCode);
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CountQuantity);
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Packaging);
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument);
		}

		public void TestDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalInformation()
		{
			SetEmptyData_EntryLine();
			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation);

			entryLine1Mock.Setup(x => x.PreApprovalNo).Returns("수출승인요건번호");
			entryLine1Mock.Setup(x => x.PreApprovalEffectiveFromDate).Returns(ZDate.Invalid);
			entryLine1Mock.Setup(x => x.PreApprovalEffectiveToDate).Returns(ZDate.Invalid);
			exportHeaderMock.Setup(m => m.EntryLines).Returns(new IExportEntryLine[] { entryLine1Mock.Object });
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("수출승인요건번호", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.Id.Value);
			AssertEquals("A", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.StatementCode.Value);
			AssertEquals(ZString.Empty, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.BeginningDateTime);
			AssertEquals(ZString.Empty, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.EndingDateTime);

			entryLine1Mock.Setup(x => x.PreApprovalNo).Returns(ZString.Empty);
			entryLine1Mock.Setup(x => x.PreApprovalEffectiveFromDate).Returns(new ZDate("2023-01-01"));
			entryLine1Mock.Setup(x => x.PreApprovalEffectiveToDate).Returns(ZDate.Invalid);
			exportHeaderMock.Setup(m => m.EntryLines).Returns(new IExportEntryLine[] { entryLine1Mock.Object });
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals(ZString.Empty, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.Id.Value);
			AssertEquals("A", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.StatementCode.Value);
			AssertEquals("20230101", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.BeginningDateTime);
			AssertEquals(ZString.Empty, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.EndingDateTime);

			entryLine1Mock.Setup(x => x.PreApprovalNo).Returns(ZString.Empty);
			entryLine1Mock.Setup(x => x.PreApprovalEffectiveFromDate).Returns(ZDate.Invalid);
			entryLine1Mock.Setup(x => x.PreApprovalEffectiveToDate).Returns(new ZDate("2023-01-01"));
			exportHeaderMock.Setup(m => m.EntryLines).Returns(new IExportEntryLine[] { entryLine1Mock.Object });
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals(ZString.Empty, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.Id.Value);
			AssertEquals("A", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.StatementCode.Value);
			AssertEquals(ZString.Empty, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.BeginningDateTime);
			AssertEquals("20230101", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.EndingDateTime);
		}

		public void TestCountQtyAndPackQtyInEntryLine()
		{
			SetEmptyData_EntryLine();

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CountQuantity);
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Packaging);

			entryLine1Mock.Setup(x => x.Qty).Returns(10);
			entryLine1Mock.Setup(x => x.PackQty).Returns(20);
			exportHeaderMock.Setup(m => m.EntryLines).Returns(new IExportEntryLine[] { entryLine1Mock.Object });
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals(10m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CountQuantity.Value);
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CountQuantity.KswUnitCode);
			AssertEquals(20m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Packaging.QuantityQuantity.Value);
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Packaging.TypeCode);

			entryLine1Mock.Setup(x => x.Qty).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(x => x.QtyUnit).Returns("U");
			entryLine1Mock.Setup(x => x.PackQty).Returns(ZInt.Zero);
			entryLine1Mock.Setup(x => x.PackType).Returns("BL");
			exportHeaderMock.Setup(m => m.EntryLines).Returns(new IExportEntryLine[] { entryLine1Mock.Object });
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals(ZDecimal.Zero, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CountQuantity.Value);
			AssertEquals("U", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CountQuantity.KcsUnitCode);
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Packaging.QuantityQuantity);
			AssertEquals("BL", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Packaging.TypeCode.Value);
		}

		public void TestDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityPreviousDocument()
		{
			SetEmptyData_EntryLine();

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument);

			entryLine1Mock.Setup(x => x.ImportDeclarationNumber).Returns("신고번호");
			exportHeaderMock.Setup(m => m.EntryLines).Returns(new IExportEntryLine[] { entryLine1Mock.Object });
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument);
			AssertEquals("신고번호", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument.Id.Value);
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument.LineNumeric);

			entryLine1Mock.Setup(x => x.ImportDeclarationNumber).Returns(ZString.Empty);
			entryLine1Mock.Setup(x => x.ImportEntryLineNo).Returns("001");
			exportHeaderMock.Setup(m => m.EntryLines).Returns(new IExportEntryLine[] { entryLine1Mock.Object });
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument);
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument.Id);
			AssertEquals("001", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument.LineNumeric.Value);

			entryLine1Mock.Setup(x => x.ImportDeclarationNumber).Returns("신고번호");
			entryLine1Mock.Setup(x => x.ImportEntryLineNo).Returns("001");
			exportHeaderMock.Setup(m => m.EntryLines).Returns(new IExportEntryLine[] { entryLine1Mock.Object });
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument);
			AssertEquals("신고번호", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument.Id.Value);
			AssertEquals("001", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument.LineNumeric.Value);
		}

		void SetEmptyData_EntryLine()
		{
			entryLine1Mock.Setup(x => x.HSDescription).Returns(ZString.Empty);
			entryLine1Mock.Setup(x => x.BrandName).Returns(ZString.Empty);
			entryLine1Mock.Setup(x => x.InvoiceNo).Returns(ZString.Empty);
			entryLine1Mock.Setup(x => x.CountryOfOriginDeterminationRule).Returns(ZString.Empty);
			entryLine1Mock.Setup(x => x.CountryOfOriginLabelLocation).Returns(ZString.Empty);
			entryLine1Mock.Setup(x => x.CertificateOfOriginIssued).Returns(ZString.Empty);
			entryLine1Mock.Setup(x => x.QtyUnit).Returns(ZString.Empty);
			entryLine1Mock.Setup(x => x.Qty).Returns(ZDecimal.Zero);
			entryLine1Mock.Setup(x => x.PackQty).Returns(ZInt.Zero);
			entryLine1Mock.Setup(x => x.PackType).Returns(ZString.Empty);
			entryLine1Mock.Setup(x => x.ImportDeclarationNumber).Returns(ZString.Empty);
			entryLine1Mock.Setup(x => x.ImportEntryLineNo).Returns(ZString.Empty);
			entryLine1Mock.Setup(x => x.SkipManifestReporting).Returns(ZString.Empty);
			entryLine1Mock.Setup(x => x.FTAType).Returns(ZString.Empty);
			entryLine1Mock.Setup(x => x.NetWeightUQ).Returns("KG");
			entryLine1Mock.Setup(x => x.DocumentAttached).Returns("N");
			entryLine1Mock.Setup(x => x.PreApprovalNo).Returns(ZString.Empty);
			entryLine1Mock.Setup(x => x.PreApprovalType).Returns("A");
			entryLine1Mock.Setup(x => x.PreApprovalEffectiveFromDate).Returns(ZDate.Invalid);
			entryLine1Mock.Setup(x => x.PreApprovalEffectiveToDate).Returns(ZDate.Invalid);

			exportHeaderMock.Setup(m => m.EntryLines).Returns(new IExportEntryLine[] { entryLine1Mock.Object });
		}

		public void TestInvoiceLineCondtionalElementsNotCreated()
		{
			exportHeaderMock.Setup(m => m.EntryLines).Returns(new IExportEntryLine[] { entryLine1Mock.Object });
			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity);

			var exportInvoiceLineMock = new Mock<IExportInvoiceLine>();
			entryLine1Mock.Setup(m => m.InvoiceLines).Returns(new IExportInvoiceLine[] { exportInvoiceLineMock.Object });

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument);
			AssertEquals(null, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].Vehicle);

			exportInvoiceLineMock.Setup(m => m.QtyOrWeight).Returns(ZDecimal.Zero);
			exportInvoiceLineMock.Setup(m => m.QtyOrWeightUnit).Returns(ZString.Empty);
			exportInvoiceLineMock.Setup(m => m.Ingredient).Returns(ZString.Empty);
			exportInvoiceLineMock.Setup(m => m.LotNumber).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.InvoiceLines).Returns(new IExportInvoiceLine[] { exportInvoiceLineMock.Object });

			var exportGAApprovalDocumentMock = new Mock<IExportGAApprovalDocument>();
			exportGAApprovalDocumentMock.Setup(x => x.ApprovalDate).Returns(ZDate.Empty);
			exportGAApprovalDocumentMock.Setup(x => x.RequirementDocumentType).Returns(ZString.Empty);
			exportGAApprovalDocumentMock.Setup(x => x.DocumentName).Returns(ZString.Empty);
			exportGAApprovalDocumentMock.Setup(x => x.ReasonForMissingApprovalNumber).Returns(ZString.Empty);
			exportGAApprovalDocumentMock.Setup(x => x.UniqueItemID).Returns(ZString.Empty);
			exportGAApprovalDocumentMock.Setup(x => x.NonGAReasonType).Returns(ZString.Empty);
			exportInvoiceLineMock.Setup(x => x.GAApprovalDocuments).Returns(new IExportGAApprovalDocument[] { exportGAApprovalDocumentMock.Object });

			var exportVehicleNoMock = new Mock<IExportVehicleNo>();
			exportVehicleNoMock.Setup(m => m.SequenceNo).Returns(ZString.Empty);
			exportVehicleNoMock.Setup(m => m.VIN).Returns(ZString.Empty);
			exportInvoiceLineMock.Setup(m => m.VehicleNumbers).Returns(new IExportVehicleNo[] { exportVehicleNoMock.Object });

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			var detailedCommodity = result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0];
			AssertEquals(ZDecimal.Zero, detailedCommodity.CountQuantity.Value);
			AssertEquals(ZString.Empty, detailedCommodity.CountQuantity.KcsUnitCode);
			AssertEquals(null, detailedCommodity.Constituent);
			AssertEquals(null, detailedCommodity.LotNumberId);

			AssertEquals(string.Empty, detailedCommodity.AdditionalDocument[0].IssueDateTime);
			AssertEquals(null, detailedCommodity.AdditionalDocument[0].TypeCode);
			AssertEquals(null, detailedCommodity.AdditionalDocument[0].Name);
			AssertEquals(null, detailedCommodity.AdditionalDocument[0].NonDescriptionReason);
			AssertEquals(null, detailedCommodity.AdditionalDocument[0].CommercialCategorizationId);
			AssertEquals(null, detailedCommodity.AdditionalDocument[0].NonObjectReasonCode);

			AssertNull(detailedCommodity.Vehicle);
		}

		public void TestQtyOrWeightInInvoiceLine()
		{
			var exportInvoiceLineMock = new Mock<IExportInvoiceLine>();
			entryLine1Mock.Setup(m => m.InvoiceLines).Returns(new IExportInvoiceLine[] { exportInvoiceLineMock.Object });

			exportHeaderMock.Setup(m => m.EntryLines).Returns(new IExportEntryLine[] { entryLine1Mock.Object });
			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].CountQuantity);
			AssertEquals(ZDecimal.Zero, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].CountQuantity.Value);
			AssertEquals(ZString.Empty, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].CountQuantity.KcsUnitCode);

			exportInvoiceLineMock.Setup(m => m.QtyOrWeight).Returns(10);
			exportInvoiceLineMock.Setup(m => m.QtyOrWeightUnit).Returns(ZString.Empty);
			entryLine1Mock.Setup(m => m.InvoiceLines).Returns(new IExportInvoiceLine[] { exportInvoiceLineMock.Object });
			exportHeaderMock.Setup(m => m.EntryLines).Returns(new IExportEntryLine[] { entryLine1Mock.Object });

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].CountQuantity);
			AssertEquals(10m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].CountQuantity.Value);
			AssertEquals(ZString.Empty, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].CountQuantity.KcsUnitCode);

			exportInvoiceLineMock.Setup(m => m.QtyOrWeight).Returns(ZDecimal.Zero);
			exportInvoiceLineMock.Setup(m => m.QtyOrWeightUnit).Returns("KG");
			entryLine1Mock.Setup(m => m.InvoiceLines).Returns(new IExportInvoiceLine[] { exportInvoiceLineMock.Object });
			exportHeaderMock.Setup(m => m.EntryLines).Returns(new IExportEntryLine[] { entryLine1Mock.Object });

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].CountQuantity);
			AssertEquals(ZDecimal.Zero, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].CountQuantity.Value);
			AssertEquals("KG", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].CountQuantity.KcsUnitCode);

			exportInvoiceLineMock.Setup(m => m.QtyOrWeight).Returns(10);
			exportInvoiceLineMock.Setup(m => m.QtyOrWeightUnit).Returns("KG");
			entryLine1Mock.Setup(m => m.InvoiceLines).Returns(new IExportInvoiceLine[] { exportInvoiceLineMock.Object });
			exportHeaderMock.Setup(m => m.EntryLines).Returns(new IExportEntryLine[] { entryLine1Mock.Object });

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].CountQuantity);
			AssertEquals(10m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].CountQuantity.Value);
			AssertEquals("KG", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].CountQuantity.KcsUnitCode);
		}

		public void TestNoExceptionWithInvalidCurrencyID()
		{
			#region exportHeader
			exportHeaderMock.Setup(m => m.ExportTypeCode).Returns("B");
			exportHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("130");
			exportHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("10");
			exportHeaderMock.Setup(m => m.PortOfLoading).Returns("KRINC");
			exportHeaderMock.Setup(m => m.SouthNorthTradeIdentification).Returns("999999");
			exportHeaderMock.Setup(m => m.FinalLoadingPlace).Returns("9999999");
			exportHeaderMock.Setup(m => m.GoodsLocationBondedAreaCode).Returns("99999999");
			exportHeaderMock.Setup(m => m.PreferredInspectionDate).Returns(ZDate.Today);
			exportHeaderMock.Setup(m => m.BondedTransportationFromDate).Returns(new ZDate("2014-01-01"));
			exportHeaderMock.Setup(m => m.BondedTransportationToDate).Returns(new ZDate("2014-01-01"));
			exportHeaderMock.Setup(m => m.DepartureDate).Returns(new ZDate("2014-01-01"));
			exportHeaderMock.Setup(m => m.DrawbackApplicantType).Returns("1");
			exportHeaderMock.Setup(m => m.InvoicePaymentTerm).Returns("AA");
			exportHeaderMock.Setup(m => m.OutOfHoursDeclarationIndicator).Returns("B");
			exportHeaderMock.Setup(m => m.ReturnReason).Returns("ZZ");
			exportHeaderMock.Setup(m => m.ReturnType).Returns("A");
			exportHeaderMock.Setup(m => m.GoodsStatus).Returns("O");
			exportHeaderMock.Setup(m => m.ContainerizedIndicator).Returns(false);
			exportHeaderMock.Setup(m => m.SouthNorthTradeYN).Returns("N");
			exportHeaderMock.Setup(m => m.ContainerPackMode).Returns("AA");
			exportHeaderMock.Setup(m => m.LCNo).Returns("1234567");
			exportHeaderMock.Setup(m => m.TransportMode).Returns("10");
			exportHeaderMock.Setup(m => m.UCR).Returns("99999999999999999");
			exportHeaderMock.Setup(m => m.LocationIDInBondedArea).Returns("9999999999");
			exportHeaderMock.Setup(m => m.FreightForwarderContactName).Returns("나대표");
			exportHeaderMock.Setup(m => m.CarrierID).Returns("KE");
			exportHeaderMock.Setup(m => m.ShippingLineOrAirlineName).Returns("대한항공");
			exportHeaderMock.Setup(m => m.VesselNameOrFlightNo).Returns("AA9999");
			exportHeaderMock.Setup(m => m.Freight).Returns(899999999m);
			exportHeaderMock.Setup(m => m.Insurance).Returns(799999999m);
			exportHeaderMock.Setup(m => m.ExchangeRate).Returns(1084.25m);
			exportHeaderMock.Setup(m => m.DeclarantAdditionalDescription).Returns("신고인 기재란");
			exportHeaderMock.Setup(m => m.IndustrialParkCode).Returns("999");
			#endregion

			#region Containers
			var exportContainer1 = new Mock<IExportContainer>();
			exportContainer1.Setup(m => m.SequenceNo).Returns("01");
			exportContainer1.Setup(m => m.ContainerNo).Returns("999999999999999");
			var exportContainer2 = new Mock<IExportContainer>();
			exportContainer2.Setup(m => m.SequenceNo).Returns("02");
			exportContainer2.Setup(m => m.ContainerNo).Returns("899999999999999");
			exportHeaderMock.Setup(m => m.Containers).Returns(new IExportContainer[] { exportContainer1.Object, exportContainer2.Object });
			#endregion

			#region Declarant
			var declarantMock = new Mock<IOrganization>();
			declarantMock.Setup(m => m.CompanyName).Returns("레디코리아");
			declarantMock.Setup(m => m.RepresentativeName).Returns("김환태");

			exportHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			exportHeaderMock.Setup(m => m.UnipassDeclarantID).Returns("6N002");
			#endregion

			#region Exporter
			var exporterMock = new Mock<IOrganization>();
			exporterMock.Setup(m => m.CompanyName).Returns("레디코리아");
			exporterMock.Setup(m => m.OfficeID).Returns("899999999");
			exporterMock.Setup(m => m.UnipassIDForOrganization).Returns("레디코리1971018");

			exportHeaderMock.Setup(m => m.Exporter).Returns(exporterMock.Object);
			#endregion

			#region Supplier
			var supplierMock = new Mock<IOrganization>();
			supplierMock.Setup(m => m.CompanyName).Returns("레디코리아");
			supplierMock.Setup(m => m.RepresentativeName).Returns("김택윤");
			supplierMock.Setup(m => m.AddressLine1).Returns("서울특별시 서초구 동광로 41");
			supplierMock.Setup(m => m.AddressLine2).Returns("레디인빌딩");
			supplierMock.Setup(m => m.Postcode).Returns("06561");
			supplierMock.Setup(m => m.RoadNameCode).Returns("101010");
			supplierMock.Setup(m => m.BuildingNumber).Returns("020120");
			supplierMock.Setup(m => m.OfficeID).Returns("899999999");
			supplierMock.Setup(m => m.UnipassIDForOrganization).Returns("레디코리1971018");
			supplierMock.Setup(m => m.BusinessRegNo).Returns("1028142299");

			exportHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);
			#endregion

			#region Manufacturer
			var manufacturerMock = new Mock<IOrganization>();
			manufacturerMock.Setup(m => m.CompanyName).Returns("제조미상");
			manufacturerMock.Setup(m => m.Postcode).Returns("04784");
			manufacturerMock.Setup(m => m.OfficeID).Returns("899999999");
			manufacturerMock.Setup(m => m.UnipassIDForOrganization).Returns("제조미상9999000");
			exportHeaderMock.Setup(m => m.Manufacturer).Returns(manufacturerMock.Object);
			#endregion

			#region Importer
			var importerMock = new Mock<IOrganization>();
			importerMock.Setup(m => m.CompanyName).Returns("BOA RAM HONGKONG INTERNATIONAL TRADING CO LIMITED");
			importerMock.Setup(m => m.ForeignCompanyID).Returns("HKBOARAM0001A");
			exportHeaderMock.Setup(m => m.Importer).Returns(importerMock.Object);
			#endregion

			#region EntryLine
			entryLine1Mock.Setup(x => x.HSDescription).Returns("USED EXCAVATOR");
			entryLine1Mock.Setup(x => x.BrandName).Returns("상표명");
			entryLine1Mock.Setup(x => x.InvoiceNo).Returns("999999999");
			entryLine1Mock.Setup(x => x.CountryOfOriginDeterminationRule).Returns("A");
			entryLine1Mock.Setup(x => x.CountryOfOriginLabelLocation).Returns("N");
			entryLine1Mock.Setup(x => x.CertificateOfOriginIssued).Returns("Y");
			entryLine1Mock.Setup(x => x.QtyUnit).Returns("CT");
			entryLine1Mock.Setup(x => x.Qty).Returns(1292529.99m);
			entryLine1Mock.Setup(x => x.PackQty).Returns(3);
			entryLine1Mock.Setup(x => x.PackType).Returns("CT");
			entryLine1Mock.Setup(x => x.ImportDeclarationNumber).Returns("A");
			entryLine1Mock.Setup(x => x.ImportEntryLineNo).Returns("001");
			entryLine1Mock.Setup(x => x.SkipManifestReporting).Returns("N");
			entryLine1Mock.Setup(x => x.FTAType).Returns(ZString.Empty);

			var entryLine2Mock = new Mock<IExportEntryLine>();
			entryLine2Mock.Setup(x => x.EntryLineNo).Returns("002");
			entryLine2Mock.Setup(x => x.HSCode).Returns("899999999");
			entryLine2Mock.Setup(x => x.TradeName).Returns("거래품명2");
			entryLine2Mock.Setup(x => x.CountryOfOrigin).Returns("KR");
			entryLine2Mock.Setup(x => x.NetWeightInKG).Returns(1299999.99m);
			entryLine2Mock.Setup(x => x.NetWeightUQ).Returns("KG");
			entryLine2Mock.Setup(x => x.CustomsValue).Returns(8599999.99m);
			entryLine2Mock.Setup(x => x.PreApprovalType).Returns("A");
			entryLine2Mock.Setup(x => x.PreApprovalNo).Returns("KR00101010101");
			entryLine2Mock.Setup(x => x.PreApprovalEffectiveFromDate).Returns(new ZDate("2019-01-01"));
			entryLine2Mock.Setup(x => x.PreApprovalEffectiveToDate).Returns(new ZDate("2019-07-01"));
			entryLine2Mock.Setup(x => x.InvoiceLines).Returns(System.Array.Empty<IExportInvoiceLine>());
			entryLine2Mock.Setup(x => x.InvoiceLines).Returns(System.Array.Empty<IExportInvoiceLine>());
			entryLine2Mock.Setup(x => x.HSDescription).Returns("품명2");
			entryLine2Mock.Setup(x => x.BrandName).Returns("상표명2");
			entryLine2Mock.Setup(x => x.InvoiceNo).Returns("899999999");
			entryLine2Mock.Setup(x => x.CountryOfOriginDeterminationRule).Returns("A");
			entryLine2Mock.Setup(x => x.CountryOfOriginLabelLocation).Returns("N");
			entryLine2Mock.Setup(x => x.CertificateOfOriginIssued).Returns("Y");
			entryLine2Mock.Setup(x => x.QtyUnit).Returns("CT");
			entryLine2Mock.Setup(x => x.Qty).Returns(1292529.99m);
			entryLine2Mock.Setup(x => x.PackQty).Returns(3);
			entryLine2Mock.Setup(x => x.PackType).Returns("CT");
			entryLine2Mock.Setup(x => x.ImportDeclarationNumber).Returns("A");
			entryLine2Mock.Setup(x => x.ImportEntryLineNo).Returns("001");
			entryLine2Mock.Setup(x => x.SkipManifestReporting).Returns("N");
			entryLine2Mock.Setup(x => x.FTAType).Returns(ZString.Empty);

			exportHeaderMock.Setup(m => m.EntryLines).Returns(new IExportEntryLine[] { entryLine1Mock.Object, entryLine2Mock.Object });
			#endregion

			#region IExportInvoiceLine (Line1)
			var exportInvoiceLine11Mock = new Mock<IExportInvoiceLine>();
			exportInvoiceLine11Mock.Setup(m => m.InvoiceLineNo).Returns("01");
			exportInvoiceLine11Mock.Setup(m => m.UnitPrice).Returns(27670.000000m);
			exportInvoiceLine11Mock.Setup(m => m.Amount).Returns(27670m);
			exportInvoiceLine11Mock.Setup(m => m.DetailDescription).Returns("2007 N81011141");

			exportInvoiceLine11Mock.Setup(m => m.QtyOrWeight).Returns(1m);
			exportInvoiceLine11Mock.Setup(m => m.QtyOrWeightUnit).Returns("U");
			exportInvoiceLine11Mock.Setup(m => m.Ingredient).Returns("dfewe");
			exportInvoiceLine11Mock.Setup(m => m.LotNumber).Returns("AbcdZZZ");

			var exportInvoiceLine12Mock = new Mock<IExportInvoiceLine>();
			exportInvoiceLine12Mock.Setup(m => m.InvoiceLineNo).Returns("02");
			exportInvoiceLine12Mock.Setup(m => m.UnitPrice).Returns(4564m);
			exportInvoiceLine12Mock.Setup(m => m.Amount).Returns(899999.25m);
			exportInvoiceLine12Mock.Setup(m => m.DetailDescription).Returns("모델규격2");

			exportInvoiceLine12Mock.Setup(m => m.QtyOrWeight).Returns(1234);
			exportInvoiceLine12Mock.Setup(m => m.QtyOrWeightUnit).Returns("KG");
			exportInvoiceLine12Mock.Setup(m => m.Ingredient).Returns("성분2");
			exportInvoiceLine12Mock.Setup(m => m.LotNumber).Returns("ZZZZEEE");
			exportInvoiceLine12Mock.Setup(m => m.GAApprovalDocuments).Returns(System.Array.Empty<IExportGAApprovalDocument>());

			entryLine1Mock.Setup(m => m.InvoiceLines).Returns(new IExportInvoiceLine[] { exportInvoiceLine11Mock.Object, exportInvoiceLine12Mock.Object });
			#endregion

			#region IExportGAApprovalDocument (Line1_exportInvoiceLine1)
			var exportGAApprovalDocument111Mock = new Mock<IExportGAApprovalDocument>();
			exportGAApprovalDocument111Mock.Setup(m => m.RegulationCategoryCode).Returns("05");
			exportGAApprovalDocument111Mock.Setup(m => m.SequenceNo).Returns("01");
			exportGAApprovalDocument111Mock.Setup(m => m.RequirementType).Returns("1");
			exportGAApprovalDocument111Mock.Setup(m => m.RequirementApprovalNumber).Returns("NO");

			exportGAApprovalDocument111Mock.Setup(x => x.ApprovalDate).Returns(new ZDate("2021-02-26"));
			exportGAApprovalDocument111Mock.Setup(x => x.RequirementDocumentType).Returns("A");
			exportGAApprovalDocument111Mock.Setup(x => x.DocumentName).Returns("식품등의 수입신고확인증");
			exportGAApprovalDocument111Mock.Setup(x => x.ReasonForMissingApprovalNumber).Returns("테스트");
			exportGAApprovalDocument111Mock.Setup(x => x.UniqueItemID).Returns("TEST");
			exportGAApprovalDocument111Mock.Setup(x => x.NonGAReasonType).Returns(ZString.Empty);

			var exportGAApprovalDocument112Mock = new Mock<IExportGAApprovalDocument>();
			exportGAApprovalDocument112Mock.Setup(m => m.RegulationCategoryCode).Returns("8999");
			exportGAApprovalDocument112Mock.Setup(m => m.SequenceNo).Returns("899999999");
			exportGAApprovalDocument112Mock.Setup(m => m.RequirementType).Returns("Y");
			exportGAApprovalDocument112Mock.Setup(m => m.RequirementApprovalNumber).Returns("899999999");

			exportGAApprovalDocument112Mock.Setup(x => x.ApprovalDate).Returns(new ZDate("2015-01-01"));
			exportGAApprovalDocument112Mock.Setup(x => x.RequirementDocumentType).Returns("A");
			exportGAApprovalDocument112Mock.Setup(x => x.DocumentName).Returns("발급서류명2");
			exportGAApprovalDocument112Mock.Setup(x => x.ReasonForMissingApprovalNumber).Returns("사유2");
			exportGAApprovalDocument112Mock.Setup(x => x.UniqueItemID).Returns("586455632");
			exportGAApprovalDocument112Mock.Setup(x => x.NonGAReasonType).Returns(ZString.Empty);
			exportInvoiceLine11Mock.Setup(x => x.GAApprovalDocuments).Returns(new IExportGAApprovalDocument[] { exportGAApprovalDocument111Mock.Object, exportGAApprovalDocument112Mock.Object });
			#endregion

			#region IExportVehicleNo (Line1_exportInvoiceLine1_exportVehicleNo1)
			var exportVehicleNo111Mock = new Mock<IExportVehicleNo>();
			exportVehicleNo111Mock.Setup(m => m.SequenceNo).Returns("001");
			exportVehicleNo111Mock.Setup(m => m.VIN).Returns("KN3HNP6N18K283119");

			var exportVehicleNo112Mock = new Mock<IExportVehicleNo>();
			exportVehicleNo112Mock.Setup(m => m.SequenceNo).Returns("002");
			exportVehicleNo112Mock.Setup(m => m.VIN).Returns("CCCCZZZ");
			exportInvoiceLine11Mock.Setup(m => m.VehicleNumbers).Returns(new IExportVehicleNo[] { exportVehicleNo111Mock.Object, exportVehicleNo112Mock.Object });
			#endregion

			exportHeaderMock.Setup(m => m.Currency).Returns(";;;");

			AssertNoExceptionThrown("No exception should be thrown when no currency is entered", () => new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage());
			exportHeaderMock.VerifyAll();
			entryLine1Mock.VerifyAll();
			exportContainer1.VerifyAll();
			exportContainer2.VerifyAll();
			declarantMock.VerifyAll();
			exporterMock.VerifyAll();
			supplierMock.VerifyAll();
			manufacturerMock.VerifyAll();
			importerMock.VerifyAll();
			entryLine2Mock.VerifyAll();
			exportInvoiceLine11Mock.VerifyAll();
			exportInvoiceLine12Mock.VerifyAll();
			exportGAApprovalDocument111Mock.VerifyAll();
			exportGAApprovalDocument112Mock.VerifyAll();
			exportVehicleNo111Mock.VerifyAll();
			exportVehicleNo112Mock.VerifyAll();
		}

		public void TestEmptyDeclarationOfficeID()
		{
			exportHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns(string.Empty);
			exportHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns(string.Empty);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.DeclarationOfficeId);

			exportHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("130");
			exportHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("10");

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("13010", result.DeclarationOfficeId.Value);
		}

		public void TestEmptyAdditionalCode()
		{
			exportHeaderMock.Setup(m => m.ReturnReason).Returns(ZString.Empty);
			exportHeaderMock.Setup(m => m.ReturnType).Returns(ZString.Empty);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.AdditionalCode);

			exportHeaderMock.Setup(m => m.ReturnReason).Returns(ZString.Empty);
			exportHeaderMock.Setup(m => m.ReturnType).Returns(ExportAmendmentReasonCodeList.Codes._11);

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.AdditionalCode.ReturnReasonCode);
			AssertEquals(ExportAmendmentReasonCodeList.Codes._11, result.AdditionalCode.ReturnScopeCode.Value);

			exportHeaderMock.Setup(m => m.ReturnReason).Returns("Return reason");
			exportHeaderMock.Setup(m => m.ReturnType).Returns(ZString.Empty);

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("Return reason", result.AdditionalCode.ReturnReasonCode.Value);
			AssertNull(result.AdditionalCode.ReturnScopeCode);
		}
		public void TestAdditionalInformation()
		{
			exportHeaderMock.Setup(m => m.OutOfHoursDeclarationIndicator).Returns(ZString.Empty);
			exportHeaderMock.Setup(m => m.DeclarantAdditionalDescription).Returns(ZString.Empty);
			exportHeaderMock.Setup(m => m.BondedTransportationFromDate).Returns(ZDate.Invalid);
			exportHeaderMock.Setup(m => m.BondedTransportationToDate).Returns(ZDate.Invalid);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.AdditionalInformation);

			exportHeaderMock.Setup(m => m.OutOfHoursDeclarationIndicator).Returns("N");
			exportHeaderMock.Setup(m => m.DeclarantAdditionalDescription).Returns(ZString.Empty);
			exportHeaderMock.Setup(m => m.BondedTransportationFromDate).Returns(ZDate.Invalid);
			exportHeaderMock.Setup(m => m.BondedTransportationToDate).Returns(ZDate.Invalid);

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("N", result.AdditionalInformation.StatementCode.Value);
			AssertNull(result.AdditionalInformation.StatementDescription);
			AssertNull(result.AdditionalInformation.PeriodDateTime);

			exportHeaderMock.Setup(m => m.OutOfHoursDeclarationIndicator).Returns(ZString.Empty);
			exportHeaderMock.Setup(m => m.DeclarantAdditionalDescription).Returns("신고인 기재란");
			exportHeaderMock.Setup(m => m.BondedTransportationFromDate).Returns(ZDate.Invalid);
			exportHeaderMock.Setup(m => m.BondedTransportationToDate).Returns(ZDate.Invalid);

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.AdditionalInformation.StatementCode);
			AssertEquals("신고인 기재란", result.AdditionalInformation.StatementDescription.Value);
			AssertNull(result.AdditionalInformation.PeriodDateTime);

			exportHeaderMock.Setup(m => m.OutOfHoursDeclarationIndicator).Returns("N");
			exportHeaderMock.Setup(m => m.DeclarantAdditionalDescription).Returns("신고인 기재란");

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.AdditionalInformation.PeriodDateTime);

			exportHeaderMock.Setup(m => m.BondedTransportationFromDate).Returns(new ZDate(2014, 01, 01));
			exportHeaderMock.Setup(m => m.BondedTransportationToDate).Returns(ZDate.Invalid);

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.AdditionalInformation.PeriodDateTime);

			exportHeaderMock.Setup(m => m.BondedTransportationFromDate).Returns(ZDate.Invalid);
			exportHeaderMock.Setup(m => m.BondedTransportationToDate).Returns(new ZDate(2014, 01, 02));

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.AdditionalInformation.PeriodDateTime);

			exportHeaderMock.Setup(m => m.OutOfHoursDeclarationIndicator).Returns(ZString.Empty);
			exportHeaderMock.Setup(m => m.DeclarantAdditionalDescription).Returns(ZString.Empty);
			exportHeaderMock.Setup(m => m.BondedTransportationFromDate).Returns(new ZDate(2014, 01, 01));
			exportHeaderMock.Setup(m => m.BondedTransportationToDate).Returns(new ZDate(2014, 01, 02));

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.AdditionalInformation.StatementCode);
			AssertNull(result.AdditionalInformation.StatementDescription);
			AssertEquals("2014010120140102", result.AdditionalInformation.PeriodDateTime);
		}

		public void TestAgent()
		{
			exportHeaderMock.Setup(m => m.Exporter).Returns((IOrganization)null);
			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals(ZString.Empty, result.Agent.Name.Value);
			AssertNull(result.Agent.Id);

			var exporterMock = new Mock<IOrganization>();
			exporterMock.Setup(m => m.CompanyName).Returns("레디코리아");
			exporterMock.Setup(m => m.OfficeID).Returns("899999999");
			exporterMock.Setup(m => m.UnipassIDForOrganization).Returns("레디코리1971018");

			exportHeaderMock.Setup(m => m.Exporter).Returns(exporterMock.Object);
			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("레디코리아", result.Agent.Name.Value);
			AssertEquals(2, result.Agent.Id.Count);
			AssertEquals("레디코리1971018", result.Agent.Id[0].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Item380, result.Agent.Id[0].SchemeAgencyId);
			AssertEquals("899999999", result.Agent.Id[1].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Kts, result.Agent.Id[1].SchemeAgencyId);
		}

		public void TestEmptyElementBorderTransportMeans()
		{
			exportHeaderMock.Setup(m => m.VesselNameOrFlightNo).Returns(string.Empty);
			exportHeaderMock.Setup(m => m.DepartureDate).Returns(ZDate.Invalid);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.BorderTransportMeans);

			exportHeaderMock.Setup(m => m.VesselNameOrFlightNo).Returns("AA9999");
			exportHeaderMock.Setup(m => m.DepartureDate).Returns(new ZDate("2014-01-01"));

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("AA9999", result.BorderTransportMeans.Name.Value);
			AssertEquals("20140101", result.BorderTransportMeans.EstimatedDepartureDateTime);
			AssertNull(result.BorderTransportMeans.TransportEquipment);

			#region Containers
			var exportContainer1 = new Mock<IExportContainer>();
			exportContainer1.Setup(m => m.SequenceNo).Returns("01");
			var exportContainer2 = new Mock<IExportContainer>();
			exportContainer2.Setup(m => m.SequenceNo).Returns("02");
			exportContainer2.Setup(m => m.ContainerNo).Returns("B4");

			exportHeaderMock.Setup(m => m.Containers).Returns(new IExportContainer[] { exportContainer1.Object, exportContainer2.Object });
			#endregion

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals(2, result.BorderTransportMeans.TransportEquipment.Count);
			AssertNull(result.BorderTransportMeans.TransportEquipment[0].Id);
			AssertEquals("01", result.BorderTransportMeans.TransportEquipment[0].SequenceNumeric.Value);
			AssertEquals("B4", result.BorderTransportMeans.TransportEquipment[1].Id.Value);
			AssertEquals("02", result.BorderTransportMeans.TransportEquipment[1].SequenceNumeric.Value);
		}

		public void TestEmptyElementCarrier()
		{
			exportHeaderMock.Setup(m => m.CarrierID).Returns(string.Empty);
			exportHeaderMock.Setup(m => m.ShippingLineOrAirlineName).Returns(string.Empty);
			exportHeaderMock.Setup(m => m.FreightForwarderContactName).Returns(string.Empty);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.Carrier);

			exportHeaderMock.Setup(m => m.CarrierID).Returns("KE");
			exportHeaderMock.Setup(m => m.ShippingLineOrAirlineName).Returns("대한항공");
			exportHeaderMock.Setup(m => m.FreightForwarderContactName).Returns("나대표");

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("KE", result.Carrier.Id.Value);
			AssertEquals("대한항공", result.Carrier.Name.Value);
			AssertEquals("나대표", result.Carrier.Contact.Name.Value);
		}

		public void TestEmptyElementCurrencyExchange()
		{
			exportHeaderMock.Setup(m => m.ExchangeRate).Returns(ZDecimal.Zero);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.CurrencyExchange);

			exportHeaderMock.Setup(m => m.ExchangeRate).Returns(1084.25m);

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals(1084.25m, result.CurrencyExchange.RateNumeric);
		}

		public void TestEmptyElementConsignment()
		{
			exportHeaderMock.Setup(m => m.GoodsLocationBondedAreaCode).Returns(string.Empty);
			exportHeaderMock.Setup(m => m.LocationIDInBondedArea).Returns(string.Empty);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.Consignment.GoodsLocation);
			AssertNull(result.Consignment.ConsignmentItem.PreviousDocument);

			exportHeaderMock.Setup(m => m.GoodsLocationBondedAreaCode).Returns("99999999");
			exportHeaderMock.Setup(m => m.LocationIDInBondedArea).Returns("9999999999");

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("99999999", result.Consignment.GoodsLocation.Id.Value);
			AssertEquals("9999999999", result.Consignment.ConsignmentItem.PreviousDocument.Id.Value);
		}

		public void TestEmptyExporter()
		{
			exportHeaderMock.Setup(x => x.ExporterType).Returns(ZString.Empty);

			AssertNull(exportHeaderMock.Object.Supplier);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();

			AssertEquals(ZString.Empty, result.Exporter.Name.Value);
			AssertEquals(ZString.Empty, result.Exporter.RoleCode.Value);
			AssertEquals(ZString.Empty, result.Exporter.TypeCode.Value);
			AssertNull(result.Exporter.Address.CountrySubDivisionId);
			AssertNull(result.Exporter.Address.Line);
			AssertEquals(ZString.Empty, result.Exporter.Address.PostcodeId.Value);
			AssertNull(result.Exporter.Address.BuildingNumber);
			AssertEquals(ZString.Empty, result.Exporter.Address.Description.Value);
			AssertEquals(ZString.Empty, result.Exporter.Contact.RepresentativeName.Value);
			AssertEquals(1, result.Exporter.Id.Count);
		}

		public void TestEmptyElementExporter()
		{
			var supplierMock = new Mock<IOrganization>();
			supplierMock.Setup(m => m.RoadNameCode).Returns(string.Empty);
			supplierMock.Setup(m => m.AddressLine2).Returns(string.Empty);
			supplierMock.Setup(m => m.BuildingNumber).Returns(string.Empty);

			exportHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.Exporter.Address.CountrySubDivisionId);
			AssertNull(result.Exporter.Address.Line);
			AssertNull(result.Exporter.Address.BuildingNumber);
			AssertEquals(1, result.Exporter.Id.Count);

			supplierMock.Setup(m => m.OfficeID).Returns(string.Empty);
			exportHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals(1, result.Exporter.Id.Count);

			supplierMock.Setup(m => m.RoadNameCode).Returns("101010");
			supplierMock.Setup(m => m.AddressLine2).Returns("레디인빌딩");
			supplierMock.Setup(m => m.BuildingNumber).Returns("020120");
			supplierMock.Setup(m => m.UnipassIDForOrganization).Returns("레디코리1971018");
			supplierMock.Setup(m => m.BusinessRegNo).Returns("1028142299");

			exportHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("101010", result.Exporter.Address.CountrySubDivisionId.Value);
			AssertEquals("레디인빌딩", result.Exporter.Address.Line.Value);
			AssertEquals("020120", result.Exporter.Address.BuildingNumber.Value);

			AssertEquals(2, result.Exporter.Id.Count);
			AssertEquals(AgencyIdentificationCodeContentType.Zzz, result.Exporter.Id[0].SchemeAgencyId);
			AssertEquals("1028142299", result.Exporter.Id[0].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Item380, result.Exporter.Id[1].SchemeAgencyId);
			AssertEquals("레디코리1971018", result.Exporter.Id[1].Value);
		}

		public void TestEmptyElementFinalTransportMeansLoadingPlace()
		{
			exportHeaderMock.Setup(m => m.FinalLoadingPlace).Returns(string.Empty);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertNull(result.FinalTransportMeansLoadingPlace);

			exportHeaderMock.Setup(m => m.FinalLoadingPlace).Returns("9999999");

			result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("9999999", result.FinalTransportMeansLoadingPlace.Id.Value);
		}

		public void TestDigitFormat()
		{
			var supplierMock = new Mock<IOrganization>();
			supplierMock.Setup(m => m.KoreanRegNoForResident).Returns("1234567890123");
			supplierMock.Setup(m => m.IsIndividual).Returns(true);

			var exportInvoiceLineMock = new Mock<IExportInvoiceLine>();
			exportInvoiceLineMock.Setup(m => m.InvoiceLineNo).Returns("01");

			var exportVehicleNo111Mock = new Mock<IExportVehicleNo>();
			exportVehicleNo111Mock.Setup(m => m.SequenceNo).Returns("001");

			var exportVehicleNo112Mock = new Mock<IExportVehicleNo>();
			exportVehicleNo112Mock.Setup(m => m.SequenceNo).Returns("02");
			exportInvoiceLineMock.Setup(m => m.VehicleNumbers).Returns(new IExportVehicleNo[] { exportVehicleNo111Mock.Object, exportVehicleNo112Mock.Object });

			var exportGAApprovalDocument111Mock = new Mock<IExportGAApprovalDocument>();
			exportGAApprovalDocument111Mock.Setup(m => m.SequenceNo).Returns("01");
			exportInvoiceLineMock.Setup(x => x.GAApprovalDocuments).Returns(new IExportGAApprovalDocument[] { exportGAApprovalDocument111Mock.Object });

			entryLine1Mock.Setup(m => m.InvoiceLines).Returns(new IExportInvoiceLine[] { exportInvoiceLineMock.Object });
			entryLine1Mock.Setup(m => m.EntryLineNo).Returns("001");
			entryLine1Mock.Setup(m => m.ImportEntryLineNo).Returns("001");
			exportHeaderMock.Setup(m => m.EntryLines).Returns(new IExportEntryLine[] { entryLine1Mock.Object });
			exportHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);

			var result = new GOVCBR830MessageBuilder(exportHeaderMock.Object).GenerateMessage();
			AssertEquals("01", result.Exporter.RoleCode.Value);
			AssertEquals("001", result.GoodsShipment.GovernmentAgencyGoodsItem[0].SequenceNumeric.Value);
			AssertEquals("001", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.PreviousDocument.LineNumeric.Value);
			AssertEquals("01", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].SequenceNumeric.Value);
			AssertEquals("001", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].Vehicle[0].SequenceNumeric.Value);
			AssertEquals("01", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity[0].AdditionalDocument[0].CriteriaConformanceId.Value);
		}
	}
}
