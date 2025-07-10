using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Module.Testing
{
	[TestedType(typeof(AddInfoModuleBooleanFilter))]
	public class AddInfoModuleBooleanFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new AddInfoModuleBooleanFilter("TestDescription", (c, v) => new ZQuery(), new CodeDescriptionPairList());

		public void TestConstructor()
		{
			GetTextQueryWithOperator testQuery = (c, v) => new ZQuery();
			var tester = new AddInfoModuleBooleanFilter("DESC", testQuery, new CodeDescriptionPairList());
			AssertNotNull(tester);
			AssertEquals(testQuery, tester.QueryDelegate);
		}
	}
}
