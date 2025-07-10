using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class ConsolCostListWithStrategy : BusinessObjectListWithContext<JobConsolCost, ConsolCostStrategy>
	{
		public ConsolCostListWithStrategy(ConsolCostStrategy strategy)
			: base(strategy)
		{
		}
	}

	public class BusinessObjectListWithContext<BizoType, ContextType> : IList<BizoType>
		where BizoType : BusinessObject
		where ContextType : struct, IComparable, IConvertible
	{
		public BusinessObjectListWithContext(ContextType contextForeachBizo)
		{
			context = contextForeachBizo;
		}

		public void ForEach(Action<BizoType> action)
		{
			list.ForEach(action);
		}

		public BizoType Find(Predicate<BizoType> match)
		{
			return list.Find(match);
		}

		public int IndexOf(BizoType item)
		{
			return list.IndexOf(item);
		}

		public void Insert(int index, BizoType item)
		{
			OnAdd(item);
			list.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			OnRemove(this[index]);
			list.RemoveAt(index);
		}

		public BizoType this[int index]
		{
			get { return list[index]; }
			set { list[index] = value; }
		}

		public void Add(BizoType item)
		{
			OnAdd(item);
			list.Add(item);
		}

		public void AddRange(IEnumerable<BizoType> collection)
		{
			foreach (var item in collection)
			{
				OnAdd(item);
			}
			list.AddRange(collection);
		}

		public void Clear()
		{
			this.ForEach(x => OnRemove(x));
			list.Clear();
		}

		public bool Contains(BizoType item)
		{
			return list.Contains(item);
		}

		public void CopyTo(BizoType[] array, int arrayIndex)
		{
			list.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return list.Count; }
		}

		public bool IsReadOnly
		{
			get { return ((ICollection<BizoType>)list).IsReadOnly; }
		}

		public bool Remove(BizoType item)
		{
			OnRemove(item);
			return list.Remove(item);
		}

		public IEnumerator<BizoType> GetEnumerator()
		{
			return list.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator();
		}

		void OnAdd(BizoType item)
		{
			item.SetContext(context);
		}

		void OnRemove(BizoType item)
		{
			item.RemoveContext(context);
		}

		readonly List<BizoType> list = new List<BizoType>();
		readonly ContextType context;
	}
}
