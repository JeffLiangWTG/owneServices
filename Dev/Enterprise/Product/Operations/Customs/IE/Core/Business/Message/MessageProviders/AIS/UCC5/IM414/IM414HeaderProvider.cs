using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM414HeaderProvider : EntryHeaderMessageProvider, IIM414Header
	{
		readonly AISUCC5MessageSendingAction sendingAction;

		public IM414HeaderProvider(AISUCC5MessageSendingAction sendingAction) : base(sendingAction.EntryHeader)
		{
			this.sendingAction = sendingAction;
		}

		public IIM414DeclarationType Declaration => CachedValueHelper.GetValue(ref declarationCached, () => new IM414DeclarationTypeProvider(entryHeaderWrapper, PreparationDateAndTime, sendingAction));
		CachedValue<IIM414DeclarationType> declarationCached;
	}
}
