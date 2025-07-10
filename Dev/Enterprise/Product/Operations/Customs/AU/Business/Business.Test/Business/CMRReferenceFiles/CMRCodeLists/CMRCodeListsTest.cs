using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCodeLists))]
	sealed class CMRCodeListsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadWithFilter()
		{
			var codeList = CMRCodeLists.New(Factory);
			codeList.CI_Code = "ZZZ";

			var filter = new ZQuery(CMRCodeListsSchema.CI_Code, SQLComparisonOperator.Equal, "ZZZ");
			AssertEquals("load with filter", codeList, CMRCodeLists.Load(Factory, filter));

			filter.AddToFilter(JoinCondition.And, CMRCodeListsSchema.CI_CodeType, SQLComparisonOperator.Equal, "XXX");
			AssertNull("Load with filter", CMRCodeLists.Load(Factory, filter));
		}

		protected override BusinessObject GetNewBusinessObject() => CMRCodeLists.New(Factory);
	}
}
