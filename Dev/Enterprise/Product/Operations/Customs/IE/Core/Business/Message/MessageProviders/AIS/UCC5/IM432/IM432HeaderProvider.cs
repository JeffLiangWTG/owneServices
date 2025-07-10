using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	[CodeAlive("Will be made use in following change.")]
	public class IM432HeaderProvider : EntryHeaderMessageProvider, IIM432Header
	{
		public IM432HeaderProvider(AISUCC5MessageSendingAction sendingAction) : base(sendingAction.EntryHeader)
		{
		}

		IIM432DeclarationType IIM432Header.Declaration => CachedValueHelper.GetValue(ref declarationCached, () => new IM432DeclarationProvider(entryHeaderWrapper));
		CachedValue<IIM432DeclarationType> declarationCached;

		IIM432GoodsShipmentType IIM432Header.GoodsShipment => CachedValueHelper.GetValue(ref goodsShipmentCached, () => new IM432GoodsShipmentProvider(entryHeaderWrapper));
		CachedValue<IIM432GoodsShipmentType> goodsShipmentCached;
	}
}
