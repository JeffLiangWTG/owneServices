using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface IDocDataManager
	{
		ZString GetValue(ZString docDataIdentifier);
	}
}
