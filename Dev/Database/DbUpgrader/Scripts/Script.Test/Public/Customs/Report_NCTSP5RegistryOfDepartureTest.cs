using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(Report_NCTSP5RegistryOfDeparture))]
	sealed class Report_NCTSP5RegistryOfDepartureTest : CustomsReportDbCreateScriptTest
	{
		#region columns

		public void TestRegistryNumber()
		{
			AssertFunctionReturnExpectedValue("RegistryNumber", "1");
		}

		public void TestLRNNumber()
		{
			AssertFunctionReturnExpectedValue("LRNNumber", string.Empty);

			UpdateCusInBondMoveHeaderColumn("BM_PaperlessInbondNum", "LRN001", SqlDbType.VarChar, inBondMoveHeaderPK);
			AssertFunctionReturnExpectedValue("LRNNumber", "LRN001");
		}

		public void TestMRNNumberOfTheDepartureDeclaration_WhenParentIsBH()
		{
			AssertFunctionReturnExpectedValue("MRNNumberOfTheDepartureDeclaration", "MRN001");
		}

		public void TestMRNNumberOfTheDepartureDeclaration_WhenParentIsBM()
		{
			using var command = Db.Connection.Command("DELETE FROM dbo.CusEntryNum WHERE CE_PK = @cusEntryPK");
			command.AddParameter("@cusEntryPK", SqlDbType.UniqueIdentifier, entryNumPK);
			command.ExecuteScalar();
			TestDataCreator.CreateCusEntryNum(inBondMoveHeaderPK, "CusInBondMoveHeader", "MRN002", "MRN", "CUS", "");

			AssertFunctionReturnExpectedValue("MRNNumberOfTheDepartureDeclaration", "MRN002");
		}

		public void TestDateOfDeparture() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("DateOfDeparture", new DateTime(2024, 01, 15, 00, 00, 00));
		});

		public void TestDateDeclaration() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("DateDeclaration", DBNull.Value);

			UpdateCusInBondMoveHeaderColumn("BM_EntryDate", new DateTime(2023, 03, 01, 23, 15, 00), SqlDbType.SmallDateTime, inBondMoveHeaderPK);
			AssertFunctionReturnExpectedValue("DateDeclaration", new DateTime(2023, 03, 01, 23, 15, 00));
		});

		public void TestLimitDate() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("LimitDate", DBNull.Value);

			UpdateCusInBondMoveHeaderColumn("BM_ExportDate", new DateTime(2023, 04, 01, 23, 15, 00), SqlDbType.SmallDateTime, inBondMoveHeaderPK);
			AssertFunctionReturnExpectedValue("LimitDate", new DateTime(2023, 04, 01, 23, 15, 00));
		});

		public void TestStatusDeclaration()
		{
			AssertFunctionReturnExpectedValue("StatusDeclaration", "DRL");
		}

		public void TestCustomerReferenceOfTheDeparture()
		{
			AssertFunctionReturnExpectedValue("CustomerReferenceOfTheDeparture", "001");
		}

		public void TestOfficeOfDestination() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("OfficeOfDestination", DBNull.Value);

			_ = TestDataCreator.CreateCusCodeData("EUO", "DES", "1234", inBondMoveHeaderPK, "BM");
			AssertFunctionReturnExpectedValue("OfficeOfDestination", "1234");
		});

		public void TestIDInlandTransportMeans() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("IDInlandTransportMeans", string.Empty);

			UpdateCusInBondMoveHeaderColumn("BM_TransportAtDeparture", "TAD", SqlDbType.VarChar, inBondMoveHeaderPK);
			UpdateCusInBondMoveHeaderColumn("BM_TransportAtDepartureTrailer1RegNo", "TADT1", SqlDbType.VarChar, inBondMoveHeaderPK);
			UpdateCusInBondMoveHeaderColumn("BM_TransportAtDepartureTrailer2RegNo", "TADT2", SqlDbType.VarChar, inBondMoveHeaderPK);
			AssertFunctionReturnExpectedValue("IDInlandTransportMeans", "TAD\nTADT1\nTADT2");

			var inbondBillPK = TestDataCreator.CreateCusInbondBill(inBondHeaderPK);
			_ = TestDataCreator.CreateCusTransportMeans(inbondBillPK, "B0", "TADB");
			AssertFunctionReturnExpectedValue("IDInlandTransportMeans", "TAD\nTADT1\nTADT2\nTADB");
		});

		public void TestEquipmentIdentifiers() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("EquipmentIdentifiers", DBNull.Value);

			_ = TestDataCreator.CreateCusInBondContainer(inBondHeaderPK, "BH", "FRNCT", "CNTR1234567");
			AssertFunctionReturnExpectedValue("EquipmentIdentifiers", "CNTR1234567");

			_ = TestDataCreator.CreateCusInBondContainer(inBondHeaderPK, "BH", "FRNCT", "CNTR1122334");
			AssertFunctionReturnExpectedValue("EquipmentIdentifiers", "CNTR1234567\nCNTR1122334");
		});

		public void TestSeals() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("Seals", DBNull.Value);

			_ = TestDataCreator.CreateCusInBondContainer(inBondHeaderPK, "BH", "FRNCT", "CNTR1234567");
			AssertFunctionReturnExpectedValue("Seals", string.Empty);

			var bc_pk = TestDataCreator.CreateCusInBondContainer(inBondHeaderPK, "BH", "FRNCT", "CNTR1122334", "Seal1", "Seal2");
			AssertFunctionReturnExpectedValue("Seals", "\n2 Seal1 Seal2");

			_ = TestDataCreator.CreateCusSeal(bc_pk, "BC", "Seal3");
			AssertFunctionReturnExpectedValue("Seals", "\n3 Seal1 Seal2 Seal3");

			_ = TestDataCreator.CreateCusInBondContainer(inBondHeaderPK, "BH", "FRNCT", "CNTR9988776", "SealZ");
			AssertFunctionReturnExpectedValue("Seals", "\n3 Seal1 Seal2 Seal3\n1 SealZ");
		});

		public void TestHWBReference() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("HWBReference", DBNull.Value);

			var inbondBillPK = TestDataCreator.CreateCusInbondBill(inBondHeaderPK);
			_ = TestDataCreator.CreateCusSupportingInfo("OTH", "CD1", "TRA", "REFNR", inbondBillPK, "B0");
			AssertFunctionReturnExpectedValue("HWBReference", "CD1 REFNR");
		});

		public void TestUCRReference() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("UCRReference", DBNull.Value);

			var inbondBillPK = TestDataCreator.CreateCusInbondBill(inBondHeaderPK);
			var goodsPK = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc123", "NEW", 150, 200);
			UpdateCusInBondMoveHeaderColumn("BM_UniqueConsignmentReference", "UCREF", SqlDbType.VarChar, inBondMoveHeaderPK);
			AssertFunctionReturnExpectedValue("UCRReference", "UCREF");

			UpdateCusInBondCargoDescColumn("BY_CommercialReferenceNumber", "CREF", SqlDbType.VarChar, goodsPK);
			AssertFunctionReturnExpectedValue("UCRReference", "CREF");
		});

		public void TestConsignee() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("Consignee", DBNull.Value);

			var inbondBillPK = TestDataCreator.CreateCusInbondBill(inBondHeaderPK);
			var goodsPK = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc123", "NEW", 150, 200);

			var consigneeBHPK = TestDataCreator.CreateOrganisation("OrgCode BH", "Company Name BH");
			var addressBHPK = TestDataCreator.CreateAddress(consigneeBHPK, "Delivery Address", "Address BH");
			_ = TestDataCreator.CreateDocAddress(addressBHPK, "", inBondHeaderPK, "BH", "CEA");
			AssertFunctionReturnExpectedValue("Consignee", "Company Name BH");

			var consigneeB0PK = TestDataCreator.CreateOrganisation("OrgCode B0", "Company Name B0");
			var addressB0PK = TestDataCreator.CreateAddress(consigneeB0PK, "Delivery Address", "Address B0");
			_ = TestDataCreator.CreateDocAddress(addressB0PK, "", inbondBillPK, "B0", "CEA");
			AssertFunctionReturnExpectedValue("Consignee", "Company Name B0");

			var consigneeBYPK = TestDataCreator.CreateOrganisation("OrgCode BY", "Company Name BY");
			var addressBYPK = TestDataCreator.CreateAddress(consigneeBYPK, "Delivery Address", "Address BY");
			_ = TestDataCreator.CreateDocAddress(addressBYPK, "", goodsPK, "BY", "CEA");
			AssertFunctionReturnExpectedValue("Consignee", "Company Name BY");
		});

		public void TestDescriptionOfTheGoods() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("DescriptionOfTheGoods", DBNull.Value);

			var inbondBillPK = TestDataCreator.CreateCusInbondBill(inBondHeaderPK);
			var goodsPK = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc123", "NEW", 150, 200);
			AssertFunctionReturnExpectedValue("DescriptionOfTheGoods", "desc123");

			_ = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc456", "NEW", 150, 200);
			AssertFunctionReturnExpectedValue("DescriptionOfTheGoods", "desc123\ndesc456");
		});

		public void TestCommodityCode() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("CommodityCode", DBNull.Value);

			var inbondBillPK = TestDataCreator.CreateCusInbondBill(inBondHeaderPK);
			var goodsPK = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc123", "NEW", 150, 200, "555");
			AssertFunctionReturnExpectedValue("CommodityCode", "555");

			_ = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc456", "NEW", 150, 200, "666");
			AssertFunctionReturnExpectedValue("CommodityCode", "555\n666");
		});

		public void TestNumberOfPackagesAndCodeOfPackages() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("NumberOfPackagesAndCodeOfPackages", DBNull.Value);

			var inbondBillPK = TestDataCreator.CreateCusInbondBill(inBondHeaderPK);
			var cargoDescPK = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc123", "NEW", 150, 200);
			_ = TestDataCreator.CreateCusInvPack(cargoDescPK, "BY", "CNT", 50);
			AssertFunctionReturnExpectedValue("NumberOfPackagesAndCodeOfPackages", "50 CNT");

			_ = TestDataCreator.CreateCusInvPack(cargoDescPK, "BY", "CNT", 55);
			AssertFunctionReturnExpectedValue("NumberOfPackagesAndCodeOfPackages", "50 CNT\n55 CNT");
		});

		public void TestNetWeight() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("NetWeight", DBNull.Value);

			var inbondBillPK = TestDataCreator.CreateCusInbondBill(inBondHeaderPK);
			_ = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc123", "NEW", 150, 200);
			AssertFunctionReturnExpectedValue("NetWeight", "150");

			_ = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc123", "NEW", 155, 200);
			AssertFunctionReturnExpectedValue("NetWeight", "150\n155");
		});

		public void TestGrossWeight() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("GrossWeight", DBNull.Value);

			var inbondBillPK = TestDataCreator.CreateCusInbondBill(inBondHeaderPK);
			_ = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc123", "NEW", 150, 200);
			AssertFunctionReturnExpectedValue("GrossWeight", "200");

			_ = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc123", "NEW", 150, 205);
			AssertFunctionReturnExpectedValue("GrossWeight", "200\n205");
		});

		public void TestCountryOfDestination() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("CountryOfDestination", DBNull.Value);

			var inbondBillPK = TestDataCreator.CreateCusInbondBill(inBondHeaderPK);
			var goodsPK = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc123", "NEW", 150, 200);
			UpdateCusInBondMoveHeaderColumn("BM_RL_NKDestinationPort", "CM", SqlDbType.VarChar, inBondMoveHeaderPK);
			AssertFunctionReturnExpectedValue("CountryOfDestination", "CM");

			UpdateCusInBondBillColumn("B0_RN_NKCountryOfDestination", "C0", SqlDbType.VarChar, inbondBillPK);
			AssertFunctionReturnExpectedValue("CountryOfDestination", "C0");

			UpdateCusInBondCargoDescColumn("BY_RN_NKCountryOfDestination", "CY", SqlDbType.VarChar, goodsPK);
			AssertFunctionReturnExpectedValue("CountryOfDestination", "CY");
		});

		public void TestCountryOfDispatch() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("CountryOfDestination", DBNull.Value);

			var inbondBillPK = TestDataCreator.CreateCusInbondBill(inBondHeaderPK);
			var goodsPK = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc123", "NEW", 150, 200);
			UpdateCusInBondMoveHeaderColumn("BM_RN_NKCountryOfDispatch", "CM", SqlDbType.VarChar, inBondMoveHeaderPK);
			AssertFunctionReturnExpectedValue("CountryOfDispatch", "CM");

			UpdateCusInBondBillColumn("B0_RN_NKCountryOfExport", "C0", SqlDbType.VarChar, inbondBillPK);
			AssertFunctionReturnExpectedValue("CountryOfDispatch", "C0");

			UpdateCusInBondCargoDescColumn("BY_RN_NKCountryOfDispatch", "CY", SqlDbType.VarChar, goodsPK);
			AssertFunctionReturnExpectedValue("CountryOfDispatch", "CY");
		});

		public void TestGoodsAndPackages() => CombineAssertions(() =>
		{
			var inbondBillPK = TestDataCreator.CreateCusInbondBill(inBondHeaderPK);
			var cargoDescPK1 = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc123", "NEW", 150, 200, "555");
			UpdateCusInBondCargoDescColumn("BY_RN_NKCountryOfDestination", "L1", SqlDbType.VarChar, cargoDescPK1);
			UpdateCusInBondCargoDescColumn("BY_RN_NKCountryOfDispatch", "N1", SqlDbType.VarChar, cargoDescPK1);
			_ = TestDataCreator.CreateCusInvPack(cargoDescPK1, "BY", "CNT", 50);
			_ = TestDataCreator.CreateCusInvPack(cargoDescPK1, "BY", "CNT", 55);
			var cargoDescPK2 = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc456", "NEW", 160, 210, "666");
			UpdateCusInBondCargoDescColumn("BY_RN_NKCountryOfDestination", "L2", SqlDbType.VarChar, cargoDescPK2);
			UpdateCusInBondCargoDescColumn("BY_RN_NKCountryOfDispatch", "N2", SqlDbType.VarChar, cargoDescPK2);
			_ = TestDataCreator.CreateCusInvPack(cargoDescPK2, "BY", "CNT", 60);
			_ = TestDataCreator.CreateCusInvPack(cargoDescPK2, "BY", "CNT", 65);

			AssertFunctionReturnExpectedValue("DescriptionOfTheGoods", "desc123\n\ndesc456");
			AssertFunctionReturnExpectedValue("CommodityCode", "555\n\n666");
			AssertFunctionReturnExpectedValue("NumberOfPackagesAndCodeOfPackages", "50 CNT\n55 CNT\n60 CNT\n65 CNT");
			AssertFunctionReturnExpectedValue("NetWeight", "150\n\n160");
			AssertFunctionReturnExpectedValue("GrossWeight", "200\n\n210");
			AssertFunctionReturnExpectedValue("CountryOfDestination", "L1\n\nL2");
			AssertFunctionReturnExpectedValue("CountryOfDispatch", "N1\n\nN2");
		});

		readonly string dateTimeStringFormat = "dd/MM/yyyy H:mm:ss";

		public void TestSubsequentDeclarations_JI() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValueForSubsequentDeclarations(DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);

			var dateTime1 = new DateTime(2023, 11, 28, 23, 15, 00);
			SetupSubsequentDeclarations_JI(1, "MRN002", dateTime1, "A", 100, "CT", 10, "PL");
			var dateTimeString1 = dateTime1.ToString(dateTimeStringFormat);
			AssertFunctionReturnExpectedValueForSubsequentDeclarations("MRN002", dateTimeString1, "A", "10", "PL", "100", "CT");

			var dateTime2 = new DateTime(2023, 11, 20, 23, 15, 00);
			SetupSubsequentDeclarations_JI(2, "MRN012", dateTime2, "B", 110, "TC", 11, "LP");
			var dateTimeString2 = dateTime2.ToString(dateTimeStringFormat);
			AssertFunctionReturnExpectedValueForSubsequentDeclarations("MRN002\nMRN012", $"{dateTimeString1}\n{dateTimeString2}", "A\nB", "10\n11", "PL\nLP", "100\n110", "CT\nTC");
		});

		public void TestSubsequentDeclarations_BY() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValueForSubsequentDeclarations(DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);

			var dateTime1 = new DateTime(2023, 11, 27, 23, 15, 00);
			SetupSubsequentDeclarations_BY("MRN003", dateTime1, 100, "CT", 10, "PL");
			var dateTimeString1 = dateTime1.ToString(dateTimeStringFormat);
			AssertFunctionReturnExpectedValueForSubsequentDeclarations("MRN003", dateTimeString1, "D1", "10", "PL", "100", "CT");

			var dateTime2 = new DateTime(2023, 11, 19, 23, 15, 00);
			SetupSubsequentDeclarations_BY("MRN013", dateTime1, 110, "TC", 11, "LP");
			var dateTimeString2 = dateTime1.ToString(dateTimeStringFormat);
			AssertFunctionReturnExpectedValueForSubsequentDeclarations("MRN003\nMRN013", $"{dateTimeString1}\n{dateTimeString2}", "D1\nD1", "10\n11", "PL\nLP", "100\n110", "CT\nTC");
		});

		public void TestSubsequentDeclarations_ABL() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValueForSubsequentDeclarations(DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);

			var inbondBillPK = TestDataCreator.CreateCusInbondBill(inBondHeaderPK);
			var cargoDescPK = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc123", "NEW", 90, 100, "555");
			UpdateCusInBondCargoDescColumn("BY_RN_NKCountryOfDestination", "L1", SqlDbType.VarChar, cargoDescPK);
			UpdateCusInBondCargoDescColumn("BY_RN_NKCountryOfDispatch", "N1", SqlDbType.VarChar, cargoDescPK);
			_ = TestDataCreator.CreateCusInvPack(cargoDescPK, "BY", "X1", 10);

			var dateTime1 = new DateTime(2023, 11, 26, 23, 15, 00);
			SetupSubsequentDeclarations_ABL(1, "MRN004", dateTime1);
			var dateTimeString1 = dateTime1.ToString(dateTimeStringFormat);
			AssertFunctionReturnExpectedValueForSubsequentDeclarations("MRN004", dateTimeString1, "G4", "10", "X1", "100", "KG");

			var dateTime2 = new DateTime(2023, 11, 18, 23, 15, 00);
			SetupSubsequentDeclarations_ABL(2, "MRN014", dateTime2);
			var dateTimeString2 = dateTime2.ToString(dateTimeStringFormat);
			AssertFunctionReturnExpectedValueForSubsequentDeclarations("MRN004\nMRN014", $"{dateTimeString1}\n{dateTimeString2}", "G4\nG4", "10\n10", "X1\nX1", "100\n100", "KG\nKG");
		});

		public void TestSubsequentDeclarations() => CombineAssertions(() =>
		{
			var inbondBillPK = TestDataCreator.CreateCusInbondBill(inBondHeaderPK);
			var cargoDescPK = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc123", "NEW", 90, 100, "555");
			UpdateCusInBondCargoDescColumn("BY_RN_NKCountryOfDestination", "L1", SqlDbType.VarChar, cargoDescPK);
			UpdateCusInBondCargoDescColumn("BY_RN_NKCountryOfDispatch", "N1", SqlDbType.VarChar, cargoDescPK);
			_ = TestDataCreator.CreateCusInvPack(cargoDescPK, "BY", "X1", 10);

			var dateTime1 = new DateTime(2023, 11, 26, 23, 15, 00);
			SetupSubsequentDeclarations_ABL(1, "MRN004", dateTime1);

			var dateTime2 = new DateTime(2023, 11, 27, 23, 15, 00);
			SetupSubsequentDeclarations_BY("MRN003", dateTime2, 101, "Y2", 11, "X2");

			var dateTime3 = new DateTime(2023, 11, 28, 23, 15, 00);
			SetupSubsequentDeclarations_JI(1, "MRN002", dateTime3, "A", 102, "Y3", 12, "X3");

			var dateTimeString1 = dateTime1.ToString(dateTimeStringFormat);
			var dateTimeString2 = dateTime2.ToString(dateTimeStringFormat);
			var dateTimeString3 = dateTime3.ToString(dateTimeStringFormat);

			AssertFunctionReturnExpectedValueForSubsequentDeclarations("MRN004\nMRN003\nMRN002", $"{dateTimeString1}\n{dateTimeString2}\n{dateTimeString3}", "G4\nD1\nA", "10\n11\n12", "X1\nX2\nX3", "100\n101\n102", "KG\nY2\nY3");
		});

		void AssertFunctionReturnExpectedValueForSubsequentDeclarations(object subDecMrnNumber, object subDecAcceptanceDateDocument, object subDecSubSequentProcedure, object subDecNumberOfPackages, object subDecPackageCode, object subDecGrossWeight, object subDecWeightCode)
		{
			AssertFunctionReturnExpectedValue("SubDecMrnNumber", subDecMrnNumber);
			AssertFunctionReturnExpectedValue("SubDecAcceptanceDateDocument", subDecAcceptanceDateDocument);
			AssertFunctionReturnExpectedValue("SubDecSubsequentProcedure", subDecSubSequentProcedure);
			AssertFunctionReturnExpectedValue("SubDecNumberOfPackages", subDecNumberOfPackages);
			AssertFunctionReturnExpectedValue("SubDecPackageCode", subDecPackageCode);
			AssertFunctionReturnExpectedValue("SubDecGrossWeight", subDecGrossWeight);
			AssertFunctionReturnExpectedValue("SubDecWeightCode", subDecWeightCode);
		}

		#endregion
		#region Filters

		public void TestFilterByOrganisation()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "RegistryNumber", (Report_NCTSP5RegistryOfDepartureParameters.Organisation, orgHeaderPK));
			AssertContainsExactElementsInAnyOrder(new string[] { "1" }, filteredRows);
		}

		public void TestFilterByStartDate()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "RegistryNumber", (Report_NCTSP5RegistryOfDepartureParameters.StartingDate, new DateTime(2023, 11, 22)));
			AssertContainsExactElementsInAnyOrder(new string[] { "1" }, filteredRows);
		}

		public void TestFilterByRegistryNumberRangeFrom()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "RegistryNumber", (Report_NCTSP5RegistryOfDepartureParameters.RegistryNumberFrom, 1));
			AssertContainsExactElementsInAnyOrder(new string[] { "1" }, filteredRows);
		}

		public void TestFilterByRegistryNumberRangeTo()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "RegistryNumber", (Report_NCTSP5RegistryOfDepartureParameters.RegistryNumberTo, 1));
			AssertContainsExactElementsInAnyOrder(new string[] { "1" }, filteredRows);
		}

		public void TestFilterByRegistryNumberRange()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "RegistryNumber", (Report_NCTSP5RegistryOfDepartureParameters.RegistryNumberFrom, 1), (Report_NCTSP5RegistryOfDepartureParameters.RegistryNumberTo, 1));
			AssertContainsExactElementsInAnyOrder(new string[] { "1" }, filteredRows);
		}

		public void TestFilterAll()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "RegistryNumber", (Report_NCTSP5RegistryOfDepartureParameters.Organisation, orgHeaderPK), (Report_NCTSP5RegistryOfDepartureParameters.StartingDate, new DateTime(2023, 11, 22)), (Report_NCTSP5RegistryOfDepartureParameters.RegistryNumberFrom, 1), (Report_NCTSP5RegistryOfDepartureParameters.RegistryNumberTo, 1));
			AssertContainsExactElementsInAnyOrder(new string[] { "1" }, filteredRows);
		}

		#endregion
		#region implementation

		protected override void SetUp()
		{
			base.SetUp();

			companyPK = TestDataCreator.CreateCompany(companyCode: "BEL", countryCode: "BE", currencyCode: "EUR");
			branchPK = TestDataCreator.CreateBranch(companyPK, branchCode: "BEL", homePort: "BELGIUM");
			orgHeaderPK = TestDataCreator.CreateOrganisation("BOB", "Company Name X");
			inBondHeaderPK = TestDataCreator.CreateCusInbondHeader("001", branchPK, "NC5", "D");
			inBondMoveHeaderPK = TestDataCreator.CreateCusInBondMoveHeader(inBondHeaderPK, "D", "DRL");
			entryNumPK = TestDataCreator.CreateCusEntryNum(inBondHeaderPK, "CusInBondHeader", "MRN001", "MRN", "CUS", "", issueDate: new DateTime(2024, 01, 15));
			_ = TestDataCreator.CreateCusEntryNum(inBondMoveHeaderPK, "CusInBondMoveHeader", "1", "REG", "CUS", "", issueDate: new DateTime(2024, 01, 16), entryLineReference: "TD-BOB");
		}

		Guid companyPK;
		Guid branchPK;
		Guid orgHeaderPK;
		Guid inBondHeaderPK;
		Guid inBondMoveHeaderPK;
		Guid entryNumPK;

		void SetupSubsequentDeclarations_JI(int clusterKey, string subDecMrnNumber, DateTime subDecAcceptanceDateDocument, string subDecSubSequentProcedure, int subDecGrossWeight, string subDecWeightCode, int subDecNumberOfPackages, string subDecPackageCode)
		{
			var je_pk = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, Guid.NewGuid().ToString("n"), "IMP", clusterKey);
			var cei_pk = TestDataCreator.CreateCusEntryInstruction(je_pk, subDecSubSequentProcedure, "D", new DateTime(2023, 11, 29), clusterKey);
			var ch_pk = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "100", "", 0f, 0, "", je_pk, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), cei_pk, "", new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), clusterKey);
			var jz_pk = TestDataCreator.CreateJobComInvoiceHeader(invoiceCurrency: "EUR", defaultOrigin: "NL", invoiceNumber: "123", incoTerm: "CIP", jobDeclarationPK: je_pk, clusterKey: clusterKey);
			var cl_pk = TestDataCreator.CreateCusEntryLine(clCh: ch_pk, lineNumber: 1, dutyPercent: 2, flatAmount: 300, flatAmountUQ: "KG", clusterKey: clusterKey);
			var cc_pk = TestDataCreator.CreateCusClassification(lookupCode: $"A{clusterKey}", countryCode: "BE", classificationType: "IMP");
			var ji_pk = TestDataCreator.CreateJobComInvoiceLine(lineNo: 1, partNo: 1, description: "Invoice line", invoiceQuantity: 100, invoiceUQ: "EUR", customsQuantity: 101, customsUnitQty: "EUR", linePrice: 45, tariff: 2, jobComInvoiceHeaderPK: jz_pk, cl_pk: cl_pk, cc_pk: cc_pk, clusterKey: clusterKey, cei_pk: cei_pk);
			_ = TestDataCreator.CreateCusSupportingInfo("PRE", "", "", "MRN001", ji_pk, "JI", "BE", subDecGrossWeight, subDecWeightCode, 0, "", subDecNumberOfPackages, subDecPackageCode);
			_ = TestDataCreator.CreateCusEntryNum(ch_pk, "CusEntryHeader", subDecMrnNumber, "MRN", "CUS", "", issueDate: subDecAcceptanceDateDocument);
		}

		void SetupSubsequentDeclarations_BY(string subDecMrnNumber, DateTime subDecAcceptanceDateDocument, int subDecGrossWeight, string subDecWeightCode, int subDecNumberOfPackages, string subDecPackageCode)
		{
			var bh_pk = TestDataCreator.CreateCusInbondHeader("002", branchPK, "NC5", "D");
			var b0_pk = TestDataCreator.CreateCusInbondBill(bh_pk);
			var by_pk = TestDataCreator.CreateCusInBondCargoDesc(b0_pk, "B0", "", "", 0, 0);
			_ = TestDataCreator.CreateCusSupportingInfo("PRE", "", "", "MRN001", by_pk, "BY", "", subDecGrossWeight, subDecWeightCode, subDecNumberOfPackages, subDecPackageCode);
			_ = TestDataCreator.CreateCusEntryNum(bh_pk, "CusInBondHeader", subDecMrnNumber, "MRN", "CUS", "", issueDate: subDecAcceptanceDateDocument);
		}

		void SetupSubsequentDeclarations_ABL(int clusterKey, string subDecMrnNumber, DateTime subDecAcceptanceDateDocument)
		{
			var ama_pk = TestDataCreator.CreateAsycudaManifestHeader(branchPK, $"TS{clusterKey}", "BE", clusterKey);
			var abl_pk = TestDataCreator.CreateAsycudaBill(ama_pk, clusterKey);
			_ = TestDataCreator.CreateCusSupportingInfo("PRE", "", "", "MRN001", abl_pk, "ABL");
			_ = TestDataCreator.CreateCusEntryNum(ama_pk, "AsycudaManifestHeader", subDecMrnNumber, "MRN", "CUS", "", issueDate: subDecAcceptanceDateDocument);
		}

		protected override IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters()
		{
			yield return (SqlDbType.UniqueIdentifier, Report_NCTSP5RegistryOfDepartureParameters.Organisation);
			yield return (SqlDbType.SmallDateTime, Report_NCTSP5RegistryOfDepartureParameters.StartingDate);
			yield return (SqlDbType.VarChar, Report_NCTSP5RegistryOfDepartureParameters.RegistryNumberFrom);
			yield return (SqlDbType.VarChar, Report_NCTSP5RegistryOfDepartureParameters.RegistryNumberTo);
		}

		class Report_NCTSP5RegistryOfDepartureParameters
		{
			public const string Organisation = "@Organisation";
			public const string StartingDate = "@StartingDate";
			public const string RegistryNumberFrom = "@RegistryNumberFrom";
			public const string RegistryNumberTo = "@RegistryNumberTo";
		}

		void UpdateCusInBondMoveHeaderColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusInBondMoveHeader", columnName, columnValue, columnType, "BM_PK", primaryKeyValue);
		}

		void UpdateCusInBondCargoDescColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusInBondCargoDesc", columnName, columnValue, columnType, "BY_PK", primaryKeyValue);
		}

		void UpdateCusInBondBillColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusInBondBill", columnName, columnValue, columnType, "B0_PK", primaryKeyValue);
		}

		#endregion
	}
}
