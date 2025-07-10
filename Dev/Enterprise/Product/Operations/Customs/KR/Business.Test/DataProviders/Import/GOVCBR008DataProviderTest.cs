using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR008DataProviderTest : XMLMessageTestHelper<GOVCBR008DataProviderTest>
	{
		public void TestHeader()
		{
			SetUp_Header();

			var result = new Import008Creator().Create(entry);

			AssertEquals("1235621434585M", result.ImportDeclarationNumber);
			AssertEquals("040", result.DeclarationCustomsOffice);
			AssertEquals("58", result.DeclarationCustomsDivision);
			AssertEquals(Core.Constants.CountryCodes.Philippines, result.ForeignCountryCode);
			AssertEquals("Manila", result.ForeignCity);
			AssertEquals("01", result.DecType);
			AssertEquals(Core.Constants.CountryCodes.Philippines, result.LoadingPort);
			AssertEquals("8236212036", result.HBL);
			AssertEquals(new ZDate(2020, 07, 19), result.TransportationStartDate);
			AssertEquals(new ZDate(2021, 04, 02), result.TransportationArrivalDate);
			AssertEquals(84500m, result.Freight);
			AssertEquals("DHL_foreign", result.ForeignCarrier);
			AssertEquals("DHL_domestic", result.DomesticCarrier);
		}

		public void TestDecQuestion()
		{
			var errCodeData1 = declaration.TransportMeans.AddNew();
			errCodeData1.CY_Code = "9";
			errCodeData1.CY_Data = "N";
			errCodeData1.CY_ParentID = entry.PK;

			var errCodeData2 = declaration.TransportMeans.AddNew();
			errCodeData2.CY_Code = "0";
			errCodeData2.CY_Data = "N";
			errCodeData2.CY_ParentID = entry.PK;

			SetUp_DecQuestion();

			var result = new Import008Creator().Create(entry);

			AssertEquals(8, result.QuestionsAndAnswers.Length);
			AssertEquals("1", result.QuestionsAndAnswers[0].QuestionID);
			AssertEquals("2", result.QuestionsAndAnswers[1].QuestionID);
			AssertEquals("3", result.QuestionsAndAnswers[2].QuestionID);
			AssertEquals("4", result.QuestionsAndAnswers[3].QuestionID);
			AssertEquals("5", result.QuestionsAndAnswers[4].QuestionID);
			AssertEquals("6", result.QuestionsAndAnswers[5].QuestionID);
			AssertEquals("7", result.QuestionsAndAnswers[6].QuestionID);
			AssertEquals("8", result.QuestionsAndAnswers[7].QuestionID);
			AssertEquals("N", result.QuestionsAndAnswers[0].Answer);
		}

		public void TestLine()
		{
			SetUp_Line();

			var result = new Import008Creator().Create(entry);

			AssertEquals(2, result.Lines.Length);

			AssertEquals("1", result.Lines[0].ItemCategory);
			AssertEquals("001", result.Lines[0].ItemCode);
			AssertEquals("TV", result.Lines[0].InvoiceDescription);
			AssertEquals("LG", result.Lines[0].BrandName);
			AssertEquals(5, result.Lines[1].MonthOfUse);
			AssertEquals(2m, result.Lines[1].Quantity);
			AssertEquals(250m, result.Lines[1].Price);
			AssertEquals("수동형랜즈장착", result.Lines[1].Model);
		}

		public void TestPerson()
		{
			SetUp_Person();

			var result = new Import008Creator().Create(entry);
			AssertEquals("주진상", result.Owner.Name);
			AssertEquals(new ZDate(1964, 06, 13), result.Owner.BirthDate);
			AssertEquals("M53481920", result.Owner.PassportNumber);
			AssertEquals("99", result.Owner.JobCode);
			AssertEquals(new ZDate(2020, 07, 19), result.Owner.ScheduledStartDateInKR);
			AssertEquals(new ZDate(2021, 02, 16), result.Owner.ScheduledEndDateInKR);
			AssertEquals(Core.Constants.CountryCodes.KoreaSouth, result.Owner.Nationality);
			AssertEquals("2", result.Owner.NationalityClassCode);

			AssertEquals(2, result.FamilyMembers.Length);
			AssertEquals("김성아", result.FamilyMembers[0].Name);
			AssertEquals("배우자", result.FamilyMembers[0].RelationshipToImporter);
			AssertEquals(new ZDate(1968, 11, 05), result.FamilyMembers[0].BirthDate);
			AssertEquals("M53481975", result.FamilyMembers[0].PassportNumber);
			AssertEquals("25", result.FamilyMembers[1].JobCode);
			AssertEquals("N", result.FamilyMembers[1].EntryToKR_YN);
			AssertEquals(new ZDate(2020, 08, 20), result.FamilyMembers[1].ScheduledStartDateInKR);
			AssertEquals(new ZDate(2021, 03, 18), result.FamilyMembers[1].ScheduledEndDateInKR);
		}

		public void TestBulkItem()
		{
			SetUp_BulkItem();
			var result = new Import008Creator().Create(entry);

			AssertEquals("1", result.Vehicle.Type);
			AssertEquals("자동차", result.Vehicle.ModelName);
			AssertEquals("WBADT63432CH90499", result.Vehicle.IdentificationNumber);
			AssertEquals(1500m, result.Vehicle.EngineDisplacement);
			AssertEquals("2014", result.Vehicle.ModelYear);
			AssertEquals(Core.Constants.CountryCodes.KoreaSouth, result.Vehicle.ManufacturingCountry);
			AssertEquals(5, result.Vehicle.SeatingCapacity);
			AssertEquals(new ZDate(2014, 05, 06), result.Vehicle.FirstRegistrationDate);
			AssertEquals(new ZDate(2014, 05, 06), result.Vehicle.CurrentRegistrationDate);
		}

		public void TestXML()
		{
			SetUp_Header();
			SetUp_DecQuestion();
			SetUp_Line();
			SetUp_Person();
			SetUp_BulkItem();

			var import008 = new Import008Creator().Create(entry);
			var result = new GOVCBR008MessageBuilder(import008).GenerateMessage();
			var fileReader = new TestFileReader(typeof(GOVCBR008DataProviderTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR008_D1.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
		}

		public void TestEmptyXML()
		{
			declaration.PersonalItemDecQuestions.AddNew();
			var import008 = new Import008Creator().Create(entry);
			var result = new GOVCBR008MessageBuilder(import008).GenerateMessage();
			var fileReader = new TestFileReader(typeof(GOVCBR008DataProviderTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR008_Empty.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();

			entry = declaration.CustomsEntryHeaders.AddNew();
		}
		JobDeclaration declaration;
		CusEntryHeader entry;

		void SetUp_Header()
		{
			var foreignCarrier = Factory.NewWithValidTestData<OrgHeader>();
			foreignCarrier.OH_FullName = "DHL_foreign";

			var domesticCarrier = Factory.NewWithValidTestData<OrgHeader>();
			domesticCarrier.OH_FullName = "DHL_domestic";

			declaration.JE_RL_NKOrigin = "PHMNA";
			declaration.JE_CustomsOffice = "040";
			declaration.JE_CustomsDivision = "58";
			declaration.JE_RL_NKOrigin = "PHMNL";
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "01";
			declaration.JE_RL_NKPortOfLoading = Core.Constants.CountryCodes.Philippines;
			declaration.JE_HouseBill = "8236212036";
			declaration.JE_ExportDate = new ZDate(2020, 07, 19);
			declaration.JE_DateOfArrival = new ZDate(2021, 04, 02);
			declaration.JE_OH_ShippingLine = foreignCarrier.PK;
			declaration.JE_OA_DeliveryOrPickupCartageCoAddr = domesticCarrier.MainAddress.PK;

			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryNum = "1235621434585M";
			entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._008;

			var charge = declaration.Invoices.AddNew().Charges.AddNew();
			charge.J7_Amount = 84500m;
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
		}

		void SetUp_DecQuestion()
		{
			var pornography = declaration.PersonalItemDecQuestions.AddNew();
			pornography.CY_Code = Import008DecQuestion.DecQuestion.PossessingPornography;
			pornography.CY_Data = Constants.YesNo.No;
			pornography.CY_ParentID = entry.PK;

			var itemBeyondDeclarationDueDate = declaration.PersonalItemDecQuestions.AddNew();
			itemBeyondDeclarationDueDate.CY_Code = Import008DecQuestion.DecQuestion.PossessingItemBeyondDeclarationDueDate;
			itemBeyondDeclarationDueDate.CY_Data = Constants.YesNo.No;
			itemBeyondDeclarationDueDate.CY_ParentID = entry.PK;

			var commercialUse = declaration.PersonalItemDecQuestions.AddNew();
			commercialUse.CY_Code = Import008DecQuestion.DecQuestion.PossessingCommercialUse;
			commercialUse.CY_Data = Constants.YesNo.No;
			commercialUse.CY_ParentID = entry.PK;

			var counterfeitItem = declaration.PersonalItemDecQuestions.AddNew();
			counterfeitItem.CY_Code = Import008DecQuestion.DecQuestion.PossessingCounterfeitItem;
			counterfeitItem.CY_Data = Constants.YesNo.No;
			counterfeitItem.CY_ParentID = entry.PK;

			var endangeredSpecies = declaration.PersonalItemDecQuestions.AddNew();
			endangeredSpecies.CY_Code = Import008DecQuestion.DecQuestion.PossessingEndangeredSpecies;
			endangeredSpecies.CY_Data = Constants.YesNo.No;
			endangeredSpecies.CY_ParentID = entry.PK;

			var liveAnimal = declaration.PersonalItemDecQuestions.AddNew();
			liveAnimal.CY_Code = Import008DecQuestion.DecQuestion.PossessingLiveAnimal;
			liveAnimal.CY_Data = Constants.YesNo.No;
			liveAnimal.CY_ParentID = entry.PK;

			var drug = declaration.PersonalItemDecQuestions.AddNew();
			drug.CY_Code = Import008DecQuestion.DecQuestion.PossessingDrug;
			drug.CY_Data = Constants.YesNo.No;
			drug.CY_ParentID = entry.PK;

			var weapon = declaration.PersonalItemDecQuestions.AddNew();
			weapon.CY_Code = Import008DecQuestion.DecQuestion.PossessingWeapon;
			weapon.CY_Data = Constants.YesNo.No;
			weapon.CY_ParentID = entry.PK;
		}

		void SetUp_Line()
		{
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_ProductTypeCode = "3";
			invoiceLine2.JI_InvoiceUQ = "003";
			invoiceLine2.JI_Description = "카메라";
			invoiceLine2.JI_BrandName = "삼성";
			invoiceLine2.JI_CustomsQuantity = 5m;
			invoiceLine2.JI_InvoiceQuantity = 2m;
			invoiceLine2.JI_LinePrice = 250m;
			invoiceLine2.JI_Model = "수동형랜즈장착";
			invoiceLine2.JI_LineNo = 2;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_ProductTypeCode = "1";
			invoiceLine1.JI_InvoiceUQ = "001";
			invoiceLine1.JI_Description = "TV";
			invoiceLine1.JI_BrandName = "LG";
			invoiceLine1.JI_CustomsQuantity = 2m;
			invoiceLine1.JI_InvoiceQuantity = 10m;
			invoiceLine1.JI_LinePrice = 1542m;
			invoiceLine1.JI_Model = "LG TV";
			invoiceLine1.JI_LineNo = 1;
		}

		void SetUp_Person()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			importer.OA_Mobile = "821023047856";
			importer.OA_Email = "abc@naver.com";
			importer.OA_Address1 = "서울 송파구 올림픽로 135 (리센츠)";
			importer.OA_Address2 = "103동 201호";
			importer.CustomsCodes.AddNew(MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.RoadNameCode, "110001");
			importer.CustomsCodes.AddNew(MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.BuildingNumber, "121200");
			importer.Postcode = "10535";
			declaration.JE_OA_ImporterAddress = importer.PK;

			var owner = declaration.Persons.AddNew();
			owner.JobCode = "99";
			owner.ResidencyStartDate = new ZDate(2020, 07, 19);
			owner.ResidencyEndDate = new ZDate(2021, 02, 16);
			owner.NationalityClassCode = "2";

			owner.CPN_PER_Person = Factory.New<GlbPerson>().PK;
			owner.Person.PER_FullName = "주진상";
			owner.Person.PER_BirthDate = new ZDate(1964, 06, 13);
			owner.Person.PER_Passport = "M53481920";
			owner.Person.PER_RN_NKNationalityCodeISO = Core.Constants.CountryCodes.KoreaSouth;
			owner.RelationshipToDeclarant = "본인";

			var family1 = declaration.Persons.AddNew();
			family1.RelationshipToDeclarant = "배우자";
			family1.JobCode = "26";
			family1.EntryStatus = "N";
			family1.ResidencyStartDate = new ZDate(2020, 06, 18);
			family1.ResidencyEndDate = new ZDate(2021, 03, 15);

			family1.CPN_PER_Person = Factory.New<GlbPerson>().PK;
			family1.Person.PER_FullName = "김성아";
			family1.Person.PER_BirthDate = new ZDate(1968, 11, 05);
			family1.Person.PER_Passport = "M53481975";
			family1.Person.PER_RN_NKNationalityCodeISO = Core.Constants.CountryCodes.KoreaSouth;

			var family2 = declaration.Persons.AddNew();
			family2.RelationshipToDeclarant = "자녀";
			family2.JobCode = "25";
			family2.EntryStatus = "N";
			family2.ResidencyStartDate = new ZDate(2020, 08, 20);
			family2.ResidencyEndDate = new ZDate(2021, 03, 18);

			family2.CPN_PER_Person = Factory.New<GlbPerson>().PK;
			family2.Person.PER_FullName = "주자녀";
			family2.Person.PER_BirthDate = new ZDate(1995, 04, 23);
			family2.Person.PER_Passport = "M53481234";
			family2.Person.PER_RN_NKNationalityCodeISO = Core.Constants.CountryCodes.KoreaSouth;
		}

		void SetUp_BulkItem()
		{
			declaration.ModelName = "자동차";
			declaration.VehicleIdentificationNumber = "WBADT63432CH90499";
			declaration.EngineCapacity = 1500;
			declaration.ModelYear = "2014";
			declaration.CountryOfManufacture = Core.Constants.CountryCodes.KoreaSouth;
			declaration.SeatingCapacity = 5;
			declaration.DateOfFirstRegistration = new ZDate(2014, 05, 06);
			declaration.DateOfCurrentRegistration = new ZDate(2014, 05, 06);
		}
	}
}
