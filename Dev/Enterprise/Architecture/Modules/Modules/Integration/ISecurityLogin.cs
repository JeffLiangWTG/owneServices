using CargoWise.Types;
using Enterprise.Core.Environment;

namespace Enterprise.ZArchitecture.Modules
{
	public interface ISecurityLogin
	{
		ZString Login { get; set; }

		bool IsRestrictedByCheckpoint(IZSecurity security, params CheckpointLookupKey[] lookupKeys);
	}
}
