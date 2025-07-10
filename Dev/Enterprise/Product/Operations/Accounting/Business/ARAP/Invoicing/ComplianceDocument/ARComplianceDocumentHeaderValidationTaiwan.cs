using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.ComplianceDocument
{
	public class ARComplianceDocumentHeaderValidationTaiwan : ARComplianceDocumentHeaderValidation
	{
		public ARComplianceDocumentHeaderValidationTaiwan(ARComplianceDocumentHeader parent) : base(parent)
		{
		}

		protected override void CheckADH_DocumentDate()
		{
			base.CheckADH_DocumentDate();
			if (Parent.IsAdded)
			{
				if (!Parent.ADH_DocumentDateInfo.HasErrors() && Parent.ADH_DocumentDate.Date != ZDateTime.Today)
				{
					if (Parent.ADH_ComplianceSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE)
					{
						Parent.ADH_DocumentDateInfo.AddError(Res.GetString("3F15980B-647D-420D-A94B-1F2229F56F8E", "This date cannot be changed for TXE compliance sub type. Please set to today date."));
					}

					if (!Parent.ADH_DocumentNumberInfo.HasErrors() && Parent.ADH_TransactionType == TransactionTypes.CreditNote)
					{
						Parent.ADH_DocumentDateInfo.AddError(Res.GetString("4ADFC18B-257E-471B-A159-DDC86D0A0AC2", "This date cannot be changed for CRD compliance document created in Taiwan Login Company. Please set to today date."));
					}
				}
			}
		}

		protected override void CheckADH_DocumentNumber()
		{
			base.CheckADH_DocumentNumber();

			if (!Parent.ADH_DocumentNumberInfo.HasErrors() && Parent.IsAdded)
			{
				if (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.Value
					&& Parent.ADH_TransactionType == TransactionTypes.CreditNote
					&& AccComplianceDocumentHeaderDetailValidationHelper.SubTypeListForCheckDocumentNumber.Contains(Parent.ADH_ComplianceSubType))
				{
					var error = AccComplianceDocumentHeaderDetailValidationHelper.ValidateDocumentNumberWithValidPrefixBasic();
					if (!error.IsEmpty)
					{
						Parent.ADH_DocumentNumberInfo.AddError(error);
					}
				}
			}
		}
	}
}