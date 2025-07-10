using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface ITriggerUserContextConditions
	{
		ZString TriggerContextCode { get; set; }
		ZString TriggerStaffCode { get; set; }
		ZGuid TriggerBranch { get; set; }
		ZGuid TriggerCompany { get; set; }
		ZGuid TriggerDepartment { get; set; }
	}
}
