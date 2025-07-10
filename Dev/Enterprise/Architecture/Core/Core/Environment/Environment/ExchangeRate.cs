using System;
using System.Collections.Generic;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class ExchangeRate
	{
		public ExchangeRate(bool isReciprocal, int localCurrencyDecimals, Guid companyPK)
		{
			IsReciprocal = isReciprocal;
			LocalCurrencyDecimals = localCurrencyDecimals;
			CompanyPK = companyPK;
		}

		public decimal LocalToForeign(decimal localAmount, decimal exchangeRate, string foreignCurrencyCode)
		{
			Currency foreignCurrency = new Currency(foreignCurrencyCode);
			return ConvertedAmount(localAmount, exchangeRate, IsReciprocal, foreignCurrency.Decimals, true);
		}

		public decimal LocalToForeignWithoutRounding(decimal localAmount, decimal exchangeRate, string foreignCurrencyCode)
		{
			Currency foreignCurrency = new Currency(foreignCurrencyCode);
			return ConvertedAmount(localAmount, exchangeRate, IsReciprocal, foreignCurrency.Decimals, false);
		}

		public decimal ForeignToLocal(decimal foreignAmount, decimal exchangeRate)
		{
			return ConvertedAmount(foreignAmount, exchangeRate, !IsReciprocal, LocalCurrencyDecimals, true);
		}

		public decimal ForeignToLocalWithoutRounding(decimal foreignAmount, decimal exchangeRate)
		{
			return ConvertedAmount(foreignAmount, exchangeRate, !IsReciprocal, LocalCurrencyDecimals, false);
		}

		public decimal ForeignToForeign(decimal sourceAmount, decimal sourceExchangeRate, decimal targetExchangeRate, string targetCurrencyCode)
		{
			decimal amountInLocal = ForeignToLocalWithoutRounding(sourceAmount, sourceExchangeRate);
			return LocalToForeign(amountInLocal, targetExchangeRate, targetCurrencyCode);
		}

		public decimal ForeignToForeignWithoutRounding(decimal sourceAmount, decimal sourceExchangeRate, decimal targetExchangeRate, string targetCurrencyCode)
		{
			decimal amountInLocal = ForeignToLocalWithoutRounding(sourceAmount, sourceExchangeRate);
			return LocalToForeignWithoutRounding(amountInLocal, targetExchangeRate, targetCurrencyCode);
		}

		public decimal GetRate(decimal localAmount, decimal foreignAmount)
		{
			return GetRateCore(localAmount, foreignAmount, RateDecimals);
		}

		public decimal GetRate(decimal localAmount, decimal foreignAmount, int decimals)
		{
			return GetRateCore(localAmount, foreignAmount, decimals);
		}

		decimal GetRateCore(decimal localAmount, decimal foreignAmount, int decimals)
		{
			decimal result = 1;

			if (localAmount != 0 || foreignAmount != 0)
			{
				decimal numerator = IsReciprocal ? localAmount : foreignAmount;
				decimal denominator = IsReciprocal ? foreignAmount : localAmount;
				result = denominator == 0 ? 0 : Utilities.Round(numerator / denominator, decimals);
			}

			return result;
		}

		#region Get Rate

		/// <summary>
		/// Rate at ValuationDatTime of a particular type for a given currency; zero if none.
		/// </summary>
		public decimal GetRate(string currencyCode, ExchangeRateType rateType, DateTime valuationDateTime, Guid? localClientPK = null)
		{
			decimal result = 0;

			if (!string.IsNullOrEmpty(currencyCode))
			{
				var rateReader = ExchangeRateReader.GetReaderInstance();
				var exchangeRateType = GetExchangeRateType(rateType);

				result = string.IsNullOrEmpty(exchangeRateType) ? 0 : rateReader.GetRate(CompanyPK, exchangeRateType, currencyCode, valuationDateTime, false, localClientPK);
			}
			return result;
		}

		/// <summary>
		/// Rate at time for type for a given currency; falling back to last defined rate for date
		/// </summary>
		public decimal GetRateIncludingExpired(string currencyCode, ExchangeRateType rateType, DateTime valuationDateTime, Guid? localClientPK = null)
		{
			decimal result = 0;

			if (!string.IsNullOrEmpty(currencyCode))
			{
				var rateReader = ExchangeRateReader.GetReaderInstance();
				var exchangeRateType = GetExchangeRateType(rateType);

				result = string.IsNullOrEmpty(exchangeRateType) ? 0 : rateReader.GetRate(CompanyPK, exchangeRateType, currencyCode, valuationDateTime, true, localClientPK);
			}
			return result;
		}

		public static string GetExchangeRateType(ExchangeRateType rateType)
		{
			var exchangeRateTypeList = new Dictionary<ExchangeRateType, string>()
			{
				{ ExchangeRateType.Buy, Constants.ExchangeRateTypes.Code.BuyRate },
				{ ExchangeRateType.Sell, Constants.ExchangeRateTypes.Code.SellRate },
				{ ExchangeRateType.CustomsMeasureEURExRate, Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate },
				{ ExchangeRateType.Customs, Constants.ExchangeRateTypes.Code.CustomsRate },
				{ ExchangeRateType.CustomsSecondary, Constants.ExchangeRateTypes.Code.CustomsRateSecondary },
				{ ExchangeRateType.PeriodEnd, Constants.ExchangeRateTypes.Code.PeriodEndRate },
				{ ExchangeRateType.GlobalCreditControl, Constants.ExchangeRateTypes.Code.GlobalCreditControl },
				{ ExchangeRateType.IATA, Constants.ExchangeRateTypes.Code.IATARate },
				{ ExchangeRateType.C01, Constants.ExchangeRateTypes.Code.C01Rate },
				{ ExchangeRateType.C02, Constants.ExchangeRateTypes.Code.C02Rate },
				{ ExchangeRateType.C03, Constants.ExchangeRateTypes.Code.C03Rate },
				{ ExchangeRateType.C04, Constants.ExchangeRateTypes.Code.C04Rate },
				{ ExchangeRateType.C05, Constants.ExchangeRateTypes.Code.C05Rate },
				{ ExchangeRateType.C06, Constants.ExchangeRateTypes.Code.C06Rate },
				{ ExchangeRateType.C07, Constants.ExchangeRateTypes.Code.C07Rate },
				{ ExchangeRateType.C08, Constants.ExchangeRateTypes.Code.C08Rate },
				{ ExchangeRateType.C09, Constants.ExchangeRateTypes.Code.C09Rate },
				{ ExchangeRateType.C10, Constants.ExchangeRateTypes.Code.C10Rate },
				{ ExchangeRateType.C11, Constants.ExchangeRateTypes.Code.C11Rate },
				{ ExchangeRateType.C12, Constants.ExchangeRateTypes.Code.C12Rate },
				{ ExchangeRateType.C13, Constants.ExchangeRateTypes.Code.C13Rate },
				{ ExchangeRateType.C14, Constants.ExchangeRateTypes.Code.C14Rate },
				{ ExchangeRateType.C15, Constants.ExchangeRateTypes.Code.C15Rate },
				{ ExchangeRateType.C16, Constants.ExchangeRateTypes.Code.C16Rate },
				{ ExchangeRateType.C17, Constants.ExchangeRateTypes.Code.C17Rate },
				{ ExchangeRateType.C18, Constants.ExchangeRateTypes.Code.C18Rate },
				{ ExchangeRateType.C19, Constants.ExchangeRateTypes.Code.C19Rate },
				{ ExchangeRateType.C20, Constants.ExchangeRateTypes.Code.C20Rate },
				{ ExchangeRateType.C21, Constants.ExchangeRateTypes.Code.C21Rate },
				{ ExchangeRateType.C22, Constants.ExchangeRateTypes.Code.C22Rate },
				{ ExchangeRateType.C23, Constants.ExchangeRateTypes.Code.C23Rate },
				{ ExchangeRateType.C24, Constants.ExchangeRateTypes.Code.C24Rate },
				{ ExchangeRateType.C25, Constants.ExchangeRateTypes.Code.C25Rate },
				{ ExchangeRateType.C26, Constants.ExchangeRateTypes.Code.C26Rate },
				{ ExchangeRateType.C27, Constants.ExchangeRateTypes.Code.C27Rate },
				{ ExchangeRateType.C28, Constants.ExchangeRateTypes.Code.C28Rate },
				{ ExchangeRateType.C29, Constants.ExchangeRateTypes.Code.C29Rate },
				{ ExchangeRateType.C30, Constants.ExchangeRateTypes.Code.C30Rate },
				{ ExchangeRateType.C31, Constants.ExchangeRateTypes.Code.C31Rate },
				{ ExchangeRateType.C32, Constants.ExchangeRateTypes.Code.C32Rate },
				{ ExchangeRateType.C33, Constants.ExchangeRateTypes.Code.C33Rate },
				{ ExchangeRateType.C34, Constants.ExchangeRateTypes.Code.C34Rate },
				{ ExchangeRateType.C35, Constants.ExchangeRateTypes.Code.C35Rate },
				{ ExchangeRateType.C36, Constants.ExchangeRateTypes.Code.C36Rate },
				{ ExchangeRateType.C37, Constants.ExchangeRateTypes.Code.C37Rate },
				{ ExchangeRateType.C38, Constants.ExchangeRateTypes.Code.C38Rate },
				{ ExchangeRateType.C39, Constants.ExchangeRateTypes.Code.C39Rate },
				{ ExchangeRateType.C40, Constants.ExchangeRateTypes.Code.C40Rate },
				{ ExchangeRateType.C41, Constants.ExchangeRateTypes.Code.C41Rate },
				{ ExchangeRateType.C42, Constants.ExchangeRateTypes.Code.C42Rate },
				{ ExchangeRateType.C43, Constants.ExchangeRateTypes.Code.C43Rate },
				{ ExchangeRateType.C44, Constants.ExchangeRateTypes.Code.C44Rate },
				{ ExchangeRateType.C45, Constants.ExchangeRateTypes.Code.C45Rate },
				{ ExchangeRateType.C46, Constants.ExchangeRateTypes.Code.C46Rate },
				{ ExchangeRateType.C47, Constants.ExchangeRateTypes.Code.C47Rate },
				{ ExchangeRateType.C48, Constants.ExchangeRateTypes.Code.C48Rate },
				{ ExchangeRateType.C49, Constants.ExchangeRateTypes.Code.C49Rate },
				{ ExchangeRateType.C50, Constants.ExchangeRateTypes.Code.C50Rate },
				{ ExchangeRateType.C51, Constants.ExchangeRateTypes.Code.C51Rate },
				{ ExchangeRateType.C52, Constants.ExchangeRateTypes.Code.C52Rate },
				{ ExchangeRateType.C53, Constants.ExchangeRateTypes.Code.C53Rate },
				{ ExchangeRateType.C54, Constants.ExchangeRateTypes.Code.C54Rate },
				{ ExchangeRateType.C55, Constants.ExchangeRateTypes.Code.C55Rate },
				{ ExchangeRateType.C56, Constants.ExchangeRateTypes.Code.C56Rate },
				{ ExchangeRateType.C57, Constants.ExchangeRateTypes.Code.C57Rate },
				{ ExchangeRateType.C58, Constants.ExchangeRateTypes.Code.C58Rate },
				{ ExchangeRateType.C59, Constants.ExchangeRateTypes.Code.C59Rate },
				{ ExchangeRateType.C60, Constants.ExchangeRateTypes.Code.C60Rate },
				{ ExchangeRateType.C61, Constants.ExchangeRateTypes.Code.C61Rate },
				{ ExchangeRateType.C62, Constants.ExchangeRateTypes.Code.C62Rate },
				{ ExchangeRateType.C63, Constants.ExchangeRateTypes.Code.C63Rate },
				{ ExchangeRateType.C64, Constants.ExchangeRateTypes.Code.C64Rate },
				{ ExchangeRateType.C65, Constants.ExchangeRateTypes.Code.C65Rate },
				{ ExchangeRateType.C66, Constants.ExchangeRateTypes.Code.C66Rate },
				{ ExchangeRateType.C67, Constants.ExchangeRateTypes.Code.C67Rate },
				{ ExchangeRateType.C68, Constants.ExchangeRateTypes.Code.C68Rate },
				{ ExchangeRateType.C69, Constants.ExchangeRateTypes.Code.C69Rate },
				{ ExchangeRateType.C70, Constants.ExchangeRateTypes.Code.C70Rate },
				{ ExchangeRateType.C71, Constants.ExchangeRateTypes.Code.C71Rate },
				{ ExchangeRateType.C72, Constants.ExchangeRateTypes.Code.C72Rate },
				{ ExchangeRateType.C73, Constants.ExchangeRateTypes.Code.C73Rate },
				{ ExchangeRateType.C74, Constants.ExchangeRateTypes.Code.C74Rate },
				{ ExchangeRateType.C75, Constants.ExchangeRateTypes.Code.C75Rate },
				{ ExchangeRateType.C76, Constants.ExchangeRateTypes.Code.C76Rate },
				{ ExchangeRateType.C77, Constants.ExchangeRateTypes.Code.C77Rate },
				{ ExchangeRateType.C78, Constants.ExchangeRateTypes.Code.C78Rate },
				{ ExchangeRateType.C79, Constants.ExchangeRateTypes.Code.C79Rate },
				{ ExchangeRateType.C80, Constants.ExchangeRateTypes.Code.C80Rate },
				{ ExchangeRateType.C81, Constants.ExchangeRateTypes.Code.C81Rate },
				{ ExchangeRateType.C82, Constants.ExchangeRateTypes.Code.C82Rate },
				{ ExchangeRateType.C83, Constants.ExchangeRateTypes.Code.C83Rate },
				{ ExchangeRateType.C84, Constants.ExchangeRateTypes.Code.C84Rate },
				{ ExchangeRateType.C85, Constants.ExchangeRateTypes.Code.C85Rate },
				{ ExchangeRateType.C86, Constants.ExchangeRateTypes.Code.C86Rate },
				{ ExchangeRateType.C87, Constants.ExchangeRateTypes.Code.C87Rate },
				{ ExchangeRateType.C88, Constants.ExchangeRateTypes.Code.C88Rate },
				{ ExchangeRateType.C89, Constants.ExchangeRateTypes.Code.C89Rate },
				{ ExchangeRateType.C90, Constants.ExchangeRateTypes.Code.C90Rate },
				{ ExchangeRateType.C91, Constants.ExchangeRateTypes.Code.C91Rate },
				{ ExchangeRateType.C92, Constants.ExchangeRateTypes.Code.C92Rate },
				{ ExchangeRateType.C93, Constants.ExchangeRateTypes.Code.C93Rate },
				{ ExchangeRateType.C94, Constants.ExchangeRateTypes.Code.C94Rate },
				{ ExchangeRateType.C95, Constants.ExchangeRateTypes.Code.C95Rate },
				{ ExchangeRateType.C96, Constants.ExchangeRateTypes.Code.C96Rate },
				{ ExchangeRateType.C97, Constants.ExchangeRateTypes.Code.C97Rate },
				{ ExchangeRateType.C98, Constants.ExchangeRateTypes.Code.C98Rate },
				{ ExchangeRateType.C99, Constants.ExchangeRateTypes.Code.C99Rate },
				{ ExchangeRateType.L01, Constants.ExchangeRateTypes.Code.L01Rate },
				{ ExchangeRateType.L02, Constants.ExchangeRateTypes.Code.L02Rate },
				{ ExchangeRateType.L03, Constants.ExchangeRateTypes.Code.L03Rate },
				{ ExchangeRateType.L04, Constants.ExchangeRateTypes.Code.L04Rate },
				{ ExchangeRateType.L05, Constants.ExchangeRateTypes.Code.L05Rate },
				{ ExchangeRateType.L06, Constants.ExchangeRateTypes.Code.L06Rate },
				{ ExchangeRateType.L07, Constants.ExchangeRateTypes.Code.L07Rate },
				{ ExchangeRateType.L08, Constants.ExchangeRateTypes.Code.L08Rate },
				{ ExchangeRateType.L09, Constants.ExchangeRateTypes.Code.L09Rate },
				{ ExchangeRateType.L10, Constants.ExchangeRateTypes.Code.L10Rate },
				{ ExchangeRateType.L11, Constants.ExchangeRateTypes.Code.L11Rate },
				{ ExchangeRateType.L12, Constants.ExchangeRateTypes.Code.L12Rate },
				{ ExchangeRateType.L13, Constants.ExchangeRateTypes.Code.L13Rate },
				{ ExchangeRateType.L14, Constants.ExchangeRateTypes.Code.L14Rate },
				{ ExchangeRateType.L15, Constants.ExchangeRateTypes.Code.L15Rate },
				{ ExchangeRateType.L16, Constants.ExchangeRateTypes.Code.L16Rate },
				{ ExchangeRateType.L17, Constants.ExchangeRateTypes.Code.L17Rate },
				{ ExchangeRateType.L18, Constants.ExchangeRateTypes.Code.L18Rate },
				{ ExchangeRateType.L19, Constants.ExchangeRateTypes.Code.L19Rate },
				{ ExchangeRateType.L20, Constants.ExchangeRateTypes.Code.L20Rate },
				{ ExchangeRateType.L21, Constants.ExchangeRateTypes.Code.L21Rate },
				{ ExchangeRateType.L22, Constants.ExchangeRateTypes.Code.L22Rate },
				{ ExchangeRateType.L23, Constants.ExchangeRateTypes.Code.L23Rate },
				{ ExchangeRateType.L24, Constants.ExchangeRateTypes.Code.L24Rate },
				{ ExchangeRateType.L25, Constants.ExchangeRateTypes.Code.L25Rate },
				{ ExchangeRateType.L26, Constants.ExchangeRateTypes.Code.L26Rate },
				{ ExchangeRateType.L27, Constants.ExchangeRateTypes.Code.L27Rate },
				{ ExchangeRateType.L28, Constants.ExchangeRateTypes.Code.L28Rate },
				{ ExchangeRateType.L29, Constants.ExchangeRateTypes.Code.L29Rate },
				{ ExchangeRateType.L30, Constants.ExchangeRateTypes.Code.L30Rate },
				{ ExchangeRateType.L31, Constants.ExchangeRateTypes.Code.L31Rate },
				{ ExchangeRateType.L32, Constants.ExchangeRateTypes.Code.L32Rate },
				{ ExchangeRateType.L33, Constants.ExchangeRateTypes.Code.L33Rate },
				{ ExchangeRateType.L34, Constants.ExchangeRateTypes.Code.L34Rate },
				{ ExchangeRateType.L35, Constants.ExchangeRateTypes.Code.L35Rate },
				{ ExchangeRateType.L36, Constants.ExchangeRateTypes.Code.L36Rate },
				{ ExchangeRateType.L37, Constants.ExchangeRateTypes.Code.L37Rate },
				{ ExchangeRateType.L38, Constants.ExchangeRateTypes.Code.L38Rate },
				{ ExchangeRateType.L39, Constants.ExchangeRateTypes.Code.L39Rate },
				{ ExchangeRateType.L40, Constants.ExchangeRateTypes.Code.L40Rate },
				{ ExchangeRateType.L41, Constants.ExchangeRateTypes.Code.L41Rate },
				{ ExchangeRateType.L42, Constants.ExchangeRateTypes.Code.L42Rate },
				{ ExchangeRateType.L43, Constants.ExchangeRateTypes.Code.L43Rate },
				{ ExchangeRateType.L44, Constants.ExchangeRateTypes.Code.L44Rate },
				{ ExchangeRateType.L45, Constants.ExchangeRateTypes.Code.L45Rate },
				{ ExchangeRateType.L46, Constants.ExchangeRateTypes.Code.L46Rate },
				{ ExchangeRateType.L47, Constants.ExchangeRateTypes.Code.L47Rate },
				{ ExchangeRateType.L48, Constants.ExchangeRateTypes.Code.L48Rate },
				{ ExchangeRateType.L49, Constants.ExchangeRateTypes.Code.L49Rate },
				{ ExchangeRateType.L50, Constants.ExchangeRateTypes.Code.L50Rate },
				{ ExchangeRateType.L51, Constants.ExchangeRateTypes.Code.L51Rate },
				{ ExchangeRateType.L52, Constants.ExchangeRateTypes.Code.L52Rate },
				{ ExchangeRateType.L53, Constants.ExchangeRateTypes.Code.L53Rate },
				{ ExchangeRateType.L54, Constants.ExchangeRateTypes.Code.L54Rate },
				{ ExchangeRateType.L55, Constants.ExchangeRateTypes.Code.L55Rate },
				{ ExchangeRateType.L56, Constants.ExchangeRateTypes.Code.L56Rate },
				{ ExchangeRateType.L57, Constants.ExchangeRateTypes.Code.L57Rate },
				{ ExchangeRateType.L58, Constants.ExchangeRateTypes.Code.L58Rate },
				{ ExchangeRateType.L59, Constants.ExchangeRateTypes.Code.L59Rate },
				{ ExchangeRateType.L60, Constants.ExchangeRateTypes.Code.L60Rate },
				{ ExchangeRateType.L61, Constants.ExchangeRateTypes.Code.L61Rate },
				{ ExchangeRateType.L62, Constants.ExchangeRateTypes.Code.L62Rate },
				{ ExchangeRateType.L63, Constants.ExchangeRateTypes.Code.L63Rate },
				{ ExchangeRateType.L64, Constants.ExchangeRateTypes.Code.L64Rate },
				{ ExchangeRateType.L65, Constants.ExchangeRateTypes.Code.L65Rate },
				{ ExchangeRateType.L66, Constants.ExchangeRateTypes.Code.L66Rate },
				{ ExchangeRateType.L67, Constants.ExchangeRateTypes.Code.L67Rate },
				{ ExchangeRateType.L68, Constants.ExchangeRateTypes.Code.L68Rate },
				{ ExchangeRateType.L69, Constants.ExchangeRateTypes.Code.L69Rate },
				{ ExchangeRateType.L70, Constants.ExchangeRateTypes.Code.L70Rate },
				{ ExchangeRateType.L71, Constants.ExchangeRateTypes.Code.L71Rate },
				{ ExchangeRateType.L72, Constants.ExchangeRateTypes.Code.L72Rate },
				{ ExchangeRateType.L73, Constants.ExchangeRateTypes.Code.L73Rate },
				{ ExchangeRateType.L74, Constants.ExchangeRateTypes.Code.L74Rate },
				{ ExchangeRateType.L75, Constants.ExchangeRateTypes.Code.L75Rate },
				{ ExchangeRateType.L76, Constants.ExchangeRateTypes.Code.L76Rate },
				{ ExchangeRateType.L77, Constants.ExchangeRateTypes.Code.L77Rate },
				{ ExchangeRateType.L78, Constants.ExchangeRateTypes.Code.L78Rate },
				{ ExchangeRateType.L79, Constants.ExchangeRateTypes.Code.L79Rate },
				{ ExchangeRateType.L80, Constants.ExchangeRateTypes.Code.L80Rate },
				{ ExchangeRateType.L81, Constants.ExchangeRateTypes.Code.L81Rate },
				{ ExchangeRateType.L82, Constants.ExchangeRateTypes.Code.L82Rate },
				{ ExchangeRateType.L83, Constants.ExchangeRateTypes.Code.L83Rate },
				{ ExchangeRateType.L84, Constants.ExchangeRateTypes.Code.L84Rate },
				{ ExchangeRateType.L85, Constants.ExchangeRateTypes.Code.L85Rate },
				{ ExchangeRateType.L86, Constants.ExchangeRateTypes.Code.L86Rate },
				{ ExchangeRateType.L87, Constants.ExchangeRateTypes.Code.L87Rate },
				{ ExchangeRateType.L88, Constants.ExchangeRateTypes.Code.L88Rate },
				{ ExchangeRateType.L89, Constants.ExchangeRateTypes.Code.L89Rate },
				{ ExchangeRateType.L90, Constants.ExchangeRateTypes.Code.L90Rate },
				{ ExchangeRateType.L91, Constants.ExchangeRateTypes.Code.L91Rate },
				{ ExchangeRateType.L92, Constants.ExchangeRateTypes.Code.L92Rate },
				{ ExchangeRateType.L93, Constants.ExchangeRateTypes.Code.L93Rate },
				{ ExchangeRateType.L94, Constants.ExchangeRateTypes.Code.L94Rate },
				{ ExchangeRateType.L95, Constants.ExchangeRateTypes.Code.L95Rate },
				{ ExchangeRateType.L96, Constants.ExchangeRateTypes.Code.L96Rate },
				{ ExchangeRateType.L97, Constants.ExchangeRateTypes.Code.L97Rate },
				{ ExchangeRateType.L98, Constants.ExchangeRateTypes.Code.L98Rate },
				{ ExchangeRateType.L99, Constants.ExchangeRateTypes.Code.L99Rate },
			};
			return exchangeRateTypeList.TryGetValue(rateType, out var value) ? value : string.Empty;
		}

		#endregion

		public bool IsExchangeRateTypeNotEqualToSpecified(string currencyCode, string rateType, DateTime valuationDateTime, bool includeExpired, Guid? localClientPK)
		{
			var rateReader = ExchangeRateReader.GetReaderInstance();
			return rateReader.IsExchangeRateTypeNotEqualToSpecified(CompanyPK, rateType, currencyCode, valuationDateTime, includeExpired, localClientPK);
		}

		/// <summary>
		/// Today's rate of a particular type for a given currency; zero if none.
		/// </summary>
		public decimal TodaysRate(string currencyCode, ExchangeRateType rateType)
		{
			return GetRate(currencyCode, rateType, CurrentLocalDateTime);
		}

		/// <summary>
		/// Today's rate of a particular type for a given currency; falling back to last rate defined before today
		/// </summary>
		public decimal TodaysRateIncludingExpired(string currencyCode, ExchangeRateType rateType)
		{
			return GetRateIncludingExpired(currencyCode, rateType, CurrentLocalDateTime);
		}

		/// <summary>
		/// Converts a RateType string to the ExchangeRateType enum; falling back to ExchangeRateType.All
		/// </summary>
		public static ExchangeRateType GetExchangeRateType(string rateType)
		{
			var exchangeRateTypeList = new Dictionary<string, ExchangeRateType>()
			{
				{ Constants.ExchangeRateTypes.Code.BuyRate,ExchangeRateType.Buy },
				{ Constants.ExchangeRateTypes.Code.SellRate,ExchangeRateType.Sell },
				{ Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate, ExchangeRateType.CustomsMeasureEURExRate },
				{ Constants.ExchangeRateTypes.Code.CustomsRate,ExchangeRateType.Customs },
				{ Constants.ExchangeRateTypes.Code.CustomsRateSecondary,ExchangeRateType.CustomsSecondary },
				{ Constants.ExchangeRateTypes.Code.PeriodEndRate,ExchangeRateType.PeriodEnd },
				{ Constants.ExchangeRateTypes.Code.GlobalCreditControl,ExchangeRateType.GlobalCreditControl },
				{ Constants.ExchangeRateTypes.Code.IATARate,ExchangeRateType.IATA },
				{ Constants.ExchangeRateTypes.Code.C01Rate,ExchangeRateType.C01 },
				{ Constants.ExchangeRateTypes.Code.C02Rate,ExchangeRateType.C02 },
				{ Constants.ExchangeRateTypes.Code.C03Rate,ExchangeRateType.C03 },
				{ Constants.ExchangeRateTypes.Code.C04Rate,ExchangeRateType.C04 },
				{ Constants.ExchangeRateTypes.Code.C05Rate,ExchangeRateType.C05 },
				{ Constants.ExchangeRateTypes.Code.C06Rate,ExchangeRateType.C06 },
				{ Constants.ExchangeRateTypes.Code.C07Rate,ExchangeRateType.C07 },
				{ Constants.ExchangeRateTypes.Code.C08Rate,ExchangeRateType.C08 },
				{ Constants.ExchangeRateTypes.Code.C09Rate,ExchangeRateType.C09 },
				{ Constants.ExchangeRateTypes.Code.C10Rate,ExchangeRateType.C10 },
				{ Constants.ExchangeRateTypes.Code.C11Rate,ExchangeRateType.C11 },
				{ Constants.ExchangeRateTypes.Code.C12Rate,ExchangeRateType.C12 },
				{ Constants.ExchangeRateTypes.Code.C13Rate,ExchangeRateType.C13 },
				{ Constants.ExchangeRateTypes.Code.C14Rate,ExchangeRateType.C14 },
				{ Constants.ExchangeRateTypes.Code.C15Rate,ExchangeRateType.C15 },
				{ Constants.ExchangeRateTypes.Code.C16Rate,ExchangeRateType.C16 },
				{ Constants.ExchangeRateTypes.Code.C17Rate,ExchangeRateType.C17 },
				{ Constants.ExchangeRateTypes.Code.C18Rate,ExchangeRateType.C18 },
				{ Constants.ExchangeRateTypes.Code.C19Rate,ExchangeRateType.C19 },
				{ Constants.ExchangeRateTypes.Code.C20Rate,ExchangeRateType.C20 },
				{ Constants.ExchangeRateTypes.Code.C21Rate,ExchangeRateType.C21 },
				{ Constants.ExchangeRateTypes.Code.C22Rate,ExchangeRateType.C22 },
				{ Constants.ExchangeRateTypes.Code.C23Rate,ExchangeRateType.C23 },
				{ Constants.ExchangeRateTypes.Code.C24Rate,ExchangeRateType.C24 },
				{ Constants.ExchangeRateTypes.Code.C25Rate,ExchangeRateType.C25 },
				{ Constants.ExchangeRateTypes.Code.C26Rate,ExchangeRateType.C26 },
				{ Constants.ExchangeRateTypes.Code.C27Rate,ExchangeRateType.C27 },
				{ Constants.ExchangeRateTypes.Code.C28Rate,ExchangeRateType.C28 },
				{ Constants.ExchangeRateTypes.Code.C29Rate,ExchangeRateType.C29 },
				{ Constants.ExchangeRateTypes.Code.C30Rate,ExchangeRateType.C30 },
				{ Constants.ExchangeRateTypes.Code.C31Rate,ExchangeRateType.C31 },
				{ Constants.ExchangeRateTypes.Code.C32Rate,ExchangeRateType.C32 },
				{ Constants.ExchangeRateTypes.Code.C33Rate,ExchangeRateType.C33 },
				{ Constants.ExchangeRateTypes.Code.C34Rate,ExchangeRateType.C34 },
				{ Constants.ExchangeRateTypes.Code.C35Rate,ExchangeRateType.C35 },
				{ Constants.ExchangeRateTypes.Code.C36Rate,ExchangeRateType.C36 },
				{ Constants.ExchangeRateTypes.Code.C37Rate,ExchangeRateType.C37 },
				{ Constants.ExchangeRateTypes.Code.C38Rate,ExchangeRateType.C38 },
				{ Constants.ExchangeRateTypes.Code.C39Rate,ExchangeRateType.C39 },
				{ Constants.ExchangeRateTypes.Code.C40Rate,ExchangeRateType.C40 },
				{ Constants.ExchangeRateTypes.Code.C41Rate,ExchangeRateType.C41 },
				{ Constants.ExchangeRateTypes.Code.C42Rate,ExchangeRateType.C42 },
				{ Constants.ExchangeRateTypes.Code.C43Rate,ExchangeRateType.C43 },
				{ Constants.ExchangeRateTypes.Code.C44Rate,ExchangeRateType.C44 },
				{ Constants.ExchangeRateTypes.Code.C45Rate,ExchangeRateType.C45 },
				{ Constants.ExchangeRateTypes.Code.C46Rate,ExchangeRateType.C46 },
				{ Constants.ExchangeRateTypes.Code.C47Rate,ExchangeRateType.C47 },
				{ Constants.ExchangeRateTypes.Code.C48Rate,ExchangeRateType.C48 },
				{ Constants.ExchangeRateTypes.Code.C49Rate,ExchangeRateType.C49 },
				{ Constants.ExchangeRateTypes.Code.C50Rate,ExchangeRateType.C50 },
				{ Constants.ExchangeRateTypes.Code.C51Rate,ExchangeRateType.C51 },
				{ Constants.ExchangeRateTypes.Code.C52Rate,ExchangeRateType.C52 },
				{ Constants.ExchangeRateTypes.Code.C53Rate,ExchangeRateType.C53 },
				{ Constants.ExchangeRateTypes.Code.C54Rate,ExchangeRateType.C54 },
				{ Constants.ExchangeRateTypes.Code.C55Rate,ExchangeRateType.C55 },
				{ Constants.ExchangeRateTypes.Code.C56Rate,ExchangeRateType.C56 },
				{ Constants.ExchangeRateTypes.Code.C57Rate,ExchangeRateType.C57 },
				{ Constants.ExchangeRateTypes.Code.C58Rate,ExchangeRateType.C58 },
				{ Constants.ExchangeRateTypes.Code.C59Rate,ExchangeRateType.C59 },
				{ Constants.ExchangeRateTypes.Code.C60Rate,ExchangeRateType.C60 },
				{ Constants.ExchangeRateTypes.Code.C61Rate,ExchangeRateType.C61 },
				{ Constants.ExchangeRateTypes.Code.C62Rate,ExchangeRateType.C62 },
				{ Constants.ExchangeRateTypes.Code.C63Rate,ExchangeRateType.C63 },
				{ Constants.ExchangeRateTypes.Code.C64Rate,ExchangeRateType.C64 },
				{ Constants.ExchangeRateTypes.Code.C65Rate,ExchangeRateType.C65 },
				{ Constants.ExchangeRateTypes.Code.C66Rate,ExchangeRateType.C66 },
				{ Constants.ExchangeRateTypes.Code.C67Rate,ExchangeRateType.C67 },
				{ Constants.ExchangeRateTypes.Code.C68Rate,ExchangeRateType.C68 },
				{ Constants.ExchangeRateTypes.Code.C69Rate,ExchangeRateType.C69 },
				{ Constants.ExchangeRateTypes.Code.C70Rate,ExchangeRateType.C70 },
				{ Constants.ExchangeRateTypes.Code.C71Rate,ExchangeRateType.C71 },
				{ Constants.ExchangeRateTypes.Code.C72Rate,ExchangeRateType.C72 },
				{ Constants.ExchangeRateTypes.Code.C73Rate,ExchangeRateType.C73 },
				{ Constants.ExchangeRateTypes.Code.C74Rate,ExchangeRateType.C74 },
				{ Constants.ExchangeRateTypes.Code.C75Rate,ExchangeRateType.C75 },
				{ Constants.ExchangeRateTypes.Code.C76Rate,ExchangeRateType.C76 },
				{ Constants.ExchangeRateTypes.Code.C77Rate,ExchangeRateType.C77 },
				{ Constants.ExchangeRateTypes.Code.C78Rate,ExchangeRateType.C78 },
				{ Constants.ExchangeRateTypes.Code.C79Rate,ExchangeRateType.C79 },
				{ Constants.ExchangeRateTypes.Code.C80Rate,ExchangeRateType.C80 },
				{ Constants.ExchangeRateTypes.Code.C81Rate,ExchangeRateType.C81 },
				{ Constants.ExchangeRateTypes.Code.C82Rate,ExchangeRateType.C82 },
				{ Constants.ExchangeRateTypes.Code.C83Rate,ExchangeRateType.C83 },
				{ Constants.ExchangeRateTypes.Code.C84Rate,ExchangeRateType.C84 },
				{ Constants.ExchangeRateTypes.Code.C85Rate,ExchangeRateType.C85 },
				{ Constants.ExchangeRateTypes.Code.C86Rate,ExchangeRateType.C86 },
				{ Constants.ExchangeRateTypes.Code.C87Rate,ExchangeRateType.C87 },
				{ Constants.ExchangeRateTypes.Code.C88Rate,ExchangeRateType.C88 },
				{ Constants.ExchangeRateTypes.Code.C89Rate,ExchangeRateType.C89 },
				{ Constants.ExchangeRateTypes.Code.C90Rate,ExchangeRateType.C90 },
				{ Constants.ExchangeRateTypes.Code.C91Rate,ExchangeRateType.C91 },
				{ Constants.ExchangeRateTypes.Code.C92Rate,ExchangeRateType.C92 },
				{ Constants.ExchangeRateTypes.Code.C93Rate,ExchangeRateType.C93 },
				{ Constants.ExchangeRateTypes.Code.C94Rate,ExchangeRateType.C94 },
				{ Constants.ExchangeRateTypes.Code.C95Rate,ExchangeRateType.C95 },
				{ Constants.ExchangeRateTypes.Code.C96Rate,ExchangeRateType.C96 },
				{ Constants.ExchangeRateTypes.Code.C97Rate,ExchangeRateType.C97 },
				{ Constants.ExchangeRateTypes.Code.C98Rate,ExchangeRateType.C98 },
				{ Constants.ExchangeRateTypes.Code.C99Rate,ExchangeRateType.C99 },
				{ Constants.ExchangeRateTypes.Code.L01Rate,ExchangeRateType.L01 },
				{ Constants.ExchangeRateTypes.Code.L02Rate,ExchangeRateType.L02 },
				{ Constants.ExchangeRateTypes.Code.L03Rate,ExchangeRateType.L03 },
				{ Constants.ExchangeRateTypes.Code.L04Rate,ExchangeRateType.L04 },
				{ Constants.ExchangeRateTypes.Code.L05Rate,ExchangeRateType.L05 },
				{ Constants.ExchangeRateTypes.Code.L06Rate,ExchangeRateType.L06 },
				{ Constants.ExchangeRateTypes.Code.L07Rate,ExchangeRateType.L07 },
				{ Constants.ExchangeRateTypes.Code.L08Rate,ExchangeRateType.L08 },
				{ Constants.ExchangeRateTypes.Code.L09Rate,ExchangeRateType.L09 },
				{ Constants.ExchangeRateTypes.Code.L10Rate,ExchangeRateType.L10 },
				{ Constants.ExchangeRateTypes.Code.L11Rate,ExchangeRateType.L11 },
				{ Constants.ExchangeRateTypes.Code.L12Rate,ExchangeRateType.L12 },
				{ Constants.ExchangeRateTypes.Code.L13Rate,ExchangeRateType.L13 },
				{ Constants.ExchangeRateTypes.Code.L14Rate,ExchangeRateType.L14 },
				{ Constants.ExchangeRateTypes.Code.L15Rate,ExchangeRateType.L15 },
				{ Constants.ExchangeRateTypes.Code.L16Rate,ExchangeRateType.L16 },
				{ Constants.ExchangeRateTypes.Code.L17Rate,ExchangeRateType.L17 },
				{ Constants.ExchangeRateTypes.Code.L18Rate,ExchangeRateType.L18 },
				{ Constants.ExchangeRateTypes.Code.L19Rate,ExchangeRateType.L19 },
				{ Constants.ExchangeRateTypes.Code.L20Rate,ExchangeRateType.L20 },
				{ Constants.ExchangeRateTypes.Code.L21Rate,ExchangeRateType.L21 },
				{ Constants.ExchangeRateTypes.Code.L22Rate,ExchangeRateType.L22 },
				{ Constants.ExchangeRateTypes.Code.L23Rate,ExchangeRateType.L23 },
				{ Constants.ExchangeRateTypes.Code.L24Rate,ExchangeRateType.L24 },
				{ Constants.ExchangeRateTypes.Code.L25Rate,ExchangeRateType.L25 },
				{ Constants.ExchangeRateTypes.Code.L26Rate,ExchangeRateType.L26 },
				{ Constants.ExchangeRateTypes.Code.L27Rate,ExchangeRateType.L27 },
				{ Constants.ExchangeRateTypes.Code.L28Rate,ExchangeRateType.L28 },
				{ Constants.ExchangeRateTypes.Code.L29Rate,ExchangeRateType.L29 },
				{ Constants.ExchangeRateTypes.Code.L30Rate,ExchangeRateType.L30 },
				{ Constants.ExchangeRateTypes.Code.L31Rate,ExchangeRateType.L31 },
				{ Constants.ExchangeRateTypes.Code.L32Rate,ExchangeRateType.L32 },
				{ Constants.ExchangeRateTypes.Code.L33Rate,ExchangeRateType.L33 },
				{ Constants.ExchangeRateTypes.Code.L34Rate,ExchangeRateType.L34 },
				{ Constants.ExchangeRateTypes.Code.L35Rate,ExchangeRateType.L35 },
				{ Constants.ExchangeRateTypes.Code.L36Rate,ExchangeRateType.L36 },
				{ Constants.ExchangeRateTypes.Code.L37Rate,ExchangeRateType.L37 },
				{ Constants.ExchangeRateTypes.Code.L38Rate,ExchangeRateType.L38 },
				{ Constants.ExchangeRateTypes.Code.L39Rate,ExchangeRateType.L39 },
				{ Constants.ExchangeRateTypes.Code.L40Rate,ExchangeRateType.L40 },
				{ Constants.ExchangeRateTypes.Code.L41Rate,ExchangeRateType.L41 },
				{ Constants.ExchangeRateTypes.Code.L42Rate,ExchangeRateType.L42 },
				{ Constants.ExchangeRateTypes.Code.L43Rate,ExchangeRateType.L43 },
				{ Constants.ExchangeRateTypes.Code.L44Rate,ExchangeRateType.L44 },
				{ Constants.ExchangeRateTypes.Code.L45Rate,ExchangeRateType.L45 },
				{ Constants.ExchangeRateTypes.Code.L46Rate,ExchangeRateType.L46 },
				{ Constants.ExchangeRateTypes.Code.L47Rate,ExchangeRateType.L47 },
				{ Constants.ExchangeRateTypes.Code.L48Rate,ExchangeRateType.L48 },
				{ Constants.ExchangeRateTypes.Code.L49Rate,ExchangeRateType.L49 },
				{ Constants.ExchangeRateTypes.Code.L50Rate,ExchangeRateType.L50 },
				{ Constants.ExchangeRateTypes.Code.L51Rate,ExchangeRateType.L51 },
				{ Constants.ExchangeRateTypes.Code.L52Rate,ExchangeRateType.L52 },
				{ Constants.ExchangeRateTypes.Code.L53Rate,ExchangeRateType.L53 },
				{ Constants.ExchangeRateTypes.Code.L54Rate,ExchangeRateType.L54 },
				{ Constants.ExchangeRateTypes.Code.L55Rate,ExchangeRateType.L55 },
				{ Constants.ExchangeRateTypes.Code.L56Rate,ExchangeRateType.L56 },
				{ Constants.ExchangeRateTypes.Code.L57Rate,ExchangeRateType.L57 },
				{ Constants.ExchangeRateTypes.Code.L58Rate,ExchangeRateType.L58 },
				{ Constants.ExchangeRateTypes.Code.L59Rate,ExchangeRateType.L59 },
				{ Constants.ExchangeRateTypes.Code.L60Rate,ExchangeRateType.L60 },
				{ Constants.ExchangeRateTypes.Code.L61Rate,ExchangeRateType.L61 },
				{ Constants.ExchangeRateTypes.Code.L62Rate,ExchangeRateType.L62 },
				{ Constants.ExchangeRateTypes.Code.L63Rate,ExchangeRateType.L63 },
				{ Constants.ExchangeRateTypes.Code.L64Rate,ExchangeRateType.L64 },
				{ Constants.ExchangeRateTypes.Code.L65Rate,ExchangeRateType.L65 },
				{ Constants.ExchangeRateTypes.Code.L66Rate,ExchangeRateType.L66 },
				{ Constants.ExchangeRateTypes.Code.L67Rate,ExchangeRateType.L67 },
				{ Constants.ExchangeRateTypes.Code.L68Rate,ExchangeRateType.L68 },
				{ Constants.ExchangeRateTypes.Code.L69Rate,ExchangeRateType.L69 },
				{ Constants.ExchangeRateTypes.Code.L70Rate,ExchangeRateType.L70 },
				{ Constants.ExchangeRateTypes.Code.L71Rate,ExchangeRateType.L71 },
				{ Constants.ExchangeRateTypes.Code.L72Rate,ExchangeRateType.L72 },
				{ Constants.ExchangeRateTypes.Code.L73Rate,ExchangeRateType.L73 },
				{ Constants.ExchangeRateTypes.Code.L74Rate,ExchangeRateType.L74 },
				{ Constants.ExchangeRateTypes.Code.L75Rate,ExchangeRateType.L75 },
				{ Constants.ExchangeRateTypes.Code.L76Rate,ExchangeRateType.L76 },
				{ Constants.ExchangeRateTypes.Code.L77Rate,ExchangeRateType.L77 },
				{ Constants.ExchangeRateTypes.Code.L78Rate,ExchangeRateType.L78 },
				{ Constants.ExchangeRateTypes.Code.L79Rate,ExchangeRateType.L79 },
				{ Constants.ExchangeRateTypes.Code.L80Rate,ExchangeRateType.L80 },
				{ Constants.ExchangeRateTypes.Code.L81Rate,ExchangeRateType.L81 },
				{ Constants.ExchangeRateTypes.Code.L82Rate,ExchangeRateType.L82 },
				{ Constants.ExchangeRateTypes.Code.L83Rate,ExchangeRateType.L83 },
				{ Constants.ExchangeRateTypes.Code.L84Rate,ExchangeRateType.L84 },
				{ Constants.ExchangeRateTypes.Code.L85Rate,ExchangeRateType.L85 },
				{ Constants.ExchangeRateTypes.Code.L86Rate,ExchangeRateType.L86 },
				{ Constants.ExchangeRateTypes.Code.L87Rate,ExchangeRateType.L87 },
				{ Constants.ExchangeRateTypes.Code.L88Rate,ExchangeRateType.L88 },
				{ Constants.ExchangeRateTypes.Code.L89Rate,ExchangeRateType.L89 },
				{ Constants.ExchangeRateTypes.Code.L90Rate,ExchangeRateType.L90 },
				{ Constants.ExchangeRateTypes.Code.L91Rate,ExchangeRateType.L91 },
				{ Constants.ExchangeRateTypes.Code.L92Rate,ExchangeRateType.L92 },
				{ Constants.ExchangeRateTypes.Code.L93Rate,ExchangeRateType.L93 },
				{ Constants.ExchangeRateTypes.Code.L94Rate,ExchangeRateType.L94 },
				{ Constants.ExchangeRateTypes.Code.L95Rate,ExchangeRateType.L95 },
				{ Constants.ExchangeRateTypes.Code.L96Rate,ExchangeRateType.L96 },
				{ Constants.ExchangeRateTypes.Code.L97Rate,ExchangeRateType.L97 },
				{ Constants.ExchangeRateTypes.Code.L98Rate,ExchangeRateType.L98 },
				{ Constants.ExchangeRateTypes.Code.L99Rate,ExchangeRateType.L99 },
			};
			return rateType != null && exchangeRateTypeList.TryGetValue(rateType, out var value) ? value : ExchangeRateType.All;
		}

		#region Implementation

		protected readonly bool IsReciprocal;
		protected readonly int LocalCurrencyDecimals;
		protected readonly Guid CompanyPK;

		public virtual int RateDecimals
		{
			get { return 6; }
		}

		protected decimal ConvertedAmount(decimal amount, decimal exchangeRate, bool divide, int decimals, bool shouldRound)
		{
			decimal result = 0;

			try
			{
				if (exchangeRate != 0)
				{
					result = divide ? amount / exchangeRate : amount * exchangeRate;
					result = shouldRound ? Utilities.Round(result, decimals) : result;
				}
			}
			catch (OverflowException)
			// catch and handle the exception for very rare cases that the inputed Amount value is actually too large which will cause this Overflow exception
			// See issue 00915332
			{
				result = -1;
			}

			return result;
		}

		protected virtual DateTime CurrentLocalDateTime
		{
			get { return EnvProxy.Instance.Time.CurrentLocalDateTime; }
		}

		#endregion
	}
}
