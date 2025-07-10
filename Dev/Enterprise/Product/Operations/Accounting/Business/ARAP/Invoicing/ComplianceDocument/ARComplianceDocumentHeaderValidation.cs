using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.ComplianceDocument
{
	public class ARComplianceDocumentHeaderValidation : AccComplianceDocumentHeaderValidation
	{
		public ARComplianceDocumentHeaderValidation(ARComplianceDocumentHeader parent) : base(parent)
		{
		}

		protected new ARComplianceDocumentHeader Parent => (ARComplianceDocumentHeader)base.Parent;

		protected override void CheckADH_DocumentDate()
		{
			base.CheckADH_DocumentDate();

			if (!Parent.ADH_DocumentDateInfo.HasErrors() && Parent.IsAdded)
			{
				var period = Parent.PeriodCalculator.GetPeriodFromDate(Parent.ADH_DocumentDate, GlbCompany.CurrentCompany.PK);
				if (Parent.PeriodCalculator.IsPeriodSubLedgerClosed(period))
				{
					Parent.ADH_DocumentDateInfo.AddError(PeriodValidation.SubLedgerPeriodClosedError);
				}
			}
		}
	}
}