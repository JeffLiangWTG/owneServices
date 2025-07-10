using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;

namespace Enterprise.DocumentEngine.DocBuilder.SectionEditing
{
	public class CustomizeSectionController
	{
		public CustomizeSectionController(CustomizeSectionManager manager, ICustomizeSectionView view)
		{
			this.manager = manager;
			this.view = view;
		}

		readonly CustomizeSectionManager manager;
		readonly ICustomizeSectionView view;

		public void CopySection(TemplateSection section, ZString language)
		{
			var sourceTemplate = GetSourceTemplate(section.SectionName, language);
			var targetTemplate = GetCustomizedDocumentElements(language);
			var customizedSection = targetTemplate.TemplateSections.Find(section.SectionName);
			if (customizedSection != null)
			{
				if (sourceTemplate != targetTemplate)
				{
					if (view.ShouldOverrideExistingSection(section.SectionName, targetTemplate.SO_Name))
					{
						var row = customizedSection.StartingRowNumber - 1;
						var builder = new DocBuilderTemplateBuilder(targetTemplate);
						builder.RemoveSection(customizedSection.SectionName);

						var copiedSection = builder.CopySectionAndInsertAt(sourceTemplate, section.SectionName, row);
						EditTemplateSection(targetTemplate, copiedSection);
					}
					else
					{
						EditTemplateSection(targetTemplate, customizedSection);
					}
				}
				else
				{
					view.ShowEditOnlyMessage(section.SectionName, targetTemplate.SO_Name);
					EditTemplateSection(targetTemplate, customizedSection);
				}
			}
			else
			{
				var builder = new DocBuilderTemplateBuilder(targetTemplate);
				var copiedSection = builder.CopySectionAndInsertAtEnd(sourceTemplate, section.SectionName);
				EditTemplateSection(targetTemplate, copiedSection);
			}
		}

		StmTemplateBase GetSourceTemplate(ZString sectionName, ZString language)
		{
			var templates = new List<StmTemplateBase>();

			if (!language.IsEmpty && !language.EqualsIgnoringCase(Enterprise.Core.Constants.Languages.English))
			{
				AddTemplateIfNotNull(templates, StmTemplateBase.GetDocBuilderTemplate(manager.Factory, DocBuilderTemplateType.System, language));
			}

			AddTemplateIfNotNull(templates, StmTemplateBase.GetDocBuilderTemplate(manager.Factory, DocBuilderTemplateType.System));
			AddTemplateIfNotNull(templates, StmTemplateBase.GetDocBuilderTemplate(manager.Factory, DocBuilderTemplateType.Customized));

			if (!language.IsEmpty && !language.EqualsIgnoringCase(Enterprise.Core.Constants.Languages.English))
			{
				AddTemplateIfNotNull(templates, StmTemplateBase.GetDocBuilderTemplate(manager.Factory, DocBuilderTemplateType.Customized, language));
			}

			foreach (var template in templates)
			{
				if (template.TemplateSections.Find(sectionName) != null)
				{
					return template;
				}
			}

			return null;
		}

		void AddTemplateIfNotNull(List<StmTemplateBase> templates, StmTemplateBase template)
		{
			if (template != null)
			{
				templates.Add(template);
			}
		}

		void EditTemplateSection(StmTemplateBase template, TemplateSection section)
		{
			var editor = view.GetTemplateEditor(template);
			editor.Edit(section.StartingRowNumber - 1, 0);
		}

		StmTemplateBase GetCustomizedDocumentElements(ZString language)
		{
			var result = StmTemplateBase.GetDocBuilderTemplate(manager.Factory, DocBuilderTemplateType.Customized, language);
			if (result == null)
			{
				var templateFactory = new CustomizedDocumentElementsTemplateCreator(manager.Factory);
				result = templateFactory.Create(language);
			}

			return result;
		}
	}
}
