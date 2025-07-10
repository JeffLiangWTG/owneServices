using System;
using System.Collections;
using System.Collections.Generic;

namespace Enterprise.Services.OperationalActions.Support
{
	[System.Diagnostics.DebuggerDisplay("Count = {Count}")]
	public sealed class FilterRequirement : ICollection<string>, ICollection
	{
		public FilterRequirement(string constraintName)
		{
			if (constraintName == null)
			{
				throw new ArgumentNullException(nameof(constraintName));
			}

			ConstraintName = constraintName;
			values = new List<string>();
		}

		public string ConstraintName { get; private set; }

		public int Count
		{
			get { return values.Count; }
		}

		public void Add(string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			int index = values.BinarySearch(value);

			if (index < 0)
			{
				values.Insert(~index, value);
			}
		}

		public void AddRange(string[] values)
		{
			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}

			if (values.Length > 0)
			{
				Array.Sort(values);

				this.values = Union(this.values, values);
			}
		}

		public void AddRange(FilterRequirement requirement)
		{
			if (requirement == null)
			{
				throw new ArgumentNullException(nameof(requirement));
			}

			this.values = Union(this.values, requirement.values);
		}

		public bool Remove(string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			int index = values.BinarySearch(value);

			if (index < 0)
			{
				return false;
			}
			else
			{
				values.RemoveAt(index);
				return true;
			}
		}

		public bool Contains(string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return values.BinarySearch(value) >= 0;
		}

		public bool Overlaps(FilterRequirement requirement)
		{
			if (requirement == null)
			{
				throw new ArgumentNullException(nameof(requirement));
			}

			return Overlapps(values, requirement.values);
		}

		public bool IsSuperSetOf(FilterRequirement requirement)
		{
			if (requirement == null)
			{
				throw new ArgumentNullException(nameof(requirement));
			}

			return IsSuperSetOf(values, requirement.values);
		}

		#region ICollection<string> Members

		void ICollection<string>.Clear()
		{
			values.Clear();
		}

		void ICollection<string>.CopyTo(string[] array, int arrayIndex)
		{
			values.CopyTo(array, arrayIndex);
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		bool ICollection<string>.IsReadOnly
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return false; }
		}

		#endregion

		#region ICollection Members

		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)values).CopyTo(array, index);
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		bool ICollection.IsSynchronized
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return false; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		object ICollection.SyncRoot
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return this; }
		}

		#endregion

		#region IEnumerable<string> Members

		public IEnumerator<string> GetEnumerator()
		{
			return values.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		[System.Diagnostics.DebuggerStepThrough]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Implementation

		static List<string> Union(List<string> list1, IList<string> list2)
		{
			List<string> result = new List<string>(list1.Count + list2.Count);

			int read1 = 0;
			int read2 = 0;

			while (read1 < list1.Count && read2 < list2.Count)
			{
				string v1 = list1[read1];
				string v2 = list2[read2];

				int diff = string.CompareOrdinal(v1, v2);

				if (diff < 0)
				{
					result.Add(v1);
					read1++;
				}
				else if (diff > 0)
				{
					result.Add(v2);

					do
					{
						read2++;
					} while (read2 < list2.Count && list2[read2] == v2);
				}
				else
				{
					result.Add(v1);
					read1++;

					do
					{
						read2++;
					} while (read2 < list2.Count && list2[read2] == v2);
				}
			}

			if (read1 < list1.Count)
			{
				do
				{
					result.Add(list1[read1]);
					read1++;
				} while (read1 < list1.Count);
			}
			else
			{
				while (read2 < list2.Count)
				{
					result.Add(list2[read2]);
					read2++;
				}
			}

			return result;
		}

		static bool Overlapps(List<string> list1, IList<string> list2)
		{
			int read1 = 0;
			int read2 = 0;

			while (read1 < list1.Count && read2 < list2.Count)
			{
				string v1 = list1[read1];
				string v2 = list2[read2];

				int diff = string.CompareOrdinal(v1, v2);

				if (diff < 0)
				{
					read1++;
				}
				else if (diff > 0)
				{
					read2++;
				}
				else
				{
					return true;
				}
			}

			return false;
		}

		static bool IsSuperSetOf(List<string> list1, IList<string> list2)
		{
			int read1 = 0;
			int read2 = 0;

			while (read1 < list1.Count && read2 < list2.Count)
			{
				string v1 = list1[read1];
				string v2 = list2[read2];

				int diff = string.CompareOrdinal(v1, v2);

				if (diff < 0)
				{
					read1++;
				}
				else if (diff > 0)
				{
					return false;
				}
				else
				{
					read1++;
					read2++;
				}
			}

			return read2 == list2.Count;
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
		List<string> values;

		#endregion
	}
}
