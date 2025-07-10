using CargoWise.Types;

namespace Enterprise.ArchiveManager.Integration.Test
{
	public interface ICustomsAttachedDeclarationDataCreator
	{
		ZGuid CreateAttachedDeclarationData(ZGuid shipmentPK, bool isCancelled);
	}
}
