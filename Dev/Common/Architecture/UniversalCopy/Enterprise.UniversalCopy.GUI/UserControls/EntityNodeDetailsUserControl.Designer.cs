using System.Windows.Forms;

namespace Enterprise.UniversalCopy.GUI
{
	partial class EntityNodeDetailsUserControl
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			zMultiControlColumnStyleInfo1 = new Enterprise.UniversalCopy.GUI.UniversalCopyMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.splitContainerElementDetails = new CargoWise.Windows.UI.KSplitContainer();
			this.groupBoxProperties = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.gridProperties = new Enterprise.ZArchitecture.ZGrid();
			this.tabControlDetails = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.tabPageElementDetails = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxElement = new Enterprise.ZArchitecture.ZTextBox();
			this.tabPageCollectionFilter = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.panelFilter = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.panelSort = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.tabPagePropertiesDetails = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zTextBox7 = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxPropertyValue = new Enterprise.ZArchitecture.ZTextBox();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zTextBox5 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox4 = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerElementDetails)).BeginInit();
			this.splitContainerElementDetails.Panel1.SuspendLayout();
			this.splitContainerElementDetails.Panel2.SuspendLayout();
			this.splitContainerElementDetails.SuspendLayout();
			this.groupBoxProperties.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridProperties)).BeginInit();
			this.gridProperties.SuspendLayout();
			this.tabControlDetails.SuspendLayout();
			this.tabPageElementDetails.SuspendLayout();
			this.tabPageElementDetails.SuspendLayout();
			this.tabPageCollectionFilter.SuspendLayout();
			this.tabPageCollectionFilter.SuspendLayout();
			this.tabPagePropertiesDetails.SuspendLayout();
			this.tabPagePropertiesDetails.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo);
			// 
			// splitContainerElementDetails
			// 
			this.splitContainerElementDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerElementDetails.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainerElementDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerElementDetails.Name = "splitContainerElementDetails";
			this.splitContainerElementDetails.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerElementDetails.Panel1
			// 
			this.splitContainerElementDetails.Panel1.Controls.Add(this.groupBoxProperties);
			this.splitContainerElementDetails.Panel1MinSize = 50;
			// 
			// splitContainerElementDetails.Panel2
			// 
			this.splitContainerElementDetails.Panel2.Controls.Add(this.tabControlDetails);
			this.splitContainerElementDetails.Panel2MinSize = 100;
			this.splitContainerElementDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 600, true);
			this.splitContainerElementDetails.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(486);
			this.splitContainerElementDetails.TabIndex = 1;
			// 
			// groupBoxProperties
			// 
			this.groupBoxProperties.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("92468515-963a-42c1-b468-a34d66a8de42", "Properties");
			this.groupBoxProperties.Controls.Add(this.gridProperties);
			this.groupBoxProperties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxProperties.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupBoxProperties.Name = "groupBoxProperties";
			this.groupBoxProperties.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 486, true);
			this.groupBoxProperties.TabIndex = 3;
			this.groupBoxProperties.TabStop = false;
			// 
			// gridProperties
			// 
			this.gridProperties.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.gridProperties, "PropertyNodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).PropertyNodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.PropertyCopyTemplateBizo)(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).PropertyNodes)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.PropertyCopyTemplateBizo)(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).PropertyNodes)).SyncRoot)).CopyMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.PropertyCopyTemplateBizo)(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).PropertyNodes)).SyncRoot)).CopyMethods)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.PropertyCopyTemplateBizo)(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).PropertyNodes)).SyncRoot)).CopyMethodDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.PropertyCopyTemplateBizo)(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).PropertyNodes)).SyncRoot)).Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.PropertyCopyTemplateBizo)(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).PropertyNodes)).SyncRoot)).ValueColumnType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.PropertyCopyTemplateBizo)(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).PropertyNodes)).SyncRoot)).ValuesList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.PropertyCopyTemplateBizo)(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).PropertyNodes)).SyncRoot)).ValueType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.PropertyCopyTemplateBizo)(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).PropertyNodes)).SyncRoot)).Name)));
			this.gridProperties.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("50026af9-e0e1-44b4-94e3-1d6f00e0107a", "Caption");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zDropEditColumnStyleInfo1.BindToList = "CopyMethods";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("5b1b05c3-2ed0-4cc4-a71b-eaf0137985aa", "Copy Method Code");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CopyMethod";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo2.BindToList = "CopyMethodDescriptions";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("2f478dad-04c1-45ce-b096-79412a2ae4df", "Copy Method Description");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zDropEditColumnStyleInfo2.ColumnName = "CopyMethodDescription";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zMultiControlColumnStyleInfo1.BindToList = "ValuesList";
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("c8f1edaf-8ffe-4a99-b57d-d45ef404d2b7", "Value");
			zMultiControlColumnStyleInfo1.ColumnName = "Value";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ValueColumnType";
			zMultiControlColumnStyleInfo1.SupportsMacroTemplates = true;
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zMultiControlColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("1257c117-1b1f-4c72-82eb-ec4378849d98", "Value Type");
			zTextBoxColumnStyleInfo3.ColumnName = "ValueType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("046633cd-49ed-416b-beeb-7f71610c4bcc", "Data Field");
			zTextBoxColumnStyleInfo4.ColumnName = "Name";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.gridProperties.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.gridProperties.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.gridProperties.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.gridProperties.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.gridProperties.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.gridProperties.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.gridProperties.CopySelectedRowsAllowed = true;
			this.gridProperties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridProperties.GridId = "2ece9a15-1a7e-4884-bb9e-48d4b8e685ee";
			this.gridProperties.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridProperties.LayoutKey = "gridProperties";
			this.gridProperties.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.gridProperties.Name = "gridProperties";
			this.gridProperties.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 467, true);
			this.gridProperties.TabIndex = 3;
			// 
			// tabControlDetails
			// 
			this.tabControlDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.tabControlDetails.Controls.Add(this.tabPageElementDetails);
			this.tabControlDetails.Controls.Add(this.tabPageCollectionFilter);
			this.tabControlDetails.Controls.Add(this.tabPagePropertiesDetails);
			this.tabControlDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tabControlDetails.Name = "tabControlDetails";
			this.tabControlDetails.SelectedIndex = 0;
			this.tabControlDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 110, true);
			this.tabControlDetails.TabIndex = 0;
			// 
			// tabPageElementDetails
			// 
			this.tabPageElementDetails.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("b73a951a-531f-47fb-8df7-e7d3843d5a25", "Element Details");
			this.tabPageElementDetails.Controls.Add(this.zTextBox3);
			this.tabPageElementDetails.Controls.Add(this.zTextBox2);
			this.tabPageElementDetails.Controls.Add(this.zTextBox1);
			this.tabPageElementDetails.Controls.Add(this.textBoxElement);
			this.tabPageElementDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.tabPageElementDetails.Name = "tabPageElementDetails";
			this.tabPageElementDetails.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tabPageElementDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 83, true);
			this.tabPageElementDetails.TabIndex = 0;
			// 
			// zTextBox3
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "Kind");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).Kind)));
			this.zTextBox3.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("6ba76cec-e803-495f-9ff2-2d7d7b8a2fb9", "Kind", "Related record kind.");
			this.zTextBox3.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 32, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.zTextBox3.TabIndex = 4;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "EntityTableNameDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).EntityTableNameDescription)));
			this.zTextBox2.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("08a21d84-257f-48cf-bdbf-3f107df9c6bf", "Type", "Related record\'s type.");
			this.zTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 32, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.zTextBox2.TabIndex = 3;
			// 
			// zTextBox1
			// 
			this.zTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zTextBox1, "Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).Description)));
			this.zTextBox1.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("565dc95c-1766-4951-9038-b0ad0ecf1a34", "Description", "Related record\'s description.");
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 6, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(359, 20, true);
			this.zTextBox1.TabIndex = 2;
			// 
			// textBoxElement
			// 
			this.BindingSource.SetBindingMember(this.textBoxElement, "Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).Name)));
			this.textBoxElement.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("5b009c6b-c66a-4a83-ad5d-f91eafda320a", "Element Name", "Related record\'s name.");
			this.textBoxElement.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.textBoxElement.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 6, true);
			this.textBoxElement.Name = "textBoxElement";
			this.textBoxElement.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.textBoxElement.TabIndex = 1;
			// 
			// tabPageCollectionFilter
			// 
			this.tabPageCollectionFilter.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("bab2ac10-8273-49cc-85b2-8194ac8373f5", "Collection Filter");
			this.tabPageCollectionFilter.Controls.Add(this.panelFilter);
			this.tabPageCollectionFilter.Controls.Add(this.panelSort);
			this.tabPageCollectionFilter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.tabPageCollectionFilter.Name = "tabPageCollectionFilter";
			this.tabPageCollectionFilter.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tabPageCollectionFilter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 83, true);
			this.tabPageCollectionFilter.TabIndex = 1;
			// 
			// panelFilter
			// 
			this.panelFilter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelFilter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 29, true);
			this.panelFilter.Name = "panelFilter";
			this.panelFilter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 51, true);
			this.panelFilter.TabIndex = 5;
			// 
			// panelSort
			// 
			this.panelSort.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelSort.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.panelSort.Name = "panelSort";
			this.panelSort.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 26, true);
			this.panelSort.TabIndex = 4;
			this.panelSort.Visible = false;
			// 
			// tabPagePropertiesDetails
			// 
			this.tabPagePropertiesDetails.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("b6069a0a-24c7-4316-80c6-2b2a5b9a6f61", "Property Details");
			this.tabPagePropertiesDetails.Controls.Add(this.zTextBox7);
			this.tabPagePropertiesDetails.Controls.Add(this.textBoxPropertyValue);
			this.tabPagePropertiesDetails.Controls.Add(this.zDropEdit1);
			this.tabPagePropertiesDetails.Controls.Add(this.zTextBox5);
			this.tabPagePropertiesDetails.Controls.Add(this.zTextBox4);
			this.tabPagePropertiesDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.tabPagePropertiesDetails.Name = "tabPagePropertiesDetails";
			this.tabPagePropertiesDetails.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tabPagePropertiesDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 83, true);
			this.tabPagePropertiesDetails.TabIndex = 2;
			// 
			// zTextBox7
			// 
			this.BindingSource.SetBindingMember(this.zTextBox7, "PropertyNodes.ValueType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.PropertyCopyTemplateBizo)(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).PropertyNodes)).SyncRoot)).ValueType)));
			this.zTextBox7.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("dfeb06ca-6e8f-4c62-b608-6939454dc916", "Value Type", "Type of values of selected property.");
			this.zTextBox7.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 32, true);
			this.zTextBox7.Name = "zTextBox7";
			this.zTextBox7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.zTextBox7.TabIndex = 4;
			// 
			// textBoxPropertyValue
			// 
			this.textBoxPropertyValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.textBoxPropertyValue, "PropertyNodes.Value");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.PropertyCopyTemplateBizo)(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).PropertyNodes)).SyncRoot)).Value)));
			this.textBoxPropertyValue.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("e272a918-6daa-4a66-b8bc-f845a550bbe7", "Value", "New value to be written into selected property on copied element.");
			this.textBoxPropertyValue.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.textBoxPropertyValue.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 58, true);
			this.textBoxPropertyValue.Name = "textBoxPropertyValue";
			this.textBoxPropertyValue.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(727, 20, true);
			this.textBoxPropertyValue.TabIndex = 3;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "PropertyNodes.CopyMethodDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.UniversalCopy.Business.PropertyCopyTemplateBizo)(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).PropertyNodes)).SyncRoot)).CopyMethodDescription)));
			this.zDropEdit1.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("254074c7-bcab-47d4-8aaf-0067fc465485", "Copy Method", "Copy method for selected property.");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 32, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.zDropEdit1.CharacterCasing = CharacterCasing.Normal;
			this.zDropEdit1.ShowDescriptionBox = false;
			this.zDropEdit1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.zDropEdit1.UseFullWidthForCodeBox = true;
			this.zDropEdit1.TabIndex = 2;
			// 
			// zTextBox5
			// 
			this.zTextBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zTextBox5, "PropertyNodes.Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.PropertyCopyTemplateBizo)(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).PropertyNodes)).SyncRoot)).Description)));
			this.zTextBox5.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("f9009c29-4d30-4dcb-96cc-cf089dd18734", "Caption", "Selected property\'s caption.");
			this.zTextBox5.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 6, true);
			this.zTextBox5.Name = "zTextBox5";
			this.zTextBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(359, 20, true);
			this.zTextBox5.TabIndex = 1;
			// 
			// zTextBox4
			// 
			this.BindingSource.SetBindingMember(this.zTextBox4, "PropertyNodes.Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.PropertyCopyTemplateBizo)(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.EntityCopyTemplateBizo)(null)).PropertyNodes)).SyncRoot)).Name)));
			this.zTextBox4.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("7a249a1b-168c-427a-9336-30e9fa572cd3", "Property Name", "Selected property\'s name.");
			this.zTextBox4.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 6, true);
			this.zTextBox4.Name = "zTextBox4";
			this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.zTextBox4.TabIndex = 0;
			// 
			// EntityNodeDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainerElementDetails);
			this.Name = "EntityNodeDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 600, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainerElementDetails.Panel1.ResumeLayout(false);
			this.splitContainerElementDetails.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerElementDetails)).EndInit();
			this.splitContainerElementDetails.ResumeLayout(false);
			this.splitContainerElementDetails.PerformLayout();
			this.groupBoxProperties.ResumeLayout(false);
			this.groupBoxProperties.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridProperties)).EndInit();
			this.gridProperties.ResumeLayout(false);
			this.gridProperties.PerformLayout();
			this.tabControlDetails.ResumeLayout(false);
			this.tabControlDetails.PerformLayout();
			this.tabPageElementDetails.ResumeLayout(false);
			this.tabPageElementDetails.PerformLayout();
			this.tabPageElementDetails.ResumeLayout(false);
			this.tabPageElementDetails.PerformLayout();
			this.tabPageCollectionFilter.ResumeLayout(false);
			this.tabPageCollectionFilter.PerformLayout();
			this.tabPageCollectionFilter.ResumeLayout(false);
			this.tabPagePropertiesDetails.ResumeLayout(false);
			this.tabPagePropertiesDetails.PerformLayout();
			this.tabPagePropertiesDetails.ResumeLayout(false);
			this.tabPagePropertiesDetails.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitContainerElementDetails;
		internal ZArchitecture.GUI.ZTabControl tabControlDetails;
		protected ZArchitecture.GUI.ZTabPage tabPageElementDetails;
		internal ZArchitecture.GUI.ZTabPage tabPageCollectionFilter;
		internal ZArchitecture.GUI.ZTabPage tabPagePropertiesDetails;
		internal ZArchitecture.ZTextBox zTextBox1;
		private ZArchitecture.ZTextBox textBoxElement;
		private ZArchitecture.ZTextBox zTextBox3;
		private ZArchitecture.ZTextBox zTextBox2;
		private ZArchitecture.ZTextBox zTextBox7;
		private ZArchitecture.ZTextBox textBoxPropertyValue;
		private ZArchitecture.GUI.ZDropEdit zDropEdit1;
		private ZArchitecture.ZTextBox zTextBox5;
		private ZArchitecture.ZTextBox zTextBox4;
		protected ZArchitecture.GUI.ZPanel panelFilter;
		protected ZArchitecture.GUI.ZPanel panelSort;
		private ZArchitecture.GUI.ZGroupBox groupBoxProperties;
		internal ZArchitecture.ZGrid gridProperties;
		internal Enterprise.UniversalCopy.GUI.UniversalCopyMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1;
	}
}
