using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Environment
{
	class EnvI : IEnv
	{
		public IEnvironment Instance
		{
			get { return Env.Instance; }
		}
	}
}
