using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APCreditNoteValidation : CreditNoteValidation
	{
		public APCreditNoteValidation(APCreditNote parent)
			: base(parent)
		{
		}

		protected override BooleanRegistryItem OriginalInvoiceDetailsMandatoryRegistryItem => AccountingConfigurationRegistry.Instance.OriginalInvoiceDetailsMandatoryOnAPCreditNotes;

		protected override void CheckValidateExpectedInvoiceTotal()
		{
			if (!Env.Security.AllowAPCreditNoteChangeDefaultExpectedTotalValue.IsAllowed)
			{
				CheckValidateExpectedInvoiceTotalCore();
			}
		}

		protected override void CheckAH_OriginalInvoiceDate()
		{
			base.CheckAH_OriginalInvoiceDate();

			if (IsValidationEnabled(Parent.AH_ComplianceSubType) && Parent.AH_OriginalInvoiceDate.IsEmpty)
			{
				Parent.AH_OriginalInvoiceDateInfo.AddError(Res.GetString("caa180aa-b97e-44c5-9796-c173ccfa026e",
						"Please enter an Original Invoice Date or select an Original Reference."));
			}
		}

		protected override void CheckAH_OriginalTransactionNum()
		{
			base.CheckAH_OriginalTransactionNum();

			if (IsValidationEnabled(Parent.AH_ComplianceSubType) && Parent.AH_OriginalTransactionNum.IsEmpty)
			{
				Parent.AH_OriginalTransactionNumInfo.AddError(Res.GetString("60af4015-1267-459f-afba-0a94e7b98587",
						"Please enter an Original Invoice Number or select an Original Reference."));
			}
		}

		bool IsValidationEnabled(ZString complianceSubType)
			=> (ObjectFactory.Get<ICountryComplianceFactoryIntegration>()?.GetICountryComplianceInfo(Parent.Company.Country.Code) as IOriginalInvoiceNumberAndDateValidationDecider)?
				.ShouldValidateOriginalTransactionNumberAndDate(Parent.AH_ComplianceSubType) ?? false;
	}
}
