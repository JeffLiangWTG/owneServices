using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGSPOCusTempStorageLineValidation : CusTempStorageLineValidation
	{
		public CHGSPOCusTempStorageLineValidation(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		protected new CHGSPOCusTempStorageLine Parent => (CHGSPOCusTempStorageLine)base.Parent;

		protected override void CheckTSL_LineNo()
		{
			base.CheckTSL_LineNo();
			CheckLineNoIsUnique();
		}

		protected override void CheckTSL_OwnerReferenceType()
		{
			base.CheckTSL_OwnerReferenceType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_OwnerReferenceTypeInfo);
		}

		protected override void CheckTSL_OwnerReferenceNumber()
		{
			base.CheckTSL_OwnerReferenceNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_OwnerReferenceNumberInfo);
		}

		protected override void CheckTSL_CustodianIdentifier()
		{
		}
	}
}
