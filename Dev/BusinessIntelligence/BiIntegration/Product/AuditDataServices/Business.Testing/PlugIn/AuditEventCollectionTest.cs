namespace Enterprise.AuditDataServices.Business.Testing
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Globalization;
	using System.Reflection;
	using CargoWise.Common;
	using CargoWise.Data;
	using CargoWise.Data.Testing;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Schema;
	using CargoWise.Types;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ProcessManagement.Business;
	using Enterprise.ZArchitecture.Business.Testing;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	[TestedType(typeof(AuditEventCollection))]
	class AuditEventCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AuditEventCollection>
	{
		public void TestReload_WithSecurityCheck()
		{
			ZString viewDeniedMessage = "** View Denied due to Security Access **";

			var testBizObj = Factory.New<GlbStaff>();
			var bankAccount = testBizObj.FindPropertyInfo(GlbStaffSchema.Constants.GS_WagesBankAccount).HumanReadableName;
			var staffEntity = new AuditEntity(GlbStaffSchema.PK, null);

			var staffWithoutBankingDetailsPermissions = Factory.NewWithValidTestData<GlbStaff>();

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewBankingDetails.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutBankingDetailsPermissions.PK;
			staffWithoutBankingDetailsPermissions.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);
			Factory.Save();

			InsertTestLsnMappingRecords();
			InsertTestAuditRecordsForTestSecurity(testBizObj.PK.ToGuid());

			using (Env.SetTemporaryUserContext(new UserContext(testBizObj.GS_LoginName,
					   GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				var auditCollection = new AuditEventCollection(Factory, new AuditMasterTable(testBizObj));

				auditCollection.Reload(sourceEntity: staffEntity, changeUserCode: "", utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: AuditTest.MaxToDate);
				auditCollection.Sort("TimeUtc", ListSortDirection.Ascending);
				// -- Event 0
				AuditChangeCollectionTest.AssertAuditChange(auditCollection[0].ChangeCollection, bankAccount, "", "Test");
				// -- Event 1
				AuditChangeCollectionTest.AssertAuditChange(auditCollection[1].ChangeCollection, bankAccount, "Test", "TestUpdate");
			}

			using (Env.SetTemporaryUserContext(new UserContext(staffWithoutBankingDetailsPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				var auditCollection = new AuditEventCollection(Factory, new AuditMasterTable(testBizObj));

				auditCollection.Reload(sourceEntity: staffEntity, changeUserCode: "", utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: AuditTest.MaxToDate);
				auditCollection.Sort("TimeUtc", ListSortDirection.Ascending);
				// -- Event 0
				AssertEquals("[Event 0] Change Count > 0", true, auditCollection[0].ChangeCollection.Count > 0);
				AuditChangeCollectionTest.AssertAuditChange(auditCollection[0].ChangeCollection, bankAccount, viewDeniedMessage, viewDeniedMessage);
				// -- Event 1
				AuditChangeCollectionTest.AssertAuditChange(auditCollection[1].ChangeCollection, bankAccount, viewDeniedMessage, viewDeniedMessage);
			}
		}

		public void TestConstructorValidation()
		{
			AssertExceptionThrown(
				"Constructor Throws Exception if masterTable is null",
				typeof(ArgumentNullException),
#if NETFRAMEWORK
				"Value cannot be null.\r\nParameter name: masterTable",
#else
				"Value cannot be null. (Parameter 'masterTable')",
#endif
				() => new AuditEventCollection(Factory, masterTable: null));
		}

		public void TestReload()
		{
			var testBizObj = Factory.New<GlbStaff>();
			var auditCollection = new AuditEventCollection(Factory, new AuditMasterTable(testBizObj));
			var lastEditUserFriendlyName = testBizObj.FindPropertyInfo(GlbStaffSchema.Constants.GS_SystemLastEditUser).HumanReadableName;
			var staffEntity = new AuditEntity(GlbStaffSchema.PK, null);

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
				GlbStaffHolidaySchema.Constants.GA_RecordType);

			// ----------
			// No Filters
			// ----------
			auditCollection.Reload(sourceEntity: staffEntity, changeUserCode: "", utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: AuditTest.MaxToDate);
			AssertEquals("[No filters] Count", 3, auditCollection.Count);
			auditCollection.Sort("TimeUtc", ListSortDirection.Ascending);
			// -- Event 0
			AssertAuditEvent(auditCollection[0], testBizObj.PK, bin1, Audit.ChangeOperation.Insert, 1704, new ZDateTime(2017, 4, 28, 4, 0, 0), GlbStaff.CurrentUser.GS_Code, GlbStaff.CurrentUser.GS_FullName);
			AssertEquals("[Event 0] Change Count > 0", true, auditCollection[0].ChangeCollection.Count > 0);
			AuditChangeCollectionTest.AssertAuditChange(auditCollection[0].ChangeCollection, lastEditUserFriendlyName, "", GlbStaff.CurrentUser.GS_Code);
			// -- Event 1
			AssertAuditEvent(auditCollection[1], testBizObj.PK, bin2, Audit.ChangeOperation.AfterUpdate, 1704, new ZDateTime(2017, 4, 29, 10, 0, 0, 7), "#U!", "-");
			AssertEquals("[Event 1] Change Count", 1, auditCollection[1].ChangeCollection.Count);
			AuditChangeCollectionTest.AssertAuditChange(auditCollection[1].ChangeCollection, lastEditUserFriendlyName, GlbStaff.CurrentUser.GS_Code, "#U!");
			// -- Event 2
			AssertAuditEvent(auditCollection[2], testBizObj.PK, bin3, Audit.ChangeOperation.AfterUpdate, 1704, new ZDateTime(2017, 4, 30, 15, 0, 0), GlbStaff.CurrentUser.GS_Code, GlbStaff.CurrentUser.GS_FullName);
			AssertEquals("[Event 2] Change Count", 1, auditCollection[2].ChangeCollection.Count);
			AuditChangeCollectionTest.AssertAuditChange(auditCollection[2].ChangeCollection, lastEditUserFriendlyName, "#U!", GlbStaff.CurrentUser.GS_Code);

			// User = #U!
			auditCollection.Reload(sourceEntity: staffEntity, changeUserCode: "#U!", utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: AuditTest.MaxToDate);
			AssertEquals("[User = #U!] Count", 1, auditCollection.Count);
			AssertAuditEvent(auditCollection[0], testBizObj.PK, bin2, Audit.ChangeOperation.AfterUpdate, 1704, new ZDateTime(2017, 4, 29, 10, 0, 0, 7), "#U!", "-");

			// User = CurrentUser, From = 2017-04-29 00:00:00
			auditCollection.Reload(sourceEntity: staffEntity, changeUserCode: GlbStaff.CurrentUser.GS_Code, utcTimeFrom: new ZDateTime(2017, 4, 29), utcTimeTo: AuditTest.MaxToDate);
			AssertEquals("[User = CurrentUser, From = 2017-04-29 00:00:00] Count", 1, auditCollection.Count);
			AssertAuditEvent(auditCollection[0], testBizObj.PK, bin3, Audit.ChangeOperation.AfterUpdate, 1704, new ZDateTime(2017, 4, 30, 15, 0, 0), GlbStaff.CurrentUser.GS_Code, GlbStaff.CurrentUser.GS_FullName);

			// User = CurrentUser, From = 2017-04-29 00:00:00, To = 2017-04-30 00:00:00
			auditCollection.Reload(sourceEntity: staffEntity, changeUserCode: GlbStaff.CurrentUser.GS_Code, utcTimeFrom: new ZDateTime(2017, 4, 29), utcTimeTo: new ZDateTime(2017, 4, 30));
			AssertEquals("[User = CurrentUser, From = 2017-04-29 00:00:00, To = 2017-04-30 00:00:00] Count", 0, auditCollection.Count);

			// From = 2017-04-28 04:00:00.000, To = 2017-04-29 10:00:01
			auditCollection.Reload(sourceEntity: staffEntity, changeUserCode: "", utcTimeFrom: new ZDateTime(2017, 4, 28, 4, 0, 0), utcTimeTo: new ZDateTime(2017, 4, 29, 10, 0, 1));
			AssertEquals("[From = 2017-04-28 04:00:00, To = 2017-04-29 10:00:01] Count", 2, auditCollection.Count);
			auditCollection.Sort("TimeUtc", ListSortDirection.Ascending);
			AssertAuditEvent(auditCollection[0], testBizObj.PK, bin1, Audit.ChangeOperation.Insert, 1704, new ZDateTime(2017, 4, 28, 4, 0, 0), GlbStaff.CurrentUser.GS_Code, GlbStaff.CurrentUser.GS_FullName);
			AssertAuditEvent(auditCollection[1], testBizObj.PK, bin2, Audit.ChangeOperation.AfterUpdate, 1704, new ZDateTime(2017, 4, 29, 10, 0, 0, 7), "#U!", "-");
		}

		public void TestActualDurationFieldDisplay()
		{
			var testBizObj = Factory.New<ProcessTask>();
			var friendlyName = testBizObj.FindPropertyInfo(ProcessTasksSchema.Constants.P9_ActualDuration).HumanReadableName;
			InsertTestLsnMappingRecords();
			InsertTestAuditRecords(
				ProcessTasksSchema.Constants.SqlSchemaName,
				ProcessTasksSchema.Constants.TableName,
				ProcessTasksSchema.Constants.Prefix,
				ProcessTasksSchema.Constants.P9_ActualDuration,
				"2024-01-05 04:23:00.000",
				"0x01",
				testBizObj.PK.ToGuid());

			var auditCollection = new AuditEventCollection(Factory, new AuditMasterTable(testBizObj));
			var taskEntity = new AuditEntity(ProcessTasksSchema.PK, null);
			auditCollection.Reload(sourceEntity: taskEntity, changeUserCode: "", utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: AuditTest.MaxToDate);
			AuditChangeCollectionTest.AssertAuditChange(auditCollection[0].ChangeCollection, friendlyName, "", "100:23");
		}

		public void TestGuidFieldReplaceByCode()
		{
			var testBizObj = Factory.New<ProcessTask>();
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DAN";
			Factory.Save();
			var auditCollection = new AuditEventCollection(Factory, new AuditMasterTable(testBizObj));
			var p9_GCFriendlyName = testBizObj.FindPropertyInfo(ProcessTasksSchema.Constants.P9_GC).HumanReadableName;
			var taskEntity = new AuditEntity(ProcessTasksSchema.PK, null);
			InsertTestLsnMappingRecords();

			var companyPk = company.PK;
			InsertTestAuditRecords(
				ProcessTasksSchema.Constants.SqlSchemaName,
				ProcessTasksSchema.Constants.TableName,
				ProcessTasksSchema.Constants.Prefix,
				ProcessTasksSchema.Constants.P9_GC,
				companyPk.ToGuid().ToString(),
				"0x01",
				testBizObj.PK.ToGuid());
			auditCollection.Reload(sourceEntity: taskEntity, changeUserCode: "", utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: AuditTest.MaxToDate);
			AuditChangeCollectionTest.AssertAuditChange(auditCollection[0].ChangeCollection, p9_GCFriendlyName, "", "DAN");

			company.GC_Code = "INV";
			Factory.Save();
			auditCollection.Reload(sourceEntity: taskEntity, changeUserCode: "", utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: AuditTest.MaxToDate);
			AuditChangeCollectionTest.AssertAuditChange(auditCollection[0].ChangeCollection, p9_GCFriendlyName, "", "INV");

			company.Delete();
			Factory.Save();
			auditCollection.Reload(sourceEntity: taskEntity, changeUserCode: "", utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: AuditTest.MaxToDate);
			AuditChangeCollectionTest.AssertAuditChange(auditCollection[0].ChangeCollection, p9_GCFriendlyName, "", companyPk);
		}

		[ExpectNoExceptions]
		public void TestGuidFieldReplaceByCode_WithTheSameAuditServer()
		{
			// Arrange
			var testBizObj_WorkItem = Factory.NewWithValidTestData<WorkItem>();
			Factory.Save();

			var factory = Factory;
			var task1 = factory.NewWithValidTestData<ProcessTask>();
			task1.P9_TaskID = "Task1";
			testBizObj_WorkItem.WKI_P9_DefectCausedByTask = task1.PK;
			Factory.Save();

			var wKI_P9_DefectCausedByTaskFriendlyName = testBizObj_WorkItem.FindPropertyInfo(WorkItemSchema.Constants.WKI_P9_DefectCausedByTask).HumanReadableName;
			var taskEntity = new AuditEntity(WorkItemSchema.PK, null);
			InsertTestLsnMappingRecords();

			InsertTestAuditRecords(
				WorkItemSchema.Constants.SqlSchemaName,
				WorkItemSchema.Constants.TableName,
				WorkItemSchema.Constants.Prefix,
				WorkItemSchema.Constants.WKI_P9_DefectCausedByTask,
				task1.PK.ToString(),
				"0x01",
				testBizObj_WorkItem.PK.ToGuid());
			Factory.Save();

			var auditWrapper = new AuditForTest(testBizObj_WorkItem, Db.ServerName);
			AssertEquals("Pre-Condition: Audit Server Is The Same", ((IDbConnected)auditWrapper.Factory).Connection.CurrentDatabase, ((IDbConnected)auditWrapper.AuditServerFactoryForTest).Connection.CurrentDatabase);
			AssertEquals("Pre-Condition: Audit Server Is The Same", false, ((IDbConnected)auditWrapper.AuditServerFactoryForTest).Connection.CurrentDatabase.Contains(Db.AuditDatabaseSuffix));
			var auditCollection = new AuditEventCollection(auditWrapper.AuditServerFactoryForTest, new AuditMasterTable(testBizObj_WorkItem));

			// Act
			auditCollection.Reload(sourceEntity: taskEntity, changeUserCode: "", utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: AuditTest.MaxToDate);

			// Assert
			AuditChangeCollectionTest.AssertAuditChange(auditCollection[0].ChangeCollection, wKI_P9_DefectCausedByTaskFriendlyName, "", task1.P9_TaskID);
		}

		[ExpectNoExceptions]
		public void TestGuidFieldReplaceByCode_WithDifferentAuditServer()
		{
			// Arrange
			var testBizObj_WorkItem = Factory.NewWithValidTestData<WorkItem>();
			Factory.Save();

			var factory = Factory;
			var task1 = factory.NewWithValidTestData<ProcessTask>();
			task1.P9_TaskID = "Task1";
			testBizObj_WorkItem.WKI_P9_DefectCausedByTask = task1.PK;
			Factory.Save();

			var wKI_P9_DefectCausedByTaskFriendlyName = testBizObj_WorkItem.FindPropertyInfo(WorkItemSchema.Constants.WKI_P9_DefectCausedByTask).HumanReadableName;
			var taskEntity = new AuditEntity(WorkItemSchema.PK, null);
			InsertTestLsnMappingRecords();

			InsertTestAuditRecords(
				WorkItemSchema.Constants.SqlSchemaName,
				WorkItemSchema.Constants.TableName,
				WorkItemSchema.Constants.Prefix,
				WorkItemSchema.Constants.WKI_P9_DefectCausedByTask,
				task1.PK.ToString(),
				"0x01",
				testBizObj_WorkItem.PK.ToGuid());
			Factory.Save();

			var auditWrapper = new AuditForTest(testBizObj_WorkItem, Db.ServerName);
			auditWrapper.AuditServerIsNotSameAsFactoryServerName = true;
			AssertEquals("Pre-Condition: Audit Server Is Different", true, ((IDbConnected)auditWrapper.AuditServerFactoryForTest).Connection.CurrentDatabase.Contains(Db.AuditDatabaseSuffix));
			AssertNotEquals("Pre-Condition: Audit Server Is Different", ((IDbConnected)auditWrapper.Factory).Connection.CurrentDatabase, ((IDbConnected)auditWrapper.AuditServerFactoryForTest).Connection.CurrentDatabase);
			var auditCollection = new AuditEventCollection(auditWrapper.AuditServerFactoryForTest, new AuditMasterTable(testBizObj_WorkItem));

			// Act
			auditCollection.Reload(sourceEntity: taskEntity, changeUserCode: "", utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: AuditTest.MaxToDate);

			// Assert
			AssertEquals("No Result (Object has been deleted)", 0, auditCollection.Count);
		}

		public void TestGuidFieldReplaceByCodeFromAuditDB()
		{
			var testBizObj = Factory.New<ProcessTask>();
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DAN";
			Factory.Save();
			var companyPk = company.PK;
			var auditCollection = new AuditEventCollection(Factory, new AuditMasterTable(testBizObj));
			var p9_GCFriendlyName = testBizObj.FindPropertyInfo(ProcessTasksSchema.Constants.P9_GC).HumanReadableName;
			var taskEntity = new AuditEntity(ProcessTasksSchema.PK, null);
			InsertTestLsnMappingRecordsForTestGuidReplaceByCode();

			InsertTestAuditRecords(
				GlbCompanySchema.Constants.SqlSchemaName,
				GlbCompanySchema.Constants.TableName,
				GlbCompanySchema.Constants.Prefix,
				GlbCompanySchema.Constants.GC_Code,
				"A00",
				"0x02",
				companyPk.ToGuid());

			InsertTestAuditRecords(
				GlbCompanySchema.Constants.SqlSchemaName,
				GlbCompanySchema.Constants.TableName,
				GlbCompanySchema.Constants.Prefix,
				GlbCompanySchema.Constants.GC_Code,
				"A01",
				"0x04",
				companyPk.ToGuid());

			InsertTestAuditRecords(
				ProcessTasksSchema.Constants.SqlSchemaName,
				ProcessTasksSchema.Constants.TableName,
				ProcessTasksSchema.Constants.Prefix,
				ProcessTasksSchema.Constants.P9_GC,
				companyPk.ToGuid().ToString(),
				"0x03",
				testBizObj.PK.ToGuid());

			InsertTestAuditRecords(
				ProcessTasksSchema.Constants.SqlSchemaName,
				ProcessTasksSchema.Constants.TableName,
				ProcessTasksSchema.Constants.Prefix,
				ProcessTasksSchema.Constants.P9_GC,
				companyPk.ToGuid().ToString(),
				"0x01",
				testBizObj.PK.ToGuid());

			auditCollection.Reload(sourceEntity: taskEntity, changeUserCode: "", utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: AuditTest.MaxToDate);

			var auditEventLsnMapping = new Dictionary<ZBlob, AuditEvent>();
			foreach (AuditEvent auditEvent in auditCollection)
			{
				auditEventLsnMapping.Add(auditEvent.ChangeLsn, auditEvent);
			}

			var lsn = new ZBlob(new byte[] { 3, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
			AssertEquals(ZShort.Parse("1703"), auditEventLsnMapping[lsn].ChangePeriod);
			AuditChangeCollectionTest.AssertAuditChange(auditEventLsnMapping[lsn].ChangeCollection, p9_GCFriendlyName, "", "A00");

			lsn = new ZBlob(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
			AssertEquals(ZShort.Parse("1701"), auditEventLsnMapping[lsn].ChangePeriod);
			AuditChangeCollectionTest.AssertAuditChange(auditEventLsnMapping[lsn].ChangeCollection, p9_GCFriendlyName, "", "DAN");

			company.Delete();
			Factory.Save();

			auditCollection.Reload(sourceEntity: taskEntity, changeUserCode: "", utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: AuditTest.MaxToDate);

			auditEventLsnMapping.Clear();
			foreach (AuditEvent auditEvent in auditCollection)
			{
				auditEventLsnMapping.Add(auditEvent.ChangeLsn, auditEvent);
			}
			AssertEquals(ZShort.Parse("1701"), auditEventLsnMapping[lsn].ChangePeriod);
			AuditChangeCollectionTest.AssertAuditChange(auditEventLsnMapping[lsn].ChangeCollection, p9_GCFriendlyName, "", companyPk);
		}

		public void TestReload_WithRelatedTablesWithClusterKeys()
		{
			var testBizObj = Factory.New<Integration.Customs.IBaseJobDeclaration>();
			testBizObj.JE_ClusterKey = 1;
			var testRelatedBizo = testBizObj.Invoices.AddNew();
			var auditCollection = new AuditEventCollection(Factory, new AuditMasterTable((BusinessObject)testBizObj));

			InsertTestLsnMappingRecords();
			InsertTestAuditRecords(
				JobComInvoiceHeaderSchema.Constants.SqlSchemaName,
				JobComInvoiceHeaderSchema.Constants.TableName,
				JobComInvoiceHeaderSchema.Constants.Prefix,
				JobComInvoiceHeaderSchema.Constants.JZ_ClusterKey,
				"1",
				"0x01",
				testRelatedBizo.PK.ToGuid());

			auditCollection.Reload(sourceEntity: null, changeUserCode: "", utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: AuditTest.MaxToDate);
			AssertEquals("[No filters] Count", 1, auditCollection.Count);
			AssertAuditEvent(auditCollection[0], testRelatedBizo.PK, bin1, Audit.ChangeOperation.Insert, 1701, new ZDateTime(2017, 4, 28, 4, 0, 0), "", "");
		}

		public void TestReload_WithRelatedTables()
		{
			var testBizObj = Factory.New<GlbStaff>();
			var testRelatedBizo = testBizObj.Holidays.AddNew();
			var auditCollection = new AuditEventCollection(Factory, new AuditMasterTable(testBizObj));
			var lastEditUserFriendlyName = testBizObj.FindPropertyInfo(GlbStaffSchema.Constants.GS_SystemLastEditUser).HumanReadableName;
			var recordTypeFriendlyName = testRelatedBizo.FindPropertyInfo(GlbStaffHolidaySchema.Constants.GA_RecordType).HumanReadableName;
			var staffEntity = new AuditEntity(GlbStaffSchema.PK, null);
			var staffHolidayEntity = new AuditEntity(GlbStaffHolidaySchema.GA_GS, null);

			InsertTestLsnMappingRecords();
			InsertTestAuditRecords(
				GlbStaffSchema.Constants.SqlSchemaName,
				GlbStaffSchema.Constants.TableName,
				GlbStaffSchema.Constants.Prefix,
				testBizObj.PK.ToGuid(),
				Guid.NewGuid());
			InsertTestRelatedAuditRecords(
				GlbStaffHolidaySchema.Instance,
				testRelatedBizo.PK.ToGuid(),
				testBizObj.PK.ToGuid(),
				GlbStaffHolidaySchema.Constants.GA_GS,
				GlbStaffHolidaySchema.Constants.GA_RecordType);

			// ----------
			// No Filters
			// ----------
			auditCollection.Reload(sourceEntity: null, changeUserCode: "", utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: AuditTest.MaxToDate);
			auditCollection.Sort(AuditEvent.Schema.TimeUtc, ListSortDirection.Descending);
			AssertEquals("[No filters] Count", 6, auditCollection.Count);
			// -- Event 0
			AssertAuditEvent(auditCollection[0], testRelatedBizo.PK, bin5, Audit.ChangeOperation.Delete, 1705, new ZDateTime(2017, 5, 1, 20, 0, 0), "", "");
			AssertEquals("[Event 0] Change Count > 0", true, auditCollection[0].ChangeCollection.Count > 0);
			AuditChangeCollectionTest.AssertAuditChange(auditCollection[0].ChangeCollection, recordTypeFriendlyName, "#", "");
			// -- Event 1
			AssertAuditEvent(auditCollection[1], testBizObj.PK, bin3, Audit.ChangeOperation.AfterUpdate, 1704, new ZDateTime(2017, 4, 30, 15, 0, 0), GlbStaff.CurrentUser.GS_Code, GlbStaff.CurrentUser.GS_FullName);
			AssertEquals("[Event 1] Change Count", 1, auditCollection[1].ChangeCollection.Count);
			AuditChangeCollectionTest.AssertAuditChange(auditCollection[1].ChangeCollection, lastEditUserFriendlyName, "#U!", GlbStaff.CurrentUser.GS_Code);
			// -- Event 2
			AssertAuditEvent(auditCollection[2], testRelatedBizo.PK, bin3, Audit.ChangeOperation.AfterUpdate, 1704, new ZDateTime(2017, 4, 30, 15, 0, 0), "", "");
			AssertEquals("[Event 2] Change Count", 1, auditCollection[2].ChangeCollection.Count);
			AuditChangeCollectionTest.AssertAuditChange(auditCollection[2].ChangeCollection, recordTypeFriendlyName, "@", "#");
			// -- Event 3
			AssertAuditEvent(auditCollection[3], testBizObj.PK, bin2, Audit.ChangeOperation.AfterUpdate, 1704, new ZDateTime(2017, 4, 29, 10, 0, 0, 7), "#U!", "-");
			AssertEquals("[Event 3] Change Count", 1, auditCollection[3].ChangeCollection.Count);
			AuditChangeCollectionTest.AssertAuditChange(auditCollection[3].ChangeCollection, lastEditUserFriendlyName, GlbStaff.CurrentUser.GS_Code, "#U!");
			// -- Event 4
			AssertAuditEvent(auditCollection[4], testRelatedBizo.PK, bin2, Audit.ChangeOperation.Insert, 1704, new ZDateTime(2017, 4, 29, 10, 0, 0, 7), "", "");
			AssertEquals("[Event 4] Change Count > 0", true, auditCollection[4].ChangeCollection.Count > 0);
			AuditChangeCollectionTest.AssertAuditChange(auditCollection[4].ChangeCollection, recordTypeFriendlyName, "", "@");
			// -- Event 5
			AssertAuditEvent(auditCollection[5], testBizObj.PK, bin1, Audit.ChangeOperation.Insert, 1704, new ZDateTime(2017, 4, 28, 4, 0, 0), GlbStaff.CurrentUser.GS_Code, GlbStaff.CurrentUser.GS_FullName);
			AssertEquals("[Event 5] Change Count > 0", true, auditCollection[5].ChangeCollection.Count > 0);
			AuditChangeCollectionTest.AssertAuditChange(auditCollection[5].ChangeCollection, lastEditUserFriendlyName, "", GlbStaff.CurrentUser.GS_Code);

			// Table = GlbStaff, User = #U!
			auditCollection.Reload(sourceEntity: staffEntity, changeUserCode: "#U!", utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: AuditTest.MaxToDate);
			auditCollection.Sort(AuditEvent.Schema.TimeUtc, ListSortDirection.Descending);
			AssertEquals("[Table = GlbStaff, User = #U!] Count", 1, auditCollection.Count);
			AssertAuditEvent(auditCollection[0], testBizObj.PK, bin2, Audit.ChangeOperation.AfterUpdate, 1704, new ZDateTime(2017, 4, 29, 10, 0, 0, 7), "#U!", "-");

			// Table = GlbStaff, User = CurrentUser, From = 2017-04-29 00:00:00
			auditCollection.Reload(sourceEntity: staffEntity, changeUserCode: GlbStaff.CurrentUser.GS_Code, utcTimeFrom: new ZDateTime(2017, 4, 29), utcTimeTo: AuditTest.MaxToDate);
			auditCollection.Sort(AuditEvent.Schema.TimeUtc, ListSortDirection.Descending);
			AssertEquals("[Table = GlbStaff, User = CurrentUser, From = 2017-04-29 00:00:00] Count", 1, auditCollection.Count);
			AssertAuditEvent(auditCollection[0], testBizObj.PK, bin3, Audit.ChangeOperation.AfterUpdate, 1704, new ZDateTime(2017, 4, 30, 15, 0, 0), GlbStaff.CurrentUser.GS_Code, GlbStaff.CurrentUser.GS_FullName);

			// Table = GlbStaffHoliday, User = , From = 2017-04-29 00:00:00, To = 2017-04-30 23:59:59
			auditCollection.Reload(sourceEntity: staffHolidayEntity, changeUserCode: "", utcTimeFrom: new ZDateTime(2017, 4, 29), utcTimeTo: new ZDateTime(2017, 4, 30, 23, 59, 59));
			auditCollection.Sort(AuditEvent.Schema.TimeUtc, ListSortDirection.Descending);
			AssertEquals("[Table = GlbStaffHoliday, User = , From = 2017-04-29 00:00:00, To = 2017-04-30 23:59:59] Count", 2, auditCollection.Count);
			AssertAuditEvent(auditCollection[0], testRelatedBizo.PK, bin3, Audit.ChangeOperation.AfterUpdate, 1704, new ZDateTime(2017, 4, 30, 15, 0, 0), "", "");
			AssertAuditEvent(auditCollection[1], testRelatedBizo.PK, bin2, Audit.ChangeOperation.Insert, 1704, new ZDateTime(2017, 4, 29, 10, 0, 0, 7), "", "");

			// Table = GlbStaffHoliday, From = 2017-04-28 04:00:00.000, To = 2017-04-29 10:00:01
			auditCollection.Reload(sourceEntity: staffHolidayEntity, changeUserCode: "", utcTimeFrom: new ZDateTime(2017, 4, 28, 4, 0, 0), utcTimeTo: new ZDateTime(2017, 4, 29, 10, 0, 1));
			auditCollection.Sort(AuditEvent.Schema.TimeUtc, ListSortDirection.Descending);
			AssertEquals("[Table = GlbStaffHoliday, From = 2017-04-28 04:00:00, To = 2017-04-29 10:00:01] Count", 1, auditCollection.Count);
			AssertAuditEvent(auditCollection[0], testRelatedBizo.PK, bin2, Audit.ChangeOperation.Insert, 1704, new ZDateTime(2017, 4, 29, 10, 0, 0, 7), "", "");
		}

		void InsertTestAuditRecordsForTestSecurity(Guid pk1)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[dbo].[GLBStaff] ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], GS_PK, GS_WagesBankAccount)
					VALUES
						(1704, 1, 0x01, 0x01, 2, 0x0, '{1}', 'Test'),
						(1704, 1, 0x03, 0x01, 3, 0x0, '{1}', 'Test'),
						(1704, 1, 0x03, 0x01, 4, 0x0, '{1}', 'TestUpdate');
				",
				/*0*/Db.AuditDatabaseName,
				/*1*/pk1.ToString()
			);
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText);
		}

		void InsertTestLsnMappingRecords()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].biadmin.LsnTimeMapping (StartLsn, TranEndTimeUtc)
					VALUES
						(0x01, '2017-04-28 04:00:00.000'),
						(0x02, '2017-04-29 10:00:00.007'),
						(0x03, '2017-04-30 15:00:00.000'),
						(0x04, '2017-04-30 18:00:00.000'),
						(0x05, '2017-05-01 20:00:00.000');
				",
				/*0*/Db.AuditDatabaseName
			);
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText);
		}

		void InsertTestLsnMappingRecordsForTestGuidReplaceByCode()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].biadmin.LsnTimeMapping (StartLsn, TranEndTimeUtc)
					VALUES
						(0x01, '2017-01-01 04:00:00.000'),
						(0x02, '2017-02-01 10:00:00.007'),
						(0x03, '2017-03-01 15:00:00.000'),
						(0x04, '2017-04-01 18:00:00.000'),
						(0x05, '2017-05-01 20:00:00.000');
				",
				/*0*/Db.AuditDatabaseName
			);
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText);
		}
		void InsertTestAuditRecords(string tableSchema, string tableName, string tablePrefix, Guid pk1, Guid pk2)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[{1}].[{2}] ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [{3}_PK], [{3}_SystemLastEditUser])
					VALUES
						(1704, 1, 0x01, 0x01, 2, 0x0, '{4}', '{6}'),

						(1704, 1, 0x02, 0x01, 3, 0x0, '{4}', '{6}'),
						(1704, 1, 0x02, 0x01, 4, 0x0, '{4}', '#U!'),

						(1704, 1, 0x03, 0x01, 3, 0x0, '{4}', '#U!'),
						(1704, 1, 0x03, 0x01, 4, 0x0, '{4}', '{6}'),

						(1704, 1, 0x04, 0x01, 2, 0x0, '{5}', '#U!'),

						(1705, 1, 0x05, 0x01, 3, 0x0, '{5}', '#U!'),
						(1705, 1, 0x05, 0x01, 4, 0x0, '{5}', '{6}');
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

		void InsertTestAuditRecords(string tableSchema, string tableName, string tablePrefix, string column, string value, string startLsn, Guid pk)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[{1}].[{2}] ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [{3}_PK], [{4}])
					VALUES
						(17{5}, 1, {6}, 0x01, 2, 0x0, '{7}', '{8}')
				",
				/*0*/Db.AuditDatabaseName,
				/*1*/tableSchema,
				/*2*/tableName,
				/*3*/tablePrefix,
				/*4*/column,
				/*5*/startLsn.Substring(2),
				/*6*/startLsn,
				/*7*/pk.ToString(),
				/*8*/value
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
						(1704, 1, 0x02, 0x02, 2, 0x0, '{6}', '{7}', '@'),

						(1704, 1, 0x03, 0x02, 3, 0x0, '{6}', '{7}', '@'),
						(1704, 1, 0x03, 0x02, 4, 0x0, '{6}', '{7}', '#'),

						(1705, 1, 0x05, 0x02, 1, 0x0, '{6}', '{7}', '#');
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

		public void TestReload_WithNoLastEditUserField()
		{
			var testBizObj = Factory.New<GenExportBatchSequence>();
			var auditCollection = new AuditEventCollection(Factory, new AuditMasterTable(testBizObj));
			var customFieldDataFriendlyName = testBizObj.FindPropertyInfo(GenExportBatchSequenceSchema.Constants.XB_Type).HumanReadableName;
			var customValueEntity = new AuditEntity(GenExportBatchSequenceSchema.PK, null);

			InsertTestLsnMappingRecords();
			InsertTestAuditRecordsWithNoLastEditUserField(testBizObj.PK.ToGuid());

			// ----------
			// No Filters
			// ----------
			auditCollection.Reload(sourceEntity: customValueEntity, changeUserCode: "", utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: AuditTest.MaxToDate);
			AssertEquals("[No filters] Count", 2, auditCollection.Count);
			auditCollection.Sort("TimeUtc", ListSortDirection.Ascending);
			// -- Event 0
			AssertAuditEvent(auditCollection[0], testBizObj.PK, bin1, Audit.ChangeOperation.Insert, 1704, new ZDateTime(2017, 4, 28, 4, 0, 0), ZString.Empty, ZString.Empty);
			AssertEquals("[Event 0] Change Count > 0", true, auditCollection[0].ChangeCollection.Count > 0);
			AuditChangeCollectionTest.AssertAuditChange(auditCollection[0].ChangeCollection, customFieldDataFriendlyName, "", "Ini");
			// -- Event 1
			AssertAuditEvent(auditCollection[1], testBizObj.PK, bin3, Audit.ChangeOperation.AfterUpdate, 1704, new ZDateTime(2017, 4, 30, 15, 0, 0), ZString.Empty, ZString.Empty);
			AssertEquals("[Event 1] Change Count", 1, auditCollection[1].ChangeCollection.Count);
			AuditChangeCollectionTest.AssertAuditChange(auditCollection[1].ChangeCollection, customFieldDataFriendlyName, "Ini", "Upd");

			// User = #ANYTHING#
			auditCollection.Reload(sourceEntity: customValueEntity, changeUserCode: "#ANYTHING#", utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: AuditTest.MaxToDate);
			AssertEquals("[User Filter = #ANYTHING# (ignored)] Count", 2, auditCollection.Count);
			auditCollection.Sort("TimeUtc", ListSortDirection.Ascending);
			AssertAuditEvent(auditCollection[0], testBizObj.PK, bin1, Audit.ChangeOperation.Insert, 1704, new ZDateTime(2017, 4, 28, 4, 0, 0), ZString.Empty, ZString.Empty);
			AssertAuditEvent(auditCollection[1], testBizObj.PK, bin3, Audit.ChangeOperation.AfterUpdate, 1704, new ZDateTime(2017, 4, 30, 15, 0, 0), ZString.Empty, ZString.Empty);

			// From = 2017-04-30
			auditCollection.Reload(sourceEntity: customValueEntity, changeUserCode: "", utcTimeFrom: new ZDateTime(2017, 4, 30), utcTimeTo: AuditTest.MaxToDate);
			AssertEquals("[From = 2017-04-30] Count", 1, auditCollection.Count);
			AssertAuditEvent(auditCollection[0], testBizObj.PK, bin3, Audit.ChangeOperation.AfterUpdate, 1704, new ZDateTime(2017, 4, 30, 15, 0, 0), ZString.Empty, ZString.Empty);

			// From = 2017-04-29, To = 2017-04-30
			auditCollection.Reload(sourceEntity: customValueEntity, changeUserCode: "", utcTimeFrom: new ZDateTime(2017, 4, 29), utcTimeTo: new ZDateTime(2017, 4, 30));
			AssertEquals("[From = 2017-04-29, To = 2017-04-30] Count", 0, auditCollection.Count);
		}

		void InsertTestAuditRecordsWithNoLastEditUserField(Guid pk1)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[dbo].[GenExportBatchSequence] ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], XB_PK, XB_Type)
					VALUES
						(1704, 1, 0x01, 0x01, 2, 0x0, '{1}', 'Ini'),
						(1704, 1, 0x03, 0x01, 3, 0x0, '{1}', 'Ini'),
						(1704, 1, 0x03, 0x01, 4, 0x0, '{1}', 'Upd');
				",
				/*0*/Db.AuditDatabaseName,
				/*1*/pk1.ToString()
			);
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText);
		}

		void AssertAuditEvent(AuditEvent actualEvent, ZGuid pk, ZBlob lsn, Audit.ChangeOperation operation, ZShort period, ZDateTime timeUtc, ZString userCode, ZString userName)
		{
			AssertEquals("ParentPk", pk, actualEvent.ParentPk);
			AssertEquals("ChangePeriod", period, actualEvent.ChangePeriod);
			AssertEquals("ChangeLsn", lsn, actualEvent.ChangeLsn);
			AssertEquals("Operation", (int)operation, actualEvent.Operation);
			AssertEquals("TimeUtc", timeUtc, actualEvent.TimeUtc);
			AssertEquals("TimeLocal", timeUtc.ToLocalBranchTime(), actualEvent.TimeLocal);
			AssertEquals("UserCode", userCode, actualEvent.UserCode);
			AssertEquals("UserName", (userCode.IsEmpty) ? "" : userName + " (" + userCode + ")", actualEvent.UserName);
		}

		public void TestReload_WithInvalidDateTimeFilterValues()
		{
			var auditCollection = new AuditEventCollection(Factory, new AuditMasterTable(Factory.New<GlbStaff>()));

			AssertExceptionThrown(
				"AuditEventCollection.Reload throws exception if utcTimeFrom is empty",
				typeof(ArgumentException),
#if NETFRAMEWORK
				"Invalid time-from value.\r\nParameter name: utcTimeFrom",
#else
				"Invalid time-from value. (Parameter 'utcTimeFrom')",
#endif
				() => auditCollection.Reload(sourceEntity: null, changeUserCode: ZString.Empty, utcTimeFrom: ZDateTime.Empty, utcTimeTo: ZDateTime.Empty));

			AssertExceptionThrown(
				"AuditEventCollection.Reload throws exception if utcTimeTo is empty",
				typeof(ArgumentException),
#if NETFRAMEWORK
				"Invalid time-to value.\r\nParameter name: utcTimeTo",
#else
				"Invalid time-to value. (Parameter 'utcTimeTo')",
#endif
				() => auditCollection.Reload(sourceEntity: null, changeUserCode: ZString.Empty, utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: ZDateTime.Empty));

			AssertExceptionThrown(
				"AuditEventCollection.Reload throws exception if utcTimeFrom.Year < 2000",
				typeof(ArgumentException),
#if NETFRAMEWORK
				"Invalid time-from value.\r\nParameter name: utcTimeFrom",
#else
				"Invalid time-from value. (Parameter 'utcTimeFrom')",
#endif
				() => auditCollection.Reload(sourceEntity: null, changeUserCode: ZString.Empty, utcTimeFrom: new ZDateTime(1900, 1, 1), utcTimeTo: ZDateTime.Empty));

			AssertExceptionThrown(
				"AuditEventCollection.Reload throws exception if utcTimeTo.Year < 2000",
				typeof(ArgumentException),
#if NETFRAMEWORK
				"Invalid time-to value.\r\nParameter name: utcTimeTo",
#else
				"Invalid time-to value. (Parameter 'utcTimeTo')",
#endif
				() => auditCollection.Reload(sourceEntity: null, changeUserCode: ZString.Empty, utcTimeFrom: AuditTest.MinFromDate, utcTimeTo: new ZDateTime(1999, 12, 31)));
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestReloadDoesNotLockLsnTimeMapping()
		{
			var auditCollection = new AuditEventCollection(Factory, new AuditMasterTable(GlbStaff.CurrentUser));
			var staffEntity = new AuditEntity(GlbStaffSchema.PK, null);

			using (var testAdminConnection = Db.NewAdminConnection())
			{
				string sqlText = string.Format(CultureInfo.InvariantCulture, @"
					MERGE INTO [{0}].[biadmin].[LsnTimeMapping] AS tgt
						USING (VALUES (0x01, '2017-08-11')) AS src (NewLsn, NewTime)
							ON tgt.StartLsn = src.NewLsn
						WHEN MATCHED THEN
							UPDATE SET TranEndTimeUtc = src.NewTime
						WHEN NOT MATCHED BY TARGET THEN
							INSERT (StartLsn, TranEndTimeUtc) VALUES (NewLsn, NewTime);
					INSERT [{0}].[dbo].[GlbStaff] ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [GS_PK])
						VALUES (1708, 1, 0x01, 0x01, 2, 0x0, '{1}');",
					/*0*/Db.AuditDatabaseName,
					/*1*/GlbStaff.CurrentUser.PK.ToString()
				);
				testAdminConnection.ExecuteNonQuery(sqlText);

				try
				{
					testAdminConnection.BeginTransaction();
					sqlText = string.Format(CultureInfo.InvariantCulture, @"
					DELETE [{0}].[biadmin].[LsnTimeMapping] WHERE StartLsn = 0x01;",
						/*0*/Db.AuditDatabaseName,
						/*1*/GlbStaff.CurrentUser.PK.ToString()
					);
					testAdminConnection.ExecuteNonQuery(sqlText);

					auditCollection.Reload(sourceEntity: staffEntity, changeUserCode: "", utcTimeFrom: new ZDateTime(2017, 8, 10), utcTimeTo: new ZDateTime(2017, 8, 12));
					AssertEquals("Count", 0, auditCollection.Count);
				}
				finally
				{
					testAdminConnection.RollbackTransaction();
				}
			}
		}

		public void TestBusinessObjectTypeDoesNotExistInDevSchema()
		{
			var auditCollection = new AuditEventCollectionForTest(Factory, new AuditMasterTable(Factory.New<GlbStaff>()));
			var dictionary = auditCollection.GetGuidPropertyAndTableInfoDict_exposed("TST");
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertEquals(0, dictionary.Count);
		}

		public void TestBusinessObjectTypeWithDuplicatePropertyNames()
		{
			var dictionary = new Dictionary<string, TableInfo>();
			var auditCollection = new AuditEventCollectionForTest(Factory, new AuditMasterTable(Factory.New<RefTimeZoneSet>()));
			AssertNoExceptionThrown(() => dictionary = auditCollection.GetGuidPropertyAndTableInfoDict_exposed("R3"));
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertEquals(2, dictionary.Count);
			AssertEquals(typeof(StandardTimeZone), dictionary[nameof(RefTimeZoneSet.R3_R2_StandardZone)].RelatedBizObjType);
			AssertEquals(typeof(DaylightSavingTimeZone), dictionary[nameof(RefTimeZoneSet.R3_R2_DaylightSavingZone)].RelatedBizObjType);
		}

		public void TestIsForeignKey()
		{
			var businessObjectBaseType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix("P9", false);

			var twoComponentsForeignKeyPropertyInfo = businessObjectBaseType.GetProperty("P9_GC");
			var threeComponentsForeignKeyPropertyInfo = businessObjectBaseType.GetProperty("P9_FC_CurrentComponent");
			var normalPropertyInfo = businessObjectBaseType.GetProperty("P9_ActualDate");

			var auditCollection = new AuditEventCollectionForTest(Factory, new AuditMasterTable(Factory.New<GlbStaff>()));
			AssertEquals(expected: true, auditCollection.IsForeignKey_Exposed(twoComponentsForeignKeyPropertyInfo));
			AssertEquals(expected: true, auditCollection.IsForeignKey_Exposed(threeComponentsForeignKeyPropertyInfo));
			AssertEquals(expected: false, auditCollection.IsForeignKey_Exposed(normalPropertyInfo));
		}

		protected override AuditEventCollection GetCollectionToTest()
		{
			return new AuditEventCollection(Factory, new AuditMasterTable(Factory.New<DummyLogged>()));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AuditEventForTest(Factory);
		}

		class AuditEventCollectionForTest : AuditEventCollection
		{
			public AuditEventCollectionForTest(BusinessObjectFactory dwServerFactory, AuditMasterTable masterTable) : base(dwServerFactory, masterTable)
			{
			}

			public Dictionary<string, TableInfo> GetGuidPropertyAndTableInfoDict_exposed(string tablePrefix)
			{
				return base.GetGuidPropertyAndTableInfoDict(tablePrefix);
			}

			public Boolean IsForeignKey_Exposed(PropertyInfo propertyInfo)
			{
				return base.IsForeignKey(propertyInfo);
			}
		}

		readonly byte[] bin1 = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		readonly byte[] bin2 = new byte[] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		readonly byte[] bin3 = new byte[] { 3, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		readonly byte[] bin5 = new byte[] { 5, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	}
}
