using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management.Matching
{
	public interface IModuleMatcher<T, U>
		where T : IShipmentDataObjectReader
		where U : BusinessObject
	{
		ModuleMatchResult<U> GetBestMatch(T dataObjectReader);
	}
}
