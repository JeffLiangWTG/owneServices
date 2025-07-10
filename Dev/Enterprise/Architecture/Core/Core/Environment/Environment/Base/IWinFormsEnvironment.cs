using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Environment
{
	public interface IWinFormsEnvironment : IEnvironment
	{
		string CurrentModule { get; }
		GridLayoutRegistry GridLayoutRegistry { get; }
		SplitterLayoutRegistry SplitterLayoutRegistry { get; }
	}
}
