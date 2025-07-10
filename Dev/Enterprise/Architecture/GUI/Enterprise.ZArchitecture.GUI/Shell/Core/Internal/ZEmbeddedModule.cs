using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules
{
	public abstract class ZEmbeddedModule : ZModule
	{
		public Control EmbeddedControl
		{
			get
			{
				if (embeddedControl == null)
				{
					embeddedControl = GetNewEmbeddedControl();
					embeddedControl.Dock = DockStyle.Fill;
					var handler = GetDragAndDropHandler();
					if (handler != null)
					{
						new DragAndDropFileSupporter(embeddedControl, handler);
					}
				}
				return embeddedControl;
			}
		}
		Control embeddedControl;

		public bool IsEmbeddedControlConstructed => embeddedControl != null;

		protected abstract Control GetNewEmbeddedControl();

		protected virtual DragAndDropSingleFileHandler GetDragAndDropHandler()
		{
			return null;
		}

		public virtual ToolBarButton[] ToolBarButtons
		{
			get { return null; }
		}

		public virtual MenuItem[] FormActionMenu
		{
			get { return null; }
		}

		public override IZForm GetForm()
		{
			var form = new ZChildForm();
			form.MinimumSize = EmbeddedControl.Size;
			form.Controls.Add(EmbeddedControl);
			form.Text = Description;
			return form;
		}

		public override IZForm ShowPopup()
		{
			var form = GetForm();
			form.Show();
			return form;
		}

		#region Dispose

		protected override void Dispose(bool isDisposing)
		{
			try
			{
				if (isDisposing)
				{
					if (embeddedControl != null)
					{
						embeddedControl.Dispose();
					}
				}
			}
			finally
			{
				base.Dispose(isDisposing);
			}
		}

		#endregion
	}
}
