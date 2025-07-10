using CargoWise.EntityFramework;
using Enterprise.Core.Environment;

namespace Enterprise.Security
{
	public interface ISecurityCheckpointProvider
	{
		void LoadCheckpoints(BusinessObjectFactory factory, IZSecurity securityInstance);
	}
}
