using System;
using System.Collections.ObjectModel;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class ZCompositeQueryProvider : IQueryProvider
	{
		public ZCompositeQueryProvider(SQLComparisonOperator[] operators, params IQueryProvider[] providers)
		{
			if (operators.Length != providers.Length)
			{
				throw new ArgumentException("Operators length not equal to Providers length");
			}
			this.Operators = new ReadOnlyCollection<SQLComparisonOperator>(operators);
			this.Providers = new ReadOnlyCollection<IQueryProvider>(providers);
		}

		public readonly ReadOnlyCollection<IQueryProvider> Providers;
		public readonly ReadOnlyCollection<SQLComparisonOperator> Operators;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public ZQuery GetQuery(SQLComparisonOperator @operator, object value)
		{
			int i = 0;
			ZQuery query = new ZQuery();
			foreach (IQueryProvider provider in Providers)
			{
				SQLComparisonOperator effectiveOperator = @operator;
				if (Operators[i] != SQLComparisonOperator.NotSpecified)
				{
					effectiveOperator = Operators[i];
				}
				query.AddToFilter(provider.GetQuery(effectiveOperator, value), JoinCondition.Or);
				i++;
			}
			return query;
		}
	}
}
