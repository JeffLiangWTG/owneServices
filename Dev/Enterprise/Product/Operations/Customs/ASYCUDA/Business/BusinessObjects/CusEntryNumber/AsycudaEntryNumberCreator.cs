using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public static class AsycudaEntryNumberCreator
	{
		static T CreateOrUpdate<T>(BusinessObject bizObj, ZString number, ZString entryType, ZString countryCode)
			where T : CusEntryNumber
		{
			var result = CusEntryNumber.LoadOrCreate<T>(bizObj, entryType, countryCode);
			if (!result.CE_EntryNum.EqualsIgnoringCase(number))
			{
				result.CE_EntryNum = number;
			}
			return result;
		}

		public static T CreateOrUpdateCustomsEntryNumber<T>(BusinessObject bizObj, ZString number, ZString countryCode)
			where T : CusEntryNumber
		{
			return CreateOrUpdate<T>(bizObj, number, Constants.CustomsEntryType.TradeNetPermit, countryCode);
		}

		public static T CreateOrUpdateRegistrationNumber<T>(BusinessObject bizObj, ZString number, ZString countryCode)
			where T : CusEntryNumber
		{
			return CreateOrUpdate<T>(bizObj, number, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, countryCode);
		}
	}
}
