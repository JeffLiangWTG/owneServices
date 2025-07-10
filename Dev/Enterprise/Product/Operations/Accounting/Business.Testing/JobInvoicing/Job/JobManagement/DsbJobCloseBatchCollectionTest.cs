using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(DsbJobCloseBatchCollection))]
	public class DsbJobCloseBatchCollectionTest : ActiveBusinessObjectCollectionTestCase<DsbJobCloseBatchCollection>
	{
		public void TestRelationshipFilter()
		{
			var collection = GetCollectionToTest();
			AssertEquals(0, collection.Count);

			var batch1 =  Factory.NewWithValidTestData<DsbJobCloseBatch>();
			Factory.Save();
			AssertEquals(1, collection.Count);

			var notCurrentCompanyQuery = new ZQuery(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_Code);
			var notCurrentCompany = Factory.LoadTop1<GlbCompany>(notCurrentCompanyQuery);
			AssertNotNull(notCurrentCompany);
			var batch2 =  Factory.NewWithValidTestData<DsbJobCloseBatch>();
			batch2.JBB_GC = notCurrentCompany.PK;
			Factory.Save();
			AssertEquals(1, collection.Count);

			batch2.JBB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			AssertEquals(2, collection.Count);
		}
	}
}
