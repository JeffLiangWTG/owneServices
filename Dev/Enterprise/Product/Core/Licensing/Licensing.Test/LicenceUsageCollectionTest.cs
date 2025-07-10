using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Licensing.Testing
{
	[TestedType(typeof(LicenceUsageCollection))]
	sealed class LicenceUsageCollectionTest : ActiveBusinessObjectCollectionTestCase<LicenceUsageCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			LicenceUsageLog log = Factory.New<LicenceUsageLog>();
			log.S7_ControllerID = LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString();
			log.S7_ParentID = GlbCompany.CurrentCompany.PK;
			return log;
		}

		public void TestFilter()
		{
			LicenceUsageLog log1 = Factory.New<LicenceUsageLog>();
			log1.S7_ControllerID = LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString();

			LicenceUsageLog log2 = Factory.New<LicenceUsageLog>();
			log2.S7_ControllerID = "XYZ";
			log2.S7_ParentID = GlbCompany.CurrentCompany.PK;

			LicenceUsageLog log3 = Factory.New<LicenceUsageLog>();
			log3.S7_ControllerID = LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString();
			log3.S7_ParentID = GlbCompany.CurrentCompany.PK;

			LicenceUsageLog log4 = Factory.New<LicenceUsageLog>();
			log4.S7_ControllerID = LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString();
			log4.S7_ParentID = Factory.New<GlbCompany>().PK;

			LicenceUsageCollection collection = new LicenceUsageCollection(Factory);
			AssertCollectionNotContains(log1, collection);
			AssertCollectionContains(log3, collection);
			AssertCollectionNotContains(log2, collection);
		}

		public void TestFilter_ActualLog()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			var creator = new Enterprise.Licensing.Testing.LicenceConsumptionLogCreatorTest.LicenceConsumptionLogCreatorForTest();

			LicenceCheckpoint checkpoint1;
			using (branch1.SetAsTemporaryContext())
			{
				checkpoint1 = Env.Licence.Accountant;
				creator.CreateLog(checkpoint1, factory2);
			}

			LicenceCheckpoint checkpoint2;
			using (branch2.SetAsTemporaryContext())
			{
				checkpoint2 = Env.Licence.Broker;
				creator.CreateLog(checkpoint2, factory2);
			}

			factory2.Save();

			using (branch1.SetAsTemporaryContext())
			{
				LicenceUsageCollection collection = new LicenceUsageCollection(Factory);
				AssertEquals(1, collection.Count);
				AssertEquals(checkpoint1.Name, collection[0].S7_FormCaption);
			}

			using (branch2.SetAsTemporaryContext())
			{
				LicenceUsageCollection collection = new LicenceUsageCollection(Factory);
				AssertEquals(1, collection.Count);
				AssertEquals(checkpoint2.Name, collection[0].S7_FormCaption);
			}
		}
	}
}
