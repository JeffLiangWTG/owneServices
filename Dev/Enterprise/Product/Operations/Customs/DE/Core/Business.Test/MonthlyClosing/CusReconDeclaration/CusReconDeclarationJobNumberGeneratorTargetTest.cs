using System;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.Registry.Business.Customs.Manifest.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.Testing
{
	class CusReconDeclarationJobNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestOverrides()
		{
			var registryItem = DECustomsDataRegistry.Instance.MonthlyClosingJobNumberCustomization;
			var customization = new ManifestJobNumberCustomisation();
			ManifestJobNumberCustomisationTest.Set(customization, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, false, "MON");
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customization))
			{
				var target = new CusReconDeclarationJobNumberGeneratorTarget { Context = new NumberGeneratorContext() };
				CombineAssertions(() =>
				{
					AssertCustomisation("Should find the customization", "MON", target.NumberCustomisation);
					AssertLocation(registryItem, target.NumberCustomisationLocation);
					AssertEquals("MaxLength", CusReconDeclarationSchema.CRD_JobReferenceNumber.MaxLength, target.MaxLength);
					AssertEquals("Name", "monthly closing job number", target.Name);
				});
			}
		}
	}
}
