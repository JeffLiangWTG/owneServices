
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.BE.GUI
{
	partial class ImportInvoiceLineUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 350, true);

			// 
			// StatisticalValueLocalCurrencyControl
			// 
			this.StatisticalValueLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("921566BE-C193-4022-B232-ED3835AF4A64", englishCaption: "[UCC 8/6] Stat.", englishFullDescription: "[UCC 8/6] Stat. Value");
		}

		#endregion
	}
}
