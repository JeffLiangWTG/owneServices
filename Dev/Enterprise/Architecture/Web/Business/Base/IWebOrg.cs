using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.Business
{
	public interface IWebOrg : IFactoryProvider
	{
		ZGuid PK { get; }
		ZString OH_FullName { get; }
		ZString OH_FullNameTruncated { get; }
		ZString OH_Code { get; }
		ZString CountryCode { get; }
		ZBool OH_IsConsignee { get; }
		ZBool OH_IsConsignor { get; }
		ZBool OH_IsWarehouseClient { get; }
		ZBool OH_IsForwarder { get; }
		ZString OH_RL_NKClosestPort { get; }
		ZBool OH_IsGlobalAccount { get; }

		OrgHeader GetHeader();
		OrgMiscServ GetMiscServ();
		PartAttributeManager PartAttributeManager { get; }
	}
}
