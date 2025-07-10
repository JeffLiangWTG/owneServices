using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ExpressionNoteTemplate))]
	sealed class ExpressionNoteTemplateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTemplateText()
		{
			var noteTemplate = Factory.New<ExpressionNoteTemplate>();
			noteTemplate.TemplateText = "\"<SomeField>\" == \"___\" && \"<AnotherField>\" == \"___\"";
			AssertEquals(2, noteTemplate.Placeholders.Count);

			noteTemplate.Placeholders[0].Description = "Explanation of SomeField";
			noteTemplate.Placeholders[1].Description = "AnotherField explanation";

			noteTemplate.TemplateText = "\"<SomeField>\" == \"___\"";
			AssertEquals(1, noteTemplate.Placeholders.Count);
			AssertEquals("Explanation of SomeField", noteTemplate.Placeholders[0].Description);

			noteTemplate.TemplateText = "\"<SomeField>\" == \"___\" || \"<ThirdField>\" == \"___\"";
			AssertEquals(2, noteTemplate.Placeholders.Count);
		}

		public void TestHasChanges()
		{
			var noteTemplate = Factory.New<ExpressionNoteTemplate>();

			Factory.Save();
			AssertEquals(false, noteTemplate.HasChanges);
			noteTemplate.TemplateText = "aaa";
			AssertEquals(true, noteTemplate.HasChanges);

			Factory.Save();
			AssertEquals(false, noteTemplate.HasChanges);
			noteTemplate.TemplateText = "bbb";
			AssertEquals(true, noteTemplate.HasChanges);

			Factory.Save();
			AssertEquals(false, noteTemplate.HasChanges);
			noteTemplate.TemplateText = "";
			AssertEquals(true, noteTemplate.HasChanges);

			Factory.Save();
			AssertEquals(false, noteTemplate.HasChanges);
		}

		public void TestSaveAndLoad()
		{
			var noteTemplate = Factory.New<ExpressionNoteTemplate>();
			noteTemplate.TemplateText = "\"<SomeField>\" == \"___\" && \"<AnotherField>\" == \"___\"";
			noteTemplate.Placeholders[0].Description = "SomeField explanation";
			noteTemplate.Placeholders[1].Description = "AnotherField explanation";
			Factory.Save();

			AssertEquals("\"<SomeField>\" == \"___\" && \"<AnotherField>\" == \"___\"◄◘►SomeField explanation◄◘►AnotherField explanation", noteTemplate.S8_TemplateText);

			var loadedTemplate = new BusinessObjectFactory().Load<ExpressionNoteTemplate>(noteTemplate.PK);
			AssertEquals("\"<SomeField>\" == \"___\" && \"<AnotherField>\" == \"___\"", loadedTemplate.TemplateText);
			AssertEquals(2, loadedTemplate.Placeholders.Count);
			AssertEquals("SomeField explanation", loadedTemplate.Placeholders[0].Description);
			AssertEquals("AnotherField explanation", loadedTemplate.Placeholders[1].Description);
		}

		public void TestReplacement()
		{
			var noteTemplate = Factory.New<ExpressionNoteTemplate>();
			noteTemplate.TemplateText = "\"<SomeField>\" == \"___\" && \"<AnotherField>\" == \"___\" && \"<ThirdField>\" == \"___\"";
			noteTemplate.Placeholders[0].Replacement = "ABC";
			noteTemplate.Placeholders[1].Replacement = ZString.Empty;
			noteTemplate.Placeholders[2].Replacement = "XYZ";

			AssertEquals("\"<SomeField>\" == \"ABC\" && \"<AnotherField>\" == \"___\" && \"<ThirdField>\" == \"XYZ\"", noteTemplate.Replacement);
		}
	}
}
