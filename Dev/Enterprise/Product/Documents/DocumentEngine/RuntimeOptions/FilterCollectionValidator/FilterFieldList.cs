using System.Collections;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class FilterFieldList : CollectionBase
	{
		public FilterFieldList()
		{
		}

		public FilterFieldList(FilterFieldList value)
		{
			AddRange(value);
		}

		public FilterFieldList(FilterField[] value)
		{
			AddRange(value);
		}

		public FilterField this[int index]
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return (FilterField)List[index]; }
			[System.Diagnostics.DebuggerStepThrough()]
			set
			{
				List[index] = value;
			}
		}

		public int Add(FilterField value)
		{
			return List.Add(value);
		}

		public void AddRange(FilterField[] value)
		{
			for (int i = 0; (i < value.Length); i = (i + 1))
			{
				Add(value[i]);
			}
		}

		public void AddRange(FilterFieldList value)
		{
			for (int i = 0; (i < value.Count); i = (i + 1))
			{
				Add((FilterField)value.List[i]);
			}
		}

		public bool Contains(FilterField value)
		{
			return List.Contains(value);
		}

		public void CopyTo(FilterField[] array, int index)
		{
			List.CopyTo(array, index);
		}

		public FilterField[] ToArray()
		{
			FilterField[] array = new FilterField[Count];
			CopyTo(array, 0);

			return array;
		}

		public int IndexOf(FilterField value)
		{
			return List.IndexOf(value);
		}

		public void Insert(int index, FilterField value)
		{
			List.Insert(index, value);
		}

		public void Remove(FilterField value)
		{
			List.Remove(value);
		}

		[System.Diagnostics.DebuggerStepThrough()]
		public new FilterFieldListEnumerator GetEnumerator()
		{
			return new FilterFieldListEnumerator(this);
		}

		public class FilterFieldListEnumerator : IEnumerator
		{
			readonly IEnumerator _enumerator;
			readonly IEnumerable _temp;

			public FilterFieldListEnumerator(FilterFieldList mappings)
			{
				_temp = mappings;
				_enumerator = _temp.GetEnumerator();
			}

			public object Current
			{
				get { return ((FilterField)(_enumerator.Current)); }
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
}
