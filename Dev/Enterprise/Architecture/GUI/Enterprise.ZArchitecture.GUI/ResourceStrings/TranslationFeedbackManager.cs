using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Integration;
#if !WINZOR
using CargoWise.Main.Navigation.WPF;
#endif
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

#if DEBUG
using CargoWise.Types;
#endif

namespace Enterprise.ZArchitecture.GUI
{
#if !WINZOR
	public class TranslationFeedbackManagerProvider : IWpfTranslationFeedbackManager
	{
		public bool InTranslationFeedbackMode()
		{
			return TranslationFeedbackManager.InTranslationFeedbackMode();
		}

		public void OpenFeedbackForm(string caption)
		{
			TranslationFeedbackManager.OpenFeedbackForm(null, caption);
		}

		public void OpenFeedbackForm(MultilingualString multilingualString)
		{
			TranslationFeedbackManager.OpenFeedbackForm(null, multilingualString);
		}

#if DEBUG
		public IDisposable EnableTranslationFeedbackModeForTest()
		{
			return TranslationFeedbackManager.EnableTranslationFeedbackModeForTest();
		}
#endif
	}
#endif

	public partial class TranslationFeedbackManager : Disposable
	{
#if !WINZOR
		public static bool InTranslationFeedbackMode()
		{
			return
				(InTranslationFeedbackModeForTest || MouseHook.Win32.IsKeyDown(VK_F2)) &&
				(!Res.IsEnglish(Res.CurrentLanguage) || Globals.IsDebugMode) &&
				EnvProxy.Instance.Security != null && EnvProxy.Instance.Security.TranslationFeedback.IsAllowed;
		}

		const int VK_F2 = 0x71;
#endif

		static bool InTranslationFeedbackModeForTest
		{
			get
			{
#if DEBUG
				return inTranslationFeedbackModeForTest;
#else
				return false;
#endif
			}
		}

#if DEBUG
		[ThreadStatic]
		static bool inTranslationFeedbackModeForTest;

		public static IDisposable EnableTranslationFeedbackModeForTest()
		{
			if (inTranslationFeedbackModeForTest)
			{
				throw new InvalidOperationException("Translation feedback mode already enabled for testing");
			}
			else
			{
				inTranslationFeedbackModeForTest = true;
				return new DisposableAction(delegate { inTranslationFeedbackModeForTest = false; });
			}
		}
#endif

		#region PaintCaptionHighlight

		// Generally we paint over a control directly instead of using the control's paint event
		// this is done to handle controls implemented by native windows such as readonly textboxs and tabs

		public static void PaintCaptionHighlight(Control control)
		{
			PaintCaptionHighlight(control, ControlDpiScalingHelper.NewScaledRectangle(Point.Empty.X, Point.Empty.Y, control.Size.Width, control.Size.Height, false));
		}

		public static void PaintCaptionHighlight(Control control, Rectangle bounds)
		{
#if !WINZOR
			control.Invalidate(bounds);
			control.Update();
			using (var graphics = control.CreateGraphics())
			{
				PaintCaptionHighlight(graphics, bounds);
			}
#endif
		}

		public static void PaintCaptionHighlight(Graphics graphics, Rectangle bounds)
		{
#if !WINZOR
			using (var captionHighlightBrush = new SolidBrush(Color.FromArgb(128, Color.LightSeaGreen)))
			{
				graphics.FillRectangle(captionHighlightBrush, bounds);
			}
#endif
		}

		#endregion

		#region OpenFeedbackForm

		public static void OpenFeedbackForm(Control control)
		{
			var labelCaptionRender = control is IExtendedControl ? ((IExtendedControl)control).Extensions.Get<ILabelCaptionRenderer>() : null;
			var renderedCaption = labelCaptionRender != null && (!labelCaptionRender.IsCaptionOverridden || string.IsNullOrEmpty(control.Text) || !(control is IVariableLengthCaptionRenderer)) ? labelCaptionRender.Caption : control.Text;
			if (control is ZForm)
			{
				renderedCaption = ((ZForm)control).FormCaption;
			}

			var resCaptionedControl = control as IResCaptionedControl;
			if (resCaptionedControl != null && resCaptionedControl.CaptionResourceString != null && !resCaptionedControl.CaptionResourceString.IsEmpty())
			{
				OpenFeedbackForm(control, resCaptionedControl.CaptionResourceString, renderedCaption);
			}
			else
			{
				var dataString = new ResourceStringKeyCalculator(control).DataString;
				if ((dataString != null && !dataString.IsEmpty()) || (!string.IsNullOrEmpty(dataString.Key) && Res.CurrentLanguage == Res.DefaultLanguage))
				{
					OpenFeedbackForm(control, dataString, renderedCaption);
				}
				else
				{
					OpenFeedbackForm(control, renderedCaption);
				}
			}
		}

