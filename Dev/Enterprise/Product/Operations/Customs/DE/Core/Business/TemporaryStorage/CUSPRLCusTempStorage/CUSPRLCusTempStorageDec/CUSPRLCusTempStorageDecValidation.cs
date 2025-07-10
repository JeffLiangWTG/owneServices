using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CUSPRLCusTempStorageDecValidation : CusTempStorageDecValidation
	{
		public CUSPRLCusTempStorageDecValidation(AutoCusTempStorageDec parent) : base(parent)
		{
		}

		public new CUSPRLCusTempStorageDec Parent => (CUSPRLCusTempStorageDec)base.Parent;

		protected override bool ShouldValidateRegistrationNumberLengthAndMrnFormatCore => false;

		protected override void CheckSTH_IdentificationIndicator()
		{
			//Not used for CUSPRL
		}
	}
}
