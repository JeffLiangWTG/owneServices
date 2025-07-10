[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(CargoWise.Bi.Common.BiConstants))]

namespace Enterprise.AuditDataServices.Business.Testing
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Globalization;
	using System.Linq;
	using CargoWise.Bi.Common;
	using CargoWise.ComponentModel;
	using CargoWise.Data;
	using CargoWise.Data.Testing;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Schema;
	using CargoWise.Types;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Business.Testing;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	[TestedType(typeof(Audit))]
	public class AuditTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructorValidation()
		{
			AssertExceptionThrown(
				"Constructor Throws Exception if businessObject is null",
				typeof(ArgumentNullException),
#if NETFRAMEWORK
				"Value cannot be null.\r\nParameter name: businessObject",
#else
				"Value cannot be null. (Parameter 'businessObject')",
#endif
				() => new Audit(businessObject: null, auditServer: ""));

			AssertExceptionThrown(
				"Constructor Throws Exception if auditServer is null",
				typeof(ArgumentNullException),
#if NETFRAMEWORK
				"Value cannot be null.\r\nParameter name: auditServer",
#else
				"Value cannot be null. (Parameter 'auditServer')",
#endif
				() => new Audit(businessObject: Factory.New<RefCountry>(), auditServer: null));
		}

		public void TestAuditServerFactoryIsNotChanged_WhenAuditServerIsTheSame()
		{
			// Arrange
			var testBizObj = Factory.New<GlbStaff>();

			// Act
			var auditWrapper = new AuditForTest(testBizObj, Db.ServerName);

			// Assert
			AssertEquals("Audit Server The Same", false, ((IDbConnected)auditWrapper.AuditServerFactoryForTest).Connection.CurrentDatabase.Contains(Db.AuditDatabaseSuffix));
		}

		public void TestAuditServerFactoryIsChanged_WhenAuditServerIsDifferent()
		{
			// Arrange
			var testBizObj = Factory.New<GlbStaff>();
			var auditWrapper = new AuditForTest(testBizObj, Db.ServerName);

			// Act
			auditWrapper.AuditServerIsNotSameAsFactoryServerName = true;

			// Assert
			AssertEquals("Audit Server Is Different", true, ((IDbConnected)auditWrapper.AuditServerFactoryForTest).Connection.CurrentDatabase.Contains(Db.AuditDatabaseSuffix));
		}

		public void TestFilterValidation()
		{
			var testBizObj = Factory.New<DummyLogged>();
			var auditWrapper = new Audit(testBizObj, Db.ServerName);

			auditWrapper.FilterTimeLocalFrom = ZDateTime.Invalid;
			auditWrapper.FilterTimeLocalTo = ZDateTime.Invalid;
			auditWrapper.FilterSourceEntity = "~Invalid-Table-Name!";
			AssertEquals("Filter has errors?", true, auditWrapper.HasErrors);
			AssertEquals("Filter Time From Error", "Please enter a valid date.", auditWrapper.FilterTimeLocalFromInfo.GetErrors().First().Message);
			AssertEquals("Filter Time To Error", "Please enter a valid date.", auditWrapper.FilterTimeLocalToInfo.GetErrors().First().Message);
			AssertEquals("Filter Source Entity Error", "Please select a valid source.", auditWrapper.FilterSourceEntityInfo.GetErrors().First().Message);

			auditWrapper.FilterTimeLocalFrom = ZDateTime.Now;
			auditWrapper.FilterTimeLocalTo = auditWrapper.FilterTimeLocalFrom.AddSeconds(-1);
			auditWrapper.FilterSourceEntity = auditWrapper.SourceEntities.First().Code;
			AssertEquals("Filter has errors?", true, auditWrapper.HasErrors);
			AssertEquals("Filter Time From Error", "Date From must be older than Date To.", auditWrapper.FilterTimeLocalFromInfo.GetErrors().First().Message);
			AssertEquals("Filter Time To Error", "Date From must be older than Date To.", auditWrapper.FilterTimeLocalToInfo.GetErrors().First().Message);

			auditWrapper.FilterTimeLocalFrom = auditWrapper.FilterTimeLocalTo.AddDays(-800);
			AssertEquals("Filter has errors?", true, auditWrapper.HasErrors);
			AssertEquals("Filter Time From Error", $"Date range must not exceed audit data retention period of {SystemDataRegistry.Instance.AuditRetentionPeriod.Value} months.", auditWrapper.FilterTimeLocalFromInfo.GetErrors().First().Message);
			AssertEquals("Filter Time To Error", $"Date range must not exceed audit data retention period of {SystemDataRegistry.Instance.AuditRetentionPeriod.Value} months.", auditWrapper.FilterTimeLocalToInfo.GetErrors().First().Message);

			auditWrapper.FilterTimeLocalFrom = auditWrapper.FilterTimeLocalTo.AddDays(-80);
			AssertEquals("Filter has errors?", false, auditWrapper.HasErrors);
		}

		public void TestReadOnlySecurity()
		{
			var auditPermission = Env.Security.AuditServices.IsAllowed;
			var testObject = Factory.New<DummyLogged>();
			var audit = new Audit(testObject, Db.ServerName);
			var properties = TypeDescriptor.GetProperties(audit);
			var objectNames = new List<string>()
			{
				"FilterTimeLocalFrom",
				"FilterTimeLocalTo",
			};

			foreach (var p in properties)
			{
				var property = (PropertyDescriptor)p;
				if (objectNames.Contains(property.Name))
				{
					Env.Security.AuditServices.IsAllowed = true;
					Assert("Access Allowed - Not ReadOnly", !audit.GetReadOnlySecurity(property));

					Env.Security.AuditServices.IsAllowed = false;
					Assert("Access Disallowed - Not ReadOnly", !audit.GetReadOnlySecurity(property));
				}
			}

			Env.Security.AuditServices.IsAllowed = auditPermission;
		}

		public void TestSourceEntities()
		{
			// GlbStaff
			var auditWrapper = new Audit(Factory.New<GlbStaff>(), Db.ServerName);
			AssertEquals("SourceEntities has values", true, auditWrapper.SourceEntities.Any());
			AssertEquals("Code (1st)", Audit.SourceEntityWrapper.AllRelatedEntitiesCode, auditWrapper.SourceEntities.First().Code);
			AssertNull("SourceEntity (1st)", auditWrapper.SourceEntities.First().SourceEntity);
			var secondEntityWrapper = auditWrapper.SourceEntities.Skip(1).First();
			AssertEquals("Code (2nd)", DataBoundResourceStrings.GetTableDescriptiveName(GlbStaffSchema.Constants.TableName), secondEntityWrapper.Code);
			AssertEquals("SourceEntity (2nd)", GlbStaffSchema.Constants.PK, secondEntityWrapper.SourceEntity.KeyColumn.Name);
			AssertEquals("SourceEntities has more than 2 values", true, auditWrapper.SourceEntities.Skip(2).Any());

			// GlbStaffHoliday
			auditWrapper = new Audit(Factory.New<GlbStaffHoliday>(), Db.ServerName);
			AssertEquals("SourceEntities has values", true, auditWrapper.SourceEntities.Any());
			AssertEquals("Code (1st)", DataBoundResourceStrings.GetTableDescriptiveName(GlbStaffHolidaySchema.Constants.TableName), auditWrapper.SourceEntities.First().Code);
			AssertEquals("SourceEntities has only 1 value", false, auditWrapper.SourceEntities.Skip(1).Any());
		}

		public void TestSourceEntityWrapper()
		{
			var testEntityWrapper = new Audit.SourceEntityWrapper(null);
			AssertEquals("Code", Audit.SourceEntityWrapper.AllRelatedEntitiesCode, testEntityWrapper.Code);
			AssertNull("SourceEntity", testEntityWrapper.SourceEntity);

			var unlocoRwEntity = new AuditEntity(RefUNLOCOSchema.RL_RW, null);
			testEntityWrapper = new Audit.SourceEntityWrapper(unlocoRwEntity);
			AssertEquals("Code", unlocoRwEntity.ToString(), testEntityWrapper.Code);
			AssertEquals("KeyColumn", RefUNLOCOSchema.Constants.RL_RW, unlocoRwEntity.KeyColumn.Name);
		}

		public void TestEventHeaderAndDetail()
		{
			var testBizObj = Factory.New<GlbStaff>();
			var auditWrapper = new Audit(testBizObj, Db.ServerName);

			InsertTestLsnMappingRecords();
			InsertTestAuditRecords(
				GlbStaffSchema.Constants.SqlSchemaName,
				GlbStaffSchema.Constants.TableName,
				GlbStaffSchema.Constants.Prefix,
				testBizObj.PK.ToGuid(),
				Guid.NewGuid());
			InsertTestRelatedAuditRecords(
				GlbStaffHolidaySchema.Instance,
				Guid.NewGuid(),
				testBizObj.PK.ToGuid(),
				GlbStaffHolidaySchema.Constants.GA_GS,
				GlbStaffHolidaySchema.Constants.GA_ApprovalStatus);

			// Assert audit event headers (AuditEventData)
			auditWrapper.FilterSourceEntity = DataBoundResourceStrings.GetTableDescriptiveName(GlbStaffSchema.Constants.TableName);
			auditWrapper.FilterUserCode = GlbStaff.CurrentUser.GS_Code;
			auditWrapper.FilterTimeLocalFrom = MinFromDate;
			auditWrapper.FilterTimeLocalTo = new ZDateTime(2017, 2, 22);
			auditWrapper.ReloadAuditEvents();
			auditWrapper.AuditEventData.Sort(AuditEvent.Schema.TimeUtc, ListSortDirection.Descending);
			AssertEquals("Event Header Count", 2, auditWrapper.AuditEventData.Count);
			AssertEventHeader(auditWrapper.AuditEventData[0], testBizObj.PK, Audit.ChangeOperation.AfterUpdate, 1702, new ZDateTime(2017, 2, 12, 12, 0, 0), GlbStaff.CurrentUser.GS_Code);
			AssertEventHeader(auditWrapper.AuditEventData[1], testBizObj.PK, Audit.ChangeOperation.Insert, 1702, new ZDateTime(2017, 2, 1, 1, 0, 0), GlbStaff.CurrentUser.GS_Code);

			// Assert audit event details (AuditChangeData)
			AssertEquals("Event Detail Count", 3, auditWrapper.AuditEventData[0].ChangeCollection.Count);
			AuditChangeCollectionTest.AssertAuditChange(
				auditWrapper.AuditEventData[0].ChangeCollection,
				testBizObj.FindPropertyInfo(GlbStaffSchema.Constants.GS_SystemLastEditUser).HumanReadableName,
				"#U!", GlbStaff.CurrentUser.GS_Code);
			AuditChangeCollectionTest.AssertAuditChange(
				auditWrapper.AuditEventData[0].ChangeCollection,
				testBizObj.FindPropertyInfo(GlbStaffSchema.Constants.GS_Code).HumanReadableName,
				"A1", "A2");
			AuditChangeCollectionTest.AssertAuditChange(
				auditWrapper.AuditEventData[0].ChangeCollection,
				testBizObj.FindPropertyInfo(GlbStaffSchema.Constants.GS_IsActive).HumanReadableName,
				ZBool.False, ZBool.True);
		}

		public void TestEventHeaderAndDetail_WithRelatedTables()
		{
			Guid relatedObjectPk = Guid.NewGuid();
			var testBizObj = Factory.New<GlbStaff>();
			var auditWrapper = new Audit(testBizObj, Db.ServerName);

			InsertTestLsnMappingRecords();
			InsertTestAuditRecords(
				GlbStaffSchema.Constants.SqlSchemaName,
				GlbStaffSchema.Constants.TableName,
				GlbStaffSchema.Constants.Prefix,
				testBizObj.PK.ToGuid(),
				Guid.NewGuid());
			InsertTestRelatedAuditRecords(
				GlbStaffHolidaySchema.Instance,
				relatedObjectPk,
				testBizObj.PK.ToGuid(),
				GlbStaffHolidaySchema.Constants.GA_GS,
				GlbStaffHolidaySchema.Constants.GA_ApprovalStatus);

			// Assert audit event headers (AuditEventData)
			auditWrapper.FilterUserCode = ZString.Empty;
			auditWrapper.FilterTimeLocalFrom = MinFromDate;
			auditWrapper.FilterTimeLocalTo = MaxToDate;
			auditWrapper.ReloadAuditEvents();
			auditWrapper.AuditEventData.Sort(AuditEvent.Schema.TimeUtc, ListSortDirection.Descending);
			AssertEquals("Event Header Count", 7, auditWrapper.AuditEventData.Count);
			AssertEventHeader(auditWrapper.AuditEventData[0], testBizObj.PK, Audit.ChangeOperation.AfterUpdate, 1702, new ZDateTime(2017, 2, 22, 22, 0, 0), GlbStaff.CurrentUser.GS_Code);
			AssertEventHeader(auditWrapper.AuditEventData[1], relatedObjectPk, Audit.ChangeOperation.Delete, 1702, new ZDateTime(2017, 2, 21, 21, 0, 0), "");
			AssertEventHeader(auditWrapper.AuditEventData[2], testBizObj.PK, Audit.ChangeOperation.AfterUpdate, 1702, new ZDateTime(2017, 2, 12, 12, 0, 0), GlbStaff.CurrentUser.GS_Code);
			AssertEventHeader(auditWrapper.AuditEventData[3], testBizObj.PK, Audit.ChangeOperation.AfterUpdate, 1702, new ZDateTime(2017, 2, 11, 11, 0, 0), "#U!");
			AssertEventHeader(auditWrapper.AuditEventData[4], relatedObjectPk, Audit.ChangeOperation.AfterUpdate, 1702, new ZDateTime(2017, 2, 11, 11, 0, 0), "");
			AssertEventHeader(auditWrapper.AuditEventData[5], relatedObjectPk, Audit.ChangeOperation.Insert, 1702, new ZDateTime(2017, 2, 2, 2, 0, 0), "");
			AssertEventHeader(auditWrapper.AuditEventData[6], testBizObj.PK, Audit.ChangeOperation.Insert, 1702, new ZDateTime(2017, 2, 1, 1, 0, 0), GlbStaff.CurrentUser.GS_Code);

			// Assert audit event details (AuditChangeData)
			AssertEquals("Event Detail Count", 3, auditWrapper.AuditEventData[2].ChangeCollection.Count);
			AuditChangeCollectionTest.AssertAuditChange(
				auditWrapper.AuditEventData[2].ChangeCollection,
				testBizObj.FindPropertyInfo(GlbStaffSchema.Constants.GS_SystemLastEditUser).HumanReadableName,
				"#U!", GlbStaff.CurrentUser.GS_Code);
			AuditChangeCollectionTest.AssertAuditChange(
				auditWrapper.AuditEventData[2].ChangeCollection,
				testBizObj.FindPropertyInfo(GlbStaffSchema.Constants.GS_Code).HumanReadableName,
				"A1", "A2");
			AuditChangeCollectionTest.AssertAuditChange(
				auditWrapper.AuditEventData[2].ChangeCollection,
				testBizObj.FindPropertyInfo(GlbStaffSchema.Constants.GS_IsActive).HumanReadableName,
				ZBool.False, ZBool.True);
		}

		public void TestWithNoAuditRecords()
		{
			var testBizObj = Factory.New<GlbStaff>();
			var auditWrapper = new Audit(testBizObj, Db.ServerName);
			auditWrapper.FilterUserCode = ZString.Empty;
			auditWrapper.FilterTimeLocalFrom = MinFromDate;
			auditWrapper.FilterTimeLocalTo = MaxToDate;

			auditWrapper.ReloadAuditEvents();
			AssertEquals("Event Header Count", 0, auditWrapper.AuditEventData.Count);
		}

		public void TestReloadAuditEventsPreservesSort()
		{
			var testBizObj = Factory.New<GlbStaff>();
			var auditWrapper = new Audit(testBizObj, Db.ServerName);
			auditWrapper.FilterUserCode = ZString.Empty;
			auditWrapper.FilterTimeLocalFrom = MinFromDate;
			auditWrapper.FilterTimeLocalTo = MaxToDate;

			var sortInfo = new SortInfo(AuditEvent.Schema.TimeUtc, ListSortDirection.Ascending);
			auditWrapper.AuditEventData.Sort(sortInfo);
			auditWrapper.ReloadAuditEvents();
			AssertEquals("Sort Information should be preserved", auditWrapper.AuditEventData.SortInformation, sortInfo);
		}

		public void TestMinAuditDataAndLastProcessedLocalTime()
		{
			var testBizObj = Factory.New<DummyLogged>();
			var auditWrapper = new Audit(testBizObj, Db.ServerName);

			AssertEquals("[Initial Value] MinAuditDataLocalTime", ZString.Empty, auditWrapper.MinAuditDataLocalTime);
			AssertEquals("[Initial Value] LastProcessedLocalTime", ZString.Empty, auditWrapper.LastProcessedLocalTime);

			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"TRUNCATE TABLE [{0}].[{1}].LsnTimeMapping",
				/*0*/Db.AuditDatabaseName,
				/*1*/BiConstants.BiAdminSchemaName
			);
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText);

			auditWrapper.RefreshMinAndMaxAuditDataTime();
			AssertEquals("MinAuditDataLocalTime", ZString.Empty, auditWrapper.MinAuditDataLocalTime);
			AssertEquals("LastProcessedLocalTime", ZString.Empty, auditWrapper.LastProcessedLocalTime);

			ZDateTime testMinAuditTime = new ZDateTime(2015, 10, 10, 10, 10, 10);
			ZDateTime testMaxAuditTime = new ZDateTime(2017, 3, 3, 3, 3, 3);

			sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[{1}].LsnTimeMapping (StartLsn, TranEndTimeUtc)
					VALUES
						(0x01, '{2}'),
						(0x02, '2016-01-01'),
						(0x03, '{3}');
				",
				/*0*/Db.AuditDatabaseName,
				/*1*/BiConstants.BiAdminSchemaName,
				/*2*/SqlFormatInfo.ToSqlDateTimeString(testMinAuditTime.ToDateTime()),
				/*3*/SqlFormatInfo.ToSqlDateTimeString(testMaxAuditTime.ToDateTime())
			);
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText);

			auditWrapper.RefreshMinAndMaxAuditDataTime();
			AssertEquals("MinAuditDataLocalTime", testMinAuditTime.ToLocalBranchTime(Factory).ToBestReadableDateTimeString(), auditWrapper.MinAuditDataLocalTime);
			AssertEquals("LastProcessedLocalTime", testMaxAuditTime.ToLocalBranchTime(Factory).ToBestReadableDateTimeString(), auditWrapper.LastProcessedLocalTime);
		}

		public void TestNotHasChanges()
		{
			var testBizObj = Factory.New<GlbStaff>();
			var auditWrapper = new Audit(testBizObj, Db.ServerName);
			AssertEquals("Audit doesn't has change after init", expected: false, auditWrapper.HasChanges);
		}

		void InsertTestLsnMappingRecords()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[{1}].LsnTimeMapping (StartLsn, TranEndTimeUtc)
					VALUES
						(0x01, '2017-02-01 01:00:00'),
						(0x02, '2017-02-02 02:00:00'),
						(0x03, '2017-02-11 11:00:00'),
						(0x04, '2017-02-12 12:00:00'),
						(0x05, '2017-02-21 21:00:00'),
						(0x06, '2017-02-22 22:00:00');
				",
				/*0*/Db.AuditDatabaseName,
				/*1*/BiConstants.BiAdminSchemaName
			);
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText);
		}

		void InsertTestAuditRecords(string tableSchema, string tableName, string tablePrefix, Guid pk1, Guid pk2)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[{1}].[{2}] (
					[__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask],
					[{3}_PK], [{3}_SystemLastEditUser],
					[{3}_Code], [{3}_IsActive]
				)
					VALUES
						(1702, 1, 0x01, 0x01, 2, 0x0, '{4}', '{6}', 'A1', 1),

						(1702, 1, 0x02, 0x01, 2, 0x0, '{5}', '{6}', 'B1', 1),

						(1702, 1, 0x03, 0x01, 3, 0x0, '{4}', '{6}', 'A1', 1),
						(1702, 1, 0x03, 0x01, 4, 0x0, '{4}', '#U!', 'A1', 0),

						(1702, 1, 0x04, 0x01, 3, 0x0, '{4}', '#U!', 'A1', 0),
						(1702, 1, 0x04, 0x01, 4, 0x0, '{4}', '{6}', 'A2', 1),

						(1702, 1, 0x05, 0x01, 3, 0x0, '{5}', '{6}', 'B1', 1),
						(1702, 1, 0x05, 0x01, 4, 0x0, '{5}', '{6}', 'B1', 0),

						(1702, 1, 0x06, 0x01, 3, 0x0, '{4}', '{6}', 'A2', 1),
						(1702, 1, 0x06, 0x01, 4, 0x0, '{4}', '{6}', 'A3', 1);
				",
				/*0*/Db.AuditDatabaseName,
				/*1*/tableSchema,
				/*2*/tableName,
				/*3*/tablePrefix,
				/*4*/pk1.ToString(),
				/*5*/pk2.ToString(),
				/*6*/GlbStaff.CurrentUser.GS_Code
			);
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText);
		}

		void InsertTestRelatedAuditRecords(ITableSchema table, Guid pk, Guid parentPk, string fkColumn, string someCharColumn)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[{1}].[{2}] (
					[__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask],
					[{3}], [{4}], [{5}]
				)
					VALUES
						(1702, 1, 0x02, 0x02, 2, 0x0, '{6}', '{7}', '@'),

						(1702, 1, 0x03, 0x02, 3, 0x0, '{6}', '{7}', '@'),
						(1702, 1, 0x03, 0x02, 4, 0x0, '{6}', '{7}', '#'),

						(1702, 1, 0x05, 0x02, 1, 0x0, '{6}', '{7}', '#');
				",
				/*0*/Db.AuditDatabaseName,
				/*1*/table.SqlSchemaName,
				/*2*/table.TableName,
				/*3*/table.PK.Name,
				/*4*/fkColumn,
				/*5*/someCharColumn,
				/*6*/pk.ToString(),
				/*7*/parentPk.ToString()
			);
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText);
		}

		internal static void AssertEventHeader(AuditEvent actualEvent, ZGuid pk, Audit.ChangeOperation operation, ZShort period, ZDateTime timeUtc, ZString userCode)
		{
			AssertEquals("ParentPk", pk, actualEvent.ParentPk);
			AssertEquals("ChangePeriod", period, actualEvent.ChangePeriod);
			AssertEquals("Operation", (int)operation, actualEvent.Operation);
			AssertEquals("TimeUtc", timeUtc, actualEvent.TimeUtc);
			AssertEquals("UserCode", userCode, actualEvent.UserCode);
		}

		internal static readonly ZDateTime MinFromDate = new ZDateTime(2010, 1, 1);
		internal static readonly ZDateTime MaxToDate = new ZDateTime(2020, 1, 1);

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new Audit(GlbStaff.CurrentUser, Db.ServerName);
		}

		#endregion
	}

	public class AuditConcurrencyTest : TestCase
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestReloadAuditEventsWithConcurrentEtl()
		{
			var factory = new BusinessObjectFactory();
			var testBizObj = factory.New<GlbCompany>();
			var auditWrapper = new Audit(testBizObj, Db.ServerName);
			auditWrapper.FilterTimeLocalFrom = new ZDateTime(2018, 1, 1);
			auditWrapper.FilterTimeLocalTo = new ZDateTime(2018, 12, 31);

			//Db.NewExtraConnection(Db.ServerName, Db.AuditDatabaseName, Db.AdminUserLogin, Db.AdminUserPassword)
			//using (var auditConnection = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.AuditDatabaseName))
			using (var auditConnection = Db.NewExtraConnectionToMainDb())
			{
				try
				{
					//
					// Concurrent ETL inserts first batch of audit table and LSN mapping records transactionally
					//
					auditConnection.BeginTransaction();

					string sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT [{0}].dbo.GlbCompany ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], GC_PK, GC_Code, GC_Name) VALUES
							(1801, 1, 0x01, 0x01, 2, 0x01, '{2}', '~1', '~Test~Company~1'),
							(1802, 1, 0x02, 0x01, 3, 0x01, '{2}', '~1', '~Test~Company~2'),
							(1802, 1, 0x02, 0x01, 4, 0x01, '{2}', '#1', '#Test#Company#2');
						INSERT [{0}].[{1}].LsnTimeMapping(StartLsn, TranEndTimeUtc) VALUES
							(0x01, '2018-01-20'),
							(0x02, '2018-02-01');",
						/*0*/Db.AuditDatabaseName,
						/*1*/BiConstants.BiAdminSchemaName,
						/*2*/testBizObj.PK.ToString());
					auditConnection.ExecuteNonQuery(sqlText);

					//
					// Reload audit events => no committed audit events to load
					//--//auditWrapper.FilterSourceEntity = DataBoundResourceStrings.GetTableDescriptiveName(GlbStaffSchema.Constants.TableName);
					//--//auditWrapper.FilterUserCode = GlbStaff.CurrentUser.GS_Code;
					//auditWrapper.FilterTimeLocalFrom = new ZDateTime(2018, 1, 1);
					//auditWrapper.FilterTimeLocalTo = new ZDateTime(2018, 12, 31);
					auditWrapper.ReloadAuditEvents();
					AssertEquals("Event Count", 0, auditWrapper.AuditEventData.Count);
					//

					// Commit first batch of audit records
					auditConnection.CommitTransaction();

					//
					// Concurrent ETL inserts more audit table and LSN mapping records transactionally
					//
					auditConnection.BeginTransaction();

					sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT [{0}].dbo.GlbCompany ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], GC_PK, GC_Code, GC_Name) VALUES
							(1803, 1, 0x03, 0x01, 1, 0x01, '{2}', '#1', '#Test#Company#2');
						INSERT [{0}].[{1}].LsnTimeMapping(StartLsn, TranEndTimeUtc) VALUES
							(0x03, '2018-03-10');",
						/*0*/Db.AuditDatabaseName,
						/*1*/BiConstants.BiAdminSchemaName,
						/*2*/testBizObj.PK.ToString());
					auditConnection.ExecuteNonQuery(sqlText);

					//
					// Reload audit events => should only load committed audit data (first ETL transaction)
					//--//auditWrapper.FilterSourceEntity = DataBoundResourceStrings.GetTableDescriptiveName(GlbStaffSchema.Constants.TableName);
					//--//auditWrapper.FilterUserCode = GlbStaff.CurrentUser.GS_Code;
					//auditWrapper.FilterTimeLocalFrom = new ZDateTime(2018, 2, 15);
					//auditWrapper.FilterTimeLocalTo = new ZDateTime(2018, 3, 5);
					auditWrapper.ReloadAuditEvents();
					AssertEquals("Event Count", 2, auditWrapper.AuditEventData.Count);
					auditWrapper.AuditEventData.Sort(AuditEvent.Schema.TimeUtc, ListSortDirection.Ascending);
					AuditTest.AssertEventHeader(auditWrapper.AuditEventData[0], testBizObj.PK, Audit.ChangeOperation.Insert, 1801, new ZDateTime(2018, 1, 20), ZString.Empty);
					AuditTest.AssertEventHeader(auditWrapper.AuditEventData[1], testBizObj.PK, Audit.ChangeOperation.AfterUpdate, 1802, new ZDateTime(2018, 2, 1), ZString.Empty);
					//

					// Commit second batch of audit records
					auditConnection.CommitTransaction();

					//
					// Reload audit events => should load all committed audit records (first and second ETL transactions)
					auditWrapper.ReloadAuditEvents();
					AssertEquals("Event Count", 3, auditWrapper.AuditEventData.Count);
					auditWrapper.AuditEventData.Sort(AuditEvent.Schema.TimeUtc, ListSortDirection.Ascending);
					AuditTest.AssertEventHeader(auditWrapper.AuditEventData[0], testBizObj.PK, Audit.ChangeOperation.Insert, 1801, new ZDateTime(2018, 1, 20), ZString.Empty);
					AuditTest.AssertEventHeader(auditWrapper.AuditEventData[1], testBizObj.PK, Audit.ChangeOperation.AfterUpdate, 1802, new ZDateTime(2018, 2, 1), ZString.Empty);
					AuditTest.AssertEventHeader(auditWrapper.AuditEventData[2], testBizObj.PK, Audit.ChangeOperation.Delete, 1803, new ZDateTime(2018, 3, 10), ZString.Empty);
					//
				}
				finally
				{
					if (auditConnection.IsInTransaction)
					{
						auditConnection.RollbackTransaction();
					}
				}
			}
		}
	}

	public class AuditForTest : Audit
	{
		public AuditForTest(BusinessObject businessObject, string auditServer) : base(businessObject, auditServer)
		{
		}
		public bool AuditServerIsNotSameAsFactoryServerName { get; set; }

		public BusinessObjectFactory AuditServerFactoryForTest => AuditServerFactory;

		protected override bool IsAuditServerSameAsFactoryServerName => !AuditServerIsNotSameAsFactoryServerName && base.IsAuditServerSameAsFactoryServerName;
	}
}
