using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BarcodeParsing.Business
{
	// tested in BarcodeRuleSetLookupsTest and BusinessObjectFactoryExtensionsTest
	public static class BarcodeParsingLookupHelper
	{
		internal static OrgHeaderCollection GetBuyers(BusinessObjectFactory factory, ZString moduleCode)
		{
			return factory.GetCachedValue("BarcodeParsing|Buyers|" + moduleCode,
				() => factory.GetBarcodeParsingConsumerFromModuleCode(moduleCode).Buyers ?? new OrgHeaderCollection(factory));
		}

		internal static OrgHeaderCollection GetSuppliers(BusinessObjectFactory factory, ZString moduleCode)
		{
			return factory.GetCachedValue("BarcodeParsing|Suppliers|" + moduleCode,
				() => factory.GetBarcodeParsingConsumerFromModuleCode(moduleCode).Suppliers ?? new OrgHeaderCollection(factory));
		}

		public static BarcodeModuleTypes ModuleTypes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("BarcodeParsing|ModuleTypes", GetBarcodeModuleTypes);
		}

		static BarcodeModuleTypes GetBarcodeModuleTypes()
		{
			var moduleTypes = new BarcodeModuleTypes();

			return moduleTypes;
		}

		internal static IBusinessObjectCollection RelatedEntityList(BusinessObjectFactory factory, ZString moduleCode, OrgHeader buyer, OrgHeader supplier)
		{
			var buyerPK = buyer == null ? ZGuid.Empty : buyer.PK;
			var supplierPK = supplier == null ? ZGuid.Empty : supplier.PK;
			return factory.GetCachedValue("BarcodeParsing|RelatedEntityList|" +
				moduleCode + "|" + buyerPK + "|" + supplierPK, () => factory.GetBarcodeParsingConsumerFromModuleCode(moduleCode).GetRelatedEntityList(buyer, supplier));
		}

		public static DiagnosticsTypes DiagnosticsTypes(BusinessObjectFactory factory) => factory.GetCachedValue("BarcodeParsing|DiagnosticsTypes", () => new DiagnosticsTypes());

		internal static ReadOnlyCodeDescriptionPairList GetTargetFields(BusinessObjectFactory factory, ZString moduleCode)
		{
			return factory.GetCachedValue("BarcodeParsing|TargetFields|" + moduleCode, () => factory.GetBarcodeParsingConsumerFromModuleCode(moduleCode).TargetFields);
		}
	}
}
