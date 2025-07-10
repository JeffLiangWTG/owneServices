namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class CusExitConsignmentPivotValidation : EU.ExitControl.Business.CusExitConsignmentPivotValidation
	{
		public CusExitConsignmentPivotValidation(CusExitConsignmentPivot parent) : base(parent)
		{
		}

		protected override void ValidateContainerIsEmpty(EU.ExitControl.Business.CusExitHeader header)
		{
			var parent = Parent;
			if (parent.CNP_CXP_Package.IsValid && header is CusExitHeader)
			{
				(bool hasAnEquipment, bool hasANonEquipment) = header.CusExitContainers.Flags;
				if ((hasANonEquipment && !hasAnEquipment) || hasAnEquipment)
				{
					parent.CNP_CXN_ContainerInfo.AddMessageError(Res.GetString("E52CA908-4E71-4F9E-A454-CB8460B0AAAF", "It is mandatory to associate a package to its container/equipment. If a package is not in a container, but in a transport equipment, then it is also mandatory to associate the package."));
				}
			}
		}
	}
}
