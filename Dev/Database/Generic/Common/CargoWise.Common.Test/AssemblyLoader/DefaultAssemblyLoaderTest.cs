using System.Reflection;
using NUnit.Framework;

namespace CargoWise.Common
{
	class DefaultAssemblyLoaderTest : TestCase
	{
		public void TestLoadAssembly()
		{
			DefaultAssemblyLoader loader = new DefaultAssemblyLoader();
			AssertEquals(typeof(DefaultAssemblyLoaderTest).Assembly, loader.LoadAssembly(new AssemblyName("CargoWise.Common.Testing")));
		}
	}
}
