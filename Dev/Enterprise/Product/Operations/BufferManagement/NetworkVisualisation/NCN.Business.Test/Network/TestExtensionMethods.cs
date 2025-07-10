using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	public static class TestExtensionMethods
	{
		#region Network Actions

		public static JobNetworkActionBase AsJobNetworkAction(this INetworkAction action)
		{
			if (action is JobNetworkActionBase jobNetworkAction)
			{
				return jobNetworkAction;
			}
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot convert network action {0} into a dynamic network action.", action));
		}

		#region Shortcuts for Backward Compatibility with Old Tests

		public static ResourceString GetNameAfterActivatingEntity_ForTest(this DynamicNetworkAction action, INetworkEntity activeEntityOverride)
		{
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(action.NetworkViewModel, activeEntityOverride))
			{
				return action.GetName();
			}
		}

		public static ResourceString GetDescriptionAfterActivatingEntity_ForTest(this DynamicNetworkAction action, INetworkEntity activeEntityOverride)
		{
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(action.NetworkViewModel, activeEntityOverride))
			{
				return action.GetDescription();
			}
		}

		public static bool IsActivatedAfterActivatingEntity_ForTest(this DynamicNetworkAction action, INetworkEntity activeEntityOverride)
		{
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(action.NetworkViewModel, activeEntityOverride))
			{
				return action.IsActivated();
			}
		}

		public static IEnumerable<INetworkAction> GetChildActionsAfterActivatingEntity_ForTest(this DynamicNetworkAction action, INetworkEntity activeEntityOverride)
		{
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(action.NetworkViewModel, activeEntityOverride))
			{
				return action.GetChildActions();
			}
		}

		public static INetworkActionAccessibility IsApplicableAfterActivatingEntity_ForTest(this DynamicNetworkAction action, INetworkEntity activeEntityOverride)
		{
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(action.NetworkViewModel, activeEntityOverride))
			{
				return action.IsApplicable();
			}
		}

		public static INetworkActionAccessibility IsEnabledAfterActivatingEntity_ForTest(this DynamicNetworkAction action, INetworkEntity activeEntityOverride)
		{
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(action.NetworkViewModel, activeEntityOverride))
			{
				return action.IsEnabled();
			}
		}

		public static INetworkActionAccessibility CheckCanStartExecutionAfterActivatingEntity_ForTest(this DynamicNetworkAction action, INetworkEntity activeEntityOverride)
		{
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(action.NetworkViewModel, activeEntityOverride))
			{
				return action.CheckCanStartExecution();
			}
		}

		public static INetworkActionResult ExecuteAfterActivatingEntity_ForTest(this DynamicNetworkAction action, INetworkEntity activeEntityOverride)
		{
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(action.NetworkViewModel, activeEntityOverride))
			{
				return action.Execute();
			}
		}

		#endregion

		#endregion
	}
}
