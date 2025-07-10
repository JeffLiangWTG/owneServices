using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public partial class CustomisedVisualLayoutsUserControl : ZUserControl
	{
		public CustomisedVisualLayoutsUserControl()
		{
			InitializeComponent();
		}

		BMControlCustomisation SelectedCustomisation
		{
			get { return LayoutsGrid.ListManager != null && LayoutsGrid.ListManager.Position > -1 && LayoutsGrid.ListManager.Count > 0 ? (BMControlCustomisation)LayoutsGrid.ListManager.GetCurrent() : null; }
		}

		#region Event Handlers

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (LayoutsGrid.ListManager != null)
			{
				LayoutsGrid.ListManager.PositionChanged += LayoutsGrid_PositionChanged;
			}

			UpdatePreview();
		}

		void LayoutsGrid_DoubleClick(object sender, MouseEventArgs e)
		{
			var customisation = SelectedCustomisation;
			if (customisation != null)
			{
				GridEntityFormOpener.OpenForm(LayoutsGrid, e, () => customisation, ControllerIDs.BMControlCustomisation);
			}
		}

		void LayoutsGrid_PositionChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		#endregion

		#region Preview

		void UpdatePreview()
		{
			PreviewControlDestination.Controls.RemoveAndDisposeAll();

			var customisation = SelectedCustomisation;
			if (customisation != null)
			{
				var control = CustomisedControlRenderer.RenderForPreview(customisation);
				if (control != null)
				{
					Control controlPreview = null;
#if !WINZOR
					var bitmap = new Bitmap(control.Width, control.Height, PixelFormat.Format32bppPArgb);
					control.DrawToBitmapFixed(bitmap, control.ClientRectangle);

					controlPreview = new OptimisticPictureBox(shouldDisposeImageOnControlDispose: true)
					{
						Size = control.Size,
						Image = bitmap,
					};
					control.Dispose();
#else
					// Make the element is never the target of pointer events.
					control.ExtraStyleString = (ZArchitecture.Core.NoResString)"pointer-events:none;";
					controlPreview = control;
#endif
					PreviewControlDestination.Controls.Add(controlPreview);
				}
			}
		}

		Control PreviewControlDestination
		{
			get { return LayoutUsagePreviewSplitContainer.Panel2; }
		}

#if DEBUG
		public Control PreviewControl_Exposed
		{
			get { return PreviewControlDestination.Controls.OfType<Control>().SingleOrDefault(); }
		}
#endif

		#endregion
	}
}
