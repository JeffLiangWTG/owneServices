using System;
using System.Data;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(RegistryItemTVP))]
	sealed class RegistryItemTVPTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("<RegistryItemTVP(aaa)>", Passes.FirstPass));
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("<registryitemTVP(aaa)>", Passes.FirstPass));
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("< Registry Item TVP(aaa) >", Passes.FirstPass));

			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<reg istryitemtvp(aaa)>", Passes.FirstPass));
			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<RegistryItemTVP>", Passes.FirstPass));
			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			var collection = new CommissionPeriodCollection();

			collection.Add(new CommissionPeriod()
			{
				Code = "NEW",
				Description = (NoResString)"New Client",
				Start = 0,
				End = 12,
				IsEnabled = true
			});

			collection.Add(new CommissionPeriod()
			{
				Code = "EXS",
				Description = (NoResString)"Existing Client",
				Start = 12,
				End = 24,
				IsEnabled = true
			});

			OrganisationsDataRegistry.Instance.CommissionPeriodList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var registryItemMacro = new RegistryItem();
			var returnedCollection = (CommissionPeriodCollection)registryItemMacro.GetReplacement("<RegistryItem(OrganisationsDataRegistry.Instance.CommissionPeriodList)>", Report);
			AssertEquals("assert return registry value", "NEW", collection[0].Code);
			AssertEquals("assert return registry value", "EXS", collection[1].Code);

			var macroValue = ValueProviderToTest.GetReplacement("<RegistryItemTVP(OrganisationsDataRegistry.Instance.CommissionPeriodList)>", Report);
			if (macroValue is ReplacementWithParameterType replacement)
			{
				AssertEquals("Parameter Type is table valued type", "dbo.TVP_CommissionPeriod", replacement.ParameterTypeName);
				var table = replacement.MacroValue as DataTable;
				AssertNotNull("macro value is a DataTable", table);
				AssertEquals("Rows in DataTable", 2, table.Rows.Count);
				AssertEquals("Code in First Row", "NEW", table.Rows[0]["Code"].ToString());
				AssertEquals("Code in Second Row", "EXS", table.Rows[1]["Code"].ToString());
			}
			else
			{
				Fail("Is ReplacementWithSqlDbType");
			}
		}

		#region Asserts Override

		protected override void AssertExamplesAreReplacedAsExpected(string example, object expectedResult)
		{
			using (((ReportRenderer)Report.Renderer).TemporarilySwitchCurrentPassForTest(PassToReplaceExample))
			{
				var actualResult = Report.MacroTranslator.GetValue(example, PassToReplaceExample);

				if (actualResult is ReplacementWithParameterType replacement)
				{
					AssertStartsWith($"Example {example}, parameter type", "dbo.TVP_", replacement.ParameterTypeName);
					AssertEquals($"Example {example}, macro value is DataTable", true, replacement.MacroValue is DataTable);
				}
				else
				{
					Fail($"Example {example}, Is ReplacementWithParameterType");
				}
			}
		}

		#endregion

		#region Implementation

		protected override ValueProvider GetNewValueProvider()
		{
			return new RegistryItemTVP();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var collection = new CommissionPeriodCollection();

			collection.Add(new CommissionPeriod()
			{
				Code = "NEW",
				Description = (NoResString)"New Client",
				Start = 0,
				End = 12,
				IsEnabled = true
			});

			collection.Add(new CommissionPeriod()
			{
				Code = "EXS",
				Description = (NoResString)"Existing Client",
				Start = 12,
				End = 24,
				IsEnabled = true
			});

			OrganisationsDataRegistry.Instance.CommissionPeriodList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		#endregion

	}
}
