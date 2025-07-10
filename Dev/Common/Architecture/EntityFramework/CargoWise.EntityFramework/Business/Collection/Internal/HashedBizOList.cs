using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Common.Collections;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class HashedBizOList : IEnumerable, IEnumerable<BusinessObject>, ISortable
	{
		public HashedBizOList()
		{
		}

		public BusinessObject this[int index]
		{
			get
			{
				return list != null ? list[index] : null;
			}
			set
			{
				if (hashByPK != null)
				{
					hashByPK.Remove(list[index].PK);
				}
				EnsureList();
				list[index] = value;
				if (hashByPK != null)
				{
					hashByPK[value.PK] = index;
				}
			}
		}

		#region Implementation

		void EnsureList()
		{
			if (list == null)
			{
				list = new List<BusinessObject>(1);
			}
		}

		List<BusinessObject> list;
		Dictionary<ZGuid, int> hashByPK;

		internal bool HasHash => hashByPK != null;

		public BusinessObject[] ToArray()
		{
			return list != null ? list.ToArray() : Array.Empty<BusinessObject>();
		}

		internal Array ToArray(Type arrayType)
		{
			return list != null ? new ArrayList(list).ToArray(arrayType) : new ArrayList().ToArray(arrayType);
		}

		internal void CopyTo(Array array, int index)
		{
			if (list != null)
			{
				((ICollection)list).CopyTo(array, index);
			}
		}

		internal void Insert(int index, BusinessObject value)
		{
			if (ReferenceEquals(value, null))
			{
				throw new ArgumentNullException(nameof(value));
			}
			hashByPK = null;

			EnsureList();
			list.Insert(index, value);
		}

		internal int Add(BusinessObject element)
		{
			if (ReferenceEquals(element, null))
			{
				throw new ArgumentNullException(nameof(element));
			}

			EnsureList();
			list.Add(element);
			var position = list.Count - 1;
			if (hashByPK != null)
			{
				hashByPK[element.PK] = position;
			}
			return position;
		}

		internal void Remove(BusinessObject element)
		{
			hashByPK = null;
			if (list != null)
			{
				list.Remove(element);
				if (list.Count == 0)
				{
					list = null;
				}
			}
		}

		internal bool Contains(BusinessObject element)
		{
			bool result = false;

			if (element != null && list != null)
			{
				EnsureHashByPK();
				result = hashByPK.ContainsKey(element.PK);
				if (!result && element.Row == null)
				{
					result = list.Contains(element);
				}
			}

			return result;
		}

		internal BusinessObject GetByPK(ZGuid pk)
		{
			if (list != null)
			{
				EnsureHashByPK();
				int position = -1;
				if (hashByPK.TryGetValue(pk, out position))
				{
					return list[position];
				}
			}
			return null;
		}

		internal void Clear()
		{
			hashByPK = null;
			if (list != null)
			{
				list.Clear();
			}
		}

		public int IndexOfOptimisedForHashByPK(BusinessObject bizObj)
		{
			int result = -1;
			if (hashByPK != null)
			{
				result = IndexOf(bizObj);
			}
			else if (bizObj != null && list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] == bizObj)
					{
						result = i;
						break;
					}
				}
			}
			return result;
		}

		public int IndexOf(BusinessObject bizObj)
		{
			int result = -1;
			if (bizObj != null)
			{
				EnsureHashByPK();
				int tempValue;
				if (hashByPK.TryGetValue(bizObj.PK, out tempValue))
				{
					result = tempValue;
				}
			}
			return result;
		}

		public int IndexOf(BusinessObject bizObj, int startIndex, int countToSearchFromStartIndex)
		{
			if (list != null)
			{
				int maxCount = Count - startIndex;
				if (countToSearchFromStartIndex > maxCount)
				{
					countToSearchFromStartIndex = maxCount;
				}

				return list.IndexOf(bizObj, startIndex, countToSearchFromStartIndex);
			}
			return -1;
		}

		internal int Count
		{
			get { return list != null ? list.Count : 0; }
		}

		internal bool ApplySort<T>(IComparer<T> comparer) where T : BusinessObject
		{
			if (list != null)
			{
				var result = list.StableSortWithReorderCheck(new TypedBusinessObjectComparer<T>(comparer));
				if (result)
				{
					hashByPK = null;
				}
				return result;
			}
			else
			{
				hashByPK = null;
				return true;
			}
		}

		internal bool ApplySort(IComparer comparer)
		{
			if (list != null)
			{
				var result = list.StableSortWithReorderCheck(new GeneralBusinessObjectComparer(comparer));
				if (result)
				{
					hashByPK = null;
				}
				return result;
			}
			else
			{
				hashByPK = null;
				return true;
			}
		}

		void ISortable.ApplySort(IComparer comparer)
		{
			ApplySort(comparer);
		}

		#region IEnumerable Members

		IEnumerator<BusinessObject> IEnumerable<BusinessObject>.GetEnumerator()
		{
			return list != null ? list.GetEnumerator() : new List<BusinessObject>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return list != null ? list.GetEnumerator() : new List<BusinessObject>().GetEnumerator();
		}

		#endregion

		void EnsureHashByPK()
		{
			if (hashByPK == null)
			{
				hashByPK = new Dictionary<ZGuid, int>(list != null ? list.Count : 0);
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						hashByPK[list[i].PK] = i;
					}
				}
			}
		}

		#endregion

		#region Comparers

#if DEBUG
		internal
#endif
		class GeneralBusinessObjectComparer : IComparer<BusinessObject>
		{
			public GeneralBusinessObjectComparer(IComparer baseComparer)
			{
				this.baseComparer = baseComparer ?? Comparer<BusinessObject>.Default;
			}

			readonly IComparer baseComparer;

			public int Compare(BusinessObject x, BusinessObject y)
			{
				var xIsDeletedOrNull = ReferenceEquals(x, null) || x.IsDeleted;
				var yIsDeletedOrNull = ReferenceEquals(y, null) || y.IsDeleted;

				if (xIsDeletedOrNull && yIsDeletedOrNull)
				{
					return 0;
				}
				if (xIsDeletedOrNull)
				{
					return -1;
				}
				if (yIsDeletedOrNull)
				{
					return 1;
				}

				return baseComparer.Compare(x, y);
			}
		}

		class TypedBusinessObjectComparer<T> : IComparer<BusinessObject> where T : BusinessObject
		{
			public TypedBusinessObjectComparer(IComparer<T> baseComparer)
			{
				this.baseComparer = baseComparer ?? Comparer<T>.Default;
			}

			readonly IComparer<T> baseComparer;

			public int Compare(BusinessObject x, BusinessObject y)
			{
				var xIsDeletedOrNull = ReferenceEquals(x, null) || x.IsDeleted;
				var yIsDeletedOrNull = ReferenceEquals(y, null) || y.IsDeleted;

				if (xIsDeletedOrNull && yIsDeletedOrNull)
				{
					return 0;
				}
				if (xIsDeletedOrNull)
				{
					return -1;
				}
				if (yIsDeletedOrNull)
				{
					return 1;
				}

				return baseComparer.Compare((T)x, (T)y);
			}
		}

		#endregion
	}
}
