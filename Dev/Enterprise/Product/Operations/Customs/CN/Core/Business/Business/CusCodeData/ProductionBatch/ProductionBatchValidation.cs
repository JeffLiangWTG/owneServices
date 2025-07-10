using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public class ProductionBatchValidation : Customs.Business.CusCodeDataValidation
	{
		public ProductionBatchValidation(ProductionBatch parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			MandatoryValidation.WarnIfNotEntered(Parent.CY_DataInfo);
		}
	}
}
