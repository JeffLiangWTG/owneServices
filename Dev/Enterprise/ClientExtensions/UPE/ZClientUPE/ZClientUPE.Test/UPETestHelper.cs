using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.DataImport;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.ClientSharedComponents;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.UPE.Testing
{
	sealed class UPETestHelper : SharedTestHelper
	{
		internal UPETestHelper() { }

		internal UPETestHelper(BusinessObjectFactory factory) : base(factory) { }

		internal Callout TestCallout
		{
			get
			{
				if (testCallout == null)
				{
					testCallout = (Callout)MasterBill.ChildBills.AddNew(typeof(Callout));
				}
				return testCallout;
			}
		}
		Callout testCallout;

		#region HouseBill and Test Data
		internal UPECusHAWB HouseBill
		{
			get
			{
				if (houseBill == null)
				{
					houseBill = (UPECusHAWB)MasterBill.ChildBills.AddNew();
					houseBill.CS_RL_NKOrigin = "SGSIN";
					houseBill.CS_RL_NKDestination = "AUSYD";
				}
				return houseBill;
			}
		}
		UPECusHAWB houseBill;

		internal ZQuery HouseBillFilter
		{
			get { return houseBillFilter ?? (houseBillFilter = new ZQuery(CusHAWBSchema.CS_CM, MasterBill.PK)); }
		}
		ZQuery houseBillFilter;

		internal void SetHouseBillTestData()
		{
			CreateCusHAWB("101", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.Completed);             // Finalised
			CreateCusHAWB("102", AutoCargoReportQueueCodeDescriptionPairList.Codes.AwaitingDeclaration, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.Completed);
			CreateCusHAWB("103", AutoCargoReportQueueCodeDescriptionPairList.Codes.AwaitingEvaluation, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.Completed);
			CreateCusHAWB("104", AutoCargoReportQueueCodeDescriptionPairList.Codes.Intervention, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.Completed);
			CreateCusHAWB("105", AutoCargoReportQueueCodeDescriptionPairList.Codes.Pending, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.Completed);
			CreateCusHAWB("106", AutoCargoReportQueueCodeDescriptionPairList.Codes.Quarantine, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.Completed);
			CreateCusHAWB("107", AutoCargoReportQueueCodeDescriptionPairList.Codes.Unknown, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.Completed);
			CreateCusHAWB("108", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.OnFile, AutoDeclarationQueueCodeDescriptionPairList.Codes.Completed, true);              // Finalised
			CreateCusHAWB("109", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.Chase, AutoDeclarationQueueCodeDescriptionPairList.Codes.Completed, false);                  // Finalised
			CreateCusHAWB("110", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.Rebill, AutoDeclarationQueueCodeDescriptionPairList.Codes.Completed, true);              // Finalised
			CreateCusHAWB("111", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.Hold, AutoDeclarationQueueCodeDescriptionPairList.Codes.Completed);
			CreateCusHAWB("112", AutoCargoReportQueueCodeDescriptionPairList.Codes.Hold, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.Completed);
			CreateCusHAWB("113", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.EIR, AutoDeclarationQueueCodeDescriptionPairList.Codes.Completed);
			CreateCusHAWB("114", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.Finance, AutoDeclarationQueueCodeDescriptionPairList.Codes.Completed);
			CreateCusHAWB("115", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding);        // Finalised
			CreateCusHAWB("116", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.BCA);
			CreateCusHAWB("117", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.EIR);
			CreateCusHAWB("118", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.Lodgement);
			CreateCusHAWB("119", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.Pending);
			CreateCusHAWB("120", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.Classification);
			CreateCusHAWB("121", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.Submitted);
			CreateCusHAWB("122", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.Compiling);
			CreateCusHAWB("123", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.Unknown);
			CreateCusHAWB("124", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, null);                                                                        // Finalised
			CreateCusHAWB("125", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.Chase, null);                                                                            // Finalised
			CreateCusHAWB("126", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.EIR, null);
			CreateCusHAWB("127", AutoCargoReportQueueCodeDescriptionPairList.Codes.Hold, AutoCommercialQueueCodeDescriptionPairList.Codes.Rebill, null);
			Factory.Save();
		}

		internal UPECusHAWB CreateCusHAWB(ZString houseBillId, ZString customsQueue, ZString commercialQueue, ZString declarationQueue)
		{
			return CreateCusHAWB(houseBillId, customsQueue, commercialQueue, declarationQueue, false);
		}

		UPECusHAWB CreateCusHAWB(ZString houseBillId, ZString customsQueue, ZString commercialQueue, ZString declarationQueue, bool isMessageContains)
		{
			UPECusHAWB result = (UPECusHAWB)MasterBill.ChildBills.AddNew();
			result.CS_HAWB = houseBillId;
			result.CurrentQueue.P4_CustomsQueue = customsQueue;
			result.CurrentQueue.P4_QueueName = commercialQueue;
			if (!declarationQueue.IsEmpty)
			{
				result.CS_JE_CustomsFormalEntry = Factory.New<UPEJobDeclaration>().PK;
				result.Declaration.CurrentQueue.P4_CustomsQueue = declarationQueue;
			}
			else
			{
				result.CS_GoodsValue = 0;
			}
			if (isMessageContains)
			{
				EDIMessage message = result.Declaration.CurrentQueue.FirstUPECusHAWB.Messages.AddNew();
				message.EM_MessageType = UPECargoReportQueue.EdiMessageTypes.Withdrawn;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			}
			return result;
		}
		#endregion

		#region Callout and Test Data

		internal Callout CalloutHouseBill
		{
			get
			{
				if (calloutHouseBill == null)
				{
					calloutHouseBill = (Callout)MasterBill.ChildBills.AddNew(typeof(Callout));
					calloutHouseBill.CS_RL_NKOrigin = "SGSIN";
					calloutHouseBill.CS_RL_NKDestination = "AUSYD";
				}
				return calloutHouseBill;
			}
		}
		Callout calloutHouseBill;

		#endregion

		#region SplitShipmentsTestData

		internal void Create2SplitShipmentGroups()
		{
			HouseBill.CS_HAWB = "FirstHouseBill";
			HouseBill.CS_RN_NKConsigneeCountry = Constants.CountryCodes.Australia;
			HouseBill.SetSplitShipment();

			SplitShipment1HouseBill2 = (UPECusHAWB)MasterBill.ChildBills.AddNew();
			SplitShipment1HouseBill2.CS_HAWB = "1z1234567890";
			SplitShipment1HouseBill2.CS_RN_NKConsigneeCountry = Constants.CountryCodes.Australia;
			SplitShipment1HouseBill2.SetSplitShipment();

			CreateSecondMasterBillWithHouseBill(out SplitShipment2MasterBill2, out SplitShipment2HouseBill3);
			SplitShipment2HouseBill3.CS_HAWB = "ThirdHouseBill";
			SplitShipment2HouseBill3.CS_RN_NKConsigneeCountry = Constants.CountryCodes.Australia;
			SplitShipment2HouseBill3.SetSplitShipment();

			SplitShipment3HouseBill4 = (UPECusHAWB)MasterBill.ChildBills.AddNew();
			SplitShipment3HouseBill4.CS_HAWB = "FourthHouseBill";
			SplitShipment3HouseBill4.CS_RN_NKConsigneeCountry = Constants.CountryCodes.Australia;
			SplitShipment3HouseBill4.SetSplitShipment();

			UPEJobRelatedWayBill.CreateJobRelatedWaybill(Factory, HouseBill.PK, HouseBill.CS_HAWB, string.Empty, UPEJobRelatedWayBill.Constants.RelatedWayBillType.Parent);
			UPEJobRelatedWayBill.CreateJobRelatedWaybill(Factory, HouseBill.PK, SplitShipment1HouseBill2.CS_HAWB, string.Empty, UPEJobRelatedWayBill.Constants.RelatedWayBillType.Child);
			UPEJobRelatedWayBill.CreateJobRelatedWaybill(Factory, SplitShipment2HouseBill3.PK, SplitShipment2HouseBill3.CS_HAWB, string.Empty, UPEJobRelatedWayBill.Constants.RelatedWayBillType.Parent);
			UPEJobRelatedWayBill.CreateJobRelatedWaybill(Factory, SplitShipment2HouseBill3.PK, SplitShipment3HouseBill4.CS_HAWB, string.Empty, UPEJobRelatedWayBill.Constants.RelatedWayBillType.Child);
			Factory.Save();
		}

		internal UPECusMAWB SplitShipment2MasterBill2;
		internal UPECusHAWB SplitShipment1HouseBill2;
		internal UPECusHAWB SplitShipment2HouseBill3;
		internal UPECusHAWB SplitShipment3HouseBill4;

		#endregion

		internal UPEJobDeclarationWithDummyCharges JobDeclaration
		{
			get
			{
				if (jobDeclaration == null)
				{
					jobDeclaration = Factory.New<UPEJobDeclarationWithDummyCharges>();
					jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				}
				return jobDeclaration;
			}
		}
		UPEJobDeclarationWithDummyCharges jobDeclaration;

		#region JobRelatedWayBills data

		internal void NewJobRelatedWayBillsWithValidTestData()
		{
			JobDeclaration.JE_HouseBill = "1Z1801790310013282";
			JobDeclaration.JE_RL_NKPortOfFirstArrival = "AUSYD";
			Factory.Save();  // It is important we save so the following lines will trigger a status change.
			JobDeclaration.CurrentQueue.P4_QueueName = "BCA";
			JobDeclaration.CurrentQueue.P4_CustomsStatus = AutoReasonCodeDescriptionPairList.Codes.E8_QuarantineHold;
			JobDeclaration.CurrentQueue.P4_CustomsSubStatus = AutoStatusCodeDescriptionPairList.Codes.BQ_NoAnswer;

			HouseBill.CS_HAWB = "1Z1801790310013282";
			HouseBill.CS_JE_CustomsFormalEntry = JobDeclaration.PK;
			HouseBill.CurrentQueue.P4_QueueName = "XXX";
			HouseBill.CurrentQueue.P4_Status = AutoCommercialQueueCodeDescriptionPairList.Codes.AlternateBroker;
			HouseBill.CurrentQueue.P4_SubStatus = "YYY";
			HouseBill.CurrentQueue.P4_CustomsStatus = "ZZZ";

			UPECusHAWB childHouseBill = Factory.NewWithValidTestData<UPECusHAWB>();
			childHouseBill.CS_HAWB = "AnotherHouseBill";
			childHouseBill.CS_JE_CustomsFormalEntry = JobDeclaration.PK;
			childHouseBill.CurrentQueue.P4_QueueName = "AAA";
			childHouseBill.CurrentQueue.P4_Status = AutoCommercialQueueCodeDescriptionPairList.Codes.Hold;
			childHouseBill.CurrentQueue.P4_SubStatus = "BBB";
			childHouseBill.CurrentQueue.P4_CustomsStatus = "ZZZ";

			UPEJobRelatedWayBill.CreateJobRelatedWaybill(Factory, HouseBill.PK, HouseBill.CS_HAWB, "short#", UPEJobRelatedWayBill.Constants.RelatedWayBillType.Parent);
			UPEJobRelatedWayBill.CreateJobRelatedWaybill(Factory, HouseBill.PK, childHouseBill.CS_HAWB, ZString.Empty, UPEJobRelatedWayBill.Constants.RelatedWayBillType.Child);
			UPEJobRelatedWayBill.CreateJobRelatedWaybill(Factory, HouseBill.PK, "VirtualChild", ZString.Empty, UPEJobRelatedWayBill.Constants.RelatedWayBillType.Child);
			UPEJobRelatedWayBill.CreateJobRelatedWaybill(Factory, childHouseBill.PK, childHouseBill.CS_HAWB, "short2#", UPEJobRelatedWayBill.Constants.RelatedWayBillType.Parent);
			UPEJobRelatedWayBill.CreateJobRelatedWaybill(Factory, childHouseBill.PK, "VirtualChild2", ZString.Empty, UPEJobRelatedWayBill.Constants.RelatedWayBillType.Child);
		}

		internal List<string> GssiExpectedMessages
		{
			get
			{
				if (gssiExpectedMessages == null)
				{
					gssiExpectedMessages = new List<string>(4);
					gssiExpectedMessages.Add("01ERR       7340      N73401Z1801790310013282                 03E8");  // Parent house bill.
					gssiExpectedMessages.Add("01ERR       7340      N7340AnotherHouseBill                   03E8");  // Child house bill.
					gssiExpectedMessages.Add("01ERR       7340      N7340VirtualChild                       03E8");  // JobRelatedWayBill virtual only.
					gssiExpectedMessages.Add("01ERR       7340      N7340VirtualChild2                      03E8");  // JobRelatedWayBill virtual only.
				}
				return gssiExpectedMessages;
			}
		}
		List<string> gssiExpectedMessages;

		#endregion

		#region Set Registry Items
		#region Set Valid Registry Items
		public override void SetValidRegistryAll()
		{
			SetValidBISIUploadEverydayHWM();
			SetValidRegistryDocumentImageTypes();
		}

		internal static void SetValidBISICurrentBatchNumber(int batchNumber)
		{
			Rego.BISIUploadCurrentBatchNumber = batchNumber;
		}

		internal void SetValidBISIUploadEverydayHWM()
		{
			SetValidBISIUploadEverydayHWM(ZDateTime.UtcNow.AddDays(1));
		}

		internal static void SetValidBISIUploadEverydayHWM(ZDateTime highWateMarkDateTime)
		{
			Rego.BISIUploadEverydayHWM = highWateMarkDateTime;
		}

		internal void SetValidRegistryDocumentImageTypes()
		{
			DocumentImageTypeCollection imageTypeCollection = new DocumentImageTypeCollection();
			DocumentImageType imageType = imageTypeCollection.AddNew();
			imageType.UPSCode = "03";
			imageType.Description = "Blah";
			imageType.DocTypeCode = "PUB";
			Rego.DocumentImagingImageTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, imageTypeCollection);
		}

		#endregion

		#region Set Invalid Registry Items
		#endregion
		#endregion

		#region MasterBill Test Data

		internal UPECusMAWB MasterBill
		{
			get
			{
				if (masterBill == null)
				{
					masterBill = Factory.New<UPECusMAWB>();
					masterBill.CM_MAWB = "MAWB101";
					masterBill.CM_RL_NKDischargePort = "AUSYD";
				}
				return masterBill;
			}
			set
			{
				masterBill = value;
			}
		}
		UPECusMAWB masterBill;

		internal void CreateSecondMasterBillWithHouseBill(out UPECusMAWB masterBill2, out UPECusHAWB houseBill2)
		{
			masterBill2 = Factory.New<UPECusMAWB>();
			masterBill2.CM_MAWB = "MAWB202";
			masterBill2.CM_RL_NKDischargePort = "AUSYD";
			houseBill2 = (UPECusHAWB)masterBill2.ChildBills.AddNew();
			houseBill2.CS_RL_NKOrigin = "SGSIN";
			houseBill2.CS_RL_NKDestination = "AUSYD";
		}

		#endregion

		internal NotificationBuffer Buffer
		{
			get { return buffer ?? (buffer = new NotificationBuffer()); }
		}
		NotificationBuffer buffer;

		internal TestServiceLogger Logger
		{
			get { return logger ?? (logger = new TestServiceLogger()); }
		}
		TestServiceLogger logger;

		internal static UPEDataRegistry Rego
		{
			get { return UPEDataRegistry.Instance; }
		}

		#region Test File Resources
		internal static class TestFiles
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
			internal static readonly string Folder = TestCase.BaseSourcePath + folder;
			const string folder = @"Enterprise\ClientExtensions\UPE\ZClientUPE\ZClientUPE.Test\";

			internal static byte[] CommercialInvoiceBmpBytes
			{
				get
				{
					using (var resourceRetriever = new EmbeddedResourceRetriever())
					{
						return resourceRetriever.GetBytes("Enterprise.Client.UPE.Testing.Business.DocWrappers.TestFiles.CommercialInvoice.bmp");
					}
				}
			}

			internal static class DataImport
			{
				internal static readonly string ExportCustomsManifestFolder = TestFiles.Folder + exportCustomsManifestFolder;
				const string exportCustomsManifestFolder = @"Business\ExportCustomsManifest\DataImport\TestFiles\";
			}

			internal static class DocumentImaging
			{
				internal static readonly string Folder = TestFiles.Folder + folder;
				const string folder = @"Business\DocumentImaging\TestFiles\";
			}

			internal static class BISI
			{
				static readonly string Folder = TestFiles.Folder + folder;
				const string folder = @"Business\BISI\";

				internal static class Export
				{
					internal static readonly string Folder = TestFiles.BISI.Folder + folder;
					const string folder = @"Export\Testing\";
				}

				internal static class Import
				{
					internal static readonly string Folder = TestFiles.BISI.Folder + folder;
					const string folder = @"Import\Testing\";
				}
			}

			internal static class Rating
			{
				internal static readonly string Folder = TestFiles.Folder + folder;
				const string folder = @"Business\Rating\TestFiles\";
			}
		}
		#endregion

		#region Test File Resources
		internal static class TestResource
		{
			internal static class DataImport
			{
				internal static readonly string Path = "Enterprise.Client.UPE.Testing.Business.DataImport.TestFiles";
			}

			public static string ExtractToFile(string embeddedResourcePath, string outputFilePath, string fileName)
			{
				string outputFileFullPath = System.IO.Path.Combine(outputFilePath, fileName);
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResourcePath + "." + fileName))
				using (var writer = File.Create(outputFileFullPath))
				{
					stream.Seek(0, SeekOrigin.Begin);
					stream.CopyTo(writer);
				}
				return outputFileFullPath;
			}
		}
		#endregion

		#region IO Stuff
		/// <summary>
		/// Delete all files for a given folder.
		/// </summary>
		/// <param name="folder">Folder from which all files will be deleted.</param>
		internal static void DeleteFiles(string folderName)
		{
			DeleteFiles(folderName, "*");
		}

		/// <summary>
		/// Delete all files for a given folder based on search pattern.
		/// </summary>
		/// <param name="folder">Folder from which either none, some or all files will be deleted.</param>
		/// <param name="searchPattern">Standard IO wild card search pattern.</param>
		internal static void DeleteFiles(string folderName, string searchPattern)
		{
			DeleteFilesAndOrFolder(folderName, searchPattern, false);
		}

		static void DeleteFilesAndOrFolder(string folderName, string searchPattern, bool deleteFolder)
		{
			if (Directory.Exists(folderName))
			{
				foreach (string file in Directory.GetFiles(folderName, searchPattern))
				{
					File.Delete(file);
				}
				if (deleteFolder)
				{
					Directory.Delete(folderName);
				}
			}
		}

		internal static void CreateEmptyDirectory(string folderName)
		{
			DirectoryInfo folder = new DirectoryInfo(folderName);
			if (folder.Exists)
			{
				DeleteFiles(folderName);
			}
			else
			{
				folder.Create();
			}
		}

		internal static void DeleteDirectory(string folderName)
		{
			DeleteFilesAndOrFolder(folderName, "*", true);
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		internal static void Delay(int seconds)
		{
			Thread.Sleep(0);
			Thread.Sleep(seconds * 1000);
			Thread.Sleep(0);
		}

		internal ZQuery CreateExportLogFilter(ZGuid parentPK, ZString shipmentStatus, ZString reasonCode, ZString trackingID, string triggeredBy = "")
		{
			var filter = new ZQuery(StmALogSchema.SL_Parent, parentPK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode);
			if (string.IsNullOrEmpty(triggeredBy))
			{
				filter.AddToFilter(StmALogSchema.SL_Reference, trackingID + "|" + shipmentStatus + "|" + reasonCode);
			}
			else
			{
				filter.AddToFilter(StmALogSchema.SL_Reference, trackingID + "|" + shipmentStatus + "|" + reasonCode + "|" + triggeredBy);
			}

			return filter;
		}
		internal void AssertContainsResolutionExportLog(ZGuid parentPK, BusinessObjectFactory factory, ZString reasonCode, ZString trackingID)
		{
			var resolutionExportLogs = factory.LoadTop1<StmALog>(CreateExportLogFilter(parentPK, "04", reasonCode, trackingID));
			Assertion.AssertNotNull(resolutionExportLogs);
		}

		internal void AssertContainsHoldExportLog(ZGuid parentPK, BusinessObjectFactory factory, ZString reasonCode, ZString trackingID, string triggeredBy = "")
		{
			var holdExportLogs = factory.LoadTop1<StmALog>(CreateExportLogFilter(parentPK, "03", reasonCode, trackingID, triggeredBy));
			Assertion.AssertNotNull(holdExportLogs);
		}

		#region Level1DataImport
		internal Level1DataImport GetNewLevel1DataImport(ZString flightNumber, ZDateTime arrivalDate, ZString portOfLoading, ZString portOfDischarge, ZString masterBill, ZString fileName)
		{
			return new Level1DataImport(Factory)
			{
				FlightNumber = flightNumber,
				ArrivalDate = arrivalDate,
				PortOfLoading = portOfLoading,
				PortOfDischarge = portOfDischarge,
				MasterBill = masterBill,
				FileName = fileName,
				SurplusIndicatedNote = "SurplusIndicatedNote Notes",
				FlightNotInScheduleNote = "FlightNotInScheduleNote Notes",
				DuplicateHAWBsNote = "DuplicateHAWB Notes",
				MasterbillWarningNote = "MasterbillWarning Note",
				ArrivalDateWarningNote = "ArrivalDateWarning Note",
				UnmatchedFilenameNote = "UnmatchedFilename Note"
			};
		}

		internal Level1DataFileImporterForAU GetNewLevel1DataImporterForAU(Level1DataImport importBizo) => new Level1DataFileImporterForAU(importBizo, Buffer);
		#endregion

		internal static void TaxOrFeeTestSetUp(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateTaxOrFee("DEM", 400m, "SG", new ZDateTime(2010, 1, 1), new ZDateTime(2079, 6, 6), "Low Value Threshold");
			factory.Save();
		}
	}
}
