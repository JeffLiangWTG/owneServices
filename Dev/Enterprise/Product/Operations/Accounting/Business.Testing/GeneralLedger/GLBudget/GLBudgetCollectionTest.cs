using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLBudget.Testing
{
	[TestedType(typeof(GLBudgetCollection))]
	public class GLBudgetCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GLBudgetCollection(Factory);
		}
		public void TestAdditionalFilter()
		{
			ZQuery differentBranchOfLoginCompanyQuery = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			differentBranchOfLoginCompanyQuery.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK);
			GlbBranch differentBranchOfLoginCompany = Factory.LoadTop1<GlbBranch>(differentBranchOfLoginCompanyQuery);
			GlbBranch branchOfDifferentCompany = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			GLBudget budget1 = Factory.NewWithValidTestData<GLBudget>();
			budget1.AU_GB = GlbBranch.CurrentBranch.PK;

			GLBudget budget2 = Factory.NewWithValidTestData<GLBudget>();
			budget2.AU_GB = differentBranchOfLoginCompany.PK;

			GLBudget budget3 = Factory.NewWithValidTestData<GLBudget>();
			budget3.AU_GB = branchOfDifferentCompany.PK;

			Factory.Save();

			GLBudgetCollection col = (GLBudgetCollection)GetCollectionToTest();
			col.Load();

			AssertEquals("Wrong number of element in the collection", 2, col.Count);
			Assert("budget1 should be loaded", col.Contains(budget1));
			Assert("budget2 should be loaded", col.Contains(budget2));
		}
	}
}
