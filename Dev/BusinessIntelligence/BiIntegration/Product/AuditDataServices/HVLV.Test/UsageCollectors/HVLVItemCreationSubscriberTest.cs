using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.AuditDataServices.HVLV.Subscribers;
using Enterprise.eTail.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.eTail.Integration.HVLVConstants;

namespace Enterprise.AuditDataServices.HVLV.Test
{
	[TestedType(typeof(HVLVItemCreationSubscriber))]
	public class HVLVItemCreationSubscriberTest : HVLVUsageSubcriberTest
	{
		public void TestHVLVItemCreationUsageSubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			CombineAssertions("Subscriber should be configured correctly", () =>
			{
				AssertEquals("Code", "HAI", subscriber.Code);
				AssertEquals("Description", "HVLV Items Usage Subscriber", subscriber.Description);
				AssertEquals("Table Name", HVLVItemSchema.Constants.TableName, subscriber.Table.TableName);
				AssertEquals("Notify Insert", true, subscriber.NotifyInsert);
				AssertEquals("Notify Update", false, subscriber.NotifyUpdate);
				AssertEquals("Notify Delete", false, subscriber.NotifyDelete);
				AssertNull("Specific Columns", subscriber.SpecificColumns);
			});
		}

		public void TestHVLVItemCreationUsageSubscriberCannotListenToUpdate()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertEquals("HVLVItemCreationUsageSubscriber cannot listen to Update operation, otherwise it will conflict with HVLVItemLastUsageCodeSubscriber. They share the field HVI_LastUsageCode.", false, subscriber.NotifyUpdate);
		}

		public void TestSubscriberInsertsNewRecordIntoUsageTable_ItemAddedByUserInCW1()
		{
			var factory = new BusinessObjectFactory();
			SetupBranchAndStaff(factory);

			var item = factory.NewWithValidTestData<HVLVItem>();
			factory.Save();

			var changeTable = GetTestDataTable();
			CreateHVIChangeTableRow(changeTable, item.PK, userCode: "STF", branchCode: "CPU");

			var subscriber = NewDataChangeSubscriber();
			var logger = new LoggerForTest();
			subscriber.ProcessChanges(logger, changeTable);

			CombineAssertions("HVLVUsage records should be populated correctly", () =>
			{
				AssertEquals("Should insert 1 records in HVLVUsage table", 1, GetRecordCountFromHXUTable());
				AssertHXURecords(new[]
				{
					new HXURecord(item.PK.ToString(), "CW1", "CWU", "STF", "INT", "CPU"),
				});
				AssertContainsExactElementsInAnyOrder(new[]
				{
					"1 creation usage(s) of HVLV Items have been collected, populating usage table if required...",
					"1 usage(s) of CWU have been recorded."
				}, logger.LogEntries);
			});
		}

		public void TestSubscriberInsertsNewRecordIntoUsageTable_ItemAddedFromDataImport()
		{
			var factory = new BusinessObjectFactory();
			SetupBranchAndStaff(factory);

			var item = factory.NewWithValidTestData<HVLVItem>();

			factory.Save();

			var changeTable = GetTestDataTable();
			CreateHVIChangeTableRow(changeTable, item.PK, userCode: "~AD", branchCode: "CPU");

			var subscriber = NewDataChangeSubscriber();
			var logger = new LoggerForTest();

			subscriber.ProcessChanges(logger, changeTable);

			CombineAssertions("HVLVUsage records should be populated correctly", () =>
			{
				AssertEquals("Should insert 1 records in HVLVUsage table", 1, GetRecordCountFromHXUTable());
				AssertHXURecords(new[]
				{
					new HXURecord(item.PK.ToString(), "CW1", "CWU", "~AD", "INT", "CPU"),
				});
				AssertContainsExactElementsInAnyOrder(new[]
				{
					"1 creation usage(s) of HVLV Items have been collected, populating usage table if required...",
					"1 usage(s) of CWU have been recorded."
				}, logger.LogEntries);
			});
		}

		public void TestSubscriberInsertsNewRecordIntoUsageTable_ItemAddedInGLOWByStaff()
		{
			var factory = new BusinessObjectFactory();
			SetupBranchAndStaff(factory);

			var company = factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "IN2";

			var branch = factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_Code = "CP2";

			var item = factory.NewWithValidTestData<HVLVItem>();
			factory.Save();

			CreateAndSaveLog(factory, "ADD", item, "CP2");

			var changeTable = GetTestDataTable();
			CreateHVIChangeTableRow(changeTable, item.PK,userCode: "STF", branchCode: "");

			var subscriber = NewDataChangeSubscriber();
			var logger = new LoggerForTest();
			subscriber.ProcessChanges(logger, changeTable);

			CombineAssertions("HVLVUsage records should be populated correctly", () =>
			{
				AssertEquals("Should insert 1 records in HVLVUsage table", 1, GetRecordCountFromHXUTable());
				AssertHXURecords(new[]
				{
					new HXURecord(item.PK.ToString(), "WEB", "GLS", "STF", "IN2", "CP2"),
				});
				AssertContainsExactElementsInAnyOrder(new[]
				{
					"1 creation usage(s) of HVLV Items have been collected, populating usage table if required...",
					"1 usage(s) of GLS have been recorded."
				}, logger.LogEntries);
			});
		}

		public void TestSubscriberInsertsNewRecordIntoUsageTable_ItemAddedInGLOWByContact()
		{
			var factory = new BusinessObjectFactory();
			SetupBranchAndStaff(factory);

			var item = factory.NewWithValidTestData<HVLVItem>();
			factory.Save();

			CreateAndSaveLog(factory, "ADD", item, "CPU");

			var changeTable = GetTestDataTable();
			CreateHVIChangeTableRow(changeTable, item.PK, userCode: "ZZ", branchCode: "");

			var subscriber = NewDataChangeSubscriber();
			var logger = new LoggerForTest();
			subscriber.ProcessChanges(logger, changeTable);

			CombineAssertions("HVLVUsage records should be populated correctly", () =>
			{
				AssertEquals("Should insert 1 records in HVLVUsage table", 1, GetRecordCountFromHXUTable());
				AssertHXURecords(new[]
				{
					new HXURecord(item.PK.ToString(), "WEB", "GLC", "ZZ", "INT", "CPU"),
				});
				AssertContainsExactElementsInAnyOrder(new[]
				{
					"1 creation usage(s) of HVLV Items have been collected, populating usage table if required...",
					"1 usage(s) of GLC have been recorded."
				}, logger.LogEntries);
			});
		}

		public void TestReportErrorIfBranchIsNull()
		{
			ErrorReporter.Clear();

			var factory = new BusinessObjectFactory();

			var item = factory.NewWithValidTestData<HVLVItem>();
			factory.Save();

			var changeTable = GetTestDataTable();
			var logPK = CreateHVIChangeTableRow(changeTable, item.PK, userCode: "XX", branchCode: "");

			var subscriber = NewDataChangeSubscriber();
			var logger = new LoggerForTest();

			CombineAssertions("Should report error when log branch is unknown", () =>
			{
				AssertNoExceptionThrown(() => subscriber.ProcessChanges(logger, changeTable));
				AssertEquals("Should insert no record in HVLVUsage table", 0, GetRecordCountFromHXUTable());
				AssertContainsExactElementsInAnyOrder(new[] { "1 creation usage(s) of HVLV Items have been collected, populating usage table if required..." }, logger.LogEntries);

				var expectedMessageBeginning = $@"An item creation usage has been skipped as the branch for HVI_SystemCreateUser was unknown.
-------DEBUG INFORMATION-------
Values from the HVLVItem record being processed when error occured:
  Item PK: {item.PK}
  User: XX";
				AssertContains(expectedMessageBeginning, ErrorReporter.LastMessageReported);
			});

			ErrorReporter.Clear();
		}

		public void TestSubscriberHandlesDeferredUpdate_WhenHVI_LastUsageCodeDoesNotMatchABranchAndUsageAlreadyExists_NoErrorReported()
		{
			var factory = new BusinessObjectFactory();

			var item1 = factory.NewWithValidTestData<HVLVItem>();
			var item2 = factory.NewWithValidTestData<HVLVItem>();
			var item3 = factory.NewWithValidTestData<HVLVItem>();
			factory.Save();

			PopulateHVLVUsageRow(item1.PK, "XX", UsageCodes.GlowContactAddUsage);
			PopulateHVLVUsageRow(item2.PK, "XX", UsageCodes.GlowStaffAddUsage);
			PopulateHVLVUsageRow(item3.PK, "XX", UsageCodes.CargoWiseUsage);

			var initialRecordCountFromHXUTable = GetRecordCountFromHXUTable();

			var changeTable = GetTestDataTable();
			CreateHVIChangeTableRow(changeTable, item1.PK, userCode: "XX", branchCode: "");
			CreateHVIChangeTableRow(changeTable, item2.PK, userCode: "XX", branchCode: "");
			CreateHVIChangeTableRow(changeTable, item3.PK, userCode: "XX", branchCode: "");

			var subscriber = NewDataChangeSubscriber();
			var logger = new LoggerForTest();

			CombineAssertions("Should Not report error when log branch is unknown but any Create usage code exists for PK", () =>
			{
				AssertNoExceptionThrown(() => subscriber.ProcessChanges(logger, changeTable));
				AssertEquals("Should insert no record in HVLVUsage table", initialRecordCountFromHXUTable, GetRecordCountFromHXUTable());
				AssertContainsExactElementsInAnyOrder(new[] { "3 creation usage(s) of HVLV Items have been collected, populating usage table if required..." }, logger.LogEntries);

				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			});
		}

		public void TestSubscriberHandlesDeferredUpdate_WhenHVI_LastUsageCodeMatchesABranch_NoNewRecordInUsageTable()
		{
			var factory = new BusinessObjectFactory();

			var company = factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "IN2";

			var branch = factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_Code = UsageCodes.NZAirCRE;

			var item1 = factory.NewWithValidTestData<HVLVItem>();
			var item2 = factory.NewWithValidTestData<HVLVItem>();
			var item3 = factory.NewWithValidTestData<HVLVItem>();
			factory.Save();

			PopulateHVLVUsageRow(item1.PK, "XX", UsageCodes.GlowContactAddUsage);
			PopulateHVLVUsageRow(item2.PK, "XX", UsageCodes.GlowStaffAddUsage);
			PopulateHVLVUsageRow(item3.PK, "XX", UsageCodes.CargoWiseUsage);

			var initialRecordCountFromHXUTable = GetRecordCountFromHXUTable();

			var changeTable = GetTestDataTable();
			CreateHVIChangeTableRow(changeTable, item1.PK, userCode: "XX", branchCode: UsageCodes.NZAirCRE);
			CreateHVIChangeTableRow(changeTable, item2.PK, userCode: "XX", branchCode: UsageCodes.NZAirCRE);
			CreateHVIChangeTableRow(changeTable, item3.PK, userCode: "XX", branchCode: UsageCodes.NZAirCRE);

			var subscriber = NewDataChangeSubscriber();
			var logger = new LoggerForTest();

			CombineAssertions("Should Not insert new records when any Creation usage code exists for PK", () =>
			{
				AssertNoExceptionThrown(() => subscriber.ProcessChanges(logger, changeTable));
				AssertEquals("Should insert no record in HVLVUsage table", initialRecordCountFromHXUTable, GetRecordCountFromHXUTable());
				AssertContainsExactElementsInAnyOrder(new[] { "3 creation usage(s) of HVLV Items have been collected, populating usage table if required..." }, logger.LogEntries);

				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			});
		}

		public override void TestCustomFilter()
		{
			var subscriber = new HVLVItemCreationSubscriber();

			var table = GetTestDataTable();

			CreateHVIChangeTableRow(table, Guid.NewGuid(), userCode: User.ServiceUserCode, branchCode: "YYY");
			CreateHVIChangeTableRow(table, Guid.NewGuid(), userCode: User.SupportUserCode, branchCode: "YYY");
			CreateHVIChangeTableRow(table, Guid.NewGuid(), userCode: User.UnKnownUserCode, branchCode: "YYY");
			var rowPk = CreateHVIChangeTableRow(table, Guid.NewGuid(), userCode: User.WebUserCode, branchCode: "YYY");
			table.AcceptChanges();

			for (var i = 0; i < 4; i++)
			{
				RunCustomFilter(table.Rows[i], subscriber);
			}
			table.AcceptChanges();

			AssertEquals(1, table.Rows.Count);
			AssertEquals(rowPk, table.Rows[0][HVLVItemSchema.Constants.PK]);
		}

		#region Implementation

		static void CreateAndSaveLog(BusinessObjectFactory factory, ZString logEvent, BusinessObject parent, string branchCode = "")
		{
			var log = parent.GetLogs().AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = logEvent;
				log.SL_Parent = parent.PK;
				log.SL_GB_NKBranch = branchCode;
			}

			factory.Save();
		}

		static void SetupBranchAndStaff(BusinessObjectFactory factory)
		{
			var company = factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "INT";

			var branch = factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_Code = "CPU";

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "STF";
			staff.GS_GB_HomeBranch = branch.PK;

			factory.Save();
		}

		protected override DataTable GetTestDataTable()
		{
			var logTable = new DataTable();
			logTable.Columns.Add(HVLVItemSchema.Constants.PK, typeof(ZGuid));
			logTable.Columns.Add(HVLVItemSchema.Constants.HVI_SystemCreateUser, typeof(string));
			logTable.Columns.Add(HVLVItemSchema.Constants.HVI_ClusterKey, typeof(int));
			logTable.Columns.Add(HVLVItemSchema.Constants.HVI_LastUsageCode, typeof(string));

			return logTable;
		}

		ZGuid CreateHVIChangeTableRow(DataTable dataTable, ZGuid itemPK, ZString userCode, ZString branchCode)
		{
			var row = dataTable.NewRow();
			row[HVLVItemSchema.Constants.PK] = itemPK;
			row[HVLVItemSchema.Constants.HVI_SystemCreateUser] = userCode;
			row[HVLVItemSchema.Constants.HVI_LastUsageCode] = branchCode;

			dataTable.Rows.Add(row);
			return (ZGuid)row[HVLVItemSchema.Constants.PK];
		}

		void AssertHXURecords(HXURecord[] expectedRecords)
		{
			var hxuRecords = new List<HXURecord>();
			TestConnection.ExecuteReader("SELECT * FROM dbo.HVLVUsage", row =>
			{
				hxuRecords.Add(new HXURecord(
					row["HXU_HVI_ParentItem"].ToString(),
					row["HXU_Category"].ToString(),
					row["HXU_Code"].ToString(),
					row["HXU_GS_NKUser"].ToString(),
					row["HXU_GC_NKCompany"].ToString(),
					row["HXU_BranchCode"].ToString()
				));
			});

			AssertContainsExactElementsInAnyOrder(record =>
			{
				return $"ItemPK: {record.ItemPK} Usage Category: {record.UsageCatetory} UsageCode: {record.UsageCode} UserCode: {record.UserCode} CompanyCode: {record.CompanyCode} BranchCode: {record.BranchCode}";
			}, expectedRecords, hxuRecords);
		}

		void PopulateHVLVUsageRow(ZGuid itemPK, ZString companyCode, ZString usageCode)
		{
			var sql = $@"
INSERT INTO dbo.HVLVUsage (HXU_PK, HXU_HVI_ParentItem, HXU_Category, HXU_GS_NKUser, HXU_Code, HXU_GC_NKCompany, HXU_BranchCode, HXU_UsageTimeUtc)
VALUES (NEWID(), '{itemPK}', 'CW1', 'ZZ', '{usageCode}', '{companyCode}', 'BRN', GETUTCDATE())";

			Db.Connection.ExecuteNonQuery(sql);
		}

		class HXURecord : IEquatable<HXURecord>
		{
			public HXURecord(string itemPK, string usageCatetory, string usageCode, string userCode, string companyCode, string branchCode)
			{
				ItemPK = itemPK;
				UsageCatetory = usageCatetory;
				UsageCode = usageCode;
				UserCode = userCode;
				CompanyCode = companyCode;
				BranchCode = branchCode;
			}

			public string ItemPK { get; }
			public string UsageCatetory { get; }
			public string UsageCode { get; }
			public string UserCode { get; }
			public string CompanyCode { get; }
			public string BranchCode { get; }

			public bool Equals(HXURecord other)
			{
				return ItemPK == other.ItemPK
					&& UsageCatetory == other.UsageCatetory
					&& UsageCode == other.UsageCode
					&& UserCode == other.UserCode
					&& CompanyCode == other.CompanyCode
					&& BranchCode == other.BranchCode;
			}
		}
		#endregion
	}
}
