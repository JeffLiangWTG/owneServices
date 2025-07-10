using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public interface IConsolidationOptionsWrapper
	{
		ZGuid ImporterPK { get; }
		ZGuid BranchPK { get; }
		ZGuid CompanyPK { get; }
		ZString LVSType { get; }
		ZString Broker { get; }
		ZString PortOfClearanceCode { get; }
		ZDateTime EntryAuthorisationDate { get; }
		OrgHeader Importer { get; }
		ZZRefCusCodeListCombined PortOfClearance { get; }
		ZBool IsAllowOIC { get; }
		ZString Province { get; }
	}
}
