using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class PopupDynamicEditorView : IEditorView
	{
		public PopupDynamicEditorView(IDynamicContentLayoutElement content, IEditorPresenter presenter, Control[] controls, IMacroBusinessObjectProperty[] properties, float scale)
		{
			Argument.NotNull(content, nameof(content));
			Argument.NotNull(presenter, nameof(presenter));
			Argument.NotNull(controls, nameof(controls));
			Argument.NotNull(properties, nameof(properties));
			if (scale <= 0f)
			{
				throw new ArgumentOutOfRangeException(nameof(scale));
			}

			this.content = content;
			this.presenter = presenter;
			this.controls = controls;
			this.properties = properties;
			this.scale = scale;

			presenter.Init(this);
		}

		readonly IDynamicContentLayoutElement content;
		readonly IEditorPresenter presenter;
		readonly Control[] controls;
		readonly IMacroBusinessObjectProperty[] properties;
		readonly float scale;

		public IDynamicContentLayoutElement Content
		{
			get { return content; }
		}

		public object Control
		{
			get { return control ?? (control = GetControl()); }
		}

		PopupDynamicContentControl control;

		public bool WaitForUserInput
		{
			get { return true; }
		}

		public void BeginEdit()
		{
			var topMostControl = controls
				.Where(ctrl => ctrl.Visible)
				.OrderBy(ctrl => ctrl.Location.Y)
				.FirstOrDefault();

			if (topMostControl != null)
			{
				topMostControl.Focus();
			}
		}

		public void EndEdit()
		{
			if (control != null)
			{
				control.CommitBindings();
				control.Remove();
			}
		}

		public void CancelEdit()
		{
			if (control != null)
			{
				control.Remove();
			}
		}

		public bool IsPopup
		{
			get { return true; }
		}

		#region Implementation

		PopupDynamicContentControl GetControl()
		{
			var editor = new PopupDynamicContentControl();

			editor.OnTabOffControl += (s, e) => presenter.MoveToNextEditor();

			editor.SuspendLayout();

			editor.AddContentControls(controls);

			var unscaleBoundariesX = ControlDpiScalingHelper.UnscaleFromCurrentDpiX((int)Content.Boundaries.Location.X);
			var unscalesBoundariesY = ControlDpiScalingHelper.UnscaleFromCurrentDpiY((int)Content.Boundaries.Location.Y);

			var location = ControlDpiScalingHelper.NewScaledPoint(unscaleBoundariesX, unscalesBoundariesY);

			editor.Location = location;

			editor.SetScale(scale);

			var bizObj = properties.CreateBusinessObject();

			editor.OnClose += (s, e) => presenter.CommitChanges();

			editor.SetDataBinding(bizObj, "");

			editor.ResumeLayout();

			bizObj.Validation.ValidateAll();

			return editor;
		}

		#endregion

		#region IDisposable members

		public void Dispose()
		{
			if (control != null && !control.IsDisposed)
			{
				if (control.Parent != null)
				{
					control.Parent.Controls.Remove(control);
				}

				control.Dispose();
				control = null;
			}
		}

		#endregion
	}
}
