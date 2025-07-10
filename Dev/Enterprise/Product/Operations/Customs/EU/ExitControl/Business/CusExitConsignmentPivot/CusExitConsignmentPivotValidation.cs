using CargoWise.EntityFramework;
using Enterprise.Customs.ExitControlBase.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitConsignmentPivotValidation : ExitControlBase.Business.CusExitConsignmentPivotValidation
	{
		public CusExitConsignmentPivotValidation(AutoCusExitConsignmentPivot parent)
			: base(parent)
		{
		}
		protected new CusExitConsignmentPivot Parent => (CusExitConsignmentPivot)base.Parent;
		protected CusExitConsignmentItem ConsignmentItem => Parent.ConsignmentItem;
		protected CusExitHeader Header => ConsignmentItem?.Consignment?.Header;

		protected override void CheckCNP_CXN_Container()
		{
			base.CheckCNP_CXN_Container();
			if (Parent.CNP_CXN_Container.IsEmpty)
			{
				ValidateContainerIsEmpty(Header);
			}
			else
			{
				ListValidation.ErrorIfInvalidPK(Parent.CNP_CXN_ContainerInfo);
			}
		}

		protected virtual void ValidateContainerIsEmpty(CusExitHeader header)
		{
			var parent = Parent;
			if (parent.CNP_CXP_Package.IsValid && header is CusExitHeader)
			{
				(bool hasAnEquipment, bool hasANonEquipment) = header.CusExitContainers.Flags;
				if (hasANonEquipment && !hasAnEquipment)
				{
					parent.CNP_CXN_ContainerInfo.AddMessageError(ContainerOrEquipmentIsRequired);
				}
				else if (hasAnEquipment)
				{
					parent.CNP_CXN_ContainerInfo.AddWarning(ContainerOrEquipmentIsRequired);
				}
			}
		}

		public static string ContainerOrEquipmentIsRequired => Res.GetString("{72541338-EB51-4571-965F-941C13A4300F}", "It is mandatory to associate a package to its container. If a package is not in a container but in a conveyance and the conveyance is sealed, then it is also mandatory to associate the package to the correct conveyance.");
	}
}
