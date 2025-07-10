using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.UCC5;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413DeclarationTypeProvider : IM413AndIM415DeclarationTypeProvider, IIM413DeclarationType
	{
		public IM413DeclarationTypeProvider(EntryHeaderWrapper entryHeaderWrapper) : base(entryHeaderWrapper, false)
		{
		}

		public string MRN => entryHeader.MovementReferenceNumber;
		public string Remarks => entryHeader.CH_CustomsMessageRemarks;
	}
	public class IM413HeaderProvider : IM413AndIM415HeaderProvider, IIM413Header
	{
		public IM413HeaderProvider(AISUCC5MessageSendingAction sendingAction) : base(sendingAction)
		{
		}
		public new IIM413DeclarationType Declaration => CachedValueHelper.GetValue(ref declarationCached, () => new IM413DeclarationTypeProvider(entryHeaderWrapper));
		CachedValue<IIM413DeclarationType> declarationCached;
	}
}
