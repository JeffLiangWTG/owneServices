namespace Enterprise.Customs.EU.GUI
{
	partial class UNDGFlashpointUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.FlashPointDescLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FlashPointCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGDataItem);
			// 
			// FlashPointDescLabel
			// 
			this.FlashPointDescLabel.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("UNDGFlashpointUserControl|36e6a59f-4751-4198-81fb-a1f78570d830", "(Manufacturer Specified)");
			this.FlashPointDescLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FlashPointDescLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 0, true);
			this.FlashPointDescLabel.Name = "FlashPointDescLabel";
			this.FlashPointDescLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.FlashPointDescLabel.TabIndex = 1;
			// 
			// FlashPointCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FlashPointCalcEdit, "DI_DGFlashPoint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGDataItem)(null)).DI_DGFlashPoint)));
			this.FlashPointCalcEdit.CaptionResourceString = null;
			this.FlashPointCalcEdit.DecimalPlaces = 1;
			this.FlashPointCalcEdit.Decimals = 1;
			this.FlashPointCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FlashPointCalcEdit.Name = "FlashPointCalcEdit";
			this.FlashPointCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.FlashPointCalcEdit.TabIndex = 0;
			this.FlashPointCalcEdit.Text = "0.0";
			this.FlashPointCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UNDGFlashpointUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FlashPointDescLabel);
			this.Controls.Add(this.FlashPointCalcEdit);
			this.Name = "UNDGFlashpointUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZLabel FlashPointDescLabel;
		internal ZArchitecture.ZCalcEdit FlashPointCalcEdit;
	}
}
