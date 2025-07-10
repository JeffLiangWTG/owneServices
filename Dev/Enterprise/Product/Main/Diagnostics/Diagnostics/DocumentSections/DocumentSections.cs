using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Diagnostics
{
	public class DocumentSections : NonPersistentBusinessObject
	{
		public DocumentSections(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public EXTemplateSectionCollectionView AvailableSections
		{
			get
			{
				if (availableSections == null)
				{
					availableSections = new EXTemplateSectionCollectionView(GetNewCollection());
					availableSections.SetReadOnlyIncludingChildren(true);
				}
				return availableSections;
			}
		}
		EXTemplateSectionCollectionView availableSections;

		public CodeDescriptionPairList Categories
		{
			get { return categories ?? (categories = GetCategories()); }
		}
		CodeDescriptionPairList categories;

		CodeDescriptionPairList GetCategories()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(string.Empty);

			foreach (var category in AvailableSections.Cast<EXTemplateSection>()
				.Select(section => section.Category)
				.Where(c => !c.IsEmpty)
				.Distinct()
				.OrderBy(c => c))
			{
				result.AddPair(category);
			}

			return result;
		}

		public ZString CategoryFilter
		{
			get { return categoryFilter; }
			set
			{
				if (value != categoryFilter)
				{
					categoryFilter = value;
					AvailableSections.CategoryFilter = value;
					AvailableSections.Rebuild();
				}
			}
		}
		ZString categoryFilter;

		EXTemplateSectionCollection GetNewCollection()
		{
			var result = new EXTemplateSectionCollection();

			var systemTemplateQuery = new ZQuery(StmTemplateSchema.SO_Name, SectionRepositoryTemplateNames.System);
			var systemTemplate = Factory.LoadTop1<StmTemplateBase>(systemTemplateQuery);

			if (systemTemplate != null)
			{
				var userTemplateQuery = new ZQuery(StmTemplateSchema.SO_Name, SectionRepositoryTemplateNames.User);
				var userTemplate = Factory.LoadTop1<StmTemplateBase>(userTemplateQuery);
				var userTemplateSections = userTemplate != null ? userTemplate.TemplateSections.Cast<TemplateSection>().ToArray() : null;

				if (userTemplateSections != null)
				{
					result.AddRange(userTemplateSections.Select(section => new EXTemplateSection(section, true)));
				}

				foreach (TemplateSection systemSection in systemTemplate.TemplateSections)
				{
					if (userTemplateSections == null
						|| !userTemplateSections.Any(section => section.SectionName.EqualsIgnoringCase(systemSection.SectionName)))
					{
						result.Add(new EXTemplateSection(systemSection, false));
					}
				}
			}

			return result;
		}
	}
}
