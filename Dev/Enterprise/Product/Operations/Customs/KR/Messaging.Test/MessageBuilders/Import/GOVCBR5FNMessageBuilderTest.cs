using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5FNMessageBuilderTest : TestCaseWithFactory
	{
		Mock<IImport5FNLine> importLine;
		Mock<IImport5FNHeader> importHeaderMock;

		protected override void SetUp()
		{
			base.SetUp();
			importHeaderMock = new Mock<IImport5FNHeader>();
			importHeaderMock.Setup(m => m.ImportDeclarationNumber).Returns("111111111");
			importHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			importHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("10");
			importHeaderMock.Setup(m => m.TypeOfBusiness).Returns("164973258");

			var customsBrokerMock = new Mock<IOrganization>();
			customsBrokerMock.Setup(m => m.CompanyName).Returns("상호2");
			customsBrokerMock.Setup(m => m.MobileNumber).Returns("01022223333");
			customsBrokerMock.Setup(m => m.FaxNumber).Returns("22222222222");
			customsBrokerMock.Setup(m => m.Email).Returns("fake@addr.com");
			importHeaderMock.Setup(m => m.CustomsBroker).Returns(customsBrokerMock.Object);

			importLine = new Mock<IImport5FNLine>();
			importLine.Setup(m => m.EntryLineNo).Returns(1);
			importLine.Setup(m => m.HSCode).Returns("121312345");
			importLine.Setup(m => m.ModelName).Returns("model name");
			importLine.Setup(m => m.SerialNumber).Returns("SerialNumber");
			importLine.Setup(m => m.UseCodeDescription).Returns("UseCodeDescription");
			importLine.Setup(m => m.ProductTypeCode).Returns("1");
			importLine.Setup(m => m.PostClearanceProcedureYN).Returns("Y");
			importLine.Setup(m => m.Remark).Returns("비고");
			importLine.Setup(m => m.DutyReductionClassification).Returns("A");
			importLine.Setup(m => m.ReductionRateRegulationGroupNumber).Returns("A");
			importLine.Setup(m => m.ReductionRateRegulationSeqNumber).Returns("1");
			importLine.Setup(m => m.ReductionRateRegulationItemNumber).Returns("1");
			importLine.Setup(m => m.ScheduledReExportCustomsOffice).Returns("030");
			importLine.Setup(m => m.ReExportDestinationCountryCode).Returns("UK");
			importLine.Setup(m => m.ScheduledReExportDate).Returns(new ZDate("2014-01-01"));
			importLine.Setup(m => m.JurisdictionalCustomsOffice).Returns("020");
		}

		Mock<IOrganization> PayerSetting(bool isIndividual)
		{
			var payerMock = new Mock<IOrganization>();
			payerMock.Setup(m => m.CompanyName).Returns("상호1");
			payerMock.Setup(m => m.MobileNumber).Returns("01032165498");
			payerMock.Setup(m => m.FaxNumber).Returns("11111111111");
			payerMock.Setup(m => m.Email).Returns("payer@addr.com");
			payerMock.Setup(m => m.IsIndividual).Returns(isIndividual);

			if (isIndividual)
			{
				payerMock.Setup(m => m.KoreanRegNoForResident).Returns("9911112222222");
			}
			else
			{
				payerMock.Setup(m => m.BusinessRegNo).Returns("1234512123");
			}
			return payerMock;
		}

		void GoodsLocationSetting()
		{
			var goodsLocationMock = new Mock<IOrganization>();
			goodsLocationMock.Setup(m => m.PhoneNumber).Returns("01123234567");
			goodsLocationMock.Setup(m => m.Postcode).Returns("11111");
			goodsLocationMock.Setup(m => m.RoadNameCode).Returns("도로명주소");
			goodsLocationMock.Setup(m => m.BuildingNumber).Returns("건물관리번호");
			goodsLocationMock.Setup(m => m.AddressLine1).Returns("기본주소");
			goodsLocationMock.Setup(m => m.AddressLine2).Returns("상세주소");

			importLine.Setup(m => m.GoodsLocation).Returns(goodsLocationMock.Object);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestForIndividual()
		{
			var payerMock = PayerSetting(true);
			importHeaderMock.Setup(m => m.Payer).Returns(payerMock.Object);
			GoodsLocationSetting();
			importLine.Setup(m => m.Header).Returns(importHeaderMock.Object);

			var result1 = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR5FN_1.xml"));
			using (var testStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var result = KRXmlObjectSerializer.Serialize(result1))
				{
					var testReaderSource = new TextReaderSource(testStream);
					var testXml = testReaderSource.GetReader().ReadToEnd();

					var readerSource = new TextReaderSource(result);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(testXml, serialisedXml);
				}
			}

			#region AssertEquals
			AssertEquals("01010", result1.DeclarationOfficeId.Value);
			AssertEquals("111111111", result1.Id.Value);
			AssertEquals("GOVCBR5FN", result1.TypeCode.Value);
			AssertEquals("A", result1.TransactionNatureCode.Value);
			AssertEquals("Y", result1.AdditionalInformation.StatementCode.Value);

			AssertEquals(1m, result1.GoodsShipment.SequenceNumeric);

			AssertEquals("상호1", result1.GoodsShipment.Agent[0].Name.Value);
			AssertEquals("PR", result1.GoodsShipment.Agent[0].RoleCode.Value);
			AssertEquals("CE", result1.GoodsShipment.Agent[0].Communication[0].TypeId.Value);
			AssertEquals("01032165498", result1.GoodsShipment.Agent[0].Communication[0].Id.Value);
			AssertEquals("FX", result1.GoodsShipment.Agent[0].Communication[1].TypeId.Value);
			AssertEquals("11111111111", result1.GoodsShipment.Agent[0].Communication[1].Id.Value);
			AssertEquals("EM", result1.GoodsShipment.Agent[0].Communication[2].TypeId.Value);
			AssertEquals("payer@addr.com", result1.GoodsShipment.Agent[0].Communication[2].Id.Value);

			AssertEquals("상호2", result1.GoodsShipment.Agent[1].Name.Value);
			AssertEquals("CB", result1.GoodsShipment.Agent[1].RoleCode.Value);
			AssertEquals("CE", result1.GoodsShipment.Agent[1].Communication[0].TypeId.Value);
			AssertEquals("01022223333", result1.GoodsShipment.Agent[1].Communication[0].Id.Value);
			AssertEquals("FX", result1.GoodsShipment.Agent[1].Communication[1].TypeId.Value);
			AssertEquals("22222222222", result1.GoodsShipment.Agent[1].Communication[1].Id.Value);
			AssertEquals("EM", result1.GoodsShipment.Agent[1].Communication[2].TypeId.Value);
			AssertEquals("fake@addr.com", result1.GoodsShipment.Agent[1].Communication[2].Id.Value);

			AssertEquals("20140101", result1.GoodsShipment.Consignment.BorderTransportMeans.DepartureDateTime);
			AssertEquals("UK", result1.GoodsShipment.Consignment.GoodsConsignedPlace.Id.Value);

			AssertEquals("030", result1.GoodsShipment.ExitOffice.Id.Value);

			AssertEquals("A", result1.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalDocument.TypeCode.Value);
			AssertEquals("1", result1.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalDocument.Id.Value);
			AssertEquals(1m, result1.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalDocument.SequenceNumeric);
			AssertEquals("비고", result1.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalInformation.Content.Value);
			AssertEquals("UseCodeDescription", result1.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.IntendedUse.Value);
			AssertEquals("1", result1.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.IntendedUseCode.Value);
			AssertEquals("SerialNumber", result1.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.LotNumberId.Value);
			AssertEquals("model name", result1.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.Name.Value);
			AssertEquals("121312345", result1.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.Classification.Id.Value);
			AssertEquals("020", result1.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.ResponsibleGovernmentAgency.Id.Value);
			AssertEquals("164973258", result1.GoodsShipment.Importer.TypeOfBusiness.Value);
			AssertEquals("도로명주소", result1.GoodsShipment.Warehouse.Address.CountrySubDivisionId.Value);
			AssertEquals("상세주소", result1.GoodsShipment.Warehouse.Address.Line.Value);
			AssertEquals("11111", result1.GoodsShipment.Warehouse.Address.PostcodeId.Value);
			AssertEquals("건물관리번호", result1.GoodsShipment.Warehouse.Address.BuildingNumber.Value);
			AssertEquals("기본주소", result1.GoodsShipment.Warehouse.Address.Description.Value);
			AssertEquals("01123234567", result1.GoodsShipment.Warehouse.Communication.Id.Value);

			AssertEquals("9911112222222", result1.Payer.Id.Value);
			AssertEquals("01", result1.Payer.RoleCode.Value);
			#endregion
			importHeaderMock.VerifyAll();
			importLine.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestForBusiness()
		{
			var payerMock = PayerSetting(false);
			importHeaderMock.Setup(m => m.Payer).Returns(payerMock.Object);
			GoodsLocationSetting();
			importLine.Setup(m => m.Header).Returns(importHeaderMock.Object);

			var result2 = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR5FN_2.xml"));
			using (var testStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var result = KRXmlObjectSerializer.Serialize(result2))
				{
					var testReaderSource = new TextReaderSource(testStream);
					var testXml = testReaderSource.GetReader().ReadToEnd();

					var readerSource = new TextReaderSource(result);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(testXml, serialisedXml);
				}
			}

			AssertEquals("1234512123", result2.Payer.Id.Value);
			AssertEquals("04", result2.Payer.RoleCode.Value);
			importHeaderMock.VerifyAll();
			importLine.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestForGoodsLocationIsNull()
		{
			var payerMock = PayerSetting(false);
			importHeaderMock.Setup(m => m.Payer).Returns(payerMock.Object);
			importLine.Setup(m => m.Header).Returns(importHeaderMock.Object);

			var result3 = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR5FN_3.xml"));
			using (var testStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var result = KRXmlObjectSerializer.Serialize(result3))
				{
					var testReaderSource = new TextReaderSource(testStream);
					var testXml = testReaderSource.GetReader().ReadToEnd();

					var readerSource = new TextReaderSource(result);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(testXml, serialisedXml);
				}
			}

			AssertNull(result3.GoodsShipment.Warehouse);
			importHeaderMock.VerifyAll();
			importLine.VerifyAll();
		}

		public void TestEmptyMandatoryElements()
		{
			var importLine = new Mock<IImport5FNLine>();
			var importHeaderMock = new Mock<IImport5FNHeader>();

			importLine.Setup(m => m.Header).Returns(importHeaderMock.Object);
			importLine.Setup(m => m.ReductionRateRegulationItemNumber).Returns("0");

			var result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertNotNull(result.DeclarationOfficeId);
			AssertNotNull(result.Id);
			AssertNotNull(result.TypeCode);
			AssertNotNull(result.TransactionNatureCode);
			AssertNotNull(result.AdditionalInformation.StatementCode);
			AssertNotNull(result.GoodsShipment.SequenceNumeric);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.Classification.Id);
			AssertNotNull(result.Payer.Id);
			AssertNotNull(result.Payer.RoleCode);
		}

		public void TestEmptyConditionalSingleElements()
		{
			var importLine = new Mock<IImport5FNLine>();
			var importHeaderMock = new Mock<IImport5FNHeader>();
			importLine.Setup(m => m.Header).Returns(importHeaderMock.Object);
			importLine.Setup(m => m.ReductionRateRegulationItemNumber).Returns("0");

			var result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.ExitOffice);
			AssertNull(result.GoodsShipment.Importer);

			importLine.Setup(m => m.ScheduledReExportCustomsOffice).Returns("office");
			importHeaderMock.Setup(m => m.TypeOfBusiness).Returns("businessType");
			importLine.Setup(m => m.Header).Returns(importHeaderMock.Object);
			result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertEquals("office", result.GoodsShipment.ExitOffice.Id.Value);
			AssertEquals("businessType", result.GoodsShipment.Importer.TypeOfBusiness.Value);
		}

		public void TestEmptyAgent()
		{
			var importLine = new Mock<IImport5FNLine>();
			var importHeaderMock = new Mock<IImport5FNHeader>();

			importLine.Setup(m => m.Header).Returns(importHeaderMock.Object);
			importLine.Setup(m => m.ReductionRateRegulationItemNumber).Returns("0");

			var result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.Agent);

			var payer = new Mock<IOrganization>();
			importLine.Setup(m => m.Header.Payer).Returns(payer.Object);
			result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.Agent[0].Name);
			AssertNull(result.GoodsShipment.Agent[0].Communication);

			payer.Setup(m => m.CompanyName).Returns("company");
			payer.Setup(m => m.MobileNumber).Returns("01012345678");
			importLine.Setup(m => m.Header.Payer).Returns(payer.Object);
			result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertEquals("company", result.GoodsShipment.Agent[0].Name.Value);
			AssertEquals(1, result.GoodsShipment.Agent[0].Communication.Count);
			AssertEquals("01012345678", result.GoodsShipment.Agent[0].Communication[0].Id.Value);

			payer.Setup(m => m.FaxNumber).Returns("115222789");
			payer.Setup(m => m.Email).Returns("fake@addr.com");
			importLine.Setup(m => m.Header.Payer).Returns(payer.Object);
			result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertEquals(3, result.GoodsShipment.Agent[0].Communication.Count);
			AssertEquals("115222789", result.GoodsShipment.Agent[0].Communication[1].Id.Value);
			AssertEquals("fake@addr.com", result.GoodsShipment.Agent[0].Communication[2].Id.Value);
		}

		public void TestEmptyGovernmentAgencyGoodsItem()
		{
			var importLine = new Mock<IImport5FNLine>();
			var importHeaderMock = new Mock<IImport5FNHeader>();

			importLine.Setup(m => m.Header).Returns(importHeaderMock.Object);
			importLine.Setup(m => m.ReductionRateRegulationSeqNumber).Returns("0");
			var result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalDocument);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalInformation);

			importLine.Setup(m => m.Remark).Returns("Remark");
			result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalDocument);
			AssertEquals("Remark", result.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalInformation.Content.Value);

			importLine.Setup(m => m.Remark).Returns(ZString.Empty);
			importLine.Setup(m => m.ReductionRateRegulationItemNumber).Returns("1");
			importLine.Setup(m => m.ReductionRateRegulationSeqNumber).Returns("11");
			importLine.Setup(m => m.ReductionRateRegulationGroupNumber).Returns("A");
			result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertEquals("1", result.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalDocument.Id.Value);
			AssertEquals(11m, result.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalDocument.SequenceNumeric);
			AssertEquals("A", result.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalDocument.TypeCode.Value);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalInformation);
		}

		public void TestEmptyConsignment()
		{
			var importLine = new Mock<IImport5FNLine>();
			var importHeaderMock = new Mock<IImport5FNHeader>();

			importLine.Setup(m => m.Header).Returns(importHeaderMock.Object);
			importLine.Setup(m => m.ReductionRateRegulationItemNumber).Returns("0");
			var result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.Consignment);

			importLine.Setup(m => m.ScheduledReExportDate).Returns(new ZDate("2023-01-01"));
			result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertEquals("20230101", result.GoodsShipment.Consignment.BorderTransportMeans.DepartureDateTime);
			AssertNull(result.GoodsShipment.Consignment.GoodsConsignedPlace);

			importLine.Setup(m => m.ScheduledReExportDate).Returns(ZDate.Invalid);
			importLine.Setup(m => m.ReExportDestinationCountryCode).Returns("UK");
			result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.Consignment.BorderTransportMeans);
			AssertEquals("UK", result.GoodsShipment.Consignment.GoodsConsignedPlace.Id.Value);
		}

		public void TestEmptyCommodity()
		{
			var importLine = new Mock<IImport5FNLine>();
			var importHeaderMock = new Mock<IImport5FNHeader>();

			importLine.Setup(m => m.Header).Returns(importHeaderMock.Object);
			importLine.Setup(m => m.ReductionRateRegulationItemNumber).Returns("0");
			var result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.IntendedUse);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.IntendedUseCode);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.LotNumberId);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.Name);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.ResponsibleGovernmentAgency);

			importLine.Setup(m => m.UseCodeDescription).Returns("UseCodeDescription");
			importLine.Setup(m => m.ProductTypeCode).Returns("ProductTypeCode");
			importLine.Setup(m => m.SerialNumber).Returns("SerialNumber");
			importLine.Setup(m => m.ModelName).Returns("ModelName");
			importLine.Setup(m => m.JurisdictionalCustomsOffice).Returns("JurisdictionalCustomsOffice");
			importLine.Setup(m => m.Header).Returns(importHeaderMock.Object);

			result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertEquals("UseCodeDescription", result.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.IntendedUse.Value);
			AssertEquals("ProductTypeCode", result.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.IntendedUseCode.Value);
			AssertEquals("SerialNumber", result.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.LotNumberId.Value);
			AssertEquals("ModelName", result.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.Name.Value);
			AssertEquals("JurisdictionalCustomsOffice", result.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.ResponsibleGovernmentAgency.Id.Value);
		}

		public void TestEmptyWarehouse()
		{
			var importLine = new Mock<IImport5FNLine>();
			var importHeaderMock = new Mock<IImport5FNHeader>();

			importLine.Setup(m => m.Header).Returns(importHeaderMock.Object);
			importLine.Setup(m => m.ReductionRateRegulationItemNumber).Returns("0");
			var result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.Warehouse);

			var goodsLocationMock = new Mock<IOrganization>();

			goodsLocationMock.Setup(m => m.PhoneNumber).Returns("11111111111");
			importLine.Setup(m => m.GoodsLocation).Returns(goodsLocationMock.Object);
			result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertEquals("11111111111", result.GoodsShipment.Warehouse.Communication.Id.Value);
			AssertNull(result.GoodsShipment.Warehouse.Address);

			goodsLocationMock.Setup(m => m.Postcode).Returns("11111");
			goodsLocationMock.Setup(m => m.PhoneNumber).Returns(ZString.Empty);
			importLine.Setup(m => m.GoodsLocation).Returns(goodsLocationMock.Object);
			result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.Warehouse.Communication);
			AssertEquals("11111", result.GoodsShipment.Warehouse.Address.PostcodeId.Value);
			AssertNull(result.GoodsShipment.Warehouse.Address.BuildingNumber);
			AssertNull(result.GoodsShipment.Warehouse.Address.Line);
			AssertNull(result.GoodsShipment.Warehouse.Address.CountrySubDivisionId);
			AssertNull(result.GoodsShipment.Warehouse.Address.Description);

			goodsLocationMock.Setup(m => m.Postcode).Returns(ZString.Empty);
			goodsLocationMock.Setup(m => m.RoadNameCode).Returns("111111111");
			goodsLocationMock.Setup(m => m.BuildingNumber).Returns("건물관리번호");
			goodsLocationMock.Setup(m => m.AddressLine1).Returns("기본주소");
			goodsLocationMock.Setup(m => m.AddressLine2).Returns("상세주소");
			importLine.Setup(m => m.GoodsLocation).Returns(goodsLocationMock.Object);
			result = new GOVCBR5FNMessageBuilder(importLine.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.Warehouse.Address.PostcodeId);
			AssertEquals("111111111", result.GoodsShipment.Warehouse.Address.CountrySubDivisionId.Value);
			AssertEquals("건물관리번호", result.GoodsShipment.Warehouse.Address.BuildingNumber.Value);
			AssertEquals("기본주소", result.GoodsShipment.Warehouse.Address.Description.Value);
			AssertEquals("상세주소", result.GoodsShipment.Warehouse.Address.Line.Value);
		}
	}
}
