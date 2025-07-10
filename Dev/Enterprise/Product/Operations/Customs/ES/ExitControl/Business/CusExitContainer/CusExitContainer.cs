using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.ExitControlBase.Business;
using static Enterprise.Integration.Customs.ESExitControl;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class CusExitContainer : EU.ExitControl.Business.CusExitContainer, ICusExitContainer, ICusSealTypeSupporter
	{
		public CusExitContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusExitContainerValidation Validation => (CusExitContainerValidation)base.Validation;

		protected override ExitControlBase.Business.CusExitContainerValidation GetNewValidation() => new CusExitContainerValidation(this);

		protected override EU.ExitControl.Business.CusExitSealCollection CreateNewCusExitSealCollection() => new CusExitSealCollection(this);

		protected override ICusExitConsignmentPivotCollection<ExitControlBase.Business.CusExitConsignmentPivot> CreateNewCusExitConsignmentPivotCollection() => new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this);

		protected override Type CusSealTypeCore => typeof(CusExitSeal);

		protected override bool IsUCC6Core => Header?.IsUCC6 ?? true;
	}
}
