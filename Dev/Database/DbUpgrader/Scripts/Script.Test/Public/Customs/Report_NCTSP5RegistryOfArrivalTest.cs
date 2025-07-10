using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(Report_NCTSP5RegistryOfArrival))]
	sealed class Report_NCTSP5RegistryOfArrivalTest : CustomsReportDbCreateScriptTest
	{
		#region columns

		public void TestRegistryNumber()
		{
			AssertFunctionReturnExpectedValue("RegistryNumber", "1");
		}

		public void TestMRNNumberOfTheArrivalNotification()
		{
			AssertFunctionReturnExpectedValue("MRNNumberOfTheArrivalNotification", "MRN001");
		}

		public void TestDateOfArrival() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("DateOfArrival", DBNull.Value);

			UpdateCusInBondMoveHeaderColumn("BM_ArrivalDate", new DateTime(2023, 11, 24, 23, 15, 00), SqlDbType.SmallDateTime, inBondMoveHeaderPK);
			AssertFunctionReturnExpectedValue("DateOfArrival", new DateTime(2023, 11, 24, 23, 15, 00));
		});

		public void TestPresentationOffice_WhenParentTableIsBH() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("PresentationOffice", DBNull.Value);

			_ = TestDataCreator.CreateCusCodeData("EUO", "DSA", "1234", inBondHeaderPK, "BH");
			AssertFunctionReturnExpectedValue("PresentationOffice", "1234");
		});

		public void TestPresentationOffice_WhenParentTableIsBM() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("PresentationOffice", DBNull.Value);

			_ = TestDataCreator.CreateCusCodeData("EUO", "DSA", "1234", inBondMoveHeaderPK, "BM");
			AssertFunctionReturnExpectedValue("PresentationOffice", "1234");
		});

		public void TestUnloadingDate() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("UnloadingDate", DBNull.Value);

			UpdateCusInBondMoveHeaderColumn("BM_UnloadingDate", new DateTimeOffset(new DateTime(2023, 11, 24, 12, 0, 0)), SqlDbType.DateTimeOffset, inBondMoveHeaderPK);
			AssertFunctionReturnExpectedValue("UnloadingDate", new DateTimeOffset(new DateTime(2023, 11, 24, 12, 0, 0)));
		});

		public void TestCustomerReferenceOfTheArrival()
		{
			AssertFunctionReturnExpectedValue("CustomerReferenceOfTheArrival", "001");
		}

		public void TestConsignorOfNCTSDeparture()
		{
			var consignorB0PK = TestDataCreator.CreateOrganisation("OrgCode B0", "Company Name B0");
			var consignorBHPK = TestDataCreator.CreateOrganisation("OrgCode BH", "Company Name BH");
			var addressB0PK = TestDataCreator.CreateAddress(consignorB0PK, "Delivery Address", "Address B0");
			var addressBHPK = TestDataCreator.CreateAddress(consignorBHPK, "Delivery Address", "Address BH");
			var bh_pk = TestDataCreator.CreateCusInbondHeader("002", branchPK, "NC5", "D");
			_ = TestDataCreator.CreateCusEntryNum(bh_pk, "CusInBondHeader", "MRN001", "MRN", "CUS", "");
			var b0_pk = TestDataCreator.CreateCusInbondBill(bh_pk);

			CombineAssertions(() =>
			{
				AssertFunctionReturnExpectedValue("ConsignorOfNCTSDeparture", DBNull.Value);

				_ = TestDataCreator.CreateDocAddress(addressB0PK, "", b0_pk, "B0", "CRD");
				AssertFunctionReturnExpectedValue("ConsignorOfNCTSDeparture", "Company Name B0");

				_ = TestDataCreator.CreateDocAddress(addressBHPK, "", bh_pk, "BH", "CRD");
				AssertFunctionReturnExpectedValue("ConsignorOfNCTSDeparture", "Company Name BH");
			});
		}

		public void TestIDTransportMeans() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("IDTransportMeans", DBNull.Value);

			_ = TestDataCreator.CreateCusTransportMeans(inBondMoveHeaderPK, "BM", "Id1");
			AssertFunctionReturnExpectedValue("IDTransportMeans", "Id1");

			var inbondBillPK = TestDataCreator.CreateCusInbondBill(inBondHeaderPK);
			_ = TestDataCreator.CreateCusTransportMeans(inbondBillPK, "B0", "Id2");

			var rows = GetFilteredRows("IDTransportMeans");
			AssertEquals($"Expect 2 rows", 2, rows.Length);
			AssertContainsExactElementsInAnyOrder("2 IDTransportMeans", new string[] { "Id1", "Id2" }, rows.ToList());
		});

		public void TestEquipmentIdentifiers() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("EquipmentIdentifiers", DBNull.Value);

			_ = TestDataCreator.CreateCusInBondContainer(inBondHeaderPK, "BH", "FRNCT", "CNTR1234567");
			AssertFunctionReturnExpectedValue("EquipmentIdentifiers", "CNTR1234567");

			_ = TestDataCreator.CreateCusInBondContainer(inBondHeaderPK, "BH", "FRNCT", "CNTR1122334");
			AssertFunctionReturnExpectedValue("EquipmentIdentifiers", "CNTR1234567\nCNTR1122334");
		});

		public void TestDescriptionOfTheGoods() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("DescriptionOfTheGoods", DBNull.Value);

			var inbondBillPK = TestDataCreator.CreateCusInbondBill(inBondHeaderPK);
			var goodsPK = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc123", "NEW", 150, 200);
			AssertFunctionReturnExpectedValue("DescriptionOfTheGoods", "desc123");

			_ = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc456", "NEW", 150, 200);
			AssertFunctionReturnExpectedValue("DescriptionOfTheGoods", "desc123\ndesc456");

			var goodsBYPK = TestDataCreator.CreateCusInBondCargoDesc(goodsPK, "BY", "desc789", "NEW", 150, 200);
			UpdateCusInBondCargoDescColumn("BY_BY_Commodity", goodsBYPK, SqlDbType.UniqueIdentifier, goodsPK);
			AssertFunctionReturnExpectedValue("DescriptionOfTheGoods", "desc789\ndesc456");
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

		public void TestGoodsAndPackages() => CombineAssertions(() =>
		{
			var inbondBillPK = TestDataCreator.CreateCusInbondBill(inBondHeaderPK);
			var cargoDescPK1 = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc123", "NEW", 150, 200);
			_ = TestDataCreator.CreateCusInvPack(cargoDescPK1, "BY", "CNT", 50);
			_ = TestDataCreator.CreateCusInvPack(cargoDescPK1, "BY", "CNT", 55);
			var cargoDescPK2 = TestDataCreator.CreateCusInBondCargoDesc(inbondBillPK, "B0", "desc456", "NEW", 160, 210);
			_ = TestDataCreator.CreateCusInvPack(cargoDescPK2, "BY", "CNT", 60);
			_ = TestDataCreator.CreateCusInvPack(cargoDescPK2, "BY", "CNT", 65);

			AssertFunctionReturnExpectedValue("DescriptionOfTheGoods", "desc123\n\ndesc456");
			AssertFunctionReturnExpectedValue("NumberOfPackagesAndCodeOfPackages", "50 CNT\n55 CNT\n60 CNT\n65 CNT");
			AssertFunctionReturnExpectedValue("NetWeight", "150\n\n160");
			AssertFunctionReturnExpectedValue("GrossWeight", "200\n\n210");
		});

		readonly string dateTimeStringFormat = "dd/MM/yyyy H:mm:ss";

		public void TestSubsequentDeclarations_JI() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValueForSubsequentDeclarations(DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);

			var dateTime1 = new DateTime(2023, 11, 28, 23, 15, 00);
			SetupSubsequentDeclarations_JI(1, "MRN002", dateTime1, "A", 10, "PL", 100, "CT");
			var dateTimeString1 = dateTime1.ToString(dateTimeStringFormat);
			AssertFunctionReturnExpectedValueForSubsequentDeclarations("MRN002", dateTimeString1, "A", "10", "PL", "100", "CT");

			var dateTime2 = new DateTime(2023, 11, 20, 23, 15, 00);
			SetupSubsequentDeclarations_JI(2, "MRN012", dateTime2, "B", 11, "LP", 110, "TC");
			var dateTimeString2 = dateTime2.ToString(dateTimeStringFormat);
			AssertFunctionReturnExpectedValueForSubsequentDeclarations("MRN002\nMRN012", $"{dateTimeString1}\n{dateTimeString2}", "A\nB", "10\n11", "PL\nLP", "100\n110", "CT\nTC");
		});

		public void TestSubsequentDeclarations_BY() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValueForSubsequentDeclarations(DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);

			var dateTime1 = new DateTime(2023, 11, 27, 23, 15, 00);
			SetupSubsequentDeclarations_BY("MRN003", dateTime1, 10, "PL", 100, "CT");
			var dateTimeString1 = dateTime1.ToString(dateTimeStringFormat);
			AssertFunctionReturnExpectedValueForSubsequentDeclarations("MRN003", dateTimeString1, "D1", "10", "PL", "100", "CT");

			var dateTime2 = new DateTime(2023, 11, 19, 23, 15, 00);
			SetupSubsequentDeclarations_BY("MRN013", dateTime1, 11, "LP", 110, "TC");
			var dateTimeString2 = dateTime1.ToString(dateTimeStringFormat);
			AssertFunctionReturnExpectedValueForSubsequentDeclarations("MRN003\nMRN013", $"{dateTimeString1}\n{dateTimeString2}", "D1\nD1", "10\n11", "PL\nLP", "100\n110", "CT\nTC");
		});

		public void TestSubsequentDeclarations_ABL() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValueForSubsequentDeclarations(DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);

			var dateTime1 = new DateTime(2023, 11, 26, 23, 15, 00);
			SetupSubsequentDeclarations_ABL(1, "MRN004", dateTime1, 10, "PL", 100, "CT");
			var dateTimeString1 = dateTime1.ToString(dateTimeStringFormat);
			AssertFunctionReturnExpectedValueForSubsequentDeclarations("MRN004", dateTimeString1, "G4", "10", "PL", "100", "CT");

			var dateTime2 = new DateTime(2023, 11, 18, 23, 15, 00);
			SetupSubsequentDeclarations_ABL(2, "MRN014", dateTime2, 11, "LP", 110, "TC");
			var dateTimeString2 = dateTime2.ToString(dateTimeStringFormat);
			AssertFunctionReturnExpectedValueForSubsequentDeclarations("MRN004\nMRN014", $"{dateTimeString1}\n{dateTimeString2}", "G4\nG4", "10\n11", "PL\nLP", "100\n110", "CT\nTC");
		});

		public void TestSubsequentDeclarations() => CombineAssertions(() =>
		{
			var dateTime1 = new DateTime(2023, 11, 26, 23, 15, 00);
			SetupSubsequentDeclarations_ABL(1, "MRN004", dateTime1, 10, "X1", 100, "Y1");

			var dateTime2 = new DateTime(2023, 11, 27, 23, 15, 00);
			SetupSubsequentDeclarations_BY("MRN003", dateTime2, 11, "X2", 101, "Y2");

			var dateTime3 = new DateTime(2023, 11, 28, 23, 15, 00);
			SetupSubsequentDeclarations_JI(1, "MRN002", dateTime3, "A", 12, "X3", 102, "Y3");

			var dateTimeString1 = dateTime1.ToString(dateTimeStringFormat);
			var dateTimeString2 = dateTime2.ToString(dateTimeStringFormat);
			var dateTimeString3 = dateTime3.ToString(dateTimeStringFormat);

			AssertFunctionReturnExpectedValueForSubsequentDeclarations("MRN004\nMRN003\nMRN002", $"{dateTimeString1}\n{dateTimeString2}\n{dateTimeString3}", "G4\nD1\nA", "10\n11\n12", "X1\nX2\nX3", "100\n101\n102", "Y1\nY2\nY3");
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
			var filteredRows = GetFilteredRows(selectedColumn: "RegistryNumber", (Report_NCTSP5RegistryOfArrivalParameters.Organisation, orgHeaderPK));
			AssertContainsExactElementsInAnyOrder(new string[] { "1" }, filteredRows);
		}

		public void TestFilterByStartDate()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "RegistryNumber", (Report_NCTSP5RegistryOfArrivalParameters.StartingDate, new DateTime(2023, 11, 22)));
			AssertContainsExactElementsInAnyOrder(new string[] { "1" }, filteredRows);
		}

		public void TestFilterByRegistryNumberRangeFrom()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "RegistryNumber", (Report_NCTSP5RegistryOfArrivalParameters.RegistryNumberFrom, 1));
			AssertContainsExactElementsInAnyOrder(new string[] { "1" }, filteredRows);
		}

		public void TestFilterByRegistryNumberRangeTo()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "RegistryNumber", (Report_NCTSP5RegistryOfArrivalParameters.RegistryNumberTo, 1));
			AssertContainsExactElementsInAnyOrder(new string[] { "1" }, filteredRows);
		}

		public void TestFilterByRegistryNumberRange()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "RegistryNumber", (Report_NCTSP5RegistryOfArrivalParameters.RegistryNumberFrom, 1), (Report_NCTSP5RegistryOfArrivalParameters.RegistryNumberTo, 1));
			AssertContainsExactElementsInAnyOrder(new string[] { "1" }, filteredRows);
		}

		public void TestFilterAll()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "RegistryNumber", (Report_NCTSP5RegistryOfArrivalParameters.Organisation, orgHeaderPK), (Report_NCTSP5RegistryOfArrivalParameters.StartingDate, new DateTime(2023, 11, 22)), (Report_NCTSP5RegistryOfArrivalParameters.RegistryNumberFrom, 1), (Report_NCTSP5RegistryOfArrivalParameters.RegistryNumberTo, 1));
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
			inBondHeaderPK = TestDataCreator.CreateCusInbondHeader("001", branchPK, "NC5", "A");
			inBondMoveHeaderPK = TestDataCreator.CreateCusInBondMoveHeader(inBondHeaderPK, "A", "DRL");
			_ = TestDataCreator.CreateCusEntryNum(inBondHeaderPK, "CusInBondHeader", "MRN001", "MRN", "CUS", "");
			_ = TestDataCreator.CreateCusEntryNum(inBondMoveHeaderPK, "CusInBondMoveHeader", "1", "REG", "CUS", "", issueDate: new DateTime(2023, 11, 22), entryLineReference: "TA-BOB");
		}

		Guid companyPK;
		Guid branchPK;
		Guid orgHeaderPK;
		Guid inBondHeaderPK;
		Guid inBondMoveHeaderPK;

		void SetupSubsequentDeclarations_JI(int clusterKey, string subDecMrnNumber, DateTime subDecAcceptanceDateDocument, string subDecSubSequentProcedure, int subDecNumberOfPackages, string subDecPackageCode, int subDecGrossWeight, string subDecWeightCode)
		{
			var je_pk = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, Guid.NewGuid().ToString("n"), "IMP", clusterKey);
			var cei_pk = TestDataCreator.CreateCusEntryInstruction(je_pk, subDecSubSequentProcedure, "D", new DateTime(2023, 11, 29), clusterKey);
			var ch_pk = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "100", "", 0f, 0, "", je_pk, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), cei_pk, "", new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), clusterKey);
			var jz_pk = TestDataCreator.CreateJobComInvoiceHeader(invoiceCurrency: "EUR", defaultOrigin: "NL", invoiceNumber: "123", incoTerm: "CIP", jobDeclarationPK: je_pk, clusterKey: clusterKey);
			var cl_pk = TestDataCreator.CreateCusEntryLine(clCh: ch_pk, lineNumber: 1, dutyPercent: 2, flatAmount: 300, flatAmountUQ: "KG", clusterKey: clusterKey);
			var cc_pk = TestDataCreator.CreateCusClassification(lookupCode: $"A{clusterKey}", countryCode: "BE", classificationType: "IMP");
			var ji_pk = TestDataCreator.CreateJobComInvoiceLine(lineNo: 1, partNo: 1, description: "Invoice line", invoiceQuantity: 100, invoiceUQ: "EUR", customsQuantity: 101, customsUnitQty: "EUR", linePrice: 45, tariff: 2, jobComInvoiceHeaderPK: jz_pk, cl_pk: cl_pk, cc_pk: cc_pk, clusterKey: clusterKey, cei_pk: cei_pk);
			_ = TestDataCreator.CreateCusSupportingInfo("PRE", "", "", "MRN001", ji_pk, "JI", "BE", subDecGrossWeight, subDecWeightCode, subDecNumberOfPackages, subDecPackageCode);
			_ = TestDataCreator.CreateCusEntryNum(ch_pk, "CusEntryHeader", subDecMrnNumber, "MRN", "CUS", "", issueDate: subDecAcceptanceDateDocument);
		}

		void SetupSubsequentDeclarations_BY(string subDecMrnNumber, DateTime subDecAcceptanceDateDocument, int subDecNumberOfPackages, string subDecPackageCode, int subDecGrossWeight, string subDecWeightCode)
		{
			var bh_pk = TestDataCreator.CreateCusInbondHeader("002", branchPK, "NC5", "D");
			var b0_pk = TestDataCreator.CreateCusInbondBill(bh_pk);
			var by_pk = TestDataCreator.CreateCusInBondCargoDesc(b0_pk, "B0", "", "", 0, 0);
			_ = TestDataCreator.CreateCusSupportingInfo("PRE", "", "", "MRN001", by_pk, "BY", "BE", subDecGrossWeight, subDecWeightCode, subDecNumberOfPackages, subDecPackageCode);
			_ = TestDataCreator.CreateCusEntryNum(bh_pk, "CusInBondHeader", subDecMrnNumber, "MRN", "CUS", "", issueDate: subDecAcceptanceDateDocument);
		}

		void SetupSubsequentDeclarations_ABL(int clusterKey, string subDecMrnNumber, DateTime subDecAcceptanceDateDocument, int subDecNumberOfPackages, string subDecPackageCode, int subDecGrossWeight, string subDecWeightCode)
		{
			var ama_pk = TestDataCreator.CreateAsycudaManifestHeader(branchPK, $"TS{clusterKey}", "BE", clusterKey);
			var abl_pk = TestDataCreator.CreateAsycudaBill(ama_pk, clusterKey);
			_ = TestDataCreator.CreateCusSupportingInfo("PRE", "", "", "MRN001", abl_pk, "ABL", "BE", subDecGrossWeight, subDecWeightCode, subDecNumberOfPackages, subDecPackageCode);
			_ = TestDataCreator.CreateCusEntryNum(ama_pk, "AsycudaManifestHeader", subDecMrnNumber, "MRN", "CUS", "", issueDate: subDecAcceptanceDateDocument);
		}

		protected override IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters()
		{
			yield return (SqlDbType.UniqueIdentifier, Report_NCTSP5RegistryOfArrivalParameters.Organisation);
			yield return (SqlDbType.SmallDateTime, Report_NCTSP5RegistryOfArrivalParameters.StartingDate);
			yield return (SqlDbType.VarChar, Report_NCTSP5RegistryOfArrivalParameters.RegistryNumberFrom);
			yield return (SqlDbType.VarChar, Report_NCTSP5RegistryOfArrivalParameters.RegistryNumberTo);
		}

		class Report_NCTSP5RegistryOfArrivalParameters
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

		#endregion
	}
}
