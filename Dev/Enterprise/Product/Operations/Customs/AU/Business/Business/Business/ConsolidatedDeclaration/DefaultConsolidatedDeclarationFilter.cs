using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class DefaultConsolidatedDeclarationFilter
	{
		public static FilterBusinessObjectDefaults GetDefaultConsolidatedDeclarationFilter(BaseJobDeclaration declaration)
		{
			var defaults = new FilterBusinessObjectDefaults
			{
				new FilterBusinessObjectDefault("Importer/Supplier", "Property1", declaration.JE_OH_Importer, false),
				new FilterBusinessObjectDefault("Transport Mode", "Property", declaration.JE_TransportMode, false),
				new FilterBusinessObjectDefault("Shipment Sub-Type", "Property", declaration.JE_MessageSubType, false),
				new FilterBusinessObjectDefault("Master Bill", "Property", declaration.JE_MasterBill, false),
				new FilterBusinessObjectDefault("Arrival at Discharge Port", "PropertySearch", ModuleDateFilter.SpecifiedDateRange, false),
				new FilterBusinessObjectDefault("Arrival at Discharge Port", "Property1", declaration.JE_DateAtFinalDestination.Date, false),
				new FilterBusinessObjectDefault("Arrival at Discharge Port", "Property2", declaration.JE_DateAtFinalDestination.Date, false)
			};

			return defaults;
		}
	}
}
