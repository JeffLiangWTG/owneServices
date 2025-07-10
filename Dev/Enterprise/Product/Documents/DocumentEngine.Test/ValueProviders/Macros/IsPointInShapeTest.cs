using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(IsPointInShape))]
	sealed class IsPointInShapeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			CombineAssertions(() =>
			{
				AssertEquals(false, ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
				AssertEquals(false, ValueProviderToTest.IsResponsibleForReplacing("<IsPointInShape>", Passes.FirstPass));
				AssertEquals(false, ValueProviderToTest.IsResponsibleForReplacing("<IsPointInShape()>", Passes.FirstPass));
				AssertEquals(false, ValueProviderToTest.IsResponsibleForReplacing("<IsPointInShape(aaa)>", Passes.FirstPass));

				AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<IsPointInShape(aaa,bbb)>", Passes.FirstPass));
				AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("< IsPointInShape(aaa,bbb)>", Passes.FirstPass));
				AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<IsPointInShape (aaa,bbb)>", Passes.FirstPass));
				AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<IsPointInShape( aaa,bbb)>", Passes.FirstPass));
				AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<IsPointInShape(aaa ,bbb)>", Passes.FirstPass));
				AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<IsPointInShape(aaa, bbb)>", Passes.FirstPass));
				AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<IsPointInShape(aaa,bbb )>", Passes.FirstPass));
				AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<IsPointInShape(aaa,bbb) >", Passes.FirstPass));
			});
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestReplacement()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Edinburgh", ZGeography.CreatePoint(-3.1903984, 55.9532499)));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("London", ZGeography.CreatePoint(-0.1299567, 51.5073803)));

			CombineAssertions(() =>
			{
				AssertReplacement(ZBool.True.ToYN(), "<IsPointInShape(<Edinburgh>, <GetGeography(\"Scotland\", \"UKN\")>)>", Report);
				AssertReplacement(ZBool.True.ToYN(), "<IsPointInShape(<London>, <GetGeography(\"England\", \"UKN\")>)>", Report);
				AssertReplacement(ZBool.False.ToYN(), "<IsPointInShape(<Edinburgh>, <GetGeography(\"England\", \"UKN\")>)>", Report);
				AssertReplacement(ZBool.False.ToYN(), "<IsPointInShape(<London>, <GetGeography(\"Scotland\", \"UKN\")>)>", Report);
			});
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestReplacement_InvalidPoint()
		{
			CombineAssertions(() =>
			{
				AssertReplacement(ZBool.False.ToYN(), "<IsPointInShape(<Polygon>, <Polygon>)>", GetErrorTestReport(), true);
				AssertReplacement(ZBool.False.ToYN(), "<IsPointInShape(<String>, <Polygon>)>", GetErrorTestReport(), true);
			});
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestReplacement_InvalidShape()
		{
			CombineAssertions(() =>
			{
				AssertReplacement(ZBool.False.ToYN(), "<IsPointInShape(<Point>, <Point>)>", GetErrorTestReport(), true);
				AssertReplacement(ZBool.False.ToYN(), "<IsPointInShape(<Point>, <String>)>", GetErrorTestReport(), true);
			});
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestDocumentation()
		{
			base.TestDocumentation();
		}

		protected override ValueProvider GetNewValueProvider() => new IsPointInShape();

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("GetConsigneeDocAddress.E2_GeoLocation", ZGeography.CreatePoint(-3.1903984, 55.9532499)));
		}

		void AssertReplacement(string expectedResult, string macro, Report report, bool hasErrors = false)
		{
			AssertEquals(expectedResult, ValueProviderToTest.GetReplacement(macro, report).ToString());
			AssertEquals(hasErrors, report.ErrorManager.HasErrors);
		}

		Report GetErrorTestReport()
		{
			var report = new Report(Pack, ExcelTemplate);
			var polygon = ZGeography.CreatePolygon("0 0,1 0,1 1,0 1,0 0");
			report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Polygon", polygon));
			var point = ZGeography.CreatePoint(0.5, 0.5);
			report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Point", point));
			report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("String", "NOT A GEOGRAPHY VALUE"));

			AssertEquals(true, polygon.STContains(point));
			AssertReplacement(ZBool.True.ToYN(), "<IsPointInShape(<Point>, <Polygon>)>", report);

			return report;
		}
	}
}
