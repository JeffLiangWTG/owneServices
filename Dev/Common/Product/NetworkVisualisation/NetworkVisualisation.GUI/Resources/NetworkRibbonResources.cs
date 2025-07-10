using System;
using System.Windows;
using System.Windows.Media;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class NetworkRibbonResources
	{
		public NetworkRibbonResources(Uri resourceFileUri)
		{
			ribbonResources = new ResourceDictionary
			{
				Source = resourceFileUri
			};
		}

		public Drawing GetDrawing(string resourceName) => Get<Drawing>(resourceName);

		T Get<T>(string resourceName) where T : class
		{
			var resource = ribbonResources[resourceName];

			return (T)resource;
		}

		readonly ResourceDictionary ribbonResources;
	}
}
