using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class ILManifestTypesTest : TestCaseWithFactory
	{
		public void TestImportManifest()
		{
			CombineAssertions("Import Manifest", () =>
			{
				AssertManifest(cgmManifest: new ILManifestTypes().ImportManifest, "Import Manifest", new[] { "SEA", "ROA" }, "IMP", true);
			});
		}

		public void TestExportManifest()
		{
			CombineAssertions("Export Manifest", () =>
			{
				AssertManifest(cgmManifest: new ILManifestTypes().ExportManifest, "Export Manifest", new[] { "ROA" }, "EXP", true);
			});
		}

		public void TestAllManifest()
		{
			CombineAssertions("All Manifest", () =>
			{
				using (ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "NONE"))
				{
					AssertManifest(cgmManifest: new ILManifestTypes().AllManifest, "Import/Export Manifest", new[] { "SEA", "ROA" }, "EXP, IMP", false);
				}

				using (ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "EXPORT"))
				{
					AssertManifest(cgmManifest: new ILManifestTypes().AllManifest, "Import/Export Manifest", new[] { "SEA", "ROA" }, "EXP, IMP", false);
				}

				using (ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "IMPORT"))
				{
					AssertManifest(cgmManifest: new ILManifestTypes().AllManifest, "Import/Export Manifest", new[] { "SEA", "ROA" }, "EXP, IMP", false);
				}

				using (ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL"))
				{
					AssertManifest(cgmManifest: new ILManifestTypes().AllManifest, "Import/Export Manifest", new[] { "SEA", "ROA" }, "EXP, IMP", true);
				}
			});
		}

		void AssertManifest(ASYCUDA.Business.IManifestType cgmManifest, string expectedDescription, string[] expectedTransportModes, string expectedManifestNatures, bool expectedEnabled)
		{
			AssertEquals("Code", "785", cgmManifest.Code);
			AssertEquals("Description", expectedDescription, cgmManifest.Description);
			AssertContainsExactElementsInAnyOrder("ApplicableTransportModes", expectedTransportModes, cgmManifest.ApplicableTransportModes);
			AssertContainsExactElementsInAnyOrder("ApplicableManifestStyles", new[] { "NVC" }, cgmManifest.ApplicableManifestStyles);
			AssertEquals("MessageLevel", "Manifest", cgmManifest.MessageLevel.ToString());
			AssertEquals("ManifestNatures", expectedManifestNatures, cgmManifest.ManifestNatures.CodesAsString);
			AssertEquals("Enabled", expectedEnabled, cgmManifest.Enabled);
		}
	}
}
