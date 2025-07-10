using System;
using System.Collections.Generic;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZA;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZA.Testing
{
	[TestedType(typeof(Report_ZACustomsEntryHeader))]
	class Report_ZACustomsEntryHeaderTest : DbCreateScriptTest
	{
		public void TestFunctionalityDbFunction()
		{
			var gc_pk = TestDataCreator.CreateCompany("DZ1", "ZA", "ZAR");
			var gb_pk = TestDataCreator.CreateBranch(gc_pk, "GB1", "");

			var je_importer = TestDataCreator.CreateOrganisation("JEIMPORTER", "JE IMPORTER NAME");
			var je_supplier = TestDataCreator.CreateOrganisation("JESUPPLIER", "JE SUPPLIER NAME");
			var cei_warehouse = TestDataCreator.CreateOrganisation("CEIW", "CEI WAREHOUSE NAME");
			var cei_warehouse2 = TestDataCreator.CreateOrganisation("CEIW2", "CEI WAREHOUSE2 NAME");
			var cei_carrier = TestDataCreator.CreateOrganisation("CEICAR", "CEI CARRIER NAME");
			var cei_bondHolder = TestDataCreator.CreateOrganisation("CEIBH", "CEI BOND HOLDER NAME");
			var cei_ohOwner = TestDataCreator.CreateOrganisation("CEIOH", "CEI OH OWNER NAME");

			var add1_pk = TestDataCreator.CreateAddress(cei_warehouse, "CEIWA", "CEI WAREHOUSE ADDRESS");
			var add2_pk = TestDataCreator.CreateAddress(cei_warehouse2, "CEIW2", "CEI WAREHOUSE2 ADDRESS");

			var group_pk = TestDataCreator.CreateRefDatabaseRefDataGrouping("ZIP", "ZZZ Data Group 1");
			var type_pk_duty = TestDataCreator.CreateRefDbEntZZRefCusRateType("DTY", "Cus Rate Type 1", "ZIP");
			var type_pk_s1p2b = TestDataCreator.CreateRefDbEntZZRefCusRateType("EX1", "Cus Rate Type 2", "ZIP");

			var type_pk_pp = TestDataCreator.CreateRefDbEntZZRefCusRateType("PRP", "Cus Rate Type 3", "ZIP");
			var type_pk_tariff = TestDataCreator.CreateRefDbEntZZRefCusTariffType("1P1", "Schedule 1 Part 1", "ZA", "ZA");

			var type_pk_tariff_3P1 = TestDataCreator.CreateRefDbEntZZRefCusTariffType("3P1", "Schedule 3 Part 1", "ZA", "ZA");
			var type_pk_tariff_3P2 = TestDataCreator.CreateRefDbEntZZRefCusTariffType("3P2", "Schedule 3 Part 2", "ZA", "ZA");
			var type_pk_tariff_4P1 = TestDataCreator.CreateRefDbEntZZRefCusTariffType("4P1", "Schedule 4 Part 1", "ZA", "ZA");
			var type_pk_tariff_4P2 = TestDataCreator.CreateRefDbEntZZRefCusTariffType("4P2", "Schedule 4 Part 2", "ZA", "ZA");
			var type_pk_tariff_4P3 = TestDataCreator.CreateRefDbEntZZRefCusTariffType("4P3", "Schedule 4 Part 3", "ZA", "ZA");
			var type_pk_tariff_4P4 = TestDataCreator.CreateRefDbEntZZRefCusTariffType("4P4", "Schedule 4 Part 4", "ZA", "ZA");
			var type_pk_tariff_4P5 = TestDataCreator.CreateRefDbEntZZRefCusTariffType("4P5", "Schedule 4 Part 5", "ZA", "ZA");
			var type_pk_tariff_4P6 = TestDataCreator.CreateRefDbEntZZRefCusTariffType("4P6", "Schedule 4 Part 6", "ZA", "ZA");

			var clusterKey = 1;

			var je_pk = TestDataCreator.CreateJobDeclaration(
				declarationReference: "BUS100TEST",
				branchPK: gb_pk,
				companyPK: gc_pk,
				applicationCode: "BLT",
				importer: je_importer,
				supplier: je_supplier,

				customsOffice: "JHB",
				masterBill: "OOCL12345678",
				portOfLoading: "CATOR",
				portOfArrival: "ZAJNB",
				transportMode: "AIR",

				voyageFlightNo: "SA123",
				houseBill: "S00049567",
				createTime: new DateTime(2017, 1, 1, 12, 13, 0),
				messageType: "IMP",

				clusterKey: clusterKey
				);
			var cei_pk = TestDataCreator.CreateCusEntryInstruction(
				style: "40",
				je: je_pk,
				oaWarehouse: add1_pk,
				description: "Entry Instruction Description",
				addInfo: "CreditTerms=NEP*PortOfExit=CTN*PreviousMRN=DFM201609225000601*UCROrderNumber=CPC6211SEATEST",
				ohBondHolder: cei_bondHolder,
				ohCarrier: cei_carrier,
				oaWareHouse2: add2_pk,
				ohOwner: cei_ohOwner,
				assessmentDate: new DateTime(2016, 09, 22),
				clusterKey: clusterKey
			);
			var ch_pk = TestDataCreator.CreateCusEntryHeader(
				isValid: true,
				messageType: "IMP",
				status: "CSA",
				entryStatus: "CEO",
				bgmReference: "00505655JSA20170214004008",

				totalPaid: 470.33f,
				linenum: 1,
				addInfo: "Test=test*Packages=5*test1=test1*RelPrintInd=Y",
				jePk: je_pk,
				entrySubmittedDate: new DateTime(2017, 1, 2, 0, 11, 0),

				entryReleaseDate: new DateTime(2017, 1, 3, 0, 12, 0),
				instruction: cei_pk,
				warehouseTransactionStatus: "IUP",
				warehouseReleaseDate: new DateTime(2017, 1, 4, 0, 14, 0),
				bondAcquittedDate: new DateTime(2017, 1, 5),
				bondValidToDate: new DateTime(2017, 1, 6),

				clusterKey: clusterKey
			);

			var l1pk = TestDataCreator.CreateCusEntryLine(ch_pk, clusterKey);
			var l2pk = TestDataCreator.CreateCusEntryLine(ch_pk, clusterKey);
			var l3pk = TestDataCreator.CreateCusEntryLine(ch_pk, clusterKey);
			var l4pk = TestDataCreator.CreateCusEntryLine(ch_pk, clusterKey);
			var l5pk = TestDataCreator.CreateCusEntryLine(ch_pk, clusterKey);

			var cuscode1PK = TestDataCreator.CreateCusCodeData("ADI", "BND", "1", l1pk, "CL");
			var cuscode2PK = TestDataCreator.CreateCusCodeData("ADI", "BND", "A", l2pk, "CL");
			var cuscode3PK = TestDataCreator.CreateCusCodeData("ADI", "BND", "0", l3pk, "CL");
			var cuscode4PK = TestDataCreator.CreateCusCodeData("ADI", "BND", "100", l4pk, "CL");

			var fee1Pk = TestDataCreator.CreateCusEntryLineFee(l1pk, "1P1", 123.00f, clusterKey);
			var fee2Pk = TestDataCreator.CreateCusEntryLineFee(l2pk, "EX1", 456.00f, clusterKey);
			var fee3Pk = TestDataCreator.CreateCusEntryLineFee(l3pk, "PRP", 789.00f, clusterKey);
			var fee4Pk = TestDataCreator.CreateCusEntryLineFee(l4pk, "VAT", 600.00f, clusterKey);

			var fee3P1 = TestDataCreator.CreateCusEntryLineFee(l5pk, "3P1", 10.00f, clusterKey);
			var fee3P2 = TestDataCreator.CreateCusEntryLineFee(l5pk, "3P2", 10.00f, clusterKey);
			var fee4P1 = TestDataCreator.CreateCusEntryLineFee(l5pk, "4P1", 10.00f, clusterKey);
			var fee4P2 = TestDataCreator.CreateCusEntryLineFee(l5pk, "4P2", 10.00f, clusterKey);
			var fee4P3 = TestDataCreator.CreateCusEntryLineFee(l5pk, "4P3", 10.00f, clusterKey);
			var fee4P4 = TestDataCreator.CreateCusEntryLineFee(l5pk, "4P4", 10.00f, clusterKey);
			var fee4P5 = TestDataCreator.CreateCusEntryLineFee(l5pk, "4P5", 10.00f, clusterKey);
			var fee4P6 = TestDataCreator.CreateCusEntryLineFee(l5pk, "4P6", 10.00f, clusterKey);

			var groupPK = TestDataCreator.CreateRefDatabaseRefDataGrouping("ZA", "sssss");

			var type1PK = TestDataCreator.CreateRefCusRateType("1P1", "1P11111", 0, "ZA", "");
			var type2PK = TestDataCreator.CreateRefCusRateType("EX1", "EX111", 0, "ZA", "");

			var typeREB = TestDataCreator.CreateRefCusRateType("REB", "Rebate", 0, "ZA", "");

			var ratecode1PK = TestDataCreator.CreateRefCusRateCode("1P1", type1PK, "Schedule 1 Part 1");
			var ratecode1PK2 = TestDataCreator.CreateRefCusRateCode("EX1", type2PK, "ex111");

			var ratecode3P1 = TestDataCreator.CreateRefCusRateCode("3P1", typeREB, "Schedule 3 Part 1");
			var ratecode3P2 = TestDataCreator.CreateRefCusRateCode("3P2", typeREB, "Schedule 3 Part 2");
			var ratecode4P1 = TestDataCreator.CreateRefCusRateCode("4P1", typeREB, "Schedule 4 Part 1");
			var ratecode4P2 = TestDataCreator.CreateRefCusRateCode("4P2", typeREB, "Schedule 4 Part 2");
			var ratecode4P3 = TestDataCreator.CreateRefCusRateCode("4P3", typeREB, "Schedule 4 Part 3");
			var ratecode4P4 = TestDataCreator.CreateRefCusRateCode("4P4", typeREB, "Schedule 4 Part 4");
			var ratecode4P5 = TestDataCreator.CreateRefCusRateCode("4P5", typeREB, "Schedule 4 Part 5");
			var ratecode4P6 = TestDataCreator.CreateRefCusRateCode("4P6", typeREB, "Schedule 4 Part 6");

			var ce1Pk = TestDataCreator.CreateCusEntryNum(ch_pk, "CusEntryHeader", "AAAANGWMT", "MRN", "CUS", "ZA");
			var ce2Pk = TestDataCreator.CreateCusEntryNum(ch_pk, "CusEntryHeader", "UCRFROMCUSENTRYNUMBER00001", "UCR", "CUS", "ZA");

			using (var command = CargoWise.Data.Db.Connection.Command(
				$@"SELECT * FROM Report_ZACustomsEntryHeader ('{gc_pk}', 'IMP', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)"))
			{
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						AssertEquals("Result not empty", true, reader.Read());

						AssertEquals("JobNumber", "BUS100TEST", reader["JobNumber"]);
						AssertEquals("EntryNumber", "AAAANGWMT", reader["EntryNumber"]);
						AssertEquals("LRN", "00505655JSA20170214004008", reader["LRN"]);
						AssertEquals("EntryStatus", "CEO", reader["EntryStatus"]);
						AssertEquals("ProcedureCode", "40", reader["ProcedureCode"]);

						AssertEquals("EntryInstructionDescription", "Entry Instruction Description", reader["EntryInstructionDescription"]);
						AssertEquals("ImporterCode", "JEIMPORTER", reader["ImporterCode"]);
						AssertEquals("ImporterName", "JE IMPORTER NAME", reader["ImporterName"]);
						AssertEquals("SupplierCode", "JESUPPLIER", reader["SupplierCode"]);
						AssertEquals("SupplierName", "JE SUPPLIER NAME", reader["SupplierName"]);

						AssertEquals("TotalDuty", 123.00m, reader["TotalDuty"]);
						AssertEquals("Total1p2B", 456.00m, reader["TotalS1p2B"]);
						AssertEquals("TotalPpPen", 789.00m, reader["TotalPpPen"]);
						AssertEquals("TotalVat", 600m, reader["TotalVat"]);
						AssertEquals("NoOfPkgs", "5", reader["NoOfPkgs"]);

						AssertEquals("NoOfLines", 5, reader["NoOfLines"]);
						AssertEquals("ShipmentType", "IMP", reader["ShipmentType"]);
						AssertEquals("CustomsOffice", "JHB", reader["CustomsOffice"]);
						AssertEquals("MasterBill", "OOCL12345678", reader["MasterBill"]);
						AssertEquals("CountryOfExit", "CATOR", reader["CountryOfExit"]);

						AssertEquals("CountryOfDestination", "ZAJNB", reader["CountryOfDestination"]);
						AssertEquals("OfficeOfExit", "CTN", reader["OfficeOfExit"]);
						AssertEquals("Transport", "AIR", reader["Transport"]);
						AssertEquals("VoyFlight", "SA123", reader["VoyFlight"]);
						AssertEquals("BondAcquittedDate", new DateTime(2017, 1, 5), reader["BondAcquittedDate"]);

						AssertEquals("BondValidToDate", new DateTime(2017, 1, 6), reader["BondValidToDate"]);
						AssertEquals("EntrySubmittedDate", new DateTime(2017, 1, 2, 0, 11, 0), reader["EntrySubmittedDate"]);
						AssertEquals("EntryReleaseDate", new DateTime(2017, 1, 3, 0, 12, 0), reader["EntryReleaseDate"]);
						AssertEquals("WarehouseReleaseDate", new DateTime(2017, 1, 4, 0, 14, 0), reader["WarehouseReleaseDate"]);
						AssertEquals("WarehouseTransactionStatus", "IUP", reader["WarehouseTransactionStatus"]);

						AssertEquals("PreviousMRN", "DFM201609225000601", reader["PreviousMRN"]);
						AssertEquals("UcrNo", "UCRFROMCUSENTRYNUMBER00001", reader["UcrNo"]);
						AssertEquals("AssessmentDate", new DateTime(2016, 9, 22), reader["AssessmentDate"]);
						AssertEquals("FromWarehouseCode", "CEIW", reader["FromWarehouseCode"]);
						AssertEquals("FromWarehouseName", "CEI WAREHOUSE NAME", reader["FromWarehouseName"]);

						AssertEquals("ToWarehouseCode", "CEIW2", reader["ToWarehouseCode"]);
						AssertEquals("ToWarehouseName", "CEI WAREHOUSE2 NAME", reader["ToWarehouseName"]);
						AssertEquals("BondHolderCode", "CEIBH", reader["BondHolderCode"]);
						AssertEquals("BondHolderName", "CEI BOND HOLDER NAME", reader["BondHolderName"]);
						AssertEquals("RemoverCode", "CEICAR", reader["RemoverCode"]);

						AssertEquals("RemoverName", "CEI CARRIER NAME", reader["RemoverName"]);
						AssertEquals("NewWhOwnerCode", "CEIOH", reader["NewWhOwnerCode"]);
						AssertEquals("NewWhOwnerName", "CEI OH OWNER NAME", reader["NewWhOwnerName"]);
						AssertEquals("HouseBill", "S00049567", reader["HouseBill"]);
						AssertEquals("JobRegisteredDate", new DateTime(2017, 1, 1, 12, 13, 0), reader["JobRegisteredDate"]);

						AssertEquals("BranchPK", gb_pk, reader["BranchPK"]);
						AssertEquals("ImporterPk", je_importer, reader["ImporterPk"]);
						AssertEquals("SupplierPk", je_supplier, reader["SupplierPk"]);

						AssertEquals("RelPrintInd", "Y", reader["RelPrintInd"]);

						AssertEquals("BondAcquittalValue", 101m, reader["BondAcquittalValue"]);
					});
				}
			}
		}

		public void TestReport_ZACustomsEntryHeader_OverrideCustomsOffice()
		{
			var gc_pk = TestDataCreator.CreateCompany("AR1", "ZA", "ZAR");
			var gb_pk = TestDataCreator.CreateBranch(gc_pk, "GB1", "");
			var je_importer = TestDataCreator.CreateOrganisation("ACMEIMPORT", "IMPORTER NAME");
			var je_supplier = TestDataCreator.CreateOrganisation("ACMESUPPLIER", "SUPPLIER NAME");

			var je1_pk = TestDataCreator.CreateJobDeclaration("MSGSTATUSTEST001", gb_pk, gc_pk, "BLT", je_importer, je_supplier, "CTN", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, new DateTime(2022, 5, 12, 17, 0, 0), "IMP", 1);
			var cei1_pk = TestDataCreator.CreateCusEntryInstruction(je1_pk, "11", "Instruction Description1", new DateTime(2022, 05, 12), 1, "CustomsOfficeOverride=");
			var ch1_pk = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "CEO", 1, je1_pk, new DateTime(2022, 5, 12, 18, 30, 0), new DateTime(2022, 5, 12, 19, 30, 0), cei1_pk, new DateTime(2022, 5, 9), 1);
			var l1pk = TestDataCreator.CreateCusEntryLine(ch1_pk, 1);

			var je2_pk = TestDataCreator.CreateJobDeclaration("MSGSTATUSTEST002", gb_pk, gc_pk, "BLT", je_importer, je_supplier, "JHB", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, new DateTime(2022, 5, 12, 17, 0, 0), "IMP", 2);
			var cei2_pk = TestDataCreator.CreateCusEntryInstruction(je2_pk, "22", "Instruction Description2", new DateTime(2022, 05, 12), 1, "CustomsOfficeOverride=CTN");
			var ch2_pk = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "CEO", 1, je2_pk, new DateTime(2022, 5, 12, 18, 30, 0), new DateTime(2022, 5, 12, 19, 30, 0), cei2_pk, new DateTime(2022, 5, 9), 2);
			var l2pk = TestDataCreator.CreateCusEntryLine(ch2_pk, 2);

				var results = new List<string>();
				var sql = $"SELECT * FROM Report_ZACustomsEntryHeader ('{gc_pk}', 'IMP', NULL, 'CTN', '2022-05-01', '2022-05-15', NULL, NULL, NULL, NULL, NULL, NULL, 'NOT', NULL, NULL, NULL)";
				CargoWise.Data.Db.Connection.ExecuteReader(sql, reader =>
				{
					results.Add($"{reader["JobNumber"]}, {reader["CustomsOffice"]}");
				});
				AssertContainsExactElementsInAnyOrder("The declarations with CustomsOfficeOverride = CTN or empty CustomsOfficeOverride but JE_CustomsOffice = CTN", new[] { "MSGSTATUSTEST001, CTN", "MSGSTATUSTEST002, CTN" }, results);
		}

		public void TestReport_ZACustomsEntryHeader_TakesDataGroupingIntoAccount()
		{
			var gc_pk = TestDataCreator.CreateCompany("DZ1", "ZA", "ZAR");
			var gb_pk = TestDataCreator.CreateBranch(gc_pk, "GB1", "");

			var je_importer = TestDataCreator.CreateOrganisation("JEIMPORTER", "JE IMPORTER NAME");
			var je_supplier = TestDataCreator.CreateOrganisation("JESUPPLIER", "JE SUPPLIER NAME");
			var cei_warehouse = TestDataCreator.CreateOrganisation("CEIW", "CEI WAREHOUSE NAME");
			var cei_warehouse2 = TestDataCreator.CreateOrganisation("CEIW2", "CEI WAREHOUSE2 NAME");
			var cei_carrier = TestDataCreator.CreateOrganisation("CEICAR", "CEI CARRIER NAME");
			var cei_bondHolder = TestDataCreator.CreateOrganisation("CEIBH", "CEI BOND HOLDER NAME");
			var cei_ohOwner = TestDataCreator.CreateOrganisation("CEIOH", "CEI OH OWNER NAME");

			var add1_pk = TestDataCreator.CreateAddress(cei_warehouse, "CEIWA", "CEI WAREHOUSE ADDRESS");
			var add2_pk = TestDataCreator.CreateAddress(cei_warehouse2, "CEIW2", "CEI WAREHOUSE2 ADDRESS");

			var group_pk = TestDataCreator.CreateRefDatabaseRefDataGrouping("ZIP", "ZZZ Data Group 1");
			var type_pk_duty = TestDataCreator.CreateRefDbEntZZRefCusRateType("DTY", "Cus Rate Type 1", "ZIP");
			var type_pk_s1p2b = TestDataCreator.CreateRefDbEntZZRefCusRateType("EX1", "Cus Rate Type 2", "ZIP");
			var type_pk_pp = TestDataCreator.CreateRefDbEntZZRefCusRateType("PRP", "Cus Rate Type 3", "ZIP");
			var type_pk_tariff = TestDataCreator.CreateRefDbEntZZRefCusTariffType("1P1", "Schedule 1 Part 1", "ZA", "ZA");

			var clusterKey = 1;

			var je_pk = TestDataCreator.CreateJobDeclaration(
				declarationReference: "BUS100TEST",
				branchPK: gb_pk,
				companyPK: gc_pk,
				applicationCode: "BLT",
				importer: je_importer,
				supplier: je_supplier,

				customsOffice: "JHB",
				masterBill: "OOCL12345678",
				portOfLoading: "CATOR",
				portOfArrival: "ZAJNB",
				transportMode: "AIR",

				voyageFlightNo: "SA123",
				houseBill: "S00049567",
				createTime: new DateTime(2017, 1, 1, 12, 13, 0),
				messageType: "IMP",

				clusterKey: clusterKey
				);
			var cei_pk = TestDataCreator.CreateCusEntryInstruction(
				style: "11",
				je: je_pk,
				oaWarehouse: add1_pk,
				description: "Entry Instruction Description",
				addInfo: "CreditTerms=NEP*PortOfExit=CTN*PreviousMRN=DFM201609225000601*UCROrderNumber=CPC6211SEATEST",
				ohBondHolder: cei_bondHolder,
				ohCarrier: cei_carrier,
				oaWareHouse2: add2_pk,
				ohOwner: cei_ohOwner,
				assessmentDate: new DateTime(2016, 09, 22),
				clusterKey: clusterKey
			);
			var ch_pk = TestDataCreator.CreateCusEntryHeader(
				isValid: true,
				messageType: "IMP",
				status: "CSA",
				entryStatus: "CEO",
				bgmReference: "00505655JSA20170214004008",

				totalPaid: 18006.90f,
				linenum: 1,
				addInfo: "Test=test*Packages=5*test1=test1*RelPrintInd=Y",
				jePk: je_pk,
				entrySubmittedDate: new DateTime(2017, 1, 2, 0, 11, 0),

				entryReleaseDate: new DateTime(2017, 1, 3, 0, 12, 0),
				instruction: cei_pk,
				warehouseTransactionStatus: "IUP",
				warehouseReleaseDate: new DateTime(2017, 1, 4, 0, 14, 0),
				bondAcquittedDate: new DateTime(2017, 1, 5),
				bondValidToDate: new DateTime(2017, 1, 6),

				clusterKey: clusterKey
			);

			var ch2_pk = TestDataCreator.CreateCusEntryHeader(
				isValid: true,
				messageType: "IMP",
				status: "CSA",
				entryStatus: "CEO",
				bgmReference: "00505655JSA20170214004008",

				totalPaid: 18006.90f,
				linenum: 1,
				addInfo: "Test=test*Packages=5*test1=test1*RelPrintInd=Y",
				jePk: je_pk,
				entrySubmittedDate: new DateTime(2017, 1, 2, 0, 11, 0),

				entryReleaseDate: new DateTime(2017, 1, 3, 0, 12, 0),
				instruction: cei_pk,
				warehouseTransactionStatus: "IUP",
				warehouseReleaseDate: new DateTime(2017, 1, 4, 0, 14, 0),
				bondAcquittedDate: new DateTime(2017, 1, 5),
				bondValidToDate: new DateTime(2017, 1, 6),

				clusterKey: 2
			);

			var l1pk = TestDataCreator.CreateCusEntryLine(ch_pk, clusterKey);
			var l2pk = TestDataCreator.CreateCusEntryLine(ch_pk, clusterKey);
			var l4pk = TestDataCreator.CreateCusEntryLine(ch_pk, clusterKey);
			var polishLinePK = TestDataCreator.CreateCusEntryLine(ch2_pk, 2);

			var cuscode1PK = TestDataCreator.CreateCusCodeData("ADI", "BND", "10", l1pk, "CL");

			var fee1Pk = TestDataCreator.CreateCusEntryLineFee(l1pk, "1P1", 18000.00f, clusterKey);
			var fee2Pk = TestDataCreator.CreateCusEntryLineFee(l2pk, "13E", 6.9f, clusterKey);
			var fee4Pk = TestDataCreator.CreateCusEntryLineFee(l4pk, "VAT", 22501.05f, clusterKey);
			var polishFee1Pk = TestDataCreator.CreateCusEntryLineFee(polishLinePK, "1P1", 18000.00f, 2);

			var groupPK = TestDataCreator.CreateRefDatabaseRefDataGrouping("ZA", "South Africa");
			var polishGroupPK = TestDataCreator.CreateRefDatabaseRefDataGrouping("PL", "Poland");

			var type1P1PK = TestDataCreator.CreateRefCusRateType("DTY", "ZA Duty rate type", 0, "ZA", "");
			var polishType1P1PK = TestDataCreator.CreateRefCusRateType("SPL", "Polish rate type", 0, "PL", "");
			var typeLVYPK = TestDataCreator.CreateRefCusRateType("LVY", "ZA Levy rate type", 0, "ZA", "");

			var rateCode1P1PK = TestDataCreator.CreateRefCusRateCode("1P1", type1P1PK, "1P1 rate code");
			var polishRateCode1P1PK = TestDataCreator.CreateRefCusRateCode("1P1", polishType1P1PK, "Polish rate code");
			var rateCode13EPK = TestDataCreator.CreateRefCusRateCode("13E", typeLVYPK, "13E rate code");

			using (var command = CargoWise.Data.Db.Connection.Command(
				$@"SELECT * FROM Report_ZACustomsEntryHeader ('{gc_pk}', 'IMP', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)"))
			{
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						AssertEquals("Result not empty", true, reader.Read());
						AssertEquals("JobNumber", "BUS100TEST", reader["JobNumber"]);
						AssertEquals("TotalDuty", 18006.9m, reader["TotalDuty"]);
						AssertEquals("TotalVat", 22501.05m, reader["TotalVat"]);
					});
				}
			}
		}

		public void TestReport_ZACustomsEntryHeader_MessageSatus()
		{
			var gc_pk = TestDataCreator.CreateCompany("AR1", "ZA", "ZAR");
			var gb_pk = TestDataCreator.CreateBranch(gc_pk, "GB1", "");
			var je_importer = TestDataCreator.CreateOrganisation("ACMEIMPORT", "IMPORTER NAME");
			var je_supplier = TestDataCreator.CreateOrganisation("ACMESUPPLIER", "SUPPLIER NAME");
			var cei_bondHolder = TestDataCreator.CreateOrganisation("CEIBH", "CEI BOND HOLDER NAME");
			var cei_carrier = TestDataCreator.CreateOrganisation("CEICAR", "CEI CARRIER NAME");
			var cei_warehouse1 = TestDataCreator.CreateOrganisation("CEIW", "CEI WAREHOUSE NAME");
			var add1_pk = TestDataCreator.CreateAddress(cei_warehouse1, "CEIWA", "CEI WAREHOUSE ADDRESS");
			var cei_warehouse2 = TestDataCreator.CreateOrganisation("CEIW2", "CEI WAREHOUSE2 NAME");
			var add2_pk = TestDataCreator.CreateAddress(cei_warehouse2, "CEIW2", "CEI WAREHOUSE2 ADDRESS");
			var cei_ohOwner = TestDataCreator.CreateOrganisation("CEIOH", "CEI OH OWNER NAME");
			var cusEntryHeaderStatus = string.Empty;
			var shipmentType = "IMP";
			var dateFrom = "2022-05-01";
			var dateTo = "2022-05-15";
			var messageStatus = "NOT";

			var je1_pk = TestDataCreator.CreateJobDeclaration("MSGSTATUSTEST001", gb_pk, gc_pk, "BLT", je_importer, je_supplier, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
				new DateTime(2022, 4, 12, 17, 0, 0), "IMP", 1);
			var cei1_pk = TestDataCreator.CreateCusEntryInstruction("11", je1_pk, add1_pk, "Entry Instruction Description", string.Empty, cei_bondHolder, cei_carrier, add2_pk, cei_ohOwner, new DateTime(2022, 05, 12), 1);
			var ch1_pk = TestDataCreator.CreateCusEntryHeader(true, "IMP", cusEntryHeaderStatus, "CEO", 1, je1_pk, new DateTime(2022, 5, 12, 18, 30, 0), new DateTime(2022, 5, 12, 19, 30, 0), cei1_pk, new DateTime(2022, 4, 12), 1);
			var l1pk = TestDataCreator.CreateCusEntryLine(ch1_pk, 1);

			var je2_pk = TestDataCreator.CreateJobDeclaration("MSGSTATUSTEST002", gb_pk, gc_pk, "BLT", je_importer, je_supplier, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
				new DateTime(2022, 5, 12, 17, 0, 0), "IMP", 2);
			var cei2_pk = TestDataCreator.CreateCusEntryInstruction("11", je2_pk, add1_pk, "Entry Instruction Description", string.Empty, cei_bondHolder, cei_carrier, add2_pk, cei_ohOwner, new DateTime(2022, 05, 12), 2);
			var ch2_pk = TestDataCreator.CreateCusEntryHeader(true, "IMP", cusEntryHeaderStatus, "CEO", 1, je2_pk, new DateTime(2022, 5, 12, 18, 30, 0), new DateTime(2022, 5, 12, 19, 30, 0), cei2_pk, new DateTime(2022, 5, 9), 2);
			var l2pk = TestDataCreator.CreateCusEntryLine(ch2_pk, 2);

			using (var command = CargoWise.Data.Db.Connection.Command($@"SELECT * FROM Report_ZACustomsEntryHeader ('{gc_pk}', '{shipmentType}', NULL, NULL, '{dateFrom}', '{dateTo}', NULL, NULL, NULL, NULL, NULL, NULL, '{messageStatus}', NULL, NULL, NULL)"))
			{
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						AssertEquals("Prerequisite: @MessageStatus must be NOT", "NOT", messageStatus);
						AssertEquals("Prerequisite: CusEntryHeader.CH_Status must be empty", string.Empty, cusEntryHeaderStatus);
						AssertEquals("Result not empty", true, reader.Read());
						AssertEquals("JobNumber", "MSGSTATUSTEST002", reader["JobNumber"]);
					});
				}
			}
		}

		public void TestReport_ZACustomsEntryHeader_FiltersOnEndDateCorrectly()
		{
			var gc_pk = TestDataCreator.CreateCompany("DZ1", "ZA", "ZAR");
			var gb_pk = TestDataCreator.CreateBranch(gc_pk, "GB1", "");

			var je_importer = TestDataCreator.CreateOrganisation("JEIMPORTER", "JE IMPORTER NAME");
			var je_supplier = TestDataCreator.CreateOrganisation("JESUPPLIER", "JE SUPPLIER NAME");
			var cei_warehouse = TestDataCreator.CreateOrganisation("CEIW", "CEI WAREHOUSE NAME");
			var cei_warehouse2 = TestDataCreator.CreateOrganisation("CEIW2", "CEI WAREHOUSE2 NAME");
			var cei_carrier = TestDataCreator.CreateOrganisation("CEICAR", "CEI CARRIER NAME");
			var cei_bondHolder = TestDataCreator.CreateOrganisation("CEIBH", "CEI BOND HOLDER NAME");
			var cei_ohOwner = TestDataCreator.CreateOrganisation("CEIOH", "CEI OH OWNER NAME");

			var add1_pk = TestDataCreator.CreateAddress(cei_warehouse, "CEIWA", "CEI WAREHOUSE ADDRESS");
			var add2_pk = TestDataCreator.CreateAddress(cei_warehouse2, "CEIW2", "CEI WAREHOUSE2 ADDRESS");

			var group_pk = TestDataCreator.CreateRefDatabaseRefDataGrouping("ZIP", "ZZZ Data Group 1");
			var type_pk_duty = TestDataCreator.CreateRefDbEntZZRefCusRateType("DTY", "Cus Rate Type 1", "ZIP");
			var type_pk_s1p2b = TestDataCreator.CreateRefDbEntZZRefCusRateType("EX1", "Cus Rate Type 2", "ZIP");
			var type_pk_pp = TestDataCreator.CreateRefDbEntZZRefCusRateType("PRP", "Cus Rate Type 3", "ZIP");
			var type_pk_tariff = TestDataCreator.CreateRefDbEntZZRefCusTariffType("1P1", "Schedule 1 Part 1", "ZA", "ZA");

			var clusterKey = 1;

			var je_pk = TestDataCreator.CreateJobDeclaration(
				declarationReference: "BUS100TEST",
				branchPK: gb_pk,
				companyPK: gc_pk,
				applicationCode: "BLT",
				importer: je_importer,
				supplier: je_supplier,

				customsOffice: "JHB",
				masterBill: "OOCL12345678",
				portOfLoading: "CATOR",
				portOfArrival: "ZAJNB",
				transportMode: "AIR",

				voyageFlightNo: "SA123",
				houseBill: "S00049567",
				createTime: new DateTime(2017, 1, 1, 12, 13, 0),
				messageType: "IMP",

				clusterKey: clusterKey
				);
			var cei_pk = TestDataCreator.CreateCusEntryInstruction(
				style: "11",
				je: je_pk,
				oaWarehouse: add1_pk,
				description: "Entry Instruction Description",
				addInfo: "CreditTerms=NEP*PortOfExit=CTN*PreviousMRN=DFM201609225000601*UCROrderNumber=CPC6211SEATEST",
				ohBondHolder: cei_bondHolder,
				ohCarrier: cei_carrier,
				oaWareHouse2: add2_pk,
				ohOwner: cei_ohOwner,
				assessmentDate: new DateTime(2016, 09, 22),
				clusterKey: clusterKey
			);
			var ch_pk = TestDataCreator.CreateCusEntryHeader(
				isValid: true,
				messageType: "IMP",
				status: "CSA",
				entryStatus: "CEO",
				bgmReference: "00505655JSA20170214004008",

				totalPaid: 18006.90f,
				linenum: 1,
				addInfo: "Test=test*Packages=5*test1=test1*RelPrintInd=Y",
				jePk: je_pk,
				entrySubmittedDate: new DateTime(2017, 1, 2, 0, 0, 0),

				entryReleaseDate: new DateTime(2017, 1, 3, 0, 12, 0),
				instruction: cei_pk,
				warehouseTransactionStatus: "IUP",
				warehouseReleaseDate: new DateTime(2017, 1, 4, 0, 14, 0),
				bondAcquittedDate: new DateTime(2017, 1, 5),
				bondValidToDate: new DateTime(2017, 1, 6),

				clusterKey: clusterKey
			);

			var ch2_pk = TestDataCreator.CreateCusEntryHeader(
				isValid: true,
				messageType: "IMP",
				status: "CSA",
				entryStatus: "CEO",
				bgmReference: "00505655JSA20170214004008",

				totalPaid: 18006.90f,
				linenum: 1,
				addInfo: "Test=test*Packages=5*test1=test1*RelPrintInd=Y",
				jePk: je_pk,
				entrySubmittedDate: new DateTime(2017, 1, 2, 0, 0, 0),

				entryReleaseDate: new DateTime(2017, 1, 3, 0, 12, 0),
				instruction: cei_pk,
				warehouseTransactionStatus: "IUP",
				warehouseReleaseDate: new DateTime(2017, 1, 4, 0, 14, 0),
				bondAcquittedDate: new DateTime(2017, 1, 5),
				bondValidToDate: new DateTime(2017, 1, 6),

				clusterKey: 2
			);

			var l1pk = TestDataCreator.CreateCusEntryLine(ch_pk, clusterKey);
			var l2pk = TestDataCreator.CreateCusEntryLine(ch_pk, clusterKey);
			var l4pk = TestDataCreator.CreateCusEntryLine(ch_pk, clusterKey);
			var polishLinePK = TestDataCreator.CreateCusEntryLine(ch2_pk, 2);

			var cuscode1PK = TestDataCreator.CreateCusCodeData("ADI", "BND", "10", l1pk, "CL");

			var fee1Pk = TestDataCreator.CreateCusEntryLineFee(l1pk, "1P1", 18000.00f, clusterKey);
			var fee2Pk = TestDataCreator.CreateCusEntryLineFee(l2pk, "13E", 6.9f, clusterKey);
			var fee4Pk = TestDataCreator.CreateCusEntryLineFee(l4pk, "VAT", 22501.05f, clusterKey);
			var polishFee1Pk = TestDataCreator.CreateCusEntryLineFee(polishLinePK, "1P1", 18000.00f, 2);

			var groupPK = TestDataCreator.CreateRefDatabaseRefDataGrouping("ZA", "South Africa");
			var polishGroupPK = TestDataCreator.CreateRefDatabaseRefDataGrouping("PL", "Poland");

			var type1P1PK = TestDataCreator.CreateRefCusRateType("DTY", "ZA Duty rate type", 0, "ZA", "");
			var polishType1P1PK = TestDataCreator.CreateRefCusRateType("SPL", "Polish rate type", 0, "PL", "");
			var typeLVYPK = TestDataCreator.CreateRefCusRateType("LVY", "ZA Levy rate type", 0, "ZA", "");

			var rateCode1P1PK = TestDataCreator.CreateRefCusRateCode("1P1", type1P1PK, "1P1 rate code");
			var polishRateCode1P1PK = TestDataCreator.CreateRefCusRateCode("1P1", polishType1P1PK, "Polish rate code");
			var rateCode13EPK = TestDataCreator.CreateRefCusRateCode("13E", typeLVYPK, "13E rate code");

			using (var command = CargoWise.Data.Db.Connection.Command(
				$@"SELECT * FROM Report_ZACustomsEntryHeader ('{gc_pk}', 'IMP', NULL, NULL, NULL, NULL, NULL, '2017-01-02', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)"))
			{
				using (var reader = command.ExecuteReader())
				{
					AssertEquals("Result empty", false, reader.Read());
				}
			}
		}

		public void TestReport_ZACustomsEntryHeader_JobRegisteredFilterUseEntryThenDeclaration()
		{
			TestConnection.ExecuteNonQuery("ALTER TABLE CusEntryHeader DISABLE TRIGGER TG_CusEntryHeader_AuditDetailsAreNotMissing_Insert");

			var gc_pk = TestDataCreator.CreateCompany("AR1", "ZA", "ZAR");
			var gb_pk = TestDataCreator.CreateBranch(gc_pk, "GB1", "");
			var je_importer = TestDataCreator.CreateOrganisation("ACMEIMPORT", "IMPORTER NAME");
			var je_supplier = TestDataCreator.CreateOrganisation("ACMESUPPLIER", "SUPPLIER NAME");

			var je1_pk = TestDataCreator.CreateJobDeclaration("MSGSTATUSTEST001", gb_pk, gc_pk, "BLT", je_importer, je_supplier, "CTN", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, new DateTime(2022, 5, 12, 17, 0, 0), "IMP", 1);
			var cei1_pk = TestDataCreator.CreateCusEntryInstruction(je1_pk, "11", "Instruction Description1", new DateTime(2022, 05, 12), 1, "CustomsOfficeOverride=");
			var ch1_pk = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "CEO", 1, je1_pk, new DateTime(2022, 5, 12, 18, 30, 0), new DateTime(2022, 5, 12, 19, 30, 0), cei1_pk, new DateTime(2022, 5, 9), 1);
			var l1pk = TestDataCreator.CreateCusEntryLine(ch1_pk, 1);

			var je2_pk = TestDataCreator.CreateJobDeclaration("MSGSTATUSTEST002", gb_pk, gc_pk, "BLT", je_importer, je_supplier, "CTN", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, new DateTime(2022, 5, 12, 17, 0, 0), "IMP", 2);
			var cei2_pk = TestDataCreator.CreateCusEntryInstruction(je2_pk, "22", "Instruction Description2", new DateTime(2022, 05, 12), 1, "CustomsOfficeOverride=CTN");
			var ch2_pk = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "CEO", 1, je2_pk, new DateTime(2022, 5, 12, 18, 30, 0), new DateTime(2022, 5, 12, 19, 30, 0), cei2_pk, null, 2);
			var l2pk = TestDataCreator.CreateCusEntryLine(ch2_pk, 2);

			var results = new List<string>();
			var sql = $"SELECT * FROM Report_ZACustomsEntryHeader ('{gc_pk}', 'IMP', NULL, 'CTN', '2022-05-01', '2022-05-15', NULL, NULL, NULL, NULL, NULL, NULL, 'NOT', NULL, NULL, NULL)";
			CargoWise.Data.Db.Connection.ExecuteReader(sql, reader =>
			{
				results.Add($"{reader["JobNumber"]}");
			});
			AssertContainsExactElementsInAnyOrder("If CH_SystemCreateTimeUtc is null, we should look at JE_SystemCreateTimeUtc", new[] { "MSGSTATUSTEST001", "MSGSTATUSTEST002" }, results);

			TestConnection.ExecuteNonQuery("ALTER TABLE CusEntryHeader ENABLE TRIGGER TG_CusEntryHeader_AuditDetailsAreNotMissing_Insert");
		}
	}
}

