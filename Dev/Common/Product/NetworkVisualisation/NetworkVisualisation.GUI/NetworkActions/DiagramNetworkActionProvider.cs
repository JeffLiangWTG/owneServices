using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	static class DiagramNetworkActionProvider
	{
		/// <summary>
		/// Gets a sequence of network actions for the given network view model and control.
		/// </summary>
		/// <param name="networkViewModel">The view model representing the network.</param>
		/// <param name="control">The control for user interactions with the network.</param>
		/// <returns>An enumerable of network actions.</returns>
#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public static IEnumerable<INetworkAction> GetNetworkActions(NetworkViewModel networkViewModel, NetworkUserControl control)
		{
			var group = 100;
			yield return GetBackgroundColorAction(networkViewModel, group);
			yield return GetTextColorAction(networkViewModel, group);

			group++;
			yield return new SearchAction(networkViewModel, control, group);
			yield return GetRefreshAction(networkViewModel, group);
			yield return GetPopOutAction(networkViewModel, control, group);
			yield return new FitNodesAction(networkViewModel, control, group);
			yield return GetFillAction(networkViewModel, control, group);
			yield return GetOneHundredPercentAction(networkViewModel, control, group);
			yield return GetJumpBackToPrevZoomAction(networkViewModel, control, group);

			group++;
			yield return GetZoomOutAction(networkViewModel, control, group);
			yield return GetZoomInAction(networkViewModel, control, group);
		}
#pragma warning restore CS0618 // Restore the warning for obsolete usage

		/// <summary>
		/// Gets an action to set the background color of the network entity.
		/// </summary>
		/// <param name="networkViewModel">The view model representing the network.</param>
		/// <param name="group">The group number for organizing actions.</param>
		/// <returns>An action to set the background color.</returns>
		public static INetworkAction GetBackgroundColorAction(NetworkViewModel networkViewModel, int group = 0) => new StaticNetworkAction(
			name: ResString.GetMultilingualString("ce6ebc6c-030a-48da-810f-4b9d2aa42ff2", "Background Color"),
			description: ResString.GetMultilingualString("50cc3b7d-729a-467a-8618-90a221728516", "Select the background color of this entity"),
			childActions: GetColourTransformActions(color => networkViewModel.DiagramNodeViewModel.Entity.BackColor = color),
			group: group,
				iconName: "ColorBucket");

		/// <summary>
		/// Gets an action to set the text color of the network entity.
		/// </summary>
		/// <param name="networkViewModel">The view model representing the network.</param>
		/// <param name="group">The group number for organizing actions.</param>
		/// <returns>An action to set the text color.</returns>
		public static INetworkAction GetTextColorAction(NetworkViewModel networkViewModel, int group = 0) => new StaticNetworkAction(
			name: ResString.GetMultilingualString("8ffac13e-ec07-4588-be2b-9bed2fe950ba", "Text Color"),
			description: ResString.GetMultilingualString("638DE70F-409B-4031-BF5A-7E3C2C13B9E2", "Select the text color of this entity"),
			childActions: GetColourTransformActions(color => networkViewModel.DiagramNodeViewModel.Entity.ForeColor = color),
			group: group);

		/// <summary>
		/// Gets an action to refresh the network diagram.
		/// </summary>
		/// <param name="networkViewModel">The view model representing the network.</param>
		/// <param name="group">The group number for organizing actions.</param>
		/// <returns>An action to refresh the network diagram.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "resource name")]
		public static INetworkAction GetRefreshAction(NetworkViewModel networkViewModel, int group = 0) => new StaticNetworkAction(
			() => networkViewModel.Network.Refresh(RefreshType.RedrawDiagram),
			name: ResString.GetMultilingualString("2ffcbb3f-0568-423a-b7f1-bb0584f53b8c", "Refresh"),
			description: ResString.GetMultilingualString("65305bb6-3412-4c4c-81c3-b5262f4e8c11", "Refreshes the diagram"),
			group: group,
			iconName: "Refresh");

		/// <summary>
		/// Gets an action to pop out the network diagram into a new window.
		/// </summary>
		/// <param name="networkViewModel">The view model representing the network.</param>
		/// <param name="control">The control for user interactions with the network.</param>
		/// <param name="group">The group number for organizing actions.</param>
		/// <returns>An action to pop out the network diagram into a new window.</returns>
#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public static INetworkAction GetPopOutAction(NetworkViewModel networkViewModel, NetworkUserControl control, int group = 0) => new StaticNetworkAction(
			() => control?.PopOut(),
			name: ResString.GetMultilingualString("ca0db41c-0d58-4a3a-b54a-b3b13a2d276b", "Pop Out"),
			description: ResString.GetMultilingualString("f3bd0275-a0e6-4c2f-9cde-f075b2514c00", "Pop out the entire diagram into a new window"),
			group: group,
			iconName: "PopOut");
#pragma warning restore CS0618 // Restore the warning for obsolete usage

		/// <summary>
		/// Gets an action to fit the entire content area to the view-port.
		/// </summary>
		/// <param name="networkViewModel">The view model representing the network.</param>
		/// <param name="control">The control for user interactions with the network.</param>
		/// <param name="group">The group number for organizing actions.</param>
		/// <returns>An action to fit the content to the view-port.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "resource name")]
#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public static INetworkAction GetFillAction(NetworkViewModel networkViewModel, NetworkUserControl control, int group = 0) => new StaticNetworkAction(() => control?.Fill(),
					name: ResString.GetMultilingualString("d10dc367-09d7-4085-ad20-7f301a4200ef", "Fill"),
					description: ResString.GetMultilingualString("3122c168-80bc-4e9f-9910-ca948d55425f", "Fit the entire content area to the view-port"),
					group: group,
					iconName: "Fill"
				);
