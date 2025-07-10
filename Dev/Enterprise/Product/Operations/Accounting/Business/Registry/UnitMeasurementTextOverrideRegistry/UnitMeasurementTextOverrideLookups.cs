using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Registry.Business
{
	public class UnitMeasurementTextOverrideLookups
	{
		public UnitMeasurementTextOverrideLookups() { }

		public CodeDescriptionPairList UnitMeasurementList
		{
			get
			{
				var cacheKey = "UnitMeasurementList";
				return Factory.GetCachedValue(cacheKey,
				() =>
				{
					var result = new CodeDescriptionPairList();

					var packageDescription = MeasureTypeDescriptions.GetDescription(MeasureType.Package);
					var loadingMeterDescription = MeasureTypeDescriptions.GetDescription(MeasureType.LoadingMeters);
					var unitDescription = MeasureTypeDescriptions.GetDescription(MeasureType.Unit);
					var shipmentDescription = MeasureTypeDescriptions.GetDescription(MeasureType.Shipment);

					result.AddRange(UnitHelper.GetForwardingAndCustomsUnits(Factory, CountryCodes.VietNam));
					result.AddPair(packageDescription, packageDescription);
					result.AddPair(loadingMeterDescription, loadingMeterDescription);
					result.AddPair(unitDescription, unitDescription);
					result.AddPair(shipmentDescription, shipmentDescription);
					result.AddPair(TEU, TEU);

					return result;
				});
			}
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		const string TEU = "TEU";
	}
}
