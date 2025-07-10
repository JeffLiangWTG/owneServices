using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Notifications
{
	public sealed class NotificationAdornmentFactory
	{
#if DEBUG
		internal
#endif
			static NotificationAdornmentFactory Instance
		{
			get { return instance.Value; }
#if DEBUG
			set { instance = new Lazy<NotificationAdornmentFactory>(() => { return value; }); }
#endif
		}
		[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Safely handled, required for forms on other threads")]
		static Lazy<NotificationAdornmentFactory> instance = new Lazy<NotificationAdornmentFactory>();

		readonly List<IAdornmentLayout> layouts = new List<IAdornmentLayout>();
		readonly IAdornmentLayout defaultLayout = new DefaultAdornmentLayout();

		public static void RegisterCustomLayout(IAdornmentLayout layout)
		{
			lock (Instance.layouts)
			{
				Instance.layouts.Add(layout);
			}
		}

		public static NotificationAdornment Create(Control control)
		{
			var result = new CompositeAdornment();
			var layout = Instance.GetLayout(control.GetType());

			AddBackgroundAdornments(result, layout, control);
			AddIconAdornments(result, layout, control);

			return result;
		}

		static void AddBackgroundAdornments(CompositeAdornment result, IAdornmentLayout layout, Control control)
		{
			foreach (var target in layout.GetBackroundAdornmentTargets(control))
			{
				var adornment = new BackgroundAdornment();
				adornment.SetControl(target);
				result.Add(adornment);
			}
		}

		static void AddIconAdornments(CompositeAdornment result, IAdornmentLayout layout, Control control)
		{
			foreach (var info in layout.GetIconAdornmentTargets(control))
			{
				var adornment = new IconAdornment();
				adornment.SetControl(info.Target);
				adornment.IconLayout = info;
				result.Add(adornment);
			}
		}

		internal IAdornmentLayout GetLayout(Type controlType)
		{
			IAdornmentLayout result = null;
			lock (layouts)
			{
				result = layouts.FirstOrDefault(item => item.ControlType == controlType);

				if (result == null)
				{
					result = layouts.FirstOrDefault(item => item.ControlType.IsAssignableFrom(controlType));
				}
			}

			if (result == null)
			{
				result = defaultLayout;
			}

			return result;
		}
	}
}
