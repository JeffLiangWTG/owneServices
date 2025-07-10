using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.Universal.Constants;
using ESNctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.DocumentWrappers.NCTS.Testing
{
	sealed class CommonNctsHeaderDocumentWrapperTest : TestCaseWithFactory
	{
		public void TestGetNumberFormatSpain()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number introduced is long 1000 and it's returned with the correct format", "1.000", CommonNctsHeaderDocumentWrapper.GetNumberFormatSpain((ZLong)1000, false));

				AssertEquals("Number introduced is int 1000000 and it's returned with the correct format", "1.000.000", CommonNctsHeaderDocumentWrapper.GetNumberFormatSpain((ZInt)1000000, false));

				AssertEquals("Number introduced is short 2000 and it's returned with the correct format", "2.000", CommonNctsHeaderDocumentWrapper.GetNumberFormatSpain((ZShort)2000, false));

				AssertEquals("Number introduced is null so we get empty string returned", ZString.Empty, CommonNctsHeaderDocumentWrapper.GetNumberFormatSpain(null, false));

				AssertEquals("Number introduced is decimal 5100.05 and flag isWeight is true and is final period and it's returned with the correct format", "5.100,050000", CommonNctsHeaderDocumentWrapper.GetNumberFormatSpain((ZDecimal)5100.05, false));

				AssertEquals("Number introduced is decimal 100.00 and flag isWeight is true and is final period and it's returned with the correct format", "100,000000", CommonNctsHeaderDocumentWrapper.GetNumberFormatSpain((ZDecimal)100.00, false));

				AssertEquals("Number introduced is decimal 0.100 and is final period and it's returned with the correct format", "0,100000", CommonNctsHeaderDocumentWrapper.GetNumberFormatSpain((ZDecimal)0.100, false));

				AssertEquals("Number introduced is decimal 1000000.1234 and flag isWeight is true and is final period and it's returned with the correct format", "1.000.000,123400", CommonNctsHeaderDocumentWrapper.GetNumberFormatSpain((ZDecimal)1000000.1234, false));

				AssertEquals("Number introduced is decimal 100.1234567 and flag isWeight is true and is final period and it's returned with the correct format", "100,123457", CommonNctsHeaderDocumentWrapper.GetNumberFormatSpain((ZDecimal)100.1234567, false));

				AssertEquals("Number introduced is decimal 5100.05 and flag isWeight is true and is transition period and it's returned with the correct format", "5.100,050", CommonNctsHeaderDocumentWrapper.GetNumberFormatSpain((ZDecimal)5100.05, true));

				AssertEquals("Number introduced is decimal 100.00 and flag isWeight is true and is transition period and it's returned with the correct format", "100,000", CommonNctsHeaderDocumentWrapper.GetNumberFormatSpain((ZDecimal)100.00, true));

				AssertEquals("Number introduced is decimal 0.100 and is transition period and it's returned with the correct format", "0,100", CommonNctsHeaderDocumentWrapper.GetNumberFormatSpain((ZDecimal)0.100, true));

				AssertEquals("Number introduced is decimal 1000000.1234 and flag isWeight is true and is transition period and it's returned with the correct format", "1.000.000,123", CommonNctsHeaderDocumentWrapper.GetNumberFormatSpain((ZDecimal)1000000.1234, true));

				AssertEquals("Number introduced is decimal 100.1234567 and flag isWeight is true and is transition period and it's returned with the correct format", "100,123", CommonNctsHeaderDocumentWrapper.GetNumberFormatSpain((ZDecimal)100.1234567, true));
			});
		}

		public void TestGetDateFormatSpain()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Date introduced is 16/02/1995 and it's returned with the correct format", "16-02-2022", CommonNctsHeaderDocumentWrapper.GetDateFormatSpain(new ZDateTime(2022, 02, 16)));

				AssertEquals("Date introduced is null so we get empty string returned", ZString.Empty, CommonNctsHeaderDocumentWrapper.GetDateFormatSpain(null));
			});
		}

		public void TestGetBox35GrossMass()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Method returns 1,100000 when gross mass is 1.1 and final period", "1,100000", CommonNctsHeaderDocumentWrapper.GetBox35GrossMass(1.1, false));

				AssertEquals("Method returns 0,010000 when gross mass is 0.01 and final period", "0,010000", CommonNctsHeaderDocumentWrapper.GetBox35GrossMass(0.01, false));

				AssertEquals("Method returns 100,010000 when gross mass is 100.01 and final period", "100,010000", CommonNctsHeaderDocumentWrapper.GetBox35GrossMass(100.01, false));

				AssertEquals("Method returns 1,100 when gross mass is 1.1 and transition period", "1,100", CommonNctsHeaderDocumentWrapper.GetBox35GrossMass(1.1, true));

				AssertEquals("Method returns 0,010 when gross mass is 0.01 and transition period", "0,010", CommonNctsHeaderDocumentWrapper.GetBox35GrossMass(0.01, true));

				AssertEquals("Method returns 100,010 when gross mass is 100.01 and transition period", "100,010", CommonNctsHeaderDocumentWrapper.GetBox35GrossMass(100.01, true));
			});
		}

		public void TestGetTotalBox35GrossMass()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var moveHeader = header.MovementHeader;
			var item1 = moveHeader.GoodsItems.AddNew();
			item1.BY_GrossWeight = 30.1;
			item1.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;

			var item2 = moveHeader.GoodsItems.AddNew();
			item2.BY_GrossWeight = 30.2;
			item2.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;

			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
				{
					moveHeader.BM_GrossWeight = 40.123456789m;
					AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 40,123457 when gross mass is declared in moveHeader and final period", "40,123457", CommonNctsHeaderDocumentWrapper.GetTotalBox35GrossMass(header));

					moveHeader.BM_GrossWeight = 0.03m;
					AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 0,030000 when gross mass is declared in moveHeader and final period", "0,030000", CommonNctsHeaderDocumentWrapper.GetTotalBox35GrossMass(header));

					moveHeader.BM_GrossWeight = 0m;
					AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 0,000000 when gross mass is 0 in moveHeader (even when there is gross weight in items) and final period", "0,000000", CommonNctsHeaderDocumentWrapper.GetTotalBox35GrossMass(header));
				}

				moveHeader.GoodsItems.DeleteAll();

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
				{
					moveHeader.BM_GrossWeight = 40.123456789m;
					AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 40,123 when gross mass is declared in moveHeader and transition period", "40,123", CommonNctsHeaderDocumentWrapper.GetTotalBox35GrossMass(header));

					moveHeader.BM_GrossWeight = 0.03m;
					AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 0,030 when gross mass is declared in moveHeader and transition period", "0,030", CommonNctsHeaderDocumentWrapper.GetTotalBox35GrossMass(header));

					moveHeader.BM_GrossWeight = 0m;
					AssertEquals(nameof(NctsHeaderDocumentWrapper.BOX35GROSSMASS) + " is 0,000 when gross mass is 0 in moveHeader (even when there is gross weight in items) and transition period", "0,000", CommonNctsHeaderDocumentWrapper.GetTotalBox35GrossMass(header));
				}
			});
		}

		public void TestGetBoxCOfficeOfDepartureData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			var countryCode = Core.Constants.CountryCodes.Spain;
			helper.CreateNewOrGetExistingDataGrouping(countryCode, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");

			var officeCode = "ES002801";
			var description = "Office Description";

			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_Code = officeCode;
			cusCodeList.ZZD_Description = description;
			cusCodeList.ZZD_CodeType = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			cusCodeList.ZZD_CountryOrGrouping = countryCode;
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddDays(2);
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddDays(-2);

			var header = Factory.New<ESNctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.CustomsOffices.RemoveAndDeleteAll();

			CombineAssertions(() =>
			{
				AssertEquals("GetBoxCOfficeOfDepartureData returns empty string when no DEP customs office exists", ZString.Empty, CommonNctsHeaderDocumentWrapper.GetBoxCOfficeOfDepartureData(header));

				var cusOffice = header.CustomsOffices.AddNew();
				cusOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				cusOffice.CY_Data = officeCode;
				AssertEquals("GetBoxCOfficeOfDepartureData shows office's description and code when DEP customs office exists but mrn doesn't", description + "\n" + officeCode, CommonNctsHeaderDocumentWrapper.GetBoxCOfficeOfDepartureData(header));

				var entryNum = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, header.CountryCode);
				entryNum.CE_EntryNum = "21ES00999912345678";
				entryNum.CE_IssueDate = new ZDateTime(2022, 02, 16);
				AssertEquals("GetBoxCOfficeOfDepartureData shows office's description, code and arrival date when DEP customs office and mrn exist and mrn has issue date", description + "\n" + officeCode + "\n16-02-2022", CommonNctsHeaderDocumentWrapper.GetBoxCOfficeOfDepartureData(header));

				header.CustomsOffices.RemoveAndDeleteAll();
				AssertEquals("GetBoxCOfficeOfDepartureData shows arrival date mrn exists and has issue date but no DEP customs office exists", "\n16-02-2022", CommonNctsHeaderDocumentWrapper.GetBoxCOfficeOfDepartureData(header));
			});
		}

		public void TestGetBoxDResult()
		{
			var header = Factory.New<ESNctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			CombineAssertions(() =>
			{
				AssertEquals("GetBoxDResult returns empty string when no clearance criteria is set", ZString.Empty, CommonNctsHeaderDocumentWrapper.GetBoxDResult(header));

				header.ClearanceCriteria = "A2";
				AssertEquals("GetBoxDResult shows the correct description when clearance criteria is A2", "A2 Normal Procedure", CommonNctsHeaderDocumentWrapper.GetBoxDResult(header));

				header.ClearanceCriteria = "A3";
				AssertEquals("GetBoxDResult shows the correct description when clearance criteria is A3", "A3 Simplified Procedure", CommonNctsHeaderDocumentWrapper.GetBoxDResult(header));

				header.ClearanceCriteria = "A1";
				AssertEquals("GetBoxDResult returns empty string when clearance criteria is not A2 or A3", ZString.Empty, CommonNctsHeaderDocumentWrapper.GetBoxDResult(header));
			});
		}

		public void TestGetBoxDTimeLimitDate()
		{
			var header = Factory.New<ESNctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			CombineAssertions(() =>
			{
				AssertEquals("GetBoxDTimeLimitDate returns empty string when no csv clearance is set", ZString.Empty, CommonNctsHeaderDocumentWrapper.GetBoxDTimeLimitDate(header));

				var entryNum = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Spain.ClearanceCSV, header.CountryCode);
				entryNum.CE_EntryNum = "B9026422646FE2AB";
				entryNum.CE_IssueDate = new ZDateTime(2022, 02, 16);
				entryNum.CE_ExpiryDate = new ZDateTime(2025, 02, 16);
				AssertEquals("GetBoxDTimeLimitDate shows the correct date when a csv clearance number is set with expiry date", "16-02-2025", CommonNctsHeaderDocumentWrapper.GetBoxDTimeLimitDate(header));

				entryNum.CE_ExpiryDate = ZDateTime.Empty;
				AssertEquals("GetBoxDTimeLimitDate returns empty string when  a csv clearance number is set with empty expiry date", ZString.Empty, CommonNctsHeaderDocumentWrapper.GetBoxDTimeLimitDate(header));
			});
		}

		public void TestGetBoxDClearance()
		{
			var header = Factory.New<ESNctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			CombineAssertions(() =>
			{
				AssertEquals("GetBoxDClearance returns only expected Spanish text when no csv clearance is set", "----------------------------------------------------------------------------------------------------\nAutentificación", CommonNctsHeaderDocumentWrapper.GetBoxDClearance(header));

				var entryNum = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Spain.ClearanceCSV, header.CountryCode);
				entryNum.CE_EntryNum = "B9026422646FE2AB";
				entryNum.CE_IssueDate = new ZDateTime(2022, 02, 16);
				AssertEquals("GetBoxDClearance shows the Spanish text and the csv clrarance code when a csv clearance number is set", "----------------------------------------------------------------------------------------------------\nAutentificación: B9026422646FE2AB", CommonNctsHeaderDocumentWrapper.GetBoxDClearance(header));
			});
		}
	}
}
