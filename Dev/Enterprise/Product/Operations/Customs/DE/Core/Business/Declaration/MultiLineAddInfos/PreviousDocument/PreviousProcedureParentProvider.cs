using CargoWise.Common;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class PreviousProcedureParentProvider : IPreviousDocumentParentProvider
	{
		readonly JobComInvoiceLine line;

		public PreviousProcedureParentProvider(JobComInvoiceLine line)
		{
			this.line = Argument.NotNull(line, nameof(line));
		}

		public PreviousDocumentCollection PreviousDocuments => line.PreviousProcedures;

		public PreviousDocumentMaster PreviousDocumentMaster => line.PreviousProcedureMaster;

		public JobDeclaration JobDeclaration => line.Declaration;

		internal JobComInvoiceLine InvoiceLine => line;
	}
}
