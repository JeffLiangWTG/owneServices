using System.Diagnostics;
using CargoWise.Types;
using GlowIndexQueryService.Business;

namespace Enterprise.ZArchitecture.Business
{
	public class IndexSearchModuleFountainFilter : IndexSearchModuleTextFilter, IIndexSearchModuleFilter
	{
		public IndexSearchModuleFountainFilter(SearchField searchField, string fountainPrefix = "", int fountainPaddingLength = 8, string description = null, FilterCategory filterCategory = null)
		: base(searchField, description, filterCategory)
		{
			prefix = fountainPrefix;
			paddingLength = fountainPaddingLength;
		}

		readonly ZString prefix;

		readonly int paddingLength;

		protected override FilterCategory DefaultCategory => FilterCategories.NumbersAndReferences;

		public override ZString Property
		{
			[DebuggerStepThrough]
			get
			{
				return base.Property;
			}
			set
			{
				base.Property = value;
				ExpandValue();
			}
		}

		public override ZString ComparisonOperator
		{
			[DebuggerStepThrough]
			get
			{
				return base.ComparisonOperator;
			}
			set
			{
				base.ComparisonOperator = value;
				ExpandValue();
			}
		}

		void ExpandValue()
		{
			if (!Property.IsEmpty)
			{
				var property = Property.ToUpper();
				if (property.Length < checked(prefix.Length + paddingLength) && (!property.StartsWith(prefix) || prefix.IsEmpty))
				{
					Property = prefix + property.PadLeft(paddingLength, '0');
				}
			}
		}
	}
}
