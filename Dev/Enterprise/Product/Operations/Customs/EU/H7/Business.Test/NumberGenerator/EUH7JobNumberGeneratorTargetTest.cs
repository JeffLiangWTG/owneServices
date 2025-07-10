using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	sealed class EUH7JobNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestOverrides()
		{
			Set(EUH7CustomsDataRegistry.Instance.EUH7JobNumberCustomization, "TEST");

			var target = new EUH7JobNumberGeneratorTarget();
			target.Context = new NumberGeneratorContext();

			AssertCustomisation("Should find the LowValueH7Customization", "TEST", target.NumberCustomisation);
			AssertLocation(EUH7CustomsDataRegistry.Instance.EUH7JobNumberCustomization, target.NumberCustomisationLocation);
			AssertEquals(AsycudaManifestHeaderSchema.AMA_JobReference.MaxLength, target.MaxLength);
			AssertEquals("Low Value (H7) Job Number Customization", target.Name);
		}
	}
}
