using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;
using AccGenericConsol = Enterprise.Accounting.Business.GenericConsol.GenericConsol;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceConsolCollection))]
	public class APInvoiceConsolCollectionTest : MainFormGenericConsolCollectionBOCollectionTest<AccGenericConsol>
	{
		public override void TestIndexer()
		{
			APInvoiceConsolCollection consols = new APInvoiceConsolCollection(Factory);
			AccGenericConsol consol1 = consols.AddNew();
			AssertEquals(consol1, consols[0]);

			AccGenericConsol consol2 = consols.AddNew();
			AssertEquals(consol2, consols[1]);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new APInvoiceConsolCollection(Factory);
		}

		public override void TestDelete()
		{
			base.TestRemoveFromRelationship();
		}

		public void TestRelationshipFilter()
		{
			ForwardingConsol forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			CommonConsol cfsConsol = (CommonConsol)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Freight.Integration.CFS.ICFSLoadListConsol)));
			Factory.Save();

			Collection.Load();
			AssertEquals("Collection should contain 1 element", 1, Collection.Count);
			Assert("The element should be INInv", Collection.Contains(forwardingConsol));
		}
	}
}
