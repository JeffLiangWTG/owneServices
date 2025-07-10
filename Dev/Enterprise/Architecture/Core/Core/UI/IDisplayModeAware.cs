namespace Enterprise.ZArchitecture.Core
{
	public interface IDisplayModeAware
	{
		ODisplayMode DisplayMode { get; set; }
	}

	public enum ODisplayMode
	{
		Undefined,
		Browse,
		Edit,
		ReadOnly,
		Delete,
		New,
		NewSaved
	}
}
