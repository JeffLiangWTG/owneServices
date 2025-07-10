using CargoWise.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI
{
	/// <summary>
	/// This class is used as a regular ribbon in a network diagram or a base class for an extended ribbon.<br/>
	/// If a full customized ribbon is required, consider extend from <see cref="RibbonViewModel"/>.
	/// </summary>
	public class DefaultNetworkRibbonViewModel : RibbonViewModel
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a resource identifier")]
#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public DefaultNetworkRibbonViewModel(NetworkViewModel networkViewModel, NetworkUserControl control)
			: base()
		{
			if (networkViewModel == null || !HasResources())
			{
				IsSuccessfullyConstructed = false;
				return;
			}

			HomeTab = new RibbonTabViewModel(ResString.GetMultilingualString("DefaultRibbon|Home", "Home"));
			Tabs.Add(HomeTab);

			HomeActionsGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("DefaultRibbon|Home|Actions", "Actions"), "Refresh");
			HomeTab.Groups.Add(HomeActionsGroup);
			HomeActionsGroup.Items.Add(new RibbonButtonViewModel(this, "DefaultRibbon|Home|Actions|Refresh", DiagramNetworkActionProvider.GetRefreshAction(networkViewModel)));

			HomeEditRemoveGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("DefaultRibbon|Home|EditRemove", "Edit and Remove"), "Cog");
			HomeTab.Groups.Add(HomeEditRemoveGroup);
			HomeEditRemoveGroup.Items.Add(new RibbonButtonViewModel(this, "DefaultRibbon|Home|EditRemove|EditProperties", RibbonImageLayout.SmallImageOnly, new EditPropertiesAction(networkViewModel)));
			HomeEditRemoveGroup.Items.Add(new RibbonButtonViewModel(this, ResString.GetMultilingualString("DefaultRibbon|Home|EditRemove|RemoveFromDiagram", "Remove"), RibbonImageLayout.SmallImageOnly, new RemoveFromDiagramAction(networkViewModel)));
			HomeEditRemoveGroup.Items.Add(new RibbonButtonViewModel(this, "DefaultRibbon|Home|EditRemove|RemoveAndDeleteEntity", RibbonImageLayout.SmallImageOnly, new RemoveAndDeleteAction(networkViewModel)));

			HomeAffinitiesGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("DefaultRibbon|Home|Affinities", "Affinities"), "Plus");
			HomeTab.Groups.Add(HomeAffinitiesGroup);
			HomeAffinitiesGroup.Items.Add(new RibbonMenuButtonViewModel(this, ResString.GetMultilingualString("DefaultRibbon|Home|Affinities|ApplyAffinity", "Apply Affinity"), new ApplyAffinitiesAction(networkViewModel)));
			HomeAffinitiesGroup.Items.Add(new RibbonMenuButtonViewModel(this, ResString.GetMultilingualString("DefaultRibbon|Home|Affinities|RemoveAffinity", "Remove Affinity"), new RemoveAffinitiesAction(networkViewModel)));
			HomeAffinitiesGroup.Items.Add(new RibbonButtonViewModel(this, "DefaultRibbon|Home|Affinities|EditAffinity", new EditAffinitiesAction(networkViewModel)));

			DiagramTab = new RibbonTabViewModel(ResString.GetMultilingualString("DefaultRibbon|Diagram", "Diagram"));
			Tabs.Add(DiagramTab);

			DiagramAppearanceGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("DefaultRibbon|Diagram|Appearance", "Appearance"), "ColorBucket");
			DiagramTab.Groups.Add(DiagramAppearanceGroup);
			DiagramAppearanceGroup.Items.Add(new RibbonMenuButtonViewModel(this, "DefaultRibbon|Diagram|Appearance|BackgroundColor", DiagramNetworkActionProvider.GetBackgroundColorAction(networkViewModel)));

			ShapeTab = new RibbonTabViewModel(ResString.GetMultilingualString("DefaultRibbon|Shape", "Shape"));
			Tabs.Add(ShapeTab);

			ShapeOrderGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("DefaultRibbon|Shape|Order", "Order"), "BringToFront");
			ShapeTab.Groups.Add(ShapeOrderGroup);

			ShapeOrderGroup.Items.Add(new RibbonButtonViewModel(this, "DefaultRibbon|Shape|Order|BringToFront", new BringToFrontAction(networkViewModel)));
			ShapeOrderGroup.Items.Add(new RibbonButtonViewModel(this, "DefaultRibbon|Shape|Order|SendToBack", new SendToBackAction(networkViewModel)));

			ViewTab = new RibbonTabViewModel(ResString.GetMultilingualString("DefaultRibbon|View", "View"));
			Tabs.Add(ViewTab);

			ViewZoomGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("DefaultRibbon|View|Zoom", "Zoom"), "ZoomIn");
			ViewTab.Groups.Add(ViewZoomGroup);
			ViewZoomGroup.Items.Add(new RibbonButtonViewModel(this, "DefaultRibbon|View|Zoom|ZoomIn", DiagramNetworkActionProvider.GetZoomInAction(networkViewModel, control)));
			ViewZoomGroup.Items.Add(new RibbonButtonViewModel(this, "DefaultRibbon|View|Zoom|ZoomOut", DiagramNetworkActionProvider.GetZoomOutAction(networkViewModel, control)));
			ViewZoomGroup.Items.Add(new RibbonButtonViewModel(this, "DefaultRibbon|View|Zoom|OneHundredPercent", DiagramNetworkActionProvider.GetOneHundredPercentAction(networkViewModel, control)));
			ViewZoomGroup.Items.Add(new RibbonButtonViewModel(this, ResString.GetMultilingualString("DefaultRibbon|View|Zoom|Fit", "Fit"), new FitNodesAction(networkViewModel, control)));
			ViewZoomGroup.Items.Add(new RibbonButtonViewModel(this, ResString.GetMultilingualString("DefaultRibbon|View|Zoom|Fill", "Fill"), DiagramNetworkActionProvider.GetFillAction(networkViewModel, control)));
			ViewZoomGroup.Items.Add(new RibbonButtonViewModel(this, ResString.GetMultilingualString("DefaultRibbon|View|Zoom|Previous", "Previous"), DiagramNetworkActionProvider.GetJumpBackToPrevZoomAction(networkViewModel, control)));

			ViewShowHiddenGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("DefaultRibbon|View|ShowHidden", "Show Hidden"), "Entity");
			ViewTab.Groups.Add(ViewShowHiddenGroup);
			ViewShowHiddenGroup.Items.Add(new RibbonMenuButtonViewModel(this, "DefaultRibbon|View|ShowHidden|Entity", new ShowHiddenEntityAction(networkViewModel)));
			ViewShowHiddenGroup.Items.Add(new RibbonMenuButtonViewModel(this, "DefaultRibbon|View|ShowHidden|Dependency", new ShowHiddenDependencyAction(networkViewModel)));

			ViewWindowGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("DefaultRibbon|View|Window", "Window"), "PopOut");
			ViewTab.Groups.Add(ViewWindowGroup);
			ViewWindowGroup.Items.Add(new RibbonButtonViewModel(this, ResString.GetMultilingualString("DefaultRibbon|View|Window|PopOut", "Pop Out"), DiagramNetworkActionProvider.GetPopOutAction(networkViewModel, control)));
		}
