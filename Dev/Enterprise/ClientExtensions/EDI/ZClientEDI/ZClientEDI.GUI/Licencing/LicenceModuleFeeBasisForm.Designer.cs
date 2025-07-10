namespace Enterprise.Client.EDI.Licencing.GUI
{
	partial class LicenceModuleFeeBasisForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEdit2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 182, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Licencing.Business.LicenceModuleFeeBasis);
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "ModuleCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Licencing.Business.LicenceModuleFeeBasis)(null)).ModuleCode)));
			this.zDropEdit1.CaptionResourceString = null;
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 40, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.zDropEdit1.TabIndex = 1;
			// 
			// zDropEdit2
			// 
			this.zDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit2, "FeeBasis");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Licencing.Business.LicenceModuleFeeBasis)(null)).FeeBasis)));
			this.zDropEdit2.CaptionResourceString = null;
			this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 77, true);
			this.zDropEdit2.Name = "zDropEdit2";
			this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.zDropEdit2.TabIndex = 2;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = null;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 40, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.zLabel1.TabIndex = 3;
			this.zLabel1.Text = "Module";
			this.zLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel2
			// 
			this.zLabel2.CaptionResourceString = null;
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 77, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.zLabel2.TabIndex = 4;
			this.zLabel2.Text = "Fee Basis";
			this.zLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// okButton
			// 
			this.okButton.CaptionResourceString = null;
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 128, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 5;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = null;
			this.cancelButton.CausesValidation = false; // SuppressCodeSmell Reason = cancel buttons shouldn't cause validation
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 128, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 6;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// LicenceModuleFeeBasisForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 206, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zDropEdit1);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.zDropEdit2);
			this.DataSourceType = typeof(Enterprise.Client.EDI.Licencing.Business.LicenceModuleFeeBasis);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "LicenceModuleFeeBasisForm";
			this.Text = "Licence Module Fee Basis";
			this.Controls.SetChildIndex(this.zDropEdit2, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.zDropEdit1, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit zDropEdit1;
		private ZArchitecture.GUI.ZDropEdit zDropEdit2;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.GUI.ZButton okButton;
		private ZArchitecture.GUI.ZButton cancelButton;
	}
}