		public static void OpenFeedbackForm(Control control, object selectedValue)
		{
			if (selectedValue is ICodeDescription)
			{
				OpenFeedbackForm(control, (ICodeDescription)selectedValue);
			}
			else if (selectedValue is ZGridColumn)
			{
				OpenFeedbackForm(control, (ZGridColumn)selectedValue);
			}
			else if (selectedValue is ZGridColumnGroup)
			{
				if (((ZGridColumnGroup)selectedValue).GroupName != null && !string.IsNullOrEmpty(((ZGridColumnGroup)selectedValue).GroupName.Key))
				{
					OpenFeedbackForm(control, ((ZGridColumnGroup)selectedValue).GroupName);
				}
				else
				{
					OpenFeedbackForm(control, ((ZGridColumnGroup)selectedValue).Columns[0]);
				}
			}
			else if (selectedValue is ResourceStringData)
			{
				OpenFeedbackForm(control, (ResourceStringData)selectedValue);
			}
			else if (selectedValue is MultilingualString)
			{
				OpenFeedbackForm(control, (MultilingualString)selectedValue);
			}
			else if (selectedValue is IMultilingualDescription)
			{
				OpenFeedbackForm(control, ((IMultilingualDescription)selectedValue).MultilingualDescription);
			}
			else if (selectedValue is Control)
			{
				OpenFeedbackForm((Control)selectedValue);
			}
			else if (selectedValue is TreeNode)
			{
				OpenFeedbackForm(control, ((TreeNode)selectedValue).Text);
			}
			else
			{
				OpenFeedbackForm(control, selectedValue.ToString());
			}
		}

		public static void OpenFeedbackForm(Control control, ICodeDescription selectedValue)
		{
			ObjectFactory.Get<ITranslationFeedbackProvider>().Feedback(control, selectedValue);
		}

		public static void OpenFeedbackForm(Control control, ZGridColumn selectedValue)
		{
			var columnStyle = (ZGridColumnStyle)selectedValue.ColumnStyle;
			var renderedCaption = columnStyle.HeaderText;
			if (columnStyle.Alignment == HorizontalAlignment.Right && renderedCaption.EndsWith(ZGridColumnStyle.PadRightPadding))
			{
				renderedCaption = renderedCaption.Substring(0, renderedCaption.Length - 2);
			}
			if (columnStyle.ResourceHeader.Data != null)
			{
				OpenFeedbackForm(control, columnStyle.ResourceHeader.Data, renderedCaption);
			}
			else
			{
				OpenFeedbackForm(control, renderedCaption);
			}
		}

		public static void OpenFeedbackForm(Control control, ResourceStringData resourceStringData)
		{
			ObjectFactory.Get<ITranslationFeedbackProvider>().Feedback(control, resourceStringData);
		}

		public static void OpenFeedbackForm(Control control, ResourceStringData resourceStringData, string renderedCaption, bool hasAccelerators = false)
		{
			ObjectFactory.Get<ITranslationFeedbackProvider>().Feedback(control, resourceStringData, renderedCaption, hasAccelerators);
		}

		public static void OpenFeedbackForm(Control control, MultilingualString multilingualString)
		{
			ObjectFactory.Get<ITranslationFeedbackProvider>().Feedback(control, multilingualString);
		}

		public static void OpenFeedbackForm(Control control, string caption)
		{
			ObjectFactory.Get<ITranslationFeedbackProvider>().Feedback(control, caption);
		}

		#endregion

		#region Control Instance TranslationFeedbackManager

		public enum ClickMode
		{
			OnClick,
			OnMouseUp,
			None
		}

#if !WINZOR
		public TranslationFeedbackManager(Control control, ClickMode clickMode = ClickMode.OnClick)
		{
			this.control = control;
			this.captionedComponents = control as ICaptionedComponents;
			control.MouseMove += new MouseEventHandler(OnMouseMove);
			control.MouseLeave += new EventHandler(OnMouseLeave);
			this.clickMode = clickMode;
			switch (clickMode)
			{
				case ClickMode.OnClick: control.Click += new EventHandler(control_Click); break;
				case ClickMode.OnMouseUp: control.MouseUp += new MouseEventHandler(control_MouseUp); break;
			}
		}

		public void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (TranslationFeedbackManager.InTranslationFeedbackMode())
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
#endif

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
			if (InMode)
			{
				Unfocus();
				OpenFeedbackForm();
				handled = true;
			}
			return handled;
		}

#if !WINZOR
		object GetComponentAt(Point p)
		{
			return captionedComponents != null ? captionedComponents.GetCaptionedComponentAt(p) : 0;
		}

		Rectangle GetComponentRect(object component)
		{
			return captionedComponents != null ? captionedComponents.GetCaptionedComponentRect(component) : ControlDpiScalingHelper.NewScaledRectangle(0, 0, control.Width, control.Height, false);
		}

		protected virtual void OpenFeedbackForm()
		{
			if (captionedComponents != null)
			{
				TranslationFeedbackManager.OpenFeedbackForm(control, captionedComponents.GetCaptionedComponentData(activeComponent));
			}
			else
			{
				TranslationFeedbackManager.OpenFeedbackForm(control);
			}
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
#endif

#if DEBUG
		public override string ToString()
		{
			var controlsDescription = new ZStringBuilder();
			controlsDescription.Append(GetType().Name);

			if (control != null)
			{
				controlsDescription.Append(" [").Append(ControlDescription.GetControlPath(control)).Append("]");
			}
			return controlsDescription.ToString();
		}
#endif

		#endregion
	}
}
