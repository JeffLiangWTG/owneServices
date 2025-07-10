using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	[TestedType(typeof(BusinessObjectLogger))]
	sealed class BusinessObjectLoggerTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestExceptionIsNotThrownIfTopLevelBusinessObjectIsNotTopLevel()
		{
			BusinessObjectLogger logger = new BusinessObjectLogger((IStmALogParent)Dummy.Classification);
		}

		public void TestReferenceLength()
		{
			AssertEquals("ReferenceInfo.MaxLength", StmALogSchema.SL_Reference.MaxLength - 5, LoggerWithSameTopLevel.ReferenceInfo.MaxLength);
		}

		public void TestTopLevelBusinessObjectHasChanges()
		{
			Dummy.ZL2_Description = "x";
			AssertEquals("TopLevelBusinessObjectHasChanges", true, LoggerWithDifferentTopLevel.TopLevelBusinessObjectHasChanges);
		}

		public void TestRevisedBusinessObjectHasChanges()
		{
			Dummy.Classification[CusClassificationSchema.Constants.CC_Description] = new ZString("x");
			AssertEquals("RevisedBusinessObjectHasChanges", true, LoggerWithDifferentTopLevel.RevisedBusinessObjectHasChanges);
		}

		#region Write To Log

		[ExpectExceptionMessage(typeof(InvalidOperationException), "The topLevelBusinessObject has changes. Writing to Log aborted.")]
		public void TestCannotWriteToLogIfTopLevelBusinessObjectHasChanges()
		{
			Dummy.ZL2_Description = "x";
			LoggerWithDifferentTopLevel.WriteToLog();
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "The revisedBusinessObject has changes. Writing to Log aborted.")]
		public void TestCannotWriteToLogIfRevisedBusinessObjectHasChanges()
		{
			DummyAutoLogged dummy1 = Factory.New<DummyAutoLogged>();
			dummy1.IsTopLevel = true;
			dummy1.ZL2_Description = "x";
			Factory.Save();

			DummyAutoLogged dummy2 = Factory.New<DummyAutoLogged>();

			BusinessObjectLogger logger = new BusinessObjectLogger(dummy1, dummy2);
			logger.WriteToLog();
		}

		public void TestConcurrencyErrorWhileLogging_SameTopLevel()
		{
			Dummy.Classification.IsTopLevel = true;
			Dummy.Classification[CusClassificationSchema.Constants.CC_Description] = new ZString("x");
			Dummy.Classification[CusClassificationSchema.Constants.CC_ClassificationType] = new ZString("BTH");
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;

			BusinessObject loadedClassification = (BusinessObject)newFactory.Load<Enterprise.Integration.Customs.IBaseCusClassification>(Dummy.Classification.PK);
			loadedClassification.Delete();

			newFactory.Save();

			BusinessObjectLogger logger = new BusinessObjectLogger((IStmALogParent)Dummy.Classification);
			AssertEquals("WriteToLog()", false, logger.WriteToLog());
		}

		[TestDate(2050, 1, 31, 12, 1, 2)]
		public void TestWriteToLog_SameTopLevel()
		{
			Dummy.ZL2_Description = "x";
			Factory.Save();

			ZDateTime initialSystemCreateTime = Dummy.ZL2_SystemCreateTimeUtc;
			ZString initialSystemCreateUser = Dummy.ZL2_SystemCreateUser;
			ZDateTime initialSystemLastEditTime = Dummy.ZL2_SystemLastEditTimeUtc;
			ZString initialSystemLastEditUser = Dummy.ZL2_SystemLastEditUser;

			int initialLogCount = Dummy.Logs.GetAllLogs().Count;

			LoggerWithSameTopLevel.Reference = "Ouch.";
			AssertEquals("WriteToLog()", true, LoggerWithSameTopLevel.WriteToLog());

			StmALogDependentCollection logs = Dummy.Logs.GetAllLogs();
			AssertEquals("Only one log should be added.", initialLogCount + 1, logs.Count);

			StmALog[] reviseLogs = (StmALog[])logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.RecordAudited.Code));
			AssertEquals("A 'Record Audited' log should have been created.", 1, reviseLogs.Length);
			AssertEquals("Log Reference", "Ouch.", reviseLogs[0].SL_Reference);
			AssertEquals("Log should be saved.", true, reviseLogs[0].IsInDatabase);

			AssertEquals("Dummy.ZL2_SystemCreateTimeUtc", initialSystemCreateTime, Dummy.ZL2_SystemCreateTimeUtc);
			AssertEquals("Dummy.ZL2_SystemCreateUser", initialSystemCreateUser, Dummy.ZL2_SystemCreateUser);
			AssertEquals("Dummy.ZL2_SystemLastEditTimeUtc", initialSystemLastEditTime, Dummy.ZL2_SystemLastEditTimeUtc);
			AssertEquals("Dummy.ZL2_SystemLastEditUser", initialSystemLastEditUser, Dummy.ZL2_SystemLastEditUser);
		}

		[TestDate(2050, 1, 31, 12, 1, 2)]
		public void TestWriteToLog_DifferentTopLevel()
		{
			Dummy.ZL2_Description = "x";
			Dummy.Classification[CusClassificationSchema.Constants.CC_Description] = new ZString("x");
			Dummy.Classification[CusClassificationSchema.Constants.CC_ClassificationType] = new ZString("BTH");
			Factory.Save();

			AssertEquals("Classification.CC_LastAuditedDate", ZDateTime.Empty, Dummy.Classification[CusClassificationSchema.Constants.CC_LastAuditedDate]);
			AssertEquals("Classification.CC_LastAuditedUser", "", Dummy.Classification[CusClassificationSchema.Constants.CC_LastAuditedUser]);

			int initialLogCount = Dummy.Classification.GetLogs().GetAllLogs().Count;

			LoggerWithDifferentTopLevel.Reference = "Ow.";
			AssertEquals("WriteToLog()", true, LoggerWithDifferentTopLevel.WriteToLog());

			StmALogDependentCollection logs = Dummy.Classification.GetLogs().GetAllLogs();
			AssertEquals("Only one log should be added.", initialLogCount + 1, logs.Count);

			StmALog[] reviseLogs = (StmALog[])logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.RecordAudited.Code));
			AssertEquals("A 'Record Audited' log should have been created.", 1, reviseLogs.Length);
			AssertEquals("Log Reference", "Ow.", reviseLogs[0].SL_Reference);
			AssertEquals("Log should be saved.", true, reviseLogs[0].IsInDatabase);

			AssertEquals("Dummy.Classification.CC_LastAuditedDate", TestDateAttribute.Date, Dummy.Classification[CusClassificationSchema.Constants.CC_LastAuditedDate]);
			AssertEquals("Dummy.Classification.CC_LastAuditedUser", StaticCurrentFetcher.Instance.CurrentUser.GS_Code, Dummy.Classification[CusClassificationSchema.Constants.CC_LastAuditedUser]);
		}

		[TestDate(2050, 1, 31, 12, 1, 2)]
		public void TestWriteToLogProcessAccordingToSpecifiedLogOptions()
		{
			Dummy.ZL2_Description = "x";
			Dummy.Classification[CusClassificationSchema.Constants.CC_Description] = new ZString("x");
			Dummy.Classification[CusClassificationSchema.Constants.CC_ClassificationType] = new ZString("BTH");
			Factory.Save();

			AssertEquals("Classification.CC_LastAuditedDate", ZDateTime.Empty, Dummy.Classification[CusClassificationSchema.Constants.CC_LastAuditedDate]);
			AssertEquals("Classification.CC_LastAuditedUser", "", Dummy.Classification[CusClassificationSchema.Constants.CC_LastAuditedUser]);

			int initialLogCount = Dummy.Classification.GetLogs().GetAllLogs().Count;

			BusinessObjectLogger logger = new BusinessObjectLogger(Dummy, Dummy.Classification, new BusinessObjectLoggerOptions("TST", false, Events.RecordAudited));

			logger.Reference = "Ow.";
			AssertEquals("WriteToLog()", true, logger.WriteToLog());

			StmALogDependentCollection logs = Dummy.Classification.GetLogs().GetAllLogs();
			AssertEquals("Only one log should be added.", initialLogCount + 1, logs.Count);

			StmALog[] reviseLogs = (StmALog[])logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.RecordAudited.Code));
			AssertEquals("A 'Record Audited' log should have been created.", 1, reviseLogs.Length);
			AssertEquals("Log Reference", "~TST:Ow.", reviseLogs[0].SL_Reference);
			AssertEquals("Log should be saved.", true, reviseLogs[0].IsInDatabase);

			AssertEquals("Dummy.Classification.CC_LastAuditedDate should not have been updated", ZDateTime.Empty, Dummy.Classification[CusClassificationSchema.Constants.CC_LastAuditedDate]);
			AssertEquals("Dummy.Classification.CC_LastAuditedUser should not have been updated", "", Dummy.Classification[CusClassificationSchema.Constants.CC_LastAuditedUser]);
		}

		[TestDate(2050, 09, 23, 11, 2, 1)]
		public void TestWriteToLog_WithLastLogProperties()
		{
			var testBizObj = Factory.New<DummyWithLastLog>();
			Factory.Save();

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WarehouseJobEntered.Code);

			// No logs yet
			var logs = (StmALog[])testBizObj.Logs.GetAllLogs().Find(logQuery);
			AssertEquals("[PRE-CONDITION] Number of Logs", 0, logs.Length);
			AssertEquals("[PRE-CONDITION] ZL2_DummyLastLogDate", ZDateTime.Invalid, testBizObj.ZL2_DummyLastLogDate);
			AssertEquals("[PRE-CONDITION] ZL2_DummyLastLogReference", ZString.Empty, testBizObj.ZL2_DummyLastLogReference);
			AssertEquals("[PRE-CONDITION] ZL2_DummyLastLogUser", ZString.Empty, testBizObj.ZL2_DummyLastLogUser);

			testBizObj.IsTopLevel = true;
			var loggerOptions = new BusinessObjectLoggerOptions("Hi", "ZL2_Dummy", Events.WarehouseJobEntered);
			var bizObjLogger = new BusinessObjectLogger(testBizObj, testBizObj, loggerOptions);
			bizObjLogger.Reference = "Girl!";
			bizObjLogger.WriteToLog();

			// 1 log created
			logs = (StmALog[])testBizObj.Logs.GetAllLogs().Find(logQuery);
			AssertEquals("Number of Logs", 1, logs.Length);
			AssertEquals("logs[0].SL_GS_NKUser", TestDateAttribute.Date, logs[0].SL_EventTime);
			AssertEquals("logs[0].SL_Reference", "~Hi:Girl!", logs[0].SL_Reference);
			AssertEquals("logs[0].SL_GS_NKUser", StaticCurrentFetcher.Instance.CurrentUserCode, logs[0].SL_GS_NKUser);
			AssertEquals("ZL2_DummyLastLogDate", TestDateAttribute.Date, testBizObj.ZL2_DummyLastLogDate);
			AssertEquals("ZL2_DummyLastLogReference", "Girl!", testBizObj.ZL2_DummyLastLogReference.Trim());
			AssertEquals("ZL2_DummyLastLogUser", StaticCurrentFetcher.Instance.CurrentUserCode, testBizObj.ZL2_DummyLastLogUser);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(7654321);
			bizObjLogger.Reference = "Man!";
			bizObjLogger.WriteToLog();

			// Another log created
			logs = (StmALog[])testBizObj.Logs.GetAllLogs().Find(logQuery);
			AssertEquals("Number of Logs", 2, logs.Length);

			// Latest log
			logQuery.AddToFilter(StmALogSchema.SL_EventTime, TestDateAttribute.Date);
			var latesLog = (StmALog)testBizObj.Logs.GetAllLogs().Find(logQuery)[0];
			AssertEquals("latesLog.SL_GS_NKUser", TestDateAttribute.Date, latesLog.SL_EventTime);
			AssertEquals("latesLog.SL_Reference", "~Hi:Man!", latesLog.SL_Reference);
			AssertEquals("latesLog.SL_GS_NKUser", StaticCurrentFetcher.Instance.CurrentUserCode, latesLog.SL_GS_NKUser);
			AssertEquals("ZL2_DummyLastLogDate", TestDateAttribute.Date, testBizObj.ZL2_DummyLastLogDate);
			AssertEquals("ZL2_DummyLastLogReference", "Man!", testBizObj.ZL2_DummyLastLogReference.Trim());
			AssertEquals("ZL2_DummyLastLogUser", StaticCurrentFetcher.Instance.CurrentUserCode, testBizObj.ZL2_DummyLastLogUser);
		}

		public void TestValidateReference()
		{
			LoggerWithDifferentTopLevel.Reference = "~BLAH";
			AssertHasError(LoggerWithDifferentTopLevel.ReferenceInfo, BusinessObjectLogger.EnteredReferenceStartingWithSystemFormat);

			LoggerWithDifferentTopLevel.Reference = "BLAH~";
			AssertNoError(LoggerWithDifferentTopLevel.ReferenceInfo, BusinessObjectLogger.EnteredReferenceStartingWithSystemFormat);
		}

		[TestDate(2025, 3, 6, 0, 0, 0)]
		public void TestNotTopLevelBusinessObjectChildrenDoNotUpdateAuditColumns()
		{
			var bizo = Factory.New<DummyLoggedWithChildren>();
			Factory.Save();

			AssertEquals("Initial edit time", new ZDateTime(2025, 3, 6, 0, 0, 0), bizo.ZL2_SystemLastEditTimeUtc);

			TestDateAttribute.AddMinutes(15);
			bizo.Child.ZL2_Description = "Test";
			Factory.Save();

			AssertEquals("Child edit time updated", new ZDateTime(2025, 3, 6, 0, 15, 0), bizo.Child.ZL2_SystemLastEditTimeUtc);
			AssertEquals("Editing child should not update edit time of the parent", new ZDateTime(2025, 3, 6, 0, 0, 0), bizo.ZL2_SystemLastEditTimeUtc);
		}

		[TestDate(2025, 3, 6, 0, 0, 0)]
		public void TestTopLevelBusinessObjectChildrenUpdateAuditColumns()
		{
			var bizo = Factory.New<DummyLoggedWithChildren>();
			Factory.Save();
			bizo.IsTopLevel = true;

			AssertEquals("Initial edit time", new ZDateTime(2025, 3, 6, 0, 0, 0), bizo.ZL2_SystemLastEditTimeUtc);

			TestDateAttribute.AddMinutes(15);
			bizo.Child.ZL2_Description = "Test";
			Factory.Save();

			AssertEquals("Child edit time updated", new ZDateTime(2025, 3, 6, 0, 15, 0), bizo.Child.ZL2_SystemLastEditTimeUtc);
			AssertEquals("Editing child should update edit time of the top level parent", new ZDateTime(2025, 3, 6, 0, 15, 0), bizo.ZL2_SystemLastEditTimeUtc);
		}

		[TestDate(2025, 3, 6, 0, 0, 0)]
		public void TestUpdateAuditFieldsIfOnlyChildrenHaveChanges()
		{
			var bizo = Factory.New<DummyLoggedUpdateAuditColumns>();
			Factory.Save();

			AssertEquals("Initial edit time", new ZDateTime(2025, 3, 6, 0, 0, 0), bizo.ZL2_SystemLastEditTimeUtc);

			TestDateAttribute.AddMinutes(15);
			bizo.Child.ZL2_Description = "Test";
			Factory.Save();

			AssertEquals("Child edit time updated", new ZDateTime(2025, 3, 6, 0, 15, 0), bizo.Child.ZL2_SystemLastEditTimeUtc);
			AssertEquals("Editing child should update edit time of the parent when UpdateAuditFieldsIfOnlyChildrenHaveChanges is true", new ZDateTime(2025, 3, 6, 0, 15, 0), bizo.ZL2_SystemLastEditTimeUtc);
		}

		#endregion

		#region Test Classes

		class DummyAutoLogged : DummyLogged
		{
			public DummyAutoLogged(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public BusinessObject Classification
			{
				get
				{
					if (classification == null)
					{
						classification = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseCusClassification>();
						classification[CusClassificationSchema.Constants.CC_LookupCode] = Guid.NewGuid().ToString().Replace("-", "");
						RegisterEditableChildObject(classification);
					}
					return classification;
				}
			}
			BusinessObject classification;

			protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
			{
				get
				{
					List<BusinessObject> result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
					result.Add(classification);
					return result.ToArray();
				}
			}

			protected override AutologState AutoLoggingState => AutologState.AutoLogged;
		}

		class DummyWithLastLog : DummyLogged
		{
			public DummyWithLastLog(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZDateTime ZL2_DummyLastLogDate
			{
				get
				{
					var stringResult = ZL2_Description.SubstringSafe(0, 23);
					var result = DateTime.MinValue;
					DateTime.TryParse(stringResult, out result);
					return new ZDateTime(result);
				}
				set
				{
					ZL2_Description =
						CargoWise.Data.SqlFormatInfo.ToSqlDateTimeString(value.ToDateTime()).PadRight(25) +
						ZL2_Description.SubstringSafe(25, 25).PadRight(25) +
						ZL2_Description.SubstringSafe(50, 3).PadRight(3);
				}
			}

			public ZPropertyInfo ZL2_DummyLastLogDateInfo
			{
				get { return GetZPropertyInfo(nameof(ZL2_DummyLastLogDate)); }
			}

			[MaxLength(20)]
			public ZString ZL2_DummyLastLogReference
			{
				get
				{
					return ZL2_Description.SubstringSafe(25, 20);
				}
				set
				{
					ZL2_Description =
						ZL2_Description.SubstringSafe(0, 25).PadRight(25) +
						value.PadRight(25) +
						ZL2_Description.SubstringSafe(50, 3).PadRight(3);
				}
			}

			public ZPropertyInfo ZL2_DummyLastLogReferenceInfo
			{
				get { return GetZPropertyInfo(nameof(ZL2_DummyLastLogReference)); }
			}

			[MaxLength(3)]
			public ZString ZL2_DummyLastLogUser
			{
				get
				{
					return ZL2_Description.SubstringSafe(50, 3);
				}
				set
				{
					ZL2_Description =
						ZL2_Description.SubstringSafe(0, 25).PadRight(25) +
						ZL2_Description.SubstringSafe(25, 20).PadRight(25) +
						value.PadRight(3);
				}
			}

			public ZPropertyInfo ZL2_DummyLastLogUserInfo
			{
				get { return GetZPropertyInfo(nameof(ZL2_DummyLastLogUser)); }
			}
		}

		class DummyLoggedWithChildren : DummyLogged
		{
			public DummyLoggedWithChildren(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DummyLogged Child
			{
				get
				{
					if (child == null)
					{
						child = Factory.New<DummyLogged>();
						RegisterEditableChildObject(child);
					}
					return child;
				}
			}
			DummyLogged child;
		}

		class DummyLoggedUpdateAuditColumns : DummyLoggedWithChildren
		{
			public DummyLoggedUpdateAuditColumns(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override bool UpdateAuditFieldsIfOnlyChildrenHaveChanges => true;
		}

		#endregion

		#region Implementation

		DummyAutoLogged dummy;
		BusinessObjectLogger loggerWithSameTopLevel;
		BusinessObjectLogger loggerWithDifferentTopLevel;

		DummyAutoLogged Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyAutoLogged>();
					dummy.IsTopLevel = true;
				}
				return dummy;
			}
		}

		BusinessObjectLogger LoggerWithSameTopLevel
		{
			get
			{
				if (loggerWithSameTopLevel == null)
				{
					loggerWithSameTopLevel = new BusinessObjectLogger(Dummy);
				}
				return loggerWithSameTopLevel;
			}
		}

		BusinessObjectLogger LoggerWithDifferentTopLevel
		{
			get
			{
				if (loggerWithDifferentTopLevel == null)
				{
					loggerWithDifferentTopLevel = new BusinessObjectLogger(Dummy, Dummy.Classification);
				}
				return loggerWithDifferentTopLevel;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return LoggerWithSameTopLevel;
		}

		#endregion
	}
}
