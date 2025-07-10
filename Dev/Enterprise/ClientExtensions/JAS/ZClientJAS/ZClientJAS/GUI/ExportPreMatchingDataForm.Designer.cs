using System;
using Enterprise.Client.JAS.Business.Matching;
using Enterprise.Client.JAS.Module;
using Enterprise.ClientSharedComponents;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI
{
	public partial class ExportPreMatchingDataForm : ZChildForm
	{
		private Enterprise.ZArchitecture.ZLabel NettingCycleLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit NettingCycleDropEdit;
		private Enterprise.ZArchitecture.GUI.ZButton OKBoundButton;
		private Enterprise.ZArchitecture.GUI.ZButton CancelBoundButton;
		private ZFolderBrowserDialog DumpDirectoryBrowser;
		private JASDataExporterUserControl jasDataExporterUserControl1;

		new void InitializeComponent()
		{
			this.NettingCycleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NettingCycleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OKBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DumpDirectoryBrowser = new Enterprise.ZArchitecture.GUI.ZFolderBrowserDialog();
			this.jasDataExporterUserControl1 = new Enterprise.Client.JAS.GUI.JASDataExporterUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 156, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 4;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.JAS.Business.Matching.PreMatchedDataExporter);
			// 
			// NettingCycleLabel
			// 
			this.NettingCycleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 11, true);
			this.NettingCycleLabel.Name = "NettingCycleLabel";
			this.NettingCycleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.NettingCycleLabel.TabIndex = 1;
			this.NettingCycleLabel.Text = "Netting Cycle";
			// 
			// NettingCycleDropEdit
			// 
			this.BindingSource.SetBindingMember(this.NettingCycleDropEdit, "NettingCycle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.JAS.Business.Matching.PreMatchedDataExporter)(null)).NettingCycle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.JAS.Business.Matching.PreMatchedDataExporter)(null)).NettingCycleList)));
			this.NettingCycleDropEdit.BindToList = "NettingCycleList";
			this.NettingCycleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 11, true);
			this.NettingCycleDropEdit.Name = "NettingCycleDropEdit";
			this.NettingCycleDropEdit.ShowDescriptionBox = false;
			this.NettingCycleDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.NettingCycleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.NettingCycleDropEdit.TabIndex = 0;
			// 
			// OKBoundButton
			// 
			this.OKBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 127, true);
			this.OKBoundButton.Name = "OKBoundButton";
			this.OKBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKBoundButton.TabIndex = 2;
			this.OKBoundButton.Text = "OK";
			this.OKBoundButton.Click += new System.EventHandler(this.OKBoundButton_Click);
			// 
			// CancelBoundButton
			// 
			this.CancelBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 127, true);
			this.CancelBoundButton.Name = "CancelBoundButton";
			this.CancelBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelBoundButton.TabIndex = 3;
			this.CancelBoundButton.Text = "Cancel";
			this.CancelBoundButton.Click += new System.EventHandler(this.CancelBoundButton_Click);
			// 
			// DumpDirectoryBrowser
			// 
			this.DumpDirectoryBrowser.Description = "Please select folder where the A/R and A/P file will be placed";
			// 
			// jasDataExporterUserControl1
			// 
			this.BindingSource.SetBindingMember(this.jasDataExporterUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Client.JAS.Business.JASDataExporterBizO)(((Enterprise.Client.JAS.Business.Matching.PreMatchedDataExporter)(null)))));
			this.jasDataExporterUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 39, true);
			this.jasDataExporterUserControl1.Name = "jasDataExporterUserControl1";
			this.jasDataExporterUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 82, true);
			this.jasDataExporterUserControl1.TabIndex = 1;
			// 
			// ExportPreMatchingDataForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 180, true);
			this.Controls.Add(this.jasDataExporterUserControl1);
			this.Controls.Add(this.OKBoundButton);
			this.Controls.Add(this.NettingCycleDropEdit);
			this.Controls.Add(this.NettingCycleLabel);
			this.Controls.Add(this.CancelBoundButton);
			this.DataSourceAssemblyName = "ZClientJAS";
			this.DataSourceType = typeof(Enterprise.Client.JAS.Business.Matching.PreMatchedDataExporter);
			this.DataSourceTypeName = "Enterprise.Client.JAS.Business.Matching.PreMatchedDataExporter";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "ExportPreMatchingDataForm";
			this.Text = " ";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelBoundButton, 0);
			this.Controls.SetChildIndex(this.NettingCycleLabel, 0);
			this.Controls.SetChildIndex(this.NettingCycleDropEdit, 0);
			this.Controls.SetChildIndex(this.OKBoundButton, 0);
			this.Controls.SetChildIndex(this.jasDataExporterUserControl1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
