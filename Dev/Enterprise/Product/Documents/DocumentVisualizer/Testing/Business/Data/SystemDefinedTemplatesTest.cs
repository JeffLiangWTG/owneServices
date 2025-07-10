using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class SystemDefinedTemplatesTest : TestCaseWithFactory
	{
		#region TestSystemTemplatesPersistentSettings

		[SnailTest]
		public void TestSystemTemplatesPersistentSettings()
		{
			var query = new ZQuery(StmTemplateSchema.SO_TemplateType, StmTemplateTypes.Codes.Form);

			var systemTemplates = Factory.Load<VisualizerTemplate>(query);

			var errorBuilder = new StringBuilder();

			foreach (var systemTemplate in systemTemplates)
			{
				var worksheet = systemTemplate.GetFlexCelWorksheet();

				if (worksheet == null)
				{
					errorBuilder.AppendLine($"Could not create excel spreadsheet from '{systemTemplate.SO_Name}' template binary data.");
					continue;
				}

				var template = worksheet.CreateTemplate();

				if (template.IsLeft)
				{
					errorBuilder.AppendLine($"Template {systemTemplate.SO_Name} could not be parsed; {template.Left}");
					continue;
				}

				if (template.Right is StandardTemplate standardTemplate)
				{
					var dataContext = standardTemplate.GetDataContext();
					CheckTemplatePersistentSetting(errorBuilder, systemTemplate, StmTemplateSchema.SO_DataContext, dataContext);
				}
			}

			var errors = errorBuilder.ToString();
			AssertEquals(errors, errors, string.Empty);
		}

		void CheckTemplatePersistentSetting(StringBuilder errorBuilder, VisualizerTemplate systemTemplate, SchemaColumn settingStorage, string valueInTemplate)
		{
			var valueInTable = Convert.ToString(systemTemplate[settingStorage]);

			if ((!string.IsNullOrEmpty(valueInTemplate) || !string.IsNullOrEmpty(valueInTable))
				&& string.CompareOrdinal(valueInTemplate, valueInTable) != 0)
			{
				errorBuilder.AppendLine(
					$"Template '{systemTemplate.SO_Name}' has a discrepancy with the setting stored in the '{settingStorage.Name}' column. The template defines '{valueInTemplate}' but the database has value of '{valueInTable}'.");
			}
		}

		#endregion
	}
}