using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class UniversalValidationHelper
	{
		public static void CheckCountryOfDispatchOrDestinationIsAtLeastOneC0009Code(BusinessObjectFactory factory, ZString declarationType, ZString countryOfDispatch, ZString countryOfDestination, ZString dataGroupingCode, ZPropertyInfo propertyInfo)
		{
			if (declarationType == NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure)
			{
				var codeList9 = factory.GetCachedValue("EU.Ncts.GetC0009CodeList_" + dataGroupingCode, () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(factory, dataGroupingCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, ZDateTime.Today));
					if (dataGroupingCode == Core.Constants.CountryCodes.UnitedKingdom)
					{
						result.AddPairIfNotExist(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, Res.GetString("09ff724c-5a18-4d3e-9cd2-200e823385aa", "Northern Ireland"));
					}
					return result;
				});

				if (!codeList9.ContainsCode(countryOfDispatch) && !codeList9.ContainsCode(countryOfDestination))
				{
					propertyInfo.AddMessageError(Res.GetString("86F8151F-ABFD-4D0D-BA51-DE97DEC96D83", "For T2 Declarations [15A] and/or [17A] must be a country from Code List 9 (Countries if European Union + NCTS Contracting Parties)."));
				}
			}
		}

		public static void CheckMaxLengthIfPhase5TransitionPeriod(bool isInPhase5TransitionPeriod, ZPropertyInfo propertyInfo, int maxLengthPhase5TP, string rulePrefix = "")
		{
			if (isInPhase5TransitionPeriod)
			{
				CheckMaxLength(propertyInfo, maxLengthPhase5TP, rulePrefix);
			}
		}

		public static void CheckMaxLength(ZPropertyInfo propertyInfo, int maxLength, string rulePrefix = "")
		{
			var value = (ZString)propertyInfo.Value;
			if (value.Length > maxLength)
			{
				var baseMessage = Res.GetString("FDB73564-B864-4050-B34F-038F2BBF2502", "Length of {0} must not exceed {1} characters.", propertyInfo.HumanReadableName, maxLength);
				var finalMessage = new ZStringBuilder().AppendIfNotEmpty(rulePrefix).Append(baseMessage).ToString();
				propertyInfo.AddMessageError(finalMessage);
			}
		}

		public static void CheckMaxLengthForPhase5AndTransitionPeriod(bool isInPhase5TransitionPeriod, ZPropertyInfo propertyInfo, int maxLengthPhase5, int maxLengthPhase5TP)
		{
			int actualMaxLength;
			if (isInPhase5TransitionPeriod)
			{
				actualMaxLength = maxLengthPhase5TP;
			}
			else
			{
				actualMaxLength = maxLengthPhase5;
			}
			CheckMaxLength(propertyInfo, actualMaxLength);
		}

		public static void CheckNoLowerCaseLetters(ZPropertyInfo propertyInfo) => CheckNoLowerCaseLetters(propertyInfo, "");

		public static void CheckNoLowerCaseLetters(ZPropertyInfo propertyInfo, string messagePrefix)
		{
			var value = propertyInfo.Value.ToString();
			if (value.Any(char.IsLower))
			{
				var baseMessage = Res.GetString("88498CF3-0E6C-41D6-99A3-2F448EC3AEFC", "Must not contain lower case letters.");
				var finalMessage = new ZStringBuilder()
					.AppendIfNotEmpty(messagePrefix)
					.Append(baseMessage)
					.ToStringWithDelimiterBetweenAppends(" ");
				propertyInfo.AddMessageError(finalMessage);
			}
		}

		public static void CheckMaxValueAndMaxDecimalLengthForWeightIfPhase5TransitionPeriod(bool isInPhase5TransitionPeriod, string message, ZPropertyInfo propertyInfo, int maxValueLength, int maxDecimalLength)
		{
			if (isInPhase5TransitionPeriod)
			{
				var value = (ZDecimal)propertyInfo.Value;
				var decimalPlaces = value.DecimalPlaces;

				if (decimalPlaces > maxDecimalLength || value.Truncate().ToString().Length + decimalPlaces > maxValueLength)
				{
					propertyInfo.AddMessageError(message);
				}
			}
		}

		public static bool IsDocumentTypeIsMRN(string documentType)
			=> documentType.In(new[] {
				NctsConstants.NctsTypeOfPreviousDocument.Codes.N820,
				NctsConstants.NctsTypeOfPreviousDocument.Codes.N821,
				NctsConstants.NctsTypeOfPreviousDocument.Codes.N822,
				NctsConstants.NctsTypeOfPreviousDocument.Codes.N830
		});
	}
}
