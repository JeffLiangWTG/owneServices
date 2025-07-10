using CargoWise.EntityFramework;

namespace Enterprise.Integration.MasterFiles
{
	public interface IJobNumUniqueIndexErrorHelper
	{
		string GetErrorMessage(ZSaveException ex);
	}
}
