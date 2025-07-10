using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AnnexHeaderWrapper : IAnnexHeader
	{
		public AnnexHeaderWrapper(CusEntryHeader cusEntryHeader, ZBool endOfAnnexes)
		{
			entryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
			jobDeclaration = Argument.NotNull(entryHeader.Declaration, "entryHeader.Declaration");
			FinalAnnexIndicator = endOfAnnexes;
		}
		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration jobDeclaration;

		public ZString T2LReferenceNumber => entryHeader.MovementReferenceNumber;

		public IPartyNameProvider Declarant => CachedValueHelper.GetValue(ref declarant, () => PartyNameWrapper.New(jobDeclaration.Declarant));
		CachedValue<IPartyNameProvider> declarant;

		public ZBool FinalAnnexIndicator { get; }
	}
}
