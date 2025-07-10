using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class ProcedureProvider : IProcedure
	{
		public ProcedureProvider(JobComInvoiceLine invoiceLine)
		{
			RequestedProcedure = invoiceLine.JI_Calc_RequestedProcedure;
			PreviousProcedure = invoiceLine.JI_Calc_PreviousProcedure;
		}

		public string RequestedProcedure { get; }

		public string PreviousProcedure { get; }
	}
}
