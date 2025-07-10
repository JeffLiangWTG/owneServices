using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Forwarding.Module.Testing
{
	[TestedType(typeof(AMSBillStatusFilter))]
	class AMSBillStatusFilterTest : ModuleTextFilterTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var filterBO = new DummyFilterBusinessObject();
			return new AMSBillStatusFilter("TEST Filter", GetZQueryForTest, new List<ZString>(), filterBO);
		}

		ZQuery GetZQueryForTest(SQLComparisonOperator filterOperator, ZString value)
		{
			return new ZQuery();
		}
	}
}
