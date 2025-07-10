using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public interface IMatchingBusinessEntityFinder<T> where T : BusinessObject
	{
		T GetBestMatch();
	}
}
