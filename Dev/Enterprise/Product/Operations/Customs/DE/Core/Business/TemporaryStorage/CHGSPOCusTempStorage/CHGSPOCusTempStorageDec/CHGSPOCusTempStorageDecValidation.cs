using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGSPOCusTempStorageDecValidation : CusTempStorageDecValidation
	{
		public CHGSPOCusTempStorageDecValidation(CHGSPOCusTempStorageDec parent) : base(parent)
		{
		}

		protected override void CheckSTH_OwnerReferenceNumber()
		{
			base.CheckSTH_OwnerReferenceNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.STH_OwnerReferenceNumberInfo);
		}

		protected new CHGSPOCusTempStorageDec Parent => (CHGSPOCusTempStorageDec)base.Parent;
	}
}
