using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.CustomerService.Integration
{
	public interface IIncidentApproval : IBusiness
	{
		ZString IA_IncidentSummary { get; set; }
		ZString IA_IncidentDetails { get; set; }
		ZString IA_Module { get; set; }
		ZString IA_Criticality { get; set; }
	}
}
