using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Client.JAS.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos
{
	class NonAccountingCognosLineGeneratorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals("Should be assigned in the constructor", Factory, Generator.Factory);
			AssertNotNull("Should be assigned in the constructor", Generator.CognosLineHelper);
			AssertEquals("Should be assigned in the constructor", new ZDateTime(2006, 1, 1), Generator.CalendarYearStartDate);
			AssertEquals("Should be assigned in the constructor", new ZDateTime(2006, 5, 5), Generator.ExportStartDate);
		}

		public void TestGetLinesAsString()
		{
			SetupStaffCountForTotalHeadCounts();
			SetupStaffCountForHeadCountsByGroup();
			SetupValidAirExportConsolAndShipments();
			SetupValidAirImportConsolAndShipments();
			SetupValidSeaExportConsolAndShipments();
			SetupValidSeaImportConsolAndShipments();
			SetupValidCustomsBrokerageJobs();
			SetupValidWarehouseJobs();
			SetupValidAirFreightWeightRecords();
			SetupValidConsolWithContainers();
			SetupValidInvoices();
			Factory.Save();
			#region Expected
			const string ExpectedLinesAsString = @"
Headcount,HCOUNT,ICTOTA,,C01,,14.00+,,,
Headcount,HCOUNT,ICTOTA,,C02,,12.00+,,,
Headcount,HCOUNT,ICTOTA,,C03,,15.00+,,,
Headcount,HCOUNT,ICTOTA,,,,2.00+,,,
Headcount Administration,HCOUNTADM,,,C01,,1.00+,,,
Headcount Administration,HCOUNTADM,,AI,C01,,2.00+,,,
Headcount Administration,HCOUNTADM,,AE,C01,,1.00+,,,
Headcount Administration,HCOUNTADM,,MI,C01,,0,,,
Headcount Administration,HCOUNTADM,,ME,C01,,0,,,
Headcount Administration,HCOUNTADM,,CHB,C01,,1.00+,,,
Headcount Administration,HCOUNTADM,,NV,C01,,0,,,
Headcount Administration,HCOUNTADM,,WPT,C01,,0,,,
Headcount Administration,HCOUNTADM,,PR,C01,,0,,,
Headcount Administration,HCOUNTADM,,OTH,C01,,0,,,
Headcount Administration,HCOUNTADM,,,C02,,0,,,
Headcount Administration,HCOUNTADM,,AI,C02,,1.00+,,,
Headcount Administration,HCOUNTADM,,AE,C02,,1.00+,,,
Headcount Administration,HCOUNTADM,,MI,C02,,0,,,
Headcount Administration,HCOUNTADM,,ME,C02,,1.00+,,,
Headcount Administration,HCOUNTADM,,CHB,C02,,0,,,
Headcount Administration,HCOUNTADM,,NV,C02,,1.00+,,,
Headcount Administration,HCOUNTADM,,WPT,C02,,0,,,
Headcount Administration,HCOUNTADM,,PR,C02,,0,,,
Headcount Administration,HCOUNTADM,,OTH,C02,,0,,,
Headcount Administration,HCOUNTADM,,,C03,,0,,,
Headcount Administration,HCOUNTADM,,AI,C03,,1.00+,,,
Headcount Administration,HCOUNTADM,,AE,C03,,1.00+,,,
Headcount Administration,HCOUNTADM,,MI,C03,,0,,,
Headcount Administration,HCOUNTADM,,ME,C03,,1.00+,,,
Headcount Administration,HCOUNTADM,,CHB,C03,,0,,,
Headcount Administration,HCOUNTADM,,NV,C03,,0,,,
Headcount Administration,HCOUNTADM,,WPT,C03,,1.00+,,,
Headcount Administration,HCOUNTADM,,PR,C03,,0,,,
Headcount Administration,HCOUNTADM,,OTH,C03,,0,,,
Headcount Sales,HCOUNTSALES,,,C01,,1.00+,,,
Headcount Sales,HCOUNTSALES,,AI,C01,,3.00+,,,
Headcount Sales,HCOUNTSALES,,AE,C01,,2.00+,,,
Headcount Sales,HCOUNTSALES,,MI,C01,,1.00+,,,
Headcount Sales,HCOUNTSALES,,ME,C01,,0,,,
Headcount Sales,HCOUNTSALES,,CHB,C01,,1.00+,,,
Headcount Sales,HCOUNTSALES,,NV,C01,,0,,,
Headcount Sales,HCOUNTSALES,,WPT,C01,,0,,,
Headcount Sales,HCOUNTSALES,,PR,C01,,0,,,
Headcount Sales,HCOUNTSALES,,OTH,C01,,2.00+,,,
Headcount Sales,HCOUNTSALES,,,C02,,1.00+,,,
Headcount Sales,HCOUNTSALES,,AI,C02,,1.00+,,,
Headcount Sales,HCOUNTSALES,,AE,C02,,2.00+,,,
Headcount Sales,HCOUNTSALES,,MI,C02,,1.00+,,,
Headcount Sales,HCOUNTSALES,,ME,C02,,3.00+,,,
Headcount Sales,HCOUNTSALES,,CHB,C02,,0,,,
Headcount Sales,HCOUNTSALES,,NV,C02,,0,,,
Headcount Sales,HCOUNTSALES,,WPT,C02,,1.00+,,,
Headcount Sales,HCOUNTSALES,,PR,C02,,1.00+,,,
Headcount Sales,HCOUNTSALES,,OTH,C02,,0,,,
Headcount Sales,HCOUNTSALES,,,C03,,0,,,
Headcount Sales,HCOUNTSALES,,AI,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,AE,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,MI,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,ME,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,CHB,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,NV,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,WPT,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,PR,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,OTH,C03,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,AI,C01,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,AE,C01,,2.00+,,,
Headcount Shipment Control,HCOUNTSC,,MI,C01,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,ME,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,CHB,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,NV,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,WPT,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,PR,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,OTH,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,,C02,,0,,,
Headcount Shipment Control,HCOUNTSC,,AI,C02,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,AE,C02,,0,,,
Headcount Shipment Control,HCOUNTSC,,MI,C02,,0,,,
Headcount Shipment Control,HCOUNTSC,,ME,C02,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,CHB,C02,,0,,,
Headcount Shipment Control,HCOUNTSC,,NV,C02,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,WPT,C02,,0,,,
Headcount Shipment Control,HCOUNTSC,,PR,C02,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,OTH,C02,,0,,,
Headcount Shipment Control,HCOUNTSC,,,C03,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,AI,C03,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,AE,C03,,2.00+,,,
Headcount Shipment Control,HCOUNTSC,,MI,C03,,0,,,
Headcount Shipment Control,HCOUNTSC,,ME,C03,,0,,,
Headcount Shipment Control,HCOUNTSC,,CHB,C03,,0,,,
Headcount Shipment Control,HCOUNTSC,,NV,C03,,0,,,
Headcount Shipment Control,HCOUNTSC,,WPT,C03,,0,,,
Headcount Shipment Control,HCOUNTSC,,PR,C03,,0,,,
Headcount Shipment Control,HCOUNTSC,,OTH,C03,,1.00+,,,
Air Export,SDIRECT,ICTOTA,AE,C01,,3.00+,,,
Air Export,SHOUSES,ICTOTA,AE,C01,,10.00+,,,
Air Import,SHOUSES,ICTOTA,AI,C01,,6.00+,,,
Maritime Export,SHOUSES,ICTOTA,ME,C01,,6.00+,,,
Maritime Import,SHOUSES,ICTOTA,MI,C01,,12.00+,,,
Maritime Export NVOCC,SHOUSES,ICTOTA,NV,C01,,3.00+,,,
Customs House Brokerage,SHOUSES,ICTOTA,CHB,C01,,2.00+,,,
Warehousing/Packing/Trucking,SHOUSES,ICTOTA,WPT,C01,,5.00+,,,
Air Export,SMASTERS,ICTOTA,AE,C01,,3.00+,,,
Air Import,SMASTERS,ICTOTA,AI,C01,,3.00+,,,
Maritime Export,SMASTERS,ICTOTA,ME,C01,,4.00+,,,
Maritime Import,SMASTERS,ICTOTA,MI,C01,,4.00+,,,
Maritime Export NVOCC,SMASTERS,ICTOTA,NV,C01,,1.00+,,,
Air Export,SDIRECT,ICTOTA,AE,C02,,2.00+,,,
Air Export,SHOUSES,ICTOTA,AE,C02,,3.00+,,,
Air Import,SHOUSES,ICTOTA,AI,C02,,2.00+,,,
Maritime Export,SHOUSES,ICTOTA,ME,C02,,3.00+,,,
Maritime Import,SHOUSES,ICTOTA,MI,C02,,5.00+,,,
Maritime Export NVOCC,SHOUSES,ICTOTA,NV,C02,,4.00+,,,
Customs House Brokerage,SHOUSES,ICTOTA,CHB,C02,,2.00+,,,
Warehousing/Packing/Trucking,SHOUSES,ICTOTA,WPT,C02,,4.00+,,,
Air Export,SMASTERS,ICTOTA,AE,C02,,1.00+,,,
Air Import,SMASTERS,ICTOTA,AI,C02,,1.00+,,,
Maritime Export,SMASTERS,ICTOTA,ME,C02,,2.00+,,,
Maritime Import,SMASTERS,ICTOTA,MI,C02,,2.00+,,,
Maritime Export NVOCC,SMASTERS,ICTOTA,NV,C02,,1.00+,,,
Air Export,SDIRECT,ICTOTA,AE,C03,,0,,,
Air Export,SHOUSES,ICTOTA,AE,C03,,13.00+,,,
Air Import,SHOUSES,ICTOTA,AI,C03,,9.00+,,,
Maritime Export,SHOUSES,ICTOTA,ME,C03,,12.00+,,,
Maritime Import,SHOUSES,ICTOTA,MI,C03,,9.00+,,,
Maritime Export NVOCC,SHOUSES,ICTOTA,NV,C03,,2.00+,,,
Customs House Brokerage,SHOUSES,ICTOTA,CHB,C03,,1.00+,,,
Warehousing/Packing/Trucking,SHOUSES,ICTOTA,WPT,C03,,3.00+,,,
Air Export,SMASTERS,ICTOTA,AE,C03,,5.00+,,,
Air Import,SMASTERS,ICTOTA,AI,C03,,5.00+,,,
Maritime Export,SMASTERS,ICTOTA,ME,C03,,6.00+,,,
Maritime Import,SMASTERS,ICTOTA,MI,C03,,5.00+,,,
Maritime Export NVOCC,SMASTERS,ICTOTA,NV,C03,,2.00+,,,
Airfreight Kilos,SKGS,ICTOTA,AE,C01,,118.93+,,,
Airfreight Kilos,SKGS,ICTOTA,AI,C01,,4.25+,,,
Airfreight Kilos,SKGS,ICTOTA,AE,C02,,6.50+,,,
Airfreight Kilos,SKGS,ICTOTA,AI,C02,,10.00+,,,
Airfreight Kilos,SKGS,ICTOTA,AE,C03,,0.71+,,,
Airfreight Kilos,SKGS,ICTOTA,AI,C03,,25.59+,,,
Oceanfreight TEUs,STEU,ICTOTA,ME,C01,,8.00+,,,
Oceanfreight TEUs,STEU,ICTOTA,MI,C01,,2.00+,,,
Oceanfreight TEUs,STEU,ICTOTA,NV,C01,,0,,,
Oceanfreight TEUs,STEU,ICTOTA,ME,C02,,14.60+,,,
Oceanfreight TEUs,STEU,ICTOTA,MI,C02,,4.00+,,,
Oceanfreight TEUs,STEU,ICTOTA,NV,C02,,0,,,
Oceanfreight TEUs,STEU,ICTOTA,ME,C03,,5.00+,,,
Oceanfreight TEUs,STEU,ICTOTA,MI,C03,,0,,,
Oceanfreight TEUs,STEU,ICTOTA,NV,C03,,8.60+,,,
Total Invoiced,TI,ICTOTA,,C01,,187.00+,,,
Total Invoiced,TI,ICTOTA,,C02,,297.00+,,,
Total Invoiced,TI,ICTOTA,,C03,,220.00+,,,
";
			#endregion
			AssertMultilineEquals("Should be equal", ExpectedLinesAsString.Trim(), Generator.GetLinesAsString(), '\n');
		}

		#region Head Count
		public void TestAppendTotalHeadCounts()
		{
			SetupStaffCountForTotalHeadCounts();
			Generator.AppendTotalHeadCounts(StringBuilderForTest);
			const string ExpectedLinesAsString = @"
Headcount,HCOUNT,ICTOTA,,C01,,3.00+,,,
Headcount,HCOUNT,ICTOTA,,C02,,1.00+,,,
Headcount,HCOUNT,ICTOTA,,C03,,4.00+,,,
Headcount,HCOUNT,ICTOTA,,,,2.00+,,,
";
			AssertMultilineEquals("Should be equal", ExpectedLinesAsString.Trim(), StringBuilderForTest.ToStringWithNewLineBetweenAppends(), '\n');
		}

		public void TestAppendHeadCountsByGroup()
		{
			SetupStaffCountForHeadCountsByGroup();
			Generator.AppendHeadCountsByGroup(StringBuilderForTest);
			#region Expected
			const string ExpectedLinesAsString = @"
Headcount Administration,HCOUNTADM,,,C01,,1.00+,,,
Headcount Administration,HCOUNTADM,,AI,C01,,2.00+,,,
Headcount Administration,HCOUNTADM,,AE,C01,,1.00+,,,
Headcount Administration,HCOUNTADM,,MI,C01,,0,,,
Headcount Administration,HCOUNTADM,,ME,C01,,0,,,
Headcount Administration,HCOUNTADM,,CHB,C01,,1.00+,,,
Headcount Administration,HCOUNTADM,,NV,C01,,0,,,
Headcount Administration,HCOUNTADM,,WPT,C01,,0,,,
Headcount Administration,HCOUNTADM,,PR,C01,,0,,,
Headcount Administration,HCOUNTADM,,OTH,C01,,0,,,
Headcount Administration,HCOUNTADM,,,C02,,0,,,
Headcount Administration,HCOUNTADM,,AI,C02,,1.00+,,,
Headcount Administration,HCOUNTADM,,AE,C02,,1.00+,,,
Headcount Administration,HCOUNTADM,,MI,C02,,0,,,
Headcount Administration,HCOUNTADM,,ME,C02,,1.00+,,,
Headcount Administration,HCOUNTADM,,CHB,C02,,0,,,
Headcount Administration,HCOUNTADM,,NV,C02,,1.00+,,,
Headcount Administration,HCOUNTADM,,WPT,C02,,0,,,
Headcount Administration,HCOUNTADM,,PR,C02,,0,,,
Headcount Administration,HCOUNTADM,,OTH,C02,,0,,,
Headcount Administration,HCOUNTADM,,,C03,,0,,,
Headcount Administration,HCOUNTADM,,AI,C03,,1.00+,,,
Headcount Administration,HCOUNTADM,,AE,C03,,1.00+,,,
Headcount Administration,HCOUNTADM,,MI,C03,,0,,,
Headcount Administration,HCOUNTADM,,ME,C03,,1.00+,,,
Headcount Administration,HCOUNTADM,,CHB,C03,,0,,,
Headcount Administration,HCOUNTADM,,NV,C03,,0,,,
Headcount Administration,HCOUNTADM,,WPT,C03,,1.00+,,,
Headcount Administration,HCOUNTADM,,PR,C03,,0,,,
Headcount Administration,HCOUNTADM,,OTH,C03,,0,,,
Headcount Sales,HCOUNTSALES,,,C01,,1.00+,,,
Headcount Sales,HCOUNTSALES,,AI,C01,,3.00+,,,
Headcount Sales,HCOUNTSALES,,AE,C01,,2.00+,,,
Headcount Sales,HCOUNTSALES,,MI,C01,,1.00+,,,
Headcount Sales,HCOUNTSALES,,ME,C01,,0,,,
Headcount Sales,HCOUNTSALES,,CHB,C01,,1.00+,,,
Headcount Sales,HCOUNTSALES,,NV,C01,,0,,,
Headcount Sales,HCOUNTSALES,,WPT,C01,,0,,,
Headcount Sales,HCOUNTSALES,,PR,C01,,0,,,
Headcount Sales,HCOUNTSALES,,OTH,C01,,2.00+,,,
Headcount Sales,HCOUNTSALES,,,C02,,1.00+,,,
Headcount Sales,HCOUNTSALES,,AI,C02,,1.00+,,,
Headcount Sales,HCOUNTSALES,,AE,C02,,2.00+,,,
Headcount Sales,HCOUNTSALES,,MI,C02,,1.00+,,,
Headcount Sales,HCOUNTSALES,,ME,C02,,3.00+,,,
Headcount Sales,HCOUNTSALES,,CHB,C02,,0,,,
Headcount Sales,HCOUNTSALES,,NV,C02,,0,,,
Headcount Sales,HCOUNTSALES,,WPT,C02,,1.00+,,,
Headcount Sales,HCOUNTSALES,,PR,C02,,1.00+,,,
Headcount Sales,HCOUNTSALES,,OTH,C02,,0,,,
Headcount Sales,HCOUNTSALES,,,C03,,0,,,
Headcount Sales,HCOUNTSALES,,AI,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,AE,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,MI,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,ME,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,CHB,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,NV,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,WPT,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,PR,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,OTH,C03,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,AI,C01,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,AE,C01,,2.00+,,,
Headcount Shipment Control,HCOUNTSC,,MI,C01,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,ME,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,CHB,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,NV,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,WPT,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,PR,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,OTH,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,,C02,,0,,,
Headcount Shipment Control,HCOUNTSC,,AI,C02,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,AE,C02,,0,,,
Headcount Shipment Control,HCOUNTSC,,MI,C02,,0,,,
Headcount Shipment Control,HCOUNTSC,,ME,C02,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,CHB,C02,,0,,,
Headcount Shipment Control,HCOUNTSC,,NV,C02,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,WPT,C02,,0,,,
Headcount Shipment Control,HCOUNTSC,,PR,C02,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,OTH,C02,,0,,,
Headcount Shipment Control,HCOUNTSC,,,C03,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,AI,C03,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,AE,C03,,2.00+,,,
Headcount Shipment Control,HCOUNTSC,,MI,C03,,0,,,
Headcount Shipment Control,HCOUNTSC,,ME,C03,,0,,,
Headcount Shipment Control,HCOUNTSC,,CHB,C03,,0,,,
Headcount Shipment Control,HCOUNTSC,,NV,C03,,0,,,
Headcount Shipment Control,HCOUNTSC,,WPT,C03,,0,,,
Headcount Shipment Control,HCOUNTSC,,PR,C03,,0,,,
Headcount Shipment Control,HCOUNTSC,,OTH,C03,,1.00+,,,
";
			#endregion
			AssertMultilineEquals("Should be equal", ExpectedLinesAsString.Trim(), StringBuilderForTest.ToStringWithNewLineBetweenAppends(), '\n');
		}

		[ExpectNoExceptions]
		public void TestAppendHeadCountsByGroup_GroupPKRegistrySettingsNotSet()
		{
			Generator.AppendHeadCountsByGroup(StringBuilderForTest);
		}

		public void TestAppendHeadCountLines()
		{
			SetupStaffCountForTotalHeadCounts();
			SetupStaffCountForHeadCountsByGroup();
			Generator.AppendHeadCountLines(StringBuilderForTest);
			#region Expected
			const string ExpectedLinesAsString = @"
Headcount,HCOUNT,ICTOTA,,C01,,14.00+,,,
Headcount,HCOUNT,ICTOTA,,C02,,12.00+,,,
Headcount,HCOUNT,ICTOTA,,C03,,15.00+,,,
Headcount,HCOUNT,ICTOTA,,,,2.00+,,,
Headcount Administration,HCOUNTADM,,,C01,,1.00+,,,
Headcount Administration,HCOUNTADM,,AI,C01,,2.00+,,,
Headcount Administration,HCOUNTADM,,AE,C01,,1.00+,,,
Headcount Administration,HCOUNTADM,,MI,C01,,0,,,
Headcount Administration,HCOUNTADM,,ME,C01,,0,,,
Headcount Administration,HCOUNTADM,,CHB,C01,,1.00+,,,
Headcount Administration,HCOUNTADM,,NV,C01,,0,,,
Headcount Administration,HCOUNTADM,,WPT,C01,,0,,,
Headcount Administration,HCOUNTADM,,PR,C01,,0,,,
Headcount Administration,HCOUNTADM,,OTH,C01,,0,,,
Headcount Administration,HCOUNTADM,,,C02,,0,,,
Headcount Administration,HCOUNTADM,,AI,C02,,1.00+,,,
Headcount Administration,HCOUNTADM,,AE,C02,,1.00+,,,
Headcount Administration,HCOUNTADM,,MI,C02,,0,,,
Headcount Administration,HCOUNTADM,,ME,C02,,1.00+,,,
Headcount Administration,HCOUNTADM,,CHB,C02,,0,,,
Headcount Administration,HCOUNTADM,,NV,C02,,1.00+,,,
Headcount Administration,HCOUNTADM,,WPT,C02,,0,,,
Headcount Administration,HCOUNTADM,,PR,C02,,0,,,
Headcount Administration,HCOUNTADM,,OTH,C02,,0,,,
Headcount Administration,HCOUNTADM,,,C03,,0,,,
Headcount Administration,HCOUNTADM,,AI,C03,,1.00+,,,
Headcount Administration,HCOUNTADM,,AE,C03,,1.00+,,,
Headcount Administration,HCOUNTADM,,MI,C03,,0,,,
Headcount Administration,HCOUNTADM,,ME,C03,,1.00+,,,
Headcount Administration,HCOUNTADM,,CHB,C03,,0,,,
Headcount Administration,HCOUNTADM,,NV,C03,,0,,,
Headcount Administration,HCOUNTADM,,WPT,C03,,1.00+,,,
Headcount Administration,HCOUNTADM,,PR,C03,,0,,,
Headcount Administration,HCOUNTADM,,OTH,C03,,0,,,
Headcount Sales,HCOUNTSALES,,,C01,,1.00+,,,
Headcount Sales,HCOUNTSALES,,AI,C01,,3.00+,,,
Headcount Sales,HCOUNTSALES,,AE,C01,,2.00+,,,
Headcount Sales,HCOUNTSALES,,MI,C01,,1.00+,,,
Headcount Sales,HCOUNTSALES,,ME,C01,,0,,,
Headcount Sales,HCOUNTSALES,,CHB,C01,,1.00+,,,
Headcount Sales,HCOUNTSALES,,NV,C01,,0,,,
Headcount Sales,HCOUNTSALES,,WPT,C01,,0,,,
Headcount Sales,HCOUNTSALES,,PR,C01,,0,,,
Headcount Sales,HCOUNTSALES,,OTH,C01,,2.00+,,,
Headcount Sales,HCOUNTSALES,,,C02,,1.00+,,,
Headcount Sales,HCOUNTSALES,,AI,C02,,1.00+,,,
Headcount Sales,HCOUNTSALES,,AE,C02,,2.00+,,,
Headcount Sales,HCOUNTSALES,,MI,C02,,1.00+,,,
Headcount Sales,HCOUNTSALES,,ME,C02,,3.00+,,,
Headcount Sales,HCOUNTSALES,,CHB,C02,,0,,,
Headcount Sales,HCOUNTSALES,,NV,C02,,0,,,
Headcount Sales,HCOUNTSALES,,WPT,C02,,1.00+,,,
Headcount Sales,HCOUNTSALES,,PR,C02,,1.00+,,,
Headcount Sales,HCOUNTSALES,,OTH,C02,,0,,,
Headcount Sales,HCOUNTSALES,,,C03,,0,,,
Headcount Sales,HCOUNTSALES,,AI,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,AE,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,MI,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,ME,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,CHB,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,NV,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,WPT,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,PR,C03,,1.00+,,,
Headcount Sales,HCOUNTSALES,,OTH,C03,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,AI,C01,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,AE,C01,,2.00+,,,
Headcount Shipment Control,HCOUNTSC,,MI,C01,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,ME,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,CHB,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,NV,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,WPT,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,PR,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,OTH,C01,,0,,,
Headcount Shipment Control,HCOUNTSC,,,C02,,0,,,
Headcount Shipment Control,HCOUNTSC,,AI,C02,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,AE,C02,,0,,,
Headcount Shipment Control,HCOUNTSC,,MI,C02,,0,,,
Headcount Shipment Control,HCOUNTSC,,ME,C02,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,CHB,C02,,0,,,
Headcount Shipment Control,HCOUNTSC,,NV,C02,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,WPT,C02,,0,,,
Headcount Shipment Control,HCOUNTSC,,PR,C02,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,OTH,C02,,0,,,
Headcount Shipment Control,HCOUNTSC,,,C03,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,AI,C03,,1.00+,,,
Headcount Shipment Control,HCOUNTSC,,AE,C03,,2.00+,,,
Headcount Shipment Control,HCOUNTSC,,MI,C03,,0,,,
Headcount Shipment Control,HCOUNTSC,,ME,C03,,0,,,
Headcount Shipment Control,HCOUNTSC,,CHB,C03,,0,,,
Headcount Shipment Control,HCOUNTSC,,NV,C03,,0,,,
Headcount Shipment Control,HCOUNTSC,,WPT,C03,,0,,,
Headcount Shipment Control,HCOUNTSC,,PR,C03,,0,,,
Headcount Shipment Control,HCOUNTSC,,OTH,C03,,1.00+,,,
";
			#endregion
			AssertMultilineEquals("Should be equal", ExpectedLinesAsString.Trim(), StringBuilderForTest.ToStringWithNewLineBetweenAppends(), '\n');
		}

		void SetupStaffGroups()
		{
			AdminGroup = Factory.NewWithValidTestData<GlbGroup>();
			SalesGroup = Factory.NewWithValidTestData<GlbGroup>();
			ShipmentControlGroup = Factory.NewWithValidTestData<GlbGroup>();
			JASDataRegistry.Instance.CognosAdminGroup = AdminGroup.PK;
			JASDataRegistry.Instance.CognosSalesGroup = SalesGroup.PK;
			JASDataRegistry.Instance.CognosShipmentControlGroup = ShipmentControlGroup.PK;
		}

		void SetupStaffCountForHeadCountsByGroup()
		{
			SetupStaffGroups();
			CreateNewGlbStaff(true, SYDBranch, GetDeptFromCode("FIA"), SalesGroup, AdminGroup);
			CreateNewGlbStaff(true, SYDBranch, GetDeptFromCode("LEA"), SalesGroup);
			CreateNewGlbStaff(true, SYDBranch, GetDeptFromCode("FEA"), SalesGroup, ShipmentControlGroup);
			CreateNewGlbStaff(true, SYDBranch, GetDeptFromCode("BRN"), SalesGroup);
			CreateNewGlbStaff(true, SYDBranch, GetDeptFromCode("BRN"), SalesGroup);
			CreateNewGlbStaff(true, SYDBranch, GetDeptFromCode("CEA"), AdminGroup, ShipmentControlGroup);
			CreateNewGlbStaff(true, SYDBranch, GetDeptFromCode("CIA"), SalesGroup, AdminGroup, ShipmentControlGroup);
			CreateNewGlbStaff(true, SYDBranch, GetDeptFromCode("CPP"), SalesGroup, AdminGroup);
			CreateNewGlbStaff(true, SYDBranch, GetDeptFromCode("LIS"), SalesGroup, ShipmentControlGroup);
			CreateNewGlbStaff(true, SYDBranch, GetDeptFromCode("LIA"), SalesGroup);
			CreateNewGlbStaff(false, SYDBranch, GetDeptFromCode("CIA"), SalesGroup, AdminGroup, ShipmentControlGroup);
			CreateNewGlbStaff(false, SYDBranch, GetDeptFromCode("CIS"), SalesGroup, AdminGroup, ShipmentControlGroup);
			CreateNewGlbStaff(true, SYDBranch, null, SalesGroup, AdminGroup);
			CreateNewGlbStaff(true, MELBranch, GetDeptFromCode("LES"), SalesGroup, AdminGroup);
			CreateNewGlbStaff(true, MELBranch, GetDeptFromCode("LIS"), SalesGroup);
			CreateNewGlbStaff(true, MELBranch, GetDeptFromCode("CES"), SalesGroup, ShipmentControlGroup);
			CreateNewGlbStaff(true, MELBranch, GetDeptFromCode("WBS"), SalesGroup);
			CreateNewGlbStaff(true, MELBranch, GetDeptFromCode("CEA"), SalesGroup);
			CreateNewGlbStaff(true, MELBranch, GetDeptFromCode("DIR"), AdminGroup, ShipmentControlGroup);
			CreateNewGlbStaff(true, MELBranch, GetDeptFromCode("FIA"), SalesGroup, AdminGroup, ShipmentControlGroup);
			CreateNewGlbStaff(true, MELBranch, GetDeptFromCode("FEA"), SalesGroup, AdminGroup);
			CreateNewGlbStaff(true, MELBranch, GetDeptFromCode("MSJ"), SalesGroup, ShipmentControlGroup);
			CreateNewGlbStaff(true, MELBranch, GetDeptFromCode("FES"), SalesGroup);
			CreateNewGlbStaff(false, MELBranch, GetDeptFromCode("FES"), SalesGroup, AdminGroup, ShipmentControlGroup);
			CreateNewGlbStaff(false, MELBranch, GetDeptFromCode("FIA"), SalesGroup, AdminGroup, ShipmentControlGroup);
			CreateNewGlbStaff(true, MELBranch, null, SalesGroup);
			CreateNewGlbStaff(true, ADLBranch, GetDeptFromCode("WBS"), SalesGroup, AdminGroup);
			CreateNewGlbStaff(true, ADLBranch, GetDeptFromCode("MSJ"), SalesGroup);
			CreateNewGlbStaff(true, ADLBranch, GetDeptFromCode("BRN"), SalesGroup, ShipmentControlGroup);
			CreateNewGlbStaff(true, ADLBranch, GetDeptFromCode("CPP"), SalesGroup);
			CreateNewGlbStaff(true, ADLBranch, GetDeptFromCode("DIR"), SalesGroup);
			CreateNewGlbStaff(true, ADLBranch, GetDeptFromCode("CEA"), AdminGroup, ShipmentControlGroup);
			CreateNewGlbStaff(true, ADLBranch, GetDeptFromCode("CIA"), SalesGroup, AdminGroup, ShipmentControlGroup);
			CreateNewGlbStaff(true, ADLBranch, GetDeptFromCode("FES"), SalesGroup, AdminGroup);
			CreateNewGlbStaff(true, ADLBranch, GetDeptFromCode("LEA"), SalesGroup, ShipmentControlGroup);
			CreateNewGlbStaff(true, ADLBranch, GetDeptFromCode("FIS"), SalesGroup);
			CreateNewGlbStaff(false, ADLBranch, GetDeptFromCode("LEA"), SalesGroup, AdminGroup, ShipmentControlGroup);
			CreateNewGlbStaff(false, ADLBranch, GetDeptFromCode("LIS"), SalesGroup, AdminGroup, ShipmentControlGroup);
			CreateNewGlbStaff(true, ADLBranch, null, ShipmentControlGroup);
			Factory.Save();
		}

		void SetupStaffCountForTotalHeadCounts()
		{
			CreateNewGlbStaff(true, SYDBranch);
			CreateNewGlbStaff(true, SYDBranch);
			CreateNewGlbStaff(true, SYDBranch);
			CreateNewGlbStaff(false, SYDBranch);
			CreateNewGlbStaff(true, MELBranch);
			CreateNewGlbStaff(false, MELBranch);
			CreateNewGlbStaff(true, ADLBranch);
			CreateNewGlbStaff(true, ADLBranch);
			CreateNewGlbStaff(true, ADLBranch);
			CreateNewGlbStaff(true, ADLBranch);
			CreateNewGlbStaff(false, ADLBranch);
			CreateNewGlbStaff(true, null);
			CreateNewGlbStaff(true, null);
			CreateNewGlbStaff(false, null);
			Factory.Save();
		}

		void CreateNewGlbStaff(bool isActive, GlbBranch homeBranch)
		{
			CreateNewGlbStaff(isActive, homeBranch, null);
		}

		void CreateNewGlbStaff(bool isActive, GlbBranch homeBranch, GlbDepartment dept, params GlbGroup[] groups)
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsActive = isActive;
			staff.GS_GB_HomeBranch = (homeBranch != null) ? homeBranch.PK : ZGuid.Empty;
			staff.GS_GE_HomeDepartment = (dept != null) ? dept.PK : ZGuid.Empty;
			staff.Groups.AddRange(groups);
		}

		GlbGroup AdminGroup;
		GlbGroup SalesGroup;
		GlbGroup ShipmentControlGroup;
		#endregion
		#region Shipment Job Count
		[TestDate(2006, 5, 5)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAppendShipmentCountLines()
		{
			SetupConsolAndShipmentCountsForShipmentCountLines();
			Generator.AppendShipmentCountLines(StringBuilderForTest);
			#region Expected
			const string ExpectedLinesAsString = @"
Air Export,SDIRECT,ICTOTA,AE,C01,,3.00+,,,
Air Export,SHOUSES,ICTOTA,AE,C01,,10.00+,,,
Air Import,SHOUSES,ICTOTA,AI,C01,,6.00+,,,
Maritime Export,SHOUSES,ICTOTA,ME,C01,,5.00+,,,
Maritime Import,SHOUSES,ICTOTA,MI,C01,,11.00+,,,
Maritime Export NVOCC,SHOUSES,ICTOTA,NV,C01,,3.00+,,,
Customs House Brokerage,SHOUSES,ICTOTA,CHB,C01,,2.00+,,,
Warehousing/Packing/Trucking,SHOUSES,ICTOTA,WPT,C01,,5.00+,,,
Air Export,SMASTERS,ICTOTA,AE,C01,,3.00+,,,
Air Import,SMASTERS,ICTOTA,AI,C01,,3.00+,,,
Maritime Export,SMASTERS,ICTOTA,ME,C01,,3.00+,,,
Maritime Import,SMASTERS,ICTOTA,MI,C01,,3.00+,,,
Maritime Export NVOCC,SMASTERS,ICTOTA,NV,C01,,1.00+,,,
Air Export,SDIRECT,ICTOTA,AE,C02,,2.00+,,,
Air Export,SHOUSES,ICTOTA,AE,C02,,3.00+,,,
Air Import,SHOUSES,ICTOTA,AI,C02,,2.00+,,,
Maritime Export,SHOUSES,ICTOTA,ME,C02,,2.00+,,,
Maritime Import,SHOUSES,ICTOTA,MI,C02,,4.00+,,,
Maritime Export NVOCC,SHOUSES,ICTOTA,NV,C02,,4.00+,,,
Customs House Brokerage,SHOUSES,ICTOTA,CHB,C02,,2.00+,,,
Warehousing/Packing/Trucking,SHOUSES,ICTOTA,WPT,C02,,4.00+,,,
Air Export,SMASTERS,ICTOTA,AE,C02,,1.00+,,,
Air Import,SMASTERS,ICTOTA,AI,C02,,1.00+,,,
Maritime Export,SMASTERS,ICTOTA,ME,C02,,1.00+,,,
Maritime Import,SMASTERS,ICTOTA,MI,C02,,1.00+,,,
Maritime Export NVOCC,SMASTERS,ICTOTA,NV,C02,,1.00+,,,
Air Export,SDIRECT,ICTOTA,AE,C03,,0,,,
Air Export,SHOUSES,ICTOTA,AE,C03,,13.00+,,,
Air Import,SHOUSES,ICTOTA,AI,C03,,9.00+,,,
Maritime Export,SHOUSES,ICTOTA,ME,C03,,11.00+,,,
Maritime Import,SHOUSES,ICTOTA,MI,C03,,9.00+,,,
Maritime Export NVOCC,SHOUSES,ICTOTA,NV,C03,,1.00+,,,
Customs House Brokerage,SHOUSES,ICTOTA,CHB,C03,,1.00+,,,
Warehousing/Packing/Trucking,SHOUSES,ICTOTA,WPT,C03,,3.00+,,,
Air Export,SMASTERS,ICTOTA,AE,C03,,5.00+,,,
Air Import,SMASTERS,ICTOTA,AI,C03,,5.00+,,,
Maritime Export,SMASTERS,ICTOTA,ME,C03,,5.00+,,,
Maritime Import,SMASTERS,ICTOTA,MI,C03,,5.00+,,,
Maritime Export NVOCC,SMASTERS,ICTOTA,NV,C03,,1.00+,,,
";
			#endregion
			AssertMultilineEquals("Should be equal", ExpectedLinesAsString.Trim(), StringBuilderForTest.ToStringWithNewLineBetweenAppends(), '\n');
		}

		void SetupConsolAndShipmentCountsForShipmentCountLines()
		{
			SetupValidAirExportConsolAndShipments();
			SetupValidAirImportConsolAndShipments();
			SetupValidSeaExportConsolAndShipments();
			SetupValidSeaImportConsolAndShipments();
			SetupInvalidConsolAndShipments();
			SetupValidCustomsBrokerageJobs();
			SetupInvalidCustomsBrokerageJobs();
			SetupValidWarehouseJobs();
			SetupInvalidWarehouseJobs();
			Factory.Save();
		}

		void SetupValidAirExportConsolAndShipments()
		{
			AddConsol(Core.Constants.AgentType.Agent, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 2, 1), "AUSYD", "ITMIL", 5, SYDBranch);
			AddConsol(Core.Constants.AgentType.Charter, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 2, 10), "AUSYD", "DEFRA", 2, SYDBranch);
			AddConsol(Core.Constants.AgentType.Other, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 3, 1), "AUSYD", "USATL", 3, SYDBranch);
			AddConsol(Core.Constants.AgentType.Direct, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 5, 1), "AUSYD", "IDJKT", 1, SYDBranch);
			AddConsol(Core.Constants.AgentType.Direct, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 5, 2), "AUSYD", "JPTYO", 1, SYDBranch);
			AddConsol(Core.Constants.AgentType.Direct, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 5, 3), "AUSYD", "FRPAR", 1, SYDBranch);
			AddConsol(Core.Constants.AgentType.Agent, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 2, 1), "AUOOL", "ITVAL", 1, ADLBranch);
			AddConsol(Core.Constants.AgentType.Agent, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 2, 10), "AUDRW", "SGSIN", 2, ADLBranch);
			AddConsol(Core.Constants.AgentType.Other, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 3, 1), "AUPER", "USATL", 3, ADLBranch);
			AddConsol(Core.Constants.AgentType.Charter, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 1, 1), "AUADL", "USBOS", 3, ADLBranch);
			AddConsol(Core.Constants.AgentType.Charter, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 2, 2), "AUADL", "CAYTO", 4, ADLBranch);
			AddConsol(Core.Constants.AgentType.Other, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 3, 1), "AUMEL", "KRSEL", 3, MELBranch);
			AddConsol(Core.Constants.AgentType.Direct, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 5, 1), "AUMEL", "ZAJOH", 1, MELBranch);
			AddConsol(Core.Constants.AgentType.Direct, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 5, 2), "AUMEL", "GBLON", 1, MELBranch);
		}

		void SetupValidAirImportConsolAndShipments()
		{
			AddConsol(Core.Constants.AgentType.Agent, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 2, 1), "GBLON", "AUSYD", 1, SYDBranch);
			AddConsol(Core.Constants.AgentType.Charter, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 2, 10), "CHCHI", "AUSYD", 2, SYDBranch);
			AddConsol(Core.Constants.AgentType.Other, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 3, 1), "USCHI", "AUSYD", 3, SYDBranch);
			AddConsol(Core.Constants.AgentType.Agent, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 2, 1), "THBKK", "AUBNE", 2, ADLBranch);
			AddConsol(Core.Constants.AgentType.Agent, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 2, 10), "NZAKL", "AUDRW", 1, ADLBranch);
			AddConsol(Core.Constants.AgentType.Other, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 3, 1), "USATL", "AUOOL", 3, ADLBranch);
			AddConsol(Core.Constants.AgentType.Charter, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 1, 1), "USBOS", "AUADL", 1, ADLBranch);
			AddConsol(Core.Constants.AgentType.Charter, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 2, 2), "CAYTO", "AUADL", 2, ADLBranch);
			AddConsol(Core.Constants.AgentType.Other, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 3, 1), "KRSEL", "AUMEL", 2, MELBranch);
		}

		void SetupValidSeaExportConsolAndShipments()
		{
			AddConsol(Core.Constants.AgentType.Agent, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 2, 1), "AUSYD", "ITMIL", 2, SYDBranch);
			AddConsol(Core.Constants.AgentType.Other, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 2, 10), "AUSYD", "DEFRA", 1, SYDBranch);
			AddConsol(Core.Constants.AgentType.Other, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 3, 1), "AUSYD", "USATL", 2, SYDBranch);
			AddConsol(Core.Constants.AgentType.CoLoad, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 4, 1), "AUSYD", "HKHKG", 3, SYDBranch);
			AddConsol(Core.Constants.AgentType.Agent, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 2, 1), "AUOOL", "ITVAL", 1, ADLBranch);
			AddConsol(Core.Constants.AgentType.Charter, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 2, 10), "AUDRW", "SGSIN", 3, ADLBranch);
			AddConsol(Core.Constants.AgentType.Other, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 3, 1), "AUPER", "USATL", 2, ADLBranch);
			AddConsol(Core.Constants.AgentType.Charter, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 1, 1), "AUADL", "USBOS", 3, ADLBranch);
			AddConsol(Core.Constants.AgentType.Charter, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 2, 2), "AUADL", "CAYTO", 2, ADLBranch);
			AddConsol(Core.Constants.AgentType.CoLoad, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 4, 1), "AUBNE", "CNSHA", 1, ADLBranch);
			AddConsol(Core.Constants.AgentType.Other, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 3, 1), "AUMEL", "KRSEL", 2, MELBranch);
			AddConsol(Core.Constants.AgentType.CoLoad, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 4, 1), "AUMEL", "BRSAO", 4, MELBranch);
		}

		void SetupValidSeaImportConsolAndShipments()
		{
			AddConsol(Core.Constants.AgentType.Agent, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 2, 1), "GBLON", "AUSYD", 5, SYDBranch);
			AddConsol(Core.Constants.AgentType.Charter, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 2, 10), "CHCHI", "AUSYD", 4, SYDBranch);
			AddConsol(Core.Constants.AgentType.Other, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 3, 1), "USCHI", "AUSYD", 2, SYDBranch);
			AddConsol(Core.Constants.AgentType.Agent, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 2, 1), "THBKK", "AUBNE", 2, ADLBranch);
			AddConsol(Core.Constants.AgentType.Agent, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 2, 10), "NZAKL", "AUDRW", 1, ADLBranch);
			AddConsol(Core.Constants.AgentType.Other, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 3, 1), "USATL", "AUOOL", 3, ADLBranch);
			AddConsol(Core.Constants.AgentType.Charter, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 1, 1), "USBOS", "AUADL", 1, ADLBranch);
			AddConsol(Core.Constants.AgentType.Charter, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 2, 2), "CAYTO", "AUADL", 2, ADLBranch);
			AddConsol(Core.Constants.AgentType.Other, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 3, 1), "KRSEL", "AUMEL", 4, MELBranch);
		}

		void SetupInvalidConsolAndShipments()
		{
			AddConsol(Core.Constants.AgentType.OnBoardCourier, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 2, 1), "AUSYD", "ITMIL", 1, SYDBranch);
			AddConsol(Core.Constants.AgentType.Direct, true, false, Core.Constants.TransportModes.Sea, new ZDateTime(2006, 2, 10), "AUSYD", "DEFRA", 2, SYDBranch);
			AddConsol(Core.Constants.AgentType.Agent, true, true, Core.Constants.TransportModes.Air, new ZDateTime(2006, 3, 1), "AUSYD", "USATL", 3, SYDBranch);
			AddConsol(Core.Constants.AgentType.Direct, false, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 5, 1), "AUSYD", "IDJKT", 1, SYDBranch);
			AddConsol(Core.Constants.AgentType.Agent, true, false, Core.Constants.TransportModes.Road, new ZDateTime(2006, 5, 2), "AUSYD", "JPTYO", 1, SYDBranch);
			AddConsol(Core.Constants.AgentType.Agent, true, false, Core.Constants.TransportModes.Unknown, new ZDateTime(2006, 5, 3), "AUSYD", "FRPAR", 1, SYDBranch);
			AddConsol(Core.Constants.AgentType.Agent, true, false, Core.Constants.TransportModes.Rail, new ZDateTime(2006, 2, 1), "AUOOL", "ITVAL", 1, ADLBranch);
			AddConsol(Core.Constants.AgentType.Agent, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 5, 10), "AUDRW", "SGSIN", 2, ADLBranch);
			AddConsol(Core.Constants.AgentType.Other, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2005, 12, 31), "AUPER", "USATL", 3, ADLBranch);
			AddConsol(Core.Constants.AgentType.Agent, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 1, 1), "AUADL", "AUMEL", 3, ADLBranch);
			AddConsol(Core.Constants.AgentType.Charter, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 2, 2), "AUMEL", "AUBNE", 2, ADLBranch);
			AddConsol(Core.Constants.AgentType.Other, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 3, 1), "AUMEL", "KRSEL", 3, null);
			AddConsol(Core.Constants.AgentType.Direct, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 5, 1), "ITMIL", "AUMEL", 0, MELBranch);
			AddConsol(Core.Constants.AgentType.Direct, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 5, 2), "GBLON", "AUMEL", 1, OtherCompany.Branches[0]);
			AddConsol(Core.Constants.AgentType.CoLoad, true, false, Core.Constants.TransportModes.Air, new ZDateTime(2006, 2, 1), "AUSYD", "ITMIL", 3, SYDBranch);
		}

		void SetupValidCustomsBrokerageJobs()
		{
			AddDeclaration(true, false, new ZDateTime(2006, 2, 1), SYDBranch);
			AddDeclaration(true, false, new ZDateTime(2006, 1, 1), SYDBranch);
			AddDeclaration(true, false, new ZDateTime(2006, 5, 1), MELBranch);
			AddDeclaration(true, false, new ZDateTime(2006, 1, 2), MELBranch);
			AddDeclaration(true, false, new ZDateTime(2006, 4, 4), ADLBranch);
		}

		void SetupInvalidCustomsBrokerageJobs()
		{
			AddDeclaration(false, false, new ZDateTime(2006, 2, 1), SYDBranch);
			AddDeclaration(true, true, new ZDateTime(2006, 1, 1), MELBranch);
			AddDeclaration(true, false, new ZDateTime(2006, 5, 10), ADLBranch);
			AddDeclaration(true, false, new ZDateTime(2005, 12, 30), SYDBranch);
			AddDeclaration(true, false, new ZDateTime(2006, 1, 3), OtherCompany.Branches[0]);
		}

		void SetupValidWarehouseJobs()
		{
			AddWhsDocket(DocketType.Codes.Order, SYDWarehouse);
			AddWhsDocket(DocketType.Codes.Order, SYDWarehouse);
			AddWhsDocket(DocketType.Codes.Receive, SYDWarehouse);
			AddWhsDocket(DocketType.Codes.Receive, SYDWarehouse);
			AddWhsDocket(DocketType.Codes.Receive, SYDWarehouse);
			AddWhsDocket(DocketType.Codes.Order, MELWarehouse);
			AddWhsDocket(DocketType.Codes.Order, MELWarehouse);
			AddWhsDocket(DocketType.Codes.Order, MELWarehouse);
			AddWhsDocket(DocketType.Codes.Receive, MELWarehouse);
			AddWhsDocket(DocketType.Codes.Receive, ADLWarehouse);
			AddWhsDocket(DocketType.Codes.Order, ADLWarehouse);
			AddWhsDocket(DocketType.Codes.Receive, ADLWarehouse);
		}

		void SetupInvalidWarehouseJobs()
		{
			AddWhsDocket(DocketType.Codes.Adjustment, SYDWarehouse);
			AddWhsDocket(DocketType.Codes.Transfer, SYDWarehouse);
			AddWhsDocket(DocketType.Codes.Receive, null);
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WH1", OtherCompany.Branches[0]);
			AddWhsDocket(DocketType.Codes.Order, warehouse);
		}

		BaseJobDeclaration AddDeclaration(ZBool hasShipment, ZBool isCancelled, ZDateTime createTime, GlbBranch branch)
		{
			BaseJobDeclaration result = Factory.New<BaseJobDeclaration>();
			if (hasShipment)
			{
				result.JE_JS = Factory.New<JASForwardingShipment>().PK;
			}

			result.JE_IsCancelled = isCancelled;
			result.JE_SystemCreateTimeUtc = createTime;
			result.JE_GB = (branch != null) ? branch.PK : ZGuid.Empty;
			return result;
		}

		JASForwardingConsol AddConsol(ZString agentType, ZBool isForwarding, ZBool isCancelled, ZString transportMode, ZDateTime createTime, ZString loadPort, ZString discPort, int noOfShipments, GlbBranch branch)
		{
			JASForwardingConsol result = Factory.New<JASForwardingConsol>();
			result.JK_AgentType = agentType;
			result.JK_IsForwarding = isForwarding;
			result.JK_IsCancelled = isCancelled;
			result.JK_TransportMode = transportMode;
			result.JK_SystemCreateTimeUtc = createTime;
			result.JK_RL_NKLoadPort = loadPort;
			result.JK_RL_NKDischargePort = discPort;
			for (int i = 0; i < noOfShipments; i++)
			{
				JASForwardingShipment shipment = (JASForwardingShipment)result.Shipments.AddNew();
				shipment.FillWithValidTestData();
				if (branch != null)
				{
					JobHeader shipmentJob = Factory.NewJobForTesting<JobHeader>();
					shipmentJob.JH_JobNum = shipment.JS_UniqueConsignRef;
					shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
					shipmentJob.JH_GC = branch.Company.PK;
					shipmentJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
					shipmentJob.JH_ParentID = shipment.PK;
					shipmentJob.JH_GB = branch.PK;
				}
			}

			return result;
		}

		WhsDocket AddWhsDocket(ZString docketType, IWhsWarehouse warehouse)
		{
			var result = Factory.NewWithValidTestData<WhsReceive>();
			result.WD_DocketType = docketType;
			result.WD_DocketSubType = GetValidDocketSubType(docketType);
			if (warehouse != null)
			{
				result.WD_WW_Whs = warehouse.PK;
			}

			return result;
		}

		ZString GetValidDocketSubType(ZString docketType)
		{
			switch (docketType)
			{
				case DocketType.Codes.Receive:
					return ReceiveType.Codes.Receipt;

				case DocketType.Codes.Transfer:
					return TransferType.Codes.Internal;

				case DocketType.Codes.Adjustment:
					return AdjustmentType.Codes.Adjustment;

				case DocketType.Codes.Order:
					return OrderType.Codes.Order;

				case DocketType.Codes.DynamicWorkOrder:
				case DocketType.Codes.WorkOrder:
					return WorkOrderType.Codes.Assemble;

				default:
					throw new InvalidOperationException($"Invalid Warehouse Docket Type was provided, '{docketType}'");
			}
		}

		#endregion
		#region Air Freight Kilos
		public void TestAppendAirFreightKilosLines()
		{
			SetupValidAirFreightWeightRecords();
			SetupInvalidFreightWeightRecords();
			Factory.Save();
			Generator.AppendAirFreightKilosLines(StringBuilderForTest);
			const string ExpectedLinesAsString = @"
Airfreight Kilos,SKGS,ICTOTA,AE,C01,,118.93+,,,
Airfreight Kilos,SKGS,ICTOTA,AI,C01,,4.25+,,,
Airfreight Kilos,SKGS,ICTOTA,AE,C02,,6.50+,,,
Airfreight Kilos,SKGS,ICTOTA,AI,C02,,10.00+,,,
Airfreight Kilos,SKGS,ICTOTA,AE,C03,,0.71+,,,
Airfreight Kilos,SKGS,ICTOTA,AI,C03,,25.59+,,,
";
			AssertMultilineEquals("Should be equal", ExpectedLinesAsString.Trim(), StringBuilderForTest.ToStringWithNewLineBetweenAppends(), '\n');
		}

		void SetupValidAirFreightWeightRecords()
		{
			AddFreightWeightRecord(Core.Constants.TransportModes.Air, true, false, 1m, Core.Constants.Weight.Kilograms, "AUSYD", "ITMIL", new ZDateTime(2006, 1, 1), SYDBranch);
			AddFreightWeightRecord(Core.Constants.TransportModes.AirSea, true, false, 10m, Core.Constants.Weight.Pounds, "AUSYD", "GBLON", new ZDateTime(2006, 3, 1), SYDBranch);
			AddFreightWeightRecord(Core.Constants.TransportModes.SeaAir, true, false, 250m, Core.Constants.Weight.Pounds, "AUSYD", "HKHKG", new ZDateTime(2006, 2, 1), SYDBranch);
			AddFreightWeightRecord(Core.Constants.TransportModes.Air, true, false, 150m, Core.Constants.Weight.Ounces, "USBOS", "AUSYD", new ZDateTime(2006, 1, 2), SYDBranch);
			AddFreightWeightRecord(Core.Constants.TransportModes.Air, true, false, 5m, Core.Constants.Weight.Kilograms, "AUMEL", "SGSIN", new ZDateTime(2006, 5, 1), MELBranch);
			AddFreightWeightRecord(Core.Constants.TransportModes.SeaAir, true, false, 1500m, Core.Constants.Weight.Grams, "AUMEL", "IDJKT", new ZDateTime(2006, 4, 3), MELBranch);
			AddFreightWeightRecord(Core.Constants.TransportModes.AirSea, true, false, 10m, Core.Constants.Weight.Kilograms, "USATL", "AUMEL", new ZDateTime(2006, 2, 2), MELBranch);
			AddFreightWeightRecord(Core.Constants.TransportModes.Air, true, false, 25m, Core.Constants.Weight.Ounces, "AUADL", "THBKK", new ZDateTime(2006, 2, 1), ADLBranch);
			AddFreightWeightRecord(Core.Constants.TransportModes.AirSea, true, false, 10000m, Core.Constants.Weight.Grams, "ITMIL", "AUADL", new ZDateTime(2006, 1, 3), ADLBranch);
			AddFreightWeightRecord(Core.Constants.TransportModes.SeaAir, true, false, 550m, Core.Constants.Weight.Ounces, "ITVAL", "AUADL", new ZDateTime(2006, 4, 3), ADLBranch);
		}

		void SetupInvalidFreightWeightRecords()
		{
			AddFreightWeightRecord(Core.Constants.TransportModes.Sea, true, false, 1m, Core.Constants.Weight.Kilograms, "AUSYD", "ITMIL", new ZDateTime(2006, 1, 1), SYDBranch);
			AddFreightWeightRecord(Core.Constants.TransportModes.Air, false, false, 10m, Core.Constants.Weight.Pounds, "AUADL", "THBKK", new ZDateTime(2006, 1, 1), ADLBranch);
			AddFreightWeightRecord(Core.Constants.TransportModes.SeaAir, true, true, 250m, Core.Constants.Weight.Pounds, "AUMEL", "SGSIN", new ZDateTime(2006, 1, 1), MELBranch);
			AddFreightWeightRecord(Core.Constants.TransportModes.Air, true, false, 5m, Core.Constants.Weight.Kilograms, "SGSIN", "AUMEL", new ZDateTime(2006, 1, 1), null);
			AddFreightWeightRecord(Core.Constants.TransportModes.Air, true, false, 5m, Core.Constants.Weight.Kilograms, "AUMEL", "AUADL", new ZDateTime(2006, 1, 1), MELBranch);
			AddFreightWeightRecord(Core.Constants.TransportModes.Air, true, false, 5m, Core.Constants.Weight.Kilograms, "AUBNE", "AUMEL", new ZDateTime(2006, 1, 1), MELBranch);
			AddFreightWeightRecord(Core.Constants.TransportModes.Air, true, false, 5m, Core.Constants.Weight.Kilograms, "AUBNE", "IDJKT", new ZDateTime(2005, 1, 1), MELBranch);
			AddFreightWeightRecord(Core.Constants.TransportModes.Air, true, false, 5m, Core.Constants.Weight.Kilograms, "AUBNE", "IDJKT", new ZDateTime(2006, 5, 10), MELBranch);
		}

		JASForwardingShipment AddFreightWeightRecord(ZString transportMode, ZBool isForwarding, ZBool isCancelled, ZDecimal weight, ZString unitOfWeight, ZString originPort, ZString destPort, ZDateTime createTime, GlbBranch branch)
		{
			JASForwardingShipment result = Factory.NewWithValidTestData<JASForwardingShipment>();
			result.JS_TransportMode = transportMode;
			result.JS_IsForwardRegistered = isForwarding;
			result.JS_IsCancelled = isCancelled;
			result.JS_ActualWeight = weight;
			result.JS_UnitOfWeight = unitOfWeight;
			result.JS_RL_NKOrigin = originPort;
			result.JS_RL_NKDestination = destPort;
			result.JS_SystemCreateTimeUtc = createTime;
			if (branch != null)
			{
				JobHeader shipmentJob = Factory.NewJobForTesting<JobHeader>();
				shipmentJob.JH_JobNum = result.JS_UniqueConsignRef;
				shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
				shipmentJob.JH_GC = branch.Company.PK;
				shipmentJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				shipmentJob.JH_ParentID = result.PK;
				shipmentJob.JH_GB = branch.PK;
			}

			return result;
		}

		#endregion
		#region TEUs
		public void TestAppendTEUCountLines()
		{
			SetupValidConsolWithContainers();
			SetupInvalidConsolWithContainers();
			Factory.Save();
			Generator.AppendTEUCountLines(StringBuilderForTest);
			const string ExpectedLinesAsString = @"
Oceanfreight TEUs,STEU,ICTOTA,ME,C01,,8.00+,,,
Oceanfreight TEUs,STEU,ICTOTA,MI,C01,,2.00+,,,
Oceanfreight TEUs,STEU,ICTOTA,NV,C01,,0,,,
Oceanfreight TEUs,STEU,ICTOTA,ME,C02,,14.60+,,,
Oceanfreight TEUs,STEU,ICTOTA,MI,C02,,4.00+,,,
Oceanfreight TEUs,STEU,ICTOTA,NV,C02,,0,,,
Oceanfreight TEUs,STEU,ICTOTA,ME,C03,,5.00+,,,
Oceanfreight TEUs,STEU,ICTOTA,MI,C03,,0,,,
Oceanfreight TEUs,STEU,ICTOTA,NV,C03,,8.60+,,,
";
			AssertMultilineEquals("Should be equal", ExpectedLinesAsString.Trim(), StringBuilderForTest.ToStringWithNewLineBetweenAppends(), '\n');
		}

		void SetupValidConsolWithContainers()
		{
			AddConsolWithContainers(Core.Constants.AgentType.Agent, Core.Constants.TransportModes.Sea, true, false, new ZDateTime(2006, 2, 14), "AUSYD", "IDJKT", SYDBranch, "20NOR", "40FR", "20FR");
			AddConsolWithContainers(Core.Constants.AgentType.Charter, Core.Constants.TransportModes.Sea, true, false, new ZDateTime(2006, 2, 15), "DEFRA", "AUSYD", SYDBranch, "20GP");
			AddConsolWithContainers(Core.Constants.AgentType.Other, Core.Constants.TransportModes.Sea, true, false, new ZDateTime(2006, 2, 16), "AUMEL", "CAYTO", MELBranch, "40HC", "20GP", "40NOR", "40PL");
			AddConsolWithContainers(Core.Constants.AgentType.Agent, Core.Constants.TransportModes.Sea, true, false, new ZDateTime(2006, 2, 17), "CNSHA", "AUMEL", MELBranch, "20PL", "20RE");
			AddConsolWithContainers(Core.Constants.AgentType.Charter, Core.Constants.TransportModes.Sea, true, false, new ZDateTime(2006, 2, 18), "AUADL", "CAYTO", ADLBranch, "45HC");
			AddConsolWithContainers(Core.Constants.AgentType.CoLoad, Core.Constants.TransportModes.Sea, true, false, new ZDateTime(2006, 2, 19), "AUADL", "USBOS", ADLBranch, "40REHC", "40OT");
		}

		void SetupInvalidConsolWithContainers()
		{
			AddConsolWithContainers(Core.Constants.AgentType.Direct, Core.Constants.TransportModes.Sea, true, false, new ZDateTime(2006, 2, 14), "AUSYD", "IDJKT", SYDBranch, "20NOR", "40FR", "20FR");
			AddConsolWithContainers(Core.Constants.AgentType.Charter, Core.Constants.TransportModes.Air, true, false, new ZDateTime(2006, 2, 15), "DEFRA", "AUSYD", SYDBranch, "20NOR", "40FR", "20FR");
			AddConsolWithContainers(Core.Constants.AgentType.Other, Core.Constants.TransportModes.Sea, false, false, new ZDateTime(2006, 2, 16), "AUMEL", "CAYTO", MELBranch, "40HC", "20GP", "40NOR", "40PL");
			AddConsolWithContainers(Core.Constants.AgentType.Agent, Core.Constants.TransportModes.Sea, true, true, new ZDateTime(2006, 2, 17), "CNSHA", "AUMEL", MELBranch, "20PL", "20RE");
			AddConsolWithContainers(Core.Constants.AgentType.Charter, Core.Constants.TransportModes.Sea, true, false, new ZDateTime(2005, 2, 18), "AUADL", "CAYTO", ADLBranch, "45HC");
			AddConsolWithContainers(Core.Constants.AgentType.CoLoad, Core.Constants.TransportModes.Sea, true, false, new ZDateTime(2006, 12, 19), "AUADL", "USBOS", ADLBranch, "40REHC", "40OT");
			AddConsolWithContainers(Core.Constants.AgentType.Charter, Core.Constants.TransportModes.Sea, true, false, new ZDateTime(2006, 2, 18), "AUADL", "AUMEL", ADLBranch, "45HC");
			AddConsolWithContainers(Core.Constants.AgentType.CoLoad, Core.Constants.TransportModes.Sea, true, false, new ZDateTime(2006, 2, 19), "AUADL", "USBOS", null, "40REHC", "40OT");
			AddConsolWithContainers(Core.Constants.AgentType.CoLoad, Core.Constants.TransportModes.Sea, true, false, new ZDateTime(2006, 2, 19), "AUADL", "USBOS", null);
		}

		JASForwardingConsol AddConsolWithContainers(ZString agentType, ZString transportMode, ZBool isForwarding, ZBool isCancelled, ZDateTime createTime, ZString loadPort, ZString discPort, GlbBranch branch, params ZString[] containerTypes)
		{
			JASForwardingConsol result = Factory.New<JASForwardingConsol>();
			result.JK_AgentType = agentType;
			result.JK_TransportMode = transportMode;
			result.JK_IsForwarding = isForwarding;
			result.JK_IsCancelled = isCancelled;
			result.JK_SystemCreateTimeUtc = createTime;
			result.JK_RL_NKLoadPort = loadPort;
			result.JK_RL_NKDischargePort = discPort;
			if (branch != null)
			{
				JASForwardingShipment shipment = (JASForwardingShipment)result.Shipments.AddNew();
				shipment.FillWithValidTestData();
				shipment.JS_SystemCreateTimeUtc = createTime;
				JobHeader shipmentJob = Factory.NewJobForTesting<JobHeader>();
				shipmentJob.JH_JobNum = shipment.JS_UniqueConsignRef;
				shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
				shipmentJob.JH_GC = branch.Company.PK;
				shipmentJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				shipmentJob.JH_ParentID = shipment.PK;
				shipmentJob.JH_GB = branch.PK;
			}

			foreach (ZString containerType in containerTypes)
			{
				RefContainer refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType);
				ForwardingContainer container = result.Containers.AddNew();
				container.JC_ContainerCount = 2;
				container.JC_RC = refContainer.PK;
			}

			return result;
		}

		#endregion
		#region Total Invoiced
		public void TestAppendTotalInvoiceCountLines()
		{
			SetupValidInvoices();
			SetupInvalidInvoices();
			Factory.Save();
			Generator.AppendTotalInvoiceCountLines(StringBuilderForTest);
			const string ExpectedLinesAsString = @"
Total Invoiced,TI,ICTOTA,,C01,,187.00+,,,
Total Invoiced,TI,ICTOTA,,C02,,297.00+,,,
Total Invoiced,TI,ICTOTA,,C03,,220.00+,,,
";
			AssertMultilineEquals("Should be equal", ExpectedLinesAsString.Trim(), StringBuilderForTest.ToStringWithNewLineBetweenAppends(), '\n');
		}

		void SetupValidInvoices()
		{
			AddNewInvoice<JASARInvoice>(false, 20m, new ZDateTime(2006, 1, 9), SYDBranch);
			AddNewInvoice<JASARInvoice>(false, 50m, new ZDateTime(2006, 1, 10), SYDBranch);
			AddNewInvoice<JASARAdjustmentNote>(false, 100m, new ZDateTime(2006, 1, 11), SYDBranch);
			AddNewInvoice<JASARInvoice>(false, 120m, new ZDateTime(2006, 1, 2), MELBranch);
			AddNewInvoice<JASARInvoice>(false, 150m, new ZDateTime(2006, 1, 3), MELBranch);
			AddNewInvoice<JASARAdjustmentNote>(false, 200m, new ZDateTime(2006, 1, 4), ADLBranch);
		}

		void SetupInvalidInvoices()
		{
			AddNewInvoice<APInvoice>(false, 20m, new ZDateTime(2006, 1, 9), SYDBranch);
			AddNewInvoice<JASARInvoice>(true, 50m, new ZDateTime(2006, 1, 10), SYDBranch);
			AddNewInvoice<JASARAdjustmentNote>(false, -100m, new ZDateTime(2006, 1, 11), SYDBranch);
			AddNewInvoice<JASARInvoice>(false, 150m, new ZDateTime(2005, 1, 3), MELBranch);
			AddNewInvoice<JASARAdjustmentNote>(false, 200m, new ZDateTime(2006, 11, 4), ADLBranch);
			AddNewInvoice<JASARAdjustmentNote>(false, 200m, new ZDateTime(2006, 5, 4), OtherCompany.Branches[0]);
		}

		T AddNewInvoice<T>(ZBool isCancelled, ZDecimal invoiceAmount, ZDateTime postDate, GlbBranch branch)
			where T : InvoicingBase
		{
			T result = Factory.NewWithValidTestData<T>();
			result.AH_IsCancelled = isCancelled;
			if (isCancelled)
			{
				((IMatching)result).CurrentMatchGroup.AddNew().AP_AH = result.PK;
				TestObjectCreator.SetupMatchLinkMatchDate(result);
			}

			result.AH_PostDate = postDate;
			result.AH_GB = (branch != null) ? branch.PK : ZGuid.Empty;
			TestObjectCreator.CreateInvoiceLine(result, result.TransactionCurrency, result.AH_ExchangeRate, invoiceAmount, invoiceAmount * 0.1m, 0m);
			return result;
		}

		#endregion
		public void TestNormalShipmentAgentTypes()
		{
			AssertEquals(3, Generator.NormalShipmentAgentTypes.Length);
			Assert(((IList<string>)Generator.NormalShipmentAgentTypes).Contains(Core.Constants.AgentType.Agent));
			Assert(((IList<string>)Generator.NormalShipmentAgentTypes).Contains(Core.Constants.AgentType.Charter));
			Assert(((IList<string>)Generator.NormalShipmentAgentTypes).Contains(Core.Constants.AgentType.Other));
		}

		public void TestAppendCognosLine()
		{
			Generator.AppendCognosLine(StringBuilderForTest, "SHOUSES", "Blah Blah MEH MEH", "SYD", CognosModes.AE, 20.38m);
			Generator.AppendCognosLine(StringBuilderForTest, "SDIRECT", "Foo Bar", "BNE", CognosModes.WPT, (ZInt)13);
			Generator.AppendCognosLine(StringBuilderForTest, "SMASTERS", "Go Socceroos!", "MEL", CognosModes.Empty, 384m);
			const string ExpectedLinesAsString = @"
Blah Blah MEH MEH,SHOUSES,ICTOTA,AE,SYD,,20.38+,,,
Foo Bar,SDIRECT,ICTOTA,WPT,BNE,,13.00+,,,
Go Socceroos!,SMASTERS,ICTOTA,,MEL,,384.00+,,,
";
			AssertMultilineEquals("Should be equal", ExpectedLinesAsString.Trim(), StringBuilderForTest.ToStringWithNewLineBetweenAppends(), '\n');
		}

		#region TestActiveBranchCollection
		public void TestActiveBranchCollection()
		{
			AddActiveBranchesFromOtherCompany();
			AddNonActiveBranchesFromCurrentCompany();
			AssertEquals("Should not include non active branches or branches from other company", 3, Generator.ActiveBranchCollection.Count);
			Generator.ActiveBranchCollection.Sort(GlbBranchSchema.Constants.GB_Code, ListSortDirection.Ascending);
			Assert(Generator.ActiveBranchCollection.Contains(SYDBranch.PK));
			Assert(Generator.ActiveBranchCollection.Contains(MELBranch.PK));
			Assert(Generator.ActiveBranchCollection.Contains(ADLBranch.PK));
		}

		void AddActiveBranchesFromOtherCompany()
		{
			GlbBranch activeBranchFromOtherCompany1 = Factory.NewWithValidTestData<GlbBranch>();
			activeBranchFromOtherCompany1.GB_Code = "OT1";
			activeBranchFromOtherCompany1.GB_GC = OtherCompany.PK;
			activeBranchFromOtherCompany1.GB_IsActive = true;
			GlbBranch activeBranchFromOtherCompany2 = Factory.NewWithValidTestData<GlbBranch>();
			activeBranchFromOtherCompany2.GB_Code = "OT2";
			activeBranchFromOtherCompany2.GB_GC = OtherCompany.PK;
			activeBranchFromOtherCompany2.GB_IsActive = true;
			GlbBranch activeBranchFromOtherCompany3 = Factory.NewWithValidTestData<GlbBranch>();
			activeBranchFromOtherCompany3.GB_Code = "OT3";
			activeBranchFromOtherCompany3.GB_GC = OtherCompany.PK;
			activeBranchFromOtherCompany3.GB_IsActive = true;
			Factory.Save();
		}

		void AddNonActiveBranchesFromCurrentCompany()
		{
			GlbBranch nonActiveBranchFromCurrentCompany1 = Factory.NewWithValidTestData<GlbBranch>();
			nonActiveBranchFromCurrentCompany1.GB_Code = "NA1";
			nonActiveBranchFromCurrentCompany1.GB_GC = GlbCompany.CurrentCompany.PK;
			nonActiveBranchFromCurrentCompany1.GB_IsActive = false;
			GlbBranch nonActiveBranchFromCurrentCompany2 = Factory.NewWithValidTestData<GlbBranch>();
			nonActiveBranchFromCurrentCompany2.GB_Code = "NA2";
			nonActiveBranchFromCurrentCompany2.GB_GC = GlbCompany.CurrentCompany.PK;
			nonActiveBranchFromCurrentCompany2.GB_IsActive = false;
			GlbBranch nonActiveBranchFromCurrentCompany3 = Factory.NewWithValidTestData<GlbBranch>();
			nonActiveBranchFromCurrentCompany3.GB_Code = "NA3";
			nonActiveBranchFromCurrentCompany3.GB_GC = GlbCompany.CurrentCompany.PK;
			nonActiveBranchFromCurrentCompany3.GB_IsActive = false;
			Factory.Save();
		}

		#endregion
		public void TestGetDbCommand()
		{
			DbCommand command = Generator.GetDbCommand("MEH");
			AssertEquals("Timeout should be set to 600 seconds", 600, command.CommandTimeout);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			SetupNewCompanyAndActiveBranches();
			SetupCognosModesMapping();
			Factory.Save();
			userContextChange = Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, TestBranchPK, Env.CurrentDepartment.PK);
			OtherCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "DEM");
		}

		protected override void TearDown()
		{
			userContextChange.Dispose();
			base.TearDown();
		}

		void SetupNewCompanyAndActiveBranches()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "CGN";
			SYDBranch = company.Branches.AddNew();
			SYDBranch.FillWithValidTestData();
			SYDBranch.GB_Code = "C01";
			SYDBranch.GB_RL_NKHomePort = "AUSYD";
			MELBranch = company.Branches.AddNew();
			MELBranch.FillWithValidTestData();
			MELBranch.GB_Code = "C02";
			MELBranch.GB_RL_NKHomePort = "AUMEL";
			ADLBranch = company.Branches.AddNew();
			ADLBranch.FillWithValidTestData();
			ADLBranch.GB_Code = "C03";
			ADLBranch.GB_RL_NKHomePort = "AUADL";
			TestBranchPK = SYDBranch.PK.ToGuid();
		}

		void SetupCognosModesMapping()
		{
			CognosModeMapping mapping = JASDataRegistry.Instance.CognosModeMapping;
			mapping.SelectedMode = nameof(CognosModes.AE);
			mapping.MapDepartments(GetDeptFromCode("LEA"), GetDeptFromCode("FEA"), GetDeptFromCode("CEA"));
			mapping.SelectedMode = nameof(CognosModes.AI);
			mapping.MapDepartments(GetDeptFromCode("LIA"), GetDeptFromCode("FIA"), GetDeptFromCode("CIA"));
			mapping.SelectedMode = nameof(CognosModes.ME);
			mapping.MapDepartments(GetDeptFromCode("LES"), GetDeptFromCode("FES"), GetDeptFromCode("CES"));
			mapping.SelectedMode = nameof(CognosModes.MI);
			mapping.MapDepartments(GetDeptFromCode("LIS"), GetDeptFromCode("FIS"), GetDeptFromCode("CIS"));
			mapping.SelectedMode = nameof(CognosModes.NV);
			mapping.MapDepartments(GetDeptFromCode("DIR"));
			mapping.SelectedMode = nameof(CognosModes.WPT);
			mapping.MapDepartments(GetDeptFromCode("WBS"));
			mapping.SelectedMode = nameof(CognosModes.PR);
			mapping.MapDepartments(GetDeptFromCode("MSJ"));
			mapping.SelectedMode = nameof(CognosModes.OTH);
			mapping.MapDepartments(GetDeptFromCode("BRN"));
			mapping.SelectedMode = nameof(CognosModes.CHB);
			mapping.MapDepartments(GetDeptFromCode("CPP"));
			JASDataRegistry.Instance.CognosModeMappingItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mapping);
		}

		GlbDepartment GetDeptFromCode(string deptCode)
		{
			return Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, deptCode);
		}

		ZStringBuilder StringBuilderForTest
		{
			get
			{
				if (fStringBuilderForTest == null)
				{
					fStringBuilderForTest = new ZStringBuilder();
				}

				return fStringBuilderForTest;
			}
		}

		NonAccountingCognosLineGenerator Generator
		{
			get
			{
				if (fGenerator == null)
				{
					fGenerator = new NonAccountingCognosLineGenerator(Factory, new ZDateTime(2006, 5, 5));
				}

				return fGenerator;
			}
		}

		IWhsTransactionTestHelper Helper
		{
			get { return helper ?? (helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory)); }
		}

		IWhsWarehouse ADLWarehouse
		{
			get
			{
				if (adlWarehouse == null)
				{
					adlWarehouse = (IWhsWarehouse)Helper.CreateWarehouse("ADL", ADLBranch);
				}

				return adlWarehouse;
			}
		}

		IWhsWarehouse MELWarehouse
		{
			get
			{
				if (melWarehouse == null)
				{
					melWarehouse = (IWhsWarehouse)Helper.CreateWarehouse("MEL", MELBranch);
				}

				return melWarehouse;
			}
		}

		IWhsWarehouse SYDWarehouse
		{
			get
			{
				if (sydWarehouse == null)
				{
					sydWarehouse = (IWhsWarehouse)Helper.CreateWarehouse("SYD", SYDBranch);
				}

				return sydWarehouse;
			}
		}

		IWhsTransactionTestHelper helper;
		ZStringBuilder fStringBuilderForTest;
		NonAccountingCognosLineGenerator fGenerator;
		GlbBranch SYDBranch;
		GlbBranch MELBranch;
		GlbBranch ADLBranch;
		IDisposable userContextChange;
		Guid TestBranchPK;
		GlbCompany OtherCompany;
		IWhsWarehouse adlWarehouse;
		IWhsWarehouse melWarehouse;
		IWhsWarehouse sydWarehouse;
		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get
			{
				return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
			}
		}
		#endregion
	}
}
