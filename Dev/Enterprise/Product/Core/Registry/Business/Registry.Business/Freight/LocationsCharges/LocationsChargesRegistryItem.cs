using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class LocationsChargesRegistryItem : StronglyTypedRegistryItem<LocationsChargesCollection>
	{
		public LocationsChargesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, LocationsChargesCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new LocationsChargesRegistryDataType(), storage, defaultValue))
		{
		}

		public LocationsChargesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, LocationsChargesCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new LocationsChargesRegistryDataType(), storage, options, defaultValue))
		{
		}

		public Guid[] GetAsGuidArray(ZString locationCode)
		{
			List<Guid> result = new List<Guid>();
			if (Value[locationCode] != null)
			{
				foreach (ChargeCodeGroup element in Value[locationCode].Charges)
				{
					result.Add(element.ChargeCodePK.ToGuid());
				}
			}
			return result.ToArray();
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.LocationsChargesRegistryItemEditor, Enterprise.Registry.GUI")]
	class LocationsChargesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<LocationsChargesCollection>
	{
		public LocationsChargesRegistryDataType()
		{
		}

		protected override LocationsChargesCollection CloneValue(LocationsChargesCollection value)
		{
			return (LocationsChargesCollection)value.Clone(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), null);
		}
	}
}
