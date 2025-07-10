using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.AuditDataServices.HVLV.Subscribers;
using Enterprise.eTail.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.eTail.Integration.HVLVConstants;

namespace Enterprise.AuditDataServices.HVLV.Test
{
	[TestedType(typeof(HVLVItemLastUsageCodeSubscriber))]
	public class HVLVItemLastUsageCodeSubscriberTest : HVLVUsageSubcriberTest
	{
		public void TestHVLVItemLastUsageCodeSubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			CombineAssertions("Subscriber should be configured correctly", () =>
			{
				AssertEquals("Code", "HIU", subscriber.Code);
				AssertEquals("Description", "HVLV Items LastUsageCode Subscriber", subscriber.Description);
				AssertEquals("Table Name", HVLVItemSchema.Constants.TableName, subscriber.Table.TableName);
				AssertEquals("Notify Insert", false, subscriber.NotifyInsert);
				AssertEquals("Notify Update", true, subscriber.NotifyUpdate);
				AssertEquals("Notify Delete", false, subscriber.NotifyDelete);
				AssertContainsExactElementsInAnyOrder("Specific Columns", new[] { HVLVItemSchema.Constants.HVI_LastUsageCode }, subscriber.SpecificColumns.Select(c => c.Name));
			});
		}

		public void TestHVLVItemLastUsageCodeSubscriberCannotListenToInsert()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertEquals("HVLVItemLastUsageCodeSubscriber cannot listen to Insert operation, otherwise it will conflict with HVLVItemCreationUsageSubscriber. They share the field HVI_LastUsageCode.", false, subscriber.NotifyInsert);
		}

		public void TestSubscriberInsertsNewRecordIntoUsageTableWhenItemsLastUsageCodeIsUpdated()
		{
			var expectedUsageCategory = "SEC";
			AssertEquals("Precondition: Can retrieve usageCategory from usageCode", expectedUsageCategory, UsageCategories.LookupByUsageCode[UsageCodes.ACAS]);

			var factory = new BusinessObjectFactory();

			var editingCompany = factory.NewWithValidTestData<GlbCompany>();
			editingCompany.GC_Code = "INT";

			var editingBranch = factory.NewWithValidTestData<GlbBranch>();
			editingBranch.GB_GC = editingCompany.PK;
			editingBranch.GB_Code = "CPU";

			var editingStaff = factory.NewWithValidTestData<GlbStaff>();
			editingStaff.GS_GB_LastLogonBranch = editingBranch.PK;
			editingStaff.GS_Code = "PGS";

			var item1 = factory.NewWithValidTestData<HVLVItem>();

			factory.Save();

			var changeTable = GetTestDataTable();

			_ = CreateRow(changeTable, item1, editingStaff.GS_Code, UsageCodes.ACAS);

			var subscriber = NewDataChangeSubscriber();
			var logger = new LoggerForTest();
			subscriber.ProcessChanges(logger, changeTable);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[]
				{
					"A usage of 1 HVLV Items LastUsageCode has been collected, populating usage table if required...",
					"1 usages of ACA have been recorded.",
				}, logger.LogEntries);
				AssertEquals("One entry should be added to HVLVUsage table", 1, GetRecordCountFromHXUTable());
				AssertSingleHXURecordContents(item1.PK, expectedUsageCategory, "ACA", editingStaff.GS_Code, "INT", "CPU");
			});
		}

		public void TestReportErrorIfUsageCodeIsInvalid()
		{
			ErrorReporter.Clear();
			var factory = new BusinessObjectFactory();

			var editingCompany = factory.NewWithValidTestData<GlbCompany>();
			editingCompany.GC_Code = "INT";

			var editingBranch = factory.NewWithValidTestData<GlbBranch>();
			editingBranch.GB_GC = editingCompany.PK;
			editingBranch.GB_Code = "CPU";

			var editingStaff = factory.NewWithValidTestData<GlbStaff>();
			editingStaff.GS_GB_HomeBranch = editingBranch.PK;
			editingStaff.GS_Code = "PGS";

			var item1 = factory.NewWithValidTestData<HVLVItem>();

			factory.Save();

			var changeTable = GetTestDataTable();

			var unknownUsageCode = "ZZZ";
			Assert("Precondition: UsageCategory is not found", !UsageCategories.LookupByUsageCode.ContainsKey(unknownUsageCode));
			_ = CreateRow(changeTable, item1, editingStaff.GS_Code, unknownUsageCode);

			var subscriber = NewDataChangeSubscriber();
			var logger = new LoggerForTest();
			AssertNoExceptionThrown(() => subscriber.ProcessChanges(logger, changeTable));
			AssertEquals("No entries should be added to HVLVUsage table", 0, GetRecordCountFromHXUTable());
			AssertContainsExactElementsInAnyOrder(new[] { $"A usage of 1 HVLV Items LastUsageCode has been collected, populating usage table if required..." }, logger.LogEntries);

			var expectedMessageBeginning = $@"A usage has been skipped for the following reason(s):
  --The UsageCategory was unknown.
-------DEBUG INFORMATION-------
Values from the HVLVItem record being processed when error occured:
  HVLVItem.PK: {item1.PK}
  UsageCode: ZZZ
  LastEdit Staff Code: PGS
Computed values that should not be empty or null:
  LastEdit Branch Code: CPU
  UsageCategory: 
Current contents of HVLVConstants.UsageCategories.LookupByUsageCode:";

			AssertContains(expectedMessageBeginning, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestReportErrorIfBranchIsNull()
		{
			ErrorReporter.Clear();
			var factory = new BusinessObjectFactory();

			var editingStaff = factory.NewWithValidTestData<GlbStaff>();
			editingStaff.GS_GB_HomeBranch = new ZGuid();
			editingStaff.GS_Code = "PGS";

			var item1 = factory.NewWithValidTestData<HVLVItem>();

			factory.Save();

			var changeTable = GetTestDataTable();

			var usageCode = UsageCodes.ACAS;

			var branch = factory.Load<GlbBranch>(editingStaff.GS_GB_HomeBranch);
			AssertNull("Precondition: Branch is null - not found", branch);

			_ = CreateRow(changeTable, item1, editingStaff.GS_Code, usageCode);

			var subscriber = NewDataChangeSubscriber();
			var logger = new LoggerForTest();
			AssertNoExceptionThrown(() => subscriber.ProcessChanges(logger, changeTable));
			AssertEquals("No entries should be added to HVLVUsage table", 0, GetRecordCountFromHXUTable());
			AssertContainsExactElementsInAnyOrder(new[] { $"A usage of 1 HVLV Items LastUsageCode has been collected, populating usage table if required..." }, logger.LogEntries);

			var expectedMessageBeginning = $@"A usage has been skipped for the following reason(s):
  --The lastEditBranch was unknown.
-------DEBUG INFORMATION-------
Values from the HVLVItem record being processed when error occured:
  HVLVItem.PK: {item1.PK}
  UsageCode: ACA
  LastEdit Staff Code: PGS
Computed values that should not be empty or null:
  LastEdit Branch Code: 
  UsageCategory: SEC
Current contents of HVLVConstants.UsageCategories.LookupByUsageCode:";

			AssertContains(expectedMessageBeginning, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestSubscriberLogging()
		{
			var factory = new BusinessObjectFactory();

			var editingCompany = factory.NewWithValidTestData<GlbCompany>();
			editingCompany.GC_Code = "INT";

			var editingBranch = factory.NewWithValidTestData<GlbBranch>();
			editingBranch.GB_GC = editingCompany.PK;
			editingBranch.GB_Code = "CPU";

			var editingStaff = factory.NewWithValidTestData<GlbStaff>();
			editingStaff.GS_GB_LastLogonBranch = editingBranch.PK;
			editingStaff.GS_Code = "PGS";

			var item1 = factory.NewWithValidTestData<HVLVItem>();
			var item2 = factory.NewWithValidTestData<HVLVItem>();
			var item3 = factory.NewWithValidTestData<HVLVItem>();

			factory.Save();

			var changeTable = GetTestDataTable();

			_ = CreateRow(changeTable, item1, editingStaff.GS_Code, UsageCodes.ACAS);
			_ = CreateRow(changeTable, item2, editingStaff.GS_Code, UsageCodes.ACAS);
			_ = CreateRow(changeTable, item3, editingStaff.GS_Code, UsageCodes.AUAirCargoReport);

			var subscriber = NewDataChangeSubscriber();
			var logger = new LoggerForTest();
			subscriber.ProcessChanges(logger, changeTable);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[]
				{
					"A usage of 3 HVLV Items LastUsageCode has been collected, populating usage table if required...",
					"2 usages of ACA have been recorded.",
					"1 usages of ACR have been recorded.",
				}, logger.LogEntries);
				AssertEquals("Three entries should be added to HVLVUsage table", 3, GetRecordCountFromHXUTable());
			});
		}

		public void TestValidUsageCodes()
		{
			var usageCodes = new[]
			{
				"ACA",
				"ACR",
				"ANC",
				"ANI",
				"DEC",
				"FHL",
				"SCR",
				"SGA",
				"SNC",
				"SNI",
				"TCD",
				"TET",
				"TWM",
				"UAM",
				"USF",
				"USC",
				"USM",
				"USR",
			};

			usageCodes.ForEach(usageCode =>
			{
				Assert(UsageCategories.LookupByUsageCode.ContainsKey(usageCode));
			});
		}

		public override void TestCustomFilter()
		{
			var subscriber = new HVLVItemLastUsageCodeSubscriber();

			var table = GetTestDataTable();
			var row1 = GetPopulatedDataRow(table, false);
			var row2 = GetPopulatedDataRow(table, false);
			GetPopulatedDataRow(table, true);
			GetPopulatedDataRow(table, true);
			table.AcceptChanges();

			for (var i = 0; i < 4; i++)
			{
				RunCustomFilter(table.Rows[i], subscriber);
			}
			table.AcceptChanges();

			AssertEquals(2, table.Rows.Count);
			AssertEquals(row1, table.Rows[0]);
			AssertEquals(row2, table.Rows[1]);
		}

		DataRow GetPopulatedDataRow(DataTable table, bool shouldBeFiltered)
		{
			var row = table.NewRow();
			row[HVLVItemSchema.PK.Name] = new ZGuid();
			if (shouldBeFiltered)
			{
				row[HVLVItemSchema.HVI_IsActive.Name] = 0;
				row[HVLVItemSchema.HVI_SystemLastEditUser.Name] = new ZString("E");
				row[HVLVItemSchema.HVI_SystemCreateUser.Name] = new ZString("E");
				row[HVLVItemSchema.HVI_LastUsageCode.Name] = ZString.Empty;
			}
			else
			{
				row[HVLVItemSchema.HVI_IsActive.Name] = 1;
				row[HVLVItemSchema.HVI_SystemLastEditUser.Name] = new ZString("KG5");
				row[HVLVItemSchema.HVI_SystemCreateUser.Name] = new ZString("KG5");
				row[HVLVItemSchema.HVI_LastUsageCode.Name] = new ZString("EEE");
			}
			table.Rows.Add(row);
			return row;
		}

		#region Implementation

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			_ = changeTable.Columns.Add(HVLVItemSchema.Constants.PK, typeof(ZGuid));
			_ = changeTable.Columns.Add(HVLVItemSchema.Constants.HVI_SystemLastEditUser, typeof(ZString));
			_ = changeTable.Columns.Add(HVLVItemSchema.Constants.HVI_SystemCreateUser, typeof(ZString));
			_ = changeTable.Columns.Add(HVLVItemSchema.Constants.HVI_LastUsageCode, typeof(ZString));
			_ = changeTable.Columns.Add(HVLVItemSchema.Constants.HVI_IsActive, typeof(bool));

			return changeTable;
		}

		DataRow CreateRow(DataTable dataTable, HVLVItem item, ZString staffCode, ZString lastUsageCode)
		{
			var row = dataTable.NewRow();

			row[HVLVItemSchema.Constants.PK] = item.PK;
			row[HVLVItemSchema.Constants.HVI_SystemLastEditUser] = staffCode;
			row[HVLVItemSchema.Constants.HVI_LastUsageCode] = lastUsageCode;

			dataTable.Rows.Add(row);
			return row;
		}

		void AssertSingleHXURecordContents(ZGuid itemPK, string usageCategory, string usageCode, string user, string companyCode, string branchCode)
		{
			TestConnection.ExecuteReader("SELECT TOP(1) * FROM dbo.HVLVUsage", (row) =>
			{
				AssertEquals(itemPK, row["HXU_HVI_ParentItem"]);
				AssertEquals(usageCategory, row["HXU_Category"]);
				AssertEquals(usageCode, row["HXU_Code"]);
				AssertEquals(user, row["HXU_GS_NKUser"]);
				AssertEquals(companyCode, row["HXU_GC_NKCompany"]);
				AssertEquals(branchCode, row["HXU_BranchCode"]);
			});
		}

		#endregion
	}
}
