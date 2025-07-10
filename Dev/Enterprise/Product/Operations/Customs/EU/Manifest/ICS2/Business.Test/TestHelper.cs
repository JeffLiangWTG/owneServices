using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	static class TestHelper
	{
		internal static void PrepareCL010(BusinessObjectFactory factory)
		{
			var startDate = ZDateTime.Today.AddMonths(-1);
			var endDate = ZDateTime.Today.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eunCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			helper.CreateNewOrGetExistingDataGrouping(eunCode);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "European Countries Of Destination");
			helper.CreateCusCodeList(eunCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.Germany, startDate, endDate);
			factory.Save();
		}
	}
}
