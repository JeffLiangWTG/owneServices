using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageLineValidation : AutoCusTempStorageLineValidation
	{
		public CusTempStorageLineValidation(AutoCusTempStorageLine parent)
			: base(parent)
		{
		}

		protected override void CheckTSL_LineNo()
		{
			base.CheckTSL_LineNo();
			MandatoryValidation.CheckNotNegative(Parent.TSL_LineNoInfo);
			MandatoryValidation.MessageErrorIfIsZero(Parent.TSL_LineNoInfo);
		}

		protected override void CheckTSL_PackageQty()
		{
			base.CheckTSL_PackageQty();
			MandatoryValidation.CheckNotNegative(Parent.TSL_PackageQtyInfo);
		}

		protected override void CheckTSL_GrossWeight()
		{
			base.CheckTSL_PackageQty();
			MandatoryValidation.CheckNotNegative(Parent.TSL_GrossWeightInfo);
		}

		protected override void CheckTSL_ReferenceNumberLine()
		{
			base.CheckTSL_ReferenceNumberLine();
			MandatoryValidation.CheckNotNegative(Parent.TSL_ReferenceNumberLineInfo);
		}

		protected override void CheckTSL_ReferenceNumber2Line()
		{
			base.CheckTSL_ReferenceNumber2Line();
			MandatoryValidation.CheckNotNegative(Parent.TSL_ReferenceNumber2LineInfo);
		}
	}
}
