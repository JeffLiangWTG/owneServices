using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.HVLV.Subscribers;
using Enterprise.eTail.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.HVLV.Test
{
	[TestedType(typeof(HVLVItemUsedByOtherCompanySubscriber))]
	class HVLVItemUsedByOtherCompanySubscriberTest : HVLVUsageSubcriberTest
	{
		public void TestHVLVItemUsedByOtherCompanySubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			CombineAssertions("Subscriber should be configured correctly", () =>
			{
				AssertEquals("Code", "HXI", subscriber.Code);
				AssertEquals("Table Name", HVLVItemSchema.Constants.TableName, subscriber.Table.TableName);
				AssertEquals("Notify Insert", false, subscriber.NotifyInsert);
				AssertEquals("Notify Update", true, subscriber.NotifyUpdate);
				AssertEquals("Notify Delete", false, subscriber.NotifyDelete);
				AssertContainsExactElementsInAnyOrder("Specific Columns", HVLVItemSchema.All.Except(ExcludedColumns).Select(c => c.Name), subscriber.SpecificColumns.Select(c => c.Name));
			});
		}

		[TestDate(2023, 1, 11, 0, 0, 0)]
		public void TestSubscriberInsertsNewRecordIntoExtraUsageTableWhenCreatingCompanyDifferentFromLastEditingCompany()
		{
			var factory = new BusinessObjectFactory();

			var creatingCompany = factory.NewWithValidTestData<GlbCompany>();
			creatingCompany.GC_Code = "LTT";

			var creatingBranch = factory.NewWithValidTestData<GlbBranch>();
			creatingBranch.GB_GC = creatingCompany.PK;
			creatingBranch.GB_Code = "LAB";

			var creatingStaff = factory.NewWithValidTestData<GlbStaff>();
			creatingStaff.GS_GB_LastLogonBranch = creatingBranch.PK;
			creatingStaff.GS_Code = "LS";

			var editingCompany = factory.NewWithValidTestData<GlbCompany>();
			editingCompany.GC_Code = "INT";

			var editingBranch = factory.NewWithValidTestData<GlbBranch>();
			editingBranch.GB_GC = editingCompany.PK;
			editingBranch.GB_Code = "CPU";

			var editingStaff = factory.NewWithValidTestData<GlbStaff>();
			editingStaff.GS_GB_LastLogonBranch = editingBranch.PK;
			editingStaff.GS_Code = "PGS";

			var item = factory.NewWithValidTestData<HVLVItem>();
			item.HVI_SystemCreateUser = creatingStaff.GS_Code;
			item.HVI_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			factory.Save();

			var changeTable = GetTestDataTable();
			_ = CreateRow(changeTable, item, "LTT001", editingStaff.GS_Code);

			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(new DummyLogger(), changeTable);

			AssertEquals("A new record has been created since editing company is different from creating company", 1, GetRecordCountFromHXUTable());
			AssertEquals("The user in HVLVUsage table should record the last edit user", editingStaff.GS_Code, GetUserFromHXUTable());
			AssertEquals("The usageCode in HVLVUsage table should be CWE", "CWE", GetUsageCodeFromHXUTable());
		}

		[TestDate(2023, 1, 11, 0, 0, 0)]
		public void TestSubscriberDoesNotInsertNewRecordIntoExtraUsageTableWhenLastEditingCompanySameAsCreatingCompany()
		{
			var factory = new BusinessObjectFactory();

			var creatingCompany = factory.NewWithValidTestData<GlbCompany>();
			creatingCompany.GC_Code = "LTT";

			var creatingBranch = factory.NewWithValidTestData<GlbBranch>();
			creatingBranch.GB_GC = creatingCompany.PK;
			creatingBranch.GB_Code = "LAB";

			var creatingStaff = factory.NewWithValidTestData<GlbStaff>();
			creatingStaff.GS_GB_LastLogonBranch = creatingBranch.PK;
			creatingStaff.GS_Code = "LS";

			var editingStaffInSameCompany = factory.NewWithValidTestData<GlbStaff>();
			editingStaffInSameCompany.GS_GB_LastLogonBranch = creatingBranch.PK;
			editingStaffInSameCompany.GS_Code = "RM";

			var item = factory.NewWithValidTestData<HVLVItem>();
			item.HVI_SystemCreateUser = creatingStaff.GS_Code;
			item.HVI_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			factory.Save();

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("LS belongs to creating company", creatingCompany, creatingStaff.LastLogonBranch.Company);
				AssertEquals("RM belongs to same company", creatingCompany, editingStaffInSameCompany.LastLogonBranch.Company);
			});

			var changeTable = GetTestDataTable();
			var row = CreateRow(changeTable, item, "LTT001", editingStaffInSameCompany.GS_Code);

			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(new DummyLogger(), changeTable);
			AssertEquals("No new records created as both staff are from same company", 0, GetRecordCountFromHXUTable());
		}

		public void TestReportErrorWhenCreatingBranchIsNull()
		{
			ErrorReporter.Clear();
			var factory = new BusinessObjectFactory();

			var editingBranch = factory.NewWithValidTestData<GlbBranch>();
			editingBranch.GB_Code = "XIV";

			var editingStaff = factory.NewWithValidTestData<GlbStaff>();
			editingStaff.GS_GB_HomeBranch = editingBranch.PK;
			editingStaff.GS_Code = "BLM";

			var creatingStaff = factory.NewWithValidTestData<GlbStaff>();
			creatingStaff.GS_GB_HomeBranch = new ZGuid();
			creatingStaff.GS_Code = "RDM";

			var item = factory.NewWithValidTestData<HVLVItem>();
			item.HVI_SystemCreateUser = creatingStaff.GS_Code;

			factory.Save();

			var changeTable = GetTestDataTable();

			var creatingBranch = factory.Load<GlbBranch>(creatingStaff.GS_GB_HomeBranch);
			AssertNull("Precondition: Branch is null - not found", creatingBranch);

			_ = CreateRow(changeTable, item, "LTT001", editingStaff.GS_Code);

			var subscriber = NewDataChangeSubscriber();
			var logger = new LoggerForTest();
			AssertNoExceptionThrown(() => subscriber.ProcessChanges(logger, changeTable));
			AssertEquals("No entries should be added to HVLVUsage table", 0, GetRecordCountFromHXUTable());

			var expectedMessageBeginning = $@"A usage was not collected as an error has occurred.
-------DEBUG INFORMATION-------
Values from the HVLVItem record being processed when error occured:
  HVLVItem.PK: {item.PK}
  LastEdit Staff Code: BLM
  LastEdit Branch Code: XIV
  Creating Staff Code: RDM
  Creating Branch Code: 
";

			AssertContains(expectedMessageBeginning, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestReportErrorWhenEditingBranchIsNull()
		{
			ErrorReporter.Clear();
			var factory = new BusinessObjectFactory();

			var creatingBranch = factory.NewWithValidTestData<GlbBranch>();
			creatingBranch.GB_Code = "XIV";

			var editingStaff = factory.NewWithValidTestData<GlbStaff>();
			editingStaff.GS_GB_HomeBranch = new ZGuid();
			editingStaff.GS_Code = "BLM";

			var creatingStaff = factory.NewWithValidTestData<GlbStaff>();
			creatingStaff.GS_GB_HomeBranch = creatingBranch.PK;
			creatingStaff.GS_Code = "RDM";

			var item = factory.NewWithValidTestData<HVLVItem>();
			item.HVI_SystemCreateUser = creatingStaff.GS_Code;

			factory.Save();

			var changeTable = GetTestDataTable();

			var editingBranch = factory.Load<GlbBranch>(editingStaff.GS_GB_HomeBranch);
			AssertNull("Precondition: Branch is null - not found", editingBranch);

			_ = CreateRow(changeTable, item, "LTT001", editingStaff.GS_Code);

			var subscriber = NewDataChangeSubscriber();
			var logger = new LoggerForTest();
			AssertNoExceptionThrown(() => subscriber.ProcessChanges(logger, changeTable));
			AssertEquals("No entries should be added to HVLVUsage table", 0, GetRecordCountFromHXUTable());

			var expectedMessageBeginning = $@"A usage was not collected as an error has occurred.
-------DEBUG INFORMATION-------
Values from the HVLVItem record being processed when error occured:
  HVLVItem.PK: {item.PK}
  LastEdit Staff Code: BLM
  LastEdit Branch Code: 
  Creating Staff Code: RDM
  Creating Branch Code: XIV
";

			AssertContains(expectedMessageBeginning, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public override void TestCustomFilter()
		{
			var subscriber = new HVLVItemUsedByOtherCompanySubscriber();

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
				row[HVLVItemSchema.HVI_IsActive.Name] = false;
				row[HVLVItemSchema.HVI_SystemLastEditUser.Name] = new ZString("E");
				row[HVLVItemSchema.HVI_SystemCreateUser.Name] = new ZString("E");
			}
			else
			{
				row[HVLVItemSchema.HVI_IsActive.Name] = true;
				row[HVLVItemSchema.HVI_SystemLastEditUser.Name] = new ZString("KG5");
				row[HVLVItemSchema.HVI_SystemCreateUser.Name] = new ZString("KG5");
			}
			table.Rows.Add(row);
			return row;
		}

		#region Implementation

		IEnumerable<SchemaColumn> ExcludedColumns => new SchemaColumn[]
		{
			HVLVItemSchema.PK,
			HVLVItemSchema.HVI_ClusterKey,
			HVLVItemSchema.HVI_ItemId,
			HVLVItemSchema.HVI_IsValidatedForUniqueness,
			HVLVItemSchema.HVI_SystemCreateTimeUtc,
			HVLVItemSchema.HVI_SystemCreateUser,
		};

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			_ = changeTable.Columns.Add(HVLVItemSchema.Constants.PK, typeof(ZGuid));
			_ = changeTable.Columns.Add(HVLVItemSchema.Constants.HVI_ItemId, typeof(ZString));
			_ = changeTable.Columns.Add(HVLVItemSchema.Constants.HVI_CurrentBarcode, typeof(ZString));
			_ = changeTable.Columns.Add(HVLVItemSchema.Constants.HVI_SystemCreateUser, typeof(ZString));
			_ = changeTable.Columns.Add(HVLVItemSchema.Constants.HVI_SystemLastEditUser, typeof(ZString));
			_ = changeTable.Columns.Add(HVLVItemSchema.Constants.HVI_SystemCreateTimeUtc, typeof(ZDateTime));
			_ = changeTable.Columns.Add(HVLVItemSchema.Constants.HVI_SystemLastEditTimeUtc, typeof(ZDateTime));
			_ = changeTable.Columns.Add(HVLVItemSchema.Constants.HVI_IsActive, typeof(bool));

			return changeTable;
		}

		DataRow CreateRow(DataTable dataTable, HVLVItem item, ZString barcode, ZString staffCode)
		{
			var row = dataTable.NewRow();

			row[HVLVItemSchema.Constants.PK] = item.PK;
			row[HVLVItemSchema.Constants.HVI_ItemId] = item.HVI_ItemId;
			row[HVLVItemSchema.Constants.HVI_SystemCreateUser] = item.HVI_SystemCreateUser;
			row[HVLVItemSchema.Constants.HVI_SystemCreateTimeUtc] = item.HVI_SystemCreateTimeUtc;

			row[HVLVItemSchema.Constants.HVI_CurrentBarcode] = barcode;
			row[HVLVItemSchema.Constants.HVI_SystemLastEditUser] = staffCode;
			row[HVLVItemSchema.Constants.HVI_SystemLastEditTimeUtc] = ZDateTime.UtcNow;
			row[HVLVItemSchema.Constants.HVI_IsActive] = (bool)item.HVI_IsActive;

			dataTable.Rows.Add(row);
			return row;
		}

		#endregion
	}
}
