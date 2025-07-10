using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business
{
	public class CusLineTariffDetailValidation : EU.Business.CusLineTariffDetailValidation
	{
		public CusLineTariffDetailValidation(EU.Business.CusLineTariffDetail parent) : base(parent)
		{
		}

		public new CusLineTariffDetail Parent => (CusLineTariffDetail)base.Parent;

		protected override void CheckBZ_Type()
		{
			base.CheckBZ_Type();
			ListValidation.MessageErrorIfInvalidCode(Parent.BZ_TypeInfo);
		}

		protected override void CheckBZ_Tariff()
		{
			base.CheckBZ_Tariff();
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.BZ_TariffInfo, Parent.BZ_TypeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.BZ_TariffInfo);
		}

		protected override void CheckBZ_Qty1()
		{
			base.CheckBZ_Qty1();

			var parent = Parent;
			var qty = parent.BZ_Qty1;
			if (qty < ZDecimal.Zero)
			{
				MandatoryValidation.CheckNotNegative(parent.BZ_Qty1Info);
			}
			else if (qty.IsEmpty)
			{
				if (!parent.BZ_UQ1.IsEmpty)
				{
					parent.BZ_Qty1Info.AddMessageError(Res.GetString("2BC312E3-22BB-49A6-915E-BE5B742F5281", "Quantity should be greater than 0."));
				}
			}
			else if (qty > 100m)
			{
				var uq = parent.BZ_UQ1;
				if (uq == UniversalReferenceConstants.QuantityUnits.ASVPercentage || uq == UniversalReferenceConstants.QuantityUnits.ASV)
				{
					parent.BZ_Qty1Info.AddMessageError(Res.GetString("C05266CE-99AC-4E5F-B653-2BD34F2200E3", "The maximum value is 100."));
				}
			}
		}

		protected override void CheckBZ_UQ1()
		{
			base.CheckBZ_UQ1();

			var parent = Parent;
			var firstPartUnit = parent.RateFormulaUnits.FirstPartUnit;
			if (!parent.BZ_UQ1.EqualsIgnoringCase(firstPartUnit))
			{
				if (firstPartUnit.IsEmpty)
				{
					parent.BZ_UQ1Info.AddMessageError(Res.GetString("304FB033-BBE3-40BC-B2B0-42FE52368887", "Quantity Unit should be empty."));
				}
				else
				{
					parent.BZ_UQ1Info.AddMessageError(Res.GetString("40E758E2-E6A9-439B-8972-17E52C7E4F7A", "Quantity Unit should be {0}.", firstPartUnit));
				}
			}
		}

		protected override void CheckBZ_Qty2()
		{
			base.CheckBZ_Qty2();

			var parent = Parent;
			var qty2 = parent.BZ_Qty2;
			if (qty2 < ZDecimal.Zero)
			{
				MandatoryValidation.CheckNotNegative(parent.BZ_Qty2Info);
			}
			else if (qty2.IsEmpty && !parent.BZ_UQ2.IsEmpty)
			{
				parent.BZ_Qty2Info.AddMessageError(Res.GetString("70087CE4-E90D-4A81-AAF4-625CC1ED0A6B", "Quantity 2 should be greater than 0."));
			}
		}

		protected override void CheckBZ_UQ2()
		{
			base.CheckBZ_UQ2();

			var parent = Parent;
			var secondPartUnit = parent.RateFormulaUnits.SecondPartUnit;
			if (!parent.BZ_UQ2.EqualsIgnoringCase(secondPartUnit))
			{
				if (secondPartUnit.IsEmpty)
				{
					parent.BZ_UQ2Info.AddMessageError(Res.GetString("90EA342D-62D1-4FD6-BEE5-F56AAB3AB5DD", "Quantity Unit 2 should be empty."));
				}
				else
				{
					parent.BZ_UQ2Info.AddMessageError(Res.GetString("AB9F86A0-F814-46BC-9395-7CBF2C1E2777", "Quantity Unit 2 should be {0}.", secondPartUnit));
				}
			}
		}
	}
}
