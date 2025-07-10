using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DocBuilder;

namespace Enterprise.DocumentEngine.Business
{
	[DependentBusinessObject(typeof(StmMenuDocumentConfig), "ConfigItems")]
	public class StmMenuDocumentConfigItem : AutoStmMenuDocumentConfigItem, IDocumentConfigItem
	{
		public StmMenuDocumentConfigItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public StmMenuDocumentConfig ParentConfig
		{
			get { return Factory.Load<StmMenuDocumentConfig>(S4_S3); }
		}

		[ReadOnly(true)]
		public override ZString S4_SectionItemName
		{
			get { return base.S4_SectionItemName; }
			set { base.S4_SectionItemName = value; }
		}

		[ReadOnly(true)]
		public override ZInt S4_PrintOrder
		{
			get { return base.S4_PrintOrder; }
			set { base.S4_PrintOrder = value; }
		}

		[List("Lookups.GenericSectionTypes")]
		public override ZString S4_SectionType
		{
			get { return base.S4_SectionType; }
			set { base.S4_SectionType = value; }
		}

		protected bool S4_SectionType_ReadOnly
		{
			get { return !IsGenericSectionType; }
		}

		internal bool IsGenericSectionType
		{
			get { return TemplateSection != null && TemplateSection.IsGenericSectionType; }
		}

		public MenuEditingMode EditingMode
		{
			get { return editingMode; }
			set
			{
				editingMode = value;

				if (ParentConfig != null)
				{
					switch (value)
					{
						case MenuEditingMode.AllowEditingOfClientSpecificOnly:
							ReadOnly = (ParentConfig.S3_IsSystem && ParentConfig.S3_OH.IsEmpty);
							break;

						case MenuEditingMode.AllowEditingOfSystemDefinedOnly:
							ReadOnly = (ParentConfig.S3_IsSystem && !ParentConfig.S3_OH.IsEmpty);
							break;

						case MenuEditingMode.NotAllowEditingOfSystemOrClientMenus:
							ReadOnly = (ParentConfig.S3_IsSystem || !ParentConfig.S3_OH.IsEmpty);
							break;

						case MenuEditingMode.AllowAll:
						default:
							ReadOnly = false;
							break;
					}
				}
			}
		}

		MenuEditingMode editingMode;

		public TemplateSection TemplateSection
		{
			get { return templateSection ?? (templateSection = GetTemplateSection()); }
		}

		TemplateSection GetTemplateSection()
		{
			TemplateSection result = null;
			var config = ParentConfig;
			if (config != null)
			{
				var templateSections = config.TemplateSections;
				if (templateSections != null)
				{
					result = templateSections.Find(S4_SectionItemName);
				}
			}

			return result;
		}

		TemplateSection templateSection;

		#region IDocumentConfigItem Members

		ZString IDocumentConfigItem.SectionName
		{
			get { return S4_SectionItemName; }
		}

		ZString IDocumentConfigItem.SectionType
		{
			get { return S4_SectionType; }
		}

		ZString IDocumentConfigItem.FilterList
		{
			get { return S4_FilterList; }
		}

		bool? IDocumentConfigItem.EvaluatedValue { get; set; }

		#endregion
	}
}
