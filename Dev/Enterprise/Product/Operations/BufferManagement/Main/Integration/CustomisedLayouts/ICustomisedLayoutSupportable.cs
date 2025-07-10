using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Integration
{
	public interface ICustomisedLayoutSupportable : IBusiness
	{
		IBMControlCustomisationLinkCollection CustomisedLayoutLinks { get; }
		bool AreCustomisedLayoutFetchHintsAdded { get; set; }
	}
}
