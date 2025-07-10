using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.GVMS
{
	public class GvmsInspectionAtLocationCusCodeDataLookups : CusCodeDataLookups
	{
		public GvmsInspectionAtLocationCusCodeDataLookups(AutoCusCodeData parent) : base(parent) { }

		public CodeDescriptionPairList TypeList => GetInspectionTypeList(Factory);

		public CodeDescriptionPairList InspectionLocationList => GetInspectionLocationList(Factory);

		public static CodeDescriptionPairList GetInspectionTypeList(BusinessObjectFactory factory) => RefCusCodeListTypes.GetCachedList(factory, CountryCodes.UnitedKingdom,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbGVMSInspectionType, ZDateTime.Now);

		public static CodeDescriptionPairList GetInspectionLocationList(BusinessObjectFactory factory) => RefCusCodeListTypes.GetCachedList(factory, CountryCodes.UnitedKingdom,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbGVMSInspectionLocation, ZDateTime.Now);
	}
}
