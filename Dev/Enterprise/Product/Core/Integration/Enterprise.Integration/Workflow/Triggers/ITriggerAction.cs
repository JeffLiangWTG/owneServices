using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public enum TriggerRunLocation
	{
		Server,
		Client
	}

	public interface ITriggerAction : IBusiness
	{
		ZString ActionType { get; set; }
		TriggerRunLocation RunLocation { get; }
	}
}
