using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	#region class ModuleNumberFilter

	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ModuleNumberFilter : ModuleTextFilter
	{
		#region Construction

		protected ModuleNumberFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
			UseMultiSearch = true;
		}

		public ModuleNumberFilter(ZString description, SchemaStringColumn filterColumn)
			: base(description, filterColumn)
		{
			UseMultiSearch = true;
		}

		public ModuleNumberFilter(ZString description, GetTextQueryWithOperator queryDelegate)
			: base(description, queryDelegate)
		{
			UseMultiSearch = true;
		}

		#endregion

		#region GetNewCommonModuleFilter

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ModuleNumberFilter(category, parentCollection);
		}

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.NumbersAndReferences; }
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			Property = RandomInt(100).ToString(CultureInfo.InvariantCulture);
		}

#endif
		#endregion
	}

	#endregion
}
