using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class ImportDirectoryWithoutCoverSheetForm : ImportDirectoryForm
	{
		ZArchitecture.GUI.ZDropEdit DocTypeDropDownEdit;
		ZArchitecture.GUI.ZDropEdit JobTypeDropDownEdit;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.JobTypeDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DocTypeDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ACancelButton
			// 
			this.ACancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(338, 161, true);
			this.ACancelButton.TabIndex = 15;
			// 
			// OKButton
			// 
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 161, true);
			this.OKButton.TabIndex = 14;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 198, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 23, true);
			// 
			// JobTypeDropDownEdit
			// 
			this.JobTypeDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JobTypeDropDownEdit, "JobType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentScanning.Business.FileImporter)(null)).JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.FileImporter)(null)).JobTypeList)));
			this.JobTypeDropDownEdit.BindToList = "JobTypeList";
			this.JobTypeDropDownEdit.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportDirectoryWithoutCoverSheetForm|7A3BA0D1-116B-41F5-B101-FF108005B3B4", "Job Type");
			this.JobTypeDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 138, true);
			this.JobTypeDropDownEdit.MaxItemsToShowInDropDown = 20;
			this.JobTypeDropDownEdit.Name = "JobTypeDropDownEdit";
			this.JobTypeDropDownEdit.ShowDescriptionBox = false;
			this.JobTypeDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.JobTypeDropDownEdit.TabIndex = 12;
			// 
			// DocTypeDropDownEdit
			// 
			this.DocTypeDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocTypeDropDownEdit, "DocType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentScanning.Business.FileImporter)(null)).DocType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.FileImporter)(null)).DocTypeList)));
			this.DocTypeDropDownEdit.BindToList = "DocTypeList";
			this.DocTypeDropDownEdit.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ImportDirectoryWithoutCoverSheetForm|0427012B-C61F-4E76-B58A-756AC2CDFF23", "Doc Type");
			this.DocTypeDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 164, true);
			this.DocTypeDropDownEdit.MaxItemsToShowInDropDown = 20;
			this.DocTypeDropDownEdit.Name = "DocTypeDropDownEdit";
			this.DocTypeDropDownEdit.ShowDescriptionBox = false;
			this.DocTypeDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.DocTypeDropDownEdit.TabIndex = 13;
			// 
			// ImportDirectoryWithoutCoverSheetForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 221, true);
			this.Controls.Add(this.DocTypeDropDownEdit);
			this.Controls.Add(this.JobTypeDropDownEdit);
			this.Name = "ImportDirectoryWithoutCoverSheetForm";
			this.Text = "ImportDirectoryWithoutCoverSheetForm";
			this.Controls.SetChildIndex(this.ACancelButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.JobTypeDropDownEdit, 0);
			this.Controls.SetChildIndex(this.DocTypeDropDownEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
