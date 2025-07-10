using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.Integration;

namespace CargoWise.EntityFramework
{
	public class BusinessObjectFactoryChildCollection : ICollection
	{
		readonly BusinessObjectFactory parent;

		public BusinessObjectFactoryChildCollection(BusinessObjectFactory parent)
		{
			Argument.NotNull(parent, nameof(parent));
			this.parent = parent;
		}

		public void Add(BusinessObjectFactory factory)
		{
			if (!Factories.Contains(factory))
			{
				factory.Parent = parent;
				Factories.Add(factory);
			}
		}

		public void Remove(BusinessObjectFactory factory)
		{
			Factories.Remove(factory);
			factory.Parent = null;
		}

		public void Clear()
		{
			Factories.Clear();
		}

		public bool Contains(BusinessObjectFactory factory)
		{
			return Factories.Contains(factory);
		}

		public BusinessObjectFactory this[int index]
		{
			get { return (BusinessObjectFactory)Factories[index]; }
		}

		public ITransactionParticipant[] ToArray()
		{
			return (ITransactionParticipant[])Factories.ToArray(typeof(ITransactionParticipant));
		}

		#region Factories

		internal ArrayList Factories
		{
			get
			{
				if (factories == null)
				{
					factories = new ArrayList();
				}
				return factories;
			}
		}

		ArrayList factories;

		#endregion

		#region ICollection Members

		bool ICollection.IsSynchronized
		{
			get { return Factories.IsSynchronized; }
		}

		public int Count
		{
			get { return Factories.Count; }
		}

		void ICollection.CopyTo(Array array, int index)
		{
			Factories.CopyTo(array, index);
		}

		object ICollection.SyncRoot
		{
			get { return Factories.SyncRoot; }
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Factories.GetEnumerator();
		}

		#endregion
	}
}
