using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public class AdditionalInformationHelper : IAdditionalElementCollection
	{
		public AdditionalInformationHelper(IAdditionalInformationWrapperParent parent, ZPropertyInfo nameOfGoodsInfo, ZPropertyInfo goodsSpecModelInfo, Func<EnteringOrExiting> isEnteringOrExitingFunc)
		{
			this.parent = Argument.NotNull(parent, nameof(parent));
			this.nameOfGoodsInfo = Argument.NotNull(nameOfGoodsInfo, nameof(nameOfGoodsInfo));
			this.goodsSpecModelInfo = Argument.NotNull(goodsSpecModelInfo, nameof(goodsSpecModelInfo));
			this.isEnteringOrExitingFunc = Argument.NotNull(isEnteringOrExitingFunc, nameof(isEnteringOrExitingFunc));
		}

		readonly IAdditionalInformationWrapperParent parent;
		readonly ZPropertyInfo nameOfGoodsInfo;
		readonly ZPropertyInfo goodsSpecModelInfo;
		readonly Func<EnteringOrExiting> isEnteringOrExitingFunc;

		internal IValidationModeProvider ValidationModeProvider => parent;

		#region Properties

		internal TariffView UniversalTariff => parent.UniversalTariff;

		internal ZString NameOfGoods => (ZString)nameOfGoodsInfo.Value;
		internal ZString GoodsSpecModel => (ZString)goodsSpecModelInfo.Value;
		internal EnteringOrExiting IsEnteringOrExiting => isEnteringOrExitingFunc();

		internal IEnumerable<AdditionalElementValue> AdditionalElementValues
		{
			get
			{
				var newKey = GenerateCacheKey();
				if (fAdditionalElementValues == null || cacheKey != newKey)
				{
					fAdditionalElementValues = UniversalTariff?.DecomposeGoodsSpecModel(IsEnteringOrExiting, GoodsSpecModel).ToArray() ?? Array.Empty<AdditionalElementValue>();
					ClearExtractedFields();
					cacheKey = newKey;
				}
				return fAdditionalElementValues;
			}
		}
		IEnumerable<AdditionalElementValue> fAdditionalElementValues;

		ZString cacheKey;
		ZString GenerateCacheKey() => ZString.Format("{0}_{1}_{2}", UniversalTariff?.ZZ1_TariffCode ?? ZString.Empty, IsEnteringOrExiting, GoodsSpecModel);

		#endregion

		#region Utilities

		static bool ShouldPopulateExtraInforToOther(ZString additionalInfoCode)
		{
			return additionalInfoCode == PackSpecElementStrategy.AdditionalElementCode || additionalInfoCode == SpecModelElementStrategy.AdditionalElementCode;
		}

		static ZString GetMergedAdditionalInfoValue(IEnumerable<AdditionalInformationHelper> helpers, ZString additionalInfoCode, Action<ZString> actionForPopulateExtraInfo)
		{
			var values = helpers?.Select(x => x.GetAdditionalElementValue(additionalInfoCode)).Where(x => !x.IsEmpty).Distinct();
			return GetMergedAdditionalInfoValue(additionalInfoCode, values, actionForPopulateExtraInfo);
		}

		static ZString GetMergedAdditionalInfoValue(ZString additionalInfoCode, IEnumerable<ZString> values, Action<ZString> actionForPopulateExtraInfo)
		{
			var result = ZString.Empty;
			var shouldPopulateExtraInfoToOther = ShouldPopulateExtraInforToOther(additionalInfoCode);

			var count = values.Count();
			if (count < 2)
			{
				result = values.FirstOrDefault();
			}
			else if (count == 2)
			{
				if (shouldPopulateExtraInfoToOther)
				{
					result = values.ElementAt(0);
					actionForPopulateExtraInfo(values.ElementAt(1));
				}
				else
				{
					result = ZString.Join(AddInfoValueDelimeter, values.ToArray());
				}
			}
			else if (count > 2)
			{
				if (shouldPopulateExtraInfoToOther)
				{
					result = values.ElementAt(0);

					if (additionalInfoCode == SpecModelElementStrategy.AdditionalElementCode)
					{
						actionForPopulateExtraInfo(SpecialMergeForSpecificationModelValue(values.Skip(1)));
					}
					else
					{
						actionForPopulateExtraInfo(values.ElementAt(1) + AndSoOnSuffix);
					}
				}
				else
				{
					result = values.ElementAt(0) + AndSoOnSuffix;
				}
			}

			return result;
		}

		static ZString SpecialMergeForSpecificationModelValue(IEnumerable<ZString> values)
		{
			var result = ZString.Empty;
			var value0 = values.ElementAt(0);
			if (!value0.Contains(SpecificationModelSeperator))
			{
				result = value0 + AndSoOnSuffix;
			}
			else if (values.Skip(1).Any(x => !x.Contains(SpecificationModelSeperator)))
			{
				result = ZString.Join(new ZString(SpecificationModelSeperator), value0.Split(SpecificationModelSeperator).Select(x => new ZString(x + AndSoOnSuffix)).ToArray());
			}
			else
			{
				var splitedValues = values.Select(x => x.Split(SpecificationModelSeperator));
				result = MergeSplitedValues(splitedValues, 0) + SpecificationModelSeperator + MergeSplitedValues(splitedValues, 1);
			}

			return result;
		}

		static ZString MergeOtherInfomationValues(IEnumerable<ZString> values)
		{
			var sensibleValues = values.Where(x => !x.IsEmpty && !MeaninglessValues.Contains(x)).Distinct();
			if (!sensibleValues.Any())
			{
				var value = values.FirstOrDefault(x => MeaninglessValues.Contains(x));
				if (!value.IsEmpty)
				{
					sensibleValues = new[] { value };
				}
			}

			return ZString.Join(AddInfoValueDelimeter, sensibleValues.ToArray());
		}

		static ZString MergeSplitedValues(IEnumerable<ZString[]> splitedValues, int index)
		{
			return splitedValues.First()[index] + (splitedValues.Select(x => x[index]).Distinct().Count() > 1 ? AndSoOnSuffix : string.Empty);
		}

		static ZString AddElementNameIfNeeded(TariffView tariff, bool includeElementName, ZString additionalInfoCode, ZString value)
		{
			var prefix = ZString.Empty;
			if (includeElementName)
			{
				var description = tariff?.GetAdditionalElementDescription(additionalInfoCode) ?? ZString.Empty;
				if (!description.IsEmpty)
				{
					prefix = description + NameValueDelimeter;
				}
			}
			return prefix + value;
		}

		internal static ZString TrimEndDelimeterForNonMandatoryElements(TariffView tariff, EnteringOrExiting isEnteringOrExiting, ZString input)
		{
			var result = input;
			if (!result.IsEmpty && result.EndsWith(AddInfoElementDelimeter.ToString()))
			{
				var mandatoryElementsCount = tariff.MandatoryGoodsSpecModelAttributesCount(isEnteringOrExiting);
				if (mandatoryElementsCount > 0)
				{
					var lastMandatoryDelimeterIndex = input.IndexOf(AddInfoElementDelimeter, mandatoryElementsCount);
					result = lastMandatoryDelimeterIndex > -1 ? input.Left(lastMandatoryDelimeterIndex) + input.Substring(lastMandatoryDelimeterIndex).TrimEnd(AddInfoElementDelimeter) : input.ToString();
				}
			}
			return result;
		}

		#endregion

		#region Constants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Suffix string")]
		const string AndSoOnSuffix = "\u7b49";
		const string NameValueDelimeter = ":";
		const string AddInfoValueDelimeter = "/";
		public const char AddInfoElementDelimeter = '|';
		const char SpecificationModelSeperator = '\u3001';
		public const int NameOfGoodsMaxLength = 50;

		#region SuppressResourceStringsCheckRegion
		static readonly ImmutableArray<string> MeaninglessValues = ImmutableArray.Create(
			"无",
			"无其他",
			"无需报",
			"无需申报",
			"其他无"
		);
		#endregion

		#endregion

		public static int GoodsSpecModelMaxLength => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.CNDecMessageV2020GoLiveDate, Core.Constants.CountryCodes.China, ZDateTime.Today) ? 1300 : 255;

		#region For Merging

		public static ZString GetMergedGoodsSpecModel(IEnumerable<AdditionalInformationHelper> helpers, bool includeElementName = false)
		{
			var result = ZString.Empty;
			var helper = helpers.FirstOrDefault();
			var tariff = helper?.UniversalTariff;

			if (tariff != null)
			{
				var additionalElementValuesList = helpers.Select(x => x.AdditionalElementValues);
				if (additionalElementValuesList.Any())
				{
					var attributeCodes = tariff.GetSortedGoodsSpecModelAttribues(helper.IsEnteringOrExiting)?.Select(attr => attr.ZZ3_Value).ToList();
					var indexOfOtherElement = attributeCodes.IndexOf(OthersElementStrategy.AdditionalElementCode);

					if (indexOfOtherElement > -1)
					{
						var otherInformation = helpers.Select(x => x.GetAdditionalElementValue(OthersElementStrategy.AdditionalElementCode)).ToList();

						result = attributeCodes.Take(indexOfOtherElement)
							.Select(code => AddElementNameIfNeeded(tariff, includeElementName, code, GetMergedAdditionalInfoValue(helpers, code, (otherInfo) => { otherInformation.Add(otherInfo); })))
							.JoinAsString(AddInfoElementDelimeter, false);

						if (otherInformation.Count > 0)
						{
							result += AddInfoElementDelimeter + MergeOtherInfomationValues(otherInformation);
						}

						if (attributeCodes.Count > indexOfOtherElement + 1)
						{
							result += AddInfoElementDelimeter + attributeCodes.Skip(indexOfOtherElement + 1).Select(code => DecorateADDCVDElementValue(helper.GetAdditionalElementValue(code))).JoinAsString(string.Empty, false);
						}
					}
					else
					{
						result = attributeCodes.Select(code => helper.GetAdditionalElementValue(code)).JoinAsString(AddInfoElementDelimeter, false);
					}

					result = TrimEndDelimeterForNonMandatoryElements(tariff, helper.IsEnteringOrExiting, result);
				}
			}

			return result;
		}

		public static ZString DecorateADDCVDElementValue(ZString value) => FormattableString.Invariant($"<{value}>");

		public static ZString GetMergedNameOfGoods(IEnumerable<AdditionalInformationHelper> helpers)
		{
			var nameOfGoods = helpers.Select(x => x.NameOfGoods);
			return nameOfGoods.FirstOrDefault() + (nameOfGoods.AllSame() ? string.Empty : AndSoOnSuffix);
		}

		#endregion

		#region For AdditionalElementValue

		AdditionalElementValue GetAdditionalElement(ZString additionalInfoCode) => AdditionalElementValues?.FirstOrDefault(x => x.Code == additionalInfoCode);

		internal ZString GetAdditionalElementValue(ZString additionalInfoCode) => GetAdditionalElement(additionalInfoCode)?.Value ?? ZString.Empty;

		public void ValidateGoodsSpecModel()
		{
			if (UniversalTariff != null)
			{
				var additionalElementValues = AdditionalElementValues;

				if (!parent.ElementValueAllowEmpty && additionalElementValues.Any(x => x.Value.IsEmpty && x.AdditionalElementStrategy.IsMandatory))
				{
					goodsSpecModelInfo.AddNotification(Res.GetString("f689a0f4-8c52-48ba-b1e0-78cf1adadef8", "You have not entered some Additional Information. Click the '...' button to see details."), ValidationModeProvider);
				}

				if (additionalElementValues.Any(x => !x.Value.IsEmpty && x.AdditionalElementStrategy.ProvideList && !x.AdditionalElementStrategy.GetList(UniversalTariff.Factory, IsEnteringOrExiting).ContainsCode(x.Value)))
				{
					goodsSpecModelInfo.AddNotification(Res.GetString("6d6b5a1d-3b94-4e73-ab11-6769eca013dd", "Some Additional Information you entered is incorrect. Click the '...' button to see details."), ValidationModeProvider);
				}

				if (additionalElementValues.Any(x => !x.AdditionalElementStrategy.GetFormatCheckMessage(x.Value).IsEmpty))
				{
					goodsSpecModelInfo.AddWarning(Res.GetString("af853d4e-8e29-42ff-a45f-2c4c537826df", "Some Additional Information you entered has incorrect format. Click the '...' button to see details."));
				}

				foreach (var additionalElementValue in additionalElementValues)
				{
					additionalElementValue.AdditionalElementStrategy.ValidateAdditionalElement(parent, additionalElementValue.Value, goodsSpecModelInfo);
				}
			}
		}

		public bool IsStandardAdditionalInformation()
		{
			return UniversalTariff != null && GoodsSpecModel.ToString().Count(x => x == AddInfoElementDelimeter) + 1 >= UniversalTariff.MandatoryGoodsSpecModelAttributesCount(IsEnteringOrExiting);
		}

		public ZString GetMergeKeyFromGoodsSpecModel()
		{
			ZString result = ZString.Empty;

			if (UniversalTariff != null && IsStandardAdditionalInformation())
			{
				result = IsEnteringOrExiting == EnteringOrExiting.Entering && UniversalTariff.HasCommodityTypeATP() ? GoodsSpecModel
					: ZString.Join(AddInfoElementDelimeter.ToString(), AdditionalElementValues.Where(x => x.AdditionalElementStrategy.IsMergeKey).Select(x => x.Value).ToArray());
			}
			return result;
		}

		public void CorrectAdditionalInfoValues()
		{
			var tariff = UniversalTariff;
			if (tariff != null)
			{
				var isEnteringOrExiting = IsEnteringOrExiting;
				foreach (var additionalElementValue in AdditionalElementValues)
				{
					var strategy = additionalElementValue.AdditionalElementStrategy;
					if (strategy.ProvideList && !strategy.GetList(tariff.Factory, isEnteringOrExiting).ContainsCode(additionalElementValue.Value))
					{
						additionalElementValue.Value = strategy.GetDefaultValue(isEnteringOrExiting);
					}
				}

				goodsSpecModelInfo.Value = this.GetGoodsSpecModel(tariff, isEnteringOrExiting);
			}
		}

		#endregion

		#region IAdditionalElementCollection

		ZString IAdditionalElementCollection.GetValue(ZString code) => GetAdditionalElementValue(code);

		void IAdditionalElementCollection.SetValue(ZString code, ZString value) => throw new NotSupportedException();

		#endregion

		#region Sync CIQ Details

		public ZString ExtractedIngredient => fExtractedIngredient ?? (fExtractedIngredient = CIQDetailsExtractStrategy.ExtractIngredient(AdditionalElementValues));
		string fExtractedIngredient;

		public ZString ExtractedSpecification => fExtractedSpecification ?? (fExtractedSpecification = CIQDetailsExtractStrategy.ExtractSpecification(AdditionalElementValues));
		string fExtractedSpecification;

		public ZString ExtractedBrand => fExtractedBrand ?? (fExtractedBrand = CIQDetailsExtractStrategy.ExtractBrand(AdditionalElementValues));
		string fExtractedBrand;

		public ZString ExtractedModel => fExtractedModel ?? (fExtractedModel = CIQDetailsExtractStrategy.ExtractModel(AdditionalElementValues));
		string fExtractedModel;

		public ZDateTime[] ExtractedManufactureDates => fExtractedManufactureDates ?? (fExtractedManufactureDates = CIQDetailsExtractStrategy.ExtractManufactureDates(AdditionalElementValues));
		ZDateTime[] fExtractedManufactureDates;

		void ClearExtractedFields()
		{
			fExtractedIngredient = null;
			fExtractedSpecification = null;
			fExtractedBrand = null;
			fExtractedModel = null;
			fExtractedManufactureDates = null;
		}

		#endregion
	}
}
