using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusExitReportItem : EU.ExitControl.Business.CusExitReportItem
	{
		public CusExitReportItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZGuid ERI_CCI_ConsignmentItem
		{
			get => base.ERI_CCI_ConsignmentItem;
			set
			{
				var oldValue = ERI_CCI_ConsignmentItem;
				base.ERI_CCI_ConsignmentItem = value;
				if (!IsValidationSuspended && !IsCopying && oldValue != ERI_CCI_ConsignmentItem && Validation is CusExitReportItemValidation validation)
				{
					validation.ValidateERI_GrossMass();
					validation.ValidateERI_NetMass();
					validation.ValidateERI_Quantity();
				}
			}
		}

		public new CusExitConsignmentPackage Package => (CusExitConsignmentPackage)base.Package;

		public new CusExitReportItemValidation Validation => (CusExitReportItemValidation)base.Validation;

		protected override ExitControlBase.Business.CusExitReportItemValidation GetNewValidation() => new CusExitReportItemValidation(this);

		protected override EU.ExitControl.Business.IAdditionalInfoCollection<EU.ExitControl.Business.AdditionalInfo> CreateNewAdditionalInfoCollection() => new EU.ExitControl.Business.AdditionalInfoCollection<AdditionalInfo>(this);
	}
}
