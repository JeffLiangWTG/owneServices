using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Customs.EU.H7.GUI
{
	partial class EUH7SimilarAddressesSelectionForm
	{
		protected override void InitializeComponent()
		{
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.BillAddressGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BillAddressDisplayGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
			this.OrgSelectionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SimilarOrgsDisplayGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
			this.IgnoreButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectOrgButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CreateOrgButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.BillAddressGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillAddressDisplayGrid)).BeginInit();
			this.BillAddressDisplayGrid.SuspendLayout();
			this.OrgSelectionGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SimilarOrgsDisplayGrid)).BeginInit();
			this.SimilarOrgsDisplayGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 687, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(809, 29, true);
			this.MainStatusBar.Visible = true;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel);
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 10, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.BillAddressGroupBox);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.OrgSelectionGroupBox);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(809, 687, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(284);
			this.MainSplitContainer.SplitterWidth = 5;
			this.MainSplitContainer.TabIndex = 1;
			// 
			// BillAddressGroupBox
			// 
			this.BillAddressGroupBox.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("3b8eb5d8-661f-45d1-9fe2-5b86c66f3a75", "Select parties to convert to Organizations");
			this.BillAddressGroupBox.Controls.Add(this.BillAddressDisplayGrid);
			this.BillAddressGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillAddressGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillAddressGroupBox.Name = "BillAddressGroupBox";
			this.BillAddressGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(809, 284, true);
			this.BillAddressGroupBox.TabIndex = 0;
			this.BillAddressGroupBox.TabStop = false;
			// 
			// BillAddressDisplayGrid
			// 
			this.BillAddressDisplayGrid.AllowNavigation = false;
			this.BillAddressDisplayGrid.CaptionVisible = false;
			this.BillAddressDisplayGrid.GridId = "7c2a772a-a477-4d39-a399-3a82d8b91e40";
			this.BillAddressDisplayGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BillAddressDisplayGrid.LayoutKey = "BillAddressDisplayGrid";
			this.BillAddressDisplayGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 19, true);
			this.BillAddressDisplayGrid.Name = "BillAddressDisplayGrid";
			this.BillAddressDisplayGrid.ShouldSetErrorsOnTabPage = false;
			this.BillAddressDisplayGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 259, true);
			this.BillAddressDisplayGrid.TabIndex = 0;
			SetupBillAddressSelectionGridColumn();
			// 
			// OrgSelectionGroupBox
			// 
			this.OrgSelectionGroupBox.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("2dcb6699-0875-4d17-bcab-f0ad61e24d3e", "Matching Organizations");
			this.OrgSelectionGroupBox.Controls.Add(this.SimilarOrgsDisplayGrid);
			this.OrgSelectionGroupBox.Controls.Add(this.IgnoreButton);
			this.OrgSelectionGroupBox.Controls.Add(this.SelectOrgButton);
			this.OrgSelectionGroupBox.Controls.Add(this.CreateOrgButton);
			this.OrgSelectionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrgSelectionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrgSelectionGroupBox.Name = "OrgSelectionGroupBox";
			this.OrgSelectionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(809, 398, true);
			this.OrgSelectionGroupBox.TabIndex = 1;
			this.OrgSelectionGroupBox.TabStop = false;
			// 
			// SimilarOrgsDisplayGrid
			// 
			this.SimilarOrgsDisplayGrid.AllowNavigation = false;
			this.SimilarOrgsDisplayGrid.CaptionVisible = false;
			this.SimilarOrgsDisplayGrid.GridId = "37ee7af5-e647-428a-86cc-59157401d0ef";
			this.SimilarOrgsDisplayGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SimilarOrgsDisplayGrid.LayoutKey = "SimilarOrgsDisplayGrid";
			this.SimilarOrgsDisplayGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 19, true);
			this.SimilarOrgsDisplayGrid.Name = "SimilarOrgsDisplayGrid";
			this.SimilarOrgsDisplayGrid.ShouldSetErrorsOnTabPage = false;
			this.SimilarOrgsDisplayGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 321, true);
			this.SimilarOrgsDisplayGrid.TabIndex = 0;
			SetupSimilarAddressSelectionGridColumn();
			// 
			// IgnoreButton
			// 
			this.IgnoreButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.IgnoreButton.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("bb953c0a-1215-4736-b625-1f8f95cf6e5d", "Ignore");
			this.IgnoreButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(671, 358, true);
			this.IgnoreButton.Name = "IgnoreButton";
			this.IgnoreButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 23, true);
			this.IgnoreButton.TabIndex = 4;
			this.IgnoreButton.ToolTipCaption = null;
			this.IgnoreButton.UseVisualStyleBackColor = true;
			this.IgnoreButton.Click += IgnoreButton_Click;
			// 
			// SelectOrgButton
			// 
			this.SelectOrgButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectOrgButton.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("721ebea9-d784-48a9-b007-ab4fecd05f6a", "&Select Address");
			this.SelectOrgButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(479, 358, true);
			this.SelectOrgButton.Name = "SelectOrgButton";
			this.SelectOrgButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 23, true);
			this.SelectOrgButton.TabIndex = 3;
			this.SelectOrgButton.ToolTipCaption = null;
			this.SelectOrgButton.UseVisualStyleBackColor = true;
			this.SelectOrgButton.Click += SelectOrgButton_Click;
			// 
			// CreateOrgButton
			// 
			this.CreateOrgButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CreateOrgButton.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("0e61e183-0644-4893-bae2-d595322e8cdc", "&New Organization");
			this.CreateOrgButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 358, true);
			this.CreateOrgButton.Name = "CreateOrgButton";
			this.CreateOrgButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 23, true);
			this.CreateOrgButton.TabIndex = 2;
			this.CreateOrgButton.ToolTipCaption = null;
			this.CreateOrgButton.UseVisualStyleBackColor = true;
			this.CreateOrgButton.Click += CreateOrgButton_Click;
			// 
			// EUH7SimilarAddressesSelectionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("4e931d25-76ee-4496-8291-272c85b1ca33", "Convert parties to Organizations");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(809, 716, true);
			this.Controls.Add(this.MainSplitContainer);
			this.DataSourceType = typeof(Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel);
			this.Name = "EUH7SimilarAddressesSelectionForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MainSplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.BillAddressGroupBox.ResumeLayout(false);
			this.BillAddressGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillAddressDisplayGrid)).EndInit();
			this.BillAddressDisplayGrid.ResumeLayout(false);
			this.BillAddressDisplayGrid.PerformLayout();
			this.OrgSelectionGroupBox.ResumeLayout(false);
			this.OrgSelectionGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SimilarOrgsDisplayGrid)).EndInit();
			this.SimilarOrgsDisplayGrid.ResumeLayout(false);
			this.SimilarOrgsDisplayGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		void SetupBillAddressSelectionGridColumn()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.BindingSource.SetBindingMember(this.BillAddressDisplayGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).BillNumber));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).AddressType));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).Address1));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).Address2));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).City));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).State));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).Postcode));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).Country));
			zTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("3e7d8900-f5ef-4a34-9a98-47285671530f", "Bill Number");
			zTextBoxColumnStyleInfo.ColumnName = "BillNumber";
			zTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("b329b95c-8755-4e12-bd07-4a84b653773b", "Party type");
			zTextBoxColumnStyleInfo1.ColumnName = "AddressType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("8ecec5b8-5507-4343-bbff-8b13ca5782b7", "Name");
			zTextBoxColumnStyleInfo2.ColumnName = "OrgName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("d743969e-103c-49c0-9c70-eb0c72c52298", "Address 1");
			zTextBoxColumnStyleInfo3.ColumnName = "Address1";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("26a2e4e8-b059-47d8-8eb7-e73fea69b301", "Address 2");
			zTextBoxColumnStyleInfo4.ColumnName = "Address2";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("26a2e4e8-b059-47d8-8eb7-e73fea69b301", "City");
			zTextBoxColumnStyleInfo5.ColumnName = "City";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("26a2e4e8-b059-47d8-8eb7-e73fea69b301", "State");
			zTextBoxColumnStyleInfo6.ColumnName = "State";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("26a2e4e8-b059-47d8-8eb7-e73fea69b301", "Postcode");
			zTextBoxColumnStyleInfo7.ColumnName = "Postcode";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("26a2e4e8-b059-47d8-8eb7-e73fea69b301", "Ctry/Rgn.");
			zTextBoxColumnStyleInfo8.ColumnName = "Country";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.BillAddressDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo);
			this.BillAddressDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BillAddressDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.BillAddressDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.BillAddressDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.BillAddressDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.BillAddressDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.BillAddressDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.BillAddressDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
		}

		void SetupSimilarAddressSelectionGridColumn()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.BindingSource.SetBindingMember(this.SimilarOrgsDisplayGrid, "SimilarOrgMatches");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).SimilarOrgMatches)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).SimilarOrgMatches)).SyncRoot)).OS_Rank)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).SimilarOrgMatches)).SyncRoot)).OH_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).SimilarOrgMatches)).SyncRoot)).OH_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_Address2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_City)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_State)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_PostCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).SimilarOrgMatches)).SyncRoot)).OS_UNLOCO)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_Phone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_Fax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).SimilarOrgMatches)).SyncRoot)).LocalBusinessNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBillSimilarAddressViewModel)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_Email)));
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Res.GetData("b369d143-a9d6-4c79-ad5b-9bf47a7034c8", "Rank");
			zCalcEditColumnStyleInfo1.ColumnName = "OS_Rank";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("1ba694e8-00e4-4e50-9da1-53d8c8bf7185", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "OH_Code";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = GUI.Res.GetData("6bff7f44-7dc4-4fc0-921d-9ec5970a350a", "Full Name");
			zTextBoxColumnStyleInfo2.ColumnName = "OH_FullName";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = GUI.Res.GetData("050fe056-b4d7-41c5-909f-a257a9adc6f9", "Street", "Address 1", "");
			zTextBoxColumnStyleInfo3.ColumnName = "OH_Calc_Address1";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = GUI.Res.GetData("e82e2047-c388-4039-97fb-f640ba3d07a2", "Street 2", "Address 2", "");
			zTextBoxColumnStyleInfo4.ColumnName = "OH_Calc_Address2";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.CaptionResourceString = GUI.Res.GetData("23f072c3-15ab-4d9e-af39-d4eb59f267d1", "City", "City", "");
			zTextBoxColumnStyleInfo5.ColumnName = "OH_Calc_City";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = GUI.Res.GetData("a241c18f-fc36-425e-be91-d50ff20fbd61", "State", "State", "");
			zTextBoxColumnStyleInfo6.ColumnName = "OH_Calc_State";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = GUI.Res.GetData("ac2f2e70-9b02-4822-b38f-0f4ee5627dbf", "P. Code", "Post Code", "");
			zTextBoxColumnStyleInfo7.ColumnName = "OH_Calc_PostCode";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo8.ColumnName = "OS_UNLOCO";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = GUI.Res.GetData("d46ccb71-229c-44aa-aa1f-3c256ae291be", "Phone", "Phone", "");
			zTextBoxColumnStyleInfo9.ColumnName = "OH_Calc_Phone";
			zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = GUI.Res.GetData("74ec96228-2723-469f-9de6-3f10ad0482c3", "Fax", "Fax", "");
			zTextBoxColumnStyleInfo10.ColumnName = "OH_Calc_Fax";
			zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = GUI.Res.GetData("e8074186-d09c-40ec-bf1e-e4d86beb6aa3", "Reg. #", "Business Reg No.", "");
			zTextBoxColumnStyleInfo11.ColumnName = "LocalBusinessNumber";
			zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo12.CaptionResourceString = GUI.Res.GetData("06615bd5-b0d4-4060-bbac-6f393eaa09b3", "Email", "Email", "");
			zTextBoxColumnStyleInfo12.ColumnName = "OH_Calc_Email";
			zTextBoxColumnStyleInfo12.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SimilarOrgsDisplayGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SimilarOrgsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SimilarOrgsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SimilarOrgsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SimilarOrgsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SimilarOrgsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.SimilarOrgsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.SimilarOrgsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.SimilarOrgsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.SimilarOrgsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.SimilarOrgsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.SimilarOrgsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.SimilarOrgsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
		}

		private CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		private ZArchitecture.GUI.ZGroupBox BillAddressGroupBox;
		private ZDisplayGrid BillAddressDisplayGrid;
		private ZDisplayGrid SimilarOrgsDisplayGrid;
		private ZArchitecture.GUI.ZGroupBox OrgSelectionGroupBox;
		private ZArchitecture.GUI.ZButton CreateOrgButton;
		private ZArchitecture.GUI.ZButton SelectOrgButton;
		private ZArchitecture.GUI.ZButton IgnoreButton;
	}
}
