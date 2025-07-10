using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.CusTempStorage
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

			if (Parent?.Dec != null && Parent.TSL_OwnerReferenceType == "AWB")
			{
				if (Parent.Dec.CusTempStorageLines.Find(new ZQuery(CusTempStorageLineSchema.TSL_OwnerReferenceType, Parent.TSL_OwnerReferenceType)).Length > 1)
				{
					Parent.TSL_OwnerReferenceTypeInfo.AddMessageError(ResString.GetMultilingualString("71F750EA-A27B-4F67-8FFC-22F832D6F7E9", "Only a single AWB is allowed on a declaration"));
				}
			}
		}
	}
}
