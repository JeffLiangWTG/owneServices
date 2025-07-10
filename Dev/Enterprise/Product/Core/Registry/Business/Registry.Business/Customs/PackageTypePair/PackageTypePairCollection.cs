using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Registry.Business.Customs
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public abstract class PackageTypePairCollection<T> : RegistryBusinessObjectCollectionTemplate, IBindingListView
		where T : PackageTypePair
	{
		public new T this[int index]
		{
			get { return (T)Elements[index]; }
		}

		public new T AddNew()
		{
			return (T)base.AddNew();
		}

		public T AddNew(ZString customsPackageType, ZString freightPackageType)
		{
			T result = AddNew();
			result.CustomsPackageType = customsPackageType;
			result.FreightPackageType = freightPackageType;
			return result;
		}

		public void AddDefaultValues()
		{
			AddDefaultValuesCore();
		}

		protected virtual void AddDefaultValuesCore()
		{
		}

		public ZString GetMappedPackageType(ZString freightPackageType)
		{
			ZString result = freightPackageType;
			foreach (PackageTypePair pair in ToArray())
			{
				if (pair.FreightPackageType == freightPackageType)
				{
					result = pair.CustomsPackageType;
					break;
				}
			}
			return result;
		}

		public ZString GetMappedFreightPackageType(ZString customsPackageType)
		{
			return GetMappedFreightPackagePair(customsPackageType) is PackageTypePair packageTypePair ? packageTypePair.FreightPackageType : customsPackageType;
		}

		public PackageTypePair GetMappedFreightPackagePair(ZString customsPackageType)
		{
			return this.Cast<PackageTypePair>().FirstOrDefault(pair => pair.CustomsPackageType == customsPackageType);
		}

		ListSortDescriptionCollection IBindingListView.SortDescriptions
		{
			get { return new ListSortDescriptionCollection(DefaultSortDirections); }
		}

		ListSortDescription[] DefaultSortDirections
		{
			get
			{
				if (defaultSortDirections == null)
				{
					defaultSortDirections = new[] { new ListSortDescription(GetProperties(TypeOfElements)[PackageTypePair.Schema.CustomsPackageTypeDescription], ListSortDirection.Descending) };
				}
				return defaultSortDirections;
			}
		}
		ListSortDescription[] defaultSortDirections;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return (T)Activator.CreateInstance(typeof(T));
		}
	}
}
