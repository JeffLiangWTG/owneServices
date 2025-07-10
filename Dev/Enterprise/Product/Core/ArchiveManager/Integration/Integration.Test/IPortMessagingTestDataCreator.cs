using CargoWise.Types;

namespace Enterprise.ArchiveManager.Integration.Test
{
	public interface IPortMessagingTestDataCreator
	{
		void CreatePackLineData(ZGuid pk);
	}
}
