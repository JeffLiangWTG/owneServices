using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(GetGeography))]
	sealed class GetGeographyTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<GetGeography>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<GetGeography()>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<GetGeography(,)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<GetGeography(\"\",)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<GetGeography(,\"\")>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<GetGeography(\"\",\"\")>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<GetGeography(\" \",\" \")>", Passes.FirstPass));

			Assert(ValueProviderToTest.IsResponsibleForReplacing("<GetGeography(\"a\", \"b\")>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<GetGeography(\"abc\",\"def\")>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< GetGeography(\"abc\",\"def\")>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<GetGeography (\"abc\",\"def\")>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<GetGeography( \"abc\", \"def\")>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<GetGeography(\" abc\",\" def\")>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<GetGeography(\"abc \",\"def \")>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<GetGeography(\"abc\" ,\"def\" )>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<GetGeography(\"abc\",\"def\") >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<GetGeography(\"northern ireland\", \"UKN\")>", Passes.FirstPass));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReplacement()
		{
			Assert(!((ZGeography)ValueProviderToTest.GetReplacement("<GetGeography(\"england\",\"UKN\")>", Report)).IsEmpty);
			Assert(!Report.ErrorManager.HasErrors);
			Assert(!((ZGeography)ValueProviderToTest.GetReplacement("<GetGeography(\"scotland\",\"UKN\")>", Report)).IsEmpty);
			Assert(!Report.ErrorManager.HasErrors);
			Assert(!((ZGeography)ValueProviderToTest.GetReplacement("<GetGeography(\"wales\",\"UKN\")>", Report)).IsEmpty);
			Assert(!Report.ErrorManager.HasErrors);
			Assert(!((ZGeography)ValueProviderToTest.GetReplacement("<GetGeography(\"northern ireland\",\"UKN\")>", Report)).IsEmpty);
			Assert(!Report.ErrorManager.HasErrors);

			var testReport = new Report(Pack, ExcelTemplate);
			Assert(((ZGeography)ValueProviderToTest.GetReplacement("<GetGeography(\"moon\",\"UKN\")>", testReport)).IsEmpty);
			Assert(testReport.ErrorManager.HasErrors);
			Assert(testReport.ErrorManager.HasWarningsOnly);

			testReport = new Report(Pack, ExcelTemplate);
			Assert(((ZGeography)ValueProviderToTest.GetReplacement("<GetGeography(\"northern ireland\",\"DUM\")>", testReport)).IsEmpty);
			Assert(testReport.ErrorManager.HasErrors);
			Assert(testReport.ErrorManager.HasWarningsOnly);

			testReport = new Report(Pack, ExcelTemplate);
			Factory.LoadTop1<IGenShapeGeography>(new ZQuery(GenShapeGeographySchema.SHG_Name, "england")).SHG_IsActive = false;
			Factory.Save();
			Assert(((ZGeography)ValueProviderToTest.GetReplacement("<GetGeography(\"england\",\"UKN\")>", testReport)).IsEmpty);
			Assert(testReport.ErrorManager.HasErrors);
			Assert(testReport.ErrorManager.HasWarningsOnly);
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestDocumentation()
		{
			base.TestDocumentation();
		}

		protected override ValueProvider GetNewValueProvider() => new GetGeography();

		protected override void PrepareDataForExamplesEvaluate()
		{
			var exampleShape = Factory.LoadTop1<IGenShapeGeography>(new ZQuery(GenShapeGeographySchema.SHG_Name, "SCOTLAND").AddToFilter(GenShapeGeographySchema.SHG_Type, "UKN"));
			if (exampleShape == null)
			{
				exampleShape = Factory.New<IGenShapeGeography>();
				exampleShape.SHG_Name = "SCOTLAND";
				exampleShape.SHG_Type = "UKN";
			}

			exampleShape.SHG_IsActive = true;
			exampleShape.SHG_Shape = ZGeography.CreatePolygon("0 0,1 0,1 1,0 1,0 0");
			Factory.Save();
		}
	}
}
