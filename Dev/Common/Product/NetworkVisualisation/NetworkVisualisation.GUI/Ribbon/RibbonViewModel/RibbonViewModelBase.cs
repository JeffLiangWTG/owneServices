using System;
#if !WINZOR
using CargoWise.Main.Navigation.ViewModels;
#endif

namespace CargoWise.NetworkVisualisation.GUI
{
	public abstract class RibbonViewModelBase : ViewModelWithNotification
	{
		protected RibbonViewModelBase(string key)
		{
			Key = key;
		}

		public string Key { get; }

		protected static string GetUniqueKey() => Guid.NewGuid().ToString();
	}
}
