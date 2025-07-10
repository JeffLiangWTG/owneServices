using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageLineValidation : EU.Business.CusTempStorage.CusTempStorageLineValidation
	{
		public CusTempStorageLineValidation(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		protected new CusTempStorageLine Parent => (CusTempStorageLine)base.Parent;

		protected override void CheckTSL_OwnerReferenceType()
		{
			base.CheckTSL_OwnerReferenceType();

			if (Parent?.Dec != null && Parent.TSL_OwnerReferenceType == OwnerReferenceTypeList.Codes.ORT_AWB)
			{
				if (Parent.Dec.CusTempStorageLines.Find(new ZQuery(CusTempStorageLineSchema.TSL_OwnerReferenceType, Parent.TSL_OwnerReferenceType)).Length > 1)
				{
					Parent.TSL_OwnerReferenceTypeInfo.AddMessageError(DuplicateOwnerReferenceTypeError);
				}
			}
		}

		protected override void CheckTSL_PackageQty()
		{
			base.CheckTSL_PackageQty();

			if (Parent.TSL_PackageQty == 0)
			{
				Parent.TSL_PackageQtyInfo.AddError(PackageQtyError);
			}
		}

		static MultilingualString DuplicateOwnerReferenceTypeError => ResString.GetMultilingualString("1C9757D1-6C2E-40E8-966B-41743188480C", "Only a single AWB is allowed on a declaration");
		static MultilingualString PackageQtyError => ResString.GetMultilingualString("EDC25005-4877-4EEF-A9B9-0A22F94B4973", "Package quantity must be greater than 0");
	}
}
