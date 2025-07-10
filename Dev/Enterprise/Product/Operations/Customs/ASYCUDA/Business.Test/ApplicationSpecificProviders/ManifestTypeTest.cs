using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class ManifestTypeTest : TestCaseWithFactory
	{
		public void TestEqualityComparer()
		{
			var comparer = new ManifestType.EqualityComparer();
			var manifestTypeA1 = new ManifestType("A", "A", new[] { "A" }, new[] { "A" }, MessageLevel.Manifest);
			var manifestTypeA2 = new ManifestType("A", "A", new[] { "A" }, new[] { "A" }, MessageLevel.Manifest);
			Assert(comparer.Equals(manifestTypeA1, manifestTypeA2));

			Assert("Code", !comparer.Equals(manifestTypeA1, new ManifestType("B", "A", new[] { "A" }, new[] { "A" }, MessageLevel.Manifest)));
			Assert("Description", !comparer.Equals(manifestTypeA1, new ManifestType("A", "B", new[] { "A" }, new[] { "A" }, MessageLevel.Manifest)));
			Assert("ApplicableTransportModes", !comparer.Equals(manifestTypeA1, new ManifestType("A", "A", new[] { "B" }, new[] { "A" }, MessageLevel.Manifest)));
			Assert("ApplicableTransportModes", !comparer.Equals(manifestTypeA1, new ManifestType("A", "A", new[] { "A", "B" }, new[] { "A" }, MessageLevel.Manifest)));
			Assert("ApplicableManifestStyles", !comparer.Equals(manifestTypeA1, new ManifestType("A", "A", new[] { "A" }, new[] { "B" }, MessageLevel.Manifest)));
			Assert("ApplicableManifestStyles", !comparer.Equals(manifestTypeA1, new ManifestType("A", "A", new[] { "A" }, new[] { "A", "B" }, MessageLevel.Manifest)));
			Assert("MessageLevel", !comparer.Equals(manifestTypeA1, new ManifestType("A", "A", new[] { "A" }, new[] { "A" }, MessageLevel.Bill)));
		}
		public void TestEqualityComparerWithNaturesSet()
		{
			var comparer = new ManifestType.EqualityComparer();
			var manifestTypeA1 = new ManifestType("A", "A", new[] { "A" }, new[] { "A" }, MessageLevel.Manifest);
			var manifestTypeA2 = new ManifestType("A", "A", new[] { "A" }, new[] { "A" }, MessageLevel.Manifest);
			Assert(comparer.Equals(manifestTypeA1, manifestTypeA2));
			manifestTypeA1 = new ManifestType("A", "A", "A", new[] { "A" }, MessageLevel.Manifest, ShipmentTypeList.Import23Only());
			Assert(!comparer.Equals(manifestTypeA1, manifestTypeA2));
			manifestTypeA2 = new ManifestType("A", "A", "A", new[] { "A" }, MessageLevel.Manifest, ShipmentTypeList.Import23Only());
			Assert(comparer.Equals(manifestTypeA1, manifestTypeA2));
			manifestTypeA2 = new ManifestType("A", "A", "A", new[] { "A" }, MessageLevel.Manifest, ShipmentTypeList.Export22AndImport23());
			Assert(!comparer.Equals(manifestTypeA1, manifestTypeA2));
		}

		public void TestManifestNature()
		{
			var manifestTypeA1 = new ManifestType("A", "A", "A", new[] { "A" }, MessageLevel.Manifest);
			var manifestTypeA2 = new ManifestType("B", "B", "B", new[] { "B" }, MessageLevel.Manifest, ShipmentTypeList.Import23Only());
			var manifestTypeA3 = new ManifestType("C", "C", "C", new[] { "BC" }, MessageLevel.Manifest, ShipmentTypeList.Export22AndImport23());
			CombineAssertions(() =>
			{
				AssertEquals("No Manifest Natures", manifestTypeA1.ManifestNatures, null);
				AssertContainsExactElementsInAnyOrder("Import Manifest Nature", ShipmentTypeList.Import23Only(), manifestTypeA2.ManifestNatures);
				AssertContainsExactElementsInAnyOrder("Import and Export Manifest Nature", ShipmentTypeList.Export22AndImport23(), manifestTypeA3.ManifestNatures);
			});
		}
	}
}
