using System.IO;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.Dat.SpecialFileHandling.Testing
{
	sealed class PrebuiltAssemblyTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAssmblyInBinIsSameAsPrebuilt()
		{
			const string specailFileHandlerDll = "Enterprise.Dat.SpecialFileHandling.dll";
			AssertFileSameAsBytes(Path.Combine(AssemblyLoader.GetBinPath(), specailFileHandlerDll), File.ReadAllBytes(Path.Combine(BaseSourcePath, "Common", "Testing", "Enterprise.Dat.Implementation", "Enterprise.Dat.SpecialFileHandling", "prebuilt", specailFileHandlerDll)));
		}
	}
}
