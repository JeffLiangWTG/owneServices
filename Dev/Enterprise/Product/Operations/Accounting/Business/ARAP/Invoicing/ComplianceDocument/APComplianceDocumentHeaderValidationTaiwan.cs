using CargoWise.ComponentModel;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.ComplianceDocument
{
	public class APComplianceDocumentHeaderValidationTaiwan : APComplianceDocumentHeaderValidation
	{
		public APComplianceDocumentHeaderValidationTaiwan(APComplianceDocumentHeader parent) : base(parent)
		{
		}

		protected override void CheckADH_DocumentNumber()
		{
			base.CheckADH_DocumentNumber();

			if (!Parent.ADH_DocumentNumberInfo.HasErrors() && Parent.IsAdded)
			{
				var error = AccComplianceDocumentHeaderDetailValidationHelper.ValidateDocumentNumberWithValidPrefix();
				if (!error.IsEmpty)
				{
					Parent.ADH_DocumentNumberInfo.AddError(error);
				}
			}
		}

		protected override bool CheckDuplicateDocumentNumber()
		{
			return AccComplianceDocumentHeaderDetailValidationHelper.GetDuplicateNumberComplianceDocument(Parent.ADH_TransactionType) != null;
		}
	}
}
