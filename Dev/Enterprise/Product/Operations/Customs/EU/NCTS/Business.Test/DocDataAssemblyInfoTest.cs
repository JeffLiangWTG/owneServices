using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class DocDataAssemblyInfoTest : TestCaseWithFactory
	{
		public void TestNctsAssemblyForEdocHelperAttribute()
		{
			var assembly = typeof(NctsHeader).Assembly;

			var attributes = assembly.GetCustomAttributes(typeof(AssemblyDataProviderAttribute), false).Cast<AssemblyDataProviderAttribute>().ToArray();

			CombineAssertions(() =>
			{
				AssertEquals(2, attributes.Length);
				AssertCollectionContains("NCT", attributes, a => a.DocManagerCode == "NCT");
				AssertCollectionContains("NCM", attributes, a => a.DocManagerCode == "NCM");
			});
		}
	}
}
