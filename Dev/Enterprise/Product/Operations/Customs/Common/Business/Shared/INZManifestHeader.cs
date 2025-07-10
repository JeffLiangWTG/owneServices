using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Common
{
	public interface INZManifestHeader : IBusiness
	{
		ZString JobName { get; }
		ZString DocumentParentType { get; }
		Logs Logs { get; }
		ZString MasterBillNumber { get; }
		ZString JobNumber { get; }
		CusEntryNumber LoadAndCreateRegistrationNumber();
	}
}
