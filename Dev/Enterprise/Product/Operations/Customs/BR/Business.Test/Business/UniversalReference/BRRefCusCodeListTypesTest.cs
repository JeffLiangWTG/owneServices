using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;
using ECC = Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRRefCusCodeListTypesTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestGetCustomsOfficeList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helper.CreateCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0000001", "Test1", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0000002", "Test2", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			Factory.Save();
			var list1 = BRRefCusCodeListTypes.GetCustomsOfficeList(Factory);
			var list2 = BRRefCusCodeListTypes.GetCustomsOfficeList(Factory);
			AssertSame(list1, list2);
			Assert(!object.ReferenceEquals(list1, BRRefCusCodeListTypes.GetCustomsOfficeList(new BusinessObjectFactory())));
			AssertEquals("Test1", list1.GetDescriptionFromCode("0000001"));
		}

		public void TestGetCustomsEnclosureList()
		{
			var helperCustomsOffice = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helperCustomsOffice.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helperCustomsOffice.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0000001", "Test1", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			Factory.Save();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Customs Enclosure");
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "0000001", "Customs Enclosure Test1", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "0000002", "Customs Enclosure Test2", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "0000003", "Customs Enclosure TestExpired", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(-1));
			Factory.Save();
			var customsEnclosureList = BRRefCusCodeListTypes.GetCustomsEnclosureList(Factory);

			CombineAssertions(() =>
			{
				AssertEquals("GetCustomsEnclosureList must have", 2, customsEnclosureList.Count);
				Assert(!object.ReferenceEquals(customsEnclosureList, BRRefCusCodeListTypes.GetCustomsEnclosureList(new BusinessObjectFactory())));
				AssertEquals("Customs Enclosure Test1", customsEnclosureList.GetDescriptionFromCode("0000001"));
			});
		}

		public void TestGetIssuingAgencyList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRIssuingAgency, "Issuing Agency");
			helper.CreateCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRIssuingAgency, "TE1", "Issuing Agency Test", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRIssuingAgency, "TE2", "Issuing Agency Test 2", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(-1));
			Factory.Save();

			var collection = BRRefCusCodeListTypes.GetIssuingAgencyList(Factory);
			collection.Load();

			AssertEquals(1, collection.Count);
			AssertEquals("Issuing Agency Test", collection[0].ZZD_Description);
		}

		public void TestGetConsentingBodyList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRConsentingBodyCode, "Consenting Body");
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRConsentingBodyCode, "Consent001", "Consenting Body Test 001", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRConsentingBodyCode, "Consent002", "Consenting Body Test 002", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRConsentingBodyCode, "Consent003", "Consenting Body Test 003", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(-1));
			Factory.Save();

			var collection = BRRefCusCodeListTypes.GetConsentingBodyList(Factory);

			AssertEquals(2, collection.Count);
			AssertEquals("Consenting Body Test 001", collection.GetDescriptionFromCode("Consent001"));
		}

		public void TestGetExchangeHedgePaymentMethodList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRExchangeHedgePaymentCode, "Exchange Hedge Method of Payment");
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRExchangeHedgePaymentCode, "EX10", "Method of Payment 01", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRExchangeHedgePaymentCode, "EX20", "Method of Payment 02", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRExchangeHedgePaymentCode, "EX30", "Method of Payment 03", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(-1));
			Factory.Save();

			var collection = BRRefCusCodeListTypes.GetExchangeHedgePaymentMethodList(Factory);

			AssertEquals(2, collection.Count);
			AssertEquals("Method of Payment 01", collection.GetDescriptionFromCode("EX10"));
		}

		public void TestGetTaxRegimeList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			ReferenceTestDataHelper.CreateReferenceDataForTaxRegimeList(Factory, BRJobMessageTypeList.Codes.ImportSiscomex);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxationRegimeCode, "4", "REDUCAO", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			Factory.Save();

			var testCollection = BRRefCusCodeListTypes.GetTaxRegimeList(Factory, BRJobMessageTypeList.Codes.ImportSiscomex, MessageSubTypeList.Codes._01, ZDateTime.Now.AddDays(5), Constants.ProcedureCategories.Duty);

			CombineAssertions(() =>
			{
				AssertEquals("3 Procedure for Brazil ISW - 01", 3, testCollection.Count);
				AssertEquals("First Procedure code should be 1", "1", testCollection[0].Code);
				AssertEquals("First Procedure description should be RECOLHIMENTO INTEGRAL", "RECOLHIMENTO INTEGRAL", testCollection[0].Description);
				AssertEquals("Second Procedure code should be 2", "2", testCollection[1].Code);
				AssertEquals("Second Procedure description should be IMUNIDADE", "IMUNIDADE", testCollection[1].Description);
				AssertEquals("third Procedure code should be 3", "3", testCollection[2].Code);
				AssertEquals("third Procedure description should be ISENCAO", "ISENCAO", testCollection[2].Description);
			});

			testCollection = BRRefCusCodeListTypes.GetTaxRegimeList(Factory, BRJobMessageTypeList.Codes.ImportSiscomex, MessageSubTypeList.Codes._01, ZDateTime.Now.AddDays(5), Constants.ProcedureCategories.PisCofins);

			CombineAssertions(() =>
			{
				AssertEquals("2 Procedure for Brazil ISW - 01", 2, testCollection.Count);
				AssertEquals("First Procedure code should be 1", "1", testCollection[0].Code);
				AssertEquals("First Procedure description should be RECOLHIMENTO INTEGRAL", "RECOLHIMENTO INTEGRAL", testCollection[0].Description);
				AssertEquals("Second Procedure code should be 2", "2", testCollection[1].Code);
				AssertEquals("Second Procedure description should be IMUNIDADE", "IMUNIDADE", testCollection[1].Description);
			});

			testCollection = BRRefCusCodeListTypes.GetTaxRegimeList(Factory, BRJobMessageTypeList.Codes.ImportLicense, ZString.Empty, ZDateTime.Now.AddDays(5), ZString.Empty);

			CombineAssertions(() =>
			{
				AssertEquals("4 Procedure for Brazil LIC - 01", 4, testCollection.Count);
				AssertEquals("First Procedure code should be 1", "1", testCollection[0].Code);
				AssertEquals("First Procedure description should be RECOLHIMENTO INTEGRAL", "RECOLHIMENTO INTEGRAL", testCollection[0].Description);
				AssertEquals("Second Procedure code should be 2", "2", testCollection[1].Code);
				AssertEquals("Second Procedure description should be IMUNIDADE", "IMUNIDADE", testCollection[1].Description);
				AssertEquals("third Procedure code should be 3", "3", testCollection[2].Code);
				AssertEquals("third Procedure description should be ISENCAO", "ISENCAO", testCollection[2].Description);
				AssertEquals("fourth Procedure code should be 4", "4", testCollection[3].Code);
				AssertEquals("fourth Procedure description should be REDUCAO", "REDUCAO", testCollection[3].Description);
			});
		}

		public void TestGetLegalBaseList()
		{
			CombineAssertions(() =>
			{
				var taxRegime = "1";
				var startDate = ZDateTime.Now.AddDays(-30);
				var endDate = ZDateTime.Now.AddDays(30);

				var testCusProcedure = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure.ZZ6_ProcedureCode = MessageSubTypeList.Codes._01;
				testCusProcedure.ZZ6_ZZZ_NKDataGrouping = ECC.CountryCodes.Brazil;
				testCusProcedure.ZZ6_PreviousProcedureCode = taxRegime;
				testCusProcedure.ZZ6_Concession = string.Empty;
				testCusProcedure.ZZ6_ShipmentType = BRJobMessageTypeList.Codes.ImportSiscomex;
				testCusProcedure.ZZ6_StartDate = startDate;
				testCusProcedure.ZZ6_EndDate = endDate;
				testCusProcedure.ZZ6_Category = Constants.ProcedureCategories.Duty;

				var testCusProcedure1 = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure1.ZZ6_ProcedureCode = MessageSubTypeList.Codes._01;
				testCusProcedure1.ZZ6_PreviousProcedureCode = taxRegime;
				testCusProcedure1.ZZ6_Concession = "01";
				testCusProcedure1.ZZ6_ShipmentType = BRJobMessageTypeList.Codes.ImportSiscomex;
				testCusProcedure1.ZZ6_ZZZ_NKDataGrouping = ECC.CountryCodes.Brazil;
				testCusProcedure1.ZZ6_StartDate = startDate;
				testCusProcedure1.ZZ6_EndDate = endDate;
				testCusProcedure1.ZZ6_Category = Constants.ProcedureCategories.Duty;

				var testCusProcedure2 = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure2.ZZ6_ProcedureCode = MessageSubTypeList.Codes._01;
				testCusProcedure2.ZZ6_PreviousProcedureCode = taxRegime;
				testCusProcedure2.ZZ6_Concession = "02";
				testCusProcedure2.ZZ6_ShipmentType = BRJobMessageTypeList.Codes.ImportSiscomex;
				testCusProcedure2.ZZ6_ZZZ_NKDataGrouping = ECC.CountryCodes.Brazil;
				testCusProcedure2.ZZ6_StartDate = startDate;
				testCusProcedure2.ZZ6_EndDate = endDate;
				testCusProcedure2.ZZ6_Category = Constants.ProcedureCategories.Duty;

				var testCusProcedure3 = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure3.ZZ6_ProcedureCode = MessageSubTypeList.Codes._01;
				testCusProcedure3.ZZ6_PreviousProcedureCode = taxRegime;
				testCusProcedure3.ZZ6_Concession = "02";
				testCusProcedure3.ZZ6_ShipmentType = BRJobMessageTypeList.Codes.ImportSiscomex;
				testCusProcedure3.ZZ6_ZZZ_NKDataGrouping = ECC.CountryCodes.Brazil;
				testCusProcedure3.ZZ6_StartDate = startDate;
				testCusProcedure3.ZZ6_EndDate = endDate;
				testCusProcedure3.ZZ6_Category = Constants.ProcedureCategories.PisCofins;

				var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalBaseCode, "BR Legal Base Regime");
				helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalBaseCode, "01", "LIVROS, JORNAIS E PERIODICOS - CF/88, ART.150,VI,D - LEI 8032/90, ART.2,II,A - LEI 8402/92, ART 1,IV", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
				helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalBaseCode, "02", "PAPEL DEST. A IMPRESSAO D/LIVROS, JORNAIS E PERIODICOS - CF/88,ART.150,VI,D - L. 8032/90,ART.2,II,A - L. 8402/92,ART.1,IV", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
				helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalBaseCode, "03", "UNIãO,ESTADOS,DF E MUNICíPIOS;AUTARQUIAS E FUNDS.INSTITUíDAS E MANTIDAS P/PODER PúBLICO - CF,ART.150,INC.VI,A C/C PAR.2º", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));

				helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRPisLegalBaseCode, "BR Pis Legal Base Regime");
				helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRPisLegalBaseCode, "01", "Pis Cofins Legal base LIVROS, JORNAIS E PERIODICOS - CF/88, ART.150,VI,D - LEI 8032/90, ART.2,II,A - LEI 8402/92, ART 1,IV", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
				helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRPisLegalBaseCode, "02", "Pis Cofins Legal base PAPEL DEST. A IMPRESSAO D/LIVROS, JORNAIS E PERIODICOS - CF/88,ART.150,VI,D - L. 8032/90,ART.2,II,A - L. 8402/92,ART.1,IV", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
				helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRPisLegalBaseCode, "03", "Pis Cofins Legal base UNIãO,ESTADOS,DF E MUNICíPIOS;AUTARQUIAS E FUNDS.INSTITUíDAS E MANTIDAS P/PODER PúBLICO - CF,ART.150,INC.VI,A C/C PAR.2º", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));

				Factory.Save();

				var testCollection = BRRefCusCodeListTypes.GetLegalBaseList(Factory, BRJobMessageTypeList.Codes.ImportSiscomex, MessageSubTypeList.Codes._01, taxRegime, ZDateTime.Now.AddDays(5), Constants.ProcedureCategories.Duty, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalBaseCode);

				AssertEquals("2 Procedure for Brazil IMP - 01", 2, testCollection.Count);
				AssertEquals("First Procedure code should be 1", "01", testCollection[0].Code);
				AssertEquals("First Procedure description should be LIVROS, JORNAIS E PERIODICOS - CF/88, ART.150,VI,D - LEI 8032/90, ART.2,II,A - LEI 8402/92, ART 1,IV", "LIVROS, JORNAIS E PERIODICOS - CF/88, ART.150,VI,D - LEI 8032/90, ART.2,II,A - LEI 8402/92, ART 1,IV", testCollection[0].Description);
				AssertEquals("Second Procedure code should be 2", "02", testCollection[1].Code);
				AssertEquals("Second Procedure description should be PAPEL DEST.A IMPRESSAO D / LIVROS, JORNAIS E PERIODICOS - CF / 88, ART.150, VI, D - L. 8032 / 90, ART.2, II, A - L. 8402 / 92, ART.1, IV", "PAPEL DEST. A IMPRESSAO D/LIVROS, JORNAIS E PERIODICOS - CF/88,ART.150,VI,D - L. 8032/90,ART.2,II,A - L. 8402/92,ART.1,IV", testCollection[1].Description);

				testCollection = BRRefCusCodeListTypes.GetLegalBaseList(Factory, BRJobMessageTypeList.Codes.ImportLicense, MessageSubTypeList.Codes._01, ZString.Empty, ZDateTime.Now.AddDays(5), Constants.ProcedureCategories.Duty, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalBaseCode);

				AssertEquals("3 Procedure for Brazil LIC - 01", 3, testCollection.Count);
				AssertEquals("First Procedure code should be 1", "01", testCollection[0].Code);
				AssertEquals("First Procedure description should be LIVROS, JORNAIS E PERIODICOS - CF/88, ART.150,VI,D - LEI 8032/90, ART.2,II,A - LEI 8402/92, ART 1,IV", "LIVROS, JORNAIS E PERIODICOS - CF/88, ART.150,VI,D - LEI 8032/90, ART.2,II,A - LEI 8402/92, ART 1,IV", testCollection[0].Description);
				AssertEquals("Second Procedure code should be 2", "02", testCollection[1].Code);
				AssertEquals("Second Procedure description should be PAPEL DEST.A IMPRESSAO D / LIVROS, JORNAIS E PERIODICOS - CF / 88, ART.150, VI, D - L. 8032 / 90, ART.2, II, A - L. 8402 / 92, ART.1, IV", "PAPEL DEST. A IMPRESSAO D/LIVROS, JORNAIS E PERIODICOS - CF/88,ART.150,VI,D - L. 8032/90,ART.2,II,A - L. 8402/92,ART.1,IV", testCollection[1].Description);
				AssertEquals("third Procedure code should be 3", "03", testCollection[2].Code);
				AssertEquals("third Procedure description should be UNIãO,ESTADOS,DF E MUNICíPIOS;AUTARQUIAS E FUNDS.INSTITUíDAS E MANTIDAS P/PODER PúBLICO - CF,ART.150,INC.VI,A C/C PAR.2º", "UNIãO,ESTADOS,DF E MUNICíPIOS;AUTARQUIAS E FUNDS.INSTITUíDAS E MANTIDAS P/PODER PúBLICO - CF,ART.150,INC.VI,A C/C PAR.2º", testCollection[2].Description);

				testCollection = BRRefCusCodeListTypes.GetLegalBaseList(Factory, BRJobMessageTypeList.Codes.ImportSiscomex, MessageSubTypeList.Codes._01, taxRegime, ZDateTime.Now.AddDays(5), Constants.ProcedureCategories.PisCofins, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRPisLegalBaseCode);

				AssertEquals("1 Procedure for Brazil IMP - 01", 1, testCollection.Count);
				AssertEquals("First Procedure code should be 02", "02", testCollection[0].Code);
				AssertEquals("First Procedure description should be Pis Cofins Legal base PAPEL DEST. A IMPRESSAO D/LIVROS, JORNAIS E PERIODICOS - CF/88,ART.150,VI,D - L. 8032/90,ART.2,II,A - L. 8402/92,ART.1,IV", "Pis Cofins Legal base PAPEL DEST. A IMPRESSAO D/LIVROS, JORNAIS E PERIODICOS - CF/88,ART.150,VI,D - L. 8032/90,ART.2,II,A - L. 8402/92,ART.1,IV", testCollection[0].Description);
			});
		}

		public void TestGetWarehousingSectorList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRWarehousingSectorsCode, "Warehousing Sectors");
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRWarehousingSectorsCode, "CO00001;CE00001;WS00001", "Warehousing Sector Test1", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRWarehousingSectorsCode, "CO00002;CE00002;WS00002", "Warehousing Sector Test2", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRWarehousingSectorsCode, "CO00001;CE00002;WS00003", "Warehousing Sector Test3", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRWarehousingSectorsCode, "CO00002;CE00001;WS00004", "Warehousing Sector Test4", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRWarehousingSectorsCode, "CO00001;CE00001;WS00005", "Warehousing Sector Test5", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRWarehousingSectorsCode, "WS00006", "Incorrect format code", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			Factory.Save();

			var list = BRRefCusCodeListTypes.GetWarehousingSectorList(Factory, ZString.Empty, ZString.Empty);
			AssertEquals("Must be empty", 0, list.Count);

			list = BRRefCusCodeListTypes.GetWarehousingSectorList(Factory, ZString.Empty, "CE00001");
			AssertEquals("Must be empty", 0, list.Count);

			list = BRRefCusCodeListTypes.GetWarehousingSectorList(Factory, "CO00001", ZString.Empty);
			AssertEquals("Must be empty", 0, list.Count);

			list = BRRefCusCodeListTypes.GetWarehousingSectorList(Factory, "CO00001", "CE00001");
			AssertContainsExactElementsInExactOrder(new[] { "WS00001", "WS00005" }, list.GetAllCodes());

			list = BRRefCusCodeListTypes.GetWarehousingSectorList(Factory, "CO00002", "CE00001");
			AssertContainsExactElementsInExactOrder(new[] { "WS00004" }, list.GetAllCodes());
			AssertSame("List cached", list, BRRefCusCodeListTypes.GetWarehousingSectorList(Factory, "CO00002", "CE00001"));
		}

		public void TestGetExTariffLegalActList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRExTariffLegalAct, "Ex Tariff Legal Act");
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRExTariffLegalAct, "EX1", "Ex Tariff Legal Act 01", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRExTariffLegalAct, "EX2", "Ex Tariff Legal Act 02", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRExTariffLegalAct, "EX3", "Ex Tariff Legal Act 03", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(-1));
			Factory.Save();

			var collection = BRRefCusCodeListTypes.GetExTariffLegalActList(Factory);

			AssertEquals(2, collection.Count);
			AssertEquals("Ex Tariff Legal Act 01", collection.GetDescriptionFromCode("EX1"));
		}

		public void TestGetLegalActIssuingAuthorityList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalActIssuingAuthority, "Legal Act Issuing Authority");
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalActIssuingAuthority, "EX1", "Legal Act Issuing Authority 01", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalActIssuingAuthority, "EX2", "Legal Act Issuing Authority 02", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalActIssuingAuthority, "EX3", "Legal Act Issuing Authority 03", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(-1));
			Factory.Save();

			var collection = BRRefCusCodeListTypes.GetLegalActIssuingAuthorityList(Factory);

			AssertEquals(2, collection.Count);
			AssertEquals("Legal Act Issuing Authority 01", collection.GetDescriptionFromCode("EX1"));
		}

		public void TestGetTariffAgreementCodeList()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);

			var tariffAgreementCodesList = BRRefCusCodeListTypes.GetTariffAgreementCodeList(Factory, ZDateTime.Now);

			CombineAssertions(() =>
			{
				AssertEquals(4, tariffAgreementCodesList.Count);
				AssertEquals("AR99, ASGPC, CO99, MX99", tariffAgreementCodesList.CodesAsString);
			});

			AssertEquals("AR99", BRRefCusCodeListTypes.GetTariffAgreementCode(Factory, "AR99", ZDateTime.Now).ZZD_Code);
		}

		public void TestGetICMSLegalBaseList()
		{
			ReferenceTestDataHelper.CreateReferenceDataForICMSLegalBaseList(Factory);
			var collection = BRRefCusCodeListTypes.GetICMSLegalBaseList(Factory);

			AssertEquals(2, collection.Count);
			AssertEquals("ICMS Legal Base 01", collection.GetDescriptionFromCode("01"));
		}

		public void TestGetEntryStatusList()
		{
			ReferenceTestDataHelper.CreateEntryStatusForLicenseAndExportList(Factory);
			var collection = BRRefCusCodeListTypes.GetEntryStatusList(Factory);

			AssertEquals(17, collection.Count);
			AssertEquals("Registered", collection.GetDescriptionFromCode("E10"));
		}

		public void TestGetCustomsStatusByCustomsCode()
		{
			ReferenceTestDataHelper.CreateEntryStatusForExport(Factory);
			ReferenceTestDataHelper.CreateEntryStatusForImport(Factory);
			ReferenceTestDataHelper.CreateEntryStatusForLPCO(Factory);

			Factory.Save();
			AssertEquals("E01", BRRefCusCodeListTypes.GetCustomsStatusByCustomsCode(Factory, "EM_ELABORACAO", EntryStatusListHelper.ExportEntryStatusPrefix));
			AssertEquals("E10", BRRefCusCodeListTypes.GetCustomsStatusByCustomsCode(Factory, "REGISTRADA", EntryStatusListHelper.ExportEntryStatusPrefix));
			AssertEquals("E70", BRRefCusCodeListTypes.GetCustomsStatusByCustomsCode(Factory, "AVERBADA_SEM_DIVERGENCIA", EntryStatusListHelper.ExportEntryStatusPrefix));
			AssertEquals("", BRRefCusCodeListTypes.GetCustomsStatusByCustomsCode(Factory, "XXXXX", EntryStatusListHelper.ExportEntryStatusPrefix));

			AssertEquals("I11", BRRefCusCodeListTypes.GetCustomsStatusByCustomsCode(Factory, "REGISTRADA_AGUARDANDO_CANAL", EntryStatusListHelper.ImportEntryStatusPrefix));
			AssertEquals("I31", BRRefCusCodeListTypes.GetCustomsStatusByCustomsCode(Factory, "EM_CONFERENCIA_SELECIONADA", EntryStatusListHelper.ImportEntryStatusPrefix));
			AssertEquals("I43", BRRefCusCodeListTypes.GetCustomsStatusByCustomsCode(Factory, "DESEMBARACADA_CARGA_ENTREGUE", EntryStatusListHelper.ImportEntryStatusPrefix));
			AssertEquals("", BRRefCusCodeListTypes.GetCustomsStatusByCustomsCode(Factory, "XXXXX", EntryStatusListHelper.ImportEntryStatusPrefix));

			AssertEquals("P01", BRRefCusCodeListTypes.GetCustomsStatusByCustomsCode(Factory, "PARA_ANALISE", EntryStatusListHelper.LPCOStatusPrefix));
			AssertEquals("P02", BRRefCusCodeListTypes.GetCustomsStatusByCustomsCode(Factory, "EM_ANALISE", EntryStatusListHelper.LPCOStatusPrefix));
			AssertEquals("P17", BRRefCusCodeListTypes.GetCustomsStatusByCustomsCode(Factory, "RECURSO_DIVERSO", EntryStatusListHelper.LPCOStatusPrefix));
			AssertEquals("", BRRefCusCodeListTypes.GetCustomsStatusByCustomsCode(Factory, "XXXXX", EntryStatusListHelper.LPCOStatusPrefix));
		}

		public void TestGetTariffAgreementCodeByTypeAndAgreementCodeInImportEntry()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);
			var result = BRRefCusCodeListTypes.GetTariffAgreementCodeByTypeAndAgreementCodeInImportEntry(Factory, Constants.TariffAgreementTypes.Aladi, "336");
			AssertEquals("MX99", result);

			result = BRRefCusCodeListTypes.GetTariffAgreementCodeByTypeAndAgreementCodeInImportEntry(Factory, Constants.TariffAgreementTypes.SGPC, "336");
			Assert(result.IsEmpty);

			result = BRRefCusCodeListTypes.GetTariffAgreementCodeByTypeAndAgreementCodeInImportEntry(Factory, Constants.TariffAgreementTypes.Aladi, "386");
			Assert(result.IsEmpty);
		}

		public void TestGetDuimpLegalBaseList()
		{
			ReferenceTestDataHelper.CreateDuimpLegalBaseCodes(Factory);
			AssertContainsExactElementsInAnyOrder(new[] { "P01", "P02", "P03", "P04" }, BRRefCusCodeListTypes.GetDuimpLegalBaseList(Factory).GetAllCodes());
		}
	}
}
