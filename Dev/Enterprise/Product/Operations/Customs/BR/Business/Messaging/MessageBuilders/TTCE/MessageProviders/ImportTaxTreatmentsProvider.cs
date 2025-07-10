using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Interfaces.TTCE.Outgoing;

namespace Enterprise.Customs.BR.Business
{
	public class ImportTaxTreatmentsProvider : IImportTaxTreatments
	{
		public ImportTaxTreatmentsProvider(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		readonly JobComInvoiceLine invoiceLine;

		public string Ncm => invoiceLine.JI_Tariff;

		public int CountryCode => int.TryParse(BRRefCusMapper.MapCW1CountryCodeToCustomsCode(invoiceLine.Factory, invoiceLine.JI_CountryOfOrigin), out var result) ? result : 0;

		public DateTime TaxEventDate => invoiceLine.InvoiceHeader.ExchangeRateDate.ToDateTimeSafe();

		public string OperationType => Constants.OperationType.Import;

		public IEnumerable<IOptionalLegalBasis> OptionalLegalBasisList => null;
	}
}
