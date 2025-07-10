using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Application;

namespace Enterprise.Services.OperationalActions.Business
{
	[ImmutableObject(true)]
	public sealed partial class EnvironmentFilterProvider : IFilterValueProvider, IEnumerable<IFilterConstraint>
	{
		#region Instance
		static readonly Lazy<EnvironmentFilterProvider> instance = new Lazy<EnvironmentFilterProvider>(() => new EnvironmentFilterProvider());
		public static EnvironmentFilterProvider Instance
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return instance.Value; }
		}

		#endregion

		EnvironmentFilterProvider()
		{
			List<IFilterConstraint> constraints = new List<IFilterConstraint>(new IFilterConstraint[]
			{
				new FilterCompanyConstraint(),
				new FilterCountryConstraint(),
				new FilterBranchConstraint(),
				new FilterDepartmentConstraint(),
			});

			foreach (IFilterConstraint constraint in ObjectFactory.Get<IEnumerable>("OperationalActionsFilterConstraints"))
			{
				if (constraint != null)
				{
					constraints.Add(constraint);
				}
			}

			constraints.Sort((c1, c2) => StringComparer.OrdinalIgnoreCase.Compare(c1.Name, c2.Name));

			lookup = new SortedList<string, IFilterConstraint>(StringComparer.OrdinalIgnoreCase);

			foreach (IFilterConstraint constraint in constraints)
			{
				lookup.Add(constraint.Name, constraint);
			}

			lookup.TrimExcess();
		}

		#region IFilterValueProvider

		public IFilterConstraint GetConstraint(string constraintName)
		{
			IFilterConstraint constraint;
			lookup.TryGetValue(constraintName, out constraint);
			return constraint;
		}

		#endregion

		#region IEnumerable<IConstraint> Members

		public IEnumerator<IFilterConstraint> GetEnumerator()
		{
			return lookup.Values.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		readonly SortedList<string, IFilterConstraint> lookup;
	}
}
