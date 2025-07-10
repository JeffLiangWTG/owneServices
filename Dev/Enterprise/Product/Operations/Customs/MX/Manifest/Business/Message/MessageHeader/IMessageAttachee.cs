using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public interface IMessageAttachee
	{
		ZGuid PK { get; }
		ZString TableName { get; }
		ZGuid GlobalBranchPK { get; }
		ZString JobReference { get; }
		IBusinessObjectCollection Messages { get; }
	}

	public interface IManifestMessageAttachee : IMessageAttachee
	{
		ZString BillNumber { get; }
	}
}
