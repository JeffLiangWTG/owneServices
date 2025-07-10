using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AUTariffModuleFilter))]
	sealed class AUTariffModuleFilterTest : ModuleFilterTestCase<AUTariffModuleFilter>
	{
		public void TestFormatter()
		{
			Filter.CC_TariffNum = "12345678";
			AssertEquals(Filter.CC_TariffNum, "1234.56.78");
			Filter.CC_TariffNum = "1234.56.78";
			AssertEquals(Filter.CC_TariffNum, "1234.56.78");
			Filter.CC_TariffNum = "123456";
			AssertEquals(Filter.CC_TariffNum, "1234.56");
		}

		public void TestQuery()
		{
			Classification exportClass1 = Factory.New<Classification>();
			Classification exportClass2 = Factory.New<Classification>();
			exportClass1.CC_ClassificationType = "EXP";
			exportClass1.CC_TariffNum = "1234.56.78";
			exportClass2.CC_ClassificationType = "EXP";
			exportClass2.CC_TariffNum = "8765.43.21";
			Filter.CC_TariffNum = "12345678";
			Filter.IsActive = true;
			Classification[] found = (Classification[])Factory.Load(typeof(Classification), Filter.Query);
			AssertEquals("There should be one record found", 1, found.Length);
			AssertEquals("1234.56.78", found[0].CC_TariffNum);
			Filter.CC_TariffNum = "87654321";
			found = (Classification[])Factory.Load(typeof(Classification), Filter.Query);
			AssertEquals("There should be one record found", 1, found.Length);
			AssertEquals("8765.43.21", found[0].CC_TariffNum);
			Filter.CC_TariffNum = "1234.56.78";
			found = (Classification[])Factory.Load(typeof(Classification), Filter.Query);
			AssertEquals("There should be one record found", 1, found.Length);
			AssertEquals("1234.56.78", found[0].CC_TariffNum);
			Filter.CC_TariffNum = "1234";
			found = (Classification[])Factory.Load(typeof(Classification), Filter.Query);
			AssertEquals("There should be one record found", 1, found.Length);
			AssertEquals("1234.56.78", found[0].CC_TariffNum);
			Filter.CC_TariffNum = "1235";
			found = (Classification[])Factory.Load(typeof(Classification), Filter.Query);
			AssertEquals("There should be no records found", 0, found.Length);
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override AUTariffModuleFilter GetNewModuleFilter() => new AUTariffModuleFilter("Tariff No", TariffModuleFilterType.Export, new AUExportTariffFormatter());

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		protected override ZString ExpectedDescription => "Tariff No";
	}
}
