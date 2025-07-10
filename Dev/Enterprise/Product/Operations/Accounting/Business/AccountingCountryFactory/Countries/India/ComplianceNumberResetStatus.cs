using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.India
{
	class ComplianceNumberResetStatus : IComplianceNumberResetStatus
	{
		bool IComplianceNumberResetStatus.CheckIsComplianceNumberResetAllowedForSubmittedEInvoice(IComplianceNumberResetStatusInputData inputData)
		{
			var complianceNumberAppliesFrom = AccountingConfigurationRegistry.Instance.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.GetFallBackValueAtAllLevels(inputData.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty);

			return !IndiaComplianceInfo.UseComplianceNumberForEInvoicingMapping((ZDate)complianceNumberAppliesFrom.Date, inputData.InvoiceDate);
		}
	}
}
