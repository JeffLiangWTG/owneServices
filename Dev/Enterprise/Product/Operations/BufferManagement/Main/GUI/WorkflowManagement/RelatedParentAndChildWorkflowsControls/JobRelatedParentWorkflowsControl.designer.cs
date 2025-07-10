namespace Enterprise.BufferManagement.GUI
{
	partial class JobRelatedParentWorkflowsControl
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
			this.ParentWorkflowGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ParentWorkflowGrid)).BeginInit();
			this.ParentWorkflowGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.RelatedParentChildWorkflowsViewModel);
			// 
			// AddButton
			// 
			this.AddButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AddButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("3FC40AB5-3E55-44A6-AB35-A7DED9A52715", "Add");
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
			this.RemoveButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("3481AB35-5CB8-4D6B-8A4C-3E522056BEBA", "Remove");
			this.RemoveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(722, 230, true);
			this.RemoveButton.Name = "RemoveButton";
			this.RemoveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.RemoveButton.TabIndex = 10;
			this.RemoveButton.ToolTipCaption = null;
			this.RemoveButton.UseVisualStyleBackColor = true;
			this.RemoveButton.Click += new System.EventHandler(this.RemoveButton_Click);
			// 
			// ParentWorkflowGrid
			// 
			this.ParentWorkflowGrid.AllowNavigation = false;
			this.ParentWorkflowGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ParentWorkflowGrid, "ExternalWorkflowLinks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.RelatedParentChildWorkflowsViewModel)(null)).ExternalWorkflowLinks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.RelatedParentChildWorkflowsViewModel)(null)).ExternalWorkflowLinks)).SyncRoot)).FP_FH_HeaderTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.RelatedParentChildWorkflowsViewModel)(null)).ExternalWorkflowLinks)).SyncRoot)).HeaderTo.ProcessHeaderType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.RelatedParentChildWorkflowsViewModel)(null)).ExternalWorkflowLinks)).SyncRoot)).HeaderTo.ProviderJobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.RelatedParentChildWorkflowsViewModel)(null)).ExternalWorkflowLinks)).SyncRoot)).HeaderTo.FH_StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.RelatedParentChildWorkflowsViewModel)(null)).ExternalWorkflowLinks)).SyncRoot)).FP_SynchroniseBufferPenetration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.RelatedParentChildWorkflowsViewModel)(null)).ExternalWorkflowLinks)).SyncRoot)).HeaderTo.ProviderJobDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.ProcessHeaderLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.RelatedParentChildWorkflowsViewModel)(null)).ExternalWorkflowLinks)).SyncRoot)).FP_FH_HeaderFrom)));
			this.ParentWorkflowGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("5C565488-EF89-407C-89F3-4B80C9176259", "Parent Workflow");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "FP_FH_HeaderTo";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			zTextBoxColumnStyleInfo1.ColumnName = "HeaderTo+ProcessHeaderType";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("F71D5B41-0382-49D0-A3EF-96697C7F872C", "Parent Job Number");
			zTextBoxColumnStyleInfo2.ColumnName = "HeaderTo+ProviderJobNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "HeaderTo+FH_StatusDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zCheckBoxColumnStyleInfo1.ColumnName = "FP_SynchroniseBufferPenetration";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo4.ColumnName = "HeaderTo+ProviderJobDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("63DCE18B-6F5C-4E2B-A5FC-7F8B18A2041A", "Parent Job Description");
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("553C3DFA-C278-4869-9286-3AF679C35568", "Child Workflow");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "FP_FH_HeaderFrom";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ParentWorkflowGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ParentWorkflowGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ParentWorkflowGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ParentWorkflowGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ParentWorkflowGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ParentWorkflowGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ParentWorkflowGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ParentWorkflowGrid.GridId = "175eddfb-56da-4355-93e0-0478607c654f";
			this.ParentWorkflowGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ParentWorkflowGrid.LayoutKey = "ParentWorkflowGrid";
			this.ParentWorkflowGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 30, true);
			this.ParentWorkflowGrid.Name = "ParentWorkflowGrid";
			this.ParentWorkflowGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(797, 197, true);
			this.ParentWorkflowGrid.TabIndex = 11;
			this.ParentWorkflowGrid.TabStop = false;
			this.ParentWorkflowGrid.DoubleClick += new System.EventHandler(this.ParentWorkflowGrid_DoubleClick);
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("13219057-73B9-420B-A43B-157779EEFE56", "Below are listed the workflows outside the job that are parents for this job\'s workflows. Double click a row in this grid to open the parent workflow.");
			this.zLabel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 27, true);
			this.zLabel1.TabIndex = 12;
			// 
			// JobRelatedParentWorkflowsControl
			// 
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.ParentWorkflowGrid);
			this.Controls.Add(this.AddButton);
			this.Controls.Add(this.RemoveButton);
			this.Name = "JobRelatedParentWorkflowsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 259, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ParentWorkflowGrid)).EndInit();
			this.ParentWorkflowGrid.ResumeLayout(false);
			this.ParentWorkflowGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		protected ZArchitecture.GUI.ZButton AddButton;
		protected ZArchitecture.GUI.ZButton RemoveButton;
		protected ZArchitecture.ZGrid ParentWorkflowGrid;
		private ZArchitecture.ZLabel zLabel1;
	}
}
