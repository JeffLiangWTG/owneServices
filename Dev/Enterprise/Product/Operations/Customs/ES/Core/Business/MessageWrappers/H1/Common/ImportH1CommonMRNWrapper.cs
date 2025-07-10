using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ImportH1CommonMRNWrapper : IH1CommonMRN
	{
		public ImportH1CommonMRNWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = entryHeader;
		}
		readonly CusEntryHeader entryHeader;

		public ZString MRN => entryHeader.MovementReferenceNumber;
	}
}
