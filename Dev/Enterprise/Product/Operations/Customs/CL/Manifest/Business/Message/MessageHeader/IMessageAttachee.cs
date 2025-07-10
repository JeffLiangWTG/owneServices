using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public interface IMessageAttachee
	{
		ZGuid PK { get; }
		ZString Number { get; }
		ZGuid GlobalBranchPK { get; }
		ASYCUDA.Business.AsycudaManifestHeader Header { get; }
		IBusinessObjectCollection Messages { get; }
	}
}
