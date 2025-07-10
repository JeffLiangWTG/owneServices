using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ClearanceHeaderWrapper : IClearanceHeader
	{
		public ClearanceHeaderWrapper(CusEntryHeader cusEntryHeader)
		{
			entryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
			declaration = Argument.NotNull(entryHeader.Declaration, "entryHeader.Declaration");
		}
		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;

		public ZString ReceptionCustomsOffice => declaration.JE_CustomsOffice.Right(6);

		public ZString ReceptionT2LReference => entryHeader.MovementReferenceNumber;

		public IPartyNameProvider Declarant => CachedValueHelper.GetValue(ref declarant, () => PartyNameWrapper.New(declaration.Declarant));
		CachedValue<IPartyNameProvider> declarant;

		public ZString GoodsLocation => declaration.JE_LocationOfGoods.Length > 10 ? declaration.JE_LocationOfGoods.SubstringSafe(4, 10) : declaration.JE_LocationOfGoods;

		public ZInt TotalLinesNum => entryHeader.MergedLines.Count;

		public ZBool ContainersIndicator => entryHeader.IsContainerised();

		public IT2LCommunicationsCommon Communications => communications ?? (communications = new T2LCommunicationsCommonWrapper(declaration));
		T2LCommunicationsCommonWrapper communications;
	}
}
