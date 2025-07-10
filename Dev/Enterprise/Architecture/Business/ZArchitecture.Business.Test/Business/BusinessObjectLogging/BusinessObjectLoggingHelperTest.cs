using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class BusinessObjectLoggingHelperTest : TestCaseWithDummy
	{
		public void TestSystemCreateDetailsNotSetOnSavingIfAlreadySet()
		{
			var dummy = Factory.New<DummyLogged>();
			Assert("Initially, no Create time set", dummy.ZL2_SystemCreateTimeUtc.IsEmpty);

			ZDateTime beforeFirstSave = ZDateTime.UtcNow;
			Factory.Save();
			ZDateTime afterFirstSave = ZDateTime.UtcNow;

			Assert("Create time set", dummy.ZL2_SystemCreateTimeUtc >= beforeFirstSave && dummy.ZL2_SystemCreateTimeUtc <= afterFirstSave);
			AssertEquals("Create and Edit time the same", dummy.ZL2_SystemCreateTimeUtc, dummy.ZL2_SystemLastEditTimeUtc);
			AssertEquals("Create user set", EnvProxy.Instance.CurrentUser.Initials.Trim(), dummy.ZL2_SystemCreateUser);
			AssertEquals("Last Edit user set", EnvProxy.Instance.CurrentUser.Initials.Trim(), dummy.ZL2_SystemCreateUser);
			AssertEquals("Create branch set", EnvProxy.Instance.CurrentBranch.Code.Trim(), dummy.ZL2_SystemCreateBranch);
			AssertEquals("Create department set", EnvProxy.Instance.CurrentDepartment.Code.Trim(), dummy.ZL2_SystemCreateDepartment);

			var dummy2 = Factory.New<DummyLogged>();
			Assert("Initially, no Create time set", dummy2.ZL2_SystemCreateTimeUtc.IsEmpty);
			dummy2.ZL2_SystemCreateTimeUtc = new ZDateTime(2005, 2, 24, 9, 32, 13);
			dummy2.ZL2_SystemCreateUser = "ZUB";
			dummy2.ZL2_SystemCreateBranch = "ZAB";
			dummy2.ZL2_SystemCreateDepartment = "ZOB";
			dummy2.ZL2_SystemLastEditUser = "RAK";
			Factory.Save();

			AssertEquals("Create time set", new ZDateTime(2005, 2, 24, 9, 32, 13), dummy2.ZL2_SystemCreateTimeUtc);
			Assert("Create and Last Edit time NOT the same", dummy2.ZL2_SystemCreateTimeUtc != dummy2.ZL2_SystemLastEditTimeUtc);
			AssertEquals("Create user set", "ZUB", dummy2.ZL2_SystemCreateUser);
			AssertEquals("Create branch set", "ZAB", dummy2.ZL2_SystemCreateBranch);
			AssertEquals("Create department set", "ZOB", dummy2.ZL2_SystemCreateDepartment);
			AssertEquals("Last Edit user RESET", EnvProxy.Instance.CurrentUser.Initials.Trim(), dummy2.ZL2_SystemLastEditUser);
		}

		#region Updating Logging Fields

		[ExpectNoExceptions()]
		public void TestEditAndLoggingFieldsAreNotInConcurrencyCheck()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			var dummy1 = factory1.New<DummyLogged>();
			factory1.Save();
			var dummy2 = factory2.Load<DummyLogged>(dummy1.PK);

			ConcurrencyInfo.SetConcurrencyPolicy(dummy1, "ZL2_Description", ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(dummy2, "ZL2_Description", ConcurrencyPolicy.Ignore);

			dummy1.ZL2_Description = "Dummy1";
			dummy2.ZL2_Description = "Dummy2";

			factory1.Save();
			factory2.Save();
		}

		public void TestEditAndAddLoggingFields()
		{
			string oldCode = StaticCurrentFetcher.Instance.CurrentUser.GS_Code;
			string oldBranchCode = StaticCurrentFetcher.Instance.CurrentBranch.GB_Code;
			string oldDeptCode = StaticCurrentFetcher.Instance.CurrentDepartment.GE_Code;

			var branch1 = Factory.New<IGlbBranch>();
			((BusinessObject)branch1).FillWithValidTestData();
			branch1.GB_GC = Env.CurrentCompanyPK;
			var branch2 = Factory.New<IGlbBranch>();
			((BusinessObject)branch2).FillWithValidTestData();
			branch2.GB_GC = Env.CurrentCompanyPK;
			var branch3 = Factory.New<IGlbBranch>();
			((BusinessObject)branch3).FillWithValidTestData();
			branch3.GB_GC = Env.CurrentCompanyPK;

			var department1 = Factory.New<IGlbDepartment>();
			((BusinessObject)department1).FillWithValidTestData();
			var department2 = Factory.New<IGlbDepartment>();
			((BusinessObject)department2).FillWithValidTestData();
			var department3 = Factory.New<IGlbDepartment>();
			((BusinessObject)department3).FillWithValidTestData();

			var staff1 = Factory.New<IGlbStaff>();
			((BusinessObject)staff1).FillWithValidTestData();
			var staff2 = Factory.New<IGlbStaff>();
			((BusinessObject)staff2).FillWithValidTestData();
			var staff3 = Factory.New<IGlbStaff>();
			((BusinessObject)staff3).FillWithValidTestData();
			var staff4 = Factory.New<IGlbStaff>();
			((BusinessObject)staff4).FillWithValidTestData();

			Factory.Save();

			var dummy = Factory.New<DummyLogged>();
			Assert("Initially, no Create time set", dummy.ZL2_SystemCreateTimeUtc.IsEmpty);
			Assert("Initially, no Edit time set", dummy.ZL2_SystemLastEditTimeUtc.IsEmpty);
			Assert("Initially, no Edit User set", dummy.ZL2_SystemLastEditUser.IsEmpty);
			Assert("Initially, no Create User set", dummy.ZL2_SystemCreateUser.IsEmpty);

			ZDateTime beforeFirstSave = ZDateTime.UtcNow;
			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), branch1.PK.ToGuid(), department1.PK.ToGuid()))
			{
				Factory.Save();
			}
			ZDateTime afterFirstSave = ZDateTime.UtcNow;

			Assert("Create time set", dummy.ZL2_SystemCreateTimeUtc >= beforeFirstSave && dummy.ZL2_SystemCreateTimeUtc <= afterFirstSave);
			AssertEquals("Create and Edit time the same", dummy.ZL2_SystemCreateTimeUtc, dummy.ZL2_SystemLastEditTimeUtc);
			AssertEquals("Create user set", staff1.GS_Code, dummy.ZL2_SystemCreateUser);
			AssertEquals("Create branch set", branch1.GB_Code, dummy.ZL2_SystemCreateBranch);
			AssertEquals("Create department set", department1.GE_Code, dummy.ZL2_SystemCreateDepartment);
			AssertEquals("Edit user set", staff1.GS_Code, dummy.ZL2_SystemLastEditUser);

			ZDateTime pretendCreatedTime = ZDateTime.UtcNow.AddDays(-1);

			dummy.ZL2_SystemCreateTimeUtc = pretendCreatedTime;
			dummy.ZL2_SystemLastEditTimeUtc = pretendCreatedTime;
			dummy.HasChanges = false;

			using (Env.SetTemporaryUserContext(staff2.PK.ToGuid(), branch2.PK.ToGuid(), department2.PK.ToGuid()))
			{
				Factory.Save();
			}

			AssertEquals("Not saved, so no time change", pretendCreatedTime, dummy.ZL2_SystemCreateTimeUtc);
			AssertEquals("Not saved, so no time change", pretendCreatedTime, dummy.ZL2_SystemLastEditTimeUtc);
			AssertEquals("No change as not saved", staff1.GS_Code, dummy.ZL2_SystemCreateUser);
			AssertEquals("No change as not saved", branch1.GB_Code, dummy.ZL2_SystemCreateBranch);
			AssertEquals("No change as not saved", department1.GE_Code, dummy.ZL2_SystemCreateDepartment);
			AssertEquals("No change as not saved", staff1.GS_Code, dummy.ZL2_SystemLastEditUser);

			dummy.ZL2_Description = "Yeah";

			ZDateTime beforeSecondSave = ZDateTime.UtcNow;
			using (Env.SetTemporaryUserContext(staff3.PK.ToGuid(), branch3.PK.ToGuid(), department3.PK.ToGuid()))
			{
				Factory.Save();
			}
			ZDateTime afterSecondSave = ZDateTime.UtcNow;

			AssertEquals("Already created, so no time change", pretendCreatedTime, dummy.ZL2_SystemCreateTimeUtc);
			Assert("Edit time updated", dummy.ZL2_SystemLastEditTimeUtc >= beforeSecondSave && dummy.ZL2_SystemLastEditTimeUtc <= afterSecondSave);
			AssertEquals("Already created, so no change", staff1.GS_Code, dummy.ZL2_SystemCreateUser);
			AssertEquals("Already created, so no change", branch1.GB_Code, dummy.ZL2_SystemCreateBranch);
			AssertEquals("Already created, so no change", department1.GE_Code, dummy.ZL2_SystemCreateDepartment);
			AssertEquals("Last edit user should be updated", staff3.GS_Code, dummy.ZL2_SystemLastEditUser);

			dummy.ZL2_Description = "Baby";

			ZDateTime beforeThirdSave = ZDateTime.UtcNow;
			using (Env.SetTemporaryUserContext(staff4.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				Factory.Save();
			}
			ZDateTime afterThirdSave = ZDateTime.UtcNow;

			AssertEquals("Already created, so no time change", pretendCreatedTime, dummy.ZL2_SystemCreateTimeUtc);
			Assert("Edit time updated", dummy.ZL2_SystemLastEditTimeUtc >= beforeThirdSave && dummy.ZL2_SystemLastEditTimeUtc <= afterThirdSave);
			AssertEquals("Already created, so no change", staff1.GS_Code, dummy.ZL2_SystemCreateUser);
			AssertEquals("Already created, so no change", branch1.GB_Code, dummy.ZL2_SystemCreateBranch);
			AssertEquals("Already created, so no change", department1.GE_Code, dummy.ZL2_SystemCreateDepartment);
			AssertEquals("Last edit user should be updated", staff4.GS_Code, dummy.ZL2_SystemLastEditUser);
		}

		public void TestEditAndAddLoggingFieldsUsingUtcTime()
		{
			var dummy = Factory.New<DummyLogged>();
			Assert("Initially, no Create time set", dummy.ZL2_SystemCreateTimeUtc.IsEmpty);
			Assert("Initially, no Edit time set", dummy.ZL2_SystemLastEditTimeUtc.IsEmpty);

			ZDateTime beforeFirstSave = ZDateTime.UtcNow;
			Factory.Save();
			ZDateTime afterFirstSave = ZDateTime.UtcNow;

			Assert("Create time set", dummy.ZL2_SystemCreateTimeUtc >= beforeFirstSave && dummy.ZL2_SystemCreateTimeUtc <= afterFirstSave);
			AssertEquals("Create and Edit time the same", dummy.ZL2_SystemCreateTimeUtc, dummy.ZL2_SystemLastEditTimeUtc);

			ZDateTime pretendCreatedTime = ZDateTime.UtcNow.AddDays(-1);
			dummy.ZL2_SystemCreateTimeUtc = pretendCreatedTime;
			dummy.ZL2_SystemLastEditTimeUtc = pretendCreatedTime;

			dummy.HasChanges = false;
			Factory.Save();

			AssertEquals("Not saved, so no time change (create time)", pretendCreatedTime, dummy.ZL2_SystemCreateTimeUtc);
			AssertEquals("Not saved, so no time change (edit time)", pretendCreatedTime, dummy.ZL2_SystemLastEditTimeUtc);

			dummy.ZL2_Description = "Yeah";
			ZDateTime beforeSecondSave = ZDateTime.UtcNow;
			Factory.Save();
			ZDateTime afterSecondSave = ZDateTime.UtcNow;

			AssertEquals("Already created, so no time change (2nd save)", pretendCreatedTime, dummy.ZL2_SystemCreateTimeUtc);
			Assert("Edit time updated", dummy.ZL2_SystemLastEditTimeUtc >= beforeSecondSave && dummy.ZL2_SystemLastEditTimeUtc <= afterSecondSave);

			dummy.ZL2_Description = "Baby";
			ZDateTime beforeThirdSave = ZDateTime.UtcNow;
			Factory.Save();
			ZDateTime afterThirdSave = ZDateTime.UtcNow;

			AssertEquals("Already created, so no time change (3rd save)", pretendCreatedTime, dummy.ZL2_SystemCreateTimeUtc);
			Assert("Edit time updated", dummy.ZL2_SystemLastEditTimeUtc >= beforeThirdSave && dummy.ZL2_SystemLastEditTimeUtc <= afterThirdSave);
		}

		public void TestEditAndAddLoggingFieldsHaveSameUtcTime()
		{
			var dummy1 = Factory.New<DummyLoggedUsingServerTimeWithSaveDelay>();
			Assert("Initially, no Create time set", dummy1.ZL2_SystemCreateTimeUtc.IsEmpty);
			Assert("Initially, no Edit time set", dummy1.ZL2_SystemLastEditTimeUtc.IsEmpty);

			var dummy2 = Factory.New<DummyLoggedUsingServerTimeWithSaveDelay>();
			Assert("Initially, no Create time set", dummy2.ZL2_SystemCreateTimeUtc.IsEmpty);
			Assert("Initially, no Edit time set", dummy2.ZL2_SystemLastEditTimeUtc.IsEmpty);

			ZDateTime beforeFirstSave = ZDateTime.UtcNow;
			Factory.Save();
			ZDateTime afterFirstSave = ZDateTime.UtcNow;

			Assert("Create time set", dummy1.ZL2_SystemCreateTimeUtc >= beforeFirstSave && dummy1.ZL2_SystemCreateTimeUtc <= afterFirstSave);
			AssertEquals("All times are same", dummy1.ZL2_SystemCreateTimeUtc, dummy1.ZL2_SystemLastEditTimeUtc);
			AssertEquals("All times are same", dummy1.ZL2_SystemCreateTimeUtc, dummy2.ZL2_SystemCreateTimeUtc);
			AssertEquals("All times are same", dummy1.ZL2_SystemCreateTimeUtc, dummy2.ZL2_SystemLastEditTimeUtc);
		}

		[ExpectNoExceptions]
		public void TestEditLogNotSetIfObjectAlreadyDeleted()
		{
			var dummy = Factory.New<DummyLoggedCanBeDeletedWhenSave>();
			Factory.Save();
			dummy.ZL2_Description = "This object will be deleted.";
			dummy.ShouldFireDelete = true;
			Factory.Save();
		}

		#endregion

		#region Logs

		public void TestAutoCreateAddLog()
		{
			var autoLoggedDummy = Factory.New<DummyBizOWithAutoLogs>();
			AssertEquals("Precondition - no logs", 0, autoLoggedDummy.Logs.AllElements.Count);
			Assert("Precondition - no 'ADD' log", autoLoggedDummy.Logs.AddedLog.SL_PostedTimeUtc.IsEmpty);

			Factory.Save();
			AssertEquals("Should have 1 log", 1, autoLoggedDummy.Logs.AllElements.Count);
			Assert("Should have 1 'ADD' log", !autoLoggedDummy.Logs.AddedLog.SL_PostedTimeUtc.IsEmpty);

			StmALog addLog = autoLoggedDummy.Logs.GetAllLogs()[0];
			AssertEquals("'ADD' log code", Events.AddedARecordToTheSystem.Code, addLog.SL_SE_NKEvent);
			AssertEquals("'ADD' log table name", DummyBizoSchema.Constants.TableName, addLog.SL_Table);
		}

		public void TestAutoCreateEditLog()
		{
			var autoLoggedDummy = Factory.New<DummyBizOWithAutoLogs>();
			Factory.Save();
			autoLoggedDummy.Z0_Description = "INRI";
			AssertEquals("Precondition - 1 'ADD' log", 1, autoLoggedDummy.Logs.AllElements.Count);
			AssertNotNull("Precondition - 1 'ADD' log", autoLoggedDummy.Logs.AddedLog);

			Factory.Save();
			AssertEquals("Should have 2 logs", 2, autoLoggedDummy.Logs.AllElements.Count);

			StmALog editLog = autoLoggedDummy.Logs.GetAllLogs()[1];
			AssertEquals("'EDT' log code", Events.EditedARecord.Code, editLog.SL_SE_NKEvent);
			AssertEquals("'EDT' log table name", DummyBizoSchema.Constants.TableName, editLog.SL_Table);
		}

		public void TestCreateAutoLogIfOnlyChildrenHaveChanges()
		{
			DummyAutoLogged autoLoggedDummy = Factory.New<DummyAutoLogged>();
			autoLoggedDummy.IsTopLevel = true;
			autoLoggedDummy.HasChanges = true;
			autoLoggedDummy.RegisterEditableChildObject(autoLoggedDummy.Collection);
			DummyChildBusinessObject dummyChild = autoLoggedDummy.Collection.AddNew();
			Factory.Save();
			AssertEquals("Precondition - 1 'ADD' log", 1, autoLoggedDummy.Logs.AllElements.Count);

			ZString description = autoLoggedDummy.ZL2_Description;
			autoLoggedDummy.ZL2_Description = "BLAH";
			autoLoggedDummy.ZL2_Description = description;//The parent hasn't really changed and so won't get saved
			Factory.Save();
			AssertEquals("Still 1 'ADD' log since the parent hasn't really changed", 1, autoLoggedDummy.Logs.AllElements.Count);

			dummyChild.Z0_Description = "BLAH";//The child has changed and the parent's HasChangesNotIncludingChildren is false
			Factory.Save();
			AssertEquals("2 'EDT' log should be added", 2, autoLoggedDummy.Logs.AllElements.Count);
			AssertEquals("'EDT' log", Events.EditedARecord.Code, ((StmALogCollection)autoLoggedDummy.Logs.AllElements)[1].SL_SE_NKEvent);

			autoLoggedDummy.ZL2_Description = "BLAH";
			autoLoggedDummy.ZL2_Description = description;
			dummyChild.Z0_Description = "";//The child has changed, the parent's HasChangesNotIncludingChildren is true but the parent hasn't really changed
			Factory.Save();

			System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
			AssertEquals("Another 'EDT' log should be added", 3, autoLoggedDummy.Logs.AllElements.Count);
			AssertEquals("'EDT' log", Events.EditedARecord.Code, ((StmALogCollection)autoLoggedDummy.Logs.AllElements)[2].SL_SE_NKEvent);

			using (CurrentUserChanger.SwitchToNewUserTemporarily(User.PostMasterUserName))
			{
				dummyChild.Z0_Description = "Description";
				Factory.Save();
				AssertEquals(StaticCurrentFetcher.Instance.CurrentUserCode, autoLoggedDummy.ZL2_SystemLastEditUser);

				var factory = new BusinessObjectFactory();
				var loadedDummy = factory.Load<DummyAutoLogged>(autoLoggedDummy.PK);
				AssertEquals(StaticCurrentFetcher.Instance.CurrentUserCode, loadedDummy.ZL2_SystemLastEditUser);
			}
		}

		public void TestCreateAutoLogIfOnlyChildrenHaveChangesWhenChildHasCustomLogReferenceSuffix()
		{
			DummyAutoLoggedWithCustomReference dummyWithCustomReference = Factory.New<DummyAutoLoggedWithCustomReference>();
			dummyWithCustomReference.IsTopLevel = true;
			dummyWithCustomReference.HasChanges = true;
			dummyWithCustomReference.RegisterEditableChildObject(dummyWithCustomReference.Collection);
			dummyWithCustomReference.Logs.AutoCreatedLogDefaultSL_Reference = "BLAH";
			DummyChildBusinessObject dummyChild = dummyWithCustomReference.Collection.AddNew();
			Factory.Save();
			AssertEquals("Precondition - 1 'ADD' log", 1, dummyWithCustomReference.Logs.AllElements.Count);

			dummyChild.Z0_Description = "BLAH";
			Factory.Save();
			AssertEquals("1 'EDT' log should be added", 2, dummyWithCustomReference.Logs.AllElements.Count);
			AssertEquals("'EDT' log", Events.EditedARecord.Code, ((StmALogCollection)dummyWithCustomReference.Logs.AllElements)[1].SL_SE_NKEvent);
		}

		public void TestAutoCreateDeleteLog()
		{
			var autoLoggedDummy = Factory.New<DummyBizOWithAutoLogs>();
			Factory.Save();
			AssertEquals("Precondition - 1 'ADD' log", 1, autoLoggedDummy.Logs.AllElements.Count);
			AssertNotNull("Precondition - 1 'ADD' log", autoLoggedDummy.Logs.AddedLog);

			autoLoggedDummy.Delete();
			Factory.Save();
			AssertEquals("Should have 2 logs", 2, autoLoggedDummy.Logs.AllElements.Count);

			StmALog deleteLog = autoLoggedDummy.Logs.GetAllLogs()[1];
			AssertEquals("'DEL' log code", Events.DeletedARecordInTheSystem.Code, deleteLog.SL_SE_NKEvent);
			AssertEquals("'DEL' log table name", DummyBizoSchema.Constants.TableName, deleteLog.SL_Table);
		}

		public void TestDeleteLogNotAutoCreatedWhenIsInDatabaseIsFalse()
		{
			var autoLoggedDummy = Factory.New<DummyBizOWithAutoLogs>();
			AssertEquals("Precondition - no logs", 0, autoLoggedDummy.Logs.AllElements.Count);

			autoLoggedDummy.Delete();
			Factory.Save();
			AssertEquals("Should have no 'DEL' log when InInDatabase is false", 0, autoLoggedDummy.Logs.AllElements.Count);
		}

		public void TestAutoCreatedLogIsRemovedOnSaveFailure()
		{
			var autoLoggedDummy = Factory.New<DummyBizOWithAutoLogsFakesSaveFailure>();
			AssertEquals("Precondition - no logs", 0, autoLoggedDummy.Logs.AllElements.Count);
			Assert("Precondition - no 'ADD' log", autoLoggedDummy.Logs.AddedLog.SL_PostedTimeUtc.IsEmpty);

			try
			{
				Factory.Save();
			}
			catch (NotSupportedException)
			{
				// Expect exception
			}
			AssertEquals("Should have no log", 1, autoLoggedDummy.Logs.AllElements.Count);
			Assert("Should have no posted 'ADD' log", autoLoggedDummy.Logs.AddedLog.SL_PostedTimeUtc.IsEmpty);
		}

		public void TestOnCreateAutoAdminLogWithAutoLoggedOn()
		{
			var autoLoggedDummy = Factory.New<DummyBizOWithAutoLogs2>();
			AssertEquals("Precondition - not called", 0, autoLoggedDummy.fOnCreateAutoAdminLogCount);

			Factory.Save();
			AssertEquals("OnCreateAutoAdminLog() should have been called once", 1, autoLoggedDummy.fOnCreateAutoAdminLogCount);
			AssertEquals("AutoLoggedDummy.Logs should have 1 logs (ADD).", 1, autoLoggedDummy.Logs.GetAllLogs().Count);
			AssertEquals("AutoLoggedDummy.Logs should have ADD log.", AutoEvents.AddedARecordToTheSystem.Code, autoLoggedDummy.Logs.GetAllLogs()[0].SL_SE_NKEvent);

			autoLoggedDummy.Z0_Description = "INRI";
			Factory.Save();
			AssertEquals("OnCreateAutoAdminLog() should have been called twice", 2, autoLoggedDummy.fOnCreateAutoAdminLogCount);
			AssertEquals("AutoLoggedDummy.Logs should have 2 logs (ADD + EDT).", 2, autoLoggedDummy.Logs.GetAllLogs().Count);
			AssertEquals("AutoLoggedDummy.Logs should have ADD log.", AutoEvents.AddedARecordToTheSystem.Code, autoLoggedDummy.Logs.GetAllLogs()[0].SL_SE_NKEvent);
			AssertEquals("AutoLoggedDummy.Logs should have EDT log.", AutoEvents.EditedARecord.Code, autoLoggedDummy.Logs.GetAllLogs()[1].SL_SE_NKEvent);

			autoLoggedDummy.Delete();
			AssertEquals("OnCreateAutoAdminLog() should have been called 2 times", 2, autoLoggedDummy.fOnCreateAutoAdminLogCount);
			AssertEquals("AutoLoggedDummy.Logs should have 3 logs (ADD + EDT + DEL).", 3, autoLoggedDummy.Logs.GetAllLogs().Count);
			AssertEquals("AutoLoggedDummy.Logs should have ADD log.", AutoEvents.AddedARecordToTheSystem.Code, autoLoggedDummy.Logs.GetAllLogs()[0].SL_SE_NKEvent);
			AssertEquals("AutoLoggedDummy.Logs should have EDT log.", AutoEvents.EditedARecord.Code, autoLoggedDummy.Logs.GetAllLogs()[1].SL_SE_NKEvent);
			AssertEquals("AutoLoggedDummy.Logs should have DEL log.", AutoEvents.DeletedARecordInTheSystem.Code, autoLoggedDummy.Logs.GetAllLogs()[2].SL_SE_NKEvent);
		}

		public void TestOnCreateAutoAdminLogWithAutoLoggedOff()
		{
			var autoLoggedDummy = Factory.New<DummyBizOWithAutoLogs2>();
			autoLoggedDummy.fAutoLogged = EnterpriseBusinessObject.AutologState.NotLogged;
			AssertEquals("Precondition - not called", 0, autoLoggedDummy.fOnCreateAutoAdminLogCount);

			Factory.Save();
			AssertEquals("OnCreateAutoAdminLog() should not have been called", 0, autoLoggedDummy.fOnCreateAutoAdminLogCount);
			AssertEquals("AutoLoggedDummy.Logs should have 0 logs.", 0, autoLoggedDummy.Logs.GetAllLogs().Count);

			autoLoggedDummy.Z0_Description = "INRI";
			Factory.Save();
			AssertEquals("OnCreateAutoAdminLog() should not have been called", 0, autoLoggedDummy.fOnCreateAutoAdminLogCount);
			AssertEquals("AutoLoggedDummy.Logs should have 0 logs.", 0, autoLoggedDummy.Logs.GetAllLogs().Count);

			autoLoggedDummy.Delete();
			AssertEquals("AutoLoggedDummy.Logs should have 0 logs", 0, autoLoggedDummy.Logs.GetAllLogs().Count);
		}

		public void TestAutoCreatedDeleteLogIsDeletedOnBizORollback()
		{
			var autoLoggedDummy = Factory.New<DummyBizOWithAutoLogs>();
			Factory.Save();

			autoLoggedDummy.Delete();
			AssertEquals("Precondition - AutoLoggedDummy should have a delete log.", Events.DeletedARecordInTheSystem.Code, autoLoggedDummy.Logs.AutoCreatedLog.SL_SE_NKEvent);
			AssertEquals("Precondition - AutoLoggedDummy.Logs should have 2 logs (ADD + DEL).", 2, autoLoggedDummy.Logs.GetAllLogs().Count);
			AssertEquals("Precondition - AutoLoggedDummy delete log should be not be deleted.", false, autoLoggedDummy.Logs.AutoCreatedLog.IsDeleted);

			((INeedRow)autoLoggedDummy).Row.RejectChanges();
			autoLoggedDummy.OnSaveRollback();
			AssertNull("Changes rejected, AutoLoggedDummy delete log should be null.", autoLoggedDummy.Logs.AutoCreatedLog);
			AssertEquals("Changes rejected, AutoLoggedDummy.Logs should have 1 log (ADD).", 1, autoLoggedDummy.Logs.GetAllLogs().Count);
		}

		public void TestAutoCreatedLogsAppendSLReference()
		{
			var plainDummy = Factory.New<DummyBizOWithAutoLogs>();
			var plainDummyWithDefaultSL_Reference = Factory.New<DummyBizOWithAutoLogs>();
			plainDummyWithDefaultSL_Reference.Logs.AutoCreatedLogDefaultSL_Reference = "Default SL_Reference in AutoLog Only";
			var dummyWithCustomReference = Factory.New<DummyBizOWithAutoLogsCustomReference>();
			var dummyWithCustomReferenceAndDefaultSLReference = Factory.New<DummyBizOWithAutoLogsCustomReference>();
			dummyWithCustomReferenceAndDefaultSLReference.Logs.AutoCreatedLogDefaultSL_Reference = "Default SL_Reference in AutoLog";

			Factory.Save();

			AssertEquals("No reference should be set", "", plainDummy.Logs.AutoCreatedLog.SL_Reference);
			AssertEquals("Reference should be only custom reference", "Default SL_Reference in AutoLog Only", plainDummyWithDefaultSL_Reference.Logs.AutoCreatedLog.SL_Reference);
			AssertEquals("Reference should be only autolog reference", "Custom Dummy Reference", dummyWithCustomReference.Logs.AutoCreatedLog.SL_Reference);
			AssertEquals("Reference should be custom reference concatenated to AutoLogDefault Reference", "Default SL_Reference in AutoLog - Custom Dummy Reference", dummyWithCustomReferenceAndDefaultSLReference.Logs.AutoCreatedLog.SL_Reference);
		}

		public void TestAutoCreatedLogsAppendSLReferenceAndNonASCIIValueDoesNotRaiseValidationError()
		{
			DummyBizOWithAutoLogs dummyWithCustomReferenceAndDefaultSLReference = Factory.New<DummyBizOWithAutoLogsCustomReference>();
			dummyWithCustomReferenceAndDefaultSLReference.Logs.AutoCreatedLogDefaultSL_Reference = "Default SL_Reference in AutoLog а тут будут знаки вопроса";

			Factory.Save();

			Assert("No error on SL_Reference about non English characters", !dummyWithCustomReferenceAndDefaultSLReference.Logs.AutoCreatedLog.SL_ReferenceInfo.HasError("Reference only accepts Western European languages characters."));
			AssertEquals("Reference should be custom reference concatenated to AutoLogDefault Reference", "Default SL_Reference in AutoLog ? ??? ????? ????? ??????? - Custom Dummy Reference", dummyWithCustomReferenceAndDefaultSLReference.Logs.AutoCreatedLog.SL_Reference);
		}

		public void TestIsNewAndAllPropertiesDuplicatesOfExistingObject()
		{
			var bizO1 = Factory.New<DummyBusinessObject>();
			DummyMToNCollection collection = new DummyMToNCollection(bizO1);
			DummyDependantBusinessObject dependentObject = collection.AddNew();
			Factory.Save();
			collection.Remove(dependentObject);
			collection.Add(dependentObject);
			DummyPivot pivot = (DummyPivot)collection.GetRelationshipBusinessObject(dependentObject);
			AssertEquals("Dummy Pivot Object is actually not duplicated because local copy is deleted", false, pivot.ExposedIsNewAndAllPropertiesDuplicatesOfExistingObject);
			var duplicatePivot = Factory.New<DummyPivot>();
			duplicatePivot.ZDP_Z0 = pivot.ZDP_Z0;
			duplicatePivot.ZDP_ZD1 = pivot.ZDP_ZD1;
			duplicatePivot.ZDP_AddInfo = pivot.ZDP_AddInfo;
			AssertEquals("Duplicate Dummy Pivot Object", true, duplicatePivot.ExposedIsNewAndAllPropertiesDuplicatesOfExistingObject);
		}

		public void TestAutoCreateEditLogLightValidation()
		{
			var autoLoggedDummy = Factory.New<DummyBizOWithAutoLogs>();
			Factory.Save();
			AssertEquals("AutoLoggedDummy.Logs.AllElements.Count", 1, autoLoggedDummy.Logs.AllElements.Count);
			AssertNotNull("AutoLoggedDummy.Logs.AddedLog", autoLoggedDummy.Logs.AddedLog);

			((ILightValidationInternals)autoLoggedDummy).IsValid = true;
			Factory.Save();
			AssertEquals("AutoLoggedDummy.Logs.AllElements.Count", 1, autoLoggedDummy.Logs.AllElements.Count);

			autoLoggedDummy.Z0_Description = "INRI";
			((ILightValidationInternals)autoLoggedDummy).IsValid = false;
			Factory.Save();

			AssertEquals("AutoLoggedDummy.Logs.AllElements.Count", 2, autoLoggedDummy.Logs.AllElements.Count);
		}

		#endregion

		#region TestFetchHints

		public void TestFetchHints()
		{
			DummyBizOWithAutoLogs dummy1 = Factory.New<DummyBizOWithAutoLogs>();
			DummyBizOWithAutoLogs dummy2 = Factory.New<DummyBizOWithAutoLogs>();
			DummyBizOWithAutoLogs dummy3 = Factory.New<DummyBizOWithAutoLogs>();

			Factory.Save();

			dummy1.Z0_Description = "Qwerty";
			dummy2.Z0_Description = "Qwerty";
			dummy3.Z0_Description = "Qwerty";

			AssertEquals("Precondition - 1 'ADD' log", 1, dummy1.Logs.AllElements.Count);
			AssertNotNull("Precondition - 1 'ADD' log", dummy1.Logs.AddedLog);

			AssertEquals(0, Factory.GetTableHitCount(ProcessTasksSchema.Constants.TableName));

			ISavingFetchStrategy b = new BusinessObjectLoggingStrategy(dummy1);
			b.FetchForSaving(dummy1);

			// this test needs fixing
			AssertEquals(0, Factory.GetTableHitCount(ProcessTasksSchema.Constants.TableName));
			Factory.Save();

			AssertEquals("Should have 2 logs", 2, dummy1.Logs.AllElements.Count);

			StmALog editLog = dummy1.Logs.GetAllLogs()[1];
			AssertEquals("'EDT' log code", Events.EditedARecord.Code, editLog.SL_SE_NKEvent);
			AssertEquals("'EDT' log table name", DummyBizoSchema.Constants.TableName, editLog.SL_Table);

			AssertEquals(0, Factory.GetTableHitCount(ProcessTasksSchema.Constants.TableName));
		}

		#endregion

		#region Generic Behaviour Tests

		public void TestDoNotLogIfBizoIsNotLogTarget()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var deferredLogger = new DummyBizoLoggerDeferred(Factory);
			var nonDeferredLogger = new DummyBizoLoggerNonDeferred(Factory);

			var loggingStrategy = new BusinessObjectLoggingStrategy(null, b => new IBusinessObjectLogger[] { deferredLogger, nonDeferredLogger });

			loggingStrategy.OnSaving(dummy);

			AssertEquals(deferredLogger.CreateSaveLogCount, 0);
			AssertEquals(nonDeferredLogger.CreateSaveLogCount, 0);

			var businessObjectLoggingService = Factory.ServiceContainer.GetAfterOnSavingService<BusinessObjectLoggingService>();
			AssertNull(businessObjectLoggingService);
		}

		#endregion

		#region Implementation

		protected override Type TypeOfDummy
		{
			get { return typeof(DummyEnterpriseBusinessObject); }
		}

		#endregion
	}
}
