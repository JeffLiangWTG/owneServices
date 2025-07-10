using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Count))]
	sealed class CountTest : ValueProviderTest
	{
		public void TestReplacementNonExistingTable()
		{
			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = new DocumentHeaderArea(1, 2, Report, "");
			AssertEquals(0, ValueProviderToTest.GetReplacement("<Count(FrickaseedChicken)>", Report));
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
			AssertEquals("report.ErrorManager.IsWarningOnly is true", true, Report.ErrorManager.HasWarningsOnly);
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in Count Macro: Evaluating <Count(FrickaseedChicken)> :- Table FrickaseedChicken Does not belong to data provider.]",
									Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< Count (    Tst ) >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Count(12,1)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Count()>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<Cout(Name)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Count(Tbl.Name)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Count( Tbl.Name, (\"FieldOne\"  == \"FieldTwo\" && \"FieldOne\" == \"FieldTwo\"))>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Count( Tble, (\"FieldOne\"  == \"FieldTwo\" && \"FieldOne\" == \"FieldTwo\"))>", Passes.FirstPass));
		}

		public void TestCountFunctionWorkForCollectionOfSubBusinessObject()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var relatedDummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Guid = relatedDummy.PK;
			var r1 = relatedDummy.Collection.AddNew();
			r1.Z0_NVarChar = "C1";
			var r2 = relatedDummy.Collection.AddNew();
			r2.Z0_NVarChar = "C1";
			var r3 = relatedDummy.Collection.AddNew();
			r3.Z0_NVarChar = "C2";
			var r4 = relatedDummy.Collection.AddNew();
			r4.Z0_NVarChar = "C2";
			var r5 = relatedDummy.Collection.AddNew();
			r5.Z0_NVarChar = "C3";

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3]
{A}-[PageStyle=Portrait]
{A}-[#SectionBody]
{B}-[<Count(RelatedDummy.Collection, 1 == 1)>]
{B}-[<Count(RelatedDummy.Collection, ""<Z0_NVarChar>"" == ""C1"")>]
{B}-[<Count(RelatedDummy.Collection, ""<Z0_NVarChar>"" == ""C2"")>]
{B}-[<Count(RelatedDummy.Collection, ""<Z0_NVarChar>"" == ""C3"")>]
{B}-[<Count(DummyCollection, 1 == 1)>]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate, BODocDataProvider.Get(dummy), "Test", null, null, DocumentDirection.ANY, false))
			using (var stream = new MemoryStream())
			{
				report.PrepareForRender();
				report.Save(stream);
				using (var excel = new ExcelInterface())
				{
					excel.LoadExcelFile(stream);
					AssertEquals(@"{B}-[5]
{B}-[2]
{B}-[2]
{B}-[1]
{B}-[0]", excel.WorkSheets[0].ToString());
				}
			}
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
			AssertEquals(1, ValueProviderToTest.GetReplacement("< Count ( Header )  >", Report));
			AssertEquals(160, ValueProviderToTest.GetReplacement("< Count ( Lines )  >", Report));
			AssertEquals(20, ValueProviderToTest.GetReplacement("< Count ( Lines, <GST>==5)  >", Report));
			AssertEquals(20, ValueProviderToTest.GetReplacement("< Count ( Lines, (<GST>==5))  >", Report));
			AssertEquals(20, ValueProviderToTest.GetReplacement("< Count ( Lines, <GST>==5 && (<GST>==5)) >", Report));
			AssertEquals(20, ValueProviderToTest.GetReplacement("< Count ( Lines, <GST>==5 && <GST>==5) >", Report));
			AssertEquals(20, Report.MacroTranslator.GetValue("< Count ( Lines, <GST>==5) >", Passes.FirstPass));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new Count();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
		}

		protected override Passes PassToReplaceExample => Passes.SecondPass;
	}
}
