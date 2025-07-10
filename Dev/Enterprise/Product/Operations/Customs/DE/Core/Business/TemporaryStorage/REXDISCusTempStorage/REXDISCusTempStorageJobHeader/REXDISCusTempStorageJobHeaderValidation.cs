using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class REXDISCusTempStorageJobHeaderValidation : CusTempStorageJobHeaderValidation
	{
		public REXDISCusTempStorageJobHeaderValidation(CusTempStorageJobHeader header) : base(header)
		{
		}
		protected override void CheckSJH_DepartureDate()
		{
			if (Parent.IsAir)
			{
				base.CheckSJH_DepartureDate();
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SJH_DepartureDateInfo);
			}
		}
	}
}
