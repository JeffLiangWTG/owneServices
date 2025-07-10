using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class CusSupplyChainActorReferenceProviderTest : TestCaseWithFactory
	{
		public void TestGetByDataGroupingCode()
		{
			var provider = EU.Business.Declaration.CusSupplyChainActorReferenceProvider.GetByDataGroupingCode(Core.Constants.CountryCodes.Ireland);
			CombineAssertions(() =>
			{
				AssertType<CusSupplyChainActorReferenceProvider>("Type", provider);
				AssertEquals("DataGroupingCode", Core.Constants.CountryCodes.Ireland, provider.DataGroupingCode);
			});
		}

		public void TestOverwrittenReferenceColumnCaption()
		{
			AssertEquals("Identification (TCUI/EORI)", EU.Business.Declaration.CusSupplyChainActorReferenceProvider.GetByDataGroupingCode(Core.Constants.CountryCodes.Ireland).OverwrittenReferenceColumnCaption);
		}

		public void TestReferenceColumnCasingToUpper()
		{
			AssertEquals(true, EU.Business.Declaration.CusSupplyChainActorReferenceProvider.GetByDataGroupingCode(Core.Constants.CountryCodes.Ireland).ReferenceColumnCasingToUpper);
		}

		public void TestValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusSupplyChainActorReference = entryInstruction.CusSupplyChainActorReferences.AddNew();
			AssertType<CusSupplyChainActorReferenceValidation>(cusSupplyChainActorReference.Validation);
		}
	}
}
