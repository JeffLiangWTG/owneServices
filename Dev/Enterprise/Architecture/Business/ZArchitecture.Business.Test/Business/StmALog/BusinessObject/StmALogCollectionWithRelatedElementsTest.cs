using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmALogCollectionWithRelatedElements))]
	sealed class StmALogCollectionWithRelatedElementsTest : BusinessObjectCollectionTestCase
	{
		public void TestRemoveAllRelatedElements()
		{
			StmALogCollectionWithRelatedElements collection = new StmALogCollectionWithRelatedElements(Dummy);
			AssertEquals("StmALogCollectionWithRelatedElements.Count", 0, collection.Count);

			StmALog log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = Dummy.PK;
			}
			StmALog relatedLog = Factory.New<StmALog>();

			collection.AddRange(new BusinessObject[] { log, relatedLog });
			AssertEquals("StmALogCollectionWithRelatedElements.Count", 2, collection.Count);

			collection.RemoveAllRelatedElements();
			AssertEquals("StmALogCollectionWithRelatedElements.Count", 1, collection.Count);
			AssertEquals("StmALogCollectionWithRelatedElements should contain the non-related log", log, collection[0]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Dummy = Factory.New<DummyEnterpriseBusinessObject>();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmALogCollectionWithRelatedElements(Factory.New<DummyEnterpriseBusinessObject>());
		}

		DummyEnterpriseBusinessObject Dummy;
	}
}
