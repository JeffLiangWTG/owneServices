using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// All credit to this class goes to Rick Brewster: http://blogs.msdn.com/b/rickbrew/archive/2006/01/09/511003.aspx
	/// This is a subclass for ToolStrip, that adds fallthrough (when you click on a ztoolstripbutton while the window is not focused, it should act like you clicked it too)
	/// It also fixes the Microsoft bug which causes a white line under the toolstrip
	/// </summary>
	public class ZToolStrip
		: KToolStrip
	{
		public ZToolStrip()
		{
#if !WINZOR
			if (Math.Abs(ImageScalingSize.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(16)) <= 1) // Allow 1 pixel difference for different rounding by ControlDpiScalingHelper and System.Windows.Forms.DpiHelper
			{
				// Images scaling already is done by ToolStrip class automatically in its constructor
				imageScalingSizeSetFlag = true;
			}
			readyForScaling = true;
#endif
			Renderer = new FixedToolStripRenderer();
		}

#if !WINZOR
		#region Click-Through

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);

			if (m.Msg == NativeConstants.WM_MOUSEACTIVATE &&
				m.Result == (IntPtr)NativeConstants.MA_ACTIVATEANDEAT)
			{
				m.Result = (IntPtr)NativeConstants.MA_ACTIVATE;
			}
		}

		#endregion

		#region CJK Layout

		protected override void OnLayout(LayoutEventArgs e)
		{
			ModifyCjkVerticalLayout();
			ModifyImageScalingSize();
			base.OnLayout(e);
		}

		readonly bool readyForScaling;
		bool imageScalingSizeSetFlag;
		void ModifyImageScalingSize()
		{
			if (readyForScaling && !imageScalingSizeSetFlag)
			{
				ImageScalingSize = ControlDpiScalingHelper.NewScaledSize(ImageScalingSize);
				imageScalingSizeSetFlag = true;
			}
		}

		void ModifyCjkVerticalLayout()
		{
			foreach (ToolStripItem item in this.Items)
			{
				if (!item.IsDisposed)
				{
					if (item.Text.Length > 0 && item.Text[0].IsCjk() && (item.TextDirection == ToolStripTextDirection.Vertical270 || item.TextDirection == ToolStripTextDirection.Vertical90))
					{
						item.TextDirection = ToolStripTextDirection.Horizontal;
						var modifiedText = new char[item.Text.Length * 3];
						for (var i = 0; i < item.Text.Length; i++)
						{
							modifiedText[i * 3] = item.Text[i];
							modifiedText[i * 3 + 1] = '\r';
							modifiedText[i * 3 + 2] = '\n';
						}
						item.Text = new string(modifiedText);
					}
				}
			}
		}
		#endregion
#endif

		#region Focus on click

		/// <summary>
		/// Specifies if this toolstrip should be focused when it or any of its items is clicked or invoked indirectly (with shortcut)
		/// </summary>
		[DefaultValue(false)]
		public bool FocusOnClick { get; set; }

		#endregion

		#region Overrides

		protected override void Select(bool directed, bool forward) //a
		{
			if (directed && AlwaysSelectLast)
			{
				base.Select(directed, false);
				//at some point ToolStrip logic changed and selecting one from the end doesn't seem to work consistently anymore... so let's force it
				typeof(ToolStrip).GetMethod("ChangeSelection", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(this, new object[] { Items.OfType<ToolStripItem>().LastOrDefault() });
			}
			else
			{
				base.Select(directed, forward);
			}
		}

		[DefaultValue(false)]
		public bool AlwaysSelectLast { get; set; }

		protected override void OnClick(EventArgs e)
		{
			if (FocusOnClick && !Focused)
			{
				Focus();
			}

			base.OnClick(e);
		}

		protected override void OnMouseClick(MouseEventArgs e)
		{
			if (FocusOnClick && !Focused)
			{
				Focus();
			}

			base.OnMouseClick(e);
		}

		#endregion
	}

	#region Renderer

	/// <summary>
	///	Microsoft suggested fix for 'white line under toolstrip' bug
	///	http://stackoverflow.com/questions/1918247/how-to-disable-the-line-under-tool-strip-in-winform-c	
	/// When we use ToolStripSystemRenderer and change system display scale, Windows will help us create an image for ToolStripSplitButton's arrow which will raise the arrow missing issue.
	/// While ToolStripProfessionalRenderer can help us draw the arrow manually.
	/// https://referencesource.microsoft.com/#System.Windows.Forms/winforms/Managed/System/WinForms/ToolStripProfessionalRenderer.cs,1441
	/// </summary>
	class FixedToolStripRenderer : ToolStripProfessionalRenderer
	{
		public FixedToolStripRenderer()
		{
#if !WINZOR
			Offset2X = ControlDpiScalingHelper.ScaleToCurrentDpiX(3);
			Offset2Y = ControlDpiScalingHelper.ScaleToCurrentDpiY(3);
#endif
		}

		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
		{
		}
	}

	#endregion

	#region Win32

	internal static class NativeConstants
	{
		internal const uint WM_MOUSEACTIVATE = 0x21;
		internal const uint MA_ACTIVATE = 1;
		internal const uint MA_ACTIVATEANDEAT = 2;
		internal const uint MA_NOACTIVATE = 3;
		internal const uint MA_NOACTIVATEANDEAT = 4;
	}

	#endregion
}
