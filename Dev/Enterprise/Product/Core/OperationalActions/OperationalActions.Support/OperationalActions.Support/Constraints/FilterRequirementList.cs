using System;
using System.Collections;
using System.Collections.Generic;

namespace Enterprise.Services.OperationalActions.Support
{
	public sealed class FilterRequirementList : ICollection<FilterRequirement>, ICollection
	{
		public FilterRequirementList()
		{
			lookup = new Dictionary<string, FilterRequirement>(StringComparer.OrdinalIgnoreCase);
		}

		public int Count
		{
			get { return lookup.Count; }
		}

		public void Add(string constraint, string[] values)
		{
			if (constraint == null)
			{
				throw new ArgumentNullException(nameof(constraint));
			}

			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}

			FilterRequirement existing;

			if (lookup.TryGetValue(constraint, out existing))
			{
				existing.AddRange(values);
			}
			else
			{
				existing = new FilterRequirement(constraint);
				existing.AddRange(values);

				lookup.Add(constraint, existing);
			}
		}

		public void Add(FilterRequirement requirement)
		{
			if (requirement == null)
			{
				throw new ArgumentNullException(nameof(requirement));
			}

			FilterRequirement existing;

			if (lookup.TryGetValue(requirement.ConstraintName, out existing))
			{
				existing.AddRange(requirement);
			}
			else
			{
				lookup.Add(requirement.ConstraintName, requirement);
			}
		}

		public bool RemoveConstraint(string constraint)
		{
			if (constraint == null)
			{
				throw new ArgumentNullException(nameof(constraint));
			}

			return lookup.Remove(constraint);
		}

		public FilterRequirement this[string constraint]
		{
			get
			{
				if (constraint == null)
				{
					throw new ArgumentNullException(nameof(constraint));
				}

				FilterRequirement result;
				lookup.TryGetValue(constraint, out result);
				return result;
			}
		}

		#region ICollection<FilterRequirement> Members

		void ICollection<FilterRequirement>.Clear()
		{
			lookup.Clear();
		}

		bool ICollection<FilterRequirement>.Contains(FilterRequirement item)
		{
			throw new NotSupportedException();
		}

		void ICollection<FilterRequirement>.CopyTo(FilterRequirement[] array, int arrayIndex)
		{
			lookup.Values.CopyTo(array, arrayIndex);
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		bool ICollection<FilterRequirement>.IsReadOnly
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return false; }
		}

		bool ICollection<FilterRequirement>.Remove(FilterRequirement item)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region ICollection Members

		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)lookup.Values).CopyTo(array, index);
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		object ICollection.SyncRoot
		{
			get { return ((ICollection)lookup).SyncRoot; }
		}

		#endregion

		#region IEnumerable<FilterRequirement> Members

		public IEnumerator<FilterRequirement> GetEnumerator()
		{
			return lookup.Values.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		[System.Diagnostics.DebuggerStepThrough]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		readonly Dictionary<string, FilterRequirement> lookup;
	}
}
