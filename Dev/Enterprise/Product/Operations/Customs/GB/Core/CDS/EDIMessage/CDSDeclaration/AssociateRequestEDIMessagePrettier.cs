using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS
{
	public class AssociateRequestEDIMessagePrettier : CDSEDIMessagePrettier<CDSEDIMessage>
	{
		public AssociateRequestEDIMessagePrettier(CDSEDIMessage message) : base(message)
		{
			this.entryHeader = message.EM_LinkedObject as CusEntryHeader;
		}
		readonly CusEntryHeader entryHeader;

		public override ZString MakeHumanReadable()
		{
			var declaration = entryHeader.Declaration;
			var bgmReference = entryHeader?.CH_BGMReference ?? ZString.Empty;
			var mucr = declaration?.JE_MasterUCR ?? ZString.Empty;
			return MessagePrettierCss.CSS + ToH3IfNotEmpty($@"Associate {bgmReference} into consol {mucr}");
		}
	}
}
