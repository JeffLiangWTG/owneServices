using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public class SectionPreviewManager : NonPersistentBusinessObject
	{
		public SectionPreviewManager(BusinessObjectFactory factory, ZString sectionName, ZString category, ZString documentTitle, ContactType contactType, DocumentDirection documentDirection, params IBODocDataProvider[] docDataProviders)
			: base(factory)
		{
			this.sectionName = sectionName;
			this.category = category;
			this.languages = new AvailableDocBuilderLanguageList(Factory);
			this.documentTitle = documentTitle;
			this.docDataProviders = docDataProviders;
			this.contactType = contactType;
			this.documentDirection = documentDirection;
		}

		readonly ZString sectionName;
		readonly ZString category;
		readonly AvailableDocBuilderLanguageList languages;
		readonly ZString documentTitle;
		readonly IBODocDataProvider[] docDataProviders;
		readonly ContactType contactType;
		readonly DocumentDirection documentDirection;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			Zoom = 100;
		}

		public ContactType ContactType
		{
			get { return contactType; }
		}

		public DocumentDirection DocumentDirection
		{
			get { return documentDirection; }
		}

		[MaxLength(80)]
		public ZString SectionName
		{
			get { return sectionName; }
		}

		public ZPropertyInfo SectionNameInfo
		{
			get { return GetZPropertyInfo(nameof(SectionName)); }
		}

		[MaxLength(30)]
		public ZString Category
		{
			get { return category; }
		}

		public ZPropertyInfo CategoryInfo
		{
			get { return GetZPropertyInfo(nameof(Category)); }
		}

		[List("Languages")]
		[MaxLength(7)]
		public ZString Language
		{
			get { return language; }
			set
			{
				if (value != language)
				{
					CheckMaximumLength(LanguageInfo, value);
					language = value;

					UpdateSystemExcelTemplate();
					UpdateCustomizedExcelTemplate();

					LanguageInfo.RefreshBinding();
				}
			}
		}
		ZString language;

		public ZPropertyInfo LanguageInfo
		{
			get { return GetZPropertyInfo(nameof(Language)); }
		}

		public AvailableDocBuilderLanguageList Languages
		{
			get { return languages; }
		}

		public ZString DocumentTitle
		{
			get { return documentTitle; }
		}

		public IReadOnlyCollection<IBODocDataProvider> DocDataProviders
		{
			get { return docDataProviders; }
		}

		public ZInt Zoom
		{
			get { return zoom; }
			set { SetNonPersistentPropertyValue<ZInt>(ZoomInfo, ref zoom, value); }
		}
		ZInt zoom;

		public ZPropertyInfo ZoomInfo
		{
			get { return GetZPropertyInfo(nameof(Zoom)); }
		}

		public ZBool IsOriginalViewVisible
		{
			get { return isOriginalViewVisible; }
			set { SetNonPersistentPropertyValue<ZBool>(IsOriginalViewVisibleInfo, ref isOriginalViewVisible, value); }
		}
		ZBool isOriginalViewVisible;

		public ZPropertyInfo IsOriginalViewVisibleInfo
		{
			get { return GetZPropertyInfo(nameof(IsOriginalViewVisible)); }
		}

		public ZBool IsCustomizedViewVisible
		{
			get { return isCustomizedViewVisible; }
			set { SetNonPersistentPropertyValue<ZBool>(IsCustomizedViewVisibleInfo, ref isCustomizedViewVisible, value); }
		}
		ZBool isCustomizedViewVisible;

		public ZPropertyInfo IsCustomizedViewVisibleInfo
		{
			get { return GetZPropertyInfo(nameof(IsCustomizedViewVisible)); }
		}

		public ExcelTemplate SystemExcelTemplate
		{
			get { return systemExcelTemplate; }
			private set
			{
				if (value != systemExcelTemplate)
				{
					systemExcelTemplate = value;
				}
			}
		}
		ExcelTemplate systemExcelTemplate;

		public ExcelTemplate CustomizedExcelTemplate
		{
			get { return customizedExcelTemplate; }
			private set
			{
				if (value != customizedExcelTemplate)
				{
					customizedExcelTemplate = value;
				}
			}
		}
		ExcelTemplate customizedExcelTemplate;

		public SectionPreviewManagerValidation Validation
		{
			get { return new SectionPreviewManagerValidation(this); }
		}

		#region Implementation

		void UpdateSystemExcelTemplate()
		{
			SystemExcelTemplate = GetExcelTemplate(Language, false);
		}

		void UpdateCustomizedExcelTemplate()
		{
			if (IsCustomized)
			{
				CustomizedExcelTemplate = GetExcelTemplate(Language, true);
			}
			else
			{
				CustomizedExcelTemplate = null;
			}
		}

		bool IsCustomized
		{
			get
			{
				var templates = new List<StmTemplateBase>();

				if (!Language.IsEmpty && Language != Enterprise.Core.Constants.Languages.English)
				{
					var template = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.Customized, Language);
					if (template != null)
					{
						templates.Add(template);
					}
				}

				{
					var template = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.Customized);
					if (template != null)
					{
						templates.Add(template);
					}
				}

				foreach (var template in templates)
				{
					if (template.TemplateSections.Contains(SectionName))
					{
						return true;
					}
				}

				return false;
			}
		}

		ExcelTemplate GetExcelTemplate(ZString language, bool includeCustomizedSections)
		{
			var configItem = new SectionPreviewDocumentConfigItem();
			configItem.SectionName = SectionName;
			configItem.SectionType = GenericSectionUsageList.Codes.BodySection;

			var config = new SectionPreviewDocumentConfig(DocumentTitle);
			config.ConfigItems.Add(configItem);

			var systemTemplate = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			var generator = new TemplateGenerator(config, systemTemplate, language, includeCustomizedSections);

			return generator.Generate(null);
		}

		class SectionPreviewDocumentConfigItem : IDocumentConfigItem
		{
			[MaxLength(80)]
			public ZString SectionName { get; set; }
			public ZString SectionType { get; set; }
			public ZString FilterList { get; set; }
			bool? IDocumentConfigItem.EvaluatedValue { get; set; }
		}

		class SectionPreviewDocumentConfigItemCollection : List<IDocumentConfigItem>, IDocumentConfigItemCollection
		{
			public void SortByPrintOrder()
			{
				// Don't need to sort as section preview only ever contains one section.
			}
		}

		class SectionPreviewDocumentConfig : IDocumentConfig
		{
			public SectionPreviewDocumentConfig(string documentTitle)
			{
				this.documentTitle = documentTitle;
			}

			readonly string documentTitle;
			readonly IDocumentConfigItemCollection configItems = new SectionPreviewDocumentConfigItemCollection();

			public IDocumentConfigItemCollection ConfigItems
			{
				get { return configItems; }
			}

			public string PageStyle
			{
				get { return DocumentConfigPageStyleList.Codes.Portrait; }
			}

			public string OverrideDataContext
			{
				get { return ZString.Empty; }
			}

			public string DocumentTitle
			{
				get { return documentTitle; }
			}

			public string DocumentType
			{
				get { return ZString.Empty; }
			}

			public bool IsTemplate
			{
				get { return false; }
			}
		}

		#endregion
	}
}
