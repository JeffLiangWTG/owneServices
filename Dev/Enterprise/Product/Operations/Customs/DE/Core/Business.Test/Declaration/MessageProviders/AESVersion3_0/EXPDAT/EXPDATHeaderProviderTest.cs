using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPDATHeaderProvider))]
	sealed class EXPDATHeaderProviderTest : AESHeaderProviderAbstractTest<EXPDATHeaderProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("Necessary arguments exist", () => new EXPDATHeaderProvider(action));

				declaration = Factory.New<JobDeclaration>();
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				action = new ExportEntryMessageSendingAction(entryHeader);
				AssertExceptionThrown<ArgumentException>("EntryInstruction missing", () => new EXPDATHeaderProvider(action));
			});
		}

		public void TestDeclarationType()
		{
			declaration.JE_EntryStyle = "EX";
			AssertEquals("EX", Provider.DeclarationType);
		}

		public void TestExportDeclarationType()
		{
			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._10;
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000000;
			AssertEquals("10000000", Provider.ExportDeclarationType);
		}

		public void TestPartyConstellation()
		{
			entryInstruction.ZG_PartyConstellation = "0000";
			AssertEquals("0000", Provider.PartyConstellation);
		}

		public void TestDecisiveDate()
		{
			entryInstruction.CEI_DateForDuty = new ZDate(2020, 1, 1);
			AssertEquals(ZDate.Empty, Provider.DecisiveDate);
		}

		public void TestDecisiveDate_SubStyle1stDigitIs1()
		{
			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._10;
			entryInstruction.CEI_DateForDuty = new ZDate(2020, 1, 1);
			AssertEquals(new ZDate(2020, 1, 1), Provider.DecisiveDate);
		}

		public void TestDecisiveDate_SubStyleIs20AndStyleIs000000()
		{
			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._20;
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000000;
			entryInstruction.CEI_DateForDuty = new ZDate(2020, 1, 1);
			AssertEquals(new ZDate(2020, 1, 1), Provider.DecisiveDate);
		}

		public void TestExitDate()
		{
			entryInstruction.ZG_ExitDate = new ZDate(2020, 12, 12);
			AssertEquals(ZDate.Empty, Provider.ExitDate);
		}

		public void TestExitDate_SubStyleIs10AndStyle2ndDigitIs0()
		{
			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._10;
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._201300;
			entryInstruction.ZG_ExitDate = new ZDate(2020, 12, 12);
			AssertEquals(new ZDate(2020, 12, 12), Provider.ExitDate);
		}

		public void TestExitDate_SubStyleIs11AndStyleIs000000()
		{
			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._11;
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000000;
			entryInstruction.ZG_ExitDate = new ZDate(2020, 12, 12);
			AssertEquals(new ZDate(2020, 12, 12), Provider.ExitDate);
		}

		public void TestPresentationStartDateAndTimeUtc()
		{
			declaration.ZG_PresentationStartDate = new ZDateTime(2021, 7, 22, 10, 04, 30, 221);
			AssertEquals(default(DateTime), Provider.PresentationStartDateAndTimeUtc);
		}

		public void TestPresentationStartDateAndTimeUtc_Style4thDigitIs2()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000200;
			declaration.ZG_PresentationStartDate = new ZDateTime(2021, 7, 22, 12, 04, 30, 221);
			CombineAssertions(() =>
			{
				AssertEquals("No millisecond", 0, Provider.PresentationStartDateAndTimeUtc.Millisecond);
				AssertEquals("Format", "2021-07-22T10:04:00", Provider.PresentationStartDateAndTimeUtc.ToString(@"yyyy'-'MM'-'dd'T'hh':'mm':'ss", CultureInfo.InvariantCulture));
			});
		}

		public void TestPresentationStartDateAndTimeUtc_Style4thDigitIs4()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000400;
			declaration.ZG_PresentationStartDate = new ZDateTime(2021, 7, 22, 10, 04, 30, 221);
			AssertNotEquals(default(DateTime), Provider.PresentationStartDateAndTimeUtc);
		}

		public void TestLoadingEndDateAndTimeUtc()
		{
			declaration.ZG_PresentationEndDate = new ZDateTime(2021, 7, 23, 10, 04, 30, 221);
			AssertEquals(default(DateTime), Provider.LoadingEndDateAndTimeUtc);
		}

		public void TestLoadingEndDateAndTimeUtc_Style4thDigitIs2()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000200;
			declaration.ZG_PresentationEndDate = new ZDateTime(2021, 7, 23, 12, 04, 30, 221);
			CombineAssertions(() =>
			{
				AssertEquals("No millisecond", 0, Provider.LoadingEndDateAndTimeUtc.Millisecond);
				AssertEquals("Format", "2021-07-23T10:04:00", Provider.LoadingEndDateAndTimeUtc.ToString(@"yyyy'-'MM'-'dd'T'hh':'mm':'ss", CultureInfo.InvariantCulture));
			});
		}

		public void TestSecurity()
		{
			action.SecurityType = ExportSecurityTypeList.Codes.EXS;
			AssertEquals("2", Provider.Security);
		}

		public void TestSpecificCircumstanceIndicator()
		{
			declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicatorForUCCList.Codes.A20;
			AssertEquals(SpecificCircumstanceIndicatorForUCCList.Codes.A20, Provider.SpecificCircumstanceIndicator);
		}

		public void TestAuthorisations_000100_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._000100, ExportDeclarationTypeTimeList.Codes._00, false);
		public void TestAuthorisations_000110_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._000110, ExportDeclarationTypeTimeList.Codes._00, false);
		public void TestAuthorisations_000200_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._000200, ExportDeclarationTypeTimeList.Codes._00, false);
		public void TestAuthorisations_000210_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._000210, ExportDeclarationTypeTimeList.Codes._00, false);
		public void TestAuthorisations_000400_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._000400, ExportDeclarationTypeTimeList.Codes._00, true);
		public void TestAuthorisations_000410_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._000410, ExportDeclarationTypeTimeList.Codes._00, true);
		public void TestAuthorisations_000901_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._000901, ExportDeclarationTypeTimeList.Codes._00, false);
		public void TestAuthorisations_000902_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._000902, ExportDeclarationTypeTimeList.Codes._00, false);
		public void TestAuthorisations_001300_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._001300, ExportDeclarationTypeTimeList.Codes._00, true);
		public void TestAuthorisations_001310_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._001310, ExportDeclarationTypeTimeList.Codes._00, true);
		public void TestAuthorisations_001410_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._001410, ExportDeclarationTypeTimeList.Codes._00, true);
		public void TestAuthorisations_110100_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._110100, ExportDeclarationTypeTimeList.Codes._00, true);
		public void TestAuthorisations_110110_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._110110, ExportDeclarationTypeTimeList.Codes._00, true);
		public void TestAuthorisations_110200_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._110200, ExportDeclarationTypeTimeList.Codes._00, true);
		public void TestAuthorisations_110210_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._110210, ExportDeclarationTypeTimeList.Codes._00, true);
		public void TestAuthorisations_110400_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._110400, ExportDeclarationTypeTimeList.Codes._00, true);
		public void TestAuthorisations_110410_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._110410, ExportDeclarationTypeTimeList.Codes._00, true);
		public void TestAuthorisations_111300_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._111300, ExportDeclarationTypeTimeList.Codes._00, true);
		public void TestAuthorisations_111310_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._111310, ExportDeclarationTypeTimeList.Codes._00, true);
		public void TestAuthorisations_111410_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._111410, ExportDeclarationTypeTimeList.Codes._00, true);
		public void TestAuthorisations_120000_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._120000, ExportDeclarationTypeTimeList.Codes._00, false);
		public void TestAuthorisations_120100_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._120100, ExportDeclarationTypeTimeList.Codes._00, false);
		public void TestAuthorisations_120110_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._120110, ExportDeclarationTypeTimeList.Codes._00, false);
		public void TestAuthorisations_120200_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._120200, ExportDeclarationTypeTimeList.Codes._00, false);
		public void TestAuthorisations_120210_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._120210, ExportDeclarationTypeTimeList.Codes._00, false);
		public void TestAuthorisations_200100_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._200100, ExportDeclarationTypeTimeList.Codes._00, false);
		public void TestAuthorisations_200110_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._200110, ExportDeclarationTypeTimeList.Codes._00, false);
		public void TestAuthorisations_200200_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._200200, ExportDeclarationTypeTimeList.Codes._00, false);
		public void TestAuthorisations_200210_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._200210, ExportDeclarationTypeTimeList.Codes._00, false);
		public void TestAuthorisations_200400_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._200400, ExportDeclarationTypeTimeList.Codes._00, true);
		public void TestAuthorisations_200410_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._200410, ExportDeclarationTypeTimeList.Codes._00, true);
		public void TestAuthorisations_201300_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._201300, ExportDeclarationTypeTimeList.Codes._00, true);
		public void TestAuthorisations_201310_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._201310, ExportDeclarationTypeTimeList.Codes._00, true);
		public void TestAuthorisations_201410_00() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._201410, ExportDeclarationTypeTimeList.Codes._00, true);

		public void TestAuthorisations_000000_10() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._000000, ExportDeclarationTypeTimeList.Codes._10, false);
		public void TestAuthorisations_110000_10() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._110000, ExportDeclarationTypeTimeList.Codes._10, true);
		public void TestAuthorisations_200000_10() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._200000, ExportDeclarationTypeTimeList.Codes._10, false);

		public void TestAuthorisations_000000_11() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._000000, ExportDeclarationTypeTimeList.Codes._11, false);
		public void TestAuthorisations_110000_11() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._110000, ExportDeclarationTypeTimeList.Codes._11, true);
		public void TestAuthorisations_120000_11() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._120000, ExportDeclarationTypeTimeList.Codes._11, false);
		public void TestAuthorisations_200000_11() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._200000, ExportDeclarationTypeTimeList.Codes._11, false);

		public void TestAuthorisations_000000_12() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._000000, ExportDeclarationTypeTimeList.Codes._12, false);
		public void TestAuthorisations_110000_12() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._110000, ExportDeclarationTypeTimeList.Codes._12, true);
		public void TestAuthorisations_120000_12() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._120000, ExportDeclarationTypeTimeList.Codes._12, false);
		public void TestAuthorisations_200000_12() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._200000, ExportDeclarationTypeTimeList.Codes._12, false);

		public void TestAuthorisations_000000_20() => AssertAuthorisations(ExportDeclarationTypeProcedureList.Codes._000000, ExportDeclarationTypeTimeList.Codes._20, true);

		void AssertAuthorisations(string style, ZString subStyle, bool shouldBeFilled)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, MapDirectionList.Codes.OUT, "EUNAU", true);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, "C019", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "C512", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.CentralizedClearance, "C513", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "C514", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Factory.Save();

			entryInstruction.CEI_Style = style;
			entryInstruction.CEI_SubStyle = subStyle;
			CreateAuthorizationUsage(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "SDE001");
			CreateAuthorizationUsage(CusAuthorizationHeaderTypeList.Codes.CentralizedClearance, "CCL001");
			CreateAuthorizationUsage(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "EIR001");
			CreateAuthorizationUsage(CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, "OPO001");

			CombineAssertions(() =>
			{
				if (shouldBeFilled)
				{
					AssertContainsExactElementsInAnyOrder(new[] { ("C512", "SDE001"), ("C513", "CCL001"), ("C514", "EIR001"), ("C019", "OPO001") }, Provider.Authorisations.Select(x => (x.Type, x.ReferenceNumber)));
				}
				else
				{
					AssertEquals(false, Provider.Authorisations.Any());
				}
			});
		}

		public void TestCustomsOfficeOfPresentation()
		{
			var office = declaration.CustomsOffices.AddNew();
			office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			office.CY_Data = "PRE_OFFICE";

			AssertNullOrEmpty(Provider.CustomsOfficeOfPresentation);
		}

		public void TestCustomsOfficeOfPresentation_Style4thDigitIs4()
		{
			var office = declaration.CustomsOffices.AddNew();
			office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			office.CY_Data = "PRE_OFFICE";

			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110410;
			AssertEquals("PRE_OFFICE", Provider.CustomsOfficeOfPresentation);
		}

		public void TestSupplementaryDeclarationCustomsOffice()
		{
			var office = declaration.CustomsOffices.AddNew();
			office.CY_Code = EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice;
			office.CY_Data = "EAM_OFFICE";

			AssertEquals(null, Provider.SupplementaryDeclarationCustomsOffice);
		}

		public void TestCustomsOfficeOfSupplement__Style5thDigitIs1()
		{
			var office = declaration.CustomsOffices.AddNew();
			office.CY_Code = EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice;
			office.CY_Data = "EAM_OFFICE";

			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000110;
			AssertEquals("EAM_OFFICE", Provider.SupplementaryDeclarationCustomsOffice);
		}

		public void TestIntendedExitCustomsOffice()
		{
			var office = declaration.CustomsOffices.AddNew();
			office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
			office.CY_Data = "EXT_OFFICE";
			AssertEquals(string.Empty, Provider.IntendedExitCustomsOffice);
		}

		public void TestIntendedExitCustomsOffice_SubStyle1stDigitIs0()
		{
			var office = declaration.CustomsOffices.AddNew();
			office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
			office.CY_Data = "EXT_OFFICE";

			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
			AssertEquals("EXT_OFFICE", Provider.IntendedExitCustomsOffice);
		}

		public void TestActualExitCustomsOffice()
		{
			var office = declaration.CustomsOffices.AddNew();
			office.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
			office.CY_Data = "AEC_OFFICE";

			AssertEquals(ZString.Empty, Provider.ActualExitCustomsOffice);
		}

		public void TestActualExitCustomsOffice_SubStyleIs10AndStyle2ndDigitIs0()
		{
			var office = declaration.CustomsOffices.AddNew();
			office.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
			office.CY_Data = "AEC_OFFICE";

			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._10;
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._201300;
			AssertEquals("AEC_OFFICE", Provider.ActualExitCustomsOffice);
		}

		public void TestActualExitCustomsOffice_SubStyleIs11Or12Or13AndStyleIs000000()
		{
			var office = declaration.CustomsOffices.AddNew();
			office.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
			office.CY_Data = "AEC_OFFICE";

			CombineAssertions(() =>
			{
				entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._11;
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000000;
				AssertEquals("CEI_SubStyle is 11", "AEC_OFFICE", Provider.ActualExitCustomsOffice);

				entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._12;
				AssertEquals("CEI_SubStyle is 12", "AEC_OFFICE", Provider.ActualExitCustomsOffice);

				entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._13;
				AssertEquals("CEI_SubStyle is 13", "AEC_OFFICE", Provider.ActualExitCustomsOffice);
			});
		}

		public void TestContractualPartner()
		{
			var orgAddress1 = CreateAddress1();
			declaration.ContractualPartnerDocAddress.E2_OA_Address = orgAddress1.PK;
			AssertNull(Provider.ContractualPartner);
		}

		public void TestContractualPartner_Constellation1stDigitIs1()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var orgAddress1 = CreateAddress1();
			entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._1010;
			declaration.ContractualPartnerDocAddress.E2_OA_Address = orgAddress1.PK;
			AssertPartyWithoutContactPerson(Provider.ContractualPartner
				, "GREOR1"
				, "EBS1"
				, "MAX MUSTERMANN"
				, "TESTSTRASSE 1"
				, "MAINZ"
				, "55126"
				, Core.Constants.CountryCodes.Germany);
		}

		public void TestExporter()
		{
			var orgAddress1 = CreateAddress1();
			declaration.ExporterDocAddress.E2_OA_Address = orgAddress1.PK;
			AssertNull(Provider.Exporter);
		}

		public void TestExporter_Constellation2ndDigitIs1()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var orgAddress1 = CreateAddress1();
			entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0100;
			declaration.ExporterDocAddress.E2_OA_Address = orgAddress1.PK;
			AssertPartyWithoutContactPerson(Provider.Exporter
				, "GREOR1"
				, "EBS1"
				, "MAX MUSTERMANN"
				, "TESTSTRASSE 1"
				, "MAINZ"
				, "55126"
				, Core.Constants.CountryCodes.Germany);
		}

		public void TestDeclarant()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var orgAddress1 = CreateAddress1();

			declaration.JE_OA_DeclarantAddress = orgAddress1.PK;
			AssertPartyWithoutContactPerson(Provider.Declarant
				, "GREOR1"
				, "EBS1"
				, "MAX MUSTERMANN"
				, "TESTSTRASSE 1"
				, "MAINZ"
				, "55126"
				, Core.Constants.CountryCodes.Germany);
		}

		public void TestDeclarant_Constellation3rdDigitIs0()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var orgAddress1 = CreateAddress1();

			using (Factory.SetTemporaryCurrentUser("Sachbearbeiter", "Bob Baumeister", "06131474747", "bob.baumeister@samplefreight.de"))
			{
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0100;
				declaration.JE_OA_DeclarantAddress = orgAddress1.PK;
				AssertPartyWithContactPerson(Provider.Declarant
					, "GREOR1"
					, "EBS1"
					, "MAX MUSTERMANN"
					, "TESTSTRASSE 1"
					, "MAINZ"
					, "55126"
					, Core.Constants.CountryCodes.Germany
					, "Sachbearbeiter"
					, "Bob Baumeister"
					, "06131474747"
					, ""
					, "bob.baumeister@samplefreight.de");
			}
		}

		public void TestSubContractor()
		{
			var orgAddress1 = CreateAddress1();
			declaration.JE_OA_SellerAddress = orgAddress1.PK;
			AssertNull(Provider.SubContractor);
		}

		public void TestSubContractor_Constellation4thDigitIs1()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var orgAddress1 = CreateAddress1();
			entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0101;
			declaration.JE_OA_SellerAddress = orgAddress1.PK;
			AssertPartyWithoutContactPerson(Provider.SubContractor
				, "GREOR1"
				, "EBS1"
				, "MAX MUSTERMANN"
				, "TESTSTRASSE 1"
				, "MAINZ"
				, "55126"
				, Core.Constants.CountryCodes.Germany);
		}

		public void TestExportCountry_CEI_Style4thDigitIsNot9_AllLinesHaveSameJI_RN_NKCountryOfExport()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
			declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.Germany;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Australia;

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Australia;

			AssertEquals("CEI_Style != '***9**', All lines have same JI_RN_NKCountryOfExport, ExportCountry mapped from JE_GoodsOrigin", Core.Constants.CountryCodes.Germany, Provider.ExportCountry);
		}

		public void TestExportCountry_CEI_Style4thDigitIsNot9_LinesHaveDifferentJI_RN_NKCountryOfExport()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
			declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.Germany;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Australia;

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Lithuania;

			AssertEquals("CEI_Style != '***9**', JI_RN_NKCountryOfExport are not same, ExportCountry mapped from JE_GoodsOrigin", Core.Constants.CountryCodes.Germany, Provider.ExportCountry);
		}

		public void TestExportCountry_CEI_Style4thDigitIs9_LinesHaveDifferentJI_RN_NKCountryOfExport()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000902;
			declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.Germany;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Australia;

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Germany;

			AssertEquals("CEI_Style == '***9**', JI_RN_NKCountryOfExport are not same, ExportCountry NOT mapped", string.Empty, Provider.ExportCountry);
		}

		public void TestExportCountry_CEI_Style4thDigitIs9_AllLinesHaveSameJI_RN_NKCountryOfExport()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000902;
			declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.Austria;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Germany;

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Germany;

			AssertEquals("CEI_Style == '***9**', JI_RN_NKCountryOfExport are same, ExportCountry mapped from JE_GoodsOrigint", Core.Constants.CountryCodes.Austria, Provider.ExportCountry);
		}

		public void TestExportCountry_CEI_Style4thDigitIs9_AllLinesHaveSameJI_RN_NKCountryOfExport_JE_GoodsOriginIsEmpty()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000902;
			declaration.JE_GoodsOrigin = ZString.Empty;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Germany;

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Germany;

			AssertEquals("CEI_Style == '***9**', JI_RN_NKCountryOfExport are same, ExportCountry mapped from JE_GoodsOrigint", Core.Constants.CountryCodes.Germany, Provider.ExportCountry);
		}

		public void TestDestinationCountry()
		{
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Germany;
			AssertEquals(Core.Constants.CountryCodes.Germany, Provider.DestinationCountry);
		}

		public void TestDeclarationProcedure()
		{
			entryInstruction.CEI_Style = "AM";
			AssertEquals("AM", Provider.DeclarationProcedure);
		}

		public void TestDeclarationVariant()
		{
			entryInstruction.CEI_SubStyle = "a";
			AssertEquals("a", Provider.DeclarationVariant);
		}

		public void TestAdditionalSupplyChainActors()
		{
			entryInstruction.CusSupplyChainActorReferences.AddNew();
			entryInstruction.CusSupplyChainActorReferences.AddNew();

			AssertEquals(2, Provider.AdditionalSupplyChainActors.Count);
		}

		public void TestOutwardProcessing()
		{
			entryInstruction.ReimportCountryCodes.AddNew(Core.Constants.CountryCodes.Germany);
			AssertNull(Provider.OutwardProcessing);
		}

		public void TestOutwardProcessing_Style1stDigitIs1()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110110;
			entryInstruction.ReimportCountryCodes.AddNew(Core.Constants.CountryCodes.Germany);
			var identificationMeansCode1 = entryInstruction.IdentificationMeanCodes.AddNew();
			identificationMeansCode1.CY_Code = IdentificationMeansList.Codes.S;
			identificationMeansCode1.CY_Data = "Description1";
			var product1 = entryInstruction.Products.AddNew();
			product1.CSI_Code = "123456";
			product1.CSI_Description = "Description1";

			CombineAssertions(() =>
			{
				AssertEquals("ReimportCountries", 1, Provider.OutwardProcessing.ReimportCountries.Count);
				AssertEquals("IdentificationMeans", 1, Provider.OutwardProcessing.IdentificationMeans.Count);
				AssertEquals("Products", 1, Provider.OutwardProcessing.Products.Count);
			});
		}

		public void TestPreviousDocuments()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.PreviousDocuments.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.PreviousDocuments.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;

			AssertEquals(2, Provider.PreviousDocuments.Count);
		}

		public void TestSupportingDocuments()
		{
			var invoice1 = declaration.Invoices.AddNew();
			var doc1 = invoice1.SupportingDocuments.AddNew();
			doc1.CSI_LineNo = 2;
			doc1.CSI_Code = "N382";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;

			var invoice2 = declaration.Invoices.AddNew();
			var doc2 = invoice2.SupportingDocuments.AddNew();
			doc2.CSI_LineNo = 1;
			doc2.CSI_Code = "N380";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;

			AssertSequencesEqual("sorted by CSI_LineNo", new string[] { "N380", "N382" }, Provider.SupportingDocuments.Select(d => d.Type));
		}

		public void TestAdditionalReferences()
		{
			var invoice1 = declaration.Invoices.AddNew();
			var additionInfo = invoice1.AdditionalInfos.AddNew();
			additionInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
			var additionInfo2 = invoice1.AdditionalInfos.AddNew();
			additionInfo2.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;

			var invoice2 = declaration.Invoices.AddNew();
			var additionInfo3 = invoice2.AdditionalInfos.AddNew();
			additionInfo3.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;

			AssertEquals(2, Provider.AdditionalReferences.Count);
		}

		public void TestAdditionalInformations()
		{
			var invoice1 = declaration.Invoices.AddNew();
			var additionInfo = invoice1.AdditionalInfos.AddNew();
			additionInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
			var additionInfo2 = invoice1.AdditionalInfos.AddNew();
			additionInfo2.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;

			var invoice2 = declaration.Invoices.AddNew();
			var additionInfo3 = invoice2.AdditionalInfos.AddNew();
			additionInfo3.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;

			AssertEquals(2, Provider.AdditionalInformations.Count);
		}

		public void TestCarrier()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var orgAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declaration.CarrierEUBorderDocAddress.E2_OA_Address = orgAddress.PK;

			AssertEquals("GREOR1", Provider.Carrier.EoriNumber);
			AssertEquals("EBS1", Provider.Carrier.EoriBranchSuffix);
		}

		public void TestConsignor_HeaderIsSupplierEqualConsignorsOnLine()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var orgAddress1 = CreateAddress1();
			declaration.SupplierDocumentaryAddress.E2_OA_Address = orgAddress1.PK;
			SetupInvoiceLinesWithEqualConsignors();
			AssertPartyWithoutContactPerson(Provider.Consignor
					, "GREOR1"
					, "EBS1"
					, "MAX MUSTERMANN"
					, "TESTSTRASSE 1"
					, "MAINZ"
					, "55126"
					, Core.Constants.CountryCodes.Germany);
		}

		public void TestConsignor_HeaderIsFromLineEqualConsignorsOnLineNoSupplier()
		{
			SetupInvoiceLinesWithEqualConsignors();
			AssertPartyWithoutContactPerson(Provider.Consignor
				, null
				, null
				, "MAX MUSTERMANN2"
				, "TESTSTRASSE 2"
				, "MAINZ"
				, "55122"
				, Core.Constants.CountryCodes.Germany);
		}

		public void TestConsignor_HeaderIsSupplierNoConsignorsOnLine()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var orgAddress1 = CreateAddress1();
			declaration.SupplierDocumentaryAddress.E2_OA_Address = orgAddress1.PK;
			SetupInvoiceLinesWithNoConsignors();
			AssertPartyWithoutContactPerson(Provider.Consignor
				, "GREOR1"
				, "EBS1"
				, "MAX MUSTERMANN"
				, "TESTSTRASSE 1"
				, "MAINZ"
				, "55126"
				, Core.Constants.CountryCodes.Germany);
		}

		public void TestConsignor_EmptyWhenOnLineDifferent()
		{
			var orgAddress1 = CreateAddress1();
			declaration.SupplierDocumentaryAddress.E2_OA_Address = orgAddress1.PK;
			SetupInvoiceLinesWithDifferentAndEmptyConsignors();
			AssertNull(Provider.Consignor);
		}

		public void TestConsignee_EqualConsigneesOnLine()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var orgAddress3 = CreateAddress3();
			declaration.ImporterDocumentaryAddress.E2_OA_Address = orgAddress3.PK;
			SetupInvoiceLinesWithEqualConsignees();
			AssertPartyWithoutContactPerson(Provider.Consignee
					, "GREOR1"
					, "EBS1"
					, "MAX MUSTERMANN"
					, "TESTSTRASSE 1"
					, "MAINZ"
					, "55126"
					, Core.Constants.CountryCodes.Germany);
		}

		public void TestConsignee_NoConsigneesOnLineAndEqualOnHeaders()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var orgAddress1 = CreateAddress1();
			SetupInvoiceLinesWithNoConsignees();
			declaration.Invoices.First().JZ_OA_ConsigneeAddress = orgAddress1.PK;
			declaration.Invoices.Last().JZ_OA_ConsigneeAddress = ZGuid.Empty;
			AssertPartyWithoutContactPerson(Provider.Consignee
					, "GREOR1"
					, "EBS1"
					, "MAX MUSTERMANN"
					, "TESTSTRASSE 1"
					, "MAINZ"
					, "55126"
					, Core.Constants.CountryCodes.Germany);
		}

		public void TestConsignee_SomeConsigneesOnLineAndTheSameOnHeaders()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var orgAddress1 = CreateAddress1();
			SetupInvoiceLinesWithNoConsignees();
			(declaration.Invoices.First().InvoiceLines.First() as JobComInvoiceLine).JI_OA_ConsigneeAddress = orgAddress1.PK;
			declaration.Invoices.Last().JZ_OA_ConsigneeAddress = orgAddress1.PK;
			AssertPartyWithoutContactPerson(Provider.Consignee
					, "GREOR1"
					, "EBS1"
					, "MAX MUSTERMANN"
					, "TESTSTRASSE 1"
					, "MAINZ"
					, "55126"
					, Core.Constants.CountryCodes.Germany);
		}

		public void TestConsignee_SomeConsigneesOnLineAndNotTheSameOnHeaders()
		{
			var orgAddress1 = CreateAddress1();
			var orgAddress2 = CreateAddress2();
			SetupInvoiceLinesWithNoConsignees();
			(declaration.Invoices.First().InvoiceLines.First() as JobComInvoiceLine).JI_OA_ConsigneeAddress = orgAddress1.PK;
			declaration.Invoices.Last().JZ_OA_ConsigneeAddress = orgAddress2.PK;
			AssertNull(Provider.Consignee);
		}

		public void TestConsignee_EmptyWhenConsigneesOnLineDifferent()
		{
			SetupInvoiceLinesWithDifferentAndEmptyConsignees();
			AssertNull(Provider.Consignee);
		}
		public void TestConsignee_EmptyWhenNoConsigneesOnLinesAndNoOnHeaders()
		{
			SetupInvoiceLinesWithNoConsignees();
			AssertNull(Provider.Consignee);
		}

		public void TestLocationOfGoodsSpecified()
		{
			entryInstruction.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			AssertEquals(true, Provider.LocationOfGoodsSpecified);
		}

		//Test LocationOfGoods Mapping
		public void TestLocationOfGoodsSpecified_SubStyleIs20()
		{
			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._20;
			AssertEquals(false, Provider.LocationOfGoodsSpecified);
		}

		public void TestLocationOfGoodsSpecified_QualifierOfIdentificationIsEmpty()
		{
			entryInstruction.GoodsLocation.CGL_Qualifier = "";
			AssertEquals(false, Provider.LocationOfGoodsSpecified);
		}

		public void TestTypeOfLocation()
		{
			var goodsLocation = entryInstruction.GoodsLocation;
			entryInstruction.GoodsLocation.CGL_Type = "A";
			AssertEquals("A", Provider.TypeOfLocation);
		}

		public void TestQualifierOfIdentification()
		{
			entryInstruction.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			AssertEquals(CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier, Provider.QualifierOfIdentification);
		}

		public void TestAuthorisationNumber()
		{
			CreateAuthorizationUsage(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "12345678");
			AssertEquals(string.Empty, Provider.AuthorisationNumber);
		}

		public void TestAuthorisationNumber_QualifierOfIdentificationIsY()
		{
			CreateAuthorizationUsage(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "12345678");
			entryInstruction.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			AssertEquals("12345678", Provider.AuthorisationNumber);
		}

		public void TestAuthorisationNumber_QualifierOfIdentificationIsY_ThrowsExceptionDuplicateSDE()
		{
			CreateAuthorizationUsage(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "12345678");
			CreateAuthorizationUsage(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DUPLICATE");
			entryInstruction.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			AssertExceptionThrown<InvalidOperationException>(() => _ = Provider.AuthorisationNumber);
		}

		public void TestAuthorisationNumber_QualifierOfIdentificationIsY_Style3rdDigitIs1()
		{
			CreateAuthorizationUsage(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "12345678");
			entryInstruction.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._111300;
			AssertEquals(string.Empty, Provider.AuthorisationNumber);
		}

		public void TestAuthorisationNumber_QualifierOfIdentificationIsY_Style4thDigitIs4()
		{
			CreateAuthorizationUsage(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "12345678");
			entryInstruction.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._001410;
			AssertEquals(string.Empty, Provider.AuthorisationNumber);
		}

		public void TestAdditionalIdentifier()
		{
			entryInstruction.GoodsLocation.LoadingPlace = "XX";
			AssertEquals(string.Empty, Provider.AdditionalIdentifier);
		}

		public void TestAdditionalIdentifier_QualifierOfIdentificationIsY()
		{
			var goodsLocation = entryInstruction.GoodsLocation;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			goodsLocation.CGL_AdditionalIdentifier = "XX";
			AssertEquals("XX", Provider.AdditionalIdentifier);
		}

		public void TestUNLocode_NoPickupAddress()
		{
			entryInstruction.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			AssertEquals(string.Empty, Provider.UNLocode);
		}

		public void TestUNLocode_QualifierOfIdentificationNotU()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_RL_NKRelatedPortCode = "DEHAM";
			declaration.SupplierPickupAddress.E2_OA_Address = orgAddress.PK;
			entryInstruction.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			AssertEquals(ZString.Empty, Provider.UNLocode);
		}

		public void TestUNLocode_QualifierOfIdentificationIsU()
		{
			var goodsLocation = entryInstruction.GoodsLocation;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			goodsLocation.CGL_AdditionalIdentifier = "DEHAM";
			AssertEquals("DEHAM", Provider.UNLocode);
		}

		public void TestGNSSSpecified()
		{
			AssertEquals(false, Provider.GNSSSpecified);
		}

		public void TestGNSSSpecified_QualifierOfIdentificationIsW()
		{
			entryInstruction.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			AssertEquals(true, Provider.GNSSSpecified);
		}

		public void TestGNSSLatitude_NoPickupAddress()
		{
			AssertEquals(0d, Provider.GNSSLatitude);
		}

		public void TestGNSSLatitude()
		{
			entryInstruction.GoodsLocation.Address.E2_GeoLocation = ZGeography.CreatePoint(0, 53.55109);
			AssertEquals(53.55109, Provider.GNSSLatitude);
		}

		public void TestGNSSLongitude_NoPickupAddress()
		{
			AssertEquals(0d, Provider.GNSSLongitude);
		}

		public void TestGNSSLongitude()
		{
			entryInstruction.GoodsLocation.Address.E2_GeoLocation = ZGeography.CreatePoint(9.99368, 0);
			AssertEquals(9.99368, Provider.GNSSLongitude);
		}

		public void TestLocationOfGoodsParty()
		{
			var orgAddress1 = CreateAddress1();
			declaration.SupplierPickupAddress.E2_OA_Address = orgAddress1.PK;

			AssertNull(Provider.LocationOfGoodsParty);
		}

		public void TestLocationOfGoodsParty_QualifierOfIdentificationIsZ()
		{
			var orgAddress1 = CreateAddress1();
			declaration.SupplierPickupAddress.E2_OA_Address = orgAddress1.PK;

			entryInstruction.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			AssertNotNull(Provider.LocationOfGoodsParty);
		}

		public void TestLocationOfGoodsContactPerson_WhenContactIsNull()
		{
			var orgAddress1 = CreateAddress1();
			entryInstruction.GoodsLocation.Address.E2_OA_Address = orgAddress1.PK;

			AssertNull("Precondition", declaration.SupplierPickupAddress.Contact);
			AssertNull(Provider.LocationOfGoodsContactPerson);
		}

		public void TestLocationOfGoodsContactPerson_WhenContactIsNotNull()
		{
			var orgAddress1 = CreateAddress1();
			declaration.SupplierPickupAddress.E2_OA_Address = orgAddress1.PK;
			declaration.SupplierPickupAddress.ContactPK = orgAddress1.Header.Contacts.Single().PK;

			AssertNotNull("Precondition", declaration.SupplierPickupAddress.Contact);
			AssertNotNull(Provider.LocationOfGoodsContactPerson);
		}

		public void TestLocationOfGoodsContactPerson_E2_AddressOverride()
		{
			entryInstruction.GoodsLocation.Address.E2_AddressOverride = true;
			entryInstruction.GoodsLocation.Address.E2_Contact = "VWG";
			entryInstruction.GoodsLocation.Address.E2_Phone = "12345";
			entryInstruction.GoodsLocation.Address.E2_Email = "123@abc.com";

			CombineAssertions(() =>
			{
				AssertEquals("VWG", Provider.LocationOfGoodsContactPerson.PersonName);
				AssertEquals("12345", Provider.LocationOfGoodsContactPerson.PhoneNumber);
				AssertEquals("123@abc.com", Provider.LocationOfGoodsContactPerson.MailAddress);
			});
		}

		public void TestLocationOfGoodsContactPersony_QualifierOfIdentificationIsV()
		{
			var orgAddress1 = CreateAddress1();
			entryInstruction.GoodsLocation.Address.E2_OA_Address = orgAddress1.PK;

			entryInstruction.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			AssertNull(Provider.LocationOfGoodsContactPerson);
		}

		public void TestTransportDocuments()
		{
			var invoice1 = declaration.Invoices.AddNew();
			var additionInfo = invoice1.AdditionalInfos.AddNew();
			additionInfo.CSI_SubType = AdditionalDocTypeList.Codes.TransportDocuments;
			var additionInfo2 = invoice1.AdditionalInfos.AddNew();
			additionInfo2.CSI_SubType = AdditionalDocTypeList.Codes.TransportDocuments;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;

			var invoice2 = declaration.Invoices.AddNew();
			var additionInfo3 = invoice2.AdditionalInfos.AddNew();
			additionInfo3.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;

			AssertEquals(2, Provider.TransportDocuments.Count);
		}

		public void TestTransportChargesPaymentMethod_AllSame()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.ZG_TransportChargesMethodOfPayment = "A";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.ZG_TransportChargesMethodOfPayment = "A";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			entryHeader.ResetInvoiceHeadersAndLines();
			entryLine.InvoiceLines.ReloadFromLocalCache();
			AssertEquals("Value", "A", Provider.TransportChargesPaymentMethod);
		}

		public void TestTransportChargesPaymentMethod_Different()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.ZG_TransportChargesMethodOfPayment = "A";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.ZG_TransportChargesMethodOfPayment = "B";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			entryHeader.ResetInvoiceHeadersAndLines();
			entryLine.InvoiceLines.ReloadFromLocalCache();
			AssertEquals(ZString.Empty, Provider.TransportChargesPaymentMethod);
		}

		public void TestIsContainerized()
		{
			CombineAssertions(() =>
			{
				foreach (var containerFlag in new ZString[] { Core.Constants.ContainerModes.FCL, Core.Constants.ContainerModes.ULD, Core.Constants.ContainerModes.Containerised })
				{
					declaration.JE_ContainerMode = containerFlag;
					AssertEquals($"IsContainerised = '{containerFlag}'", true, Provider.IsContainerized);
				}
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
				AssertEquals("Miscellaneous IsContainerised", false, Provider.IsContainerized);
			});
		}

		public void TestTransportEquipments()
		{
			PrepareTransportEquipment();
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals(2, Provider.TransportEquipments.Count);
		}

		public void TestTransportEquipments_NotPopulated()
		{
			PrepareTransportEquipment();
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(false, Provider.TransportEquipments.Any());
		}

		public void TestRegistrationNumber()
		{
			declaration.JE_MasterBill = "123456";
			CombineAssertions(() =>
			{
				AssertEquals("TransportMode != 'SEA' or 'AIR'", ZString.Empty, Provider.RegistrationNumber);
				declaration.JE_TransportMode = "SEA";
				AssertEquals("TransportMode == 'SEA'", "123456", Provider.RegistrationNumber);
				declaration.JE_TransportMode = "AIR";
				AssertEquals("TransportMode == 'AIR'", "123456", Provider.RegistrationNumber);
			});
		}

		public void TestAnnotation()
		{
			entryInstruction.AdditionalInformation = "TEST ANNOTATION";
			AssertEquals("TEST ANNOTATION", Provider.Annotation);
		}

		public void TestGoodsItemQuantity()
		{
			CombineAssertions(() =>
			{
				AssertEquals("1 line", 1, Provider.GoodsItemQuantity);
				var invoice1 = declaration.Invoices.AddNew();
				var line1 = invoice1.InvoiceLines.AddNew();
				line1.JI_CEI = entryInstruction.PK;
				line1.JI_CL = entryHeader.MergedLines.AddNew().PK;
				entryHeader.ResetInvoiceHeadersAndLines();
				entryLine.InvoiceLines.ReloadFromLocalCache();
				AssertEquals("2 lines", 2, Provider.GoodsItemQuantity);
			});
		}

		public void TestLocalClearanceOutwardProcessingIDNumber()
		{
			AssertEquals("Obsolete as of AES 3.0", ZString.Empty, Provider.LocalClearanceOutwardProcessingIDNumber);
		}

		public void TestOutwardProcessingIDNumber()
		{
			AssertEquals("Not implemented yet", ZString.Empty, Provider.OutwardProcessingIDNumber);
		}

		public void TestAccreditedExporterIDNumber()
		{
			AssertEquals(ZString.Empty, Provider.AccreditedExporterIDNumber);
		}

		public void TestGoodsLoadingPlace_Emit()
		{
			var orgAddress1 = CreateAddress1();
			entryInstruction.GoodsLocation.LoadingPlace = "BB00";
			orgAddress1.PrimaryOrgAddressAdditionalInfoDetail = "COMPLEMENT";
			declaration.SupplierPickupAddress.E2_OA_Address = orgAddress1.PK;
			AssertNotNull(Provider.GoodsLoadingPlace);
		}

		public void TestItineraryCountries_WithoutLinkedShipment_NoRoutingCountries()
		{
			declaration.JE_RL_NKOrigin = "DEWIB";
			declaration.JE_RL_NKFinalDestination = "CNPEK";

			var itinerary = Provider.ItineraryCountries.ToArray();
			CombineAssertions(() =>
			{
				//Ensure Origin and Destination
				AssertEquals("No routing countries: Country count = 2", 2, itinerary.Length);
				AssertEquals("No routing countries: 1. country", "DE", itinerary[0]);
				AssertEquals("No routing countries: 2. country", "CN", itinerary[1]);
			});
		}

		public void TestItineraryCountries_WithoutLinkedShipment_1RoutingCountryEqualToPreviousCountry()
		{
			declaration.JE_RL_NKOrigin = "DEWIB";
			declaration.JE_RL_NKFinalDestination = "CNPEK";

			var transport1 = declaration.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = "DEWIB";
			transport1.JW_RL_NKDiscPort = "DEFRA";

			var itinerary = Provider.ItineraryCountries.ToArray();
			CombineAssertions(() =>
			{
				//Ensure country will not be added if it's equal to previous country
				AssertEquals("1 routing country equal to previous country: Country count = 2", 2, itinerary.Length);
				AssertEquals("1 routing country equal to previous country: 1. country", "DE", itinerary[0]);
				AssertEquals("1 routing country equal to previous country: 2. country", "CN", itinerary[1]);
			});
		}

		public void TestItineraryCountries_WithoutLinkedShipment_2DifferentRoutingCountriesWithDifferentLegOrder()
		{
			declaration.JE_RL_NKOrigin = "DEWIB";
			declaration.JE_RL_NKFinalDestination = "CNPEK";

			var transport1 = declaration.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = "DEWIB";
			transport1.JW_RL_NKDiscPort = "DEFRA";

			var transport2 = declaration.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = "DEFRA";
			transport2.JW_RL_NKDiscPort = "NLAMS";

			var itinerary = Provider.ItineraryCountries.ToArray();
			CombineAssertions(() =>
			{
				//Ensure country will be added if it's different to previous country
				AssertEquals("2 different routing countries with different legOrder: Country count = 3", 3, itinerary.Length);
				AssertEquals("2 different routing countries with different legOrder: 1. country", "DE", itinerary[0]);
				AssertEquals("2 different routing countries with different legOrder: 2. country", "NL", itinerary[1]);
				AssertEquals("2 different routing countries with different legOrder: 3. country", "CN", itinerary[2]);
			});
		}

		public void TestItineraryCountries_WithoutLinkedShipment_MatchFinalDestination()
		{
			declaration.JE_RL_NKOrigin = "DEWIB";
			declaration.JE_RL_NKFinalDestination = "CNPEK";

			var transport1 = declaration.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = "DEWIB";
			transport1.JW_RL_NKDiscPort = "DEFRA";

			var transport2 = declaration.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = "DEFRA";
			transport2.JW_RL_NKDiscPort = "NLAMS";

			var transport4 = declaration.Transports.AddNew();
			transport4.JW_LegOrder = 3;
			transport4.JW_RL_NKLoadPort = "NLAMS";
			transport4.JW_RL_NKDiscPort = "CNDYG";

			var itinerary = Provider.ItineraryCountries.ToArray();
			CombineAssertions(() =>
			{
				//Ensure matches final destination country
				AssertEquals("Match Final Destination: Country count = 3", 3, itinerary.Length);
				AssertEquals("Match Final Destination: 1. country", "DE", itinerary[0]);
				AssertEquals("Match Final Destination: 2. country", "NL", itinerary[1]);
				AssertEquals("Match Final Destination: 3. country", "CN", itinerary[2]);
			});
		}

		public void TestItineraryCountries_WithLinkedShipment_NoRoutingCountries()
		{
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_RL_NKOrigin = "DEWIB";
			declaration.JE_RL_NKFinalDestination = "CNPEK";

			var itineraryCountries = Provider.ItineraryCountries;
			var itinerary = itineraryCountries.ToArray();
			CombineAssertions(() =>
			{
				//Ensure Origin and Destination
				AssertEquals("No routing countries: Country count = 2", 2, itinerary.Length);
				AssertEquals("No routing countries: 1. country", "DE", itinerary[0]);
				AssertEquals("No routing countries: 2. country", "CN", itinerary[1]);
			});
		}

		public void TestItineraryCountries_WithLinkedShipment_1RoutingCountryEqualToPreviousCountry()
		{
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_RL_NKOrigin = "DEWIB";
			declaration.JE_RL_NKFinalDestination = "CNPEK";

			var transport1 = declaration.Shipment.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = "DEWIB";
			transport1.JW_RL_NKDiscPort = "DEFRA";

			var itinerary = Provider.ItineraryCountries.ToArray();
			CombineAssertions(() =>
			{
				//Ensure country will not be added if it's equal to previous country
				AssertEquals("1 routing country equal to previous country: Country count = 2", 2, itinerary.Length);
				AssertEquals("1 routing country equal to previous country: 1. country", "DE", itinerary[0]);
				AssertEquals("1 routing country equal to previous country: 2. country", "CN", itinerary[1]);
			});
		}

		public void TestItineraryCountries_WithLinkedShipment_2DifferentRoutingCountriesWithDifferentLegOrder()
		{
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_RL_NKOrigin = "DEWIB";
			declaration.JE_RL_NKFinalDestination = "CNPEK";

			var transport1 = declaration.Shipment.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = "DEWIB";
			transport1.JW_RL_NKDiscPort = "DEFRA";

			var transport2 = declaration.Shipment.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = "DEFRA";
			transport2.JW_RL_NKDiscPort = "NLAMS";

			var itinerary = Provider.ItineraryCountries.ToArray();
			CombineAssertions(() =>
			{
				//Ensure country will be added if it's different to previous country
				AssertEquals("2 different routing countries with different legOrder: Country count = 3", 3, itinerary.Length);
				AssertEquals("2 different routing countries with different legOrder: 1. country", "DE", itinerary[0]);
				AssertEquals("2 different routing countries with different legOrder: 2. country", "NL", itinerary[1]);
				AssertEquals("2 different routing countries with different legOrder: 3. country", "CN", itinerary[2]);
			});
		}

		public void TestItineraryCountries_WithLinkedShipment_MatchFinalDestination()
		{
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_RL_NKOrigin = "DEWIB";
			declaration.JE_RL_NKFinalDestination = "CNPEK";
			var transport1 = declaration.Shipment.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = "DEWIB";
			transport1.JW_RL_NKDiscPort = "DEFRA";
			var transport2 = declaration.Shipment.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = "DEFRA";
			transport2.JW_RL_NKDiscPort = "NLAMS";
			var transport4 = declaration.Shipment.Transports.AddNew();
			transport4.JW_LegOrder = 3;
			transport4.JW_RL_NKLoadPort = "NLAMS";
			transport4.JW_RL_NKDiscPort = "CNDYG";

			var itinerary = Provider.ItineraryCountries.ToArray();

			CombineAssertions(() =>
			{
				//Ensure matches final destination country
				AssertEquals("Match Final Destination: Country count = 3", 3, itinerary.Length);
				AssertEquals("Match Final Destination: 1. country", "DE", itinerary[0]);
				AssertEquals("Match Final Destination: 2. country", "NL", itinerary[1]);
				AssertEquals("Match Final Destination: 3. country", "CN", itinerary[2]);
			});
		}

		public void TestItineraryCountries_WithLinkedShipment_CountryCountNotEffectedByDeclarationTransports()
		{
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_RL_NKOrigin = "DEWIB";
			declaration.JE_RL_NKFinalDestination = "CNPEK";

			var transport1 = declaration.Shipment.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = "DEWIB";
			transport1.JW_RL_NKDiscPort = "DEFRA";

			var transport2 = declaration.Shipment.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = "DEFRA";
			transport2.JW_RL_NKDiscPort = "NLAMS";

			var transport4 = declaration.Shipment.Transports.AddNew();
			transport4.JW_LegOrder = 3;
			transport4.JW_RL_NKLoadPort = "NLAMS";
			transport4.JW_RL_NKDiscPort = "CNDYG";

			var transportsInDeclaration = declaration.Transports.AddNew();
			transportsInDeclaration.JW_RL_NKDiscPort = "DEXXX";
			transportsInDeclaration.JW_RL_NKDiscPort = "DEXXX";

			AssertEquals("Country count not effected by Declaration.Transports", 3, Provider.ItineraryCountries.Count);
		}

		public void TestItineraryCountries_WithoutLinkedShipment_LastSegment()
		{
			declaration.JE_RL_NKOrigin = "DEWIB";
			declaration.JE_RL_NKFinalDestination = "CNPEK";
			declaration.JE_GoodsDestination = "QQ";

			var itinerary = Provider.ItineraryCountries.ToArray();
			CombineAssertions(() =>
			{
				//Ensure Origin and Destination
				AssertEquals("No routing countries: Country count = 3", 3, itinerary.Length);
				AssertEquals("No routing countries: 1. country", "DE", itinerary[0]);
				AssertEquals("No routing countries: 2. country", "CN", itinerary[1]);
				AssertEquals("No routing countries: 3. country", "QQ", itinerary[2]);
			});
		}

		public void TestItineraryCountries_WithLinkedShipment_LastSegment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_RL_NKOrigin = "DEWIB";
			declaration.JE_RL_NKFinalDestination = "CNPEK";
			declaration.JE_GoodsDestination = "QQ";

			var itineraryCountries = Provider.ItineraryCountries;
			var itinerary = itineraryCountries.ToArray();
			CombineAssertions(() =>
			{
				//Ensure Origin and Destination
				AssertEquals("No routing countries: Country count = 3", 3, itinerary.Length);
				AssertEquals("No routing countries: 1. country", "DE", itinerary[0]);
				AssertEquals("No routing countries: 2. country", "CN", itinerary[1]);
				AssertEquals("No routing countries: 2. country", "QQ", itinerary[2]);
			});
		}

		public void TestCurrency()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Invoice Lines", "EUR", Provider.Currency);
				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_RX_NKInvoice_Currency = "AUD";
				var invoiceLine1 = invoice1.InvoiceLines.AddNew();
				invoiceLine1.JI_CL = entryLine.PK;
				entryHeader.ResetInvoiceHeadersAndLines();
				entryLine.InvoiceLines.ReloadFromLocalCache();
				AssertEquals("Single Invoice", "AUD", Provider.Currency);
				var entryLine2 = entryHeader.MergedLines.AddNew();
				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_RX_NKInvoice_Currency = "EUR";
				var invoiceLine2 = invoice2.InvoiceLines.AddNew();
				invoiceLine2.JI_CL = entryLine.PK;
				entryHeader.ResetInvoiceHeadersAndLines();
				entryLine.InvoiceLines.ReloadFromLocalCache();
				AssertEquals("Different Currencies", "EUR", Provider.Currency);
			});
		}

		public void TestPresentationPackingLoading()
		{
			AssertNull("Not implemented yet", Provider.PresentationPackingLoading);
		}

		public void TestContractorEmptyOnPartyConstellationxx0x()
		{
			var orgAddress1 = CreateAddress1();
			entryInstruction.ZG_PartyConstellation = "0000";
			declaration.JE_OA_SellerAddress = orgAddress1.PK;
			AssertNull(Provider.Contractor);
		}

		public void TestContractor()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var orgAddress1 = CreateAddress1();
			entryInstruction.ZG_PartyConstellation = "0010";
			declaration.JE_OA_SellerAddress = orgAddress1.PK;
			AssertPartyWithContactPerson(Provider.Contractor
					, "GREOR1"
					, "EBS1"
					, "MAX MUSTERMANN"
					, "TESTSTRASSE 1"
					, "MAINZ"
					, "55126"
					, Core.Constants.CountryCodes.Germany
					, "CHEF"
					, "LUTZ LUSTIG"
					, "06131474747"
					, "06131474748"
					, "LUTZ.LUSTIG@EMAIL.DE");
		}

		public void TestOutwardProcessingOwnerEmptyOnPartyConstellationxxx0()
		{
			var orgAddress1 = CreateAddress1();
			entryInstruction.ZG_PartyConstellation = "0000";
			declaration.JE_OA_ManufacturerAddress = orgAddress1.PK;
			AssertNull(Provider.OutwardProcessingOwner);
		}

		public void TestOutwardProcessingOwner()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var orgAddress1 = CreateAddress1();
			entryInstruction.ZG_PartyConstellation = "0003";
			declaration.JE_OA_ManufacturerAddress = orgAddress1.PK;
			var outwardProcessingOwner = Provider.OutwardProcessingOwner;
			AssertPartyWithContactPerson(outwardProcessingOwner
					, "GREOR1"
					, "EBS1"
					, "MAX MUSTERMANN"
					, "TESTSTRASSE 1"
					, "MAINZ"
					, "55126"
					, Core.Constants.CountryCodes.Germany
					, "CHEF"
					, "LUTZ LUSTIG"
					, "06131474747"
					, "06131474748"
					, "LUTZ.LUSTIG@EMAIL.DE");
		}

		public void TestInternalCurrency()
		{
			AssertNullOrEmpty(Provider.InternalCurrency);
		}

		public void TestExchangeRate()
		{
			AssertEquals(0.0M, Provider.ExchangeRate);
		}

		public void TestDeferredPayment()
		{
			AssertNullOrEmpty(Provider.DeferredPayment);
		}

		public void TestWarehouseType()
		{
			AssertNullOrEmpty(Provider.WarehouseType);
		}

		public void TestWarehouseIdentifier()
		{
			AssertNullOrEmpty(Provider.WarehouseIdentifier);
		}

		public void TestDepartureTransportMeans()
		{
			AssertEquals(0, Provider.DepartureTransportMeans.Count);
		}

		public void TestLines()
		{
			SetupInvoiceLinesWithDifferentConsignees();
			AssertEquals(2, Provider.Lines.Count);
		}

		public void TestDeliveryTerms()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_IncoTerm = "1";
			invoice1.JZ_IncoTermPlace = "SYD";
			invoice1.ZG_AgreedPlaceCode = "AU";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			entryLine.CL_LineNumber = 1;

			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = "1";
			invoice2.JZ_IncoTermPlace = "SYD";
			invoice2.ZG_AgreedPlaceCode = "AU";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			entryLine2.CL_LineNumber = 2;

			var deliveryTerms = Provider.DeliveryTerms;
			CombineAssertions(() =>
			{
				AssertEquals("IncotermCode", "1", deliveryTerms.IncotermCode);
				AssertEquals("Location", "SYD", deliveryTerms.Location);
				AssertEquals("Country", "AU", deliveryTerms.Country);
			});
		}

		protected override IEnumerable<Expression<Func<EXPDATHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ContractualPartner;
			yield return x => x.Exporter;
			yield return x => x.Declarant;
			yield return x => x.Representative;
			yield return x => x.SubContractor;
			yield return x => x.Carrier;
			yield return x => x.Consignor;
			yield return x => x.Consignee;
			yield return x => x.LocationOfGoodsParty;
			yield return x => x.LocationOfGoodsContactPerson;
			yield return x => x.GoodsLoadingPlace;
			yield return x => x.PresentationPackingLoading;
			yield return x => x.Contractor;
			yield return x => x.OutwardProcessingOwner;
			yield return x => x.OutwardProcessing;
		}

		protected override void SetUp()
		{
			base.SetUp();

			entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;

			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_JE = declaration.PK;
			entryLine = entryHeader.MergedLines.AddNew();

			action = new ExportEntryMessageSendingAction(entryHeader);
		}
		CusEntryInstruction entryInstruction;
		CusEntryLine entryLine;
		ExportEntryMessageSendingAction action;

		protected override EXPDATHeaderProvider GetProvider() => new EXPDATHeaderProvider(action);

		new IEXPDATHeader Provider => base.Provider;

		OrgAddress CreateAddress1()
		{
			var orgAddress1 = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			orgAddress1.Header.OH_FullName = "MAX MUSTERMANN";
			orgAddress1.OA_Address1 = "TESTSTRASSE 1";
			orgAddress1.OA_City = "MAINZ";
			orgAddress1.OA_PostCode = "55126";
			orgAddress1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			var contact = orgAddress1.Header.Contacts.AddNew();
			contact.OC_Title = "CHEF";
			contact.OC_ContactName = "LUTZ LUSTIG";
			contact.OC_Phone = "06131474747";
			contact.OC_Fax = "06131474748";
			contact.OC_Email = "LUTZ.LUSTIG@EMAIL.DE";
			contact.OC_IsActive = true;
			return orgAddress1;
		}

		OrgAddress CreateAddress2()
		{
			var orgAddress2 = GetOrgWithoutEORNumberAndEORIBranch();
			orgAddress2.Header.OH_FullName = "MAX MUSTERMANN2";
			orgAddress2.OA_Address1 = "TESTSTRASSE 2";
			orgAddress2.OA_City = "MAINZ";
			orgAddress2.OA_PostCode = "55122";
			orgAddress2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			var contact2 = orgAddress2.Header.Contacts.AddNew();
			contact2.OC_Title = "CHEF";
			contact2.OC_ContactName = "LUTZ LUSTIG";
			contact2.OC_Phone = "06131474747";
			contact2.OC_Fax = "06131474748";
			contact2.OC_Email = "LUTZ.LUSTIG@EMAIL.DE";
			contact2.OC_IsActive = true;
			return orgAddress2;
		}

		OrgAddress CreateAddress3()
		{
			var orgAddress3 = GetOrgWithEORNumberAndEORIBranch("EOR3", "EBS3");
			orgAddress3.Header.OH_FullName = "MAX MUSTERMANN3";
			orgAddress3.OA_Address1 = "TESTSTRASSE 3";
			orgAddress3.OA_City = "MAINZ";
			orgAddress3.OA_PostCode = "55123";
			orgAddress3.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			return orgAddress3;
		}

		void SetupInvoiceLinesWithDifferentConsignees()
		{
			var orgAddress2 = CreateAddress2();
			var orgAddress3 = CreateAddress3();
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_OA_ConsigneeAddress = orgAddress2.PK;
			invoiceLine1.JI_CL = entryLine.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_OA_ConsigneeAddress = orgAddress3.PK;
			invoiceLine2.JI_CL = entryLine2.PK;
		}

		void SetupInvoiceLinesWithEqualConsignees()
		{
			var orgAddress1 = CreateAddress1();
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_OA_ConsigneeAddress = orgAddress1.PK;
			invoiceLine1.JI_CL = entryLine.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_OA_ConsigneeAddress = orgAddress1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;
		}

		void SetupInvoiceLinesWithNoConsignees()
		{
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
		}

		void SetupInvoiceLinesWithDifferentAndEmptyConsignees()
		{
			var orgAddress2 = CreateAddress2();
			var orgAddress3 = CreateAddress3();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_OA_ConsigneeAddress = orgAddress3.PK;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_OA_ConsigneeAddress = orgAddress2.PK;
			invoiceLine1.JI_CL = entryLine.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_OA_ConsigneeAddress = orgAddress3.PK;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_OA_ConsigneeAddress = orgAddress3.PK;
			invoiceLine2.JI_CL = entryLine2.PK;
			var entryLine3 = entryHeader.MergedLines.AddNew();
			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_OA_ConsigneeAddress = orgAddress3.PK;
			var invoiceLine3 = invoice3.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
		}

		void SetupInvoiceLinesWithEqualConsignors()
		{
			var orgAddress2 = CreateAddress2();
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_OA_ExporterAddress = orgAddress2.PK;
			invoiceLine1.JI_CL = entryLine.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_OA_ExporterAddress = orgAddress2.PK;
			invoiceLine2.JI_CL = entryLine2.PK;
		}

		void SetupInvoiceLinesWithNoConsignors()
		{
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
		}

		void SetupInvoiceLinesWithDifferentAndEmptyConsignors()
		{
			var orgAddress2 = CreateAddress2();
			var orgAddress3 = CreateAddress3();
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_OA_ExporterAddress = orgAddress2.PK;
			invoiceLine1.JI_CL = entryLine.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_OA_ExporterAddress = orgAddress3.PK;
			invoiceLine2.JI_CL = entryLine2.PK;
			var entryLine3 = entryHeader.MergedLines.AddNew();
			var invoice3 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice3.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
		}

		void CreateAuthorizationUsage(string code, string number)
		{
			var usage1 = entryInstruction.CusAuthorizationUsages.AddNew();
			usage1.AGC_Code = code;
			usage1.AGC_Number = number;
		}
	}
}
