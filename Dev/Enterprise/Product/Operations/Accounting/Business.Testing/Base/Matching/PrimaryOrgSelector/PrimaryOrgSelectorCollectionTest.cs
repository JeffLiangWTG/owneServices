using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(PrimaryOrgSelectorCollection))]
	public class PrimaryOrgSelectorCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PrimaryOrgSelectorCollection>
	{
		public void TestFindBySettlementGroupCode()
		{
			PrimaryOrgSelectorCollection collection = new PrimaryOrgSelectorCollection(Factory);
			PrimaryOrgSelector primaryOrg1 = new PrimaryOrgSelector(Factory, TestObjectCreator.AALSHI);
			PrimaryOrgSelector primaryOrg2 = new PrimaryOrgSelector(Factory, TestObjectCreator.ABIGAS);
			collection.Add(primaryOrg1);
			collection.Add(primaryOrg2);
			AssertEquals(primaryOrg1, collection.FindByOrgCode(TestObjectCreator.AALSHI.OH_Code));
			AssertEquals(primaryOrg2, collection.FindByOrgCode(TestObjectCreator.ABIGAS.OH_Code));
		}

		#region Implementation

		protected override PrimaryOrgSelectorCollection GetCollectionToTest()
		{
			return new PrimaryOrgSelectorCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PrimaryOrgSelector(Factory, TestObjectCreator.AALSHI);
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion
	}
}
