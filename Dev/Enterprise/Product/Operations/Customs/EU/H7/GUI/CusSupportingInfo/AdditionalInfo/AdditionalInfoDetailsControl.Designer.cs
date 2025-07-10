using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.EU.H7.Business;

namespace Enterprise.Customs.EU.H7.GUI
{
	partial class AdditionalInfoDetailsControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AddInfoTypeCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AddInfoDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AddInfoTypeCodeDropEdit.SuspendLayout();
			this.AddInfoDescriptionTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AsycudaBill);
			// 
			// AddInfoTypeCodeDropEdit
			// 
			this.AddInfoTypeCodeDropEdit.AllowDrop = true;
			this.AddInfoTypeCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AddInfoTypeCodeDropEdit, "AdditionalInfos.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.H7.Business.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).AdditionalInfos)).SyncRoot)).CSI_Code)));
			this.AddInfoTypeCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 14, true);
			this.AddInfoTypeCodeDropEdit.Name = "AddInfoTypeCodeDropEdit";
			this.AddInfoTypeCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 17, true);
			this.AddInfoTypeCodeDropEdit.TabIndex = 2;
			// 
			// AddInfoDescriptionTextBox
			// 
			this.AddInfoDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AddInfoDescriptionTextBox, "AdditionalInfos.CSI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).AdditionalInfos)).SyncRoot)).CSI_Description)));
			this.AddInfoDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 38, true);
			this.AddInfoDescriptionTextBox.Multiline = true;
			this.AddInfoDescriptionTextBox.Name = "AddInfoDescriptionTextBox";
			this.AddInfoDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 42, true);
			this.AddInfoDescriptionTextBox.TabIndex = 3;
			// 
			// 
			// AdditionalInfoUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AddInfoTypeCodeDropEdit);
			this.Controls.Add(this.AddInfoDescriptionTextBox);
			this.Name = "AdditionalInfoDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(710, 490, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AddInfoTypeCodeDropEdit.ResumeLayout(true);
			this.AddInfoTypeCodeDropEdit.PerformLayout();
			this.AddInfoDescriptionTextBox.ResumeLayout(true);
			this.AddInfoDescriptionTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.ZTextBox AddInfoDescriptionTextBox;
		public Enterprise.ZArchitecture.GUI.ZDropEdit AddInfoTypeCodeDropEdit;
	}
}

