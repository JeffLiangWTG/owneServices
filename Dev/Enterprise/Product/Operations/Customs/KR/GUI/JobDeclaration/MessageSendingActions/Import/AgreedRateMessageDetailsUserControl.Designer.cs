namespace Enterprise.Customs.KR.GUI
{
	partial class AgreedRateMessageDetailsUserControl
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
      Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      this.DetailsGrid = new Enterprise.ZArchitecture.ZGrid();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.DetailsGrid)).BeginInit();
      this.DetailsGrid.SuspendLayout();
      this.SuspendLayout();
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.AgreedRateMessageSendingObject);
      // 
      // DetailsGrid
      // 
      this.DetailsGrid.AllowNavigation = false;
      this.BindingSource.SetBindingMember(this.DetailsGrid, "Details");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.AgreedRateMessageSendingObject)(null)).Details)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.AgreedRateMessageSendingObject)(null)).Details)).SyncRoot)).EntryLineNo)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.AgreedRateMessageSendingObject)(null)).Details)).SyncRoot)).FormattedHSCode)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.AgreedRateMessageSendingObject)(null)).Details)).SyncRoot)).HSDescription)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.AgreedRateMessageSendingObject)(null)).Details)).SyncRoot)).ModelName)));
      this.DetailsGrid.CaptionVisible = false;
      zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
      zCalcEditColumnStyleInfo1.ColumnName = "EntryLineNo";
      zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
      zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
      zTextBoxColumnStyleInfo1.ColumnName = "FormattedHSCode";
      zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
      zTextBoxColumnStyleInfo2.ColumnName = "HSDescription";
      zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
      zTextBoxColumnStyleInfo3.ColumnName = "ModelName";
      zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
      this.DetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
      this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
      this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
      this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
      this.DetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
      this.DetailsGrid.GridId = "f5009a6e-2818-436f-a071-7495c44497b4";
      this.DetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.DetailsGrid.LayoutKey = "DetailsGrid";
      this.DetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.DetailsGrid.Name = "DetailsGrid";
      this.DetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 129, true);
      this.DetailsGrid.TabIndex = 0;
      // 
      // AgreedRateMessageDetailsUserControl
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.CaptionRenderingEnabled = true;
      this.Controls.Add(this.DetailsGrid);
      this.Name = "AgreedRateMessageDetailsUserControl";
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 129, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.DetailsGrid)).EndInit();
      this.DetailsGrid.ResumeLayout(false);
      this.DetailsGrid.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZGrid DetailsGrid;
	}
}
