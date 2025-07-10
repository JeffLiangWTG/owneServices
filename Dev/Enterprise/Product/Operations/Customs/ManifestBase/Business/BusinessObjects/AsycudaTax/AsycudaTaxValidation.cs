using CargoWise.EntityFramework;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaTaxValidation : AutoAsycudaTaxValidation
	{
		public AsycudaTaxValidation(AutoAsycudaTax parent)
			: base(parent)
		{
		}

		protected new AsycudaTax Parent => (AsycudaTax)base.Parent;

		protected override void CheckAET_BaseValue()
		{
			base.CheckAET_BaseValue();
			var parent = Parent;
			if (parent.AET_BaseValue < 0)
			{
				parent.AET_BaseValueInfo.AddError(NegativeAmountNotAllowed);
			}
		}

		protected override void CheckAET_ChargeAmount()
		{
			base.CheckAET_ChargeAmount();
			var parent = Parent;
			if (parent.AET_ChargeAmount < 0)
			{
				parent.AET_ChargeAmountInfo.AddError(NegativeAmountNotAllowed);
			}
		}

		protected override void CheckAET_Rate()
		{
			base.CheckAET_Rate();
			var parent = Parent;
			if (parent.AET_Rate < 0)
			{
				parent.AET_RateInfo.AddError(NegativeAmountNotAllowed);
			}
		}

		protected override void CheckAET_RateOverrideReasonCode()
		{
			base.CheckAET_RateOverrideReasonCode();
			var parent = Parent;
			if (!parent.AET_RateOverrideReasonCode.IsEmpty && !parent.Lookups.RateOverrideReasonCodeList.ContainsCode(parent.AET_RateOverrideReasonCode))
			{
				parent.AET_RateOverrideReasonCodeInfo.AddError(CodeNotInList);
			}
		}

		protected override void CheckAET_MethodOfCalculation()
		{
			base.CheckAET_MethodOfCalculation();
			MandatoryValidation.CheckEntered(Parent.AET_MethodOfCalculationInfo);
		}

		public string NegativeAmountNotAllowed
		{
			get { return ResString.GetMultilingualString("B2BBD3B7-D92C-48A5-909A-16CB95EDA013", "Please enter a non-negative value."); }
		}

		public string CodeNotInList
		{
			get { return ResString.GetMultilingualString("9A809A09-FF37-4B7A-AC42-0F0308495D6B", "The code you have selected is not in the list."); }
		}
	}
}

