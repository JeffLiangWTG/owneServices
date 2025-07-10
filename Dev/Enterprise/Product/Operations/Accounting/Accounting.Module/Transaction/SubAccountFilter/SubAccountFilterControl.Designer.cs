namespace Enterprise.Accounting.Module
{
	partial class SubAccountFilterControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SubAccountTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SubAccountGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Module.SubAccountFilter);
			// 
			// SubAccountTypeDropEdit
			// 
			this.SubAccountTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SubAccountTypeDropEdit, "SubAccountType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Module.SubAccountFilter)(null)).SubAccountType)));
			this.SubAccountTypeDropEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("0bc90706-0628-4a83-8004-41f72d2b2b58", "Type");
			this.SubAccountTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 2, true);
			this.SubAccountTypeDropEdit.Name = "SubAccountTypeDropEdit";
			this.SubAccountTypeDropEdit.PreBoundMaxLength = 8;
			this.SubAccountTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 20, true);
			this.SubAccountTypeDropEdit.TabIndex = 2;
			// 
			// SubAccountGuidFindBox
			// 
			this.SubAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SubAccountGuidFindBox, "SubAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Module.SubAccountFilter)(null)).SubAccount)));
			this.SubAccountGuidFindBox.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("63f21a67-da94-4b1d-ad7e-35c373913975", "Sub Account");
			this.SubAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 28, true);
			this.SubAccountGuidFindBox.Name = "SubAccountGuidFindBox";
			this.SubAccountGuidFindBox.PopupCaption = null;
			this.SubAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.SubAccountGuidFindBox.TabIndex = 3;
			// 
			// SubAccountFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SubAccountGuidFindBox);
			this.Controls.Add(this.SubAccountTypeDropEdit);
			this.Name = "SubAccountFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 52, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		void SubAccountTypeDropEdit_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			SubAccountGuidFindBox.List = null;
		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit SubAccountTypeDropEdit;
		private ZArchitecture.GUI.ZGuidFindBox SubAccountGuidFindBox;

	}
}
