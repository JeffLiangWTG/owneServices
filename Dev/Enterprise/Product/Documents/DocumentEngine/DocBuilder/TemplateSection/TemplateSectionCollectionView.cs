using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public sealed class TemplateSectionCollectionView : BusinessObjectCollectionView<TemplateSection>
	{
		public TemplateSectionCollectionView(StmTemplateBase template)
			: this(new TemplateSectionCollection(template.GetExcelTemplate()))
		{
		}

		public TemplateSectionCollectionView(TemplateSectionCollection collectionToFilter)
			: base(collectionToFilter)
		{
			Rebuild();
		}

		public new TemplateSectionCollection CollectionToFilter
		{
			get { return (TemplateSectionCollection)base.CollectionToFilter; }
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var section = (TemplateSection)element;
			if (section.IsControlSection)
			{
				return false;
			}
			else
			{
				if (!CategoryFilter.IsEmpty && !section.Category.Trim().EqualsIgnoringCase(CategoryFilter.Trim()))
				{
					return false;
				}
			}

			return true;
		}

		public ZString CategoryFilter { get; set; }

		#region Override AllowNew to false
		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException();
		}

		protected override BusinessObject AddNewCore(Type bizOType)
		{
			throw new NotSupportedException();
		}
		#endregion
	}
}
