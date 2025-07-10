using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Data.HttpClient
{
	public class HttpParameterCollection : DbParameterCollection
	{
		readonly List<DbParameter> items = [];

		//Suppressed Message since List.Add already ensures the contract
		[SuppressMessage("Microsoft.Contracts", "Ensures-this.IsSynchronized || Contract.Result<int>() < 0 && this.Count == Contract.OldValue(this.Count) || this.Count == Contract.OldValue(this.Count) + 1")]
		public override int Add(object value)
		{
			items.Add((DbParameter)value);
			var result = Count - 1;
			return result;
		}

		public override void AddRange(Array values)
		{
			foreach (var parameter in values)
			{
				Add(parameter);
			}
		}

		public override void Clear()
		{
			items.Clear();
		}

		public override bool Contains(string value)
		{
			return (IndexOf(value) != -1);
		}

		public override bool Contains(object value)
		{
			return (IndexOf(value) != -1);
		}

		public override void CopyTo(Array array, int index)
		{
			throw new NotSupportedException();
		}

		public override int Count
		{
			get
			{
				return items.Count;
			}
		}

		public override IEnumerator GetEnumerator()
		{
			return items.GetEnumerator();
		}

		protected override DbParameter GetParameter(string parameterName)
		{
			int index = IndexOf(parameterName);
			return GetParameter(index);
		}

		protected override DbParameter GetParameter(int index)
		{
			if (index < 0 || items.Count <= index)
			{
				throw new IndexOutOfRangeException($"Index ({index.ToString()}) is out of range of the collection.");
			}

			return items[index];
		}

		public override int IndexOf(string parameterName)
		{
			if (parameterName != null)
			{
				int count = items.Count;

				for (int i = 0; i < count; i++)
				{
					if (items[i].ParameterName == parameterName)
					{
						return i;
					}
				}
			}

			return -1;
		}

		public override int IndexOf(object value)
		{
			if (value != null)
			{
				int count = items.Count;
				for (int i = 0; i < count; i++)
				{
					if (value == items[i])
					{
						return i;
					}
				}
			}

			return -1;
		}

		public override void Insert(int index, object value)
		{
			throw new NotSupportedException();
		}

		public override bool IsFixedSize
		{
			get
			{
				return ((IList)items).IsFixedSize;
			}
		}

		public override bool IsReadOnly
		{
			get
			{
				return ((IList)items).IsReadOnly;
			}
		}

		public override bool IsSynchronized
		{
			get
			{
				return ((IList)items).IsSynchronized;
			}
		}

		public override void Remove(object value)
		{
			throw new NotSupportedException();
		}

		public override void RemoveAt(string parameterName)
		{
			throw new NotSupportedException();
		}

		public override void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		protected override void SetParameter(string parameterName, DbParameter value)
		{
			throw new NotSupportedException();
		}

		protected override void SetParameter(int index, DbParameter value)
		{
			throw new NotSupportedException();
		}

		public override object SyncRoot
		{
			get
			{
				return ((ICollection)items).SyncRoot;
			}
		}
	}
}
