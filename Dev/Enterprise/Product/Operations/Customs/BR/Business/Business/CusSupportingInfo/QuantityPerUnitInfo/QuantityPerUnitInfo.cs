using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class QuantityPerUnitInfo : CusSupportingInfo
	{
		public QuantityPerUnitInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.QuantityPerUnit;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new QuantityPerUnitInfoValidation(this);

		public new QuantityPerUnitInfoValidation Validation => (QuantityPerUnitInfoValidation)base.Validation;

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;
	}
}
