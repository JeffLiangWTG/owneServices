using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	public static class GVMSMessageTestHelper
	{
		public static void SetupInspectionLocationsRefCusCodeList(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.GbGVMSInspectionType, "Inspection types for Goods Vehicle Movement System (UK)");
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.GbGVMSInspectionLocation, "Inspection locations for Goods Vehicle Movement System (UK)");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, RefCusCodeListTypes.Codes.GbGVMSInspectionType, "1", "CUSTOMS", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, RefCusCodeListTypes.Codes.GbGVMSInspectionType, "2", "DEFRA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, RefCusCodeListTypes.Codes.GbGVMSInspectionLocation, "L0029A", "Sevington", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, RefCusCodeListTypes.Codes.GbGVMSInspectionLocation, "L0030A", "Stop 24", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, RefCusCodeListTypes.Codes.GbGVMSInspectionLocation, "L0031A", "Dover Western Docks", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();
		}
	}
}
