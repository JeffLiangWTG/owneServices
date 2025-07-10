using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ModuleTranslatableTextFilter : ModuleTextFilter
	{
		public ModuleTranslatableTextFilter(ZString description, SchemaStringColumn filterColumn, CustomizableDataResourceStrings customizable)
			: base(description, filterColumn)
		{
			this.customizable = customizable;
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var predicate = SqlComparisonOperator.GetPredicate((string)Property);
			var captions = customizable.Source.GetRuntimeCaptions().Cast<ResourceString>().Where(caption => predicate.Invoke(caption)).Select(caption => caption.GetUnresolvedString());
			return new ZQuery(FilterColumn, SQLComparisonOperator.Equal, captions.ToArray());
		}

		public CustomizableDataResourceStrings Customizable
		{
			get { return customizable; }
		}

		readonly CustomizableDataResourceStrings customizable;
	}
}
