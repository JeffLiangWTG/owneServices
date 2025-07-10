using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415HeaderProvider : EntryHeaderMessageProvider, IIM413AndIM415Header
	{
		public IM413AndIM415HeaderProvider(AISUCC5MessageSendingAction sendingAction) : base(sendingAction.EntryHeader)
		{
		}

		public bool HasRequestedProcedure71 => instruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(line => line.JI_Calc_RequestedProcedure == UniversalReferenceConstants.ProcedureCodes.ProcedureCode._71);

		public string MessageDeclarationType => instruction.CEI_Style;

		public IIM413AndIM415DeclarationType Declaration => CachedValueHelper.GetValue(ref declarationCached, () => new IM413AndIM415DeclarationTypeProvider(entryHeaderWrapper, true));
		CachedValue<IIM413AndIM415DeclarationType> declarationCached;

		public IIM413AndIM415GoodsShipmentType GoodsShipment => CachedValueHelper.GetValue(ref goodsShipmentCached, () => new IM413AndIM415GoodsShipmentProvider(entryHeaderWrapper));
		CachedValue<IIM413AndIM415GoodsShipmentType> goodsShipmentCached;
	}
}
