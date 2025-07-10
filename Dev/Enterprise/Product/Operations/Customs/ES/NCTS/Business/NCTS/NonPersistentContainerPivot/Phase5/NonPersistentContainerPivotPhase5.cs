namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NonPersistentContainerPivotPhase5 : EU.NCTS.Business.NonPersistentContainerPivotPhase5
	{
		public NonPersistentContainerPivotPhase5(NctsPackage package, EU.NCTS.Business.NctsCusInBondContainer container) : base(package, container)
		{
		}

		protected override bool ContainerSelected_ReadOnly
		{
			get
			{
				var esPackage = (NctsPackage)package;
				return esPackage.B5_TypeOfDifference == EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF ||
					base.ContainerSelected_ReadOnly ||
					esPackage.IsUnloadingRemarksReadOnlySpain;
			}
		}
	}
}
