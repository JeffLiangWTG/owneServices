using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Loaders;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions
{
	public class DefinitionAssemlyLoaderTest : TestCase
	{
		public void TestLoad_DefinitionExist()
		{
			loader.Load("Dummy");
			Assert("Definition should be loaded to memory if definition existed", true);
		}

		public void TestLoad_DefinitionNotExist()
		{
			AssertExceptionThrown(
				typeof(NativeXMLUserVisibleException),
				() => loader.Load("NotExistName"));

			AssertExceptionThrown(
				typeof(NativeXMLUserVisibleException),
				() => loader.Load(""));
		}

		public void TestSetAssemblies()
		{
			Assert("Assemblies are the same so it should return false", !loader.SetAssemblies(new[] { typeof(TestUtil).Assembly.FullName }));
			Assert("Assemblies should be set so it should return true", loader.SetAssemblies(new[] { "Test" }));
		}

		#region Implementation
		DefinitionAssemblyLoader loader;

		protected override void SetUp()
		{
			base.SetUp();
			loader = new DefinitionAssemblyLoader(new[] { typeof(TestUtil).Assembly.FullName });
		}

		#endregion
	}
}
