using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed partial class FilterUpdateNode : UpdateNode
	{
		internal FilterUpdateNode(Type expectedType, IFilterExpression filter)
		{
			this.Filter = filter;
			this.provider = new BusinessObjectFilterValueProvider(expectedType);
		}

		public IFilterExpression Filter { get; private set; }

		public override IEnumerable<BusinessObject> Apply(IEnumerable<BusinessObject> targets)
		{
			return base.Apply(CollectionTargets(targets));
		}

		IEnumerable<BusinessObject> CollectionTargets(IEnumerable<BusinessObject> fromTargets)
		{
			List<BusinessObject> result = new List<BusinessObject>();

			foreach (BusinessObject target in fromTargets)
			{
				if (MatchesFilter(target))
				{
					result.Add(target);
				}
			}

			return result;
		}

		bool MatchesFilter(BusinessObject businessObject)
		{
			try
			{
				provider.Source = businessObject;
				return Filter.Evaluate(provider);
			}
			finally
			{
				provider.Source = null;
			}
		}

		readonly BusinessObjectFilterValueProvider provider;
	}
}

#region Test
#if DEBUG

#region DisplayProxy

namespace Enterprise.Services.OperationalActions.Business
{
	[System.Diagnostics.DebuggerTypeProxy(typeof(FilterUpdateNode.FilterDisplayProxy))]
	partial class FilterUpdateNode
	{
		sealed class FilterDisplayProxy : UpdateNode.DisplayProxy<FilterUpdateNode>
		{
			public FilterDisplayProxy(FilterUpdateNode parent)
				: base(parent) { }

			public IFilterExpression Filter
			{
				get { return parent.Filter; }
			}
		}
	}
}

#endregion

#endif
#endregion
