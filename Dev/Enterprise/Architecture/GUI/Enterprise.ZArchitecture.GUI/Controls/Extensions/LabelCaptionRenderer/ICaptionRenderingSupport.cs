using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public interface ICaptionRenderingSupport
	{
		bool? CaptionRenderingEnabled { get; }
		event EventHandler CaptionRenderingEnabledChanged;
	}

	public static class CaptionRenderingSupport
	{
		public static bool IsCaptionRenderingEnabled(Control control)
		{
			var current = control is Form ? control : control.Parent;
			while (current != null)
			{
				var captionRenderingSupport = current as ICaptionRenderingSupport;
				if (captionRenderingSupport != null && captionRenderingSupport.CaptionRenderingEnabled != null)
				{
					return captionRenderingSupport.CaptionRenderingEnabled.Value;
				}
				current = current.Parent;
			}
			return false;
		}

		public static bool ShouldSerializeCaptionRenderingEnabled(this ICaptionRenderingSupport support)
		{
			var result = false;
			var component = support as IComponent;
			if (component != null && component.Site != null)
			{
				var designerHost = (IDesignerHost)component.Site.GetService(typeof(IDesignerHost));
				result = designerHost.RootComponent == component;
			}
			return result;
		}

		public static void RefreshCaptionLabel(this Control control)
		{
#if DEBUG
			if (DesignModeFinder.IsDesigning)
			{
				var extendedControl = control as IExtendedControl;
				if (extendedControl != null)
				{
					var labelCaptionRenderer = extendedControl.Extensions.Get<ILabelCaptionRenderer>();
					if (labelCaptionRenderer != null)
					{
						labelCaptionRenderer.Refresh();
					}
				}
			}
#endif
		}

		public static void UpdateCaption(this Control control)
		{
			var resCaptionedControl = control as IResCaptionedControl;
			if (resCaptionedControl != null && resCaptionedControl.CaptionResourceString != null && !resCaptionedControl.CaptionResourceString.IsEmpty())
			{
				resCaptionedControl.CaptionResourceString = Res._GetData(ResourceStringAssemblyIdAttribute.IKnowTheAssemblyIsAlreadyLoaded, resCaptionedControl.CaptionResourceString);
			}
			else
			{
				ZLabelCaptionCache.Instance.ClearCache();
			}
			var extendedControl = control as IExtendedControl;
			if (extendedControl != null)
			{
				var labelCaptionRenderer = extendedControl.Extensions.Get<ILabelCaptionRenderer>();
				if (labelCaptionRenderer != null)
				{
					labelCaptionRenderer.Refresh();
				}
			}
		}
	}
}
