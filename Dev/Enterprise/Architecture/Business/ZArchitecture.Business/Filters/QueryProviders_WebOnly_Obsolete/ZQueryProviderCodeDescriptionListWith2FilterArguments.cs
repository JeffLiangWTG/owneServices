using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public class ZQueryProviderCodeDescriptionListWith2FilterArguments : ZQueryProviderCodeDescriptionListBase
	{
		public override void AddEmptySelection()
		{
			ZComparisonQueryProvider[] emptyColumns = new ZComparisonQueryProvider[] { new ZComparisonQueryProvider(null), new ZComparisonQueryProvider(null) };
			Add(new ZQueryProviderCodeDescription(FilterConstants.QueryDeciderNoSelectionCode, ResString.GetMultilingualString("4383b7c1-a265-4c36-bc34-9659944bb876", "None"), emptyColumns));
		}

		public void Add(ZString code, MultilingualString description, IQueryProvider queryProvider1, IQueryProvider queryProvider2)
		{
			Add(code, description, SQLComparisonOperator.NotSpecified, queryProvider1, queryProvider2);
		}

		public void Add(ZString code, MultilingualString description, SQLComparisonOperator @operator, IQueryProvider queryProvider1, IQueryProvider queryProvider2)
		{
			AddInternal(code, description, @operator, queryProvider1, queryProvider2);
		}

		public void Add(ZString code, MultilingualString description, SchemaColumn propertyName1, SchemaColumn propertyName2)
		{
			Add(code, description, SQLComparisonOperator.NotSpecified, propertyName1, propertyName2);
		}

		public void Add(ZString code, MultilingualString description, SQLComparisonOperator @operator, SchemaColumn propertyName1, SchemaColumn propertyName2)
		{
			AddInternal(code, description, @operator, propertyName1, propertyName2);
		}

		public void Add(ZString code, MultilingualString description, AddToQueryDelegate delegate1, AddToQueryDelegate delegate2)
		{
			Add(code, description, SQLComparisonOperator.NotSpecified, delegate1, delegate2);
		}

		public void Add(ZString code, MultilingualString description, SQLComparisonOperator @operator, AddToQueryDelegate delegate1, AddToQueryDelegate delegate2)
		{
			AddInternal(code, description, @operator, delegate1, delegate2);
		}

		#region Add with 2 same filter arguments

		/// <summary>
		/// Typically this will be used for FROM - TO date filter
		/// </summary>        
		public void Add(ZString code, MultilingualString description, AddToQueryDelegate delegateForBothFilterArguments)
		{
			Add(code, description, delegateForBothFilterArguments, delegateForBothFilterArguments);
		}

		/// <summary>
		/// Typically this will be used for FROM - TO date filter
		/// </summary>        
		public void Add(ZString code, MultilingualString description, SQLComparisonOperator @operator, AddToQueryDelegate delegateForBothFilterArguments)
		{
			Add(code, description, @operator, delegateForBothFilterArguments, delegateForBothFilterArguments);
		}

		/// <summary>
		/// Typically this will be used for FROM - TO date filter
		/// </summary>
		public void Add(ZString code, MultilingualString description, IQueryProvider queryProviderForBothFilterArguments)
		{
			Add(code, description, queryProviderForBothFilterArguments, queryProviderForBothFilterArguments);
		}

		/// <summary>
		/// Typically this will be used for FROM - TO date filter
		/// </summary>
		public void Add(ZString code, MultilingualString description, SQLComparisonOperator @operator, IQueryProvider queryProviderForBothFilterArguments)
		{
			Add(code, description, @operator, queryProviderForBothFilterArguments, queryProviderForBothFilterArguments);
		}

		/// <summary>
		/// Typically this will be used for FROM - TO date filter
		/// </summary>
		public void Add(ZString code, MultilingualString description, SchemaColumn propertyNameForBothFilterArguments)
		{
			Add(code, description, propertyNameForBothFilterArguments, propertyNameForBothFilterArguments);
		}

		/// <summary>
		/// Typically this will be used for FROM - TO date filter
		/// </summary>
		public void Add(ZString code, MultilingualString description, SQLComparisonOperator @operator, SchemaColumn propertyNameForBothFilterArguments)
		{
			Add(code, description, @operator, propertyNameForBothFilterArguments, propertyNameForBothFilterArguments);
		}

		#endregion
	}
}
