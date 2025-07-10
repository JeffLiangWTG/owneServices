using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.Base.Transaction;
using BusinessObjectPropertyDescriptorCollection = CargoWise.EntityFramework.BusinessObjectPropertyDescriptorCollection;

namespace Enterprise.Accounting.Business.ComponentModel
{
	internal class LineMatchingPropertyDescriptorCollection : BusinessObjectPropertyDescriptorCollection
	{
		#region Factory Method

		protected LineMatchingPropertyDescriptorCollection(Type componentType, bool includePrivate)
			: base(componentType, includePrivate)
		{
		}

		public new static LineMatchingPropertyDescriptorCollection FromType(Type componentType)
		{ return FromType(componentType, false); }

		public new static LineMatchingPropertyDescriptorCollection FromType(Type componentType, bool includePrivate)
		{ return factory.FromType(componentType, includePrivate); }

		[ThreadStatic]
		static PropertyDescriptorCollectionFactory<LineMatchingPropertyDescriptorCollection> ffactory;

		static PropertyDescriptorCollectionFactory<LineMatchingPropertyDescriptorCollection> factory
		{
			get { return ffactory ?? (ffactory = new PropertyDescriptorCollectionFactory<LineMatchingPropertyDescriptorCollection>(componentType => new LineMatchingPropertyDescriptorCollection(componentType, false))); }
		}

		protected override KPropertyDescriptorCollection NewEmptyPropertyDescriptorCollectionCore(Type componentType, bool includePrivate)
		{
			return new LineMatchingPropertyDescriptorCollection(componentType, includePrivate);
		}

		#endregion

		protected override KPropertyDescriptorCollection GetPropertyDescriptorCollectionForComponentTypeCore(Type componentType, bool includePrivate)
		{
			return FromType(componentType, includePrivate);
		}

		protected override void PopulatePropertyDescriptorsCore()
		{
			base.PopulatePropertyDescriptorsCore();
			BusinessObjectPropertyDescriptorCollection collection = BusinessObjectPropertyDescriptorCollection.FromType(typeof(ILineMatching), IncludePrivate);
			foreach (PropertyDescriptor property in collection)
			{
				AddIfNotExists(property);
			}
		}
	}
}

