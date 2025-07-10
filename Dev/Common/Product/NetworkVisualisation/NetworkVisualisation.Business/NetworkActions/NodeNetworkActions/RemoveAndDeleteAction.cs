using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public class RemoveAndDeleteAction : DynamicNetworkAction
	{
		public RemoveAndDeleteAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, new MultipleSelectedEntitiesExecutionStrategy(), group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Main Properties

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("815c28ea-600d-47b9-8a06-b66b98407a95", "Remove and Delete Entity");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("9c341b02-282a-4fbc-bb1b-034894e2275d", "Removes this shape from the diagram and deletes the underlying entity");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "resource name")]
		protected override string IconName => "Cross";

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity)
		{
			return NetworkActionAccessibilityHelper.CheckEntityIsNotRoot(entity);
		}

		protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity)
		{
			return new NetworkActionAccessibility(GetNode(entity).CanDelete,
				entity,
				() => Res.GetString("24E3565E-66DC-43F1-BD2D-6BAA1C3AE493", "The entity should represent an object which can be deleted from the database."));
		}

		#endregion

		#region Execution

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
		{
			GetNetworkViewModel().DeleteNode(GetNode(entity));
			return null;
		}

		#endregion
	}
}
