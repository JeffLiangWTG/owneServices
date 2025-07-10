namespace Enterprise.Registry.GUI
{
	partial class EBookingAirCarrierConfigurationControl : RegistryBusinessObjectTemplateZUserControl
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
		void InitializeComponent()
		{
			this.LastResponseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LastUpdatedTimeTimeEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LastUpdatedTimeTimeEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.EBookingCarrierConfiguration);
			// 
			// LastResponseTextBox
			// 
			this.LastResponseTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LastResponseTextBox, "LastResponse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.EBookingCarrierConfiguration)(null)).LastResponse)));
			this.LastResponseTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("EBookingAirCarrierConfigurationControl|86a9c125-1567-4d8e-bb7a-c5888153cb9b", "Last Response");
			this.LastResponseTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.LastResponseTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LastResponseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 94, true);
			this.LastResponseTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.LastResponseTextBox.Multiline = true;
			this.LastResponseTextBox.Name = "LastResponseTextBox";
			this.LastResponseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 154, true);
			this.LastResponseTextBox.TabIndex = 3;
			this.LastResponseTextBox.MaxLength = int.MaxValue;
			this.LastResponseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			// 
			// LastUpdatedTimeTimeEdit
			// 
			this.LastUpdatedTimeTimeEdit.AllowDrop = true;
			this.LastUpdatedTimeTimeEdit.AutoCompleteMonthThreshold = 1;
			this.LastUpdatedTimeTimeEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LastUpdatedTimeTimeEdit, "LastUpdatedTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Registry.Business.EBookingCarrierConfiguration)(null)).LastUpdatedTime)));
			this.LastUpdatedTimeTimeEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("EBookingAirCarrierConfigurationControl|6fcef5a7-2a6c-4994-8109-c3e026c30a40", "Last Update Time (UTC)");
			this.LastUpdatedTimeTimeEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.LastUpdatedTimeTimeEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LastUpdatedTimeTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 52, true);
			this.LastUpdatedTimeTimeEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.LastUpdatedTimeTimeEdit.Name = "LastUpdatedTimeEdit";
			this.LastUpdatedTimeTimeEdit.TabIndex = 1;
			// 
			// EBookingAirCarrierConfigurationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LastUpdatedTimeTimeEdit);
			this.Controls.Add(this.LastResponseTextBox);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.Name = "EBookingAirCarrierConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 249, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LastUpdatedTimeTimeEdit.ResumeLayout(true);
			this.LastUpdatedTimeTimeEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox LastResponseTextBox;
		private ZArchitecture.GUI.ZDateEdit LastUpdatedTimeTimeEdit;
	}
}
