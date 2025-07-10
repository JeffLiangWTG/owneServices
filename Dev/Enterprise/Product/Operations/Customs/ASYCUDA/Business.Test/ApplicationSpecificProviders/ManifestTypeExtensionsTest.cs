using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class ManifestTypeExtensionsTest : TestCaseWithFactory
	{
		public void TestIs_MessageLevel()
		{
			AssertMessageLevel(MessageLevel.Manifest);
			AssertMessageLevel(MessageLevel.Bill);
			AssertMessageLevel(MessageLevel.Pack);
		}

		void AssertMessageLevel(MessageLevel expectedMessageLevel)
		{
			var manifestType = new ManifestType("A", "A", new[] { "A" }, new[] { "A" }, expectedMessageLevel);
			AssertEquals("ManifestLevel", expectedMessageLevel == MessageLevel.Manifest, manifestType.IsManifestMessageLevel());
			AssertEquals("BillLevel", expectedMessageLevel == MessageLevel.Bill, manifestType.IsBillMessageLevel());
			AssertEquals("PackLevel", expectedMessageLevel == MessageLevel.Pack, manifestType.IsPackMessageLevel());
		}
	}
}
