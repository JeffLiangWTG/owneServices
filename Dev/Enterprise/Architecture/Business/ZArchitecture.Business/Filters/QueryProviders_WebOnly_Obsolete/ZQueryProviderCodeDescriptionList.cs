using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public class ZQueryProviderCodeDescriptionList : ZQueryProviderCodeDescriptionListBase
	{
		public override void AddEmptySelection()
		{
			ZComparisonQueryProvider[] emptyColumn = new ZComparisonQueryProvider[] { new ZComparisonQueryProvider(null) };
			Add(new ZQueryProviderCodeDescription(FilterConstants.QueryDeciderNoSelectionCode, ResString.GetMultilingualString("4383b7c1-a265-4c36-bc34-9659944bb876", "None"), emptyColumn));
		}

		public void Add(ZString code, MultilingualString description, IQueryProvider queryProvider)
		{
			Add(code, description, SQLComparisonOperator.NotSpecified, queryProvider);
		}

		public void Add(ZString code, MultilingualString description, SQLComparisonOperator @operator, IQueryProvider queryProvider)
		{
			AddInternal(code, description, @operator, queryProvider);
		}

		public void Add(ZString code, MultilingualString description, SchemaColumn column)
		{
			Add(code, description, SQLComparisonOperator.NotSpecified, column);
		}

		public void Add(ZString code, MultilingualString description, SQLComparisonOperator @operator, SchemaColumn column)
		{
			AddInternal(code, description, @operator, column);
		}

		public void Add(ZString code, MultilingualString description, AddToQueryDelegate @delegate)
		{
			Add(code, description, SQLComparisonOperator.NotSpecified, @delegate);
		}

		public void Add(ZString code, MultilingualString description, SQLComparisonOperator @operator, AddToQueryDelegate @delegate)
		{
			AddInternal(code, description, @operator, @delegate);
		}
	}
}
