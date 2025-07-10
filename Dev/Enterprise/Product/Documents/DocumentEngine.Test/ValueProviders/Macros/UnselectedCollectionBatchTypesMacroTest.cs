using System;
using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(UnselectedCollectionBatchTypesMacro))]
	sealed class UnselectedCollectionBatchTypesMacroTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match <UnselectedCollectionBatchTypesMacro>", ValueProviderToTest.IsResponsibleForReplacing("<UnselectedCollectionBatchTypesMacro>", Passes.FirstPass));
			Assert("should match <UnselectedCollectionBatchTypesMacro >", ValueProviderToTest.IsResponsibleForReplacing("<UnselectedCollectionBatchTypesMacro >", Passes.FirstPass));
			Assert("should match < UnselectedCollectionBatchTypesMacro>", ValueProviderToTest.IsResponsibleForReplacing("< UnselectedCollectionBatchTypesMacro>", Passes.FirstPass));
			Assert("should match < UnselectedCollectionBatchTypesMacro >", ValueProviderToTest.IsResponsibleForReplacing("< UnselectedCollectionBatchTypesMacro >", Passes.FirstPass));
			Assert("should match < Unselected Collection Batch Types Macro >", ValueProviderToTest.IsResponsibleForReplacing("< Unselected Collection Batch Types Macro >", Passes.FirstPass));
		}

		public void TestReplacementForRegistryEmpty()
		{
			var value = AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.Value;

			value.RemoveAll();
			AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

			AssertEquals("", ValueProviderToTest.GetReplacement("<Unselected CollectionBatch TypesMacro>", Report));
		}

		public void TestReplacementForTwoValuesBothTicked()
		{
			var value = AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.Value;

			value.RemoveAll();
			value.Add("STD", (NoResString)"STD Desc", true);
			value.Add("ST1", (NoResString)"STD Desc", true);
			AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

			AssertEquals("", ValueProviderToTest.GetReplacement("<UnselectedCollectionBatchTypesMacro>", Report));
		}

		public void TestReplacementForFourValuesAndOnlyTwoTicked()
		{
			var value = AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.Value;

			value.RemoveAll();
			value.Add("STD", (NoResString)"STD Desc", true);
			value.Add("ST1", (NoResString)"STD Desc", false);
			value.Add("ST2", (NoResString)"STD Desc", true);
			value.Add("ST3", (NoResString)"STD Desc", false);
			AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

			AssertEquals("ST1,ST3", ValueProviderToTest.GetReplacement("<UnselectedCollectionBatchTypesMacro>", Report));
		}

		public void TestReplacementForFourValuesNotTicked()
		{
			var value = AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.Value;

			value.RemoveAll();
			value.Add("STD", (NoResString)"STD Desc", false);
			value.Add("ST1", (NoResString)"STD Desc", false);
			value.Add("ST2", (NoResString)"STD Desc", false);
			value.Add("ST3", (NoResString)"STD Desc", false);
			AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

			AssertEquals("STD,ST1,ST2,ST3", ValueProviderToTest.GetReplacement("<UnselectedCollectionBatchTypesMacro>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new UnselectedCollectionBatchTypesMacro();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var value = AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.Value;

			value.RemoveAll();
			value.Add("STD", (NoResString)"STD Desc", true);
			value.Add("ST1", (NoResString)"STD Desc", false);
			value.Add("ST2", (NoResString)"STD Desc", true);
			value.Add("ST3", (NoResString)"STD Desc", false);

			AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}
	}
}
