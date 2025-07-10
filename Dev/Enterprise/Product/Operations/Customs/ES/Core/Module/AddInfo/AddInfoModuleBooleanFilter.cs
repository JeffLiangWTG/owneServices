using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ES.Module
{
	public class AddInfoModuleBooleanFilter : ModuleTextFilter
	{
		public AddInfoModuleBooleanFilter(ZString description, GetTextQueryWithOperator queryDelegate, IList list)
			: base(description, queryDelegate, list)
		{
		}

		public override IReadOnlyList<string> AllowedComparisonOperators => new[] { string.Empty, ComparisonConstants.Exact };

		public override bool IsExpensiveQuery => true;
	}
}
