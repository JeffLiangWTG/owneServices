using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(InvestigationItemResponseOptionCollection))]
	public class InvestigationItemResponseOptionCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestAdd()
		{
			var investigationItem = Factory.NewWithValidTestData<InvestigationItem>();
			var option1 = Factory.NewWithValidTestData<InvestigationItemResponseOption>();
			var option2 = Factory.NewWithValidTestData<InvestigationItemResponseOption>();
			Factory.Save();
			var collection = new InvestigationItemResponseOptionCollection(investigationItem, Factory);
			var pivot1 = collection.AddNew();
			var pivot2 = Factory.New<InvestigationItemResponseOption>();
			collection.Add(pivot2);
			AssertEquals(true, collection.AllowNew);
			AssertEquals(true, collection.AllowRemove);

			AssertEquals("Add should create the FK relationship to triage", investigationItem.PK, pivot1.INR_INV_InvestigationItem);
			AssertEquals("Add should create the FK relationship to triage", investigationItem.PK, pivot2.INR_INV_InvestigationItem);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var investigationItemReloaded = newFactory.Load<InvestigationItem>(investigationItem.PK);
			var collectionReloaded = new InvestigationItemResponseOptionCollection(investigationItemReloaded, newFactory);
			collectionReloaded.Load();
			AssertEquals("2 pivots should exist in the collection", 2, collectionReloaded.Count);
			AssertEquals(true, collectionReloaded.Contains(pivot1.PK));
			AssertEquals(true, collectionReloaded.Contains(pivot2.PK));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var investigationItem = Factory.NewWithValidTestData<InvestigationItem>();
			return new InvestigationItemResponseOptionCollection(investigationItem, Factory);
		}
	}
}
