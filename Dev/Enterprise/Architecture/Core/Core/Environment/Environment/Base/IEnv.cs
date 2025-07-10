using Enterprise.Environment;

namespace Enterprise.ZArchitecture.Environment
{
	public interface IEnv
	{
		IEnvironment Instance { get; }
	}
}
