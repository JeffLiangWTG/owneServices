using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public interface IIncidentEvent
	{
		ZString Code { get; }
		void Trigger();
	}
}
