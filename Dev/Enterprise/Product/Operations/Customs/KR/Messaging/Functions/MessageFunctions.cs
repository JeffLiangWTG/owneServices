using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	public static class MessageFunctions
	{
		public enum MessageFunctionCode { Extend, Amendment, Cancellation, Original, Addition, ReportCompletion }
		public enum MessageInterpretationMode { Email, FullView }

		public static ZString DeclarationNumberFormat(ZString inputdeclarationNumber)
		{
			return GetFormattedNumber(inputdeclarationNumber, new int[] { 0, 5, 7 });
		}

		public static ZString AnalysisNumberFormat(ZString analysisNumber)
		{
			return GetFormattedNumber(analysisNumber, new int[] { 0, 1, 3 });
		}

		public static ZString NoticeNumberFormat(ZString noticeNumber)
		{
			return GetFormattedNumber(noticeNumber, new int[] { 0, 4, 7, 9, 11, 12, 18 });
		}

		public static ZString HSCodeFormat(ZString hsCode)
		{
			var result = hsCode;
			if (hsCode.Length == 10)
			{
				result = hsCode.Substring(0, 4) + "." +
								hsCode.Substring(4, 2) + "-" +
								hsCode.Substring(6);
			}
			return result;
		}

		const string EntryNumberSeparator = "-";
		public static ZString GetFormattedEntryNumber(ZString unformattedEntryNumber, ZString referenceNumberType)
		{
			if (referenceNumberType == ReferenceNumberTypeList.Codes.IMP || referenceNumberType == ReferenceNumberTypeList.Codes.EXP)
			{
				return MessageFunctions.GetFormattedNumber(unformattedEntryNumber, new int[] { 0, 5, 7 }, EntryNumberSeparator);
			}
			else if (referenceNumberType == ReferenceNumberTypeList.Codes.CMN)
			{
				var positions = unformattedEntryNumber.Length == 15 ? new int[] { 0, 11 } : new int[] { 0, 11, 15 };
				return MessageFunctions.GetFormattedNumber(unformattedEntryNumber, positions, EntryNumberSeparator);
			}
			return unformattedEntryNumber;
		}
		public static ZString GenerateUnformattedEntryNumber(ZString entryNumber, int maxLength)
		{
			var tmpVal = entryNumber.Replace(EntryNumberSeparator, string.Empty);
			return tmpVal.Length > maxLength ? tmpVal.Substring(0, maxLength) : tmpVal;
		}

		public static ZString RequestDocumentNumber(ZString inputNumber)
		{
			return GetFormattedNumber(inputNumber, new int[] { 0, 3, 5, 7 });
		}

		public static ZString GetCustomsReceiptNumber(ZString inputNumber)
		{
			return MessageFunctions.GetFormattedNumber(inputNumber, new int[] { 0, 3, 5, 7, 13 });
		}

		public static ZString GetFormattedNumberByStatementType(string numberType, ZString unformattedStatementNumber)
		{
			var result = ZString.Empty;
			switch (numberType)
			{
				case StatementHeaderTypeList.Codes.Normal:
					result = MessageFunctions.GetFormattedNumber(unformattedStatementNumber, new int[] { 0, 3, 5, 7 });
					break;
				case StatementHeaderTypeList.Codes.NormalReport:
				case StatementHeaderTypeList.Codes.MonthlyReceipt:
				case StatementHeaderTypeList.Codes.IndividualCollectionReceipt:
					result = MessageFunctions.GetFormattedNumber(unformattedStatementNumber, new int[] { 0, 3, 5 });
					break;
				case StatementHeaderTypeList.Codes.Invoice:
					result = MessageFunctions.GetFormattedNumber(unformattedStatementNumber, new int[] { 0, 4, 7, 9, 11 });
					break;
				case StatementHeaderTypeList.Codes.CustomsDisbursementBill:
					result = MessageFunctions.GetFormattedNumber(unformattedStatementNumber, new int[] { 0, 4, 7, 9, 11, 12, 18 });
					break;
			}
			return result;
		}

		public static ZString GetFormattedNumber(ZString inputNumber, int[] positions, string separator = "-")
		{
			var result = inputNumber;
			if (inputNumber.Length > positions[positions.Length - 1])
			{
				var splitted = new ZStringBuilder();
				for (int index = 0; index < positions.Length - 1; index++)
				{
					var startingPosition = positions[index];
					var legnth = positions[index + 1] - positions[index];
					splitted.Append(inputNumber.SubstringSafe(startingPosition, legnth));
				}
				splitted.Append(inputNumber.SubstringSafe(positions[positions.Length - 1]));
				result = splitted.ToStringWithDelimiterBetweenAppends(separator);
			}
			return result;
		}

		/// <summary>
		/// Unipass ID usually starts with the first four letters of a company name. If the company name is shorter than four, then each missing letter is replaced by two *'s
		/// It always ends with 7 numbers.
		/// </summary>
		/// <returns></returns>
		public static ZString GetFormattedUnipassIDForOrganization(ZString inputString)
		{
			var result = inputString;
			if (!inputString.IsEmpty)
			{
				ZString numbersString = inputString.Right(7);
				ZString frontString = inputString.SubstringSafe(0, inputString.IndexOf(numbersString));

				if (!frontString.IsEmpty && !numbersString.IsEmpty)
				{
					result = frontString + '-' + GetFormattedNumber(numbersString, new int[] { 0, 1, 3, 4, 6 });
				}
			}
			return result.Trim();
		}

		public static ZString GetFormattedCustomsOfficeAndDivision(ZString customsOfficeAndDepartmentID)
		{
			var result = ZString.Empty;
			if (customsOfficeAndDepartmentID.Length == 5)
			{
				result = customsOfficeAndDepartmentID.Left(3) + "-" + customsOfficeAndDepartmentID.Right(2);
			}
			return result.Trim();
		}

		public static ZString GetCustomsOfficeAndDivision(BusinessObjectFactory factory, ZString customsOfficeAndDepartmentID)
		{
			var result = ZString.Empty;
			if (customsOfficeAndDepartmentID.Length == 5)
			{
				result = GetCustomsOffice(factory, customsOfficeAndDepartmentID.Left(3)) + " " + GetCustomsDepartment(factory, customsOfficeAndDepartmentID.Right(2));
			}
			return result.Trim();
		}

		public static ZString GetCustomsOffice(BusinessObjectFactory factory, ZString customsOffice)
		{
			var result = ZString.Empty;
			if (customsOffice.Length == 3)
			{
				result = GetRefCusCodeListDescription(factory, customsOffice, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			}
			return result;
		}

		public static ZString GetCustomsDepartment(BusinessObjectFactory factory, ZString customsDepartment)
		{
			var result = ZString.Empty;
			if (customsDepartment.Length == 2)
			{
				result = GetRefCusCodeListDescription(factory, customsDepartment, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment);
			}
			return result;
		}

		public static ZString GetRefCusCodeListDescription(BusinessObjectFactory factory, ZString zCode, ZString zType)
		{
			return GetRefCusCodeList(factory, zCode, zType)?.ZZD_Description ?? ZString.Empty;
		}

		public static ZZRefCusCodeListCombined GetRefCusCodeList(BusinessObjectFactory factory, ZString zCode, ZString zType)
		{
			return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(
					factory,
					zCode,
					Core.Constants.CountryCodes.KoreaSouth,
					zType,
					ZDate.Today);
		}

		public static ZString[] GetIndividualDigits(ZString input, int countOfEmptySpace)
		{
			var result = new List<ZString>();
			for (int index = 0; index < countOfEmptySpace; index++)
			{
				result.Add(ZString.Empty);
			}

			if (input != ZDecimal.Zero.ToString(0))
			{
				for (int index = 0; index < input.Length; index++)
				{
					result.Add(new ZString(input[index]));
				}
			}
			return result.ToArray();
		}

		public static ZString GetCountryKRCCode(BusinessObjectFactory factory, ZString countryCode)
		{
			ZString result = ZString.Empty;
			if (!countryCode.IsEmpty)
			{
				result = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(factory, Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.RefCusMap.CountryKRCCode, countryCode, ZDateTime.Now);
			}
			return result;
		}

		public static void RoundDecimalValueRoundedWithDecimalPlaces(this IMessageDataProvider messageDataProviderObject)
		{
			RoundDecimalValueRoundedWithDecimalPlacesInternal(messageDataProviderObject);
		}

		static void RoundDecimalValueRoundedWithDecimalPlacesInternal(object target)
		{
			var getters = target.GetType().GetProperties().Select(x => x.GetGetMethod()).Where(x => !x.ReturnType.IsValueType && !x.ReturnType.Equals(typeof(string)));
			foreach (var getter in getters)
			{
				var child = getter.Invoke(target, null);
				if (child == null)
				{
					continue;
				}

				if (child is Array elements)
				{
					foreach (var element in elements)
					{
						RoundDecimalValueRoundedWithDecimalPlacesInternal(element);
					}
				}
				else
				{
					RoundDecimalValueRoundedWithDecimalPlacesInternal(child);
				}
			}

			var decimalProperties = target.GetType().GetProperties().Where(x => x.PropertyType.Equals(typeof(decimal)));
			foreach (var item in decimalProperties)
			{
				var decimalPlaceFromAttribute = item.GetCustomAttribute<DecimalPlacesAttribute>();
				if (decimalPlaceFromAttribute != null)
				{
					var decimalPlaceFromRealValue = DecimalExtensions.GetNumberDecimalPlaces((decimal)item.GetValue(target));

					var decimalPlaceToBeUsed = Math.Min(decimalPlaceFromRealValue, decimalPlaceFromAttribute.DecimalPlaces);
					item.SetValue(target, ZArchitecture.Core.Utilities.Round((decimal)item.GetValue(target), decimalPlaceToBeUsed));
				}
				else
				{
					ErrorReporter.ReportOnce(string.Format("Decimal places attribute is missing for decimal property: {0}", item.Name));
				}
			}
		}

		public static ZString GetEntryType(ZString entryNumber) => entryNumber.EqualsIgnoringCase(ImportCargoManagementNumber.No) || entryNumber.Length == 15 || entryNumber.Length == 19 ? ReferenceNumberTypeList.Codes.CMN : ReferenceNumberTypeList.Codes.IMP;
	}
}
