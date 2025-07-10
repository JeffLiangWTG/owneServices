using System.Collections;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.Base.Filters
{
	public class PresentationCategoryFilter : ModuleTextFilter
	{
		public PresentationCategoryFilter(ZString description, GetTextQueryWithOperator queryDelegate, IList list)
			: base(description, queryDelegate, list)
		{
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new PresentationCategoryFilterValidation(this);
		}
	}

	public class PresentationCategoryFilterValidation : ModuleTextFilterValidation
	{
		public PresentationCategoryFilterValidation(ModuleTextFilter filter)
			: base(filter)
		{
		}

		protected override void CheckProperty()
		{
			var parent = GetParent();
			if (Parent.ComparisonOperator == ModuleTextFilter.ComparisonConstants.Exact || Parent.ComparisonOperator == ModuleTextFilter.ComparisonConstants.NotEqual)
			{
				parent.ErrorOnCodeNotPresent = true;
			}
			else
			{
				parent.ErrorOnCodeNotPresent = false;
			}
			base.CheckProperty();
		}
	}
}
