using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class SADEDocsSaver : BaseEDocsSaver<CusEntryHeader>
	{
		public SADEDocsSaver(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		protected override ZGuid BranchPKCore => target.Declaration.JE_GB;

		protected override ZString DocTitleCore => new ZString("SAD");

		protected override ZString DocNameCore => new ZString($"SAD - {target.CH_BGMReference}");

		protected override ZGuid DocumentMenuItemPKCore => new ZGuid("A412ABB6-0162-46EA-AB55-6C1177A8B131"); // PK for SADH C88 menu
	}
}
