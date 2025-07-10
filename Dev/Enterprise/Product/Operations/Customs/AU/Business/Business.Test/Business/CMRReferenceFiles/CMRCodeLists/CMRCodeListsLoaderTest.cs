using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCodeLists.Loader))]
	sealed class CMRCodeListsLoaderTest : LoaderTestCase
	{
		public void TestLoad()
		{
			var code = Factory.New<CMRCodeLists>();
			code.CI_Code = "111";
			code.CI_CodeType = "GSTX";
			code.CI_Startdate = new ZDateTime(2013, 3, 1);

			var code2 = Factory.New<CMRCodeLists>();
			code2.CI_Code = "112";
			code2.CI_CodeType = "GSTX";
			code2.CI_Startdate = new ZDateTime(2010, 3, 1);
			code2.CI_EndDate = new ZDateTime(2013, 2, 28);

			var code3 = Factory.New<CMRCodeLists>();
			code3.CI_Code = "111";
			code3.CI_CodeType = "GSTE";
			code3.CI_Startdate = new ZDateTime(2013, 3, 1);

			var loader = new CMRCodeLists.Loader(Factory);
			AssertEquals(code, loader.Load("111", "GSTX", new ZDateTime(2013, 3, 1)));
			AssertNull(loader.Load("111", "GSTX", new ZDateTime(2013, 2, 28)));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new CMRCodeLists.Loader(Factory);
	}
}
