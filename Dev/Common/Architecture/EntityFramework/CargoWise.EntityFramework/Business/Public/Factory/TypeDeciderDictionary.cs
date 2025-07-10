using System;
using System.Collections.Generic;
using CargoWise.Integration;

namespace CargoWise.EntityFramework
{
	public class TypeDeciderDictionary : ITypeDeciderDictionary
	{
		protected TypeDeciderDictionary()
		{
			inner = new Dictionary<Type, ITypeDecider>();
		}

		public TypeDeciderDictionary(IDictionary<Type, ITypeDecider> dictionary)
			: this()
		{
			foreach (KeyValuePair<Type, ITypeDecider> entry in dictionary)
			{
				inner.Add(entry.Key, entry.Value);
			}
		}

		public static TypeDeciderDictionary Empty
		{
			get { return empty ?? (empty = empty = new TypeDeciderDictionary()); }
		}
		[ThreadStatic]
		static TypeDeciderDictionary empty;

		public ITypeDecider this[Type businessObjectType]
		{
			get
			{
				ITypeDecider result;
				inner.TryGetValue(businessObjectType, out result);
				return result;
			}
		}

		public int Count
		{
			get { return inner.Count; }
		}

		public bool ContainsKey(Type type)
		{
			return inner.ContainsKey(type);
		}

		readonly Dictionary<Type, ITypeDecider> inner;

		#region IEnumerable Members

		IEnumerator<KeyValuePair<Type, ITypeDecider>> IEnumerable<KeyValuePair<Type, ITypeDecider>>.GetEnumerator()
		{
			return GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator<KeyValuePair<Type, ITypeDecider>> GetEnumerator()
		{
			return inner.GetEnumerator();
		}

		#endregion
	}
}
