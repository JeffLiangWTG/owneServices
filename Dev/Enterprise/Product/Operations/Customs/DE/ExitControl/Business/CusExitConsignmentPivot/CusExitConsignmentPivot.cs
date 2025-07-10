using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class CusExitConsignmentPivot : EU.ExitControl.Business.CusExitConsignmentPivot
		, Integration.Customs.DEExitControl.ICusExitConsignmentPivot
	{
		public CusExitConsignmentPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusExitConsignmentPivotValidation Validation => (CusExitConsignmentPivotValidation)base.Validation;

		protected override ExitControlBase.Business.CusExitConsignmentPivotValidation GetNewValidation() => new CusExitConsignmentPivotValidation(this);
	}
}
