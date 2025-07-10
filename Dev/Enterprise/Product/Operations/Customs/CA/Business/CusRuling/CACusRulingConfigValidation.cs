using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using ConfigTypes = Enterprise.Customs.Universal.RefCusRulingConfigTypes.Codes;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CACusRulingConfigValidation : CusRulingConfigCombinedValidation
	{
		public CACusRulingConfigValidation(CACusRulingConfig parent) : base(parent)
		{
		}

		protected new CACusRulingConfig Parent
		{
			get { return (CACusRulingConfig)base.Parent; }
		}

		protected override void CheckZZY_Category()
		{
			base.CheckZZY_Category();
			if (!Parent.IsSystem && Parent.IsCategoryValid)
			{
				var configs = Parent.ParentConfigs.ToArray();
				ValidateCategoryAndTypeUniqueness(Parent.ZZY_CategoryInfo, configs);
				ValidateOtherRulingConfigs(x => x.Validation.ValidateZZY_Category(), configs);
			}
		}

		protected override void CheckZZY_Type()
		{
			base.CheckZZY_Type();
			if (!Parent.IsSystem && Parent.IsCategoryValid)
			{
				var configs = Parent.ParentConfigs.ToArray();
				if (Parent.ZZY_Type == ConfigTypes.AcceptRate)
				{
					if (!configs?.Any(x => x.ZZY_Category == Parent.ZZY_Category && (x.ZZY_Type == ConfigTypes.AdValorem || x.ZZY_Type == ConfigTypes.Specific)) ?? ZBool.False)
					{
						Parent.ZZY_TypeInfo.AddError(ResString.GetMultilingualString("18A025C5-7669-4A55-834F-91A7C64950F2",
							"At least a configuration with category {0} and type Ad-Valorem or Specific should be added when a configuration with category {0} and type Accept Rate entered.", Parent.CategoryDescription));
					}
				}
				else if (Parent.ZZY_Type == ConfigTypes.NoneFree)
				{
					if (configs.Any(x => x.PK != Parent.PK && x.ZZY_Category == Parent.ZZY_Category))
					{
						Parent.ZZY_TypeInfo.AddError(ResString.GetMultilingualString("BEF907DC-723A-4576-A357-B00C679950D2", "None/Free should be the only one {0} configuration if it entered. Please remove other {0} configurations.", Parent.CategoryDescription));
					}
				}

				ValidateTwoTypesCouldNotBeSelectedBoth(Parent.ZZY_TypeInfo, Parent.ZZY_Category, ConfigTypes.AcceptAmount, ConfigTypes.AcceptRate, configs);
				ValidateTwoTypesCouldNotBeSelectedBoth(Parent.ZZY_TypeInfo, Parent.ZZY_Category, ConfigTypes.AdValorem, ConfigTypes.Specific, configs);
				ValidateTwoTypesCouldNotBeSelectedBoth(Parent.ZZY_TypeInfo, Parent.ZZY_Category, ConfigTypes.AcceptAmount, ConfigTypes.Minimum, configs);
				ValidateTwoTypesCouldNotBeSelectedBoth(Parent.ZZY_TypeInfo, Parent.ZZY_Category, ConfigTypes.AcceptAmount, ConfigTypes.Maximum, configs);
				ValidateTwoTypesCouldNotBeSelectedBoth(Parent.ZZY_TypeInfo, Parent.ZZY_Category, ConfigTypes.DSD, ConfigTypes.REL, configs, ResString.GetMultilingualString("40C5F2D8-9582-4599-98ED-7384F944E7DE", "{0} and {1} options cannot both be selected with same Category {2}.", RefCusRulingConfigTypes.Descriptions.DSD, RefCusRulingConfigTypes.Descriptions.REL, Parent.CategoryDescription));

				ValidateCategoryAndTypeUniqueness(Parent.ZZY_TypeInfo, configs);
				ValidateOtherRulingConfigs(x => x.Validation.ValidateZZY_Type(), configs);
			}
		}

		protected override void CheckZZY_Rate()
		{
			base.CheckZZY_Rate();
			if (!Parent.IsSystem && !Parent.IsRateReadOnly && Parent.IsCategoryValid)
			{
				if (!Parent.ZZY_Rate.IsEmpty && Parent.ZZY_Rate < 1m)
				{
					Parent.ZZY_RateInfo.AddWarning(ResString.GetMultilingualString("9180399E-DDC2-4A9F-A5A6-85F3A34AC187", "Rates are expressed in percentages not decimals. The rate entered is less than 1%, please confirm that this is correct."));
				}

				if (Parent.ZZY_Type == ConfigTypes.Minimum || Parent.ZZY_Type == ConfigTypes.Maximum)
				{
					if (Parent.ZZY_Rate.IsEmpty && Parent.ZZY_Value.IsEmpty)
					{
						Parent.ZZY_RateInfo.AddError(EitherRateOrValueMustBeEnteredMessage);
					}
					else
					{
						ValidateMaxAndMinValueAndRate(Parent.ZZY_RateInfo, Parent.ZZY_Rate);
					}
				}
				else if (!Parent.ZZY_Rate.IsEmpty && Parent.ZZY_Type == ConfigTypes.AdValorem)
				{
					var configs = Parent.ParentConfigs.ToArray();
					if (configs.Any(x => x.ZZY_Category == Parent.ZZY_Category && x.ZZY_Type == ConfigTypes.AcceptRate))
					{
						var rate = Parent.ZZY_Rate;
						if (rate.DecimalPlaces > 1)
						{
							Parent.ZZY_RateInfo.AddError(ResString.GetMultilingualString("47E78254-D8E6-4D47-A00A-3E7C78FD751B", "Ad-Valorem {0} rate is up to have 1 decimal place when Accept Rate {0} entered.", Parent.CategoryDescription));
						}
					}
					if (Parent.ZZY_Rate > 100m && (Parent.IsDTY || Parent.IsGST || Parent.IsEXC))
					{
						Parent.ZZY_RateInfo.AddMessageError(ResString.GetMultilingualString("8598B6A9-D2EB-4D37-8FCD-D6EF40C42264", "Invalid Rate, rate cannot be greater than 100%."));
					}
				}
			}
		}

		protected override void CheckZZY_Value()
		{
			base.CheckZZY_Value();
			if (!Parent.IsSystem && !Parent.IsValueReadOnly && Parent.IsCategoryValid)
			{
				if (Parent.ZZY_Type == ConfigTypes.Minimum || Parent.ZZY_Type == ConfigTypes.Maximum || Parent.ZZY_Type == ConfigTypes.AcceptAmount)
				{
					var decimalValue = ZDecimal.Zero;
					var isDecimal = ZDecimal.TryParse(Parent.ZZY_Value, out decimalValue);
					if (!isDecimal || decimalValue.IsEmpty)
					{
						Parent.ZZY_ValueInfo.AddError(ResString.GetMultilingualString("33D225E6-C2EA-46AF-BD20-EC2BB161A590", "{0} cannot be zero", Parent.ZZY_ValueInfo.HumanReadableName));
					}

					if (Parent.ZZY_Type == ConfigTypes.Minimum || Parent.ZZY_Type == ConfigTypes.Maximum)
					{
						if (Parent.ZZY_Rate.IsEmpty && Parent.ZZY_Value.IsEmpty)
						{
							Parent.ZZY_ValueInfo.AddError(EitherRateOrValueMustBeEnteredMessage);
						}
						else
						{
							ValidateMaxAndMinValueAndRate(Parent.ZZY_ValueInfo, decimalValue);
						}
					}
				}
				else if ((Parent.IsDTY && Parent.ZZY_Type == ConfigTypes.TreatmentCode) || ((Parent.IsGST || Parent.IsSIM || Parent.IsEXC) && Parent.ZZY_Type == ConfigTypes.ExemptCode))
				{
					MandatoryValidation.CheckEntered(Parent.ZZY_ValueInfo);
					ListValidation.ErrorIfInvalidCode(Parent.ZZY_ValueInfo);
				}
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		void ValidateMaxAndMinValueAndRate(ZPropertyInfo info, ZDecimal value)
		{
			if (!value.IsEmpty)
			{
				var otherMaxOrMin = Parent.ParentConfigs.FirstOrDefault(x => x.PK != Parent.PK && x.ZZY_Category == Parent.ZZY_Category && x.ZZY_Type != Parent.ZZY_Type
									&& (x.ZZY_Type == ConfigTypes.Minimum || x.ZZY_Type == ConfigTypes.Maximum));

				if (otherMaxOrMin != null)
				{
					var decimalValueToCompare = ZDecimal.Zero;
					if (info.Name == CACusRulingConfig.Schema.ZZY_Value)
					{
						ZDecimal.TryParse(otherMaxOrMin.ZZY_Value, out decimalValueToCompare);
						otherMaxOrMin.Validation.ValidateZZY_Value();
					}
					else if (info.Name == CACusRulingConfig.Schema.ZZY_Rate)
					{
						decimalValueToCompare = otherMaxOrMin.ZZY_Rate;
						otherMaxOrMin.Validation.ValidateZZY_Rate();
					}
					if (!decimalValueToCompare.IsEmpty && Parent.ZZY_Type == ConfigTypes.Minimum ? value > decimalValueToCompare : value < decimalValueToCompare)
					{
						info.AddError(ResString.GetMultilingualString("409C997F-6186-4C28-B34D-3A52F5283A14", "{0} entered for {1} must be {2}.", info.HumanReadableName, Parent.ZZY_Type,
							Parent.ZZY_Type == ConfigTypes.Minimum ? ResString.GetMultilingualString("596A24BA-F6AC-454F-903F-1196D373B654", "less than or equal to {0}: {1}", ConfigTypes.Maximum, decimalValueToCompare)
							: ResString.GetMultilingualString("547AB1F5-45ED-41ED-8F7F-16B3EEC47D64", "greater than or equal to {0}: {1}", ConfigTypes.Minimum, decimalValueToCompare)));
					}
				}
			}
		}

		ZString EitherRateOrValueMustBeEnteredMessage => ResString.GetMultilingualString("59EFC713-9E3F-4627-8FA8-1EB8265F39FE", "Either Rate or Value must be entered.");
	}
}
