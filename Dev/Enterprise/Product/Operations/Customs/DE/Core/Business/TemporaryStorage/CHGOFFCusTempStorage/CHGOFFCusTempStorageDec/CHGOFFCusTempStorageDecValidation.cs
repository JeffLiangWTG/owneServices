using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGOFFCusTempStorageDecValidation : CusTempStorageDecValidation
	{
		public CHGOFFCusTempStorageDecValidation(CHGOFFCusTempStorageDec parent) : base(parent)
		{
		}

		protected override void CheckSTH_OwnerReferenceNumber()
		{
			base.CheckSTH_OwnerReferenceNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.STH_OwnerReferenceNumberInfo);
		}

		protected new CHGOFFCusTempStorageDec Parent => (CHGOFFCusTempStorageDec)base.Parent;
	}
}
