using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.UPE.Testing;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterData.Business.Tests;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.Asycuda;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;
using AsycudaBill = Enterprise.Customs.SG.Access.Business.AsycudaBill;
using AsycudaManifestHeader = Enterprise.Customs.SG.Access.Business.AsycudaManifestHeader;
using AsycudaPack = Enterprise.Customs.SG.Access.Business.AsycudaPack;
using ClassificationTypeList = Enterprise.Customs.Business.ClassificationTypeList;
using OrgSupplierPart = Enterprise.Customs.SG.V4.Business.OrgSupplierPart;
using UniversalReferenceTestDataHelper = Enterprise.Customs.SG.V4.Business.Testing.UniversalReferenceTestDataHelper;

namespace Enterprise.Client.UPE.Business.DataImport
{
	sealed class Level1DataFileImporterForSGAccessTest : TestCaseWithFactory
	{
		public void TestLoadGCCShipment_ExcludeGCCChildShipment()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "CS00676853-GCC Level 1 file.txt");
			Importer.LoadFile();
			Importer.Save();

			CombineAssertions(() =>
			{
				AssertEquals(2, Importer.TotalNoOfShipments);
				AssertEquals(17, Importer.TotalPiecesManifested);
				AssertEquals(0, Importer.TotalNoOfChildPackages);
				AssertEquals(100, Importer.PercentageOfIncorrectPorts);
			});
		}

		public void TestLoadGCCShipment_ExcludeGCCLeadShipmentWithNull401000()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "GCC VL Level 1 file_401000 is null.txt");
			Importer.LoadFile();
			Importer.Save();

			CombineAssertions(() =>
			{
				AssertEquals(0, Importer.TotalNoOfShipments);
				AssertEquals(0, Importer.TotalPiecesManifested);
				AssertEquals(0, Importer.TotalNoOfChildPackages);
				AssertEquals(0, Importer.PercentageOfIncorrectPorts);
			});
		}

		public void TestLoadGCCShipment_ExcludeGCCLeadShipmentWithEmptyLeadTrackingNumberForGCCShipment()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "GCC VL Level 1 file_LeadTrackingNumberForGCCShipment is empty.txt");
			Importer.LoadFile();
			Importer.Save();

			CombineAssertions(() =>
			{
				AssertEquals(0, Importer.TotalNoOfShipments);
				AssertEquals(0, Importer.TotalPiecesManifested);
				AssertEquals(0, Importer.TotalNoOfChildPackages);
				AssertEquals(0, Importer.PercentageOfIncorrectPorts);
			});
		}

		public void TestLoadGCCShipment_Use401000LeadTrackingNumberForGCCShipmentAsBillNumberForGCCParentShipments()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "GCC VL Level 1 file.txt");
			Importer.LoadFile();
			Importer.Save();

			CombineAssertions(() =>
			{
				AssertEquals(1, Importer.TotalNoOfShipments);
				AssertEquals(5, Importer.TotalPiecesManifested);
				AssertEquals(0, Importer.TotalNoOfChildPackages);
				AssertEquals(100, Importer.PercentageOfIncorrectPorts);

				var manifestHeader = GetManifestHeader("08122222222");
				var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();

				AssertEquals(1, bills.Length);
				AssertEquals("bill.ABL_BillNumber", "23E0X1HCF9G", bills[0].ABL_BillNumber);
			});
		}

		public void TestLoadGCCShipment_Use401000TotalPackageCountForGCCShipmentAsTotalPackagesNumberForGCCParentShipments()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "GCC VL Level 1 file.txt");
			Importer.LoadFile();
			Importer.Save();

			CombineAssertions(() =>
			{
				AssertEquals(1, Importer.TotalNoOfShipments);
				AssertEquals(5, Importer.TotalPiecesManifested);
				AssertEquals(0, Importer.TotalNoOfChildPackages);
				AssertEquals(100, Importer.PercentageOfIncorrectPorts);

				var manifestHeader = GetManifestHeader("08122222222");
				var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();
				AssertEquals(1, bills.Length);
				AssertEquals("bill.TotalNoOfPacks", 5, bills[0].ABL_ManifestQty);
			});
		}

		public void TestLoadFileAndSave()
		{
			Importer.LoadFile();
			Importer.Save();
			var manifestHeader = GetManifestHeader("08122222222");
			CombineAssertions(() =>
			{
				AssertEquals("TotalNoOfShipments", 3, Importer.TotalNoOfShipments);
				AssertEquals("TotalNoOfChildPackages", 2, Importer.TotalNoOfChildPackages);
				AssertEquals("TotalPiecesManifested", 6, Importer.TotalPiecesManifested);
				AssertEquals("PercentageOfDuplicateHAWBs", 0, Importer.PercentageOfDuplicateHAWBs);
				AssertEquals("PercentageOfIncorrectPorts", 33, Importer.PercentageOfIncorrectPorts);
				AssertMultilineASCIIEquals("IncorrectChildPacks", @"1Z3824AR6640806327: 0 childpack/s, 1 expected
1Z3947806616452202: 2 childpack/s, 1 expected
1Z4A14T96650435917: 0 childpack/s, 1 expected",
new ZStringBuilder(Importer.IncorrectChildPacks.OfType<string>()).ToStringWithNewLineBetweenAppends());
				AssertEquals("IncorrectPortOfDestinationList", "1Z4A14T96650435917 PortCode: AU9641", new ZStringBuilder(Importer.IncorrectPortOfDestinationList.OfType<string>()).ToString());
				AssertEquals("EmptyShipmentsList", ZString.Empty, new ZStringBuilder(Importer.EmptyShipmentsList.OfType<string>()).ToString());
				AssertEquals("manifestHeader.AMA_ApplicationCode", ApplicationCodeTypeList.Codes.Consolidator, manifestHeader.AMA_ApplicationCode);
				AssertEquals("manifestHeader.AMA_TransportMode", Level1DataFileImporterForSGAccess.Constants.TransportMode.Air, manifestHeader.AMA_TransportMode);
				AssertEquals("manifestHeader.AMA_Voyage", Level1DataImport.FlightNumber, manifestHeader.AMA_Voyage);
				AssertEquals("manifestHeader.AMA_RL_NKPortOfLoading", Level1DataImport.PortOfLoading, manifestHeader.AMA_RL_NKPortOfLoading);
				AssertEquals("manifestHeader.AMA_RL_NKPortOfDischarge", Level1DataImport.PortOfDischarge, manifestHeader.AMA_RL_NKPortOfDischarge);
				AssertEquals("manifestHeader.AMA_E_DEP", Level1DataImport.DepartureDate, manifestHeader.AMA_E_DEP);
				AssertEquals("manifestHeader.AMA_E_ARV", Level1DataImport.ArrivalDate, manifestHeader.AMA_E_ARV);
				AssertEquals("manifestHeader.AMA_RN_NKCountry", Core.Constants.CountryCodes.Singapore, manifestHeader.AMA_RN_NKCountry);
				AssertEquals("manifestHeader.AMA_ManifestType", Level1DataFileImporterForSGAccess.Constants.ManifestType.Import, manifestHeader.AMA_ManifestType);
				AssertEquals("manifestHeader.AMA_Nature", Level1DataFileImporterForSGAccess.Constants.Nature.Import, manifestHeader.AMA_Nature);
				AssertEquals("manifestHeader.AMA_RL_NKPortOfFirstArrival", manifestHeader.AMA_RL_NKPortOfDischarge, manifestHeader.AMA_RL_NKPortOfFirstArrival);
			});
			var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();

			AssertEquals("bills.Length", 3, bills.Length);
			AssertBill(bills[0], "3824ARFY9JH", 1021.58m, "USLAX", "SGSIN", 8.618m, Core.Constants.Weight.Kilograms, "ADVANCED SHEN MIN FOR WOMEN", 0m, Core.Constants.Volume.CubicMetres, 2, "PKG",
				ZGuid.Empty, "SNOWSILL, SONYA", "15 LEANDER ST", "CONSIGNEE STR 2", "FALCON", "", "6210", "+61895343637", "AU",
				ZGuid.Empty, "BIOTECH CORP", "107 OAKWOOD DRIVE", "SHIPPER STREET 3", "GLASTONBURY", "CT", "060332481", "US",
				"SGD", 1300m, "USD", 150m, "USD", 50m, "USD", 20m, "USD", 100m, "USD", 0m, 71.52m, ZDateTime.Empty, ZString.Empty);
			AssertEquals("Packs.Count", 2, bills[0].Packs.Count);
			AssertPack(bills[0].Packs[0], "ADVANCED SHEN MIN FOR WOMEN", "", 6, "PCE", 0m, "", 0m, "",
				"US", "", 471.50m, 0m, 33.01m, Level1DataFileImporterForSGAccess.Constants.GoodsType.NormalGoods, 6m, "NMB", 600m, "USD", ZString.Empty);
			AssertPack(bills[0].Packs[1], "ADVANCED SHEN MIN FOR MEN", "", 6, "PCE", 0m, "", 0m, "",
				"US", "", 550.08m, 0m, 38.51m, Level1DataFileImporterForSGAccess.Constants.GoodsType.NormalGoods, 6m, "NMB", 700m, "USD", ZString.Empty);

			AssertBill(bills[1], "3947808NNSR", 89.93m, "USLAX", "SGSIN", 4.990m, Core.Constants.Weight.Kilograms, "COMPANY PERFORMANCE & MEASURES", 0m, "", 2, "PKG",
					ZGuid.Empty, "UNIT 7 5-7 COWPER ST", "PARRAMATTA", "NSW", "SYDNEY", "", "2150", "0061292271338", "AU",
					ZGuid.Empty, "DISTRIBUTION CENTER", "1650 BLUEGRASS LAKES PKWY", "", "ALPHARETTA", "GA", "30004", "US",
					"SGD", 125m, "USD", 0m, "USD", 0m, "SGD", 0m, "USD", 0m, "USD", 0m, 6.30m, new ZDateTime(2017, 6, 15), "5");
			AssertEquals("Packs.Count", 3, bills[1].Packs.Count);
			AssertPack(bills[1].Packs[0], "COMPANY PERFORMANCE & MEASURES", "", 1, "PCE", 0m, "", 0m, "",
				"US", "", 14.39m, 0m, 1.01m, Level1DataFileImporterForSGAccess.Constants.GoodsType.NormalGoods, 1m, "NMB", 20m, "USD", ZString.Empty);
			AssertPack(bills[1].Packs[1], "ANALYSIS OF DERIVATIVES FOR TH", "", 1, "PCE", 0m, "", 0m, "",
				"US", "", 43.17m, 0m, 3.02m, Level1DataFileImporterForSGAccess.Constants.GoodsType.NormalGoods, 1m, "NMB", 60m, "USD", ZString.Empty);
			AssertPack(bills[1].Packs[2], "LEVEL II CFA 2004 READINGS", "", 1, "PCE", 0m, "", 0m, "",
				"US", "", 32.37m, 0m, 2.27m, Level1DataFileImporterForSGAccess.Constants.GoodsType.NormalGoods, 1m, "NMB", 45m, "USD", ZString.Empty);

			AssertBill(bills[2], "4A14T9J3YYD", 33.81m, "USLAX", "9641*", 2.722m, Core.Constants.Weight.Kilograms, "", 0m, Core.Constants.Volume.CubicMetres, 2, "PKG",
					ZGuid.Empty, "", "", "", "", "", "", "", "",
					ZGuid.Empty, "CAFEPRESS.COM", "26010 EDEN LANDING ROAD", "SUITE 5", "HAYWARD", "CA", "94545", "US",
					"SGD", 0m, "USD", 47m, "USD", 0m, "SGD", 0m, "USD", 0m, "USD", 0m, 2.37m, new ZDateTime(2017, 6, 15), "5");
			AssertEquals("Packs.Count", 0, bills[2].Packs.Count);
			AssertContains("Successfully saved Manifest Header MAN0000001 with 3 Bills.", NotificationBuffer.AsString);
		}

		public void TestLoadFileAndSave_Log()
		{
			var mock = new Mock<Level1DataFileImporterForSGAccess>(Level1DataImport, NotificationBuffer) { CallBase = true };
			mock.Setup(m => m.PublishAccessEvents(
				It.IsAny<BusinessObjectFactory>(), It.IsAny<ITopLevelDataObject>(), It.IsAny<IXmlSessionTracker>()))
			.Returns<BusinessObjectFactory, ITopLevelDataObject, IXmlSessionTracker>((factory, topDataObject, sessionTracker) =>
			{
				return new PublishUniversalXmlResult(Array.Empty<UniversalDataBuss.DataObjects.Universal.Event>());
			});
			var importer = mock.Object;
			importer.LoadFile();
			importer.Save();
			AssertContains("Failed to publish Universal XML.", importer.NotificationLogger.ToString());

			mock = new Mock<Level1DataFileImporterForSGAccess>(Level1DataImport, NotificationBuffer) { CallBase = true };
			mock.Setup(m => m.FindManifestHeaderIfExists(It.IsAny<PublishToUniversalResult>()))
			.Returns<PublishToUniversalResult>(publishResult =>
			{
				return null;
			});
			importer = mock.Object;
			importer.LoadFile();
			importer.Save();
			AssertContains("Could not find new created Manifest Header.", importer.NotificationLogger.ToString());
		}

		public void TestLoadFileWithCustomsQtyFromAddInfo()
		{
			Importer.LoadFile();
			Importer.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals("manifestHeader.AMA_ApplicationCode", ApplicationCodeTypeList.Codes.Consolidator, manifestHeader.AMA_ApplicationCode);
			AssertEquals("manifestHeader.AMA_TransportMode", Level1DataFileImporterForSGAccess.Constants.TransportMode.Air, manifestHeader.AMA_TransportMode);
			AssertEquals("manifestHeader.AMA_Voyage", Level1DataImport.FlightNumber, manifestHeader.AMA_Voyage);
			AssertEquals("manifestHeader.AMA_RL_NKPortOfLoading", Level1DataImport.PortOfLoading, manifestHeader.AMA_RL_NKPortOfLoading);
			AssertEquals("manifestHeader.AMA_RL_NKPortOfDischarge", Level1DataImport.PortOfDischarge, manifestHeader.AMA_RL_NKPortOfDischarge);

			var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();
			AssertEquals("bills.Length", 3, bills.Length);
			AssertBill(bills[0], "3824ARFY9JH", 1021.58m, "USLAX", "SGSIN", 8.618m, Core.Constants.Weight.Kilograms, "ADVANCED SHEN MIN FOR WOMEN", 0m, Core.Constants.Volume.CubicMetres, 2, "PKG",
				ZGuid.Empty, "SNOWSILL, SONYA", "15 LEANDER ST", "CONSIGNEE STR 2", "FALCON", "", "6210", "+61895343637", "AU",
				ZGuid.Empty, "BIOTECH CORP", "107 OAKWOOD DRIVE", "SHIPPER STREET 3", "GLASTONBURY", "CT", "060332481", "US",
				"SGD", 1300m, "USD", 150m, "USD", 50m, "USD", 20m, "USD", 100m, "USD", 0m, 71.52m, ZDateTime.Empty, ZString.Empty);
			AssertEquals("Packs.Count", 2, bills[0].Packs.Count);
			AssertPack(bills[0].Packs[0], "ADVANCED SHEN MIN FOR WOMEN", "", 6, "PCE", 0m, "", 0m, "",
				"US", "", 471.50m, 0m, 33.01m, Level1DataFileImporterForSGAccess.Constants.GoodsType.NormalGoods, 6m, "NMB", 600m, "USD", ZString.Empty);
			AssertPack(bills[0].Packs[1], "ADVANCED SHEN MIN FOR MEN", "", 6, "PCE", 0m, "", 0m, "",
				"US", "", 550.08m, 0m, 38.51m, Level1DataFileImporterForSGAccess.Constants.GoodsType.NormalGoods, 6m, "NMB", 700m, "USD", ZString.Empty);

			AssertBill(bills[1], "3947808NNSR", 89.93m, "USLAX", "SGSIN", 4.990m, Core.Constants.Weight.Kilograms, "COMPANY PERFORMANCE & MEASURES", 0m, "", 2, "PKG",
				ZGuid.Empty, "UNIT 7 5-7 COWPER ST", "PARRAMATTA", "NSW", "SYDNEY", "", "2150", "0061292271338", "AU",
				ZGuid.Empty, "DISTRIBUTION CENTER", "1650 BLUEGRASS LAKES PKWY", "", "ALPHARETTA", "GA", "30004", "US",
				"SGD", 125m, "USD", 0m, "USD", 0m, "SGD", 0m, "USD", 0m, "USD", 0m, 6.30m, new ZDateTime(2017, 6, 15), "5");
			AssertEquals("Packs.Count", 3, bills[1].Packs.Count);
			AssertPack(bills[1].Packs[0], "COMPANY PERFORMANCE & MEASURES", "", 1, "PCE", 0m, "", 0m, "",
				"US", "", 14.39m, 0m, 1.01m, Level1DataFileImporterForSGAccess.Constants.GoodsType.NormalGoods, 1m, "NMB", 20m, "USD", ZString.Empty);
			AssertPack(bills[1].Packs[1], "ANALYSIS OF DERIVATIVES FOR TH", "", 1, "PCE", 0m, "", 0m, "",
				"US", "", 43.17m, 0m, 3.02m, Level1DataFileImporterForSGAccess.Constants.GoodsType.NormalGoods, 1m, "NMB", 60m, "USD", ZString.Empty);
			AssertPack(bills[1].Packs[2], "LEVEL II CFA 2004 READINGS", "", 1, "PCE", 0m, "", 0m, "",
				"US", "", 32.37m, 0m, 2.27m, Level1DataFileImporterForSGAccess.Constants.GoodsType.NormalGoods, 1m, "NMB", 45m, "USD", ZString.Empty);
		}

		[TestDate(2017, 11, 2)]
		public void TestLoadFileAndSave_DataLookup()
		{
			var shipper = CreateOrg("BIOTECH CORP", "SHIPTEST1", "3824AR");
			var consignee = CreateOrg("SNOWSILL, SONYA", "CGNTEST1", "8AU0495926");

			CreatePart("PART1", consignee, "10203040");
			CreatePart("PART2", consignee, "40506070");
			Factory.Save();

			CreateTariff("10203040", CommodityTypeList.Codes.Petroleum, ZDecimal.Zero, 20m, "PCE", ZBool.False, new ZDateTime(2017, 11, 1), new ZDateTime(2017, 11, 3)); //Not used as the Line Effective Date is not valid for this range
			CreateTariff("10203040", CommodityTypeList.Codes.Petroleum, ZDecimal.Zero, 15m, "PCE", ZBool.False, new ZDateTime(2017, 6, 12), new ZDateTime(2017, 6, 14)); //Is used as the Line Effective Date is valid for this Tariff (TestDate indicates otherwise)
			CreateTariff("40506070", ZString.Empty, 20m, ZDecimal.Zero, ZString.Empty, ZBool.False);
			CreateTariff("30405000", CommodityTypeList.Codes.Alcohol, 10m, ZDecimal.Zero, ZString.Empty, ZBool.False);
			var tariff = CreateTariff("20304050", CommodityTypeList.Codes.Alcohol, 15m, ZDecimal.Zero, ZString.Empty, ZBool.False);
			testDataHelper.CreateTariffExciseRate(tariff, 330m, "PCE");
			Factory.Save();

			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1Sample_WI00186442.txt");

			Importer.LoadFile();
			Importer.Save();
			var manifestHeader = GetManifestHeader("08122222222");
			CombineAssertions(() =>
			{
				AssertEquals("TotalNoOfShipments", 3, Importer.TotalNoOfShipments);
				AssertEquals("TotalNoOfChildPackages", 2, Importer.TotalNoOfChildPackages);
				AssertEquals("TotalPiecesManifested", 6, Importer.TotalPiecesManifested);
				AssertEquals("PercentageOfDuplicateHAWBs", 0, Importer.PercentageOfDuplicateHAWBs);
				AssertEquals("PercentageOfIncorrectPorts", 33, Importer.PercentageOfIncorrectPorts);
				AssertMultilineASCIIEquals("IncorrectChildPacks", @"1Z3824AR6640806327: 0 childpack/s, 1 expected
1Z3947806616452202: 2 childpack/s, 1 expected
1Z4A14T96650435917: 0 childpack/s, 1 expected",
new ZStringBuilder(Importer.IncorrectChildPacks.OfType<string>()).ToStringWithNewLineBetweenAppends());
				AssertEquals("IncorrectPortOfDestinationList", "1Z4A14T96650435917 PortCode: AU9641", new ZStringBuilder(Importer.IncorrectPortOfDestinationList.OfType<string>()).ToString());
				AssertEquals("EmptyShipmentsList", ZString.Empty, new ZStringBuilder(Importer.EmptyShipmentsList.OfType<string>()).ToString());
				AssertEquals("manifestHeader.AMA_ApplicationCode", ApplicationCodeTypeList.Codes.Consolidator, manifestHeader.AMA_ApplicationCode);
				AssertEquals("manifestHeader.AMA_TransportMode", Level1DataFileImporterForSGAccess.Constants.TransportMode.Air, manifestHeader.AMA_TransportMode);
				AssertEquals("manifestHeader.AMA_Voyage", Level1DataImport.FlightNumber, manifestHeader.AMA_Voyage);
				AssertEquals("manifestHeader.AMA_RL_NKPortOfLoading", Level1DataImport.PortOfLoading, manifestHeader.AMA_RL_NKPortOfLoading);
				AssertEquals("manifestHeader.AMA_RL_NKPortOfDischarge", Level1DataImport.PortOfDischarge, manifestHeader.AMA_RL_NKPortOfDischarge);
				AssertEquals("manifestHeader.AMA_E_DEP", Level1DataImport.DepartureDate, manifestHeader.AMA_E_DEP);
				AssertEquals("manifestHeader.AMA_E_ARV", Level1DataImport.ArrivalDate, manifestHeader.AMA_E_ARV);
				AssertEquals("manifestHeader.AMA_RN_NKCountry", Core.Constants.CountryCodes.Singapore, manifestHeader.AMA_RN_NKCountry);
				AssertEquals("manifestHeader.AMA_ManifestType", Level1DataFileImporterForSGAccess.Constants.ManifestType.Import, manifestHeader.AMA_ManifestType);
				AssertEquals("manifestHeader.AMA_Nature", Level1DataFileImporterForSGAccess.Constants.Nature.Import, manifestHeader.AMA_Nature);
				AssertEquals("manifestHeader.AMA_RL_NKPortOfFirstArrival", manifestHeader.AMA_RL_NKPortOfDischarge, manifestHeader.AMA_RL_NKPortOfFirstArrival);
			});
			var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();
			AssertEquals("bills.Length", 3, bills.Length);

			AssertBill(bills[0], "3824ARFY9JH", 1021.58m, "USLAX", "SGSIN", 8.618m, Core.Constants.Weight.Kilograms, "ADVANCED SHEN MIN FOR WOMEN", 0m, Core.Constants.Volume.CubicMetres, 2, "PKG",
				consignee.MainAddress.PK, "SNOWSILL, SONYA", "SNOWSILL, SONYA STR 1", "SNOWSILL, SONYA STR 2", "SNOWSILL, SONYA CITY", "SNOWSILL, SONYA STATE", "8AU049POST", "", "AU",
				shipper.MainAddress.PK, "BIOTECH CORP", "BIOTECH CORP STR 1", "BIOTECH CORP STR 2", "BIOTECH CORP CITY", "BIOTECH CORP STATE", "3824ARPOST", "AU",
				"SGD", 1300m, "USD", 150m, "USD", 50m, "USD", 20m, "USD", 100m, "USD", 94.30m, 78.12m, ZDateTime.Empty, ZString.Empty);
			AssertEquals("Packs.Count", 2, bills[0].Packs.Count);
			AssertPack(bills[0].Packs[0], "ADVANCED SHEN MIN FOR WOMEN", "", 6, "PCE", 0m, "", 0m, "",
				"US", "40506070", 471.50m, 94.30m, 39.61m, Level1DataFileImporterForSGAccess.Constants.GoodsType.MajorExporter, 6m, "NMB", 600m, "USD", ZString.Empty);
			AssertPack(bills[0].Packs[1], "ADVANCED SHEN MIN FOR MEN", "", 6, "PCE", 0m, "", 0m, "",
				"US", "10203040", 550.08m, 0m, 38.51m, Level1DataFileImporterForSGAccess.Constants.GoodsType.MajorExporter, 6m, "NMB", 700m, "USD", ZString.Empty);

			AssertBill(bills[1], "3947808NNSR", 90.83m, "USLAX", "SGSIN", 4.990m, Core.Constants.Weight.Kilograms, "COMPANY PERFORMANCE & MEASURES", 0m, "", 2, "PKG",
				ZGuid.Empty, "UNIT 7 5-7 COWPER ST", "PARRAMATTA", "NSW", "SYDNEY", "", "2150", "0061292271338", "AU",
				ZGuid.Empty, "DISTRIBUTION CENTER", "1650 BLUEGRASS LAKES PKWY", "", "ALPHARETTA", "GA", "30004", "US",
				"SGD", 125m, "USD", 0m, "USD", 0.9m, "SGD", 0m, "USD", 0m, "USD", 339.81m, 30.15m, ZDateTime.Empty, ZString.Empty);
			AssertEquals("Packs.Count", 3, bills[1].Packs.Count);
			AssertPack(bills[1].Packs[0], "COMPANY PERFORMANCE & MEASURES", "", 1, "PCE", 0m, "", 0m, "",
				"US", "", 14.53m, 0m, 1.02m, Level1DataFileImporterForSGAccess.Constants.GoodsType.NormalGoods, 1m, "NMB", 20m, "USD", ZString.Empty);
			AssertPack(bills[1].Packs[1], "ANALYSIS OF DERIVATIVES FOR TH", "", 1, "PCE", 0m, "", 0m, "",
				"US", "20304050", 43.60m, 336.54m, 26.61m, Level1DataFileImporterForSGAccess.Constants.GoodsType.DutiableGoods, 1m, "NMB", 60m, "USD", ZString.Empty);
			AssertPack(bills[1].Packs[2], "LEVEL II CFA 2004 READINGS", "", 1, "PCE", 0m, "", 0m, "",
				"US", "30405000", 32.70m, 3.27m, 2.52m, Level1DataFileImporterForSGAccess.Constants.GoodsType.DutiableGoods, 1m, "NMB", 45m, "USD", ZString.Empty);

			AssertBill(bills[2], "4A14T9J3YYD", 33.81m, "USLAX", "9641*", 2.722m, Core.Constants.Weight.Kilograms, "", 0m, Core.Constants.Volume.CubicMetres, 2, "PKG",
				ZGuid.Empty, "", "", "", "", "", "", "", "",
				ZGuid.Empty, "CAFEPRESS.COM", "26010 EDEN LANDING ROAD", "SUITE 5", "HAYWARD", "CA", "94545", "US",
				"SGD", 0m, "USD", 47m, "USD", 0m, "SGD", 0m, "USD", 0m, "USD", 0m, 2.37m, new ZDateTime(2017, 6, 15), "5");
			AssertEquals("Packs.Count", 0, bills[2].Packs.Count);
			AssertContains("Successfully saved Manifest Header MAN0000001 with 3 Bills.", NotificationBuffer.AsString);
		}

		[TestDate(2017, 11, 2)]
		public void TestLoadFileAndSave_AlternateBroker_IMP()
		{
			var consignee = CreateOrg("SNOWSILL, SONYA", "CGNTEST1", "8AU0495926");
			var relatedOrg = Factory.NewWithValidTestData<UPEOrgHeader>();
			consignee.AddRelatedParty(relatedOrg.PK, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Level1DataFileImporterForSGAccess.Constants.TransportMode.Air, ZString.Empty, GlbCompany.CurrentCompany);
			Factory.Save();

			Level1DataImport.CycleDate = ZDateTime.Today;
			Level1DataImport.CycleNumber = "2";
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1Sample_WI00186442.txt");

			Importer.LoadFile();
			Importer.Save();
			var manifestHeader = GetManifestHeader("08122222222");
			var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();
			AssertEquals("bills.Length", 3, bills.Length);

			CombineAssertions(() =>
			{
				AssertCycleFields(bills[0], ZDateTime.Today, "1");
				AssertCycleFields(bills[1], ZDateTime.Today, "2");
				AssertCycleFields(bills[2], ZDateTime.Today, "2");
			});
		}

		[TestDate(2017, 11, 2)]
		public void TestLoadFileAndSave_FTZ_IMP()
		{
			Level1DataImport.CycleDate = ZDateTime.Today;
			Level1DataImport.CycleNumber = "2";
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithFreeTradeZoneShipments.txt");
			Importer.LoadFile();
			Importer.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();
			AssertEquals("bills.Length", 2, bills.Length);

			CombineAssertions(() =>
			{
				AssertCycleFields(bills[0], ZDateTime.Today, "1");
				AssertCycleFields(bills[1], ZDateTime.Today, "1");
			});
		}

		[TestDate(2017, 11, 2)]
		public void TestLoadFileAndSave_Direct_IMP()
		{
			var directDeliveryConsignee = CreateOrg("LIYANA AZIZ", "LIYANA", "#04-01B DELTA HOUSE", "2 ALEXANDRA ROAD 159919", "SINGAPORE", ZString.Empty, "8869", "6590085135");
			directDeliveryConsignee.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.DirectDelivery, "NIL", Core.Constants.CountryCodes.Singapore);
			directDeliveryConsignee.CreatePatternMatchingAddressFromMainAddress(Factory);
			directDeliveryConsignee.CreatePatternMatchingName(Factory);
			Factory.Save();

			Level1DataImport.CycleDate = ZDateTime.Today;
			Level1DataImport.CycleNumber = "2";
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithHighLowValueSameConsigneeShipments.txt");

			Importer.LoadFile();
			Importer.Save();
			var manifestHeader = GetManifestHeader("08122222222");
			var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();
			AssertEquals("bills.Length", 2, bills.Length);

			CombineAssertions(() =>
			{
				AssertCycleFields(bills[0], ZDateTime.Today, "1");
				AssertCycleFields(bills[1], ZDateTime.Today, "1");
			});
		}

		[TestDate(2017, 11, 2)]
		public void TestLoadFileAndSave_AlternateBroker_EXP()
		{
			var consignee = CreateOrg("SNOWSILL, SONYA", "CGNTEST1", "8AU0495926");
			var relatedOrg = Factory.NewWithValidTestData<UPEOrgHeader>();
			consignee.AddRelatedParty(relatedOrg.PK, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Level1DataFileImporterForSGAccess.Constants.TransportMode.Air, ZString.Empty, GlbCompany.CurrentCompany);
			Factory.Save();

			Level1DataImport.PortOfLoading = "SGSIN";
			Level1DataImport.PortOfDischarge = "AUSYD";
			Level1DataImport.CycleDate = ZDateTime.Today;
			Level1DataImport.CycleNumber = "2";
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1Sample_WI00186442.txt");

			Importer.LoadFile();
			Importer.Save();
			var manifestHeader = GetManifestHeader("08122222222");
			var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();
			AssertEquals("bills.Length", 1, bills.Length);
			AssertCycleFields(bills[0], ZDateTime.Empty, ZString.Empty);
		}

		[TestDate(2017, 11, 2)]
		public void TestLoadFileAndSave_FTZ_EXP()
		{
			Level1DataImport.PortOfLoading = "SGSIN";
			Level1DataImport.PortOfDischarge = "AUSYD";
			Level1DataImport.CycleDate = ZDateTime.Today;
			Level1DataImport.CycleNumber = "2";
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithFreeTradeZoneShipments.txt");
			Importer.LoadFile();
			Importer.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();
			AssertEquals("bills.Length", 2, bills.Length);

			CombineAssertions(() =>
			{
				AssertCycleFields(bills[0], ZDateTime.Empty, ZString.Empty);
				AssertCycleFields(bills[1], ZDateTime.Empty, ZString.Empty);
			});
		}

		[TestDate(2017, 11, 2)]
		public void TestLoadFileAndSave_Direct_EXP()
		{
			var directDeliveryConsignee = CreateOrg("LIYANA AZIZ", "LIYANA", "#04-01B DELTA HOUSE", "2 ALEXANDRA ROAD 159919", "SINGAPORE", ZString.Empty, "8869", "6590085135");
			directDeliveryConsignee.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.DirectDelivery, "NIL", Core.Constants.CountryCodes.Singapore);
			Factory.Save();

			Level1DataImport.PortOfLoading = "SGSIN";
			Level1DataImport.PortOfDischarge = "AUSYD";
			Level1DataImport.CycleDate = ZDateTime.Today;
			Level1DataImport.CycleNumber = "2";
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithHighLowValueSameConsigneeShipments.txt");

			Importer.LoadFile();
			Importer.Save();
			var manifestHeader = GetManifestHeader("08122222222");
			var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();
			AssertEquals("bills.Length", 2, bills.Length);

			CombineAssertions(() =>
			{
				AssertCycleFields(bills[0], ZDateTime.Empty, ZString.Empty);
				AssertCycleFields(bills[1], ZDateTime.Empty, ZString.Empty);
			});
		}

		static void AssertCycleFields(AsycudaBill bill, ZDateTime cycleDate, ZString cycleNumber)
		{
			var billNumber = bill.ABL_BillNumber;
			AssertEquals(billNumber + " bill.CycleDate", cycleDate, bill.CycleDate);
			AssertEquals(billNumber + " bill.CycleNumber", cycleNumber, bill.CycleNumber);
		}

		public void TestLoadFile_InvalidFile()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "Level1Sample That is Invalid.txt");
			Importer.LoadFile();

			string expectedErrorMessage = "Error Level 1 File is Invalid:\n" +
					"Input string was not in a correct format.\n" +
					"Approximate Line Number: 3\n\n" +
					"Line Value: US2795AU9639040422              C4A14T9J3YYD   4A14T9J3YYD           N 1 6    LBS US          USDNNNN           USDAKE32632QF    08695             USD4700     USD21089     USDN0 N   YEDI  18APR20046  LBS         USDD4    NNN NN  NN  N USD           USD    T1                      18APR20040000           26089      P/PNTF       6    LBSNNC0000051894QF12            N N 1   N \n\n" +
					"Previous Line Value: US2795AU9639040422              T4AX585GPRZC2000004AX585GPRZC           N 1 7    LBS US          USDNNNN N         USDAKE32632QF    09686             USD         USD5000      USDN0NN   NMAS1D19APR20047  LBS         AUDD3    NNN NN  NN  N USD           USD    T1                      19APR20041800           5000       P/PNDR            LBSNNC0000051894QF12            N N 1   N ";

			AssertMultilineEquals("", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.ToString(), '\n');
			ErrorReporter.Clear();//developer exception should not cause failure
		}

		public void TestLoadWithEmptyShipment()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SampleWithEmptyShipment_W3188389763.txt");
			Importer.LoadFile();

			AssertEquals(15, Importer.TotalNoOfShipments);
			AssertEquals(1, Importer.EmptyShipmentsList.Count);
			AssertEquals("W3188389763", Importer.EmptyShipmentsList[0]);

			//ensure that there is no double reporting
			Importer.LoadFile();

			AssertEquals(15, Importer.TotalNoOfShipments);
			AssertEquals(1, Importer.EmptyShipmentsList.Count);
			AssertEquals("W3188389763", Importer.EmptyShipmentsList[0]);
		}

		public void TestInvalidOriginPortCodesIsImported()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "GCCLevel1Sample.txt");
			Importer.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();
			AssertEquals("3296*", bills[0].ABL_RL_NKOrigin);
		}

		public void TestDestinationPortCodeIsWhenCountryOfDestinationIsNotSingapore()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "Level1LoadCurrencyNotFound.txt");
			Importer.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();
			AssertEquals("SGSIN", manifestHeader.AMA_RL_NKPortOfDischarge);
			AssertEquals("8699*", bills[0].ABL_RL_NKFinalDestination);
		}

		public void TestConvertInvalidCurrencySGD()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "Level1LoadCurrencyAUS.txt");
			Importer.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();
			AssertEquals(1, bills.Length);
			AssertEquals(Core.Constants.CurrencyCodes.Singapore, bills[0].ABL_RX_NKCustomsValueCurrency);
			AssertEquals(29.41m, bills[0].ABL_CustomsValue);
		}

		public void TestConvertInvalidCurrencyRMB()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "Level1LoadCurrencyRMB.txt");
			Importer.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();
			AssertEquals(1, bills.Length);
			AssertEquals(Core.Constants.CurrencyCodes.Singapore, bills[0].ABL_RX_NKCustomsValueCurrency);
			AssertNotEquals("After conversion, the value is not equal to 0.", 0m, bills[0].ABL_CustomsValue);
		}

		public void TestRecordShouldNotBeLoaded()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1SampleWithDocumentsAndLettersShipments.txt");
			Importer.LoadFile();
			Importer.Save();
			AssertEquals("TotalNoOfShipments", 1, Importer.TotalNoOfShipments);
			var manifestHeader = GetManifestHeader("08122222222");
			var bills = manifestHeader.Bills;
			AssertEquals("bills.Length", 1, bills.Count);
		}

		public void TestLoadCorrectConsigneeDetails()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1SampleWith400100CountrySG.txt");
			Importer.LoadFile();
			Importer.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();
			AssertEquals("bills.Length", 2, bills.Length);
			var bill1 = bills[0];
			AssertConsignee(bill1, bill1.ABL_BillNumber, ZGuid.Empty, "CHRISTOPHER KWAN", "27 PARRY WALK", ZString.Empty, "SINGAPORE", ZString.Empty, "547041", ZString.Empty, Core.Constants.CountryCodes.Singapore);
			var bill2 = bills[1];
			AssertConsignee(bill2, bill2.ABL_BillNumber, ZGuid.Empty, "AXA INSURANCE PTE. L", "TOWER", "8 SHENTON WAY, #24-01/02 AXA", "SINGAPORE", "SI", "068811", "+6587999061", Core.Constants.CountryCodes.Singapore);
		}

		public void TestCustomsQtyAndPackQty()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1UQ.txt");
			Importer.LoadFile();
			Importer.Save();

			AssertEquals(5, Importer.TotalNoOfShipments);
			var manifestHeader = GetManifestHeader("08122222222");
			var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();
			AssertEquals("bills.Length", 5, bills.Length);

			CombineAssertions(() =>
			{
				var bill1 = bills[0];
				AssertEquals("Bill Number AT1665NH8DX", "AT1665NH8DX", bill1.ABL_BillNumber);
				AssertEquals("AT1665NH8DX Pack Count", 1, bill1.Packs.Count);

				var bill1Package = bill1.Packs[0];
				AssertEquals("AT1665NH8DX Pack Qty", 1000, bill1Package.APA_PackQty);
				AssertEquals("AT1665NH8DX Pack UQ", UnitOfQuantityCodeList.Codes.KGM, bill1Package.APA_PackUQ);
				var bill1PackCountry = bill1Package.PackedItem;
				AssertEquals("AT1665NH8DX Customs Qty", 1m, bill1PackCountry.API_CustomsQty);
				AssertEquals("AT1665NH8DX Customs UQ", UnitOfQuantityCodeList.Codes.TNE, bill1PackCountry.API_CustomsUQ);

				var bill2 = bills[1];
				AssertEquals("Bill Number AT1665NN3NX", "AT1665NN3NX", bill2.ABL_BillNumber);
				AssertEquals("AT1665NN3NX Pack Count", 1, bill2.Packs.Count);

				var bill2Package = bill2.Packs[0];
				AssertEquals("AT1665NN3NX Pack Qty", 4910, bill2Package.APA_PackQty);
				AssertEquals("AT1665NN3NX Pack UQ", "PCE", bill2Package.APA_PackUQ);
				var bill2PackCountry = bill2Package.PackedItem;
				AssertEquals("AT1665NN3NX Customs Qty", 4910m, bill2PackCountry.API_CustomsQty);
				AssertEquals("AT1665NN3NX Customs UQ", UnitOfQuantityCodeList.Codes.NMB, bill2PackCountry.API_CustomsUQ);

				var bill3 = bills[2];
				AssertEquals("Bill Number AT1665NN3PW", "AT1665NN3PW", bill3.ABL_BillNumber);
				AssertEquals("AT1665NN3PW Pack Count", 1, bill3.Packs.Count);

				var bill3Package = bill3.Packs[0];
				AssertEquals("AT1665NN3PW Pack Qty", 4910, bill3Package.APA_PackQty);
				AssertEquals("AT1665NN3PW Pack UQ", "PCE", bill3Package.APA_PackUQ);
				var bill3PackCountry = bill3Package.PackedItem;
				AssertEquals("AT1665NN3PW Customs Qty", 0m, bill3PackCountry.API_CustomsQty);
				AssertEquals("AT1665NN3PW Customs UQ", UnitOfQuantityCodeList.Codes.TNE, bill3PackCountry.API_CustomsUQ);

				var bill4 = bills[3];
				AssertEquals("Bill Number AT1665NH8PW", "AT1665NH8PW", bill4.ABL_BillNumber);
				AssertEquals("AT1665NH8PW Pack Count", 2, bill4.Packs.Count);

				var bill4Package1 = bill4.Packs[0];
				AssertEquals("AT1665NH8PW Pack 1 Qty", 1000, bill4Package1.APA_PackQty);
				AssertEquals("AT1665NH8PW Pack 1 Q", UnitOfQuantityCodeList.Codes.KGM, bill4Package1.APA_PackUQ);
				var bill4Pack1Country = bill4Package1.PackedItem;
				AssertEquals("AT1665NH8PW Customs 1 Qty", 1m, bill4Pack1Country.API_CustomsQty);
				AssertEquals("AT1665NH8PW Customs 1 UQ", UnitOfQuantityCodeList.Codes.TNE, bill4Pack1Country.API_CustomsUQ);

				var bill4Package2 = bill4.Packs[1];
				AssertEquals("AT1665NH8PW Pack 2 Qty", 1000, bill4Package2.APA_PackQty);
				AssertEquals("AT1665NH8PW Pack 2 UQ", "LB", bill4Package2.APA_PackUQ);
				var bill4Pack2Country = bill4Package2.PackedItem;
				AssertEquals("AT1665NH8PW Customs 2 Qty", 0.45359m, bill4Pack2Country.API_CustomsQty);
				AssertEquals("AT1665NH8PW Customs 2 UQ", UnitOfQuantityCodeList.Codes.TNE, bill4Pack2Country.API_CustomsUQ);

				var bill5 = bills[4];
				AssertEquals("Bill Number 2F361F4CLDT", "2F361F4CLDT", bill5.ABL_BillNumber);
				AssertEquals("2F361F4CLDT Pack Count", 1, bill5.Packs.Count);

				var bill5Package = bill5.Packs[0];
				AssertEquals("2F361F4CLDT Pack Qty", 1, bill5Package.APA_PackQty);
				AssertEquals("2F361F4CLDT Pack UQ", "PCE", bill5Package.APA_PackUQ);
				var bill5PackCountry = bill5Package.PackedItem;
				AssertEquals("2F361F4CLDT Customs Qty", 1m, bill5PackCountry.API_CustomsQty);
				AssertEquals("2F361F4CLDT Customs UQ", UnitOfQuantityCodeList.Codes.NMB, bill5PackCountry.API_CustomsUQ);
			});
		}

		public void TestCustomsQtyConversion()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1UQConvert.txt");
			Importer.LoadFile();
			Importer.Save();

			AssertEquals(2, Importer.TotalNoOfShipments);
			var manifestHeader = GetManifestHeader("08122222222");
			var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();
			AssertEquals("bills.Length", 2, bills.Length);

			CombineAssertions(() =>
			{
				var bill1 = bills[0];
				AssertEquals("Bill Number AT1665NH8DX", "AT1665NH8DX", bill1.ABL_BillNumber);
				AssertEquals("AT1665NH8DX Pack Count", 1, bill1.Packs.Count);

				var bill1Package = bill1.Packs[0];
				AssertEquals("AT1665NH8DX Pack Qty", 3780, bill1Package.APA_PackQty);
				AssertEquals("AT1665NH8DX Pack UQ", UnitOfQuantityCodeList.Codes.KGM, bill1Package.APA_PackUQ);
				var bill1PackedItem = bill1Package.PackedItem;
				AssertEquals("AT1665NH8DX Customs Qty for this tariff is required in tonnes so should be converted from provided commercial qty", 3.78m, bill1PackedItem.API_CustomsQty);
				AssertEquals("AT1665NH8DX Customs UQ", UnitOfQuantityCodeList.Codes.TNE, bill1PackedItem.API_CustomsUQ);

				var bill2 = bills[1];
				AssertEquals("Bill Number AT1665NN3PW", "AT1665NN3PW", bill2.ABL_BillNumber);
				AssertEquals("AT1665NN3PW Pack Count", 1, bill2.Packs.Count);

				var bill2Package = bill2.Packs[0];
				AssertEquals("AT1665NN3PW Pack Qty", 4910, bill2Package.APA_PackQty);
				AssertEquals("AT1665NN3PW Pack UQ", "PCE", bill2Package.APA_PackUQ);
				var bill3PackedItem = bill2Package.PackedItem;
				AssertEquals("AT1665NN3PW Customs Qty required is TNE, unit qty PCE is not convertible - Requires a refcuspack conversion", 0m, bill3PackedItem.API_CustomsQty);
				AssertEquals("AT1665NN3PW Customs UQ defaults to Tariff", UnitOfQuantityCodeList.Codes.TNE, bill3PackedItem.API_CustomsUQ);
			});
		}

		public void TestCustomsQtyConversionWithType()
		{
			var manifestDetails = GetTestManifestAndChildren("MAN0081");
			var bill = manifestDetails.Bill;
			bill.ABL_BillNumber = "3824ARFY9JH";

			bill.ABL_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-2);

			var query = new ZQuery(RefPacksSchema.RP_CustomsCountry, "SG");
			query.AddToFilter(RefPacksSchema.RP_CommercialPack, "PCE");

			var pack = Factory.LoadTop1<CusRefPacks>(query);

			pack.RP_CustomsPack = "UNT";
			pack.RP_Type = "GMP";

			Factory.Save();

			using (UPEDataRegistry.Instance.ShipmentReferenceNumberRecyclePeriodItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				Importer.LoadFile();
				Importer.Save();
				var newManifest = AssertBillsOnNewManifest(3);
				var newBill = newManifest.Bills.Cast<AsycudaBill>().FirstOrDefault(x => x.ABL_BillNumber == "3824ARFY9JH");
				AssertNotNull(newBill);

				AssertEquals("Packs.Count", 2, newBill.Packs.Count);

				var billPack = newBill.Packs[1];
				AssertEquals("PackUQ", "PCE", billPack.APA_PackUQ);
				AssertEquals("PackQty", 6, billPack.APA_PackQty);
				var bill5PackedItem = billPack.PackedItem;
				AssertEquals("Customs UQ", "UNT", bill5PackedItem.API_CustomsUQ);
				AssertEquals("Customs Qty", 6m, bill5PackedItem.API_CustomsQty);
			}
		}

		public void TestUQWhenNoTariff()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1UQ.txt");
			Importer.LoadFile();
			Importer.Save();

			AssertEquals(5, Importer.TotalNoOfShipments);
			var manifestHeader = GetManifestHeader("08122222222");
			var bills = manifestHeader.Bills.Cast<AsycudaBill>().ToArray();
			AssertEquals("bills.Length", 5, bills.Length);

			var bill5 = bills[4];
			AssertEquals("bill.ABL_BillNumber", "2F361F4CLDT", bill5.ABL_BillNumber);
			AssertEquals("Packs.Count", 1, bill5.Packs.Count);

			var bill5Package = bill5.Packs[0];
			AssertEquals("PackQty", 1, bill5Package.APA_PackQty);
			AssertEquals("PackUQ", "PCE", bill5Package.APA_PackUQ);
			var bill5PackedItem = bill5Package.PackedItem;
			AssertEquals("Customs Qty", 1m, bill5PackedItem.API_CustomsQty);
			AssertEquals("Customs UQ has defaulted to NMB as tariff does not exist", "NMB", bill5PackedItem.API_CustomsUQ);
		}

		public void TestCustomsValueWhenCalculatedNegative()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1NegativeCustomsValue.txt");
			Importer.LoadFile();
			Importer.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals(1, manifestHeader.Bills.Count);
			AssertEquals("2F361F4CLDT", manifestHeader.Bills[0].ABL_BillNumber);
			AssertEquals("Negative value calculation should return zero", 0m, manifestHeader.Bills[0].ABL_CustomsValue);
			AssertEquals("Negative value calculation should return zero", 0m, manifestHeader.Bills[0].ABL_InsuranceValue);
		}

		public void TestDuplicateAsycudaBillNumber()
		{
			NotificationBuffer = new NotificationBuffer();
			Level1DataImportForManifest.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "DuplicateAsycudaBillNumberSample.txt");
			ImporterForSGAccess.LoadFile();
			AssertEquals(1, ImporterForSGAccess.DuplicateAsycudaBillNumberList.Count);
			AssertEquals("4R15V3KYSM8", ImporterForSGAccess.DuplicateAsycudaBillNumberList[0]);
		}

		public void TestNoValidShipments()
		{
			Level1DataImportForManifest.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1SampleWithNoValidShipments.txt");
			ImporterForSGAccess.LoadFile();
			AssertEquals("There is no valid shipments in the file to be imported. \r\n", ImporterForSGAccess.GetErrorMessage());
		}

		public void TestLowValueAndHighValueFreeTradeZoneImportedWithTradeNet()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithFreeTradeZoneShipments.txt");
			Importer.LoadFile();
			Importer.Save();

			AssertBillsCreated("08122222222", 2, "AT1665NH8DX|AT1665NN3NX");
			AssertDeclarationsCreated("08122222222", 2, "AT1665NH8DX|AT1665NN3NX");
		}

		public void TestBothAccessAndTradenetForHighAndLowValueDutiableShipments()
		{
			CreateTariff("20304050", CommodityTypeList.Codes.Alcohol, 15m, 330m, "PCE", ZBool.False);
			Factory.Save();

			Level1DataImport.ArrivalDate = ZDateTime.Now;
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithDutiableShipments.txt");
			Importer.LoadFile();
			Importer.Save();

			AssertBillsCreated("08122222222", 2, "3947808NNSR|3824ARFY9JH");
			AssertDeclarationsCreated("08122222222", 2, "3947808NNSR|3824ARFY9JH");
		}

		public void TestLowValueNormalGoodsImportedWithNoTradeNetAndHighValueWithTradeNet()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithNormalGoodsShipments.txt");
			Importer.LoadFile();
			Importer.Save();

			AssertBillsCreated("08122222222", 2, "AT1665NH8DX|AT1665NN3NX");
			AssertDeclarationsCreated("08122222222", 1, "AT1665NN3NX");
		}

		public void TestLowValueMajorExporterImportedWithNoTradeNetAndHighValueWithTradeNet()
		{
			SetupMajorExporterConsignee();
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithHighLowValueSameConsigneeShipments.txt");
			Importer.LoadFile();
			Importer.Save();

			AssertBillsCreated("08122222222", 2, "AT1665NH8DX|AT1665NN3NX");
			AssertDeclarationsCreated("08122222222", 1, "AT1665NN3NX");
		}

		[TestDate(2018, 06, 01)]
		public void TestBothAccessAndTradenetForHighAndLowValueControlledGoodsShipments()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithControlledGoodsShipments.txt");
			Importer.LoadFile();
			Importer.Save();

			AssertBillsCreated("08122222222", 2, "AE5168THCZS|AE5168THCZM");
			AssertDeclarationsCreated("08122222222", 2, "AE5168THCZS|AE5168THCZM");
		}

		public void TestLowValueInterbankGIROImportedWithNoTradeNetAndHighValueWithTradeNet()
		{
			var interbankConsignee = CreateOrg("LIYANA AZIZ", "LIYANA", "#04-01B DELTA HOUSE", "2 ALEXANDRA ROAD 159919", "SINGAPORE", ZString.Empty, "8869", "6590085135");
			interbankConsignee.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.InterbankGIRO, "BSB123456", Core.Constants.CountryCodes.Singapore);
			Factory.Save();

			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithHighLowValueSameConsigneeShipments.txt");
			Importer.LoadFile();
			Importer.Save();

			AssertBillsCreated("08122222222", 2, "AT1665NH8DX|AT1665NN3NX");
			AssertDeclarationsCreated("08122222222", 1, "AT1665NN3NX");
		}

		public void TestBothAccessAndTradenetForHighAndLowValueDirectDeliveryShipments()
		{
			var directDeliveryConsignee = CreateOrg("LIYANA AZIZ", "LIYANA", "#04-01B DELTA HOUSE", "2 ALEXANDRA ROAD 159919", "SINGAPORE", ZString.Empty, "8869", "6590085135");
			directDeliveryConsignee.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.DirectDelivery, "NIL", Core.Constants.CountryCodes.Singapore);
			directDeliveryConsignee.CreatePatternMatchingAddressFromMainAddress(Factory);
			directDeliveryConsignee.CreatePatternMatchingName(Factory);
			Factory.Save();

			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithHighLowValueSameConsigneeShipments.txt");
			Importer.LoadFile();
			Importer.Save();

			AssertBillsCreated("08122222222", 2, "AT1665NH8DX|AT1665NN3NX");
			AssertDeclarationsCreated("08122222222", 2, "AT1665NH8DX|AT1665NN3NX");
		}

		public void TestBothAccessAndTradenetForHighAndLowValueStopWordsShipments()
		{
			using (UPEDataRegistry.Instance.StopPhrasesForSGGoodsDescriptionItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SISYPHUS"))
			{
				Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithStopWordsShipment.txt");
				Importer.LoadFile();
				Importer.Save();

				AssertBillsCreated("08122222222", 2, "AT1665NH8XX|AT1665NH8XZ");
				AssertDeclarationsCreated("08122222222", 2, "AT1665NH8XX|AT1665NH8XZ");
			}
		}

		public void TestTranshipmentsAreNotImported()
		{
			CreateLocoMap("8869", "USLAX");
			CreateLocoMap("1184", "AUSYD");
			Factory.Save();

			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1Transhipment.txt");
			Importer.LoadFile();
			Importer.Save();

			var billFilter = new ZQuery(AsycudaBillSchema.ABL_BillNumber, "08122222222");
			billFilter.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
			AssertEquals(0, Factory.Load<AsycudaBill>(billFilter).Length);

			AssertEquals(0, Factory.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MasterBill, "08122222222")).Length);
		}

		public void TestPayeeIndicatorAgainstOrganisations()
		{
			SetupPayeeAndPartyIDOrganisations();
			Importer.LoadFile();
			Importer.Save();
			var manifestHeader = GetManifestHeader("08122222222");
			var bill = GetBill(manifestHeader, "3824ARFY9JH");
			AssertEquals(SGPayeeIndicatorList.Codes.T, bill.SG_PayeeIndicator);
		}

		public void TestRegistryToDisableDecisionProvider()
		{
			using (UPEDataRegistry.Instance.EnableDecisionSupportExportShipmentsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Level1DataImportForManifest.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1Sample.txt");
				ImporterForSGAccess.LoadFile();
				ImporterForSGAccess.Save();
				var manifestHeader = GetManifestHeader("08122222222");
				AssertEquals(3, manifestHeader.Bills.Count);
				AssertEquals(0, Factory.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MasterBill, "08122222222")).Length);
			}
		}

		public void TestLevel1DisableDecisionProvider()
		{
			Level1DataImportForManifest.DisableDecisionProvider = true;
			Level1DataImportForManifest.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1Sample.txt");
			ImporterForSGAccess.LoadFile();
			ImporterForSGAccess.Save();
			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals(3, manifestHeader.Bills.Count);
			AssertEquals(0, Factory.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MasterBill, "08122222222")).Length);
		}

		public void TestMatchingHeaderBillAreUpdatedAndPacksAreOverriden()
		{
			var manifestDetails = GetTestManifestAndChildren("MAN0081");
			var header = manifestDetails.Header;
			header.AMA_Voyage = "QF656";
			var bill = manifestDetails.Bill;
			Factory.Save();

			using (UPEDataRegistry.Instance.StopImportOfBillIfMatchingBillFoundItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (UPEDataRegistry.Instance.AllowMultipleLevel1LoadsForMasterBillItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (UPEDataRegistry.Instance.AllowBillUpdatesDuringMulitpleLevel1LoadsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1UpdateBill.txt");
				Level1DataImport.PortOfLoading = "USLAX";
				Importer.LoadFile();
				Importer.Save();

				header.Reload();
				header.Bills.Reload(true);
				bill.Packs.Reload(true, true);

				AssertHeaderAndHeaderCountry(header, "MAN0081", "08122222222", Level1DataFileImporterForSGAccess.Constants.TransportMode.Air, "QF656", "USLAX", "SGSIN",
					Level1DataFileImporterForSGAccess.Constants.ManifestType.Import, "SGSIN", Level1DataFileImporterForSGAccess.Constants.Nature.Import);
				AssertEquals(1, header.Bills.Count);
				AssertBill(bill, "3824ARFY9JH", 517.98m, "USLAX", "SGSIN", 8.618m, Core.Constants.Weight.Kilograms, "ADVANCED SHEN MIN FOR WOMEN", 0m, Core.Constants.Volume.CubicMetres, 2,
					"PKG", ZGuid.Empty, "SNOWSILL, SONYA", "15 LEANDER ST", "CONSIGNEE STR 2", "FALCON", ZString.Empty, "6210", "+61895343637", Core.Constants.CountryCodes.Australia, ZGuid.Empty,
					"BIOTECH CORP", "107 OAKWOOD DRIVE", "SHIPPER STREET 3", "GLASTONBURY", "CT", "060332481", Core.Constants.CountryCodes.UnitedStates, Core.Constants.CurrencyCodes.Singapore, 600m,
					Core.Constants.CurrencyCodes.UnitedStates, 150m, Core.Constants.CurrencyCodes.UnitedStates, 50m, Core.Constants.CurrencyCodes.UnitedStates, 20m, Core.Constants.CurrencyCodes.UnitedStates,
					100m, Core.Constants.CurrencyCodes.UnitedStates, ZDecimal.Zero, 36.26m, ZDateTime.Empty, ZString.Empty);
				AssertEquals(1, bill.Packs.Count);
				var pack = bill.Packs[0];
				AssertNotNull(pack.PackedItem);
				AssertPack(pack, "ADVANCED SHEN MIN FOR WOMEN", ZString.Empty, 6, "PCE", ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, Core.Constants.CountryCodes.UnitedStates, ZString.Empty,
					517.98m, ZDecimal.Zero, 36.26m, Level1DataFileImporterForSGAccess.Constants.GoodsType.NormalGoods, 6, "NMB", 600, Core.Constants.CurrencyCodes.UnitedStates, ZString.Empty);
			}
		}

		public void TestMostRecentHeaderWithMatchingBillIsUpdated()
		{
			var manifestDetails1 = GetTestManifestAndChildren("MAN0081");
			var header1 = manifestDetails1.Header;
			header1.AMA_Voyage = "QF656";
			header1.AMA_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			var bill1 = manifestDetails1.Bill;
			var manifestDetails2 = GetTestManifestAndChildren("MAN0082");
			var header2 = manifestDetails2.Header;
			header2.AMA_Voyage = "QF656";
			var bill2 = manifestDetails2.Bill;
			Factory.Save();

			using (UPEDataRegistry.Instance.StopImportOfBillIfMatchingBillFoundItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (UPEDataRegistry.Instance.AllowMultipleLevel1LoadsForMasterBillItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (UPEDataRegistry.Instance.AllowBillUpdatesDuringMulitpleLevel1LoadsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1UpdateBill.txt");
				Level1DataImport.PortOfLoading = "USLAX";
				Importer.LoadFile();
				Importer.Save();

				header1.Reload();
				header2.Reload();
				header1.Bills.Reload(true);
				header2.Bills.Reload(true);
				bill1.Packs.Reload(true, true);
				bill2.Packs.Reload(true, true);

				AssertHeaderAndHeaderCountry(header1, "MAN0081", "08122222222", ZString.Empty, "QF656", "USLAX", "SGSIN",
					"MGI", ZString.Empty, Level1DataFileImporterForSGAccess.Constants.Nature.Import);
				AssertEquals(1, header1.Bills.Count);
				AssertBill(bill1, "3824ARFY9JH", ZDecimal.Zero, "USLAX", "SGKEL", ZDecimal.Zero, ZString.Empty, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZInt.Zero, ZString.Empty, ZGuid.Empty,
					ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZGuid.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
					ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero,
					ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDateTime.Empty, ZString.Empty);
				AssertEquals(1, bill1.Packs.Count);
				var bill1Pack = bill1.Packs[0];
				AssertNotNull(bill1Pack.PackedItem);
				AssertPack(bill1Pack, "SOME PACKS", ZString.Empty, 5, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, "US", ZString.Empty, ZDecimal.Zero, ZDecimal.Zero,
					ZDecimal.Zero, Level1DataFileImporterForSGAccess.Constants.GoodsType.NormalGoods, 5, UnitOfQuantityCodeList.Codes.NMB, ZDecimal.Zero, ZString.Empty, "PART2");

				AssertHeaderAndHeaderCountry(header2, "MAN0082", "08122222222", Level1DataFileImporterForSGAccess.Constants.TransportMode.Air, "QF656", "USLAX", "SGSIN",
					Level1DataFileImporterForSGAccess.Constants.ManifestType.Import, "SGSIN", Level1DataFileImporterForSGAccess.Constants.Nature.Import);
				AssertEquals(1, header2.Bills.Count);
				AssertBill(bill2, "3824ARFY9JH", 517.98m, "USLAX", "SGSIN", 8.618m, Core.Constants.Weight.Kilograms, "ADVANCED SHEN MIN FOR WOMEN", 0m, Core.Constants.Volume.CubicMetres, 2,
					"PKG", ZGuid.Empty, "SNOWSILL, SONYA", "15 LEANDER ST", "CONSIGNEE STR 2", "FALCON", ZString.Empty, "6210", "+61895343637", Core.Constants.CountryCodes.Australia, ZGuid.Empty,
					"BIOTECH CORP", "107 OAKWOOD DRIVE", "SHIPPER STREET 3", "GLASTONBURY", "CT", "060332481", Core.Constants.CountryCodes.UnitedStates, Core.Constants.CurrencyCodes.Singapore, 600m,
					Core.Constants.CurrencyCodes.UnitedStates, 150m, Core.Constants.CurrencyCodes.UnitedStates, 50m, Core.Constants.CurrencyCodes.UnitedStates, 20m, Core.Constants.CurrencyCodes.UnitedStates,
					100m, Core.Constants.CurrencyCodes.UnitedStates, ZDecimal.Zero, 36.26m, ZDateTime.Empty, ZString.Empty);
				AssertEquals(1, bill2.Packs.Count);
				var bill2Pack = bill2.Packs[0];
				AssertNotNull(bill2Pack.PackedItem);
				AssertPack(bill2Pack, "ADVANCED SHEN MIN FOR WOMEN", ZString.Empty, 6, "PCE", ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, Core.Constants.CountryCodes.UnitedStates, ZString.Empty,
					517.98m, ZDecimal.Zero, 36.26m, Level1DataFileImporterForSGAccess.Constants.GoodsType.NormalGoods, 6, "NMB", 600, Core.Constants.CurrencyCodes.UnitedStates, ZString.Empty);
			}
		}

		public void TestBillNotFoundAddedToTheMostRecentManifest()
		{
			var manifestDetails1 = GetTestManifestAndChildren("MAN0081");
			var header1 = manifestDetails1.Header;
			header1.AMA_Voyage = "QF656";
			header1.AMA_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			var bill1 = manifestDetails1.Bill;
			bill1.ABL_BillNumber = "ZZZZNOTMATCHING1";
			var manifestDetails2 = GetTestManifestAndChildren("MAN0082");
			var header2 = manifestDetails2.Header;
			header2.AMA_Voyage = "QF656";
			var bill2 = manifestDetails2.Bill;
			bill2.ABL_BillNumber = "ZZZZNOTMATCHING2";
			Factory.Save();

			using (UPEDataRegistry.Instance.AllowMultipleLevel1LoadsForMasterBillItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (UPEDataRegistry.Instance.AllowBillUpdatesDuringMulitpleLevel1LoadsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Importer.LoadFile();
				Importer.Save();

				header1.Reload();
				header2.Reload();
				header1.Bills.Reload(true);
				header2.Bills.Reload(true);
				bill1.Packs.Reload(true);
				bill2.Packs.Reload(true);

				AssertEquals(1, header1.Bills.Count);
				AssertEquals(1, bill1.Packs.Count);
				AssertEquals(4, header2.Bills.Count);
				AssertEquals(1, bill2.Packs.Count);
				var bill3 = GetBill(header2, "3824ARFY9JH");
				AssertEquals(2, bill3.Packs.Count);
				var bill4 = GetBill(header2, "3947808NNSR");
				AssertEquals(3, bill4.Packs.Count);
				var bill5 = GetBill(header2, "4A14T9J3YYD");
				AssertEquals(0, bill5.Packs.Count);
			}
		}

		public void TestMultipleBillsOneUpdatedTheOtherUntouched()
		{
			var manifestDetails = GetTestManifestAndChildren("MAN0081");
			var header = manifestDetails.Header;
			header.AMA_Voyage = "QF656";
			var bill1 = manifestDetails.Bill;
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "ZZZNOMATCH";
			bill2.ABL_RL_NKOrigin = "AUMEL";
			bill2.ABL_RL_NKFinalDestination = "SGSIN";
			bill2.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			var pack2 = bill2.Packs.AddNew();
			pack2.APA_GoodsDescription = "NOTHING SPECIAL";
			var packedItem2 = pack2.PackedItem;
			packedItem2.API_GoodsDescription = "NOTHING SPECIAL";
			Factory.Save();

			using (UPEDataRegistry.Instance.StopImportOfBillIfMatchingBillFoundItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (UPEDataRegistry.Instance.AllowMultipleLevel1LoadsForMasterBillItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (UPEDataRegistry.Instance.AllowBillUpdatesDuringMulitpleLevel1LoadsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1UpdateBill.txt");
				Level1DataImport.PortOfLoading = "USLAX";
				Importer.LoadFile();
				Importer.Save();

				header.Reload();
				header.Bills.Reload(true);
				bill1.Packs.Reload(true, true);
				bill2.Packs.Reload(true, true);

				AssertHeaderAndHeaderCountry(header, "MAN0081", "08122222222", Level1DataFileImporterForSGAccess.Constants.TransportMode.Air, "QF656", "USLAX", "SGSIN",
					Level1DataFileImporterForSGAccess.Constants.ManifestType.Import, "SGSIN", Level1DataFileImporterForSGAccess.Constants.Nature.Import);
				AssertEquals(2, header.Bills.Count);
				AssertBill(bill1, "3824ARFY9JH", 517.98m, "USLAX", "SGSIN", 8.618m, Core.Constants.Weight.Kilograms, "ADVANCED SHEN MIN FOR WOMEN", 0m, Core.Constants.Volume.CubicMetres, 2,
					"PKG", ZGuid.Empty, "SNOWSILL, SONYA", "15 LEANDER ST", "CONSIGNEE STR 2", "FALCON", ZString.Empty, "6210", "+61895343637", Core.Constants.CountryCodes.Australia, ZGuid.Empty,
					"BIOTECH CORP", "107 OAKWOOD DRIVE", "SHIPPER STREET 3", "GLASTONBURY", "CT", "060332481", Core.Constants.CountryCodes.UnitedStates, Core.Constants.CurrencyCodes.Singapore, 600m,
					Core.Constants.CurrencyCodes.UnitedStates, 150m, Core.Constants.CurrencyCodes.UnitedStates, 50m, Core.Constants.CurrencyCodes.UnitedStates, 20m, Core.Constants.CurrencyCodes.UnitedStates,
					100m, Core.Constants.CurrencyCodes.UnitedStates, ZDecimal.Zero, 36.26m, ZDateTime.Empty, ZString.Empty);
				AssertEquals(1, bill1.Packs.Count);
				var bill1Pack = bill1.Packs[0];
				AssertNotNull(bill1Pack.PackedItem);
				AssertPack(bill1Pack, "ADVANCED SHEN MIN FOR WOMEN", ZString.Empty, 6, "PCE", ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, Core.Constants.CountryCodes.UnitedStates, ZString.Empty,
					517.98m, ZDecimal.Zero, 36.26m, Level1DataFileImporterForSGAccess.Constants.GoodsType.NormalGoods, 6, "NMB", 600, Core.Constants.CurrencyCodes.UnitedStates, ZString.Empty);

				AssertBill(bill2, "ZZZNOMATCH", ZDecimal.Zero, "AUMEL", "SGSIN", ZDecimal.Zero, ZString.Empty, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZInt.Zero, ZString.Empty, ZGuid.Empty, ZString.Empty,
					ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZGuid.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
					ZString.Empty, ZString.Empty, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty,
					ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDateTime.Empty, ZString.Empty);
				AssertEquals(1, bill2.Packs.Count);
				var bill2Pack = bill2.Packs[0];
				AssertNotNull(bill2Pack.PackedItem);
				AssertPack(bill2Pack, "NOTHING SPECIAL", ZString.Empty, ZInt.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, "US", ZString.Empty,
					ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, Level1DataFileImporterForSGAccess.Constants.GoodsType.NormalGoods, ZDecimal.Zero, UnitOfQuantityCodeList.Codes.NMB, ZDecimal.Zero, ZString.Empty, ZString.Empty);
			}
		}

		public void TestAllowBillUpdatesDisabledDoesNotChangeJob()
		{
			var manifestDetails = GetTestManifestAndChildren("MAN0081");
			var header = manifestDetails.Header;
			var bill = manifestDetails.Bill;
			Factory.Save();

			using (UPEDataRegistry.Instance.AllowMultipleLevel1LoadsForMasterBillItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (UPEDataRegistry.Instance.AllowBillUpdatesDuringMulitpleLevel1LoadsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1UpdateBill.txt");
				Level1DataImport.FlightNumber = "QF81";
				Importer.LoadFile();
				Importer.Save();

				header.Reload();
				header.Bills.Reload(true);
				bill.Packs.Reload(true, true);

				AssertHeaderAndHeaderCountry(header, "MAN0081", "08122222222", ZString.Empty, "QF81", "USLAX", "SGSIN",
					"MGI", ZString.Empty, Level1DataFileImporterForSGAccess.Constants.Nature.Import);
				AssertEquals(1, header.Bills.Count);
				AssertBill(bill, "3824ARFY9JH", ZDecimal.Zero, "USLAX", "SGKEL", ZDecimal.Zero, ZString.Empty, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZInt.Zero, ZString.Empty, ZGuid.Empty, ZString.Empty,
					ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZGuid.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
					ZString.Empty, ZString.Empty, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero,
					ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDateTime.Empty, ZString.Empty);
				AssertEquals(1, bill.Packs.Count);
				var pack = bill.Packs[0];
				AssertNotNull(pack.PackedItem);
				AssertPack(pack, "SOME PACKS", ZString.Empty, 5, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, "US", ZString.Empty, ZDecimal.Zero, ZDecimal.Zero,
					ZDecimal.Zero, Level1DataFileImporterForSGAccess.Constants.GoodsType.NormalGoods, 5, UnitOfQuantityCodeList.Codes.NMB, ZDecimal.Zero, ZString.Empty, "PART2");
			}
		}

		public void TestBillMatchingReference()
		{
			Importer.LoadFile();
			Importer.Save();

			var manifest = GetManifestHeader("08122222222");
			AssertEquals(3, manifest.Bills.Count);
			AssertEquals("1Z3824AR6640806327", manifest.Bills[0].MatchingReference);
			AssertEquals("1Z3947806616452202", manifest.Bills[1].MatchingReference);
			AssertEquals("1Z4A14T96650435917", manifest.Bills[2].MatchingReference);
		}

		public void TestBillFilterStopsShipmentsAddWithinRecyclePeriodUsingHousebill()
		{
			var manifestDetails = GetTestManifestAndChildren("MAN0081");
			var header = manifestDetails.Header;
			header.AMA_Voyage = "FJ65";
			var bill = manifestDetails.Bill;
			bill.ABL_BillNumber = "3824ARFY9JH";
			bill.ABL_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			Factory.Save();

			using (UPEDataRegistry.Instance.ShipmentReferenceNumberRecyclePeriodItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				Importer.LoadFile();
				Importer.Save();
				var newManifest = AssertBillsOnNewManifest(2);
				AssertNull(GetBill(newManifest, "3824ARFY9JH"));
			}
		}

		public void TestBillFilterAllowsShipmentsAddOutsideRecyclePeriodUsingHouseBill()
		{
			var manifestDetails = GetTestManifestAndChildren("MAN0081");
			var bill = manifestDetails.Bill;
			bill.ABL_BillNumber = "3824ARFY9JH";
			bill.ABL_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-2);
			Factory.Save();

			using (UPEDataRegistry.Instance.ShipmentReferenceNumberRecyclePeriodItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				Importer.LoadFile();
				Importer.Save();
				var newManifest = AssertBillsOnNewManifest(3);
				AssertNotNull(GetBill(newManifest, "3824ARFY9JH"));
			}
		}

		public void TestAddressOverrideSet()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithNormalGoodsShipments.txt");
			Importer.LoadFile();
			Importer.Save();

			var header = GetManifestHeader("08122222222");
			var bill = GetBill(header, "AT1665NH8DX");
			AssertNotNull(bill);
			CombineAssertions(() =>
			{
				AssertEquals(ZGuid.Empty, bill.ABL_OA_Consignee);
				AssertEquals("LIYANA AZIZ", bill.ABL_ConsigneeName);
				AssertEquals("#04-01B DELTA HOUSE", bill.ABL_ConsigneeStreet1);
				AssertEquals("2 ALEXANDRA ROAD 159919", bill.ABL_ConsigneeStreet2);
				AssertEquals("SINGAPORE", bill.ABL_ConsigneeCity);
				AssertEquals(ZString.Empty, bill.ABL_ConsigneeState);
				AssertEquals("406234", bill.ABL_ConsigneePostcode);
				AssertEquals("+6590085135", bill.ABL_ConsigneePhone);
				AssertEquals(Core.Constants.CountryCodes.Singapore, bill.ABL_RN_NKConsigneeCountry);

				AssertEquals(ZGuid.Empty, bill.ABL_OA_Shipper);
				AssertEquals("7-1268 LAIWU WANDE LIANGHANGINTERNA", bill.ABL_ShipperName);
				AssertEquals("NO 8 NEW CROWN ROAD HIGH", bill.ABL_ShipperStreet1);
				AssertEquals("TECH ZONE LAIWU CITY SHANDONG", bill.ABL_ShipperStreet2);
				AssertEquals("KWUN TONG", bill.ABL_ShipperCity);
				AssertEquals(ZString.Empty, bill.ABL_ShipperState);
				AssertEquals("07000", bill.ABL_ShipperPostcode);
				AssertEquals(Core.Constants.CountryCodes.HongKong, bill.ABL_RN_NKShipperCountry);
			});
		}

		public void TestPartyStatus()
		{
			var consignee = CreateOrg("SNOWSILL, SONYA", "SNOWSON", "15 LEANDER ST", "CONSIGNEE STR 2", "FALCON", ZString.Empty, "6201", "+61895343637");
			consignee.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.PartyStatusType, SGPartyStatusList.Codes.A, Core.Constants.CountryCodes.Singapore);
			consignee.CreatePatternMatchingAddressFromMainAddress(Factory);
			consignee.CreatePatternMatchingName(Factory);
			Factory.Save();

			Importer.LoadFile();
			Importer.Save();
			var manifestHeader = GetManifestHeader("08122222222");
			var billCountry = GetBill(manifestHeader, "3824ARFY9JH");
			AssertEquals(SGPartyStatusList.Codes.A, billCountry.SG_PartyStatus);
		}

		public void TestPartyIndicatorImport()
		{
			SetupPayeeAndPartyIDOrganisations();
			var consignee = CreateOrg("SNOWSILL, SONYA1", "SNOWSON", "15 LEANDER ST", "CONSIGNEE STR 2", "FALCON", ZString.Empty, "6201", "+61895343637");
			consignee.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.DirectDelivery, "Test", Core.Constants.CountryCodes.Singapore);
			consignee.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "87654321Z", Core.Constants.CountryCodes.Singapore);
			consignee.CreatePatternMatchingAddressFromMainAddress(Factory);
			consignee.CreatePatternMatchingName(Factory);
			Factory.Save();

			using (UPEDataRegistry.Instance.EnableDecisionSupportImportShipmentsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ImporterForSGAccessForLowValue.LoadFile();
				ImporterForSGAccessForLowValue.Save();
				var manifestHeader = GetManifestHeader("08122222222");
				var billCountry = GetBill(manifestHeader, "3824ARFY9JH");
				AssertEquals("87654321Y", billCountry.SG_PartyID);
				billCountry = GetBill(manifestHeader, "3947808NNSR");
				AssertEquals("", billCountry.SG_PartyID);
				billCountry = GetBill(manifestHeader, "4A14T9J3YYD");
				AssertEquals("87654321Z", billCountry.SG_PartyID);
			}
		}

		public void TestPartyIndicatorExport()
		{
			SetupPayeeAndPartyIDOrganisations();
			using (UPEDataRegistry.Instance.EnableDecisionSupportExportShipmentsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				NotificationBuffer = new NotificationBuffer();
				Level1DataImportForManifest.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1Sample.txt");
				ImporterForSGAccess.LoadFile();
				ImporterForSGAccess.Save();
				var manifestHeader = GetManifestHeader("08122222222");
				var billCountry = GetBill(manifestHeader, "3824ARFY9JH");
				AssertEquals("12345678X", billCountry.SG_PartyID);
			}
		}

		[TestDate(2017, 1, 1)]
		public void TestRecalculateFreightAndInsuranceEnabled()
		{
			var testFileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1SampleWithNeither.txt");
			NotificationBuffer.Clear();
			Environment.Env.Registry.Rating.SetBrokerageRatedCodes("BRK,BON,CDS,FRT");

			Level1DataImport.FileName = testFileName;
			Importer.LoadFile();
			Importer.Save();
			AssertEquals("TotalNoOfShipments", 1, Importer.TotalNoOfShipments);
			AssertContains($"System is unable to recalculate Freight and Insurance values from Level 1 for bill 'AT1665NH8DX', please check your {Core.Constants.ProductName} Freight rating system.", NotificationBuffer.AsString);
			var manifestHeader = GetManifestHeader("08122222222");
			var bills = manifestHeader.Bills;
			AssertEquals("bills.Length", 1, bills.Count);
			AssertEquals(0m, bills[0].ABL_TransportValue);
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, bills[0].ABL_RX_NKTransportValueCurrency);
			AssertEquals(14.39m, bills[0].ABL_InsuranceValue);
			AssertEquals(Core.Constants.CurrencyCodes.Singapore, bills[0].ABL_RX_NKInsuranceValueCurrency);
			manifestHeader.Delete();
			NotificationBuffer.Clear();

			var companyTariff = new Rating.Business.Testing.TestHelper(Factory).NewCompanyTariff();
			var rateEntry = companyTariff.AddRateEntry("AIR", "LSE", "", "SG", "STD", "");
			rateEntry.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.Singapore;
			var airRateLine1 = rateEntry.RateLines[0];
			airRateLine1.RateLineItems.RemoveAndDeleteAll();
			airRateLine1.Calculator["-10"] = (ZDecimal)10m;
			airRateLine1.Calculator["+10"] = (ZDecimal)5m;
			companyTariff.Factory.Save();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "7-1268 LAIWU WANDE LIANGHANGINTERNA";
			consignor.MainAddress.OA_Address1 = "NO 8 NEW CROWN ROAD HIGH";
			consignor.MainAddress.OA_RN_NKCountryCode = "HK";
			consignor.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "LIYANA AZIZ";
			consignee.MainAddress.OA_Address1 = "LIYANA AZIZ              #04-01B DE";
			consignee.MainAddress.OA_RN_NKCountryCode = "SG";
			consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			Factory.Save();

			Level1DataImport.FileName = testFileName;
			Importer.LoadFile();
			Importer.Save();
			AssertEquals("TotalNoOfShipments", 1, Importer.TotalNoOfShipments);
			AssertContains($"Freight and Insurance values have been recalculated for bill 'AT1665NH8DX' based on {Core.Constants.ProductName} Freight rating system.", NotificationBuffer.AsString);
			manifestHeader = GetManifestHeader("08122222222");
			bills = manifestHeader.Bills;
			AssertEquals("bills.Length", 1, bills.Count);
			AssertEquals(1615.88m, bills[0].ABL_TransportValue);
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, bills[0].ABL_RX_NKTransportValueCurrency);
			AssertEquals(26.01m, bills[0].ABL_InsuranceValue);
			AssertEquals(Core.Constants.CurrencyCodes.Singapore, bills[0].ABL_RX_NKInsuranceValueCurrency);
		}

		[TestDate(2017, 1, 1)]
		public void TestRecalculateFreightAndInsuranceDisabled()
		{
			var testFileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1SampleWithNeither.txt");
			Environment.Env.Registry.Rating.SetBrokerageRatedCodes("BRK,BON,CDS,FRT");

			using (UPEDataRegistry.Instance.EnableFreightAutoRatingInLevelOneImportItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Level1DataImport.FileName = testFileName;
				Importer.LoadFile();
				Importer.Save();
				AssertEquals("TotalNoOfShipments", 1, Importer.TotalNoOfShipments);
				var manifestHeader = GetManifestHeader("08122222222");
				var bills = manifestHeader.Bills;
				AssertEquals("bills.Length", 1, bills.Count);
				AssertEquals(0m, bills[0].ABL_TransportValue);
				AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, bills[0].ABL_RX_NKTransportValueCurrency);
				AssertEquals(14.39m, bills[0].ABL_InsuranceValue);
				AssertEquals(Core.Constants.CurrencyCodes.Singapore, bills[0].ABL_RX_NKInsuranceValueCurrency);
			}
		}

		public void TestEDocAdded()
		{
			Importer.LoadFile();
			Importer.Save();
			var manifestHeader = GetManifestHeader("08122222222");
			CombineAssertions(() =>
			{
				var eDocs = ((IDocManagerSupport)manifestHeader).DocManagerInfo.Files;
				var edoc = eDocs.GetMostRecentEDoc(Core.Constants.RefDocTypes.MiscellaneousDocument);
				AssertEquals("edoc FileName", "Level1ImportSummary.txt", edoc.FileName);
				AssertEquals("edoc Description", "Level 1 Import Summary Log", edoc.Description);
				var edocText = edoc.ImageData.ToAscii().ToString();
				AssertContains("File Contents Flight Summary", "Flight Details Summary", edocText);
				AssertContains("File Contents File Summary", "File Summary", edocText);
				AssertContains("File Contents Empty Shipments", "Empty Shipments Not Imported", edocText);
				AssertContains("File Contents Start Information", "Save started at:", edocText);
				AssertContains("File Contents Starting Level 1 to XML", "Starting Level 1 to Universal XML process", edocText);
				AssertContains("File Contents Creating Global Manifest from XML", "Creating Global Manifest Jobs. This may take some time.", edocText);
				AssertContains("File Contents Global Manifest Successfull", "Successfully saved Manifest Header", edocText);
				AssertContains("File Contents Not Imported", "Shipments Not Imported", edocText);
				AssertContains("File Contents Save Information", "Save completed at:", edocText);
				AssertContains("File Contents Duration Information", "Duration (minutes):", edocText);
				AssertContains("File Contents Organization Matching", "Matching Organizations for Bill: 3824ARFY9JH", edocText);
				AssertContains("File Contents Organization Matching", "Matching Organizations for Bill: 3947808NNSR", edocText);
				AssertContains("File Contents Organization Matching", "Matching Organizations for Bill: 4A14T9J3YYD", edocText);
			});
		}

		public void TestShipmentNotLoadedWhenNo200000Record()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1No200000Record.txt");
			Importer.LoadFile();
			Importer.Save();
			AssertEquals("TotalNoOfShipments", 1, Importer.TotalNoOfShipments);
			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals("bills.Length", 1, manifestHeader.Bills.Count);
			AssertEquals("3947808NNSR", manifestHeader.Bills[0].ABL_BillNumber);
		}

		public void TestShipmentNotLoadedWhenNo202000Record()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1No202000Record.txt");
			Importer.LoadFile();
			Importer.Save();
			AssertEquals("TotalNoOfShipments", 1, Importer.TotalNoOfShipments);
			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals("bills.Length", 1, manifestHeader.Bills.Count);
			AssertEquals("3824ARFY9JH", manifestHeader.Bills[0].ABL_BillNumber);
		}

		public void TestShipmentNotLoadedWhenNoPackageTrackingNumber()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1NoPackageTrackingNumber.txt");
			Importer.LoadFile();
			Importer.Save();
			AssertEquals("TotalNoOfShipments", 1, Importer.TotalNoOfShipments);
			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals("bills.Length", 1, manifestHeader.Bills.Count);
			AssertEquals("3824ARFY9JH", manifestHeader.Bills[0].ABL_BillNumber);
		}

		public void TestShipmentNotLoadedWhenNoTrackingNumber()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1NoTrackingNumber.txt");
			Importer.LoadFile();
			Importer.Save();
			AssertEquals("TotalNoOfShipments", 1, Importer.TotalNoOfShipments);
			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals("bills.Length", 1, manifestHeader.Bills.Count);
			AssertEquals("3947808NNSR", manifestHeader.Bills[0].ABL_BillNumber);
		}

		public void TestMajorExporterNotSetForHighValueExportShipment()
		{
			SetupMajorExporterConsignee();
			Level1DataImportForManifest.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithHighValueMajorExporterConsignee.txt");
			ImporterForSGAccess.LoadFile();
			ImporterForSGAccess.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals("bills.Length", 1, manifestHeader.Bills.Count);
			AssertEquals("AT1665NN3NX", manifestHeader.Bills[0].ABL_BillNumber);
			AssertEquals(1, manifestHeader.Bills[0].Packs.Count);
			AssertNotEquals(Level1DataFileImporterForSGAccess.Constants.GoodsType.MajorExporter, manifestHeader.Bills[0].Packs[0].PackedItem.GoodsType);
		}

		public void TestMajorExporterNotSetForLowValueImportShipment()
		{
			SetupMajorExporterConsignee();
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithLowValueMajorExporterConsignee.txt");
			Importer.LoadFile();
			Importer.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals("bills.Length", 1, manifestHeader.Bills.Count);
			AssertEquals("AT1665NH8DX", manifestHeader.Bills[0].ABL_BillNumber);
			AssertEquals(1, manifestHeader.Bills[0].Packs.Count);
			AssertNotEquals(Level1DataFileImporterForSGAccess.Constants.GoodsType.MajorExporter, manifestHeader.Bills[0].Packs[0].PackedItem.GoodsType);
		}

		public void TestMajorExporterSetForHighValueImportShipment()
		{
			SetupMajorExporterConsignee();
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithHighValueMajorExporterConsignee.txt");
			Importer.LoadFile();
			Importer.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals("bills.Length", 1, manifestHeader.Bills.Count);
			AssertEquals("AT1665NN3NX", manifestHeader.Bills[0].ABL_BillNumber);
			AssertEquals(1, manifestHeader.Bills[0].Packs.Count);
			AssertEquals(Level1DataFileImporterForSGAccess.Constants.GoodsType.MajorExporter, manifestHeader.Bills[0].Packs[0].PackedItem.GoodsType);
		}

		public void TestTariffAndInsuranceValueClearedGoodsDescriptionDefaultedForLowValueImportShipment()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithNormalGoodsShipments.txt");
			Importer.LoadFile();
			Importer.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals("bills.Length", 2, manifestHeader.Bills.Count);
			var lowValueBill = GetBill(manifestHeader, "AT1665NH8DX");
			CombineAssertions(() =>
			{
				AssertEquals("Bill Pack Count", 2, lowValueBill.Packs.Count);
				AssertEquals("Pack Line 1 Tariff", ZString.Empty, lowValueBill.Packs[0].PackedItem.API_Tariff);
				AssertEquals("Pack Line 1 Qty", 1000m, lowValueBill.Packs[0].PackedItem.API_CustomsQty);
				AssertEquals("Pack Line 1 UQ", UnitOfQuantityCodeList.Codes.KGM, lowValueBill.Packs[0].PackedItem.API_CustomsUQ);
				AssertEquals("Pack Line 1 Goods Description", "NOTEBOOK PU PAPER", lowValueBill.Packs[0].PackedItem.API_GoodsDescription);
				AssertEquals("Pack Line 1 Customs Value", 143.88m, lowValueBill.Packs[0].PackedItem.API_CustomsValue);
				AssertEquals("Pack Line 1 Duty Amount", 0m, lowValueBill.Packs[0].PackedItem.API_DutyAmount);
				AssertEquals("Pack Line 1 Tax AMount", 10.07m, lowValueBill.Packs[0].PackedItem.API_TaxAmount);
				AssertEquals("Pack Line 2 Tariff", ZString.Empty, lowValueBill.Packs[1].PackedItem.API_Tariff);
				AssertEquals("Pack Line 2 Qty", 1000m, lowValueBill.Packs[1].PackedItem.API_CustomsQty);
				AssertEquals("Pack Line 2 UQ", UnitOfQuantityCodeList.Codes.NMB, lowValueBill.Packs[1].PackedItem.API_CustomsUQ);
				AssertEquals("Pack Line 2 Goods Description", "NOTEBOOK PU PAPER", lowValueBill.Packs[1].PackedItem.API_GoodsDescription);
				AssertEquals("Pack Line 2 Customs Value", 179.86m, lowValueBill.Packs[1].PackedItem.API_CustomsValue);
				AssertEquals("Pack Line 2 Duty Amount", 0m, lowValueBill.Packs[1].PackedItem.API_DutyAmount);
				AssertEquals("Pack Line 2 Tax AMount", 12.59m, lowValueBill.Packs[1].PackedItem.API_TaxAmount);
				AssertEquals("Bill Insurance Value", 0m, lowValueBill.ABL_InsuranceValue);
				AssertEquals("Total Value", 323.74m, lowValueBill.ABL_CustomsValue);
				AssertEquals("Bill Goods Description", "NOTEBOOK PU PAPER", lowValueBill.ABL_GoodsDescription);
			});
		}

		public void TestTariffAndInsuranceValueClearedGoodsDescriptionDefaultedForLowValueExportShipment()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.ExportDeminimus, 1000m, "SG", 0m, 0m, "", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), "Export Deminimus");
			helper.CreateTaxOrFee(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 0.07m, Core.Constants.CountryCodes.Singapore, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), "Goods and Services Tax");
			Factory.Save();

			Level1DataImportForManifest.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithNormalGoodsShipments.txt");
			ImporterForSGAccess.LoadFile();
			ImporterForSGAccess.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals("bills.Length", 2, manifestHeader.Bills.Count);
			var lowValueBill = GetBill(manifestHeader, "AT1665NH8DX");
			CombineAssertions(() =>
			{
				AssertEquals("Bill Pack Count", 2, lowValueBill.Packs.Count);
				AssertEquals("Pack Line 1 Tariff", ZString.Empty, lowValueBill.Packs[0].PackedItem.API_Tariff);
				AssertEquals("Pack Line 1 Qty", 1000m, lowValueBill.Packs[0].PackedItem.API_CustomsQty);
				AssertEquals("Pack Line 1 UQ", UnitOfQuantityCodeList.Codes.KGM, lowValueBill.Packs[0].PackedItem.API_CustomsUQ);
				AssertEquals("Pack Line 1 Goods Description", "NOTEBOOK PU PAPER", lowValueBill.Packs[0].PackedItem.API_GoodsDescription);
				AssertEquals("Pack Line 1 Customs Value", 143.88m, lowValueBill.Packs[0].PackedItem.API_CustomsValue);
				AssertEquals("Pack Line 1 Duty Amount", 0m, lowValueBill.Packs[0].PackedItem.API_DutyAmount);
				AssertEquals("Pack Line 1 Tax AMount", 10.07m, lowValueBill.Packs[0].PackedItem.API_TaxAmount);
				AssertEquals("Pack Line 2 Tariff", ZString.Empty, lowValueBill.Packs[1].PackedItem.API_Tariff);
				AssertEquals("Pack Line 2 Qty", 1000m, lowValueBill.Packs[1].PackedItem.API_CustomsQty);
				AssertEquals("Pack Line 2 UQ", UnitOfQuantityCodeList.Codes.NMB, lowValueBill.Packs[1].PackedItem.API_CustomsUQ);
				AssertEquals("Pack Line 2 Goods Description", "NOTEBOOK PU PAPER", lowValueBill.Packs[1].PackedItem.API_GoodsDescription);
				AssertEquals("Pack Line 2 Customs Value", 179.86m, lowValueBill.Packs[1].PackedItem.API_CustomsValue);
				AssertEquals("Pack Line 2 Duty Amount", 0m, lowValueBill.Packs[1].PackedItem.API_DutyAmount);
				AssertEquals("Pack Line 2 Tax AMount", 12.59m, lowValueBill.Packs[1].PackedItem.API_TaxAmount);
				AssertEquals("Bill Insurance Value", 0m, lowValueBill.ABL_InsuranceValue);
				AssertEquals("Total Value", 323.74m, lowValueBill.ABL_CustomsValue);
				AssertEquals("Bill Goods Description", "NOTEBOOK PU PAPER", lowValueBill.ABL_GoodsDescription);
			});
		}

		public void TestLowValueExportShipment()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.ExportDeminimus, 1000m, "SG", 0m, 0m, "", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), "Export Deminimus");
			Factory.Save();

			SetupMajorExporterConsignee();
			Level1DataImportForManifest.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithLowValueMajorExporterConsignee.txt");
			ImporterForSGAccess.LoadFile();
			ImporterForSGAccess.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals("bills.Length", 1, manifestHeader.Bills.Count);
			AssertEquals("AT1665NH8DX", manifestHeader.Bills[0].ABL_BillNumber);
			AssertEquals(1, manifestHeader.Bills[0].Packs.Count);
		}

		[TestDate(2018, 06, 01)]
		public void TestTariffAndInsuranceValueNotClearedGoodsDescriptionNotDefaultedForLowValueControlledShipment()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithControlledGoodsShipments.txt");
			Importer.LoadFile();
			Importer.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals("bills.Length", 2, manifestHeader.Bills.Count);
			var lowValueBill = GetBill(manifestHeader, "AE5168THCZS");
			CombineAssertions(() =>
			{
				AssertEquals("Bill Pack Count", 2, lowValueBill.Packs.Count);
				var packLine1 = lowValueBill.Packs[0].PackedItem;
				AssertEquals("Pack Line 1 Tariff", "85176900", packLine1.API_Tariff);
				AssertEquals("Pack Line 1 Goods Description", "ESTIMOTE DEVELOPMENT KIT WITH PROXIMITY BEACONS", packLine1.API_GoodsDescription);
				var packLine2 = lowValueBill.Packs[1].PackedItem;
				AssertEquals("Pack Line 2 Tariff", "85176900", packLine2.API_Tariff);
				AssertEquals("Pack Line 2 Goods Description", "ESTIMOTE STICKERS DEVELOPER KIT", packLine2.API_GoodsDescription);
				AssertEquals("Bill Insurance Value", 1.14m, lowValueBill.ABL_InsuranceValue);
				AssertEquals("Total Value", 114.81m, lowValueBill.ABL_CustomsValue);
				AssertEquals("Bill Goods Description", "ESTIMOTE DEVELOPMENT KIT WITH PROXIMITY BEACONS", lowValueBill.ABL_GoodsDescription);
			});
		}

		public void TestTariffAndInsuranceValueNotClearedGoodsDescriptionNotDefaultedForLowValueFreeTradeZoneShipment()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithFreeTradeZoneShipments.txt");
			Importer.LoadFile();
			Importer.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals("bills.Length", 2, manifestHeader.Bills.Count);
			var lowValueBill = GetBill(manifestHeader, "AT1665NN3NX");
			CombineAssertions(() =>
			{
				AssertEquals(1, lowValueBill.Packs.Count);
				AssertEquals("Pack Line 1 Tariff", "48201000", lowValueBill.Packs[0].PackedItem.API_Tariff);
				AssertEquals("Pack Line 1 Goods Description", "CIRCUIT BOARD  MINI PC IRON", lowValueBill.Packs[0].PackedItem.API_GoodsDescription);
				AssertEquals("Bill Insurance Value", 0.26m, lowValueBill.ABL_InsuranceValue);
				AssertEquals("Total Value", 26.75m, lowValueBill.ABL_CustomsValue);
				AssertEquals("Bill Goods Description", "CIRCUIT BOARD  MINI PC IRON", lowValueBill.ABL_GoodsDescription);
			});
		}

		public void TestTariffAndInsuranceValueNotClearedGoodsDescriptionNotDefaultedForLowValueControlledStopWordsShipment()
		{
			using (UPEDataRegistry.Instance.StopPhrasesForSGGoodsDescriptionItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SISYPHUS"))
			{
				Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithStopWordsShipment.txt");
				Importer.LoadFile();
				Importer.Save();
				var manifestHeader = GetManifestHeader("08122222222");
				AssertEquals("bills.Length", 2, manifestHeader.Bills.Count);
				var lowValueBill = GetBill(manifestHeader, "AT1665NH8XZ");
				CombineAssertions(() =>
				{
					AssertEquals(1, lowValueBill.Packs.Count);
					AssertEquals("Pack Line 1 Tariff", "48201000", lowValueBill.Packs[0].PackedItem.API_Tariff);
					AssertEquals("Pack Line 1 Goods Description", "NOTEBOOK PU PAPER SISYPHUS", lowValueBill.Packs[0].PackedItem.API_GoodsDescription);
					AssertEquals("Bill Insurance Value", 1.44m, lowValueBill.ABL_InsuranceValue);
					AssertEquals("Total Value", 145.32m, lowValueBill.ABL_CustomsValue);
					AssertEquals("Bill Goods Description", "NOTEBOOK PU PAPER SISYPHUS", lowValueBill.ABL_GoodsDescription);
				});
			}
		}

		public void TestCustomsPackTypeConversionFallbackWithATariffContainingSGCustomsInvalidUnitOfQuantity()
		{
			CreateTariff("1020304050", CommodityTypeList.Codes.Petroleum, ZDecimal.Zero, 15m, "VAL", ZBool.False, new ZDateTime(2017, 11, 1), new ZDateTime(2017, 11, 3));
			Factory.Save();
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1TariffWithInvalidUQ.txt");
			Importer.LoadFile();
			Importer.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals("bills.Length", 1, manifestHeader.Bills.Count);
			var lowValueBill = GetBill(manifestHeader, "AT1665NH8DX");
			CombineAssertions(() =>
			{
				AssertEquals("Bill Pack Count", 1, lowValueBill.Packs.Count);
				AssertEquals("Pack Line Qty", 1000m, lowValueBill.Packs[0].PackedItem.API_CustomsQty);
				AssertEquals("Pack Line UQ", UnitOfQuantityCodeList.Codes.KGM, lowValueBill.Packs[0].PackedItem.API_CustomsUQ);
			});
		}

		public void TestCustomsPackTypeConversionFallbackWithARefPackContainingSGCustomsInvalidUnitOfQuantity()
		{
			CreateTariff("1020304050", CommodityTypeList.Codes.Petroleum, ZDecimal.Zero, 15m, "VAL", ZBool.False, new ZDateTime(2017, 11, 1), new ZDateTime(2017, 11, 3));
			Factory.Save();
			CreateRefPack("KGM", "VAL", 1m);
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1TariffWithInvalidUQ.txt");
			Importer.LoadFile();
			Importer.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals("bills.Length", 1, manifestHeader.Bills.Count);
			var lowValueBill = GetBill(manifestHeader, "AT1665NH8DX");
			CombineAssertions(() =>
			{
				AssertEquals("Bill Pack Count", 1, lowValueBill.Packs.Count);
				AssertEquals("Pack Line Qty", 1000m, lowValueBill.Packs[0].PackedItem.API_CustomsQty);
				AssertEquals("Pack Line UQ", UnitOfQuantityCodeList.Codes.KGM, lowValueBill.Packs[0].PackedItem.API_CustomsUQ);
			});
		}

		public void TestCustomsPackTypeConversionFallbackWithInvalidTariffUQAndValidRefPackUQ()
		{
			CreateRefPack("ABC", "TNE", 0.001m);
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithNoTariff.txt");
			Importer.LoadFile();
			Importer.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals("bills.Length", 1, manifestHeader.Bills.Count);
			var lowValueBill = GetBill(manifestHeader, "AT1665NH8DX");
			CombineAssertions(() =>
			{
				AssertEquals("Bill Pack Count", 1, lowValueBill.Packs.Count);
				AssertEquals("Pack Line Qty", 1m, lowValueBill.Packs[0].PackedItem.API_CustomsQty);
				AssertEquals("Pack Line UQ", UnitOfQuantityCodeList.Codes.TNE, lowValueBill.Packs[0].PackedItem.API_CustomsUQ);
			});
		}

		public void TesttCustomsPackTypeConversionFallbackToUPSDefaultNMB()
		{
			Level1DataImportForManifest.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithNoTariff.txt");
			ImporterForSGAccess.LoadFile();
			ImporterForSGAccess.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals("bills.Length", 1, manifestHeader.Bills.Count);
			var lowValueBill = GetBill(manifestHeader, "AT1665NH8DX");
			CombineAssertions(() =>
			{
				AssertEquals("Bill Pack Count", 1, lowValueBill.Packs.Count);
				AssertEquals("Pack Line Qty", 1000m, lowValueBill.Packs[0].PackedItem.API_CustomsQty);
				AssertEquals("Pack Line UQ", UnitOfQuantityCodeList.Codes.NMB, lowValueBill.Packs[0].PackedItem.API_CustomsUQ);
			});
		}

		public void TestLowValueShipmentWithZeroQuantitiesOnPackLinesDefaultToOne()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithZeroPackQuantities.txt");
			Importer.LoadFile();
			Importer.Save();

			var manifestHeader = GetManifestHeader("08122222222");
			AssertEquals("bills.Length", 2, manifestHeader.Bills.Count);
			var highBill = GetBill(manifestHeader, "3824ARFY9JH");
			CombineAssertions(() =>
			{
				foreach (AsycudaPack packLine in highBill.Packs)
				{
					Assert($"Pack Line Qty on High Bill is 0", packLine.APA_PackQty.IsEmpty);
					AssertEquals($"Customs Pack Line Qty on High Bill stays 0", 0m, packLine.PackedItem.API_CustomsQty);
				}
			});
			var lowBill = GetBill(manifestHeader, "3947808NNSR");
			CombineAssertions(() =>
			{
				foreach (AsycudaPack packLine in lowBill.Packs)
				{
					Assert($"Pack Line Qty on Low Bill is 0", packLine.APA_PackQty.IsEmpty);
					AssertEquals($"Customs Pack Line Qty on Low Bill updated to 1", 1m, packLine.PackedItem.API_CustomsQty);
				}
			});
		}

		public void TestPopulateShippingAgent()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TSTSHIPPER";
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_Code = "SHPADDR1";
			orgAddress.OA_Address1 = "TEST ADDRESS 1";
			Factory.Save();

			var level1DataImport = new Level1DataImport(Factory)
			{
				FlightNumber = "QF656",
				MasterBill = "08122222222",
				ArrivalDate = new ZDateTime(2017, 6, 13),
				PortOfLoading = "AUSYD",
				PortOfDischarge = "SGSIN",
				ShippingAgentAddress = orgAddress.PK,
				CycleDate = new ZDateTime(2017, 6, 15),
				CycleNumber = fLevel1DataImport.CycleNumbers[4].Code,
				DepartureDate = new ZDateTime(2017, 6, 14),
				FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1Sample.txt")
			};

			importer = new Level1DataFileImporterForSGAccess(level1DataImport, NotificationBuffer);
			Importer.LoadFile();
			Importer.Save();

			var manifestHeader = GetManifestHeader(level1DataImport.MasterBill);
			AssertEquals(orgAddress.PK, manifestHeader.AMA_OA_ShippingAgent);
			AssertContains("Shipping Agent : TEST ADDRESS 1 SG", importer.GetSummaryInformation());
		}

		public void TestCalculateGST()
		{
			testDataHelper.CreateTaxOrFee(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 0.08m, Core.Constants.CountryCodes.Singapore, ZDate.Today.AddDays(-5), ZDate.Today.AddDays(5), "Goods and Services Tax");
			testDataHelper.CreateTaxOrFee(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 0.09m, Core.Constants.CountryCodes.Singapore, ZDate.Today.AddDays(-15), ZDateTime.Today.AddDays(-10), "Goods and Services Tax");
			Factory.Save();

			Importer.level1DataImport.PortOfLoading = "JPTYA";
			Importer.level1DataImport.PortOfDischarge = "SGSIN";
			Importer.level1DataImport.CycleDate = ZDateTime.Today.AddDays(-12);
			var gst = Importer.CalculateGST(2.0m);
			AssertEquals("GST should be calculated based on the value in RefCusTaxOrFee", 0.18m, gst);

			Importer.level1DataImport.PortOfLoading = "SGSIN";
			Importer.level1DataImport.PortOfDischarge = "JPTYA";
			gst = Importer.CalculateGST(2.0m);
			AssertEquals("Tax amount should be calculated based on the value in RefCusTaxOrFee", 0.16m, gst);
		}

		[SnailTest]
		public void TestLevel1ImportDBHitPerformance()
		{
			var companytariff = Factory.New<CompanyTariff>();
			companytariff.AddRateEntry("AIR", "LSE", "USLAX", "SGSIN");
			Factory.New<GlobalTariff>();
			Factory.Save();

			QueryStackTraceRecorder.Instance.Enabled = true;

			SetupMajorExporterConsignee();
			CreateOrg("BIOTECH CORP", "SHIPTEST1", "3824AR");
			var consignee = CreateOrg("SNOWSILL, SONYA", "CGNTEST1", "8AU0495926");

			CreatePart("PART1", consignee, "50607080");
			CreatePart("PART2", consignee, "40506070");
			CreateTariff("50607080", ZString.Empty, 20m, ZDecimal.Zero, ZString.Empty, ZBool.False);
			CreateTariff("40506070", ZString.Empty, 20m, ZDecimal.Zero, ZString.Empty, ZBool.False);
			CreateTariff("4820100000", ZString.Empty, 20m, ZDecimal.Zero, ZString.Empty, ZBool.False);
			var tariff = CreateTariff("102030", ZString.Empty, 20m, ZDecimal.Zero, ZString.Empty, ZBool.False);
			testDataHelper.CreateCommodity(tariff, "COM102030", ZDateTime.Today.AddYears(-2), ZDateTime.Today.AddYears(-1));

			var expectedHits = new Dictionary<string, int>
			{
				{ AsycudaBillSchema.Constants.TableName, 11 },
				{ AsycudaContainerBillOrPackageLinkSchema.Constants.TableName, 8 },
				{ AsycudaManifestHeaderSchema.Constants.TableName, 8 },
				{ AsycudaPackSchema.Constants.TableName, 5 },
				{ AsycudaPackedItemSchema.Constants.TableName, 14 },
				{ AsycudaPackPackedItemPivotSchema.Constants.TableName, 16 },
				{ CusContainerSchema.Constants.TableName, 7 },
				{ CusDecHouseBillSchema.Constants.TableName, 9 },
				{ CusEntryNumSchema.Constants.TableName, 13 },
				{ CusRefRateCodeViewSchema.Constants.TableName, 2 },
				{ CusRefTariffVersionSchema.Constants.TableName, 3 },
				{ CusRefTradeGroupViewSchema.Constants.TableName, 14 },
				{ EDIMessageSchema.Constants.TableName, 1 },
				{ GenAddOnColumnSchema.Constants.TableName, 19 },
				{ GenCustomAddOnValueSchema.Constants.TableName, 9 },
				{ GenCustomColumnDefinitionSchema.Constants.TableName, 5 },
				{ GlbBranchSchema.Constants.TableName, 0 },
				{ JobComInvoiceHeaderSchema.Constants.TableName, 3 },
				{ JobDeclarationSchema.Constants.TableName, 15 },
				{ JobDocAddressSchema.Constants.TableName, 2 },
				{ JobDocsAndCartageSchema.Constants.TableName, 7 },
				{ JobHeaderSchema.Constants.TableName, 7 },
				{ OrgAddressSchema.Constants.TableName, 35 },
				{ OrgCusCodeSchema.Constants.TableName, 8 },
				{ OrgHeaderSchema.Constants.TableName, 55 },
				{ OrgPatternMatchSchema.Constants.TableName, 42 },
				{ OrgRelatedPartySchema.Constants.TableName, 11 },
				{ OrgSupplierBuyerLinkSchema.Constants.TableName, 3 },
				{ OrgSupplierPartBarcodeSchema.Constants.TableName, 3 },
				{ OrgSupplierPartSchema.Constants.TableName, 4 },
				{ RatingHeaderSchema.Constants.TableName, 11 },
				{ RefCountrySchema.Constants.TableName, 3 },
				{ RefCusProcedureSchema.Constants.TableName, 2 },
				{ RefCusTariffTypeSchema.Constants.TableName, 2 },
				{ RefCusTaxOrFeeSchema.Constants.TableName, 19 },
				{ RefDataGroupingSchema.Constants.TableName, 32 },
				{ RefExchangeRateSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 4 },
				{ RefPackTypeSchema.Constants.TableName, 2 },
				{ RefUNLOCOSchema.Constants.TableName, 5 },
				{ RefZoneHeaderSchema.Constants.TableName, 2 },
				{ RefZonePivotSchema.Constants.TableName, 4 },
				{ StmALogSchema.Constants.TableName, 3 },
				{ TariffAttributeViewSchema.Constants.TableName, 3 },
				{ TariffUOMViewSchema.Constants.TableName, 3 },
				{ TariffRelationshipViewSchema.Constants.TableName, 1 },
				{ TariffViewSchema.Constants.TableName, 3 },
				{ UNDGDataItemSchema.Constants.TableName, 7 },
				{ VATApplicabilityViewSchema.Constants.TableName, 2 },
				{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 9 },
			};

			var ignoredTables = new List<string>
			{
				DtbBookingConsolidationSchema.Constants.TableName,
				GlbPortDeliveryTimeSchema.Constants.TableName,
				JobCartageSchema.Constants.TableName,
				JobSailingSchema.Constants.TableName,
				JobVoyageSchema.Constants.TableName,
				JobVoyDestinationSchema.Constants.TableName,
				JobVoyOriginSchema.Constants.TableName,
				JobRequiredDocumentSchema.Constants.TableName,
				OrgAddressCapabilitySchema.Constants.TableName,
				OrgBrandOrRelatedNameSchema.Constants.TableName,
				OrgCompanyDataSchema.Constants.TableName,
				OrgContactSchema.Constants.TableName,
				OrgMiscServSchema.Constants.TableName,
				OrgTranslatedAddressSchema.Constants.TableName,
				OrgWebURLSchema.Constants.TableName,
				ProcessCompanyLinkRuleSchema.Constants.TableName,
				ProcessJobTriggerLinkSchema.Constants.TableName,
				ProcessTaskNotificationSchema.Constants.TableName,
				ProcessTasksSchema.Constants.TableName,
				ProcessTaskTemplateSchema.Constants.TableName,
				ProcessHeaderSchema.Constants.TableName,
				RateEntrySchema.Constants.TableName,
				RefCountryRequiredDocumentSchema.Constants.TableName,
				RefCountryRulesSchema.Constants.TableName,
				RefVesselSchema.Constants.TableName,
				StmDocDataOverrideSchema.Constants.TableName,
				StmEventSchema.Constants.TableName,
				StmNoteSchema.Constants.TableName,
				StmUniversalCopySchema.Constants.TableName,
				RefAirlineSchema.Constants.TableName,
				"NonPersitentTable JOBCOMINVOICELINE",
				"NonPersitentTable ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE",
				JobCO2eSchema.Constants.TableName,
			};

			//Amnesty test fix below to do with how the UPE Registry is loaded and used in specific UPE BizO's.
			//The below loads all the UPERegistry Entries which are cached, second run excludes all the registry hits for OrgHeader which produces consistent hit results.
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1Transhipment.txt");
			Importer.LoadFile();
			Importer.Save();

			var level1DataImport = new Level1DataImport(new BusinessObjectFactory(), true)
			{
				FlightNumber = "PF123",
				ArrivalDate = new ZDateTime(2019, 6, 20),
				PortOfLoading = "AUMEL",
				PortOfDischarge = "SGSIN",
				CycleDate = new ZDateTime(2019, 6, 20),
				CycleNumber = "4",
				MasterBill = "08122222223",
				DepartureDate = new ZDateTime(2019, 6, 19),
				FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1Performance.txt")
			};

			importer = new Level1DataFileImporterForSGAccess(level1DataImport, new NotificationBuffer());
			importer.LoadFile();
			using (UPEDataRegistry.Instance.StopPhrasesForSGConsignorAddressItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SISYPHUS"))
			using (AssertDbHitsForAllFactories("Performance - Load And Save", expectedHits, useOnlyNewFactories: true, acceptableVariance: 2, tablesToIgnore: ignoredTables))
			{
				importer.Save();
			}
		}

		public void TestBothAccessAndTradenetForLowValueOrganisationStopWordsShipments()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "7-1268 LAIWU WANDE LIANGHANGINTERNA";
			consignor.MainAddress.OA_Address1 = "NO 8 NEW CROWN ROAD HIGH";
			consignor.MainAddress.OA_RN_NKCountryCode = "HK";
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "LIYANA AZIZ";
			consignee.MainAddress.OA_Address1 = "LIYANA AZIZ              #04-01B DE";
			consignee.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			Factory.Save();

			using (UPEDataRegistry.Instance.StopPhrasesForSGConsigneeAddressItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SISYPHUS"))
			using (UPEDataRegistry.Instance.StopPhrasesForSGConsignorAddressItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SISYPHUS"))
			{
				Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WithOrgansiationStopWords.txt");
				Importer.LoadFile();
				Importer.Save();

				AssertBillsCreated("08122222222", 2, "AT1665NH8XX|AT1665NH8XZ");
				AssertDeclarationsCreated("08122222222", 2, "AT1665NH8XX|AT1665NH8XZ");
			}
		}

		public void TestCustomizedFieldDataForBill()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "SIN Global Manifest";
			template.P0_ProcessType = "GMB";
			template.GlobalTemplate = true;
			var billingTerms = template.GenCustomColumnDefinitions.AddNew();
			billingTerms.XC_Name = Level1DataFileImporterForSGAccess.Constants.CustomizedFieldConstants.Bill.BillingTerms;
			billingTerms.XC_Type = AddOnColumnDataType.Codes.String;
			var serviceLevel = template.GenCustomColumnDefinitions.AddNew();
			serviceLevel.XC_Name = Level1DataFileImporterForSGAccess.Constants.CustomizedFieldConstants.Bill.ServiceLevel;
			serviceLevel.XC_Type = AddOnColumnDataType.Codes.String;
			var longTrackingNumber = template.GenCustomColumnDefinitions.AddNew();
			longTrackingNumber.XC_Name = Level1DataFileImporterForSGAccess.Constants.CustomizedFieldConstants.Bill.LongTrackingNumber;
			longTrackingNumber.XC_Type = AddOnColumnDataType.Codes.String;

			Importer.LoadFile();
			Importer.Save();
			var manifestHeader = GetManifestHeader("08122222222");
			var bill = manifestHeader.Bills.Cast<AsycudaBill>().FirstOrDefault(x => x.ABL_BillNumber == "3824ARFY9JH");
			CombineAssertions(() =>
			{
				AssertEquals("Billing Terms", "P/P", bill.GetCustomField(Level1DataFileImporterForSGAccess.Constants.CustomizedFieldConstants.Bill.BillingTerms, null));
				AssertEquals("Long Tracking Number", "1Z3824AR6640806327", bill.GetCustomField(Level1DataFileImporterForSGAccess.Constants.CustomizedFieldConstants.Bill.LongTrackingNumber, null));
				AssertEquals("Service Level", "1", bill.GetCustomField(Level1DataFileImporterForSGAccess.Constants.CustomizedFieldConstants.Bill.ServiceLevel, null));
			});
		}

		public void TestCurrentFactoryProviderFactoryNameForDebugging()
		{
			AssertEquals("Name For Debugging Correct", "Level1 Data File Importer", Importer.FactoryProvider.Current.NameForDebugging);
			Importer.FactoryProvider.CreateNewWithoutSave();
			AssertEquals("Name For Debugging Correct", "Level1 Data File Importer", Importer.FactoryProvider.Current.NameForDebugging);
		}

		public void TestOrganizationMatchShouldTakeThresholdIntoAccount_WhenLowerThanTheThreshold()
		{
			SetOrganizationAsWI00271624();

			using (Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 80))
				{
					Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WI00271624.txt");
					Importer.LoadFile();
					Importer.Save();

					var bill = Factory.LoadTop1<AsycudaBill>(new ZQuery(AsycudaBillSchema.ABL_BillNumber, "94764RGS4I2"));
					var pack = bill.Packs[0];
					AssertEquals(ZString.Empty, bill.SG_PartyID);
					AssertEquals(SGPartyStatusList.Codes.N, bill.SG_PartyStatus);
					AssertEquals(Enterprise.Customs.SG.Access.Business.Constants.GoodsType.NormalGoods, pack.PackedItem.GoodsType);
				}
			}
		}

		public void TestOrganizationMatchShouldTakeThresholdIntoAccount_WhenHigherThanTheThreshold()
		{
			SetOrganizationAsWI00271624();

			using (Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 40))
				{
					Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1WI00271624.txt");
					Importer.LoadFile();
					Importer.Save();

					var bill = Factory.LoadTop1<AsycudaBill>(new ZQuery(AsycudaBillSchema.ABL_BillNumber, "94764RGS4I2"));
					var pack = bill.Packs[0];
					AssertEquals("19921029JEG", bill.SG_PartyID);
					AssertEquals(SGPartyStatusList.Codes.Y, bill.SG_PartyStatus);
					AssertEquals(Enterprise.Customs.SG.Access.Business.Constants.GoodsType.MajorExporter, pack.PackedItem.GoodsType);
				}
			}
		}

		void SetOrganizationAsWI00271624()
		{
			var sgsin = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "SGSIN"));
			var scheduleDlocoMap = sgsin.RefLocoMaps.AddNew();
			scheduleDlocoMap.RY_RN = Core.Constants.CountryGuids.Singapore;
			scheduleDlocoMap.RY_SystemUsage = UPEOtherLocoMapSystemUsageList.Codes.Ups;
			scheduleDlocoMap.RY_LocalPortCode = "8869";

			Environment.Env.Registry.CanUserEditOrganisationCode = true;
			var org = Factory.New<OrgHeader>();
			org.OH_Category = OrgConstants.Category.Business;
			org.OH_FullName = "JOHNSON & JOHNSON SINGAPORE";
			org.OH_RL_NKClosestPort = "SGSIN";
			org.OH_Code = "JOHJOHSIN4";
			org.OH_IsConsignee = true;
			org.OH_IsConsignor = true;
			org.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "19921029JEG");
			org.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.PartyStatusType, SGPartyStatusList.Codes.Y);
			var address = org.MainAddress;
			address.OA_Code = "10 PIONEER CRESCENT, LEVE";
			address.PrimaryOrgAddressAdditionalInfoDetail = "C/O KUEHNE + NAGEL PTE LTD";
			address.OA_Address1 = "10 PIONEER CRESCENT, LEVEL 2";
			address.OA_City = "SINGAPORE";
			address.OA_RN_NKCountryCode = "SG";
			address.OA_PostCode = "628566";
			address.OA_Phone = "912067449215";
			org.CreatePatternMatchingName(Factory);
			Factory.Save();
		}

		#region Implementation

		AsycudaManifestHeader GetManifestHeader(ZString masterBillNumber)
		{
			var billFilter = new ZQuery(AsycudaBillSchema.ABL_BillNumber, masterBillNumber);
			billFilter.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
			var bills = Factory.Load<AsycudaBill>(billFilter);
			AssertEquals(1, bills.Length);
			return bills[0].Header;
		}

		Level1DataImport Level1DataImport
		{
			get
			{
				if (fLevel1DataImport == null)
				{
					fLevel1DataImport = new Level1DataImport(Factory, true);
					fLevel1DataImport.FlightNumber = "QF656";
					fLevel1DataImport.ArrivalDate = new ZDateTime(2017, 6, 13);
					fLevel1DataImport.PortOfLoading = "AUSYD";
					fLevel1DataImport.PortOfDischarge = "SGSIN";
					fLevel1DataImport.CycleDate = new ZDateTime(2017, 6, 15);
					fLevel1DataImport.CycleNumber = fLevel1DataImport.CycleNumbers[4].Code;
					fLevel1DataImport.MasterBill = "08122222222";
					fLevel1DataImport.DepartureDate = new ZDateTime(2017, 6, 14);
					fLevel1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1Sample.txt");
				}
				return fLevel1DataImport;
			}
		}
		Level1DataImport fLevel1DataImport;

		void PopulateRefLocoMap()
		{
			var uSLocoMap = Factory.New<MasterFiles.Business.RefLocoMap>();
			uSLocoMap.RY_LocalPortCode = "2795";
			uSLocoMap.RY_RL_NKLocoPort = "USLAX";
			uSLocoMap.RY_RN = Core.Constants.CountryGuids.Singapore;
			uSLocoMap.RY_SystemUsage = UPEDataLine.Constants.RefLocoSystemUsage;

			var sYDLocoMap = Factory.New<MasterFiles.Business.RefLocoMap>();
			sYDLocoMap.RY_LocalPortCode = "9639";
			sYDLocoMap.RY_RL_NKLocoPort = "SGSIN";
			sYDLocoMap.RY_RN = Core.Constants.CountryGuids.Singapore;
			sYDLocoMap.RY_SystemUsage = UPEDataLine.Constants.RefLocoSystemUsage;
			Factory.Save();
		}

		ZString currentRegNo;
		OrgHeader currentCompany;
		BusinessObjectFactory currentCompanyFactory;
		protected override void SetUp()
		{
			countrySetter = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore);
			ClientHookLoader.Instance.OverrideClientHookForTest(null);

			base.SetUp();
			var accessEnabled = SGCustomsDataRegistry.Instance.ACCESSEnable.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var autoPopulateCycleDetails = UPEDataRegistry.Instance.EnableAutoPopulateCycleDetailsToImportGlobalManifestBillsItem.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			currentCompanyFactory = new BusinessObjectFactory();
			currentRegNo = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;
			currentCompany = currentCompanyFactory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			currentCompany.PrimaryRegistrationNumber.Number = "21 003 980 130 123";

			var helper = new UniversalReferenceTestDataHelper(currentCompanyFactory);

			helper.CreateNewOrGetExistingCusCodeType(Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			var sg = helper.CreateNewOrGetExistingCusCodeList(Universal.RefDataGrouping.Codes.CommonDataGrouping, Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Singapore, "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListTypes.Codes.NVC, "Desc.", Universal.RefCusCodeListTypes.Codes.ManifestCountry, Universal.RefDataGrouping.Codes.CommonDataGrouping, Universal.RefCusCodeListTypes.Codes.ManifestCountry);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg.PK, Universal.RefCusCodeListTypes.Codes.NVC, "17.3.29.1");

			currentCompanyFactory.Save();

			helper.CreateTaxOrFee("DEM", 400m, "SG", new ZDateTime(2010, 1, 1), new ZDateTime(2079, 6, 6, 23, 59, 0), "Low Value Threshold");
			helper.CreateTaxOrFee(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 0.07m, Core.Constants.CountryCodes.Singapore, new ZDateTime(2017, 1, 1), new ZDateTime(2017, 12, 31), "Goods and Services Tax");
			currentCompanyFactory.Save();

			PopulateRefLocoMap();
			NotificationBuffer = new NotificationBuffer();
			Level1DataImport.LoadSummaryInformation = "Summary Information From Load";
			Level1DataImportForLowValue.LoadSummaryInformation = "Summary Information From Load";

			testDataHelper = new UniversalReferenceTestDataHelper(Factory);

			tariffType = testDataHelper.LoadOrCreateTariffType(Core.Constants.CountryCodes.Singapore, Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var comType = testDataHelper.LoadOrCreateTariffType(Core.Constants.CountryCodes.Singapore, Customs.Universal.Constants.TariffTypes.Commodity);
			Factory.Save();

			var tariff1 = testDataHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "85176900", new ZDateTime(2011, 11, 26), new ZDateTime(2018, 6, 23), "OTHER RECEPTION APPARATUS NES (NMB)");
			testDataHelper.LoadOrCreateCreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, UnitOfQuantityCodeList.Codes.NMB);
			var com1 = testDataHelper.CreateCommodity(tariff1, "IDAMISC");
			testDataHelper.CreateTariffAttribute(SGConstants.Attributes.Names.ISIMPORTCONTROL, "Y", com1);

			var tariff2 = testDataHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "48201000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			testDataHelper.LoadOrCreateCreateTariffUOM(tariff2, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, UnitOfQuantityCodeList.Codes.TNE);
			Factory.Save();
		}
		UniversalReferenceTestDataHelper testDataHelper;
		RefCusTariffType tariffType;
		IDisposable countrySetter;
		IDisposable accessEnabled;
		IDisposable autoPopulateCycleDetails;

		protected override void TearDown()
		{
			currentCompany.PrimaryRegistrationNumber.Number = currentRegNo;
			currentCompanyFactory.Save();
			countrySetter?.Dispose();
			countrySetter = null;
			accessEnabled?.Dispose();
			accessEnabled = null;
			autoPopulateCycleDetails?.Dispose();
			autoPopulateCycleDetails = null;
			base.TearDown();
			tempDir?.Dispose();
		}

		Level1DataFileImporterForSGAccess Importer => importer ?? (importer = new Level1DataFileImporterForSGAccess(Level1DataImport, NotificationBuffer));
		Level1DataFileImporterForSGAccess importer;

		TempDirectory TempDir => tempDir ?? (tempDir = new TempDirectory());
		TempDirectory tempDir;

		NotificationBuffer NotificationBuffer;

		Level1DataImport Level1DataImportForManifest
		{
			get
			{
				if (fLevel1DataImportForManifest == null)
				{
					fLevel1DataImportForManifest = new Level1DataImport(Factory, isImportToManifest: true);
					fLevel1DataImportForManifest.FlightNumber = "QF656";
					fLevel1DataImportForManifest.ArrivalDate = ZDateTime.Now;
					fLevel1DataImportForManifest.DepartureDate = ZDateTime.Now.AddDays(3);
					fLevel1DataImportForManifest.PortOfLoading = "SGSIN";
					fLevel1DataImportForManifest.PortOfDischarge = "AUSYD";
					fLevel1DataImportForManifest.MasterBill = "08122222222";
					fLevel1DataImportForManifest.FlightNotInScheduleNote = "FlightNotInScheduleNote Notes";
					fLevel1DataImportForManifest.ArrivalDateWarningNote = "ArrivalDateWarning Note";
					fLevel1DataImportForManifest.UnmatchedFilenameNote = "UnmatchedFilename Note";
				}
				return fLevel1DataImportForManifest;
			}
		}
		Level1DataImport fLevel1DataImportForManifest;

		Level1DataFileImporterForSGAccess importerForSGAccess;
		Level1DataFileImporterForSGAccess ImporterForSGAccess => importerForSGAccess ?? (importerForSGAccess = NewLevel1DataFileImporterForSGAccess(Level1DataImportForManifest, NotificationBuffer));

		Level1DataImport Level1DataImportForLowValue
		{
			get
			{
				if (fLevel1DataImportForLowValue == null)
				{
					fLevel1DataImportForLowValue = new Level1DataImport(Factory, true);
					fLevel1DataImportForLowValue.FlightNumber = "QF656";
					fLevel1DataImportForLowValue.ArrivalDate = new ZDateTime(2017, 6, 13);
					fLevel1DataImportForLowValue.PortOfLoading = "AUSYD";
					fLevel1DataImportForLowValue.PortOfDischarge = "SGSIN";
					fLevel1DataImportForLowValue.CycleDate = new ZDateTime(2017, 6, 15);
					fLevel1DataImportForLowValue.CycleNumber = fLevel1DataImportForLowValue.CycleNumbers[4].Code;
					fLevel1DataImportForLowValue.MasterBill = "08122222222";
					fLevel1DataImportForLowValue.DepartureDate = new ZDateTime(2017, 6, 14);
					fLevel1DataImportForLowValue.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SGAccessLevel1SampleForLowValue.txt");
				}
				return fLevel1DataImportForLowValue;
			}
		}
		Level1DataImport fLevel1DataImportForLowValue;

		Level1DataFileImporterForSGAccess importerForSGAccessForLowValue;
		Level1DataFileImporterForSGAccess ImporterForSGAccessForLowValue => importerForSGAccessForLowValue ?? (importerForSGAccessForLowValue = new Level1DataFileImporterForSGAccess(Level1DataImportForLowValue, NotificationBuffer));

		Level1DataFileImporterForSGAccess NewLevel1DataFileImporterForSGAccess(Level1DataImport level1DataImportForManifest, NotificationBuffer notificationBuffer) => new Level1DataFileImporterForSGAccess(level1DataImportForManifest, notificationBuffer);

		OrgHeader CreateOrg(ZString companyName, ZString code, ZString accountNumber)
		{
			var org = CreateOrg(companyName, code, string.Concat(companyName, " STR 1"), string.Concat(companyName, " STR 2"), string.Concat(companyName, " CITY"), string.Concat(companyName, " STATE"), string.Concat(accountNumber.Left(6), "POST"), ZString.Empty);
			org.CustomsCodes.AddNew(UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber, accountNumber, Core.Constants.CountryCodes.Singapore);
			org.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.PartyStatusType, SGPartyStatusList.Codes.Y, Core.Constants.CountryCodes.Singapore);
			return org;
		}

		OrgHeader CreateOrg(ZString companyName, ZString code, ZString address1, ZString address2, ZString city, ZString state, ZString postcode, ZString phone)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = companyName;
			org.OH_Code = code;
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = address1;
			org.MainAddress.OA_Address2 = address2;
			org.MainAddress.OA_City = city;
			org.MainAddress.OA_State = state;
			org.MainAddress.OA_PostCode = postcode;
			org.MainAddress.OA_Phone = phone;
			org.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			return org;
		}

		void CreateLocoMap(ZString code, ZString unloco)
		{
			var locoMap = Factory.New<MasterFiles.Business.RefLocoMap>();
			locoMap.RY_LocalPortCode = code;
			locoMap.RY_RL_NKLocoPort = unloco;
			locoMap.RY_RN = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Singapore).PK;
			locoMap.RY_SystemUsage = UPEDataLine.Constants.RefLocoSystemUsage;
		}

		TariffView CreateTariff(ZString tariffCode, ZString commodityType, ZDecimal percentRate, ZDecimal perUnitRate, ZString unitOfQuantity, ZBool isImportControl, ZDateTime? tariffStartDate = null, ZDateTime? tariffEndDate = null)
		{
			var tariff = testDataHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, tariffCode, tariffStartDate ?? ZDateTime.Today.AddYears(-1), tariffEndDate ?? ZDateTime.Today.AddYears(1), tariffCode + " DESC");

			if (commodityType != ZString.Empty)
			{
				testDataHelper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, commodityType, tariff);
			}

			if (unitOfQuantity != ZString.Empty)
			{
				testDataHelper.CreateTariffUOM(tariff, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, unitOfQuantity);
			}

			if (percentRate > 0)
			{
				testDataHelper.CreateDutyRate(tariff, null, percentRate);
			}

			if (perUnitRate > 0 && unitOfQuantity != ZString.Empty)
			{
				testDataHelper.CreateDutyRate(tariff, null, perUnitRate, unitOfQuantity);
			}

			testDataHelper.CreateCommodity(tariff, "COM" + tariffCode);
			testDataHelper.CreateTariffAttribute(SGConstants.Attributes.Names.ISIMPORTCONTROL, isImportControl ? "Y" : "N", tariff);
			return tariff;
		}

		OrgSupplierPart CreatePart(ZString partNo, OrgHeader owner, ZString tariff)
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = partNo;
			part.OP_Desc = partNo + " DESC";
			part.RelatedOrganisations.AddOwner(owner);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = tariff;
			return part;
		}

		void AssertBill(AsycudaBill bill, ZString billNumber, ZDecimal customsValue, ZString origin, ZString finalDestination, ZDecimal grossWeight, ZString grossWeightUQ, ZString goodsDescription, ZDecimal volume, ZString volumeUQ, ZInt manifestQty, ZString manifestUQ,
			ZGuid consigneePK, ZString consigneeName, ZString consigneeStreet1, ZString consigneeStreet2, ZString consigneeCity, ZString consigneeState, ZString consigneePostCode, ZString consigneePhone, ZString consigneeCountry,
			ZGuid shipperPK, ZString shipperName, ZString shipperStreet1, ZString shipperStreet2, ZString shipperCity, ZString shipperState, ZString shipperPostCode, ZString shipperCountry,
			ZString customsValueCurrency, ZDecimal freightValue, ZString freightValueCurrency, ZDecimal transportValue, ZString transportValueCurrency, ZDecimal insuranceValue, ZString insuranceValueCurrency, ZDecimal otherChargesValue, ZString otherChargesValueCurrency, ZDecimal discountValue, ZString discountValueCurrency,
			ZDecimal dutyAmount, ZDecimal taxAmount, ZDateTime cycleDate, ZString cycleNumber)
		{
			CombineAssertions(() =>
			{
				AssertEquals(billNumber + " bill.ABL_BillNumber", billNumber, bill.ABL_BillNumber);
				AssertEquals(billNumber + " bill.ABL_CustomsValue", customsValue, bill.ABL_CustomsValue);
				AssertEquals(billNumber + " bill.ABL_RX_NKCustomsValueCurrency", customsValueCurrency, bill.ABL_RX_NKCustomsValueCurrency);
				AssertEquals(billNumber + " bill.ABL_RL_NKOrigin", origin, bill.ABL_RL_NKOrigin);
				AssertEquals(billNumber + " bill.ABL_RL_NKFinalDestination", finalDestination, bill.ABL_RL_NKFinalDestination);
				AssertEquals(billNumber + " bill.ABL_GrossWeight", grossWeight, bill.ABL_GrossWeight);
				AssertEquals(billNumber + " bill.ABL_GrossWeightUQ", grossWeightUQ, bill.ABL_GrossWeightUQ);
				AssertEquals(billNumber + " bill.ABL_GoodsDescription", goodsDescription, bill.ABL_GoodsDescription);
				AssertEquals(billNumber + " bill.ABL_Volume", volume, bill.ABL_Volume);
				AssertEquals(billNumber + " bill.ABL_VolumeUQ", volumeUQ, bill.ABL_VolumeUQ);
				AssertEquals(billNumber + " bill.ABL_BolType", Core.Constants.ShipmentTypes.StandardHouse, bill.ABL_BolType);
				AssertEquals(billNumber + " bill.ABL_ManifestQty", manifestQty, bill.ABL_ManifestQty);
				AssertEquals(billNumber + " bill.ABL_ManifestUQ", manifestUQ, bill.ABL_ManifestUQ);
				AssertEquals(billNumber + " bill.ABL_FreightValue", freightValue, bill.ABL_FreightValue);
				AssertEquals(billNumber + " bill.ABL_RX_NKFreightValueCurrency", freightValueCurrency, bill.ABL_RX_NKFreightValueCurrency);
				AssertEquals(billNumber + " bill.ABL_TransportValue", transportValue, bill.ABL_TransportValue);
				AssertEquals(billNumber + " bill.ABL_RX_NKTransportValueCurrency", transportValueCurrency, bill.ABL_RX_NKTransportValueCurrency);
				AssertEquals(billNumber + " bill.ABL_InsuranceValue", insuranceValue, bill.ABL_InsuranceValue);
				AssertEquals(billNumber + " bill.ABL_RX_NKInsuranceValueCurrency", insuranceValueCurrency, bill.ABL_RX_NKInsuranceValueCurrency);
				AssertEquals(billNumber + " bill.OtherChargesValue", otherChargesValue, bill.OtherChargesValue);
				AssertEquals(billNumber + " bill.OtherChargesValueCurrency", otherChargesValueCurrency, bill.OtherChargesValueCurrency);
				AssertEquals(billNumber + " bill.DiscountValue", discountValue, bill.DiscountValue);
				AssertEquals(billNumber + " bill.DiscountValueCurrency", discountValueCurrency, bill.DiscountValueCurrency);

				AssertConsignee(bill, billNumber, consigneePK, consigneeName, consigneeStreet1, consigneeStreet2, consigneeCity, consigneeState, consigneePostCode, consigneePhone, consigneeCountry);

				AssertEquals(billNumber + " bill.ABL_OA_Shipper", shipperPK, bill.ABL_OA_Shipper);
				AssertEquals(billNumber + " bill.ABL_ShipperName", shipperName, bill.ABL_ShipperName);
				AssertEquals(billNumber + " bill.ABL_ShipperStreet1", shipperStreet1, bill.ABL_ShipperStreet1);
				AssertEquals(billNumber + " bill.ABL_ShipperStreet2", shipperStreet2, bill.ABL_ShipperStreet2);
				AssertEquals(billNumber + " bill.ABL_ShipperCity", shipperCity, bill.ABL_ShipperCity);
				AssertEquals(billNumber + " bill.ABL_ShipperState", shipperState, bill.ABL_ShipperState);
				AssertEquals(billNumber + " bill.ABL_ShipperPostcode", shipperPostCode, bill.ABL_ShipperPostcode);
				AssertEquals(billNumber + " bill.ABL_RN_NKShipperCountry", shipperCountry, bill.ABL_RN_NKShipperCountry);

				AssertEquals(billNumber + " bill.CountryCode", Core.Constants.CountryCodes.Singapore, bill.CountryCode);
				AssertEquals(billNumber + " bill.ABL_ShipmentType", Level1DataFileImporterForSGAccess.Constants.Nature.Import, bill.ABL_ShipmentType);
				AssertEquals(billNumber + " bill.DutyAmount", dutyAmount, bill.DutyAmount);
				AssertEquals(billNumber + " bill.TaxAmount", taxAmount, bill.TaxAmount);
				AssertCycleFields(bill, cycleDate, cycleNumber);
			});
		}

		void AssertConsignee(AsycudaBill bill, ZString billNumber, ZGuid consigneePK, ZString consigneeName, ZString consigneeStreet1, ZString consigneeStreet2, ZString consigneeCity, ZString consigneeState, ZString consigneePostCode, ZString consigneePhone, ZString consigneeCountry)
		{
			AssertEquals(billNumber + " bill.ABL_OA_Consignee", consigneePK, bill.ABL_OA_Consignee);
			AssertEquals(billNumber + " bill.ABL_ConsigneeName", consigneeName, bill.ABL_ConsigneeName);
			AssertEquals(billNumber + " bill.ABL_ConsigneeStreet1", consigneeStreet1, bill.ABL_ConsigneeStreet1);
			AssertEquals(billNumber + " bill.ABL_ConsigneeStreet2", consigneeStreet2, bill.ABL_ConsigneeStreet2);
			AssertEquals(billNumber + " bill.ABL_ConsigneeCity", consigneeCity, bill.ABL_ConsigneeCity);
			AssertEquals(billNumber + " bill.ABL_ConsigneeState", consigneeState, bill.ABL_ConsigneeState);
			AssertEquals(billNumber + " bill.ABL_ConsigneePostcode", consigneePostCode, bill.ABL_ConsigneePostcode);
			AssertEquals(billNumber + " bill.ABL_ConsigneePhone", consigneePhone, bill.ABL_ConsigneePhone);
			AssertEquals(billNumber + " bill.ABL_RN_NKConsigneeCountry", consigneeCountry, bill.ABL_RN_NKConsigneeCountry);
		}

		void AssertPack(AsycudaPack pack, ZString goodsDescription, ZString marksAndNumbers, ZInt packQty, ZString packUQ, ZDecimal weight, ZString weightUQ, ZDecimal volume, ZString volumeUQ,
			ZString goodsOrigin, ZString tariff, ZDecimal customsValue, ZDecimal dutyAmount, ZDecimal taxAmount, ZString goodsType, ZDecimal customsQty, ZString customsUQ, ZDecimal linePrice, ZString linePriceCurrency, ZString matchingReference)
		{
			CombineAssertions(() =>
			{
				var consignmentReference = pack.ConsignmentReference;
				AssertEquals(consignmentReference + " pack.APA_CommodityCode", ZString.Empty, pack.APA_CommodityCode);
				AssertEquals(consignmentReference + " pack.APA_GoodsDescription", goodsDescription, pack.APA_GoodsDescription);
				AssertEquals(consignmentReference + " pack.APA_MarksAndNumbers", marksAndNumbers, pack.APA_MarksAndNumbers);
				AssertEquals(consignmentReference + " pack.APA_PackQty", packQty, pack.APA_PackQty);
				AssertEquals(consignmentReference + " pack.APA_PackUQ", packUQ, pack.APA_PackUQ);
				AssertEquals(consignmentReference + " pack.APA_Weight", weight, pack.APA_Weight);
				AssertEquals(consignmentReference + " pack.APA_WeightUQ", weightUQ, pack.APA_WeightUQ);
				AssertEquals(consignmentReference + " pack.APA_Volume", volume, pack.APA_Volume);
				AssertEquals(consignmentReference + " pack.APA_VolumeUQ", volumeUQ, pack.APA_VolumeUQ);
				AssertEquals(consignmentReference + " pack.LinePrice", linePrice, pack.LinePrice);
				AssertEquals(consignmentReference + " pack.LinePriceCurrency", linePriceCurrency, pack.LinePriceCurrency);
				AssertEquals(consignmentReference + " pack.MatchingReference", matchingReference, pack.MatchingReference);

				var packedItem = pack.PackedItem;
				AssertEquals(consignmentReference + " packedItem.API_RN_NKGoodsOrigin", goodsOrigin, packedItem.API_RN_NKGoodsOrigin);
				AssertEquals(consignmentReference + " packedItem.API_Tariff", tariff, packedItem.API_Tariff);
				AssertEquals(consignmentReference + " packedItem.API_CustomsValue", customsValue, packedItem.API_CustomsValue);
				AssertEquals(consignmentReference + " packedItem.API_DutyAmount", dutyAmount, packedItem.API_DutyAmount);
				AssertEquals(consignmentReference + " packedItem.API_TaxAmount", taxAmount, packedItem.API_TaxAmount);
				AssertEquals(consignmentReference + " packedItem.GoodsType", goodsType, packedItem.GoodsType);
				AssertEquals(consignmentReference + " packedItem.API_GoodsDescription", goodsDescription, packedItem.API_GoodsDescription);
				AssertEquals(consignmentReference + " packedItem.API_CustomsQty", customsQty, packedItem.API_CustomsQty);
				AssertEquals(consignmentReference + " packedItem.API_CustomsUQ", customsUQ, packedItem.API_CustomsUQ);
			});
		}

		void AssertHeaderAndHeaderCountry(AsycudaManifestHeader header, ZString jobReference, ZString billNumber, ZString transportMode, ZString flightNo, ZString portOfLoading, ZString portOfDischarge,
			ZString manifestType, ZString portOfFirstArrival, ZString nature)
		{
			CombineAssertions(() =>
			{
				var headerReference = header.AMA_JobReference;
				AssertEquals(string.Concat(headerReference, " header.AMA_JobReference"), jobReference, headerReference);
				AssertEquals(string.Concat(headerReference, " header.MasterBill.ABL_BillNumber"), billNumber, header.MasterBill.ABL_BillNumber);
				AssertEquals(string.Concat(headerReference, " header.AMA_TransportMode"), transportMode, header.AMA_TransportMode);
				AssertEquals(string.Concat(headerReference, " header.AMA_Voyage"), flightNo, header.AMA_Voyage);
				AssertEquals(string.Concat(headerReference, " header.AMA_RL_NKPortOfLoading"), portOfLoading, header.AMA_RL_NKPortOfLoading);
				AssertEquals(string.Concat(headerReference, " header.AMA_RL_NKPortOfDischarge"), portOfDischarge, header.AMA_RL_NKPortOfDischarge);
				AssertEquals(string.Concat(headerReference, " header.AMA_ManifestType"), manifestType, header.AMA_ManifestType);
				AssertEquals(string.Concat(headerReference, " header.AMA_RL_NKPortOfFirstArrival"), portOfFirstArrival, header.AMA_RL_NKPortOfFirstArrival);
				AssertEquals(string.Concat(headerReference, " header.AMA_Nature"), nature, header.AMA_Nature);
			});
		}

		void AssertBillsCreated(ZString manifestNumber, ZInt billCount, ZString billNumbers)
		{
			var manifestHeader = GetManifestHeader(manifestNumber);
			CombineAssertions(() =>
			{
				AssertEquals(string.Format("Expected bill count did not match for Manifest Number {0}", manifestNumber), billCount, manifestHeader.Bills.Count);
				foreach (AsycudaBill bill in manifestHeader.Bills)
				{
					var billNumber = bill.ABL_BillNumber;
					Assert(string.Format("Bill Number {0} was not in list {1}", billNumber, billNumbers), billNumbers.Contains(billNumber));
				}
			});
		}

		void AssertDeclarationsCreated(ZString masterBillNumber, ZInt decCount, ZString houseBillNumbers)
		{
			var declarations = Factory.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MasterBill, masterBillNumber));
			CombineAssertions(() =>
			{
				AssertEquals(string.Format("Expected declaration count did not match for MasterBill Number {0}", masterBillNumber), decCount, declarations.Length);
				foreach (var declaration in declarations)
				{
					var houseBillNumber = declaration.JE_HouseBill;
					Assert(string.Format("HouseBill Number {0} was not in list {1}", houseBillNumber, houseBillNumbers), houseBillNumbers.Contains(houseBillNumber));
				}
			});
		}

		AsycudaManifestHeader AssertBillsOnNewManifest(ZInt billsOnNewManifestCount)
		{
			var manifestQuery = new ZQuery(AsycudaManifestHeaderSchema.AMA_JobReference, SQLComparisonOperator.NotEqual, ZString.Empty);
			var manifests = Factory.Load<AsycudaManifestHeader>(manifestQuery);
			AssertEquals(2, manifests.Length);
			var newManifest = manifests.FirstOrDefault(x => x.AMA_JobReference != "MAN0081");
			AssertEquals(billsOnNewManifestCount, newManifest.Bills.Count);
			return newManifest;
		}

		(AsycudaManifestHeader Header, AsycudaBill Bill) GetTestManifestAndChildren(ZString jobReference)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";

			header.AMA_JobReference = jobReference;
			header.MasterBill.ABL_BillNumber = "08122222222";
			header.AMA_Voyage = "QF81";
			header.AMA_RL_NKPortOfLoading = "USLAX";
			header.AMA_RL_NKPortOfDischarge = "SGSIN";

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "3824ARFY9JH";
			bill.ABL_RL_NKOrigin = "USLAX";
			bill.ABL_RL_NKFinalDestination = "SGKEL";
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;

			var pack = bill.Packs.AddNew();
			pack.MatchingReference = "PART2";
			pack.APA_GoodsDescription = "SOME PACKS";
			pack.APA_PackQty = 5;

			var packedItem = pack.PackedItem;
			packedItem.API_GoodsDescription = "SOME PACKS";

			return (header, bill);
		}

		void SetupMajorExporterConsignee()
		{
			var majorExporterConsignee = CreateOrg("LIYANA AZIZ", "LIYANA", "#04-01B DELTA HOUSE", "2 ALEXANDRA ROAD 159919", "SINGAPORE", ZString.Empty, "8869", "6590085135");
			majorExporterConsignee.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.PartyStatusType, SGPartyStatusList.Codes.Y, Core.Constants.CountryCodes.Singapore);
			majorExporterConsignee.CreatePatternMatchingAddressFromMainAddress(Factory);
			majorExporterConsignee.CreatePatternMatchingName(Factory);
			Factory.Save();
		}

		void SetupPayeeAndPartyIDOrganisations()
		{
			var consignor = CreateOrg("BIOTECH CORP", "BIOCORP", "107 OAKWOOD DRIVE", "SHIPPER STREET 3", "GLASTONBURY", "CT", "060332481", "18606338111");
			consignor.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "12345678X", Core.Constants.CountryCodes.Singapore);
			consignor.MiscServ.OM_IMPaymentMethod = SGPayeeIndicatorList.Codes.Q;
			consignor.CreatePatternMatchingAddressFromMainAddress(Factory);
			consignor.CreatePatternMatchingName(Factory);
			var consignee = CreateOrg("SNOWSILL, SONYA", "SNOWSON", "15 LEANDER ST", "CONSIGNEE STR 2", "FALCON", ZString.Empty, "6201", "+61895343637");
			consignee.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "87654321Y", Core.Constants.CountryCodes.Singapore);
			consignee.MiscServ.OM_IMPaymentMethod = SGPayeeIndicatorList.Codes.T;
			consignee.CreatePatternMatchingAddressFromMainAddress(Factory);
			consignee.CreatePatternMatchingName(Factory);
			Factory.Save();
		}

		AsycudaBill GetBill(AsycudaManifestHeader manifestHeader, ZString billNumber) => manifestHeader.Bills.Cast<AsycudaBill>().FirstOrDefault(x => x.ABL_BillNumber == billNumber);

		void CreateRefPack(ZString commercialUnitOfQuantity, ZString customsUnitOfQuantity, ZDecimal conversionFactor)
		{
			var refPack = Factory.New<CusRefPacks>();
			refPack.RP_CommercialPack = commercialUnitOfQuantity;
			refPack.RP_CustomsPack = customsUnitOfQuantity;
			refPack.RP_CustomsCountry = Core.Constants.CountryCodes.Singapore;
			refPack.RP_ConversionFactor = conversionFactor;
			Factory.Save();
		}

		#endregion
	}
}
