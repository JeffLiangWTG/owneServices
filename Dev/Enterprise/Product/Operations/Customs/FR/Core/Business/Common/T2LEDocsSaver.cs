using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business
{
	public class T2LEDocsSaver : BaseEDocsSaver<CusEntryHeader>
	{
		public T2LEDocsSaver(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		protected override ZGuid BranchPKCore => target.Declaration.JE_GB;

		protected override ZString DocTitleCore => new ZString("T2L");

		protected override ZString DocNameCore => new ZString($"T2L - {target.CH_BGMReference}");

		protected override ZGuid DocumentMenuItemPKCore => new ZGuid("9d1eb5b1-86c8-45fb-909d-899d54d8836f"); // PK for T2L menu for FR 
	}
}
