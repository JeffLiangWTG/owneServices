using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.Client.UPE.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	sealed class BBISIFileExporterTest : TestCaseWithFactory
	{
		public void TestSaveDateUploadedAndBISIUploadData()
		{
			var hAWB1 = CreateCompletedShipmentAndBrokerageJobs("HAWB1");
			var hAWB2 = CreateCompletedShipmentAndBrokerageJobs("HAWB2");
			Factory.Save();
			var tempFile = Env.GetTempFileName();
			try
			{
				var exportInformation = new ExportInformation(UPEDataRegistry.Instance.BISIUploadCurrentBatchNumber, ZDateTime.Now.AddYears(-1), ZDateTime.Now);
				exportInformation.RunEveryDayExport = true;
				exportInformation.EveryDayStartDate = ZDateTime.Now.AddYears(-1);
				exportInformation.EveryDayEndDate = ZDateTime.Now;
				Exporter.ExportToFile(tempFile, exportInformation);
				Exporter.SaveDateUploadedAndBISIUploadData();
				AssertEquals("Should set the date uploaded to BISI", false, hAWB1.BisiUploadDate.IsEmpty);
				AssertEquals("Should set the date uploaded to BISI", false, hAWB2.BisiUploadDate.IsEmpty);
				AssertUploadedDataSavedInBISIShipmentTable("Expected HAWB to be saved", hAWB1, true);
				AssertUploadedDataSavedInBISIShipmentTable("Expected HAWB to be saved", hAWB2, true);
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		public void TestSaveDateUploadedAndBISIUploadData_DontSaveUploadDataIfShouldntUpload()
		{
			var hAWB1 = CreateCompletedShipmentAndBrokerageJobs("HAWB1");
			var hAWB2 = CreateCompletedShipmentAndBrokerageJobs("HAWB2");
			hAWB1.Declaration.QuarantineFee = 0;
			hAWB2.Declaration.QuarantineFee = 0;
			AssertEquals("No local charges, no upload", true, hAWB1.TotalLocalCharges.IsEmpty);
			AssertEquals("No local charges, no upload", true, hAWB2.TotalLocalCharges.IsEmpty);
			Factory.Save();
			var tempFile = Env.GetTempFileName();
			try
			{
				var exportInformation = new ExportInformation(1, ZDateTime.Now.AddYears(-1), ZDateTime.Now);
				Exporter.ExportToFile(tempFile, exportInformation);
				Exporter.SaveDateUploadedAndBISIUploadData();
				AssertEquals("Should set the date uploaded to BISI", false, hAWB1.BisiUploadDate.IsEmpty);
				AssertEquals("Should set the date uploaded to BISI", false, hAWB2.BisiUploadDate.IsEmpty);
				var notSaved = false;
				AssertUploadedDataSavedInBISIShipmentTable("Shouldn't be saved because there are no local charges", hAWB1, notSaved);
				AssertUploadedDataSavedInBISIShipmentTable("Shouldn't be saved because there are no local charges", hAWB2, notSaved);
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		public void TestSaveDateUploadedAndBISIUploadData_OnlyCreate1RecordForSplitShipment()
		{
			var hAWB1 = CreateCompletedShipmentAndBrokerageJobs("SplitHAWB");
			var hAWB2 = CreateCompletedShipmentAndBrokerageJobs("SplitHAWB");
			Factory.Save();
			var tempFile = Env.GetTempFileName();
			try
			{
				var exportInformation = new ExportInformation(1, ZDateTime.Now.AddYears(-1), ZDateTime.Now);
				exportInformation.RunEveryDayExport = true;
				exportInformation.EveryDayStartDate = ZDateTime.Now.AddYears(-1);
				exportInformation.EveryDayEndDate = ZDateTime.Now;
				Exporter.ExportToFile(tempFile, exportInformation);
				Exporter.SaveDateUploadedAndBISIUploadData();
				AssertEquals("Should set the date uploaded to BISI on both shipment splits, even though only 1 is uploaded", false, hAWB1.BisiUploadDate.IsEmpty);
				AssertEquals("Should set the date uploaded to BISI on both shipment splits, even though only 1 is uploaded", false, hAWB2.BisiUploadDate.IsEmpty);
				var isHAWB1PostedToTable = IsUploadedDataSavedInBISIShipmentTable(hAWB1);
				var isHAWB2PostedToTable = IsUploadedDataSavedInBISIShipmentTable(hAWB2);
				Assert("1 of the split shipments should be posted, but not both", isHAWB1PostedToTable || isHAWB2PostedToTable);
				Assert("1 of the split shipments should be posted, but not both", !isHAWB1PostedToTable || !isHAWB2PostedToTable);
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		public void TestSaveDateUploadedAndBISIUploadData_ReportDeveloperErrorAndContinueIfSaveFailed()
		{
			var hAWB1 = CreateCompletedShipmentAndBrokerageJobs("HAWB1");
			var hAWB2 = CreateCompletedShipmentAndBrokerageJobs("HAWB2");
			Factory.Save();
			var tempFile = Env.GetTempFileName();
			try
			{
				Db.Connection.ExecuteNonQuery("drop table " + UPEClientTables.ClientBISIShipmentCharge.TableName);
				var exportInformation = new ExportInformation(1, ZDateTime.Now.AddYears(-1), ZDateTime.Now);
				exportInformation.RunEveryDayExport = true;
				exportInformation.EveryDayStartDate = ZDateTime.Now.AddYears(-1);
				exportInformation.EveryDayEndDate = ZDateTime.Now;
				Exporter.ExportToFile(tempFile, exportInformation);
				Exporter.SaveDateUploadedAndBISIUploadData();
				AssertEquals("Should set the date uploaded to BISI", false, hAWB1.BisiUploadDate.IsEmpty);
				AssertEquals("Should set the date uploaded to BISI", false, hAWB2.BisiUploadDate.IsEmpty);
				ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		public void TestConstructor()
		{
			AssertEquals(Notifications, Exporter.Notifications);
		}

		public void TestFactoryRefreshDisabledForPerformance()
		{
			var exporter = new TestBISIFileExporterWithPropertiesExposed(Notifications);
			AssertEquals("RefreshEnabled should be false because it causes big performance problems", false, exporter.Factory.RefreshEnabled);
		}

		public void TestExportToFile_NewFactoryEachRun()
		{
			var exporter = new TestBISIFileExporterWithPropertiesExposed(Notifications);
			var exportInformation = new ExportInformation(100, ZDateTime.Now, ZDateTime.Now);
			exportInformation.RunEveryDayExport = true;
			exportInformation.EveryDayStartDate = ZDateTime.Today;
			exportInformation.EveryDayEndDate = ZDateTime.Now;
			exporter.ExportToFile("file1", exportInformation);
			var factory1 = exporter.Factory;
			UPEDataRegistry.Instance.BISIUploadCurrentBatchNumber = 101;
			exporter.ExportToFile("file2", exportInformation);
			var factory2 = exporter.Factory;
			Assert("Should be using different factories.", factory1 != factory2);
		}

		public void TestExportToFile_NewUploadedShipmentListEachRun()
		{
			var exporter = new TestBISIFileExporterWithPropertiesExposed(Notifications);
			var exportInformation = new ExportInformation(100, ZDateTime.Now, ZDateTime.Now);
			exportInformation.RunEveryDayExport = true;
			exportInformation.EveryDayStartDate = ZDateTime.Today;
			exportInformation.EveryDayEndDate = ZDateTime.Now;
			exporter.ExportToFile("file1", exportInformation);
			var uploadedShipments1 = exporter.LastUploadedCompletedShipmentsInternal;
			exportInformation.BatchNumber = 101;
			exporter.ExportToFile("file2", exportInformation);
			var uploadedShipments2 = exporter.LastUploadedCompletedShipmentsInternal;
			Assert("Should be using different array list.", uploadedShipments1 != uploadedShipments2);
		}

		public void TestResetFactoryAfterSaving()
		{
			var exporter = new TestBISIFileExporterWithPropertiesExposed(Notifications);
			var factory1 = exporter.Factory;
			exporter.SaveDateUploadedAndBISIUploadData();
			Assert("Should be using different factories", factory1 != exporter.Factory);
		}

		public void TestResetUploadedShipmentListAfterSaving()
		{
			var exporter = new TestBISIFileExporterWithPropertiesExposed(Notifications);
			var uploadedShipments = exporter.LastUploadedCompletedShipmentsInternal;
			var shipmentData = new ShipmentDataForTest();
			shipmentData.ChargesData = Array.Empty<ShipmentChargeData>();
			exporter.LastUploadedCompletedShipmentsInternal.Add(shipmentData);
			exporter.LastUploadedCompletedShipmentsInternal.Add(shipmentData);
			exporter.LastUploadedCompletedShipmentsInternal.Add(shipmentData);
			AssertEquals("Before calling save", 3, exporter.LastUploadedCompletedShipments.Count);
			exporter.SaveDateUploadedAndBISIUploadData();
			AssertEquals("Should not have any elements", 0, exporter.LastUploadedCompletedShipments.Count);
			Assert("Should be using a new object", uploadedShipments != exporter.LastUploadedCompletedShipmentsInternal);
		}

		[TestDate(2005, 10, 09, 10, 37, 43)]
		public void TestExportToFile_NoData()
		{
			var tempFileName = Path.Combine(Env.TempPath, ZGuid.NewZGuid().ToString());
			var exportInformation = new ExportInformation(200, ZDateTime.Now, ZDateTime.Now);
			exportInformation.RunEveryDayExport = true;
			exportInformation.EveryDayStartDate = ZDateTime.Now.AddDays(-1);
			exportInformation.EveryDayEndDate = ZDateTime.Now;
			AssertEquals(BISIExportResult.ExportSuccessButNoData, Exporter.ExportToFile(tempFileName, exportInformation));
			AssertEquals("No data to be exported, file should not exist", false, File.Exists(tempFileName));
			AssertEquals(7, Notifications.Events.Length);
			AssertMultilineASCIIEquals("Notifications for BISI Upload", @"
Start Adding Completed Shipment To Records
Finish Adding Completed Shipment To Records [Number of Lines: 0]
Start Adding Shipment Status To Records
Start Getting Every Day Statuses [Start Date: 08-Oct-05 10:37:43, End Date: 09-Oct-05 10:37:43]
Finish Getting 0 Candidate Every Day Statuses
Finish Adding Shipment Status To Records [Number of Lines: 0]
Warning: No shipments data to be exported".Trim(), Notifications.AsString);
			AssertEquals("No shipments data to be exported", ((WarningNotification)Notifications.LastOne).AdditionalInfo);
		}

		public void TestExportToFile_IOErrorOccurred()
		{
			var mock = new Mock<BISIFileExporter>(new object[] { Notifications });
			mock.CallBase = true;
			mock.Protected()
				.Setup("DoExport", ItExpr.IsAny<StreamWriter>(), ItExpr.IsAny<int>(), ItExpr.IsAny<BISIUploadRecordList>())
				.Throws(new IOException("Blablabla"));
			var recordListMock = new Mock<BISIUploadRecordList>();
			recordListMock.Setup(m => m.IsEmpty).Returns(false);
			var list = recordListMock.Object;
			mock.Setup(m => m.GetFileContent(It.IsAny<ExportInformation>())).Returns(list);
			var exportInformation = new ExportInformation(100, ZDateTime.Now, ZDateTime.Now);
			exportInformation.RunEveryDayExport = true;
			exportInformation.EveryDayStartDate = ZDateTime.Now;
			exportInformation.EveryDayEndDate = ZDateTime.Now;
			using (var file = TempFile.New())
			{
				AssertEquals("Should return false when there is error", BISIExportResult.ExportFails, mock.Object.ExportToFile(file.Filename, exportInformation));
				AssertEquals(1, Notifications.Events.Length);
				AssertEquals("Blablabla", ((ErrorNotification)Notifications.LastOne).AdditionalInfo);
				AssertEquals(ErrorType.IOError, ((ErrorNotification)Notifications.LastOne).ErrorType);
			}
			mock.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2005, 10, 09)]
		public void TestExportPreReleaseDeclarationToFile_PreReleaseFlagUnticked()
		{
			SetupPreReleaseFlag();
			SetupPreReleaseNotificationRegistries();
			PrepareDataForPreReleaseShipment(false);
			var testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			var exportInformation = new ExportInformation(710, ZDateTime.Now.AddDays(-1), ZDateTime.Now);
			exportInformation.RunEveryDayExport = true;
			exportInformation.EveryDayStartDate = ZDateTime.Now.AddDays(-1);
			exportInformation.EveryDayEndDate = ZDateTime.Now;
			var exportResult = Exporter.ExportToFile(testFilePath, exportInformation);
			AssertEquals("Export should be successful", BISIExportResult.ExportSuccess, exportResult);
			AssertBISIFileContent("Should have the same content.", UPETestHelper.TestFiles.BISI.Export.Folder + "BISIExportFile_PreReleaseWithNoContactFee.txt", testFilePath);
			var logsCreated = Factory.Load<ClientXPLDUploadLog>(new ZQuery());
			AssertEquals("there should be logs created for each of the XPLD", 4, logsCreated.Length);
			AssertMultilineASCIIEquals("Notifications for BISI Upload", @"
Start Adding Completed Shipment To Records
Finish Adding Completed Shipment To Records [Number of Lines: 10]
Start Adding Shipment Status To Records
Start Getting Every Day Statuses [Start Date: 08-Oct-05 13:37:43, End Date: 09-Oct-05 13:37:43]
Finish Getting 13 Candidate Every Day Statuses
Finish Adding Shipment Status To Records [Number of Lines: 5]".Trim(), Notifications.AsString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2005, 10, 09)]
		public void TestExportPreReleaseDeclarationToFile_PreReleaseFlagTicked()
		{
			SetupPreReleaseFlag();
			SetupPreReleaseNotificationRegistries();
			PrepareDataForPreReleaseShipment();
			var testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			var exportInformation = new ExportInformation(710, ZDateTime.Now.AddDays(-1), ZDateTime.Now);
			exportInformation.RunEveryDayExport = true;
			exportInformation.EveryDayStartDate = ZDateTime.Now.AddDays(-1);
			exportInformation.EveryDayEndDate = ZDateTime.Now;
			var exportResult = Exporter.ExportToFile(testFilePath, exportInformation);
			AssertEquals("Export should be successful", BISIExportResult.ExportSuccess, exportResult);
			AssertBISIFileContent("Should have the same content.", UPETestHelper.TestFiles.BISI.Export.Folder + "BISIExportFile_PreRelease.txt", testFilePath);
			var logsCreated = Factory.Load<ClientXPLDUploadLog>(new ZQuery());
			AssertEquals("there should be logs created for each of the XPLD", 4, logsCreated.Length);
			AssertMultilineASCIIEquals("Notifications for BISI Upload", @"
Start Adding Completed Shipment To Records
Finish Adding Completed Shipment To Records [Number of Lines: 12]
Start Adding Shipment Status To Records
Start Getting Every Day Statuses [Start Date: 08-Oct-05 13:37:43, End Date: 09-Oct-05 13:37:43]
Finish Getting 13 Candidate Every Day Statuses
Finish Adding Shipment Status To Records [Number of Lines: 5]".Trim(), Notifications.AsString);
			Notifications.Clear();
			var preloadedXPLD = "E7F747HML3YAU2000002010-02-17ADD04XX002010-02-17                                                                                                                                                                                                                            ";
			var expectedXPLD = "E7F747HML3YAU2000002010-02-17ADD04XX002010-02-17                                                                                        DA                                                                                                                                  ";
			var log = Factory.New<ClientXPLDUploadLog>();
			log.U3_BISIData = preloadedXPLD;
			log.U3_ReasonCode = "XX";
			log.U3_TrackingNumber = "E7F747HML3Y";
			log = Factory.New<ClientXPLDUploadLog>();
			log.U3_BISIData = "this should be ignored because it is older";
			log.U3_DateCreated = new ZDateTime(2005, 1, 1);
			log.U3_ReasonCode = "XX";
			log.U3_TrackingNumber = "E7F747HML3Y";
			Factory.Save();
			testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			exportResult = Exporter.ExportToFile(testFilePath, exportInformation);
			using (var reader = new StreamReader(testFilePath))
			{
				var result = reader.ReadToEnd();
				Assert("should contain the expected output XPLD", result.Contains(expectedXPLD));
				Assert("should NOT contain the excess log", !result.Contains("this should be ignored because it is older"));
			}

			AssertMultilineASCIIEquals("Notifications for BISI Upload", @"
Start Adding Completed Shipment To Records
Finish Adding Completed Shipment To Records [Number of Lines: 12]
Start Adding Shipment Status To Records
Start Getting Every Day Statuses [Start Date: 08-Oct-05 13:37:43, End Date: 09-Oct-05 13:37:43]
Finish Getting 13 Candidate Every Day Statuses
Finish Adding Shipment Status To Records [Number of Lines: 5]".Trim(), Notifications.AsString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2005, 10, 09)]
		public void TestExportToFile()
		{
			PrepareDataForTestFile1();
			var testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			var exportInformation = new ExportInformation(710, ZDateTime.Now.AddDays(-1), ZDateTime.Now);
			exportInformation.RunEveryDayExport = true;
			exportInformation.EveryDayStartDate = ZDateTime.Now.AddDays(-1);
			exportInformation.EveryDayEndDate = ZDateTime.Now;
			var exportResult = Exporter.ExportToFile(testFilePath, exportInformation);
			AssertEquals("Export should be successful", BISIExportResult.ExportSuccess, exportResult);
			AssertBISIFileContent("Should have the same content.", UPETestHelper.TestFiles.BISI.Export.Folder + "BISIExportFile.txt", testFilePath);
			var logsCreated = Factory.Load<ClientXPLDUploadLog>(new ZQuery());
			AssertEquals("there should be logs created for each of the XPLD", 4, logsCreated.Length);
			AssertMultilineASCIIEquals("Notifications for BISI Upload", @"
Start Adding Completed Shipment To Records
Finish Adding Completed Shipment To Records [Number of Lines: 8]
Start Adding Shipment Status To Records
Start Getting Every Day Statuses [Start Date: 08-Oct-05 13:37:43, End Date: 09-Oct-05 13:37:43]
Finish Getting 13 Candidate Every Day Statuses
Finish Adding Shipment Status To Records [Number of Lines: 5]".Trim(), Notifications.AsString);
			Notifications.Clear();
			var preloadedXPLD = "E7F747HML3YAU2000002010-02-17ADD04XX002010-02-17                                                                                                                                                                                                                            ";
			var expectedXPLD = "E7F747HML3YAU2000002010-02-17ADD04XX002010-02-17                                                                                        DA                                                                                                                                  ";
			var log = Factory.New<ClientXPLDUploadLog>();
			log.U3_BISIData = preloadedXPLD;
			log.U3_ReasonCode = "XX";
			log.U3_TrackingNumber = "E7F747HML3Y";
			log = Factory.New<ClientXPLDUploadLog>();
			log.U3_BISIData = "this should be ignored because it is older";
			log.U3_DateCreated = new ZDateTime(2005, 1, 1);
			log.U3_ReasonCode = "XX";
			log.U3_TrackingNumber = "E7F747HML3Y";
			Factory.Save();
			testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			Exporter.ExportToFile(testFilePath, exportInformation);
			using (var reader = new StreamReader(testFilePath))
			{
				var result = reader.ReadToEnd();
				Assert("should contain the expected output XPLD", result.Contains(expectedXPLD));
				Assert("should NOT contain the excess log", !result.Contains("this should be ignored because it is older"));
			}

			AssertMultilineASCIIEquals("Notifications for BISI Upload", @"
Start Adding Completed Shipment To Records
Finish Adding Completed Shipment To Records [Number of Lines: 8]
Start Adding Shipment Status To Records
Start Getting Every Day Statuses [Start Date: 08-Oct-05 13:37:43, End Date: 09-Oct-05 13:37:43]
Finish Getting 13 Candidate Every Day Statuses
Finish Adding Shipment Status To Records [Number of Lines: 5]".Trim(), Notifications.AsString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2002, 12, 19, 10, 10, 10)]
		public void TestExportToFile_DateRangeFilter()
		{
			var startDate = new ZDateTime(2005, 10, 1);
			PrepareDataForDateRangeTest(startDate);
			var testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			var exportInformation = new ExportInformation(111, startDate, startDate.AddDays(4));
			exportInformation.RunEveryDayExport = true;
			exportInformation.EveryDayStartDate = startDate;
			exportInformation.EveryDayEndDate = startDate.AddDays(4);
			exportInformation.RunEveryDayDateOfArrivalExport = true;
			exportInformation.EveryDayDateOfArrivalStartDate = startDate;
			exportInformation.EveryDayDateOfArrivalEndDate = startDate.AddDays(4);
			exportInformation.RunWorkingDayOtherExport = true;
			exportInformation.WorkingDayOtherStartDate = startDate;
			exportInformation.WorkingDayOtherEndDate = startDate.AddDays(4);
			exportInformation.RunWorkingDayOtherExport = true;
			exportInformation.WorkingDayOtherStartDate = startDate;
			exportInformation.WorkingDayOtherEndDate = startDate.AddDays(4);
			AssertEquals("Export should be successful", BISIExportResult.ExportSuccess, Exporter.ExportToFile(testFilePath, exportInformation));
			AssertBISIFileContent("Should include lower bound but not upper bound", UPETestHelper.TestFiles.BISI.Export.Folder + "BISIExportFile_DateRange1.txt", testFilePath);
			exportInformation = new ExportInformation(112, startDate.AddDays(4), startDate.AddDays(8));
			exportInformation.RunEveryDayExport = true;
			exportInformation.EveryDayStartDate = startDate.AddDays(4);
			exportInformation.EveryDayEndDate = startDate.AddDays(8);
			exportInformation.RunWorkingDayOtherExport = true;
			exportInformation.WorkingDayOtherStartDate = startDate.AddDays(4);
			exportInformation.WorkingDayOtherEndDate = startDate.AddDays(8);
			AssertEquals("Export should be successful", BISIExportResult.ExportSuccess, Exporter.ExportToFile(testFilePath, exportInformation));
			AssertBISIFileContent("Should include lower bound but not upper bound", UPETestHelper.TestFiles.BISI.Export.Folder + "BISIExportFile_DateRange2.txt", testFilePath);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2002, 12, 19, 10, 10, 10)]
		public void TestExportToFile_SplitShipmentsCompletedAtTheSameBatchRun()
		{
			var tempFileName = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			var declaration = Factory.New<UPEJobDeclaration>();
			declaration.JE_DeclarationReference = "B00000211";
			declaration.CurrentQueue.P4_CustomsQueue = DefaultQueueCodeDescriptionPairList.Codes.Completed;
			declaration.QuarantineFee = 40m;
			AddInvoiceLines(declaration);
			CusMAWB.CM_ArrivalDate = new ZDateTime(2005, 4, 4);
			var splitShipment1 = CreateUPECusHAWB("HAWB1", "AAA");
			splitShipment1.CurrentQueue.P4_CustomsStatus = "";
			splitShipment1.CS_JE_CustomsFormalEntry = declaration.PK;
			splitShipment1.DutyType = DutyTypeCodeDescriptionPairList.Codes.Dutiable;
			var splitShipment2 = CreateUPECusHAWB("HAWB1", "AAA");
			splitShipment2.CurrentQueue.P4_CustomsStatus = "";
			splitShipment2.CS_JE_CustomsFormalEntry = declaration.PK;
			splitShipment2.DutyType = DutyTypeCodeDescriptionPairList.Codes.Dutiable;
			Factory.Save();
			var exportInformation = new ExportInformation(100, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			exportInformation.RunEveryDayExport = true;
			exportInformation.EveryDayStartDate = ZDateTime.Now.AddDays(-1);
			exportInformation.EveryDayEndDate = ZDateTime.Now.AddDays(1);
			AssertEquals("Export should be successful", BISIExportResult.ExportSuccess, Exporter.ExportToFile(tempFileName, exportInformation));
			AssertBISIFileContent("Should only include one shipment with ShipmentRef AAA", ExpectedExportFilePath_SplitShipment, tempFileName);
			Exporter.SaveDateUploadedAndBISIUploadData();
			AssertSplitShipmentsCompletedAtTheSameBatchRun(splitShipment1, splitShipment2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2002, 12, 19, 10, 10, 10)]
		public void TestExportToFile_SubsequentSplitShipment()
		{
			var tempFileName = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			var declaration = Factory.New<UPEJobDeclaration>();
			declaration.JE_DeclarationReference = "B00000211";
			declaration.CurrentQueue.P4_CustomsQueue = DefaultQueueCodeDescriptionPairList.Codes.Completed;
			declaration.QuarantineFee = 40m;
			AddInvoiceLines(declaration);
			CusMAWB.CM_ArrivalDate = new ZDateTime(2005, 4, 4);
			var splitShipment1 = CreateUPECusHAWB("HAWB1", "AAA");
			splitShipment1.CS_JE_CustomsFormalEntry = declaration.PK;
			splitShipment1.CurrentQueue.P4_CustomsStatus = "";
			splitShipment1.DutyType = DutyTypeCodeDescriptionPairList.Codes.Dutiable;
			Factory.Save();
			var exportInformation = new ExportInformation(100, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			exportInformation.RunEveryDayExport = true;
			exportInformation.EveryDayStartDate = ZDateTime.Now.AddDays(-1);
			exportInformation.EveryDayEndDate = ZDateTime.Now.AddDays(1);
			AssertEquals("Export should be successful", BISIExportResult.ExportSuccess, Exporter.ExportToFile(tempFileName, exportInformation));
			AssertBISIFileContent("Invalid file content", ExpectedExportFilePath_SplitShipment, tempFileName);
			Exporter.SaveDateUploadedAndBISIUploadData();
			TestDateAttribute.Date = new DateTime(2004, 1, 1);
			declaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Lodgement;
			Factory.Save();
			declaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Completed;
			var splitShipment2 = CreateUPECusHAWB("HAWB1", "AAA");
			splitShipment2.CurrentQueue.P4_CustomsStatus = "";
			splitShipment2.CS_JE_CustomsFormalEntry = declaration.PK;
			splitShipment2.DutyType = DutyTypeCodeDescriptionPairList.Codes.Dutiable;
			Factory.Save();
			exportInformation = new ExportInformation(101, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			exportInformation.EveryDayStartDate = ZDateTime.Now.AddDays(-1);
			exportInformation.EveryDayEndDate = ZDateTime.Now.AddDays(1);
			AssertEquals("Export should be successful but without data", BISIExportResult.ExportSuccessButNoData, Exporter.ExportToFile(tempFileName, exportInformation));
			Exporter.SaveDateUploadedAndBISIUploadData();
			AssertEquals("First split shipment should be uploaded", false, splitShipment1.BisiUploadDate.IsEmpty);
			AssertEquals("Subsequent split shipment should not be uploaded", true, splitShipment2.BisiUploadDate.IsEmpty);
			AssertEquals("Subsequent split shipment should be automatically F&A completed", CommercialQueueCodeDescriptionPairList.Codes.Completed, splitShipment2.CurrentQueue.P4_QueueName);
			AssertEquals("Subsequent split shipment should be automatically F&A completed", ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment, splitShipment2.CurrentQueue.P4_Status);
		}

		public void TestExportToFile_EmptyShipmentRef()
		{
			CreateCompletedShipmentAndBrokerageJobsWithEmptyShortTrackingNumber("HAWB1");
			CreateCompletedShipmentAndBrokerageJobsWithEmptyShortTrackingNumber("HAWB2");
			CreateCompletedShipmentAndBrokerageJobsWithEmptyShortTrackingNumber("HAWB3");
			Factory.Save();
			var tempPath = Path.Combine(Env.TempPath, ZGuid.NewZGuid().ToString());
			var exportInformation = new ExportInformation(1, ZDateTime.Now, ZDateTime.Now);
			exportInformation.RunEveryDayExport = true;
			exportInformation.EveryDayStartDate = ZDateTime.Now.AddDays(-1);
			exportInformation.EveryDayEndDate = ZDateTime.Now.AddDays(1);
			var exportResult = Exporter.ExportToFile(tempPath, exportInformation);
			AssertEquals("The completed shipments do not have a short tracking number, should not be included in the upload", BISIExportResult.ExportSuccessButNoData, exportResult);
			AssertEquals("Export file should not exist", false, File.Exists(tempPath));
		}

		public void TestExportToFile_ShipmentsWhichDoNotNeedToBeUploaded()
		{
			CreateCompletedPrepaidShipmentWithNoLocalCharges("HAWB1", "AAA1");
			CreateCompletedPrepaidShipmentWithNoLocalCharges("HAWB2", "AAA2");
			CreateCompletedPrepaidShipmentWithNoLocalCharges("HAWB3", "AAA3");
			Factory.Save();
			var tempPath = Path.Combine(Env.TempPath, ZGuid.NewZGuid().ToString());
			var exportInformation = new ExportInformation(1, ZDateTime.Now, ZDateTime.Now);
			exportInformation.RunEveryDayExport = true;
			exportInformation.EveryDayStartDate = ZDateTime.Now.AddDays(-1);
			exportInformation.EveryDayEndDate = ZDateTime.Now.AddDays(1);
			var exportResult = Exporter.ExportToFile("TempFile", exportInformation);
			AssertEquals("The completed shipments do not have local charges and not categorised as shipments which need to be uploaded", BISIExportResult.ExportSuccessButNoData, exportResult);
			AssertEquals("Export file should not exist", false, File.Exists(tempPath));
		}

		[TestDate(2005, 1, 2, 10, 10, 10)]
		public void TestIfDateOfArrivalRangeIsTheUnprocessedRange()
		{
			UPEDataRegistry.Instance.BISIUploadEverydayDateOfArrivalHWM = new ZDateTime(2005, 1, 2);
			CusMAWB.CM_ArrivalDate = new ZDateTime(2005, 1, 2);
			CreateShipmentWithXPLDCode("HAWB1", "HAWB1", "X2", "");
			Factory.Save();
			var testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			var exportInformation = new ExportInformation(1711, ZDateTime.Now, ZDateTime.Now);
			exportInformation.RunEveryDayExport = true;
			exportInformation.EveryDayStartDate = new ZDateTime(2005, 1, 2, 1, 1, 2);
			exportInformation.EveryDayEndDate = new ZDateTime(2005, 1, 2, 1, 1, 3);
			exportInformation.RunEveryDayDateOfArrivalExport = true;
			exportInformation.EveryDayDateOfArrivalStartDate = UPEDataRegistry.Instance.BISIUploadEverydayDateOfArrivalHWM;
			exportInformation.EveryDayDateOfArrivalEndDate = new ZDateTime(2005, 1, 3).Date;
			var exportResult = Exporter.ExportToFile(testFilePath, exportInformation);
			AssertEquals(BISIExportResult.ExportSuccessButNoData, exportResult);
		}

		[TestDate(2006, 1, 1, 10, 10, 10)]
		public void TestRunEveryDayExport_False()
		{
			CreateShipmentWithXPLDCode("100", "100", "DA", "");
			Factory.Save();
			var testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime = false;
			var exportInformation = new ExportInformation(1, ZDateTime.Now, ZDateTime.Now);
			AssertEquals(BISIExportResult.ExportSuccessButNoData, Exporter.ExportToFile(testFilePath, exportInformation));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 1, 1, 10, 10, 10)]
		public void TestRunEveryDayExport_True()
		{
			CreateShipmentWithXPLDCode("100", "100", "DA", "");
			Factory.Save();
			var testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime = false;
			var exportInformation = new ExportInformation(10, ZDateTime.Now, ZDateTime.Now);
			exportInformation.RunEveryDayExport = true;
			exportInformation.EveryDayStartDate = new ZDateTime(2006, 1, 1, 1, 1, 1);
			exportInformation.EveryDayEndDate = new ZDateTime(2006, 1, 1, 12, 12, 12);
			AssertEquals(BISIExportResult.ExportSuccess, Exporter.ExportToFile(testFilePath, exportInformation));
			AssertBISIFileContent(UPETestHelper.TestFiles.BISI.Export.Folder + "EveryDayExport.txt", testFilePath);
			AssertMultilineASCIIEquals("Notifications for BISI Upload", @"
Start Adding Completed Shipment To Records
Finish Adding Completed Shipment To Records [Number of Lines: 0]
Start Adding Shipment Status To Records
Start Getting Every Day Statuses [Start Date: 01-Jan-06 01:01:01, End Date: 01-Jan-06 12:12:12]
Finish Getting 2 Candidate Every Day Statuses
Finish Adding Shipment Status To Records [Number of Lines: 1]".Trim(), Notifications.AsString);
		}

		[TestDate(2006, 1, 1, 10, 10, 10)]
		public void TestRunEveryDayDateOfArrivalExport_False()
		{
			CreateShipmentWithXPLDCode("100", "100", "E8", "");
			Factory.Save();
			var testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime = false;
			var exportInformation = new ExportInformation(1, ZDateTime.Now, ZDateTime.Now);
			AssertEquals(BISIExportResult.ExportSuccessButNoData, Exporter.ExportToFile(testFilePath, exportInformation));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2005, 12, 1, 10, 10, 10)]
		public void TestRunEveryDayDateOfArrivalExport_True()
		{
			CreateShipmentWithXPLDCode("100", "100", "E8", "");
			CusMAWB.CM_ArrivalDate = new ZDateTime(2006, 1, 1, 1, 1, 1);
			Factory.Save();
			var testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime = false;
			var exportInformation = new ExportInformation(10, ZDateTime.Now, ZDateTime.Now);
			exportInformation.RunEveryDayDateOfArrivalExport = true;
			exportInformation.EveryDayDateOfArrivalStartDate = new ZDateTime(2006, 1, 1, 1, 1, 1);
			exportInformation.EveryDayDateOfArrivalEndDate = new ZDateTime(2006, 1, 1, 12, 12, 12);
			AssertEquals(BISIExportResult.ExportSuccess, Exporter.ExportToFile(testFilePath, exportInformation));
			AssertBISIFileContent(UPETestHelper.TestFiles.BISI.Export.Folder + "EverydayDateOfArrivalExport.txt", testFilePath);
			AssertMultilineASCIIEquals("Notifications for BISI Upload", @"
Start Adding Completed Shipment To Records
Finish Adding Completed Shipment To Records [Number of Lines: 0]
Start Adding Shipment Status To Records
Start Getting Every Day Date Of Arrival Statuses [Start Date: 01-Jan-06 00:00:00, End Date: 01-Jan-06 00:00:00]
Finish Getting 1 Candidate Every Day Date Of Arrival Statuses
Finish Adding Shipment Status To Records [Number of Lines: 1]".Trim(), Notifications.AsString);
		}

		[TestDate(2011, 2, 7, 10, 0, 0)]
		public void TestRunEveryDayDateOfArrivalExportForSelectedFlights()
		{
			var cusMAWB1 = Factory.New<CusMAWB>();
			cusMAWB1.CM_MAWB = "17628165315";
			cusMAWB1.CM_ArrivalDate = new ZDateTime(2011, 2, 9, 13, 7, 0);
			cusMAWB1.CM_FlightNo = "QA100";
			var cusMAWB2 = Factory.New<CusMAWB>();
			cusMAWB2.CM_MAWB = "17628165313";
			cusMAWB2.CM_ArrivalDate = new ZDateTime(2011, 2, 9, 11, 7, 0);
			cusMAWB2.CM_FlightNo = "QF101";
			var cusMAWB3 = Factory.New<CusMAWB>();
			cusMAWB3.CM_MAWB = "17628165317";
			cusMAWB3.CM_ArrivalDate = new ZDateTime(2011, 2, 10, 1, 7, 0);
			cusMAWB3.CM_FlightNo = "QF102";
			CreateShipmentWithXPLDCode("100", "100", "E8", "", cusMAWB1);
			CreateShipmentWithXPLDCode("101", "101", "E8", "", cusMAWB2);
			CreateShipmentWithXPLDCode("102", "102", "E8", "", cusMAWB3);
			Factory.Save();
			UPEDataRegistry.Instance.FlightNumbersForUploadOnDayOfArrivalPlusOne = new string[] { cusMAWB2.CM_FlightNo, cusMAWB3.CM_FlightNo };
			UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime = false;
			var timeNow = new ZDateTime(2011, 2, 9, 17, 00, 00);
			var exportInformation = new ExportInformation(10, timeNow.Add(-timeNow.TimeOfDay), timeNow);
			exportInformation.RunEveryDayDateOfArrivalExport = true;
			exportInformation.EveryDayDateOfArrivalStartDate = new ZDateTime(2011, 2, 9, 0, 0, 0);
			exportInformation.EveryDayDateOfArrivalEndDate = new ZDateTime(2011, 2, 9, 23, 59, 59);
			var recordList = Exporter.GetFileContent(exportInformation);
			AssertEquals("Today", 1, recordList.TotalLineCount);
			AssertEquals("QA100", true, recordList.Records[0]._200000Lines[0].LineAsString.StartsWith("100"));
			exportInformation.EveryDayDateOfArrivalStartDate = new ZDateTime(2011, 2, 10, 0, 0, 0);
			exportInformation.EveryDayDateOfArrivalEndDate = new ZDateTime(2011, 2, 10, 23, 59, 59);
			recordList = Exporter.GetFileContent(exportInformation);
			AssertEquals("Tommorrow", 1, recordList.TotalLineCount);
			AssertEquals("QA101", true, recordList.Records[0]._200000Lines[0].LineAsString.StartsWith("101"));
			exportInformation.EveryDayDateOfArrivalStartDate = new ZDateTime(2011, 2, 11, 0, 0, 0);
			exportInformation.EveryDayDateOfArrivalEndDate = new ZDateTime(2011, 2, 11, 23, 59, 59);
			recordList = Exporter.GetFileContent(exportInformation);
			AssertEquals("After Tommorow", 1, recordList.TotalLineCount);
			AssertEquals("QA102", true, recordList.Records[0]._200000Lines[0].LineAsString.StartsWith("102"));
			exportInformation.EveryDayDateOfArrivalStartDate = new ZDateTime(2011, 2, 9, 0, 0, 0);
			exportInformation.EveryDayDateOfArrivalEndDate = new ZDateTime(2011, 2, 10, 23, 59, 59);
			AssertEquals("2 days", 2, Exporter.GetFileContent(exportInformation).TotalLineCount);
			exportInformation.EveryDayDateOfArrivalStartDate = new ZDateTime(2011, 2, 9, 0, 0, 0);
			exportInformation.EveryDayDateOfArrivalEndDate = new ZDateTime(2011, 2, 11, 23, 59, 59);
			AssertEquals("3 days", 3, Exporter.GetFileContent(exportInformation).TotalLineCount);
		}

		[TestDate(2011, 2, 7, 9, 0, 0)]
		public void TestRunEveryDayExportForSelectedFlights()
		{
			var cusMAWB1 = Factory.New<CusMAWB>();
			cusMAWB1.CM_MAWB = "17628165315";
			cusMAWB1.CM_ArrivalDate = new ZDateTime(2011, 2, 7, 13, 7, 0);
			cusMAWB1.CM_FlightNo = "QA101";
			var cusMAWB2 = Factory.New<CusMAWB>();
			cusMAWB2.CM_MAWB = "17628165313";
			cusMAWB2.CM_ArrivalDate = new ZDateTime(2011, 2, 7, 11, 7, 0);
			cusMAWB2.CM_FlightNo = "QF100";
			var cusMAWB3 = Factory.New<CusMAWB>();
			cusMAWB3.CM_MAWB = "17628165317";
			cusMAWB3.CM_ArrivalDate = new ZDateTime(2011, 2, 8, 1, 7, 0);
			cusMAWB3.CM_FlightNo = "QF102";
			CreateShipmentWithXPLDCode("100", "100", "E8", "", cusMAWB2);
			CreateShipmentWithXPLDCode("101", "101", "E8", "", cusMAWB1);
			CreateShipmentWithXPLDCode("102", "102", "E8", "", cusMAWB3);
			Factory.Save();
			UPEDataRegistry.Instance.FlightNumbersForUploadOnDayOfArrivalPlusOne = new string[] { cusMAWB2.CM_FlightNo };
			var codeProibted = new CodeDescriptionPairList();
			codeProibted.AddPair("E8");
			codeProibted.AddPair("SR");
			var codeAllowed = new CodeDescriptionPairList();
			codeAllowed.AddPair("E8");
			UPEDataRegistry.Instance.XPLDForWorkingDays = codeProibted;
			UPEDataRegistry.Instance.XPLDForNonWorkingDaysDateOfArrivalPassed = codeAllowed;
			UPEDataRegistry.Instance.XPLDForWorkingDaysDateOfArrivalPassed = new CodeDescriptionPairList();
			UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime = false;
			var timeNow = new ZDateTime(2011, 2, 7, 17, 00, 00);
			var exportInformation = new ExportInformation(10, timeNow.Add(-timeNow.TimeOfDay), timeNow);
			exportInformation.RunEveryDayExport = true;
			exportInformation.EveryDayStartDate = new ZDateTime(2011, 2, 7, 0, 0, 0);
			exportInformation.EveryDayEndDate = new ZDateTime(2011, 2, 7, 23, 59, 59);
			AssertEquals("Today", 1, Exporter.GetFileContent(exportInformation).TotalLineCount);
			exportInformation.EveryDayStartDate = new ZDateTime(2011, 2, 8, 0, 0, 0);
			exportInformation.EveryDayEndDate = new ZDateTime(2011, 2, 8, 23, 59, 59);
			AssertEquals("Tommorrow", 0, Exporter.GetFileContent(exportInformation).TotalLineCount);
			exportInformation.EveryDayStartDate = new ZDateTime(2011, 2, 7, 0, 0, 0);
			exportInformation.EveryDayEndDate = new ZDateTime(2011, 2, 8, 23, 59, 59);
			AssertEquals("2 days", 1, Exporter.GetFileContent(exportInformation).TotalLineCount); // Because we compare with DateTime.Now not with import end date
		}

		[TestDate(2006, 2, 1, 10, 10, 10)]
		public void TestInspectIndicatorTrue()
		{
			CreateUPSZones();
			var uPECusHAWB = CreateShipmentWithXPLDCode("100", "100", "NY", "");
			uPECusHAWB.CS_ConsigneePostcode = "2010";
			Factory.Save();
			var testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			var exportInformation = new ExportInformation(1, ZDateTime.Now, ZDateTime.Now);
			exportInformation.EveryDayStartDate = ZDateTime.Now.AddSeconds(-2);
			exportInformation.EveryDayEndDate = ZDateTime.Now.AddSeconds(2);
			var exportResult = Exporter.ExportToFile(testFilePath, exportInformation);
			AssertEquals(BISIExportResult.ExportSuccessButNoData, exportResult);
			uPECusHAWB.CurrentQueue.P4_CustomsQueue = "QUA";
			Factory.Save();
			exportResult = Exporter.ExportToFile(testFilePath, exportInformation);
			AssertEquals(BISIExportResult.ExportSuccessButNoData, exportResult);
		}

		[TestDate(2006, 2, 1, 10, 10, 10)]
		public void TestRunWorkingDayMetroExport_False()
		{
			CreateUPSZones();
			var uPECusHAWB = CreateShipmentWithXPLDCode("100", "100", "NY", "");
			uPECusHAWB.CS_ConsigneePostcode = "2010";
			Factory.Save();
			var testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime = false;
			var exportInformation = new ExportInformation(1, ZDateTime.Now, ZDateTime.Now);
			AssertEquals(BISIExportResult.ExportSuccessButNoData, Exporter.ExportToFile(testFilePath, exportInformation));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 2, 1, 10, 10, 10)]
		public void TestRunWorkingDayMetroExport_True()
		{
			CreateUPSZones();
			var uPECusHAWB = CreateShipmentWithXPLDCode("100", "100", "NY", "");
			uPECusHAWB.CS_ConsigneePostcode = "2010";
			Factory.Save();
			var testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime = false;
			var exportInformation = new ExportInformation(10, ZDateTime.Now, ZDateTime.Now);
			exportInformation.RunWorkingDayMetroExport = true;
			exportInformation.WorkingDayMetroStartDate = new ZDateTime(2006, 2, 1, 1, 1, 1);
			exportInformation.WorkingDayMetroEndDate = new ZDateTime(2006, 2, 1, 12, 12, 12);
			AssertEquals(BISIExportResult.ExportSuccess, Exporter.ExportToFile(testFilePath, exportInformation));
			AssertBISIFileContent(UPETestHelper.TestFiles.BISI.Export.Folder + "WorkingDayMetroExport.txt", testFilePath);
			AssertMultilineASCIIEquals("Notifications for BISI Upload", @"
Start Adding Completed Shipment To Records
Finish Adding Completed Shipment To Records [Number of Lines: 0]
Start Adding Shipment Status To Records
Start Getting Working Day Metro Statuses [Start Date: 01-Feb-06 01:01:01, End Date: 01-Feb-06 12:12:12]
Finish Getting 2 Candidate Working Day Metro Statuses
Finish Adding Shipment Status To Records [Number of Lines: 1]".Trim(), Notifications.AsString);
		}

		[TestDate(2005, 12, 3, 10, 10, 10)]
		public void TestRunWorkingDayDateOfArrivalMetroExport_False()
		{
			CreateUPSZones();
			var uPECusHAWB = CreateShipmentWithXPLDCode("100", "100", "SN", "");
			uPECusHAWB.CS_ConsigneePostcode = "2010";
			CusMAWB.CM_ArrivalDate = new ZDateTime(2006, 1, 1, 1, 1, 1);
			Factory.Save();
			var testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime = false;
			var exportInformation = new ExportInformation(1, ZDateTime.Now, ZDateTime.Now);
			AssertEquals(BISIExportResult.ExportSuccessButNoData, Exporter.ExportToFile(testFilePath, exportInformation));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2005, 12, 3, 10, 10, 10)]
		public void TestRunWorkingDayDateOfArrivalMetroExport_True()
		{
			CreateUPSZones();
			var uPECusHAWB = CreateShipmentWithXPLDCode("100", "100", "SN", "");
			uPECusHAWB.CS_ConsigneePostcode = "2010";
			CusMAWB.CM_ArrivalDate = new ZDateTime(2006, 2, 1, 1, 1, 1);
			Factory.Save();
			var testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime = false;
			var exportInformation = new ExportInformation(10, ZDateTime.Now, ZDateTime.Now);
			exportInformation.RunWorkingDayDateOfArrivalMetroExport = true;
			exportInformation.WorkingDayDateOfArrivalMetroStartDate = new ZDateTime(2006, 2, 1, 1, 1, 1);
			exportInformation.WorkingDayDateOfArrivalMetroEndDate = new ZDateTime(2006, 2, 1, 12, 12, 12);
			AssertEquals(BISIExportResult.ExportSuccess, Exporter.ExportToFile(testFilePath, exportInformation));
			AssertBISIFileContent(UPETestHelper.TestFiles.BISI.Export.Folder + "WorkingDayDateOfArrivalMetroExport.txt", testFilePath);
			AssertMultilineASCIIEquals("Notifications for BISI Upload", @"
Start Adding Completed Shipment To Records
Finish Adding Completed Shipment To Records [Number of Lines: 0]
Start Adding Shipment Status To Records
Start Getting Working Day Date Of Arrival Metro Statuses [Start Date: 01-Feb-06 00:00:00, End Date: 01-Feb-06 00:00:00]
Finish Getting 1 Candidate Working Day Date Of Arrival Metro Statuses
Finish Adding Shipment Status To Records [Number of Lines: 1]".Trim(), Notifications.AsString);
		}

		[TestDate(2011, 2, 9, 23, 0, 0)]
		public void TestRunWorkingDayDateOfArrivalMetroExportForSelectedFlights()
		{
			CreateUPSZones();
			var cusMAWB1 = Factory.New<CusMAWB>();
			cusMAWB1.CM_MAWB = "17628165318";
			cusMAWB1.CM_ArrivalDate = new ZDateTime(2011, 2, 8, 1, 7, 0);
			cusMAWB1.CM_FlightNo = "QF101";
			var cusMAWB2 = Factory.New<CusMAWB>();
			cusMAWB2.CM_MAWB = "17628165313";
			cusMAWB2.CM_ArrivalDate = new ZDateTime(2011, 2, 9, 11, 7, 0);
			cusMAWB2.CM_FlightNo = "QF102";
			var cusMAWB3 = Factory.New<CusMAWB>();
			cusMAWB3.CM_MAWB = "17628165317";
			cusMAWB3.CM_ArrivalDate = new ZDateTime(2011, 2, 10, 1, 7, 0);
			cusMAWB3.CM_FlightNo = "QF103";
			var uPECusHAWB1 = CreateShipmentWithXPLDCode("101", "101", "SS", "", cusMAWB1);
			uPECusHAWB1.CS_ConsigneePostcode = "2010";
			var uPECusHAWB2 = CreateShipmentWithXPLDCode("102", "102", "SS", "", cusMAWB2);
			uPECusHAWB2.CS_ConsigneePostcode = "2010";
			var uPECusHAWB3 = CreateShipmentWithXPLDCode("103", "103", "SS", "", cusMAWB3);
			uPECusHAWB3.CS_ConsigneePostcode = "2010";
			Factory.Save();
			UPEDataRegistry.Instance.FlightNumbersForUploadOnDayOfArrivalPlusOne = new string[] { cusMAWB1.CM_FlightNo, cusMAWB2.CM_FlightNo, cusMAWB3.CM_FlightNo };
			UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime = false;
			var timeNow = new ZDateTime(2011, 2, 11, 10, 31, 59);
			var exportInformation = new ExportInformation(10, timeNow.Add(-timeNow.TimeOfDay), timeNow);
			exportInformation.RunWorkingDayMetroExport = true;
			exportInformation.WorkingDayMetroStartDate = new ZDateTime(2011, 2, 09, 22, 1, 0);
			exportInformation.WorkingDayMetroEndDate = timeNow;
			exportInformation.RunWorkingDayDateOfArrivalMetroExport = true;
			exportInformation.WorkingDayDateOfArrivalMetroStartDate = new ZDateTime(2011, 2, 10, 0, 0, 0);
			exportInformation.WorkingDayDateOfArrivalMetroEndDate = timeNow;
			AssertEquals("All statuses should be uploaded ", 3, Exporter.GetFileContent(exportInformation).TotalLineCount);
			AssertMultilineASCIIEquals("Notifications for BISI Upload", @"
Start Adding Completed Shipment To Records
Finish Adding Completed Shipment To Records [Number of Lines: 0]
Start Adding Shipment Status To Records
Start Getting Working Day Metro Statuses [Start Date: 09-Feb-11 22:01:00, End Date: 11-Feb-11 10:31:59]
Finish Getting 6 Candidate Working Day Metro Statuses
Start Getting Working Day Date Of Arrival Metro Statuses [Start Date: 10-Feb-11 00:00:00, End Date: 11-Feb-11 00:00:00]
Finish Getting 2 Candidate Working Day Date Of Arrival Metro Statuses
Finish Adding Shipment Status To Records [Number of Lines: 3]".Trim(), Notifications.AsString);
		}

		[TestDate(2011, 2, 9, 23, 0, 0)]
		public void TestRunWorkingDayDateOfArrivalMetroExport()
		{
			CreateUPSZones();
			var cusMAWB1 = Factory.New<CusMAWB>();
			cusMAWB1.CM_MAWB = "17628165318";
			cusMAWB1.CM_ArrivalDate = new ZDateTime(2011, 2, 8, 1, 7, 0);
			cusMAWB1.CM_FlightNo = "QF101";
			var cusMAWB2 = Factory.New<CusMAWB>();
			cusMAWB2.CM_MAWB = "17628165313";
			cusMAWB2.CM_ArrivalDate = new ZDateTime(2011, 2, 9, 11, 7, 0);
			cusMAWB2.CM_FlightNo = "QF102";
			var cusMAWB3 = Factory.New<CusMAWB>();
			cusMAWB3.CM_MAWB = "17628165317";
			cusMAWB3.CM_ArrivalDate = new ZDateTime(2011, 2, 10, 1, 7, 0);
			cusMAWB3.CM_FlightNo = "QF103";
			var uPECusHAWB1 = CreateShipmentWithXPLDCode("101", "101", "SS", "", cusMAWB1);
			uPECusHAWB1.CS_ConsigneePostcode = "2010";
			var uPECusHAWB2 = CreateShipmentWithXPLDCode("102", "102", "SS", "", cusMAWB2);
			uPECusHAWB2.CS_ConsigneePostcode = "2010";
			var uPECusHAWB3 = CreateShipmentWithXPLDCode("103", "103", "SS", "", cusMAWB3);
			uPECusHAWB3.CS_ConsigneePostcode = "2010";
			Factory.Save();
			UPEDataRegistry.Instance.FlightNumbersForUploadOnDayOfArrivalPlusOne = new string[] { "QF100" };
			UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime = false;
			var timeNow = new ZDateTime(2011, 2, 11, 10, 31, 59);
			var exportInformation = new ExportInformation(10, timeNow.Add(-timeNow.TimeOfDay), timeNow);
			exportInformation.RunWorkingDayMetroExport = true;
			exportInformation.WorkingDayMetroStartDate = new ZDateTime(2011, 2, 09, 22, 1, 0);
			exportInformation.WorkingDayMetroEndDate = timeNow;
			exportInformation.RunWorkingDayDateOfArrivalMetroExport = true;
			exportInformation.WorkingDayDateOfArrivalMetroStartDate = new ZDateTime(2011, 2, 10, 0, 0, 0);
			exportInformation.WorkingDayDateOfArrivalMetroEndDate = timeNow;
			AssertEquals("All statuses should be uploaded ", 3, Exporter.GetFileContent(exportInformation).TotalLineCount);
			AssertMultilineASCIIEquals("Notifications for BISI Upload", @"
Start Adding Completed Shipment To Records
Finish Adding Completed Shipment To Records [Number of Lines: 0]
Start Adding Shipment Status To Records
Start Getting Working Day Metro Statuses [Start Date: 09-Feb-11 22:01:00, End Date: 11-Feb-11 10:31:59]
Finish Getting 6 Candidate Working Day Metro Statuses
Start Getting Working Day Date Of Arrival Metro Statuses [Start Date: 10-Feb-11 00:00:00, End Date: 11-Feb-11 00:00:00]
Finish Getting 1 Candidate Working Day Date Of Arrival Metro Statuses
Finish Adding Shipment Status To Records [Number of Lines: 3]".Trim(), Notifications.AsString);
		}

		[TestDate(2006, 2, 1, 10, 10, 10)]
		public void TestRunWorkingDayOtherExport_False()
		{
			CreateShipmentWithXPLDCode("100", "100", "NY", "");
			Factory.Save();
			var testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime = false;
			var exportInformation = new ExportInformation(1, ZDateTime.Now, ZDateTime.Now);
			AssertEquals(BISIExportResult.ExportSuccessButNoData, Exporter.ExportToFile(testFilePath, exportInformation));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 2, 1, 10, 10, 10)]
		public void TestRunWorkingDayOtherExport_True()
		{
			CreateShipmentWithXPLDCode("100", "100", "NY", "");
			Factory.Save();
			var testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime = false;
			var exportInformation = new ExportInformation(10, ZDateTime.Now, ZDateTime.Now);
			exportInformation.RunWorkingDayOtherExport = true;
			exportInformation.WorkingDayOtherStartDate = new ZDateTime(2006, 2, 1, 1, 1, 1);
			exportInformation.WorkingDayOtherEndDate = new ZDateTime(2006, 2, 1, 12, 12, 12);
			AssertEquals(BISIExportResult.ExportSuccess, Exporter.ExportToFile(testFilePath, exportInformation));
			AssertBISIFileContent(UPETestHelper.TestFiles.BISI.Export.Folder + "WorkingDayOtherExport.txt", testFilePath);
			AssertMultilineASCIIEquals("Notifications for BISI Upload", @"
Start Adding Completed Shipment To Records
Finish Adding Completed Shipment To Records [Number of Lines: 0]
Start Adding Shipment Status To Records
Start Getting Working Day Other Statuses [Start Date: 01-Feb-06 01:01:01, End Date: 01-Feb-06 12:12:12]
Finish Getting 2 Candidate Working Day Other Statuses
Finish Adding Shipment Status To Records [Number of Lines: 1]".Trim(), Notifications.AsString);
		}

		[TestDate(2005, 12, 3, 10, 10, 10)]
		public void TestRunWorkingDayDateOfArrivalOtherExport_False()
		{
			CreateShipmentWithXPLDCode("100", "100", "SN", "");
			CusMAWB.CM_ArrivalDate = new ZDateTime(2006, 1, 1, 1, 1, 1);
			Factory.Save();
			var testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime = false;
			var exportInformation = new ExportInformation(1, ZDateTime.Now, ZDateTime.Now);
			AssertEquals(BISIExportResult.ExportSuccessButNoData, Exporter.ExportToFile(testFilePath, exportInformation));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2005, 12, 3, 10, 10, 10)]
		public void TestRunWorkingDayDateOfArrivalOtherExport_True()
		{
			CreateShipmentWithXPLDCode("100", "100", "SN", "");
			CusMAWB.CM_ArrivalDate = new ZDateTime(2006, 2, 1, 1, 1, 1);
			Factory.Save();
			var testFilePath = Path.Combine(TestDir, ZGuid.NewZGuid().ToString());
			UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime = false;
			var exportInformation = new ExportInformation(10, ZDateTime.Now, ZDateTime.Now);
			exportInformation.RunWorkingDayDateOfArrivalOtherExport = true;
			exportInformation.WorkingDayDateOfArrivalOtherStartDate = new ZDateTime(2006, 2, 1, 1, 1, 1);
			exportInformation.WorkingDayDateOfArrivalOtherEndDate = new ZDateTime(2006, 2, 1, 12, 12, 12);
			AssertEquals(BISIExportResult.ExportSuccess, Exporter.ExportToFile(testFilePath, exportInformation));
			AssertBISIFileContent(UPETestHelper.TestFiles.BISI.Export.Folder + "WorkingDayDateOfArrivalOtherExport.txt", testFilePath);
			AssertMultilineASCIIEquals("Notifications for BISI Upload", @"
Start Adding Completed Shipment To Records
Finish Adding Completed Shipment To Records [Number of Lines: 0]
Start Adding Shipment Status To Records
Start Getting Working Day Date Of Arrival Other Statuses [Start Date: 01-Feb-06 00:00:00, End Date: 01-Feb-06 00:00:00]
Finish Getting 1 Candidate Working Day Date Of Arrival Other Statuses
Finish Adding Shipment Status To Records [Number of Lines: 1]".Trim(), Notifications.AsString);
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			TestCaseHelper.RunClientDbCreateScripts();
			Directory.CreateDirectory(TestDir);
		}

		protected override void TearDown()
		{
			base.TearDown();
			TempDirectory.DeleteDirectory(TestDir);
		}

		void CreateUPSZones()
		{
			ZonesTestHelper.CreateUPSZones(Factory);
		}

		ZonesTestHelper zonesTestHelper;
		ZonesTestHelper ZonesTestHelper => zonesTestHelper ?? (zonesTestHelper = new ZonesTestHelper());

		string GetTextFileContent(string filePath)
		{
			var result = "";
			using (var reader = new StreamReader(filePath))
			{
				result = reader.ReadToEnd();
			}

			return result;
		}

		string TestDir => Path.Combine(Env.TempPath, "BISIFILEEXPORTERTEST");

		void PrepareDataForPreReleaseShipment(bool preReleaseflag = true)
		{
			var importer = Factory.LoadTop1<UPEOrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			importer.IsPreReleaseContactFeeApplicable = preReleaseflag;
			CusMAWB.CM_ArrivalDate = new ZDateTime(2005, 10, 09);
			var hAWB1 = CreateCompletedShipmentAndBrokerageJobs("HAWB1", "1W0282GSLRC");
			hAWB1.DutyType = DutyTypeCodeDescriptionPairList.Codes.Dutiable;
			hAWB1.Declaration.JE_DeclarationReference = "B00016042";
			hAWB1.Declaration.JE_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			hAWB1.Declaration.JE_OH_Importer = importer.PK;
			var hAWB2 = CreateCompletedShipmentAndBrokerageJobsWithXPLDCode("HAWB2", "98Y44YKLNHW", ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, "", "Test Note" + System.Environment.NewLine + "Test Note Line 2");
			hAWB2.DutyType = DutyTypeCodeDescriptionPairList.Codes.Dutiable;
			hAWB2.Declaration.JE_DeclarationReference = "B00015035";
			hAWB2.Declaration.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			hAWB2.Declaration.JE_OH_Importer = importer.PK;
			CreateShipmentWithXPLDCode("HAWB3", "Y29770VRKZ7", "DN", "");
			CreateShipmentWithXPLDCode("HAWB4", "A8V2493DDTB", "SR", "");
			CreateShipmentWithXPLDCode("HAWB5", "E7F747HML3Y", "DA", "");
			CreateShipmentWithXPLDCode("HAWB6", "W21676J99TC", "DN", "Y1");
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2005, 10, 9, 13, 37, 43);
		}

		void PrepareDataForTestFile1()
		{
			CusMAWB.CM_ArrivalDate = new ZDateTime(2005, 10, 09);
			var hAWB1 = CreateCompletedShipmentAndBrokerageJobs("HAWB1", "1W0282GSLRC");
			hAWB1.DutyType = DutyTypeCodeDescriptionPairList.Codes.Dutiable;
			hAWB1.Declaration.JE_DeclarationReference = "B00016042";
			var hAWB2 = CreateCompletedShipmentAndBrokerageJobsWithXPLDCode("HAWB2", "98Y44YKLNHW", ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, "", "Test Note" + System.Environment.NewLine + "Test Note Line 2");
			hAWB2.DutyType = DutyTypeCodeDescriptionPairList.Codes.Dutiable;
			hAWB2.Declaration.JE_DeclarationReference = "B00015035";
			CreateShipmentWithXPLDCode("HAWB3", "Y29770VRKZ7", "DN", "");
			CreateShipmentWithXPLDCode("HAWB4", "A8V2493DDTB", "SR", "");
			CreateShipmentWithXPLDCode("HAWB5", "E7F747HML3Y", "DA", "");
			CreateShipmentWithXPLDCode("HAWB6", "W21676J99TC", "DN", "Y1");
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2005, 10, 9, 13, 37, 43);
		}

		void PrepareDataForDateRangeTest(ZDateTime startDate)
		{
			CusMAWB.CM_ArrivalDate = new ZDateTime(2005, 10, 9);
			CreateShipmentWithXPLDCode("HAWB1", "AA1", "NY", "", startDate);
			CreateShipmentWithXPLDCode("HAWB2", "AA2", "SR", "", startDate.AddDays(1));
			CreateShipmentWithXPLDCode("HAWB3", "AA3", "NY", "Y1", startDate.AddDays(2));
			CreateShipmentWithXPLDCode("HAWB4", "AA4", "DA", "", startDate.AddDays(3));
			CreateShipmentWithXPLDCode("HAWB5", "AA5", "BP", "", startDate.AddDays(4));
			CreateShipmentWithXPLDCode("HAWB6", "AA6", "NY", "KO", startDate.AddDays(5));
			CreateShipmentWithXPLDCode("HAWB7", "AA7", "FE", "", startDate.AddDays(6));
			CreateShipmentWithXPLDCode("HAWB8", "AA8", "FF", "", startDate.AddDays(7));
			CreateShipmentWithXPLDCode("HAWB9", "AA9", "TT", "KO", startDate.AddDays(8));
			Factory.Save();
		}

		void AssertBISIFileContent(string expectedFilePath, string generatedFilePath)
		{
			AssertBISIFileContent("File content should be the same", expectedFilePath, generatedFilePath);
		}

		void AssertBISIFileContent(string errorMessage, string expectedFilePath, string generatedFilePath)
		{
			var expectedFileContent = GetTextFileContent(expectedFilePath);
			var generatedFileContent = GetTextFileContent(generatedFilePath);
			var expectedFileLines = expectedFileContent.Replace("\r\n", "\n").Split('\n');
			var generatedFileLines = generatedFileContent.Replace("\r\n", "\n").Split('\n');
			Array.Sort(expectedFileLines, 1, expectedFileLines.Length - 1);
			Array.Sort(generatedFileLines, 1, generatedFileLines.Length - 1);
			var sortedExpectedFileContent = string.Join("\r\n", expectedFileLines);
			var sortedGeneratedFileContent = string.Join("\r\n", generatedFileLines);
			AssertMultilineEquals(errorMessage, sortedExpectedFileContent, sortedGeneratedFileContent, '\n');
		}

		void AssertSplitShipmentsCompletedAtTheSameBatchRun(params UPECusHAWB[] splitShipments)
		{
			var firstSplitShipmentFound = false;
			foreach (var splitShipment in splitShipments)
			{
				if (!splitShipment.BisiUploadDate.IsEmpty)
				{
					if (!firstSplitShipmentFound)
					{
						firstSplitShipmentFound = true;
					}
					else
					{
						Fail("Only one split shipment can be uploaded");
					}
				}
				else
				{
					AssertEquals("Subsequent split shipment should be automatically F&A completed", CommercialQueueCodeDescriptionPairList.Codes.Completed, splitShipment.CurrentQueue.P4_QueueName);
					AssertEquals("Subsequent split shipment should be automatically F&A completed", ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment, splitShipment.CurrentQueue.P4_Status);
				}
			}
		}

		UPECusHAWB CreateCompletedShipmentAndBrokerageJobsWithEmptyShortTrackingNumber(ZString hAWB)
		{
			var result = CreateCompletedShipmentAndBrokerageJobs(hAWB);
			result.WayBillShort = "";
			return result;
		}

		UPECusHAWB CreateCompletedShipmentAndBrokerageJobs(ZString hAWB) => CreateCompletedShipmentAndBrokerageJobs(hAWB, "");

		UPECusHAWB CreateCompletedPrepaidShipmentWithNoLocalCharges(ZString trackingNumber, ZString shortTrackingNumber)
		{
			var result = CreateUPECusHAWB(trackingNumber, shortTrackingNumber);
			result.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Completed;
			result.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.Prepaid;
			return result;
		}

		UPECusHAWB CreateCompletedShipmentAndBrokerageJobs(ZString trackingNumber, ZString shortTrackingNumber)
		{
			var cusHAWB = CreateUPECusHAWB(trackingNumber, shortTrackingNumber);
			var declaration = Factory.New<UPEJobDeclaration>();
			declaration.QuarantineFee = 40m;
			AddInvoiceLines(declaration);
			new UPEDeclarationFromAirCargoCreator(cusHAWB).Create(declaration, Notifications);
			cusHAWB.CurrentQueue.P4_CustomsQueue = DefaultQueueCodeDescriptionPairList.Codes.Completed;
			declaration.CurrentQueue.P4_CustomsQueue = DefaultQueueCodeDescriptionPairList.Codes.Completed;
			return cusHAWB;
		}

		void SetupPreReleaseNotificationRegistries()
		{
			UPEDataRegistry.Instance.PreReleaseChargeEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			UPEDataRegistry.Instance.EntryLineChargeCappedAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 400m);
			UPEDataRegistry.Instance.PerLineChargeAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4m);
			UPEDataRegistry.Instance.LinesExemptedFromLineCharge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			UPEDataRegistry.Instance.EntryLineChargeBaseAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 80);
			UPEDataRegistry.Instance.ContactFeeAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 50);
		}

		void SetupPreReleaseFlag()
		{
			var testTemplate = Factory.New<ProcessTaskTemplate>();
			testTemplate.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			testTemplate.P0_Name = "test template";
			var preReleaseFlag = testTemplate.GenCustomColumnDefinitions.AddNew();
			preReleaseFlag.XC_Name = UPEOrgHeader.PreReleaseNotificationFieldName;
			preReleaseFlag.XC_Type = AddOnColumnDataType.Codes.Boolean;
			Factory.Save();
		}

		void AddInvoiceLines(UPEJobDeclaration declaration)
		{
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_InvoiceNumber = "1111";
			var invLine1 = invHeader.JobComInvoiceLines.AddNew();
			invLine1.JI_Description = "Goods1";
			invLine1.JI_Tariff = "1";
			invLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invLine1.JI_LinePrice = 100.10;
			var invLine2 = invHeader.JobComInvoiceLines.AddNew();
			invLine2.JI_Description = "Goods2";
			invLine2.JI_Tariff = "2";
			invLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			invLine2.JI_LinePrice = 200.20;
		}

		UPECusHAWB CreateCompletedShipmentAndBrokerageJobsWithXPLDCode(ZString trackingNumber, ZString shortTrackingNumber, ZString statusCode, ZString subStatusCode, ZString remarks)
		{
			var cusHAWB = CreateCompletedShipmentAndBrokerageJobs(trackingNumber, shortTrackingNumber);
			InsertXPLDCodeForCusHAWB(cusHAWB, statusCode, subStatusCode, remarks);
			return cusHAWB;
		}

		UPECusHAWB CreateShipmentWithXPLDCode(ZString trackingNumber, ZString shortTrackingNumber, ZString statusCode, ZString subStatusCode)
		{
			return CreateShipmentWithXPLDCode(trackingNumber, shortTrackingNumber, statusCode, subStatusCode, ZDateTime.Empty, null);
		}

		UPECusHAWB CreateShipmentWithXPLDCode(ZString trackingNumber, ZString shortTrackingNumber, ZString statusCode, ZString subStatusCode, CusMAWB mawb)
		{
			return CreateShipmentWithXPLDCode(trackingNumber, shortTrackingNumber, statusCode, subStatusCode, ZDateTime.Empty, mawb);
		}

		UPECusHAWB CreateShipmentWithXPLDCode(ZString trackingNumber, ZString shortTrackingNumber, ZString statusCode, ZString subStatusCode, ZDateTime postedTime)
		{
			return CreateShipmentWithXPLDCode(trackingNumber, shortTrackingNumber, statusCode, subStatusCode, postedTime, null);
		}

		UPECusHAWB CreateShipmentWithXPLDCode(ZString trackingNumber, ZString shortTrackingNumber, ZString statusCode, ZString subStatusCode, ZDateTime postedTime, CusMAWB mawb)
		{
			var testDate = TestDateAttribute.Date;
			var cusHAWB = CreateUPECusHAWB(trackingNumber, shortTrackingNumber, mawb);
			try
			{
				if (!postedTime.IsEmpty)
				{
					TestDateAttribute.Date = postedTime.ToDateTime();
				}

				InsertXPLDCodeForCusHAWB(cusHAWB, statusCode, subStatusCode);
				Factory.Save();
			}
			finally
			{
				TestDateAttribute.Date = testDate;
			}

			return cusHAWB;
		}

		ProcessQueueLog InsertXPLDCodeForCusHAWB(UPECusHAWB cusHAWB, ZString statusCode, ZString subStatusCode)
		{
			return cusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", statusCode, subStatusCode, "", "");
		}

		ProcessQueueLog InsertXPLDCodeForCusHAWB(UPECusHAWB cusHAWB, ZString statusCode, ZString subStatusCode, ZString remarks)
		{
			return cusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", statusCode, subStatusCode, remarks, "");
		}

		UPECusHAWB CreateUPECusHAWB(ZString trackingNumber, ZString shortTrackingNumber)
		{
			return CreateUPECusHAWB(trackingNumber, shortTrackingNumber, null);
		}

		UPECusHAWB CreateUPECusHAWB(ZString trackingNumber, ZString shortTrackingNumber, CusMAWB mawb)
		{
			var cusMawb = mawb ?? CusMAWB;
			var cusHAWB = (UPECusHAWB)cusMawb.ChildBills.AddNew(typeof(UPECusHAWB));
			cusHAWB.CS_HAWB = trackingNumber;
			if (!shortTrackingNumber.IsEmpty)
			{
				cusHAWB.WayBillShort = shortTrackingNumber;
			}

			return cusHAWB;
		}

		CusMAWB cusMAWB;
		CusMAWB CusMAWB
		{
			get
			{
				if (cusMAWB == null)
				{
					cusMAWB = Factory.New<CusMAWB>();
					cusMAWB.CM_MAWB = "17628165314";
					cusMAWB.CM_FlightNo = "QA100";
				}

				return cusMAWB;
			}
		}

		BISIFileExporterForTest exporter;
		BISIFileExporterForTest Exporter => exporter ?? (exporter = new BISIFileExporterForTest(Notifications));

		NotificationBufferForTesting notifications;
		NotificationBufferForTesting Notifications => notifications ?? (notifications = new NotificationBufferForTesting());

		string ExpectedExportFilePath_SplitShipment => UPETestHelper.TestFiles.BISI.Export.Folder + "BISIExportFile_SplitShipment.txt";

		void AssertUploadedDataSavedInBISIShipmentTable(string message, CusHAWB hAWB, bool expectData)
		{
			AssertEquals(message, expectData, IsUploadedDataSavedInBISIShipmentTable(hAWB));
		}

		bool IsUploadedDataSavedInBISIShipmentTable(CusHAWB hAWB)
		{
			var sQL = string.Format("SELECT COUNT(*) FROM {0} WHERE {1}='{2}'", UPEClientTables.ClientBISIShipmentHeader.TableName, UPEClientTables.ClientBISIShipmentHeader.T8_CS, hAWB.PK.ToString());
			var command = Db.Connection.Command(sQL);
			var count = (int)command.ExecuteScalar();
			return count > 0;
		}

		sealed class TestBISIFileExporterWithPropertiesExposed : BISIFileExporter
		{
			public TestBISIFileExporterWithPropertiesExposed(INotifications notify) : base(notify)
			{
			}

			internal new BusinessObjectFactory Factory => base.Factory;

			internal new List<IShipmentData> LastUploadedCompletedShipmentsInternal => base.LastUploadedCompletedShipmentsInternal;
		}
	}
}
