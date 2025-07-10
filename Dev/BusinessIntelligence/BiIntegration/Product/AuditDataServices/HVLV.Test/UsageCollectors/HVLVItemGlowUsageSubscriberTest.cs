using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.AuditDataServices.HVLV.Subscribers;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.eTail.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.HVLV.Test
{
	[TestedType(typeof(HVLVItemGlowUsageSubscriber))]
	public class HVLVItemGlowUsageSubscriberTest : HVLVUsageSubcriberTest
	{
		public void TestHVLVItemGlowUsageSubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			CombineAssertions("Subscriber should be configured correctly", () =>
			{
				AssertEquals("Code", "HIG", subscriber.Code);
				AssertEquals("Description", "HVLV Items Glow Usage Subscriber", subscriber.Description);
				AssertEquals("Table Name", StmALogSchema.Constants.TableName, subscriber.Table.TableName);
				AssertEquals("Notify Insert", true, subscriber.NotifyInsert);
				AssertEquals("Notify Update", false, subscriber.NotifyUpdate);
				AssertEquals("Notify Delete", false, subscriber.NotifyDelete);
				AssertNull("Specific Columns", subscriber.SpecificColumns);
			});
		}

		public void TestSubscriberInsertsNewRecordIntoUsageTableWhenItemLogAddedInGlow()
		{
			var factory = new BusinessObjectFactory();

			var companyEdt = factory.NewWithValidTestData<GlbCompany>();
			companyEdt.GC_Code = "AMD";

			var branchEdt = factory.NewWithValidTestData<GlbBranch>();
			branchEdt.GB_GC = companyEdt.PK;
			branchEdt.GB_Code = "GPU";

			var itemEdt0 = factory.NewWithValidTestData<HVLVItem>();
			var itemEdt1 = factory.NewWithValidTestData<HVLVItem>();
			var itemEdt2 = factory.NewWithValidTestData<HVLVItem>();
			var itemEdt3 = factory.NewWithValidTestData<HVLVItem>();
			var itemEdt4 = factory.NewWithValidTestData<HVLVItem>();
			var itemEdt5 = factory.NewWithValidTestData<HVLVItem>();
			var itemEdt6 = factory.NewWithValidTestData<HVLVItem>();
			var itemEdt7 = factory.NewWithValidTestData<HVLVItem>();

			factory.Save();

			var changeTable = GetTestDataTable();

			CreateLogRow(changeTable, itemEdt0.PK, "EDT", "G", "ZZ ", "GPU");
			CreateLogRow(changeTable, itemEdt1.PK, "SSC", "G", "ZZ ", "GPU", ",|MID=ESH");
			CreateLogRow(changeTable, itemEdt2.PK, "SSC", "G", "ZZ ", "GPU", ",|MID=EDP");
			CreateLogRow(changeTable, itemEdt3.PK, "SSC", "G", "ZZ ", "GPU", ",|MID=EOD");
			CreateLogRow(changeTable, itemEdt4.PK, "SSC", "G", "ZZ ", "GPU", ",|MID=EOS");
			CreateLogRow(changeTable, itemEdt5.PK, "SSC", "G", "ZZ ", "GPU", ",|MID=ETL");
			CreateLogRow(changeTable, itemEdt6.PK, "SSC", "G", "ZZ ", "GPU", ",|MID=ES2");
			CreateLogRow(changeTable, itemEdt7.PK, "SSC", "G", "ZZ ", "GPU", ",|MID=NEO");

			var subscriber = NewDataChangeSubscriber();
			var logger = new LoggerForTest();
			subscriber.ProcessChanges(logger, changeTable);

			CombineAssertions("HVLVUsage records should be populated correctly", () =>
			{
				AssertEquals("Should insert 8 record in HVLVUsage table", 8, GetRecordCountFromHXUTable());
				AssertHXURecords(new[]
				{
					new HXURecord(itemEdt0.PK.ToString(), "WEB", "GCE", "ZZ", "AMD", "GPU"),
					new HXURecord(itemEdt1.PK.ToString(), "WEB", "ESH", "ZZ", "AMD", "GPU"),
					new HXURecord(itemEdt2.PK.ToString(), "WEB", "EDP", "ZZ", "AMD", "GPU"),
					new HXURecord(itemEdt3.PK.ToString(), "WEB", "EOD", "ZZ", "AMD", "GPU"),
					new HXURecord(itemEdt4.PK.ToString(), "WEB", "EOD", "ZZ", "AMD", "GPU"),
					new HXURecord(itemEdt5.PK.ToString(), "WEB", "EDP", "ZZ", "AMD", "GPU"),
					new HXURecord(itemEdt6.PK.ToString(), "WEB", "ESH", "ZZ", "AMD", "GPU"),
					new HXURecord(itemEdt7.PK.ToString(), "WEB", "ESH", "ZZ", "AMD", "GPU"),
				});
				AssertContainsExactElementsInAnyOrder(new[]
				{
					"8 Glow usage(s) of HVLV Items have been collected, populating usage table if required...",
					"1 Glow usage(s) of GCE have been recorded.",
					"3 Glow usage(s) of ESH have been recorded.",
					"2 Glow usage(s) of EDP have been recorded.",
					"2 Glow usage(s) of EOD have been recorded.",
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
			var logPK = CreateLogRow(changeTable, item.PK, "EDT", "G", "XX", string.Empty);

			var subscriber = NewDataChangeSubscriber();
			var logger = new LoggerForTest();

			CombineAssertions("Should report error when log branch is unknown", () =>
			{
				AssertNoExceptionThrown(() => subscriber.ProcessChanges(logger, changeTable));
				AssertEquals("Should insert no record in HVLVUsage table", 0, GetRecordCountFromHXUTable());
				AssertContainsExactElementsInAnyOrder(new[] { "1 Glow usage(s) of HVLV Items have been collected, populating usage table if required..." }, logger.LogEntries);

				var expectedMessageBeginning = $@"A Glow usage has been skipped as the branch for event log was unknown.
-------DEBUG INFORMATION-------
Values from the StmALog record being processed when error occured:
  Log PK: {logPK}
  Item PK: {item.PK}
  Event: EDT
  Data Source: G
  Reference: 
  User: XX
  Branch: 
  Department: 
  Posted Time: 
  Event Time: ";
				AssertContains(expectedMessageBeginning, ErrorReporter.LastMessageReported);
			});

			ErrorReporter.Clear();
		}

		public void TestIBacklogCountOverridableMembers()
		{
			var subscriber = NewDataChangeSubscriber();

			var backlogOverridableSubscriber = subscriber as IBacklogCountOverridable;

			AssertNotNull($"Should implement {nameof(IBacklogCountOverridable)}", backlogOverridableSubscriber);

			CombineAssertions($"{nameof(IBacklogCountOverridable)} members", () =>
			{
				AssertEquals(nameof(IBacklogCountOverridable.EffectiveTableName), HVLVItemSchema.Constants.TableName, backlogOverridableSubscriber.EffectiveTableName);
				AssertEquals(nameof(IBacklogCountOverridable.CountInsert), false, backlogOverridableSubscriber.CountInsert);
				AssertEquals(nameof(IBacklogCountOverridable.CountUpdate), true, backlogOverridableSubscriber.CountUpdate);
				AssertEquals(nameof(IBacklogCountOverridable.CountDelete), false, backlogOverridableSubscriber.CountDelete);
			});
		}

		public override void TestCustomFilter()
		{
			var subscriber = new HVLVItemGlowUsageSubscriber();

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
			row[StmALogSchema.PK.Name] = new ZGuid();
			if (shouldBeFiltered)
			{
				row[StmALogSchema.Constants.SL_Table] = StmALogSchema.Constants.TableName;
				row[StmALogSchema.Constants.SL_SE_NKEvent] = "DEL";
				row["SL_DataSource"] = "E";
			}
			else
			{
				row[StmALogSchema.Constants.SL_Table] = HVLVItemSchema.Constants.TableName;
				row[StmALogSchema.Constants.SL_SE_NKEvent] = "EDT";
				row["SL_DataSource"] = "G";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var logTable = new DataTable();
			logTable.Columns.Add(StmALogSchema.Constants.PK, typeof(ZGuid));
			logTable.Columns.Add(StmALogSchema.Constants.SL_Table, typeof(string));
			logTable.Columns.Add(StmALogSchema.Constants.SL_Parent, typeof(ZGuid));
			logTable.Columns.Add(StmALogSchema.Constants.SL_SE_NKEvent, typeof(string));
			logTable.Columns.Add(StmALogSchema.Constants.SL_Reference, typeof(string));
			logTable.Columns.Add(StmALogSchema.Constants.SL_GS_NKUser, typeof(string));
			logTable.Columns.Add(StmALogSchema.Constants.SL_GB_NKBranch, typeof(string));
			logTable.Columns.Add(StmALogSchema.Constants.SL_GE_NKDepartment, typeof(string));
			logTable.Columns.Add(StmALogSchema.Constants.SL_PostedTimeUtc, typeof(string));
			logTable.Columns.Add(StmALogSchema.Constants.SL_EventTimeUtc, typeof(string));
			logTable.Columns.Add("SL_DataSource", typeof(string));

			return logTable;
		}

		ZGuid CreateLogRow(DataTable dataTable, ZGuid itemPK, ZString logEvent, ZString logDataSource, ZString userCode, ZString branchCode, ZString reference = default)
		{
			var row = dataTable.NewRow();
			var logPK = ZGuid.NewZGuid();

			row[StmALogSchema.Constants.PK] = logPK;
			row[StmALogSchema.Constants.SL_Table] = HVLVItemSchema.Constants.TableName;
			row[StmALogSchema.Constants.SL_Parent] = itemPK;
			row[StmALogSchema.Constants.SL_SE_NKEvent] = logEvent;
			row["SL_DataSource"] = logDataSource;
			row[StmALogSchema.Constants.SL_GS_NKUser] = userCode;
			row[StmALogSchema.Constants.SL_GB_NKBranch] = branchCode;
			row[StmALogSchema.Constants.SL_Reference] = reference;

			dataTable.Rows.Add(row);
			return logPK;
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
	}
}
