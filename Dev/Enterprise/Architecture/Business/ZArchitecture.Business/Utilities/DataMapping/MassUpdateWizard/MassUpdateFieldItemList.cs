using System;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class MassUpdateFieldItem : ICodeDescription
	{
		public MassUpdateFieldItem(IImportPropertyInfo propertyInfo)
		{
			this.propertyInfo = propertyInfo;
		}

		public readonly IImportPropertyInfo propertyInfo;

		#region ICodeDescription Members

		string ICodeDescription.Code
		{
			get { return propertyInfo.HeaderText.TrimEnd() + " (" + propertyInfo.MappingName + ")"; }
		}

		string ICodeDescription.Description
		{
			get { return propertyInfo.HeaderText; }
		}

		object ICodeDescription.PK
		{
			get { return null; }
		}

		#endregion
	}

	public class MassUpdateFieldItemList : ICodeDescriptionPairList
	{
		public MassUpdateFieldItemList()
		{
			list = new CodeDescriptionPairList();
		}

		public void Add(IImportPropertyInfo property)
		{
			list.Add(new MassUpdateFieldItem(property));
		}

		public MassUpdateFieldItem this[string fieldName]
		{
			get { return (MassUpdateFieldItem)list[fieldName, StringComparison.OrdinalIgnoreCase]; }
		}

		readonly CodeDescriptionPairList list;

		public int Count
		{
			get { return list.Count; }
		}

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

		int System.Collections.IList.Add(object value)
		{
			return list.Add((ICodeDescription)value);
		}

		void System.Collections.IList.Clear()
		{
			list.Clear();
		}

		bool System.Collections.IList.Contains(object value)
		{
			return list.Contains((ICodeDescription)value);
		}

		int System.Collections.IList.IndexOf(object value)
		{
			return list.IndexOf((ICodeDescription)value);
		}

		void System.Collections.IList.Insert(int index, object value)
		{
			list.Insert(index, value);
		}

		bool System.Collections.IList.IsFixedSize
		{
			get { return list.IsFixedSize; }
		}

		bool System.Collections.IList.IsReadOnly
		{
			get { return list.IsReadOnly; }
		}

		void System.Collections.IList.Remove(object value)
		{
			list.Remove((ICodeDescription)value);
		}

		void System.Collections.IList.RemoveAt(int index)
		{
			list.RemoveAt(index);
		}

		object System.Collections.IList.this[int index]
		{
			get { return list[index]; }
			set { list[index] = (ICodeDescription)value; }
		}

		#endregion

		#region ICollection Members

		void System.Collections.ICollection.CopyTo(Array array, int index)
		{
			list.CopyTo(array, index);
		}

		int System.Collections.ICollection.Count
		{
			get { return list.Count; }
		}

		bool System.Collections.ICollection.IsSynchronized
		{
			get { return list.IsSynchronized; }
		}

		object System.Collections.ICollection.SyncRoot
		{
			get { return list.SyncRoot; }
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator();
		}

		#endregion
	}
}
