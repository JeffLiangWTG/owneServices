using System;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CustomisedColumn))]
	sealed class CustomisedColumnTest : ValueProviderTest
	{
		protected override Type ValueProviderType
		{
			get { return typeof(CustomisedColumn); }
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CustomisedColumn();
		}

		public void TestGetReplacement()
		{
			ColumnHeading[] headings = new ColumnHeading[]
			{
				new ColumnHeading("Cow", "CowDesc", "Moo", 0, 0, 0, false),
				new ColumnHeading("Pig", "PigDesc", "Oink", 0, 0, 0, false)
			};
			Report.ColumnHeadingManager.CurrentConfiguration.Worksheets.Add(new Worksheet(new ColumnHeadingCollection(headings), "Sheet1"));
			AssertEquals("GetReplacement()", "", ValueProviderToTest.GetReplacement("<CustomisedColumn(Butter)>", Report));
			AssertEquals("GetReplacement()", "Moo", ValueProviderToTest.GetReplacement("<CustomisedColumn(Cow)>", Report));
			AssertEquals("GetReplacement()", "Oink", ValueProviderToTest.GetReplacement("<CustomisedColumn(\"Pig\")>", Report));
		}

		public void TestIsResponsibleForReplacing()
		{
			AssertEquals("IsResponsibleForReplacing()", false, ValueProviderToTest.IsResponsibleForReplacing("", Passes.FirstPass));
			AssertEquals("IsResponsibleForReplacing()", false, ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			AssertEquals("IsResponsibleForReplacing()", false, ValueProviderToTest.IsResponsibleForReplacing("<CustomisedColumn()>", Passes.FirstPass));
			AssertEquals("IsResponsibleForReplacing()", false, ValueProviderToTest.IsResponsibleForReplacing("<CustomisedColumn(\"\")>", Passes.FirstPass));
			AssertEquals("IsResponsibleForReplacing()", false, ValueProviderToTest.IsResponsibleForReplacing("<CustomisedColumn(\"x\"y\")>", Passes.FirstPass));
			AssertEquals("IsResponsibleForReplacing()", true, ValueProviderToTest.IsResponsibleForReplacing("<CustomisedColumn(x)>", Passes.FirstPass));
			AssertEquals("IsResponsibleForReplacing()", true, ValueProviderToTest.IsResponsibleForReplacing("<CustomisedColumn(\"y\")>", Passes.FirstPass));
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			ColumnHeading[] headings = new ColumnHeading[]
			{
				new ColumnHeading("ShoeSize", "ShoeSizeDesc", "5", 0, 0, 0, false)
			};
			Report.ColumnHeadingManager.CurrentConfiguration.Worksheets.Add(new Worksheet(new ColumnHeadingCollection(headings), "Sheet1"));
		}
	}
}
