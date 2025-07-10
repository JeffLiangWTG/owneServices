using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class DataTransferRegistryControl : AutomaticProcessRegistryControl
	{
		protected ZTextBox DirectoryTextBox;
		protected ZButton DirectorySelectorButton;
		protected internal ZGuidFindBox zGuidFindBox1;

		void InitializeComponent()
		{
			this.DirectoryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DirectorySelectorButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zGuidFindBox1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// zTextBox1
			// 
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 55, true);
			// 
			// NextRunDateTimeEdit
			// 
			this.NextRunDateTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 91, true);
			// 
			// IntervalCalcEdit
			// 
			this.IntervalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 121, true);
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 121, true);
			// 
			// zLabel5
			// 
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 124, true);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.zGuidFindBox1);
			this.zGroupBox1.Controls.Add(this.DirectoryTextBox);
			this.zGroupBox1.Controls.Add(this.DirectorySelectorButton);
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 180, true);
			this.zGroupBox1.Controls.SetChildIndex(this.DirectorySelectorButton, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.zTextBox1, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.IntervalCalcEdit, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.DirectoryTextBox, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.zDropEdit1, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.zLabel5, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.zGuidFindBox1, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.NextRunDateTimeEdit, 0);
			// 
			// DirectoryTextBox
			// 
			this.BindingSource.SetBindingMember(this.DirectoryTextBox, "Directory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DataTransferRegistryBusinessObject)(null)).Directory)));
			this.DirectoryTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("DataTransferRegistryControl|786b8178-a6a0-4eea-b0c7-f216bcdf5979", "Directory");
			this.DirectoryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 17, true);
			this.DirectoryTextBox.Name = "DirectoryTextBox";
			this.DirectoryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.DirectoryTextBox.TabIndex = 1;
			// 
			// DirectorySelectorButton
			// 
			this.DirectorySelectorButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 14, true);
			this.DirectorySelectorButton.Name = "DirectorySelectorButton";
			this.DirectorySelectorButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 23, true);
			this.DirectorySelectorButton.TabIndex = 2;
			this.DirectorySelectorButton.Text = "...";
			this.DirectorySelectorButton.UseVisualStyleBackColor = true;
			this.DirectorySelectorButton.Click += new System.EventHandler(this.DirectorySelectorButton_Click);
			// 
			// zGuidFindBox1
			// 
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "GroupPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.DataTransferRegistryBusinessObject)(null)).GroupPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.DataTransferRegistryBusinessObject)(null)).GroupList)));
			this.zGuidFindBox1.BindToList = "GroupList";
			this.zGuidFindBox1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("DataTransferRegistryControl|a4dcb15e-5fd4-4673-aa78-016cde1cf5c9", "Notify Group");
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 149, true);
			this.zGuidFindBox1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbGroup;
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.zGuidFindBox1.TabIndex = 19;
			// 
			// DataTransferRegistryControl
			// 
			this.Name = "DataTransferRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 180, true);
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
