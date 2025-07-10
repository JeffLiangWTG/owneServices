namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class BaseCusOutturnValidation : Customs.Business.CusOutturnValidation
	{
		public BaseCusOutturnValidation(CusOutturn outturn)
			: base(outturn)
		{
			this.Outturn = outturn;
		}

		protected readonly CusOutturn Outturn;

		protected override void CheckC5_GoodsDescription()
		{
			base.CheckC5_GoodsDescription();
			if (Outturn.C5_GoodsDescription.IsEmpty &&
				(Outturn.C5_OutturnResultType == CMROutturnResultType.Codes.SurplusPackages ||
				 Outturn.C5_OutturnResultType == CMROutturnResultType.Codes.SurplusConsignment))
			{
				Outturn.C5_GoodsDescriptionInfo.AddError("Goods description is mandatory for Surplus Consignment or Surplus Packages.");
			}
		}

		#region Implementation

		protected MessageValidation MessageValidation
		{
			get
			{
				if (fMessageValidation == null)
				{
					fMessageValidation = new MessageValidation(Parent);
				}
				return fMessageValidation;
			}
		}
		MessageValidation fMessageValidation;

		#endregion
	}
}
