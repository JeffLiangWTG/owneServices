using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IStmUniversalJobLink
	{
		ZString UCL_EnterpriseCode { get; set; }
		ZString UCL_ServerCode { get; set; }
		ZString UCL_CompanyCode { get; set; }
		ZString UCL_SourceType { get; set; }
		ZString UCL_SourceKey { get; set; }
		ZDateTime UCL_SystemCreateTimeUtc { get; set; }
		ZGuid UCL_OH_Owner { get; set; }
		ZString UCL_ParentTableCode { get; set; }
		bool HasChanges { get; set; }
	}
}
