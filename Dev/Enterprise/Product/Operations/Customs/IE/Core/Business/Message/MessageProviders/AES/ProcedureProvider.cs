using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	public class ProcedureProvider : IProcedure
	{
		public ProcedureProvider(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}
		readonly JobComInvoiceLine invoiceLine;

		public string RequestedProcedure => invoiceLine.JI_Calc_RequestedProcedure;

		public string PreviousProcedure => invoiceLine.JI_Calc_PreviousProcedure;

		public IReadOnlyCollection<string> AdditionalProcedures => additionalProcedures ?? (additionalProcedures = invoiceLine.AdditionalProcedureCodesIncludingConcession.OrderBy(x => x).Select(x => x.ToString()).ToArray());
		IReadOnlyCollection<string> additionalProcedures;
	}
}
