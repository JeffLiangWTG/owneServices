namespace Enterprise.Customs.ES.GUI
{
	public partial class ImportSupportingDocumentReferenceNumberUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReferenceNumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReferenceNumberCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.SupportingDocument);
			// 
			// ReferenceNumberTextBox
			// 
			this.ReferenceNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.SupportingDocument)(null)).CSI_ReferenceNumber)));
			this.ReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReferenceNumberTextBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 0, true);
			this.ReferenceNumberTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 20, true);
			this.ReferenceNumberTextBox.TabIndex = 1;
			// 
			// ReferenceNumberCodeFindBox
			// 
			this.ReferenceNumberCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReferenceNumberCodeFindBox, "CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.SupportingDocument)(null)).CSI_ReferenceNumber)));
			this.ReferenceNumberCodeFindBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ReferenceNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReferenceNumberCodeFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ReferenceNumberCodeFindBox.Name = "ReferenceNumberCodeFindBox";
			this.ReferenceNumberCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ReferenceNumberCodeFindBox.ParentType = null;
			this.ReferenceNumberCodeFindBox.PreBoundMaxLength = 15;
			this.ReferenceNumberCodeFindBox.ShouldResize = false;
			this.ReferenceNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 20, true);
			this.ReferenceNumberCodeFindBox.TabIndex = 0;
			this.ReferenceNumberCodeFindBox.Visible = false;
			// 
			// ImportSupportingDocumentReferenceNumberUserControl
			// 
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReferenceNumberTextBox);
			this.Controls.Add(this.ReferenceNumberCodeFindBox);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.Name = "ImportSupportingDocumentReferenceNumberUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReferenceNumberCodeFindBox.ResumeLayout(true);
			this.ReferenceNumberCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox ReferenceNumberTextBox;
		internal ZArchitecture.GUI.ZCodeFindBox ReferenceNumberCodeFindBox;
	}
}
