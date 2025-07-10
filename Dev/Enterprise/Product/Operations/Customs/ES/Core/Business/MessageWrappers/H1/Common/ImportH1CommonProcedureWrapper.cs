using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class ImportH1CommonProcedureWrapper : ICommonH1Procedure
{
	public ImportH1CommonProcedureWrapper(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}
	readonly JobComInvoiceLine invoiceLine;

	public ZString RequestedCPC => invoiceLine.JI_FormattedProcedure.SubstringSafe(0, 2);

	public ZString PreviousCPC => invoiceLine.JI_FormattedProcedure.SubstringSafe(2, 2);
}
