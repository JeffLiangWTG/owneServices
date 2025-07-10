using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	/// <summary>
	/// Controller for managing User Interaction actions and notifications. It provides Notification for actions that cannot be executed or partially executed.
	/// It is used to show error when search finder input string is empty or search results is none or not updated in <see cref="SearchFinderViewModel"/>
	/// </summary>
	public class NetworkEntityController : INetworkEntityController
	{
		public NetworkEntityController(INetworkUserInteractionImplementor userInteractionImplementor)
		{
			UserInteractionImplementor = userInteractionImplementor;
		}

		#region Accessors

		public INetworkUserInteractionImplementor UserInteractionImplementor { get; private set; }

		#endregion

		#region Network Action Accessibility Notifications

		public void NotifyActionCannotBeExecuted(INetworkActionAccessibility result)
		{
			var message = NetworkActionHelper.GetActionIsNotAccessibleMessage(result,
				header: Res.GetString("9B85B59B-F2A3-4BF8-8DD2-FDAC621D56D4", "This action cannot be executed for the given shape(s) due to the following reasons:"));
			UserInteractionImplementor.ShowError(message, Res.GetString("35470218-F765-4BBA-AB67-3F93BB415730", "Action cannot be executed"));
		}

		public void NotifyActionExecutedPartially(INetworkActionAccessibility result)
		{
			var message = NetworkActionHelper.GetActionIsNotAccessibleMessage(result,
				header: Res.GetString("75576F6C-7F3A-4256-9804-9DA315FBC7AF", "This action failed to execute for some of the given shapes due to the following reasons:"));
			UserInteractionImplementor.ShowError(message, Res.GetString("17471BA7-7146-4A9C-8938-5F5E681D7C8B", "Action was executed partially"));
		}

		#endregion

		#region Deactivation

		public bool IsDeactivated { get; private set; }

		protected virtual void Deactivate()
		{
			UserInteractionImplementor = null;
			IsDeactivated = true;
		}

		#endregion
	}
}
