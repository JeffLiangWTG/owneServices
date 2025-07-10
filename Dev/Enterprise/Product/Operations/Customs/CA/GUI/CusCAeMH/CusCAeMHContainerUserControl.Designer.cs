namespace Enterprise.Customs.CA.GUI
{
	partial class CusCAeMHContainerUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.eMHContainerSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ContainersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ContainerTypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.Seal2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Seal1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContainerNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.eMHContainerSplitContainer)).BeginInit();
			this.eMHContainerSplitContainer.Panel1.SuspendLayout();
			this.eMHContainerSplitContainer.Panel2.SuspendLayout();
			this.eMHContainerSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.CusCAeMHContainerCollection);
			// 
			// eMHContainerSplitContainer
			// 
			this.eMHContainerSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eMHContainerSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.eMHContainerSplitContainer.IsSplitterFixed = true;
			this.eMHContainerSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.eMHContainerSplitContainer.Name = "eMHContainerSplitContainer";
			this.eMHContainerSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// eMHContainerSplitContainer.Panel1
			// 
			this.eMHContainerSplitContainer.Panel1.Controls.Add(this.ContainersGrid);
			// 
			// eMHContainerSplitContainer.Panel2
			// 
			this.eMHContainerSplitContainer.Panel2.Controls.Add(this.ContainerTypeCodeFindBox);
			this.eMHContainerSplitContainer.Panel2.Controls.Add(this.Seal2TextBox);
			this.eMHContainerSplitContainer.Panel2.Controls.Add(this.Seal1TextBox);
			this.eMHContainerSplitContainer.Panel2.Controls.Add(this.ContainerNumberTextBox);
			this.eMHContainerSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 179, true);
			this.eMHContainerSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(121);
			this.eMHContainerSplitContainer.TabIndex = 0;
			// 
			// ContainersGrid
			// 
			this.ContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainersGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusCAeMHContainer)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHContainer)(null)).BQ_ContainerNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHContainer)(null)).BQ_RC_NKContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHContainer)(null)).BQ_Seal1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHContainer)(null)).BQ_Seal2)));
			this.ContainersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8fc95529-f214-4c83-b53b-57662a257d3d", "Container No.");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "BQ_ContainerNumber";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("13398939-f92b-4893-b30a-306188d03932", "Type");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "BQ_RC_NKContainerType";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0232c208-5a8a-4bd3-a85a-ef5bfda9916e", "Seal 1");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "BQ_Seal1";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("fcac9cd8-3e4d-4057-b757-dd5c612797ea", "Seal 2");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "BQ_Seal2";
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ContainersGrid.CopySelectedRowsAllowed = true;
			this.ContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersGrid.GridId = "505cf355-a6df-4cff-8473-a03c0e260378";
			this.ContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersGrid.LayoutKey = "ContainersGrid";
			this.ContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainersGrid.Name = "ContainersGrid";
			this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 121, true);
			this.ContainersGrid.TabIndex = 0;
			// 
			// ContainerTypeCodeFindBox
			// 
			this.ContainerTypeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerTypeCodeFindBox, "BQ_RC_NKContainerType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHContainer)(null)).BQ_RC_NKContainerType)));
			this.ContainerTypeCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b079756a-bb2e-43fd-9e6e-201f088c968e", "Type");
			this.ContainerTypeCodeFindBox.DisableInvalidation = false;
			this.ContainerTypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 3, true);
			this.ContainerTypeCodeFindBox.Name = "ContainerTypeCodeFindBox";
			this.ContainerTypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ContainerTypeCodeFindBox.TabIndex = 1;
			// 
			// Seal2TextBox
			// 
			this.BindingSource.SetBindingMember(this.Seal2TextBox, "BQ_Seal2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHContainer)(null)).BQ_Seal2)));
			this.Seal2TextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("03bf6c34-d485-4acb-b71d-c74f5e358f46", "Seal 2");
			this.Seal2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 29, true);
			this.Seal2TextBox.Name = "Seal2TextBox";
			this.Seal2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.Seal2TextBox.TabIndex = 3;
			// 
			// Seal1TextBox
			// 
			this.BindingSource.SetBindingMember(this.Seal1TextBox, "BQ_Seal1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHContainer)(null)).BQ_Seal1)));
			this.Seal1TextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e7af22ed-655d-41b3-9abe-d5edc601f95f", "Seal 1");
			this.Seal1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 29, true);
			this.Seal1TextBox.Name = "Seal1TextBox";
			this.Seal1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.Seal1TextBox.TabIndex = 2;
			// 
			// ContainerNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContainerNumberTextBox, "BQ_ContainerNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHContainer)(null)).BQ_ContainerNumber)));
			this.ContainerNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e4a45331-ecb9-493b-ad16-ebdfca28dcc5", "Container No.");
			this.ContainerNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 3, true);
			this.ContainerNumberTextBox.Name = "ContainerNumberTextBox";
			this.ContainerNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.ContainerNumberTextBox.TabIndex = 0;
			// 
			// CusCAeMHContainerUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.eMHContainerSplitContainer);
			this.Name = "CusCAeMHContainerUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 179, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.eMHContainerSplitContainer.Panel1.ResumeLayout(false);
			this.eMHContainerSplitContainer.Panel2.ResumeLayout(false);
			this.eMHContainerSplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.eMHContainerSplitContainer)).EndInit();
			this.eMHContainerSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer eMHContainerSplitContainer;
		private ZArchitecture.ZGrid ContainersGrid;
		private ZArchitecture.ZTextBox ContainerNumberTextBox;
		private ZArchitecture.ZTextBox Seal2TextBox;
		private ZArchitecture.ZTextBox Seal1TextBox;
		private ZArchitecture.GUI.ZCodeFindBox ContainerTypeCodeFindBox;
	}
}
