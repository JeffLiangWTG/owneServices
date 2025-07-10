using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CUSPCSCusTempStorageDecValidation : CusTempStorageDecValidation
	{
		public CUSPCSCusTempStorageDecValidation(AutoCusTempStorageDec parent) : base(parent)
		{
		}

		public new CUSPCSCusTempStorageDec Parent => (CUSPCSCusTempStorageDec)base.Parent;

		protected override bool ShouldValidateRegistrationNumberLengthAndMrnFormatCore => false;
	}
}
