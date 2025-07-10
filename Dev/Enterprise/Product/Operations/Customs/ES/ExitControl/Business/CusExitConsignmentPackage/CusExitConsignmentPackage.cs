using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.ExitControlBase.Business;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	sealed class CusExitConsignmentPackage : EU.ExitControl.Business.CusExitConsignmentPackage, Integration.Customs.ESExitControl.ICusExitConsignmentPackage
	{
		public CusExitConsignmentPackage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new EU.ExitControl.Business.CusExitConsignmentPackageLookups Lookups => base.Lookups;

		protected override bool CXP_SequenceReadOnly => false;

		public new CusExitConsignmentPackageValidation Validation => (CusExitConsignmentPackageValidation)base.Validation;

		protected override ExitControlBase.Business.CusExitConsignmentPackageValidation GetNewValidation() => new CusExitConsignmentPackageValidation(this);

		protected override ICusExitReportItemCollection<ExitControlBase.Business.CusExitReportItem> CreateNewCusExitReportItemCollection() => new CusExitReportItemCollection<CusExitReportItem>(this);

		protected override bool IsUCC6Core => Header?.IsUCC6 ?? true;
	}
}
