using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.BR.Business.Constants;

namespace Enterprise.Customs.BR.Business
{
	public static class Extensions
	{
		public static ZString GetCNPJOrCPF(this OrgHeader org) => org?.PrimaryRegistrationNumber?.Number.KeepNumericCharacters() ?? ZString.Empty;

		public static ZString GetRootCNPJ(this OrgHeader org) => org?.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ) ?? ZString.Empty;

		public static ZString GetRootCNPJFromCNPJ(this OrgHeader org) => org?.PrimaryRegistrationNumber?.NumberType.ToString() switch
		{
			BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration => org.GetCNPJOrCPF().SubstringSafe(0, 11),
			BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ => org.GetCNPJOrCPF().SubstringSafe(0, 8),
			_ => ZString.Empty
		};

		public static ZString GetTinCode(this OrgHeader org) => org?.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.TIN) ?? ZString.Empty;

		public static ZString GetInternalCode(this OrgHeader org) => org?.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.ForeignOperatorInternalCode) ?? ZString.Empty;

		public static ZString GetForeignOperatorEmail(this OrgHeader org) => org?.Contacts?.GetContactForAllocation(OrgConstants.ContactAllocationType.BRForeignOperator)?.Email ?? ZString.Empty;

		public static ZDateTime GetNearestWorkingdayBefore(this ZDateTime date)
		{
			return WorkingDaysHelper.GetNearestWorkingdayBefore(date);
		}

		public static string ConvertToYesNoList(this ZBool value) => value ? YesNoList.Codes.Yes : YesNoList.Codes.No;

		public static ZString ConvertYesNoToBool(this ZString value) => value.ToString() switch
		{
			Profile.AnswerValues.Yes => true.ToString().ToLower(),
			Profile.AnswerValues.No => false.ToString().ToLower(),
			_ => ZString.Empty
		};

		public static string ConvertBoolToYesNo(this ZString value) => value == true.ToString().ToLower() ? Profile.AnswerValues.Yes : value == false.ToString().ToLower() ? Profile.AnswerValues.No : string.Empty;

		public static string CombineValuesAsString<T>(this IEnumerable<T> values, string multipleText, Func<T, string> format = null) where T : IZType
		{
			var result = string.Empty;

			if (values != null)
			{
				var nonEmptyValues = values.Where(x => !x.IsEmpty).ToList();
				if (nonEmptyValues.Any())
				{
					if (format == null)
					{
						format = x => x.ToString();
					}
					result = nonEmptyValues.AllSame() ? format(nonEmptyValues.First()) : multipleText;
				}
			}
			return result;
		}

		public static string ReturnNullIfEmpty(this ZString inputValue)
		{
			return inputValue.IsEmpty ? null : inputValue.ToString();
		}

		public static decimal? ReturnNullIfEmpty(this ZDecimal inputValue)
		{
			return inputValue.IsEmpty ? null : inputValue;
		}

		public static ZDecimal ParseToDecimal(this ZString input) => ZDecimal.ParseSafe(input, ZDecimal.Zero);

		public static (ZString DescInEnglish, ZString DescInPortugueseBrazil) GetInvoiceUQDescriptions(this BusinessObjectFactory factory, CodeDescriptionPairList invoiceUQList, string invoiceUQ)
		{
			var cachedDescriptions = factory.GetCachedValue("", () => new Dictionary<string, (ZString, ZString)>());
			if (!cachedDescriptions.ContainsKey(invoiceUQ))
			{
				ZString descInPt = invoiceUQList.GetMultilingualDescriptionFromCode(invoiceUQ)?.ToString(Core.SharedConstants.Languages.PortugueseBrazil);
				ZString descInEn = invoiceUQList.GetMultilingualDescriptionFromCode(invoiceUQ)?.ToString(Core.SharedConstants.Languages.English);

				cachedDescriptions.Add(invoiceUQ, (descInEn, descInPt));
			}
			return cachedDescriptions[invoiceUQ];
		}

		public static decimal GetTotalChargesAmountOnInvoiceLines(this IEnumerable<JobComInvoiceLine> invoiceLines, Func<JobComInvCharge, bool> predicate, RefCurrency currency = null)
		{
			var total = invoiceLines.Sum(x => x.GetAmountOnCharges(predicate, currency, roundToDestinationCurrencyDecimals: false));

			return currency == null ? decimal.Zero : Utilities.Round(total, currency.Decimals);
		}

		public static decimal GetTotalChargesAmountOnInvoiceLines(this IEnumerable<JobComInvoiceLine> invoiceLines, ZString chargeType, RefCurrency currency = null)
		{
			return invoiceLines.GetTotalChargesAmountOnInvoiceLines(x => x.J7_ChargeType == chargeType, currency);
		}

		public static RefCurrency GetFirstChargeCurrency(this IEnumerable<JobComInvoiceLine> invoiceLines, params ZString[] chargeTypes)
		{
			return invoiceLines.SelectMany(line => line.GetAllCharges(charge => chargeTypes.Contains(charge.J7_ChargeType) && !charge.J7_RX_NKCurrency.IsEmpty))
				.FirstOrDefault()?.Currency;
		}

		public static OrgHeader FindOrganizationByRootCNPJ(this BusinessObjectFactory factory, ZString rootCnpj)
		{
			return rootCnpj.IsEmpty ? null : OrgHeader.FindByOrgCusCode(factory, BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, rootCnpj, Core.Constants.CountryCodes.Brazil);
		}
	}
}
