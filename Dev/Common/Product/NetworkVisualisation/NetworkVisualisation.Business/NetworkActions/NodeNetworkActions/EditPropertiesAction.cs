using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public class EditPropertiesAction : DynamicNetworkAction
	{
		public EditPropertiesAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, null, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Main Properties

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("55862e32-fbe6-48c4-9e0a-5887bc94dd66", "Edit Properties");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("5e9aeebd-354e-40df-bc72-b3c3eb21eebe", "Edit the properties of the shape's underlying entity");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "resource name")]
		protected override string IconName => "Cog";

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity)
		{
			return new NetworkActionAccessibility(GetNode(entity).SupportsEditEntity,
				entity,
				() => Res.GetString("9BD95E13-915F-456B-AAAA-DE63AAAC6FFF", "The entity should support properties editing."));
		}

		protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

		#endregion

		#region Execution

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
		{
			NetworkViewModel.Network.EditEntity(entity);
			return null;
		}

		#endregion
	}
}
