using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class InsuranceProvider : ICharge
	{
		InsuranceProvider(CusEntryInstruction entryInstruction)
		{
			this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
			declaration = Argument.NotNull(entryInstruction.JobDeclaration, nameof(entryInstruction.JobDeclaration));
			invoiceLines = entryInstruction.InvoiceLines;
		}
		readonly CusEntryInstruction entryInstruction;
		readonly IEnumerable<JobComInvoiceLine> invoiceLines;
		readonly JobDeclaration declaration;

		public static InsuranceProvider New(CusEntryInstruction entryInstruction) => entryInstruction == null ? null : new InsuranceProvider(entryInstruction);

		public string CurrencyCode => OverseasInsuranceCurrency?.Code;

		public double Amount => fOverseas ??= (double)invoiceLines.GetTotalChargesAmountOnInvoiceLines(
			CustomsChargeTypeList.Codes.OverseasInsurance, OverseasInsuranceCurrency);
		double? fOverseas;

		RefCurrency OverseasInsuranceCurrency => fOverseasInsuranceCurrency ??=
			entryInstruction.JobDeclaration.AllOverseasInsuranceChargesHaveTheSameCurrency ? invoiceLines.GetFirstChargeCurrency(CustomsChargeTypeList.Codes.OverseasInsurance) : LocalCurrency;
		RefCurrency fOverseasInsuranceCurrency;

		RefCurrency LocalCurrency => fLocalCurrency ??= declaration.LocalCurrency;
		RefCurrency fLocalCurrency;
	}
}
