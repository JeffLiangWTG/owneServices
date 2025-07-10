namespace Enterprise.Customs.DE.Business
{
	public class AddInfoCusLineTariffDetailValidation : EU.Business.AddInfoCusLineTariffDetailValidation
	{
		public AddInfoCusLineTariffDetailValidation(AddInfoCusLineTariffDetail parent)
			: base(parent)
		{
		}

		protected override void CheckZG_AlcoholicStrength()
		{
			base.CheckZG_AlcoholicStrength();
			var parent = Parent;
			var info = parent.ZG_AlcoholicStrengthInfo;
			if (CusLineTariffDetailHelper.IsPercentAlcoholMandatory(parent.Parent))
			{
				if (!parent.ZG_AlcoholicStrength.IsInRange(0.01, 100))
				{
					info.AddMessageError(Res.GetString("D698C524-7213-4D60-B2A5-A66B6823E718", "The 'Degree Percentage' must be between 0,01% and 100%"));
				}
			}
		}

		public new AddInfoCusLineTariffDetail Parent => (AddInfoCusLineTariffDetail)base.Parent;
	}
}
