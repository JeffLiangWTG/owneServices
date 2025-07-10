using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	class DutyAndTaxAmountDescriptor
	{
		internal static string GetDescription(IDutyAndTaxDataForCalculation dutyOrTax)
		{
			var result = string.Empty;
			if (!dutyOrTax.RateType.IsEmpty)
			{
				switch (dutyOrTax.RateType)
				{
					case RateTypes.Codes.AdValorem:
						if (!dutyOrTax.Rate.IsEmpty)
						{
							result = GetAmountDescription(dutyOrTax, GetDutyOrTaxValueDescription(dutyOrTax), GetDutyOrTaxValueString(dutyOrTax));
						}

						break;
					case RateTypes.Codes.Specific:
						result = GetAmountDescription(dutyOrTax, GetQuantityDescription(dutyOrTax), GetSpecificValueString(dutyOrTax));
						break;
					default:
						result = RateTypes.GetDescriptionFromCode(dutyOrTax.RateType) + Dot;
						break;
				}
			}
			return result;
		}

		static RateTypes RateTypes => new RateTypes();
		static CustomsUnitOfMeasureList CustomsUQList => new CustomsUnitOfMeasureList();

		internal static string GetCombinedDutyDescription(DutyAndTaxManager.CombinedDuty duty)
		{
			var result = ZString.Empty;
			var parent = duty.Regulars[0].Parent;

			if (parent != null)
			{
				var builder = new ZStringBuilder(GetCombinedDutyTypeDescription(duty));

				if ((parent.CalculationMethod == CalculationMethods.Codes.RepairsRemission
				|| parent.CalculationMethod == CalculationMethods.Codes.RegularRemission
				|| parent.CalculationMethod == CalculationMethods.Codes.DutyDeferral
				|| parent.CalculationMethod == CalculationMethods.Codes.WarrantyRepairsRemission
				|| parent.CalculationMethod == CalculationMethods.Codes.SoftwareRemission)
				&& !parent.RulingConfigs.Any())
				{
					builder.Append(GetZeroForRemissionDescription(parent));
				}
				else
				{
					duty.Regulars.ForEach(regDuty => builder.Append(regDuty.AmountDescription));
					if (duty.Regulars.Count > 1)
					{
						builder.Append(Res.GetString("d4b53e10-7652-46a6-9dfa-51af6be86e4f", "The results are added."));
					}

					AddDutyConstraintsText(builder, duty);

					builder.AppendIfNotEmpty(GetOneSixtiethRemissionDescription(parent));
				}

				result = builder.ToStringWithDelimiterBetweenAppends(Delimiter).TrimEnd(Delimiter[0], Dot[0]) + Dot;
			}

			return result;
		}

		static void AddDutyConstraintsText(ZStringBuilder builder, DutyAndTaxManager.CombinedDuty duty, bool showUomCode = false)
		{
			if (duty.Min != null || duty.Max != null)
			{
				builder.Append(Res.GetString("40abcd93-a7ef-48be-b6be-6db0497335af", "But"));
				if (duty.Min != null)
				{
					builder.Append(Res.GetString("4e839883-711c-4630-9a4b-386e2562ac29", "not less than"));

					if (duty.Max == null)
					{
						builder.Append(GetRateString(duty.Min, showUomCode) + Dot);
					}
					else
					{
						builder.Append(GetRateString(duty.Min, showUomCode));
						builder.Append(Res.GetString("c56de419-9f69-4c25-bd78-8b09724d89aa", "or"));
					}
				}

				if (duty.Max != null)
				{
					builder.Append(Res.GetString("56003560-4491-4bee-9860-0f332bc2d7ee", "not more than"));
					builder.Append(GetRateString(duty.Max, showUomCode) + Dot);
				}
			}
		}

		internal static string GetCombinedShortDutyDescription(DutyAndTaxManager.CombinedDuty duty)
		{
			var builder = new ZStringBuilder();
			int count = duty.Regulars.Count;
			foreach (var regDuty in duty.Regulars)
			{
				count--;
				builder.Append(GetShortDutyDescription(regDuty) + (count > 0 ? " +" : ""));
			}
			AddDutyConstraintsText(builder, duty, showUomCode: true);
			return builder.ToStringWithDelimiterBetweenAppends(Delimiter).TrimEnd(Delimiter[0], Dot[0]);
		}

		internal static string GetShortDutyDescription(IDutyAndTaxDataForCalculation duty)
		{
			var result = ZString.Empty;
			switch (duty.RateType)
			{
				case RateTypes.Codes.AdValorem:
				case RateTypes.Codes.Specific:
					result = GetRateString(duty, showUomCode: true);
					break;
				case RateTypes.Codes.Free:
					result = Res.GetString("ee414fd5-e389-4436-9032-f347c4e287d8", "Free");
					break;
				case RateTypes.Codes.Exempt:
					result = Res.GetString("766460e5-29dc-46c8-b1cf-10ed36630d47", "Exempt");
					break;
				case RateTypes.Codes.AcceptT:
				case RateTypes.Codes.AcceptX:
					result = Res.GetString("c81e0fdc-79a5-4c5a-899e-f163e11ca07d", "Manual");
					break;
			}
			return result;
		}

		#region Implementation

		#region Amount Description

		static string GetAmountDescription(IDutyAndTaxDataForCalculation dutyOrTax, string valueDescription, string valueString)
		{
			var builder = new ZStringBuilder();
			var parent = dutyOrTax.Parent;
			if (parent != null)
			{
				var isTax = dutyOrTax.TaxType == DutyAndTaxTypes.Codes.ExciseTax || dutyOrTax.TaxType == DutyAndTaxTypes.Codes.GST;
				var calculationMethod = parent.CalculationMethod;
				if ((calculationMethod == CalculationMethods.Codes.RepairsRemission
				|| (calculationMethod == CalculationMethods.Codes.DutyDeferral && dutyOrTax.TaxType != DutyAndTaxTypes.Codes.GST)
				|| calculationMethod == CalculationMethods.Codes.WarrantyRepairsRemission
				|| calculationMethod == CalculationMethods.Codes.SoftwareRemission
				|| (calculationMethod == CalculationMethods.Codes.RegularRemission && !isTax))
				&& !parent.RulingConfigs.Any())
				{
					builder.Append(GetZeroForRemissionDescription(parent));
				}
				else if (calculationMethod == CalculationMethods.Codes.RegularRemission
					&& parent.AuthorityNumber == DutyAndTaxManager.TaxRemittedOICNumber && isTax)
				{
					builder.Append(GetZeroForRemissionAndOICNumberDescription(parent));
				}
				else if (calculationMethod == CalculationMethods.Codes.GiftsUpTo60 && dutyOrTax.TaxType != DutyAndTaxTypes.Codes.ExciseTax)
				{
					var dutyTaxValue = GetDutyOrTaxValue(dutyOrTax);
					var effectiveValue = dutyTaxValue - 60m;
					effectiveValue = effectiveValue < 0m ? 0m : effectiveValue;
					builder.Append(GetGiftsUpTo60RemissionDescription(GetValueString(effectiveValue), valueDescription, GetValueString(dutyTaxValue), RateTypes.GetDescriptionFromCode(dutyOrTax.RateType), GetRateString(dutyOrTax)));
				}
				else
				{
					builder.Append(Res.GetString("aeac9aea-714a-4f80-a8c6-025e83343af2", "{0} ({1}) is multiplied by the {2} rate ({3}).",
																			 valueDescription,
																			 valueString,
																			 RateTypes.GetDescriptionFromCode(dutyOrTax.RateType),
																			 GetRateString(dutyOrTax)));

					if (isTax || !dutyOrTax.IsInRefFiles)
					{
						builder.AppendIfNotEmpty(GetOneSixtiethRemissionDescription(parent));
					}
				}
			}

			return builder.ToStringWithDelimiterBetweenAppends(Delimiter);
		}

		internal static string GetRateString(IDutyAndTaxDataForCalculation dutyOrTax, bool showUomCode = false)
		{
			string result = string.Empty;
			switch (dutyOrTax.RateType)
			{
				case RateTypes.Codes.AdValorem:
					result = dutyOrTax.Rate.ToString(DecimalFormat) + AmountTypes.Codes.Percent;
					break;
				case RateTypes.Codes.Specific:
					var uom = GetUnitOfMeasure(dutyOrTax);
					if (showUomCode)
					{
						if (string.IsNullOrEmpty(uom))
						{
							uom = CustomsUnitOfMeasureList.Codes.Piece;
						}
					}
					else
					{
						uom = (string.IsNullOrEmpty(uom) ? CustomsUnitOfMeasureList.Descriptions.Piece : CustomsUQList.GetDescriptionFromCode(uom));
					}
					result = string.Format("{0}/{1}", GetValueString(dutyOrTax.Rate), uom);
					break;
			}
			return result;
		}

		static string GetValueString(ZDecimal value)
		{
			return value < 1 ? (value * 100).ToString(DecimalFormat) + "¢" : AmountTypes.Codes.Dollar + value.ToString(DecimalFormat);
		}

		#endregion

		#region Specific Rate Description

		static string GetQuantityDescription(IDutyAndTaxDataForCalculation dutyOrTax)
		{
			string result = Res.GetString("7dd8720e-5889-44d4-9c9b-a458d6c6324e", "Undefined quantity");
			var parent = dutyOrTax.Parent;
			if (parent != null)
			{
				//NOTE: This conditional operator should match the one in DutyAndTax.Quantity property.
				if (dutyOrTax.UnitOfMeasure.IsEmpty || (dutyOrTax.UnitOfMeasure == parent.CustomsUnits && dutyOrTax.TaxType != DutyAndTaxTypes.Codes.CustomsDuty))
				{
					result = Res.GetString("5682a7e0-f0d6-484e-afc6-876f9801759c", "The quantity of the first unit of measure");
				}
				else if (dutyOrTax.UnitOfMeasure == parent.CustomsUnits2)
				{
					result = Res.GetString("9c855e7a-42f9-468b-8319-208dfc56438b", "The quantity of the second unit of measure");
				}
				else if (dutyOrTax.UnitOfMeasure == parent.CustomsUnits3)
				{
					result = Res.GetString("51c7a337-9f95-4b76-91f9-ae7925e239fc", "The quantity of the third unit of measure");
				}
			}

			return result;
		}

		static string GetSpecificValueString(IDutyAndTaxDataForCalculation dutyOrTax)
		{
			return dutyOrTax.Quantity.ToString(DecimalFormat) + GetUnitOfMeasure(dutyOrTax);
		}

		static string GetUnitOfMeasure(IDutyAndTaxDataForCalculation dutyOrTax)
		{
			var unitOfMeasure = dutyOrTax.UnitOfMeasure;
			return dutyOrTax.UnitOfMeasure.IsEmpty && dutyOrTax.RateType == RateTypes.Codes.Specific ? dutyOrTax.Parent?.CustomsUnits ?? unitOfMeasure : unitOfMeasure;
		}

		#endregion

		#region Ad Valorem Rate Description

		static string GetDutyOrTaxValueDescription(IDutyAndTaxDataForCalculation dutyOrTax)
		{
			string result;
			switch (dutyOrTax.TaxType)
			{
				case DutyAndTaxTypes.Codes.CustomsDuty:
				case DutyAndTaxTypes.Codes.ADD:
				case DutyAndTaxTypes.Codes.CVD:
				case DutyAndTaxTypes.Codes.SUR:
					result = Res.GetString("b7587be7-74ee-4047-9d27-98ed6f390d10", "The customs value");
					break;
				case DutyAndTaxTypes.Codes.ExciseTax:
					result = Res.GetString("9343834e-371b-4599-91bb-77edff0d9e9e", "The normal duty paid value");
					break;
				case DutyAndTaxTypes.Codes.GST:
					result = Res.GetString("0aeb82bb-0b84-4bff-9a02-9eca5be89b84", "The normal value for tax");
					break;
				default:
					result = string.Empty;
					break;
			}
			return result;
		}

		static ZDecimal GetDutyOrTaxValue(IDutyAndTaxDataForCalculation dutyOrTax)
		{
			var result = ZDecimal.Zero;
			var parent = dutyOrTax.Parent;
			if (parent != null)
			{
				switch (dutyOrTax.TaxType)
				{
					case DutyAndTaxTypes.Codes.CustomsDuty:
					case DutyAndTaxTypes.Codes.ADD:
					case DutyAndTaxTypes.Codes.CVD:
					case DutyAndTaxTypes.Codes.SUR:
						result = parent.CustomsValue;
						break;
					case DutyAndTaxTypes.Codes.ExciseTax:
						result = parent.NormalDutyPaidValue;
						break;
					case DutyAndTaxTypes.Codes.GST:
						result = parent.NormalValueForTax;
						break;
					default:
						result = ZDecimal.Zero;
						break;
				}
			}
			return result;
		}

		static string GetDutyOrTaxValueString(IDutyAndTaxDataForCalculation dutyOrTax)
		{
			ZDecimal result;
			if (!dutyOrTax.ValueForCalculation.IsEmpty)
			{
				result = dutyOrTax.ValueForCalculation;
			}
			else
			{
				result = GetDutyOrTaxValue(dutyOrTax);
			}
			return GetValueString(result);
		}

		#endregion

		#region Combined Duty Type Description

		static string GetCombinedDutyTypeDescription(DutyAndTaxManager.CombinedDuty duty)
		{
			switch (duty.DutyType)
			{
				case DutyAndTaxManager.CombinedDuty.Type.Classification:
					return Res.GetString("13867a7b-4308-4f97-9432-1086fe7d5add", "Classification duty rate.");
				case DutyAndTaxManager.CombinedDuty.Type.Excise:
					return Res.GetString("607a95f4-d90f-4940-b801-60da28923750", "Excise duty rate.");
				case DutyAndTaxManager.CombinedDuty.Type.Tariff:
					return Res.GetString("9dbb33ba-0abe-4cbd-b43b-8d50cf44e6b9", "Tariff duty rate.");
				default:
					return string.Empty;
			}
		}

		#endregion

		#region Remission Description

		static string GetOneSixtiethRemissionDescription(IDutyAndTaxData data)
		{
			var result = string.Empty;
			if (data != null)
			{
				result = data.CalculationMethod != CalculationMethods.Codes.OneSixtiethRemission ? string.Empty
					: Res.GetString("6ab77e4a-4eb8-4208-a9c3-0e3a50f7574c", "The result is divided by 60 and multiplied by the monthly time limit ({0}).", data.MonthlyTimeLimit);
			}
			return result;
		}

		static string GetZeroForRemissionDescription(IDutyAndTaxData data)
		{
			var result = string.Empty;
			if (data != null)
			{
				result = Res.GetString("2bb7766f-98f5-4f64-9d0f-27561bc1124a", "0 (zero) for {0}.", new CalculationMethods().GetDescriptionFromCode(data.CalculationMethod).ToLower());
			}
			return result;
		}

		static string GetZeroForRemissionAndOICNumberDescription(IDutyAndTaxData data)
		{
			var result = string.Empty;
			if (data != null)
			{
				result = Res.GetString("43eab919-d514-4478-9229-8d1749c3d978", "0 (zero) for {0} and OIC number {1}.",
				new CalculationMethods().GetDescriptionFromCode(data.CalculationMethod).ToLower(CultureInfo.CurrentCulture), DutyAndTaxManager.TaxRemittedOICNumber);
			}
			return result;
		}

		static string GetGiftsUpTo60RemissionDescription(string effectiveValue, string valueDescription, string totalValueString, string rateDescription, string rateString)
		{
			return Res.GetString("73212B0A-E2FA-4B2A-A289-AE650E855240", "The first $60 of {0}({1}) is free, the rest ({2}) is multiplied by the {3} rate ({4}).", valueDescription, totalValueString, effectiveValue, rateDescription, rateString);
		}

		#endregion

		const string DecimalFormat = "0.#####";
		const string Dot = ".";
		internal const string Delimiter = " ";

		#endregion
	}
}
