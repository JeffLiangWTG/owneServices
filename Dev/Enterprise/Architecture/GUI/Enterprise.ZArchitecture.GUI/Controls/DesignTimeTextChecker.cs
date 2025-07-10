using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;

using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	internal static class DesignTimeTextChecker
	{
		public static void Subscribe(ISite site, IVariableLengthCaptionRenderer control)
		{
			var changeService =  site == null ? null : (IComponentChangeService)site.GetService(typeof(IComponentChangeService));
			if (changeService != null)
			{
				changeService.ComponentChanged -= OnComponentChanged;
				changeService.ComponentChanged += OnComponentChanged;
			}
		}

		public static void UnSubscribe(ISite site, IVariableLengthCaptionRenderer control)
		{
			var changeService = site == null ? null : (IComponentChangeService)site.GetService(typeof(IComponentChangeService));
			if (changeService != null)
			{
				changeService.ComponentChanged -= OnComponentChanged;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Development string")]
		static void OnComponentChanged(object sender, ComponentChangedEventArgs e)
		{
			var captionRenderer = e.Component as IVariableLengthCaptionRenderer;
			var component = e.Component as IComponent;
			var site = component == null ? null : component.Site;
			var designerHost = site == null ? null : (IDesignerHost)site.GetService(typeof(IDesignerHost));
			var warned = designerHost == null ? null : (WarnedControlsList)designerHost.GetService(typeof(WarnedControlsList));

			if (e.Member != null &&
				e.Member.Name == "Text" &&
				site != null &&
				captionRenderer != null &&
				designerHost != null &&
				!designerHost.Loading &&
				(warned == null || !warned.Warned.ContainsKey(site.Name)) &&
				captionRenderer.IsCaptionOverridden)
			{
				DesignTimeUI.ShowMessageOnceUntilIdle(designerHost, "Please use Resource Strings to set this up captions to provide multi-lingual support.\nhttps://wisetechglobal.sharepoint.com/Development/Development%20Wiki/ResourceStringEditing.aspx");
				if (warned == null)
				{
					warned = new WarnedControlsList();
					designerHost.AddService(typeof(WarnedControlsList), warned);
				}
				warned.Warned.Add(site.Name, site.Name);
			}
		}

		class WarnedControlsList
		{
			public readonly IDictionary<string, string> Warned = new Dictionary<string, string>();
		}
	}
}
