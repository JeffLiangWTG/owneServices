using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.GUI.JSInterop;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public sealed partial class MagnifyForm : ZChildForm
	{
		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			GetJSInterop<IMagnifyFormJSInterop>()?.PreloadInterop();
		}

		void InitializeFormForWinzor()
		{
			LoadImage();
			AddEvents();
			HideOCRButtons(); //OCR functionality is not currently implemented.
		}

		void LoadImage()
		{
			//The original version will crop/scale the image based on zooming/movement.
			//Here, we load the entire image.
			PictureBox.Image = (Image)MagnifyManager.LatestBitmap?.Clone();

			var zoom = float.Parse(MagnifyManager.Zoom) / 100;

			if (zoom > 0 && PictureBox.Image != null) //0 = 'fit to width'
			{
				var width = (int)(PictureBox.Image.Width * zoom);
				var height = (int)(PictureBox.Image.Height * zoom);
#pragma warning disable CW1017 // Non DPI-aware code has been detected
				PictureBox.Size = new Size(width, height);
#pragma warning restore CW1017 // Non DPI-aware code has been detected
			}
		}

		#region Movement
		void AddEvents()
		{
			const int buttonMoveDistance = 23;

			RegisterAfterRenderAction(async () => {
				await (GetJSInterop<IMagnifyFormJSInterop>()?.EnableDraggablePositionAsync(PictureBox.ElementReference) ?? Task.CompletedTask);
				await (GetJSInterop<IMagnifyFormJSInterop>()?.EnableMovementByToolbarButtonsAsync(PictureBox.ElementReference, MagnifyToolBar.ElementReference, buttonMoveDistance) ?? Task.CompletedTask);
				await (GetJSInterop<IMagnifyFormJSInterop>()?.EnableZoomByDropEditAsync(PictureBox.ElementReference, ZoomDropEdit.ElementReference) ?? Task.CompletedTask);

				if (MagnifyManager.Zoom == "0")
				{
					await (GetJSInterop<IMagnifyFormJSInterop>()?.FillToWidthAsync(PictureBox.ElementReference) ?? Task.CompletedTask);
				}
			});
		}
		#endregion

		#region OCR
		void HideOCRButtons()
		{
			OCRButton.Enabled = false;
			OCRButton.Visible = false;
			SelectTextButton.Enabled = false;
			SelectTextButton.Visible = false;
			ClearSelectionButton.Enabled = false;
			ClearSelectionButton.Visible = false;
			Separator2Button.Enabled = false;
			Separator2Button.Visible = false;
		}
		#endregion
		#region Stubs
		void MagnifyToolBar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
		{
		}

		void ZoomDropEdit_KeyDown(object sender, KeyEventArgs e)
		{
		}

		void PictureBox_MouseMove(object sender, MouseEventArgs e)
		{
		}

		void PictureBox_Paint(object sender, PaintEventArgs e)
		{
		}

		void PictureBox_MouseDown(object sender, MouseEventArgs e)
		{
		}

		void PictureBox_MouseUp(object sender, MouseEventArgs e)
		{
		}

		public void UpdateLatestImage(float extraMovementProportionX, float extraMovementProportionY, bool updateImmediately)
		{
			if (MagnifyManager is null)
			{
				return;
			}
			else if (ImagePanel.ClientSize.Width <= 0 && ImagePanel.ClientSize.Height <= 0)
			{
				return;
			}
			else if (PictureBox.Image is null || PictureBox.Image.Width <= 0 || PictureBox.Image.Height <= 0)
			{
				return;
			}

			UpdateTitle();
		}
		#endregion
	}
}
