using CargoWise.EntityFramework;
using CargoWise.Types;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	public class ZZDataTestHelper : Universal.Testing.UniversalReferenceTestDataHelper
	{
		public ZZDataTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static void SetupZZ(BusinessObjectFactory factory, ZString countryCode)
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			var manifestCountryData = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestCountry, countryCode, countryCode + " DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(manifestCountryData.PK, RefCusCodeListTypes.Codes.NVC, "17.3.29.1");
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsManifestStatus, "Customs Manifest Status");
			var list6 = helper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.CustomsManifestStatus, "6", "Rejected", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var list8 = helper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.CustomsManifestStatus, "8", "Proceed to Border", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(list6.PK, "CustomsRejected", "true");
			helper.CreateNewOrGetExistingCusCodeListAttribute(list6.PK, "IUpdateCustomsStatus", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(list6.PK, "INotify", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(list8.PK, "CustomsCleared", "true");
			helper.CreateNewOrGetExistingCusCodeListAttribute(list8.PK, "INotify", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(list8.PK, "IAllowCancel", "true");
			helper.CreateNewOrGetExistingCusCodeListAttribute(list8.PK, "IUpdateCustomsStatus", "");
			factory.Save();
		}
	}
}
