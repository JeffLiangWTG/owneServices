using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business;

public class ProcedureProvider : IProcedure
{
	public ProcedureProvider(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}

	readonly JobComInvoiceLine invoiceLine;

	public IReadOnlyCollection<IAdditionalProcedure> AdditionalProcedure => additionalProcedures ?? (additionalProcedures =
		invoiceLine.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>().Select((t, index) => new AdditionalProcedureProvider(t, index + 1)).ToArray());
	IReadOnlyCollection<IAdditionalProcedure> additionalProcedures;

	public string PreviousProcedure => invoiceLine.JI_Procedure.SubstringSafe(2, 2);

	public string RequestedProcedure => invoiceLine.JI_Procedure.SubstringSafe(0, 2);
}
