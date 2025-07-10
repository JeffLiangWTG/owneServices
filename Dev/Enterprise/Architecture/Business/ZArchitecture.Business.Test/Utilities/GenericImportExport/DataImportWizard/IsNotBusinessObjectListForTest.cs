using System;
using System.Collections;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class IsNotBusinessObjectListForTest : ICodeDescriptionPairList
	{
		public IsNotBusinessObjectListForTest()
		{
			list = new CodeDescriptionPairList();
		}

		public void Add(string code)
		{
			list.Add(new IsNotBusinessObjectForTest(code));
		}

		readonly CodeDescriptionPairList list;

		#region ICodeDescriptionPairList Members

		bool ICodeDescriptionPairList.ContainsCode(object code)
		{
			return list.ContainsCode(code);
		}

		string ICodeDescriptionPairList.GetDescriptionFromCode(string code)
		{
			return list.GetDescriptionFromCode(code);
		}

		#endregion

		#region IList Members

		int IList.Add(object value)
		{
			return list.Add((ICodeDescription)value);
		}

		void IList.Clear()
		{
			list.Clear();
		}

		bool IList.Contains(object value)
		{
			return list.Contains((ICodeDescription)value);
		}

		int IList.IndexOf(object value)
		{
			return list.IndexOf((ICodeDescription)value);
		}

		void IList.Insert(int index, object value)
		{
			list.Insert(index, value);
		}

		bool IList.IsFixedSize => list.IsFixedSize;

		bool IList.IsReadOnly => list.IsReadOnly;

		void IList.Remove(object value)
		{
			list.Remove((ICodeDescription)value);
		}

		void IList.RemoveAt(int index)
		{
			list.RemoveAt(index);
		}

		object IList.this[int index]
		{
			get => list[index];
			set => list[index] = (ICodeDescription)value;
		}

		#endregion

		#region ICollection Members

		void ICollection.CopyTo(Array array, int index)
		{
			list.CopyTo(array, index);
		}

		int ICollection.Count => list.Count;

		bool ICollection.IsSynchronized => list.IsSynchronized;

		object ICollection.SyncRoot => list.SyncRoot;

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator();
		}

		#endregion
	}
}
