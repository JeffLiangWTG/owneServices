using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class AssemblyHelperTest : TestCaseWithFactory
	{
		public void TestGetDistinctAssemblyNames()
		{
			var testCollection = new[]
			{
				"executable.exe",
				"textfile.txt",
				"somedoc.doc",
				"website.htm",
				"project.csproj",
				"test1.dll",
				"test2.DlL",
				"test1.DLL",
			};

			AssertContainsExactElementsInAnyOrder(new[] { "test1", "test2" }, AssemblyHelper.GetDistinctAssemblyNames(testCollection));
		}
	}
}
