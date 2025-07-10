using System.Data;
using CargoWise.EntityFramework;
using static Enterprise.Integration.Customs.ESExitControl;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class CusExitConsignmentPivot : EU.ExitControl.Business.CusExitConsignmentPivot, ICusExitConsignmentPivot
	{
		public CusExitConsignmentPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new EU.ExitControl.Business.CusExitConsignmentItem ConsignmentItem => (CusExitConsignmentItem)base.ConsignmentItem;
		public new EU.ExitControl.Business.CusExitConsignmentPackage Package => (CusExitConsignmentPackage)base.Package;
		public new EU.ExitControl.Business.CusExitContainer Container => (CusExitContainer)base.Container;
	}
}
