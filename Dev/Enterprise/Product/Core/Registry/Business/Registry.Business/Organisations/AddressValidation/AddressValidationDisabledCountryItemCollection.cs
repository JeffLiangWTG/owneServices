using System;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AddressValidationDisabledCountryItemCollection : RegistryBusinessObjectCollectionTemplate
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AddressValidationDisabledCountryItem();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AddressValidationDisabledCountryItemCollection();
		}

		public new AddressValidationDisabledCountryItem AddNew()
		{
			return (AddressValidationDisabledCountryItem)base.AddNew();
		}

		public new AddressValidationDisabledCountryItem this[int i] => (AddressValidationDisabledCountryItem)Elements[i];

		public IBusinessObjectCollection CountryCollection
		{
			get
			{
				if (countryCollection == null)
				{
					countryCollection = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<MasterFiles.Integration.IRefCountryCollection>(), CurrentFactory);
				}

				return countryCollection;
			}
		}

		IBusinessObjectCollection countryCollection;
	}
}
