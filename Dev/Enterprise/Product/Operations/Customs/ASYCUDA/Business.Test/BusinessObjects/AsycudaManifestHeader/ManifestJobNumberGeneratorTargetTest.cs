using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.Registry.Business.Customs.Manifest.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class ManifestJobNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestOverrides()
		{
			var customisation = new ManifestJobNumberCustomisation();
			ManifestJobNumberCustomisationTest.Set(customisation, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, false, "MAN");
			ManifestCustomsDataRegistry.Instance.ManifestJobNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);

			var target = new ManifestJobNumberGeneratorTarget { Context = new NumberGeneratorContext() };
			AssertCustomisation("Should find the customisation", "MAN", target.NumberCustomisation);
			AssertLocation(ManifestCustomsDataRegistry.Instance.ManifestJobNumberCustomization, target.NumberCustomisationLocation);
			AssertEquals(AsycudaManifestHeaderSchema.AMA_JobReference.MaxLength, target.MaxLength);
			AssertEquals("Manifest job number", target.Name);
		}
	}
}
