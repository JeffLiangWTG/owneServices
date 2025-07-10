using System;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(LandedCostingPreference))]
	sealed class LandedCostingPreferenceTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertEquals("should not match ", false, ValueProviderToTest.IsResponsibleForReplacing("<LandedCostingPreference>", Passes.FirstPass));
			AssertEquals("should not match ", false, ValueProviderToTest.IsResponsibleForReplacing("<   Landed Costing Preference   >", Passes.FirstPass));
			AssertEquals("should not match ", false, ValueProviderToTest.IsResponsibleForReplacing("<   LandedCostingPreference somefield   >", Passes.FirstPass));
			AssertEquals("should match <LandedCostingPreference(AField)>", true, ValueProviderToTest.IsResponsibleForReplacing("<LandedCostingPreference(AField)>", Passes.FirstPass));
			AssertEquals("should not match ", false, ValueProviderToTest.IsResponsibleForReplacing("<   LandedCostingPreference(AField, other)   >", Passes.SecondPass));
			AssertEquals("should match ", true, ValueProviderToTest.IsResponsibleForReplacing("<LandedCostingPreference()>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			FreightDataRegistry.Instance.LandedCostingPreferences.Value.RemoveAndDeleteAll();
			var collection = new LandedCostingGroupCollection();
			var lCGroup = collection.AddNew();
			lCGroup.GroupID = 1;
			lCGroup.GroupName = "TestTest";
			lCGroup.CostDistributionCode = "VAV";

			FreightDataRegistry.Instance.LandedCostingPreferences.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			AssertEquals("The first Landed Costing Preference in Registry", lCGroup.GroupName, ValueProviderToTest.GetReplacement("<LandedCostingPreference(1)>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new LandedCostingPreference();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			FreightDataRegistry.Instance.LandedCostingPreferences.Value.RemoveAndDeleteAll();
			var collection = new LandedCostingGroupCollection();
			var lCGroup = collection.AddNew();
			lCGroup.GroupID = 1;
			lCGroup.GroupName = "Intl Freight Charges";
			lCGroup.CostDistributionCode = "AWV";
			FreightDataRegistry.Instance.LandedCostingPreferences.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
		}
	}
}
