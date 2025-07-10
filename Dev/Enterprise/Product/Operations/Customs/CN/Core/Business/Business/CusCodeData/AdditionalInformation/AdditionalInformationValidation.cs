namespace Enterprise.Customs.CN.Business
{
	public class AdditionalInformationValidation : Customs.Business.CusCodeDataValidation
	{
		public AdditionalInformationValidation(AdditionalInformation parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
		}
	}
}
