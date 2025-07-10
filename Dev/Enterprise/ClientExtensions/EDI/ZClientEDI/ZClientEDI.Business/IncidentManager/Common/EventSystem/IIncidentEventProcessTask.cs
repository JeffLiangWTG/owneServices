using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public interface IIncidentEventProcessTask
	{
		ZString EventCode { get; set; }
	}
}
