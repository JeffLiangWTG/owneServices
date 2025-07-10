using Enterprise.Customs.ExitControlBase.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitConsignmentPivotLookups : ExitControlBase.Business.CusExitConsignmentPivotLookups
	{
		public CusExitConsignmentPivotLookups(AutoCusExitConsignmentPivot parent)
			: base(parent)
		{
		}

		protected new CusExitConsignmentPivot Parent => (CusExitConsignmentPivot)base.Parent;
		protected CusExitConsignmentItem ConsignmentItem => Parent.ConsignmentItem;
		public ICusExitContainerCollection<CusExitContainer> CusExitContainers => ConsignmentItem?.Consignment?.Header?.CusExitContainers;
	}
}
