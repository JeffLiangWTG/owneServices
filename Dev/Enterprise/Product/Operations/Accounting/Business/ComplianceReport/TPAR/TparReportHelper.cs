using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ComplianceReport.TPAR
{
	public static class TparReportHelper
	{
		public static string Truncate(string value, int maxChars)
		{
			value = value.Length <= maxChars ? value : value.Substring(0, maxChars);

			ZString zValue = value;
			if (zValue.IsEnglishOnlyOrEmpty)
			{
				return value;
			}

			string normalizedString = value.Normalize(NormalizationForm.FormD);
			var stringBuilder = new StringBuilder(capacity: value.Length);

			int subStrStart = 0;
			int subStrEnd = 0;
			for (; subStrEnd < normalizedString.Length; ++subStrEnd)
			{
				char ch = normalizedString[subStrEnd];
				if (!(ch >= 32 && ch <= 127) || (ch >= 9 && ch <= 13))
				{
					stringBuilder.Append(normalizedString.Substring(subStrStart, subStrEnd - subStrStart));
					if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
					{
						stringBuilder.Append('0');
					}
					subStrStart = subStrEnd + 1;
				}
			}
			stringBuilder.Append(normalizedString.Substring(subStrStart, subStrEnd - subStrStart));

			string normalizedValue = stringBuilder.ToString();
			return normalizedValue.Length <= maxChars ? normalizedValue : normalizedValue.Substring(0, maxChars);
		}

		public static string GetState(string state, string countryCode)
		{
			var result = state;

			if (countryCode == Core.Constants.CountryCodes.NorfolkIsland)
			{
				result = "NSW";
			}
			else if (countryCode != Core.Constants.CountryCodes.Australia)
			{
				result = OverseasState;
			}
			return result;
		}

		public static string GetPostCode(string postCode, string countryCode) => (countryCode == Core.Constants.CountryCodes.Australia
			|| countryCode == Core.Constants.CountryCodes.NorfolkIsland) ? postCode : OverseasPostCode;

		public static string GetCity(string city, string postCode, string state, string countryCode) => (countryCode == Core.Constants.CountryCodes.Australia
			|| countryCode == Core.Constants.CountryCodes.NorfolkIsland) ? city : string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", city, state, postCode);

		public static string FormatDecimal(ZDecimal value, int maxChars) => value.ToZInt().ToString().PadLeft(maxChars, '0');

		public static string GetFinancialYear(AccComplianceReport complianceReport) => complianceReport.UseFinancialYear
			? complianceReport.AccountingPeriod.ToString()
			: complianceReport.UseAccountingPeriod
				? (complianceReport.AccountingPeriod / 100).ToString()
				: complianceReport.ACR_DateTo.Year.ToString();

		public static string GetAustralianBusinessNumber(OrgHeader org) => GetNumbersOnlyAustralianBusinessNumber(org?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber,
				Core.Constants.CountryCodes.Australia)?.OK_CustomsRegNo.KeepChars(ABNValidation.ValidChars) ?? string.Empty);

		public static ZString GetNumbersOnlyAustralianBusinessNumber(ZString abn) => abn.KeepChars(ABNValidation.ValidChars);

		public static string FormatAustralianBusinessNumber(ZString abn) => GetNumbersOnlyAustralianBusinessNumber(abn).Left(11).PadLeft(11, '0');

		public static string GetCountryName(RefCountry country) => country.RN_Code == Core.Constants.CountryCodes.NorfolkIsland ? ZString.Empty : country.RN_Desc;

		public static bool GetStatementBySupplierProvidedByPayee(OrgHeader org, AccComplianceReport complianceReport)
		{
			return org.RequiredDocuments.Cast<JobRequiredDocument>().Any(x =>
				x.EQ_DocCategory == ReferenceTypes.ClientSupplierRelationship
				&& x.EQ_DocType == RefDocTypes.WithholdingTaxExemption
				&& x.EQ_RN_NKRelatedCountry == complianceReport.Company.GC_RN_NKCountryCode
				&& x.EQ_ValidToDate >= complianceReport.ACR_DateFrom
				&& x.EQ_DateReceived.ToZDateTime() < complianceReport.ACR_DateTo.AddDays(1));
		}

		public static string OverseasPostCode => "9999";
		public static string OverseasState => "OTH";
		public static int RecordLength => 996;
		public static string RecordHeader => RecordLength.ToString(CultureInfo.InvariantCulture);
		public static string FixWidth(string value, int width, bool padWithZeros = false) => Truncate(value, width).PadRight(width, padWithZeros ? '0' : ' ');
	}
}
