using CargoWise.Types;

namespace Enterprise.DocumentScanning.Integration
{
	public interface IOverrideStorageMainDocManagerCode
	{
		ZString GetOverridenCodeIfNecessary(ZString docManagerCode);
	}
}
