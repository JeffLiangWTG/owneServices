namespace Enterprise.Customs.IT.NCTS.GUI
{
	partial class DeclarationDetailsUserControl
	{

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ReleaseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReleaseDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.WriteOffDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ControlChannelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReleaseDateEdit.SuspendLayout();
			this.WriteOffDateEdit.SuspendLayout();
			this.ControlChannelDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.NCTS.Business.NctsDepartureMovementHeader);
			// 
			// ReleaseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReleaseCodeTextBox, "Header.ReleaseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureMovementHeader)(null)).Header.ReleaseCode)));
			this.ReleaseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 20, true);
			this.ReleaseCodeTextBox.Name = "ReleaseCodeTextBox";
			this.ReleaseCodeTextBox.ReadOnly = true;
			this.ReleaseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 15, true);
			this.ReleaseCodeTextBox.TabIndex = 0;
			this.ReleaseCodeTextBox.TabStop = false;
			// 
			// ReleaseDateEdit
			// 
			this.ReleaseDateEdit.AllowDrop = true;
			this.ReleaseDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ReleaseDateEdit, "Header.CustomsReleaseIssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureMovementHeader)(null)).Header.CustomsReleaseIssueDate)));
			this.ReleaseDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReleaseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 140, true);
			this.ReleaseDateEdit.Name = "ReleaseDateEdit";
			this.ReleaseDateEdit.TabIndex = 1;
			// 
			// WriteOffDateEdit
			// 
			this.WriteOffDateEdit.AllowDrop = true;
			this.WriteOffDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.WriteOffDateEdit, "Header.CustomsWriteOffDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureMovementHeader)(null)).Header.CustomsWriteOffDate)));
			this.WriteOffDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.WriteOffDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 170, true);
			this.WriteOffDateEdit.Name = "WriteOffDateEdit";
			this.WriteOffDateEdit.TabIndex = 2;
			// 
			// ControlChannelDropEdit
			//
			this.ControlChannelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ControlChannelDropEdit, "BM_ControlChannel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureMovementHeader)(null)).BM_ControlChannel)));
			this.ControlChannelDropEdit.ReadOnly = true;
			this.ControlChannelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 200, true);
			this.ControlChannelDropEdit.Name = "ControlChannelDropEdit";
			this.ControlChannelDropEdit.TabIndex = 3;
			// 
			// DeclarationDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReleaseCodeTextBox);
			this.Controls.Add(this.ReleaseDateEdit);
			this.Controls.Add(this.WriteOffDateEdit);
			this.Controls.Add(this.ControlChannelDropEdit);
			this.Name = "DeclarationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 299, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReleaseDateEdit.ResumeLayout(true);
			this.ReleaseDateEdit.PerformLayout();
			this.WriteOffDateEdit.ResumeLayout(true);
			this.WriteOffDateEdit.PerformLayout();
			this.ControlChannelDropEdit.ResumeLayout(true);
			this.ControlChannelDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox ReleaseCodeTextBox;
		internal ZArchitecture.GUI.ZDateEdit ReleaseDateEdit;
		internal ZArchitecture.GUI.ZDateEdit WriteOffDateEdit;
		internal ZArchitecture.GUI.ZDropEdit ControlChannelDropEdit;
	}
}
