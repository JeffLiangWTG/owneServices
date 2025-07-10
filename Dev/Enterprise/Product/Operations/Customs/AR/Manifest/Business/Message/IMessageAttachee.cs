using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public interface IMessageAttachee
	{
		ZGuid PK { get; }
		ZString TableName { get; }
		ZGuid GlobalBranchPK { get; }
		ZString JobReference { get; }
		IBusinessObjectCollection Messages { get; }
	}
}
