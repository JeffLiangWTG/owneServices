using System.Collections.Generic;
#if !WINZOR
using System.Windows.Media;
#endif
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class RibbonViewModel : RibbonViewModelBase
	{
		public RibbonViewModel()
			: base(key: null)
		{
			LoadResources();
		}

		public bool IsSuccessfullyConstructed { get; protected set; } = true;

		public RibbonCollection<RibbonTabViewModel> Tabs { get; } = new RibbonCollection<RibbonTabViewModel>();

		public void RefreshActions(RefreshArgs args)
		{
			foreach (var button in ButtonViewModels)
			{
				button.RefreshActionAndProperties(args, this);
			}
		}

		public IEnumerable<RibbonButtonViewModel> ButtonViewModels
		{
			get
			{
				foreach (var tab in Tabs)
				{
					foreach (var group in tab.Groups)
					{
						foreach (var button in group.Items)
						{
							yield return button;
						}
					}
				}
			}
		}

		public void DeactivateButtons()
		{
			foreach (var button in ButtonViewModels)
			{
				button.Deactivate();
			}
		}

#region Resources

		NetworkRibbonResourcesProvider ResourcesProvider { get; } = new NetworkRibbonResourcesProvider();

		protected bool HasResources() => ResourcesProvider.HasResources();

#if !WINZOR
		protected void AddResourceFile(string path) => ResourcesProvider.AddRibbonResources(path);

		public Drawing GetResource(string resourceName) => ResourcesProvider.GetResource(resourceName);
#else
		public void AddResources(Dictionary<string, string> resources) => ResourcesProvider.AddRibbonResources(resources);

		public string GetResource(string resourceName) => ResourcesProvider.GetResource(resourceName);
#endif

		protected virtual void LoadResources()
		{
		}

#endregion
	}
}
