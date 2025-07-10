namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageLinePivotValidation : AutoCusTempStorageLinePivotValidation
	{
		public CusTempStorageLinePivotValidation(AutoCusTempStorageLinePivot parent)
			: base(parent)
		{
		}

		protected new CusTempStorageLinePivot Parent => (CusTempStorageLinePivot)base.Parent;
	}
}
