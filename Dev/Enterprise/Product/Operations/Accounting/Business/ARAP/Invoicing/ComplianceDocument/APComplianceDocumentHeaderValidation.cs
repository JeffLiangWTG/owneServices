using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.ComplianceDocument
{
	public class APComplianceDocumentHeaderValidation : AccComplianceDocumentHeaderValidation
	{
		public APComplianceDocumentHeaderValidation(APComplianceDocumentHeader parent) : base(parent)
		{
		}

		protected new APComplianceDocumentHeader Parent => (APComplianceDocumentHeader)base.Parent;

		protected override void CheckADH_DocumentNumber()
		{
			base.CheckADH_DocumentNumber();

			if (!Parent.ADH_DocumentNumberInfo.HasErrors() && AccComplianceDocumentHeaderDetailValidationHelper.ShouldValidateDuplicateNumber && CheckDuplicateDocumentNumber())
			{
				Parent.ADH_DocumentNumberInfo.AddError(AccComplianceDocumentHeaderDetailValidationHelper.DuplicateNumberError);
			}
		}

		protected virtual bool CheckDuplicateDocumentNumber()
		{
			return AccComplianceDocumentHeaderDetailValidationHelper.GetDuplicateNumberComplianceDocument(Parent.ADH_TransactionType, Parent.Organisation?.PK ?? ZGuid.Empty) != null;
		}
	}
}
