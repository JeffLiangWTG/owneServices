namespace CargoWise.NetworkVisualisation.Integration
{
	public interface INetworkEntityController
	{
		INetworkUserInteractionImplementor UserInteractionImplementor { get; }

		void NotifyActionCannotBeExecuted(INetworkActionAccessibility result);

		void NotifyActionExecutedPartially(INetworkActionAccessibility result);

		bool IsDeactivated { get; }
	}
}
