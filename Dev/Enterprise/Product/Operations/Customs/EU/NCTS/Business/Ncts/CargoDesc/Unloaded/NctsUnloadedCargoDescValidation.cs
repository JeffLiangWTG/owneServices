using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsUnloadedCargoDescValidation : NctsCommonCargoDescValidation
	{
		public NctsUnloadedCargoDescValidation(NctsUnloadedCargoDesc parent)
			: base(parent)
		{
		}

		protected override void CheckBY_CusC4Number()
		{
			base.CheckBY_CusC4Number();
			CheckBY_CusC4Number_ListValidation();
		}

		protected override void CheckBY_HarmonisedTariff()
		{
			base.CheckBY_HarmonisedTariff();
			var parent = (NctsUnloadedCargoDesc)Parent;
			if (parent.ArrivalCargoDescParent?.IsUnloadedCommodityCodeRequired ?? true)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.BY_HarmonisedTariffInfo);
			}

			if (parent.ArrivalCargoDescParent?.ValidationDecider is INctsArrivalCargoDescPhase5ValidationDecider validationDecider)
			{
				if (validationDecider.IsRuleNR0004Active)
				{
					parent.CheckRuleNR0004(parent.BY_HarmonisedTariffInfo);
				}
				if (validationDecider.IsRuleNR0055Active)
				{
					parent.CheckRuleNR0055(parent.BY_HarmonisedTariffInfo);
				}
			}

			CheckBY_HarmonisedTariffIsValid();
		}

		protected virtual void CheckBY_HarmonisedTariffIsValid()
		{
			var parent = (NctsUnloadedCargoDesc)Parent;
			if (parent.UniversalTariff == null)
			{
				parent.BY_HarmonisedTariffInfo.AddMessageError(ListValidation.InvalidCodeMessageError.ToString());
			}
		}

		protected override void CheckBY_Description()
		{
			base.CheckBY_Description();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_DescriptionInfo);
		}

		protected override void CheckBY_GrossWeightUnit()
		{
			base.CheckBY_GrossWeightUnit();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_GrossWeightUnitInfo);
		}

		protected override void CheckBY_NetWeightUnit()
		{
			base.CheckBY_NetWeightUnit();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_NetWeightUnitInfo);
		}
	}
}
