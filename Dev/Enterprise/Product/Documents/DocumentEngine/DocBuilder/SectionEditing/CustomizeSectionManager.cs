using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DocBuilder.SectionEditing
{
	public class CustomizeSectionManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CustomizeSectionManager(BusinessObjectFactory factory, ZString language)
			: base(factory)
		{
			this.sections = new TemplateSectionCollection(null);
			this.sectionsView = new TemplateSectionCollectionView(sections);
			this.sectionTemplates = new Dictionary<string, StmTemplateBase>();
			this.Language = language;
		}

		readonly TemplateSectionCollection sections;
		readonly TemplateSectionCollectionView sectionsView;
		readonly Dictionary<string, StmTemplateBase> sectionTemplates;

		public TemplateSectionCollectionView Sections
		{
			get { return sectionsView; }
		}

		public Dictionary<string, StmTemplateBase> SectionTemplates
		{
			get { return sectionTemplates; }
		}

		[List("Categories")]
		public ZString CategoryFilter
		{
			get { return sectionsView.CategoryFilter; }
			set
			{
				if (value != sectionsView.CategoryFilter)
				{
					sectionsView.CategoryFilter = value;
					sectionsView.Rebuild();
				}
			}
		}

		CodeDescriptionPairList categories;
		public CodeDescriptionPairList Categories
		{
			get { return categories ?? (categories = GetCategories()); }
		}

		CodeDescriptionPairList GetCategories()
		{
			var result = new UntranslatableCodeDescriptionPairList((NoResString)"DocBuilder Template section categories are not translatable"); // Untranslatable reason
			result.AddPair(string.Empty);

			foreach (TemplateSection section in sections)
			{
				var category = section.Category;
				if (!category.IsEmpty && !result.ContainsCode(category))
				{
					result.AddPair(category);
				}
			}

			return result;
		}

		[List("LanguageList")]
		public ZString Language
		{
			get { return language; }
			set
			{
				if (value != language)
				{
					language = value;
					UpdateSections();
				}
			}
		}
		ZString language;

		public CodeDescriptionPairList LanguageList
		{
			get
			{
				if (languageList == null)
				{
					languageList = new AvailableDocBuilderLanguageList(Factory);
					foreach (CodeDescriptionPair item in languageList.ToArray())
					{
						if (Res.IsEnglish(item.Code))
						{
							languageList.Remove(item);
						}
					}
					languageList.Add(new CodeDescriptionPairList(OLookUpEditType.Language)[Res.DefaultLanguage]);
				}
				return languageList;
			}
		}
		CodeDescriptionPairList languageList;

		void UpdateSections()
		{
			var templates = new List<StmTemplateBase>();

			AddTemplateIfNotNull(templates, StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System));
			AddTemplateIfNotNull(templates, StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.Customized));

			if (!Language.EqualsIgnoringCase(Res.DefaultLanguage) && LanguageList.ContainsCode(Language))
			{
				AddTemplateIfNotNull(templates, StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System, Language));
				AddTemplateIfNotNull(templates, StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.Customized, Language));
			}

			sectionTemplates.Clear();
			foreach (var template in templates)
			{
				foreach (TemplateSection section in template.TemplateSections)
				{
					section.Language = template.DocBuilderLanguageCode;

					if (!section.IsControlSection)
					{
						sectionTemplates[section.SectionName] = template;
					}
				}
			}

			sections.RemoveAll();
			foreach (var sectionTemplate in sectionTemplates)
			{
				var sectionName = sectionTemplate.Key;
				var template = sectionTemplate.Value;

				sections.Add(template.TemplateSections.Find(sectionName));
			}
		}

		void AddTemplateIfNotNull(List<StmTemplateBase> templates, StmTemplateBase template)
		{
			if (template != null)
			{
				templates.Add(template);
			}
		}
	}
}
