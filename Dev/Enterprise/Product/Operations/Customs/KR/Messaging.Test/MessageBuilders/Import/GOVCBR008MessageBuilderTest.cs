using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Moq;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR008MessageBuilderTest : TestCaseWithFactory
	{
		Mock<IImport008Header> import008Mock;

		protected override void SetUp()
		{
			import008Mock = new Mock<IImport008Header>();
			import008Mock.Setup(m => m.ImportDeclarationNumber).Returns("4062001070010U");
			import008Mock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			import008Mock.Setup(m => m.DeclarationCustomsDivision).Returns("20");
			import008Mock.Setup(m => m.ForeignCountry).Returns("KR");
			import008Mock.Setup(m => m.ForeignCity).Returns("서울");
			import008Mock.Setup(m => m.DecType).Returns("1");
			import008Mock.Setup(m => m.LoadingPort).Returns("JP");
			import008Mock.Setup(m => m.HBL).Returns("06WCCMPH110");
			import008Mock.Setup(m => m.TransportationStartDate).Returns(new ZDate("2014-05-06"));
			import008Mock.Setup(m => m.TransportationArrivalDate).Returns(new ZDate("2014-05-06"));
			import008Mock.Setup(m => m.Freight).Returns(12300);
			import008Mock.Setup(m => m.ForeignCarrier).Returns("운송회사");
			import008Mock.Setup(m => m.DomesticCarrier).Returns("운송회사");

			var entryLine1Mock = new Mock<IImport008Line>();
			entryLine1Mock.Setup(m => m.InvoiceDescription).Returns("카메라");
			entryLine1Mock.Setup(m => m.ItemCategory).Returns("1");
			entryLine1Mock.Setup(m => m.Quantity).Returns(99);
			entryLine1Mock.Setup(m => m.Model).Returns("수동형랜즈장착");
			entryLine1Mock.Setup(m => m.BrandName).Returns("삼성");
			entryLine1Mock.Setup(m => m.Price).Returns(500);
			entryLine1Mock.Setup(m => m.MonthOfUse).Returns(12);
			entryLine1Mock.Setup(m => m.ItemCode).Returns("001");

			var entryLine2Mock = new Mock<IImport008Line>();
			entryLine2Mock.Setup(m => m.InvoiceDescription).Returns("냉장고");
			entryLine2Mock.Setup(m => m.ItemCategory).Returns("2");
			entryLine2Mock.Setup(m => m.Quantity).Returns(100);
			entryLine2Mock.Setup(m => m.Model).Returns("양문형냉장고");
			entryLine2Mock.Setup(m => m.BrandName).Returns("LG");
			entryLine2Mock.Setup(m => m.Price).Returns(501);
			entryLine2Mock.Setup(m => m.MonthOfUse).Returns(13);
			entryLine2Mock.Setup(m => m.ItemCode).Returns("002");
			import008Mock.Setup(m => m.Lines).Returns(new IImport008Line[] { entryLine1Mock.Object, entryLine2Mock.Object });

			var entryDecQuestion1Mock = new Mock<IImport008DecQuestion>();
			entryDecQuestion1Mock.Setup(m => m.QuestionID).Returns("1");
			entryDecQuestion1Mock.Setup(m => m.Answer).Returns("Y");

			var entryDecQuestion2Mock = new Mock<IImport008DecQuestion>();
			entryDecQuestion2Mock.Setup(m => m.QuestionID).Returns("2");
			entryDecQuestion2Mock.Setup(m => m.Answer).Returns("Y");

			var entryDecQuestion3Mock = new Mock<IImport008DecQuestion>();
			entryDecQuestion3Mock.Setup(m => m.QuestionID).Returns("3");
			entryDecQuestion3Mock.Setup(m => m.Answer).Returns("Y");

			var entryDecQuestion4Mock = new Mock<IImport008DecQuestion>();
			entryDecQuestion4Mock.Setup(m => m.QuestionID).Returns("4");
			entryDecQuestion4Mock.Setup(m => m.Answer).Returns("Y");

			var entryDecQuestion5Mock = new Mock<IImport008DecQuestion>();
			entryDecQuestion5Mock.Setup(m => m.QuestionID).Returns("5");
			entryDecQuestion5Mock.Setup(m => m.Answer).Returns("Y");

			var entryDecQuestion6Mock = new Mock<IImport008DecQuestion>();
			entryDecQuestion6Mock.Setup(m => m.QuestionID).Returns("6");
			entryDecQuestion6Mock.Setup(m => m.Answer).Returns("Y");

			var entryDecQuestion7Mock = new Mock<IImport008DecQuestion>();
			entryDecQuestion7Mock.Setup(m => m.QuestionID).Returns("7");
			entryDecQuestion7Mock.Setup(m => m.Answer).Returns("Y");

			var entryDecQuestion8Mock = new Mock<IImport008DecQuestion>();
			entryDecQuestion8Mock.Setup(m => m.QuestionID).Returns("8");
			entryDecQuestion8Mock.Setup(m => m.Answer).Returns("Y");
			import008Mock.Setup(m => m.QuestionsAndAnswers).Returns(new IImport008DecQuestion[] { entryDecQuestion1Mock.Object, entryDecQuestion2Mock.Object, entryDecQuestion3Mock.Object, entryDecQuestion4Mock.Object, entryDecQuestion5Mock.Object, entryDecQuestion6Mock.Object, entryDecQuestion7Mock.Object, entryDecQuestion8Mock.Object });

			var entryBulkItem1Mock = new Mock<IImport008BulkItem>();
			entryBulkItem1Mock.Setup(m => m.Type).Returns("1");
			entryBulkItem1Mock.Setup(m => m.EngineDisplacement).Returns(1500);
			entryBulkItem1Mock.Setup(m => m.IdentificationNumber).Returns("WBADT63432CH90499");
			entryBulkItem1Mock.Setup(m => m.ModelYear).Returns("2014");
			entryBulkItem1Mock.Setup(m => m.ModelName).Returns("제우스");
			entryBulkItem1Mock.Setup(m => m.SeatingCapacity).Returns(5);
			entryBulkItem1Mock.Setup(m => m.CurrentRegistrationDate).Returns(new ZDate("2014-05-06"));
			entryBulkItem1Mock.Setup(m => m.FirstRegistrationDate).Returns(new ZDate("2014-05-06"));
			entryBulkItem1Mock.Setup(m => m.ManufacturingCountry).Returns("KR");

			import008Mock.Setup(m => m.Vehicle).Returns(entryBulkItem1Mock.Object);

			var ownerMock = new Mock<IOrganization>();
			ownerMock.Setup(m => m.RoadNameCode).Returns("10020");
			ownerMock.Setup(m => m.AddressLine2).Returns("상세주소");
			ownerMock.Setup(m => m.AddressLine1).Returns("기본주소");
			ownerMock.Setup(m => m.Postcode).Returns("46512");
			ownerMock.Setup(m => m.BuildingNumber).Returns("11101");
			ownerMock.Setup(m => m.PhoneNumber).Returns("042-548-7895");
			ownerMock.Setup(m => m.Email).Returns("aaa@naver.com");
			ownerMock.Setup(m => m.IsIndividual).Returns(ZBool.False);

			import008Mock.Setup(m => m.Declarant).Returns(ownerMock.Object);

			var ownerMock1 = new Mock<IImport008Person>();
			ownerMock1.Setup(m => m.PassportNumber).Returns("M20120450");
			ownerMock1.Setup(m => m.Name).Returns("성명");
			ownerMock1.Setup(m => m.ScheduledStartDateInKR).Returns(new ZDate("2014-05-06"));
			ownerMock1.Setup(m => m.ScheduledEndDateInKR).Returns(new ZDate("2014-05-09"));
			ownerMock1.Setup(m => m.Nationality).Returns("KR");
			ownerMock1.Setup(m => m.RelationshipToImporter).Returns("00");
			ownerMock1.Setup(m => m.JobCode).Returns("1");
			ownerMock1.Setup(m => m.BirthDate).Returns(new ZDate("1984-05-28"));
			ownerMock1.Setup(m => m.NationalityClassCode).Returns("1");

			import008Mock.Setup(m => m.Owner).Returns(ownerMock1.Object);

			var ownerMock2 = new Mock<IImport008Person>();
			ownerMock2.Setup(m => m.Name).Returns("동반가족명");
			ownerMock2.Setup(m => m.PassportNumber).Returns("KR0177360");
			ownerMock2.Setup(m => m.BirthDate).Returns(new ZDate("1984-05-28"));
			ownerMock2.Setup(m => m.RelationshipToImporter).Returns("10");
			ownerMock2.Setup(m => m.JobCode).Returns("1");
			ownerMock2.Setup(m => m.EntryToKR_YN).Returns("Y");
			ownerMock2.Setup(m => m.ScheduledStartDateInKR).Returns(new ZDate("2014-01-01"));
			ownerMock2.Setup(m => m.ScheduledEndDateInKR).Returns(new ZDate("2014-12-31"));
			var ownerMock3 = new Mock<IImport008Person>();
			ownerMock3.Setup(m => m.Name).Returns("동반가족명2");
			ownerMock3.Setup(m => m.PassportNumber).Returns("KR0177360");
			ownerMock3.Setup(m => m.BirthDate).Returns(new ZDate("1984-05-28"));
			ownerMock3.Setup(m => m.RelationshipToImporter).Returns("20");
			ownerMock3.Setup(m => m.JobCode).Returns("2");
			ownerMock3.Setup(m => m.EntryToKR_YN).Returns("N");
			ownerMock3.Setup(m => m.ScheduledStartDateInKR).Returns(new ZDate("2014-01-01"));
			ownerMock3.Setup(m => m.ScheduledEndDateInKR).Returns(new ZDate("2014-12-31"));
			import008Mock.Setup(m => m.FamilyMembers).Returns(new IImport008Person[] { ownerMock2.Object, ownerMock3.Object });
		}

		public void TestGenerateDeclaration()
		{
			var result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertEquals("4062001070010U", result.Id.Value);
			AssertEquals("01020", result.DeclarationOfficeId.Value);
			AssertEquals("KR", result.TransitDestination.Id.Value);
			AssertEquals("서울", result.TransitDestination.Name.Value);
			AssertEquals("20140506", result.AdditionalInformation.BeginningDateTime);
			AssertEquals("20140509", result.AdditionalInformation.EndingDateTime);
			AssertEquals("1", result.AdditionalInformation.StatementCode.Value);
			AssertEquals("JP", result.LoadingLocation.Id.Value);
			AssertEquals("06WCCMPH110", result.TransportContractDocument.Id.Value);
			AssertEquals("20140506", result.AdditionalInformation.LimitDateTime);
			AssertEquals("20140506", result.UnloadingLocation.ArrivalDateTime);
			AssertEquals(12300m, result.GoodsShipment.CustomsValuation.FreightChargeAmount.Value);
			AssertEquals("1", result.Carrier[0].RoleCode.Value);
			AssertEquals("운송회사", result.Carrier[0].Name.Value);
			AssertEquals("2", result.Carrier[1].RoleCode.Value);
			AssertEquals("운송회사", result.Carrier[1].Name.Value);

			AssertEquals("1", result.GoodsShipment.AdditionalInformation[0].StatementCode.Value);
			AssertEquals("2", result.GoodsShipment.AdditionalInformation[1].StatementCode.Value);
			AssertEquals("3", result.GoodsShipment.AdditionalInformation[2].StatementCode.Value);
			AssertEquals("4", result.GoodsShipment.AdditionalInformation[3].StatementCode.Value);
			AssertEquals("5", result.GoodsShipment.AdditionalInformation[4].StatementCode.Value);
			AssertEquals("6", result.GoodsShipment.AdditionalInformation[5].StatementCode.Value);
			AssertEquals("7", result.GoodsShipment.AdditionalInformation[6].StatementCode.Value);
			AssertEquals("8", result.GoodsShipment.AdditionalInformation[7].StatementCode.Value);
			AssertEquals("Y", result.GoodsShipment.AdditionalInformation[0].StatementTypeCode.Value);

			AssertEquals("1", result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalInformation.StatementCode.Value);
			AssertEquals("1500", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Description.Value);
			AssertEquals("WBADT63432CH90499", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Id.Value);
			AssertEquals("2014", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.ManufactureDateTime);
			AssertEquals("제우스", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Name.Value);
			AssertEquals("5", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.Content.Value);
			AssertEquals("20140506", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.LimitDateTime);
			AssertEquals("20140506", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.RegisterDateTime);
			AssertEquals("KR", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Origin.CountryCode.Value);

			AssertEquals("카메라", result.Consignment[0].ConsignmentItem.Commodity.CargoDescription.Value);
			AssertEquals("1", result.Consignment[0].ConsignmentItem.Commodity.CharacteristicCode.Value);
			AssertEquals(99M, result.Consignment[0].ConsignmentItem.Commodity.CountQuantity.Value);
			AssertEquals("수동형랜즈장착", result.Consignment[0].ConsignmentItem.Commodity.Description.Value);
			AssertEquals("삼성", result.Consignment[0].ConsignmentItem.Commodity.Name.Value);
			AssertEquals(500M, result.Consignment[0].ConsignmentItem.Commodity.ValueAmount.Value);
			AssertEquals("12", result.Consignment[0].ConsignmentItem.Commodity.AdditionalInformation.Content.Value);
			AssertEquals("001", result.Consignment[0].ConsignmentItem.Commodity.Classification.Id.Value);
			AssertEquals("냉장고", result.Consignment[1].ConsignmentItem.Commodity.CargoDescription.Value);
			AssertEquals("2", result.Consignment[1].ConsignmentItem.Commodity.CharacteristicCode.Value);
			AssertEquals(100M, result.Consignment[1].ConsignmentItem.Commodity.CountQuantity.Value);
			AssertEquals("양문형냉장고", result.Consignment[1].ConsignmentItem.Commodity.Description.Value);
			AssertEquals("LG", result.Consignment[1].ConsignmentItem.Commodity.Name.Value);
			AssertEquals(501M, result.Consignment[1].ConsignmentItem.Commodity.ValueAmount.Value);
			AssertEquals("13", result.Consignment[1].ConsignmentItem.Commodity.AdditionalInformation.Content.Value);
			AssertEquals("002", result.Consignment[1].ConsignmentItem.Commodity.Classification.Id.Value);

			AssertEquals("KR0177360", result.Agent[0].Id.Value);
			AssertEquals("동반가족명", result.Agent[0].Name.Value);
			AssertEquals("19840528", result.Agent[0].Contact.BirthDate);
			AssertEquals("10", result.Agent[0].Contact.Relationship.Value);
			AssertEquals("1", result.Agent[0].Contact.Occupation.Value);
			AssertEquals("Y", result.Agent[0].AdditionalInformation.StatementCode.Value);
			AssertEquals("20140101", result.Agent[0].AdditionalInformation.BeginningDateTime);
			AssertEquals("20141231", result.Agent[0].AdditionalInformation.EndingDateTime);

			AssertEquals("M20120450", result.Submitter.Id.Value);
			AssertEquals("성명", result.Submitter.Name.Value);
			AssertEquals("1", result.Submitter.RoleCode.Value);
			AssertEquals("KR", result.Submitter.Address.CountryCode.Value);
			AssertEquals("10020", result.Submitter.Address.CountrySubDivisionId.Value);
			AssertEquals("상세주소", result.Submitter.Address.Line.Value);
			AssertEquals("46512", result.Submitter.Address.PostcodeId.Value);
			AssertEquals("11101", result.Submitter.Address.BuildingNumber.Value);
			AssertEquals("기본주소", result.Submitter.Address.Description.Value);
			AssertEquals("19840528", result.Submitter.Contact.BirthDate);
			AssertEquals("1", result.Submitter.Contact.Occupation.Value);
			AssertEquals("TE", result.Submitter.Communication[0].TypeId.Value);
			AssertEquals("042-548-7895", result.Submitter.Communication[0].Id.Value);
			AssertEquals("EM", result.Submitter.Communication[1].TypeId.Value);
			AssertEquals("aaa@naver.com", result.Submitter.Communication[1].Id.Value);
		}

		public void TestEmptyDeclaration()
		{
			var import008Mock = new Mock<IImport008Header>();
			var result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();
			AssertNotNull(result.Id);
			AssertNotNull(result.TypeCode);
			AssertNotNull(result.Carrier);
			AssertNotNull(result.LoadingLocation.Id);
			AssertNotNull(result.TransitDestination.Id);
			AssertNotNull(result.TransitDestination.Name);
			AssertNull(result.TransportContractDocument);
			AssertNull(result.UnloadingLocation);
		}

		public void TestEmptyDeclarationOfficeID()
		{
			var import008Mock = new Mock<IImport008Header>();
			var result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();
			AssertNull(result.DeclarationOfficeId);

			import008Mock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			import008Mock.Setup(m => m.DeclarationCustomsDivision).Returns("10");
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();
			AssertNotNull(result.DeclarationOfficeId);
		}

		public void TestEmptyAdditionalInformation()
		{
			var import008Mock = new Mock<IImport008Header>();
			var result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();
			AssertNotNull(result.AdditionalInformation.StatementCode);
			AssertNotNull(result.AdditionalInformation.BeginningDateTime);
			AssertNotNull(result.AdditionalInformation.EndingDateTime);
			AssertNull(result.AdditionalInformation.LimitDateTime);
		}

		public void TestEmptyAgent()
		{
			var import008Mock = new Mock<IImport008Header>();
			var result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();
			AssertNull(result.Agent);

			var person = new Mock<IImport008Person>();
			person.Setup(m => m.PassportNumber).Returns("KR0177360");

			import008Mock.Setup(m => m.FamilyMembers).Returns(new[] { person.Object });
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNotNull(result.Agent[0].Id.Value);
			AssertNull(result.Agent[0].Name);

			person.Setup(m => m.PassportNumber).Returns(ZString.Empty);
			person.Setup(m => m.Name).Returns("홍길동");
			import008Mock.Setup(m => m.FamilyMembers).Returns(new[] { person.Object });
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNull(result.Agent[0].Id);
			AssertNotNull(result.Agent[0].Name);

			person.Setup(m => m.BirthDate).Returns(new ZDate("1970-01-01"));
			person.Setup(m => m.RelationshipToImporter).Returns("딸");
			person.Setup(m => m.JobCode).Returns("1");

			import008Mock.Setup(m => m.FamilyMembers).Returns(new[] { person.Object });
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNotNull(result.Agent[0].Contact.BirthDate);
			AssertNotNull(result.Agent[0].Contact.Relationship);
			AssertNotNull(result.Agent[0].Contact.Occupation);

			person.Setup(m => m.BirthDate).Returns(ZDate.Empty);
			import008Mock.Setup(m => m.FamilyMembers).Returns(new[] { person.Object });
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNull(result.Agent[0].Contact.BirthDate);
			AssertNotNull(result.Agent[0].Contact.Relationship);
			AssertNotNull(result.Agent[0].Contact.Occupation);

			person.Setup(m => m.RelationshipToImporter).Returns(ZString.Empty);
			import008Mock.Setup(m => m.FamilyMembers).Returns(new[] { person.Object });
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNull(result.Agent[0].Contact.BirthDate);
			AssertNull(result.Agent[0].Contact.Relationship);
			AssertNotNull(result.Agent[0].Contact.Occupation);

			person.Setup(m => m.JobCode).Returns(ZString.Empty);
			import008Mock.Setup(m => m.FamilyMembers).Returns(new[] { person.Object });
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNull(result.Agent[0].Contact);

			person.Setup(m => m.EntryToKR_YN).Returns("Y");
			person.Setup(m => m.ScheduledStartDateInKR).Returns(new ZDate("2021-01-01"));
			person.Setup(m => m.ScheduledEndDateInKR).Returns(new ZDate("2021-01-02"));

			person.Setup(m => m.JobCode).Returns(ZString.Empty);
			import008Mock.Setup(m => m.FamilyMembers).Returns(new[] { person.Object });
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNotNull(result.Agent[0].AdditionalInformation.StatementCode);
			AssertNotNull(result.Agent[0].AdditionalInformation.BeginningDateTime);
			AssertNotNull(result.Agent[0].AdditionalInformation.EndingDateTime);

			person.Setup(m => m.EntryToKR_YN).Returns(ZString.Empty);
			import008Mock.Setup(m => m.FamilyMembers).Returns(new[] { person.Object });
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNull(result.Agent[0].AdditionalInformation.StatementCode);
			AssertNotNull(result.Agent[0].AdditionalInformation.BeginningDateTime);
			AssertNotNull(result.Agent[0].AdditionalInformation.EndingDateTime);

			person.Setup(m => m.ScheduledStartDateInKR).Returns(ZDate.Empty);
			import008Mock.Setup(m => m.FamilyMembers).Returns(new[] { person.Object });
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNull(result.Agent[0].AdditionalInformation.StatementCode);
			AssertNull(result.Agent[0].AdditionalInformation.BeginningDateTime);
			AssertNotNull(result.Agent[0].AdditionalInformation.EndingDateTime);

			person.Setup(m => m.ScheduledEndDateInKR).Returns(ZDate.Empty);
			import008Mock.Setup(m => m.FamilyMembers).Returns(new[] { person.Object });
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNull(result.Agent[0].AdditionalInformation);
		}

		public void TestEmptyConsignment()
		{
			var import008Mock = new Mock<IImport008Header>();
			var result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();
			AssertNull(result.Consignment);

			var line = new Mock<IImport008Line>();

			line.Setup(m => m.Quantity).Returns(1m);
			line.Setup(m => m.Model).Returns("양문형냉장고");
			line.Setup(m => m.BrandName).Returns("삼성");
			line.Setup(m => m.Price).Returns(1200000m);
			line.Setup(m => m.MonthOfUse).Returns(12);
			line.Setup(m => m.ItemCode).Returns("001");

			import008Mock.Setup(m => m.Lines).Returns(new[] { line.Object });
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.CargoDescription);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.CharacteristicCode);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.CountQuantity);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.Description);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.Name);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.ValueAmount);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.AdditionalInformation);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.Classification);

			line.Setup(m => m.Quantity).Returns(0m);

			import008Mock.Setup(m => m.Lines).Returns(new[] { line.Object });
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.CountQuantity);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.Description);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.Name);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.ValueAmount);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.AdditionalInformation);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.Classification);

			line.Setup(m => m.Model).Returns(ZString.Empty);
			import008Mock.Setup(m => m.Lines).Returns(new[] { line.Object });
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.CountQuantity);
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.Description);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.Name);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.ValueAmount);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.AdditionalInformation);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.Classification);

			line.Setup(m => m.BrandName).Returns(ZString.Empty);
			import008Mock.Setup(m => m.Lines).Returns(new[] { line.Object });
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.CountQuantity);
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.Description);
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.Name);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.ValueAmount);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.AdditionalInformation);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.Classification);

			line.Setup(m => m.Price).Returns(0m);
			import008Mock.Setup(m => m.Lines).Returns(new[] { line.Object });
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.CountQuantity);
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.Description);
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.Name);
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.ValueAmount);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.AdditionalInformation);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.Classification);

			line.Setup(m => m.MonthOfUse).Returns(0);
			import008Mock.Setup(m => m.Lines).Returns(new[] { line.Object });
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.CountQuantity);
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.Description);
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.Name);
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.ValueAmount);
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.AdditionalInformation);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.Classification);

			line.Setup(m => m.ItemCode).Returns(ZString.Empty);
			import008Mock.Setup(m => m.Lines).Returns(new[] { line.Object });
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.CountQuantity);
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.Description);
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.Name);
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.ValueAmount);
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.AdditionalInformation);
			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.Classification);
		}

		public void TestEmptyGoodsShipment()
		{
			var import008Mock = new Mock<IImport008Header>();
			var result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNotNull(result.GoodsShipment.AdditionalInformation);
			AssertNotNull(result.GoodsShipment.CustomsValuation);

			var bulkItem = new Mock<IImport008BulkItem>();
			bulkItem.Setup(m => m.SeatingCapacity).Returns(3);
			bulkItem.Setup(m => m.Type).Returns("1");
			bulkItem.Setup(m => m.ManufacturingCountry).Returns("KR");

			import008Mock.Setup(m => m.Vehicle).Returns(bulkItem.Object);
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalInformation);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Origin);

			bulkItem.Setup(m => m.SeatingCapacity).Returns(0);
			import008Mock.Setup(m => m.Vehicle).Returns(bulkItem.Object);
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalInformation);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Origin);

			bulkItem.Setup(m => m.Type).Returns(ZString.Empty);
			import008Mock.Setup(m => m.Vehicle).Returns(bulkItem.Object);
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].AdditionalInformation);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Origin);

			bulkItem.Setup(m => m.ManufacturingCountry).Returns(ZString.Empty);
			import008Mock.Setup(m => m.Vehicle).Returns(bulkItem.Object);
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem);
		}

		public void TestEmptyGovernmentAgencyGoodsItemCommodity()
		{
			var bulkItem = new Mock<IImport008BulkItem>();
			bulkItem.Setup(m => m.SeatingCapacity).Returns(3);
			bulkItem.Setup(m => m.CurrentRegistrationDate).Returns(new ZDate("2022-01-01"));
			bulkItem.Setup(m => m.FirstRegistrationDate).Returns(new ZDate("2021-01-01"));
			var import008Mock = new Mock<IImport008Header>();
			import008Mock.Setup(m => m.Vehicle).Returns(bulkItem.Object);
			var result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.Content);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.LimitDateTime);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.RegisterDateTime);

			bulkItem.Setup(m => m.SeatingCapacity).Returns(0);
			import008Mock.Setup(m => m.Vehicle).Returns(bulkItem.Object);
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.Content);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.LimitDateTime);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.RegisterDateTime);

			bulkItem.Setup(m => m.CurrentRegistrationDate).Returns(ZDate.Empty);
			import008Mock.Setup(m => m.Vehicle).Returns(bulkItem.Object);
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.Content);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.LimitDateTime);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.AdditionalInformation.RegisterDateTime);

			bulkItem.Setup(m => m.FirstRegistrationDate).Returns(ZDate.Empty);
			import008Mock.Setup(m => m.Vehicle).Returns(bulkItem.Object);
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem);

			bulkItem.Setup(m => m.EngineDisplacement).Returns(1500m);
			bulkItem.Setup(m => m.IdentificationNumber).Returns("WBADT63432CH90499");
			bulkItem.Setup(m => m.ModelYear).Returns("2000");
			bulkItem.Setup(m => m.ModelName).Returns("제우스");
			import008Mock.Setup(m => m.Vehicle).Returns(bulkItem.Object);
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Description);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Id);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.ManufactureDateTime);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Name);

			bulkItem.Setup(m => m.EngineDisplacement).Returns(0m);
			import008Mock.Setup(m => m.Vehicle).Returns(bulkItem.Object);
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Description);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Id);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.ManufactureDateTime);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Name);

			bulkItem.Setup(m => m.IdentificationNumber).Returns(ZString.Empty);
			import008Mock.Setup(m => m.Vehicle).Returns(bulkItem.Object);
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Description);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Id);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.ManufactureDateTime);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Name);

			bulkItem.Setup(m => m.ModelYear).Returns(ZString.Empty);
			import008Mock.Setup(m => m.Vehicle).Returns(bulkItem.Object);
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Description);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Id);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.ManufactureDateTime);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Name);

			bulkItem.Setup(m => m.ModelName).Returns(ZString.Empty);
			import008Mock.Setup(m => m.Vehicle).Returns(bulkItem.Object);
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem);
		}

		public void TestEmptySubmitter()
		{
			var import008Mock = new Mock<IImport008Header>();
			var result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();

			AssertNotNull(result.Submitter.Id);
			AssertNotNull(result.Submitter.Name);
			AssertNotNull(result.Submitter.RoleCode);
			AssertNotNull(result.Submitter.Address.CountryCode);
			AssertNotNull(result.Submitter.Address.Description);
			AssertNotNull(result.Submitter.Contact.Occupation);
			AssertNull(result.Submitter.Contact.BirthDate);
			AssertNull(result.Submitter.Address.CountrySubDivisionId);
			AssertNull(result.Submitter.Address.Line);
			AssertNull(result.Submitter.Address.PostcodeId);
			AssertNull(result.Submitter.Address.BuildingNumber);
			AssertNull(result.Submitter.Communication);
		}

		public void TestEmptyDate()
		{
			var import008Mock = new Mock<IImport008Header>();
			var person = new Mock<IImport008Person>();
			person.Setup(m => m.RelationshipToImporter).Returns("딸");
			person.Setup(m => m.BirthDate).Returns(ZDate.Empty);
			person.Setup(m => m.ScheduledStartDateInKR).Returns(ZDate.Empty);
			person.Setup(m => m.ScheduledEndDateInKR).Returns(ZDate.Empty);
			person.Setup(m => m.EntryToKR_YN).Returns(YesNoList.Codes.No);
			import008Mock.Setup(m => m.FamilyMembers).Returns(new[] { person.Object });
			import008Mock.Setup(m => m.Owner).Returns(person.Object);
			var result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();
			AssertNull(result.Agent[0].Contact.BirthDate);
			AssertNull(result.Agent[0].AdditionalInformation.BeginningDateTime);
			AssertNull(result.Agent[0].AdditionalInformation.EndingDateTime);
			AssertEquals(string.Empty, result.AdditionalInformation.BeginningDateTime);
			AssertEquals(string.Empty, result.AdditionalInformation.EndingDateTime);
			AssertNull(result.Submitter.Contact.BirthDate);

			person.Setup(m => m.BirthDate).Returns(ZDate.Invalid);
			person.Setup(m => m.ScheduledStartDateInKR).Returns(ZDate.Invalid);
			person.Setup(m => m.ScheduledEndDateInKR).Returns(ZDate.Invalid);
			import008Mock.Setup(m => m.FamilyMembers).Returns(new[] { person.Object });
			import008Mock.Setup(m => m.Owner).Returns(person.Object);
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();
			AssertNull(result.Agent[0].Contact.BirthDate);
			AssertNull(result.Agent[0].AdditionalInformation.BeginningDateTime);
			AssertNull(result.Agent[0].AdditionalInformation.EndingDateTime);
			AssertEquals(string.Empty, result.AdditionalInformation.BeginningDateTime);
			AssertEquals(string.Empty, result.AdditionalInformation.EndingDateTime);
			AssertNull(result.Submitter.Contact.BirthDate);

			person.Setup(m => m.EntryToKR_YN).Returns(ZString.Empty);
			import008Mock.Setup(m => m.FamilyMembers).Returns(new[] { person.Object });
			import008Mock.Setup(m => m.Owner).Returns(person.Object);
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();
			AssertNull(result.Agent[0].AdditionalInformation);

			person.Setup(m => m.BirthDate).Returns(new ZDate("1970-01-01"));
			person.Setup(m => m.ScheduledStartDateInKR).Returns(new ZDate("2025-01-01"));
			person.Setup(m => m.ScheduledEndDateInKR).Returns(new ZDate("2025-01-11"));
			import008Mock.Setup(m => m.FamilyMembers).Returns(new[] { person.Object });
			result = new GOVCBR008MessageBuilder(import008Mock.Object).GenerateMessage();
			AssertEquals("19700101", result.Agent[0].Contact.BirthDate);
			AssertEquals("20250101", result.Agent[0].AdditionalInformation.BeginningDateTime);
			AssertEquals("20250111", result.Agent[0].AdditionalInformation.EndingDateTime);
			AssertEquals("20250101", result.AdditionalInformation.BeginningDateTime);
			AssertEquals("20250111", result.AdditionalInformation.EndingDateTime);
			AssertEquals("19700101", result.Submitter.Contact.BirthDate);
		}
	}
}
