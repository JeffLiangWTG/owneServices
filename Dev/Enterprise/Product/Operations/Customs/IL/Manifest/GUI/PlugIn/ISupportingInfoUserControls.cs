using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public interface ISupportingInfoUserControls
	{
		string GridBindingMember { get; }
		ZGrid Grid { get; }
	}
}
