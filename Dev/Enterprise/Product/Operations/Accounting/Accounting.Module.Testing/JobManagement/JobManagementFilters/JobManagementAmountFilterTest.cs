using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(JobManagementAmountFilter))]
	public class JobManagementAmountFilterTest : ModuleFilterTestCase<JobManagementAmountFilter>
	{
		protected override JobManagementAmountFilter GetNewModuleFilter()
		{
			return new JobManagementAmountFilter("moo", AccTransactionHeaderSchema.AH_ExchangeRate);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override Dictionary<string, IZType> GetDummyValuesForCacheInvalidationTest(JobManagementAmountFilter filter)
		{
			var values = base.GetDummyValuesForCacheInvalidationTest(filter);
			values.Add(nameof(filter.DefaultComparisonOperator), new ZString("less than"));
			values.Add(nameof(filter.Property), new ZDecimal(50m));

			return values;
		}
	}
}
