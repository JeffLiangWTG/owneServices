using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public interface IResultCountHandler
	{
		void UpdateNumberLoadedMessage(ZString message, int numberOfRecordsFound, bool shouldShowNumberLoadedMessageBox);
	}
}
