using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Interfaces;

namespace Enterprise.Accounting.Business.ComponentModel
{
	internal class TransactionPropertyDescriptorCollection : BusinessObjectPropertyDescriptorCollection
	{
		#region Factory Method

		protected TransactionPropertyDescriptorCollection(Type componentType, bool includePrivate)
			: base(componentType, includePrivate)
		{
		}

		public new static TransactionPropertyDescriptorCollection FromType(Type componentType)
		{ return FromType(componentType, false); }

		public new static TransactionPropertyDescriptorCollection FromType(Type componentType, bool includePrivate)
		{ return factory.FromType(componentType, includePrivate); }

		[ThreadStatic]
		static PropertyDescriptorCollectionFactory<TransactionPropertyDescriptorCollection> ffactory;

		static PropertyDescriptorCollectionFactory<TransactionPropertyDescriptorCollection> factory
		{
			get { return ffactory ?? (ffactory = new PropertyDescriptorCollectionFactory<TransactionPropertyDescriptorCollection>(componentType => new TransactionPropertyDescriptorCollection(componentType, false))); }
		}

		protected override KPropertyDescriptorCollection NewEmptyPropertyDescriptorCollectionCore(Type componentType, bool includePrivate)
		{
			return new TransactionPropertyDescriptorCollection(componentType, includePrivate);
		}

		#endregion

		protected override KPropertyDescriptorCollection GetPropertyDescriptorCollectionForComponentTypeCore(Type componentType, bool includePrivate)
		{
			return FromType(componentType, includePrivate);
		}

		protected override void PopulatePropertyDescriptorsCore()
		{
			base.PopulatePropertyDescriptorsCore();
			BusinessObjectPropertyDescriptorCollection collection = BusinessObjectPropertyDescriptorCollection.FromType(typeof(ITransaction), IncludePrivate);
			foreach (PropertyDescriptor property in collection)
			{
				AddIfNotExists(property);
			}
		}
	}
}

