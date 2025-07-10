using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IN.Business;

public interface IMessageAttachee : IJobNumber
{
	BusinessObjectFactory Factory { get; }
	IBusinessObjectCollection Messages { get; }

	ZGuid BranchPK { get; }
	ZString MessageOwner { get; }
	ZString MessageStatus { get; set; }
	ZString CustomsStatus { get; set; }

	ZString CalculateStatusAfterSending(ZString messageType);

	void RollbackChangesOnStatus();
}
