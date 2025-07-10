namespace Enterprise.BufferManagement.GUI
{
	partial class JobRelatedChildWorkflowsControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.AddButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RemoveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ChildWorkflowGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChildWorkflowGrid)).BeginInit();
			this.ChildWorkflowGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.RelatedParentChildWorkflowsViewModel);
			// 
			// AddButton
			// 
			this.AddButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AddButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("DC786A6B-1654-41A9-97BA-269C9A7F8591", "Add");
			this.AddButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(643, 230, true);
			this.AddButton.Name = "AddButton";
			this.AddButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AddButton.TabIndex = 9;
			this.AddButton.ToolTipCaption = null;
			this.AddButton.UseVisualStyleBackColor = true;
			this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
			// 
			// RemoveButton
			// 
			this.RemoveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RemoveButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("BABA6A31-F1CE-456C-92CF-3CD4E8BC2115", "Remove");
			this.RemoveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(722, 230, true);
			this.RemoveButton.Name = "RemoveButton";
			this.RemoveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.RemoveButton.TabIndex = 10;
			this.RemoveButton.ToolTipCaption = null;
			this.RemoveButton.UseVisualStyleBackColor = true;
			this.RemoveButton.Click += new System.EventHandler(this.RemoveButton_Click);
			// 
			// ChildWorkflowGrid
			// 
			this.ChildWorkflowGrid.AllowNavigation = false;
			this.ChildWorkflowGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ChildWorkflowGrid, "ExternalWorkflowLinks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.RelatedParentChildWorkflowsViewModel)(null)).ExternalWorkflowLinks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.RelatedParentChildWorkflowsViewModel)(null)).ExternalWorkflowLinks)).SyncRoot)).FP_FH_HeaderFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.RelatedParentChildWorkflowsViewModel)(null)).ExternalWorkflowLinks)).SyncRoot)).HeaderTo.ProcessHeaderType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.RelatedParentChildWorkflowsViewModel)(null)).ExternalWorkflowLinks)).SyncRoot)).HeaderTo.ProviderJobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.RelatedParentChildWorkflowsViewModel)(null)).ExternalWorkflowLinks)).SyncRoot)).HeaderTo.FH_StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.RelatedParentChildWorkflowsViewModel)(null)).ExternalWorkflowLinks)).SyncRoot)).FP_SynchroniseBufferPenetration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.RelatedParentChildWorkflowsViewModel)(null)).ExternalWorkflowLinks)).SyncRoot)).HeaderTo.ProviderJobDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.RelatedParentChildWorkflowsViewModel)(null)).ExternalWorkflowLinks)).SyncRoot)).FP_FH_HeaderTo)));
			this.ChildWorkflowGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("431EF800-F004-4516-8764-8E95B0E053C1", "Child Workflow");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "FP_FH_HeaderFrom";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			zTextBoxColumnStyleInfo1.ColumnName = "HeaderFrom+ProcessHeaderType";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("C9793AFC-5468-40FF-9055-37D91A4E1170", "Child Job Number");
			zTextBoxColumnStyleInfo2.ColumnName = "HeaderFrom+ProviderJobNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "HeaderFrom+FH_StatusDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zCheckBoxColumnStyleInfo1.ColumnName = "FP_SynchroniseBufferPenetration";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo4.ColumnName = "HeaderFrom+ProviderJobDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("DCA93A46-4F01-4AD7-B33C-45EF67DED331", "Child Job Description");
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("B9E51097-6DD1-4604-8E27-871D8FE7AF65", "Parent Workflow");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "FP_FH_HeaderTo";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ChildWorkflowGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ChildWorkflowGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChildWorkflowGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ChildWorkflowGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ChildWorkflowGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ChildWorkflowGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ChildWorkflowGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ChildWorkflowGrid.GridId = "5064878D-6AF8-4A61-AB7F-9B26BA6A43CC";
			this.ChildWorkflowGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChildWorkflowGrid.LayoutKey = "ChildWorkflowGrid";
			this.ChildWorkflowGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 30, true);
			this.ChildWorkflowGrid.Name = "ChildWorkflowGrid";
			this.ChildWorkflowGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(797, 197, true);
			this.ChildWorkflowGrid.TabIndex = 11;
			this.ChildWorkflowGrid.TabStop = false;
			this.ChildWorkflowGrid.DoubleClick += new System.EventHandler(this.ChildWorkflowGrid_DoubleClick);
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("C1349DB4-346E-48B9-88EB-C4BECFFB83EE", "Below are listed the workflows outside the job that are children for this job\'s workflows. Double click a row in this grid to open the child workflow.");
			this.zLabel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 27, true);
			this.zLabel1.TabIndex = 12;
			// 
			// JobRelatedChildWorkflowsControl
			// 
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.ChildWorkflowGrid);
			this.Controls.Add(this.AddButton);
			this.Controls.Add(this.RemoveButton);
			this.Name = "JobRelatedChildWorkflowsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 259, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChildWorkflowGrid)).EndInit();
			this.ChildWorkflowGrid.ResumeLayout(false);
			this.ChildWorkflowGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		protected ZArchitecture.GUI.ZButton AddButton;
		protected ZArchitecture.GUI.ZButton RemoveButton;
		protected ZArchitecture.ZGrid ChildWorkflowGrid;
		private ZArchitecture.ZLabel zLabel1;
	}
}
