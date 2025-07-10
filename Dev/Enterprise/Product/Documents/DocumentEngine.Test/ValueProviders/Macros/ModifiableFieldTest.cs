using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.Visualisation;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ModifiableField))]
	sealed class ModifiableFieldTest : ValueProviderTest<ModifiableField>
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing(@"<ModifiableField()>");
			AssertNotResponsibleForReplacing(@"<ModifiableField(FieldName)>");
			AssertNotResponsibleForReplacing(@"<ModifiableField("""")>");

			AssertIsResponsibleForReplacing(@"<ModifiableField(""FieldName"")>");
			AssertIsResponsibleForReplacing(@"<ModifiableField(""Field Name"")>");

			AssertNotResponsibleForReplacing(@"<ModifiableField(""FieldName"", )>");

			AssertIsResponsibleForReplacing(@"<ModifiableField(""FieldName"", """")>");
			AssertIsResponsibleForReplacing(@"<ModifiableField(""FieldName"", ""Default Value"")>");
		}

		public void TestReplacement()
		{
			AssertIsReplacedWith("", @"<ModifiableField(""FieldName"")>");
			AssertIsReplacedWith("", @"<ModifiableField(""Field Name"")>");
			AssertIsReplacedWith("", @"<ModifiableField(""FieldName"", """")>");
			AssertIsReplacedWith("Default Value", @"<ModifiableField(""FieldName"", ""Default Value"")>");
		}

		public void TestCellContentReplacerGetsTheRightValue()
		{
			CombineAssertions(delegate
			{
				AssertReplacementViaCellReplacer(@"<ModifiableField(""FieldName"")>", "", "Entered A Value");
				AssertReplacementViaCellReplacer(@"<ModifiableField(""Field Name"")>", "", "Entered Another Value");
				AssertReplacementViaCellReplacer(@"<ModifiableField(""FieldName"", """")>", "", "Entered Yet Another Value");
				AssertReplacementViaCellReplacer(@"<ModifiableField(""FieldName"", ""Default Value"")>", "Default Value", "Entered MY Value");
			});
		}

		void AssertReplacementViaCellReplacer(string macroToReplace, string defaultValue, string replacedValue)
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test", "");
			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();

				var replacer = new CellContentReplacer(report, macroToReplace);
				replacer.ReplaceMacros();
				AssertEquals(string.Format(@"Before setting to [{0}]: {1}", replacedValue, macroToReplace), defaultValue, replacer.ContentAsString);

				var overridingDataSet = report.OverridingDataSet;
				overridingDataSet.MainTable.Columns.Add(macroToReplace);
				overridingDataSet.MainRow[macroToReplace] = replacedValue;

				replacer = new CellContentReplacer(report, macroToReplace);
				replacer.ReplaceMacros();
				AssertEquals(string.Format(@"After setting to [{0}]: {1}", replacedValue, macroToReplace), replacedValue, replacer.ContentAsString);
			}
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals(VisualiserComponentTypes.TextEdit, GetNewValueProvider().ComponentType);
		}
	}
}
