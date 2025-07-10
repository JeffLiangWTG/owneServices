using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EdificeCusContainerValidation : CusContainerValidation
	{
		public EdificeCusContainerValidation(CusContainer parent)
			: base(parent)
		{
		}

		protected override void CheckCO_FCL_LCL_AIR()
		{
			base.CheckCO_FCL_LCL_AIR();
			if (Parent.CO_FCL_LCL_AIR.IsEmpty && Parent.Declaration != null && Parent.Declaration.IsImport)
			{
				Parent.CO_FCL_LCL_AIRInfo.AddMessageError("Container mode required.");
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CO_FCL_LCL_AIRInfo, Parent.CO_FCL_LCL_NCT_List);
			}
		}
	}
}
