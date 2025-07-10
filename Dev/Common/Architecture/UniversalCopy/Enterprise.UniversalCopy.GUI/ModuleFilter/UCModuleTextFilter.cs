using System.Collections;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.UniversalCopy.GUI
{
	public class UCModuleTextFilter : ModuleTextFilter
	{
		public UCModuleTextFilter(ZString description, SchemaStringColumn filterColumn, IList list)
			: base(description, filterColumn, list)
		{
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new UCModuleTextFilterValidation(this);
		}
	}
}
