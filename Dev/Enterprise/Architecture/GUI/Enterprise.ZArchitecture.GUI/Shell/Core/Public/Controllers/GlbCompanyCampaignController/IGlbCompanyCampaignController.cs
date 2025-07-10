using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules
{
	public interface IGlbCompanyCampaignController
	{
		IZForm ShowEditFormButFocusOnCampaignItem(IGlbCompanyCampaignItem campaignItem);
	}
}
