using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class CcsukShipmentAndHawbLinkerControl
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
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.textBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.buttonMakeLinks = new ZButton();
			this.cancelButton = new ZButton();
			this.buttonOldMethod = new ZButton();
			this.label1 = new Enterprise.ZArchitecture.ZLabel();
			this.checkBoxRemoveAllSplitsFromBasic = new ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher.NonPersistentShipmentToHawbMatcherHeader);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGrid1, "Pivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher.NonPersistentShipmentToHawbMatcherHeader)(null)).Pivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher.NonPersistentShipmentToHawbMatcherLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher.NonPersistentShipmentToHawbMatcherHeader)(null)).Pivots)).SyncRoot)).JS)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher.NonPersistentShipmentToHawbMatcherLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher.NonPersistentShipmentToHawbMatcherHeader)(null)).Pivots)).SyncRoot)).ShipmentHouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher.NonPersistentShipmentToHawbMatcherLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher.NonPersistentShipmentToHawbMatcherHeader)(null)).Pivots)).SyncRoot)).CS)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher.NonPersistentShipmentToHawbMatcherLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher.NonPersistentShipmentToHawbMatcherHeader)(null)).Pivots)).SyncRoot)).CreateNewHawb)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher.NonPersistentShipmentToHawbMatcherLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher.NonPersistentShipmentToHawbMatcherHeader)(null)).Pivots)).SyncRoot)).HawbNumber)));
			this.zGrid1.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo3.ColumnName = "JS";
			zGuidDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(104);
			zTextBoxColumnStyleInfo3.ColumnName = "ShipmentHouseBill";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(159);
			zGuidDropEditColumnStyleInfo4.ColumnName = "CS";
			zGuidDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zCheckBoxColumnStyleInfo2.ColumnName = "CreateNewHawb";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo4.ColumnName = "HawbNumber";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114);
			this.zGrid1.ColumnStyles.Add(zGuidDropEditColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zGuidDropEditColumnStyleInfo4);
			this.zGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.zGrid1.GridId = "07bd4c8e-aa88-42e5-b302-f0958fa18a1d";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 121, true);
			this.zGrid1.TabIndex = 0;

			this.mawbDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.label2 = new Enterprise.ZArchitecture.ZLabel();
			this.Controls.Add(this.mawbDropEdit);
			this.Controls.Add(this.label2);

			// 
			// mawbDropEdit
			// 
			this.mawbDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.mawbDropEdit, "SelectedMawbWrapperGUID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher.NonPersistentShipmentToHawbMatcherHeader)(null)).SelectedMawbWrapperGUID)));
			this.mawbDropEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("804741a2-5869-4b74-90f4-210dfcd79521", "MAWB");
			this.mawbDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 7, true);
			this.mawbDropEdit.Name = "mawbDropEdit";
			this.mawbDropEdit.ShouldResizeByMaxLength = true;
			this.mawbDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 17, true);
			CargoWise.Windows.UI.ControlDpiScalingHelper.SetWidth(this.mawbDropEdit.CodeBox, 185, true);
			this.mawbDropEdit.TabIndex = 0;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("24336e21-77a2-47d8-9572-a7c460a17f33", "MAWB");
			this.label2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 9, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 13, true);
			this.label2.TabIndex = 8;
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.textBox1, "Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher.NonPersistentShipmentToHawbMatcherHeader)(null)).Status)));
			this.textBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 174, true);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 109, true);
			this.textBox1.TabIndex = 2;
			this.textBox1.TabStop = false;
			// 
			// buttonMakeLinks
			// 
			this.buttonMakeLinks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonMakeLinks.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("aa27e9ff-dc23-41e7-bc2a-5829273bfd1c", "Link shipments to HAWBs");
			this.buttonMakeLinks.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 174);
			this.buttonMakeLinks.Name = "buttonMakeLinks";
			this.buttonMakeLinks.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 23);
			this.buttonMakeLinks.TabIndex = 3;
			this.buttonMakeLinks.UseVisualStyleBackColor = true;
			this.buttonMakeLinks.Click += new System.EventHandler(this.buttonMakeLinks_Click);
			// 
			// buttonOldMethod
			// 
			this.buttonOldMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOldMethod.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("a8bb5735-13a8-4888-a893-fa84d4dfc760", "Link using old method");
			this.buttonOldMethod.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 199);
			this.buttonOldMethod.Name = "buttonOldMethod";
			this.buttonOldMethod.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 23);
			this.buttonOldMethod.TabIndex = 4;
			this.buttonOldMethod.UseVisualStyleBackColor = true;
			this.buttonOldMethod.Click += new System.EventHandler(this.buttonOldMethod_Click);

			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("70530dcd-ffb8-46b4-b091-ad9c512a7e74", "Cancel (create/link none)");
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 225);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 23);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.label1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("b69b1ff9-f974-4a33-bb13-a9e80d2a9629", "Proposed links:");
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 148, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 14, true);
			this.label1.TabIndex = 1;
			//
			// checkBoxRemoveAllSplitsFromBasic
			//
			this.checkBoxRemoveAllSplitsFromBasic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.checkBoxRemoveAllSplitsFromBasic, "DeleteAnyExistingLocalSplitOnBasicIfStatusISR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher.NonPersistentShipmentToHawbMatcherHeader)(null)).DeleteAnyExistingLocalSplitOnBasicIfStatusISR)));
			this.checkBoxRemoveAllSplitsFromBasic.AutoSize = true;
			this.checkBoxRemoveAllSplitsFromBasic.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("00ed5071-9f69-4cb4-b5c6-cfc0321061e4", "Remove all splits from basic if necessary");
			this.checkBoxRemoveAllSplitsFromBasic.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 300, true);
			this.checkBoxRemoveAllSplitsFromBasic.Name = "checkBoxRemoveAllSplitsFromBasic";
			this.checkBoxRemoveAllSplitsFromBasic.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 6, true);
			this.checkBoxRemoveAllSplitsFromBasic.TabIndex = 6;
			//
			// CcsukShipmentAndHawbLinkerControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonOldMethod);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.buttonMakeLinks);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.checkBoxRemoveAllSplitsFromBasic);
			this.Controls.Add(this.zGrid1);
			this.MinimumSize = this.Size;
			this.Name = "CcsukShipmentAndHawbLinkerControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 386, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 386, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGuidDropEdit mawbDropEdit;
		private ZLabel label2;

		private ZArchitecture.ZGrid zGrid1;
		private ZTextBox textBox1;
		private ZButton buttonMakeLinks;
		private ZButton cancelButton;
		private ZButton buttonOldMethod;
		private ZLabel label1;
		private ZCheckBox checkBoxRemoveAllSplitsFromBasic;

	}
}
