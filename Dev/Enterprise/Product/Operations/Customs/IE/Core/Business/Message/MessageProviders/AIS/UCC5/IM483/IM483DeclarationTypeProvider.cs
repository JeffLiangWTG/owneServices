using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM483DeclarationTypeProvider : IIM483DeclarationType
	{
		readonly CusEntryHeader entryHeader;

		public IM483DeclarationTypeProvider(CusEntryHeader entryHeader)
		{
			this.entryHeader = entryHeader;
		}

		public string MRN => entryHeader.MovementReferenceNumber;

		public string LRN => entryHeader.CH_BGMReference;
	}
}
