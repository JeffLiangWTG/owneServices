using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public interface ISupportingInfoUserControls
	{
		string GridBindingMember { get; }
		ZGrid Grid { get; }
	}
}
