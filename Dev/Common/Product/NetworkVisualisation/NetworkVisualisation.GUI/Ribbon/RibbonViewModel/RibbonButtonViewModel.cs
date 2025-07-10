using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.GUI
{
	/// <summary>
	/// This view model used in <see cref="RibbonGroupViewModel"/> to create buttons as part of Ribbon groups such as Insert, Linked Entity under Home Ribbon Tab and
	/// clone Diagram under Diagram Tab, Approve Diagram and Show Dependencies under Schedule Ribbon Tab, these buttons are used to perfrom network actions
	/// such as create shape and annotations and approve diagrams, clone diagrams and show dependecies etc.
	/// </summary>
	public class RibbonButtonViewModel : RibbonGraphicContentViewModel
	{
		public RibbonButtonViewModel(RibbonViewModel ribbonViewModel, INetworkAction action)
			: this(ribbonViewModel, imageLayout: RibbonImageLayout.BothLargeAndSmallImages, action)
		{
		}
		public RibbonButtonViewModel(RibbonViewModel ribbonViewModel, RibbonImageLayout imageLayout, INetworkAction action)
			: this(ribbonViewModel, GetUniqueKey(), imageLayout, action)
		{
		}

		public RibbonButtonViewModel(RibbonViewModel ribbonViewModel, string key, INetworkAction action)
			: this(ribbonViewModel, key, RibbonImageLayout.BothLargeAndSmallImages, action)
		{
		}

		public RibbonButtonViewModel(RibbonViewModel ribbonViewModel, string key, RibbonImageLayout imageLayout, INetworkAction action)
			: this(ribbonViewModel, key, label: null, tooltip: null, action?.GetIconName(), imageLayout, action)
		{
		}

		public RibbonButtonViewModel(RibbonViewModel ribbonViewModel, ResourceString label, INetworkAction action)
			: this(ribbonViewModel, key: label.ResourceKey, label, tooltip: null, action?.GetIconName(), imageLayout: RibbonImageLayout.BothLargeAndSmallImages, action)
		{
		}

		public RibbonButtonViewModel(RibbonViewModel ribbonViewModel, ResourceString label, RibbonImageLayout imageLayout, INetworkAction action)
			: this(ribbonViewModel, key: label.ResourceKey, label, tooltip: null, action?.GetIconName(), imageLayout, action)
		{
		}

		public RibbonButtonViewModel(RibbonViewModel ribbonViewModel, ResourceString label, ResourceString tooltip, INetworkAction action)
			: this(ribbonViewModel, key: label.ResourceKey, label, tooltip, action?.GetIconName(), imageLayout: RibbonImageLayout.BothLargeAndSmallImages, action)
		{
		}

		public RibbonButtonViewModel(RibbonViewModel ribbonViewModel, ResourceString label, ResourceString tooltip, string iconName, RibbonImageLayout imageLayout, INetworkAction action)
			: this(ribbonViewModel, label.ResourceKey, label, tooltip, iconName, imageLayout, action)
		{
		}

		protected RibbonButtonViewModel(RibbonViewModel ribbonViewModel, string key, ResourceString label, ResourceString tooltip, string iconName, RibbonImageLayout imageLayout, INetworkAction action = null)
			: base(ribbonViewModel, key, iconName, imageLayout)
		{
			Action = action;
			RefreshPropertiesFromAction(ribbonViewModel, label, tooltip);
		}

		internal void RefreshActionAndProperties(RefreshArgs args, RibbonViewModel ribbonViewModel)
		{
			Action?.Refresh(args);

			var newLabel = isLabelManuallySet ? Label : null;
			RefreshPropertiesFromAction(ribbonViewModel, newLabel, null);
		}

		void RefreshPropertiesFromAction(RibbonViewModel ribbonViewModel, ResourceString newLabel, ResourceString newTooltip)
		{
			if (isDeactivated)
			{
				return;
			}

			if (newLabel != null)
			{
				Label = newLabel;
				isLabelManuallySet = true;
			}
			else if (Action != null)
			{
				Label = Action.GetName();
			}
			else
			{
				throw new ArgumentException("Label should be defined either directly or as network action name.", nameof(newLabel));
			}

			if (newTooltip != null)
			{
				Tooltip = newTooltip;
			}
			else if (Action != null)
			{
				Tooltip = NetworkActionHelper.GetTooltipForAction(Action.GetDescription(), Action.IsEnabled());
			}
			else
			{
				throw new ArgumentException("Tooltip should be defined either directly or as network action name.", nameof(newTooltip));
			}

			if (Action != null)
			{
				IsChecked = Action.IsActivated();

				if (Action is DynamicNetworkAction || ChildButtons == null)
				{
					DeactivateChildButtons();
					// we need to assign an actual array instead of a deferred linq request here as latter makes WPF's binding engine keep strong references to network actions
					// that may result in keeping NetworkViewModel, network entity controller and other associated objects including the form in memory
					ChildButtons = Action.GetChildActions().Select(i => GetChildButtonViewModel(ribbonViewModel, i)).ToArray();
				}

				IsEnabled = Action.IsEnabled().IsAllowed;
			}
		}

		bool isLabelManuallySet;

		public INetworkAction Action { get; private set; }

		#region Bindable Properties

		public ResourceString Label
		{
			get => label;
			set
			{
				label = value;
				OnPropertyChanged(nameof(Label));
			}
		}

		ResourceString label;

		public string Tooltip
		{
			get => tooltip;
			set
			{
				tooltip = value;
				OnPropertyChanged(nameof(Tooltip));
			}
		}

		string tooltip;

		/// <summary>
		/// This value is set if network action is activated for the button <see cref="INetworkAction.IsActivated()"/>.
		/// </summary>
		public bool IsChecked
		{
			get => isChecked;
			set
			{
				isChecked = value;
				OnPropertyChanged(nameof(IsChecked));
			}
		}

		bool isChecked;

		public IEnumerable<RibbonButtonViewModel> ChildButtons
		{
			get => childButtons;
			set
			{
				childButtons = value;
				OnPropertyChanged(nameof(ChildButtons));
			}
		}

		IEnumerable<RibbonButtonViewModel> childButtons;

		public bool IsEnabled
		{
			get => isEnabled;
			set
			{
				isEnabled = value;
				OnPropertyChanged(nameof(IsEnabled));
			}
		}

		bool isEnabled = true;

		#endregion

		#region Child Button View Models

		RibbonButtonViewModel GetChildButtonViewModel(RibbonViewModel ribbonViewModel, INetworkAction action)
		{
			return new RibbonButtonViewModel(ribbonViewModel, RibbonImageLayout.SmallImageOnly, action);
		}

		#endregion

		#region Deactivation to Prevent Memory Leaks

		bool isDeactivated;

		public void Deactivate()
		{
			Action = null;
			DeactivateChildButtons();
			ChildButtons = Array.Empty<RibbonButtonViewModel>();
			isDeactivated = true;
		}

		void DeactivateChildButtons()
		{
			if (ChildButtons != null)
			{
				foreach (var childButton in ChildButtons)
				{
					childButton.Deactivate();
				}
			}
		}

		#endregion
	}
}
