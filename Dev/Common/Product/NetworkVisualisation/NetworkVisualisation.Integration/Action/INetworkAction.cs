using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Integration
{
	public interface INetworkAction
	{
		#region Main Network Action Attributes

		ResourceString GetName();

		ResourceString GetDescription();

		string GetIconName();

		bool IsActivated();

		IEnumerable<INetworkAction> GetChildActions();

		#endregion

		#region Accessibility

		INetworkActionAccessibility IsApplicable();

		INetworkActionAccessibility IsEnabled();

		INetworkActionAccessibility CheckCanStartExecution();

		#endregion

		#region Execution

		INetworkActionResult Execute();

		#endregion

		#region Grouping

		int Group { get; set; }

		int GroupIndex { get; set; }

		#endregion

		void Refresh(RefreshArgs args);
	}
}
