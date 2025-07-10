using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class EMCSLocalReferenceNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestParameters()
		{
			Set(CustomsDataRegistry.Instance.EMCSLocalReferenceNumberCustomisation, "EMC");

			var generatorTarget = new EMCSLocalReferenceNumberGeneratorTarget();
			generatorTarget.Context = new NumberGeneratorContext();

			AssertCustomisation("Should find the EMCSLocalReferenceNumberCustomisation", "EMC", generatorTarget.NumberCustomisation);
			AssertLocation(CustomsDataRegistry.Instance.EMCSLocalReferenceNumberCustomisation, generatorTarget.NumberCustomisationLocation);
			AssertEquals(JobDeclarationSchema.JE_DeclarationReference.MaxLength, generatorTarget.MaxLength);
			AssertEquals("EMCS Job Number", generatorTarget.Name);
		}
	}
}
