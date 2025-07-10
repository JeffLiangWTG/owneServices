using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.ZArchitecture.GUI.TranslationFeedbackManager;

namespace Enterprise.ZArchitecture.GUI
{
	public class DevInfoPopupManager : Disposable
	{
		public DevInfoPopupManager(Control control, ClickMode clickMode = ClickMode.OnClick)
		{
			this.control = control;
			this.captionedComponents = control as ICaptionedComponents;
			this.clickMode = clickMode;
			control.MouseMove += new MouseEventHandler(OnMouseMove);
			control.MouseLeave += new EventHandler(OnMouseLeave);
			switch (clickMode)
			{
				case ClickMode.OnClick: control.Click += new EventHandler(control_Click); break;
				case ClickMode.OnMouseUp: control.MouseUp += new MouseEventHandler(control_MouseUp); break;
			}
		}

		const int VK_F12 = 0x7B;

		[ThreadStatic]
		static bool isShowingDebugInfo;

		public static bool InDevelopInformationMode()
		{
			return InDeveloperInformationModeForTest || MouseHook.Win32.IsKeyDown(VK_F12) && !isShowingDebugInfo;
		}

		public static void ShowDevelopInfoForm(Control control)
		{
			try
			{
				isShowingDebugInfo = true;
				ZFormUtilities.ShowDebugInfo(control);
			}
			finally
			{
				isShowingDebugInfo = false;
			}
		}

#if DEBUG
		[ThreadStatic]
		static bool inDeveloperInformationModeForTest;

		public static IDisposable EnableDeveloperInformationModeForTest()
		{
			if (inDeveloperInformationModeForTest)
			{
				throw new InvalidOperationException("Developer information mode already enabled for testing");
			}
			else
			{
				inDeveloperInformationModeForTest = true;
				return new DisposableAction(delegate { inDeveloperInformationModeForTest = false; });
			}
		}
#endif

		static bool InDeveloperInformationModeForTest
		{
			get
			{
#if DEBUG
				return inDeveloperInformationModeForTest;
#else
				return false;
#endif
			}
		}

		public static void PaintCaptionHighlight(Control control)
		{
			PaintCaptionHighlight(control, ControlDpiScalingHelper.NewScaledRectangle(Point.Empty.X, Point.Empty.Y, control.Size.Width, control.Size.Height, false));
		}

		public static void PaintCaptionHighlight(Control control, Rectangle bounds)
		{
			control.Invalidate(bounds);
			control.Update();
			using (var graphics = control.CreateGraphics())
			{
				PaintCaptionHighlight(graphics, bounds);
			}
		}

		public static void PaintCaptionHighlight(Graphics graphics, Rectangle bounds)
		{
			graphics.FillRectangle(captionHighlightBrush.Value, bounds);
		}

		[ThreadSafe]
		static readonly Lazy<Brush> captionHighlightBrush = new Lazy<Brush>(() => new SolidBrush(Color.FromArgb(128, Color.Blue)));

		public void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (DevInfoPopupManager.InDevelopInformationMode())
			{
				UpdateActiveComponent(GetComponentAt(e.Location));
			}
			else
			{
				UpdateActiveComponent(null);
			}
		}

		public void OnMouseLeave(object sender, EventArgs e)
		{
			UpdateActiveComponent(null);
		}

		void UpdateActiveComponent(object newActiveComponent)
		{
			if (activeComponent != null && activeComponent != newActiveComponent)
			{
				control.Invalidate(GetComponentRect(activeComponent));
				control.Update();
			}
			if (newActiveComponent != null)
			{
				if (activeComponent == null)
				{
					controlCursor = control.Cursor;
					control.Cursor = Cursors.Arrow;
					Unfocus();
				}
				PaintCaptionHighlight(control, GetComponentRect(newActiveComponent));
			}
			else if (activeComponent != null)
			{
				control.Cursor = controlCursor;
			}
			activeComponent = newActiveComponent;
		}

		void Unfocus()
		{
			if (control.Focused)
			{
				var form = control.FindForm();
				if (form != null)
				{
					form.ActiveControl = null;
				}
			}
		}

		void control_Click(object sender, EventArgs e)
		{
			HandleClick();
		}

		void control_MouseUp(object sender, MouseEventArgs e)
		{
			HandleClick();
		}

		public bool HandleClick()
		{
			var handled = false;
			if (activeComponent != null)
			{
				if (DevInfoPopupManager.InDevelopInformationMode())
				{
					try
					{
						isShowingDebugInfo = true;
						ZFormUtilities.ShowDebugInfo(control);
					}
					finally
					{
						isShowingDebugInfo = false;
					}
					handled = true;
				}
			}
			return handled;
		}

		object GetComponentAt(Point p)
		{
			return captionedComponents != null ? captionedComponents.GetCaptionedComponentAt(p) : 0;
		}

		Rectangle GetComponentRect(object component)
		{
			return captionedComponents != null ? captionedComponents.GetCaptionedComponentRect(component) : ControlDpiScalingHelper.NewScaledRectangle(0, 0, control.Width, control.Height, false);
		}

		public bool InMode
		{
			get { return activeComponent != null; }
		}

		protected override void Dispose(bool isDisposing)
		{
			control.MouseMove -= new MouseEventHandler(OnMouseMove);
			control.MouseLeave -= new EventHandler(OnMouseLeave);
			switch (clickMode)
			{
				case ClickMode.OnClick: control.Click -= new EventHandler(control_Click); break;
				case ClickMode.OnMouseUp: control.MouseUp -= new MouseEventHandler(control_MouseUp); break;
			}
		}

		readonly protected internal Control control;
		readonly ICaptionedComponents captionedComponents;
		readonly ClickMode clickMode;
		Cursor controlCursor;
		object activeComponent;
	}
}
