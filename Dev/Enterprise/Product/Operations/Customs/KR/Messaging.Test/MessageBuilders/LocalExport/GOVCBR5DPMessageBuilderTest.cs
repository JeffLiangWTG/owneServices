using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5DPMessageBuilderTest : TestCaseWithFactory
	{
		Mock<ILocalExportEntryHeader> localExportHeaderMock;

		protected override void SetUp()
		{
			base.SetUp();

			localExportHeaderMock = new Mock<ILocalExportEntryHeader>();
			localExportHeaderMock.Setup(m => m.DeclarationType).Returns(LocalExportTransactionNatureCodeList.Codes._01);
			localExportHeaderMock.Setup(m => m.DeclarationNumber).Returns("1083699012345");
			localExportHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			localExportHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("10");
			localExportHeaderMock.Setup(m => m.BondedAreaCode).Returns("반입장소");
			localExportHeaderMock.Setup(m => m.DeclarationDate).Returns(new ZDate("2013-01-01"));
			var supplierMock = new Mock<IOrganization>();
			supplierMock.Setup(m => m.BusinessRegNo).Returns("0000000000");
			supplierMock.Setup(m => m.UnipassIDForOrganization).Returns("111111111111111");
			localExportHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);

			#region Importer
			var importerMock = new Mock<IOrganization>();
			importerMock.Setup(m => m.CompanyName).Returns("무한상사");
			importerMock.Setup(m => m.RepresentativeName).Returns("김대표");
			importerMock.Setup(m => m.AddressLine1).Returns("기본주소");
			importerMock.Setup(m => m.AddressLine2).Returns("상세주소");
			importerMock.Setup(m => m.Postcode).Returns("32012");
			importerMock.Setup(m => m.RoadNameCode).Returns("110001");
			importerMock.Setup(m => m.BuildingNumber).Returns("121200");
			importerMock.Setup(m => m.BusinessRegNo).Returns("1200020212");

			localExportHeaderMock.Setup(m => m.Importer).Returns(importerMock.Object);
			#endregion
			localExportHeaderMock.Setup(m => m.GoodsType).Returns("1");
			localExportHeaderMock.Setup(m => m.DrawbackApplicantType).Returns("1");
			localExportHeaderMock.Setup(m => m.TotalDeclarationAmount).Returns(13332m);
			localExportHeaderMock.Setup(m => m.TotalGrossWeight).Returns(900m);
			localExportHeaderMock.Setup(m => m.TotalPackages).Returns(100);
			localExportHeaderMock.Setup(m => m.EntryLines).Returns(SetUpEntryLines().Select(m => m.Object));
		}

		IEnumerable<Mock<ILocalExportEntryLine>> SetUpEntryLines()
		{
			var entryLines1 = new Mock<ILocalExportEntryLine>();
			entryLines1.Setup(m => m.EntryLineNo).Returns(1);
			entryLines1.Setup(m => m.HSCode).Returns("1234567890");
			entryLines1.Setup(m => m.InvoiceDescription).Returns("STAINLESS STEEL");
			entryLines1.Setup(m => m.GoodsNo).Returns("000000000");
			entryLines1.Setup(m => m.Quantity).Returns(9999m);
			entryLines1.Setup(m => m.QuantityUnit).Returns("KG");
			entryLines1.Setup(m => m.Packages).Returns(99);
			entryLines1.Setup(m => m.PackagesType).Returns("VL");
			entryLines1.Setup(m => m.NetWeight).Returns(9999m);
			entryLines1.Setup(m => m.FOBAmount).Returns(9999m);
			entryLines1.Setup(m => m.DocumentNo).Returns("L172770925459");
			entryLines1.Setup(m => m.DocumentType).Returns("1");
			entryLines1.Setup(m => m.InboundDate).Returns(new ZDate("2013-01-01"));
			entryLines1.Setup(m => m.PreviousTransactionReferenceNo).Returns("010151234567001999");
			entryLines1.Setup(m => m.PreviousTransactionReferenceNoType).Returns("01");
			entryLines1.Setup(m => m.MaterialCode).Returns("ABCD9589375");
			yield return entryLines1;

			var entryLines2 = new Mock<ILocalExportEntryLine>();
			entryLines2.Setup(m => m.EntryLineNo).Returns(2);
			entryLines2.Setup(m => m.HSCode).Returns("0987654321");
			entryLines2.Setup(m => m.InvoiceDescription).Returns("STAINLESS STEEL2");
			entryLines2.Setup(m => m.GoodsNo).Returns("1111111111");
			entryLines2.Setup(m => m.Quantity).Returns(1111m);
			entryLines2.Setup(m => m.QuantityUnit).Returns("KG");
			entryLines2.Setup(m => m.Packages).Returns(11);
			entryLines2.Setup(m => m.PackagesType).Returns("VL");
			entryLines2.Setup(m => m.NetWeight).Returns(2222m);
			entryLines2.Setup(m => m.FOBAmount).Returns(3333m);
			entryLines2.Setup(m => m.DocumentNo).Returns("L172770925459");
			entryLines2.Setup(m => m.DocumentType).Returns("1");
			entryLines2.Setup(m => m.InboundDate).Returns(new ZDate("2020-02-02"));
			entryLines2.Setup(m => m.PreviousTransactionReferenceNo).Returns("999100765432151010");
			entryLines2.Setup(m => m.PreviousTransactionReferenceNoType).Returns("02");
			entryLines2.Setup(m => m.MaterialCode).Returns("5739859DCBA");
			yield return entryLines2;
		}

		IEnumerable<Mock<ILocalExportEntryLine>> SetUpEntryLinesWithEmptyElements()
		{
			return SetUpEntryLines().Select(m =>
			{
				m.Setup(n => n.GoodsNo).Returns(ZString.Empty);
				m.Setup(n => n.InboundDate).Returns(ZDate.Empty);
				m.Setup(n => n.PreviousTransactionReferenceNo).Returns(ZString.Empty);
				m.Setup(n => n.PreviousTransactionReferenceNoType).Returns(ZString.Empty);
				m.Setup(n => n.MaterialCode).Returns(ZString.Empty);
				m.Setup(n => n.DocumentType).Returns(ZString.Empty);
				return m;
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateDeclaration()
		{
			#region Manufacturer
			var manufacturerMock = new Mock<IOrganization>();
			manufacturerMock.Setup(m => m.BusinessRegNo).Returns("0000000000");
			manufacturerMock.Setup(m => m.UnipassIDForOrganization).Returns("111111111111111");
			localExportHeaderMock.Setup(m => m.Manufacturer).Returns(manufacturerMock.Object);
			#endregion

			var result = new GOVCBR5DPMessageBuilder(localExportHeaderMock.Object).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.LocalExportOutgoingTestFilePath, "GOVCBR5DP_0.xml"));
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(stream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("01010", result.DeclarationOfficeId.Value);
			AssertEquals("9", result.FunctionCode.Value);
			AssertEquals(2m, result.GoodsItemQuantity.Value);
			AssertEquals("1083699012345", result.Id.Value);
			AssertEquals(13332m, result.InvoiceAmount.Value);
			AssertEquals(900m, result.TotalGrossMassMeasure.Value);
			AssertEquals(100m, result.TotalPackageQuantity.Value);
			AssertEquals("GOVCBR5DP", result.TypeCode.Value);
			AssertEquals("01", result.TransactionNatureCode.Value);
			AssertEquals("1", result.AdditionalInformation.StatementCode.Value);
			#region Consignee (Importer)
			AssertEquals("1200020212", result.Consignment.Consignee.Id.Value);
			AssertEquals("무한상사", result.Consignment.Consignee.Name.Value);
			AssertEquals("110001", result.Consignment.Consignee.Address.CountrySubDivisionId.Value);
			AssertEquals("상세주소", result.Consignment.Consignee.Address.Line.Value);
			AssertEquals("32012", result.Consignment.Consignee.Address.PostcodeId.Value);
			AssertEquals("121200", result.Consignment.Consignee.Address.BuildingNumber.Value);
			AssertEquals("기본주소", result.Consignment.Consignee.Address.Description.Value);
			AssertEquals("김대표", result.Consignment.Consignee.Contact.RepresentativeName.Value);
			#endregion
			#region ConsignmentItem (EntryLines)
			AssertEquals(1m, result.Consignment.ConsignmentItem[0].SequenceNumeric);
			AssertEquals("1", result.Consignment.ConsignmentItem[0].AdditionalDocument.TypeCode.Value);
			AssertEquals("L172770925459", result.Consignment.ConsignmentItem[0].AdditionalDocument.Id.Value);
			AssertEquals("STAINLESS STEEL", result.Consignment.ConsignmentItem[0].Commodity.CargoDescription.Value);
			AssertEquals(9999m, result.Consignment.ConsignmentItem[0].Commodity.CountQuantity.Value);
			AssertEquals("KG", result.Consignment.ConsignmentItem[0].Commodity.CountQuantity.KcsUnitCode);
			AssertEquals("000000000", result.Consignment.ConsignmentItem[0].Commodity.Id.Value);
			AssertEquals("20130101", result.Consignment.ConsignmentItem[0].Commodity.JurisdictionDateTime);
			AssertEquals(9999m, result.Consignment.ConsignmentItem[0].Commodity.ValueAmount.Value);
			AssertEquals("010151234567001999", result.Consignment.ConsignmentItem[0].Commodity.AdditionalDocument.Id.Value);
			AssertEquals("01", result.Consignment.ConsignmentItem[0].Commodity.AdditionalDocument.TypeCode.Value);
			AssertEquals("1234567890", result.Consignment.ConsignmentItem[0].Commodity.Classification.Id.Value);
			AssertEquals(9999m, result.Consignment.ConsignmentItem[0].GoodsMeasure.NetNetWeightMeasure.Value);
			AssertEquals(99m, result.Consignment.ConsignmentItem[0].Packaging.QuantityQuantity.Value);
			AssertEquals("VL", result.Consignment.ConsignmentItem[0].Packaging.TypeCode.Value);
			AssertEquals("ABCD9589375", result.Consignment.ConsignmentItem[0].PreviousDocument.Id.Value);

			AssertEquals(2m, result.Consignment.ConsignmentItem[1].SequenceNumeric);
			AssertEquals("1", result.Consignment.ConsignmentItem[1].AdditionalDocument.TypeCode.Value);
			AssertEquals("L172770925459", result.Consignment.ConsignmentItem[1].AdditionalDocument.Id.Value);
			AssertEquals("STAINLESS STEEL2", result.Consignment.ConsignmentItem[1].Commodity.CargoDescription.Value);
			AssertEquals(1111m, result.Consignment.ConsignmentItem[1].Commodity.CountQuantity.Value);
			AssertEquals("KG", result.Consignment.ConsignmentItem[1].Commodity.CountQuantity.KcsUnitCode);
			AssertEquals("1111111111", result.Consignment.ConsignmentItem[1].Commodity.Id.Value);
			AssertEquals("20200202", result.Consignment.ConsignmentItem[1].Commodity.JurisdictionDateTime);
			AssertEquals(3333m, result.Consignment.ConsignmentItem[1].Commodity.ValueAmount.Value);
			AssertEquals("999100765432151010", result.Consignment.ConsignmentItem[1].Commodity.AdditionalDocument.Id.Value);
			AssertEquals("02", result.Consignment.ConsignmentItem[1].Commodity.AdditionalDocument.TypeCode.Value);
			AssertEquals("0987654321", result.Consignment.ConsignmentItem[1].Commodity.Classification.Id.Value);
			AssertEquals(2222m, result.Consignment.ConsignmentItem[1].GoodsMeasure.NetNetWeightMeasure.Value);
			AssertEquals(11m, result.Consignment.ConsignmentItem[1].Packaging.QuantityQuantity.Value);
			AssertEquals("VL", result.Consignment.ConsignmentItem[1].Packaging.TypeCode.Value);
			AssertEquals("5739859DCBA", result.Consignment.ConsignmentItem[1].PreviousDocument.Id.Value);
			#endregion
			AssertEquals("20130101", result.Consignment.Warehouse.ArrivalDateTime);
			AssertEquals("반입장소", result.Consignment.Warehouse.Name.Value);
			#region Manufacturer (Manufacturer)
			AssertEquals("111111111111111", result.Manufacturer[0].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Item380, result.Manufacturer[0].SchemeAgencyId);
			AssertEquals("0000000000", result.Manufacturer[1].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Ktx, result.Manufacturer[1].SchemeAgencyId);
			#endregion
			AssertNull(result.PreviousDocument?.Id?.Value);
			#region Submitter (Supplier)
			AssertEquals("111111111111111", result.Submitter.Id[0].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Item380, result.Submitter.Id[0].SchemeAgencyId);
			AssertEquals("0000000000", result.Submitter.Id[1].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Ktx, result.Submitter.Id[1].SchemeAgencyId);
			#endregion
			AssertEquals("1", result.Submitter.RoleCode.Value);
			localExportHeaderMock.VerifyAll();
			manufacturerMock.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNotManufacturer()
		{
			var result = new GOVCBR5DPMessageBuilder(localExportHeaderMock.Object).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.LocalExportOutgoingTestFilePath, "GOVCBR5DP_1.xml"));
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(stream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("01010", result.DeclarationOfficeId.Value);
			AssertEquals("9", result.FunctionCode.Value);
			AssertEquals(2m, result.GoodsItemQuantity.Value);
			AssertEquals("1083699012345", result.Id.Value);
			AssertEquals(13332m, result.InvoiceAmount.Value);
			AssertEquals(900m, result.TotalGrossMassMeasure.Value);
			AssertEquals(100m, result.TotalPackageQuantity.Value);
			AssertEquals("GOVCBR5DP", result.TypeCode.Value);
			AssertEquals("01", result.TransactionNatureCode.Value);
			AssertEquals("1", result.AdditionalInformation.StatementCode.Value);
			#region Consignee (Importer)
			AssertEquals("1200020212", result.Consignment.Consignee.Id.Value);
			AssertEquals("무한상사", result.Consignment.Consignee.Name.Value);
			AssertEquals("110001", result.Consignment.Consignee.Address.CountrySubDivisionId.Value);
			AssertEquals("상세주소", result.Consignment.Consignee.Address.Line.Value);
			AssertEquals("32012", result.Consignment.Consignee.Address.PostcodeId.Value);
			AssertEquals("121200", result.Consignment.Consignee.Address.BuildingNumber.Value);
			AssertEquals("기본주소", result.Consignment.Consignee.Address.Description.Value);
			AssertEquals("김대표", result.Consignment.Consignee.Contact.RepresentativeName.Value);
			#endregion
			#region ConsignmentItem (EntryLines)
			AssertEquals(1m, result.Consignment.ConsignmentItem[0].SequenceNumeric);
			AssertEquals("1", result.Consignment.ConsignmentItem[0].AdditionalDocument.TypeCode.Value);
			AssertEquals("L172770925459", result.Consignment.ConsignmentItem[0].AdditionalDocument.Id.Value);
			AssertEquals("STAINLESS STEEL", result.Consignment.ConsignmentItem[0].Commodity.CargoDescription.Value);
			AssertEquals(9999m, result.Consignment.ConsignmentItem[0].Commodity.CountQuantity.Value);
			AssertEquals("KG", result.Consignment.ConsignmentItem[0].Commodity.CountQuantity.KcsUnitCode);
			AssertEquals("000000000", result.Consignment.ConsignmentItem[0].Commodity.Id.Value);
			AssertEquals("20130101", result.Consignment.ConsignmentItem[0].Commodity.JurisdictionDateTime);
			AssertEquals(9999m, result.Consignment.ConsignmentItem[0].Commodity.ValueAmount.Value);
			AssertEquals("010151234567001999", result.Consignment.ConsignmentItem[0].Commodity.AdditionalDocument.Id.Value);
			AssertEquals("01", result.Consignment.ConsignmentItem[0].Commodity.AdditionalDocument.TypeCode.Value);
			AssertEquals("1234567890", result.Consignment.ConsignmentItem[0].Commodity.Classification.Id.Value);
			AssertEquals(9999m, result.Consignment.ConsignmentItem[0].GoodsMeasure.NetNetWeightMeasure.Value);
			AssertEquals(99m, result.Consignment.ConsignmentItem[0].Packaging.QuantityQuantity.Value);
			AssertEquals("VL", result.Consignment.ConsignmentItem[0].Packaging.TypeCode.Value);
			AssertEquals("ABCD9589375", result.Consignment.ConsignmentItem[0].PreviousDocument.Id.Value);

			AssertEquals(2m, result.Consignment.ConsignmentItem[1].SequenceNumeric);
			AssertEquals("1", result.Consignment.ConsignmentItem[1].AdditionalDocument.TypeCode.Value);
			AssertEquals("L172770925459", result.Consignment.ConsignmentItem[1].AdditionalDocument.Id.Value);
			AssertEquals("STAINLESS STEEL2", result.Consignment.ConsignmentItem[1].Commodity.CargoDescription.Value);
			AssertEquals(1111m, result.Consignment.ConsignmentItem[1].Commodity.CountQuantity.Value);
			AssertEquals("KG", result.Consignment.ConsignmentItem[1].Commodity.CountQuantity.KcsUnitCode);
			AssertEquals("1111111111", result.Consignment.ConsignmentItem[1].Commodity.Id.Value);
			AssertEquals("20200202", result.Consignment.ConsignmentItem[1].Commodity.JurisdictionDateTime);
			AssertEquals(3333m, result.Consignment.ConsignmentItem[1].Commodity.ValueAmount.Value);
			AssertEquals("999100765432151010", result.Consignment.ConsignmentItem[1].Commodity.AdditionalDocument.Id.Value);
			AssertEquals("02", result.Consignment.ConsignmentItem[1].Commodity.AdditionalDocument.TypeCode.Value);
			AssertEquals("0987654321", result.Consignment.ConsignmentItem[1].Commodity.Classification.Id.Value);
			AssertEquals(2222m, result.Consignment.ConsignmentItem[1].GoodsMeasure.NetNetWeightMeasure.Value);
			AssertEquals(11m, result.Consignment.ConsignmentItem[1].Packaging.QuantityQuantity.Value);
			AssertEquals("VL", result.Consignment.ConsignmentItem[1].Packaging.TypeCode.Value);
			AssertEquals("5739859DCBA", result.Consignment.ConsignmentItem[1].PreviousDocument.Id.Value);
			#endregion
			AssertEquals("20130101", result.Consignment.Warehouse.ArrivalDateTime);
			AssertEquals("반입장소", result.Consignment.Warehouse.Name.Value);
			AssertNull(result.PreviousDocument);
			#region Submitter (Supplier)
			AssertEquals("111111111111111", result.Submitter.Id[0].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Item380, result.Submitter.Id[0].SchemeAgencyId);
			AssertEquals("0000000000", result.Submitter.Id[1].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Ktx, result.Submitter.Id[1].SchemeAgencyId);
			#endregion
			AssertEquals("1", result.Submitter.RoleCode.Value);
			localExportHeaderMock.VerifyAll();
		}

		public void TestArrivalDateTimeAndJurisdictionDateTime()
		{
			var localExportHeaderMock = new Mock<ILocalExportEntryHeader>();
			var importerMock = new Mock<IOrganization>();
			localExportHeaderMock.Setup(m => m.Importer).Returns(importerMock.Object);
			var entryLines = new Mock<ILocalExportEntryLine>();
			entryLines.Setup(m => m.InboundDate).Returns(ZDate.Empty);
			localExportHeaderMock.Setup(m => m.EntryLines).Returns(new ILocalExportEntryLine[] { entryLines.Object });
			var result = new GOVCBR5DPMessageBuilder(localExportHeaderMock.Object).GenerateMessage();
			AssertEquals("", result.Consignment.Warehouse.ArrivalDateTime);
			AssertNull(result.Consignment.ConsignmentItem[0].Commodity.JurisdictionDateTime);

			localExportHeaderMock.Setup(m => m.DeclarationDate).Returns(new ZDate("2022-11-01"));
			entryLines.Setup(m => m.InboundDate).Returns(new ZDate("2022-11-02"));

			result = new GOVCBR5DPMessageBuilder(localExportHeaderMock.Object).GenerateMessage();
			AssertEquals("20221101", result.Consignment.Warehouse.ArrivalDateTime);
			AssertEquals("20221102", result.Consignment.ConsignmentItem[0].Commodity.JurisdictionDateTime);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateDeclarationWithEmptyElements()
		{
			localExportHeaderMock.Setup(m => m.BondedAreaCode).Returns(ZString.Empty);
			localExportHeaderMock.Setup(m => m.DeclarationDate).Returns(ZDate.Invalid);

			var importer = new Mock<IOrganization>();
			importer.Setup(m => m.RoadNameCode).Returns(ZString.Empty);
			importer.Setup(m => m.CompanyName).Returns("무한상사");
			importer.Setup(m => m.RepresentativeName).Returns("김대표");
			importer.Setup(m => m.BusinessRegNo).Returns("");
			localExportHeaderMock.Setup(m => m.Importer).Returns(importer.Object);
			localExportHeaderMock.Setup(m => m.EntryLines).Returns(SetUpEntryLinesWithEmptyElements().Select(m => m.Object));
			localExportHeaderMock.Setup(m => m.MRNNo).Returns(ZString.Empty);

			var result = new GOVCBR5DPMessageBuilder(localExportHeaderMock.Object).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.LocalExportOutgoingTestFilePath, "GOVCBR5DP_2.xml"));
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(stream.WriteToString(), serialisedXml);
				}
			}

			#region Consignee (Importer)
			AssertNull(result.Consignment.Consignee.Address);
			#endregion
			#region ConsignmentItem (EntryLines)
			AssertNull(result.Consignment.ConsignmentItem[0].Commodity.Id);
			AssertNull(result.Consignment.ConsignmentItem[0].Commodity.JurisdictionDateTime);
			AssertNull(result.Consignment.ConsignmentItem[0].Commodity.AdditionalDocument);
			AssertNull(result.Consignment.ConsignmentItem[0].PreviousDocument);

			AssertNull(result.Consignment.ConsignmentItem[1].Commodity.Id);
			AssertNull(result.Consignment.ConsignmentItem[1].Commodity.JurisdictionDateTime);
			AssertNull(result.Consignment.ConsignmentItem[1].Commodity.AdditionalDocument);
			AssertNull(result.Consignment.ConsignmentItem[1].PreviousDocument);
			#endregion
			AssertNull(result.Consignment.Warehouse);
			#region Manufacturer (Manufacturer)
			AssertNull(result.Manufacturer);
			#endregion
			AssertNull(result.PreviousDocument);
			AssertNull(result.BorderTransportMeans);
			AssertNull(result.Agent);
			localExportHeaderMock.VerifyAll();
		}

		public void TestEmptyImporter()
		{
			localExportHeaderMock.Setup(m => m.Importer).Returns((IOrganization)null);
			var result = new GOVCBR5DPMessageBuilder(localExportHeaderMock.Object).GenerateMessage();
			AssertEquals(ZString.Empty, result.Consignment.Consignee.Id.Value);
			AssertEquals(ZString.Empty, result.Consignment.Consignee.Name.Value);
			AssertEquals(ZString.Empty, result.Consignment.Consignee.Contact.RepresentativeName.Value);
		}

		public void TestNotUsedAgent()
		{
			var exporterMock = new Mock<IOrganization>();
			exporterMock.Setup(m => m.BusinessRegNo).Returns("0000000000");
			localExportHeaderMock.Setup(m => m.Exporter).Returns(exporterMock.Object);

			var result = new GOVCBR5DPMessageBuilder(localExportHeaderMock.Object).GenerateMessage();
			AssertNull(result.Agent);
		}
	}
}
