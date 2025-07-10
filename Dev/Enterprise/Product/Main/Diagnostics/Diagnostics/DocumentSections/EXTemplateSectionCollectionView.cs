using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Diagnostics
{
	public sealed class EXTemplateSectionCollectionView : BusinessObjectCollectionView<EXTemplateSection>
	{
		public EXTemplateSectionCollectionView(EXTemplateSectionCollection collectionToFilter)
			: base(collectionToFilter)
		{
			Rebuild();
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var section = element as EXTemplateSection;

			if (!CategoryFilter.IsEmpty
				&& section != null
				&& !section.Category.Trim().EqualsIgnoringCase(CategoryFilter.Trim()))
			{
				return false;
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

		protected override BusinessObject AddNewCore(Type bizoType)
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
