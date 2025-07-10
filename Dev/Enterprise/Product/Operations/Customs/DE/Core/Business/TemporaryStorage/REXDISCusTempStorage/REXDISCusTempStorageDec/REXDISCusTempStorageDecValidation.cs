using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class REXDISCusTempStorageDecValidation : CusTempStorageDecValidation
	{
		public REXDISCusTempStorageDecValidation(AutoCusTempStorageDec parent) : base(parent)
		{
		}

		public new REXDISCusTempStorageDec Parent => (REXDISCusTempStorageDec)base.Parent;

		protected override bool ShouldValidateRegistrationNumberLengthAndMrnFormatCore => false;

		protected override void CheckSTH_DeclarationSubType()
		{
			base.CheckSTH_DeclarationSubType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.STH_DeclarationSubTypeInfo);
		}
	}
}
