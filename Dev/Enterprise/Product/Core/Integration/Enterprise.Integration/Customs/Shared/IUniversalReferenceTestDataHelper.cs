using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public static partial class Universal
			{
				public interface IUniversalReferenceTestDataHelper
				{
					BusinessObject CreateTaxOrFee(ZString code, ZDecimal rate, ZString dataGroupingCode,
						ZDateTime? startDate = null, ZDateTime? endDate = null, string description = null,
						bool ensureDataGroupingExists = true, decimal threshold = 0m);

					BusinessObject CreateNewOrGetExistingDataGrouping(ZString zzzDataGrouping, string description = null, BusinessObject parent = null);

					BusinessObject CreateTradeGroup(ZString dataGroupingCode, ZString tradeGroup, ZDateTime startDate, ZDateTime endDate, string description = null, bool ensureDataGroupingExists = true, bool isSystem = true);

					BusinessObject AddCountry(BusinessObject tradeGroup, ZString tradeAgreementCountryCode, ZDate? startDate = null, ZDate? endDate = null, string countryCodeDescription = "");

					BusinessObject CreateNewOrGetExistingCusCodeType(ZString code, ZString desc, string dataGrouping = "ZZ", byte maxLength = 0, bool allowCreationOfFuncsOrPFunc = false);

					BusinessObject CreateNewOrGetExistingCusCodeList(ZString dataGroupingCode, ZString codeType, ZString code, ZString description, ZDateTime startDate, ZDateTime endDate);

					BusinessObject CreateNewOrGetExistingCusCodeListAttribute(ZGuid cusCodeListPK, ZString attributeName, ZString attributeValue);

					BusinessObject CreatePreferenceForCountry(ZString code, ZString description, ZString dataGroupingCode, bool isSystem = true);
				}
			}
		}
	}
}
