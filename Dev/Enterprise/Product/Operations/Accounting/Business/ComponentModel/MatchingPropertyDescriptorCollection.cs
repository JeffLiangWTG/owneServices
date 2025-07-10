using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ComponentModel
{
	internal class MatchingPropertyDescriptorCollection : BusinessObjectPropertyDescriptorCollection
	{
		#region Factory Method

		protected MatchingPropertyDescriptorCollection(Type componentType, bool includePrivate)
			: base(componentType, includePrivate)
		{
		}

		public new static MatchingPropertyDescriptorCollection FromType(Type componentType)
		{ return FromType(componentType, false); }

		public new static MatchingPropertyDescriptorCollection FromType(Type componentType, bool includePrivate)
		{ return factory.FromType(componentType, includePrivate); }

		[ThreadStatic]
		static PropertyDescriptorCollectionFactory<MatchingPropertyDescriptorCollection> ffactory;

		static PropertyDescriptorCollectionFactory<MatchingPropertyDescriptorCollection> factory
		{
			get { return ffactory ?? (ffactory = new PropertyDescriptorCollectionFactory<MatchingPropertyDescriptorCollection>(componentType => new MatchingPropertyDescriptorCollection(componentType, false))); }
		}

		protected override KPropertyDescriptorCollection NewEmptyPropertyDescriptorCollectionCore(Type componentType, bool includePrivate)
		{
			return new MatchingPropertyDescriptorCollection(componentType, includePrivate);
		}

		#endregion

		protected override KPropertyDescriptorCollection GetPropertyDescriptorCollectionForComponentTypeCore(Type componentType, bool includePrivate)
		{
			return FromType(componentType, includePrivate);
		}

		protected override void PopulatePropertyDescriptorsCore()
		{
			base.PopulatePropertyDescriptorsCore();
			BusinessObjectPropertyDescriptorCollection collection = BusinessObjectPropertyDescriptorCollection.FromType(typeof(IMatching), IncludePrivate);
			foreach (PropertyDescriptor property in collection)
			{
				AddIfNotExists(property);
			}
		}
	}
}

