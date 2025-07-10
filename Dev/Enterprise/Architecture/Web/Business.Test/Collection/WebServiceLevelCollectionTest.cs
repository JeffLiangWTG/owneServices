using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class WebServiceLevelCollectionTest : TestCaseWithFactory
	{
		public void TestCombineWithPublishedFilter() //Cannot test anything else as WebEnv.AppInstance is null while testing
		{
			AssertEquals(ZQuery.NoResultQuery, WebServiceLevelCollection.CombineWithPublishedFilter(new ZQuery(RefServiceLevelSchema.RS_Code, "D2D"), JoinCondition.And));
			AssertEquals(new ZQuery(RefServiceLevelSchema.RS_Code, "D2D").LiteralTextSqlFormatted, WebServiceLevelCollection.CombineWithPublishedFilter(new ZQuery(RefServiceLevelSchema.RS_Code, "D2D"), JoinCondition.Or).LiteralTextSqlFormatted);
		}

		public void TestCollectionFilter_ShouldContainOneFilterPartOnly()
		{
			WebServiceLevelCollection.LoggedInOrgHeaderForTest = Factory.New<OrgHeader>();

			try
			{
				Assert("Should have at least 5 service levels defined", WebServiceLevelCollection.LoggedInOrgHeaderForTest.OrgServiceLevels.Count >= 5);

				var collection = new WebServiceLevelCollection(Factory);
				AssertStartsWith("Filter should use IN clause rather than a bunch of ORs, otherwise we can get a StackOverflowException in System.Data.DataExpression.Invoke from RowFilterComparer",
					"(" + RefServiceLevelSchema.RS_Code.Name + " in (", collection.CompleteFilter.LiteralTextADO);
			}
			finally
			{
				WebServiceLevelCollection.LoggedInOrgHeaderForTest = null;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			isWebInitially = Globals.IsWeb;
			Globals.IsWeb = true;
		}

		protected override void TearDown()
		{
			Globals.IsWeb = isWebInitially;
			base.TearDown();
		}

		bool isWebInitially;
	}
}
