using CargoWise.EntityFramework;

namespace Enterprise.Integration.Rating
{
	public interface IQuotationsController
	{
		bool CheckIsCopyAllowed(BusinessObject inMemorySourceEntity);
	}
}
