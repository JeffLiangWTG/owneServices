// CW1115:Use Set Temporary User Context Instead Of Set User Context Analyzer

using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1115
	{
		public void Method(UserContext context)
		{
			EnvProxy.Instance.SetUserContext(context);
		}
	}
}
