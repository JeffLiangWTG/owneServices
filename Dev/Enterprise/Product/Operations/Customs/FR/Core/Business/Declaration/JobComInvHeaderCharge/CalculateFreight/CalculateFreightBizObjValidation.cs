using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CalculateFreightBizObjValidation : EU.Business.Declaration.CalculateFreightBizObjValidation
	{
		public CalculateFreightBizObjValidation(EU.Business.Declaration.AutoCalculateFreightBizObj parent)
			: base(parent)
		{
		}

		public new CalculateFreightBizObj Parent => (CalculateFreightBizObj)base.Parent;

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();

			ValidatePercentageInEUBorder();
			ValidatePercentageDomestic();
			ValidateInsuranceAmount();
			ValidateInsuranceCurrency();
		}

		public void ValidateInsuranceAmount()
		{
			ValidateCalculatedProperty(Parent.InsuranceAmountInfo);
		}

		protected void CheckInsuranceAmount()
		{
			if (Parent.InsuranceAmount.IsEmpty && Parent.Amount.IsEmpty)
			{
				Parent.InsuranceAmountInfo.AddError(Res.GetString("8e317f1f-c562-40e6-8ec2-3811e5acbfc0", "The freight and insurance amounts cannot both be 0"));
			}
		}

		public void ValidateInsuranceCurrency()
		{
			ValidateCalculatedProperty(Parent.InsuranceCurrencyInfo);
		}

		protected void CheckInsuranceCurrency()
		{
			MandatoryValidation.CheckEntered(Parent.InsuranceCurrencyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.InsuranceCurrencyInfo);
			if (Parent.InsuranceCurrency.IsEmpty && Parent.InsuranceAmount == 0m)
			{
				Parent.InsuranceCurrencyInfo.AddError(InconsistentInsuranceCurrencies);
			}
		}

		protected override void CheckPercentage()
		{
			base.CheckPercentage();

			if (!CheckTotalPercentage())
			{
				Parent.PercentageInfo.AddError(PercentageShouldBe100Totally);
			}
		}

		public void ValidatePercentageInEUBorder()
		{
			ValidateCalculatedProperty(Parent.PercentageInEUBorderInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "ZAttribute validation method called by reflection. Original of the call from ValidatePercentageInEUBorder")]
		void CheckPercentageInEUBorder()
		{
			var propertyInfo = Parent.PercentageInEUBorderInfo;
			if (!Parent.PercentageInEUBorder.IsInRange(ZDecimal.Zero, 100m))
			{
				propertyInfo.AddError(PercentageShouldBeBetween);
			}

			if (!CheckTotalPercentage())
			{
				propertyInfo.AddError(PercentageShouldBe100Totally);
			}
		}

		public void ValidatePercentageDomestic()
		{
			ValidateCalculatedProperty(Parent.PercentageDomesticInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "ZAttribute validation method called by reflection. Original of the call from ValidatePercentageDomestic")]
		void CheckPercentageDomestic()
		{
			var propertyInfo = Parent.PercentageDomesticInfo;
			if (!Parent.PercentageDomestic.IsInRange(ZDecimal.Zero, 100m))
			{
				propertyInfo.AddError(PercentageShouldBeBetween);
			}

			if (!CheckTotalPercentage())
			{
				propertyInfo.AddError(PercentageShouldBe100Totally);
			}
		}

		ZBool CheckTotalPercentage()
		{
			return Parent.Percentage + Parent.PercentageInEUBorder + Parent.PercentageDomestic == 100m;
		}

		public static string PercentageShouldBe100Totally => Res.GetString("50CCD1B5-8688-4658-9C4B-35570F7D00CE", "Percentage before value and percentage in value and percentage domestic value should be 100 totally.");

		public static string InconsistentInsuranceCurrencies => Res.GetString("2AF8A99C-BF35-4443-8F44-59A4C875C1D5", "Insurance charge currencies inconsistency detected in charge grid.");
	}
}
