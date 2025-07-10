using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class PRLCONCusTempStorageDecValidation : CusTempStorageDecValidation
	{
		public PRLCONCusTempStorageDecValidation(AutoCusTempStorageDec parent) : base(parent)
		{
		}

		protected new PRLCONCusTempStorageDec Parent => (PRLCONCusTempStorageDec)base.Parent;

		protected override bool ShouldValidateRegistrationNumberLengthAndMrnFormatCore => false;

		protected override void CheckSTH_IdentificationIndicator()
		{
			base.CheckSTH_IdentificationIndicator();
			if (Parent.IsREGDeclaration && Parent.CusTempStorageLines.Count < 2)
			{
				Parent.STH_IdentificationIndicatorInfo.AddMessageError(Res.GetString("CF631FCB-649B-4E46-B953-C0E9A824A779", "For Identification Type 'REG' a minimum of two lines are required to consolidate"));
			}
		}
	}
}
