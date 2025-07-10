using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class CusContainerValidation : Customs.Business.CusContainerValidation
	{
		public CusContainerValidation(AutoCusContainer parent) : base(parent)
		{
		}

		protected override void CheckCO_ContainerNumber()
		{
			base.CheckCO_ContainerNumber();
			if (Parent.Declaration.IsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CO_ContainerNumberInfo);
			}
		}
	}
}
