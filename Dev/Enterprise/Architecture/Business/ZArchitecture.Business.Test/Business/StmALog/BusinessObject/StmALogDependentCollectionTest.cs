using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmALogDependentCollection))]
	sealed class StmALogDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmALogDependentCollection(Factory.New<DummyEnterpriseBusinessObject>());
		}

		[TestDate(2010, 10, 13)]
		public void TestOverrideAddNew()
		{
			var dummyBizO = Factory.New<DummyEnterpriseBusinessObject>();
			var collection = new StmALogDependentCollection(dummyBizO);
			var log = collection.AddNew(Events.EditedARecord);
			AssertEquals("Event Type should be the same", Events.EditedARecordCode, log.SL_SE_NKEvent);
			AssertEquals("Event Time's default Value should set to ZDateTime.Now", ZDateTime.Now, log.SL_EventTime);
			AssertEquals("Event Reference should be ZString.Empty by default", ZString.Empty, log.SL_Reference);

			log = collection.AddNew(Events.EditedARecord, new ZDateTimeOffset(1981, 10, 22));
			AssertEquals("Event Type should be the same", Events.EditedARecordCode, log.SL_SE_NKEvent);
			AssertEquals("Event Time should be the same", new ZDateTime(1981, 10, 22), log.SL_EventTime);
			AssertEquals("Event Reference should be ZString.Empty by default", ZString.Empty, log.SL_Reference);

			log = collection.AddNew(Events.EditedARecord, "Reference", new ZDateTimeOffset(1981, 10, 22));
			AssertEquals("Event Type should be the same", Events.EditedARecordCode, log.SL_SE_NKEvent);
			AssertEquals("Event Time should be the same", new ZDateTime(1981, 10, 22), log.SL_EventTime);
			AssertEquals("Event Reference should be the same", "Reference", log.SL_Reference);

			var longString = CreateVeryLongString("Base");

			log = collection.AddNew(Events.EditedARecord, longString, new ZDateTimeOffset(1981, 10, 22));
			AssertEquals("Event Reference should be the same", log.SL_ReferenceInfo.MaxLength, log.SL_Reference.Length);
		}

		public void TestCancelAll()
		{
			var dummyBusinessObject = Factory.New<DummyEnterpriseBusinessObject>();
			var collection = new StmALogDependentCollection(dummyBusinessObject);
			var log = collection.AddNew();
			Assert(!log.SL_IsCancelled);
			collection.CancelAll();
			Assert(log.SL_IsCancelled);
		}

		public void TestMasterIsSetOnCreateAndLoad()
		{
			var dummyBusinessObject = Factory.New<DummyEnterpriseBusinessObject>();
			var collection = new StmALogDependentCollection(dummyBusinessObject);
			var log = collection.AddNew();
			AssertEquals("Master after create", dummyBusinessObject, log.Master);

			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var dummyBusinessObject2 = factory2.Load<DummyEnterpriseBusinessObject>(dummyBusinessObject.PK);
			var collection2 = new StmALogDependentCollection(dummyBusinessObject2);
			collection2.Load();
			AssertEquals("Master after load", dummyBusinessObject2, collection2[0].Master);
		}

		public void TestCollectionIsSortedByPostedTimeDesc()
		{
			var collection = new StmALogDependentCollection(Dummy);
			Factory.Save();

			CreateLogInDB(Dummy);
			System.Threading.Thread.Sleep(1000);
			CreateLogInDB(Dummy);
			System.Threading.Thread.Sleep(1000);
			CreateLogInDB(Dummy);

			Factory.Save();
			collection.Load();

			Assert("Precondition : Times must be different", collection[0].SL_PostedTimeUtc != collection[1].SL_PostedTimeUtc);
			Assert("Precondition : Times must be different", collection[1].SL_PostedTimeUtc != collection[2].SL_PostedTimeUtc);

			Assert(collection[0].SL_PostedTimeUtc > collection[1].SL_PostedTimeUtc);
			Assert(collection[1].SL_PostedTimeUtc > collection[2].SL_PostedTimeUtc);
		}

		#region Implementation

		void CreateLogInDB(BusinessObject parent)
		{
			var savingFactory = new BusinessObjectFactory();
			var log = savingFactory.New<StmALog>();
			savingFactory.ImportFromAnotherFactory(parent);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = parent.PK;
				log.SL_Table = parent.TableName;
			}
			savingFactory.Save();
		}

		string CreateVeryLongString(string baseString)
		{
			var stringBuilder = new ZStringBuilder(baseString);
			for (var i = 0; i < 1000; i++)
			{
				stringBuilder.Append(baseString);
			}
			return stringBuilder.ToString();
		}

		DummyEnterpriseBusinessObject Dummy;
		protected override void SetUp()
		{
			Dummy = Factory.New<DummyEnterpriseBusinessObject>();
			base.SetUp();
		}
		#endregion
	}
}
