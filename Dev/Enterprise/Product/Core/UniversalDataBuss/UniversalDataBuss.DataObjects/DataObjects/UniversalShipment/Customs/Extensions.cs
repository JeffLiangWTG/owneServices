using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	public static class Extensions
	{
		public static IWarehouseCustomsDetailsChangeOfOwnership GetWarehouseCustomsDetailsChangeOfOwnership(this Shipment shipment, IDataContextDataObject context)
		{
			var types = ObjectFactory.Get<Hashtable>("WarehouseCustomsDetailsChangeOfOwnerships");
			var objectHandle = (ObjectHandle)types[context.CountryCodeToImportInto.ToString()] ?? (ObjectHandle)types["Shared"];
			return objectHandle != null ? (IWarehouseCustomsDetailsChangeOfOwnership)objectHandle.GetObject(shipment) : null;
		}

		public static IEnumerable<IWarehouseCustomsLineDetails> GetWarehouseCustomsLineDetails(this Shipment shipment, IDataContextDataObject context)
		{
			var result = Enumerable.Empty<IWarehouseCustomsLineDetails>();

			Hashtable types = null;

			if (context.DataSourceCollection?.Any(dataSource => dataSource?.Type?.EqualsIgnoringCase(nameof(DataContextType.NctsHeader)) ?? false) ?? false)
			{
				types = ObjectFactory.Get<Hashtable>("WarehouseNctsCustomsLineDetailsProviders");
			}

			types ??= ObjectFactory.Get<Hashtable>("WarehouseCustomsLineDetailsProviders");

			var countryCode = context.CountryCodeToImportInto.ToString();
			var objectHandle = (ObjectHandle)types[countryCode];
			if (objectHandle == null && ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnion(countryCode))
			{
				objectHandle = (ObjectHandle)types["EUN"];
			}
			if (objectHandle == null && ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry(countryCode))
			{
				objectHandle = (ObjectHandle)types["AsycudaCustoms"];
			}
			if (objectHandle == null)
			{
				objectHandle = (ObjectHandle)types["Shared"];
			}
			var provider = objectHandle != null ? (IWarehouseCustomsLineDetailsProvider)objectHandle.GetObject(shipment) : null;
			if (provider != null)
			{
				result = provider.GetLineDetails();
			}
			return result;
		}

		public static WarehouseType GetWarehouseType(this Shipment shipment)
		{
			var result = WarehouseType.Default;
			if (shipment != null && shipment.DataContext != null)
			{
				var types = ObjectFactory.Get<Hashtable>("WarehouseTypeProviders");
				var objectHandle = (ObjectHandle)types[shipment.DataContext.CountryCodeToImportInto.ToString()] ?? (ObjectHandle)types["Shared"];
				var provider = objectHandle != null ? (IWarehouseTypeProvider)objectHandle.GetObject(shipment) : null;
				if (provider != null)
				{
					result = provider.GetWarehouseType();
				}
			}
			return result;
		}

		public static CustomsRegime GetWarehouseRegimeType(this Shipment shipment)
		{
			var result = CustomsRegime.BondedWarehouse;
			if (shipment != null && shipment.DataContext != null)
			{
				var types = ObjectFactory.Get<Hashtable>("WarehouseRegimeTypeProviders");
				var objectHandle = (ObjectHandle)types[shipment.DataContext.CountryCodeToImportInto.ToString()] ?? (ObjectHandle)types["Shared"];

				var provider = (IWarehouseRegimeTypeProvider)objectHandle?.GetObject(shipment);
				if (provider != null)
				{
					result = provider.GetCustomsRegime();
				}
			}
			return result;
		}

		public static ZString? GetWarehouseCustomsOutwardEntryNumber(this Shipment shipment, IDataContextDataObject context)
		{
			ZString? result = null;
			var types = ObjectFactory.Get<Hashtable>("WarehouseCustomsOutwardEntryNumberProviders");
			var objectHandle = (ObjectHandle)types[context.CountryCodeToImportInto.ToString()] ?? (ObjectHandle)types["Shared"];
			var provider = objectHandle != null ? (IWarehouseCustomsOutwardEntryNumberProvider)objectHandle.GetObject(shipment) : null;
			if (provider != null)
			{
				result = provider.GetEntryNumber();
			}
			return result;
		}

		public static IWarehouseCustomsDetailsChangeOfRegime GetWarehouseCustomsDetailsChangeOfRegime(this Shipment shipment, IDataContextDataObject context)
		{
			var types = ObjectFactory.Get<Hashtable>("WarehouseCustomsDetailsChangeOfRegimes");
			var objectHandle = (ObjectHandle)types[context.CountryCodeToImportInto.ToString()] ?? (ObjectHandle)types["Shared"];
			return objectHandle != null ? (IWarehouseCustomsDetailsChangeOfRegime)objectHandle.GetObject(shipment) : null;
		}
	}
}