#pragma warning restore CS0618 // Restore the warning for obsolete usage

		/// <summary>
		/// Gets an action to scale the content to 100%.
		/// </summary>
		/// <param name="networkViewModel">The view model representing the network.</param>
		/// <param name="control">The control for user interactions with the network.</param>
		/// <param name="group">The group number for organizing actions.</param>
		/// <returns>An action to scale the content to 100%.</returns>
#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public static INetworkAction GetOneHundredPercentAction(NetworkViewModel networkViewModel, NetworkUserControl control, int group = 0) => new StaticNetworkAction(() => control?.OneHundredPercent(),
					name: ResString.GetMultilingualString("DDC604D8-8779-4EF9-A8B4-FE6C6AC63D0C", "100%"),
					description: ResString.GetMultilingualString("de75c132-8724-45d6-b43c-7a8d16275734", "Scale the content to 100%"),
					group: group,
					iconName: "Zoom100");
#pragma warning restore CS0618 // Restore the warning for obsolete usage

		/// <summary>
		/// Gets an action to zoom out from the canvas.
		/// </summary>
		/// <param name="networkViewModel">The view model representing the network.</param>
		/// <param name="control">The control for user interactions with the network.</param>
		/// <param name="group">The group number for organizing actions.</param>
		/// <returns>An action to zoom out from the canvas.</returns>
#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public static INetworkAction GetZoomOutAction(NetworkViewModel networkViewModel, NetworkUserControl control, int group = 0) => new StaticNetworkAction(() => control?.ZoomOut(),
				name: ResString.GetMultilingualString("ba599187-3bc4-42ce-9ebf-dad3c566fc2e", "Zoom Out"),
				description: ResString.GetMultilingualString("3af79cc1-2355-4b71-b002-8ef6ab8e0c15", "Zooms out from the canvas"),
				group: group,
				iconName: "ZoomOut");
#pragma warning restore CS0618 // Restore the warning for obsolete usage

		/// <summary>
		/// Gets an action to zoom in on the canvas.
		/// </summary>
		/// <param name="networkViewModel">The view model representing the network.</param>
		/// <param name="control">The control for user interactions with the network.</param>
		/// <param name="group">The group number for organizing actions.</param>
		/// <returns>An action to zoom in on the canvas.</returns>
#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public static INetworkAction GetZoomInAction(NetworkViewModel networkViewModel, NetworkUserControl control, int group = 0) => new StaticNetworkAction(() => control?.ZoomIn(),
				name: ResString.GetMultilingualString("036557c7-3905-4c83-8dbc-14c355fec741", "Zoom In"),
				description: ResString.GetMultilingualString("0cb96a3e-7466-47c0-b2ab-0329473be487", "Zooms in on the canvas"),
				group: group,
				iconName: "ZoomIn");
#pragma warning restore CS0618 // Restore the warning for obsolete usage

		/// <summary>
		/// Gets an action to zoom in on the content.
		/// </summary>
		/// <param name="control">The control for user interactions with the network.</param>
		/// <returns>An action to zoom in on the content.</returns>
#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public static INetworkAction GetZoomInActionButton(NetworkUserControl control) => new StaticNetworkAction(() => control?.ZoomIn(),
				name: ResString.GetMultilingualString("FA846438-001A-4C8B-952B-21FFFB10B467", "+"),
				description: ResString.GetMultilingualString("FFC01D18-FA7A-40AF-B22E-2B207B24CC7B", "Zoom in on the content (Ctrl++)"));
#pragma warning restore CS0618 // Restore the warning for obsolete usage

		/// <summary>
		/// Gets an action to Zoom out from the content.
		/// </summary>
		/// <param name="control">The control for user interactions with the network.</param>
		/// <returns>An action to Zoom out from the content.</returns>
#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public static INetworkAction GetZoomOutActionButton(NetworkUserControl control) => new StaticNetworkAction(() => control?.ZoomOut(),
				name: ResString.GetMultilingualString("DBC5CBAF-3B00-405C-B437-9C3D629BDF13", "-"),
				description: ResString.GetMultilingualString("2BA799A6-8DAA-4741-8EF3-39E94AAC0E63", "Zoom out from the content (Ctrl+-)"));
#pragma warning restore CS0618 // Restore the warning for obsolete usage

		/// <summary>
		/// Gets an action to jump back to the previous zoom level.
		/// </summary>
		/// <param name="networkViewModel">The view model representing the network.</param>
		/// <param name="control">The control for user interactions with the network.</param>
		/// <param name="group">The group number for organizing actions.</param>
		/// <returns>An action to jump back to the previous zoom level.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "resource name")]
#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public static INetworkAction GetJumpBackToPrevZoomAction(NetworkViewModel networkViewModel, NetworkUserControl control, int group = 0) => new StaticNetworkAction(() => control?.JumpBackToPrevZoom(),
				name: ResString.GetMultilingualString("8173333d-e2fb-44b0-9257-94f5a911018f", "Previous Zoom"),
				description: ResString.GetMultilingualString("dfa62d1e-cf9f-4074-93d8-1aa01d160db6", "Return to the previous zoom level"),
				group: group,
				iconName: "Previous");
#pragma warning restore CS0618 // Restore the warning for obsolete usage

		#region Color Actions

		static IEnumerable<INetworkAction> GetColourTransformActions(Action<Color> colorAction)
		{
			foreach (var color in new ColorList())
			{
				yield return new StaticNetworkAction(() => colorAction(color.Color), name: color.TranslatedName);
			}
		}

		#endregion
	}
}
