using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public abstract class NctsCommonCargoDescValidation : Customs.Business.CusInBondCargoDescValidation
	{
		public NctsCommonCargoDescValidation(NctsCommonCargoDesc parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateBY_Supplements();
		}

		public void ValidateBY_Supplements()
		{
			ValidateCalculatedProperty(Parent.BY_SupplementsInfo);
		}

		protected void CheckBY_CusC4Number_ListValidation()
		{
			var cusC4Number = Parent.BY_CusC4Number;
			if (!cusC4Number.IsEmpty)
			{
				var harmonisedTariff = Parent.BY_HarmonisedTariff;
				if (!harmonisedTariff.IsEmpty)
				{
					var cnCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Parent.Factory, cusC4Number, Parent.Lookups.CusCodeListDataGroupingCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, Parent.ValuationDate, attributeFilters: new[]
					{
						new RefCusCodeListAttributeFilter(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CombinedNomenclatureCode, SQLComparisonOperator.StartsWith, harmonisedTariff.SubstringSafe(0, 6))
					});
					if (cnCode == null)
					{
						Parent.BY_CusC4NumberInfo.AddMessageError(Res.GetString("01F3423A-C7AA-40B7-9661-A6E8FE773B06", "{0} does not belong to the commodity code", Parent.BY_CusC4NumberInfo.HumanReadableName));
					}
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.BY_CusC4NumberInfo);
				}
			}
		}

		protected override void CheckBY_HarmonisedTariff()
		{
			base.CheckBY_HarmonisedTariff();
			if (Parent.Header != null)
			{
				CheckConditionC015(Parent.BY_HarmonisedTariffInfo);
				BY_HarmonisedTariffCharacterCheck();
			}

			void CheckConditionC015(ZPropertyInfo info)
			{
				if (Parent.SgiCodes.Any() && Parent.BY_HarmonisedTariff.IsEmpty)
				{
					info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.C015, Res.GetString("77D996B9-A2D3-4B26-8C34-9651C231C0D3", "Commodity Code is required for Safety and Security movements.")));
				}
			}
		}

		protected virtual void BY_HarmonisedTariffCharacterCheck()
		{
			CheckConditionR060(Parent, Parent.BY_HarmonisedTariffInfo);

			void CheckConditionR060(NctsCommonCargoDesc goodsItem, ZPropertyInfo info)
			{
				var commodityCode = goodsItem.BY_HarmonisedTariff;
				if (!commodityCode.IsEmpty && (commodityCode.Length < 4 || commodityCode.Length > 10))
				{
					info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.R060, Res.GetString("ABE65C06-5F79-4E17-BAB9-383E00005059", "Commodity Code must be at least 4 and up to 10 digits of the full commodity code.")));
				}
			}
		}

		protected override void CheckBY_GrossWeightUnit()
		{
			base.CheckBY_GrossWeightUnit();
			if (Parent.Header != null)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.BY_GrossWeightUnitInfo, Parent.Lookups.WeightUnitList);
			}
		}

		protected override void CheckBY_NetWeight()
		{
			base.CheckBY_NetWeight();

			var netWeight = Parent.BY_NetWeight;
			var netWeightUnit = Parent.BY_NetWeightUnit;
			var grossWeight = Parent.BY_GrossWeight;
			var grossWeightUnit = Parent.BY_GrossWeightUnit;
			if (!netWeight.IsEmpty && !netWeightUnit.IsEmpty && !grossWeight.IsEmpty && !grossWeightUnit.IsEmpty && Core.Constants.Weight.ContainsCode(grossWeightUnit) && Core.Constants.Weight.ContainsCode(netWeightUnit))
			{
				var netZWeight = new ZWeight(netWeight, netWeightUnit);
				var grossZWeight = new ZWeight(grossWeight, grossWeightUnit);
				if (grossZWeight.IsValid && netZWeight.IsValid && grossZWeight < netZWeight)
				{
					AddR0223Notification(Parent.BY_NetWeightInfo);
				}
			}
		}

		protected virtual void AddR0223Notification(ZPropertyInfo netWeightInfo)
		{
			netWeightInfo.AddWarning(Res.GetString("C592CDFB-76BD-4746-B9EE-B9745914BC8A", "If Gross Weight is not 0, then it must be greater or equal to Net Weight."));
		}

		protected override void CheckBY_NetWeightUnit()
		{
			base.CheckBY_NetWeightUnit();
			if (Parent.Header != null)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.BY_NetWeightUnitInfo, Parent.Lookups.WeightUnitList);
			}
		}

		protected virtual void CheckBY_Supplements()
		{
		}

		protected new NctsCommonCargoDesc Parent => (NctsCommonCargoDesc)base.Parent;
	}
}
