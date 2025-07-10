namespace CargoWise.EntityFramework
{
	public interface IClusterKeyMappingsProvider
	{
		IClusterKeyMappingData[] GetAllClusterKeyMappings();
	}
}
