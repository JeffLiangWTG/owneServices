namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class TransportUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CommentWordWrapTextBox = new Enterprise.Customs.GUI.WordWrappingTextBox();
			this.SealDetailsWordWrapTextBox = new Enterprise.Customs.GUI.WordWrappingTextBox();
			this.SealNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IdentityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportUnitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TransportsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportUnitDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransportsGrid)).BeginInit();
			this.TransportsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration);
			// 
			// CommentWordWrapTextBox
			// 
			this.CommentWordWrapTextBox.AcceptsReturn = true;
			this.CommentWordWrapTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommentWordWrapTextBox, "CusContainers.Comment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSCusContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).CusContainers)).SyncRoot)).Comment)));
			this.CommentWordWrapTextBox.CaptionResourceString = null;
			this.CommentWordWrapTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelTop(this.CommentWordWrapTextBox, 2);
			this.CommentWordWrapTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 268, true);
			this.CommentWordWrapTextBox.Multiline = true;
			this.CommentWordWrapTextBox.Name = "CommentWordWrapTextBox";
			this.CommentWordWrapTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.CommentWordWrapTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 65, true);
			this.CommentWordWrapTextBox.TabIndex = 5;
			// 
			// SealDetailsWordWrapTextBox
			// 
			this.SealDetailsWordWrapTextBox.AcceptsReturn = true;
			this.SealDetailsWordWrapTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SealDetailsWordWrapTextBox, "CusContainers.SealDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSCusContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).CusContainers)).SyncRoot)).SealDetails)));
			this.SealDetailsWordWrapTextBox.CaptionResourceString = null;
			this.SealDetailsWordWrapTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelTop(this.SealDetailsWordWrapTextBox, 2);
			this.SealDetailsWordWrapTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 197, true);
			this.SealDetailsWordWrapTextBox.Multiline = true;
			this.SealDetailsWordWrapTextBox.Name = "SealDetailsWordWrapTextBox";
			this.SealDetailsWordWrapTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.SealDetailsWordWrapTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 65, true);
			this.SealDetailsWordWrapTextBox.TabIndex = 4;
			// 
			// SealNumberTextBox
			// 
			this.SealNumberTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SealNumberTextBox, "CusContainers.CO_Seal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSCusContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).CusContainers)).SyncRoot)).CO_Seal)));
			this.SealNumberTextBox.CaptionResourceString = null;
			this.SealNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SealNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 171, true);
			this.SealNumberTextBox.Name = "SealNumberTextBox";
			this.SealNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 20, true);
			this.SealNumberTextBox.TabIndex = 3;
			// 
			// IdentityTextBox
			// 
			this.IdentityTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IdentityTextBox, "CusContainers.CO_ContainerNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSCusContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).CusContainers)).SyncRoot)).CO_ContainerNumber)));
			this.IdentityTextBox.CaptionResourceString = null;
			this.IdentityTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.IdentityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 145, true);
			this.IdentityTextBox.Name = "IdentityTextBox";
			this.IdentityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 20, true);
			this.IdentityTextBox.TabIndex = 2;
			// 
			// TransportUnitDropEdit
			// 
			this.TransportUnitDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportUnitDropEdit, "CusContainers.ZG_UnitCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.EMCS.Business.EMCSCusContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).CusContainers)).SyncRoot)).ZG_UnitCode)));
			this.TransportUnitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 119, true);
			this.TransportUnitDropEdit.Name = "TransportUnitDropEdit";
			this.TransportUnitDropEdit.ShouldResizeByMaxLength = true;
			this.TransportUnitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 20, true);
			this.TransportUnitDropEdit.TabIndex = 1;
			// 
			// TransportsGrid
			// 
			this.TransportsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TransportsGrid, "CusContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).CusContainers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSCusContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).CusContainers)).SyncRoot)).ZG_UnitCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSCusContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).CusContainers)).SyncRoot)).ZG_UnitCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSCusContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).CusContainers)).SyncRoot)).CO_ContainerNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSCusContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).CusContainers)).SyncRoot)).CO_Seal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSCusContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).CusContainers)).SyncRoot)).SealDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSCusContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).CusContainers)).SyncRoot)).Comment)));
			this.TransportsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "ZG_UnitCode";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("C6B055E6-51DD-4173-91AF-8CC38C13BC57", "Type of Service");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo1.ColumnName = "ZG_UnitCodeDescription";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("C6B055E6-51DD-4173-91AF-8CC38C13BC57", "Type of Service");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo2.ColumnName = "CO_ContainerNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zTextBoxColumnStyleInfo3.ColumnName = "CO_Seal";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo4.ColumnName = "SealDetails";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo5.ColumnName = "Comment";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			this.TransportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TransportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TransportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TransportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TransportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.TransportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.TransportsGrid.GridId = "{6CF40E03-0679-4747-8EE8-CDDCA1214DBB}";
			this.TransportsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransportsGrid.LayoutKey = "TransportsGrid";
			this.TransportsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TransportsGrid.Name = "TransportsGrid";
			this.TransportsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 110, true);
			this.TransportsGrid.TabIndex = 0;
			// 
			// TransportUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CommentWordWrapTextBox);
			this.Controls.Add(this.SealDetailsWordWrapTextBox);
			this.Controls.Add(this.SealNumberTextBox);
			this.Controls.Add(this.IdentityTextBox);
			this.Controls.Add(this.TransportUnitDropEdit);
			this.Controls.Add(this.TransportsGrid);
			this.Name = "TransportUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 335, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportUnitDropEdit.ResumeLayout(true);
			this.TransportUnitDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransportsGrid)).EndInit();
			this.TransportsGrid.ResumeLayout(false);
			this.TransportsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid TransportsGrid;
		private ZArchitecture.GUI.ZDropEdit TransportUnitDropEdit;
		private ZArchitecture.ZTextBox IdentityTextBox;
		private ZArchitecture.ZTextBox SealNumberTextBox;
		private Enterprise.Customs.GUI.WordWrappingTextBox SealDetailsWordWrapTextBox;
		private Enterprise.Customs.GUI.WordWrappingTextBox CommentWordWrapTextBox;
	}
}
