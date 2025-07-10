using System;
using System.Data;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[UseSnapshotProtection]
	sealed class ObjectChangeSetTest : TestCase
	{
		BusinessObjectFactory factory;

		DummyBusinessObject sessionInstance;
		DummyBusinessObject databaseInstance;

		ObjectChangeSet changeSet;

		protected override void SetUp()
		{
			factory = new BusinessObjectFactory();

			sessionInstance = factory.New<DummyBusinessObject>();
			sessionInstance.Z0_DateTimeOffset = ZDateTimeOffset.UtcNow;
			sessionInstance.Z0_Date = ZDateTime.UtcNow;
			sessionInstance.Z0_SmallDateTime = ZDateTime.UtcNow;
			factory.Save();

			databaseInstance = new BusinessObjectFactory().Load<DummyBusinessObject>(sessionInstance.PK);
		}

		IApplicationSchemaResolver GetNewSchemaResolver() => ObjectFactory.Get<IApplicationSchemaResolver>();

		ObjectChangeSet Create()
		{
			var result = new ObjectChangeSet(sessionInstance, databaseInstance, GetNewSchemaResolver());
			result.Populate();
			return result;
		}

		class DummyChildBusinessObjectWithExtraPropertyForRelatedDummy : DummyChildBusinessObject
		{
			public DummyChildBusinessObjectWithExtraPropertyForRelatedDummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString RelatdDummyName
			{
				get
				{
					return RelatedDummy != null ? RelatedDummy.Z0_NVarChar : ZString.Empty;
				}
			}

			public virtual ZPropertyInfo RelatdDummyNameInfo
			{
				get { return GetWrappedZPropertyInfo(nameof(RelatdDummyName), x => RelatedDummy != null ? RelatedDummy.Z0_NVarCharInfo : null); }
			}
		}

		class DummyBusinessObjectForDelete : DummyBusinessObject
		{
			public DummyBusinessObjectForDelete(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void DeleteForDataRefresh()
			{
				return;
			}
		}

		public void TestDeleteWithRefreshing1()
		{
			var parentObject = factory.New<DummyBusinessObject>();
			var childObject = factory.New<DummyChildBusinessObjectWithExtraPropertyForRelatedDummy>();
			childObject.Z0_Guid = parentObject.PK;
			factory.Save();

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var parentOnDifferentFactory = otherFactory.Load<DummyBusinessObject>(parentObject.PK);
			var childOnOtherFactory = otherFactory.Load<DummyChildBusinessObjectWithExtraPropertyForRelatedDummy>(childObject.PK);
			childOnOtherFactory.Delete();
			parentOnDifferentFactory.Delete();
			otherFactory.Save();

			childObject.Z0_AnotherNumber = 5;

			var dummy = new ObjectChangeSet(childObject, null, GetNewSchemaResolver());
			if (dummy.IsExistsInDatabase)
			{
				dummy.Populate();
			}

			//If used with Reload(): this test will fail with DeveloperNotificationException "Shouldn't be accessing the property of a deleted object
			AssertNoExceptionThrown(() => dummy.Delete());
		}

		public void TestDeleteWithRefreshing2()
		{
			var parentObject = factory.New<DummyBusinessObject>();
			var childObject = factory.New<DummyChildBusinessObjectWithExtraPropertyForRelatedDummy>();
			childObject.Z0_Guid = parentObject.PK;
			factory.Save();

			var secondFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var parentOnSecondFactory = secondFactory.Load<DummyBusinessObject>(parentObject.PK);
			var childOnSecondFactory = secondFactory.Load<DummyBusinessObject>(childObject.PK);
			secondFactory.Save();

			childOnSecondFactory.Z0_AnotherNumber = 10;

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var parentOnDifferentFactory = otherFactory.Load<DummyBusinessObject>(parentObject.PK);
			var childOnOtherFactory = otherFactory.Load<DummyChildBusinessObjectWithExtraPropertyForRelatedDummy>(childObject.PK);
			var child2OnOtherFactory = otherFactory.Load<DummyBusinessObject>(childObject.PK);
			childOnOtherFactory.Z0_AnotherNumber = 25;

			secondFactory.Save();

			var dummy1 = new ObjectChangeSet(childOnOtherFactory, null, GetNewSchemaResolver());
			if (dummy1.IsExistsInDatabase)
			{
				dummy1.Populate();
			}

			var dummy2 = new ObjectChangeSet(child2OnOtherFactory, null, GetNewSchemaResolver());
			if (dummy2.IsExistsInDatabase)
			{
				dummy2.Populate();
			}

			dummy1.Delete();
			// If used without checking rowState: test will fail with RowNotInTableException for second changeSet.
			AssertNoExceptionThrown(() => dummy2.Delete());
		}

		[ExpectNoExceptions]
		public void TestDeleteWithRefreshing3()
		{
			var obj = factory.NewWithValidTestData<DummyBusinessObject>();
			obj.Z0_NVarChar = "one";
			factory.Save();
			var objReloaded = new BusinessObjectFactory() { RefreshEnabled = false }.Load<DummyBusinessObject>(obj.PK);
			objReloaded.Delete();
			objReloaded.Factory.Save();

			obj.Z0_NVarChar = "two";

			// If used without either Reload() or AcceptChanges() the test will fail
			AssertNoExceptionThrown(() => ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null, true));
			Assert("Delete completed by ConcurrencyResolver", obj.IsDeleted);
		}

		public void TestPopulatedOnlyWithPropertiesThatWereChangedInDatabase()
		{
			databaseInstance.Z0_Description = "CHANGED";
			databaseInstance.Row.AcceptChanges();

			changeSet = Create();
			Assert(changeSet.IsModifiedInDatabase);

			Assert(changeSet.NonMergeableProperties.Count == 0);
			Assert(changeSet.MergeableProperties.Count == 1);
			Assert(changeSet.MergeableProperties[0].ColumnName == DummyBizoSchema.Z0_Description.Name);
		}

		public void TestIsExistsInDatabase()
		{
			changeSet = Create();
			Assert(changeSet.IsExistsInDatabase);

			changeSet = new ObjectChangeSet(sessionInstance, null, GetNewSchemaResolver());
			Assert(!changeSet.IsExistsInDatabase);
		}

		public void TestIsModifiedInDatabase()
		{
			sessionInstance.Z0_Description = "CHANGED";

			changeSet = Create();
			Assert(!changeSet.IsModifiedInDatabase);

			databaseInstance.Z0_Description = "CHANGED";
			databaseInstance.Row.AcceptChanges();

			changeSet = Create();
			Assert(changeSet.IsModifiedInDatabase);
		}

		public void TestDisplayName()
		{
			changeSet = Create();
			Assert(changeSet.DisplayName == sessionInstance.HumanReadableName);
		}

		public void TestCriticalProperties()
		{
			changeSet = Create();
			Assert(changeSet.NonMergeableProperties.Count == 0);

			ConcurrencyInfo.SetConcurrencyPolicy(sessionInstance.Row, "Z0_Description", ConcurrencyPolicy.Strict);
			databaseInstance.Z0_Description = "CHANGED";
			databaseInstance.Row.AcceptChanges();

			changeSet = Create();

			Assert(changeSet.NonMergeableProperties.Count == 1);
			Assert(changeSet.NonMergeableProperties[0].PropertyInfo == sessionInstance.Z0_DescriptionInfo);
		}

		public void TestNonCriticalProperties()
		{
			changeSet = Create();
			Assert(changeSet.MergeableProperties.Count == 0);

			sessionInstance.Z0_Code = "CHANG";
			databaseInstance.Z0_Description = "CHANGED";
			databaseInstance.Row.AcceptChanges();

			changeSet = Create();
			Assert(changeSet.MergeableProperties.Count == 1); // session change should be ignored
		}

		public void TestLastModifiedNoLoggingNoSystemLastEditTime()
		{
			var factory = new BusinessObjectFactory();
			var sessionInstance = factory.NewWithValidTestData<DummyBusinessObject>();
			factory.Save();

			var databaseInstance = new BusinessObjectFactory().Load<DummyBusinessObject>(sessionInstance.PK);

			changeSet = new ObjectChangeSet(sessionInstance, databaseInstance, GetNewSchemaResolver());
			changeSet.Populate();

			AssertEquals("Last Modified should not return a date since no logging or system last edit time exists.", string.Empty, changeSet.LastModified);
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestLastModifiedNoLoggingButHasSystemLastEditTime()
		{
			factory = new BusinessObjectFactory();
			var sessionInstance = factory.NewWithValidTestData<DummyBusinessObject>();
			factory.Save();

			var databaseInstance = new BusinessObjectFactory().Load<DummyBusinessObject>(sessionInstance.PK);
			databaseInstance.Row.Table.Columns.Add("Z0_SystemLastEditTimeUtc", typeof(DateTime));
			databaseInstance.Row.Table.Columns.Add("Z0_SystemLastEditUser", typeof(string));
			databaseInstance.Row["Z0_SystemLastEditTimeUtc"] = new DateTime(2020, 1, 20, 8, 30, 30);
			databaseInstance.Row["Z0_SystemLastEditUser"] = "HP";

			changeSet = new ObjectChangeSet(sessionInstance, databaseInstance, GetNewSchemaResolver());
			changeSet.Populate();

			AssertEndsWith("Last modified time is incorrect.", "@ 20 Jan 2020 08:30:30", changeSet.LastModified);
		}

		class DummyLoggingBusinessObject : DummyBusinessObject, IAutoLog
		{
			public DummyLoggingBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			bool IAutoLog.IsAutoLogOnlyEnabledForACT => true;

			bool IAutoLog.IsAutoAdminBusinessObjectLoggerEnabled => true;
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestLastModifiedLoggingButNoSystemLastEditTime()
		{
			var factory = new BusinessObjectFactory();
			var sessionInstance = factory.NewWithValidTestData<DummyLoggingBusinessObject>();
			factory.Save();
			var databaseInstance = new BusinessObjectFactory().Load<DummyLoggingBusinessObject>(sessionInstance.PK);

			var sql =
				"INSERT " + StmALogSchema.Constants.SqlSchemaName + "." + StmALogSchema.Constants.TableName +
				"(" + StmALogSchema.Constants.PK +
				"," + StmALogSchema.Constants.SL_Table +
				"," + StmALogSchema.Constants.SL_Parent +
				"," + StmALogSchema.Constants.SL_IsEstimate +
				"," + StmALogSchema.Constants.SL_IsCancelled +
				"," + StmALogSchema.Constants.SL_Reference +
				"," + StmALogSchema.Constants.SL_EventTime +
				"," + StmALogSchema.Constants.SL_GS_NKUser +
				"," + StmALogSchema.Constants.SL_SE_NKEvent +
				"," + StmALogSchema.Constants.SL_PostedTimeUtc +
				"," + StmALogSchema.Constants.SL_GB_NKBranch +
				"," + StmALogSchema.Constants.SL_GE_NKDepartment +
				") VALUES (" +
				"NEWID(), @SL_Table, @SL_Parent, @SL_IsEstimate, @SL_IsCancelled," +
				"@SL_Reference, @SL_EventTime, @SL_GS_NKUser, @SL_SE_NKEvent, @SL_PostedTimeUtc, @SL_GB_NKBranch, @SL_GE_NKDepartment)";

			Db.Connection.ExecuteNonQuery(sql, (cmd) =>
			{
				cmd.AddParameterBasedOnDbColumn("@SL_Table", databaseInstance.Row.Table.TableName, StmALogSchema.SL_Table);
				cmd.AddParameterBasedOnDbColumn("@SL_Parent", ZDataUtils.GetPK(databaseInstance.Row), StmALogSchema.SL_Parent);
				cmd.AddParameterBasedOnDbColumn("@SL_IsEstimate", "N", StmALogSchema.SL_IsEstimate);
				cmd.AddParameterBasedOnDbColumn("@SL_IsCancelled", "N", StmALogSchema.SL_IsCancelled);
				cmd.AddParameterBasedOnDbColumn("@SL_Reference", "Test Reference", StmALogSchema.SL_Reference);
				cmd.AddParameterBasedOnDbColumn("@SL_EventTime", new DateTime(2020, 1, 20, 7, 30, 30), StmALogSchema.SL_EventTime);
				cmd.AddParameterBasedOnDbColumn("@SL_GS_NKUser", "E", StmALogSchema.SL_GS_NKUser);
				cmd.AddParameterBasedOnDbColumn("@SL_SE_NKEvent", "EDT", StmALogSchema.SL_SE_NKEvent);
				cmd.AddParameterBasedOnDbColumn("@SL_PostedTimeUtc", new DateTime(2020, 1, 20, 7, 30, 30), StmALogSchema.SL_PostedTimeUtc);
				cmd.AddParameterBasedOnDbColumn("@SL_GB_NKBranch", "HOG", StmALogSchema.SL_GB_NKBranch);
				cmd.AddParameterBasedOnDbColumn("@SL_GE_NKDepartment", "MAG", StmALogSchema.SL_GE_NKDepartment);
			});

			changeSet = new ObjectChangeSet(sessionInstance, databaseInstance, GetNewSchemaResolver());
			changeSet.Populate();

			AssertNotEquals("Last modified should get information from dbo.StmALog since it has no LastEditTime or LastEditUser columns.", string.Empty, changeSet.LastModified);
			AssertEquals("Last modified time is incorrect.", "CargoWise Support @ 20 Jan 2020 07:30:30", changeSet.LastModified);
		}

		public void TestCanMerge()
		{
			changeSet = Create();
			Assert(changeSet.CanMerge());

			ConcurrencyInfo.SetConcurrencyPolicy(sessionInstance.Row, "Z0_Description", ConcurrencyPolicy.Strict);
			databaseInstance.Z0_Description = "CHANGED";
			databaseInstance.Row.AcceptChanges();

			changeSet = Create();
			Assert(!changeSet.CanMerge());
		}

		public void TestRowStateWhenNoDelete()
		{
			changeSet = Create();
			changeSet.Delete();
			factory.Save();

			AssertEquals(true, sessionInstance.IsDeleted);
			AssertEquals(DataRowState.Detached, sessionInstance.Row.RowState);

			sessionInstance = factory.New<DummyBusinessObjectForDelete>();
			factory.Save();
			sessionInstance.Z0_Date = ZDateTime.Now;
			databaseInstance = new BusinessObjectFactory().Load<DummyBusinessObjectForDelete>(sessionInstance.PK);
			changeSet = Create();
			changeSet.Delete();
			AssertEquals(false, sessionInstance.IsDeleted);
			AssertEquals(DataRowState.Modified, sessionInstance.Row.RowState);
		}

		public void TestCanMergeDeleted()
		{
			changeSet = Create();
			Assert(changeSet.CanMerge());

			sessionInstance.Delete();
			databaseInstance.Z0_Description = "CHANGED";
			databaseInstance.Row.AcceptChanges(); // To prevent using Original version by PropertyRecord

			changeSet = Create();
			Assert("Should be able to merge deleted bizo with changes with default/mergeable concurrency policy", changeSet.CanMerge());

			ConcurrencyInfo.SetConcurrencyPolicy(sessionInstance.Row, "Z0_Description", ConcurrencyPolicy.Protect);

			changeSet = Create();
			Assert("Should not merge deleted bizo with db changes in protected properties", !changeSet.CanMerge());
		}
		public void TestMergeForDeletedRecord()
		{
			sessionInstance.Delete();
			databaseInstance.Z0_Description = "CHANGED";
			databaseInstance.Row.AcceptChanges();

			changeSet = Create();
			AssertNoExceptionThrown(() => changeSet.Merge());
		}

		public void TestMerge()
		{
			sessionInstance.Z0_Code = "CHANG";
			databaseInstance.Z0_Description = "CHANGED";
			databaseInstance.Row.AcceptChanges();

			changeSet = Create();
			changeSet.Merge();

			Assert(sessionInstance.Z0_Code == "CHANG");
			Assert(sessionInstance.Z0_Description == "CHANGED");

			sessionInstance.Z0_Description = "Change";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "Method does not accept type parameter")]
		public void TestMerge_LazyLoadedProperty()
		{
			// Make an uncompressible (random) blob so when compressed by the factory
			// it is larger than the Small Blob limit.
			var rand = new Random(1234);
			var bigBlob = new byte[new ZQuery().LoadSmallBlobs * 2];
			for (int i = 0; i < bigBlob.Length; ++i)
			{
				bigBlob[i] = (byte)rand.Next(byte.MaxValue);
			}

			sessionInstance.Z0_VarBinaryMax = new ZBlob(bigBlob);
			factory.Save();

			databaseInstance = (DummyBusinessObject)new BusinessObjectFactory().LoadFromDatabase(typeof(DummyBusinessObject), sessionInstance.PK);
			AssertArrayEqualsByElements("blob is not loaded", LazyLoading.BinaryPlaceholder, (byte[])databaseInstance.Row[DummyBizoSchema.Constants.Z0_VarBinaryMax]);
			var record1 = Create();
			AssertEquals(false, record1.IsModifiedInDatabase);

			sessionInstance.Z0_VarBinaryMax = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var record2 = Create();
			AssertEquals(false, record2.IsModifiedInDatabase);

			((IBusinessObjectInternals)sessionInstance).Row.AcceptChanges();
			var record3 = Create();
			AssertEquals(true, record3.IsModifiedInDatabase);
		}
	}
}
