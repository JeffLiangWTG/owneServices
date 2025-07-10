using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory.Mexico;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class MexicoAccountingCountryFactory :
		IAccountingCountryFactory,
		ICountrySpecificLabelTranslator,
		IDebtorNumberProvider,
		IQRCodeDataProvider,
		IEquivalentAgreedPaymentMethodProvider,
		IInvoicePaymentMethodProvider,
		IInstanceProvider<IComplianceNumberResetStatus>,
		IInstanceProvider<IReversalStatusCodeConfiguration>
	{
		ZString? ICountrySpecificLabelTranslator.GetTranslation(LabelsEnum? label, params object[] parameters)
		{
			return MexicoSpecificTranslactions.TranslateLabel(label);
		}

		ZString IDebtorNumberProvider.GetDebtorName(AccTransactionHeaderAuthorisationRecord transactionHeaderAuthorisationRecord) => transactionHeaderAuthorisationRecord != null ? GetDebtorNumber(transactionHeaderAuthorisationRecord) : ZString.Empty;

		static ZString GetDebtorNumber(AccTransactionHeaderAuthorisationRecord transactionHeaderAuthorisationRecord)
		{
			var usosCFDI = transactionHeaderAuthorisationRecord.Factory.GetCachedValue("Mexico_UsosCFDI_PairList", () => ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetMexicoEInvoicingExtension().GetUsosCFDI());
			var description = usosCFDI?.GetDescriptionFromCode(transactionHeaderAuthorisationRecord.AHF_DebtorNumber) ?? ZString.Empty;

			#region SuppressResourceStringsCheckRegion

			if (!string.IsNullOrEmpty(description))
			{
				description = $" - {description}";
			}

			return $"{transactionHeaderAuthorisationRecord.AHF_DebtorNumber}{description}";

			#endregion SuppressResourceStringsCheckRegion
		}

		string IQRCodeDataProvider.GetTransactionQRCodeString(InvoicingBase invoicing)
		{
			return (new MexicoQRCodeDataProvider() as IQRCodeDataProvider).GetTransactionQRCodeString(invoicing);
		}

		IEquivalentAgreedPaymentMethod IEquivalentAgreedPaymentMethodProvider.GetEquivalentAgreedPaymentMethodProvider() => new MexicoEquivalentAgreedPaymentMethod();

		IInvoicePaymentMethod IInvoicePaymentMethodProvider.GetInvoicePaymentMethodProvider() => new MexicoInvoicePaymentMethod();

		IComplianceNumberResetStatus IInstanceProvider<IComplianceNumberResetStatus>.Get() => new ComplianceNumberResetStatus();

		IReversalStatusCodeConfiguration IInstanceProvider<IReversalStatusCodeConfiguration>.Get() => new MexicoReversalStatusCodeConfiguration();
	}
}
