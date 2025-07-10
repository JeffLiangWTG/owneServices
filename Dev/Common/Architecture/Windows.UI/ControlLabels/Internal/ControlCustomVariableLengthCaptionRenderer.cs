using System;
using System.Windows.Forms;
using CargoWise.Common;

namespace CargoWise.Windows.UI
{
	internal delegate string[] GetCaptionsDelegate();

	/// <summary>
	/// Manages a custom IVariableLengthCaptionRenderer, deferring setting the Captions property until the control is visible.
	/// </summary>
	internal class ControlCustomVariableLengthCaptionRenderer : IVariableLengthCaptionRenderer
	{
		public ControlCustomVariableLengthCaptionRenderer()
		{
		}

		public ControlCustomVariableLengthCaptionRenderer(GetCaptionsDelegate captionGetter)
		{
			this.captionGetter = captionGetter;
		}

		public Control Control
		{
			get { return control; }
			set
			{
				if (control != value)
				{
					if (control != null)
					{
						control.VisibleChanged -= new EventHandler(Control_VisibleOrParentChanged);
						control.HandleCreated -= new EventHandler(Control_VisibleOrParentChanged);
						control.ParentChanged -= new EventHandler(Control_VisibleOrParentChanged);
					}
					control = value;
					Renderer = value as IVariableLengthCaptionRenderer;
				}
			}
		}
		Control control;

		public IVariableLengthCaptionRenderer Renderer
		{
			get { return renderer; }
			set
			{
				if (renderer != value)
				{
					renderer = value;
					if (value != null)
					{
						UpdateCaptionOnVisible();
					}
				}
			}
		}
		IVariableLengthCaptionRenderer renderer;

		public string[] Captions
		{
			get { return ((captions != null && captions.Length > 0) || captionGetter == null) ? captions : captionGetter(); }
			set
			{
				if (!ArrayUtil.ArrayEquals(captions, value) || (value != null && value.Length > 0 && string.IsNullOrEmpty(Control.Text)))
				{
					captions = value;
					UpdateCaptionOnVisible();
				}
			}
		}
		string[] captions = Array.Empty<string>();

		public bool IsCaptionOverridden
		{
			get { return Renderer != null && Renderer.IsCaptionOverridden; }
			set
			{
				if (Renderer != null)
				{
					Renderer.IsCaptionOverridden = value;
				}
			}
		}

		#region Implementation

		readonly GetCaptionsDelegate captionGetter;

		void UpdateCaptionOnVisible()
		{
			if (Control == null || IsReadyToRender)
			{
				Renderer.Captions = Captions;
			}
			else
			{
				Control.VisibleChanged -= new EventHandler(Control_VisibleOrParentChanged);
				Control.VisibleChanged += new EventHandler(Control_VisibleOrParentChanged);
				Control.HandleCreated -= new EventHandler(Control_VisibleOrParentChanged);
				Control.HandleCreated += new EventHandler(Control_VisibleOrParentChanged);
				Control.ParentChanged -= new EventHandler(Control_VisibleOrParentChanged);
				Control.ParentChanged += new EventHandler(Control_VisibleOrParentChanged);
			}
		}

		void Control_VisibleOrParentChanged(object sender, EventArgs e)
		{
			if (IsReadyToRender)
			{
				Control.VisibleChanged -= new EventHandler(Control_VisibleOrParentChanged);
				Control.HandleCreated -= new EventHandler(Control_VisibleOrParentChanged);
				Control.ParentChanged -= new EventHandler(Control_VisibleOrParentChanged);
				if (Renderer != null && (!Renderer.IsCaptionOverridden || Control.IsDesignMode()))
				{
					Renderer.Captions = Captions;
				}
			}
		}

		bool IsReadyToRender
		{
			get
			{
				return
					((Control.Created && Control.Visible) || Control is TabPage) &&
					!(Control is TabPage && Control.Parent == null);
			}
		}

		#endregion
	}
}
