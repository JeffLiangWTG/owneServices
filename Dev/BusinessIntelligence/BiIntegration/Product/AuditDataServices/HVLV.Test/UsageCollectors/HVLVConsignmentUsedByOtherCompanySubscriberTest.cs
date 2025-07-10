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
	[TestedType(typeof(HVLVConsignmentUsedByOtherCompanySubscriber))]
	class HVLVConsignmentUsedByOtherCompanySubscriberTest : HVLVUsageSubcriberTest
	{
		public void TestHVLVConsignmentUsedByOtherCompanySubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			CombineAssertions("Subscriber should be configured correctly", () =>
			{
				AssertEquals("Code", "HXC", subscriber.Code);
				AssertEquals("Table Name", HVLVConsignmentSchema.Constants.TableName, subscriber.Table.TableName);
				AssertEquals("Notify Insert", false, subscriber.NotifyInsert);
				AssertEquals("Notify Update", true, subscriber.NotifyUpdate);
				AssertEquals("Notify Delete", false, subscriber.NotifyDelete);
				AssertContainsExactElementsInAnyOrder("Specific Columns", HVLVConsignmentSchema.All.Except(ExcludedColumns).Select(c => c.Name), subscriber.SpecificColumns.Select(c => c.Name));
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

			var consignment = factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_SystemCreateUser = creatingStaff.GS_Code;
			consignment.HVC_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			var itemOnConsignment = consignment.Items.AddNew();
			itemOnConsignment.HVI_SystemCreateUser = creatingStaff.GS_Code;
			itemOnConsignment.HVI_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			var anotherItemOnConsignment = consignment.Items.AddNew();
			anotherItemOnConsignment.HVI_SystemCreateUser = creatingStaff.GS_Code;
			anotherItemOnConsignment.HVI_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			factory.Save();

			var changeTable = GetTestDataTable();
			_ = CreateRow(changeTable, consignment, "LTT001", editingStaff.GS_Code);

			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(new DummyLogger(), changeTable);

			AssertEquals("2 new records have been created (one for each item) since editing company is different from creating company", 2, GetRecordCountFromHXUTable());
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

			var consignment = factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_SystemCreateUser = creatingStaff.GS_Code;
			consignment.HVC_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			var itemOnConsignment = consignment.Items.AddNew();
			itemOnConsignment.HVI_SystemCreateUser = creatingStaff.GS_Code;
			itemOnConsignment.HVI_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			var anotherItemOnConsignment = consignment.Items.AddNew();
			anotherItemOnConsignment.HVI_SystemCreateUser = creatingStaff.GS_Code;
			anotherItemOnConsignment.HVI_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			factory.Save();

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("LS belongs to creating company", creatingCompany, creatingStaff.LastLogonBranch.Company);
				AssertEquals("RM belongs to same company", creatingCompany, editingStaffInSameCompany.LastLogonBranch.Company);
			});

			var changeTable = GetTestDataTable();
			var row = CreateRow(changeTable, consignment, "LTT001", editingStaffInSameCompany.GS_Code);

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

			var consignment = factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_SystemCreateUser = creatingStaff.GS_Code;

			factory.Save();

			var changeTable = GetTestDataTable();

			var creatingBranch = factory.Load<GlbBranch>(creatingStaff.GS_GB_HomeBranch);
			AssertNull("Precondition: Branch is null - not found", creatingBranch);

			_ = CreateRow(changeTable, consignment, "LTT001", editingStaff.GS_Code);

			var subscriber = NewDataChangeSubscriber();
			var logger = new LoggerForTest();
			AssertNoExceptionThrown(() => subscriber.ProcessChanges(logger, changeTable));
			AssertEquals("No entries should be added to HVLVUsage table", 0, GetRecordCountFromHXUTable());

			var expectedMessageBeginning = $@"A usage was not collected as an error has occurred.
-------DEBUG INFORMATION-------
Values from the HVLVConsignment record being processed when error occured:
  HVLVConsignment.PK: {consignment.PK}
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

			var consignment = factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_SystemCreateUser = creatingStaff.GS_Code;

			factory.Save();

			var changeTable = GetTestDataTable();

			var editingBranch = factory.Load<GlbBranch>(editingStaff.GS_GB_HomeBranch);
			AssertNull("Precondition: Branch is null - not found", editingBranch);

			_ = CreateRow(changeTable, consignment, "LTT001", editingStaff.GS_Code);

			var subscriber = NewDataChangeSubscriber();
			var logger = new LoggerForTest();
			AssertNoExceptionThrown(() => subscriber.ProcessChanges(logger, changeTable));
			AssertEquals("No entries should be added to HVLVUsage table", 0, GetRecordCountFromHXUTable());

			var expectedMessageBeginning = $@"A usage was not collected as an error has occurred.
-------DEBUG INFORMATION-------
Values from the HVLVConsignment record being processed when error occured:
  HVLVConsignment.PK: {consignment.PK}
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
			var subscriber = new HVLVConsignmentUsedByOtherCompanySubscriber();

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
			row[HVLVConsignmentSchema.PK.Name] = new ZGuid();
			if (shouldBeFiltered)
			{
				row[HVLVConsignmentSchema.HVC_IsActive.Name] = 0;
				row[HVLVConsignmentSchema.HVC_SystemLastEditUser.Name] = new ZString("E");
				row[HVLVConsignmentSchema.HVC_SystemCreateUser.Name] = new ZString("E");
			}
			else
			{
				row[HVLVConsignmentSchema.HVC_IsActive.Name] = 1;
				row[HVLVConsignmentSchema.HVC_SystemLastEditUser.Name] = new ZString("KG5");
				row[HVLVConsignmentSchema.HVC_SystemCreateUser.Name] = new ZString("KG5");
			}
			table.Rows.Add(row);
			return row;
		}

		#region Implementation

		IEnumerable<SchemaColumn> ExcludedColumns => new SchemaColumn[]
		{
			HVLVConsignmentSchema.PK,
			HVLVConsignmentSchema.HVC_ClusterKey,
			HVLVConsignmentSchema.HVC_ConsignmentId,
			HVLVConsignmentSchema.HVC_GoodsDescription,
			HVLVConsignmentSchema.HVC_IsValidatedForUniqueness,
			HVLVConsignmentSchema.HVC_SystemCreateTimeUtc,
			HVLVConsignmentSchema.HVC_SystemCreateUser,
		};

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			_ = changeTable.Columns.Add(HVLVConsignmentSchema.Constants.PK, typeof(ZGuid));
			_ = changeTable.Columns.Add(HVLVConsignmentSchema.Constants.HVC_ConsignmentId, typeof(ZString));
			_ = changeTable.Columns.Add(HVLVConsignmentSchema.Constants.HVC_WaybillNumber, typeof(ZString));
			_ = changeTable.Columns.Add(HVLVConsignmentSchema.Constants.HVC_SystemCreateUser, typeof(ZString));
			_ = changeTable.Columns.Add(HVLVConsignmentSchema.Constants.HVC_SystemLastEditUser, typeof(ZString));
			_ = changeTable.Columns.Add(HVLVConsignmentSchema.Constants.HVC_SystemCreateTimeUtc, typeof(ZDateTime));
			_ = changeTable.Columns.Add(HVLVConsignmentSchema.Constants.HVC_SystemLastEditTimeUtc, typeof(ZDateTime));
			_ = changeTable.Columns.Add(HVLVConsignmentSchema.Constants.HVC_IsActive, typeof(bool));

			return changeTable;
		}

		DataRow CreateRow(DataTable dataTable, HVLVConsignment consignment, ZString waybillNumber, ZString staffCode)
		{
			var row = dataTable.NewRow();

			row[HVLVConsignmentSchema.Constants.PK] = consignment.PK;
			row[HVLVConsignmentSchema.Constants.HVC_ConsignmentId] = consignment.HVC_ConsignmentId;
			row[HVLVConsignmentSchema.Constants.HVC_SystemCreateUser] = consignment.HVC_SystemCreateUser;
			row[HVLVConsignmentSchema.Constants.HVC_SystemCreateTimeUtc] = consignment.HVC_SystemCreateTimeUtc;

			row[HVLVConsignmentSchema.Constants.HVC_WaybillNumber] = waybillNumber;
			row[HVLVConsignmentSchema.Constants.HVC_SystemLastEditUser] = staffCode;
			row[HVLVConsignmentSchema.Constants.HVC_SystemLastEditTimeUtc] = ZDateTime.UtcNow;
			row[HVLVConsignmentSchema.Constants.HVC_IsActive] = true;

			dataTable.Rows.Add(row);
			return row;
		}

		#endregion
	}
}
