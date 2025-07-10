using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants.Customs.Universal;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	class EMCSPackageTestHelper
	{
		internal static void SetupPackageTypeCodeList(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateCusCodeType(RefCusCodeListTypes.Codes.EMCSPackTypes, "Package types", RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Countable, "Countable", RefCusCodeListTypes.Codes.EMCSPackTypes, RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateCusCodeList(RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.EMCSPackTypes, UncountableUnitType, "bulk", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateCusCodeListWithAttribute(RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.EMCSPackTypes, CountableUnitType, "box", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), RefCusCodeListAttributeTypes.Codes.Countable, RefCusCodeListAttributeTypes.Codes.Countable);
			helper.CreateCusCodeListWithAttribute(RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.EMCSPackTypes, CountableUnitType2, "ampoule", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), RefCusCodeListAttributeTypes.Codes.Countable, RefCusCodeListAttributeTypes.Codes.Countable);
			factory.Save();
		}

		internal const string UncountableUnitType = "NE";
		internal const string CountableUnitType = "BX";
		internal const string CountableUnitType2 = "AP";
	}
}
