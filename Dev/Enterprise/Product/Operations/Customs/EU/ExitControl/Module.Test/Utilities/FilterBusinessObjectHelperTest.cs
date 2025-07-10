using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.ExitControl.Module.Testing
{
	sealed class FilterBusinessObjectHelperTest : TestCaseWithFactory
	{
		public void TestGetJobNumberQuery()
		{
			var header1 = Factory.New<CusExitHeader>();
			header1.CXH_JobReference = "E0172821";
			var header2 = Factory.New<CusExitHeader>();
			header2.CXH_JobReference = "E0932878";
			var header3 = Factory.New<CusExitHeader>();
			header3.CXH_JobReference = "B0718221";

			Factory.Save();

			BusinessObject[] filteredDecs = null;

			CombineAssertions(() =>
			{
				var filter = new ExitControlFilterBusinessObject();
				var jobNumberFilter = (ModuleTextFilter)filter[ExitControlFilterBusinessObject.FilterConstants.JobNumber];
				jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				jobNumberFilter.IsActive = true;
				jobNumberFilter.Property = " E0172821 , E0932878 ";
				filteredDecs = Factory.Load(typeof(CusExitHeader), filter.Filter);
				Assert("Should find 2 records but found " + filteredDecs.Length, filteredDecs.Length == 2);

				jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				jobNumberFilter.Property = " E0172 , E0932 ";
				filteredDecs = Factory.Load(typeof(CusExitHeader), filter.Filter);
				Assert("Should find 2 records but found " + filteredDecs.Length, filteredDecs.Length == 2);

				jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				jobNumberFilter.Property = " 21 , 78 ";
				filteredDecs = Factory.Load(typeof(CusExitHeader), filter.Filter);
				Assert("Should find 3 records but found " + filteredDecs.Length, filteredDecs.Length == 3);

				jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				jobNumberFilter.Property = " EE , B ";
				filteredDecs = Factory.Load(typeof(CusExitHeader), filter.Filter);
				Assert("Should find 1 record but found " + filteredDecs.Length, filteredDecs.Length == 1);
			});
		}
	}
}