#pragma warning restore CS0618 // Restore the warning for obsolete usage

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a location of resources")]
		protected override void LoadResources()
		{
			base.LoadResources();
#if !WINZOR
			AddResourceFile(@"pack://application:,,,/CargoWise.NetworkVisualisation.GUI;component/Ribbon/DefaultViewModel/DefaultNetworkRibbonResources.xaml");
#else
			AddResources(DefaultNetworkRibbonResources.Icons);
			AddResources(DefaultNetworkRibbonResources.IconsInBase64);
#endif
		}

		#region Implementation

		protected RibbonTabViewModel HomeTab { get; private set; }
		protected RibbonGroupViewModel HomeActionsGroup { get; private set; }
		protected RibbonGroupViewModel HomeEditRemoveGroup { get; private set; }
		protected RibbonGroupViewModel HomeLinkedEntityGroup { get; private set; }
		protected RibbonGroupViewModel HomeAffinitiesGroup { get; private set; }

		protected RibbonTabViewModel DiagramTab { get; private set; }
		protected RibbonGroupViewModel DiagramAppearanceGroup { get; private set; }

		protected RibbonTabViewModel ShapeTab { get; private set; }

		protected RibbonGroupViewModel ShapeOrderGroup { get; private set; }

		protected RibbonTabViewModel ViewTab { get; private set; }
		protected RibbonGroupViewModel ViewZoomGroup { get; private set; }
		protected RibbonGroupViewModel ViewShowHiddenGroup { get; private set; }
		protected RibbonGroupViewModel ViewWindowGroup { get; private set; }

		#endregion
	}
}
