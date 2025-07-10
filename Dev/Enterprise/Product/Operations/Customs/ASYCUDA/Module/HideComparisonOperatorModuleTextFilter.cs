using System.Collections;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public class HideComparisonOperatorModuleTextFilter : ModuleTextFilter
	{
		public HideComparisonOperatorModuleTextFilter(ZString description, GetTextQuery queryDelegate, IList list)
			: base(description, queryDelegate, list)
		{
		}

		public HideComparisonOperatorModuleTextFilter(ZString description, SchemaStringColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		public HideComparisonOperatorModuleTextFilter(ZString description, SchemaStringColumn filterColumn, IList list)
			: base(description, filterColumn, list)
		{
		}

		public override bool HasComparisonOperator => false;
	}
}
