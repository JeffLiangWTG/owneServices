using System;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GLAccountFormat
{
	public partial class GLFormatMatchingForm : KForm
	{
		public GLFormatMatchingForm(string oldFormat, string newFormat)
		{
			InitializeComponent();
			MiddleBox = new GLNumberBox();

			fOldFormat = oldFormat;
			fNewFormat = newFormat;
			ResetData();

			ControlDpiScalingHelper.SetLeft(ref MiddleBox, this.OldFormatMaskEdit.Left + ControlDpiScalingHelper.ScaleToCurrentDpiX(100), false);
			ControlDpiScalingHelper.SetTop(ref MiddleBox, this.OldFormatMaskEdit.Bottom + (this.NewFormatMaskEdit.Top - this.OldFormatMaskEdit.Bottom - MiddleBox.Height) / 2, false);
			MiddleBox.Parent = this;
			this.ClientSize = ControlDpiScalingHelper.NewScaledSize(this.NewFormatMaskEdit.Width + this.NewFormatMaskEdit.Left + ControlDpiScalingHelper.ScaleToCurrentDpiX(16),
				this.NewFormatMaskEdit.Top + this.NewFormatMaskEdit.Height + OKButton.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(24), false);
		}

		public string UpdatedMask = "";

		#region Implementation

		protected GLFormatMask OldFormatMaskEdit;
		protected GLFormatMask NewFormatMaskEdit;
		protected GLNumberBox MiddleBox;

		protected string fOldFormat;
		ZLabel label1;
		ZLabel label2;
		ZButton MyCancelButton;
		ZButton ResetButton;
		ZButton OKButton;

		protected string fNewFormat;

		protected void ResetData()
		{
			if (OldFormatMaskEdit != null)
			{
				Controls.Remove(OldFormatMaskEdit);
			}

			if (NewFormatMaskEdit != null)
			{
				Controls.Remove(NewFormatMaskEdit);
			}

			OldFormatMaskEdit = new GLFormatMask(fOldFormat, false);
			ControlDpiScalingHelper.SetTop(ref OldFormatMaskEdit, 20, true);
			ControlDpiScalingHelper.SetLeft(ref OldFormatMaskEdit, 100, true);
			OldFormatMaskEdit.Parent = this;
			OldFormatMaskEdit.BoxClicked += new BoxEventHandler(OldFormat_BoxClicked);

			NewFormatMaskEdit = new GLFormatMask(fNewFormat, true);
			ControlDpiScalingHelper.SetTop(ref NewFormatMaskEdit, 150, true);
			ControlDpiScalingHelper.SetLeft(ref NewFormatMaskEdit, 100, true);
			NewFormatMaskEdit.Parent = this;
			NewFormatMaskEdit.BoxClicked += new BoxEventHandler(NewFormat_BoxClicked);
			NewFormatMaskEdit.FullyEntered += new EventHandler(NewFormat_FullyEntered);

			MiddleBox.Caption = "";
			MiddleBox.Index = -1;
			IsMovingItem = false;
			this.Refresh();
		}

		#region System stuff

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		protected bool IsMovingItem;

		void ResetButton_Click(object sender, EventArgs e)
		{
			ResetData();
		}

		void Box_LabelClicked(object sender, EventArgs e)
		{
			(sender as GLNumberBox).Highlight();
		}

		void OldFormat_BoxClicked(GLNumberBox box)
		{
			if (!string.IsNullOrEmpty(box.Caption) && !IsMovingItem)
			{
				IsMovingItem = true;
				MiddleBox.Caption = box.Caption;
				MiddleBox.Index = box.Index;
				box.Caption = "";
				NewFormatMaskEdit.HighlightPlaceHolders(box.Index, OldFormatMaskEdit.NumberCount);
			}
		}

		void NewFormat_BoxClicked(GLNumberBox box)
		{
			if (IsMovingItem && box.IsHighlighted)
			{
				NewFormatMaskEdit.DeselectEverything();
				box.Caption = MiddleBox.Caption;
				box.Index = MiddleBox.Index;
				MiddleBox.Caption = "";
				MiddleBox.Index = -1;
				IsMovingItem = false;
				if (OldFormatMaskEdit.IsEmpty)
				{
					if (!NewFormatMaskEdit.IsFullyEntered)
					{
						NewFormatMaskEdit.RequestRemainingNumbers();
					}
					else
					{
						NewFormat_FullyEntered(null, null);
					}
				}
			}
		}

		void NewFormat_FullyEntered(object sender, EventArgs e)
		{
			OKButton.Enabled = true;
			ResetButton.Enabled = false;
			this.UpdatedMask = NewFormatMaskEdit.FormatMask;
		}
		#endregion

	}
}

