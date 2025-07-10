using System.Collections.Generic;
using System.Linq;

namespace Enterprise.BusinessObjectGenerator.ModelView
{
	public class SqlIndexedView : SqlView
	{
		public SqlIndexedView(ModelViewContext context) : base(context)
		{ }

		internal override IEnumerable<AddInfo> AddInfosToAdd(View view)
		{
			return view.AddInfos.Where(a => a.Indexed);
		}

		internal override bool NeedGeneration() => Context.HasIndex;

		internal override string GetJoinClause() => string.Empty;

		internal override string GetColumnDefinition(AddInfo addInfo, string addInfoColumnName)
		{
			return SqlHelper.GetColumnDefinition(addInfo, addInfoColumnName);
		}

		internal override string ViewName => IndexedViewName;

		internal override bool IncludeUnderlyingColumns => false;

		internal override bool QualifyColumns => false;
	}
}
