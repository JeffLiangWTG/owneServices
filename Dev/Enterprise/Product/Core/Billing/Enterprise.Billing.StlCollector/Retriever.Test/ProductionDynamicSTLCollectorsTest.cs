using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Billing.Collectors;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Billing.Business;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever
{
	sealed class ProductionDynamicSTLCollectorsTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			Assert("These tests use the real reference data to verify consistency.  Please disable System->Testing->RedirectReferenceDataForTests and restart CargoWise before running these tests.  Please also run the RDU service task to synchronise the reference data.", !DataRegistry.Instance.RedirectReferenceDataForTests);
			base.SetUp();
		}

		[DeveloperOnlyTest]
		public void TestAllProductionCollectorsAreUnitTested()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTestingWithALPVersionBasedOffDateNow()))
			{
				var unitTestedScriptsByCode = new Dictionary<string, IStlScript>();
				foreach (var unitTestedScript in RefStlScriptFactoryTest.ProductionScriptsLoadedViaReflection)
				{
					unitTestedScriptsByCode[unitTestedScript.Bizo.FeatureCode] = unitTestedScript;
				}

				var productionScriptsNotUnitTested = Factory.Load<RefStlScript>(new ZQuery()).Where(s => (s.STL_ActiveOn == "ALL" || s.STL_ActiveOn == "PRD") && s.MaxCW1Version.IsNullOrEmpty() && !unitTestedScriptsByCode.ContainsKey(s.STL_FeatureCode));
				Assert($"Scripts with feature codes: {string.Join(",", productionScriptsNotUnitTested.Select(s => s.STL_FeatureCode))} were not in the list of unit tested collectors", !productionScriptsNotUnitTested.Any());
			}
		}

		[DeveloperOnlyTest]
		public void TestAllUnitTestedCollectorsAreInProduction()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTestingWithALPVersionBasedOffDateNow()))
			{
				var productionScripts = Factory.Load<RefStlScript>(new ZQuery()).Where(s => s.STL_ActiveOn == "ALL" || s.STL_ActiveOn == "PRD");
				var productionScriptsByCode = new Dictionary<string, RefStlScript>();
				foreach (var productionScript in productionScripts)
				{
					productionScriptsByCode[productionScript.STL_FeatureCode] = productionScript;
				}

				var unitTestedScriptsNotInProduction = RefStlScriptFactoryTest.ProductionScriptsLoadedViaReflection.Where(s => !productionScriptsByCode.ContainsKey(s.Bizo.FeatureCode));
				Assert($"Scripts with feature codes: {string.Join(",", unitTestedScriptsNotInProduction.Select(s => s.Bizo.FeatureCode))} have been unit tested but have not been deployed to production", !unitTestedScriptsNotInProduction.Any());
			}
		}

		[DeveloperOnlyTest]
		public void TestAllProductionCollectorsHaveSameDefinitionInTests()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTestingWithALPVersionBasedOffDateNow()))
			{
				var unitTestedScripts = ObjectFactory.Get<ListObject>("StlDynamicCollectorsList").Cast<IRefStlScript>().Where(s => s.MaxCW1Version.IsNullOrEmpty());

				CombineAssertions(() =>
				{
					foreach (var productionScript in Factory.Load<RefStlScript>(new ZQuery()).Where(s => (s.STL_ActiveOn == "ALL" || s.STL_ActiveOn == "PRD") && s.MaxCW1Version.IsNullOrEmpty()))
					{
						var unitTestedScript = unitTestedScripts.SingleOrDefault(s => s.FeatureCode == productionScript.STL_FeatureCode);
						AssertNotNull($"Could not find matching unit test script for Code={productionScript.STL_FeatureCode}, MinVer={productionScript.STL_MinCW1Version}, MaxVer={productionScript.STL_MaxCW1Version}", unitTestedScript);
						if (unitTestedScript != null)
						{
							AssertCollectorPropertyEquals("AdditionalRefs", unitTestedScript.FeatureCode, unitTestedScript.AdditionalRefs, productionScript.STL_AdditionalRefs);
							AssertCollectorPropertyEquals("Reference1", unitTestedScript.FeatureCode, unitTestedScript.BillingReference1, productionScript.STL_BillingReference1);
							AssertCollectorPropertyEquals("Reference2", unitTestedScript.FeatureCode, unitTestedScript.BillingReference2, productionScript.STL_BillingReference2);
							AssertCollectorPropertyEquals("Reference3", unitTestedScript.FeatureCode, unitTestedScript.BillingReference3, productionScript.STL_BillingReference3);
							AssertCollectorPropertyEquals("Reference4", unitTestedScript.FeatureCode, unitTestedScript.BillingReference4, productionScript.STL_BillingReference4);
							AssertCollectorPropertyEquals("BranchCode", unitTestedScript.FeatureCode, unitTestedScript.BranchCode, productionScript.STL_BranchCode);
							AssertCollectorPropertyEquals("CompanyCode", unitTestedScript.FeatureCode, unitTestedScript.CompanyCode, productionScript.STL_CompanyCode);
							AssertCollectorPropertyEquals("CreatingUserCode", unitTestedScript.FeatureCode, unitTestedScript.CreatingUserCode, productionScript.STL_CreatingUserCode);
							AssertCollectorPropertyEquals("DataGranularity", unitTestedScript.FeatureCode, unitTestedScript.DataGranularity, productionScript.STL_DataGranularity);
							AssertCollectorPropertyEquals("DateType", unitTestedScript.FeatureCode, unitTestedScript.DateType, productionScript.STL_DateType);
							AssertCollectorPropertyEquals("FeatureName", unitTestedScript.FeatureCode, unitTestedScript.FeatureName, productionScript.STL_FeatureName);
							AssertCollectorPropertyEquals("FromClause", unitTestedScript.FeatureCode, unitTestedScript.FromClause, productionScript.STL_FromClause);
							AssertCollectorPropertyEquals("FunctionName", unitTestedScript.FeatureCode, unitTestedScript.FunctionName, productionScript.STL_FunctionName);
							AssertCollectorPropertyEquals("GuidReference", unitTestedScript.FeatureCode, unitTestedScript.GuidReference, productionScript.STL_GuidReference);
							AssertCollectorPropertyEquals("ModuleName", unitTestedScript.FeatureCode, unitTestedScript.ModuleName, productionScript.STL_ModuleName);
							AssertCollectorPropertyEquals("PreparationScript", unitTestedScript.FeatureCode, unitTestedScript.PreparationScript, productionScript.STL_PreparationScript);
							AssertCollectorPropertyEquals("RoleName", unitTestedScript.FeatureCode, unitTestedScript.RoleName, productionScript.STL_RoleName);
							AssertCollectorPropertyEquals("TransactionCount", unitTestedScript.FeatureCode, unitTestedScript.TransactionCount, productionScript.STL_TransactionCount);
							AssertCollectorPropertyEquals("TransactionDateUtc", unitTestedScript.FeatureCode, unitTestedScript.TransactionDateUtc, productionScript.STL_TransactionDateUtc);
							AssertCollectorPropertyEquals("UsedInBilling", unitTestedScript.FeatureCode, unitTestedScript.UsedInBilling, productionScript.STL_UsedInBilling);
							AssertCollectorPropertyEquals("WhereClause", unitTestedScript.FeatureCode, unitTestedScript.WhereClause, productionScript.STL_WhereClause);
							AssertCollectorPropertyEquals("WithOptionRecompile", unitTestedScript.FeatureCode, unitTestedScript.WithOptionRecompile, productionScript.STL_WithOptionRecompile);
							AssertCollectorPropertyEquals("MinCW1Version", unitTestedScript.FeatureCode, unitTestedScript.MinCW1Version, productionScript.STL_MinCW1Version);
							AssertCollectorPropertyEquals("ActiveOn", unitTestedScript.FeatureCode, unitTestedScript.ActiveOn, productionScript.ActiveOn);
						}
					}
				});
			}
		}

		void AssertCollectorPropertyEquals(string propertyName, ZString featureCode, ZString unitTestValue, ZString productionValue)
		{
			string normalized1 = Regex.Replace(Regex.Replace(unitTestValue, @"\s", ""), "dbo.", "");
			string normalized2 = Regex.Replace(Regex.Replace(productionValue, @"\s", ""), "dbo.", "");
			Assert($"Mismatching {propertyName} for Feature code {featureCode} in UnitTest({unitTestValue}) vs Production({productionValue})", string.Equals(normalized1, normalized2, StringComparison.OrdinalIgnoreCase));
		}

		void AssertCollectorPropertyEquals(string propertyName, ZString featureCode, ZBool unitTestValue, ZBool productionValue)
		{
			AssertEquals($"Mismatching {propertyName} for Feature code {featureCode} in UnitTest vs Production", unitTestValue, productionValue);
		}
	}
}
