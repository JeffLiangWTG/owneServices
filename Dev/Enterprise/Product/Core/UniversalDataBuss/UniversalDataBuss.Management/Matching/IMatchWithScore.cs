using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management.Matching
{
	public interface IMatch
	{
		bool HasDataForMatching(IShipmentDataObjectReader topLevelDataObjectReader);
		bool IsMatch(PotentialMatch matchingBO, ISimpleLogger logger);
		string IncomingValueName { get; }
	}

	interface IMatchWithFilter : IMatch
	{
		ZQuery GetFilter();
	}

	interface IMatchWithFilterAndScore : IMatchWithFilter
	{
		int Score { get; }
	}
}
