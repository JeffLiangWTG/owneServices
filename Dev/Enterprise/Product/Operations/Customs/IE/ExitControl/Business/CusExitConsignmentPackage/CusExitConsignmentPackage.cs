using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusExitConsignmentPackage : EU.ExitControl.Business.CusExitConsignmentPackage, Integration.Customs.IEExitControl.ICusExitConsignmentPackage
	{
		public CusExitConsignmentPackage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusExitConsignmentPackageValidation Validation => (CusExitConsignmentPackageValidation)base.Validation;

		protected override ExitControlBase.Business.CusExitConsignmentPackageValidation GetNewValidation() => new CusExitConsignmentPackageValidation(this);

		protected override ExitControlBase.Business.ICusExitReportItemCollection<ExitControlBase.Business.CusExitReportItem> CreateNewCusExitReportItemCollection() => new ExitControlBase.Business.CusExitReportItemCollection<CusExitReportItem>(this);
	}
}
