using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IL.GUI
{
	public interface ISupportingInfoUserControls
	{
		string GridBindingMember { get; }
		ZGrid Grid { get; }
	}
}
