using System;
using System.Windows.Forms;

using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class RelatedJobsUserControl
	{
		#region Auto

		internal RelatedJobsGrid RelatedJobsGrid;
		internal ZButton EditJobButton;

		void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			this.EditJobButton = new ZButton();
			this.RelatedJobsGrid = new RelatedJobsGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RelatedJobsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(RelatedJobCollection);
			// 
			// EditJobButton
			// 
			this.EditJobButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.EditJobButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("RelatedJobsUserControl|e56f5c14-1748-4cb1-af8e-4e93724fab84", "View Job", "View the currently selected job.");
			this.EditJobButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(425, 312, true);
			this.EditJobButton.Name = "EditJobButton";
			this.EditJobButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.EditJobButton.TabIndex = 1;
			this.EditJobButton.UseVisualStyleBackColor = true;
			this.EditJobButton.Click += new EventHandler(this.EditJobButton_Click);
			// 
			// RelatedJobsGrid
			// 
			this.RelatedJobsGrid.AllowNavigation = false;
			this.RelatedJobsGrid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.RelatedJobsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((IRelatedJob)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IRelatedJob)(null)).JobNumber);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IRelatedJob)(null)).JobDescription);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IRelatedJob)(null)).JobStatus);
			this.RelatedJobsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("RelatedJobsUserControl|8f9834ca-063d-43dd-801a-fb4ef6365831", "Job No.", "Job Number");
			zTextBoxColumnStyleInfo1.ColumnName = "JobNumber";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("RelatedJobsUserControl|8591c147-1120-45b2-83e5-eb8c124c6b7d", "Desc.", "Description", "");
			zTextBoxColumnStyleInfo2.ColumnName = "JobDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("RelatedJobsUserControl|686fa479-09e4-4cda-bbff-6d7a979d8ae8", "Status");
			zTextBoxColumnStyleInfo3.ColumnName = "JobStatus";
			this.RelatedJobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RelatedJobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RelatedJobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RelatedJobsGrid.GridId = "5439f38f-1a27-469c-8b12-9e5240439f07";
			this.RelatedJobsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RelatedJobsGrid.IsWholeRowSelectedOnClick = true;
			this.RelatedJobsGrid.LayoutKey = "relatedJobsGrid1";
			this.RelatedJobsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.RelatedJobsGrid.Name = "RelatedJobsGrid";
			this.RelatedJobsGrid.ReadOnly = true;
			this.RelatedJobsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 303, true);
			this.RelatedJobsGrid.TabIndex = 0;
			// 
			// RelatedJobsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EditJobButton);
			this.Controls.Add(this.RelatedJobsGrid);
			this.Name = "RelatedJobsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 340, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RelatedJobsGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
