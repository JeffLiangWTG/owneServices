using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Windows.UI;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public static class TextBoxControlSize
	{
		public static int GetControlWidth(Control control, int length)
		{
			try
			{
				var result = 0;

				if (length > 0 && length < short.MaxValue)
				{
					var maxSizeCode = new String('W', length);
					var baseWidth = MeasureText(control, maxSizeCode).Width;
					result = (int)(baseWidth * EnterpriseFormLookStrategy.HorizontalScaleFactor * LengthScaleFactor(length));
				}

				return result;
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
				return control.Width;
			}
		}

		public static void ResizeControl(Control control, int length)
		{
			if (length > 0 && length < short.MaxValue)
			{
				var newWidth = GetControlWidth(control, length);
				if (newWidth > 0)
				{
					ControlDpiScalingHelper.SetWidth(ref control, newWidth, false);
				}
			}
		}

		#region Implementation

		#if !WINZOR

		static SizeF MeasureText(Control control, string value)
		{
			return TextRendererHelper.MeasureText(GetGraphics(control), value, new Font(OFont.DefaultFontName, 8), TextRendererType.GDIPlus);
		}

		#else

		static SizeF MeasureText(Control control, string value)
		{
			return TextRenderer.MeasureText(value, new Font(OFont.DefaultFontName, 8));
		}

		#endif

		static float LengthScaleFactor(int length)
		{
			var scale = 1F;

			if (length > 20)
			{
				scale = 0.6F;
			}
			else if (length > 12)
			{
				scale = 0.7F;
			}
			else if (length > 8)
			{
				scale = 0.8F;
			}
			else if (length > 5)
			{
				scale = 0.9F;
			}

			return scale;
		}

		#if !WINZOR

		static Graphics GetGraphics(Control control)
		{
			if (graphics == null)
			{
				graphics = new WeakReference(null);
			}
			var result = graphics.Target as Graphics;
			if (result == null)
			{
				using (var form = new Form()) // this form is only used to get a Graphics object
				{
					result = form.CreateGraphics();
				}
				graphics.Target = result;
				if (!control.IsDisposed)
				{
					control.SetUserData(sharedDataKey, result);
				}
			}
			return result;
		}
		[ThreadStatic]
		static WeakReference graphics;
		[SuppressThreadStaticFieldMessage]
		static readonly int sharedDataKey = ControlExtensions.CreateUserDataKey();

		#endif

		#endregion
	}
}
