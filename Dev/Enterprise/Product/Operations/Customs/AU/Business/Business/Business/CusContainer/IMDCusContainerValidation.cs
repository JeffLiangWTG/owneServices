using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class IMDCusContainerValidation : CusContainerValidation
	{
		public IMDCusContainerValidation(CusContainer parent)
			: base(parent)
		{
		}

		protected override void CheckCO_ContainerNumber()
		{
			if (Parent.Declaration != null && !Parent.CO_ContainerNumber.IsEmpty)
			{
				if (Parent.Declaration.IsPost)
				{
					Parent.CO_ContainerNumberInfo.AddMessageError("You cannot have containers on an Mail Customs Job.");
				}
				else
				{
					base.CheckCO_ContainerNumber();
				}

				bool isPacked = false;
				if (!Parent.JobDeclaration.IsSAC)
				{
					foreach (PackingGroup pack in Parent.Declaration.PackingGroups)
					{
						if (pack != null && pack.Container != null &&
							pack.Container.CO_ContainerNumber == Parent.CO_ContainerNumber)
						{
							isPacked = true;
							break;
						}
					}
					if (!isPacked)
					{
						Parent.CO_ContainerNumberInfo.AddMessageError("This container is not selected in the packing section. It will not be sent in a message.");
					}
				}
			}
		}

		protected override void CheckCO_FCL_LCL_AIR()
		{
			base.CheckCO_FCL_LCL_AIR();
			if (Parent.JobDeclaration.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CO_FCL_LCL_AIRInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.CO_FCL_LCL_AIRInfo, Parent.Lookups.CO_FCL_LCL_NCT_List);
		}
	}
}
