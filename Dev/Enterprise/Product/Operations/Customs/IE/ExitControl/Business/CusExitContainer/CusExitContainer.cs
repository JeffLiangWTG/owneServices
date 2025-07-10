using System.Data;
using CargoWise.EntityFramework;
using static Enterprise.Integration.Customs.IEExitControl;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusExitContainer : EU.ExitControl.Business.CusExitContainer, ICusExitContainer
	{
		public CusExitContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ExitControlBase.Business.ICusExitConsignmentPivotCollection<ExitControlBase.Business.CusExitConsignmentPivot> CreateNewCusExitConsignmentPivotCollection() => new ExitControlBase.Business.CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this);
	}
}
