using System;
using System.Collections;
using System.Data;

namespace CargoWise.EntityFramework
{
	#region ZDataRowDictionary

	public class ZDataRowDictionary : DictionaryBase
	{
		public void Add(object key, DataRow row)
		{
			Dictionary.Add(key, row);
		}

		public DataRow this[object key]
		{
			get { return (DataRow)Dictionary[key]; }
		}

		#region IEnumerable Members

		public ICollection Values
		{
			get { return Dictionary.Values; }
		}

		#endregion
	}

	#endregion

	#region ZDataRowCollection

	public class ZDataRowCollection : CollectionBase
	{
		public ZDataRowCollection()
		{
		}

		public ZDataRowCollection(ZDataRowCollection value)
		{
			this.AddRange(value);
		}

		public ZDataRowCollection(DataRow[] value)
		{
			this.AddRange(value);
		}

		public ZDataRowCollection(DataRowCollection value)
		{
			this.AddRange(value);
		}

		public DataRow this[int index]
		{
			get { return ((DataRow)(this.List[index])); }
			set
			{
				if (index >= List.Count)
				{
					throw new Exception("List index out of bounds.");
				}
				List[index] = value;
			}
		}

		public int Add(DataRow value)
		{
			return this.List.Add(value);
		}

		public void AddRange(DataRow[] collectionToAdd)
		{
			AddRangeWithNoTypeCheck(collectionToAdd);
		}

		public void AddRange(DataRowCollection collectionToAdd)
		{
			for (int i = 0; i < collectionToAdd.Count; i++)
			{
				this.Add(collectionToAdd[i]);
			}
		}

		public void AddRange(ZDataRowCollection collectionToAdd)
		{
			AddRangeWithNoTypeCheck(collectionToAdd);
		}

		protected void AddRangeWithNoTypeCheck(IList collectionToAdd)
		{
			for (int i = 0; i < collectionToAdd.Count; i++)
			{
				this.Add((DataRow)collectionToAdd[i]);
			}
		}

		public bool Contains(DataRow value)
		{
			return this.List.Contains(value);
		}

		public void CopyTo(DataRow[] array, int index)
		{
			this.List.CopyTo(array, index);
		}

		public DataRow[] ToArray()
		{
			DataRow[] array = new DataRow[this.Count];
			this.CopyTo(array, 0);

			return array;
		}

		public int IndexOf(DataRow value)
		{
			return this.List.IndexOf(value);
		}

		public void Insert(int index, DataRow value)
		{
			List.Insert(index, value);
		}

		public void Remove(DataRow value)
		{
			List.Remove(value);
		}

		public new ZDataRowCollectionEnumerator GetEnumerator()
		{
			return new ZDataRowCollectionEnumerator(this);
		}

		public class ZDataRowCollectionEnumerator : IEnumerator
		{
			readonly IEnumerator _enumerator;
			readonly IEnumerable _temp;

			public ZDataRowCollectionEnumerator(ZDataRowCollection mappings)
			{
				_temp = mappings;
				_enumerator = _temp.GetEnumerator();
			}

			public object Current
			{
				get { return ((DataRow)(_enumerator.Current)); }
			}

			public bool MoveNext()
			{
				return _enumerator.MoveNext();
			}

			public void Reset()
			{
				_enumerator.Reset();
			}
		}
	}

	#endregion
}
