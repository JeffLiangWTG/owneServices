using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
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

		public IReadOnlyCollection<ICcQualifierAdditionalProcedure> AdditionalProcedure => additionalProcedure ?? (additionalProcedure = invoiceLine.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>().Select((x, i) => new AdditionalProcedureProvider(i + 1, x)).ToArray());
		IReadOnlyCollection<ICcQualifierAdditionalProcedure> additionalProcedure;
	}
}
