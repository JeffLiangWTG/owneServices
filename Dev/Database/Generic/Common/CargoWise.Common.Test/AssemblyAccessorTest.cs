using System.IO;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	class AssemblyAccessorTest : TestCase
	{
		public void TestGetManifestResourceStreamFromAssemblyFile()
		{
			AssertEquals("Hello World", new StreamReader(AssemblyAccessor.GetManifestResourceStreamFromAssemblyFile(GetType().Assembly.Location, "CargoWise.Common.Testing.Testing.ResourceTest.txt")).ReadToEnd());
		}
	}
}
