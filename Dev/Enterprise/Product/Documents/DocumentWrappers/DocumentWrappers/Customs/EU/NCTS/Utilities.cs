using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.MasterFiles.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS
{
	public static class Tools
	{
		public static ZString ValueOrThreeDashes(IZType potentialValue)
		{
			return potentialValue.IsEmpty ? "---" : potentialValue.ToString();
		}

		internal static ZString GetFormattedDecimal(ZString value)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0:0.##}", ZDecimal.ParseSafe(value, ZDecimal.Zero));
		}

		public static ZString GetNotReleasedCaption() => Res.GetString("874645E5-0493-4B70-A345-F26826E9F853", "NOT RELEASED");

		public static ZString ToEuShortDateString(this ZDateTime date)
		{
			const string euShortDateFormat = "dd/MM/yyyy";

			return date.ToString(euShortDateFormat, CultureInfo.InvariantCulture);
		}

		public static decimal? NullIfZero(this ZDecimal value)
		{
			return value.IsEmpty ? null : new decimal?(value);
		}

		public static int? NullIfZero(this ZInt value)
		{
			return value.IsEmpty ? null : new int?(value);
		}

		public static string ConcatenateWithLengthLimit(string[] items, string delimiter, int maxLength)
		{
			var fullString = string.Join(delimiter, items);
			if (fullString.Length <= maxLength)
			{
				return fullString;
			}

			var threeDots = "(...)";
			var ellipsis = $"{delimiter}{threeDots}{delimiter}";

			var leftIndex = 0;
			var rightIndex = items.Length - 1;
			var leftPart = items[leftIndex];
			var rightPart = items[rightIndex];

			var latestValidReturnString = leftPart + delimiter + threeDots;

			if (latestValidReturnString.Length > maxLength)
			{
				return threeDots;
			}

			while (leftIndex + 1 < rightIndex)
			{
				var tempString = leftPart + ellipsis + rightPart;
				if (tempString.Length <= maxLength)
				{
					latestValidReturnString = tempString;
					leftPart += delimiter + items[++leftIndex];
				}
				else
				{
					break;
				}
			}

			return latestValidReturnString;
		}

		public static ZString ConvertNumberToString(IZType number) => number?.ToString() ?? ZString.Empty;
	}

	public static class ConsecutiveSequenceCondenser
	{
		public static string Condense(IEnumerable<ZString> input, string outputSeparator = ", ")
		{
			if (!input.Any())
			{
				return "";
			}

			bool isTotallySequential = true;
			var nonBlank = (from ZString s in input where !string.IsNullOrEmpty(s) select s.Trim()).Distinct();
			var longestString = (from s in nonBlank orderby s.Length select s).LastOrDefault();
			var paddeds = (from s in nonBlank orderby s.PadLeft(longestString.Length, ' ') select s.PadLeft(longestString.Length, ' ')).ToArray();
			var result = new List<string>();

			if (SequenceIsEntirelyIntegers(paddeds))
			{
				long[] allIntegers = (from long l in paddeds.Select(x => long.Parse(x)) orderby l select l).ToArray();
				for (long i = 0; i < allIntegers.Length - 1; i++)
				{
					if (allIntegers[i] + 1 != allIntegers[i + 1])
					{
						isTotallySequential = false;
						break;
					}
				}
				if (isTotallySequential)
				{
					return allIntegers.First().ToString() + "-" + allIntegers.Last().ToString();
				}
			}
			else  // some strings
			{
				int i = 0;
				while (i < paddeds.Length - 1)
				{
					if (AreSequentialStrings(paddeds[i], paddeds[i + 1]))
					{
						i++;
					}
					else
					{
						isTotallySequential = false;
						break;
					}
				}
				if (isTotallySequential)
				{
					var first = paddeds.First();
					var last = paddeds.Last();
					var commonPrefix = GetCommonSubstringPrefix(first, last);
					return string.IsNullOrEmpty(commonPrefix)
							? first + "-" + last
							: commonPrefix + first.Replace(commonPrefix, "") + "-" + last.Replace(commonPrefix, "");
				}
			}

			return string.Join(outputSeparator, nonBlank); // Just send back the distinct values that were sent in			
		}

		static bool SequenceIsEntirelyIntegers(ZString[] input)
		{
			long throwAway = 0;
			return !(from ZString s in input where !long.TryParse(s, out throwAway) select s).Any();
		}

		static string GetCommonSubstringPrefix(string a, string b)
		{
			var aChars = a.ToCharArray();
			var bChars = b.ToCharArray();
			var indexWhereCharsDiffer = -1;
			for (int i = 0; i < aChars.Length; i++)
			{
				if (aChars[i] != bChars[i])
				{
					indexWhereCharsDiffer = i;
					break;
				}
			}
			return indexWhereCharsDiffer > -1 ? a.Substring(0, indexWhereCharsDiffer) : "";
		}

		static bool AreSequentialStrings(string a, string b)
		{
			var isSequential = false;
			var aChars = a.ToCharArray();
			var bChars = b.ToCharArray();
			var indexWhereCharsDiffer = -1;
			for (int i = 0; i < aChars.Length; i++)
			{
				if (aChars[i] != bChars[i])
				{
					indexWhereCharsDiffer = i;
					break;
				}
			}
			if (indexWhereCharsDiffer > -1)// && indexWhereCharsDiffer==aChars.Length-1)
			{
				if (bChars[indexWhereCharsDiffer] == aChars[indexWhereCharsDiffer] + 1)
				{
					isSequential = true;
				}
			}
			return isSequential;
		}
	}

	public static class OrgAddressExtension
	{
		internal static string MultiLine(this OrgAddress address, params Func<OrgAddress, string>[] getters)
		{
			var result = new ZStringBuilder();
			if (address is OrgAddress orgAddress)
			{
				var addressType = address.GetType();
				foreach (var getter in getters)
				{
					result.AppendIfNotEmpty(getter.Invoke(address));
				}
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		internal static string MultiLine(this OrgAddress address) => address.MultiLine(
			a => a.Address1,
			a => a.Address2,
			a => a.City,
			a => a.Postcode,
			a => a.Country?.Code ?? string.Empty
		);

		public static ZString GetStreetAndNumber(this OrgAddress address) => address == null ? ZString.Empty : new ZString(string.Join(DocumentWrapperConstants.Delimiters.Space, address.Address1, address.Address2).Trim());
	}

	public static class TraderExtension
	{
		public static ZString GetWrappedAddress(this ITrader address)
		{
			if (address is null)
			{
				return ZString.Empty;
			}
			return ZString.Format("{0}\n{1}\n{2} {3}\n{4}", address.CompanyName, address.StreetAndNumber, address.PostalCode, address.City, address.CountryCode);
		}

		internal static ZString GetWrappedAddressSummary(this ITrader address, string streetAndNumber = null)
		{
			if (address is null)
			{
				return ZString.Empty;
			}
			return ZString.Format("{0}, {1}, {2} {3}, {4}", address.Name, streetAndNumber ?? address.StreetAndNumber, address.PostalCode, address.City, address.CountryCode);
		}
	}

	public static class GuaranteeExtension
	{
		public static ZString ToStringCertainNumberGuarantees(this IGuarantee[] guarantees, ZInt numberOfGuarantees)
		{
			if (guarantees is null)
			{
				return ZString.Empty;
			}

			var sb = new ZStringBuilder();
			for (int i = 0; i < numberOfGuarantees && i < guarantees.Length; i++)
			{
				var guarantee = guarantees[i];

				var guaranteeReferenceNumber = guarantee.GuaranteeReferenceNumber;
				var value = guaranteeReferenceNumber.IsEmpty ? guarantee.OtherGuaranteeReference : guaranteeReferenceNumber;
				sb.AppendIfNotEmpty(value);
			}
			return sb.ToStringWithDelimiterBetweenAppends(";").TrimEnd();
		}

		public static ZString ToStringThreeFirstGuaranteeValidities(this IGuarantee[] guarantees)
		{
			if (guarantees is null)
			{
				return ZString.Empty;
			}

			var sb = new ZStringBuilder();
			for (int i = 0; i < 3 && i < guarantees.Length; i++)
			{
				foreach (var deniedParty in guarantees[i].NotValidForOtherContractingParties)
				{
					sb.AppendIfNotEmpty(deniedParty);
				}
			}
			return new ZString(sb.ToStringWithDelimiterBetweenAppends(",")).Left(NctsGuarantee.Schema.PW_ValidityLimitationMaxLength);
		}

		public static ZString ToStringThreeFirstGuaranteeCodes(this IGuarantee[] guarantees) => ToStringCertainNumberGuaranteeCodes(guarantees, 3);

		public static ZString ToStringCertainNumberGuaranteeCodes(this IGuarantee[] guarantees, ZInt numberOfGuarantees)
		{
			if (guarantees is null)
			{
				return ZString.Empty;
			}

			var sb = new ZStringBuilder();
			for (var i = 0; i < numberOfGuarantees && i < guarantees.Length; i++)
			{
				sb.AppendIfNotEmpty(guarantees[i].GuaranteeType);
			}
			return sb.ToStringWithDelimiterBetweenAppends(",");
		}
	}

	public static class JobDocAddressExtension
	{
		public static ZString FormatOnThreeLines(this JobDocAddress address, bool addContact = false)
		{
			var sb = new ZStringBuilder();
			sb.Append(FormatAddress(address, DocumentWrapperConstants.Delimiters.CarriageReturn));

			if (addContact)
			{
				sb.AppendLine();
				sb.Append(FormatAddressContactOneLine(address));
			}

			return sb.ToString();
		}

		public static ZString FormatContactOnOneLine(this JobDocAddress address, bool takeContactIfNoOverride = false)
		{
			return FormatAddressContactOneLine(address, takeContactIfNoOverride);
		}

		public static ZString GetStreetAndNumber(this JobDocAddress address) => address == null ? ZString.Empty : new ZString(string.Join(DocumentWrapperConstants.Delimiters.Space, address.Address1, address.Address2).Trim());

		internal static ZString FormatAddress(JobDocAddress address, string elementsDelimiter)
		{
			if (address == null)
			{
				return ZString.Empty;
			}

			var builder = new ZStringBuilder();

			if (address.E2_AddressOverride)
			{
				builder.Append(address.E2_CompanyName);
				builder.Append(address.GetStreetAndNumber());
				builder.Append($"{address.E2_Postcode} {address.E2_City} {address.E2_RN_NKCountryCode}");
			}
			else
			{
				builder.Append(address.Organisation?.OH_FullName ?? ZString.Empty);
				var organisationAddress = address.Organisation?.Addresses.OfType<OrgAddress>().FirstOrDefault(a => a.OA_IsActive);
				builder.Append(organisationAddress.GetStreetAndNumber());
				builder.Append($"{organisationAddress?.Postcode ?? ZString.Empty} {organisationAddress?.OA_City ?? ZString.Empty} {organisationAddress?.OA_RN_NKCountryCode ?? ZString.Empty}");
			}
			return builder.ToStringWithDelimiterBetweenAppends(elementsDelimiter);
		}

		internal static ZString FormatAddressContactOneLine(JobDocAddress address, bool takeContactIfNoOverride = false)
		{
			if (address == null)
			{
				return ZString.Empty;
			}

			var builder = new ZStringBuilder();

			if (address.E2_AddressOverride)
			{
				builder.Append(address.E2_Contact);
				builder.Append(address.E2_Phone);
				builder.Append(address.E2_Email);
			}
			else
			{
				if (takeContactIfNoOverride && !address.E2_Contact.IsEmpty)
				{
					builder.Append(address.E2_Contact);
					builder.Append(address.E2_Phone);
					builder.Append(address.E2_Email);
				}
				else
				{
					var organisationContact = address.Organisation?.Contacts.OfType<OrgContact>().FirstOrDefault(c => c.OC_IsActive);
					builder.Append(organisationContact?.OC_ContactName ?? string.Empty);
					builder.Append(organisationContact?.OC_Phone ?? string.Empty);
					builder.Append(organisationContact?.OC_Email ?? string.Empty);
				}
			}

			return builder.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.Space);
		}
	}
}
