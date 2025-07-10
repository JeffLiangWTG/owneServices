using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Billing.Collectors;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Billing;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.SystemDataRegistry;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SystemDataRegistry))]
	sealed partial class SystemDataRegistryTest : RegistryItemSetTestCaseWithFactory<SystemDataRegistry>
	{
		[UseSnapshotProtection]
		public void TestSaveEDocsStorageProviderToS3WhenDocManagerDBIsReadOnly()
		{
			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.EDocsStorageProviders.Code.DB);
			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "someurl");
			SystemDataRegistry.Instance.DocManagerStorageBucketName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "somebucket");
			SystemDataRegistry.Instance.EDocsStorageAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "key/access");

			using (var adminConn = Db.NewAdminConnection())
			{
				var docDatabaseSD001 = Db.DatabaseName.Trim() + "_SD001";
				var docDatabaseSD002 = Db.DatabaseName.Trim() + "_SD002";
				try
				{
					AdoTestUtils.CreateDbIfNotExists(adminConn, docDatabaseSD001);
					AdoTestUtils.CreateDbIfNotExists(adminConn, docDatabaseSD002);

					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD001, false);
					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD002, false);
					AssertExceptionThrown<RegistryValidationException>(() => ItemSet.EDocsStorageProvider.DataType.ValidateBeforeRegistryFormSave(ItemSet.EDocsStorageProvider, Enterprise.Core.Constants.EDocsStorageProviders.Code.S3, Guid.Empty, Guid.Empty, Guid.Empty));

					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD001, false);
					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD002, true);
					AssertExceptionThrown<RegistryValidationException>(() => ItemSet.EDocsStorageProvider.DataType.ValidateBeforeRegistryFormSave(ItemSet.EDocsStorageProvider, Enterprise.Core.Constants.EDocsStorageProviders.Code.S3, Guid.Empty, Guid.Empty, Guid.Empty));

					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD001, true);
					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD002, false);
					AssertExceptionThrown<RegistryValidationException>(() => ItemSet.EDocsStorageProvider.DataType.ValidateBeforeRegistryFormSave(ItemSet.EDocsStorageProvider, Enterprise.Core.Constants.EDocsStorageProviders.Code.S3, Guid.Empty, Guid.Empty, Guid.Empty));

					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD001, true);
					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD002, true);
					AssertNoExceptionThrown(() => ItemSet.EDocsStorageProvider.DataType.ValidateBeforeRegistryFormSave(ItemSet.EDocsStorageProvider, Enterprise.Core.Constants.EDocsStorageProviders.Code.S3, Guid.Empty, Guid.Empty, Guid.Empty));
				}
				finally
				{
					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD001, true);
					adminConn.AlterDbWriteableStateForDocManager(docDatabaseSD002, true);
					AdoTestUtils.DropDbIfExists(adminConn, docDatabaseSD001);
					AdoTestUtils.DropDbIfExists(adminConn, docDatabaseSD002);
				}
			}
		}

		[ExpectNoExceptions]
		public override void TestItemsCanGetAndSetValue()
		{
			object value;
			var biItemSetOrder = new string[] { "BiDataWarehouseServer", "BiReportUserCredential", "BiAnalysisServer", "BiAuditServer", "BiPowerBiWebPortalUrl" };
			var nonOrderedItems = AllItems.ToList().Where(regi => !biItemSetOrder.Contains(regi.Name));

			foreach (IRegistryItem item in nonOrderedItems)
			{
				if (item.GetType() != typeof(LinkRegistryItem))
				{
					value = item.DefaultValue;
					int option = (int)(item.Options & RegistryOptions.CannotCallParameterlessValueGetter);
					if (option == 0)
					{
						value = item.Value;
					}
					value = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
					using (item.DataType.SuspendValidation())
					{
						RegistryTester.SetValue(item, value);
					}
				}
			}
		}

		[ExpectNoExceptions]
		public void TestItemsCanGetAndSetValue_BiItems()
		{
			object value;
			var biItemSetOrder = new string[] { "BiDataWarehouseServer", "BiReportUserCredential", "BiAnalysisServer", "BiAuditServer", "BiPowerBiWebPortalUrl" };
			var orderedItems = AllItems.ToList().Where(regi => biItemSetOrder.Contains(regi.Name)).OrderBy(regi =>
			{
				var idx = Array.IndexOf(biItemSetOrder, regi.Name);
				return idx == -1 ? int.MaxValue : idx;
			});

			foreach (IRegistryItem item in orderedItems)
			{
				if (item.GetType() != typeof(LinkRegistryItem))
				{
					value = item.DefaultValue;
					int option = (int)(item.Options & RegistryOptions.CannotCallParameterlessValueGetter);
					if (option == 0)
					{
						value = item.Value;
					}
					value = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
					using (item.DataType.SuspendValidation())
					{
						RegistryTester.SetValue(item, value);
					}
					SetTempValue(item.Name);
				}
			}
		}

		void SetTempValue(string name)
		{
			switch (name)
			{
				case "BiDataWarehouseServer":
					SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "testvalue");
					break;
				case "BiReportUserCredential":
					SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BiReportCredential { Domain = "test", UserName = "test", Password = "test" });
					break;
				case "BiAnalysisServer":
					SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "testvalue");
					break;
				case "BiAuditServer":
					SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "testvalue");
					break;
				case "BiPowerBiWebPortalUrl":
					SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "testvalue");
					break;
				default:
					break;
			}
		}

		public void TestReportMaxConnectionsLessThanSRRSecondaryProcessCount()
		{
			const string scheduledState1 = "<HostedServiceSerializableSettings><SecondaryProcessesMaxCount>2</SecondaryProcessesMaxCount></HostedServiceSerializableSettings>";
			const string scheduledState2 = "<HostedServiceSerializableSettings><SecondaryProcessesMaxCount>3</SecondaryProcessesMaxCount></HostedServiceSerializableSettings>";
			string sql = $@"INSERT INTO dbo.StmScheduleTask (S5_PK,S5_ScheduleDescription,S5_TaskPeriod,S5_WeekDaysOnly,S5_TaskPeriodCount,S5_DayNumber,S5_DayList,
S5_MonthNumber,S5_WeekDayOccurrenceNumber,S5_StartDate,S5_EndAfterCount,S5_EndDate,S5_ScheduleActualRunCount,S5_AccountingPeriodScheduleFrstRun,S5_DateScheduleFirstRun,S5_ScheduleType,
S5_TypeOfDocument,S5_NextScheduledPrintRunTimeUtc,S5_CurrentPrintRunTime,S5_IsActive,S5_IsPrivate,S5_ScheduleState,S5_DailyStartTime,S5_DailyEndTime,
S5_ParentTableCode,S5_ParentID,S5_GB,S5_RunTimeInMinutes,S5_SystemCreateTimeUtc,S5_SystemCreateUser,S5_SystemLastEditTimeUtc,S5_SystemLastEditUser,S5_GS_NKPrintUser)
									VALUES ('{Guid.NewGuid()}', 'Scheduled Report Runner', 'T', 0, 5, 0, 'NNNNNNN', 0, 0, @DATE1, 0, NULL,0,0,NULL,'SRR','DOC',
@DATE2,NULL,1,1,dbo.CLRCompressStringAsBytes('{scheduledState1}'),NULL,NULL,'SH',NULL,NULL,0,
NULL,'',@DATE3,'E','')
INSERT INTO dbo.StmScheduleTask (S5_PK,S5_ScheduleDescription,S5_TaskPeriod,S5_WeekDaysOnly,S5_TaskPeriodCount,S5_DayNumber,S5_DayList,
S5_MonthNumber,S5_WeekDayOccurrenceNumber,S5_StartDate,S5_EndAfterCount,S5_EndDate,S5_ScheduleActualRunCount,S5_AccountingPeriodScheduleFrstRun,S5_DateScheduleFirstRun,S5_ScheduleType,
S5_TypeOfDocument,S5_NextScheduledPrintRunTimeUtc,S5_CurrentPrintRunTime,S5_IsActive,S5_IsPrivate,S5_ScheduleState,S5_DailyStartTime,S5_DailyEndTime,
S5_ParentTableCode,S5_ParentID,S5_GB,S5_RunTimeInMinutes,S5_SystemCreateTimeUtc,S5_SystemCreateUser,S5_SystemLastEditTimeUtc,S5_SystemLastEditUser,S5_GS_NKPrintUser)
									VALUES ('{Guid.NewGuid()}', 'Scheduled Report Runner', 'T', 0, 5, 0, 'NNNNNNN', 0, 0, @DATE1, 0, NULL,0,0,NULL,'SRR','DOC',
@DATE2,NULL,1,1,dbo.CLRCompressStringAsBytes('{scheduledState2}'),NULL,NULL,'SH',NULL,NULL,0,
NULL,'',@DATE3,'E','')";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@DATE1", System.Data.SqlDbType.DateTime, DateTime.Today);
				command.AddParameter("@DATE2", System.Data.SqlDbType.DateTime, DateTime.Today.AddDays(1));
				command.AddParameter("@DATE3", System.Data.SqlDbType.DateTime, DateTime.Today.AddDays(-1));
				command.ExecuteNonQuery();
			}
			AssertExceptionThrown<RegistryValidationException>("ReportMaxConnections can't be less than SRRSecondaryProcessCount", () => ItemSet.ReportMaxConnections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1));
			AssertNoExceptionThrown("ReportMaxConnections can't be less than SRRSecondaryProcessCount", () => ItemSet.ReportMaxConnections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4));
		}

		public void TestCachedTables()
		{
			var defaultValue = string.Join(",", new[]
			{
				BMComponentSchema.Constants.TableName,
				BMComponentResourceLinkSchema.Constants.TableName,
				BMSystemSchema.Constants.TableName,
				BMSystemWorkflowDeterminerSchema.Constants.TableName,

				GlbBranchSchema.Constants.TableName,
				GlbCapabilitySchema.Constants.TableName,
				GlbCompanySchema.Constants.TableName,
				GlbDepartmentSchema.Constants.TableName,
				GlbGroupSchema.Constants.TableName,
				GlbGroupLinkSchema.Constants.TableName,
				GlbHolidaySchema.Constants.TableName,
				GlbResourceCapabilityPivotSchema.Constants.TableName,
				GlbStaffSchema.Constants.TableName,
				GlbStaffHolidaySchema.Constants.TableName,
				GlbWorkTimeSchema.Constants.TableName,

				RefAirlineSchema.Constants.TableName,
				RefAirlineEFreightRuleSchema.Constants.TableName,
				RefCarrierConsortiumSchema.Constants.TableName,
				RefCityPCodePivotSchema.Constants.TableName,
				RefCityTownSchema.Constants.TableName,
				RefCommodityCodeSchema.Constants.TableName,
				RefComplianceListSchema.Constants.TableName,
				RefContainerSchema.Constants.TableName,
				RefContainerCodeMapSchema.Constants.TableName,
				RefContainerStockSchema.Constants.TableName,
				RefCountrySchema.Constants.TableName,
				RefCountryRequiredDocumentSchema.Constants.TableName,
				RefCountryRulesSchema.Constants.TableName,
				RefCountryStatesSchema.Constants.TableName,
				RefCurrencySchema.Constants.TableName,
				RefDocSourceSchema.Constants.TableName,
				RefDocTypeSchema.Constants.TableName,
				RefDomesticCartageZoneSchema.Constants.TableName,
				"RefEnergySource",
				"RefEnergySourceFactor",
				RefEquipmentSchema.Constants.TableName,
				RefEquipmentConfigSchema.Constants.TableName,
				RefEquipmentConfigItemSchema.Constants.TableName,
				RefEquipmentTemplateSchema.Constants.TableName,
				RefExchangeRateSchema.Constants.TableName,
				RefLanguageTextSchema.Constants.TableName,
				RefLatLongPostcodeSchema.Constants.TableName,
				RefLocoMapSchema.Constants.TableName,
				RefNMFCSchema.Constants.TableName,
				RefOrgConsortiumPivotSchema.Constants.TableName,
				RefPackTypeSchema.Constants.TableName,
				RefPacksSchema.Constants.TableName,
				RefPostCodeSchema.Constants.TableName,
				RefPremisesGateCodeSchema.Constants.TableName,
				RefServiceLevelSchema.Constants.TableName,
				RefShippingLineSchema.Constants.TableName,
				RefTimeZoneSetSchema.Constants.TableName,
				RefTimeZoneSchema.Constants.TableName,
				RefTimeZoneRuleSchema.Constants.TableName,
				RefTransitTimeSchema.Constants.TableName,
				RefUNLOCOSchema.Constants.TableName,
				RefZoneHeaderSchema.Constants.TableName,
				RefZonePivotSchema.Constants.TableName,

				StmModuleFilterSchema.Constants.TableName,
				StmModuleFilterUserDataSchema.Constants.TableName,
				StmServiceHostSchema.Constants.TableName,

				TagDefinitionSchema.Constants.TableName,

				RefLocalLanguageSchema.Constants.TableName,

				ProcessFieldChangeRuleSchema.Constants.TableName,
				ProcessFieldChangeRuleFieldSchema.Constants.TableName,

				WhsAreaSchema.Constants.TableName,
				WhsCartonGroupSchema.Constants.TableName,
				WhsCartonSizeSchema.Constants.TableName,
				WhsCartonGroupSizeLinkSchema.Constants.TableName,
				WhsClientParameterByWarehouseSchema.Constants.TableName,
				WhsClientPickPackParamsByWhsSchema.Constants.TableName,
				WhsInventoryHeldCodeSchema.Constants.TableName,
				WhsLocationTypeSchema.Constants.TableName,
				WhsPickFaceSchema.Constants.TableName,
				WhsProductParamsByWhsAndClientSchema.Constants.TableName,
				WhsPutawayGroupSchema.Constants.TableName,
				WhsRowSchema.Constants.TableName,
				WhsSalesChannelSchema.Constants.TableName,
				WhsWarehouseSchema.Constants.TableName,
			});

			TestStringRegistryItem(Instance.CachedTables,
				"CachedTables",
				Categories.Optimization,
				"Cached Tables",
				"Allows caching at a low level of commonly used, infrequently changed tables. Separate with commas for multiple tables.",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.Default,
				defaultValue,
				CharacterCase.Normal);

			var tables = defaultValue.Split(',');
			var tablesAsInList = string.Join(",", tables.Select(o => string.Format(CultureInfo.InvariantCulture, "'{0}'", o)));
			var tableCount = TestConnection.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN ({tablesAsInList})");

			AssertEquals("All Tables should exist.", tableCount, tables.Length);
		}

		public void TestMaxNbOfFactoriesForWarning()
		{
			AssertEquals(0, ItemSet.NumberOfFactoriesNeededForWarningReport.DefaultValue);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.NumberOfFactoriesNeededForWarningReport.Options);
		}

		public void TestMDMAdministrationPanelEnabled()
		{
			TestGenericRegistryItem(ItemSet.MdmAdministrationPanelEnabled,
				"MdmAdministrationPanelEnabled",
				"Master Data/MDM Administration",
				"Administration Panel Enabled",
				"Enable the Administration Panel.",
				RegistryStorageFlags.System,
				ItemSet.IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestDisableNonVerifiablePostcodeWarning()
		{
			TestRegistryItem(ItemSet.DisableNonVerifiablePostcodeWarning, "DisableNonVerifiablePostcodeWarning", Categories.System_TransportZones,
				"Disable Non-verifiable Postcode Warning.",
				"Set this to \"Yes\" to disable the warning when importing a non-verifiable postcode on Transport Zone Sets.",
				RegistryStorageFlags.System, RegistryOptions.Default, false);
		}

		public void TestDisableNonVerifiableCityTownWarning()
		{
			TestRegistryItem(ItemSet.DisableNonVerifiableCityTownWarning, "DisableNonVerifiableCityTownWarning", Categories.System_TransportZones,
				"Disable Non-verifiable City/Town Warning.",
				"Set this to \"Yes\" to disable the warning when importing a non-verifiable city/town on Transport Zone Sets.",
				RegistryStorageFlags.System, RegistryOptions.Default, false);
		}

		public void TestPersonIntelligenceModuleEnabled()
		{
			TestGenericRegistryItem(ItemSet.PersonIntelligenceModuleEnabled,
					"PersonIntelligenceModuleEnabled",
					Categories.Persons,
					"Person Intelligence Module Enabled",
					"Enable the Person Intelligence Module.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false);
		}

		public void TestMaximumNumberOfAddressesForMDMAddressGrid()
		{
			TestGenericRegistryItem(ItemSet.MaximumNumberOfAddressesForMDMAddressGrid,
				"MaximumNumberOfAddressesForMDMAddressGrid",
				"Master Data/MDM Administration",
				"Maximum Number Of Records To Display on Address Grid",
				"The maximum number of records to display on the address filter grid (from 1 to 1000).",
				RegistryStorageFlags.System,
				100);

			AssertExceptionThrown<RegistryValidationException>("Maximum value should be 1000", () => ItemSet.MaximumNumberOfAddressesForMDMAddressGrid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1001));
			AssertExceptionThrown<RegistryValidationException>("Minimum value should be 1", () => ItemSet.MaximumNumberOfAddressesForMDMAddressGrid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0));
		}

		public void TestMaximumNumberOfOrganizationsForMDMDuplicatesOrganizationsGrid()
		{
			TestGenericRegistryItem(ItemSet.MaximumNumberOfOrganizationsForMDMDuplicatesOrganizationsGrid,
				"MaximumNumberOfOrganizationsForMDMDuplicatesOrganizationsGrid",
				"Master Data/MDM Administration",
				"Maximum Number Of Records To Display On Duplicates Organizations Grid",
				"The maximum number of records to display on the duplicates Organizations filter grid (from 1 to 1000).",
				RegistryStorageFlags.System,
				100);

			AssertExceptionThrown<RegistryValidationException>("Maximum value should be 1000", () => ItemSet.MaximumNumberOfOrganizationsForMDMDuplicatesOrganizationsGrid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1001));
			AssertExceptionThrown<RegistryValidationException>("Minimum value should be 1", () => ItemSet.MaximumNumberOfOrganizationsForMDMDuplicatesOrganizationsGrid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0));
		}

		public void TestMDMAdministrationPanelAddressTabEnabled()
		{
			TestGenericRegistryItem(ItemSet.MdmAdministrationPanelAddressTabEnabled,
				"MdmAdministrationPanelAddressTabEnabled",
				"Master Data/MDM Administration",
				"Administration Panel Address Tab Enabled",
				"Enable the Administration Panel Address Tab.",
				RegistryStorageFlags.System,
				ItemSet.IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestMDMAdministrationPanelDuplicatesTabEnabled()
		{
			TestGenericRegistryItem(ItemSet.MdmAdministrationPanelDuplicatesTabEnabled,
				"MdmAdministrationPanelDuplicatesTabEnabled",
				"Master Data/MDM Administration",
				"Administration Panel Duplicates Tab Enabled",
				"Enable the Administration Panel Duplicates Tab.",
				RegistryStorageFlags.System,
				ItemSet.IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestMdmAdministrationPanelOrganizationDuplicatesTabEnabled()
		{
			TestGenericRegistryItem(ItemSet.MdmAdministrationPanelOrganizationDuplicatesTabEnabled,
				"MdmAdministrationPanelOrganizationDuplicatesTabEnabled",
				"Master Data/MDM Administration",
				"Administration Panel Organization Duplicates Tab Enabled",
				"Enable the Administration Panel Organization Duplicates Tab.",
				RegistryStorageFlags.System,
				ItemSet.IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestMdmAdministrationPanelPersonDuplicatesTabEnabled()
		{
			TestGenericRegistryItem(ItemSet.MdmAdministrationPanelPersonDuplicatesTabEnabled,
				"MdmAdministrationPanelPersonDuplicatesTabEnabled",
				"Master Data/MDM Administration",
				"Administration Panel Person Duplicates Tab Enabled",
				"Enable the Administration Panel Person Duplicates Tab.",
				RegistryStorageFlags.System,
				ItemSet.IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestMDMAdministrationPanelDashBoardTabEnabled()
		{
			TestGenericRegistryItem(ItemSet.MdmAdministrationPanelDashBoardEnabled,
				"MdmAdministrationPanelDashBoardTabEnabled",
				"Master Data/MDM Administration",
				"Administration Panel Dashboard Tab Enabled",
				"Enable the Administration Panel Dashboard Tab.",
				RegistryStorageFlags.System,
				ItemSet.IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestBackgroundValidationAutoStart()
		{
			TestGenericRegistryItem(ItemSet.BackgroundValidationSuspended,
				"BackgroundValidationSuspended",
				"Master Data/MDM Administration",
				"Background Validation Suspended",
				"Suspend background validation when open Administration Panel Address Tab.",
				RegistryStorageFlags.System,
				ItemSet.IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
				false);
		}

		#region Persons

		public void TestPersonsEnableDuplicateDetection()
		{
			TestGenericRegistryItem(ItemSet.PersonsEnableDuplicateDetection,
				"PersonsEnableDuplicateDetection",
				"Master Data/Persons/Duplicate Detection",
				"Enable Duplicate Detection",
				"When this registry is set to 'Yes', the system will show a warning when it detects that a potential duplicate is being added.",
				RegistryStorageFlags.System,
				ItemSet.IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestPersonsDuplicateDetectionTimeout()
		{
			TestGenericRegistryItem(ItemSet.PersonsDuplicateDetectionTimeout,
				"PersonsDuplicateDetectionTimeout",
				"Master Data/Persons/Duplicate Detection",
				"Duplicate Detection Timeout",
				"Time in seconds representing timeout for duplicate detection (from 1 to 300).",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				10);

			AssertExceptionThrown<RegistryValidationException>("Maximum value should be 300", () => ItemSet.PersonsDuplicateDetectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 301));
			AssertExceptionThrown<RegistryValidationException>("Minimum value should be 1", () => ItemSet.PersonsDuplicateDetectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0));
		}

		public void TestPersonsMaximumPotentialTargets()
		{
			TestGenericRegistryItem(ItemSet.PersonsMaximumPotentialTargets,
				"PersonsMaximumPotentialTargets",
				"Master Data/Persons/Duplicate Detection",
				"Maximum Potential Targets",
				"The maximum number of potential targets that are loaded during duplicate detection (from 50 to 400).",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				150);

			AssertExceptionThrown<RegistryValidationException>("Maximum value should be 400", () => ItemSet.PersonsMaximumPotentialTargets.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 401));
			AssertExceptionThrown<RegistryValidationException>("Minimum value should be 50", () => ItemSet.PersonsMaximumPotentialTargets.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 49));
		}

		public void TestPersonsExcludeInactivePotentialDuplicates()
		{
			TestGenericRegistryItem(ItemSet.PersonsExcludeInactivePotentialDuplicates,
				"PersonsExcludeInactivePotentialDuplicates",
				"Master Data/Persons/Duplicate Detection",
				"Exclude Inactive Potential Duplicate Records",
				"When this registry is set to 'Yes', The system will exclude all inactive records from the list of potential duplicate results.",
				RegistryStorageFlags.System,
				true);
		}

		public void TestPersonsDeduplicationMinimumConfidenceResult()
		{
			TestGenericRegistryItem(ItemSet.PersonsDeduplicationMinimumConfidenceResult,
				"PersonsDeduplicationMinimumConfidenceResult",
				"Master Data/Persons/Duplicate Detection",
				"Minimum Confidence Rating",
				"Only potential duplicates where the confidence is higher than the selected value will be shown. If Medium is selected only High confidence results will be shown.",
				RegistryStorageFlags.All,
				DeDuplicationMinimumConfidenceRating.Codes.Low);
		}

		public void TestPersonsRepeatedValueLimit()
		{
			TestGenericRegistryItem(ItemSet.PersonsRepeatedValueLimit,
				"PersonsRepeatedValueLimit",
				"Master Data/Persons/Duplicate Detection",
				"Repeated Pattern Limit",
				"Any pattern that is repeated more than the specified number of times will be excluded from duplicate detection. The value can be set between 1 and 200.",
				RegistryStorageFlags.System,
				10);
		}

		public void TestPersonMergePreviewItemsRegistryItem()
		{
			TestGenericRegistryItem(ItemSet.PersonMergePreviewItemsRegistryItem,
				"PersonMergePreviewItems",
				"Master Data/Persons/Person Merge",
				"Person Merge Preview Items",
				"This registry item specifies the order and visibility for properties that should be displayed on the Person Merge Preview form.",
				RegistryStorageFlags.System);
		}

		public void TestPersonMergeWithPasswordNotificationEmailTemplate()
		{
			TestGenericRegistryItem(ItemSet.PersonMergeWithPasswordNotificationEmailTemplate,
				"PersonMergeWithPasswordNotificationEmailTemplate",
				"Master Data/Persons/Person Merge",
				"Person Merge With Password Notification Email Template",
				"Template that will be used to notify users their accounts have been merged under a single password.",
				RegistryStorageFlags.System);
		}

		#endregion

		#region Geography

		public void TestUserDefinedGeographyType()
		{
			TestGenericRegistryItem(ItemSet.UserDefinedGeographyType,
				"UserDefinedGeographyType",
				"Master Data/Geography",
				"User Defined Geography Type",
				"Allow user to customize the geography type. The user defined geography code should start with 'U' and consist of 4 numbers or letters.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue);

			AssertEquals("DataType.CodeMaxLength", 4, ((CodeDescriptionPairListRegistryDataType)ItemSet.UserDefinedGeographyType.DataType).CodeMaxLength);
			AssertEquals("DefaultValue.Count", 0, ItemSet.UserDefinedGeographyType.DefaultValue.Count);

			var newValue = new CodeDescriptionPairList();
			newValue.AddPair("UABC", "D1");

			RegistryTester.SetValue(ItemSet.UserDefinedGeographyType, newValue);
			var actualNewValue = ItemSet.UserDefinedGeographyType.Value;

			AssertEquals("Value.Count", 1, actualNewValue.Count);
			AssertEquals("Value[0].Code", "UABC", actualNewValue[0].Code);
			AssertEquals("Value[0].Description", "D1", actualNewValue[0].Description);

			var dataType = (UserDefinedGeographyTypeRegistryDataType)ItemSet.UserDefinedGeographyType.DataType;
			var editInfo = (CodeDescriptionPairListEditorInfo)ItemSet.UserDefinedGeographyType.EditorInfo;

			CombineAssertions(() =>
			{
				AssertEquals(false, dataType.AllowDuplicateCodes);
				AssertEquals(false, dataType.AllowEmptyCodes);

				AssertEquals(true, editInfo.ShowCodeColumn);
				AssertEquals(true, editInfo.ShowDescriptionColumn);
				AssertEquals(CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, editInfo.CodeFieldCasing);
				AssertEquals(CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, editInfo.DescriptionFieldCasing);
			});
		}

		public void TestEnableAddAndEditAndDeleteLogsResetsCacheOnSet()
		{
			var tempValues1 = new EnableAddEditAndDeleteLogsItemCollection();
			tempValues1.Add(new EnableAddEditAndDeleteLogsItem()
			{
				Table = "test1",
				EnableADDLogs = true,
				EnableEDTLogs = true,
				EnableDELLogs = true,
			});

			var tempValues2 = new EnableAddEditAndDeleteLogsItemCollection();
			tempValues2.Add(new EnableAddEditAndDeleteLogsItem()
			{
				Table = "test2",
				EnableADDLogs = false,
				EnableEDTLogs = false,
				EnableDELLogs = false,
			});

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValues1))
			{
				Assert(SystemDataRegistry.Instance.AuditLogsEnabledFor("test1").Value.IsEnabledForADD);
				using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValues2))
				{
					Assert(!SystemDataRegistry.Instance.AuditLogsEnabledFor("test2").Value.IsEnabledForADD);
				}
			}
		}

		public void TestEnableAddAndEditLogsItemsRegistryItem()
		{
			TestGenericRegistryItem(ItemSet.EnableAddEditAndDeleteLogsItemsRegistryItem,
				"EnableAddEditAndDeleteLogsItems",
				Categories.System_AuditLogs,
				"Enable Add/Edit/Delete Logs",
				"Control whether or not Audit Logs for add/edit/delete record operations is created for a given table.",
				RegistryStorageFlags.System,
				RegistryOptions.Default);
		}

		public void TestEnableAddAndEditLogsItemsRegistryItem_WithAllOveriddenItems_ContainsAllOverriden()
		{
			var items = EnableAddEditAndDeleteLogsItemCollection.DefaultValue
				 .Cast<EnableAddEditAndDeleteLogsItem>()
				 .Select(o => new EnableAddEditAndDeleteLogsItem()
				 {
					 Table = o.Table,
					 EnableADDLogs = !o.EnableADDLogs,
					 EnableEDTLogs = !o.EnableEDTLogs,
					 EnableDELLogs = !o.EnableDELLogs,
				 });

			var temporaryValue = new EnableAddEditAndDeleteLogsItemCollection();
			temporaryValue.AddRange(items);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryValue))
			{
				var result = ((EnableAddEditAndDeleteLogsItemCollection)SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty))
					.Cast<EnableAddEditAndDeleteLogsItem>()
					.All(o => items
						.Any(x =>
							x.Table == o.Table
							&& x.EnableADDLogs == o.EnableADDLogs
							&& x.EnableEDTLogs == o.EnableEDTLogs
							&& x.EnableDELLogs == o.EnableDELLogs));

				Assert(result);
			}
		}

		public void TestEnableAddAndEditLogsItemsRegistryItem_WithSomeOverriddenItems_ContainsOverridenAndDefaults()
		{
			var defaultValue = EnableAddEditAndDeleteLogsItemCollection.DefaultValue;
			var itemIndex = defaultValue.Count / 2;

			var defaultItems = Enumerable.Range(itemIndex, Math.Max(0, defaultValue.Count - itemIndex))
				.Select(o => defaultValue[o]);

			var overriddenItems = Enumerable.Range(0, itemIndex)
				.Select(o => new EnableAddEditAndDeleteLogsItem()
				{
					Table = defaultValue[o].Table,
					EnableADDLogs = !defaultValue[o].EnableADDLogs,
					EnableEDTLogs = !defaultValue[o].EnableEDTLogs,
					EnableDELLogs = !defaultValue[o].EnableDELLogs,
				});

			var temporaryValue = new EnableAddEditAndDeleteLogsItemCollection();
			temporaryValue.AddRange(overriddenItems);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryValue))
			{
				var itemsToCheck = defaultItems.Union(overriddenItems);
				var items = (EnableAddEditAndDeleteLogsItemCollection)SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

				var result = itemsToCheck.All(o => items
					.Cast<EnableAddEditAndDeleteLogsItem>()
					.Any(x =>
						x.Table == o.Table
						&& x.EnableADDLogs == o.EnableADDLogs
						&& x.EnableEDTLogs == o.EnableEDTLogs
						&& x.EnableDELLogs == o.EnableDELLogs));

				Assert(result);
			}
		}

		public void TestEnableEnhancedLogging()
		{
			TestRegistryItem(ItemSet.EnableEnhancedLogging,
				"EnableEnhancedLogging",
				Categories.System_AuditLogs,
				"Enable Enhanced Logging",
				"When enabled the Change Logs in the Registry will provide additional details of the changes made to Registry settings.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		public void TestMaximumGeographyPointNumberLimit()
		{
			TestRegistryItem(ItemSet.MaximumGeographyPointNumberLimit,
				"MaximumGeographyPointNumberLimit",
				Categories.Geography,
				"Maximum Geography Point Number Limit",
				"Limit the maximum point number of a KML file.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				10000,
				1,
				10000);
		}

		public void TestGeographyUnreleasedFunctions()
		{
			TestGenericRegistryItem(ItemSet.GeographyUnreleasedFunctions,
				"GeographyUnreleasedFunctions",
				Categories.Geography,
				"Enable Geography Unreleased Functions",
				"When turned on, the user can use geography unreleased functions, such as create new shape geography.",
				RegistryStorageFlags.System,
				ItemSet.IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestAuditStmALogDeciderHasExpectedTableDefaultRegistryValues()
		{
			// Arrange
			var auditStmALogDecider = ObjectFactory.Get<IAuditStmALogDecider>();
			var registryDefaultValues = (EnableAddEditAndDeleteLogsItemCollection)Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.DefaultValue;
			// Act
			// Assert
			AssertEquals("The registry item collection should be the same count as the StmALogDecider default config values count", auditStmALogDecider.AutoLoggedTablesDefaultConfigurationValues.Count, registryDefaultValues.Count);
			foreach (var item in registryDefaultValues)
			{
				Assert($"the item of name {item.Table} should be present in StmALogDecider default config values.", auditStmALogDecider.AutoLoggedTablesDefaultConfigurationValues.ContainsKey(item.Table));
			}
		}

		#endregion

		#region Enrichment

		public void TestEnrichmentWebServiceAddress()
		{
			TestRegistryItem(ItemSet.EnrichmentWebServiceAddress,
				"EnrichmentWebServiceAddress",
				"Master Data/Enrichment",
				"Enrichment Web Service Address",
				"URL for the Enrichment Web Service.",
				RegistryStorageFlags.System,
				TextEditorType.Url,
				RegistryOptions.IsOnlyForDevelopers,
				string.Empty);
		}

		#endregion

		public override void TestAccessingValuesOnlyDoesNotDemandResourceStrings()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				string sqlProc = string.Format(CultureInfo.InvariantCulture, "[{0}].sys.sp_cdc_enable_db", Db.DatabaseName);
				using (var cmd = testConnection.Command(sqlProc))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.ExecuteNonQuery();
				}
				base.TestAccessingValuesOnlyDoesNotDemandResourceStrings();
			}
		}

		public void TestPhoneDialingUriProtocols()
		{
			TestGenericRegistryItem(ItemSet.PhoneDialingUriProtocols,
				"PhoneDialingUriProtocol",
				SystemDataRegistry.Categories.System_Miscellaneous,
				"Phone Dialing URI Protocols",
				"The list of protocols to use when constructing a URI from a phone number to make a call. The full URI will be <protocol>:<phone number>. For example, sip:80012299.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}

		public void TestSystemMergeCompanyCodeMappingValidation()
		{
			var mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("AAA", "EDI");
			mappingList.AddPair("BBB", "DEM");

			AssertEquals("No errors", "", ItemSet.SystemMergeCompanyCodeMapping.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));

			mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("AAA", "EDI");
			mappingList.AddPair("BBB", "EDI");

			AssertEquals("Should validate with error", "You cannot map more than one Source Company Code to a single Replacement Company Code. Duplicated Replacement Company Code: EDI", ItemSet.SystemMergeCompanyCodeMapping.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));

			mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("AAA", "EDI");
			mappingList.AddPair("", "DEM");

			AssertEquals("Should validate with error", "You cannot enter an item with no Source Company Code.", ItemSet.SystemMergeCompanyCodeMapping.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));

			mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("AAA", "");
			mappingList.AddPair("BBB", "");

			AssertEquals("Multiple blank are allowed", "", ItemSet.SystemMergeCompanyCodeMapping.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestAllowMessageModificationBeforeSending()
		{
			TestRegistryItem(ItemSet.AllowMessageModificationBeforeSending, "AllowMessageModificationBeforeSending", "System/Messaging", "Allow Message Modification", "This registry item specifies whether or not users are allowed to modify a message before it is sent.", RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers, false);
		}

		public void TestMessageModificationBeforeSendingAuthorisationGroup()
		{
			TestRegistryItem(ItemSet.MessageModificationBeforeSendingAuthorisationGroup, "MessageModificationBeforeSendingAuthorisationGroup", "System/Messaging", "Message Modification Authorization Group", "The group that are allowed to modify a message before it is sent.", RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsValueOptional, RegistryFindBoxCollection.GlbGroup, Guid.Empty);
			AssertEquals(typeof(GuidFindBoxRegistryEditorInfo), ItemSet.MessageModificationBeforeSendingAuthorisationGroup.EditorInfo.GetType());
		}

		public void TestAccountingTransactionsExport()
		{
			TestGenericRegistryItem(ItemSet.AccountingTransactionsExport,
					"AccountingTransactionsExport",
					"System/Data Export Settings",
					"Accounting Transactions",
					"Automatic Export of Accounting Transactions in Standard XML-Format (The next time to run is shown as local time for a Branch with the earliest Time Zone)",
					RegistryStorageFlags.Company,
					RegistryOptions.NotCached
					);
		}

		public void TestImportBillingInfoFromAgencyXmlFile()
		{
			TestGenericRegistryItem(ItemSet.ImportBillingInfoFromAgencyXmlFile,
				"ImportBillingInfoFromAgencyXmlFile",
				SystemDataRegistry.Categories.System_DataImportSettings_ShippingBillofLading,
				"Import Billing Information When Importing Bills and Bookings",
				"Should the billing information in the XML be imported when importing a bill of lading or a booking.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				Enterprise.Core.Constants.ImportBillingInfoFromXMLMethod.Codes.NewOnly
				);
		}

		public void TestIncludeBillingInfoInAgencyXMLFile()
		{
			TestGenericRegistryItem(ItemSet.IncludeBillingInfoInAgencyXMLFile,
				"IncludeBillingInfoInAgencyXMLFile",
				SystemDataRegistry.Categories.System_DataExportSettings_ShippingBillofLading,
				"Include Billing Information in XML File",
				"Include charges into the interface file",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				ItemSet.GetIncludeBillingInfoInAgencyXMLFileList().DefaultCode);

			AssertEquals("IncludeBillingInfoInAgencyXMLFile.Value default", Enterprise.Core.Constants.IncludeBillingInfoInXMLMethod.Codes.All, ItemSet.IncludeBillingInfoInAgencyXMLFile.Value);

			ItemSet.IncludeBillingInfoInAgencyXMLFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.IncludeBillingInfoInXMLMethod.Codes.NotInclude);
			AssertEquals("IncludeBillingInfoInAgencyXMLFile.Value", Enterprise.Core.Constants.IncludeBillingInfoInXMLMethod.Codes.NotInclude, ItemSet.IncludeBillingInfoInAgencyXMLFile.Value);
		}

		public void TestIncludeLocValAndExRateWhenExportingBilling()
		{
			TestGenericRegistryItem(ItemSet.IncludeLocValAndExRateWhenExportingBilling,
				"IncludeLocValAndExRateWhenExportingBilling",
				"System/Data Export Settings",
				"Include Local Amount and Exchange Rate in Billing Information Export",
				$@"When this registry is set to 'Yes', {Enterprise.Core.Constants.ProductName} will include the local sell and cost values, as well as the cost and sell exchange rates for each charge line in the exported XML.
Billing information can be sent with operations jobs such as shipments when the relevant registry item is enabled (for shipments, see the registry setting System > Data Export Settings > Shipment Export > Include Billing Information in XML File)",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				ItemSet.IncludeLocValAndExRateWhenExportingBilling.DefaultValue);

			Guid companyPK = new Guid();
			AssertEquals("Default IncludeLocValAndExRateWhenExportingBilling", false, ItemSet.IncludeLocValAndExRateWhenExportingBilling.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));

			ItemSet.IncludeLocValAndExRateWhenExportingBilling.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Setting IncludeLocValAndExRateWhenExportingBilling", true, ItemSet.IncludeLocValAndExRateWhenExportingBilling.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));
		}

		public void TestIncludeBillingInfoInShipmentXML()
		{
			TestGenericRegistryItem(ItemSet.IncludeBillingInfoInShipmentXML,
						"IncludeBillingInfoInShipmentXML",
						SystemDataRegistry.Categories.System_DataExportSettings_ShipmentExport,
						"Include Billing Information in XML File",
						"Include all costs and charges from the operations job's billing tab in the XML. Warning: By turning this on, anyone who receives your XML files will be able to see all charges and costs on this job.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);

			AssertEquals("Default IncludeBillingInfoInShipmentXML", false, ItemSet.IncludeBillingInfoInShipmentXML.Value);

			ItemSet.IncludeBillingInfoInShipmentXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Setting IncludeBillingInfoInShipmentXML", true, ItemSet.IncludeBillingInfoInShipmentXML.Value);
		}

		public void TestIncludeTransportBookingInShipmentXML()
		{
			TestGenericRegistryItem(ItemSet.IncludeTransportBookingInShipmentXML,
				"IncludeTransportBookingInShipmentXML",
				SystemDataRegistry.Categories.System_DataExportSettings_ShipmentExport,
				(NoResString)"Include Transport Booking in Shipment XML File",
				(NoResString)"Universal shipment exported from a forwarding shipment can optionally include transport bookings.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				false);

			AssertEquals("Default IncludeTransportBookingInShipmentXML", false, ItemSet.IncludeTransportBookingInShipmentXML.Value);

			ItemSet.IncludeTransportBookingInShipmentXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Setting IncludeTransportBookingInShipmentXML", true, ItemSet.IncludeTransportBookingInShipmentXML.Value);
		}

		public void TestIncludeBillingInfoInWarehouseXML()
		{
			TestGenericRegistryItem(ItemSet.IncludeBillingInfoInWarehouseXML,
						"IncludeBillingInfoInWarehouseXML",
						SystemDataRegistry.Categories.System_DataExportSettings_WarehouseExport,
						"Include Billing Information in XML File",
						"Include all costs and charges from the warehouse job's billing tab in the XML. Warning: By turning this on, anyone who receives your XML files will be able to see all charges and costs on this job.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);

			AssertEquals("Default IncludeBillingInfoInWarehouseXML", false, ItemSet.IncludeBillingInfoInWarehouseXML.Value);

			ItemSet.IncludeBillingInfoInWarehouseXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Setting IncludeBillingInfoInWarehouseXML", true, ItemSet.IncludeBillingInfoInWarehouseXML.Value);
		}

		public void TestARAPBalancesUpdateImportDirectoryItem()
		{
			AssertEquals("", ItemSet.ARAPBalancesUpdateImportDirectoryItem.DefaultValue);
			AssertEquals("ARAPBalancesUpdateImportDirectoryItem", ItemSet.ARAPBalancesUpdateImportDirectoryItem.Name);
			AssertEquals("Update AP AR Account Balances Import Directory", ItemSet.ARAPBalancesUpdateImportDirectoryItem.Caption);
			AssertEquals("Directory to be used by automatic organization balance updates import", ItemSet.ARAPBalancesUpdateImportDirectoryItem.Hint);
			AssertEquals("System/Data Import Settings/Update Account Balances", ItemSet.ARAPBalancesUpdateImportDirectoryItem.Category);
			AssertEquals(RegistryStorageFlags.System, ItemSet.ARAPBalancesUpdateImportDirectoryItem.Storage);

			ItemSet.ARAPBalancesUpdateImportDirectoryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.TempPath);
			AssertEquals("Default value", Env.TempPath, ItemSet.ARAPBalancesUpdateImportDirectoryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestOrganisationMatchingUpdateAccountBalances()
		{
			TestRegistryItem(ItemSet.OrganisationMatchnigTypeItem, "OrganisationMatchnigTypeItem", SystemDataRegistry.Categories.System_DataImportSettings_UpdateAccountBalances,
						"Organization Matching Type",
						"This setting only applies to Legacy XML which is imported automatically. Select LEG if the organization codes in the XML are foreign, select ENT if the organization codes in the XML are the same as the organization codes in this CargoWise database.",
						RegistryStorageFlags.System,
						SystemDataRegistry.Instance.OrganisationMatchingList,
						SystemDataRegistry.Instance.OrganisationMatchingList.DefaultCode);
		}

		public void TestEU_AutoPopulateInvoicePackagingAndContainerDetailsFromOrderLines()
		{
			var item = SystemDataRegistry.Instance.AutoPopulateInvoicePackagingAndContainerDetailsFromOrderLines;
			AssertEquals(true, item.Category.Contains("Customs/Country or Region Specific/European Union (common)"));
		}

		public void TestOrganisationMatching()
		{
			AssertEquals("Default value", OrganisationImportMatchingType.LegacyCodeMatching, ItemSet.OrganisationMatching);
			ItemSet.OrganisationMatching = OrganisationImportMatchingType.OrganisationCodeMatching;
			AssertEquals("Value", OrganisationImportMatchingType.OrganisationCodeMatching, ItemSet.OrganisationMatching);
		}

		public void TestOrganisationMatchingItem()
		{
			TestRegistryItem(ItemSet.OrganisationMatchingItem, "OrganisationMatchingItem", SystemDataRegistry.Categories.System_DataImportSettings_Organizations,
				 "Organization Matching",
				 "This setting only applies to Legacy XML which is imported automatically. Select LEG if the organization codes in the XML are foreign, select ENT if the organization codes in the XML are the same as the organization codes in this CargoWise database.",
				 RegistryStorageFlags.System,
				 SystemDataRegistry.Instance.OrganisationMatchingList,
				 SystemDataRegistry.Instance.OrganisationMatchingList.DefaultCode);
		}

		public void TestAllowCustomsDeclarationUpdateItem()
		{
			TestRegistryItem(ItemSet.AllowCustomsDeclarationUpdateItem, "AllowCustomsDeclarationUpdate", SystemDataRegistry.Categories.System_DataImportSettings_CustomsDeclarations,
						"Allow Customs Declaration Update",
						"This setting only applies to Legacy XML. Set this to 'YES' to update a Customs Declaration's details during an automatic XML import.",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue, false);
		}

		public void TestOrganisationDataImportDirectory()
		{
			AssertEquals("Default value", "", ItemSet.OrganisationDataImportDirectory.DefaultValue);
			AssertEquals("OrganisationDataImportDirectory", ItemSet.OrganisationDataImportDirectory.Name);
			AssertEquals("Folder to scan for Organization XML files", ItemSet.OrganisationDataImportDirectory.Caption);
			AssertEquals("The folder specified here should contain any Organization XML files that are to be automatically imported.", ItemSet.OrganisationDataImportDirectory.Hint);
			AssertEquals(SystemDataRegistry.Categories.System_DataImportSettings_Organizations, ItemSet.OrganisationDataImportDirectory.Category);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.OrganisationDataImportDirectory.Storage);

			Guid companyPK = new Guid();
			ItemSet.OrganisationDataImportDirectory.SetValue(companyPK, Guid.Empty, Guid.Empty, Env.TempPath);
			AssertEquals("Default value", Env.TempPath, ItemSet.OrganisationDataImportDirectory.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));
		}

		public void TestAttachmentSizeLimitDefault()
		{
			AssertEquals("Attachment limit should be defaulted at 20MB", 20, ItemSet.EmailAttachmentSizeLimitInMB.DefaultValue);
		}

		public void TestAllocateReportOverEmailAttachmentLimitToEDocs()
		{
			TestRegistryItem(ItemSet.AllocateReportOverEmailAttachmentLimitToEDocs,
				"AllocateReportOverEmailAttachmentLimitToEDocs",
				Categories.System_Email,
				"Allocate reports over the email attachment limit to eDocs",
				$@"If a report file size exceeds the maximum email attachment size allowed by your mail server, the report will be allocated to the eDocs and an email will be sent to the recipient with a link to download the report.

Please note, this functionally only works when the GLOW web services have been deployed and configured in the GLOW > Services Registry settings.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestPurgeOutgoingEmailsOlderThan()
		{
			AssertVisible(ItemSet.PurgeOutgoingEmailsOlderThan);

			AssertEquals(5, ItemSet.PurgeOutgoingEmailsOlderThan.DefaultValue);
			AssertEquals("PurgeOutgoingEmailsOlderThan", ItemSet.PurgeOutgoingEmailsOlderThan.Name);
			AssertEquals(@"System/Email", ItemSet.PurgeOutgoingEmailsOlderThan.Category);
			AssertEquals("Purge outgoing emails older than n days", ItemSet.PurgeOutgoingEmailsOlderThan.Caption);
			AssertEquals("Outgoing emails will be deleted n number of days after they have been sent", ItemSet.PurgeOutgoingEmailsOlderThan.Hint);

			AssertRegistryItemBound(ItemSet.PurgeOutgoingEmailsOlderThan, 0, 36500);
		}

		public void TestPurgeUnsentOutgoingEmailsOlderThan()
		{
			AssertVisible(ItemSet.PurgeUnsentOutgoingEmailsOlderThan);

			AssertEquals(20, ItemSet.PurgeUnsentOutgoingEmailsOlderThan.DefaultValue);
			AssertEquals("PurgeUnsentOutgoingEmailsOlderThan", ItemSet.PurgeUnsentOutgoingEmailsOlderThan.Name);
			AssertEquals(@"System/Email", ItemSet.PurgeUnsentOutgoingEmailsOlderThan.Category);
			AssertEquals("Purge outgoing emails older than n days in QUE status", ItemSet.PurgeUnsentOutgoingEmailsOlderThan.Caption);
			AssertEquals("Outgoing emails in QUE status will be purged after n days", ItemSet.PurgeUnsentOutgoingEmailsOlderThan.Hint);

			AssertRegistryItemBound(ItemSet.PurgeUnsentOutgoingEmailsOlderThan, 0, 36500);
		}

		public void TestPurgeIncomingProcessedEmailsOlderThan()
		{
			AssertVisible(ItemSet.PurgeIncomingProcessedEmailsOlderThan);

			AssertEquals(5, ItemSet.PurgeIncomingProcessedEmailsOlderThan.DefaultValue);
			AssertEquals("PurgeIncomingProcessedEmailsOlderThan", ItemSet.PurgeIncomingProcessedEmailsOlderThan.Name);
			AssertEquals(@"System/Email", ItemSet.PurgeIncomingProcessedEmailsOlderThan.Category);
			AssertEquals("Purge incoming processed emails older than n days", ItemSet.PurgeIncomingProcessedEmailsOlderThan.Caption);
			AssertEquals("Incoming emails will be deleted n number of days after they have been received and processed", ItemSet.PurgeIncomingProcessedEmailsOlderThan.Hint);

			AssertRegistryItemBound(ItemSet.PurgeIncomingProcessedEmailsOlderThan, 0, 36500);
		}

		public void TestPurgeIncomingUnProcessedEmailsOlderThan()
		{
			AssertVisible(ItemSet.PurgeIncomingUnProcessedEmailsOlderThan);

			AssertEquals(20, ItemSet.PurgeIncomingUnProcessedEmailsOlderThan.DefaultValue);
			AssertEquals("PurgeIncomingUnProcessedEmailsOlderThan", ItemSet.PurgeIncomingUnProcessedEmailsOlderThan.Name);
			AssertEquals(@"System/Email", ItemSet.PurgeIncomingUnProcessedEmailsOlderThan.Category);
			AssertEquals("Purge incoming unprocessed emails older than n days", ItemSet.PurgeIncomingUnProcessedEmailsOlderThan.Caption);
			AssertEquals("Incoming emails will be deleted n number of days after they have been received, whether or not they have been processed", ItemSet.PurgeIncomingUnProcessedEmailsOlderThan.Hint);

			AssertRegistryItemBound(ItemSet.PurgeIncomingUnProcessedEmailsOlderThan, 0, 36500);
		}

		public void TestRunOMSInSimulationMode()
		{
			AssertVisible(ItemSet.RunOMSInSimulationMode);

			var expectedHint =
	@"This option will set the Outbound Mail Service task to run in simulation mode.  When enabled, the Outbound Mail Service task will not log into the outbound mail server and send email messages to it for delivery.  Instead, the emails will automatically be marked as SNT status from the QUE status.

Simulation mode for the Outbound Mail Service task should only be enabled in those systems where delivery of emails are not required.";

			AssertEquals(false, ItemSet.RunOMSInSimulationMode.DefaultValue);
			AssertEquals("RunOMSInSimulationMode", ItemSet.RunOMSInSimulationMode.Name);
			AssertEquals(@"System/Email", ItemSet.RunOMSInSimulationMode.Category);
			AssertEquals("Run Outbound Mail Service Task In Simulation Mode", ItemSet.RunOMSInSimulationMode.Caption);
			AssertEquals(expectedHint, ItemSet.RunOMSInSimulationMode.Hint);
		}

		public void TestNonDeliveryReceiptNotificationEmailTemplate()
		{
			var item = ItemSet.NonDeliveryReceiptNotificationEmailTemplate;
			AssertVisible(item);
			AssertEquals("NonDeliveryReceiptNotificationEmailTemplate", item.Name);
			AssertEquals(@"System/Email", item.Category);
			AssertNotNullOrEmpty(item.Caption);
			AssertNotNullOrEmpty(item.Hint);
		}

		public void TestNonDeliveryReceiptNotificationGroup()
		{
			var item = ItemSet.NonDeliveryReceiptNotificationGroup;
			AssertVisible(item);
			AssertEquals("NonDeliveryReceiptNotificationGroup", item.Name);
			AssertEquals(@"System/Email", item.Category);
			AssertNotNullOrEmpty(item.Caption);
			AssertNotNullOrEmpty(item.Hint);
		}

		void AssertRegistryItemBound(IntRegistryItem registryItem, double minValue, double maxValue)
		{
			IntRegistryDataType dataType = (IntRegistryDataType)registryItem.DataType;
			AssertEquals(minValue, dataType.LowerBound);
			AssertEquals(maxValue, dataType.UpperBound);
		}

		public void TestDocManagerDataFileDefault()
		{
			AssertEquals("Default should be blank - the stored proc will then use existing values", "", ItemSet.DocManagerDBDataFilePath.DefaultValue);
		}

		public void TestDocManagerParanoidModeEnabled()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			Assert(EnvProxy.IsHostedWithCargowise);
			TestRegistryItem(ItemSet.DocManagerParanoidModeEnabled,
				"DocManagerParanoidModeEnabled",
				Categories.System_DocManager_S3Storage,
				"Write Verification Mode",
				"If enabled, the upload to S3 compatible storage will include an MD5 data integrity verification.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				expectedDefaultValue: true);

			//Non-hosted
			((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Purge("DocManagerParanoidModeEnabled");
			EnvProxy.SetHostedLocationForTest("");
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			AssertEquals("Non-hosted Options", RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue, ItemSet.DocManagerParanoidModeEnabled.Options);
		}

		public void TestDocManagerLogFileDefault()
		{
			AssertEquals("Default should be blank - the stored proc will then use existing values", "", ItemSet.DocManagerDBLogFilePath.DefaultValue);
		}

		public void TestEDocsStorageConnectionTimeoutHostedWithCargowise()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			Assert(EnvProxy.IsHostedWithCargowise);
			TestGenericRegistryItem(ItemSet.EDocsStorageConnectionTimeout,
				"EDocsStorageConnectionTimeout",
				Categories.System_DocManager_S3Storage,
				"Connection Timeout",
				"Time in seconds that the application will wait for a successful connection to the S3 environment before timing out. Accepted values are from 0 to 210 seconds. 0 means no time limit.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue);
		}

		public void TestEDocsStorageConnectionTimeoutNotHostedWithCargowise()
		{
			EnvProxy.SetHostedLocationForTest("");
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			TestGenericRegistryItem(ItemSet.EDocsStorageConnectionTimeout,
				"EDocsStorageConnectionTimeout",
				Categories.System_DocManager_S3Storage,
				"Connection Timeout",
				"Time in seconds that the application will wait for a successful connection to the S3 environment before timing out. Accepted values are from 0 to 210 seconds. 0 means no time limit.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue);
		}

		public void TestEDocsStorageProvider()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			Assert(EnvProxy.IsHostedWithCargowise);
			TestGenericRegistryItem(ItemSet.EDocsStorageProvider,
				"EDocsStorageProvider",
				Categories.System_DocManager,
				"eDocs Storage",
				@"Define the storage of the eDocs.
Note:
This Registry item can only be set to S3 compatible storage when the following S3 Storage Registry items have been configured.

System -> DocManager -> S3 Storage -> S3 Storage URL
System -> DocManager -> S3 Storage -> S3 Bucket Name
System -> DocManager -> S3 Storage -> S3 Storage Credentials",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory | RegistryOptions.IsOnlyForSupport);

			//Non-hosted
			((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Purge("EDocsStorageProvider");
			EnvProxy.SetHostedLocationForTest("");
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			AssertEquals("Non-hosted Options", RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory, ItemSet.EDocsStorageProvider.Options);
		}

		public void TestEDocsStorageAccessHostedWithCargowise()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			Assert(EnvProxy.IsHostedWithCargowise);
			TestStringRegistryItem(ItemSet.EDocsStorageAccess,
				"EDocsStorageAccess",
				Categories.System_DocManager_S3Storage,
				"S3 Storage Credentials",
				"Please provide access to the S3 storage. For example: \"KeyId=A1B2C3;Secret=aAbBcCdDeE\"",
				RegistryStorageFlags.System,
				TextEditorType.Password,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				string.Empty,
				CharacterCase.Normal);
			AssertType<S3StorageRegistryDataType>(ItemSet.EDocsStorageAccess.DataType);
		}

		public void TestEDocsStorageAccessNotHostedWithCargowise()
		{
			EnvProxy.SetHostedLocationForTest("");
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			TestStringRegistryItem(ItemSet.EDocsStorageAccess,
				"EDocsStorageAccess",
				Categories.System_DocManager_S3Storage,
				"S3 Storage Credentials",
				"Please provide access to the S3 storage. For example: \"KeyId=A1B2C3;Secret=aAbBcCdDeE\"",
				RegistryStorageFlags.System,
				TextEditorType.Password,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				string.Empty,
				CharacterCase.Normal);
			AssertType<S3StorageRegistryDataType>(ItemSet.EDocsStorageAccess.DataType);
		}

		public void TestForcePathStyleAddressingHostedWithCargowise()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			Assert(EnvProxy.IsHostedWithCargowise);
			TestRegistryItem(ItemSet.UsePathStyleAddressing,
				"UsePathStyleAddressing",
				Categories.System_DocManager_S3Storage,
				"Use Path Style Addressing",
				"By default, the requests will always use path style addressing. If disabled, virtual hosted-style addressing will be used.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				true);
		}

		public void TestForcePathStyleAddressingNotHostedWithCargowise()
		{
			EnvProxy.SetHostedLocationForTest("");
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			TestRegistryItem(ItemSet.UsePathStyleAddressing,
				"UsePathStyleAddressing",
				Categories.System_DocManager_S3Storage,
				"Use Path Style Addressing",
				"By default, the requests will always use path style addressing. If disabled, virtual hosted-style addressing will be used.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				true);
		}

		public void TestUseChunkEncodingHostedWithCargowise()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			Assert(EnvProxy.IsHostedWithCargowise);
			TestRegistryItem(ItemSet.UseChunkEncoding,
				"UseChunkEncoding",
				Categories.System_DocManager_S3Storage,
				"Use Chunk Encoding",
				"By default, a chunked encoding upload will be used for the request. Set to No to disable it.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				true);
		}

		public void TestUseChunkEncodingNotHostedWithCargowise()
		{
			EnvProxy.SetHostedLocationForTest("");
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			TestRegistryItem(ItemSet.UseChunkEncoding,
				"UseChunkEncoding",
				Categories.System_DocManager_S3Storage,
				"Use Chunk Encoding",
				"By default, a chunked encoding upload will be used for the request. Set to No to disable it.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				true);
		}

		public void TestEDocsStorageServiceUrl()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			Assert(EnvProxy.IsHostedWithCargowise);
			TestStringRegistryItem(ItemSet.EDocsStorageServiceUrl,
				"EDocsStorageServiceUrl",
				Categories.System_DocManager_S3Storage,
				"S3 Storage URL",
				"Please provide the service URL to the S3 storage.",
				RegistryStorageFlags.System,
				TextEditorType.Url,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				string.Empty,
				CharacterCase.Normal);

			AssertType<S3StorageRegistryDataType>(ItemSet.EDocsStorageServiceUrl.DataType);

			//Non-hosted
			((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Purge("EDocsStorageServiceUrl");
			EnvProxy.SetHostedLocationForTest("");
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			AssertEquals("Non-hosted Options", RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue, ItemSet.EDocsStorageServiceUrl.Options);
		}

		public void TestDocManagerStorageBucketName()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			Assert(EnvProxy.IsHostedWithCargowise);
			var key = ObjectFactory.Get<IProductRegistration>().Key;
			TestStringRegistryItem(ItemSet.DocManagerStorageBucketName,
				"DocManagerStorageBucketName",
				Categories.System_DocManager_S3Storage,
				"S3 Bucket Name",
				"Override Default to change bucket name when using S3 for eDocs Storage.", RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				"",
				CharacterCase.Normal);

			AssertType<S3StorageRegistryDataType>(ItemSet.DocManagerStorageBucketName.DataType);

			//Non-hosted
			((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Purge("DocManagerStorageBucketName");
			EnvProxy.SetHostedLocationForTest("");
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			AssertEquals("Non-hosted Options", RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue, ItemSet.DocManagerStorageBucketName.Options);
		}

		public void TestActivateSystemMergeDataInterfaceHostedWithCargowise()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			Assert(EnvProxy.IsHostedWithCargowise);
			RegistryItemDictionary.Instance.PurgeAll();
			TestRegistryItem(ItemSet.ActivateSystemMergeDataInterface,
				"ActivateSystemMergeDataInterface",
				Categories.System_DataImportSettings_SystemMerge,
				"Activate System Merge Data Interface",
				"Activate System Merge data import and export interfaces.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestActivateSystemMergeDataInterfaceNotHostedWithCargowise()
		{
			Assert(!EnvProxy.IsHostedWithCargowise);
			TestRegistryItem(ItemSet.ActivateSystemMergeDataInterface,
				"ActivateSystemMergeDataInterface",
				Categories.System_DataImportSettings_SystemMerge,
				"Activate System Merge Data Interface",
				"Activate System Merge data import and export interfaces.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController,
				false);
		}

		public void TestEDocsDBMinimumStorageDays()
		{
			EnvProxy.SetHostedLocationForTest("");
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			TestRegistryItem(
				ItemSet.EDocsDBMinimumStorageDays,
				"EDocsDBMinimumStorageDays",
				Categories.System_DocManager,
				"eDocs Database Minimum Storage Period (Days)",
				@"The minimum number of days that eDocs are stored in the DocManager database on the SQL server before being pushed to the external storage environment (e.g. S3 storage).

Note: This setting only affects the regular process of eDocs by DER service but not the initial process by DES service.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				90, 0, 9999);

			// If Hosted
			((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Purge("EDocsDBMinimumStorageDays");
			EnvProxy.SetHostedLocationForTest("SYD");
			AssertEquals(true, EnvProxy.IsHostedWithCargowise);
			AssertEquals("Hosted Options", RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, ItemSet.EDocsDBMinimumStorageDays.Options);
		}

		public void TestAWSS3StorageClass()
		{
			EnvProxy.SetHostedLocationForTest("");
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			TestRegistryItem(
				ItemSet.AWSS3StorageClass,
				"AWSS3StorageClass",
				Categories.System_DocManager_S3Storage,
				"Amazon S3 Storage Class",
				@"Define the Amazon S3 Storage Class (Tier) to be used.
For more details, refer to: https://aws.amazon.com/s3/storage-classes/
This setting is applicable only for Amazon S3 Storage. If it is not set, Standard class will be used by default.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				ItemSet.AWSS3StorageClassListProvider.CodeDescriptionPairList,
				"");

			// If Hosted
			((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Purge("AWSS3StorageClass");
			EnvProxy.SetHostedLocationForTest("SYD");
			AssertEquals(true, EnvProxy.IsHostedWithCargowise);
			AssertEquals("Hosted Options", RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, ItemSet.AWSS3StorageClass.Options);
		}

		public void TestUseVersionID()
		{
			// Self-hosted clients
			EnvProxy.SetHostedLocationForTest("");
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);

			TestRegistryItem(ItemSet.UseVersionID,
				"UseVersionID",
				Categories.System_DocManager_S3Storage,
				"Use Version ID",
				@"When enabled, eDocs in external storage will be retrieved using the stored version ID, if versioning is enabled on the bucket in external storage.

Note:
Versioning must be enabled on the bucket in external storage before enabling this registry item.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
					false);

			// Hosted clients
			((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Purge("UseVersionID");
			EnvProxy.SetHostedLocationForTest("SYD");

			AssertEquals(true, EnvProxy.IsHostedWithCargowise);
			AssertEquals("Hosted Options", RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, ItemSet.UseVersionID.Options);
		}

		public void TestEDocsMaximumFilesize()
		{
			AssertEquals("eDocsMaximumFilesize.DefaultValue", 10, ItemSet.eDocsMaximumFilesize.DefaultValue);

			IntRegistryDataType dataType = (IntRegistryDataType)ItemSet.eDocsMaximumFilesize.DataType;
			AssertEquals("LowerBound", 0.0, dataType.LowerBound);
			AssertEquals("UpperBound", 100.0, dataType.UpperBound);

			ItemSet.eDocsMaximumFilesize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			AssertEquals("eDocsMaximumFilesize.Value", 100, ItemSet.eDocsMaximumFilesize.Value);
		}

		public void TestEDocsStorageConnectionTimeout()
		{
			AssertEquals("EDocsStorageConnectionTimeout.DefaultValue", 30, ItemSet.EDocsStorageConnectionTimeout.DefaultValue);

			var dataType = (IntRegistryDataType)ItemSet.EDocsStorageConnectionTimeout.DataType;
			AssertEquals("LowerBound", 0.0, dataType.LowerBound);
			AssertEquals("UpperBound", 210.0, dataType.UpperBound);

			ItemSet.EDocsStorageConnectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 210);
			AssertEquals("EDocsStorageConnectionTimeout.Value", 210, ItemSet.EDocsStorageConnectionTimeout.Value);
		}

		public void TestDocManagerDatabaseDataPathValidation()
		{
			AssertExceptionThrown<RegistryValidationException>(() => ItemSet.DocManagerDBDataFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "aaaa"));
			AssertExceptionThrown<RegistryValidationException>(() => ItemSet.DocManagerDBDataFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "//anotherserver/path"));
			AssertExceptionThrown<RegistryValidationException>(() => ItemSet.DocManagerDBDataFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, @":/*<>?|"));
			AssertNoExceptionThrown(() => ItemSet.DocManagerDBDataFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "C:\\PATH1\\PATH2\\"));
		}

		public void TestDocManagerDatabaseLogPathValidation()
		{
			AssertExceptionThrown<RegistryValidationException>(() => ItemSet.DocManagerDBLogFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "aaaa"));
			AssertExceptionThrown<RegistryValidationException>(() => ItemSet.DocManagerDBLogFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "//anotherserver/path"));
			AssertExceptionThrown<RegistryValidationException>(() => ItemSet.DocManagerDBLogFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, @":/*<>?|"));
			AssertNoExceptionThrown(() => ItemSet.DocManagerDBLogFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "C:\\PATH1\\PATH2\\"));
		}

		public void TestDocManagerDataFileSizeThresholdGb()
		{
			AssertEquals("DocManagerMaximumDataFileSizeGb.DefaultValue", 8, ItemSet.DocManagerDataFileSizeThresholdGb.DefaultValue);

			var dataType = (DocManagerDataFileSizeThresholdGbRegistryDataType)ItemSet.DocManagerDataFileSizeThresholdGb.DataType;
			AssertEquals("LowerBound", 2d, dataType.LowerBound);
			AssertEquals("UpperBound", (double)int.MaxValue, dataType.UpperBound);
			AssertEquals("When the current DocManager database exceeds this size limit, the DocManager Database Creation Service Task will create a new DocManager database. The minimum database size limit is 2GB and there is no maximum size limit.", ItemSet.DocManagerDataFileSizeThresholdGb.Hint);

			ItemSet.DocManagerDataFileSizeThresholdGb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20);
			AssertEquals("DocManagerMaximumDataFileSizeGb.Value", 20, ItemSet.DocManagerDataFileSizeThresholdGb.Value);
		}

		[ExpectExceptionAttribute(typeof(RegistryValidationException))]
		public void TestSettingDocManagerMaximumDataFileSizeGbOutOfBoundsThrowsException()
		{
			ItemSet.DocManagerDataFileSizeThresholdGb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, -1);
		}

		public void TestSystemLogBatchProcessHighWaterMark()
		{
			DateTime newDateTime = DateTime.Now;

			ItemSet.SystemLogBatchProcessHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newDateTime);
			AssertEquals("SystemLogBatchProcessHighWaterMark.Value", newDateTime.ToString(), ItemSet.SystemLogBatchProcessHighWaterMark.Value.ToString());
			DateTimeRegistryEditorInfo editorInfo = ItemSet.SystemLogBatchProcessHighWaterMark.EditorInfo as DateTimeRegistryEditorInfo;
			AssertNotNull("EditorInfo should be of type DateTimeRegistryEditorInfo", editorInfo);
			AssertEquals("Datetime format", ZDateTimePickerFormat.Long, editorInfo.DateTimeFormat);
			AssertEquals("RegistryStorageFlags", RegistryStorageFlags.System, ItemSet.SystemLogBatchProcessHighWaterMark.Storage);
			AssertEquals("RegistryOptions", RegistryOptions.NotCached, ItemSet.SystemLogBatchProcessHighWaterMark.Options);
		}

		public void TestDocManagerBatchProcessorImportOptions()
		{
			TestGenericRegistryItem(ItemSet.DocManagerBatchProcessorImportOptions, "DocManagerBatchProcessorImportOptions", "System/DocManager", "DocManager Import Service Task Options", "The directory that the DocManager Import Service Task will import DocManager files from. Files are deleted from this directory after they are imported.", RegistryStorageFlags.System, RegistryOptions.PreserveTestValue | RegistryOptions.IsOnlyEditableBySupportIfHosted);

			DirectorySearch search = new DirectorySearch();
			search.DirectoryPath = @"Y:\Import";
			ItemSet.DocManagerBatchProcessorImportOptions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, search);
			AssertEquals("Value.DirectoryPath", @"Y:\Import", ItemSet.DocManagerBatchProcessorImportOptions.Value.DirectoryPath);
		}

		public void TestDocManagerTIFColourDepth()
		{
			var expectedLookUpList = new CodeDescriptionPairList(OLookUpEditType.ColourDepth);
			TestRegistryItem(ItemSet.DocManagerTIFColourDepth,
				"DocManagerTIFColourDepth",
				"System/DocManager",
				"Color Depth for System Generated documents stored in eDocs",
				"Copies of documents emailed, printed or faxed from the current application will be stored on the eDocs tab at the selected color depth. If you process a large volume of documents, or have limited storage space, you should store your documents in Black and White to minimize the amount of storage space required.\r\n\r\nNOTE: Color documents can be up to 20 times larger than Black and White.\r\nNOTE: This setting is used only if TIF format is selected in eDocs Import File Format registry item.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				expectedLookUpList,
				Enterprise.Core.Constants.ColourDepth.BlackAndWhite);
		}

		public void TestMessageUserContextTracingEnabled()
		{
			AssertEquals("Default value should be true.", true, SystemDataRegistry.Instance.DocumentImportUserContextTracingEnabled.DefaultValue);
			SystemDataRegistry.Instance.DocumentImportUserContextTracingEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(!SystemDataRegistry.Instance.DocumentImportUserContextTracingEnabled.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			Assert(!SystemDataRegistry.Instance.DocumentImportUserContextTracingEnabled.Value);
			SystemDataRegistry.Instance.DocumentImportUserContextTracingEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		public void TestUnpublishOlderVersionDocument()
		{
			TestGenericRegistryItem(
				ItemSet.UnpublishOlderVersionDocument,
				"UnpublishOlderVersionDocument",
				"System/DocManager",
				"Un-publish Older Versions of Documents",
				"When enabled and a document with the same File Name and Document Type is added to the eDocs tab, older versions of the documents will be unpublished.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				false);

			AssertEquals("Default value should be false.", false, SystemDataRegistry.Instance.UnpublishOlderVersionDocument.DefaultValue);
			SystemDataRegistry.Instance.UnpublishOlderVersionDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert(SystemDataRegistry.Instance.UnpublishOlderVersionDocument.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			Assert(SystemDataRegistry.Instance.UnpublishOlderVersionDocument.Value);
			SystemDataRegistry.Instance.UnpublishOlderVersionDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		public void TestAllowDocManagerBatchProcessorImports()
		{
			TestGenericRegistryItem(ItemSet.AllowDocManagerBatchProcessorImports, "AllowDocManagerBatchProcessorImports", "System/DocManager", "Allow DocManager Import Service Task", "This registry item specifies whether or not the DocManager Import Service Task should process the files in the DocManager Batch Import Folder and incoming DocManager import emails.", RegistryStorageFlags.System, RegistryOptions.PreserveTestValue, true);
			ItemSet.AllowDocManagerBatchProcessorImports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("value should be set to false", false, ItemSet.AllowDocManagerBatchProcessorImports.Value);
		}

		public void TestDocumentTypesRestrictedForImport()
		{
			TestGenericRegistryItem(ItemSet.DocumentTypesRestrictedForImport, "DocumentTypesRestrictedForImport", "System/DocManager", "Document Types Restricted For Import", "The list of Document Type codes that, when encountered, will not be imported by DocManager Import Service Task.", RegistryStorageFlags.System, RegistryOptions.PreserveTestValue, new CodeSelectionCollection());

			var value = new CodeSelectionCollection(SystemDataRegistry.Instance.DocumentTypesRestrictedListProvider);
			var item = value.AddNew();
			item.Code = "1RM";
			AssertEquals("Request for Immediate Payment", item.Description);

			ItemSet.DocumentTypesRestrictedForImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			AssertEquals("Value should be set", value, ItemSet.DocumentTypesRestrictedForImport.Value);
		}

		#region Document Tracking

		public void TestRestrictAEDAndAIDEvents()
		{
			TestRegistryItem(ItemSet.RestrictAEDAndAIDEvents,
				"RestrictAEDAndAIDEvents",
				Categories.System_DocManager_DocumentTracking,
				"AED and AID events",
				"When enabled, AED/AID events will be generated only when all the documents required for export/import have been received. \r\n\r\nWhen disabled, AED/AID events may be generated even if no export/import documents are required. This is the current behavior.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				false);
		}

		#endregion

		public void TestIncomingEmailAddressesValidation()
		{
			CodeDescriptionPairList mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("Test@domain.com", "Test Email");
			mappingList.AddPair("Scanner-1@domain.com", "Scanner 1");
			mappingList.AddPair("Scanner-?@domain.com", "All my Scanners");
			mappingList.AddPair("*@domain.com", "Everyone on my domain");

			AssertEquals("No errors", string.Empty, ItemSet.EmailAddressesAllowedForImport.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));

			mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("tst", "");

			AssertEquals("Should throw error upon validation as email address is not correct", "The following is not a valid Email Address : tst", ItemSet.EmailAddressesAllowedForImport.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));

			mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("", "John Smith");

			AssertEquals("Should throw error upon validation as email address is empty", "Please enter an Email Address at line 1", ItemSet.EmailAddressesAllowedForImport.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));

			mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("    ", "Another test");

			AssertEquals("Should throw error upon validation as email address is empty", "Please enter an Email Address at line 1", ItemSet.EmailAddressesAllowedForImport.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));

			mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("John.Smith@domain.com", "");

			AssertEquals("Should throw error upon validation as description is empty", "Please enter a Description for Email Address John.Smith@domain.com", ItemSet.EmailAddressesAllowedForImport.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));

			mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("John.Smith@domain.com", "        ");

			AssertEquals("Should throw error upon validation as description is empty", "Please enter a Description for Email Address John.Smith@domain.com", ItemSet.EmailAddressesAllowedForImport.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));

			mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("tst", "Test Email");

			AssertEquals("Should throw error upon validation as email address is not correct", "The following is not a valid Email Address : tst", ItemSet.EmailAddressesAllowedForImport.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));

			mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("@@@", "Another test");

			AssertEquals("Should throw error upon validation as email address is not correct", "The following is not a valid Email Address : @@@", ItemSet.EmailAddressesAllowedForImport.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestStaffLeaveTypeAlertsValidation()
		{
			CodeDescriptionPairList mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("SIC", "test alert 1");
			mappingList.AddPair("CAS", "test alert 2");

			AssertEquals("No errors", string.Empty, ItemSet.StaffLeaveTypeAlerts.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));

			mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("ANN", "");

			AssertEquals("Should throw error upon validation as description is empty", "Please enter an Alert for Leave Type ANN", ItemSet.StaffLeaveTypeAlerts.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));

			mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("", "test alert");

			AssertEquals("Should throw error upon validation as code is empty", "Please enter a Leave Type code at line 1", ItemSet.StaffLeaveTypeAlerts.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));

			mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("    ", "another test alert");

			AssertEquals("Should throw error upon validation as code is empty", "Please enter a Leave Type code at line 1", ItemSet.StaffLeaveTypeAlerts.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));

			mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("ANN", "");

			AssertEquals("Should throw error upon validation as description is empty", "Please enter an Alert for Leave Type ANN", ItemSet.StaffLeaveTypeAlerts.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));

			mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("ANN", "        ");

			AssertEquals("Should throw error upon validation as description is empty", "Please enter an Alert for Leave Type ANN", ItemSet.StaffLeaveTypeAlerts.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));

			mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("ABC", "yet another test alert");

			AssertEquals("Should throw error upon validation as code is not correct", "The following is not a valid Leave Type : ABC", ItemSet.StaffLeaveTypeAlerts.GetValidationErrorMessage(mappingList, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestUseDefaultWindowsImageViewer()
		{
			TestGenericRegistryItem(ItemSet.UseDefaultWindowsImageViewer, "UseDefaultWindowsImageViewer", "System/DocManager", "Use default Windows image viewing application", $"Enable this if you want to use windows default image viewing application to view / edit eDocs rather than the {Enterprise.Core.Constants.ProductName} Viewer. This applies to all image files.", RegistryStorageFlags.All | RegistryStorageFlags.Company, RegistryOptions.Default, false);
			ItemSet.UseDefaultWindowsImageViewer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("value should be set to true", true, ItemSet.UseDefaultWindowsImageViewer.Value);
		}

		public void TestEmailNotificationForErrorsOnly()
		{
			TestRegistryItem(ItemSet.EmailNotificationForErrorsOnly, "EmailNotificationForErrorsOnly", "System/Data Import Settings", "Email Notification For Errors Only", "Set this option to \"Yes\" if Notification Emails are to be sent on Data Import Errors only.", RegistryStorageFlags.System | RegistryStorageFlags.Company, false);
		}

		public void TestStaffEmploymentTypes()
		{
			TestRegistryItem(
				ItemSet.StaffEmploymentTypes,
				"StaffEmploymentTypes",
				"System/Staff",
				"Staff Employment Types",
				"A list of the ways a staff member may be employed in this organization.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				"Full Time",
				false,
				8,
				new CodeDescriptionPair("PER", "Permanent Full Time"));

			AssertEquals(true, ItemSet.StaffEmploymentTypes.DefaultValue.GetBoolFromCode("PER"));
			AssertEquals(false, ItemSet.StaffEmploymentTypes.DefaultValue.GetBoolFromCode("PAR"));
		}

		public void TestEnableValidateLeaveOnStaffEditDefault()
		{
			Assert("Default value should be true.", ItemSet.EnableValidateLeaveOnStaffEdit.DefaultValue);
		}

		public void TestEnableValidateLeaveOnStaffEditIsEnabled()
		{
			ItemSet.EnableValidateLeaveOnStaffEdit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ItemSet.EnableValidateLeaveOnStaffEdit.Value);
		}

		public void TestEnableValidateLeaveOnStaffEditIsDisabled()
		{
			ItemSet.EnableValidateLeaveOnStaffEdit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, ItemSet.EnableValidateLeaveOnStaffEdit.Value);
		}

		public void TestStaffEmailTypeList()
		{
			TestRegistryItem(ItemSet.StaffEmailTypeList, "StaffEmailTypeList", "System/Staff", "Staff Email Type List", "A list of email types used for staff details.", RegistryStorageFlags.Company | RegistryStorageFlags.System, 20, 0);
		}

		public void TestStaffLeaveTypes()
		{
			TestRegistryItem(ItemSet.StaffLeaveTypes, "StaffLeaveTypes", "System/Staff", "Staff Leave Types", "A list of leave types that apply to staff members.", RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}

		public void TestStaffLeaveTypeAlerts()
		{
			TestGenericRegistryItem(ItemSet.StaffLeaveTypeAlerts, "StaffLeaveTypeAlerts", "System/Staff", "Staff Leave Type Alerts", "A list of alerts to display when the user creates leave of the corresponding type.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue);
		}

		public void TestStaffReportingRoles()
		{
			TestGenericRegistryItem(ItemSet.StaffReportingRoles, "StaffReportingRoles", "System/Staff", "Staff Reporting Roles", "A list of management roles which staff members can perform.", RegistryStorageFlags.System);

			var defaultValue = (StaffReportingRole)ItemSet.StaffReportingRoles.DefaultValue.Single();
			AssertEquals("Default Code", "DRM", defaultValue.Code);
			AssertEquals("Default Description", "Direct Manager", defaultValue.Description);
			AssertEquals("CodeInfo.ReadOnly", true, defaultValue.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", false, defaultValue.DescriptionInfo.ReadOnly);
			AssertEquals("BoolInfo.ReadOnly", false, defaultValue.BoolInfo.ReadOnly);
		}

		public void TestStaffCertificateTypes()
		{
			TestRegistryItem(ItemSet.StaffCertificateTypes, "StaffCertificateTypes", "System/Staff", "Staff Certificate Types", "A customizable list of certificate, ID and training types for staff. The list also contains system defined default types.", RegistryStorageFlags.System);

			var defaultValue = ItemSet.StaffCertificateTypes.DefaultValue;

			void AssertCollectionContainsCodeDescriptionBool(string code, string description, ZBool inUse)
			{
				AssertCollectionContains($"Expected: {code}|{description}|{inUse}", $"{code}|{description}|{inUse}", defaultValue.Cast<CodeDescriptionBool>().Select(x => $"{x.Code}|{x.Description}|{x.Bool}"));
			}

			AssertCollectionContainsCodeDescriptionBool("CO1", "UK Cargo Operative", true);
			AssertCollectionContainsCodeDescriptionBool("CO2", "UK Cargo Operative Screening", true);
			AssertCollectionContainsCodeDescriptionBool("CO3", "UK Cargo Operative Screening - Refresher", true);
			AssertCollectionContainsCodeDescriptionBool("CS1", "UK Cargo Supervisor", true);
			AssertCollectionContainsCodeDescriptionBool("CS2", "UK Cargo Supervisor - Refresher", true);
			AssertCollectionContainsCodeDescriptionBool("CM1", "UK Cargo Manager", true);
			AssertCollectionContainsCodeDescriptionBool("CNO", "CN e-port operator card ID", true);
			AssertCollectionContainsCodeDescriptionBool("COD", "IT Italian Registration Number", true);
			AssertCollectionContainsCodeDescriptionBool("BKG", "Background Check", true);
		}

		public void TestGroupCategoryList()
		{
			TestGenericRegistryItem(ItemSet.GroupCategoryList, "GroupCategoryList", "System/Groups", "Categories", "The values that can be selected in the Category field within the Group module.", RegistryStorageFlags.System);
		}

		public void TestDefectReportGroupCategoryList()
		{
			TestGenericRegistryItem(ItemSet.DefectReportGroupCategoryList, "DefectReportGroupCategories", "System/Groups", "Defect Report Included Categories", "The categories of Groups that will be included in the Defects Report.", RegistryStorageFlags.System);
		}

		public void TestUserIdleWorkerEnabled()
		{
			TestRegistryItem(ItemSet.UserIdleWorkerEnabled, "UserIdleWorkerEnabledDevOnly", "System/Framework", "User Idle Worker Enabled (Developer Only)", "The User Idle Worker improves system responsiveness by performing tasks when the user is idle momentarily.", RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers, true);
		}

		public void TestResourceTypes()
		{
			TestRegistryItem(ItemSet.ResourceTypes, "ResourceTypes", "System/Resources", "Resource Types", "A list of resource types.", RegistryStorageFlags.System);
		}

		public void TestUpdateBookingContainersDuringAutomaticImport()
		{
			AssertEquals("UpdateBookingContainersDuringAutomaticImport.DefaultValue", true, ItemSet.UpdateBookingContainersDuringAutomaticImport.DefaultValue);
			ItemSet.UpdateBookingContainersDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("UpdateBookingContainersDuringAutomaticImport.Value", false, ItemSet.UpdateBookingContainersDuringAutomaticImport.Value);
		}

		public void TestNewsSectionTypes()
		{
			TestGenericRegistryItem(ItemSet.NewsSectionTypes,
				"NewsSectionTypes",
				SystemDataRegistry.Categories.System_NewsAnnouncements,
				"Section Types",
				@$"A list of News & Announcements sections that can be placed on the main screen of the application, in addition to the system defined list.
The user defined News & Announcements sections can either be sorted by 'Published Time' or in 'Alpha/Numeric' order. There is a display limitation of {NewsMaximumRows} items per section. With 'Published Time', the most recent {NewsMaximumRows} items are displayed. With 'Alpha/Numeric', the first {NewsMaximumRows} items are displayed in ascending Alpha/Numeric order.",
				RegistryStorageFlags.System);

			var companyName = NewsAnnouncementSectionListRetriever.GetCompanyName();
			var newsSectionTypeList = new NewsSectionTypeList();
			for (int i = 0; i < ItemSet.NewsSectionTypes.Value.Count; i++)
			{
				var sectionType = ItemSet.NewsSectionTypes.Value[i];
				CombineAssertions(() =>
				{
					AssertEquals("Should contain default items Code.", newsSectionTypeList[i].Code, sectionType.Code);
					AssertEquals("Should contain default items Description.", newsSectionTypeList[i].Description, sectionType.Description.ToString());
					AssertEquals("Should be system defined", true, sectionType.SystemDefined);
					AssertEquals("Should order by PublishedTime by default", NewsSectionSortTypeList.Codes.PublishedTime, sectionType.OrderItemsBy);
				});
			}
		}

		#region LogWalker Registry Items

		public void TestLogWalkerEnabled()
		{
			AssertVisible(ItemSet.LogWalkerEnabled);

			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.LogWalkerEnabled.Storage);
			AssertEquals("Options", RegistryOptions.PreserveTestValue, ItemSet.LogWalkerEnabled.Options);
			AssertEquals("EditorInfo", typeof(BooleanRegistryEditorInfo), ItemSet.LogWalkerEnabled.EditorInfo.GetType());

			AssertEquals("LogWalkerEnabled", ItemSet.LogWalkerEnabled.DefaultValue, ItemSet.LogWalkerEnabled.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.LogWalkerEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("LogWalkerEnabled", false, ItemSet.LogWalkerEnabled.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestLogWalkerBatchSize()
		{
			AssertEquals(10, ItemSet.LogWalkerBatchSize.Value);
		}

		public void TestLogWalkerClearFailedLogsAfterHours()
		{
			AssertEquals(24 * 7, ItemSet.LogWalkerClearFailedLogsAfterHours.Value);
		}

		public void TestLogWalkerClearProcessedLogsAfterHours()
		{
			AssertEquals(0, ItemSet.LogWalkerClearProcessedLogsAfterHours.Value);
		}

		public void TestLogSubscriberReportResourcesUsage()
		{
			AssertEquals("LogSubscriberReportResourcesUsage", ItemSet.LogSubscriberReportResourcesUsage.Name);
			AssertEquals("Report Excessive Resources Usage", ItemSet.LogSubscriberReportResourcesUsage.Caption);
			Assert("Default value", !ItemSet.LogSubscriberReportResourcesUsage.Value);

			ItemSet.LogSubscriberReportResourcesUsage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert(ItemSet.LogSubscriberReportResourcesUsage.Value);

			ItemSet.LogSubscriberReportResourcesUsage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(!ItemSet.LogSubscriberReportResourcesUsage.Value);
		}

		public void TestLogSubscriberSlowLogBatchProcessingThresholdInSeconds()
		{
			AssertEquals("LogSubscriberSlowLogBatchProcessingThresholdInSeconds", ItemSet.LogSubscriberSlowLogBatchProcessingThresholdInSeconds.Name);
			AssertEquals("Processing time limit in seconds per Subscriber (batch)", ItemSet.LogSubscriberSlowLogBatchProcessingThresholdInSeconds.Caption);
			AssertEquals("Limit in seconds for a Subscriber to process a batch of logs before it is reported as slow. 0 = no limit.", ItemSet.LogSubscriberSlowLogBatchProcessingThresholdInSeconds.Hint);
			AssertEquals(60, ItemSet.LogSubscriberSlowLogBatchProcessingThresholdInSeconds.DefaultValue);

			try
			{
				ItemSet.LogSubscriberSlowLogBatchProcessingThresholdInSeconds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
				Assert("RegistryValidationException should have been thrown.", false);
			}
			catch (RegistryValidationException e)
			{
				AssertEquals("The value should be no less then 20, or have a 0 as a special value.", e.Message);
				ItemSet.LogSubscriberSlowLogBatchProcessingThresholdInSeconds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ItemSet.LogSubscriberSlowLogBatchProcessingThresholdInSeconds.DefaultValue);
			}
		}

		public void TestLogSubscriberSlowLogProcessingThresholdInSeconds()
		{
			AssertEquals("LogSubscriberSlowLogProcessingThresholdInSeconds", ItemSet.LogSubscriberSlowLogProcessingThresholdInSeconds.Name);
			AssertEquals("Processing time limit in seconds per Subscriber (individual log)", ItemSet.LogSubscriberSlowLogProcessingThresholdInSeconds.Caption);
			AssertEquals("Limit in seconds for a Subscriber to process a log before it is reported as slow. 0 = no limit.", ItemSet.LogSubscriberSlowLogProcessingThresholdInSeconds.Hint);
			AssertEquals(10, ItemSet.LogSubscriberSlowLogProcessingThresholdInSeconds.DefaultValue);

			try
			{
				ItemSet.LogSubscriberSlowLogProcessingThresholdInSeconds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, -5);
				Assert("RegistryValidationException should have been thrown.", false);
			}
			catch (RegistryValidationException e)
			{
				AssertEquals("The value should be no less then 1, or have a 0 as a special value.", e.Message);
				ItemSet.LogSubscriberSlowLogProcessingThresholdInSeconds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ItemSet.LogSubscriberSlowLogProcessingThresholdInSeconds.DefaultValue);
			}
		}

		public void TestLogSubscriberMemoryLeakThresholdInBytes()
		{
			AssertEquals("LogSubscriberMemoryLeakThresholdInBytes", ItemSet.LogSubscriberMemoryLeakThresholdInBytes.Name);
			AssertEquals("Memory leak reporting threshold in bytes per Subscriber", ItemSet.LogSubscriberMemoryLeakThresholdInBytes.Caption);
			AssertEquals("Maximum allowed memory amount to be consumed and not released per Log Subscriber (in bytes). 0 = no limit.", ItemSet.LogSubscriberMemoryLeakThresholdInBytes.Hint);
			AssertEquals(15000, ItemSet.LogSubscriberMemoryLeakThresholdInBytes.DefaultValue);

			try
			{
				ItemSet.LogSubscriberMemoryLeakThresholdInBytes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, -5);
				Assert("RegistryValidationException should have been thrown.", false);
			}
			catch (RegistryValidationException e)
			{
				AssertEquals("The value should be no less then 0.", e.Message);
				ItemSet.LogSubscriberMemoryLeakThresholdInBytes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ItemSet.LogSubscriberMemoryLeakThresholdInBytes.DefaultValue);
			}
		}

		public void TestSecondsUntilLogWalkerStopsAllocatingLogBatchesToASubscriber()
		{
			AssertEquals(20, ItemSet.SecondsUntilLogWalkerStopsAllocatingLogBatchesToASubscriber.DefaultValue);
			AssertEquals(30 * 60, ItemSet.SecondsUntilLogWalkerEnds.DefaultValue);
		}

		public void TestLogWalkerDelayDurationInCreatingLogQueueItemsDefaultValue()
		{
			AssertEquals(500, ItemSet.LogWalkerDelayDurationInCreatingLogQueueItems.DefaultValue);
		}

		public void TestLogWalkerPurgeBatchSize()
		{
			AssertEquals(1000, ItemSet.LogWalkerPurgeBatchSize.Value);
		}

		#endregion

		#region Logos

		public void TestCompanyLogo()
		{
			AssertVisible(ItemSet.CompanyLogo);
			AssertEquals("Name", "Logo", ItemSet.CompanyLogo.Name);
			AssertEquals("Storage", RegistryStorageFlags.All, ItemSet.CompanyLogo.Storage);
		}

		public void TestCompanyCheckLogo()
		{
			AssertVisible(ItemSet.CompanyCheckLogo);
			AssertEquals("Name", "CompanyCheckLogo", ItemSet.CompanyCheckLogo.Name);
			AssertEquals("Storage", RegistryStorageFlags.All, ItemSet.CompanyCheckLogo.Storage);
		}

		public void TestInvoceAndStatementLogo()
		{
			AssertVisible(ItemSet.InvoceAndStatementLogo);
			AssertEquals("Name", "InvoiceAndStatementLogo", ItemSet.InvoceAndStatementLogo.Name);
			AssertEquals("Storage", RegistryStorageFlags.All, ItemSet.InvoceAndStatementLogo.Storage);
		}

		public void TestHtmlEmailBannerImage()
		{
			AssertVisible(ItemSet.HtmlEmailBannerImage);
			AssertNotNull("Default image should be loaded successfully", ItemSet.HtmlEmailBannerImage.DefaultValue);
			AssertEquals("Name", "HtmlEmailBannerImage", ItemSet.HtmlEmailBannerImage.Name);
			AssertEquals("Storage", RegistryStorageFlags.All, ItemSet.HtmlEmailBannerImage.Storage);
		}

		public void TestHtmlEmailFooterImage()
		{
			AssertVisible(ItemSet.HtmlEmailFooterImage);
			AssertNotNull("Default image should be loaded successfully", ItemSet.HtmlEmailFooterImage.DefaultValue);
			AssertEquals("Name", "HtmlEmailFooterImage", ItemSet.HtmlEmailFooterImage.Name);
			AssertEquals("Storage", RegistryStorageFlags.All, ItemSet.HtmlEmailFooterImage.Storage);
		}

		public void TestHtmlEmailStyleSheet()
		{
			#region ExpectedDefaultValue

			string expectedDefaultValue = @"
P {
		FONT-SIZE: 12px;
		COLOR: #666666;
		FONT-FAMILY: Arial, sans-serif;
		line-height: 17px;
}

body {
		background-color: #FFFFFF;
	font-family: Arial, sans-serif;
	font-size: 12px;
	color: #666666;
		margin: auto;
		width: 600px;
}

th {
	font-family: Arial, sans-serif;
	font-size: 12px;
	color: #FFFFFF;
	background-color: #005596;
}

td {
	font-family: Arial, sans-serif;
	font-size: 12px;
	color: #666666;
}

a, a:visited {
	color: #666666;
	text-decoration: underline;
}

a:hover {
	color: #00a4e4;
	text-decoration: underline;
}

.heading1 {
		font-family: Arial, sans-serif;
		font-size: 20px;
		font-weight: normal;
		color: #000000;
		line-height: 36px;
}

.subheading2 {
		font-family: Arial, sans-serif;
		font-size: 16px;
		line-height: 18px;
		font-weight: bold;
	color: #00a4e4;
}

.table {
		border-right: #666666 solid thin;
		border-top: #666666 solid thin;
		border-left: #666666 solid thin;
		border-bottom: #666666 solid thin;
}

.tableheadings td, th {
		border-bottom: #666666 solid thin;
		background-color: #005596;
}

.content {
		padding: 0em 1em;
}

.banner {
		padding-top: 1em;
}

".Trim();

			#endregion

			AssertVisible(ItemSet.HtmlEmailStyleSheet);
			AssertMultilineASCIIEquals("DefaultValue", expectedDefaultValue, ItemSet.HtmlEmailStyleSheet.DefaultValue);
			AssertEquals("Name", "HtmlEmailStyleSheet", ItemSet.HtmlEmailStyleSheet.Name);
			AssertEquals("Storage", RegistryStorageFlags.All, ItemSet.HtmlEmailStyleSheet.Storage);
			AssertEquals("EditorInfo", TextEditorType.Memo, ((TextRegistryEditorInfo)ItemSet.HtmlEmailStyleSheet.EditorInfo).EditorType);
		}

		public void TestUseLoginBranchLogoForFreight()
		{
			AssertVisible(ItemSet.UseLoginBranchLogoForFreight);
			AssertEquals("Name", "UseLoginBranchLogoForFreight", ItemSet.UseLoginBranchLogoForFreight.Name);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.UseLoginBranchLogoForFreight.Storage);
			AssertEquals("Default Value", false, ItemSet.UseLoginBranchLogoForFreight.DefaultValue);
		}

		#endregion

		#region Data Export Settings

		public void TestExportEDICodeMapping()
		{
			AssertEquals("ExportEDICodeMapping Should be false by default", false, ItemSet.ExportEDICodeMapping.DefaultValue);
			ItemSet.ExportEDICodeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("ExportEDICodeMapping should have been set to true", true, ItemSet.ExportEDICodeMapping.Value);
		}

		public void TestAccountingTransactionsExportHighWaterMark()
		{
			TestGenericRegistryItem(ItemSet.AccountingTransactionsExportHighWaterMark,
				"AccountingTransactionsExportHighWaterMark",
				"System/Data Export Settings",
				"Accounting Transactions Export High Water Mark",
				"To aid performance of the Accounting Transactions Export, the system will only search for un-batched transactions that were created or edited since this date.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				DateTime.MinValue);
			AssertType(typeof(AccountingTransactionExportHighWaterMarkDataType), ItemSet.AccountingTransactionsExportHighWaterMark.DataType);
		}

		#endregion

		#region Data Import Settings

		public void TestXmlSchemaValidationStrict()
		{
			AssertEquals("Should be false by default to allow XML files generated from external ediEnterprise systems with a greater version to import", false, ItemSet.XmlSchemaValidationStrict.Value);
			ItemSet.XmlSchemaValidationStrict.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Should set the registry item", true, ItemSet.XmlSchemaValidationStrict.Value);
		}

		public void TestXmlImportSpecifiedElementsOnly()
		{
			AssertEquals("Should be TRUE by default to prevent overwriting existing data with blank values from non-specified XML elements", true, ItemSet.XmlImportSpecifiedElementsOnly.Value);
			ItemSet.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Should set the registry item", false, ItemSet.XmlImportSpecifiedElementsOnly.Value);
		}

		public void TestAllowBillingImportIntoShipment()
		{
			AssertEquals("Should be OFF by default for AllowBillingImportIntoShipment", false, ItemSet.AllowBillingImportIntoShipment.Value);
			AssertEquals("AllowBillingImportIntoShipment Should be visible for all", RegistryOptions.Default, ItemSet.AllowBillingImportIntoShipment.Options);
			ItemSet.AllowBillingImportIntoShipment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Should set the registry item", false, ItemSet.AllowBillingImportIntoShipment.Value);
		}

		public void TestCompleteOrderLineUpdate()
		{
			AssertEquals("Should be OFF by default for CompleteOrderLineUpdate", false, ItemSet.CompleteOrderLineUpdate.DefaultValue);
			AssertEquals("CompleteOrderLineUpdate Should be visible for support or Developer only", RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport, ItemSet.CompleteOrderLineUpdate.Options);
			ItemSet.CompleteOrderLineUpdate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Should set the registry item", true, ItemSet.CompleteOrderLineUpdate.Value);
		}

		public void TestAllowMatchingByOtherAgentReferencesOnImport()
		{
			BooleanRegistryItem allowMatchingItem = ItemSet.AllowMatchingByOtherAgentReferencesOnImport;

			AssertEquals("Should be ON by default", true, allowMatchingItem.Value);
			AssertEquals("Should be only for developers", RegistryOptions.IsOnlyForDevelopers, allowMatchingItem.Options);

			allowMatchingItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Should set the registry item", false, allowMatchingItem.Value);
		}

		public void TestAllowLinkingStandaloneShipmentToConsol()
		{
			AssertEquals("Should be disabled by default for AllowLinkingStandaloneShipmentToConsol", false, ItemSet.AllowLinkingStandaloneShipmentToConsol.Value);
			AssertEquals("AllowBillingImportIntoShipment Should be visible for support and developer only", RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers, ItemSet.AllowLinkingStandaloneShipmentToConsol.Options);
			ItemSet.AllowLinkingStandaloneShipmentToConsol.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Should set the registry item", true, ItemSet.AllowLinkingStandaloneShipmentToConsol.Value);
		}

		public void TestAllowUpdateShipmentOrderDuringAutomaticConsolImport()
		{
			AssertEquals("Should be disabled by default for UpdateShipmentOrdersDuringConsolAutomaticImport", false, ItemSet.UpdateShipmentOrdersDuringConsolAutomaticImport.Value);
			AssertEquals("UpdateShipmentOrdersDuringConsolAutomaticImport Should be visible for support and developer only", RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers, ItemSet.UpdateShipmentOrdersDuringConsolAutomaticImport.Options);
			ItemSet.UpdateShipmentOrdersDuringConsolAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Should set the registry item", true, ItemSet.UpdateShipmentOrdersDuringConsolAutomaticImport.Value);
		}

		public void TestAllowUpdateShipmentOrderDuringAutomaticOrderImport()
		{
			AssertEquals("Should be disabled by default for UpdateShipmentOrdersDuringAutomaticImport", false, ItemSet.UpdateShipmentOrdersDuringAutomaticImport.Value);
			AssertEquals("UpdateShipmentOrdersDuringAutomaticImport Should be visible for support and developer only", RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers, ItemSet.UpdateShipmentOrdersDuringAutomaticImport.Options);
			ItemSet.UpdateShipmentOrdersDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Should set the registry item", true, ItemSet.UpdateShipmentOrdersDuringAutomaticImport.Value);
		}

		public void TestAllowUpdateShipmentOrderDuringAutomaticShipementImport()
		{
			AssertEquals("Should be disabled by default for UpdateShipmentOrdersDuringShipmentAutomaticImport", false, ItemSet.UpdateShipmentOrdersDuringShipmentAutomaticImport.Value);
			AssertEquals("UpdateShipmentOrdersDuringShipmentAutomaticImport Should be visible for support and developer only", RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers, ItemSet.UpdateShipmentOrdersDuringShipmentAutomaticImport.Options);
			ItemSet.UpdateShipmentOrdersDuringShipmentAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Should set the registry item", true, ItemSet.UpdateShipmentOrdersDuringShipmentAutomaticImport.Value);
		}

		#region System Merge

		public void TestActivateSystemMergeDataInterface()
		{
			AssertEquals("ItemSet.ActivateSystemMergeDataInterface.DefaultValue", false, ItemSet.ActivateSystemMergeDataInterface.DefaultValue);
			ItemSet.ActivateSystemMergeDataInterface.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("ActivateSystemMergeDataInterface", true, ItemSet.ActivateSystemMergeDataInterface.Value);
		}

		#endregion

		#region Warehouse Data Import Directory

		public void TestWarehouseDataImportDirectory()
		{
			ItemSet.WarehouseDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Y:\\Test");
			AssertEquals("WarehouseDataImportDirectory.Value", "Y:\\Test", ItemSet.WarehouseDataImportDirectory.Value);
		}

		#endregion

		#region Warehouse Create Missing Products

		public void TestCreateMissingWarehouseProduct()
		{
			AssertEquals("Default Value", false, ItemSet.CreateMissingWarehouseProduct.DefaultValue);
			ItemSet.CreateMissingWarehouseProduct.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Create Missing Products When Importing", true, ItemSet.CreateMissingWarehouseProduct.Value);
		}

		#endregion

		#region Unprocessed Message Folder

		public void TestUnprocessedMessageDirectory()
		{
			ItemSet.UnprocessedMessagesDataDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Y:\\Test");
			AssertEquals("UnprocessedMessagesDataDirectory.Value", "Y:\\Test", ItemSet.UnprocessedMessagesDataDirectory.Value);
		}

		#endregion

		#region Invoices in PDF format

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Testing")]
		public void TestShowInvoicePDFExportRegistrySettings()
		{
			TestGenericRegistryItem(ItemSet.ShowInvoicePDFExportRegistrySettingsRaw, "ShowInvoicePDFExportRegistrySettings", "System/Data Export Settings/Invoices in PDF format", $"Show Invoice PDF Export Registry Settings ({Core.Constants.ProductSupportName} Only)", "Show Invoice PDF Export Registry Settings: Invoice Types, Export Directory", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
			ItemSet.ShowInvoicePDFExportRegistrySettingsRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("ShowInvoicePDFExportRegistrySettings", true, ItemSet.ShowInvoicePDFExportRegistrySettings);
		}

		public void TestInvoiceTypesToExportInPDFFormat()
		{
			CodeDescriptionBoolCollection newRoles = new CodeDescriptionBoolCollection();
			CodeDescriptionBool role = newRoles.AddNew();
			role.Code = ExportInvoiceTypesInPDFFormat.ConsolInvoice;
			role.Bool = true;
			role = newRoles.AddNew();
			role.Code = ExportInvoiceTypesInPDFFormat.ShipmentInvoice;
			role.Bool = true;

			ItemSet.InvoiceTypesToExportInPDFFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newRoles);

			AssertNotNull("ItemSet.InvoiceTypesToExportInPDFFormat.Value should not be null", ItemSet.InvoiceTypesToExportInPDFFormat.Value);
			AssertEquals("Should be two element", 2, ItemSet.InvoiceTypesToExportInPDFFormat.Value.Count);
			AssertEquals("The 1st element's Code should be: ", ExportInvoiceTypesInPDFFormat.ConsolInvoice, ItemSet.InvoiceTypesToExportInPDFFormat.Value[0].Code);
			AssertEquals("The 1st element's Bool should be True", true, ItemSet.InvoiceTypesToExportInPDFFormat.Value[0].Bool);
			AssertEquals("The 2nd element's Code should be: ", ExportInvoiceTypesInPDFFormat.ShipmentInvoice, ItemSet.InvoiceTypesToExportInPDFFormat.Value[1].Code);
			AssertEquals("The 2nd element's Bool should be True", true, ItemSet.InvoiceTypesToExportInPDFFormat.Value[1].Bool);
		}

		public void TestInvoicesInPDFFormatExportDirectory()
		{
			AssertEquals("InvoicesInPDFFormatExportDirectory default value", ZString.Empty, ItemSet.InvoicesInPDFFormatExportDirectory.DefaultValue);
			ItemSet.InvoicesInPDFFormatExportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Env.TempPath);
			AssertEquals("InvoicesInPDFFormatExportDirectory.value", Env.TempPath, ItemSet.InvoicesInPDFFormatExportDirectory.Value);
		}

		#endregion

		#region Accounting GL Transaction Export to CSV

		public void TestGLTransactionsCSVExportDirectory()
		{
			AssertEquals("GLTransactionsCSVExportDirectory default value", ZString.Empty, ItemSet.GLTransactionsCSVExportDirectory.DefaultValue);
			ItemSet.GLTransactionsCSVExportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Env.TempPath);
			AssertEquals("GLTransactionsCSVExportDirectory.value", Env.TempPath, ItemSet.GLTransactionsCSVExportDirectory.Value);
		}

		public void TestEnableAutomaticGLTransactionsCSVExport()
		{
			AssertEquals("EnableAutomaticGLTransactionsCSVExport default value", false, ItemSet.EnableAutomaticGLTransactionsCSVExport.DefaultValue);
			ItemSet.EnableAutomaticGLTransactionsCSVExport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("EnableAutomaticGLTransactionsCSVExport.Value", true, ItemSet.EnableAutomaticGLTransactionsCSVExport.Value);
		}

		public void TestGLTransCSVExportNotificationGroup()
		{
			AssertEquals("GLTransCSVExportNotificationGroup default value", Enterprise.Core.Constants.Groups.PostMastersGroupPK, ItemSet.GLTransCSVExportNotificationGroup.DefaultValue);
			ItemSet.GLTransCSVExportNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.Groups.AllPK);
			AssertEquals("GLTransCSVExportNotificationGroup.value", Enterprise.Core.Constants.Groups.AllPK, ItemSet.GLTransCSVExportNotificationGroup.Value);
		}

		public void TestGLTransactionsCSVExportHighWaterMark()
		{
			TestGenericRegistryItem(ItemSet.GLTransactionsCSVExportHighWaterMark,
				"GLTransactionsCSVExportHighWaterMark",
				"System/Data Export Settings/Export GL Transactions to CSV",
				"GL Transactions CSV Export High Water Mark",
				"To aid performance of the GL Transactions CSV Export, the system will only search for un-batched transactions that were created or edited since this date.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				DateTime.MinValue);
			AssertType(typeof(AccountingTransactionExportHighWaterMarkDataType), ItemSet.GLTransactionsCSVExportHighWaterMark.DataType);
		}

		#endregion

		#region Unprocessed Message Notifications

		public void TestUnprocessedMessageNotificationEmailGroup()
		{
			Guid allUsersGroup = GetAllUsersGlbGroup().PK.ToGuid();
			AssertEquals("DefaultMessagesNotificationEmailGroup.DefaultValue", allUsersGroup, ItemSet.UnprocessedMessageNotificationGroup.DefaultValue);

			Guid newGuid = Guid.NewGuid();
			ItemSet.UnprocessedMessageNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("DefaultMessagesNotificationEmailGroup.Value", newGuid, ItemSet.UnprocessedMessageNotificationGroup.Value);
		}

		#endregion

		public void TestShipmentImportBranchRules()
		{
			TestGenericRegistryItem(ItemSet.ShipmentImportBranchRules,
				"ShipmentImportBranchRules",
				"System/Data Import Settings/Shipment",
				"Shipment Import Branch Rules",
				@"Override these values to set the order of the default branch on creation of a new Shipment from the import XML if the Branch specified in the XML import file is not found or is to be discarded.
The values can be between 0 and 2. A value of 0 means the rule will not be used.
There can be multiple rules with a value of 0.
Any value greater than 1 must not be duplicated, i.e. 1, 1 is not valid.
There must not be a gap between the sequence of numbers for the values, i.e. 1, 2 is valid, but 0, 2 is not valid.
One of the fallback rules ""Default to a any"" or ""Do not create"" must be selected.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}

		public void TestConsolImportBranchRules()
		{
			TestGenericRegistryItem(ItemSet.ConsolImportBranchRules,
				"ConsolImportBranchRules",
				"System/Data Import Settings/Consols",
				"Consol Import Branch Rules",
				@"Override these values to set the order of the default branch on creation of a new Consol from the import XML if the Branch specified in the XML import file is not found or is to be discarded.
The values can be between 0 and 2. A value of 0 means the rule will not be used.
There can be multiple rules with a value of 0.
Any value greater than 1 must not be duplicated, i.e. 1, 1 is not valid.
There must not be a gap between the sequence of numbers for the values, i.e. 1, 2 is valid, but 0, 2 is not valid.
One of the fallback rules ""Default to a any"" or ""Do not create"" must be selected.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}

		public void TestDefaultMessagesDataImportDirectory()
		{
			ItemSet.DefaultMessagesDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Y:\\Test");
			AssertEquals("DefaultMessagesDataImportDirectory.Value", "Y:\\Test", ItemSet.DefaultMessagesDataImportDirectory.Value);
		}

		public void TestCreateMissingProductWithRelationship()
		{
			TestGenericRegistryItem(ItemSet.CreateMissingProductWithRelationship,
				"CreateMissingProductWithRelationship",
				"System/Data Import Settings/Orders",
				"Create Missing Products",
				"Create Missing Product when doing Import Process",
				RegistryStorageFlags.System);
		}

		public void TestAutomatedDataImportDirectories()
		{
			ItemSet.BookingsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Bookings");
			ItemSet.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Consols");
			ItemSet.CustomsDeclarationsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Customs Declarations");
			ItemSet.OrdersXMLDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Orders");
			ItemSet.OrdersCSVDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Orders");
			ItemSet.CommercialInvoicesDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CommercialInvoicesCsv");
			ItemSet.CommercialInvoicesDataImportDirectoryXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CommercialInvoicesXml");
			ItemSet.PODDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PODs");
			ItemSet.ProductsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Products");
			ItemSet.ProductsXMLDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Products");
			ItemSet.LocalCartageDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "LocalCartage");
			ItemSet.ShipmentDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Shipment");
			ItemSet.ScheduleDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Schedule");
			ItemSet.ContainerEventsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ContainerEvents");
			ItemSet.ProductLastCostImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Product Last Cost");
			ItemSet.EventDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Event");

			AssertEquals("BookingsDataImportDirectory.Value", "Bookings", ItemSet.BookingsDataImportDirectory.Value);
			AssertEquals("ConsolsDataImportDirectory.Value", "Consols", ItemSet.ConsolsDataImportDirectory.Value);
			AssertEquals("CustomsDeclarationsDataImportDirectory.Value", "Customs Declarations", ItemSet.CustomsDeclarationsDataImportDirectory.Value);
			AssertEquals("OrdersDataImportDirectory.Value", "Orders", ItemSet.OrdersXMLDataImportDirectory.Value);
			AssertEquals("OrdersCSVDataImportDirectory.Value", "Orders", ItemSet.OrdersCSVDataImportDirectory.Value);
			AssertEquals("CommercialInvoicesImportDirectory.Value", "CommercialInvoicesCsv", ItemSet.CommercialInvoicesDataImportDirectory.Value);
			AssertEquals("CommercialInvoicesImportDirectoryXml.Value", "CommercialInvoicesXml", ItemSet.CommercialInvoicesDataImportDirectoryXml.Value);
			AssertEquals("PODDataImportDirectory.Value", "PODs", ItemSet.PODDataImportDirectory.Value);
			AssertEquals("ProductsDataImportDirectory.Value", "Products", ItemSet.ProductsDataImportDirectory.Value);
			AssertEquals("ProductsXMLDataImportDirectory.Value", "Products", ItemSet.ProductsXMLDataImportDirectory.Value);
			AssertEquals("LocalCartageDataImportDirectory.Value", "LocalCartage", ItemSet.LocalCartageDataImportDirectory.Value);
			AssertEquals("ShipmentDataImportDirectory.Value", "Shipment", ItemSet.ShipmentDataImportDirectory.Value);
			AssertEquals("ScheduleDataImportDirectory.Value", "Schedule", ItemSet.ScheduleDataImportDirectory.Value);
			AssertEquals("ContainerEventsDataImportDirectory.Value", "ContainerEvents", ItemSet.ContainerEventsDataImportDirectory.Value);
			AssertEquals("ProductLastCostImportDirectory.Value", "Product Last Cost", ItemSet.ProductLastCostImportDirectory.Value);
			AssertEquals("EventDataImportDirectory.Value", "Event", ItemSet.EventDataImportDirectory.Value);
		}

		public void TestUpdateScheduleDuringAutomaticImport()
		{
			AssertEquals("UpdateScheduleDuringAutomaticImport.DefaultValue", true, ItemSet.UpdateSchedulesDuringAutomaticImport.DefaultValue);
			ItemSet.UpdateSchedulesDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("UpdateScheduleDuringAutomaticImport", false, ItemSet.UpdateSchedulesDuringAutomaticImport.Value);
		}

		public void TestFlightScheduleUpdateThresholdForDataImport()
		{
			AssertEquals("FlightScheduleUpdateThresholdForDataImport.DefaultValue", 24, ItemSet.FlightScheduleUpdateThresholdForDataImport.DefaultValue);

			ItemSet.FlightScheduleUpdateThresholdForDataImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			AssertEquals("Should be set", 1, ItemSet.FlightScheduleUpdateThresholdForDataImport.Value);

			AssertEquals("Expected error", "Value must be greater than or equal to the minimum (1)",
				ItemSet.FlightScheduleUpdateThresholdForDataImport.GetValidationErrorMessage(0, Guid.Empty, Guid.Empty, Guid.Empty));

			AssertEquals("Expected error", "Value must be less than or equal to the maximum (24)",
				ItemSet.FlightScheduleUpdateThresholdForDataImport.GetValidationErrorMessage(25, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestConsolsBooleanRegistryItems()
		{
			AssertEquals("AlwaysCheckForConsolSailingShipmentsAndContainers.DefaultValue", false, ItemSet.AlwaysCheckForConsolSailingShipmentsAndContainers.DefaultValue);
			AssertEquals("UpdateConsolDuringAutomaticImportOther.DefaultValue", true, ItemSet.UpdateConsolDuringAutomaticImportOther.DefaultValue);
			AssertEquals("UpdateConsolDuringAutomaticImportAir.DefaultValue", true, ItemSet.UpdateConsolDuringAutomaticImportAir.DefaultValue);
			AssertEquals("UpdateConsolDuringAutomaticImportSea.DefaultValue", true, ItemSet.UpdateConsolDuringAutomaticImportSea.DefaultValue);
			AssertEquals("UpdateConsolSailingDuringAutomaticImport.DefaultValue", true, ItemSet.UpdateSailingSchedulesDuringAutomaticImport.DefaultValue);
			AssertEquals("UpdateConsolsRoutingInformationDuringAutomaticImport.DefaultValue", true, ItemSet.UpdateConsolsRoutingInformationDuringAutomaticImport.DefaultValue);
			AssertEquals("UpdateConsolContainersDuringAutomaticImport.DefaultValue", true, ItemSet.UpdateConsolContainersDuringAutomaticImport.DefaultValue);
			AssertEquals("UpdateConsolShipmentsDuringAutomaticImportOther.DefaultValue", true, ItemSet.UpdateConsolShipmentsDuringAutomaticImportOther.DefaultValue);
			AssertEquals("UpdateConsolShipmentsDuringAutomaticImportAir.DefaultValue", true, ItemSet.UpdateConsolShipmentsDuringAutomaticImportAir.DefaultValue);
			AssertEquals("UpdateConsolShipmentsDuringAutomaticImportSea.DefaultValue", true, ItemSet.UpdateConsolShipmentsDuringAutomaticImportSea.DefaultValue);
			AssertEquals("AutoCreateDeclarationWithinShipment.DefaultValue", true, ItemSet.AutoCreateDeclarationWithinShipment.DefaultValue);

			ItemSet.AlwaysCheckForConsolSailingShipmentsAndContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ItemSet.UpdateConsolDuringAutomaticImportOther.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ItemSet.UpdateConsolDuringAutomaticImportAir.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ItemSet.UpdateConsolDuringAutomaticImportSea.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ItemSet.UpdateSailingSchedulesDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ItemSet.UpdateConsolsRoutingInformationDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ItemSet.UpdateConsolContainersDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ItemSet.UpdateConsolShipmentsDuringAutomaticImportOther.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ItemSet.UpdateConsolShipmentsDuringAutomaticImportAir.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ItemSet.UpdateConsolShipmentsDuringAutomaticImportSea.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ItemSet.AutoCreateDeclarationWithinShipment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertEquals("AlwaysCheckForConsolSailingShipmentsAndContainers.Value", true, ItemSet.AlwaysCheckForConsolSailingShipmentsAndContainers.Value);
			AssertEquals("UpdateConsolDuringAutomaticImportOther.Value", false, ItemSet.UpdateConsolDuringAutomaticImportOther.Value);
			AssertEquals("UpdateConsolDuringAutomaticImportAir.Value", false, ItemSet.UpdateConsolDuringAutomaticImportAir.Value);
			AssertEquals("UpdateConsolDuringAutomaticImportSea.Value", false, ItemSet.UpdateConsolDuringAutomaticImportSea.Value);
			AssertEquals("UpdateConsolSailingDuringAutomaticImport.Value", false, ItemSet.UpdateSailingSchedulesDuringAutomaticImport.Value);
			AssertEquals("UpdateConsolsRoutingInformationDuringAutomaticImport.Value", false, ItemSet.UpdateConsolsRoutingInformationDuringAutomaticImport.Value);
			AssertEquals("UpdateConsolContainersDuringAutomaticImport.Value", false, ItemSet.UpdateConsolContainersDuringAutomaticImport.Value);
			AssertEquals("UpdateConsolShipmentsDuringAutomaticImportOther.Value", false, ItemSet.UpdateConsolShipmentsDuringAutomaticImportOther.Value);
			AssertEquals("UpdateConsolShipmentsDuringAutomaticImportAir.Value", false, ItemSet.UpdateConsolShipmentsDuringAutomaticImportAir.Value);
			AssertEquals("UpdateConsolShipmentsDuringAutomaticImportSea.Value", false, ItemSet.UpdateConsolShipmentsDuringAutomaticImportSea.Value);
			AssertEquals("AutoCreateDeclarationWithinShipment.DefaultValue", false, ItemSet.AutoCreateDeclarationWithinShipment.Value);
		}

		public void TestImportConsolShipmOrDeclNoFromXml()
		{
			AssertEquals("ImportConsolNoFromXml.DefaultValue", false, ItemSet.ImportConsolNoFromXml.DefaultValue);
			AssertEquals("ImportShipmentNoFromXml.DefaultValue", Enterprise.Core.Constants.ShipmentNumberImportTypes.Code.HouseBill, ItemSet.ImportShipmentNoFromXml.DefaultValue);
			AssertEquals("ImportDeclarationNoFromXml.DefaultValue", false, ItemSet.ImportDeclarationNoFromXml.DefaultValue);
			AssertEquals("AllowCustomsDeclarationUpdateItem.DefaultValue", false, ItemSet.AllowCustomsDeclarationUpdateItem.DefaultValue);

			ItemSet.ImportConsolNoFromXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ItemSet.ImportShipmentNoFromXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ShipmentNumberImportTypes.Code.ShipmentNumber);
			ItemSet.ImportDeclarationNoFromXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ItemSet.AllowCustomsDeclarationUpdateItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals("ImportConsolNoFromXml.DefaultValue", true, ItemSet.ImportConsolNoFromXml.Value);
			AssertEquals("ImportShipmentNoFromXml.DefaultValue", Enterprise.Core.Constants.ShipmentNumberImportTypes.Code.ShipmentNumber, ItemSet.ImportShipmentNoFromXml.Value);
			AssertEquals("ImportDeclarationNoFromXml.DefaultValue", true, ItemSet.ImportDeclarationNoFromXml.Value);
			AssertEquals("AllowCustomsDeclarationUpdateItem.DefaultValue", true, ItemSet.AllowCustomsDeclarationUpdateItem.Value);

			ZQuery query = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK);
			BusinessObject anotherCo = (BusinessObject)Factory.LoadTop1<IGlbCompany>(query);

			ItemSet.ImportConsolNoFromXml.SetValue(anotherCo.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			ItemSet.ImportConsolNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			bool importConsolNoFromXmlForCurrentCo = ItemSet.ImportConsolNoFromXml.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			bool importConsolNoFromXmlForTheOtherCo = ItemSet.ImportConsolNoFromXml.GetFallBackValueAtAllLevels(anotherCo.PK.ToGuid(), Guid.Empty, Guid.Empty);

			AssertEquals("ImportConsolNoFromXml.DefaultValue for current company", false, importConsolNoFromXmlForCurrentCo);
			AssertEquals("ImportConsolNoFromXml.DefaultValue for the other company", true, importConsolNoFromXmlForTheOtherCo);
		}

		public void TestUpdateShipmentsDuringAutomaticImport()
		{
			AssertEquals(Enterprise.Core.Constants.ShipmentAutomaticImportOptions.Code.NoImport, ItemSet.UpdateShipmentsDuringAutomaticImport.DefaultValue);

			ItemSet.UpdateShipmentsDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ShipmentAutomaticImportOptions.Code.Create);

			AssertEquals(Enterprise.Core.Constants.ShipmentAutomaticImportOptions.Code.Create, ItemSet.UpdateShipmentsDuringAutomaticImport.Value);

			ZQuery query = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK);
			BusinessObject anotherCo = (BusinessObject)Factory.LoadTop1<IGlbCompany>(query);

			ItemSet.UpdateShipmentsDuringAutomaticImport.SetValue(anotherCo.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ShipmentAutomaticImportOptions.Code.Create);
			ItemSet.UpdateShipmentsDuringAutomaticImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ShipmentAutomaticImportOptions.Code.Update);

			string currentCo = ItemSet.UpdateShipmentsDuringAutomaticImport.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			string otherCo = ItemSet.UpdateShipmentsDuringAutomaticImport.GetFallBackValueAtAllLevels(anotherCo.PK.ToGuid(), Guid.Empty, Guid.Empty);

			AssertEquals(Enterprise.Core.Constants.ShipmentAutomaticImportOptions.Code.Create, otherCo);
			AssertEquals(Enterprise.Core.Constants.ShipmentAutomaticImportOptions.Code.Update, currentCo);
		}

		public void TestACStatusRegistry()
		{
			TestStringRegistryItem(ItemSet.ACStatusDataImportDirectory, "ACStatusDataImportDirectory", "System/Data Import Settings/Customs Declarations",
				"Folder to scan for Advantage Customs Status XML files",
				"The folder specified here should contain any Advantage Customs Status XML files that are to be automatically imported.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				TextEditorType.DirectoryBrowser,
				RegistryOptions.IsHidden,
				"",
				CharacterCase.Normal);
		}

		public void TestProductsBooleanRegistryItems()
		{
			AssertEquals("UpdateProductsDuringAutomaticImport.DefaultValue", false, ItemSet.UpdateProductsDuringAutomaticImport.DefaultValue);
			ItemSet.UpdateProductsDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("UpdateProductsDuringAutomaticImport.Value", true, ItemSet.UpdateProductsDuringAutomaticImport.Value);

			AssertEquals("UseLegacyCodesDuringAutomaticImport.DefaultValue", false, ItemSet.UseLegacyCodesDuringAutomaticImport.DefaultValue);
			ItemSet.UseLegacyCodesDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("UseLegacyCodesDuringAutomaticImport.Value", true, ItemSet.UpdateProductsDuringAutomaticImport.Value);
		}

		public void TestAutomaticallyCreateAirCargoMessageDescription()
		{
			string category = ItemSet.ConsolsDataImportDirectory.Category;
			string caption = ItemSet.ConsolsDataImportDirectory.Caption;
			AssertEquals("AutomaticallyCreateAirCargoMessage.Hint should contain ConsolsDataImportDirectory's Category", true, ItemSet.AutomaticallyCreateAirCargoJob.Hint.Contains(category));
			AssertEquals("AutomaticallyCreateAirCargoMessage.Hint should contain ConsolsDataImportDirectory's Caption", true, ItemSet.AutomaticallyCreateAirCargoJob.Hint.Contains(caption));
		}

		public void TestAutomaticallyCreateAirCargoMessage()
		{
			AssertEquals("Default value should be false", false, ItemSet.AutomaticallyCreateAirCargoJob.DefaultValue);
			ItemSet.AutomaticallyCreateAirCargoJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("AutomaticallyCreateAirCargoMessage.Value should now be true", true, ItemSet.AutomaticallyCreateAirCargoJob.Value);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, AustralianBranchPk, Env.CurrentDepartment.PK))
			{
				AssertEquals("IsVisible should be true in Australia", true, ItemSet.AutomaticallyCreateAirCargoJob.IsVisible(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
			}

			Guid departmentBRNPK = (Guid)Utilities.GetFieldFromTable(GlbDepartmentSchema.PK.Name, GlbDepartmentSchema.Constants.TableName, GlbDepartmentSchema.GE_Code, "BRN");
			Guid singaporeBranchPK = (Guid)Utilities.GetFieldFromTable(GlbBranchSchema.PK.Name, GlbBranchSchema.Constants.TableName, GlbBranchSchema.GB_Code, "SIN");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singaporeBranchPK, departmentBRNPK))
			{
				AssertEquals("IsVisible should be false when not in Australia", false, ItemSet.AutomaticallyCreateAirCargoJob.IsVisible(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
			}
		}

		#region Send Air Message

		public void TestAutomaticallySendAirCargoMessageDescription()
		{
			string category = ItemSet.ConsolsDataImportDirectory.Category;
			string caption = ItemSet.ConsolsDataImportDirectory.Caption;
			AssertEquals("AutomaticallySendAirCargoMessage.Hint should contain ConsolsDataImportDirectory's Category", true, ItemSet.AutomaticallySendAirCargoMessage.Hint.Contains(category));
			AssertEquals("AutomaticallySendAirCargoMessage.Hint should contain ConsolsDataImportDirectory's Caption", true, ItemSet.AutomaticallySendAirCargoMessage.Hint.Contains(caption));
		}

		public void TestAutomaticallySendAirCargoMessage()
		{
			AssertEquals("Default value should be false", false, ItemSet.AutomaticallySendAirCargoMessage.DefaultValue);
			ItemSet.AutomaticallySendAirCargoMessage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("AutomaticallySendAirCargoMessage.Value should now be true", true, ItemSet.AutomaticallySendAirCargoMessage.Value);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, AustralianBranchPk, Env.CurrentDepartment.PK))
			{
				AssertEquals("IsVisible should be true in Australia", true, ItemSet.AutomaticallySendAirCargoMessage.IsVisible(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
			}

			Guid departmentBRNPK = (Guid)Utilities.GetFieldFromTable(GlbDepartmentSchema.PK.Name, GlbDepartmentSchema.Constants.TableName, GlbDepartmentSchema.GE_Code, "BRN");
			Guid singaporeBranchPK = (Guid)Utilities.GetFieldFromTable(GlbBranchSchema.PK.Name, GlbBranchSchema.Constants.TableName, GlbBranchSchema.GB_Code, "SIN");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singaporeBranchPK, departmentBRNPK))
			{
				AssertEquals("IsVisible should be false when not in Australia", false, ItemSet.AutomaticallySendAirCargoMessage.IsVisible(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
			}
		}

		#endregion

		#region Send Sea Message

		public void TestAutomaticallySendSeaCargoMessageDescription()
		{
			string category = ItemSet.ConsolsDataImportDirectory.Category;
			string caption = ItemSet.ConsolsDataImportDirectory.Caption;
			AssertEquals("AutomaticallySendSeaCargoMessage.Hint should contain ConsolsDataImportDirectory's Category", true, ItemSet.AutomaticallySendSeaCargoMessage.Hint.Contains(category));
			AssertEquals("AutomaticallySendSeaCargoMessage.Hint should contain ConsolsDataImportDirectory's Caption", true, ItemSet.AutomaticallySendSeaCargoMessage.Hint.Contains(caption));
		}

		public void TestAutomaticallySendSeaCargoMessage()
		{
			AssertEquals("Default value should be false", false, ItemSet.AutomaticallySendSeaCargoMessage.DefaultValue);
			ItemSet.AutomaticallySendSeaCargoMessage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("AutomaticallySendSeaCargoMessage.Value should now be true", true, ItemSet.AutomaticallySendSeaCargoMessage.Value);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, AustralianBranchPk, Env.CurrentDepartment.PK))
			{
				AssertEquals("IsVisible should be true in Australia", true, ItemSet.AutomaticallySendSeaCargoMessage.IsVisible(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
			}

			Guid departmentBRNPK = (Guid)Utilities.GetFieldFromTable(GlbDepartmentSchema.PK.Name, GlbDepartmentSchema.Constants.TableName, GlbDepartmentSchema.GE_Code, "BRN");
			Guid singaporeBranchPK = (Guid)Utilities.GetFieldFromTable(GlbBranchSchema.PK.Name, GlbBranchSchema.Constants.TableName, GlbBranchSchema.GB_Code, "SIN");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singaporeBranchPK, departmentBRNPK))
			{
				AssertEquals("IsVisible should be false when not in Australia", false, ItemSet.AutomaticallySendSeaCargoMessage.IsVisible(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
			}
		}

		#endregion

		public void TestAutomaticallyCreateSeaCargoMessageDescription()
		{
			string category = ItemSet.ConsolsDataImportDirectory.Category;
			string caption = ItemSet.ConsolsDataImportDirectory.Caption;
			AssertEquals("AutomaticallyCreateSeaCargoMessage.Hint should contain ConsolsDataImportDirectory's Category", true, ItemSet.AutomaticallyCreateSeaCargoJob.Hint.Contains(category));
			AssertEquals("AutomaticallyCreateSeaCargoMessage.Hint should contain ConsolsDataImportDirectory's Caption", true, ItemSet.AutomaticallyCreateSeaCargoJob.Hint.Contains(caption));
		}

		public void TestAutomaticallyCreateSeaCargoMessage()
		{
			AssertEquals("Default value should be false", false, ItemSet.AutomaticallyCreateSeaCargoJob.DefaultValue);
			ItemSet.AutomaticallyCreateSeaCargoJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("AutomaticallyCreateSeaCargoMessage.Value should now be true", true, ItemSet.AutomaticallyCreateSeaCargoJob.Value);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, AustralianBranchPk, Env.CurrentDepartment.PK))
			{
				AssertEquals("IsVisible should be true in Australia", true, ItemSet.AutomaticallyCreateSeaCargoJob.IsVisible(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
			}

			Guid departmentBRNPK = (Guid)Utilities.GetFieldFromTable(GlbDepartmentSchema.PK.Name, GlbDepartmentSchema.Constants.TableName, GlbDepartmentSchema.GE_Code, "BRN");
			Guid singaporeBranchPK = (Guid)Utilities.GetFieldFromTable(GlbBranchSchema.PK.Name, GlbBranchSchema.Constants.TableName, GlbBranchSchema.GB_Code, "SIN");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singaporeBranchPK, departmentBRNPK))
			{
				AssertEquals("IsVisible should be false when not in Australia", false, ItemSet.AutomaticallyCreateSeaCargoJob.IsVisible(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
			}
		}

		#endregion

		#region XML Data Export Settings

		public void TestIncludeEdocsWhenExportingXml()
		{
			AssertEquals("IncludeConsoleDocs.DefaultValue", false, ItemSet.IncludeConsoleDocs.DefaultValue);
			AssertEquals("IncludeShipmenteDocs.DefaultValue", false, ItemSet.IncludeShipmenteDocs.DefaultValue);
			AssertEquals("IncludeWhsOrdereDocs.DefaultValue", false, ItemSet.IncludeWhsOrdereDocs.DefaultValue);
			AssertEquals("IncludeWhsReceipteDocs.DefaultValue", false, ItemSet.IncludeWhsReceipteDocs.DefaultValue);
			AssertEquals("IncludeWhsAdjustmenteDocs.DefaultValue", false, ItemSet.IncludeWhsAdjustmenteDocs.DefaultValue);

			ItemSet.IncludeConsoleDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ItemSet.IncludeShipmenteDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ItemSet.IncludeWhsOrdereDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ItemSet.IncludeWhsReceipteDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ItemSet.IncludeWhsAdjustmenteDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals("IncludeConsoleDocs.Value", true, ItemSet.IncludeConsoleDocs.Value);
			AssertEquals("IncludeShipmenteDocs.Value", true, ItemSet.IncludeShipmenteDocs.Value);
			AssertEquals("IncludeWhsOrdereDocs.Value", true, ItemSet.IncludeWhsOrdereDocs.Value);
			AssertEquals("IncludeWhsReceipteDocs.Value", true, ItemSet.IncludeWhsReceipteDocs.Value);
			AssertEquals("IncludeWhsAdjustmenteDocs.Value", true, ItemSet.IncludeWhsAdjustmenteDocs.Value);
		}

		public void TestIncludeARInvoicesWhenExportingConsolShipmentXml()
		{
			AssertEquals("IncludeConsolOrShipmentARInvoices.DefaultValue", false, ItemSet.IncludeConsolOrShipmentARInvoices.DefaultValue);
			ItemSet.IncludeConsolOrShipmentARInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("IncludeConsolOrShipmentARInvoices.Value", true, ItemSet.IncludeConsolOrShipmentARInvoices.Value);
		}

		public void TestLicencedRegistryItem()
		{
			Assert(ItemSet.BookingExportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.ConsolExportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.CFSLoadListConsolDirectory is ILicencedRegistryItem);
			Assert(ItemSet.ShipmentExportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.ShipmentAsCustomDeclarationExportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.CustomDeclarationExportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.OrderExportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.WarehouseExportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.HVLVBookingHeaderExportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.DebtorOutstandingBalancesExportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.GateBookingExportDirectory is ILicencedRegistryItem);

			Assert(ItemSet.OrganisationDataImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.ShipmentDataImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.DefaultMessagesDataImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.WarehouseDataImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.WarehouseCartageDataImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.WarehouseIFSDataImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.WarehouseOrderFlatFileImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.BookingsDataImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.ConsolsDataImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.CustomsDeclarationsDataImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.OrdersXMLDataImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.OrdersCSVDataImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.CommercialInvoicesDataImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.CommercialInvoicesDataImportDirectoryXml is ILicencedRegistryItem);
			Assert(ItemSet.PODDataImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.ProductsDataImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.ProductsXMLDataImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.ProductLastCostImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.ScheduleDataImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.LocalCartageDataImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.ContainerEventsDataImportDirectory is ILicencedRegistryItem);
			Assert(ItemSet.EventDataImportDirectory is ILicencedRegistryItem);
		}

		public void TestDataExportDirectories()
		{
			ItemSet.BookingExportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Bookings");
			ItemSet.ConsolExportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Consols");
			ItemSet.CFSLoadListConsolDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Load Lists");
			ItemSet.ALPOExportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALPO");
			ItemSet.ATLASExportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ATLAS");
			ItemSet.ShipmentExportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Shipments");
			ItemSet.OrderExportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Orders");
			ItemSet.ShipmentAsCustomDeclarationExportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Shipments As Customs Declarations");
			ItemSet.CustomDeclarationExportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Customs Declarations");
			ItemSet.WarehouseExportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Warehouse");
			ItemSet.HVLVBookingHeaderExportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "HVLV Booking Headers");
			ItemSet.DebtorOutstandingBalancesExportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "DebtorBalances");
			ItemSet.GateBookingExportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Gate Booking");

			AssertEquals("BookingExportDirectory.SetValue", "Bookings", ItemSet.BookingExportDirectory.Value);
			AssertEquals("ConsolExportDirectory.Value", "Consols", ItemSet.ConsolExportDirectory.Value);
			AssertEquals("CFSLoadListConsolDirectory.Value", "Load Lists", ItemSet.CFSLoadListConsolDirectory.Value);
			AssertEquals("ALPOExportDirectory.Value", "ALPO", ItemSet.ALPOExportDirectory.Value);
			AssertEquals("ATLASExportDirectory.Value", "ATLAS", ItemSet.ATLASExportDirectory.Value);
			AssertEquals(ItemSet.ALPOExportDirectory.Options, RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue);
			AssertEquals(ItemSet.ATLASExportDirectory.Options, RegistryOptions.IsHidden);
			AssertEquals("ShipmentExportDirectory.Value", "Shipments", ItemSet.ShipmentExportDirectory.Value);
			AssertEquals("OrderExportDirectory.Value", "Orders", ItemSet.OrderExportDirectory.Value);
			AssertEquals("ShipmentAsCustomDeclarationExportDirectory.Value", "Shipments As Customs Declarations", ItemSet.ShipmentAsCustomDeclarationExportDirectory.Value);
			AssertEquals("CustomDeclarationExportDirectory.Value", "Customs Declarations", ItemSet.CustomDeclarationExportDirectory.Value);
			AssertEquals("WarehouseExportDirectory.Value", "Warehouse", ItemSet.WarehouseExportDirectory.Value);
			AssertEquals("HVLVBookingHeaderExportDirectory.Value", "HVLV Booking Headers", ItemSet.HVLVBookingHeaderExportDirectory.Value);
			AssertEquals("DebtorOutstandingBalancesExportDirectory.Value", "DebtorBalances", ItemSet.DebtorOutstandingBalancesExportDirectory.Value);
			AssertEquals("GateBookingExportDirectory.Value", "Gate Booking", ItemSet.GateBookingExportDirectory.Value);
		}

		public void TestALPOExportDirectory()
		{
			BusinessObject glbCompany = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject glbBranch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));

			glbCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = (ZString)Enterprise.Core.Constants.CountryCodes.Germany;
			glbBranch[GlbBranchSchema.Constants.GB_GC] = glbCompany.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, glbBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals(ItemSet.ALPOExportDirectory.Options, RegistryOptions.PreserveTestValue);
			}
		}

		public void TestATLASExportDirectory()
		{
			BusinessObject glbCompany = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject glbBranch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));

			glbCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = (ZString)Enterprise.Core.Constants.CountryCodes.Germany;
			glbBranch[GlbBranchSchema.Constants.GB_GC] = glbCompany.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, glbBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals(ItemSet.ATLASExportDirectory.Options, RegistryOptions.Default);
			}
		}

		#endregion

		#region TestFinaliseOrderOnCartageImport

		public void TestFinaliseOrderOnCartageImport()
		{
			TestRegistryItem(
				ItemSet.FinaliseOrderOnCartageImport, "FinaliseOrderOnCartageImport", SystemDataRegistry.Categories.System_DataImportSettings_Warehouse,
				"Finalize Order on Port Transport Import", "When importing an Order's Port Transport details, the Order will be finalized.", RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default, false);
		}

		#endregion

		#region TestWarehouseIFSImportDirectory

		public void TestWarehouseIFSImportDirectory()
		{
			TestGenericRegistryItem(
				ItemSet.WarehouseIFSDataImportDirectory, "WarehouseIFSDataImportDirectory", SystemDataRegistry.Categories.System_DataImportSettings_Warehouse,
				"Folder to scan for IFS XML files", "The folder specified here should contain IFS XML export files that are to be automatically imported.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue);

			ItemSet.WarehouseIFSDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.TempPath);
			AssertEquals(Env.TempPath, ItemSet.WarehouseIFSDataImportDirectory.Value);
		}

		#endregion

		#region Workflow

		public void TestWorkflowExceptionGenerationHWM()
		{
			DateTime newDateTime = DateTime.Now;
			ItemSet.WorkflowExceptionGenerationHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newDateTime);
			AssertEquals("WorkflowExceptionGenerationHWM.Value", newDateTime.ToString(), ItemSet.WorkflowExceptionGenerationHWM.Value.ToString());
			AssertEquals("RegistryStorageFlags", RegistryStorageFlags.System, ItemSet.WorkflowExceptionGenerationHWM.Storage);
			AssertEquals("RegistryOptions", RegistryOptions.NotCached | RegistryOptions.IsOnlyForDevelopers, ItemSet.WorkflowExceptionGenerationHWM.Options);
		}

		public void TestWorkflowFieldChangeTriggerHWM()
		{
			DateTime newDateTime = DateTime.Now;
			ItemSet.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newDateTime);
			AssertEquals("WorkflowExceptionGenerationHWM.Value", newDateTime.ToString(), ItemSet.WorkflowFieldChangeTriggerHWM.Value.ToString());
			AssertEquals("RegistryStorageFlags", RegistryStorageFlags.System, ItemSet.WorkflowFieldChangeTriggerHWM.Storage);
			AssertEquals("RegistryOptions", RegistryOptions.NotCached | RegistryOptions.IsOnlyForDevelopers, ItemSet.WorkflowFieldChangeTriggerHWM.Options);
		}

		public void TestWorkflowFieldChangeTriggersEnabled()
		{
			ItemSet.WorkflowFieldChangeTriggersEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("WorkflowFieldChangeTriggersEnabled.Value false", false, ItemSet.WorkflowFieldChangeTriggersEnabled.Value);
			ItemSet.WorkflowFieldChangeTriggersEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("WorkflowFieldChangeTriggersEnabled.Value true", true, ItemSet.WorkflowFieldChangeTriggersEnabled.Value);

			AssertEquals("On by default", true, ItemSet.WorkflowFieldChangeTriggersEnabled.DefaultValue);
			AssertEquals("RegistryStorageFlags", RegistryStorageFlags.System, ItemSet.WorkflowFieldChangeTriggersEnabled.Storage);
			AssertEquals("RegistryOptions", RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue, ItemSet.WorkflowFieldChangeTriggersEnabled.Options);
		}

		public void TestWorkflowFieldChangeClearProcessedLogsAfterHours()
		{
			AssertEquals(24 * 7, ItemSet.WorkflowFieldChangeClearProcessedLogsAfterHours.Value);
		}

		#endregion

		#region User Account Last Report Time

		[TestDate(2023, 8, 30, 12, 34, 56)]
		public void TestUserAccountLastReportTime()
		{
			ItemSet.UserAccountLastReportTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.ToDateTime());
			AssertEquals("UserAccountLastReportTime.Value", new DateTime(2023, 8, 30, 12, 34, 56), ItemSet.UserAccountLastReportTime.Value);
			AssertEquals("RegistryStorageFlags", RegistryStorageFlags.System, ItemSet.UserAccountLastReportTime.Storage);
			AssertEquals("RegistryOptions", RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached, ItemSet.UserAccountLastReportTime.Options);

			CombineAssertions(() =>
			{
				AssertEquals("Name", "UserAccountLastReportTime", ItemSet.UserAccountLastReportTime.Name);
				AssertEquals("Category", Categories.System_License, ItemSet.UserAccountLastReportTime.Category);
				AssertEquals("Caption", "User Account Last Report Time", ItemSet.UserAccountLastReportTime.Caption);
				AssertEquals("Hint", string.Empty, ItemSet.UserAccountLastReportTime.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.UserAccountLastReportTime.Storage);
				AssertEquals("Options", RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached, ItemSet.UserAccountLastReportTime.Options);
				AssertEquals("Default Value", new DateTime(1950, 1, 1), ItemSet.UserAccountLastReportTime.DefaultValue);
			});
		}

		#endregion

		#region Identity Provider

		public void TestIdentityProviderClientID()
		{
			AssertEquals("IdentityProviderClientID", ItemSet.IdentityProviderClientID.Name);
			AssertEquals("Identity Provider Client ID", ItemSet.IdentityProviderClientID.Caption);
			AssertEquals("It's a permanent id coming from Azure application which represents Identity Provider.", ItemSet.IdentityProviderClientID.Hint);
			AssertEquals(Categories.System_IdentityProvider, ItemSet.IdentityProviderClientID.Category);
			AssertEquals(RegistryStorageFlags.System, ItemSet.IdentityProviderClientID.Storage);
			AssertEquals("7a67d4a5-7751-4a64-87b6-d06802844436", ItemSet.IdentityProviderClientID.DefaultValue);
		}

		public void TestIdentityProviderTimeoutInSeconds()
		{
			TestRegistryItem(
				ItemSet.IdentityProviderTimeoutInSeconds,
				"IdentityProviderTimeoutInSeconds",
				SystemDataRegistry.Categories.System_IdentityProvider_CargoWiseUserManagement,
				"Identity Provider Configuration Request Timeout",
				"Timeout is seconds when Identity Provider requests are executed.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				5,
				1,
				300);
		}

		#region CargoWise User Synchronization

		public void TestIdpUserSynchronisationEndpoint()
		{
			AssertEquals("IdpUserSynchronisationEndpoint", ItemSet.IdpUserSynchronisationEndpoint.Name);
			AssertEquals("System/Identity Provider/CargoWise User Synchronization", ItemSet.IdpUserSynchronisationEndpoint.Category);
			AssertEquals("Identity Provider Endpoint for creating and updating users", ItemSet.IdpUserSynchronisationEndpoint.Caption);
			AssertEquals("A support only registry for specifying the link to the user create/update Identity Provider endpoint.", ItemSet.IdpUserSynchronisationEndpoint.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.IdpUserSynchronisationEndpoint.Storage);
		}

		public void TestIdpUserSynchronisationEndpointRegistryOptions()
		{
			var mockReg = new Mock<IProductRegistration>();
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalDeveloperSystem()).Returns(true);
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalUATSystem()).Returns(true);

			RegistryItemDictionary.Instance.PurgeAll();
			using (ObjectFactory.Substitute(mockReg.Object))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.IdpUserSynchronisationEndpoint.Options);
			}

			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalDeveloperSystem()).Returns(false);
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalUATSystem()).Returns(true);

			RegistryItemDictionary.Instance.PurgeAll();
			using (ObjectFactory.Substitute(mockReg.Object))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.IdpUserSynchronisationEndpoint.Options);
			}

			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalDeveloperSystem()).Returns(true);
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalUATSystem()).Returns(false);

			RegistryItemDictionary.Instance.PurgeAll();
			using (ObjectFactory.Substitute(mockReg.Object))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.IdpUserSynchronisationEndpoint.Options);
			}

			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalDeveloperSystem()).Returns(false);
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalUATSystem()).Returns(false);

			RegistryItemDictionary.Instance.PurgeAll();
			using (ObjectFactory.Substitute(mockReg.Object))
			{
				AssertEquals(RegistryOptions.IsHidden, ItemSet.IdpUserSynchronisationEndpoint.Options);
			}
		}

		public void TestIdpUserImportHighWaterMark()
		{
			AssertEquals("IdpUserImportHighWaterMark", ItemSet.IdpUserImportHighWaterMark.Name);
			AssertEquals("System/Identity Provider/CargoWise User Synchronization", ItemSet.IdpUserImportHighWaterMark.Category);
			AssertEquals("Identity Provider User Import High Water Mark", ItemSet.IdpUserImportHighWaterMark.Caption);
			AssertEquals("Water mark for the IDP service task.", ItemSet.IdpUserImportHighWaterMark.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.IdpUserImportHighWaterMark.Storage);
			AssertEquals(RegistryOptions.NotCached | RegistryOptions.IsHidden, ItemSet.IdpUserImportHighWaterMark.Options);
		}

		public void TestIdpUserSynchronisationEnabled()
		{
			AssertEquals("IdpUserSynchronisationEnabled", ItemSet.IdpUserSynchronisationEnabled.Name);
			AssertEquals("System/Identity Provider/CargoWise User Synchronization", ItemSet.IdpUserSynchronisationEnabled.Category);
			AssertEquals("Enable Synchronization of Staff records into the Identity Provider", ItemSet.IdpUserSynchronisationEnabled.Caption);
			AssertEquals("If this registry is enabled, Staff records will automatically sync with the Identity Provider.", ItemSet.IdpUserSynchronisationEnabled.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.IdpUserSynchronisationEnabled.Storage);
		}

		public void TestIdpUserSynchronisationEnabledRegistryOptions()
		{
			var mockReg = new Mock<IProductRegistration>();
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalDeveloperSystem()).Returns(true);
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalUATSystem()).Returns(true);

			RegistryItemDictionary.Instance.PurgeAll();
			using (ObjectFactory.Substitute(mockReg.Object))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.IdpUserSynchronisationEnabled.Options);
			}

			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalDeveloperSystem()).Returns(false);
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalUATSystem()).Returns(true);

			RegistryItemDictionary.Instance.PurgeAll();
			using (ObjectFactory.Substitute(mockReg.Object))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.IdpUserSynchronisationEnabled.Options);
			}

			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalDeveloperSystem()).Returns(true);
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalUATSystem()).Returns(false);

			RegistryItemDictionary.Instance.PurgeAll();
			using (ObjectFactory.Substitute(mockReg.Object))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.IdpUserSynchronisationEnabled.Options);
			}

			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalDeveloperSystem()).Returns(false);
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalUATSystem()).Returns(false);

			RegistryItemDictionary.Instance.PurgeAll();
			using (ObjectFactory.Substitute(mockReg.Object))
			{
				AssertEquals(RegistryOptions.IsHidden, ItemSet.IdpUserSynchronisationEnabled.Options);
			}
		}

		#endregion

		#endregion

		#region OpenID Connect

		public void TestDomainHint()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			using (RawDataRegistry.Instance.SystemEnterpriseCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "UPS"))
			{
				AssertEquals("RegistryStorageFlags", RegistryStorageFlags.System, ItemSet.DomainHint.Storage);
				AssertEquals("RegistryOptions", RegistryOptions.IsOnlyForSupport, ItemSet.DomainHint.Options);
				AssertEquals("Azure", ItemSet.DomainHint.DefaultValue);

				var dataType = ItemSet.DomainHint.DataType as StringRegistryDataType;
				AssertNotNull(dataType);
				AssertEquals(true, dataType.ReadOnly);
				AssertEquals("WC_UPS", dataType.OverrideValue);
			}
		}

		public void TestAppDomainMapping()
		{
			string[] domain = { "@.test.com" };
			AssertRegistryValidationExceptionWhenDomainIsInvalid(domain);
			string[] domain2 = { "@-test.com" };
			AssertRegistryValidationExceptionWhenDomainIsInvalid(domain2);
			string[] domain3 = { "@example..com" };
			AssertRegistryValidationExceptionWhenDomainIsInvalid(domain3);
			string[] domain4 = { "@example" };
			AssertRegistryValidationExceptionWhenDomainIsInvalid(domain4);
			string[] domain5 = { "Test01@test.com" };
			AssertRegistryValidationExceptionWhenDomainIsInvalid(domain5);
			string[] domain6 = { "@test.com" };
			AssertNoExceptionThrown(() => Instance.AppDomainMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, domain6));
		}

		void AssertRegistryValidationExceptionWhenDomainIsInvalid(string[] domain)
		{
			AssertExceptionThrown<RegistryValidationException>(() => Instance.AppDomainMapping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, domain));
		}

		public void TestHintOfDomainHintRegistry_HostedSystem()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			AssertEquals(@"This is a setting for WiseCloud customers who require special configuration in the Azure AD B2C custom policy file.

Overriding this value will need to create a copy of a B2C configuration for the specific customer, and the value in this registry setting will be included in the <Domain> and <DisplayName> in the custom policy file.

This text box is always read-only by design. Overriding the default option will automatically use the prefix 'WC_' together with the customer's three letter Enterprise Code.

This combination of 'WC_' + three letter Enterprise Code is used to locate the custom configuration. If no customization is required, please do not override this setting.", ItemSet.DomainHint.Hint);
		}

		public void TestHintOfDomainHintRegistry_SelfHostedSystem()
		{
			AssertEquals("The Domain Hint for self-hosted customers is used if you are federating identities to WiseTech Global's Azure AD B2C server for CargoWise logins. If you are not federating to the WiseTech Azure AD B2C please leave this as disabled.", ItemSet.DomainHint.Hint);
		}

		public void TestHintOfDomainHintRegistry_EdiProd()
		{
			using (Enterprise.ZArchitecture.Modules.ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				AssertEquals(@"This is a setting for WiseCloud customers who require special configuration in the Azure AD B2C custom policy file.

Overriding this value will need to create a copy of a B2C configuration for the specific customer, and the value in this registry setting will be included in the <Domain> and <DisplayName> in the custom policy file.

This text box is always read-only by design. Overriding the default option will automatically use the prefix 'WC_' together with the customer's three letter Enterprise Code.

This combination of 'WC_' + three letter Enterprise Code is used to locate the custom configuration. If no customization is required, please do not override this setting.", ItemSet.DomainHint.Hint);
			}
		}

		public void TestDomainHintIsVisbleToSelfHostedCustomer()
		{
			AssertEquals("RegistryOptions", RegistryOptions.Default, ItemSet.DomainHint.Options);
		}

		public void TestOIDCConfig_NoError()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Generic,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping()
			{
				ClaimName = "SomeClaim",
				Identifier = "GlbStaff.GS_LoginName"
			});

			oidcConfig.IsVerified = true;

			AssertEquals(
				"No errors",
				string.Empty,
				ItemSet.OIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestOIDCConfig_DefaultNoError()
		{
			AssertEquals(
				"No errors",
				string.Empty,
				ItemSet.OIDCConfig.GetValidationErrorMessage(
					OIDCConfig.DefaultValue,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestOIDCConfig_InvalidType()
		{
			AssertContains(
				"Expect invalid server type",
				"OpenID Connect Server: Invalid Selection.",
				ItemSet.OIDCConfig.GetValidationErrorMessage(
					new OIDCConfig()
					{
						IsOIDCEnabled = true,
						OIDCServerTypeCode = "AAA",
						AuthorityURL = "https://identityprovider.com",
						ClientIdentifier = "SomeId",
					},
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestOIDCConfig_MandantoryClientId()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Generic,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = string.Empty,
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping()
			{
				ClaimName = "SomeClaim",
				Identifier = "GlbStaff.GS_LoginName"
			});

			AssertContains(
				"Mandantory Client Identifier",
				"Client Identifier: Please enter a Client Identifier.",
				ItemSet.OIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestOIDCConfig_InvalidAuthorityUrl()
		{
			AssertContains(
				"Expect invalid authority url",
				"OpenID Connect Authority URL: Malformed Authority URL.",
				ItemSet.OIDCConfig.GetValidationErrorMessage(
					new OIDCConfig()
					{
						IsOIDCEnabled = true,
						OIDCServerType = OIDCServerTypes.Okta,
						AuthorityURL = "asdf",
						ClientIdentifier = "SomeId",
					},
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestOIDCConfig_AuthorityShouldBeHttps()
		{
			AssertContains(
				"Expect authority url must be https",
				"OpenID Connect Authority URL: Authority URL must be https.",
				ItemSet.OIDCConfig.GetValidationErrorMessage(
					new OIDCConfig()
					{
						IsOIDCEnabled = true,
						OIDCServerType = OIDCServerTypes.Okta,
						AuthorityURL = "http://identityprovider.com",
						ClientIdentifier = "SomeId",
					},
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestOIDCConfig_ClaimIsNeeded()
		{
			AssertContains(
				"Expect claim mapping must be specified error",
				"must be specified",
				ItemSet.OIDCConfig.GetValidationErrorMessage(
					new OIDCConfig()
					{
						IsOIDCEnabled = true,
						OIDCServerType = OIDCServerTypes.Okta,
						AuthorityURL = "https://identityprovider.com",
						ClientIdentifier = "SomeId",
					},
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestOIDCConfig_ClaimNameShouldNotEmpty()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Okta,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = string.Empty,
				Identifier = "GlbStaff.GS_EmailAddress",
			});

			AssertContains(
				"Expect Claim Name must be entered error",
				"Please enter a value",
				ItemSet.OIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestOIDCConfig_InvalidClaimMapping()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Okta,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = "someClaim",
				Identifier = "invalid",
			});

			AssertContains(
				"Expect Idenfitifer invalid selection error",
				"Invalid Selection",
				ItemSet.OIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestOIDCConfig_DuplicatedClaimName()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Okta,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = "someClaim",
				Identifier = "GlbStaff.GS_LoginName",
			});

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = "someClaim",
				Identifier = "GlbStaff.GS_EmailAddress",
			});

			AssertContains(
				"Expect Claim duplicate error",
				"unique",
				ItemSet.OIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestOIDCConfig_DuplicatedClaimMapping()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Okta,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = "someClaim",
				Identifier = "GlbStaff.GS_LoginName",
			});

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = "otherclaim",
				Identifier = "GlbStaff.GS_LoginName",
			});

			AssertContains(
				"Expect Identifier duplicate error",
				"unique",
				ItemSet.OIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestOIDCConfig_ValidScope()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Okta,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = "someClaim",
				Identifier = "GlbStaff.GS_LoginName",
			});

			oidcConfig.Scopes.Add(new OIDCScope
			{
				ScopeName = "SomeScope",
			});

			oidcConfig.IsVerified = true;

			AssertEquals(
				"Expect no error",
				string.Empty,
				ItemSet.OIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestOIDCConfig_InvalidScope()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Okta,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = "someClaim",
				Identifier = "GlbStaff.GS_LoginName",
			});

			oidcConfig.Scopes.Add(new OIDCScope
			{
				ScopeName = "Some Scope",
			});

			AssertContains(
				"Expect whitespace error",
				"whitespace",
				ItemSet.OIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestOIDCConfig_ReservedScope()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Okta,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = "someClaim",
				Identifier = "GlbStaff.GS_LoginName",
			});

			oidcConfig.Scopes.Add(new OIDCScope
			{
				ScopeName = "openid",
			});

			AssertContains(
				"Expect reserved error",
				"reserved",
				ItemSet.OIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestOIDCConfig_DuplicatedScope()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Okta,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = "someClaim",
				Identifier = "GlbStaff.GS_LoginName",
			});

			oidcConfig.Scopes.Add(new OIDCScope
			{
				ScopeName = "SomeClaim",
			});

			oidcConfig.Scopes.Add(new OIDCScope
			{
				ScopeName = "SomeClaim",
			});

			AssertContains(
				"Expect duplicate error",
				"unique",
				ItemSet.OIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestOIDCConfig()
		{
			AssertEquals("Off by default", false, ItemSet.OIDCConfig.DefaultValue.IsOIDCEnabled);
			AssertEquals("RegistryStorageFlags", RegistryStorageFlags.System, ItemSet.OIDCConfig.Storage);
		}

		public void TestOIDCConfigRegistryOptionsDefault_ForSelfHostedSystem()
		{
			AssertEquals("It's default for self hosted system.", RegistryOptions.Default, ItemSet.OIDCConfig.Options);
		}

		public void TestOIDCConfigRegistryOptionsDefault_ForSelfHostedSystemNonSupportUser()
		{
			using (CurrentUserChanger.SwitchToNewUserTemporarily(User.WebUserName))
			{
				AssertEquals("It's default for self hosted system and non support user.", RegistryOptions.Default, ItemSet.OIDCConfig.Options);
			}
		}

		public void TestOIDCConfigRegistryOptionsDefault_ForSupportUser()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			using (CurrentUserChanger.SwitchToNewUserTemporarily(User.SupportUserName))
			{
				AssertEquals("It's default for support user.", RegistryOptions.Default, ItemSet.OIDCConfig.Options);
			}
		}

		public void TestOIDCConfigRegistryOptionsIsReadonly_ForHostedSystemAndNonSupportUser()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			using (CurrentUserChanger.SwitchToNewUserTemporarily(User.WebUserName))
			{
				AssertEquals("It's readonly for hosted system and non support user.", RegistryOptions.IsReadOnly, ItemSet.OIDCConfig.Options);
			}
		}

		public void TestWinzorOIDCConfig_NoError()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Generic,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping()
			{
				ClaimName = "SomeClaim",
				Identifier = "GlbStaff.GS_LoginName"
			});

			oidcConfig.IsVerified = true;

			AssertEquals(
				"No errors",
				string.Empty,
				ItemSet.WinzorOIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestWinzorOIDCConfig_DefaultNoError()
		{
			AssertEquals(
				"No errors",
				string.Empty,
				ItemSet.WinzorOIDCConfig.GetValidationErrorMessage(
					OIDCConfig.DefaultValue,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestWinzorOIDCConfig_InvalidType()
		{
			AssertContains(
				"Expect invalid server type",
				"OpenID Connect Server: Invalid Selection.",
				ItemSet.WinzorOIDCConfig.GetValidationErrorMessage(
					new OIDCConfig()
					{
						IsOIDCEnabled = true,
						OIDCServerTypeCode = "AAA",
						AuthorityURL = "https://identityprovider.com",
						ClientIdentifier = "SomeId",
					},
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestWinzorOIDCConfig_MandantoryClientId()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Generic,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = string.Empty,
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping()
			{
				ClaimName = "SomeClaim",
				Identifier = "GlbStaff.GS_LoginName"
			});

			AssertContains(
				"Mandantory Client Identifier",
				"Client Identifier: Please enter a Client Identifier.",
				ItemSet.WinzorOIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestWinzorOIDCConfig_InvalidAuthorityUrl()
		{
			AssertContains(
				"Expect invalid authority url",
				"OpenID Connect Authority URL: Malformed Authority URL.",
				ItemSet.WinzorOIDCConfig.GetValidationErrorMessage(
					new OIDCConfig()
					{
						IsOIDCEnabled = true,
						OIDCServerType = OIDCServerTypes.Okta,
						AuthorityURL = "asdf",
						ClientIdentifier = "SomeId",
					},
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestWinzorOIDCConfig_AuthorityShouldBeHttps()
		{
			AssertContains(
				"Expect authority url must be https",
				"OpenID Connect Authority URL: Authority URL must be https.",
				ItemSet.WinzorOIDCConfig.GetValidationErrorMessage(
					new OIDCConfig()
					{
						IsOIDCEnabled = true,
						OIDCServerType = OIDCServerTypes.Okta,
						AuthorityURL = "http://identityprovider.com",
						ClientIdentifier = "SomeId",
					},
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestWinzorOIDCConfig_ClaimIsNeeded()
		{
			AssertContains(
				"Expect claim mapping must be specified error",
				"must be specified",
				ItemSet.WinzorOIDCConfig.GetValidationErrorMessage(
					new OIDCConfig()
					{
						IsOIDCEnabled = true,
						OIDCServerType = OIDCServerTypes.Okta,
						AuthorityURL = "https://identityprovider.com",
						ClientIdentifier = "SomeId",
					},
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestWinzorOIDCConfig_ClaimNameShouldNotEmpty()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Okta,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = string.Empty,
				Identifier = "GlbStaff.GS_EmailAddress",
			});

			AssertContains(
				"Expect Claim Name must be entered error",
				"Please enter a value",
				ItemSet.WinzorOIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestWinzorOIDCConfig_InvalidClaimMapping()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Okta,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = "someClaim",
				Identifier = "invalid",
			});

			AssertContains(
				"Expect Idenfitifer invalid selection error",
				"Invalid Selection",
				ItemSet.WinzorOIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestWinzorOIDCConfig_DuplicatedClaimName()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Okta,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = "someClaim",
				Identifier = "GlbStaff.GS_LoginName",
			});

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = "someClaim",
				Identifier = "GlbStaff.GS_EmailAddress",
			});

			AssertContains(
				"Expect Claim duplicate error",
				"unique",
				ItemSet.WinzorOIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestWinzorOIDCConfig_DuplicatedClaimMapping()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Okta,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = "someClaim",
				Identifier = "GlbStaff.GS_LoginName",
			});

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = "otherclaim",
				Identifier = "GlbStaff.GS_LoginName",
			});

			AssertContains(
				"Expect Identifier duplicate error",
				"unique",
				ItemSet.WinzorOIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestWinzorOIDCConfig_ValidScope()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Okta,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = "someClaim",
				Identifier = "GlbStaff.GS_LoginName",
			});

			oidcConfig.Scopes.Add(new OIDCScope
			{
				ScopeName = "SomeScope",
			});

			oidcConfig.IsVerified = true;

			AssertEquals(
				"Expect no error",
				string.Empty,
				ItemSet.WinzorOIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestWinzorOIDCConfig_InvalidScope()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Okta,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = "someClaim",
				Identifier = "GlbStaff.GS_LoginName",
			});

			oidcConfig.Scopes.Add(new OIDCScope
			{
				ScopeName = "Some Scope",
			});

			AssertContains(
				"Expect whitespace error",
				"whitespace",
				ItemSet.WinzorOIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestWinzorOIDCConfig_ReservedScope()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Okta,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = "someClaim",
				Identifier = "GlbStaff.GS_LoginName",
			});

			oidcConfig.Scopes.Add(new OIDCScope
			{
				ScopeName = "openid",
			});

			AssertContains(
				"Expect reserved error",
				"reserved",
				ItemSet.WinzorOIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestWinzorOIDCConfig_DuplicatedScope()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Okta,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = "someClaim",
				Identifier = "GlbStaff.GS_LoginName",
			});

			oidcConfig.Scopes.Add(new OIDCScope
			{
				ScopeName = "SomeClaim",
			});

			oidcConfig.Scopes.Add(new OIDCScope
			{
				ScopeName = "SomeClaim",
			});

			AssertContains(
				"Expect duplicate error",
				"unique",
				ItemSet.WinzorOIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));
		}

		public void TestWinzorOIDCConfig()
		{
			AssertEquals("Off by default", false, ItemSet.WinzorOIDCConfig.DefaultValue.IsOIDCEnabled);
			AssertEquals("RegistryStorageFlags", RegistryStorageFlags.System, ItemSet.WinzorOIDCConfig.Storage);
		}

		public void TestWinzorOIDCConfigRegistryOptionsDefault_ForSelfHostedSystem()
		{
			AssertEquals("It's default for self hosted system.", RegistryOptions.Default, ItemSet.WinzorOIDCConfig.Options);
		}

		public void TestWinzorOIDCConfigRegistryOptionsDefault_ForSelfHostedSystemNonSupportUser()
		{
			using (CurrentUserChanger.SwitchToNewUserTemporarily(User.WebUserName))
			{
				AssertEquals("It's default for self hosted system and non support user.", RegistryOptions.Default, ItemSet.WinzorOIDCConfig.Options);
			}
		}

		public void TestWinzorOIDCConfigRegistryOptionsDefault_ForSupportUser()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			using (CurrentUserChanger.SwitchToNewUserTemporarily(User.SupportUserName))
			{
				AssertEquals("It's default for support user.", RegistryOptions.Default, ItemSet.WinzorOIDCConfig.Options);
			}
		}

		public void TestWinzorOIDCConfigRegistryOptionsIsReadonly_ForHostedSystemAndNonSupportUser()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			using (CurrentUserChanger.SwitchToNewUserTemporarily(User.WebUserName))
			{
				AssertEquals("It's readonly for hosted system and non support user.", RegistryOptions.IsReadOnly, ItemSet.WinzorOIDCConfig.Options);
			}
		}

		public void TestOIDCClientIDForWebApplications()
		{
			AssertEquals("OIDCClientIDForWebApplications", ItemSet.OIDCClientIDForWebApplications.Name);
			AssertEquals("OIDC Client ID For Web Applications", ItemSet.OIDCClientIDForWebApplications.Caption);
			AssertEquals("It's the client ID for web applications in OpenID Connect login.", ItemSet.OIDCClientIDForWebApplications.Hint);
			AssertEquals("System/Staff/OpenID Connect Authentication", ItemSet.OIDCClientIDForWebApplications.Category);
			AssertEquals(RegistryStorageFlags.System, ItemSet.OIDCClientIDForWebApplications.Storage);
			AssertEquals(RegistryOptions.IsReadOnly, ItemSet.OIDCClientIDForWebApplications.Options);
			AssertEquals(string.Empty, ItemSet.OIDCClientIDForWebApplications.DefaultValue);
		}

		public void TestWiseCloudAccessorTokenBasedAccess()
		{
			AssertEquals("Registry should be hidden", RegistryOptions.IsHidden, ItemSet.WiseCloudAccessorTokenBasedAccess.Options);

			EnvProxy.SetHostedLocationForTest("SYD");
			Assert(EnvProxy.IsHostedWithCargowise);

			RegistryItemDictionary.Instance.PurgeAll();

			AssertEquals("Off by default", false, ItemSet.WiseCloudAccessorTokenBasedAccess.DefaultValue);
			AssertEquals("RegistryStorageFlags", RegistryStorageFlags.System, ItemSet.WiseCloudAccessorTokenBasedAccess.Storage);
			AssertEquals("Registry should be only for support", RegistryOptions.IsOnlyForSupport, ItemSet.WiseCloudAccessorTokenBasedAccess.Options);
		}

		public void TestOIDCConfig_ServerTypeAzure()
		{
			var oidcConfig = new OIDCConfig()
			{
				OIDCServerType = OIDCServerTypes.Azure,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping()
			{
				ClaimName = "SomeClaim",
				Identifier = "GlbStaff.GS_LoginName"
			});

			AssertEquals(
				"No errors",
				string.Empty,
				ItemSet.OIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));

			AssertEquals("AZU", oidcConfig.OIDCServerTypeCode);
		}

		public void TestOIDCConfig_ServerTypeWiseTechIdP()
		{
			var oidcConfig = new OIDCConfig()
			{
				OIDCServerType = OIDCServerTypes.WiseTechIdP,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping()
			{
				ClaimName = "SomeClaim",
				Identifier = "GlbStaff.GS_EmailAddress"
			});

			AssertEquals(
				"No errors",
				string.Empty,
				ItemSet.OIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));

			AssertEquals("WTG", oidcConfig.OIDCServerTypeCode);
		}

		public void TestWinzorOIDCConfig_ServerTypeAzure()
		{
			var oidcConfig = new OIDCConfig()
			{
				OIDCServerType = OIDCServerTypes.Azure,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping()
			{
				ClaimName = "SomeClaim",
				Identifier = "GlbStaff.GS_LoginName"
			});

			AssertEquals(
				"No errors",
				string.Empty,
				ItemSet.WinzorOIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));

			AssertEquals("AZU", oidcConfig.OIDCServerTypeCode);
		}

		public void TestWinzorOIDCConfig_ServerTypeWiseTechIdP()
		{
			var oidcConfig = new OIDCConfig()
			{
				OIDCServerType = OIDCServerTypes.WiseTechIdP,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping()
			{
				ClaimName = "SomeClaim",
				Identifier = "GlbStaff.GS_EmailAddress"
			});

			AssertEquals(
				"No errors",
				string.Empty,
				ItemSet.WinzorOIDCConfig.GetValidationErrorMessage(
					oidcConfig,
					Guid.Empty,
					Guid.Empty,
					Guid.Empty));

			AssertEquals("WTG", oidcConfig.OIDCServerTypeCode);
		}

		public void TestOIDCConfigXmlShouldDeserializeToGlowOIDCConfigCorrectly()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Generic,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};
			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping()
			{
				ClaimName = "SomeClaim1",
				Identifier = "GlbStaff.GS_LoginName"
			});
			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping()
			{
				ClaimName = "SomeClaim2",
				Identifier = "GlbStaff.GS_EmailAddress"
			});
			oidcConfig.Scopes.Add(new OIDCScope()
			{
				ScopeName = "SomeScope1"
			});
			oidcConfig.Scopes.Add(new OIDCScope()
			{
				ScopeName = "SomeScope2"
			});
			var serializer = new OIDCConfigRegistryDataType(OIDCConfig.DefaultValue);

			var oidcConfigXml = Encoding.ASCII.GetString(serializer.Serialise(oidcConfig));
			var defaultSerializer = new XmlSerializer(typeof(OidcConfiguration));
			var glowOidcConfig = (OidcConfiguration)defaultSerializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(oidcConfigXml)));

			AssertEquals(oidcConfig.IsOIDCEnabled, glowOidcConfig.IsOidcEnabled);
			AssertEquals(oidcConfig.OIDCServerTypeCode, glowOidcConfig.OidcServerTypeCode);
			AssertEquals(oidcConfig.AuthorityURL, glowOidcConfig.AuthorityURL);
			AssertEquals(oidcConfig.ClientIdentifier, glowOidcConfig.ClientIdentifier);
			AssertEquals(oidcConfig.ClaimsMappings.Count, glowOidcConfig.ClaimsMappings.Count);
			for (var i = 0; i < oidcConfig.ClaimsMappings.Count; i++)
			{
				AssertEquals(oidcConfig.ClaimsMappings[i].ClaimName, glowOidcConfig.ClaimsMappings[i].ClaimName);
				AssertEquals(oidcConfig.ClaimsMappings[i].Identifier, glowOidcConfig.ClaimsMappings[i].Identifier);
			}
			AssertEquals(oidcConfig.Scopes.Count, glowOidcConfig.Scopes.Count);
			for (var i = 0; i < oidcConfig.Scopes.Count; i++)
			{
				AssertEquals(oidcConfig.Scopes[i].ScopeName, glowOidcConfig.Scopes[i].ScopeName);
			}
		}

		public void TestOIDCServerTypesListInSyncWithSharedDefinition()
		{
			var cw1Codes = GetCodes(typeof(OIDCServerTypesList.Codes));
			var sharedCodes = GetCodes(typeof(OidcServerTypesCodes));
			AssertArrayEqualsByElements(cw1Codes, sharedCodes);

			static (string name, string code)[] GetCodes(Type type)
			{
				return type.GetFields(BindingFlags.Public | BindingFlags.Static)
					.Where(f => f.IsLiteral)
					.Select(f => (f.Name, (string)f.GetValue(null)))
					.ToArray();
			}
		}

		#endregion

		#region Customer Service

		public void TestCustomerStatuses()
		{
			TestRegistryItem(ItemSet.CustomerStatuses, "CustomerStatuses", "System/Customer Service (Legacy)", "Customer Statuses", "A list of customer statuses which can be set in Customer Service Request", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, 1, new CodeDescriptionPair("UDF", "You can configure these statuses in the registry at System > Registry > System > Customer Service > Customer Statuses"));
		}

		public void TestCustomerServiceRequestNotificationRecipientsSetting()
		{
			TestGenericRegistryItem(ItemSet.CustomerServiceRequestNotificationRecipientsSetting,
				"CustomerServiceRequestNotificationRecipientsSetting",
				"System/Customer Service (Legacy)",
				"Customer Service Request Notification Recipients Setting",
				"This setting allows you to set recipient(s) to receive notification of a customer service request.\r\n\r\nNote: If you are setting \"Third Party Notify Only\" as a default, the field \"Third Party Notify\" on the service request will be Mandatory. With any other set default, the \"Third Party Notify\" specified on service request will always receive a notification.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
				"ALL");
		}

		public void TestELearningUrl()
		{
			TestGenericRegistryItem(ItemSet.ELearningUrl,
				"ELearningUrl",
				"System/Customer Service (Legacy)",
				"eLearning URL",
				"URL of the eLearning site.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
				"http://www.cargowise.com/eLearning.aspx");
		}

		public void TestHowToDocumentERequestIncidentUrl()
		{
			TestGenericRegistryItem(ItemSet.HowToDocumentERequestIncidentUrl,
				"HowToDocumentERequestIncidentUrl",
				"System/Customer Service (Legacy)",
				"How To Document eRequest Incident URL",
				"URL of how to document eRequest incident site.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
				"http://www.cargowise.com/Documents/UserGuides/HowTo/How-To%20document%20your%20eRequest%20Incident.pdf");
		}

		#endregion

		#region ReportConcurrencyErrors

		public void TestReportConcurrencyErrors()
		{
			TestRegistryItem(ItemSet.ReportConcurrencyErrors, "ReportConcurrencyErrors", "System/Database/User Options", "Report Concurrency Errors", "Report data concurrency errors to CargoWise.", RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers, true);
		}

		#endregion

		#region Database setting for reporting

		public void TestMaxOLAPConnections()
		{
			TestRegistryItem(ItemSet.ReportMaxConnections,
				"MaximumConcurrentReports",
				"System/Reports",
				"Maximum concurrent running reports",
				@"This is the maximum number of reports allowed to run concurrently. '0' denotes unlimited.
Note: This limit affects reports both run locally and generated via the SRR service task.
You can control whether reports are generated locally or via the service task with Registry Setting: Documents > Background Report Delivery.
Previewing of a report will contribute towards this limit.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				0);
		}

		public void TestReportingDBSettingsForCorrectSet()
		{
			TestGenericRegistryItem(ItemSet.ReportingDbServerNames,
				"ReportingDBServerName",
				"System/Reports",
				"Reporting databases full server names",
				"The full server names of the reporting databases, including the instance names if applicable.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue | RegistryOptions.IsOnlyEditableBySupportIfHosted);
		}

		public void TestMaxActiveScheduledReportsWarningThreshold()
		{
			TestGenericRegistryItem(ItemSet.MaxActiveScheduledReportsWarningThreshold,
				"MaxActiveScheduledReportsWarningThreshold",
				"System/Reports",
				"Maximum Active Scheduled Reports Warning Threshold",
				@"This setting manages how many Scheduled Reports have been set with a similar Next Run Time.
It is possible to experience processing delays when there are too many reports with similar run times.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				0);
		}

		public void TestDbServerThreshold()
		{
			AssertEquals(TimeSpan.FromMinutes(10), ItemSet.ReportingDbServerThreshold);
		}

		public void TestSRRErrorNotificationOptions()
		{
			TestGenericRegistryItem(ItemSet.SRRErrorNotificationOptions,
				"SRRErrorNotificationOptions",
				"System/Reports",
				"Error Notification Options",
				"This setting determines the recipients for error notifications for reports.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				Enterprise.Core.Constants.ErrorNotificationOptions.Code.DEF);
		}

		public void TestSRRErrorNotificationStaffRoles()
		{
			TestGenericRegistryItem(ItemSet.SRRErrorNotificationStaffRoles,
				"SRRErrorNotificationStaffRoles",
				"System/Reports",
				"Error Notification Staff Roles",
				"The staff roles who will receive error notification emails.",
				RegistryStorageFlags.System,
				RegistryOptions.Default);

			AssertEquals("Send Notification To", ((CodeDescriptionBoolRegistryEditorInfo)ItemSet.SRRErrorNotificationStaffRoles.EditorInfo).BoolColumnCaption);
			Assert("Bool column should be visible", ((CodeDescriptionBoolRegistryEditorInfo)ItemSet.SRRErrorNotificationStaffRoles.EditorInfo).IsBoolColumnVisible);
			Assert("Only Bool Column should be editable", ((CodeDescriptionBoolRegistryEditorInfo)ItemSet.SRRErrorNotificationStaffRoles.EditorInfo).IsOnlyBoolColumnEditable);
		}

		public void TestSRRErrorNotificationGroups()
		{
			TestGenericRegistryItem(ItemSet.SRRErrorNotificationGroups,
				"SRRErrorNotificationGroups",
				"System/Reports",
				"Error Notification Groups",
				"The Group that will be notified for error notifications related to reports.",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory);
		}

		#endregion

		public void TestSRRMaximumRetryCount()
		{
			TestRegistryItem(ItemSet.SRRMaximumRetryCount,
				"SRRMaximumRetryCount",
				"System/Reports",
				"Scheduled Report Failure Threshold",
				@"Maximum number of times that a report can fail before it is disabled. Set to 0 to disable.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				3);
		}

		#region SuspendAuditTriggers

		public void TestSuspendAuditTriggers()
		{
			TestGenericRegistryItem(ItemSet.SuspendAuditTriggers,
				DbRegistry.SuspendAuditTriggersName,
				"System/Database",
				"Suspend Audit Triggers",
				"Allows to suspend AuditTriggers (related to audit columns) for INSERT/UPDATE sql statements.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				DbRegistry.SuspendAuditTriggersDefaultValue);

			ItemSet.SuspendAuditTriggers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ItemSet.SuspendAuditTriggers.Value);

			ItemSet.SuspendAuditTriggers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, ItemSet.SuspendAuditTriggers.Value);
		}

		#endregion

		#region MissingFetchHint

		public void TestMissingFetchHintDetection()
		{
			TestGenericRegistryItem(
				ItemSet.MissingFetchHintDetection,
				"Missing Fetch Hint Detection",
				"System/Database",
				"Missing Fetch Hint Detection",
				"Throws an error when there are more identical SQL queries than threshold called from the same call stack within 5 seconds. Refreshes on application restart.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				false);

			ItemSet.MissingFetchHintDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ItemSet.MissingFetchHintDetection.Value);

			ItemSet.MissingFetchHintDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, ItemSet.MissingFetchHintDetection.Value);
		}

		public void TestMissingFetchHintThreshold()
		{
			TestGenericRegistryItem(
				ItemSet.MissingFetchHintThreshold,
				"Missing Fetch Hint Threshold",
				"System/Database",
				"Missing Fetch Hint Threshold",
				"Minimum number of identical SQL queries executed within 5 seconds to trigger an error. Refreshes on application restart.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				25);

			ItemSet.MissingFetchHintThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20);
			AssertEquals(20, ItemSet.MissingFetchHintThreshold.Value);

			ItemSet.MissingFetchHintThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 78);
			AssertEquals(78, ItemSet.MissingFetchHintThreshold.Value);
		}
		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Testing")]
		public void TestUseWindowsCertificateStore()
		{
			TestGenericRegistryItem(ItemSet.UseWindowsCertificateStore, "UseWindowsCertificateStore_0910", "System/Certificates",
				"Use Windows Certificate Store",
				$"Specifies whether or not {Core.Constants.ProductName} should use the Windows Certificate Store or the {Core.Constants.ProductName} Certificate Store to decrypt messages. Using the Windows Certificate Store allows multiple certificates to be used.",
				RegistryStorageFlags.System, true);
			ItemSet.UseWindowsCertificateStore.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ItemSet.UseWindowsCertificateStore.Value);
		}

		public void TestStoreCertificatesUnderCurrentUser()
		{
			TestRegistryItem(ItemSet.StoreCertificatesUnderCurrentUser, "StoreCertificatesUnderCurrentUser_1304", "System/Certificates",
				"Store Digital Certificates Under the Current User",
				"Specifies whether Digital Certificates are stored under the local computer or under the current user. By default Digital Certificates will be stored under the current user. If this setting is overridden and changed to NO then Digital Certificates will be stored under the local computer where the task is run.",
				RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers, true);
		}

		public void TestEnableCertificateManagementServiceTask()
		{
			AssertEquals("EnableCertificateManagementServiceTask", ItemSet.EnableCertificateManagementServiceTask.Name);
			AssertEquals("System/System-to-System Trust", ItemSet.EnableCertificateManagementServiceTask.Category);
			AssertEquals("Enable System To System Trust Certificate Management Service Task", ItemSet.EnableCertificateManagementServiceTask.Caption);
			AssertEquals("Specifies if the System To System Trust Certificate Management Task is enabled.", ItemSet.EnableCertificateManagementServiceTask.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.EnableCertificateManagementServiceTask.Storage);
		}

		public void TestEnableCertificateManagementServiceTaskRegistryOptionsAndDefaultValue()
		{
			var mockReg = new Mock<IProductRegistration>();
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalDeveloperSystem()).Returns(true);
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalUATSystem()).Returns(true);

			RegistryItemDictionary.Instance.PurgeAll();
			using (ObjectFactory.Substitute(mockReg.Object))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.EnableCertificateManagementServiceTask.Options);
				AssertEquals(false, ItemSet.EnableCertificateManagementServiceTask.DefaultValue);
			}

			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalDeveloperSystem()).Returns(false);
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalUATSystem()).Returns(true);

			RegistryItemDictionary.Instance.PurgeAll();
			using (ObjectFactory.Substitute(mockReg.Object))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.EnableCertificateManagementServiceTask.Options);
				AssertEquals(false, ItemSet.EnableCertificateManagementServiceTask.DefaultValue);
			}

			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalDeveloperSystem()).Returns(true);
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalUATSystem()).Returns(false);

			RegistryItemDictionary.Instance.PurgeAll();
			using (ObjectFactory.Substitute(mockReg.Object))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.EnableCertificateManagementServiceTask.Options);
				AssertEquals(false, ItemSet.EnableCertificateManagementServiceTask.DefaultValue);
			}

			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalDeveloperSystem()).Returns(false);
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalUATSystem()).Returns(false);

			RegistryItemDictionary.Instance.PurgeAll();
			using (ObjectFactory.Substitute(mockReg.Object))
			{
				AssertEquals(RegistryOptions.IsHidden, ItemSet.EnableCertificateManagementServiceTask.Options);
				AssertEquals(true, ItemSet.EnableCertificateManagementServiceTask.DefaultValue);
			}
		}

		public void TestMessageDecryptTimeout()
		{
			AssertEquals("MessageDecryptTimeout.DefaultValue", 10, ItemSet.MessageDecryptTimeout.DefaultValue);
		}

		public void TestLastDbHealthCheckWarningList()
		{
			AssertEquals("Name", "LastDbHealthCheckWarningList", ItemSet.LastDbHealthCheckWarningList.Name);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.LastDbHealthCheckWarningList.Storage);
			AssertEquals("LastDbHealthCheckWarningList.DefaultValue", 0, ItemSet.LastDbHealthCheckWarningList.DefaultValue.Count);

			DbHealthWarningRegistryCollection testCollection = new DbHealthWarningRegistryCollection();
			testCollection.Add(new DbHealthWarningRegistryElement("Source1", "Type1a", "Desc1aa", ZBool.False));
			testCollection.Add(new DbHealthWarningRegistryElement("Source2", "Type2b", "Desc2bb", ZBool.True, "X", ZDateTime.BrettsBirthday));
			ItemSet.LastDbHealthCheckWarningList.SetValue(testCollection);
			AssertEquals("DB Health Check Warning count", 2, ItemSet.LastDbHealthCheckWarningList.Value.Count);
			AssertEquals("Warning[0].Source", "Source1", ItemSet.LastDbHealthCheckWarningList.Value[0].Source);
			AssertEquals("Warning[0].WarningType", "Type1a", ItemSet.LastDbHealthCheckWarningList.Value[0].WarningType);
			AssertEquals("Warning[0].Description", "Desc1aa", ItemSet.LastDbHealthCheckWarningList.Value[0].Description);
			AssertEquals("Warning[0].IsAcknowledgeable", ZBool.False, ItemSet.LastDbHealthCheckWarningList.Value[0].IsAcknowledgeable);
			AssertEquals("Warning[0].IsAcknowledged", ZBool.False, ItemSet.LastDbHealthCheckWarningList.Value[0].IsAcknowledged);
			AssertEquals("Warning[0].AcknowledgedBy", "", ItemSet.LastDbHealthCheckWarningList.Value[0].AcknowledgedBy);
			AssertEquals("Warning[0].AcknowledgedDate", ZDateTime.Empty, ItemSet.LastDbHealthCheckWarningList.Value[0].AcknowledgedDate);
			AssertEquals("Warning[0].AcknowledgedByUser", "", ItemSet.LastDbHealthCheckWarningList.Value[0].AcknowledgedByUser);
			AssertEquals("Warning[1].Source", "Source2", ItemSet.LastDbHealthCheckWarningList.Value[1].Source);
			AssertEquals("Warning[1].WarningType", "Type2b", ItemSet.LastDbHealthCheckWarningList.Value[1].WarningType);
			AssertEquals("Warning[1].Description", "Desc2bb", ItemSet.LastDbHealthCheckWarningList.Value[1].Description);
			AssertEquals("Warning[1].IsAcknowledgeable", ZBool.True, ItemSet.LastDbHealthCheckWarningList.Value[1].IsAcknowledgeable);
			AssertEquals("Warning[1].IsAcknowledged", ZBool.True, ItemSet.LastDbHealthCheckWarningList.Value[1].IsAcknowledged);
			AssertEquals("Warning[1].AcknowledgedBy", "X", ItemSet.LastDbHealthCheckWarningList.Value[1].AcknowledgedBy);
			AssertEquals("Warning[1].AcknowledgedDate", ZDateTime.BrettsBirthday, ItemSet.LastDbHealthCheckWarningList.Value[1].AcknowledgedDate);
			AssertEquals("Warning[1].AcknowledgedByUser", "NonOperational", ItemSet.LastDbHealthCheckWarningList.Value[1].AcknowledgedByUser);
		}

		public void TestRefDbNamesCacheIsDirty()
		{
			TestGenericRegistryItem(ItemSet.RefDbNamesCacheIsDirty,
				"RefDbNamesCacheIsDirty",
				"System/Database",
				"Reference database name cache is dirty.",
				"Reference database name cache becomes dirty and should be refreshed.",
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden | RegistryOptions.NotCached,
				false);
		}

		public void TestShowQueryStackTraceInProcessControllerEnabled()
		{
			TestGenericRegistryItem(ItemSet.ShowQueryStackTraceInProcessControllerEnabled,
				"ShowQueryStackTraceInProcessControllerEnabled",
				"System/Process Controller",
				"Enable Query Stack Trace In Process Controllers",
				"",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsHidden,
				false);
		}

		public void TestLastYearlySyncTLS()
		{
			TestGenericRegistryItem(ItemSet.LastYearlySyncTLS,
					 "LastYearlySyncTLS",
					 "System/Process Controller",
					 "Last yearly sunc data done by TLS service tasks",
					 "",
					 RegistryStorageFlags.System,
					 RegistryOptions.IsHidden,
					 DateTime.MinValue);
		}

		public void TestContactForEDocExport()
		{
			TestRegistryItem(
				ItemSet.ContactForEDocExport,
				"ContactForEDocExport",
				"System/Data Export Settings",
				"Contact for eDoc Export",
				"The contact will be used as login to WebTracker for eDoc export.",
				RegistryStorageFlags.System, RegistryOptions.IsValueMandatory,
				RegistryFindBoxCollection.OrgContact, Guid.Empty);
		}

		public void TestEDocFileFormatListContainsCodes()
		{
			var fileFormats = ItemSet.EDocImportFileFormat;

			Assert("Pre-condition: Registry Item Data Type should be CodePairRegistryDataType", fileFormats.DataType is CodePairRegistryDataType);
			var lookupList = ((CodePairRegistryDataType)fileFormats.DataType).LookUpList;

			TestGenericRegistryItem(
				fileFormats,
				"EDocImportFileFormat",
				"System/DocManager",
				"eDoc Import File Format",
				"Documents which are automatically added to the eDocs tab will be stored in this format.",
				RegistryStorageFlags.System,
				"PDF");

			AssertEquals(lookupList.Count, 3);
			Assert(lookupList.ContainsCode("PDF"));
			Assert(lookupList.ContainsCode("TIF"));
			Assert(lookupList.ContainsCode("PDFA"));
		}

		public void TestEDocFileFormat_ShouldDefaultToPDF()
		{
			AssertEquals("PDF", ItemSet.EDocImportFileFormat.Value);
		}

		public void TestFTPRemoteDirectory()
		{
			AssertEquals("Default value", "", ItemSet.BIRDFTPRemoteDirectory.DefaultValue);
			AssertEquals("BIRDFTPRemoteDirectory", ItemSet.BIRDFTPRemoteDirectory.Name);
			AssertEquals("FTP Remote Directory Name", ItemSet.BIRDFTPRemoteDirectory.Caption);
			AssertEquals("The FTP directory for BIRD files import", ItemSet.BIRDFTPRemoteDirectory.Hint);
			AssertEquals(SystemDataRegistry.Categories.System_DataImportSettings_CustomsDeclarations_UnitedStatesofAmerica_BIRDFTPSettings, ItemSet.BIRDFTPRemoteDirectory.Category);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.BIRDFTPRemoteDirectory.Storage);

			Guid companyPK = new Guid();
			ItemSet.BIRDFTPRemoteDirectory.SetValue(companyPK, Guid.Empty, Guid.Empty, Env.TempPath);
			AssertEquals("Default value", Env.TempPath, ItemSet.BIRDFTPRemoteDirectory.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));
		}

		public void TestFTPPassword()
		{
			AssertEquals("Default value", "", ItemSet.BIRDFTPPassword.DefaultValue);
			AssertEquals("BIRDFTPPassword", ItemSet.BIRDFTPPassword.Name);
			AssertEquals("FTP Password", ItemSet.BIRDFTPPassword.Caption);
			AssertEquals("The FTP password for BIRD files import", ItemSet.BIRDFTPPassword.Hint);
			AssertEquals(SystemDataRegistry.Categories.System_DataImportSettings_CustomsDeclarations_UnitedStatesofAmerica_BIRDFTPSettings, ItemSet.BIRDFTPPassword.Category);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.BIRDFTPPassword.Storage);

			Guid companyPK = new Guid();
			ItemSet.BIRDFTPPassword.SetValue(companyPK, Guid.Empty, Guid.Empty, Env.TempPath);
			AssertEquals("Default value", Env.TempPath, ItemSet.BIRDFTPPassword.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));
		}

		public void TestFTPServerAddress()
		{
			AssertEquals("Default value", "", ItemSet.BIRDFTPServerAddress.DefaultValue);
			AssertEquals("BIRDFTPServerAddress", ItemSet.BIRDFTPServerAddress.Name);
			AssertEquals("FTP Server Address", ItemSet.BIRDFTPServerAddress.Caption);
			AssertEquals("The FTP server address for BIRD files import. It should be entered in the following format: ftp://{SERVER _NAME}/", ItemSet.BIRDFTPServerAddress.Hint);
			AssertEquals(SystemDataRegistry.Categories.System_DataImportSettings_CustomsDeclarations_UnitedStatesofAmerica_BIRDFTPSettings, ItemSet.BIRDFTPServerAddress.Category);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.BIRDFTPServerAddress.Storage);

			Guid companyPK = new Guid();
			ItemSet.BIRDFTPServerAddress.SetValue(companyPK, Guid.Empty, Guid.Empty, "ftp://rimlogisticsftp.ftphosting.net");
			AssertEquals("Default value", "ftp://rimlogisticsftp.ftphosting.net", ItemSet.BIRDFTPServerAddress.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));

			AssertEquals("No errors", string.Empty, ItemSet.BIRDFTPServerAddress.GetValidationErrorMessage("ftp://rimlogisticsftp.ftphosting.net", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Should validate with error", "Only FTP protocol is supported.", ItemSet.BIRDFTPServerAddress.GetValidationErrorMessage("http://rimlogisticsftp.ftphosting.net", Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestFTPUserName()
		{
			AssertEquals("Default value", "", ItemSet.BIRDFTPUserName.DefaultValue);
			AssertEquals("BIRDFTPUserName", ItemSet.BIRDFTPUserName.Name);
			AssertEquals("FTP User Name", ItemSet.BIRDFTPUserName.Caption);
			AssertEquals("The FTP user name for BIRD files import", ItemSet.BIRDFTPUserName.Hint);
			AssertEquals(SystemDataRegistry.Categories.System_DataImportSettings_CustomsDeclarations_UnitedStatesofAmerica_BIRDFTPSettings, ItemSet.BIRDFTPUserName.Category);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.BIRDFTPUserName.Storage);

			Guid companyPK = new Guid();
			ItemSet.BIRDFTPUserName.SetValue(companyPK, Guid.Empty, Guid.Empty, Env.TempPath);
			AssertEquals("Default value", Env.TempPath, ItemSet.BIRDFTPUserName.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));
		}

		public void TestFTPFileExtension()
		{
			AssertEquals("Default value", "BRD", ItemSet.BIRDFTPFileExtension.DefaultValue);
			AssertEquals("BIRDFTPFileExtension", ItemSet.BIRDFTPFileExtension.Name);
			AssertEquals("FTP File Extension", ItemSet.BIRDFTPFileExtension.Caption);
			AssertEquals("The BIRD importer will look into the designated FTP folder for the files with specified extension. By default, the BIRD importer will try to download and import all the '.BRD' files in the specified FTP folder. You can change the value to specify the target file extension for the BIRD import to import. The target file extension can not be empty.", ItemSet.BIRDFTPFileExtension.Hint);
			AssertEquals(SystemDataRegistry.Categories.System_DataImportSettings_CustomsDeclarations_UnitedStatesofAmerica_BIRDFTPSettings, ItemSet.BIRDFTPFileExtension.Category);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.BIRDFTPFileExtension.Storage);

			Guid companyPK = new Guid();
			ItemSet.BIRDFTPFileExtension.SetValue(companyPK, Guid.Empty, Guid.Empty, Env.TempPath);
			AssertEquals("Default value", Env.TempPath, ItemSet.BIRDFTPFileExtension.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));
			StringRegistryDataType ftpFileExtensiontype = (StringRegistryDataType)ItemSet.BIRDFTPFileExtension.DataType;
			AssertEquals(1, ftpFileExtensiontype.MinLength);
		}

		public void TestBiResetChangeDataCapture()
		{
			TestRegistryItem(ItemSet.BiResetChangeDataCapture,
				"BiResetChangeDataCapture",
				"System/BI/CDC",
				(NoResString)"Reset CDC",
				(NoResString)"This will disable then re-enable Change Data Capture for the database during the next database upgrade. It will also DROP and CREATE the Audit Database.",
				RegistryStorageFlags.System,
				RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestBiDisableChangeDataCapture()
		{
			TestRegistryItem(ItemSet.BiDisableChangeDataCapture,
				"BiDisableChangeDataCapture",
				"System/BI/CDC",
				(NoResString)"Disable CDC",
				(NoResString)"This will disable Change Data Capture for all CDC tables during the next CDN. The database will remain CDC enabled. If GLOW is enabled, then it will enable Change Tracking",
				RegistryStorageFlags.System,
				RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestBiAuditAPI()
		{
			TestRegistryItem(ItemSet.BiAuditAPI,
				"BiAuditAPI",
				"System/BI",
				(NoResString)"Audit API",
				(NoResString)"Enable Audit Web API",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				true);
		}

		public void TestBiReportAPI()
		{
			TestRegistryItem(ItemSet.BiReportAPI,
				"BiReportAPI",
				"System/BI",
				(NoResString)"Report API",
				(NoResString)"Enable Report Web API",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestBiODataAPI()
		{
			TestRegistryItem(ItemSet.BiODataAPI,
				"BiODataAPI",
				"System/BI",
				(NoResString)"BI OData API",
				(NoResString)"Enable BI OData API",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestSupportLegacyReportServer()
		{
			TestRegistryItem(ItemSet.SupportLegacyReportServer,
				"SupportLegacyReportServer",
				"System/BI",
				(NoResString)"Support Legacy Report Server",
				(NoResString)"Enable Support Legacy Report Server",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestBiAuditServer()
		{
			SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestAnalysisServer");
			SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BiReportCredential { Domain = "test", UserName = "test", Password = "test" });
			SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestAnalysisServer");

			TestStringRegistryItem(
				ItemSet.BiAuditServer,
				"BiAuditServer",
				"System/BI",
				(NoResString)"Audit Server",
				(NoResString)"Name of Audit Server",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				string.Empty,
				CharacterCase.Normal);
		}

		public void TestBiEnableAuditAccess()
		{
			TestRegistryItem(
				ItemSet.BiEnableAuditAccess,
				"BiEnableAuditAccess",
				"System/BI",
				"Allow Enterprise DB Reader access to the Audit DB",
				"Controls whether or not Enterprise Reader exists on the Audit DB.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				true);
		}

		public void TestBiDataWarehouseServer()
		{
			TestStringRegistryItem(
				ItemSet.BiDataWarehouseServer,
				"BiDataWarehouseServer",
				"System/BI",
				(NoResString)"Data Warehouse Server",
				(NoResString)"Name of Data Warehouse Server",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				string.Empty,
				CharacterCase.Normal);
		}

		public void TestCdcLatencyProviderAcceptableBacklog()
		{
			TestRegistryItem(
				ItemSet.CdcLatencyProviderAcceptableBacklog,
				"CdcLatencyProviderAcceptableBacklog",
				"System/BI/CDC",
				"CDC Acceptable Backlog Threshold",
				"Specifies the number of pending transactions to use as a threshold for throttling low priority services. When a CDC backlog greater than or equal to the specified number of transactions exists, low priority services will be throttled until the backlog decreases below this value.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				3000,
				3000,
				100000);
		}

		public void TestAuditRetentionPeriod()
		{
			TestRegistryItem(
				ItemSet.AuditRetentionPeriod,
				"AuditRetentionPeriod",
				"System/BI",
				"Audit Retention Period",
				"Number of months audit data should be kept",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				12,
				1,
				24);
		}

		public void TestListOfSupportedReportsUsingEdwAsDataSource()
		{
			AssertEquals("Name", "ListOfSupportedReportsUsingEdwAsDataSource", ItemSet.ListOfSupportedReportsUsingEdwAsDataSource.Name);
			AssertEquals("Category", "System/BI", ItemSet.ListOfSupportedReportsUsingEdwAsDataSource.Category);
			AssertEquals("Caption", "List Of Supported Reports Using EDW As Data Source", ItemSet.ListOfSupportedReportsUsingEdwAsDataSource.Caption);
			AssertEquals("Hint", "Below is the list of reports that support the use of EDW as a data source.", ItemSet.ListOfSupportedReportsUsingEdwAsDataSource.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, Instance.ListOfSupportedReportsUsingEdwAsDataSource.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, Instance.ListOfSupportedReportsUsingEdwAsDataSource.Options);
			AssertEquals("Default value", 0, ItemSet.ListOfSupportedReportsUsingEdwAsDataSource.Value.Count);
		}

		public void TestCdcMaxTransactions()
		{
			TestRegistryItem(
				ItemSet.CdcMaxTransactions,
				"CdcMaxTransactions",
				"System/BI/CDC",
				"CDC Maximum Transactions",
				"Number of maximum transactions to be scanned by CDC service task",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				100,
				100,
				5000);
		}

		public void TestCdcMaxScans()
		{
			TestRegistryItem(
				ItemSet.CdcMaxScans,
				"CdcMaxScans",
				"System/BI/CDC",
				"CDC Maximum Scans",
				"Number of maximum scans to be executed by CDC service task",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				5,
				5,
				20);
		}

		public void TestBiReportCompanyLogo()
		{
			TestGenericRegistryItem(ItemSet.BiReportCompanyLogo, "BiReportCompanyLogo", Categories.System_BusinessIntelligence_CompanyBranding, "Logo", "A company logo that appears in BI Reports", RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue);
		}

		public void TestEnableBiReport()
		{
			TestRegistryItem(
				ItemSet.EnableBiReport,
				"EnableBiReport",
				"System/BI",
				"Enable Bi Report",
				"Enable Bi Report",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				expectedDefaultValue: false);
		}

		public void TestServiceTaskMemoryConstraint()
		{
			TestRegistryItem(
				ItemSet.ServiceTaskMemoryConstraint,
				"ServiceTaskMemoryConstraint",
				"System/Process Controller",
				"Service Task Memory Constraint (in megabytes)",
				"Memory constraint for each service task",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				0,
				1000,
				10000);
		}

		public void TestDefaultColorTheme()
		{
			AssertEquals("Default value", ItemSet.ColorTheme.ToString(), ItemSet.StandardColorTheme.ToString());
		}

		public void TestLastWatermarkResetTime()
		{
			DateTime newDateTime = DateTime.Now;

			ItemSet.LastWatermarkResetTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newDateTime);
			AssertEquals("LastWatermarkResetTime.Value", newDateTime.ToString(), ItemSet.LastWatermarkResetTime.Value.ToString());
			DateTimeRegistryEditorInfo editorInfo = ItemSet.LastWatermarkResetTime.EditorInfo as DateTimeRegistryEditorInfo;
			AssertNotNull("EditorInfo should be of type DateTimeRegistryEditorInfo", editorInfo);
			AssertEquals("Datetime format", ZDateTimePickerFormat.Short, editorInfo.DateTimeFormat);
			AssertEquals("RegistryStorageFlags", RegistryStorageFlags.System, ItemSet.LastWatermarkResetTime.Storage);
			AssertEquals("RegistryOptions", RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport, ItemSet.LastWatermarkResetTime.Options);
		}

		public void TestStlCollectorLegacyHighWaterMark()
		{
			AssertStlCollectorHighWaterMark(ItemSet.StlCollectorHighWaterMark);
		}

		public void TestStlCollectorItemHighWaterMarks()
		{
			var prodKeyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(m => m.Key).Returns(prodKeyMock.Object);
			prodKeyMock.Setup(m => m.DatabaseType).Returns(DatabaseTypes.Codes.Production);
			prodKeyMock.Setup(m => m.HostedLocation).Returns("SYD");

			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTestingWithALPVersionBasedOffDateNow()))
			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			using (RawDataRegistry.Instance.ObtainDynamicSTLCollectorDefinitionsFromTheCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var staticItems = ObjectFactory.Get<ListObject>("StlCustomCollectorsList").Cast<IStlItem>();
				Assert("There should be some static items", staticItems.Any());
				foreach (var highWaterMarkItem in staticItems.Select(i => ItemSet.GetStlCollectorHighWaterMark(i.Code, i.Feature)))
				{
					AssertStlCollectorHighWaterMark(highWaterMarkItem);
				}

				var dynamicItems = ObjectFactory.Get<ListObject>("StlDynamicCollectorsList").Cast<IRefStlScript>().Where(x => x.ActiveOn.In(new string[] { "ALL", "PRD" }));
				Assert("There should be some dynamic items", dynamicItems.Any());
				var dynamicWaterMarkItems = new Dictionary<string, DateTimeRegistryItem>();
				foreach (var dynamicItem in dynamicItems)
				{
					if (!dynamicWaterMarkItems.ContainsKey(dynamicItem.FeatureCode))
					{
						dynamicWaterMarkItems.Add(dynamicItem.FeatureCode, ItemSet.GetStlCollectorHighWaterMark(dynamicItem.FeatureCode, dynamicItem.FeatureName));
					}
				}
				foreach (var highWaterMarkItem in dynamicWaterMarkItems.Values)
				{
					AssertStlCollectorHighWaterMark(highWaterMarkItem);
				}
			}
		}
		public void TestGetStlCollectorHighWaterMark_CategoryAssignment()
		{
			var prodKeyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(m => m.Key).Returns(prodKeyMock.Object);
			prodKeyMock.Setup(m => m.DatabaseType).Returns(DatabaseTypes.Codes.Production);
			prodKeyMock.Setup(m => m.HostedLocation).Returns("SYD");

			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTestingWithALPVersionBasedOffDateNow()))
			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			using (RawDataRegistry.Instance.ObtainDynamicSTLCollectorDefinitionsFromTheCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var staticItems = ObjectFactory.Get<ListObject>("StlCustomCollectorsList").Cast<IStlItem>();
				Assert("There should be some static items", staticItems.Any());
				foreach (var item in staticItems)
				{
					var dynamicCategory = new DynamicCategory(item.Module);
					var highWaterMarkItem = ItemSet.GetStlCollectorHighWaterMark(item.Code, item.Feature, dynamicCategory.CategoryString);
					AssertContains("Category", "System/STL/High Water Marks/", highWaterMarkItem.Category);
					AssertStlCollectorHighWaterMark(highWaterMarkItem);
				}
			}
		}

		public void TestHighWaterMarkExceptionThreshold()
		{
			TestRegistryItem(
				ItemSet.HighWaterMarkExceptionThresholdInHours,
				"HighWaterMarkExceptionThresholdInHours",
				Categories.System_STLHighWaterMarks,
				"High Water Mark Exception Threshold",
				"The maximum allowed time between successful data collections before an exception is thrown.",
				RegistryStorageFlags.System,
				RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport,
				expectedDefaultValue: 5,
				expectedMinValue: 0,
				expectedMaxValue: 60);
		}

		void AssertStlCollectorHighWaterMark(DateTimeRegistryItem highWaterMarkItem)
		{
			AssertVisible(highWaterMarkItem);

			AssertEquals("Storage", RegistryStorageFlags.System, highWaterMarkItem.Storage);
			AssertEquals("Options", RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport, highWaterMarkItem.Options);
			AssertEquals("EditorInfo", typeof(DateTimeRegistryEditorInfo), highWaterMarkItem.EditorInfo.GetType());
			AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Short, ((DateTimeRegistryEditorInfo)highWaterMarkItem.EditorInfo).DateTimeFormat);

			AssertEquals("StlCollectorHighWaterMark", highWaterMarkItem.DefaultValue, highWaterMarkItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			DateTime testDate = new DateTime(2015, 5, 11, 0, 0, 0);
			highWaterMarkItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testDate);
			AssertEquals("StlCollectorHighWaterMark", testDate, highWaterMarkItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestStlUsageBillingGatewayServer()
		{
			TestGenericRegistryItem(ItemSet.UsageBillingGateway,
				"UsageBillingGateway",
				SystemDataRegistry.Categories.System_STL,
				"Usage Billing Gateway Server Address",
				"This is the address of the usage billing gateway server.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
				"ehub-usage.wisegrid.net");
		}

		public void TestStlUsageBillingTestGatewayServer()
		{
			TestGenericRegistryItem(ItemSet.UsageBillingTestGateway,
				"UsageBillingTestGateway",
				SystemDataRegistry.Categories.System_STL,
				"Usage Billing Test Gateway Server Address",
				"This is the address of the usage billing gateway test server.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
				"ehub-usage-test.wisegrid.net");
		}

		public void TestStlUssOutageStartTime()
		{
			TestGenericRegistryItem(ItemSet.USSOutageStartTime,
				"USSOutageStartTime",
				SystemDataRegistry.Categories.System_STL,
				"USS Service Task Outage Start Time",
				"The time the current outage for the USS service task started",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				DateTime.MinValue);
		}

		public void TestTimeZoneOffsetCachePeriod()
		{
			AssertVisible(ItemSet.TimeZoneOffsetCachePeriod);

			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.TimeZoneOffsetCachePeriod.Storage);
			AssertEquals("24 months should be the default value, and yet...", 24, ItemSet.TimeZoneOffsetCachePeriod.Value);
			ItemSet.TimeZoneOffsetCachePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);
			AssertEquals(12, ItemSet.TimeZoneOffsetCachePeriod.Value);

			ItemSet.TimeZoneOffsetCachePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 60);
			AssertExceptionThrown<RegistryValidationException>("Minimum value should be 12, and yet...", () => ItemSet.TimeZoneOffsetCachePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 11));
			AssertExceptionThrown<RegistryValidationException>("Maximum value should be 60, and yet...", () => ItemSet.TimeZoneOffsetCachePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 61));
		}

		public void TestUseReportingDbServerNames()
		{
			var item = ItemSet.UseReportingDbServerNames;
			AssertNotVisible(item);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.UseReportingDbServerNames.Storage);
			AssertEquals("Default value", true, ItemSet.UseReportingDbServerNames.Value);

			System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(20));
			ItemSet.RemoveItemFromCacheIfOlderThan("UseReportingDbServerNames", TimeSpan.FromMilliseconds(10));

			EnvProxy.SetHostedLocationForTest("IDC");
			AssertEquals("Default value", false, ItemSet.UseReportingDbServerNames.Value);
		}

		public void TestEConversationMessageEmailTemplate()
		{
			TestGenericRegistryItem(ItemSet.EConversationMessageEmailTemplate,
				"EConversationMessageEmailTemplate",
				Categories.System_EConversations,
				"eConversation Message Email Template",
				"Configure the template for eConversation message notification emails.",
				RegistryStorageFlags.System);

			AssertType<EConversationMessageEmailTemplateRegistryItem>(ItemSet.EConversationMessageEmailTemplate);

			var invalidTemplate = new NotificationEmailTemplate { EmailSubject = "No id field" };
			AssertExceptionThrown<RegistryValidationException>("Should validate ID exists in email subject", "Email subject must contain the (*ID*) special field.", () => ItemSet.EConversationMessageEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, invalidTemplate));
		}

		#region Implementation

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return "InvoiceTypesToExportInPDFFormat";
				yield return "InvoicesInPDFFormatExportDirectory";
				yield return "ACDataImportDirectory";
				yield return "ACStatusDataImportDirectory";
				yield return "ALPOExportDirectory";
				yield return "ATLASExportDirectory";
				yield return "AutoPopulateInvoicePackagingAndContainerDetailsFromOrderLines";
				yield return "PersonMergePreviewItems";
				yield return "EnableStackTraceInUserContextSwitcher";
				yield return "MaxNumberOfAttemptsForRuleProcessing";
				yield return nameof(SystemDataRegistry.UseVersionID);
				yield return nameof(SystemDataRegistry.SystemToSystemTrustDataProtectionMechanism);
				yield return nameof(SystemDataRegistry.SystemToSystemTrustDataProtectionGroup);
				yield return nameof(SystemDataRegistry.PersonIntelligenceModuleEnabled);
				yield return nameof(SystemDataRegistry.NtpTimeServers);
			}
		}

		BusinessObject GetAllUsersGlbGroup()
		{
			return (BusinessObject)Factory.LoadTop1<IGlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL"));
		}

		Guid australianBranchPk;
		Guid AustralianBranchPk
		{
			get
			{
				if (australianBranchPk == Guid.Empty)
				{
					var newFactory = new BusinessObjectFactory();
					var auCompany = newFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
					auCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = (ZString)Enterprise.Core.Constants.CountryCodes.Australia;

					var auBranch = newFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
					auBranch[GlbBranchSchema.Constants.GB_GC] = auCompany.PK;
					auBranch[GlbBranchSchema.Constants.GB_RL_NKHomePort] = "AUBTB";
					newFactory.Save();

					australianBranchPk = auBranch.PK.ToGuid();
				}

				return australianBranchPk;
			}
		}

		#endregion

		public void TestEnableSecurityOverrideToken()
		{
			TestRegistryItem(
				ItemSet.EnableSecurityOverrideToken,
				"EnableSecurityOverrideToken",
				"System/Security Override",
				"Enable Security Override Token",
				"When OIDC is enabled, setting this registry to 'Yes' allows authorized users to generate a 6 digits, one time use token for security overrides.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		public void TestSecurityOverrideTokenExpiryTime()
		{
			TestRegistryItem(
				ItemSet.SecurityOverrideTokenExpiryTime,
				"SecurityOverrideTokenExpiryTime",
				Categories.System_SecurityOverride,
				"Security Override Token Expiry Time",
				"This value specifies the expiration time of the security override authorization token after it is generated by CargoWise. Updating this value will adjust authorization token’s validity period.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				5,
				5,
				30);
		}

		public void TestRegistryRefresh()
		{
			var mock = new Mock<IStopwatch>();

			var stopwatch = mock.Object;
			using (ObjectFactory.Substitute(stopwatch))
			{
				mock.Setup(m => m.ElapsedMilliseconds).Returns((long)TimeSpan.FromMilliseconds(1).TotalMilliseconds);
				ItemSet.BIRDFTPFileExtension.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123");

				AssertEquals("123", ItemSet.BIRDFTPFileExtension.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

				string sql = @"UPDATE dbo.StmData
							SET SD_BinaryValue = CONVERT(VarBinary, CONVERT(NVarChar(50), '124'))
							WHERE SD_Name = @name";

				Db.Connection.ExecuteNonQuery(sql, cmd => cmd.AddParameterBasedOnDbColumn("@name", "BIRDFTPFileExtension", StmDataSchema.SD_Name));

				mock.Setup(m => m.ElapsedMilliseconds).Returns((long)TimeSpan.FromMilliseconds(1).TotalMilliseconds);

				AssertEquals("123", ItemSet.BIRDFTPFileExtension.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

				mock.Setup(m => m.ElapsedMilliseconds).Returns((long)TimeSpan.FromDays(1).TotalMilliseconds);

				AssertEquals("124", ItemSet.BIRDFTPFileExtension.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			}
		}

		#region Throttling of Process Controller Dispatching Loop
		public void TestProcessControllerDispatchingLoopThrottlingDefaultValuesMakeSense()
		{
			Assert(ItemSet.ThrottlingPeriodForDispatchingLoopWithExceedingBacklog < ItemSet.ThrottlingPeriodForDispatchingLoopWithAcceptableBacklog);
			Assert(ItemSet.ThrottlingPeriodForDispatchingLoopWithAcceptableBacklog < ItemSet.ThrottlingPeriodForDispatchingLoopWithBacklogWaitingForMaxSecondary);
			Assert(ItemSet.ThrottlingPeriodForDispatchingLoopWithExceedingBacklog <= TimeSpan.FromMilliseconds(1000));
		}
		#endregion

		public void TestEnableQueryHintsForColourSchemeManager()
		{
			TestRegistryItem(
					ItemSet.EnableQueryHintsForColourSchemeManager,
					"EnableQueryHintsForColourSchemeManager",
					Categories.Optimization,
					"Enables Query Hints for Color Scheme Manager",
					"Enables Query Hints for the Color Scheme Manager when the top level table is indexed.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					true);
		}

		#region ArchiveManager

		public void TestBiIsRequiredDeleteOrphanSubscriber()
		{
			TestRegistryItem(
				ItemSet.BiIsRequiredDeleteOrphanSubscriber,
				"BiIsRequiredDeleteOrphanSubscriber",
				Categories.System_ArchiveManager,
				"Is Required for Delete Orphan Subscriber",
				"This enables/disables the Delete Orphan Subscriber (DOS).",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				expectedDefaultValue: false
			);
		}

		public void TestEnableOfflineArchiving()
		{
			var originalHostedLocation = EnvProxy.HostedLocation;
			EnvProxy.SetHostedLocationForTest("");
			AssertEquals("Hosted with cargowise", false, EnvProxy.IsHostedWithCargowise);
			TestRegistryItem(
				ItemSet.EnableOfflineArchiving,
				"EnableOfflineArchiving",
				Categories.System_ArchiveManager,
				"Enable Offline Archiving",
				"Offline Archiving is the process where documents that have been archived are stored in a location other than on the database server. The Offline Archiving process is only available for self-hosted clients.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue,
				true);
			EnvProxy.SetHostedLocationForTest("SYD");
			AssertEquals("Hosted with cargowise", true, EnvProxy.IsHostedWithCargowise);
			AssertEquals("Offline Archiving needs to be disabled for wisecloud hosted clients", false, ItemSet.EnableOfflineArchiving.Value);
			EnvProxy.SetHostedLocationForTest(originalHostedLocation);
		}

		public void TestOnlineArchiveDocumentFormat()
		{
			TestGenericRegistryItem(
					ItemSet.OnlineArchiveDocumentFormat,
					"OnlineArchiveDocumentFormat",
					Categories.System_ArchiveManager,
					"Generated Documents Format",
					"Documents that are generated during the archiving process will be stored in this format.",
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					Enterprise.Core.Constants.FileFormats.TIF);
			AssertEquals("OnlineArchiveDocumentFormat.Value default should be TIF", Enterprise.Core.Constants.FileFormats.TIF, ItemSet.OnlineArchiveDocumentFormat.Value);

			ItemSet.OnlineArchiveDocumentFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.FileFormats.PDF);
			AssertEquals("OnlineArchiveDocumentFormat.value", Enterprise.Core.Constants.FileFormats.PDF, ItemSet.OnlineArchiveDocumentFormat.Value);
		}

		public void TestOnlineArchiveDocumentFormatLookupList()
		{
			var documentFormatRegistry = ItemSet.OnlineArchiveDocumentFormat;

			Assert("Pre-condition: Registry Item Data Type should be CodePairRegistryDataType", documentFormatRegistry.DataType is CodePairRegistryDataType);
			var lookupList = ((CodePairRegistryDataType)documentFormatRegistry.DataType).LookUpList;

			AssertEquals(lookupList.Count, 3);
			Assert(lookupList.ContainsCode("PDF"));
			Assert(lookupList.ContainsCode("TIF"));
			Assert(lookupList.ContainsCode("PDFA"));
		}

		public void TestIncludeTimeTakenInTheARCLogs()
		{
			TestRegistryItem(
				ItemSet.IncludeTimeTakenInTheARCLogs,
				"IncludeTimeTakenInTheARCLogs",
				Categories.System_ArchiveManager,
				"Include Time Taken In The ARC Logs",
				"When enabled, the ARC service Task logs will include the time taken for different operations in processing the archive schedule jobs.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				true
			);
		}

		public void TestLoadArchiveSetTimeout()
		{
			TestGenericRegistryItem(ItemSet.LoadArchiveSetTimeout,
				"LoadArchiveSetTimeout",
				Categories.System_ArchiveManager,
				"Load Archive Set Timeout",
				"Time in minutes that the application will wait for a successful load of Archive Set before timing out. Accepted values are from 0 to 120. 0 means no time limit.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				30);
		}

		public void TestLoadArchiveSetBatchTimeout()
		{
			TestGenericRegistryItem(ItemSet.LoadArchiveSetBatchTimeout,
				"LoadArchiveSetBatchTimeout",
				Categories.System_ArchiveManager,
				"Load Archive Set Batch Timeout",
				"Time in minutes that the application will wait for a successful load of Archive Set Batch before timing out. Accepted values are from 0 to 120. 0 means no time limit.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				10);
		}

		#region InactiveOperationalJobsArchiveSystem

		public void TestInactiveOperationalJobsArchiveSystemOnOrBeforeMinimum()
		{
			TestRegistryItem(
				ItemSet.InactiveOperationalJobsArchiveSystemOnOrBeforeMinimum,
				"InactiveOperationalJobsArchiveSystemOnOrBeforeMinimum",
				Categories.System_ArchiveManager_InactiveOperationalJobsArchiveSystem,
				"On or Before Minimum",
				"This registry allows you to define the minimum number of years that a record must have been in the system before it can be archived using the Inactive Operational Jobs Archive System (IPS). \r\n\r\nWarning: Please make sure to confirm the data retention requirements for this data before archiving.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				7,
				0,
				100
			);
		}

		#endregion

		#region OperationalJobsArchiveSystem

		public void TestArchiveRecordsOnOrBeforeMinimum()
		{
			TestRegistryItem(
				ItemSet.ArchiveRecordsOnOrBeforeMinimum,
				"ArchiveRecordsOnOrBeforeMinimum",
				Categories.System_ArchiveManager_OperationalJobsArchiveSystem,
				"On or Before Minimum",
				"This registry allows you to define the minimum number of years that a record must have been in the system before it can be archived using the Operations Jobs Archive (OPS) system. \r\n\r\nWarning: Please make sure to confirm the data retention requirements for this data before archiving.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				7,
				0,
				100
			);
		}

		public void TestBatchSizeControl()
		{
			TestRegistryItem(
				ItemSet.BatchSizeControl,
				"BatchSizeControl",
				Categories.System_ArchiveManager,
				"Set Batch Size for Archiving and Purging Operational Jobs",
				"The maximum number of records to be processed in one batch by Archive Manager. This includes schedules for Operational Jobs Archive System (OPS), Inactive Operational Jobs Archive System (IPS), and Purge Documents and Records (PDR).",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				50,
				1,
				500
			);
		}

		#endregion

		#region StandaloneRecordsArchiveSystem

		public void TestStandaloneRecordsOnOrBeforeMinimum()
		{
			TestRegistryItem(
				ItemSet.StandaloneRecordsOnOrBeforeMinimum,
				"StandaloneRecordsOnOrBeforeMinimum",
				Categories.System_ArchiveManager_StandaloneRecordsArchiveSystem,
				"On or Before Minimum",
				"This registry allows you to define the minimum number of years that a record must have been in the system before it can be archived using the Standalone Records Archive System (STA). \r\n\r\nWarning: Please make sure to confirm the data retention requirements for this data before archiving.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				7,
				0,
				100
			);
		}

		public void TestStandaloneRecordsBatchSizeControl()
		{
			TestRegistryItem(
				ItemSet.StandaloneRecordsBatchSizeControl,
				"StandaloneRecordsBatchSizeControl",
				Categories.System_ArchiveManager_StandaloneRecordsArchiveSystem,
				"Set Batch Size",
				"The maximum number of records to be processed in one batch by Archive Manager. This includes schedule for Standalone Records Archive System (STA).",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				200,
				1,
				10000
			);
		}

		#endregion

		#region PurgeArchivedRecordsSystem

		public void TestPurgeArchivedRecordsOnOrBeforeMinimum()
		{
			TestRegistryItem(
				ItemSet.PurgeArchivedRecordsOnOrBeforeMinimum,
				"PurgeArchivedRecordsOnOrBeforeMinimum",
				Categories.System_ArchiveManager_PurgeArchivedRecordsSystem,
				"On or Before Minimum",
				"This registry allows you to define the minimum number of years that a record must have been archived before it can be deleted when using the Purge Archived Records (PAR) system.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				7,
				0,
				100
			);
		}

		public void TestPurgeArchivedRecordsBatchSizeControl()
		{
			TestRegistryItem(
				ItemSet.PurgeArchivedRecordsBatchSizeControl,
				"PurgeArchivedRecordsBatchSizeControl",
				Categories.System_ArchiveManager_PurgeArchivedRecordsSystem,
				"Set Batch Size",
				"The maximum number of records in the Archived Records module to be processed by the Archive Manager in one batch using the Purge Archived Records System (PAR).",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				200,
				1,
				10000
			);
		}

		#endregion

		#region PurgeDocumentsAndRecordsSystem

		public void TestPurgeDocumentsAndRecordsOnOrBeforeMinimum()
		{
			TestRegistryItem(
				ItemSet.PurgeDocumentsAndRecordsOnOrBeforeMinimum,
				"PurgeDocumentsAndRecordsOnOrBeforeMinimum",
				Categories.System_ArchiveManager_PurgeDocumentsAndRecordsSystem,
				"On or Before Minimum",
				"This registry allows you to define the minimum number of years that a record must have been in the system before it can be purged using the Purge Documents and Records (PDR) system. \r\n\r\nWarning: Please make sure to confirm the data retention requirements for this data before purging.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				7,
				0,
				100
			);
		}

		#endregion

		#region PurgeDocumentsOfOperationalRecordsSystem

		public void TestPurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum()
		{
			TestRegistryItem(
				ItemSet.PurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum,
				"PurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum",
				Categories.System_ArchiveManager_PurgeDocumentsOfOperationalRecordsSystem,
				"On or Before Minimum",
				"This registry allows you to define the minimum number of years that a record must have been in the system before their related documents can be deleted using the Purge Documents of Operational Records System (PDO). \r\n\r\nWarning: Please make sure to confirm the data retention requirements for this data before purging.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				10,
				0,
				100
			);
		}

		public void TestPurgeDocumentsOfOperationalRecordsSystemBatchSizeControl()
		{
			TestRegistryItem(
				ItemSet.PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl,
				"PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl",
				Categories.System_ArchiveManager_PurgeDocumentsOfOperationalRecordsSystem,
				"Set Batch Size",
				"The maximum number of records to be processed in one batch by Archive Manager.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				50,
				1,
				10000
			);
		}

		#endregion

		#region HVLVArchiveSystem

		public void TestHAROnOrBeforeMinimum()
		{
			TestRegistryItem(
				ItemSet.HVLVArchiveSystemOnOrBeforeMinimum,
				"HVLVArchiveSystemOnOrBeforeMinimum",
				Categories.System_ArchiveManager_HVLVArchiveSystem,
				"On or Before Minimum",
				"This registry setting specifies how many years from the Shipment's arrival date HVLV Consignments and Items will remain before they are marked as archived via HVLV Archive System (HAR).\r\n\r\nWarning: Please make sure to confirm the data retention requirements for this data before archiving.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				1,
				0,
				100
			);
		}

		public void TestHARBatchSizeControl()
		{
			TestRegistryItem(
				ItemSet.HVLVArchiveSystemBatchSizeControl,
				"HVLVArchiveSystemBatchSizeControl",
				Categories.System_ArchiveManager_HVLVArchiveSystem,
				"Set Batch Size",
				"The maximum number of shipments to be processed in one batch by Archive Manager using the HVLV Archive System (HAR).",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				100,
				1,
				100
			);
		}

		public void TestArchiveFileFormat()
		{
			var archiveFileFormats = ItemSet.ArchivingFileFormat;
			var lookupList = ((CodePairRegistryDataType)archiveFileFormats.DataType).LookUpList;

			TestGenericRegistryItem(
				archiveFileFormats,
				"ArchivingFileFormat",
				Categories.System_ArchiveManager_HVLVArchiveSystem,
				"Archiving File Format",
				"Select the file format that will be stored in the shipment eDocs once HVLV data has been archived by the HVLV Archive System (HAR).",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				Core.Constants.FileFormats.CSV
			);

			AssertEquals(lookupList.Count, 2);
			Assert(lookupList.ContainsCode(Core.Constants.FileFormats.XML));
			Assert(lookupList.ContainsCode(Core.Constants.FileFormats.CSV));
		}

		#endregion

		#region ExpiredRatesArchiveSystem

		public void TestExposeExpiredRatesArchiveSystem()
		{
			TestRegistryItem(
				ItemSet.ExposeExpiredRatesArchiveSystem,
				"ExposeExpiredRatesArchiveSystem",
				Categories.System_ArchiveManager_PurgeExpiredRatesSystem,
				"Expose RED",
				"Whether to make the Expired Rates Archive System visible.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				expectedDefaultValue: false
			);
		}

		public void TestREDOnOrBeforeMinimum()
		{
			TestRegistryItem(
				ItemSet.ExpiredRatesArchiveSystemOnOrBeforeMinimum,
				"ExpiredRatesArchiveSystemOnOrBeforeMinimum",
				Categories.System_ArchiveManager_PurgeExpiredRatesSystem,
				"On or Before Minimum",
				"This registry setting specifies how many years a rate must have been expired for before being eligible for archiving under the Expired Rates Archive System. A value of 0 means it will be targeted for archiving immediately.\r\n\r\nWarning: Please make sure to confirm the data retention requirements for this data before archiving.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				7,
				0,
				100
			);
		}

		public void TestExpiredRatesArchiveSystemBatchSizeControl()
		{
			TestRegistryItem(
				ItemSet.ExpiredRatesArchiveSystemBatchSizeControl,
				"ExpiredRatesArchiveSystemBatchSizeControl",
				Categories.System_ArchiveManager_PurgeExpiredRatesSystem,
				"Set Batch Size",
				"The maximum number of expired rates to be processed in one batch by Archive Manager using the Expired Rates Archive System (RED).",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				100,
				1,
				5000
			);
		}

		#endregion

		#region ActivityLogsArchiveSystem

		public void TestExposeActivityLogsArchiveSystem()
		{
			TestRegistryItem(
				ItemSet.ExposeActivityLogsArchiveSystem,
				"ExposeActivityLogsArchiveSystem",
				Categories.System_ArchiveManager_PurgeActivityLogsSystem,
				"Expose PAL",
				"Whether to make the Activity Logs Purge System visible.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				expectedDefaultValue: false
			);
		}

		public void TestPALOnOrBeforeMinimum()
		{
			TestRegistryItem(
				ItemSet.ActivityLogsArchiveSystemOnOrBeforeMinimum,
				"ActivityLogsArchiveSystemOnOrBeforeMinimum",
				Categories.System_ArchiveManager_PurgeActivityLogsSystem,
				"On or Before Minimum",
				"This registry specifies allows you to determine the minimum number of years that a record must have been in the system before it can be purged using the Purge Activity Logs (PAL).\r\n\r\nWarning: Please make sure to confirm the data retention requirements for this data before archiving.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				7,
				1,
				100
			);
		}

		#endregion

		#endregion

		public void TestQueryTimeOutForColourSchemeManager()
		{
			TestRegistryItem(
					ItemSet.QueryTimeoutForColourSchemeManager,
					"QueryTimeoutForColourSchemeManager",
					Categories.Optimization,
					"Color Scheme Manager Query Timeout",
					"Set query timeout for color scheme manager in order to prevent server overhead caused by over-complex color rules.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					5);
		}

		public void TestReportSlowUpgradeTransformSeconds()
		{
			TestRegistryItem(
				ItemSet.ReportSlowUpgradeTransformsSeconds,
				"ReportSlowUpgradeTransformsSeconds",
				Categories.Optimization,
				"Report offline upgrade transforms above this many seconds",
				"After each offline pre-upgrade or offline post-upgrade transform completes, if it took more than this many seconds it is reported.",
				RegistryStorageFlags.System,
				30);
		}

		public void TestEnableRecompileFilter()
		{
			TestRegistryItem(
				ItemSet.RecompileFilter,
				"RecompileFilter",
				Categories.Optimization,
				"Enable Recompile Filter",
				"Enabling this registry adds the Recompile option to every module filter. This will configure the search query to use OPTION(RECOMPILE).",
				RegistryStorageFlags.System,
				false
			);
		}

		public void TestEnableLegacyCardinalityEstimationFilter()
		{
			TestRegistryItem(
				ItemSet.CardinalityFilter,
				"CardinalityFilter",
				Categories.Optimization,
				"Enable Legacy Cardinality Estimation Filter",
				"Enabling this registry item adds the Legacy Cardinality Estimation option to every module filter. This will configure the search query to use the Pre-2014 Legacy Cardinality Estimation model.",
				RegistryStorageFlags.System,
				false
			);
		}

		public void TestEnableModuleQueryFromSecondaryDbReplica()
		{
			TestRegistryItem(
				ItemSet.EnableModuleQueryFromSecondaryDbReplica,
				"EnableModuleQueryFromSecondaryDbReplica",
				Categories.Optimization,
				"Enable Read from Secondary Database Replica",
				"When enabled, allows search operations to use a secondary database replica, reducing the load on the primary SQL database node.",
				RegistryStorageFlags.System,
				EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForController,
				false
			);
		}

		public void TestModuleQueryDbServerNames()
		{
			TestRegistryItem(
				ItemSet.ModuleQueryDbServerNames,
				"ModuleQueryDbServerNames",
				Categories.Optimization,
				"Secondary Databases Full Server Names",
				"The full server names of the secondary databases, including the instance names if applicable.",
				RegistryStorageFlags.System,
				EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForController
			);

			AssertEquals(128, ItemSet.ModuleQueryDbServerNames.DataType.MaximumLength);
		}

		public void TestGoogleMapsClientId()
		{
			TestRegistryItem(
				ItemSet.GoogleMapsClientId,
				"GoogleMapsClientId",
				"System/Google Maps",
				"Google Maps Client ID",
				"The client ID to be used for Google Maps services.",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
				"gme-translogix");
		}

		public void TestGoogleMapsAPIKey()
		{
			TestRegistryItem(
				ItemSet.GoogleMapsAPIKey,
				"GoogleMapsAPIKey",
				"System/Google Maps",
				"Google Maps API Key for Web",
				"The API key to be used for Google Maps API services accessible from Web.",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
				"AIzaSyAxgGMeU4YHsfnBxGVOrLRrkPaaQumFrtI");
		}
		public void TestGoogleMapsAPIKeyForServiceTasks()
		{
			TestRegistryItem(
				ItemSet.GoogleMapsAPIKeyForServiceTasks,
				"GoogleMapsAPIKeyForServiceTasks",
				"System/Google Maps",
				"Google Maps API Key for service tasks",
				"The API key to be used for Google Maps API services accessible from service tasks.",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
				"AIzaSyCt2OV3miUwsCkfAV_M8nJzMB2Fiug4MpI");
		}

		public void TestMaxNumberOfAttemptsForRuleProcessing()
		{
			TestRegistryItem(
				ItemSet.MaxNumberOfAttemptsForRuleProcessing,
				"MaxNumberOfAttemptsForRuleProcessing",
				Categories.System_ProductionRules,
				"Max No. of Attempts for Production Rule Processing",
				"An email will be sent to appropriate groups when production rules are disabled for exceeding the maximum number of attempts.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				expectedDefaultValue: 10,
				expectedMinValue: 1,
				expectedMaxValue: 1000);
		}

		public void TestRulesEngineSessionFactoryCacheMinutes()
		{
			using (Globals.TemporaryOverrideForIsTest(false))
			{
				AssertRulesEngineSessionFactoryCacheRegistry(expectedDefault: 20);
			}
		}

		public void TestRulesEngineSessionFactoryCacheMinutes_InTest()
		{
			// Cache should default to 0 in unit tests as it is, we use this approach as it is:
			// - Faster than hooking up a test listener for cleanup
			// - Simpler than making a test only dependency
			// - Much more robust than overriding the item in individual tests
			AssertRulesEngineSessionFactoryCacheRegistry(expectedDefault: 0);
		}

		void AssertRulesEngineSessionFactoryCacheRegistry(int expectedDefault)
		{
			TestRegistryItem(
				ItemSet.RulesEngineSessionFactoryCacheMinutes,
				"RulesEngineSessionFactoryCacheMinutes",
				Categories.System_ProductionRules,
				"Production Rules Engine Cache Expiration Time in Minutes",
				"Production rules will be cached and expire after configured minutes.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				expectedDefaultValue: expectedDefault,
				expectedMinValue: 0,
				expectedMaxValue: 60);
		}

		public void TestMaximumAllowedVLFsCount()
		{
			TestRegistryItem(
				ItemSet.MaximumAllowedVLFsCount,
				expectedName: "MaximumAllowedVLFsCount",
				expectedCategory: "System/Database/Log Shrink After Upgrade",
				expectedCaption: "Maximum Allowed Virtual Log Files",
				expectedHint: "This setting determines the threshold for the maximum number of Virtual Log Files (VLFs) allowed in a database log file. When the number of VLFs exceeds this threshold, the log file will be considered for shrinking during the upgrade process.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted,
				100);
		}

		public void TestMaximumLogToTwoWeekBackupPercentage()
		{
			TestRegistryItem(
				ItemSet.MaximumLogToTwoWeekBackupPercentage,
				expectedName: "MaximumLogToTwoWeekBackupPercentage",
				expectedCategory: "System/Database/Log Shrink After Upgrade",
				expectedCaption: "Maximum Log File to Backup Size Percentage",
				expectedHint: "This setting defines the maximum size of the log file in relation to the largest backup made in the last two weeks, measured as a percentage. If the size of the current log file goes beyond this set percentage, the log file will be eligible for shrinking.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted,
				120);
		}

		public void TestNtpTimeServers()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			TestRegistryItem(
				ItemSet.NtpTimeServers,
				expectedName: "NtpTimeServers",
				expectedCategory: "System/Time Servers",
				expectedCaption: "Time Server List",
				expectedHint: "List of Time Servers",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted);
		}

		public void TestNtpTimeServersSelfHosted()
		{
			EnvProxy.SetHostedLocationForTest(LicenceConstants.NotHostedWithCargoWise);

			TestRegistryItem(
				ItemSet.NtpTimeServers,
				expectedName: "NtpTimeServers",
				expectedCategory: "System/Time Servers",
				expectedCaption: "Time Server List",
				expectedHint: "List of Time Servers",
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden);
		}

		public void TestMaxNumberOfRecordsToShowInDisplayGrids()
		{
			AssertEquals("MaxNumberOfRecordsToShowInDisplayGrids", 1000, ItemSet.MaxNumberOfRecordsToShowInDisplayGrids.Value);

			AssertExceptionThrown<RegistryValidationException>("", "Value must be less than or equal to the maximum (15000)", () => ItemSet.MaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 15001));
			AssertExceptionThrown<RegistryValidationException>("", "Value must be greater than or equal to the minimum (1)", () => ItemSet.MaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0));
		}

		public void TestRegistryItemsHaveLockedDownOption()
		{
			Assert(ItemSet.MaxNumberOfRecordsToShowInDisplayGrids.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
		}

		public void TestFTPDestinationOverride()
		{
			AssertEquals("System/Testing", ItemSet.FTPDestinationOverride.Category);
			AssertEquals("FTPDestinationOverride", ItemSet.FTPDestinationOverride.Name);
			AssertEquals("Enter a FTP address here to force all scheduled reports with a delivery method of FTP to be sent to this FTP address. This is to prevent scheduled reports from being delivered to client FTP systems out of the non-production environment, while still being able to test the FTP scheduled report delivery process.", ItemSet.FTPDestinationOverride.Hint);
		}

		public void TestFTPDestinationOverrideForHostedSystem_ProductionDB()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			var itemSet = GetNewItemSet();
			Assert("This is visible", itemSet.FTPDestinationOverride.IsVisible(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			var originalHostedLocation = EnvProxy.HostedLocation;

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				AssertEquals(false, itemSet.FTPDestinationOverride.IsReadOnly);
				EnvProxy.SetHostedLocationForTest("");
				AssertEquals(false, itemSet.FTPDestinationOverride.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.PostMasterUserName, Guid.Empty, Guid.Empty))
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				AssertEquals(false, itemSet.FTPDestinationOverride.IsReadOnly);
				EnvProxy.SetHostedLocationForTest("");
				AssertEquals(false, itemSet.FTPDestinationOverride.IsReadOnly);
			}
			EnvProxy.SetHostedLocationForTest(originalHostedLocation);
		}

		public void TestFTPDestinationOverrideForHostedSystem_NonProductionDB()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Training);
			var itemSet = GetNewItemSet();
			Assert("This is visible", itemSet.FTPDestinationOverride.IsVisible(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			var originalHostedLocation = EnvProxy.HostedLocation;

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				AssertEquals(false, itemSet.FTPDestinationOverride.IsReadOnly);
				EnvProxy.SetHostedLocationForTest("");
				AssertEquals(false, itemSet.FTPDestinationOverride.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.PostMasterUserName, Guid.Empty, Guid.Empty))
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				AssertEquals(false, itemSet.FTPDestinationOverride.IsReadOnly);
				EnvProxy.SetHostedLocationForTest("");
				AssertEquals(false, itemSet.FTPDestinationOverride.IsReadOnly);
			}
			EnvProxy.SetHostedLocationForTest(originalHostedLocation);
		}

		public void TestErrorReportingServiceUri()
		{
			TestRegistryItem(
				ItemSet.ErrorReportingServiceUri,
				"ErrorReportingServiceUri",
				"System/Misc",
				"Error Reporting Service URI",
				"If set, error reports will be redirected to the supplied service URI, instead of the default error reporting service.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				TextEditorType.TextBox,
				expectedDefaultValue: WTG.ErrorReporting.WellKnownServiceUris.Production,
				testValueToSetAndRead: "https://error-reporting.example/application/sub/path/");

			AssertType<UriRegistryDataType>(ItemSet.ErrorReportingServiceUri.DataType);
		}

		public void TestActivityLogMaximumPastYears()
		{
			TestRegistryItem(
				ItemSet.ActivityLogMaximumPastYears,
				"ActivityLogMaximumPastYears",
				Categories.System_Staff_ActivityLogging,
				"Activity Log Maximum Past Years",
				"Defines the maximum number of years that the details on the Activity Log of a Staff record can be searched. 0 means no time limit.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				expectedDefaultValue: 10,
				expectedMinValue: 0,
				expectedMaxValue: DateRangeValidation.MaximumPastYears);

			AssertExceptionThrown(typeof(RegistryValidationException), () => ItemSet.ActivityLogMaximumPastYears.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000));
		}

		public void TestSystemToSystemCertificate()
		{
			TestRegistryItem(
				ItemSet.SystemToSystemCertificate,
				"SystemToSystemCertificate",
				"System/System-to-System Trust",
				"System to System Certificate",
				"This value contains the System to System Trust certificate information.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.MustOverrideDefaultValue | RegistryOptions.PreserveTestValue);
		}

		public void TestSystemToSystemTrustDataProtectionMechanism()
		{
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("NON", "No data protection");
			expectedList.AddPair("ADP", "Active Directory using DPAPI-NG");
			TestRegistryItem(
				ItemSet.SystemToSystemTrustDataProtectionMechanism,
				"SystemToSystemTrustDataProtectionMechanism",
				"System/System-to-System Trust/Data Protection",
				"Data Protection Mechanism",
				@"Select the mechanism used for data protection of the System-to-System Trust private key, where:

NON - means that the data protection is not activated.
ADP - means that DPAPI-NG data protection will be used. This requires additional setup, including setting up the SystemToSystemTrustDataProtectionGroup registry item.

Note: Changing this value can break the System-to-System Trust functionality by disassociating the private key and reliant services from the group.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForCargoWise,
				expectedList,
				"NON");
		}

		public void TestSystemToSystemTrustDataProtectionGroup()
		{
			var expectedDefaultSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
			var expectedSetSid = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
			var item = ItemSet.SystemToSystemTrustDataProtectionGroup;
			AssertEquals(expectedDefaultSid, item.SecurityIdentifier);
			TestRegistryItem(
				item,
				"SystemToSystemTrustDataProtectionGroup",
				"System/System-to-System Trust/Data Protection",
				"Data Protection Group",
				@"This is the AD group used to encrypt and decrypt protected data for System-to-System Trust.

Note: Changing this value can break the System-to-System Trust functionality by disassociating the private key and reliant services from the group.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForCargoWise,
				TextEditorType.TextBox,
				string.Empty,
				"System");
			AssertEquals(expectedSetSid, item.SecurityIdentifier);
		}

		public void TestAzureApplicationClientId()
		{
			AssertEquals("Not issued", ItemSet.AzureApplicationClientId);

			var info = new SystemToSystemTrustInfo() { ClientId = Guid.NewGuid().ToString() };
			using (ItemSet.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, info))
			{
				AssertEquals(info.ClientId, ItemSet.AzureApplicationClientId);
			}
		}

		public void TestEDIClientID()
		{
			AssertEquals("EDIClientID", ItemSet.EDIClientID.Name);
			AssertEquals("EDI Client ID", ItemSet.EDIClientID.Caption);
			AssertEquals("It's a permanent id coming from Azure application which represents EDI.", ItemSet.EDIClientID.Hint);
			AssertEquals("System/System-to-System Trust", ItemSet.EDIClientID.Category);
			AssertEquals(RegistryStorageFlags.System, ItemSet.EDIClientID.Storage);
			AssertEquals("9a6ebfef-9638-4fdd-95bb-5394926c0422", ItemSet.EDIClientID.DefaultValue);
		}

		public void TestEDIClientIDOptions()
		{
			var mockReg = new Mock<IProductRegistration>();
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalDeveloperSystem()).Returns(true);
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalUATSystem()).Returns(true);

			RegistryItemDictionary.Instance.PurgeAll();
			using (ObjectFactory.Substitute(mockReg.Object))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.EDIClientID.Options);
			}

			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalDeveloperSystem()).Returns(false);
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalUATSystem()).Returns(true);

			RegistryItemDictionary.Instance.PurgeAll();
			using (ObjectFactory.Substitute(mockReg.Object))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.EDIClientID.Options);
			}

			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalDeveloperSystem()).Returns(true);
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalUATSystem()).Returns(false);

			RegistryItemDictionary.Instance.PurgeAll();
			using (ObjectFactory.Substitute(mockReg.Object))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.EDIClientID.Options);
			}

			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalDeveloperSystem()).Returns(false);
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalUATSystem()).Returns(false);

			RegistryItemDictionary.Instance.PurgeAll();
			using (ObjectFactory.Substitute(mockReg.Object))
			{
				AssertEquals(RegistryOptions.IsHidden, ItemSet.EDIClientID.Options);
			}
		}

		#region ProductivityWise

		public void TestIsPWVisible_AccountingTransactionsExport()
		{
			AssertRegistryItemVisibleWithPWEnabledAndDisabled(RegistryOptions.IsHidden, SystemDataRegistry.Instance.AccountingTransactionsExport);
		}

		public void TestIsPWVisible_AccountingTransactionsExportHighWaterMark()
		{
			AssertRegistryItemVisibleWithPWEnabledAndDisabled(RegistryOptions.IsHidden, SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark);
		}

		public void TestIsPWVisible_AccountingTransactionTypes()
		{
			AssertRegistryItemVisibleWithPWEnabledAndDisabled(RegistryOptions.IsHidden, SystemDataRegistry.Instance.AccountingTransactionTypes);
		}

		public void TestIsPWVisible_ShowInvoicePDFExportRegistrySettingsRaw()
		{
			AssertRegistryItemVisibleWithPWEnabledAndDisabled(RegistryOptions.IsHidden, SystemDataRegistry.Instance.ShowInvoicePDFExportRegistrySettingsRaw);
		}

		public void TestIsPWVisible_InvoiceTypesToExportInPDFFormat()
		{
			AssertRegistryItemVisibleWithPWEnabledAndDisabled(RegistryOptions.IsHidden, SystemDataRegistry.Instance.InvoiceTypesToExportInPDFFormat);
		}

		public void TestIsPWVisible_InvoicesInPDFFormatExportDirectory()
		{
			AssertRegistryItemVisibleWithPWEnabledAndDisabled(RegistryOptions.IsHidden, SystemDataRegistry.Instance.InvoicesInPDFFormatExportDirectory);
		}

		public void TestIsPWVisible_ARAPBalancesUpdateImportDirectoryItem()
		{
			AssertRegistryItemVisibleWithPWEnabledAndDisabled(RegistryOptions.IsHidden, SystemDataRegistry.Instance.ARAPBalancesUpdateImportDirectoryItem);
		}

		public void TestIsPWVisible_XmlImportSpecifiedElementsOnly()
		{
			AssertRegistryItemVisibleWithPWEnabledAndDisabled(RegistryOptions.IsHidden, SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly);
		}

		public void TestIsPWVisible_DefaultMessagesDataImportDirectory()
		{
			AssertRegistryItemVisibleWithPWEnabledAndDisabled(RegistryOptions.IsHidden, SystemDataRegistry.Instance.DefaultMessagesDataImportDirectory);
		}

		public void TestIsPWVisible_ActivateSystemMergeDataInterface()
		{
			AssertRegistryItemVisibleWithPWEnabledAndDisabled(RegistryOptions.IsHidden, SystemDataRegistry.Instance.ActivateSystemMergeDataInterface);
		}

		public void TestIsPWVisible_DuplicatePaymentReferenceValidationOnImportingInvoices()
		{
			AssertRegistryItemVisibleWithPWEnabledAndDisabled(RegistryOptions.IsHidden, SystemDataRegistry.Instance.DuplicatePaymentReferenceValidationOnImportingInvoices);
		}

		static void AssertRegistryItemVisibleWithPWEnabledAndDisabled(RegistryOptions refusedOption, IRegistryItem registryItem)
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			Assert("Our registry item should be visible with PW Mode enabled, and yet...", registryItem.Options != refusedOption);

			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
			Assert("Our registry item should also be visible with PW Mode disabled, and yet...", registryItem.Options != refusedOption);
		}

		#endregion

		public void TestManagerMapping_DefaultValues()
		{
			var defaultValue = ItemSet.ManagerSecurityMapping.DefaultValue;
			AssertEquals("DefaultValue.Count", 1, defaultValue.Count);
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"DRM\")", "DRM", defaultValue.GetDescriptionFromCode("MANAGERSECURITY1"));

			var roles = new StaffReportingRoleCollection()
			{
				//Code, Description, enabled, isMandatory, isSharedRole
				{ "DRM", (NoResString)"Is Managed 1", true, false, false },
				{ "PLD", (NoResString)"Human Resources Manager", true, false, true }
			};

			Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			AssertNoExceptionThrown("Default value is valid", () => ItemSet.ManagerSecurityMapping.DataType.Validate(ItemSet.ManagerSecurityMapping, defaultValue, Guid.Empty, Guid.Empty, Guid.Empty));

			var list = new DropDownCodeDescriptionBoolCollection(16, ManagerSecurityMappingDropDownProvider.MangerMappingCodeLookup, ManagerSecurityMappingDropDownProvider.MangerMappingDescriptionLookup);
			list.Add("MANAGERSECURITY3", (NoResString)"PLD");

			ItemSet.ManagerSecurityMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			AssertEquals("Value.Count", 1, ItemSet.ManagerSecurityMapping.Value.Count);
			AssertEquals("Value.GetDescriptionFromCode(\"PLD\")", "PLD", ItemSet.ManagerSecurityMapping.Value.GetDescriptionFromCode("MANAGERSECURITY3"));

			AssertEquals("ManagerSecurityMapping", ItemSet.ManagerSecurityMapping.Name);
			AssertEquals("System/Staff/HRMS", ItemSet.ManagerSecurityMapping.Category);
		}

		public void TestPersonalRelationshipsList_ContainsCodes()
		{
			var personalRelationshipsList = ItemSet.PersonalRelationshipsList;

			TestGenericRegistryItem(
				personalRelationshipsList,
				"PersonalRelationshipsList",
				"System/Staff/HRMS",
				"Personal Relationships",
				"A list of personal relationships.",
				RegistryStorageFlags.System);

			CombineAssertions(() =>
			{
				AssertEquals(personalRelationshipsList.Value.Count, 19);
				Assert(personalRelationshipsList.Value.ContainsCode("WIF"));
				Assert(personalRelationshipsList.Value.ContainsCode("HUS"));
				Assert(personalRelationshipsList.Value.ContainsCode("SON"));
				Assert(personalRelationshipsList.Value.ContainsCode("DAU"));
				Assert(personalRelationshipsList.Value.ContainsCode("PRT"));
				Assert(personalRelationshipsList.Value.ContainsCode("BRO"));
				Assert(personalRelationshipsList.Value.ContainsCode("SIS"));
				Assert(personalRelationshipsList.Value.ContainsCode("SIB"));
				Assert(personalRelationshipsList.Value.ContainsCode("MOT"));
				Assert(personalRelationshipsList.Value.ContainsCode("FAT"));
				Assert(personalRelationshipsList.Value.ContainsCode("AUN"));
				Assert(personalRelationshipsList.Value.ContainsCode("UNC"));
				Assert(personalRelationshipsList.Value.ContainsCode("NEP"));
				Assert(personalRelationshipsList.Value.ContainsCode("NIE"));
				Assert(personalRelationshipsList.Value.ContainsCode("GUR"));
				Assert(personalRelationshipsList.Value.ContainsCode("PAR"));
				Assert(personalRelationshipsList.Value.ContainsCode("GPR"));
				Assert(personalRelationshipsList.Value.ContainsCode("COU"));
				Assert(personalRelationshipsList.Value.ContainsCode("FRI"));
			});
		}

		public void TestEnableSupportUserLoginForHostedSystem()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			TestRegistryItem(ItemSet.EnableSupportUserLogin,
				"EnableSupportUserLogin",
				"System/Staff/OpenID Connect Authentication",
				(NoResString)"Enable support user login when OIDC authentication is enabled.",
				(NoResString)"Overriding this setting to Yes will enable the CW1 Support user to log into your system when OIDC authentication is enabled. To remove access, remember to set this setting back to No.",
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden,
				true);
		}

		public void TestEnableSupportUserLoginForSelfHostedSystem()
		{
			EnvProxy.SetHostedLocationForTest("NCW");

			TestRegistryItem(ItemSet.EnableSupportUserLogin,
				"EnableSupportUserLogin",
				"System/Staff/OpenID Connect Authentication",
				(NoResString)"Enable support user login when OIDC authentication is enabled.",
				(NoResString)"Overriding this setting to Yes will enable the CW1 Support user to log into your system when OIDC authentication is enabled. To remove access, remember to set this setting back to No.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController,
				true);
		}

		public void TestExportToExcelFormat()
		{
			TestGenericRegistryItem(ItemSet.ExportToExcelFormat,
				"ExportToExcelFormat",
				SystemDataRegistry.Categories.System_UI,
				"Export To Excel Format",
				"The file format of the Excel spreadsheet when Export To Excel is used. The default format is XLSX.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				ItemSet.ExportToExcelFormatOptions().DefaultCode);

			AssertEquals("ExportToExcelFormat.Value default", Core.Constants.ExportToExcelFormats.Code.Xlsx, ItemSet.ExportToExcelFormat.Value);

			ItemSet.ExportToExcelFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ExportToExcelFormats.Code.Xls);
			AssertEquals("ExportToExcelFormat.Value", Core.Constants.ExportToExcelFormats.Code.Xls, ItemSet.ExportToExcelFormat.Value);
		}

		public void TestIsOIDCFederatedWithWTG()
		{
			TestRegistryItem(ItemSet.IsOIDCFederatedWithWTG,
				"IsOIDCFederatedWithWTG",
				"System/Staff/OpenID Connect Authentication",
				"Use WTG B2C for Identity Federation",
				"When this value is \"Yes\" it indicates that WiseTech Global web apps will federate identities with the WiseTech Global Azure B2C server. Overriding the value to \"No\" will disable federated identities using the WiseTech Global B2C",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted,
				true);
		}

		public void TesteDisableLastEditUpdateOfParentInNativeXml()
			=> TestGenericRegistryItem
			(
				ItemSet.DisableLastEditUpdateOfParentInNativeXml,
				"DisableLastEditUpdateOfParentInNativeXml",
				Categories.Optimization,
				"Disable Last Edit Update Of Parent In Native XML",
				"Comma separated list of parent tables excluded from last edit update when their child tables are modified in Native XML.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				expectedDefaultValue: ""
			);

		public void TestWebServicesConfigureRegistryItem()
		{
			TestGenericRegistryItem(ItemSet.WebServicesConfigure,
				"WebServicesConfigure",
				Categories.System_WebServices,
				"Configure Web Services",
				"Configurable Web Services. Changes here will be actioned automatically by the system. \r\n\r\nNote: Web services may appear as not modifiable if they have special requirements. Please raise an eRequest if you would like to request a change.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue | RegistryOptions.IsOnlyForSupport);
		}

		public void TestWebServicesConfigureRegistryItem_NoDefinedWebServices()
		{
			var temporaryValue = new WebServicesConfigCollection();

			using (SystemDataRegistry.Instance.WebServicesConfigure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryValue))
			{
				var result = (SystemDataRegistry.Instance.WebServicesConfigure.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty))
					.Cast<WebServicesConfig>().IsNullOrEmpty();
				Assert(result);
			}
		}

		public void TestWebServicesConfigRegistryItem_WithAllOveriddenItems_ContainsAllOverriden()
		{
			var items = WebServicesConfigCollection.DefaultValue
				 .Cast<WebServicesConfig>()
				 .Select(o => new WebServicesConfig()
				 {
					 Name = o.Name,
					 IsEnabled = !o.IsEnabled,
					 IsAutoManaged = !o.IsAutoManaged,
					 IsCustomURL = o.IsCustomURL,
					 URL = o.URL,
					 NumberOfServerClusters = o.NumberOfServerClusters,
				 });

			var temporaryValue = new WebServicesConfigCollection();
			temporaryValue.AddRange(items);

			using (SystemDataRegistry.Instance.WebServicesConfigure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryValue))
			{
				var result = (SystemDataRegistry.Instance.WebServicesConfigure.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty))
					.Cast<WebServicesConfig>()
					.All(o => items
						.Any(x =>
							x.Name == o.Name
							&& x.IsEnabled == o.IsEnabled
							&& x.IsAutoManaged == o.IsAutoManaged
							&& x.IsCustomURL == o.IsCustomURL
							&& x.URL == o.URL
							&& x.NumberOfServerClusters == o.NumberOfServerClusters));

				Assert(result);
			}
		}

		public void TestWebServicesConfigRegistryItem_WithSomeOverriddenItems_ContainsOverridenAndDefaults()
		{
			var defaultValue = WebServicesConfigCollection.DefaultValue;
			var itemIndex = defaultValue.Count / 2;

			var defaultItems = Enumerable.Range(itemIndex, defaultValue.Count > 0 ? defaultValue.Count - itemIndex : 0)
				.Select(o => defaultValue[o]);

			var overriddenItems = Enumerable.Range(0, itemIndex)
				.Select(o => new WebServicesConfig()
				{
					Name = defaultValue[o].Name,
					IsEnabled = !defaultValue[o].IsEnabled,
					IsAutoManaged = !defaultValue[o].IsAutoManaged,
					IsCustomURL = defaultValue[o].IsCustomURL,
					URL = defaultValue[o].URL,
					NumberOfServerClusters = defaultValue[o].NumberOfServerClusters + 1,
				});

			var temporaryValue = new WebServicesConfigCollection();
			temporaryValue.AddRange(defaultItems);
			temporaryValue.AddRange(overriddenItems);

			using (Instance.WebServicesConfigure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryValue))
			{
				var itemsToCheck = defaultItems.Union(overriddenItems);
				var result = (SystemDataRegistry.Instance.WebServicesConfigure.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty))
					.Cast<WebServicesConfig>()
					.All(o => itemsToCheck
						.Any(x =>
							x.Name == o.Name
							&& x.IsEnabled == o.IsEnabled
							&& x.IsAutoManaged == o.IsAutoManaged
							&& x.IsCustomURL == o.IsCustomURL
							&& x.URL == o.URL
							&& x.NumberOfServerClusters == o.NumberOfServerClusters));

				Assert(result);
			}
		}

		public void TestAllowNonSupportDiagnosticsProfiling()
		{
			TestRegistryItem(
				SystemDataRegistry.Instance.AllowNonSupportDiagnosticsProfiling,
				"AllowNonSupportDiagnosticsProfiling",
				Categories.System_Diagnostics,
				"Allow Non-Support Diagnostics Profiling",
				"Setting this to yes enables non-support users on hosted systems to run dotTrace and dotMemory profiling tools.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#region SCIM

		public void TestEnableScimService()
		{
			TestRegistryItem(ItemSet.EnableScimService,
				"EnableScimService",
				"System/SCIM",
				"Enable Scim Service",
				$"Enabling this registry item will enable the SCIM functionality for this {Core.Constants.ProductName} system. This registry is used by various pieces of SCIM functionality to drive system behaviour. Only the I&S team should enable this setting because customers first need to have signed the Early Access Addendum.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestStaffColumnToGroupNamesMapping()
		{
			AssertEquals("StaffColumnToGroupNamesMapping", ItemSet.StaffColumnToGroupNamesMapping.Name);
			AssertEquals("Staff Column To Group Names Mapping", ItemSet.StaffColumnToGroupNamesMapping.Caption);
			AssertEquals("This registry setting provides the mapping between Group Descriptions and Staff Field values. Group membership provides staff with a corresponding flag value upon SCIM import or update only. If you currently have any Staff open forms, please re-open them for these changes to take effect. Modifying this registry will affect corresponding group and staff records. Saving might take several minutes.", ItemSet.StaffColumnToGroupNamesMapping.Hint);
			AssertEquals("System/SCIM", ItemSet.StaffColumnToGroupNamesMapping.Category);
			AssertEquals(RegistryStorageFlags.System, ItemSet.StaffColumnToGroupNamesMapping.Storage);
			AssertEquals(RegistryOptions.Default, ItemSet.StaffColumnToGroupNamesMapping.Options);
			AssertEquals(0, ItemSet.StaffColumnToGroupNamesMapping.DefaultValue.Count);
		}

		public void TestScimAuthenticationMethod()
		{
			var regItem = ItemSet.ScimAuthenticationMethod;

			Assert("Pre-condition: Registry Item Data Type should be CodePairRegistryDataType", regItem.DataType is CodePairRegistryDataType);
			var lookupList = ((CodePairRegistryDataType)regItem.DataType).LookUpList;

			TestGenericRegistryItem(
				regItem,
				"ScimAuthenticationMethod",
				"System/SCIM/Authentication",
				"SCIM Authentication Method",
				"Authentication method used by the SCIM listener endpoint.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController,
				"AZE");

			AssertEquals(lookupList.Count, 2);
			Assert(lookupList.ContainsCode("AZE"));
			Assert(lookupList.ContainsCode("APT"));
		}

		public void TestScimApiTokenAuthentication()
		{
			var regItem = ItemSet.ScimApiTokenAuthentication;

			Assert("Pre-condition: Registry Item Data Type should be CodePairRegistryDataType", regItem.DataType is ScimApiTokenAuthenticationDataType);

			TestGenericRegistryItem(
				regItem,
				"ScimApiTokenAuthentication",
				"System/SCIM/Authentication",
				"SCIM API Token Authentication",
				"SCIM API Token Authentication settings used by the SCIM listener endpoint.\r\nFor security reasons, the API key is only visible once when generated, a new key must be generated if previous key is lost.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController,
				"");
		}

		public void TestScimClearPrivilegeFlagsOnMatching()
		{
			TestRegistryItem(ItemSet.ScimClearPrivilegeFlagsOnMatching,
				"ScimClearPrivilegeFlagsOnMatching",
				"System/SCIM",
				"Clear Privilege Flags On Matching",
				$"If enabled, this registry will clear following flags upon matching an external user to an existing user: Is Controller, Is Database Developer, Is Database Reader, Is Backup Operator.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController,
				true);
		}

		public void TestScimReturnAllGroupMembershipsForStaff()
		{
			TestRegistryItem(ItemSet.ScimReturnAllGroupMembershipsForStaff,
				"ScimReturnAllGroupMembershipsForStaff",
				"System/SCIM",
				"Show All Group Memberships For Staff",
				$"If enabled, user.groups collection will contain all groups regardless of whether or not they were provisioned by SCIM or created locally.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController,
				false);
		}

		public void TestScimClearRoleFlagsOnMatching()
		{
			TestRegistryItem(ItemSet.ScimClearRoleFlagsOnMatching,
				"ScimClearRoleFlagsOnMatching",
				"System/SCIM",
				"Clear Role Flags On Matching",
				$"If enabled, this registry will clear following flags upon matching an external user to an existing user: Is Sales Rep, Is Driver.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController,
				false);
		}

		public void TestScimAllowLocalEditing()
		{
			TestRegistryItem(ItemSet.ScimAllowLocalEditing,
				"ScimAllowLocalEditing",
				"System/SCIM",
				"Allow Local Editing of SCIM users and groups",
				$"If enabled, users and groups originating from an external Identity Provider and provisioned by SCIM become locally editable. Please note, this is not recommended unless your external Identity Provider is able to perform a sync via available SCIM endpoints. If not synchronised back to the Identity Provider, any future provisions may override local changes and cause data inconsistency.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController,
				false);
		}
		
		public void TestStaffColumnToGroupNamesMapping_GroupAdded_CanLogin()
		{
			AssertStaffColumnToGroupNamesMapping_GroupAdded(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.CanLogin);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupAdded_IsController()
		{
			AssertStaffColumnToGroupNamesMapping_GroupAdded(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsController);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupAdded_IsRobot()
		{
			AssertStaffColumnToGroupNamesMapping_GroupAdded(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsRobot);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupAdded_IsDevice()
		{
			AssertStaffColumnToGroupNamesMapping_GroupAdded(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDevice);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupAdded_IsDriver()
		{
			AssertStaffColumnToGroupNamesMapping_GroupAdded(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDriver);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupAdded_IsSalesRep()
		{
			AssertStaffColumnToGroupNamesMapping_GroupAdded(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsSalesRep);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupAdded_IsDatabaseDeveloper()
		{
			AssertStaffColumnToGroupNamesMapping_GroupAdded(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDatabaseDeveloper);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupAdded_IsReadOnlyDBUser()
		{
			AssertStaffColumnToGroupNamesMapping_GroupAdded(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsReadOnlyDBUser);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupAdded_IsBackupOperator()
		{
			AssertStaffColumnToGroupNamesMapping_GroupAdded(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsBackupOperator);
		}

		void AssertStaffColumnToGroupNamesMapping_GroupAdded(string column)
		{
			SetupGroupAndStaff();
			var collection = new StaffColumnToGroupDescriptionScimMappingCollection
			{
				new StaffColumnToGroupDescriptionScimMapping()
				{
					GroupDescriptionMapping = "12345",
					StaffColumnName = column
				}
			};

			AssertEquals(ZBool.False, staffScim1[column]);
			AssertEquals(ZBool.False, staffScim2[column]);
			AssertEquals(ZBool.False, staffScim3[column]);

			ItemSet.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			ItemSet.StaffColumnToGroupNamesMapping.OnUpdate(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedStaff1 = newFactory.Load<IGlbStaff>(staffScim1.PK) as BusinessObject;
			var loadedStaff2 = newFactory.Load<IGlbStaff>(staffScim2.PK) as BusinessObject;
			var loadedStaff3 = newFactory.Load<IGlbStaff>(staffScim3.PK) as BusinessObject;
			AssertEquals(ZBool.True, loadedStaff1[column]);
			AssertEquals(ZBool.False, loadedStaff2[column]);
			AssertEquals(ZBool.False, loadedStaff3[column]);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupRemoved_CanLogin()
		{
			AssertStaffColumnToGroupNamesMapping_GroupRemoved(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.CanLogin);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupRemoved_IsRobot()
		{
			AssertStaffColumnToGroupNamesMapping_GroupRemoved(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsRobot);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupRemoved_IsDevice()
		{
			AssertStaffColumnToGroupNamesMapping_GroupRemoved(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDevice);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupRemoved_IsDriver()
		{
			AssertStaffColumnToGroupNamesMapping_GroupRemoved(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDriver);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupRemoved_IsSalesRep()
		{
			AssertStaffColumnToGroupNamesMapping_GroupRemoved(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsSalesRep);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupRemoved_IsDatabaseDeveloper()
		{
			AssertStaffColumnToGroupNamesMapping_GroupRemoved(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDatabaseDeveloper);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupRemoved_IsReadOnlyDBUser()
		{
			AssertStaffColumnToGroupNamesMapping_GroupRemoved(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsReadOnlyDBUser);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupRemoved_IsBackupOperator()
		{
			AssertStaffColumnToGroupNamesMapping_GroupRemoved(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsBackupOperator);
		}

		void AssertStaffColumnToGroupNamesMapping_GroupRemoved(string column)
		{
			SetupGroupAndStaff();
			var collection = new StaffColumnToGroupDescriptionScimMappingCollection
			{
				new StaffColumnToGroupDescriptionScimMapping()
				{
					GroupDescriptionMapping = "12345",
					StaffColumnName = column
				}
			};

			AssertEquals(ZBool.False, staffScim1[column]);
			AssertEquals(ZBool.False, staffScim2[column]);
			AssertEquals(ZBool.False, staffScim3[column]);

			ItemSet.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			ItemSet.StaffColumnToGroupNamesMapping.OnUpdate(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedStaff1 = newFactory.Load<IGlbStaff>(staffScim1.PK) as BusinessObject;
			var loadedStaff2 = newFactory.Load<IGlbStaff>(staffScim2.PK) as BusinessObject;
			var loadedStaff3 = newFactory.Load<IGlbStaff>(staffScim3.PK) as BusinessObject;

			AssertEquals(ZBool.True,  loadedStaff1[column]);
			AssertEquals(ZBool.False, loadedStaff2[column]);
			AssertEquals(ZBool.False, loadedStaff3[column]);

			collection = new StaffColumnToGroupDescriptionScimMappingCollection();

			ItemSet.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			ItemSet.StaffColumnToGroupNamesMapping.OnUpdate(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			loadedStaff1 = newFactory.Load<IGlbStaff>(staffScim1.PK) as BusinessObject;
			loadedStaff2 = newFactory.Load<IGlbStaff>(staffScim2.PK) as BusinessObject;
			loadedStaff3 = newFactory.Load<IGlbStaff>(staffScim3.PK) as BusinessObject;
			AssertEquals(ZBool.False, loadedStaff1[column]);
			AssertEquals(ZBool.False, loadedStaff2[column]);
			AssertEquals(ZBool.False, loadedStaff3[column]);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupChanged_CanLogin()
		{
			AssertStaffColumnToGroupNamesMapping_GroupChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.CanLogin);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupChanged_IsRobot()
		{
			AssertStaffColumnToGroupNamesMapping_GroupChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsRobot);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupChanged_IsDevice()
		{
			AssertStaffColumnToGroupNamesMapping_GroupChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDevice);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupChanged_IsDriver()
		{
			AssertStaffColumnToGroupNamesMapping_GroupChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDriver);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupChanged_IsSalesRep()
		{
			AssertStaffColumnToGroupNamesMapping_GroupChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsSalesRep);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupChanged_IsDatabaseDeveloper()
		{
			AssertStaffColumnToGroupNamesMapping_GroupChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDatabaseDeveloper);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupChanged_IsReadOnlyDBUser()
		{
			AssertStaffColumnToGroupNamesMapping_GroupChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsReadOnlyDBUser);
		}

		public void TestStaffColumnToGroupNamesMapping_GroupChanged_IsBackupOperator()
		{
			AssertStaffColumnToGroupNamesMapping_GroupChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsBackupOperator);
		}

		void AssertStaffColumnToGroupNamesMapping_GroupChanged(string column)
		{
			SetupGroupAndStaff();
			var collection = new StaffColumnToGroupDescriptionScimMappingCollection
			{
				new StaffColumnToGroupDescriptionScimMapping()
				{
					GroupDescriptionMapping = "12345",
					StaffColumnName = column
				}
			};

			AssertEquals(ZBool.False, staffScim1[column]);
			AssertEquals(ZBool.False, staffScim2[column]);
			AssertEquals(ZBool.False, staffScim3[column]);

			ItemSet.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			ItemSet.StaffColumnToGroupNamesMapping.OnUpdate(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedStaff1 = newFactory.Load<IGlbStaff>(staffScim1.PK) as BusinessObject;
			var loadedStaff2 = newFactory.Load<IGlbStaff>(staffScim2.PK) as BusinessObject;
			var loadedStaff3 = newFactory.Load<IGlbStaff>(staffScim3.PK) as BusinessObject;

			AssertEquals(ZBool.True,  loadedStaff1[column]);
			AssertEquals(ZBool.False, loadedStaff2[column]);
			AssertEquals(ZBool.False, loadedStaff3[column]);

			collection = new StaffColumnToGroupDescriptionScimMappingCollection
			{
				new StaffColumnToGroupDescriptionScimMapping()
				{
					GroupDescriptionMapping = "6789",
					StaffColumnName = column
				}
			};

			ItemSet.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			ItemSet.StaffColumnToGroupNamesMapping.OnUpdate(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			loadedStaff1 = newFactory.Load<IGlbStaff>(staffScim1.PK) as BusinessObject;
			loadedStaff2 = newFactory.Load<IGlbStaff>(staffScim2.PK) as BusinessObject;
			loadedStaff3 = newFactory.Load<IGlbStaff>(staffScim3.PK) as BusinessObject;

			AssertEquals(ZBool.False, loadedStaff1[column]);
			AssertEquals(ZBool.False, loadedStaff2[column]);
			AssertEquals(ZBool.True, loadedStaff3[column]);
		}

		public void TestStaffColumnToGroupNamesMapping_ColumnChanged_CanLogin()
		{
			AssertStaffColumnToGroupNamesMapping_ColumnChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.CanLogin, StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsController);
		}

		public void TestStaffColumnToGroupNamesMapping_ColumnChanged_IsRobot()
		{
			AssertStaffColumnToGroupNamesMapping_ColumnChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsRobot, StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDevice);
		}

		public void TestStaffColumnToGroupNamesMapping_ColumnChanged_IsDevice()
		{
			AssertStaffColumnToGroupNamesMapping_ColumnChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDevice, StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDriver);
		}

		public void TestStaffColumnToGroupNamesMapping_ColumnChanged_IsDriver()
		{
			AssertStaffColumnToGroupNamesMapping_ColumnChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDriver, StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsSalesRep);
		}

		public void TestStaffColumnToGroupNamesMapping_ColumnChanged_IsSalesRep()
		{
			AssertStaffColumnToGroupNamesMapping_ColumnChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsSalesRep, StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDatabaseDeveloper);
		}

		public void TestStaffColumnToGroupNamesMapping_ColumnChanged_IsDatabaseDeveloper()
		{
			AssertStaffColumnToGroupNamesMapping_ColumnChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDatabaseDeveloper, StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsReadOnlyDBUser);
		}

		public void TestStaffColumnToGroupNamesMapping_ColumnChanged_IsReadOnlyDBUser()
		{
			AssertStaffColumnToGroupNamesMapping_ColumnChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsReadOnlyDBUser, StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsBackupOperator);
		}

		public void TestStaffColumnToGroupNamesMapping_ColumnChanged_IsBackupOperator()
		{
			AssertStaffColumnToGroupNamesMapping_ColumnChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsBackupOperator, StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.CanLogin);
		}

		void AssertStaffColumnToGroupNamesMapping_ColumnChanged(string column1, string column2)
		{
			SetupGroupAndStaff();
			var collection = new StaffColumnToGroupDescriptionScimMappingCollection
			{
				new StaffColumnToGroupDescriptionScimMapping()
				{
					GroupDescriptionMapping = "12345",
					StaffColumnName = column1
				}
			};

			AssertEquals(ZBool.False, staffScim1[column1]);
			AssertEquals(ZBool.False, staffScim2[column1]);
			AssertEquals(ZBool.False, staffScim3[column1]);
			AssertEquals(ZBool.False, staffScim1[column2]);
			AssertEquals(ZBool.False, staffScim2[column2]);
			AssertEquals(ZBool.False, staffScim3[column2]);

			ItemSet.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			ItemSet.StaffColumnToGroupNamesMapping.OnUpdate(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedStaff1 = newFactory.Load<IGlbStaff>(staffScim1.PK) as BusinessObject;
			var loadedStaff2 = newFactory.Load<IGlbStaff>(staffScim2.PK) as BusinessObject;
			var loadedStaff3 = newFactory.Load<IGlbStaff>(staffScim3.PK) as BusinessObject;

			AssertEquals(ZBool.True,  loadedStaff1[column1]);
			AssertEquals(ZBool.False, loadedStaff2[column1]);
			AssertEquals(ZBool.False, loadedStaff3[column1]);
			AssertEquals(ZBool.False, loadedStaff1[column2]);
			AssertEquals(ZBool.False, loadedStaff2[column2]);
			AssertEquals(ZBool.False, loadedStaff3[column2]);

			collection = new StaffColumnToGroupDescriptionScimMappingCollection
			{
				new StaffColumnToGroupDescriptionScimMapping()
				{
					GroupDescriptionMapping = "12345",
					StaffColumnName = column2
				}
			};

			ItemSet.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			ItemSet.StaffColumnToGroupNamesMapping.OnUpdate(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			loadedStaff1 = newFactory.Load<IGlbStaff>(staffScim1.PK) as BusinessObject;
			loadedStaff2 = newFactory.Load<IGlbStaff>(staffScim2.PK) as BusinessObject;
			loadedStaff3 = newFactory.Load<IGlbStaff>(staffScim3.PK) as BusinessObject;

			AssertEquals(ZBool.False, loadedStaff1[column1]);
			AssertEquals(ZBool.False, loadedStaff2[column1]);
			AssertEquals(ZBool.False, loadedStaff3[column1]);
			AssertEquals(ZBool.True,  loadedStaff1[column2]);
			AssertEquals(ZBool.False, loadedStaff2[column2]);
			AssertEquals(ZBool.False, loadedStaff3[column2]);
		}

		public void TestStaffColumnToGroupNamesMapping_ColumnAndGroupChanged_CanLogin()
		{
			AssertStaffColumnToGroupNamesMapping_ColumnAndGroupChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.CanLogin, StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsController);
		}

		public void TestStaffColumnToGroupNamesMapping_ColumnAndGroupChanged_IsRobot()
		{
			AssertStaffColumnToGroupNamesMapping_ColumnAndGroupChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsRobot, StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDevice);
		}

		public void TestStaffColumnToGroupNamesMapping_ColumnAndGroupChanged_IsDevice()
		{
			AssertStaffColumnToGroupNamesMapping_ColumnAndGroupChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDevice, StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDriver);
		}

		public void TestStaffColumnToGroupNamesMapping_ColumnAndGroupChanged_IsDriver()
		{
			AssertStaffColumnToGroupNamesMapping_ColumnAndGroupChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDriver, StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsSalesRep);
		}

		public void TestStaffColumnToGroupNamesMapping_ColumnAndGroupChanged_IsSalesRep()
		{
			AssertStaffColumnToGroupNamesMapping_ColumnAndGroupChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsSalesRep, StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDatabaseDeveloper);
		}

		public void TestStaffColumnToGroupNamesMapping_ColumnAndGroupChanged_IsDatabaseDeveloper()
		{
			AssertStaffColumnToGroupNamesMapping_ColumnAndGroupChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDatabaseDeveloper, StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsReadOnlyDBUser);
		}

		public void TestStaffColumnToGroupNamesMapping_ColumnAndGroupChanged_IsReadOnlyDBUser()
		{
			AssertStaffColumnToGroupNamesMapping_ColumnAndGroupChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsReadOnlyDBUser, StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsBackupOperator);
		}

		public void TestStaffColumnToGroupNamesMapping_ColumnAndGroupChanged_IsBackupOperator()
		{
			AssertStaffColumnToGroupNamesMapping_ColumnAndGroupChanged(StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsBackupOperator, StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.CanLogin);
		}

		void AssertStaffColumnToGroupNamesMapping_ColumnAndGroupChanged(string column1, string column2)
		{
			SetupGroupAndStaff();
			var collection = new StaffColumnToGroupDescriptionScimMappingCollection
			{
				new StaffColumnToGroupDescriptionScimMapping()
				{
					GroupDescriptionMapping = "12345",
					StaffColumnName = column1
				}
			};

			AssertEquals(ZBool.False, staffScim1[column1]);
			AssertEquals(ZBool.False, staffScim2[column1]);
			AssertEquals(ZBool.False, staffScim3[column1]);
			AssertEquals(ZBool.False, staffScim1[column2]);
			AssertEquals(ZBool.False, staffScim2[column2]);
			AssertEquals(ZBool.False, staffScim3[column2]);

			ItemSet.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			ItemSet.StaffColumnToGroupNamesMapping.OnUpdate(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedStaff1 = newFactory.Load<IGlbStaff>(staffScim1.PK) as BusinessObject;
			var loadedStaff2 = newFactory.Load<IGlbStaff>(staffScim2.PK) as BusinessObject;
			var loadedStaff3 = newFactory.Load<IGlbStaff>(staffScim3.PK) as BusinessObject;

			AssertEquals(ZBool.True,  loadedStaff1[column1]);
			AssertEquals(ZBool.False, loadedStaff2[column1]);
			AssertEquals(ZBool.False, loadedStaff3[column1]);
			AssertEquals(ZBool.False, loadedStaff1[column2]);
			AssertEquals(ZBool.False, loadedStaff2[column2]);
			AssertEquals(ZBool.False, loadedStaff3[column2]);

			collection = new StaffColumnToGroupDescriptionScimMappingCollection
			{
				new StaffColumnToGroupDescriptionScimMapping()
				{
					GroupDescriptionMapping = "6789",
					StaffColumnName = column2
				}
			};

			ItemSet.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			ItemSet.StaffColumnToGroupNamesMapping.OnUpdate(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			loadedStaff1 = newFactory.Load<IGlbStaff>(staffScim1.PK) as BusinessObject;
			loadedStaff2 = newFactory.Load<IGlbStaff>(staffScim2.PK) as BusinessObject;
			loadedStaff3 = newFactory.Load<IGlbStaff>(staffScim3.PK) as BusinessObject;

			AssertEquals(ZBool.False, loadedStaff1[column1]);
			AssertEquals(ZBool.False, loadedStaff2[column1]);
			AssertEquals(ZBool.False, loadedStaff3[column1]);
			AssertEquals(ZBool.False, loadedStaff1[column2]);
			AssertEquals(ZBool.False, loadedStaff2[column2]);
			AssertEquals(ZBool.True, loadedStaff3[column2]);
		}

		public void TestStaffColumnToGroupNamesMapping_MultipleMappings()
		{
			SetupGroupAndStaff();
			var collection = new StaffColumnToGroupDescriptionScimMappingCollection
			{
				new StaffColumnToGroupDescriptionScimMapping()
				{
					GroupDescriptionMapping = "12345",
					StaffColumnName = GlbStaffSchema.Constants.GS_CanLogin
				},
				new StaffColumnToGroupDescriptionScimMapping()
				{
					GroupDescriptionMapping = "12345",
					StaffColumnName = GlbStaffSchema.Constants.GS_IsController
				},
				new StaffColumnToGroupDescriptionScimMapping()
				{
					GroupDescriptionMapping = "12345",
					StaffColumnName = StaffColumnToGroupDescriptionScimMappingLookups.StaffColumn.IsDatabaseDeveloper
				}
			};

			AssertEquals(ZBool.False, staffScim1[GlbStaffSchema.GS_CanLogin]);
			AssertEquals(ZBool.False, staffScim2[GlbStaffSchema.GS_CanLogin]);
			AssertEquals(ZBool.False, staffScim3[GlbStaffSchema.GS_CanLogin]);
			AssertEquals(ZBool.False, staffScim1[GlbStaffSchema.GS_IsController]);
			AssertEquals(ZBool.False, staffScim2[GlbStaffSchema.GS_IsController]);
			AssertEquals(ZBool.False, staffScim3[GlbStaffSchema.GS_IsController]);

			AssertExceptionThrown<RegistryValidationException>(delegate
			{
				ItemSet.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			});
		}

		BusinessObject groupScim1;
		BusinessObject groupScim2;
		BusinessObject staffScim1;
		BusinessObject staffScim2;
		BusinessObject staffScim3;

		void SetupGroupAndStaff()
		{
			groupScim1 = Factory.New(ObjectFactory.GetType(typeof(IGlbGroup)));
			groupScim1[GlbGroupSchema.GG_Code] = "~g1";
			groupScim1[GlbGroupSchema.GG_Desc] = "12345";
			groupScim1[GlbGroupSchema.GG_ExternalId] = "group1";

			groupScim2 = Factory.New(ObjectFactory.GetType(typeof(IGlbGroup)));
			groupScim2[GlbGroupSchema.GG_Code] = "~g2";
			groupScim2[GlbGroupSchema.GG_Desc] = "6789";
			groupScim2[GlbGroupSchema.GG_ExternalId] = "group2";

			staffScim1 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbStaff)));
			staffScim1[GlbStaffSchema.GS_ExternalId] = "staff1";
			staffScim1[GlbStaffSchema.GS_CanLogin] = false;

			staffScim2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbStaff)));
			staffScim2[GlbStaffSchema.GS_ExternalId] = "";
			staffScim2[GlbStaffSchema.GS_CanLogin] = false;

			staffScim3 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbStaff)));
			staffScim3[GlbStaffSchema.GS_ExternalId] = "staff3";
			staffScim3[GlbStaffSchema.GS_CanLogin] = false;

			(groupScim1 as IGlbGroup).Staff.Add(staffScim1);
			(groupScim1 as IGlbGroup).Staff.Add(staffScim2);
			(groupScim2 as IGlbGroup).Staff.Add(staffScim3);

			Factory.Save();
		}

		public void TestScimSafelistType()
		{
			var regItem = ItemSet.ScimSafeListType;

			Assert("Pre-condition: Registry Item Data Type should be CodePairRegistryDataType", regItem.DataType is CodePairRegistryDataType);
			var lookupList = ((CodePairRegistryDataType)regItem.DataType).LookUpList;

			TestGenericRegistryItem(
				regItem,
				"ScimSafelistType",
				"System/SCIM",
				"Safelist Type",
				"Safelist Type used by SCIM service",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController,
				"NON");

			AssertEquals(lookupList.Count, 2);
			Assert(lookupList.ContainsCode("NON"));
			Assert(lookupList.ContainsCode("AZE"));
		}
		#endregion
	}
}
