using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM415HeaderProvider : IM413_414_415_432_433HeaderProvider, IIM415Header
	{
		public IM415HeaderProvider(AISMessageSendingAction sendingAction) : base(sendingAction)
		{
		}

		public IOperation ImportOperation => CachedValueHelper.GetValue(ref importOperation, () => new IM413And415OperationProvider(entryHeader, true));
		CachedValue<IM413And415OperationProvider> importOperation;

		public IAuth8F Authorisation8F => CachedValueHelper.GetValue(ref authorisation8FProviderCached, () => instruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(line => line.JI_Calc_RequestedProcedure == UniversalReferenceConstants.ProcedureCodes.ProcedureCode._71) ? null : new Authorisation8FProvider(entryHeader));
		CachedValue<Authorisation8FProvider> authorisation8FProviderCached;

		public string DeclarationType => instruction.CEI_Style;
	}
}
