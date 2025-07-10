using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusStorageDocPivotValidation : Customs.Business.CusStorageDocPivotValidation
	{
		public CusStorageDocPivotValidation(AutoCusStorageDocPivot parent) : base(parent)
		{
		}

		new CusStorageDocPivot Parent => (CusStorageDocPivot)base.Parent;

		protected override void CheckCSD_StorageDocReference()
		{
			base.CheckCSD_StorageDocReference();
			ListValidation.ErrorIfInvalidPK(Parent.CSD_StorageDocReferenceInfo);
		}

		protected override void CheckCSD_DocType()
		{
			base.CheckCSD_DocType();
			MandatoryValidation.CheckEntered(Parent.CSD_DocTypeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.CSD_DocTypeInfo);
		}

		protected override void CheckCSD_Description()
		{
			base.CheckCSD_Description();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSD_DescriptionInfo);
		}
	}
}
