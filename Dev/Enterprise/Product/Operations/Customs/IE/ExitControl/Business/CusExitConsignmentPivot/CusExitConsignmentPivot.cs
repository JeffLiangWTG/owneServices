using System.Data;
using CargoWise.EntityFramework;
using static Enterprise.Integration.Customs.IEExitControl;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusExitConsignmentPivot : EU.ExitControl.Business.CusExitConsignmentPivot, ICusExitConsignmentPivot
	{
		public CusExitConsignmentPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusExitConsignmentItem ConsignmentItem => (CusExitConsignmentItem)base.ConsignmentItem;
		public new CusExitConsignmentPackage Package => (CusExitConsignmentPackage)base.Package;
		public new CusExitContainer Container => (CusExitContainer)base.Container;
	}
}
