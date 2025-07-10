using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	public static class TestHelper
	{
		public static BusinessObjectFactory SetupEntryStatusList(this BusinessObjectFactory factory, ZString[] includingCodes = null, ZString[] notIncludingCodes = null, bool shouldSave = true)
		{
			if (includingCodes == null)
			{
				includingCodes = new ZString[] { "ST1", "ST2", "ST3" };
			}
			if (notIncludingCodes == null)
			{
				notIncludingCodes = new ZString[] { "ST9" };
			}
			var countryCode = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateCusCodeType(RefCusCodeListTypes.Codes.UserDefinedEntryStatus, "User Defined Entry Status");
			includingCodes.ToList().ForEach(code =>
			{
				helper.CreateCusCodeList(countryCode, RefCusCodeListTypes.Codes.UserDefinedEntryStatus, code, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			});
			notIncludingCodes.ToList().ForEach(ncode =>
			{
				helper.CreateCusCodeList("XX", RefCusCodeListTypes.Codes.UserDefinedEntryStatus, ncode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			});
			if (shouldSave)
			{
				factory.Save();
			}
			return factory;
		}
	}
}
