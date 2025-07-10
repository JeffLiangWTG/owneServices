using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.ClientSharedComponents.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Registry.Testing
{
	[TestedType(typeof(UPEDataRegistry))]
	sealed class UPEDataRegistryTest : RegistryItemSetTestCaseWithFactory<UPEDataRegistry>
	{
		public void TestCountryPksExcludingAustraliaAreCached()
		{
			var countryFilterPks = ItemSet.AllowMultipleLevel1LoadsForMasterBillItem.CountryFilterPKs;
			AssertSame("Country PKs are cached", countryFilterPks, ItemSet.AllowMultipleLevel1LoadsForMasterBillItem.CountryFilterPKs);
			AssertNotNull("Cache holds an evaluated Query", (Guid[])countryFilterPks);
		}

		#region UserVisibleRegistryItems

		public void TestUserVisibleRegistryItems()
		{
			var auCompany = Factory.NewWithValidTestData<GlbCompany>();
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Factory.Save();

			AssertEquals("AllItems.Count", 126, AllItems.Count);

			AssertBISIRegistryItemsInCollection();
			AssertEntryPrintRegistryItemsInCollection();
			AssertCODRegistryItemsInCollection();
			AssertChargesRegistryItemsInCollection();
			AssertBatchPrintingRegistryItemsInCollection();
			AssertTaxInvoiceBatchPrintingItemsInCollection();
			AssertScreeningItemsInCollection();
			AssertProcessQueueProcessorItemsInCollection();
			AssertDocumentImagingItemsInCollection();
			AssertSMSItemsInCollection();
			AssertGSSiRegistryItemsInCollection();

			AssertVisible(ItemSet.ClassifierStaffGroupCode);
			AssertVisible(ItemSet.DogHitXRayNotificationGroupItem);
			AssertVisible(ItemSet.MatchingActivitiesHWMItem);
			AssertVisible(ItemSet.PartPaymentNotificationGroupItem);
			AssertVisible(ItemSet.RefundNotificationGroupItem);
			AssertVisible(ItemSet.MAWBFirstMovedToClassificationNotificationGroupItem);
			AssertVisible(ItemSet.ManualbillNotificationGroupItem);
			AssertVisible(ItemSet.CusHAWBAutoQueueMovementRegistryItem);
			AssertVisible(ItemSet.AtFaultGroupsItem);
			AssertVisible(ItemSet.CreditNotificationGroupItem);
			AssertVisible(ItemSet.EnableUPECustomisationsItem);
			AssertVisible(ItemSet.CODManifestReportZoneRelatedPartyItem);
			AssertVisible(ItemSet.FlightNumbersForUploadOnDayOfArrivalPlusOneItem);
			AssertVisible(ItemSet.DashboardRefreshCycleTimeInMinutes);
			AssertVisible(ItemSet.SftpServerTimeoutItem);
			AssertVisible(ItemSet.SftpServerTimeoutItemNotificationGroupItem);

			AssertScreening();
			AssertSGScreening();
		}

		void AssertScreening()
		{
			TestGenericRegistryItem(ItemSet.EnableAutoPopulateCycleDetailsToImportGlobalManifestBillsItem,
				"EnableAutoPopulateCycleDetailsToImportGlobalManifestBills",
				ExpectedUPSSGScreeningCategory,
				"Auto Populate Cycle details to Import Global Manifest Bills",
				"Enable the auto population of cycle details entered on the Level 1 screen against low value non TradeNet screened shipments.",
				RegistryStorageFlags.Company,
				true
			);
			Assert(ItemSet.EnableAutoPopulateCycleDetailsToImportGlobalManifestBillsItem.CountryFilterPKs.Any());
			AssertContainsExactElementsInAnyOrder(RegistryItemSet.CountryFilterPKs.Singapore, ItemSet.EnableAutoPopulateCycleDetailsToImportGlobalManifestBillsItem.CountryFilterPKs);

			TestGenericRegistryItem(ItemSet.AllowMultipleLevel1LoadsForMasterBillItem,
				"AllowMultipleLevel1LoadsForMasterBill",
				ExpectedUPSScreeningCategory,
				"Allow Multiple Level 1 Loads For Master Bill",
				"Allows the Level 1 Data import to load multiple level 1 files onto an existing Master Bill",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForController,
				true
			);
			AssertCountryFilter(ItemSet.AllowMultipleLevel1LoadsForMasterBillItem);

			TestGenericRegistryItem(ItemSet.AllowBillUpdatesDuringMulitpleLevel1LoadsItem,
				"AllowBillUpdatesDuringMulitpleLevel1Loads",
				ExpectedUPSScreeningCategory,
				"Allow Bills to be Updated for Multiple Level 1 Loads",
				"Allows the Level 1 Data import to update Bills for an existing Master Bill when the Master Bill is used for multiple loads",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForController,
				false
			);
			AssertCountryFilter(ItemSet.AllowBillUpdatesDuringMulitpleLevel1LoadsItem);

			TestGenericRegistryItem(ItemSet.ShipmentReferenceNumberRecyclePeriodItem,
				"ShipmentReferenceNumberRecyclePeriod",
				ExpectedUPSScreeningCategory,
				"Shipment Reference Number Recycle Window in months",
				"The number of months after which a Shipment Reference Number may be re-used",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForController,
				12
			);
			AssertCountryFilter(ItemSet.ShipmentReferenceNumberRecyclePeriodItem);

			TestGenericRegistryItem(ItemSet.StopImportOfBillIfMatchingBillFoundItem,
				"StopImportOfBillIfMatchingBillFound",
				ExpectedUPSScreeningCategory,
				"Stops the import of the Bill if a matching Bill is found",
				"Enables the checking of Bills in the system against the Shipment Reference Number",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForController,
				true
			);
			AssertCountryFilter(ItemSet.StopImportOfBillIfMatchingBillFoundItem);

			TestGenericRegistryItem(ItemSet.EnableFreightAutoRatingInLevelOneImportItem,
				"EnableFreightAutoRatingInLevelOneImport",
				ExpectedUPSScreeningCategory,
				(NoResString)"Enable Freight Auto Rating in Level 1 Import",
				(NoResString)"Allows the Level 1 Data import to calculate freight and insurance value based on freight auto-rating system.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForController,
				true
			);
			AssertCountryFilter(ItemSet.EnableFreightAutoRatingInLevelOneImportItem);

			TestGenericRegistryItem(ItemSet.EnableDecisionSupportImportShipmentsItem,
				"EnableDecisionSupportImportShipmentsItem",
				ExpectedUPSScreeningCategory,
				"Enable Decision Support for Import Shipments",
				"Enables the Level 1 Import Decision Provider to determine if Global Manifest and Customs shipments are created for the imported level 1 file",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForController,
				true
			);
			AssertCountryFilter(ItemSet.EnableDecisionSupportImportShipmentsItem);

			TestGenericRegistryItem(ItemSet.EnableDecisionSupportExportShipmentsItem,
				"EnableDecisionSupportExportShipmentsItem",
				ExpectedUPSScreeningCategory,
				"Enable Decision Support for Export Shipments",
				"Enables the Level 1 Import Decision Provider to determine if Global Manifest and Customs shipments are created for the imported level 1 file",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForController,
				true
			);
			AssertCountryFilter(ItemSet.EnableDecisionSupportExportShipmentsItem);
		}

		void AssertCountryFilter(RegistryItemWrapper item)
		{
			Assert("Country Filter missing for " + item.Name,
					ItemSet.EnableDecisionSupportExportShipmentsItem.CountryFilterPKs.Any());
			AssertCollectionNotContains("AU Companies should be filtered from " + item.Name,
				Core.Constants.CountryGuids.Australia,
				ItemSet.EnableDecisionSupportExportShipmentsItem.CountryFilterPKs);
			AssertCollectionContains("SG Companies should be allowed for " + item.Name,
				Core.Constants.CountryGuids.Singapore,
				ItemSet.EnableDecisionSupportExportShipmentsItem.CountryFilterPKs);
		}

		void AssertSGScreening()
		{
			CombineAssertions(() =>
			{
				TestGenericRegistryItem(ItemSet.StopPhrasesForSGGoodsDescriptionItem,
					"UPEStopPhrasesForSGGoodsDescription",
					ExpectedUPSSGScreeningCategory,
					"Goods Description Stop Words",
					"Stop words or phrases that when identified in the 'Packline Goods Description' of a global manifest, will move the job to TradeNet",
					RegistryStorageFlags.System
					);
				AssertEquals(TextEditorType.Memo, ((TextRegistryEditorInfo)ItemSet.StopPhrasesForSGGoodsDescriptionItem.EditorInfo).EditorType);

				var defaultValue = new StringWriter();
				defaultValue.WriteLine("48000 to 52000");
				defaultValue.WriteLine("81000 to 81999");
				TestGenericRegistryItem(ItemSet.StopPostcodeRangesForSGFreeTradeZonesItem,
					"UPEStopPostcodeRangesForSGFreeTradeZones",
					ExpectedUPSSGScreeningCategory,
					"Postcode Ranges for Free Trade Zone filtering",
					"Stop words or phrases that when identified in the 'Consignee Postcode' of a global manifest, will move the job to TradeNet",
					RegistryStorageFlags.System,
					defaultValue.GetStringBuilder().ToString()
				);
				AssertEquals(TextEditorType.Memo, ((TextRegistryEditorInfo)ItemSet.StopPostcodeRangesForSGFreeTradeZonesItem.EditorInfo).EditorType);

				defaultValue = new StringWriter();
				defaultValue.WriteLine("FOB");
				TestGenericRegistryItem(ItemSet.SGFallBackIncotermForLevel1Item,
					"UPESGFallBackIncotermForLevel1",
					ExpectedUPSSGScreeningCategory,
					"Fallback Incoterm for Level 1",
					"Default Incoterm to be used when the billing term is missing from the Level 1 file being imported",
					RegistryStorageFlags.System,
					defaultValue.GetStringBuilder().ToString()
				);
				AssertEquals(TextEditorType.Memo, ((TextRegistryEditorInfo)ItemSet.SGFallBackIncotermForLevel1Item.EditorInfo).EditorType);

				TestGenericRegistryItem(ItemSet.StopPhrasesForSGConsigneeAccountNumItem,
					"UPEStopPhrasesForSGConsigneeAccountItem",
					ExpectedUPSSGScreeningCategory,
					"Filter Consignee Account #",
					"Stop words or phrases that when identified in the 'Consignee Account #' of a global manifest, will move the job to TradeNet",
					RegistryStorageFlags.System
				);
				AssertEquals(TextEditorType.Memo, ((TextRegistryEditorInfo)ItemSet.StopPhrasesForSGConsigneeAccountNumItem.EditorInfo).EditorType);

				TestGenericRegistryItem(ItemSet.StopPhrasesForSGConsigneeAddressItem,
					"UPEStopPhrasesForSGConsigneeAddressItem",
					ExpectedUPSSGScreeningCategory,
					"Filter Consignee Address",
					"Stop words or phrases that when identified in the 'Consignee Address' of a global manifest, will move the job to TradeNet",
					RegistryStorageFlags.System
				);
				AssertEquals(TextEditorType.Memo, ((TextRegistryEditorInfo)ItemSet.StopPhrasesForSGConsigneeAddressItem.EditorInfo).EditorType);

				TestGenericRegistryItem(ItemSet.StopPhrasesForSGConsigneeNameItem,
					"UPEStopPhrasesForSGConsigneeNameItem",
					ExpectedUPSSGScreeningCategory,
					"Filter Consignee Name",
					"Stop words or phrases that when identified in the 'Consignee Name' of a global manifest, will move the job to TradeNet",
					RegistryStorageFlags.System
				);
				AssertEquals(TextEditorType.Memo, ((TextRegistryEditorInfo)ItemSet.StopPhrasesForSGConsigneeNameItem.EditorInfo).EditorType);

				TestGenericRegistryItem(ItemSet.StopPhrasesForSGConsignorAccountNumItem,
					"UPEStopPhrasesForSGConsignorAccountItem",
					ExpectedUPSSGScreeningCategory,
					"Filter Consignor Account #",
					"Stop words or phrases that when identified in the 'Consignor Account #' of a global manifest, will move the job to TradeNet",
					RegistryStorageFlags.System
				);
				AssertEquals(TextEditorType.Memo, ((TextRegistryEditorInfo)ItemSet.StopPhrasesForSGConsignorAccountNumItem.EditorInfo).EditorType);

				TestGenericRegistryItem(ItemSet.StopPhrasesForSGConsignorAddressItem,
					"UPEStopPhrasesForSGConsignorAddressItem",
					ExpectedUPSSGScreeningCategory,
					"Filter Consignor Address",
					"Stop words or phrases that when identified in the 'Consignor Address' of a global manifest, will move the job to TradeNet",
					RegistryStorageFlags.System
				);
				AssertEquals(TextEditorType.Memo, ((TextRegistryEditorInfo)ItemSet.StopPhrasesForSGConsignorAddressItem.EditorInfo).EditorType);

				TestGenericRegistryItem(ItemSet.StopPhrasesForSGConsignorNameItem,
					"UPEStopPhrasesForSGConsignorNameItem",
					ExpectedUPSSGScreeningCategory,
					"Filter Consignor Name",
					"Stop words or phrases that when identified in the 'Consignor Name' of a global manifest, will move the job to TradeNet",
					RegistryStorageFlags.System
				);
				AssertEquals(TextEditorType.Memo, ((TextRegistryEditorInfo)ItemSet.StopPhrasesForSGConsignorNameItem.EditorInfo).EditorType);

				TestGenericRegistryItem(ItemSet.FilterSGTranshipmentsItem,
					"FilterSGTranshipmentsItem",
					ExpectedUPSSGScreeningCategory,
					"Filter Transhipments",
					"Stops Level 1 shipments that are transhipments being imported",
					RegistryStorageFlags.System,
					true
				);
			});
		}

		void AssertGSSiRegistryItemsInCollection()
		{
			AssertVisible(ItemSet.BranchCodeIDs);
			AssertVisible(ItemSet.DefaultBuildingIDItem);
		}

		void AssertBISIRegistryItemsInCollection()
		{
			AssertVisible(ItemSet.XPLDForNonWorkingDaysDateOfArrivalPassedItem);
			AssertVisible(ItemSet.XPLDForWorkingDaysItem);
			AssertVisible(ItemSet.XPLDForWorkingDaysDateOfArrivalPassedItem);
			AssertVisible(ItemSet.BISISftpServerAddressItem);
			AssertVisible(ItemSet.BISISftpServerPortItem);
			AssertVisible(ItemSet.BISISftpServerUsernameItem);
			AssertVisible(ItemSet.BISISftpServerPasswordItem);
			AssertVisible(ItemSet.BranchToUseForUPECustomisationsItem);
			AssertVisible(ItemSet.BISIUploadDirectoryItem);
			AssertVisible(ItemSet.BISIUploadFilenameItem);
			AssertVisible(ItemSet.BISIUploadCompletedHWMItem);
			AssertVisible(ItemSet.BISIUploadEverydayHWMItem);
			AssertVisible(ItemSet.BISIUploadEverydayDateOfArrivalHWMItem);
			AssertVisible(ItemSet.BISIUploadWorkingDayMetroHWMItem);
			AssertVisible(ItemSet.BISIUploadWorkingDayOtherHWMItem);
			AssertVisible(ItemSet.BISIUploadWorkingDayDateOfArrivalMetroHWMItem);
			AssertVisible(ItemSet.BISIUploadWorkingDayDateOfArrivalOtherHWMItem);
			AssertVisible(ItemSet.CODFilesParentFolderItem);
			AssertVisible(ItemSet.BISIUploadCurrentBatchNumberItem);
			AssertVisible(ItemSet.BISIUploadArchiveDirectoryItem);
			AssertVisible(ItemSet.BISIDownloadArchiveDirectoryItem);
			AssertVisible(ItemSet.BISIUploadTimeBufferItem);
			AssertVisible(ItemSet.WarningReportNotificationGroupItem);
			AssertVisible(ItemSet.InterchangeReportNotificationGroupItem);
			AssertVisible(ItemSet.BISIUploadWarningHWMItem);
			AssertVisible(ItemSet.BillingNotificationGroupItem);
			AssertVisible(ItemSet.ForceAllXPLDsToBeUploadedEveryTimeItem);
			AssertVisible(ItemSet.BISIUploadWorkingDayMetroExportTimeItem);
			AssertVisible(ItemSet.BISIUploadWorkingDayOtherExportTimeItem);
			AssertVisible(ItemSet.BISIUploadWorkingDayMetroAndOtherExportTimeItem);
			AssertVisible(ItemSet.BISIUploadLoadProcessQueueLogBatchSizeItem);
			AssertVisible(ItemSet.BISIOBCTaxCertificateNumberItem);
			AssertVisible(ItemSet.BISIUploadNoOfRecordsPerSaveItem);
		}

		void AssertEntryPrintRegistryItemsInCollection()
		{
			AssertVisible(ItemSet.EntryPrintSftpServerAddressItem);
			AssertVisible(ItemSet.EntryPrintSftpServerPasswordItem);
			AssertVisible(ItemSet.EntryPrintSftpServerPortItem);
			AssertVisible(ItemSet.EntryPrintSftpServerUploadDirectoryItem);
			AssertVisible(ItemSet.EntryPrintSftpServerUploadFilenameItem);
			AssertVisible(ItemSet.EntryPrintSftpArchiveDirectoryItem);
			AssertVisible(ItemSet.EntryPrintSftpArchiveDirectoryItem);
			AssertVisible(ItemSet.EntryPrintSftpServerUsernameItem);
			AssertVisible(ItemSet.EntryPrintMaximumDeclarationsToSendPerDayItem);
		}

		void AssertCODRegistryItemsInCollection()
		{
			AssertVisible(ItemSet.CODConfirmPaymentThresholdItem);
			AssertVisible(ItemSet.CODAutoReleaseAtUploadThresholdItem);
			AssertVisible(ItemSet.CODAutoReleaseAndChaseThresholdItem);
		}

		void AssertChargesRegistryItemsInCollection()
		{
			AssertVisible(ItemSet.SecurityFeeAmount);
			AssertVisible(ItemSet.TerminalFeeAmount);
			AssertVisible(ItemSet.AlternateBrokerStorageFeePerDay);
		}

		void AssertBatchPrintingRegistryItemsInCollection()
		{
			AssertVisible(ItemSet.PrintBatchMaxCount);
			AssertVisible(ItemSet.DocumentAutoDeliveryNotificationGroup);
			AssertVisible(ItemSet.UPSContactEmail);
			AssertVisible(ItemSet.UPSContactFax);
		}

		void AssertTaxInvoiceBatchPrintingItemsInCollection()
		{
			AssertVisible(ItemSet.TaxInvoiceCommentsText1);
			AssertVisible(ItemSet.TaxInvoiceCommentsText2);
			AssertVisible(ItemSet.TaxInvoiceFooterText);
			AssertVisible(ItemSet.TaxInvoiceImage);
		}

		void AssertScreeningItemsInCollection()
		{
			AssertVisible(GetStringRegistryItem("StopPhrasesForConsignorNameItem"));
			AssertVisible(GetStringRegistryItem("StopPhrasesForConsignorAccountNumItem"));
			AssertVisible(GetStringRegistryItem("StopPhrasesForConsignorAddressItem"));
			AssertVisible(GetStringRegistryItem("StopPhrasesForConsigneeNameItem"));
			AssertVisible(GetStringRegistryItem("StopPhrasesForConsigneeAccountNumItem"));
			AssertVisible(GetStringRegistryItem("StopPhrasesForConsigneeAddressItem"));
			AssertVisible(GetStringRegistryItem("StopPhrasesForGoodsDescriptionItem"));
			AssertVisible(ItemSet.NonDocumentScreeningValueRangesItem);
			AssertVisible(GetStringRegistryItem("QuarantineStopPhrasesForGoodsDescriptionItem"));
		}

		void AssertProcessQueueProcessorItemsInCollection()
		{
			AssertVisible(ItemSet.ProcessQueueProcessorHWMItem);
			AssertVisible(ItemSet.ProcessQueueProcessorTimeBufferItem);
			AssertVisible(ItemSet.ProcessQueueProcessorRunTimeOffSetItem);
		}

		void AssertDocumentImagingItemsInCollection()
		{
			AssertVisible(ItemSet.DocumentImagingRepositoryItem);
			AssertVisible(ItemSet.DocumentImagingImageTypes);
			AssertVisible(ItemSet.DocumentImagingNotificationsCache, true);
			AssertVisible(ItemSet.DocumentImagingNotificationGroup);
		}

		void AssertSMSItemsInCollection()
		{
			AssertVisible(ItemSet.SMSEmailAddressSuffix);
			AssertVisible(ItemSet.SMSNotificationGroup);
			AssertVisible(ItemSet.SMSShipmentLoadDeadlineBeforeArrivalInMinutes);
		}

		#endregion

		public void TestChaseQueueValidationRegistryItem()
		{
			ChaseQueueValidationCollection value = new ChaseQueueValidationCollection();
			ChaseQueueValidation validation = value.AddNew();
			validation.DayOfTheWeek = "MONDAY";
			validation.TimeFrom = ZDateTime.UtcNow;
			ItemSet.ChaseQueueValidationRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			AssertEquals(1, ItemSet.ChaseQueueValidationRegistryItem.Value.Count);
			AssertEquals("MONDAY", ItemSet.ChaseQueueValidationRegistryItem.Value[0].DayOfTheWeek);
		}

		public void TestChaseQueueValidationServiceLevelRegistryItem()
		{
			ServiceLevelRegistryBusinessObjectCollection value = new ServiceLevelRegistryBusinessObjectCollection();
			value.AddNew().ServiceLevel = "DEF";
			ItemSet.ChaseQueueValidationServiceLevelRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			AssertEquals(1, ItemSet.ChaseQueueValidationServiceLevelRegistryItem.Value.Count);
			AssertEquals("DEF", ItemSet.ChaseQueueValidationServiceLevelRegistryItem.Value[0].ServiceLevel);
		}

		public void TestChaseQueueValidationTotalLocalChargesThreshold()
		{
			ItemSet.ChaseQueueValidationTotalLocalChargesThreshold = 13.50m;
			AssertEquals(13.50m, ItemSet.ChaseQueueValidationTotalLocalChargesThreshold);
		}

		public void TestDogHitXRayNotificationGroupItem()
		{
			AssertEquals("DogHitXRayNotificationGroup", ItemSet.DogHitXRayNotificationGroupItem.Name);
			AssertEquals("Dog Hit or X-Ray Notification Group", ItemSet.DogHitXRayNotificationGroupItem.Caption);
			AssertEquals("The staff group code for staff that will receive Dog Hit or X-Ray Hold warning emails", ItemSet.DogHitXRayNotificationGroupItem.Hint);
			AssertEquals(ExpectedCategory, ItemSet.DogHitXRayNotificationGroupItem.Category);

			AssertEquals(ExpectedDefaultNotificationGroupPK, ItemSet.DogHitXRayNotificationGroupItem.DefaultValue);
			AssertEquals(ExpectedDefaultNotificationGroupPK, ItemSet.DogHitXRayNotificationGroup);
		}

		public void TestPartPaymentNotificationGroupItem()
		{
			AssertEquals("PartPaymentNotificationGroup", ItemSet.PartPaymentNotificationGroupItem.Name);
			AssertEquals("Part Payment Notification Group", ItemSet.PartPaymentNotificationGroupItem.Caption);
			AssertEquals("The staff group code for staff that will receive part payment emails", ItemSet.PartPaymentNotificationGroupItem.Hint);
			AssertEquals(ExpectedCategory, ItemSet.PartPaymentNotificationGroupItem.Category);

			Guid notificationGroupPK = new Guid("55896E13-12BE-4FD4-AC94-2956795A5BE2");
			AssertEquals(notificationGroupPK, ItemSet.PartPaymentNotificationGroupItem.DefaultValue);
			AssertEquals(notificationGroupPK, ItemSet.PartPaymentNotificationGroup);
		}

		public void TestRefundNotificationGroupItem()
		{
			AssertEquals("RefundNotificationGroup", ItemSet.RefundNotificationGroupItem.Name);
			AssertEquals("Refund Notification Group", ItemSet.RefundNotificationGroupItem.Caption);
			AssertEquals("The staff group code for staff that will receive refund emails", ItemSet.RefundNotificationGroupItem.Hint);
			AssertEquals(ExpectedCategory + "/Refund Enquiry", ItemSet.RefundNotificationGroupItem.Category);

			Guid notificationGroupPK = new Guid("55896E13-12BE-4FD4-AC94-2956795A5BE2");
			AssertEquals(notificationGroupPK, ItemSet.RefundNotificationGroupItem.DefaultValue);
			AssertEquals(notificationGroupPK, ItemSet.RefundNotificationGroup);
		}

		public void TestAtFaultGroups()
		{
			AssertEquals("AtFaultGroups", ItemSet.AtFaultGroupsItem.Name);
			AssertEquals("At Fault Groups to Filter", ItemSet.AtFaultGroupsItem.Caption);
			AssertEquals("At Fault Groups to Filter", ItemSet.AtFaultGroupsItem.Hint);
			string refundEnquiryCategory = ExpectedCategory + @"/Refund Enquiry";
			AssertEquals(refundEnquiryCategory, ItemSet.AtFaultGroupsItem.Category);

			AssertEquals(1, ItemSet.AtFaultGroupsItem.DefaultValue.Count);
			AssertEquals(Core.Constants.Groups.AllPK, ItemSet.AtFaultGroupsItem.Value[0].Group.ToGuid());
		}

		public void TestCreditNotificationGroupItem()
		{
			AssertEquals("CreditNotificationGroup", ItemSet.CreditNotificationGroupItem.Name);
			AssertEquals("Credit Notification Group", ItemSet.CreditNotificationGroupItem.Caption);
			AssertEquals("The staff group code for staff that will receive Electronic Credit Note, Entry Print, Tax Invoice, Commercial Invoice", ItemSet.CreditNotificationGroupItem.Hint);
			AssertEquals(ExpectedCategory, ItemSet.CreditNotificationGroupItem.Category);

			Guid notificationGroupPK = new Guid("55896E13-12BE-4FD4-AC94-2956795A5BE2");
			AssertEquals(notificationGroupPK, ItemSet.CreditNotificationGroupItem.DefaultValue);
			AssertEquals(notificationGroupPK, ItemSet.CreditNotificationGroup);
		}

		public void TestMAWBFirstMovedToClassificationNotificationGroupItem()
		{
			AssertEquals("MAWBFirstMovedToClassificationNotificationGroup", ItemSet.MAWBFirstMovedToClassificationNotificationGroupItem.Name);
			AssertEquals("MAWB First Moved To CLS Notification Group", ItemSet.MAWBFirstMovedToClassificationNotificationGroupItem.Caption);
			AssertEquals("The staff group code for staff that will receive MAWB moved to CLS queue emails", ItemSet.MAWBFirstMovedToClassificationNotificationGroupItem.Hint);
			AssertEquals(ExpectedCategory, ItemSet.MAWBFirstMovedToClassificationNotificationGroupItem.Category);

			Guid notificationGroupPK = new Guid("55896E13-12BE-4FD4-AC94-2956795A5BE2");
			AssertEquals(notificationGroupPK, ItemSet.MAWBFirstMovedToClassificationNotificationGroupItem.DefaultValue);
			AssertEquals(notificationGroupPK, ItemSet.MAWBFirstMovedToClassificationNotificationGroup);
		}

		public void TestManualbillNotificationGroupItem()
		{
			AssertEquals("ManualbillNotificationGroup", UPEDataRegistry.Instance.ManualbillNotificationGroupItem.Name);
			AssertEquals("Manual Bill Notification Group", UPEDataRegistry.Instance.ManualbillNotificationGroupItem.Caption);
			AssertEquals("The staff group code for staff that will receive Manual Bill request notification emails", UPEDataRegistry.Instance.ManualbillNotificationGroupItem.Hint);
			AssertEquals(ExpectedCategory, UPEDataRegistry.Instance.ManualbillNotificationGroupItem.Category);
			AssertEquals(RegistryStorageFlags.System, UPEDataRegistry.Instance.ManualbillNotificationGroupItem.Storage);
			AssertEquals(RegistryOptions.IsValueMandatory, UPEDataRegistry.Instance.ManualbillNotificationGroupItem.Options);
			AssertEquals(ExpectedDefaultBISIWarningReportGroupPK, UPEDataRegistry.Instance.ManualbillNotificationGroupItem.DefaultValue);

			Guid manualbillNotificationPK = new Guid("22A9A7A2-DD01-4cc0-AFE6-3AF3081B5F7A");
			UPEDataRegistry.Instance.ManualbillNotificationGroup = manualbillNotificationPK;
			AssertEquals(manualbillNotificationPK, UPEDataRegistry.Instance.ManualbillNotificationGroup);
		}

		#region GSSi

		#region Branch Code IDs
		public void TestBranchCodeIDs()
		{
			AssertEquals("BranchCodeIDs", ItemSet.BranchCodeIDs.Name);
			AssertEquals("Branch Code IDs", ItemSet.BranchCodeIDs.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.BranchCodeIDs.Storage);
			AssertEquals(string.Empty, ItemSet.BranchCodeIDs.Hint);
			AssertEquals(1, ItemSet.BranchCodeIDs.Categories.Length);
			AssertEquals(ExpectedGSSiCategory, ItemSet.BranchCodeIDs.Categories[0]);
		}

		public void TestGetBuildingIDByBranch()
		{
			UPEBranchIDsRegistryObjectCollection branchIDColl = new UPEBranchIDsRegistryObjectCollection();
			UPEBranchIDsRegistryObject branchID = branchIDColl.AddNew();
			branchID.FirstArrivalPort = GlbBranch.CurrentBranch.HomePort.PK;
			branchID.BuildingID = "AUAUSYD";
			ItemSet.BranchCodeIDs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, branchIDColl);
			AssertEquals(ZString.Empty, UPEDataRegistry.Instance.GetBuildingIDByPortOfArrival(new ZGuid()));
			AssertEquals("AUAUSYD", UPEDataRegistry.Instance.GetBuildingIDByPortOfArrival(GlbBranch.CurrentBranch.HomePort.PK));
		}
		#endregion

		#region Default Building ID

		public void TestDefaultBuildingIDItem()
		{
			AssertEquals("", ItemSet.DefaultBuildingIDItem.Value);
			ItemSet.DefaultBuildingIDItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test123");
			AssertEquals("Test123", ItemSet.DefaultBuildingIDItem.Value);
		}

		#endregion

		const string ExpectedGSSiCategory = "UPS Client Extensions/GSSi";

		#endregion

		#region BISI

		#region Common

		#region BISIFtpServerAddress

		public void TestBISIFtpServerAddress()
		{
			ZString initialValue = ItemSet.BISISftpServerAddress;
			try
			{
				ItemSet.BISISftpServerAddress = "TestServer";
				AssertEquals("TestServer", ItemSet.BISISftpServerAddress);
			}
			finally
			{
				ItemSet.BISISftpServerAddress = initialValue;
			}
		}

		public void TestBISIFtpServerAddressItem()
		{
			AssertEquals("BISIFtpServerAddress", ItemSet.BISISftpServerAddressItem.Name);
			AssertEquals("SFTP Server Address", ItemSet.BISISftpServerAddressItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.BISISftpServerAddressItem.Storage);
			AssertEquals(ExpectedBISICategory, ItemSet.BISISftpServerAddressItem.Category);
			AssertEquals("10.162.32.93", ItemSet.BISISftpServerAddressItem.DefaultValue);
		}
		#endregion

		public void TestBISIFtpServerPortItem()
		{
			AssertEquals("BISIFtpServerPort", ItemSet.BISISftpServerPortItem.Name);
			AssertEquals("SFTP Server Port", ItemSet.BISISftpServerPortItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.BISISftpServerPortItem.Storage);
			AssertEquals(ExpectedBISICategory, ItemSet.BISISftpServerPortItem.Category);
			AssertEquals(22, ItemSet.BISISftpServerPortItem.DefaultValue);
		}

		#region BISIFtpServerUsername

		public void TestBISIFtpServerUsername()
		{
			ZString initialValue = ItemSet.BISISftpServerUsername;
			try
			{
				ItemSet.BISISftpServerUsername = "TestUsername";
				AssertEquals("TestUsername", ItemSet.BISISftpServerUsername);
			}
			finally
			{
				ItemSet.BISISftpServerUsername = initialValue;
			}
		}

		public void TestBISIFtpServerUsernameItem()
		{
			AssertEquals("BISIFtpServerUsername", ItemSet.BISISftpServerUsernameItem.Name);
			AssertEquals("SFTP Server Username", ItemSet.BISISftpServerUsernameItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.BISISftpServerUsernameItem.Storage);
			AssertEquals(ExpectedBISICategory, ItemSet.BISISftpServerUsernameItem.Category);
			AssertEquals("anonymous", ItemSet.BISISftpServerUsernameItem.DefaultValue);
		}
		#endregion

		#region BISIFtpServerPassword

		public void TestBISIFtpServerPassword()
		{
			ZString initialValue = ItemSet.BISISftpServerPassword;
			try
			{
				ItemSet.BISISftpServerPassword = "TestPassword";
				AssertEquals("TestPassword", ItemSet.BISISftpServerPassword);
			}
			finally
			{
				ItemSet.BISISftpServerPassword = initialValue;
			}
		}

		public void TestBISIFtpServerPasswordItem()
		{
			AssertEquals("BISIFtpServerPassword", ItemSet.BISISftpServerPasswordItem.Name);
			AssertEquals("SFTP Server Password", ItemSet.BISISftpServerPasswordItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.BISISftpServerPasswordItem.Storage);
			AssertEquals(ExpectedBISICategory, ItemSet.BISISftpServerPasswordItem.Category);
			AssertEquals(TextEditorType.Password, ((TextRegistryEditorInfo)ItemSet.BISISftpServerPasswordItem.EditorInfo).EditorType);
			AssertEquals("ice@ups.com", ItemSet.BISISftpServerPasswordItem.DefaultValue);
		}

		#endregion

		#region BillingNotificationGroup

		public void TestBillingNotificationGroupItem()
		{
			AssertEquals("BillingNotificationGroup", ItemSet.BillingNotificationGroupItem.Name);
			AssertEquals("Billing Notification Group", ItemSet.BillingNotificationGroupItem.Caption);
			AssertEquals("The staff group code for staff that will receive billing related failure emails", ItemSet.BillingNotificationGroupItem.Hint);
			StringRegistryDataType dataType = (StringRegistryDataType)ItemSet.BillingNotificationGroupItem.DataType;
			AssertEquals(1, dataType.MinLength);
			AssertEquals(GlbGroupSchema.GG_Code.MaxLength, dataType.MaxLength);
			AssertEquals(TextEditorType.TextBox, ((TextRegistryEditorInfo)ItemSet.BillingNotificationGroupItem.EditorInfo).EditorType);
			AssertEquals(RegistryStorageFlags.System, ItemSet.BillingNotificationGroupItem.Storage);
			AssertEquals(RegistryOptions.Default, ItemSet.BillingNotificationGroupItem.Options);
			AssertEquals(ExpectedBISICategory, ItemSet.BillingNotificationGroupItem.Category);
			AssertEquals("BIS", ItemSet.BillingNotificationGroupItem.DefaultValue);
		}

		public void TestBillingNotificationGroup()
		{
			ItemSet.BillingNotificationGroup = "TES";
			AssertEquals("TES", ItemSet.BillingNotificationGroup);
		}

		#endregion

		const string ExpectedBISICategory = "UPS Client Extensions/BISI";

		#endregion

		#region Upload

		#region XPLDForNonWorkingDaysDateOfArrivalPassed

		public void TestXPLDForNonWorkingDaysDateOfArrivalPassed()
		{
			AssertEquals(1, ItemSet.XPLDForNonWorkingDaysDateOfArrivalPassed.Count);
		}

		public void TestXPLDForNonWorkingDaysDateOfArrivalPassedItem()
		{
			AssertEquals("XPLDForNonWorkingDaysDateOfArrivalPassed", ItemSet.XPLDForNonWorkingDaysDateOfArrivalPassedItem.Name);
			AssertEquals("XPLD's for everyday if date >= date of arrival", ItemSet.XPLDForNonWorkingDaysDateOfArrivalPassedItem.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.XPLDForNonWorkingDaysDateOfArrivalPassedItem.Storage);
			AssertEquals(ExpectedBISIUploadCategory + "/XPLD", ItemSet.XPLDForNonWorkingDaysDateOfArrivalPassedItem.Category);
			AssertEquals(typeof(CodeDescriptionPairListEditorInfo), ItemSet.XPLDForNonWorkingDaysDateOfArrivalPassedItem.EditorInfo.GetType());
			CodeDescriptionPairListEditorInfo editorInfo = (CodeDescriptionPairListEditorInfo)ItemSet.XPLDForNonWorkingDaysDateOfArrivalPassedItem.EditorInfo;
			AssertEquals(true, editorInfo.ShowCodeColumn);
			AssertEquals(false, editorInfo.ShowDescriptionColumn);
			ReadOnlyCodeDescriptionPairList pairList = ItemSet.XPLDForNonWorkingDaysDateOfArrivalPassedItem.DefaultValue;
			AssertEquals(1, pairList.Count);
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold));
		}

		public void TestFlightNumbersForUploadOnDayOfArrivalPlusOneItem()
		{
			TestRegistryItem(ItemSet.FlightNumbersForUploadOnDayOfArrivalPlusOneItem, "FlightNumbersForUploadOnDayOfArrivalPlusOneItem", ExpectedBISIUploadCategory + "/XPLD", "Flight Numbers for Upload on Day of Arrival (DoA) + 1", "Flight Number(s) entered in this list will be used in conjunction with the following 2  registries only:\r\n - XPLD's for everday if date >=Date of arrival\r\n - XPLD's for working days if date >=Date of arrival\r\nIf the Flight number is in this list, the Date of Arrival used will be the Flight Date of Arrival + 1 day", RegistryStorageFlags.System, TextEditorType.Memo, "");
		}

		#endregion

		#region XPLDForWorkingDays

		public void TestXPLDForWorkingDays()
		{
			AssertEquals(13, ItemSet.XPLDForWorkingDays.Count);
		}

		public void TestXPLDForWorkingDaysItem()
		{
			AssertEquals("XPLDForWorkingDays", ItemSet.XPLDForWorkingDaysItem.Name);
			AssertEquals("XPLD's for working days", ItemSet.XPLDForWorkingDaysItem.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.XPLDForWorkingDaysItem.Storage);
			AssertEquals(ExpectedBISIUploadCategory + "/XPLD", ItemSet.XPLDForWorkingDaysItem.Category);
			AssertEquals(typeof(CodeDescriptionPairListEditorInfo), ItemSet.XPLDForWorkingDaysItem.EditorInfo.GetType());
			CodeDescriptionPairListEditorInfo editorInfo = (CodeDescriptionPairListEditorInfo)ItemSet.XPLDForWorkingDaysItem.EditorInfo;
			AssertEquals(true, editorInfo.ShowCodeColumn);
			AssertEquals(false, editorInfo.ShowDescriptionColumn);
			ReadOnlyCodeDescriptionPairList pairList = ItemSet.XPLDForWorkingDaysItem.DefaultValue;
			AssertEquals(13, pairList.Count);
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice));
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient));
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription));
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.BK_CertificateOfOriginRequired));
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing));
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes._34_Missort));
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.FF_RTSAuthorisationRequired));
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.AM_RefusedCancelledOrder));
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.AN_RefusedDuplicateOrder));
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.AS_RefusedShippedTooLate));
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.FE_DutyTaxRefused));
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.RU_AlternateBroker));
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient));
		}

		#endregion

		#region XPLDForWorkingDaysDateOfArrivalPassed

		public void TestXPLDForWorkingDaysDateOfArrivalPassed()
		{
			AssertEquals(7, ItemSet.XPLDForWorkingDaysDateOfArrivalPassed.Count);
		}

		public void TestXPLDForWorkingDaysDateOfArrivalPassedItem()
		{
			AssertEquals("XPLDForWorkingDaysDateOfArrivalPassed", ItemSet.XPLDForWorkingDaysDateOfArrivalPassedItem.Name);
			AssertEquals("XPLD's for working days if date >= date of arrival", ItemSet.XPLDForWorkingDaysDateOfArrivalPassedItem.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.XPLDForWorkingDaysDateOfArrivalPassedItem.Storage);
			AssertEquals(ExpectedBISIUploadCategory + "/XPLD", ItemSet.XPLDForWorkingDaysDateOfArrivalPassedItem.Category);
			AssertEquals(typeof(CodeDescriptionPairListEditorInfo), ItemSet.XPLDForWorkingDaysDateOfArrivalPassedItem.EditorInfo.GetType());
			CodeDescriptionPairListEditorInfo editorInfo = (CodeDescriptionPairListEditorInfo)ItemSet.XPLDForWorkingDaysDateOfArrivalPassedItem.EditorInfo;
			AssertEquals(true, editorInfo.ShowCodeColumn);
			AssertEquals(false, editorInfo.ShowDescriptionColumn);
			ReadOnlyCodeDescriptionPairList pairList = ItemSet.XPLDForWorkingDaysDateOfArrivalPassedItem.DefaultValue;
			AssertEquals(7, pairList.Count);
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.XN_PhoneNumberInvalid));
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.SS_CustomsHold));
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration));
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration));
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.SN_PersonalEffectsHold));
			Assert(pairList.ContainsCode(ReasonCodeDescriptionPairList.Codes.RJ_ShipSparesUnderbondsTranshipments));
		}

		#endregion

		#region BISIUploadDirectory

		public void TestBISIUploadDirectory()
		{
			ZString initialValue = ItemSet.BISIUploadDirectory;
			try
			{
				ItemSet.BISIUploadDirectory = "Remotedir";
				AssertEquals("Remotedir", ItemSet.BISIUploadDirectory);
			}
			finally
			{
				ItemSet.BISIUploadDirectory = initialValue;
			}
		}

		public void TestBISIUploadDirectoryItem()
		{
			AssertEquals("BISIUploadDirectory", ItemSet.BISIUploadDirectoryItem.Name);
			AssertEquals("Upload Directory", ItemSet.BISIUploadDirectoryItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.BISIUploadDirectoryItem.Storage);
			AssertEquals(ExpectedBISIUploadCategory, ItemSet.BISIUploadDirectoryItem.Category);
			AssertEquals("ivrpfile", ItemSet.BISIUploadDirectoryItem.DefaultValue);
		}

		#endregion

		#region BISIUploadFilename

		public void TestBISIUploadFilename()
		{
			ZString initialValue = ItemSet.BISIUploadFilename;
			try
			{
				ItemSet.BISIUploadFilename = "upload.txt";
				AssertEquals("upload.txt", ItemSet.BISIUploadFilename);
			}
			finally
			{
				ItemSet.BISIUploadFilename = initialValue;
			}
		}

		[ExpectNoExceptions]
		public void TestBISIUploadFilename_WithoutExtension()
		{
			ItemSet.BISIUploadFilename = "upload";
		}

		public void TestBISIUploadFilenameItem()
		{
			AssertEquals("BISIUploadFilename", ItemSet.BISIUploadFilenameItem.Name);
			AssertEquals("Upload File Name", ItemSet.BISIUploadFilenameItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.BISIUploadFilenameItem.Storage);
			AssertEquals(ExpectedBISIUploadCategory, ItemSet.BISIUploadFilenameItem.Category);
			AssertEquals("IVRPUPLD.DAT", ItemSet.BISIUploadFilenameItem.DefaultValue);
		}

		#endregion

		#region ForceAllXPLDsToBeUploadedEveryTime

		public void TestForceAllXPLDsToBeUploadedEveryTime()
		{
			ItemSet.ForceAllXPLDsToBeUploadedEveryTime = false;
			AssertEquals("Should set the value to false correctly", false, ItemSet.ForceAllXPLDsToBeUploadedEveryTime);

			ItemSet.ForceAllXPLDsToBeUploadedEveryTime = true;
			AssertEquals("Should set the value to true correctly", true, ItemSet.ForceAllXPLDsToBeUploadedEveryTime);
		}

		#endregion

		#region BISIUploadCompletedHWM

		public void TestBISIUploadCompletedHWM()
		{
			ZDateTime expectedDate = new ZDateTime(2005, 12, 22);
			ItemSet.BISIUploadCompletedHWM = expectedDate;
			AssertEquals(expectedDate, ItemSet.BISIUploadCompletedHWM);
		}

		public void TestBISIUploadCompletedHWMItem()
		{
			AssertEquals("BISIUploadCompletedHWM", ItemSet.BISIUploadCompletedHWMItem.Name);
			AssertEquals("BISI Upload Completed HWM", ItemSet.BISIUploadCompletedHWMItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.BISIUploadCompletedHWMItem.Storage);
			AssertEquals(ExpectedBISIUploadCategory, ItemSet.BISIUploadCompletedHWMItem.Category);
		}

		#endregion

		#region BISIUploadCompletedHWM

		public void TestBISIUploadNoOfRecordsPerSave()
		{
			ItemSet.BISIUploadNoOfRecordsPerSave = 200;
			AssertEquals(200, ItemSet.BISIUploadNoOfRecordsPerSave);
		}

		public void TestBISIUploadNoOfRecordsPerSaveItem()
		{
			AssertEquals("BISIUploadNoOfRecPerSave", ItemSet.BISIUploadNoOfRecordsPerSaveItem.Name);
			AssertEquals("No of Records Per Save", ItemSet.BISIUploadNoOfRecordsPerSaveItem.Caption);
			AssertEquals("This determines the number of records per factory save.", ItemSet.BISIUploadNoOfRecordsPerSaveItem.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.BISIUploadNoOfRecordsPerSaveItem.Storage);
			AssertEquals(ExpectedBISIUploadCategory, ItemSet.BISIUploadNoOfRecordsPerSaveItem.Category);
		}

		#endregion

		#region BISIUploadEverydayHWM

		public void TestBISIUploadEverydayHWM()
		{
			ZDateTime expectedDate = new ZDateTime(2005, 12, 22);
			ItemSet.BISIUploadEverydayHWM = expectedDate;
			AssertEquals(expectedDate, ItemSet.BISIUploadEverydayHWM);
		}

		public void TestBISIUploadEverydayHWMItem()
		{
			AssertEquals("BISIUploadEverydayHWM", ItemSet.BISIUploadEverydayHWMItem.Name);
			AssertEquals("BISI Upload Everyday HWM", ItemSet.BISIUploadEverydayHWMItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.BISIUploadEverydayHWMItem.Storage);
			AssertEquals(ExpectedBISIUploadCategory, ItemSet.BISIUploadEverydayHWMItem.Category);
		}

		#endregion

		#region BISIUploadEverydayDateOfArrivalHWM

		public void TestBISIUploadEverydayDateOfArrivalHWM()
		{
			ZDateTime expectedDate = new ZDateTime(2005, 12, 22);
			ItemSet.BISIUploadEverydayDateOfArrivalHWM = expectedDate;
			AssertEquals(expectedDate, ItemSet.BISIUploadEverydayDateOfArrivalHWM);
		}

		public void TestBISIUploadEverydayDateOfArrivalHWMItem()
		{
			AssertEquals("BISIUploadEverydayDateOfArrivalHWM", ItemSet.BISIUploadEverydayDateOfArrivalHWMItem.Name);
			AssertEquals("BISI Upload Everyday DateOfArrival HWM", ItemSet.BISIUploadEverydayDateOfArrivalHWMItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.BISIUploadEverydayDateOfArrivalHWMItem.Storage);
			AssertEquals(ExpectedBISIUploadCategory, ItemSet.BISIUploadEverydayDateOfArrivalHWMItem.Category);
		}

		#endregion

		#region BISIUploadWorkingDayMetroHWM

		public void TestBISIUploadWorkingDayMetroHWM()
		{
			ZDateTime expectedDate = new ZDateTime(2005, 12, 22);
			ItemSet.BISIUploadWorkingDayMetroHWM = expectedDate;
			AssertEquals(expectedDate, ItemSet.BISIUploadWorkingDayMetroHWM);
		}

		public void TestBISIUploadWorkingDayMetroHWMItem()
		{
			AssertEquals("BISIUploadWorkingDayMetroHWM", ItemSet.BISIUploadWorkingDayMetroHWMItem.Name);
			AssertEquals("BISI Upload Working Day Metro HWM", ItemSet.BISIUploadWorkingDayMetroHWMItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.BISIUploadWorkingDayMetroHWMItem.Storage);
			AssertEquals(ExpectedBISIUploadCategory, ItemSet.BISIUploadWorkingDayMetroHWMItem.Category);
		}

		#endregion

		#region BISIUploadWorkingDayOtherHWM

		public void TestBISIUploadWorkingDayOtherHWM()
		{
			ZDateTime expectedDate = new ZDateTime(2005, 12, 22);
			ItemSet.BISIUploadWorkingDayOtherHWM = expectedDate;
			AssertEquals(expectedDate, ItemSet.BISIUploadWorkingDayOtherHWM);
		}

		public void TestBISIUploadWorkingDayOtherHWMItem()
		{
			AssertEquals("BISIUploadWorkingDayOtherHWM", ItemSet.BISIUploadWorkingDayOtherHWMItem.Name);
			AssertEquals("BISI Upload Working Day Other HWM", ItemSet.BISIUploadWorkingDayOtherHWMItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.BISIUploadWorkingDayOtherHWMItem.Storage);
			AssertEquals(ExpectedBISIUploadCategory, ItemSet.BISIUploadWorkingDayOtherHWMItem.Category);
		}

		#endregion

		#region BISIUploadWorkingDayDateOfArrivalMetroHWM

		public void TestBISIUploadWorkingDayDateOfArrivalMetroHWM()
		{
			ZDateTime expectedDate = new ZDateTime(2005, 12, 22);
			ItemSet.BISIUploadWorkingDayDateOfArrivalMetroHWM = expectedDate;
			AssertEquals(expectedDate, ItemSet.BISIUploadWorkingDayDateOfArrivalMetroHWM);
		}

		public void TestBISIUploadWorkingDayDateOfArrivalMetroHWMItem()
		{
			AssertEquals("BISIUploadWorkingDayDateOfArrivalMetroHWM", ItemSet.BISIUploadWorkingDayDateOfArrivalMetroHWMItem.Name);
			AssertEquals("BISI Upload Working Day DateOfArrival Metro HWM", ItemSet.BISIUploadWorkingDayDateOfArrivalMetroHWMItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.BISIUploadWorkingDayDateOfArrivalMetroHWMItem.Storage);
			AssertEquals(ExpectedBISIUploadCategory, ItemSet.BISIUploadWorkingDayDateOfArrivalMetroHWMItem.Category);
		}

		#endregion

		#region BISIUploadWorkingDayDateOfArrivalOtherHWM

		public void TestBISIUploadWorkingDayDateOfArrivalOtherHWM()
		{
			ZDateTime expectedDate = new ZDateTime(2005, 12, 22);
			ItemSet.BISIUploadWorkingDayDateOfArrivalOtherHWM = expectedDate;
			AssertEquals(expectedDate, ItemSet.BISIUploadWorkingDayDateOfArrivalOtherHWM);
		}

		public void TestBISIUploadWorkingDayDateOfArrivalOtherHWMItem()
		{
			AssertEquals("BISIUploadWorkingDayDateOfArrivalOtherHWM", ItemSet.BISIUploadWorkingDayDateOfArrivalOtherHWMItem.Name);
			AssertEquals("BISI Upload Working Day DateOfArrival Other HWM", ItemSet.BISIUploadWorkingDayDateOfArrivalOtherHWMItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.BISIUploadWorkingDayDateOfArrivalOtherHWMItem.Storage);
			AssertEquals(ExpectedBISIUploadCategory, ItemSet.BISIUploadWorkingDayDateOfArrivalOtherHWMItem.Category);
		}

		#endregion

		#region BISIUploadCurrentBatchNumber

		public void TestBISIUploadCurrentBatchNumber()
		{
			AssertEquals(1, ItemSet.BISIUploadCurrentBatchNumber);
			ItemSet.BISIUploadCurrentBatchNumber = 20;
			AssertEquals(20, ItemSet.BISIUploadCurrentBatchNumber);
		}

		public void TestBISIUploadCurrentBatchNumberItem()
		{
			AssertEquals("BISIUploadCurrentBatchNumber", ItemSet.BISIUploadCurrentBatchNumberItem.Name);
			AssertEquals("Current Batch Number", ItemSet.BISIUploadCurrentBatchNumberItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.BISIUploadCurrentBatchNumberItem.Storage);
			AssertEquals(ExpectedBISIUploadCategory, ItemSet.BISIUploadCurrentBatchNumberItem.Category);
			AssertEquals(1, ItemSet.BISIUploadCurrentBatchNumberItem.DefaultValue);
		}

		#endregion

		#region BISIOBCTaxCertificateNumber

		public void TestBISIOBCTaxCertificateNumber()
		{
			AssertEquals(1, ItemSet.BISIOBCTaxCertificateNumber);
			ItemSet.BISIOBCTaxCertificateNumber = 20;
			AssertEquals(20, ItemSet.BISIOBCTaxCertificateNumber);
		}

		public void TestBISIOBCTaxCertificateNumberItem()
		{
			AssertEquals("BISIOBCTaxCertificateNumber", ItemSet.BISIOBCTaxCertificateNumberItem.Name);
			AssertEquals("OBC Tax Certificate Number", ItemSet.BISIOBCTaxCertificateNumberItem.Caption);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.BISIOBCTaxCertificateNumberItem.Storage);
			AssertEquals(ExpectedBISIUploadCategory, ItemSet.BISIOBCTaxCertificateNumberItem.Category);
			AssertEquals(1, ItemSet.BISIOBCTaxCertificateNumberItem.DefaultValue);
			var dataType = (IntRegistryDataType)ItemSet.BISIOBCTaxCertificateNumberItem.DataType;
			AssertEquals((double)1, dataType.LowerBound);
			AssertEquals((double)9999999, dataType.UpperBound);
		}

		#endregion

		#region BISIUploadArchiveDirectory

		public void TestBISIUploadArchiveDirectory()
		{
			AssertEquals(string.Empty, ItemSet.BISIUploadArchiveDirectory);

			ItemSet.BISIUploadArchiveDirectory = "Test";
			AssertEquals("Test", ItemSet.BISIUploadArchiveDirectory);
		}

		public void TestBISIUploadArchiveDirectoryItem()
		{
			AssertEquals("BISIUploadArchiveDirectory", ItemSet.BISIUploadArchiveDirectoryItem.Name);
			AssertEquals("Archive Directory", ItemSet.BISIUploadArchiveDirectoryItem.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.BISIUploadArchiveDirectoryItem.Storage);
			AssertEquals(ExpectedBISIUploadCategory, ItemSet.BISIUploadArchiveDirectoryItem.Category);
		}

		#endregion

		#region BISIUploadTimeBuffer

		public void TestBISIUploadTimeBuffer()
		{
			AssertEquals("Default value", 8, ItemSet.BISIUploadTimeBuffer);

			ItemSet.BISIUploadTimeBuffer = 24;
			AssertEquals(24, ItemSet.BISIUploadTimeBuffer);
		}

		public void TestBISIUploadTimeBufferItem()
		{
			AssertEquals("BISIUploadTimeBuffer", ItemSet.BISIUploadTimeBufferItem.Name);
			AssertEquals("Upload Time Buffer (in hours)", ItemSet.BISIUploadTimeBufferItem.Caption);
			AssertEquals("This determines the maximum number of shipments to be processed in a single batch.", ItemSet.BISIUploadTimeBufferItem.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.BISIUploadTimeBufferItem.Storage);
			AssertEquals(ExpectedBISIUploadCategory, ItemSet.BISIUploadTimeBufferItem.Category);
			AssertEquals(8, ItemSet.BISIUploadTimeBufferItem.DefaultValue);
			IntRegistryDataType dataType = (IntRegistryDataType)ItemSet.BISIUploadTimeBufferItem.DataType;
			AssertEquals((double)1, dataType.LowerBound);
			AssertEquals((double)48, dataType.UpperBound);
		}

		#endregion

		#region BISIUploadWarningHWMItem

		public void TestBISIUploadWarningHWMItem()
		{
			AssertEquals("BISIUploadWarningHWM", ItemSet.BISIUploadWarningHWMItem.Name);
			AssertEquals("Time Stamp of Upload Warning Last Run", ItemSet.BISIUploadWarningHWMItem.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.BISIUploadWarningHWMItem.Storage);
			AssertEquals(ExpectedBISIUploadCategory, ItemSet.BISIUploadWarningHWMItem.Category);
		}

		#endregion

		#region BISIUploadLoadProcessQueueLogBatchSize

		public void TestBISIUploadLoadProcessQueueLogBatchSize()
		{
			AssertEquals("Default value", 100, ItemSet.BISIUploadLoadProcessQueueLogBatchSize);
			ItemSet.BISIUploadLoadProcessQueueLogBatchSize = 200;
			AssertEquals(200, ItemSet.BISIUploadLoadProcessQueueLogBatchSize);
		}

		public void TestBISIUploadLoadProcessQueueLogBatchSizeItem()
		{
			AssertEquals("DataType", typeof(IntRegistryDataType), ItemSet.BISIUploadLoadProcessQueueLogBatchSizeItem.DataType.GetType());
			AssertEquals("BISIUploadLoadProcessQueueLogBatchSize", ItemSet.BISIUploadLoadProcessQueueLogBatchSizeItem.Name);
			AssertEquals("Category", ExpectedBISIUploadCategory, ItemSet.BISIUploadLoadProcessQueueLogBatchSizeItem.Category);
			AssertEquals("Load ProcessQueueLog Batch Size", ItemSet.BISIUploadLoadProcessQueueLogBatchSizeItem.Caption);
			AssertEquals("Load ProcessQueueLog Batch Size", ItemSet.BISIUploadLoadProcessQueueLogBatchSizeItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.BISIUploadLoadProcessQueueLogBatchSizeItem.Storage);
			AssertEquals("DefaultValue", 100, ItemSet.BISIUploadLoadProcessQueueLogBatchSizeItem.DefaultValue);
		}

		#endregion

		#region WarningReportNotificationGroup

		public void TestWarningReportNotificationGroup()
		{
			AssertEquals("WarningReportNotificationGroup", ItemSet.WarningReportNotificationGroupItem.Name);
			AssertEquals("Warning Report Notification Group", ItemSet.WarningReportNotificationGroupItem.Caption);
			AssertEquals("The staff group code for staff that will receive BISI upload warning and notification emails", ItemSet.WarningReportNotificationGroupItem.Hint);
			AssertEquals(ExpectedBISIUploadCategory, ItemSet.WarningReportNotificationGroupItem.Category);
			AssertEquals(RegistryStorageFlags.System, ItemSet.WarningReportNotificationGroupItem.Storage);
			AssertEquals(RegistryOptions.IsValueMandatory, ItemSet.WarningReportNotificationGroupItem.Options);
			AssertEquals(ExpectedDefaultBISIWarningReportGroupPK, ItemSet.WarningReportNotificationGroupItem.DefaultValue);

			Guid warningReportGroup = new Guid("2E76194A-7F25-413a-941D-B3E3A98EDC39");
			UPEDataRegistry.Instance.WarningReportNotificationGroup = warningReportGroup;
			AssertEquals(warningReportGroup, UPEDataRegistry.Instance.WarningReportNotificationGroup);
		}

		#endregion

		#region InterchangeReportNotificationGroup

		public void TestInterchangeReportNotificationGroup()
		{
			AssertEquals("InterchangeReportNotificationGroup", ItemSet.InterchangeReportNotificationGroupItem.Name);
			AssertEquals("Interchange Report Notification Group", ItemSet.InterchangeReportNotificationGroupItem.Caption);
			AssertEquals("The staff group code for staff that will receive Interchange Report notification emails", ItemSet.InterchangeReportNotificationGroupItem.Hint);
			AssertEquals(ExpectedBISIUploadCategory, ItemSet.InterchangeReportNotificationGroupItem.Category);
			AssertEquals(RegistryStorageFlags.System, ItemSet.InterchangeReportNotificationGroupItem.Storage);
			AssertEquals(RegistryOptions.IsValueMandatory, ItemSet.InterchangeReportNotificationGroupItem.Options);
			AssertEquals(ExpectedDefaultBISIWarningReportGroupPK, ItemSet.InterchangeReportNotificationGroupItem.DefaultValue);

			Guid interchangeReportGroup = new Guid("2E76194A-7F25-413a-941D-B3E3A98EDC39");
			UPEDataRegistry.Instance.InterchangeReportNotificationGroup = interchangeReportGroup;
			AssertEquals(interchangeReportGroup, ItemSet.InterchangeReportNotificationGroup);
		}

		#endregion

		const string ExpectedBISIUploadCategory = ExpectedBISICategory + "/Upload";

		#endregion

		#region Download

		public void TestCODFilesParentFolderItem()
		{
			AssertEquals("CODFilesParentFolder", ItemSet.CODFilesParentFolderItem.Name);
			AssertEquals("SFTP Parent Folder for COD Files", ItemSet.CODFilesParentFolderItem.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.CODFilesParentFolderItem.Storage);
			AssertEquals(ExpectedBISIDownloadCategory, ItemSet.CODFilesParentFolderItem.Category);
			AssertEquals("BISI/PROD", ItemSet.CODFilesParentFolderItem.DefaultValue);
		}

		public void TestBISIDownloadArchiveDirectoryItem()
		{
			AssertEquals("BISIDownloadArchiveDirectory", ItemSet.BISIDownloadArchiveDirectoryItem.Name);
			AssertEquals("Archive Directory", ItemSet.BISIDownloadArchiveDirectoryItem.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.BISIDownloadArchiveDirectoryItem.Storage);
			AssertEquals(ExpectedBISIDownloadCategory, ItemSet.BISIDownloadArchiveDirectoryItem.Category);
		}

		public void TestCODFilesParentFolder()
		{
			ZString initialValue = ItemSet.CODFilesParentFolder;
			try
			{
				ItemSet.CODFilesParentFolder = "d:\\test\\";
				AssertEquals("d:\\test\\", ItemSet.CODFilesParentFolder);
			}
			finally
			{
				ItemSet.CODFilesParentFolder = initialValue;
			}
		}

		public void TestBISIDownloadArchiveDirectory()
		{
			AssertEquals(string.Empty, ItemSet.BISIDownloadArchiveDirectory);

			ItemSet.BISIDownloadArchiveDirectory = "Test";
			AssertEquals("Test", ItemSet.BISIDownloadArchiveDirectory);
		}

		const string ExpectedBISIDownloadCategory = ExpectedBISICategory + "/Download";

		#endregion

		#endregion

		#region Entry Print

		public void TestEntryPrintFtpServerAddressItem()
		{
			AssertEquals("EntryPrintFtpServerAddress", ItemSet.EntryPrintSftpServerAddressItem.Name);
			AssertEquals("SFTP Server Address", ItemSet.EntryPrintSftpServerAddressItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.EntryPrintSftpServerAddressItem.Storage);
			AssertEquals(ExpectedEntryPrintCategory, ItemSet.EntryPrintSftpServerAddressItem.Category);
			AssertEquals(string.Empty, ItemSet.EntryPrintSftpServerAddressItem.DefaultValue);
		}

		public void TestEntryPrintFtpServerPortItem()
		{
			AssertEquals("EntryPrintFtpServerPort", ItemSet.EntryPrintSftpServerPortItem.Name);
			AssertEquals("SFTP Server Port", ItemSet.EntryPrintSftpServerPortItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.EntryPrintSftpServerPortItem.Storage);
			AssertEquals(ExpectedEntryPrintCategory, ItemSet.EntryPrintSftpServerPortItem.Category);
			AssertEquals(22, ItemSet.EntryPrintSftpServerPortItem.DefaultValue);
		}

		public void TestEntryPrintFtpServerUsernameItem()
		{
			AssertEquals("EntryPrintFtpServerUsername", ItemSet.EntryPrintSftpServerUsernameItem.Name);
			AssertEquals("SFTP Server Username", ItemSet.EntryPrintSftpServerUsernameItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.EntryPrintSftpServerUsernameItem.Storage);
			AssertEquals(ExpectedEntryPrintCategory, ItemSet.EntryPrintSftpServerUsernameItem.Category);
			AssertEquals("anonymous", ItemSet.EntryPrintSftpServerUsernameItem.DefaultValue);
		}

		public void TestEntryPrintFtpServerPasswordItem()
		{
			AssertEquals("EntryPrintFtpServerPassword", ItemSet.EntryPrintSftpServerPasswordItem.Name);
			AssertEquals("SFTP Server Password", ItemSet.EntryPrintSftpServerPasswordItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.EntryPrintSftpServerPasswordItem.Storage);
			AssertEquals(ExpectedEntryPrintCategory, ItemSet.EntryPrintSftpServerPasswordItem.Category);
			AssertEquals(TextEditorType.Password, ((TextRegistryEditorInfo)ItemSet.EntryPrintSftpServerPasswordItem.EditorInfo).EditorType);
			AssertEquals("ice@ups.com", ItemSet.EntryPrintSftpServerPasswordItem.DefaultValue);
		}

		public void TestEntryPrintFtpServerUploadDirectoryItem()
		{
			AssertEquals("EntryPrintFtpServerUploadDirectory", ItemSet.EntryPrintSftpServerUploadDirectoryItem.Name);
			AssertEquals("SFTP Server Upload Directory", ItemSet.EntryPrintSftpServerUploadDirectoryItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.EntryPrintSftpServerUploadDirectoryItem.Storage);
			AssertEquals(ExpectedEntryPrintCategory, ItemSet.EntryPrintSftpServerUploadDirectoryItem.Category);
			AssertEquals(string.Empty, ItemSet.EntryPrintSftpServerUploadDirectoryItem.DefaultValue);
		}

		public void TestEntryPrintFtpServerUploadFilenameItem()
		{
			AssertEquals("EntryPrintFtpServerUploadFilename", ItemSet.EntryPrintSftpServerUploadFilenameItem.Name);
			AssertEquals("SFTP Server Upload File Name", ItemSet.EntryPrintSftpServerUploadFilenameItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.EntryPrintSftpServerUploadFilenameItem.Storage);
			AssertEquals(ExpectedEntryPrintCategory, ItemSet.EntryPrintSftpServerUploadFilenameItem.Category);
			AssertEquals("AU_CUSTOMS_ENTRIES.txt", ItemSet.EntryPrintSftpServerUploadFilenameItem.DefaultValue);
		}

		[ExpectNoExceptions]
		public void TestEntryPrintFtpServerUploadFilenameItem_WithoutExtension()
		{
			ItemSet.EntryPrintSftpServerUploadFilenameItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Filename");
		}

		public void TestEntryPrintFtpArchiveDirectoryItem()
		{
			AssertEquals("EntryPrintFtpArchiveDirectory", ItemSet.EntryPrintSftpArchiveDirectoryItem.Name);
			AssertEquals("SFTP Archive Directory", ItemSet.EntryPrintSftpArchiveDirectoryItem.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.EntryPrintSftpArchiveDirectoryItem.Storage);
			AssertEquals(ExpectedEntryPrintCategory, ItemSet.EntryPrintSftpArchiveDirectoryItem.Category);
			AssertEquals(string.Empty, ItemSet.EntryPrintSftpArchiveDirectoryItem.DefaultValue);
		}

		public void TestEntryPrintMaximumDeclarationsToSendPerDayItem()
		{
			AssertEquals("EntryPrintMaximumDeclarationsToSendPerDay", ItemSet.EntryPrintMaximumDeclarationsToSendPerDayItem.Name);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.EntryPrintSftpArchiveDirectoryItem.Storage);
			AssertEquals(ExpectedEntryPrintCategory, ItemSet.EntryPrintSftpArchiveDirectoryItem.Category);
			AssertEquals(1000, ItemSet.EntryPrintMaximumDeclarationsToSendPerDayItem.DefaultValue);
		}

		public void TestEntryPrintFtpServerAddress()
		{
			ZString initialValue = ItemSet.EntryPrintSftpServerAddress;
			try
			{
				ItemSet.EntryPrintSftpServerAddress = "BlobServer";
				AssertEquals("BlobServer", ItemSet.EntryPrintSftpServerAddress);
			}
			finally
			{
				ItemSet.EntryPrintSftpServerAddress = initialValue;
			}
		}

		public void TestEntryPrintFtpServerPort()
		{
			ZInt initialValue = ItemSet.EntryPrintSftpServerPort;
			try
			{
				ItemSet.EntryPrintSftpServerPort = 1111;
				AssertEquals(1111, ItemSet.EntryPrintSftpServerPort);
			}
			finally
			{
				ItemSet.EntryPrintSftpServerPort = initialValue;
			}
		}

		public void TestEntryPrintFtpServerUsername()
		{
			ZString initialValue = ItemSet.EntryPrintSftpServerUsername;
			try
			{
				ItemSet.EntryPrintSftpServerUsername = "UsernameT";
				AssertEquals("UsernameT", ItemSet.EntryPrintSftpServerUsername);
			}
			finally
			{
				ItemSet.EntryPrintSftpServerUsername = initialValue;
			}
		}

		public void TestEntryPrintFtpServerPassword()
		{
			ZString initialValue = ItemSet.EntryPrintSftpServerPassword;
			try
			{
				ItemSet.EntryPrintSftpServerPassword = "PasswordX";
				AssertEquals("PasswordX", ItemSet.EntryPrintSftpServerPassword);
			}
			finally
			{
				ItemSet.EntryPrintSftpServerPassword = initialValue;
			}
		}

		public void TestEntryPrintFtpServerUploadDirectory()
		{
			ZString initialValue = ItemSet.EntryPrintSftpServerUploadDirectory;
			try
			{
				ItemSet.EntryPrintSftpServerUploadDirectory = "Dir123";
				AssertEquals("Dir123", ItemSet.EntryPrintSftpServerUploadDirectory);
			}
			finally
			{
				ItemSet.EntryPrintSftpServerUploadDirectory = initialValue;
			}
		}

		public void TestEntryPrintFtpArchiveDirectory()
		{
			ZString initialValue = ItemSet.EntryPrintSftpArchiveDirectory;
			try
			{
				ItemSet.EntryPrintSftpArchiveDirectory = "bloblfile.txt";
				AssertEquals("bloblfile.txt", ItemSet.EntryPrintSftpArchiveDirectory);
			}
			finally
			{
				ItemSet.EntryPrintSftpArchiveDirectory = initialValue;
			}
		}

		public void TestEntryPrintMaximumDeclarationsToSendPerDay()
		{
			ZInt initialValue = ItemSet.EntryPrintMaximumDeclarationsToSendPerDay;
			try
			{
				ItemSet.EntryPrintMaximumDeclarationsToSendPerDay = 101;
				AssertEquals(101, ItemSet.EntryPrintMaximumDeclarationsToSendPerDay);
			}
			finally
			{
				ItemSet.EntryPrintMaximumDeclarationsToSendPerDay = initialValue;
			}
		}

		const string ExpectedEntryPrintCategory = "UPS Client Extensions/Entry Print Upload";

		#endregion

		#region UPS Charges

		public void TestSecurityFeeAmount()
		{
			AssertEquals("SecurityFeeAmount", ItemSet.SecurityFeeAmount.Name);
			AssertEquals("Security Fee Amount", ItemSet.SecurityFeeAmount.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.SecurityFeeAmount.Storage);
			AssertEquals(9.95m, ItemSet.SecurityFeeAmount.DefaultValue);
			AssertEquals(ExpectedChargesCategory, ItemSet.SecurityFeeAmount.Category);
			AssertEquals(2, ((NumericRegistryEditorInfo)ItemSet.SecurityFeeAmount.EditorInfo).DecimalPlaces);
		}

		public void TestTerminalFeeAmount()
		{
			AssertEquals("TerminalFeeAmount", ItemSet.TerminalFeeAmount.Name);
			AssertEquals("Terminal Fee Amount (ITF)", ItemSet.TerminalFeeAmount.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.TerminalFeeAmount.Storage);
			AssertEquals(45m, ItemSet.TerminalFeeAmount.DefaultValue);
			AssertEquals(ExpectedChargesCategory, ItemSet.TerminalFeeAmount.Category);
		}

		public void TestAlternateBrokerStorageFeePerDay()
		{
			AssertEquals("Name", "AlternateBrokerStorageFeePerDay", ItemSet.AlternateBrokerStorageFeePerDay.Name);
			AssertEquals("Caption", "Alternate Broker Storage Fee Per Day", ItemSet.AlternateBrokerStorageFeePerDay.Caption);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.AlternateBrokerStorageFeePerDay.Storage);
			AssertEquals("DefaultValue", 35m, ItemSet.AlternateBrokerStorageFeePerDay.DefaultValue);
			AssertEquals("Category", ExpectedChargesCategory, ItemSet.AlternateBrokerStorageFeePerDay.Category);
		}

		public void TestContactFeeAmount()
		{
			AssertEquals("UPEContactFeeAmount", ItemSet.ContactFeeAmount.Name);
			AssertEquals("Contact Fee Amount", ItemSet.ContactFeeAmount.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.ContactFeeAmount.Storage);
			AssertEquals("DefaultValue", 0m, ItemSet.ContactFeeAmount.DefaultValue);
			AssertEquals(ExpectedPreReleaseNotifcationCategory, ItemSet.ContactFeeAmount.Category);
			var dataType = (DecimalRegistryDataType)ItemSet.PerLineChargeAmount.DataType;
			AssertEquals("Lower bound", (double)0, dataType.LowerBound);
			AssertEquals("Upper bound", (double)999999m, dataType.UpperBound);
		}

		public void TestEntryLineChargeCappedAmount()
		{
			AssertEquals("UPEEntryLineChargeCappedAmount", ItemSet.EntryLineChargeCappedAmount.Name);
			AssertEquals("Capped Amount", ItemSet.EntryLineChargeCappedAmount.Caption);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.EntryLineChargeCappedAmount.Storage);
			AssertEquals("DefaultValue", 399m, ItemSet.EntryLineChargeCappedAmount.DefaultValue);
			AssertEquals(ExpectedEntryLineChargeCategory, ItemSet.EntryLineChargeCappedAmount.Category);
			var dataType = (DecimalRegistryDataType)ItemSet.PerLineChargeAmount.DataType;
			AssertEquals("Lower bound", (double)0, dataType.LowerBound);
			AssertEquals("Upper bound", (double)999999m, dataType.UpperBound);
		}
		public void TestEntryLineChargeBaseAmount()
		{
			AssertEquals("UPEEntryLineChargeBaseAmount", ItemSet.EntryLineChargeBaseAmount.Name);
			AssertEquals("Base Charge", ItemSet.EntryLineChargeBaseAmount.Caption);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.EntryLineChargeBaseAmount.Storage);
			AssertEquals("DefaultValue", 0m, ItemSet.EntryLineChargeBaseAmount.DefaultValue);
			AssertEquals(ExpectedEntryLineChargeCategory, ItemSet.EntryLineChargeBaseAmount.Category);
			var dataType = (DecimalRegistryDataType)ItemSet.PerLineChargeAmount.DataType;
			AssertEquals("Lower bound", (double)0, dataType.LowerBound);
			AssertEquals("Upper bound", (double)999999m, dataType.UpperBound);
		}

		public void TestUPEPerLineChargeAmount()
		{
			AssertEquals("UPEPerLineChargeAmount", ItemSet.PerLineChargeAmount.Name);
			AssertEquals("Charge Per Line", ItemSet.PerLineChargeAmount.Caption);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.PerLineChargeAmount.Storage);
			AssertEquals("DefaultValue", 4m, ItemSet.PerLineChargeAmount.DefaultValue);
			AssertEquals(ExpectedEntryLineChargeCategory, ItemSet.PerLineChargeAmount.Category);
			var dataType = (DecimalRegistryDataType)ItemSet.PerLineChargeAmount.DataType;
			AssertEquals("Lower bound", (double)0, dataType.LowerBound);
			AssertEquals("Upper bound", (double)999999m, dataType.UpperBound);
		}

		public void TestLinesExemptedFromLineCharge()
		{
			AssertEquals("UPELinesExemptedFromLineCharge", ItemSet.LinesExemptedFromLineCharge.Name);
			AssertEquals("Lines Exempted From 'Charge Per Line'", ItemSet.LinesExemptedFromLineCharge.Caption);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.LinesExemptedFromLineCharge.Storage);
			AssertEquals("DefaultValue", 5, ItemSet.LinesExemptedFromLineCharge.DefaultValue);
			AssertEquals(ExpectedEntryLineChargeCategory, ItemSet.LinesExemptedFromLineCharge.Category);
		}

		public void TestPreReleaseChargeEnabled()
		{
			AssertEquals("UPEPreReleaseChargeEnabled", ItemSet.PreReleaseChargeEnabled.Name);
			AssertEquals("Enable Pre-Release Charge Calculation", ItemSet.PreReleaseChargeEnabled.Caption);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.PreReleaseChargeEnabled.Storage);
			AssertEquals("DefaultValue", false, ItemSet.PreReleaseChargeEnabled.DefaultValue);
			AssertEquals(ExpectedPreReleaseNotifcationCategory, ItemSet.PreReleaseChargeEnabled.Category);
		}

		const string ExpectedChargesCategory = "UPS Client Extensions/Charges";
		const string ExpectedPreReleaseNotifcationCategory = ExpectedChargesCategory + "/Pre-Release Charge";
		const string ExpectedEntryLineChargeCategory = ExpectedPreReleaseNotifcationCategory + "/Entry Line Charge";

		#endregion

		#region ClassifierStaffGroupCode

		public void TestClassifierStaffGroupCode()
		{
			AssertNotNull("Exists ClassifierStaffGroupCode registry item", ItemSet.ClassifierStaffGroupCode);
			AssertEquals("DefaultValue", "CLS", ItemSet.ClassifierStaffGroupCode.DefaultValue);

			ItemSet.ClassifierStaffGroupCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "XXX");
			AssertEquals("Should set the value correctly", "XXX", ItemSet.ClassifierStaffGroupCode.Value);

			try
			{
				ItemSet.ClassifierStaffGroupCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
				Fail("Expected a validation exception due to an empty group code");
			}
			catch (RegistryValidationException)
			{
			}
			string largeGroupCode = new string('x', GlbGroupSchema.GG_Code.MaxLength + 1);
			try
			{
				ItemSet.ClassifierStaffGroupCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, largeGroupCode);
				Fail("Expected a validation exception due to a group code that is too large");
			}
			catch (RegistryValidationException)
			{
			}
		}

		#endregion

		#region COD Thresholds

		#region COD Confirm Payment Threshold Item

		public void TestCODConfirmPaymentThresholdItem()
		{
			AssertEquals("ConfirmPaymentThreshold", ItemSet.CODConfirmPaymentThresholdItem.Name);
			AssertEquals("COD Confirm Payment Threshold", ItemSet.CODConfirmPaymentThresholdItem.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.CODConfirmPaymentThresholdItem.Storage);
			AssertEquals(5000m, ItemSet.CODConfirmPaymentThresholdItem.DefaultValue);
			AssertEquals(ExpectedCODCategory, ItemSet.CODConfirmPaymentThresholdItem.Category);
			AssertEquals(2, ((NumericRegistryEditorInfo)ItemSet.CODConfirmPaymentThresholdItem.EditorInfo).DecimalPlaces);
		}

		const string ExpectedCODCategory = "UPS Client Extensions/COD";

		#endregion

		public void TestCODAutoReleaseAtUploadThresholdItem()
		{
			AssertEquals("AutoReleaseAtUploadThreshold", ItemSet.CODAutoReleaseAtUploadThresholdItem.Name);
			AssertEquals("COD AutoRelease at Upload Threshold", ItemSet.CODAutoReleaseAtUploadThresholdItem.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.CODAutoReleaseAtUploadThresholdItem.Storage);
			AssertEquals(100m, ItemSet.CODAutoReleaseAtUploadThresholdItem.DefaultValue);
			AssertEquals(ExpectedCODCategory, ItemSet.CODAutoReleaseAtUploadThresholdItem.Category);
			AssertEquals(2, ((NumericRegistryEditorInfo)ItemSet.CODAutoReleaseAtUploadThresholdItem.EditorInfo).DecimalPlaces);
		}

		public void TestCODAutoReleaseAndChaseThresholdItem()
		{
			AssertEquals("AutoReleaseAndChaseThreshold", ItemSet.CODAutoReleaseAndChaseThresholdItem.Name);
			AssertEquals("COD AutoRelease and Chase Threshold", ItemSet.CODAutoReleaseAndChaseThresholdItem.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.CODAutoReleaseAndChaseThresholdItem.Storage);
			AssertEquals(250m, ItemSet.CODAutoReleaseAndChaseThresholdItem.DefaultValue);
			AssertEquals(ExpectedCODCategory, ItemSet.CODAutoReleaseAndChaseThresholdItem.Category);
			AssertEquals(2, ((NumericRegistryEditorInfo)ItemSet.CODAutoReleaseAndChaseThresholdItem.EditorInfo).DecimalPlaces);
		}

		#endregion

		#region	Document Printing and Delivery

		#region InvoicePrintBatchMaxCount

		public void TestInvoicePrintBatchMaxCount()
		{
			AssertNotNull("Exists InvoicePrintBatchMaxCount registry item", ItemSet.PrintBatchMaxCount);
			AssertEquals("DefaultValue", 200, ItemSet.PrintBatchMaxCount.DefaultValue);

			ItemSet.PrintBatchMaxCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 50);
			AssertEquals("Should set the value correctly", 50, ItemSet.PrintBatchMaxCount.Value);
		}

		#endregion

		#region DocumentAutoDeliveryNotificationGroup

		public void TestDocumentAutoDeliveryNotificationGroup()
		{
			AssertNotNull("Exists DocumentAutoDeliveryNotificationGroup registry item", ItemSet.DocumentAutoDeliveryNotificationGroup);
			AssertEquals("DefaultValue", "IVN", ItemSet.DocumentAutoDeliveryNotificationGroup.DefaultValue);

			ItemSet.DocumentAutoDeliveryNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "XXX");
			AssertEquals("Should set the value correctly", "XXX", ItemSet.DocumentAutoDeliveryNotificationGroup.Value);

			try
			{
				ItemSet.DocumentAutoDeliveryNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
				Fail("Expected a validation exception due to an empty group code");
			}
			catch (RegistryValidationException)
			{
			}
			string largeGroupCode = new string('x', GlbGroupSchema.GG_Code.MaxLength + 1);
			try
			{
				ItemSet.DocumentAutoDeliveryNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, largeGroupCode);
				Fail("Expected a validation exception due to a group code that is too large");
			}
			catch (RegistryValidationException)
			{
			}
		}

		#endregion

		#region UPSContactEmail

		public void TestUPSContactEmail()
		{
			ItemSet.UPSContactEmail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "clinton@edi.com.au");
			AssertEquals("Should set the value correctly", "clinton@edi.com.au", ItemSet.UPSContactEmail.Value);
		}

		#endregion

		#region UPSContactFax

		public void TestUPSContactFax()
		{
			ItemSet.UPSContactFax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "clinton@edi.com.au");
			AssertEquals("Should set the value correctly", "clinton@edi.com.au", ItemSet.UPSContactFax.Value);
		}

		#endregion

		#endregion

		#region Invoice

		#region Tax Invoice

		#region TaxInvoiceCommentsText1

		public void TestTaxInvoiceCommentsText1()
		{
			AssertNotNull("Exists TaxInvoiceCommentsText1 registry item", ItemSet.TaxInvoiceCommentsText1);
			AssertEquals("DefaultValue", string.Empty, ItemSet.TaxInvoiceCommentsText1.DefaultValue);

			ItemSet.TaxInvoiceCommentsText1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "XXX");
			AssertEquals("Should set the value correctly", "XXX", ItemSet.TaxInvoiceCommentsText1.Value);
		}

		#endregion

		#region TaxInvoiceCommentsText2

		public void TestTaxInvoiceCommentsText2()
		{
			AssertNotNull("Exists TaxInvoiceCommentsText2 registry item", ItemSet.TaxInvoiceCommentsText2);
			AssertEquals("DefaultValue", string.Empty, ItemSet.TaxInvoiceCommentsText2.DefaultValue);

			ItemSet.TaxInvoiceCommentsText2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "XXX");
			AssertEquals("Should set the value correctly", "XXX", ItemSet.TaxInvoiceCommentsText2.Value);
		}

		#endregion

		#region TaxInvoiceFooterText

		public void TestTaxInvoiceFooterText()
		{
			AssertNotNull("Exists TaxInvoiceFooterText registry item", ItemSet.TaxInvoiceFooterText);
			AssertEquals("DefaultValue", string.Empty, ItemSet.TaxInvoiceFooterText.DefaultValue);

			ItemSet.TaxInvoiceFooterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "XXX");
			AssertEquals("Should set the value correctly", "XXX", ItemSet.TaxInvoiceFooterText.Value);
		}

		#endregion

		#region TaxInvoiceImageTest

		public void TestTaxInvoiceImage()
		{
			AssertNotNull("Exists TaxInvoiceImage registry item", ItemSet.TaxInvoiceImage);
			AssertEquals("DefaultValue", null, ItemSet.TaxInvoiceImage.DefaultValue);

			using (Bitmap testImage = new Bitmap(2, 2))
			{
				ItemSet.TaxInvoiceImage.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, testImage);
				AssertNotNull("TaxInvoiceImage default value", ItemSet.TaxInvoiceImage);
				AssertEquals("TaxInvoiceImage Logo", testImage.Size, ItemSet.TaxInvoiceImage.Value.Size);
			}
		}

		#endregion

		#endregion

		#endregion

		#region MatchingActivities

		public void TestMatchingActivitiesHWMItem()
		{
			AssertEquals("MatchingActivitiesHWM", ItemSet.MatchingActivitiesHWMItem.Name);
			AssertEquals("Time Stamp of Matching Activities Last Run", ItemSet.MatchingActivitiesHWMItem.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.MatchingActivitiesHWMItem.Storage);
			AssertEquals(MatchingActivitiesCategory, ItemSet.MatchingActivitiesHWMItem.Category);
		}

		readonly string MatchingActivitiesCategory = ExpectedCategory + "/Matching Activities";

		#endregion

		#region SAC Screening

		#region Stop Words

		public void TestStopPhrasesForConsignorName()
		{
			TestStopWordsPropertyAndItem("StopPhrasesForConsignorName", "Consignor Name", "Stop words or phrases that when identified in the 'Consignor Name' of a cargo report, will move the job to Intervention");
		}

		public void TestStopPhrasesForConsignorAccountNum()
		{
			TestStopWordsPropertyAndItem("StopPhrasesForConsignorAccountNum", "Consignor Account #", "Stop words or phrases that when identified in the 'Consignor Account # of the level 1 record' of a cargo report, will move the job to Intervention");
		}

		public void TestStopPhrasesForConsignorAddress()
		{
			TestStopWordsPropertyAndItem("StopPhrasesForConsignorAddress", "Consignor Address", "Stop words or phrases that when identified in the 'Consignor Address line 1 or 2' of a cargo report, will move the job to Intervention");
		}

		public void TestStopPhrasesForConsigneeName()
		{
			TestStopWordsPropertyAndItem("StopPhrasesForConsigneeName", "Consignee Name", "Stop words or phrases that when identified in the 'Consignee Name' of a cargo report, will move the job to Intervention");
		}

		public void TestStopPhrasesForConsigneeAccountNum()
		{
			TestStopWordsPropertyAndItem("StopPhrasesForConsigneeAccountNum", "Consignee Account #", "Stop words or phrases that when identified in the 'Consignee Account # of the level 1 record' of a cargo report, will move the job to Intervention");
		}

		public void TestStopPhrasesForConsigneeAddress()
		{
			TestStopWordsPropertyAndItem("StopPhrasesForConsigneeAddress", "Consignee Address", "Stop words or phrases that when identified in the 'Consignee Address line 1 or 2' of a cargo report, will move the job to Intervention");
		}

		public void TestStopPhrasesForGoodsDescription()
		{
			TestStopWordsPropertyAndItem("StopPhrasesForGoodsDescription", "Goods Description", "Stop words or phrases that when identified in the 'Goods Description' of a cargo report, will move the job to Intervention");
			StringRegistryItem stopPhrasesForGoodsDescriptionItem = GetStringRegistryItem("StopPhrasesForGoodsDescriptionItem");
			stopPhrasesForGoodsDescriptionItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stopPhrasesForGoodsDescriptionItem.DefaultValue);

			AssertEquals(20, ItemSet.StopPhrasesForGoodsDescription.Length);
			AssertEquals("CONSOL", ItemSet.StopPhrasesForGoodsDescription[0]);
			AssertEquals("CONSOLE", ItemSet.StopPhrasesForGoodsDescription[1]);
			AssertEquals("GIFT", ItemSet.StopPhrasesForGoodsDescription[2]);
			AssertEquals("XMAS", ItemSet.StopPhrasesForGoodsDescription[3]);
			AssertEquals("CHRISTMAS", ItemSet.StopPhrasesForGoodsDescription[4]);
			AssertEquals("TBA", ItemSet.StopPhrasesForGoodsDescription[5]);
			AssertEquals("UNKNOWN", ItemSet.StopPhrasesForGoodsDescription[6]);
			AssertEquals("PRESENT", ItemSet.StopPhrasesForGoodsDescription[7]);
			AssertEquals("NCV", ItemSet.StopPhrasesForGoodsDescription[8]);
			AssertEquals("INVOICES", ItemSet.StopPhrasesForGoodsDescription[9]);
			AssertEquals("SOUVINER", ItemSet.StopPhrasesForGoodsDescription[10]);
			AssertEquals("PERSONAL HYGIENE", ItemSet.StopPhrasesForGoodsDescription[11]);
			AssertEquals("DITTO", ItemSet.StopPhrasesForGoodsDescription[12]);
			AssertEquals("NON HAZARDOUS", ItemSet.StopPhrasesForGoodsDescription[13]);
			AssertEquals("USED", ItemSet.StopPhrasesForGoodsDescription[14]);
			AssertEquals("SAMPLE", ItemSet.StopPhrasesForGoodsDescription[15]);
			AssertEquals("PERSONAL", ItemSet.StopPhrasesForGoodsDescription[16]);
			AssertEquals("ETC", ItemSet.StopPhrasesForGoodsDescription[17]);
			AssertEquals("OLD", ItemSet.StopPhrasesForGoodsDescription[18]);
			AssertEquals("SAMPLE", ItemSet.StopPhrasesForGoodsDescription[19]);
		}

		public void TestQuarantineStopPhrasesForGoodsDescription()
		{
			TestStopWordsPropertyAndItem("QuarantineStopPhrasesForGoodsDescription", "Quarantine Stop Words", "Stop words or phrases that when identified in the 'Goods Description' of a cargo report, will move the job to Intervention");
		}

		void TestStopWordsPropertyAndItem(string propertyName, string caption, string hint)
		{
			TestStopWordsProperty(propertyName);
			TestStopWordsItem(propertyName, caption, hint);
		}

		void TestStopWordsProperty(string propertyName)
		{
			StringRegistryItem stopWordsItem = GetStringRegistryItem(propertyName + "Item");
			//PropertyDescriptor StopWordsProperty = TypeDescriptor.GetProperties(typeof(UPEDataRegistry))[PropertyName];
			stopWordsItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			AssertEquals("Should be an empty array", 0, GetStopWordsRegistryPropertyValue(propertyName).Length);

			SetStopWordsRegistryPropertyValue(propertyName, new string[] { "MEH", "NUCLEAR", "STOP", "WHISKEY" });
			AssertEquals("MEH\r\nNUCLEAR\r\nSTOP\r\nWHISKEY", stopWordsItem.Value);

			SetStopWordsRegistryPropertyValue(propertyName, new string[] { "MEH", string.Empty, "  NUCLEAR", "STOP          ", "      WHISKEY", string.Empty });
			AssertEquals("Whitespaces should be trimmed and empty string should not be included", "MEH\r\nNUCLEAR\r\nSTOP\r\nWHISKEY", stopWordsItem.Value);

			stopWordsItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "MEH\r\n\r\nNUCLEAR           \r\n    \r\nSTOP \r\n   WHISKEY");
			AssertEquals("Whitespaces should be trimmed and empty string should not be included", "MEH,NUCLEAR,STOP,WHISKEY", string.Join(",", GetStopWordsRegistryPropertyValue(propertyName)));
		}

		void TestStopWordsItem(string propertyName, string expectedCaption, string expectedHint)
		{
			StringRegistryItem stopWordsItem = GetStringRegistryItem(propertyName + "Item");

			AssertEquals("Name", "UPE" + propertyName, stopWordsItem.Name);
			AssertEquals("Caption", expectedCaption, stopWordsItem.Caption);
			AssertEquals("Hint", expectedHint, stopWordsItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, stopWordsItem.Storage);
			AssertEquals("Category", ExpectedUPSAUScreeningCategory, stopWordsItem.Category);
			AssertEquals("Should be a memo text editor, so that the user can enter line breaks", TextEditorType.Memo, ((TextRegistryEditorInfo)stopWordsItem.EditorInfo).EditorType);
		}

		string[] GetStopWordsRegistryPropertyValue(string propertyName)
		{
			PropertyDescriptor stopWordsProperty = TypeDescriptor.GetProperties(typeof(UPEDataRegistry))[propertyName];
			string[] result = (string[])stopWordsProperty.GetValue(ItemSet);
			return result;
		}

		void SetStopWordsRegistryPropertyValue(string propertyName, string[] value)
		{
			PropertyDescriptor stopWordsProperty = TypeDescriptor.GetProperties(typeof(UPEDataRegistry))[propertyName];
			stopWordsProperty.SetValue(ItemSet, value);
		}

		StringRegistryItem GetStringRegistryItem(string itemPropertyName)
		{
			PropertyInfo propertyInfo = typeof(UPEDataRegistry).GetProperty(itemPropertyName, BindingFlags.Instance | BindingFlags.NonPublic);
			return (StringRegistryItem)propertyInfo.GetValue(ItemSet, null);
		}

		#endregion

		#region NonDocumentScreeningValueRanges

		public void TestNonDocumentScreeningValueRanges()
		{
			ItemSet.NonDocumentScreeningValueRangesItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1-5 ; 10-15.9");
			AssertEquals(2, ItemSet.NonDocumentScreeningValueRanges.Count);
			AssertEquals(1.1m, ItemSet.NonDocumentScreeningValueRanges[0].Key);
			AssertEquals(5m, ItemSet.NonDocumentScreeningValueRanges[0].Value);
			AssertEquals(10m, ItemSet.NonDocumentScreeningValueRanges[1].Key);
			AssertEquals(15.9m, ItemSet.NonDocumentScreeningValueRanges[1].Value);
		}

		public void TestNonDocumentScreeningValueRanges_WithSemicolonsEitherSide()
		{
			ItemSet.NonDocumentScreeningValueRangesItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ";1-2;");
			AssertEquals(1, ItemSet.NonDocumentScreeningValueRanges.Count);
		}

		public void TestNonDocumentScreeningValueRanges_WithInvalidValues()
		{
			ItemSet.NonDocumentScreeningValueRangesItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			AssertEquals(0, ItemSet.NonDocumentScreeningValueRanges.Count);
			ItemSet.NonDocumentScreeningValueRangesItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "x;-");
			AssertEquals(0, ItemSet.NonDocumentScreeningValueRanges.Count);
			ItemSet.NonDocumentScreeningValueRangesItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1;2");
			AssertEquals(0, ItemSet.NonDocumentScreeningValueRanges.Count);
			ItemSet.NonDocumentScreeningValueRangesItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1;2");
			AssertEquals(0, ItemSet.NonDocumentScreeningValueRanges.Count);
			ItemSet.NonDocumentScreeningValueRangesItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "2-");
			AssertEquals(0, ItemSet.NonDocumentScreeningValueRanges.Count);
			ItemSet.NonDocumentScreeningValueRangesItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "-2");
			AssertEquals(0, ItemSet.NonDocumentScreeningValueRanges.Count);
		}

		#endregion

		#endregion

		#region Auto Queue Movement

		public void TestCusHAWBAutoQueueMovementRegistryItem()
		{
			string expectedHint = "Cargo Report Queue Movement based on the Free Text Segments of the received CARST Message.\r\n" +
								  "Queue is moved in the order of priority (1 being the highest priority).\r\n" +
								  "Segment Name has to be exactly as how it appears in the message.";
			AssertEquals("CusHAWBAutoQueueMovementRegistryItem", ItemSet.CusHAWBAutoQueueMovementRegistryItem.Name);
			AssertEquals("Cargo Report Queue Movement", ItemSet.CusHAWBAutoQueueMovementRegistryItem.Caption);
			AssertEquals(expectedHint, ItemSet.CusHAWBAutoQueueMovementRegistryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.CusHAWBAutoQueueMovementRegistryItem.Storage);
			AssertEquals(ExpectedAutoQueueMovementCategory, ItemSet.CusHAWBAutoQueueMovementRegistryItem.Category);
			AssertCusHAWBAutoQueueMovementRegistryItemDefaultValue();
		}

		public void TestSortedCusHAWBAutoQueueMovements()
		{
			CusHAWBAutoQueueMovementCollection defaultValue = ItemSet.CusHAWBAutoQueueMovementRegistryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			try
			{
				AssertEquals(12, ItemSet.SortedCusHAWBAutoQueueMovements.Count);
				for (int i = 1; i <= ItemSet.SortedCusHAWBAutoQueueMovements.Count; i++)
				{
					AssertEquals(i, ItemSet.SortedCusHAWBAutoQueueMovements[i - 1].Priority);
				}

				CusHAWBAutoQueueMovementCollection collection = new CusHAWBAutoQueueMovementCollection();
				collection.Add(CreateCusHAWBAutoQueueMovement(78));
				collection.Add(CreateCusHAWBAutoQueueMovement(2));
				collection.Add(CreateCusHAWBAutoQueueMovement(22));
				collection.Add(CreateCusHAWBAutoQueueMovement(122));
				ItemSet.CusHAWBAutoQueueMovementRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

				AssertEquals(4, ItemSet.SortedCusHAWBAutoQueueMovements.Count);
				AssertEquals("Should be re-sorted", 2, ItemSet.SortedCusHAWBAutoQueueMovements[0].Priority);
				AssertEquals("Should be re-sorted", 22, ItemSet.SortedCusHAWBAutoQueueMovements[1].Priority);
				AssertEquals("Should be re-sorted", 78, ItemSet.SortedCusHAWBAutoQueueMovements[2].Priority);
				AssertEquals("Should be re-sorted", 122, ItemSet.SortedCusHAWBAutoQueueMovements[3].Priority);
			}
			finally
			{
				ItemSet.CusHAWBAutoQueueMovementRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);
			}
		}

		void AssertCusHAWBAutoQueueMovementRegistryItemDefaultValue()
		{
			CusHAWBAutoQueueMovementCollection collection = ItemSet.CusHAWBAutoQueueMovementRegistryItem.DefaultValue;
			collection.Sort(CusHAWBAutoQueueMovement.Schema.Priority, ListSortDirection.Ascending);
			AssertEquals(12, collection.Count);
			AssertCusHAWBAutoQueueMovement(collection[0], "ACS/QUARANTINE IMPEDIMENT DETAILS", "QUARANTINE ACTION HOLD", CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, StatusCodeDescriptionPairList.EmptyStatus, 1);
			AssertCusHAWBAutoQueueMovement(collection[1], "ACS/QUARANTINE IMPEDIMENT DETAILS", "CONDITIONAL RELEASE", CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, StatusCodeDescriptionPairList.EmptyStatus, 2);
			AssertCusHAWBAutoQueueMovement(collection[2], "ACS/QUARANTINE IMPEDIMENT DETAILS", "DOCO ASSESSMENT", CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, StatusCodeDescriptionPairList.EmptyStatus, 3);
			AssertCusHAWBAutoQueueMovement(collection[3], "ACS/QUARANTINE IMPEDIMENT DETAILS", "PENDING QUARANTINE ACTION", CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, StatusCodeDescriptionPairList.EmptyStatus, 4);
			AssertCusHAWBAutoQueueMovement(collection[4], "ACS/QUARANTINE IMPEDIMENT DETAILS", "INSPECTION", CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, StatusCodeDescriptionPairList.EmptyStatus, 5);
			AssertCusHAWBAutoQueueMovement(collection[5], "QUARANTINE CARGO REPORT EVALUATION COMPLETE", "NO", CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, StatusCodeDescriptionPairList.EmptyStatus, 6);
			AssertCusHAWBAutoQueueMovement(collection[6], "ACS/QUARANTINE IMPEDIMENT DETAILS", "DEFICIENT CONSIGNEE ADDRESS", CargoReportQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient, StatusCodeDescriptionPairList.Codes.TF_IncorrectAddressOrNeedPhoneNumber, 7);
			AssertCusHAWBAutoQueueMovement(collection[7], "ACS/QUARANTINE IMPEDIMENT DETAILS", "DEFICIENT CONSIGNEE NAME", CargoReportQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient, StatusCodeDescriptionPairList.Codes.TF_IncorrectAddressOrNeedPhoneNumber, 8);
			AssertCusHAWBAutoQueueMovement(collection[8], "ACS/QUARANTINE IMPEDIMENT DETAILS", "DEFICIENT CONSIGNOR ADDRESS", CargoReportQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient, StatusCodeDescriptionPairList.Codes.TF_IncorrectAddressOrNeedPhoneNumber, 9);
			AssertCusHAWBAutoQueueMovement(collection[9], "ACS/QUARANTINE IMPEDIMENT DETAILS", "DEFICIENT CONSIGNOR NAME", CargoReportQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient, StatusCodeDescriptionPairList.Codes.TF_IncorrectAddressOrNeedPhoneNumber, 10);
			AssertCusHAWBAutoQueueMovement(collection[10], "ACS/QUARANTINE IMPEDIMENT DETAILS", "FULL CONSIGNEE NAME AND ADDRESS DETAILS ARE REQUIRED", CargoReportQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient, StatusCodeDescriptionPairList.Codes.TF_IncorrectAddressOrNeedPhoneNumber, 11);
			AssertCusHAWBAutoQueueMovement(collection[11], "ACS/QUARANTINE IMPEDIMENT DETAILS", "FULL CONSIGNOR NAME AND ADDRESS DETAILS ARE REQUIRED", CargoReportQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient, StatusCodeDescriptionPairList.Codes.TF_IncorrectAddressOrNeedPhoneNumber, 12);
		}

		void AssertCusHAWBAutoQueueMovement(CusHAWBAutoQueueMovement queueMovement, ZString expectedSegmentName, ZString expectedSegmentValue, ZString expectedQueueName, ZString expectedReasonCode, ZString expectedStatusCode, ZInt expectedPriority)
		{
			AssertEquals(expectedSegmentName, queueMovement.FreeTextSegmentName);
			AssertEquals(expectedSegmentValue, queueMovement.FreeTextSegmentValue);
			AssertEquals(expectedQueueName, queueMovement.Queue.QueueName);
			AssertEquals(expectedReasonCode, queueMovement.Queue.Status);
			AssertEquals(expectedStatusCode, queueMovement.Queue.SubStatus);
			AssertEquals(expectedPriority, queueMovement.Priority);
		}

		CusHAWBAutoQueueMovement CreateCusHAWBAutoQueueMovement(ZInt priority)
		{
			CusHAWBAutoQueueMovement result = new CusHAWBAutoQueueMovement();
			string priorityAsString = priority.ToString();
			result.FreeTextSegmentName = priorityAsString;
			result.FreeTextSegmentValue = priorityAsString;
			result.Queue.QueueName = CargoReportQueueCodeDescriptionPairList.Codes.Completed;
			result.Priority = priority;
			return result;
		}

		readonly string ExpectedAutoQueueMovementCategory = ExpectedCategory + "/Auto Queue Movement";

		#endregion

		#region Process Queue Processor

		public void TestProcessQueueProcessorHWMItem()
		{
			AssertEquals("ProcessQueueProcessorHWM", ItemSet.ProcessQueueProcessorHWMItem.Name);
			AssertEquals("Time Stamp of UPS Queue Processor Last Run", ItemSet.ProcessQueueProcessorHWMItem.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.ProcessQueueProcessorHWMItem.Storage);
			AssertEquals(ExpectedProcessQueueProcessorCategory, ItemSet.ProcessQueueProcessorHWMItem.Category);
			AssertEquals(RegistryOptions.NotCached, ItemSet.ProcessQueueProcessorHWMItem.Options);
		}

		public void TestProcessQueueTimeBufferItem()
		{
			AssertEquals("ProcessQueueProcessorTimeBuffer", ItemSet.ProcessQueueProcessorTimeBufferItem.Name);
			AssertEquals("UPS Queue Processor Time Buffer (in hours)", ItemSet.ProcessQueueProcessorTimeBufferItem.Caption);
			AssertEquals("This determines the maximum number of queue logs to be processed in a single batch.", ItemSet.ProcessQueueProcessorTimeBufferItem.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.ProcessQueueProcessorTimeBufferItem.Storage);
			AssertEquals(ExpectedProcessQueueProcessorCategory, ItemSet.ProcessQueueProcessorTimeBufferItem.Category);
			AssertEquals(24, ItemSet.ProcessQueueProcessorTimeBufferItem.DefaultValue);
			IntRegistryDataType dataType = (IntRegistryDataType)ItemSet.ProcessQueueProcessorTimeBufferItem.DataType;
			AssertEquals((double)1, dataType.LowerBound);
			AssertEquals((double)168, dataType.UpperBound);
		}

		public void TestProcessQueueProcessorRunTimeOffSet()
		{
			AssertEquals("ProcessQueueProcessorRunTimeOffSet", ItemSet.ProcessQueueProcessorRunTimeOffSetItem.Name);
			AssertEquals("UPS Queue Processor Run Time Offset (in seconds)", ItemSet.ProcessQueueProcessorRunTimeOffSetItem.Caption);
			AssertEquals("UPS Queue Processor Run Time Offset (in seconds). If value is set to 60 seconds, service task will look for logs with timestamp < current time - 60 seconds.", ItemSet.ProcessQueueProcessorRunTimeOffSetItem.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.ProcessQueueProcessorRunTimeOffSetItem.Storage);
			AssertEquals(ExpectedProcessQueueProcessorCategory, ItemSet.ProcessQueueProcessorRunTimeOffSetItem.Category);
			AssertEquals(RegistryOptions.Default, ItemSet.ProcessQueueProcessorRunTimeOffSetItem.Options);
			AssertEquals(60, ItemSet.ProcessQueueProcessorRunTimeOffSetItem.DefaultValue);
			AssertEquals(60, ItemSet.ProcessQueueProcessorRunTimeOffSet);
		}

		public void TestProcessQueueProcessorHWM()
		{
			ZDateTime expectedDate = new ZDateTime(2005, 12, 22);
			ItemSet.ProcessQueueProcessorHWM = expectedDate;
			AssertEquals(expectedDate, ItemSet.ProcessQueueProcessorHWM);
		}

		public void TestProcessQueueProcessorTimeBuffer()
		{
			AssertEquals("Default value", 24, ItemSet.ProcessQueueProcessorTimeBuffer);

			ItemSet.ProcessQueueProcessorTimeBuffer = 12;
			AssertEquals(12, ItemSet.ProcessQueueProcessorTimeBuffer);
		}

		const string ExpectedProcessQueueProcessorCategory = ExpectedCategory + "/UPS Queue Processor";

		#endregion

		#region Document Image Importer

		public void TestDocumentImagingRepository()
		{
			AssertEquals("UPEDocumentImagingRepository", ItemSet.DocumentImagingRepositoryItem.Name);
			AssertEquals(RegistryStorageFlags.System, ItemSet.DocumentImagingRepositoryItem.Storage);
			AssertEquals(ExpectedDocumentImageImporterCategory, ItemSet.DocumentImagingRepositoryItem.Category);
			AssertEquals(RegistryOptions.Default, ItemSet.DocumentImagingRepositoryItem.Options);
			AssertEquals("Default Value", Env.TempPath, ItemSet.DocumentImagingRepositoryItem.DefaultValue);
		}

		public void TestDocumentImagingImageTypes()
		{
			AssertEquals("UPEDocumentImagingImageTypes", ItemSet.DocumentImagingImageTypes.Name);
			AssertEquals(RegistryStorageFlags.System, ItemSet.DocumentImagingImageTypes.Storage);
			AssertEquals(ExpectedDocumentImageImporterCategory, ItemSet.DocumentImagingImageTypes.Category);
		}

		public void TestDocumentImagingNotificationGroup()
		{
			AssertEquals("UPEDocumentImagingNotificationGroup", ItemSet.DocumentImagingNotificationGroup.Name);
			AssertEquals(RegistryStorageFlags.System, ItemSet.DocumentImagingNotificationGroup.Storage);
			AssertEquals(RegistryOptions.IsValueMandatory, ItemSet.DocumentImagingNotificationGroup.Options);
			AssertEquals(ExpectedDocumentImageImporterCategory, ItemSet.DocumentImagingNotificationGroup.Category);
		}

		public void TestDocumentImagingNotificationsCache()
		{
			AssertEquals("UPEDocumentImagingNotificationsCache", ItemSet.DocumentImagingNotificationsCache.Name);
			AssertEquals(RegistryStorageFlags.System, ItemSet.DocumentImagingNotificationsCache.Storage);
			AssertEquals(RegistryOptions.Default | RegistryOptions.IsOnlyForDevelopers, ItemSet.DocumentImagingNotificationsCache.Options);
			AssertEquals(ExpectedDocumentImageImporterCategory, ItemSet.DocumentImagingNotificationsCache.Category);
		}

		const string ExpectedDocumentImageImporterCategory = ExpectedCategory + "/Document Image Importer";

		#endregion

		#region SMS

		public void TestSMSShipmentLoadDeadlineBeforeArrivalInMinutes()
		{
			AssertEquals("UPESMSShipmentLoadDeadlineBeforeArrivalInMinutes", ItemSet.SMSShipmentLoadDeadlineBeforeArrivalInMinutes.Name);
			AssertEquals(RegistryStorageFlags.System, ItemSet.SMSShipmentLoadDeadlineBeforeArrivalInMinutes.Storage);
			AssertEquals(ExpectedSMSCategory, ItemSet.SMSShipmentLoadDeadlineBeforeArrivalInMinutes.Category);
			AssertEquals(RegistryOptions.Default, ItemSet.SMSShipmentLoadDeadlineBeforeArrivalInMinutes.Options);
			AssertEquals("Default Value", 180, ItemSet.SMSShipmentLoadDeadlineBeforeArrivalInMinutes.DefaultValue);
		}

		public void TestSMSEmailAddressSuffix()
		{
			AssertEquals("UPESMSEmailAddressSuffix", ItemSet.SMSEmailAddressSuffix.Name);
			AssertEquals(RegistryStorageFlags.System, ItemSet.SMSEmailAddressSuffix.Storage);
			AssertEquals(ExpectedSMSCategory, ItemSet.SMSEmailAddressSuffix.Category);
			AssertEquals(RegistryOptions.Default, ItemSet.SMSEmailAddressSuffix.Options);
			AssertEquals("Default Value", ".fwd@messagenet.com.au", ItemSet.SMSEmailAddressSuffix.DefaultValue);
		}

		public void TestSMSNotificationGroup()
		{
			AssertNotNull("Exists SMSNotificationGroup registry item", ItemSet.SMSNotificationGroup);
			AssertEquals("DefaultValue", "SMS", ItemSet.SMSNotificationGroup.DefaultValue);

			ItemSet.SMSNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "XXX");
			AssertEquals("Should set the value correctly", "XXX", ItemSet.SMSNotificationGroup.Value);

			string largeGroupCode = new string('x', GlbGroupSchema.GG_Code.MaxLength + 1);
			try
			{
				ItemSet.SMSNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, largeGroupCode);
				Fail("Expected a validation exception due to a group code that is too large");
			}
			catch (RegistryValidationException)
			{
			}
		}

		const string ExpectedSMSCategory = ExpectedCategory + "/SMS";

		#endregion

		public void TestEnableAutoPopulateCycleDetailsToImportGlobalManifestBills()
		{
			using (Env.SetTemporaryUserContext(new NullCompanyUserContext(Env.CurrentUserContext)))
			{
				Assert("Default value true", ItemSet.EnableAutoPopulateCycleDetailsToImportGlobalManifestBills);
				ItemSet.EnableAutoPopulateCycleDetailsToImportGlobalManifestBills = false;
				Assert("New value", !ItemSet.EnableAutoPopulateCycleDetailsToImportGlobalManifestBills);
			}

			Assert("Default value true", ItemSet.EnableAutoPopulateCycleDetailsToImportGlobalManifestBills);
			ItemSet.EnableAutoPopulateCycleDetailsToImportGlobalManifestBills = false;
			Assert("New value", !ItemSet.EnableAutoPopulateCycleDetailsToImportGlobalManifestBills);
		}

		public void TestSGScreeningRegistries()
		{
			UPEDataRegistry.Instance.StopPhrasesForSGGoodsDescription = new[] { "A", "B" };
			AssertArrayEqualsByElements(new[] { "A", "B" }, UPEDataRegistry.Instance.StopPhrasesForSGGoodsDescription);

			AssertArrayEqualsByElements(new[] { "48000 to 52000", "81000 to 81999" }, UPEDataRegistry.Instance.StopPostcodeRangesForSGFreeTradeZones);
			UPEDataRegistry.Instance.StopPostcodeRangesForSGFreeTradeZones = new[] { "A", "B" };
			AssertArrayEqualsByElements(new[] { "A", "B" }, UPEDataRegistry.Instance.StopPostcodeRangesForSGFreeTradeZones);

			AssertArrayEqualsByElements(new[] { "FOB" }, UPEDataRegistry.Instance.SGFallBackIncotermForLevel1);
			UPEDataRegistry.Instance.SGFallBackIncotermForLevel1 = new[] { "A", "B" };
			AssertArrayEqualsByElements(new[] { "A", "B" }, UPEDataRegistry.Instance.SGFallBackIncotermForLevel1);

			UPEDataRegistry.Instance.StopPhrasesForSGConsigneeAccountNum = new[] { "A", "B" };
			AssertArrayEqualsByElements(new[] { "A", "B" }, UPEDataRegistry.Instance.StopPhrasesForSGConsigneeAccountNum);

			UPEDataRegistry.Instance.StopPhrasesForSGConsigneeAddress = new[] { "A", "B" };
			AssertArrayEqualsByElements(new[] { "A", "B" }, UPEDataRegistry.Instance.StopPhrasesForSGConsigneeAddress);

			UPEDataRegistry.Instance.StopPhrasesForSGConsigneeName = new[] { "A", "B" };
			AssertArrayEqualsByElements(new[] { "A", "B" }, UPEDataRegistry.Instance.StopPhrasesForSGConsigneeName);

			UPEDataRegistry.Instance.StopPhrasesForSGConsignorAccountNum = new[] { "A", "B" };
			AssertArrayEqualsByElements(new[] { "A", "B" }, UPEDataRegistry.Instance.StopPhrasesForSGConsignorAccountNum);

			UPEDataRegistry.Instance.StopPhrasesForSGConsignorAddress = new[] { "A", "B" };
			AssertArrayEqualsByElements(new[] { "A", "B" }, UPEDataRegistry.Instance.StopPhrasesForSGConsignorAddress);

			UPEDataRegistry.Instance.StopPhrasesForSGConsignorName = new[] { "A", "B" };
			AssertArrayEqualsByElements(new[] { "A", "B" }, UPEDataRegistry.Instance.StopPhrasesForSGConsignorName);

			AssertEquals(true, UPEDataRegistry.Instance.FilterSGTranshipments);
			UPEDataRegistry.Instance.FilterSGTranshipments = false;
			AssertEquals(false, UPEDataRegistry.Instance.FilterSGTranshipments);
		}

		public void TestEnableUPECustomisations()
		{
			using (Env.SetTemporaryUserContext(new NullCompanyUserContext(Env.CurrentUserContext)))
			{
				Assert("Default value false", !ItemSet.EnableUPECustomisations);
				ItemSet.EnableUPECustomisations = true;
				Assert("New value", ItemSet.EnableUPECustomisations);
			}

			Assert("Default value false", !ItemSet.EnableUPECustomisations);
			ItemSet.EnableUPECustomisations = true;
			Assert("New value", ItemSet.EnableUPECustomisations);
		}

		public void TestBranchToUseForUPECustomisations()
		{
			TestGenericRegistryItem(ItemSet.BranchToUseForUPECustomisationsItem,
				"BISIUploadBranchToUseItem",
				ExpectedCategory,
				(NoResString)"Branch to use for UPE Customisations",
				(NoResString)"Required for each company with UPE customisations enabled, time zone of selected branch will be used",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForController | RegistryOptions.IsValueMandatory);
		}

		public void TestExportShippingAgentRegistryItem()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "EXPSHPAGT";
			Factory.Save();
			var shippingAgentObject = new ShippingAgentObject
			{
				ShippingAgentAddress = orgHeader.MainAddress.PK
			};
			ItemSet.DefaultLevelOneExportCarrierAgentItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, shippingAgentObject);
			Assert(ItemSet.DefaultLevelOneExportCarrierAgentItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ShippingAgentAddress.IsEmpty);
			AssertEquals(orgHeader.MainAddress.PK, ItemSet.DefaultLevelOneExportCarrierAgentItem.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).ShippingAgentAddress);
		}

		public void TestImportShippingAgentRegistryItem()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "IMPSHPAGT";
			Factory.Save();
			var shippingAgentObject = new ShippingAgentObject
			{
				ShippingAgentAddress = orgHeader.MainAddress.PK
			};
			ItemSet.DefaultLevelOneImportCarrierAgentItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, shippingAgentObject);
			Assert(ItemSet.DefaultLevelOneImportCarrierAgentItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ShippingAgentAddress.IsEmpty);
			AssertEquals(orgHeader.MainAddress.PK, ItemSet.DefaultLevelOneImportCarrierAgentItem.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).ShippingAgentAddress);
		}

		const string ExpectedCategory = "UPS Client Extensions";
		const string ExpectedUPSScreeningCategory = ExpectedCategory + "/Screening";
		const string ExpectedUPSAUScreeningCategory = ExpectedUPSScreeningCategory + "/AU";
		const string ExpectedUPSSGScreeningCategory = ExpectedUPSScreeningCategory + "/SG";
		static readonly Guid ExpectedDefaultNotificationGroupPK = new Guid("55896E13-12BE-4FD4-AC94-2956795A5BE2");
		static readonly Guid ExpectedDefaultBISIWarningReportGroupPK = new Guid("5952B279-2F88-44EB-8A9B-44D332EB6E27");

		class NullCompanyUserContext : UserContext, IUserContext
		{
			public NullCompanyUserContext(IUserContext currentUserContext)
			{
				this.currentUserContext = currentUserContext;
			}
			readonly IUserContext currentUserContext;

			IBranch IUserContext.Branch
			{
				get { return currentUserContext.Branch; }
			}

			IDepartment IUserContext.Department
			{
				get { return currentUserContext.Department; }
			}

			IUser IUserContext.User
			{
				get { return currentUserContext.User; }
			}

			ICompany IUserContext.Company
			{
				get { return null; }
			}
		}
	}
}
