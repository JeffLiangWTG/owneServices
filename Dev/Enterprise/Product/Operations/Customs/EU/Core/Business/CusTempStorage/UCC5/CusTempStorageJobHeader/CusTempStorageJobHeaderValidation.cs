using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageJobHeaderValidation : AutoCusTempStorageJobHeaderValidation
	{
		public CusTempStorageJobHeaderValidation(AutoCusTempStorageJobHeader parent)
			: base(parent)
		{
		}

		protected override void CheckSJH_ContainerCount()
		{
			base.CheckSJH_ContainerCount();
			MandatoryValidation.CheckNotNegative(Parent.SJH_ContainerCountInfo);
		}
	}
}
