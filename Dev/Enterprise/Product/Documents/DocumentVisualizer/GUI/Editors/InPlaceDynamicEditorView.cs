using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class InPlaceDynamicEditorView : IEditorView
	{
		public InPlaceDynamicEditorView(IDynamicContentLayoutElement content, IEditorPresenter presenter, Control control, IMacroBusinessObjectProperty property, float scale)
		{
			Argument.NotNull(content, nameof(content));
			Argument.NotNull(presenter, nameof(presenter));
			Argument.NotNull(control, nameof(control));
			Argument.NotNull(property, nameof(property));
			if (scale <= 0f)
			{
				throw new ArgumentOutOfRangeException(nameof(scale));
			}

			this.content = content;
			this.presenter = presenter;
			this.propertyControl = control;
			this.property = property;
			this.scale = scale;

			presenter.Init(this);
		}

		readonly IDynamicContentLayoutElement content;
		readonly IEditorPresenter presenter;
		readonly Control propertyControl;
		readonly IMacroBusinessObjectProperty property;
		readonly float scale;

		public IDynamicContentLayoutElement Content
		{
			get { return content; }
		}

		public object Control
		{
			get { return control ?? (control = GetControl()); }
		}

		InPlaceDynamicContentControl control;

		public bool WaitForUserInput
		{
			get { return true; }
		}

		public void BeginEdit()
		{
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
				control.CommitBindings();
				control.Remove();
			}
		}

		public bool IsPopup
		{
			get { return false; }
		}

		#region Implementation

		InPlaceDynamicContentControl GetControl()
		{
			var editor = new InPlaceDynamicContentControl(presenter);
			propertyControl.SuspendLayout();
			editor.SuspendLayout();

			propertyControl.Dock = DockStyle.Fill;
			propertyControl.Font = new Font(propertyControl.Font.Name, propertyControl.Font.Size * scale);

			editor.Controls.Add(propertyControl);

			var bizObj = property.CreateBusinessObject();

			var unscaleLocationX = ControlDpiScalingHelper.UnscaleFromCurrentDpiX((int)Content.Boundaries.Location.X);
			var unscalesLocationY = ControlDpiScalingHelper.UnscaleFromCurrentDpiY((int)Content.Boundaries.Location.Y);
			var unscaleSizeWidth = ControlDpiScalingHelper.UnscaleFromCurrentDpiX((int)Content.Boundaries.Size.Width);
			var unscalesSizeHeight = ControlDpiScalingHelper.UnscaleFromCurrentDpiY((int)Content.Boundaries.Size.Height);

			var location = ControlDpiScalingHelper.NewScaledPoint(unscaleLocationX, unscalesLocationY);
			var size = ControlDpiScalingHelper.NewScaledSize(unscaleSizeWidth, unscalesSizeHeight);

			editor.Location = location;
			editor.Size = size;

			editor.SetDataBinding(bizObj, "");

			bizObj.Validation.ValidateAll();

			propertyControl.ResumeLayout(false);
			editor.ResumeLayout();
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
