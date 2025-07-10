using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInvoiceLine()
		{
			JobComInvoiceLine parent = Factory.New<JobComInvoiceLine>();
			AssertEquals(parent.Lookups.InvoiceLine, parent);
		}

		public void TestCargoPriorityCodeList()
		{
			JobComInvoiceLine parent = Factory.New<JobComInvoiceLine>();
			AssertNotNull(parent.Lookups.CargoPriorityList);
			Assert(parent.Lookups.CargoPriorityList is ICodeDescriptionPairList);
			AssertEquals(4, parent.Lookups.CargoPriorityList.Count);
		}

		public void TestCusEntryInstructionCollection()
		{
			var oDeclaration = Factory.New<JobDeclaration>();
			oDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var oInstruction1 = oDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			oInstruction1.CEI_Description = "Inst-1";
			var oInstruction2 = oDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			oInstruction2.CEI_Description = "Inst-2";
			var oInstruction3 = oDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			oInstruction3.CEI_Description = "Inst-3";
			var oInvHeader = oDeclaration.Invoices.AddNew();
			var oInvLine = oInvHeader.InvoiceLines.AddNew();
			var oLookups = oInvLine.Lookups;
			CombineAssertions("TestCPC", () =>
			{
				AssertEquals("Test Count", 3, oLookups.CustomsProcedureCodes.Count);
				AssertEquals("Test 1", true, oLookups.CustomsProcedureCodes.ContainsCode("Inst-1"));
				AssertEquals("Test 2", true, oLookups.CustomsProcedureCodes.ContainsCode("Inst-2"));
				AssertEquals("Test 3", true, oLookups.CustomsProcedureCodes.ContainsCode("Inst-3"));
			}

			);
		}

		public void TestNaladiNccaTariffList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var nccaTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, Constants.TariffTypes.NCCA);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Brazil, nccaTariffType.PK, "56049000", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Brazil, nccaTariffType.PK, "32500900", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.InvoiceLines.AddNew();

			var naladiNccaList = invoiceLine.Lookups.NaladiNccaTariffList;
			naladiNccaList.Load();
			AssertEquals(2, naladiNccaList.Count);
		}

		public void TestNaladiHsTariffList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, Constants.TariffTypes.NALADIHS);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Brazil, hsTariffType.PK, "35894400", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Brazil, hsTariffType.PK, "34884200", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.InvoiceLines.AddNew();

			var naladiHsList = invoiceLine.Lookups.NaladiHsTariffList;
			naladiHsList.Load();
			AssertEquals(2, naladiHsList.Count);
		}

		public void TestContainerTypeList()
		{
			JobComInvoiceLine parent = Factory.New<JobComInvoiceLine>();
			AssertNotNull(parent.Lookups.ContainerTypeList);
			Assert(parent.Lookups.ContainerTypeList is ICodeDescriptionPairList);
			AssertEquals(19, parent.Lookups.ContainerTypeList.Count);
		}

		public void TestCapacityUnitList()
		{
			JobComInvoiceLine parent = Factory.New<JobComInvoiceLine>();
			AssertNotNull(parent.Lookups.CapacityUnitList);
			Assert(parent.Lookups.CapacityUnitList is ICodeDescriptionPairList);
			AssertEquals(2, parent.Lookups.CapacityUnitList.Count);
		}

		public void TestCurrencies()
		{
			JobComInvoiceLine parent = Factory.New<JobComInvoiceLine>();
			AssertNotNull(parent.Lookups.Currencies);
			Assert(parent.Lookups.Currencies is RefCurrencyCollection);
		}

		public void TestCertificateTypeList()
		{
			var parent = Factory.New<JobComInvoiceLine>();
			var certificateTypeList = parent.Lookups.CertificateTypeList;
			AssertEquals(2, certificateTypeList.Count);
			AssertEquals("CCPTC, CCROM", certificateTypeList.CodesAsString);
		}

		public void TestDutyTaxRegimeList()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTaxRegimeList(Factory, BRJobMessageTypeList.Codes.ImportSiscomex);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var testCollection = invoiceLine.Lookups.DutyTaxRegimeList;

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
		}

		public void TestDutyLegalBaseList()
		{
			var taxRegimeCode = "1";
			ReferenceTestDataHelper.CreateReferenceDataForTaxRegimeList(Factory, BRJobMessageTypeList.Codes.ImportSiscomex);
			ReferenceTestDataHelper.CreateReferenceDataForDutyLegalBaseList(Factory, taxRegimeCode);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			invoiceLine.DutyTaxRegime = taxRegimeCode;

			var testCollection = invoiceLine.Lookups.DutyLegalBaseList;
			AssertEquals("1 Procedure for Brazil ISW - 01", 1, testCollection.Count);
			AssertEquals("First Procedure code should be 1", "01", testCollection[0].Code);
			AssertEquals("First Procedure description should be LIVROS, JORNAIS E PERIODICOS - CF/88, ART.150,VI,D - LEI 8032/90, ART.2,II,A - LEI 8402/92, ART 1,IV", "LIVROS, JORNAIS E PERIODICOS - CF/88, ART.150,VI,D - LEI 8032/90, ART.2,II,A - LEI 8402/92, ART 1,IV", testCollection[0].Description);
		}

		public void TestPisCofinsLegalBaseList()
		{
			var taxRegimeCode = "1";
			ReferenceTestDataHelper.CreateReferenceDataForTaxRegimeList(Factory, BRJobMessageTypeList.Codes.ImportSiscomex);
			ReferenceTestDataHelper.CreateReferenceDataForPISLegalBaseList(Factory, taxRegimeCode);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			invoiceLine.PisCofinsTaxRegime = taxRegimeCode;

			var testCollection = invoiceLine.Lookups.PisCofinsLegalBaseList;
			AssertEquals("2 Procedure for Brazil IMP - 01", 2, testCollection.Count);
			AssertEquals("First Procedure code should be 1", "01", testCollection[0].Code);
			AssertEquals("First Procedure description should be PIS LIVROS, JORNAIS E PERIODICOS - CF/88, ART.150,VI,D - LEI 8032/90, ART.2,II,A - LEI 8402/92, ART 1,IV", "PIS LIVROS, JORNAIS E PERIODICOS - CF/88, ART.150,VI,D - LEI 8032/90, ART.2,II,A - LEI 8402/92, ART 1,IV", testCollection[0].Description);
			AssertEquals("Second Procedure code should be 2", "02", testCollection[1].Code);
			AssertEquals("Second Procedure description should be PIS PAPEL DEST.A IMPRESSAO D / LIVROS, JORNAIS E PERIODICOS - CF / 88, ART.150, VI, D - L. 8032 / 90, ART.2, II, A - L. 8402 / 92, ART.1, IV", "PIS PAPEL DEST. A IMPRESSAO D/LIVROS, JORNAIS E PERIODICOS - CF/88,ART.150,VI,D - L. 8032/90,ART.2,II,A - L. 8402/92,ART.1,IV", testCollection[1].Description);
		}

		public void TestPisCofinsTaxRegimeList()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTaxRegimeList(Factory, BRJobMessageTypeList.Codes.ImportSiscomex);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var testCollection = invoiceLine.Lookups.PisCofinsTaxRegimeList;

			CombineAssertions(() =>
			{
				AssertEquals("2 Procedure for Brazil ISW - 01", 2, testCollection.Count);
				AssertEquals("First Procedure code should be 1", "1", testCollection[0].Code);
				AssertEquals("First Procedure description should be RECOLHIMENTO INTEGRAL", "RECOLHIMENTO INTEGRAL", testCollection[0].Description);
				AssertEquals("Second Procedure code should be 2", "2", testCollection[1].Code);
				AssertEquals("Second Procedure description should be IMUNIDADE", "IMUNIDADE", testCollection[1].Description);
			});
		}

		public void TestRatePreferencesList()
		{
			ReferenceTestDataHelper.CreatePreferenceViewForPrimaryPreferenceList(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var testCollection = invoiceLine.Lookups.RatePreferencesList;
			var expected = new[] { Constants.RatePreferenceType.Normal, Constants.RatePreferenceType.ExTariff, Constants.RatePreferenceType.FreeTradeAgreement, Constants.RatePreferenceType.ReducedRate, Constants.RatePreferenceType.ReductionMargin };

			AssertContainsExactElementsInAnyOrder(expected, testCollection.GetAllCodes());
		}

		public void TestProcedures()
		{
			ReferenceTestDataHelper.CreateRefCusProcedureForExport(Factory);

			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var testCollection = invoiceLine.Lookups.Procedures;
			AssertEquals("Collection must be empty", 0, testCollection.Count);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			testCollection = invoiceLine.Lookups.Procedures;
			AssertEquals("Collection must be empty", 2, testCollection.Count);
		}

		internal static void CreatePreferenceViewForPrimaryPreferenceList(JobDeclaration declaration)
		{
			var helper = new UniversalReferenceTestDataHelper(declaration.Factory);
			helper.CreatePreferenceView(Constants.RatePreferenceType.Normal, "General Rate", Core.Constants.CountryCodes.Brazil);
			helper.CreatePreferenceView(Constants.RatePreferenceType.ExTariff, "Ex Tariff", Core.Constants.CountryCodes.Brazil);
			helper.CreatePreferenceView(Constants.RatePreferenceType.FreeTradeAgreement, "Free Trade Agreeent Rate", Core.Constants.CountryCodes.Brazil);
			helper.CreatePreferenceView(Constants.RatePreferenceType.ReducedRate, "Reduced Rate", Core.Constants.CountryCodes.Brazil);
			helper.CreatePreferenceView(Constants.RatePreferenceType.ReductionMargin, "Reduction (Margin)", Core.Constants.CountryCodes.Brazil);
			declaration.Factory.Save();
		}

		public void TestICMSTaxRegimeList()
		{
			ReferenceTestDataHelper.CreateReferenceDataForICMSTaxRegimeList(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var testCollection = invoiceLine.Lookups.ICMSTaxRegimeList;

			CombineAssertions(() =>
			{
				AssertEquals("ICMSTaxRegimeList count shoud be", 9, testCollection.Count);
				AssertEquals("First code should be", "1", testCollection[0].Code);
				AssertEquals("First description should be", "Full Collection", testCollection[0].Description);
			});
		}

		public void TestICMSLegalBaseList()
		{
			ReferenceTestDataHelper.CreateReferenceDataForICMSLegalBaseList(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var testCollection = invoiceLine.Lookups.ICMSLegalBaseList;

			CombineAssertions(() =>
			{
				AssertEquals("ICMSTaxRegimeList count shoud be", 2, testCollection.Count);
				AssertEquals("First code should", "01", testCollection[0].Code);
				AssertEquals("First description should be", "ICMS Legal Base 01", testCollection[0].Description);
			});
		}

		public void TestImportLicenseFeeTypeList()
		{
			ReferenceTestDataHelper.CreateReferenceDataForILFFeeTypeList(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var testCollection = invoiceLine.Lookups.ImportLicenseFeeTypeList;

			CombineAssertions(() =>
			{
				AssertEquals("ImportLicenseFeeTypeList count shoud be", 2, testCollection.Count);
				AssertEquals("F1D5, F1ND", testCollection.CodesAsString);
			});
		}

		public void TestTariffAgreementList()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var testCollection = invoiceLine.Lookups.TariffAgreementList;

			CombineAssertions(() =>
			{
				AssertSame("The list cached", testCollection, BRRefCusCodeListTypes.GetTariffAgreementCodeList(Factory, invoiceLine.EffectiveAssessmentDate));
				AssertEquals("The count should be", 4, testCollection.Count);
				AssertEquals("The elements should be", "AR99, ASGPC, CO99, MX99", testCollection.CodesAsString);
			});
		}

		public void TestFMMBenefitList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var testCollection = invoiceLine.Lookups.FMMBenefitList;

			CombineAssertions(() =>
			{
				AssertEquals("FMMBenefitList count shoud be", 1, testCollection.Count);
				AssertEquals("First code should", "E", testCollection[0].Code);
				AssertEquals("First description should be", "Exemption", testCollection[0].Description);
			});
		}

		[TestDate(2023, 04, 17)]
		public void TestDuimpLegalBaseList_FromZZData()
		{
			ReferenceTestDataHelper.CreateTTRefCusProfileAndQuestions(Factory);
			ReferenceTestDataHelper.CreateDuimpLegalBaseCodes(Factory);

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				Factory.Save();
				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				AssertEquals(0, invoiceLine.Lookups.DuimpLegalBaseList.Count);
				invoiceLine.JI_Tariff = "12345678";
				AssertEquals(0, invoiceLine.Lookups.DuimpLegalBaseList.Count);

				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
				var legalBaseList = invoiceLine.Lookups.DuimpLegalBaseList;
				AssertContainsExactElementsInExactOrder(new[] { "P02", "P04" }, legalBaseList.GetAllCodesZString());
				AssertSame(legalBaseList, invoiceLine.Lookups.DuimpLegalBaseList);

				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Angola;
				legalBaseList = invoiceLine.Lookups.DuimpLegalBaseList;
				AssertContainsExactElementsInExactOrder(new[] { "P02", "P04" }, legalBaseList.GetAllCodesZString());
				AssertSame(legalBaseList, invoiceLine.Lookups.DuimpLegalBaseList);

				invoiceLine.JI_Tariff = "87654321";
				legalBaseList = invoiceLine.Lookups.DuimpLegalBaseList;
				AssertContainsExactElementsInExactOrder(new[] { "P03" }, legalBaseList.GetAllCodesZString());
				AssertSame(legalBaseList, invoiceLine.Lookups.DuimpLegalBaseList);
			}
		}

		public void TestDuimpLegalBaseList_FromMessage()
		{
			var date = new ZDateTime(2023, 04, 17);

			var message = Factory.New<BREDIMessage>();
			message.EM_ApplicationReference = "01010101|CN|20230417";
			message.EM_MessageNum = "1";
			message.EM_MessageType = MessageTypeList.Codes.RTT;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource("ResponseMessageOTA.json");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ValuationDate = date.Date;
			message.EM_LinkedObject = declaration;
			Factory.Save();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "01010101";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var legalBaseList = invoiceLine.Lookups.DuimpLegalBaseList;
			AssertContainsExactElementsInExactOrder(new[] { "0006", "0007", "0008", "0009" }, legalBaseList.GetAllCodesZString());
			AssertEquals("Code 0006 Description", "EX-TARIFÁRIOS TEMPORÁRIOS DE II", legalBaseList.GetDescriptionFromCode("0006"));
			AssertEquals("Code 0007 Description", "SGPC - SISTEMA GLOBAL DE PREFERÊNCIAS COMERCIAIS", legalBaseList.GetDescriptionFromCode("0007"));
			AssertEquals("Code 0008 Description", "IPI Opcional Tributo", legalBaseList.GetDescriptionFromCode("0008"));
			AssertEquals("Code 0009 Description", "AAP.AG N° 2 -  SEMENTES MERCOSUL X BOLIVIA,CHILE, CUBA, EQUADOR E PERU", legalBaseList.GetDescriptionFromCode("0009"));
			AssertSame(legalBaseList, invoiceLine.Lookups.DuimpLegalBaseList);

			invoiceLine.JI_Tariff = "01010101";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			legalBaseList = invoiceLine.Lookups.DuimpLegalBaseList;
			AssertEquals("Should not contain any code", 0, legalBaseList.Count);

			invoiceLine.JI_Tariff = "02020202";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			AssertEquals("Should not contain any code", 0, legalBaseList.Count);

			declaration.JE_ValuationDate = date.Date.AddDays(1);
			AssertEquals("Should not contain any code", 0, legalBaseList.Count);
		}

		public void TestComplementaryNoteList()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var refCusCodeList = invoiceLine.Lookups.ComplementaryNoteList;
			AssertType<CodeDescriptionPairList>(refCusCodeList);
		}

		public void TestRefCusCodeListMATMPList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRCustomsReasonTemporaryAdmissionCode, "Customs Reason Temporary Admission");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRCustomsReasonTemporaryAdmissionCode, "60", "BENS 1", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRCustomsReasonTemporaryAdmissionCode, "62", "BENS 2", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRCustomsReasonTemporaryAdmissionCode, "70", "BENS 3", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRCustomsReasonTemporaryAdmissionCode, "71", "BENS 4", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var refCusCodeList = invoiceLine.Lookups.RefCusCodeListMATMPList;

			AssertContainsExactElementsInAnyOrder(new string[] { "60", "62", "70", "71" }, refCusCodeList.GetAllCodes());
		}

		public void TestGoodsApplicationTypeList()
		{
			var declartion = Factory.New<JobDeclaration>();
			declartion.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declartion.Invoices.AddNew().InvoiceLines.AddNew();
			var list = invoiceLine.Lookups.GoodsApplicationTypeList;
			CombineAssertions(() =>
			{
				AssertSame(Factory.GetCachedValue<GoodsApplicationTypeList>(), list);
				AssertEquals("1, 2", list.CodesAsString);
			});

			declartion.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			list = invoiceLine.Lookups.GoodsApplicationTypeList;
			CombineAssertions(() =>
			{
				AssertType<ImportGoodsApplicationTypeList>(list);
				AssertEquals("1, 2, 3, 4, 5", list.CodesAsString);
			});
		}

		public void TestGoodsConditionTypeList()
		{
			var declartion = Factory.New<JobDeclaration>();
			declartion.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declartion.Invoices.AddNew().InvoiceLines.AddNew();
			var list = invoiceLine.Lookups.GoodsConditionTypeList;
			CombineAssertions(() =>
			{
				AssertSame(Factory.GetCachedValue<GoodsConditionTypeList>(), list);
				AssertEquals("1, 2", list.CodesAsString);
			});

			declartion.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			list = invoiceLine.Lookups.GoodsConditionTypeList;
			CombineAssertions(() =>
			{
				AssertSame(Factory.GetCachedValue<ImportGoodsConditionTypeList>(), list);
				AssertEquals("1, 2", list.CodesAsString);
				AssertEquals("New", list.GetDescriptionFromCode("1"));
				AssertEquals("Used", list.GetDescriptionFromCode("2"));
			});
		}

		public void TestICMSFormulaList()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var list = invoiceLine.Lookups.ICMSFormulaList;
			AssertSame(Factory.GetCachedValue<ICMSFormulaList>(), list);
			AssertEquals("BC, BCR", list.CodesAsString);
		}

		public void TestUsedMaterialRegimeList()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals(2, invoiceLine.Lookups.UsedMaterialRegimeList.Count);
		}

		public void TestGoodsConditionOperationTypeList()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals(11, invoiceLine.Lookups.GoodsConditionOperationTypeList.Count);
		}

		public void TestManufacturerIndicatorList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var list = invoiceLine.Lookups.ManufacturerIndicatorList;
			AssertEquals(3, list.Count);
			AssertEquals("1, 2, 3", list.CodesAsString);
			AssertSame(invoiceLine.Lookups.ManufacturerIndicatorList, list);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertSame(invoiceLine.Lookups.ManufacturerIndicatorList, list);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals(3, list.Count);
			AssertEquals("1, 2, 3", list.CodesAsString);
			AssertSame(invoiceLine.Lookups.ManufacturerIndicatorList, list);
		}

		public void TestSupplierList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var pivotFilterObj = invoiceLine.Lookups.SupplierList.FilterBusinessObjectDefaults;
				AssertEquals("Filters count should be", 2, pivotFilterObj.Count);

				Assert("SupplierList has not 'Is Foreign Operator' filter", !pivotFilterObj.Cast<FilterBusinessObjectDefault>().ToList().Any(f => f.FilterName == "Is Foreign Operator"));
			}

			Factory.ClearCachedValue<ConsignorCollection>("BR|JobComInvoiceLineLookups|SupplierList_True");

			using (BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var pivotFilterObj = invoiceLine.Lookups.SupplierList.FilterBusinessObjectDefaults;
				AssertEquals("Filters count should be", 3, pivotFilterObj.Count);

				var foreignOperatorFilter = pivotFilterObj.Cast<FilterBusinessObjectDefault>().ToList().First(f => f.FilterName == "Is Foreign Operator");
				AssertNotNull("SupplierList has 'Is Foreign Operator' filter", foreignOperatorFilter);
				Assert("'Is Foreign Operator' filter is removable", foreignOperatorFilter.IsRemovable);
				AssertEquals("'Is Foreign Operator' filter value is YES", OrgConstants.FilterControl.IsForeignOperator.Code.Yes, foreignOperatorFilter.Value);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				pivotFilterObj = invoiceLine.Lookups.SupplierList.FilterBusinessObjectDefaults;

				AssertEquals("Filters count should be", 2, pivotFilterObj.Count);
				Assert("SupplierList has not 'Is Foreign Operator' filter", !pivotFilterObj.Cast<FilterBusinessObjectDefault>().Any(f => f.FilterName == "Is Foreign Operator"));
			}
		}
	}
}
