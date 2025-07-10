//Auto generated using code template at 27/10/2003 12:29:11 PM
using System;
using System.Collections;

namespace Enterprise.DocumentEngine
{
	internal class FormulaProviderList : CollectionBase
	{
		public FormulaProviderList()
		{
		}

		public FormulaProviderList(FormulaProviderList value)
		{
			this.AddRange(value);
		}

		public FormulaProviderList(FormulaProvider[] value)
		{
			this.AddRange(value);
		}

		public FormulaProvider this[int index]
		{
			get
			{
				return ((FormulaProvider)(this.List[index]));
			}
			set
			{
				if (index >= List.Count)
				{
					throw new Exception("List index out of bounds.");
				}
				List[index] = value;
			}
		}

		public int Add(FormulaProvider value)
		{
			return this.List.Add(value);
		}

		public void AddRange(FormulaProvider[] value)
		{
			for (int i = 0; (i < value.Length); i = (i + 1))
			{
				this.Add(value[i]);
			}
		}

		public void AddRange(FormulaProviderList value)
		{
			for (int i = 0; (i < value.Count); i = (i + 1))
			{
				this.Add((FormulaProvider)value.List[i]);
			}
		}

		public bool Contains(FormulaProvider value)
		{
			return this.List.Contains(value);
		}

		public void CopyTo(FormulaProvider[] array, int index)
		{
			this.List.CopyTo(array, index);
		}

		public FormulaProvider[] ToArray()
		{
			FormulaProvider[] array = new FormulaProvider[this.Count];
			this.CopyTo(array, 0);

			return array;
		}

		public int IndexOf(FormulaProvider value)
		{
			return this.List.IndexOf(value);
		}

		public void Insert(int index, FormulaProvider value)
		{
			List.Insert(index, value);
		}

		public void Remove(FormulaProvider value)
		{
			List.Remove(value);
		}

		public new FormulaProviderListEnumerator GetEnumerator()
		{
			return new FormulaProviderListEnumerator(this);
		}

		public class FormulaProviderListEnumerator : IEnumerator
		{
			readonly IEnumerator _enumerator;
			readonly IEnumerable _temp;

			public FormulaProviderListEnumerator(FormulaProviderList mappings)
			{
				_temp = mappings;
				_enumerator = _temp.GetEnumerator();
			}

			public object Current
			{
				get { return ((FormulaProvider)(_enumerator.Current)); }
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
