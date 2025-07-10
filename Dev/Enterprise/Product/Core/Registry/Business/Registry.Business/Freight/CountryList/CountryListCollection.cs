using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	public class CountryListCollection : NonPersistentBusinessObjectCollection<CountryListElement>
	{
		public CountryListCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void Load(Guid[] value)
		{
			foreach (Guid guid in value)
			{
				CountryListElement element = new CountryListElement(this);
				element.CountryPK = guid;
				Add(element);
			}
		}

		public Guid[] CountryPKs
		{
			get
			{
				List<Guid> result = new List<Guid>();
				foreach (CountryListElement element in this)
				{
					ZGuid countryPK = element.CountryPK;
					Guid guid = (countryPK.IsValid && !countryPK.IsEmpty) ? countryPK.ToGuid() : Guid.Empty;
					result.Add(guid);
				}
				return result.ToArray();
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CountryListElement(this);
		}

		public IBusinessObjectCollection CountryCollection
		{
			get
			{
				if (countryCollection == null)
				{
					countryCollection = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IRefCountryCollection>(), new object[] { Factory });
				}
				return countryCollection;
			}
		}

		IBusinessObjectCollection countryCollection;
	}
}
