using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CN.Business
{
	public static class AdditionalElementCollectionExtension
	{
		#region NameOfGoods

		public static ZString GetNameOfGoods(this IAdditionalElementCollection additionalInformationCodes)
		{
			return additionalInformationCodes?.GetValue(NameOfGoodsElementStrategy.AdditionalElementCode) ?? ZString.Empty;
		}

		public static void SetNameOfGoods(this IAdditionalElementCollection additionalInformationCodes, ZString value)
		{
			if (additionalInformationCodes != null)
			{
				additionalInformationCodes.SetValue(NameOfGoodsElementStrategy.AdditionalElementCode, value);
			}
		}

		#endregion

		#region GoodsSpecModel

		public static ZString GetGoodsSpecModel(this IAdditionalElementCollection additionalInformationCodes, TariffView tariff, EnteringOrExiting isEnteringOrExiting)
		{
			var result = ZString.Empty;
			if (tariff != null && additionalInformationCodes != null)
			{
				var attributeCodes = tariff.GetSortedGoodsSpecModelAttribues(isEnteringOrExiting)?.Select(attr => attr.ZZ3_Value).ToList();
				if (attributeCodes != null && attributeCodes.Any())
				{
					var indexOfOtherElement = attributeCodes.IndexOf(OthersElementStrategy.AdditionalElementCode);
					result = attributeCodes.Take(indexOfOtherElement + 1).Select(c => additionalInformationCodes.GetValue(c)).JoinAsString(AdditionalInformationHelper.AddInfoElementDelimeter, false);

					if (indexOfOtherElement > -1 && attributeCodes.Count > indexOfOtherElement + 1)
					{
						result += AdditionalInformationHelper.AddInfoElementDelimeter + attributeCodes.Skip(indexOfOtherElement + 1).Select(c => AdditionalInformationHelper.DecorateADDCVDElementValue(additionalInformationCodes.GetValue(c))).JoinAsString(string.Empty, false);
					}

					result = AdditionalInformationHelper.TrimEndDelimeterForNonMandatoryElements(tariff, isEnteringOrExiting, result);
				}
			}

			return result;
		}

		internal static IEnumerable<AdditionalElementValue> DecomposeGoodsSpecModel(this TariffView tariff, EnteringOrExiting isEnteringOrExiting, ZString value)
		{
			var attributeCodes = tariff?.GetSortedGoodsSpecModelAttribues(isEnteringOrExiting)?.Select(attr => attr.ZZ3_Value).ToList();
			if (attributeCodes != null && attributeCodes.Any())
			{
				var indexOfOtherElement = attributeCodes.IndexOf(OthersElementStrategy.AdditionalElementCode);

				var inputs = value.ToString().Split(AdditionalInformationHelper.AddInfoElementDelimeter);
				if (indexOfOtherElement > -1 && inputs.Length > indexOfOtherElement + 1)
				{
					inputs = inputs.Take(indexOfOtherElement + 1).Concat(SplitADDCVDElement(inputs[indexOfOtherElement + 1])).ToArray();
				}

				for (var idx = 0; idx < attributeCodes.Count; idx++)
				{
					var input = inputs.ElementAtOrDefault(idx) ?? string.Empty;
					yield return new AdditionalElementValue(tariff) { Code = attributeCodes.ElementAt(idx), Value = input };
				}
			}
		}

		public static void SetGoodsSpecModel(this IAdditionalElementCollection additionalInformationCodes, TariffView tariff, EnteringOrExiting isEnteringOrExiting, ZString value)
		{
			if (additionalInformationCodes != null && tariff != null)
			{
				foreach (var additionalElement in tariff.DecomposeGoodsSpecModel(isEnteringOrExiting, value))
				{
					additionalInformationCodes.SetValue(additionalElement.Code, additionalElement.Value);
				}
			}
		}

		static IEnumerable<string> SplitADDCVDElement(string input)
		{
			var regex = new Regex(@"<([^\>]*)>");

			foreach (Match match in regex.Matches(input))
			{
				yield return match.Groups[1].Value;
			}
		}

		#endregion
	}
}
