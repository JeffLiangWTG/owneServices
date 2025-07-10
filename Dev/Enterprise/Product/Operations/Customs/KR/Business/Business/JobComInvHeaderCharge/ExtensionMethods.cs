using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public static class ExtensionMethods
	{
		public static string GetIncoTermChargeFactoryCacheKey(this JobDeclaration declaration) => GetCustomsChargeTypeListCacheKeyCore(declaration.JE_MessageType);

		public static string GetIncoTermChargeFactoryCacheKey(this JobComInvoiceHeader invoice) => GetCustomsChargeTypeListCacheKeyCore(invoice.PersistentDeclaration?.JE_MessageType);

		public static string GetIncoTermChargeFactoryCacheKey(this JobComInvoiceGroupHeader groupInvoice) => GetCustomsChargeTypeListCacheKeyCore(groupInvoice.JobDeclaration?.JE_MessageType);

		static string GetCustomsChargeTypeListCacheKeyCore(string messageType)
		{
			if (messageType == KRJobMessageTypeList.Codes.LocalExport)
			{
				return KRJobMessageTypeList.Codes.LocalExport;
			}
			else if (messageType == KRJobMessageTypeList.Codes.Import)
			{
				return Common.Shared.SharedJobMessageTypeList.Codes.Import;
			}
			else
			{
				return Common.Shared.SharedJobMessageTypeList.Codes.Export;
			}
		}

		public static CodeDescriptionPairList GetChargeTypeList(this JobComInvoiceHeader invoice, BusinessObjectFactory factory) => GetChargeTypeListCore(factory, invoice.JZ_ValuationCode);

		static CodeDescriptionPairList GetChargeTypeListCore(BusinessObjectFactory factory, string valuationCode)
		{
			var result = new CodeDescriptionPairList();
			if (!string.IsNullOrEmpty(valuationCode))
			{
				switch (valuationCode)
				{
					case ValuationCodeList.Codes.MethodOne:
						result = factory.GetCachedValue<ImportChargeMethodOneCodeList>();
						break;
					case ValuationCodeList.Codes.MethodTwo:
					case ValuationCodeList.Codes.MethodThree:
						result = factory.GetCachedValue<ImportChargeMethodTwoAndThreeCodeList>();
						break;
					case ValuationCodeList.Codes.MethodFourA:
					case ValuationCodeList.Codes.MethodFourB:
						result = factory.GetCachedValue<ImportChargeMethodFourCodeList>();
						break;
					case ValuationCodeList.Codes.MethodFive:
					case ValuationCodeList.Codes.MethodSix:
						result = factory.GetCachedValue<ImportChargeMethodFiveAndSixCodeList>();
						break;
				}
			}
			return result;
		}

		public static ZString GetLoadPortNameInKorean(BusinessObjectFactory factory, ZString portOfLoading)
		{
			var result = ZString.Empty;
			if (portOfLoading.Length == 3)
			{
				result = factory.GetCachedValue<IATALoadPortKRList>().GetDescriptionFromCode(portOfLoading);
			}
			else
			{
				var unloco = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, portOfLoading);
				result = unloco == null ? ZString.Empty : RefLanguageText.GetTranslation(factory, Constants.KRLanguageText, RefUNLOCO.Schema.RL_PortName, RefUNLOCOSchema.Constants.Prefix, unloco.PK);
			}
			return result;
		}

		public static void CheckCurrencyAndRate(this ZPropertyInfo exchangeRatePropertyInfo, JobDeclaration declaration, ZString currencyCode)
		{
			if (declaration != null)
			{
				if (declaration.IsImport || (declaration.IsExport && !declaration.IsDeclarationProcedureTypeE))
				{
					var rate = (ZDecimal)exchangeRatePropertyInfo.Value;
					if (rate == 0 && !currencyCode.IsEmpty)
					{
						if (Constants.KrCurrencyCodeList.Contains(currencyCode))
						{
							exchangeRatePropertyInfo.AddMessageError(ExchangeRatePublishedMessage);
						}
						else
						{
							exchangeRatePropertyInfo.AddMessageError(ExchangeRateNonPublishedMessage);
						}
					}
				}
			}
		}

		public static string ExchangeRatePublishedMessage => Res.GetString("9D893097-9CEA-4FE7-A777-99F5B7F9F13E", "There is no exchange rate available. System retrieves exchange rates every hour on Saturdays and Sundays. If this is a weekend and you have not done so, please wait an hour and try to enter a currency again. If it is a weekday or you have already waited an hour, then please talk to the CW1 Support team. In the meantime, if you know the rate for the week, you can register the exchange rate in Maintain > Reference Files > Exchange Rates. Please note that the exchange rate will be used by all users once registered. Please ensure they are accurate.");
		public static string ExchangeRateNonPublishedMessage => Res.GetString("5CFD2BE7-15E0-407F-A1C4-A342DB6EF9AF", "KR Customs does not publish an exchange rate for this currency. Please contact the nominated bank approved by the KR Customs and register the exchange rate in Maintain > Reference Files > Exchange Rates for this week. Please note that the exchange rate will be used by all users once registered. Please ensure they are accurate.");
	}
}
