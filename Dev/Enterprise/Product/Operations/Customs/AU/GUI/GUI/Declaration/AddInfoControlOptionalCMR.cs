namespace Enterprise.Customs.AU.Declaration.GUI;

public class AddInfoControlOptionalCMR : AddInfoControl
{
	public bool ShowCMRAddInfo
	{
		get { return fShowCMRAddInfo; }
		set { fShowCMRAddInfo = value; }
	}
	bool fShowCMRAddInfo;

	protected override bool IsCMR
	{
		get { return ShowCMRAddInfo; }
	}
}
