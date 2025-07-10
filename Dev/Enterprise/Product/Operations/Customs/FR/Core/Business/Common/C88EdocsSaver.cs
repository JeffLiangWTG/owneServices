using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business
{
	public class C88EDocsSaver : BaseEDocsSaver<CusEntryHeader>
	{
		public C88EDocsSaver(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		protected override ZGuid BranchPKCore => target.Declaration.JE_GB;

		protected override ZString DocTitleCore => new ZString("C88");

		protected override ZString DocNameCore => new ZString($"C88 - {target.CH_BGMReference}");

		protected override ZGuid DocumentMenuItemPKCore => new ZGuid("a412abb6-0162-46ea-ab55-6c1177a8b131"); // PK for C88 menu for FR 
	}
}
