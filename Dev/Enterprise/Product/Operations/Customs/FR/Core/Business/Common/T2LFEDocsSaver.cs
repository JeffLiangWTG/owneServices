using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business
{
	public class T2LFEDocsSaver : BaseEDocsSaver<CusEntryHeader>
	{
		public T2LFEDocsSaver(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		protected override ZGuid BranchPKCore => target.Declaration.JE_GB;

		protected override ZString DocTitleCore => new ZString(ExportCommunityTransitStatusList.Codes.T2LF);

		protected override ZString DocNameCore => new ZString($"{ExportCommunityTransitStatusList.Codes.T2LF} - {target.CH_BGMReference}");

		protected override ZGuid DocumentMenuItemPKCore => new ZGuid("a943f107-9b18-44d8-a5bc-f91d6629bc55"); // PK for T2LF menu for FR
	}
}
