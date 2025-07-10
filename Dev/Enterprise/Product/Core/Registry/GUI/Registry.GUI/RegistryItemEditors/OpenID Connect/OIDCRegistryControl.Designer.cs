namespace Enterprise.Registry.GUI
{
	partial class OIDCRegistryControl
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.OptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AuthorityURLGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AuthorityURLText = new Enterprise.ZArchitecture.ZTextBox();
			this.ClaimsMappingGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ClaimsMappingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ClientIdentifierText = new Enterprise.ZArchitecture.ZTextBox();
			this.ClientIdentifierGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ScopesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ScopesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OptionGroupBox.SuspendLayout();
			this.AuthorityURLGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ClaimsMappingGrid)).BeginInit();
			this.ClaimsMappingGrid.SuspendLayout();
			this.ClaimsMappingGroupBox.SuspendLayout();
			this.ClientIdentifierGroupBox.SuspendLayout();
			this.ScopesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ScopesGrid)).BeginInit();
			this.ScopesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.OIDCConfig);
			// 
			// OptionGroupBox
			// 
			this.OptionGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("A3BD3860-FAB7-436C-B454-0D38387FC6A4", "Enable OpenID Connect Authentication");
			this.OptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
			this.OptionGroupBox.Name = "OptionGroupBox";
			this.OptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 137, true);
			this.OptionGroupBox.TabIndex = 0;
			this.OptionGroupBox.TabStop = false;
			// 
			// AuthorityURLGroupBox
			// 
			this.AuthorityURLGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("6715E2BA-8A0E-4486-B620-0DD5CD637160", "Authority URL");
			this.AuthorityURLGroupBox.Controls.Add(this.AuthorityURLText);
			this.AuthorityURLGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 141, true);
			this.AuthorityURLGroupBox.Name = "AuthorityURLGroupBox";
			this.AuthorityURLGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 39, true);
			this.AuthorityURLGroupBox.TabIndex = 10;
			this.AuthorityURLGroupBox.TabStop = false;
			// 
			// AuthorityURLText
			// 
			this.BindingSource.SetBindingMember(this.AuthorityURLText, "AuthorityURL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.OIDCConfig)(null)).AuthorityURL)));
			this.AuthorityURLText.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AuthorityURLText, false);
			this.AuthorityURLText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 16, true);
			this.AuthorityURLText.Name = "AuthorityURLText";
			this.AuthorityURLText.ShouldEscapeAllSpecialCharacters = false;
			this.AuthorityURLText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 17, true);
			this.AuthorityURLText.TabIndex = 2;
			// 
			// ClaimsMappingGrid
			// 
			this.ClaimsMappingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ClaimsMappingGrid, "ClaimsMappings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.OIDCConfig)(null)).ClaimsMappings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.OIDCClaimsMapping)(((System.Collections.IList)(((Enterprise.Registry.Business.OIDCConfig)(null)).ClaimsMappings)).SyncRoot)).ClaimName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.OIDCClaimsMapping)(((System.Collections.IList)(((Enterprise.Registry.Business.OIDCConfig)(null)).ClaimsMappings)).SyncRoot)).Identifier)));
			this.ClaimsMappingGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("57BFEA77-8080-4C95-872E-ADE71A2D6423", "Claim Name");
			zTextBoxColumnStyleInfo1.ColumnName = "ClaimName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("89794CCF-B02B-46A7-8E9E-AB86567A8294", "Identifier");
			zDropEditColumnStyleInfo1.ColumnName = "Identifier";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			this.ClaimsMappingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ClaimsMappingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ClaimsMappingGrid.CopySelectedRowsAllowed = false;
			this.ClaimsMappingGrid.GridId = "5e9fa6bf-e8fa-4aed-800c-e42c04048995";
			this.ClaimsMappingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ClaimsMappingGrid.LayoutKey = "ClaimsMappingGrid";
			this.ClaimsMappingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 17, true);
			this.ClaimsMappingGrid.Name = "ClaimsMappingGrid";
			this.ClaimsMappingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 59, true);
			this.ClaimsMappingGrid.TabIndex = 4;
			// 
			// ClaimsMappingGroupBox
			// 
			this.ClaimsMappingGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("62ED570C-5974-47A5-B5F7-01435B5776F4", "Claims Mapping");
			this.ClaimsMappingGroupBox.Controls.Add(this.ClaimsMappingGrid);
			this.ClaimsMappingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 228, true);
			this.ClaimsMappingGroupBox.Name = "ClaimsMappingGroupBox";
			this.ClaimsMappingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 83, true);
			this.ClaimsMappingGroupBox.TabIndex = 21;
			this.ClaimsMappingGroupBox.TabStop = false;
			// 
			// ClientIdentifierText
			// 
			this.BindingSource.SetBindingMember(this.ClientIdentifierText, "ClientIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.OIDCConfig)(null)).ClientIdentifier)));
			this.ClientIdentifierText.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ClientIdentifierText, false);
			this.ClientIdentifierText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 15, true);
			this.ClientIdentifierText.Name = "ClientIdentifierText";
			this.ClientIdentifierText.ShouldEscapeAllSpecialCharacters = false;
			this.ClientIdentifierText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 17, true);
			this.ClientIdentifierText.TabIndex = 3;
			// 
			// ClientIdentifierGroupBox
			// 
			this.ClientIdentifierGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("B78289AC-92F6-4BB9-BD31-015DCF5E74D3", "Client Identifier");
			this.ClientIdentifierGroupBox.Controls.Add(this.ClientIdentifierText);
			this.ClientIdentifierGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 185, true);
			this.ClientIdentifierGroupBox.Name = "ClientIdentifierGroupBox";
			this.ClientIdentifierGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 39, true);
			this.ClientIdentifierGroupBox.TabIndex = 20;
			this.ClientIdentifierGroupBox.TabStop = false;
			// 
			// ScopesGroupBox
			// 
			this.ScopesGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("9EFFA164-A1A3-4695-B4CD-F39424D6FBBC", "Additional Scopes");
			this.ScopesGroupBox.Controls.Add(this.ScopesGrid);
			this.ScopesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 315, true);
			this.ScopesGroupBox.Name = "ScopesGroupBox";
			this.ScopesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 81, true);
			this.ScopesGroupBox.TabIndex = 22;
			this.ScopesGroupBox.TabStop = false;
			// 
			// ScopesGrid
			// 
			this.ScopesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ScopesGrid, "Scopes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.OIDCConfig)(null)).Scopes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.OIDCScope)(((System.Collections.IList)(((Enterprise.Registry.Business.OIDCConfig)(null)).Scopes)).SyncRoot)).ScopeName)));
			this.ScopesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("EFAC6271-0E35-4EDF-A5C4-23BB7BC9247F", "Scope Name");
			zTextBoxColumnStyleInfo2.ColumnName = "ScopeName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.ScopesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ScopesGrid.CopySelectedRowsAllowed = false;
			this.ScopesGrid.GridId = "5e9fa6bf-e8fa-4aed-800c-e42c04048995";
			this.ScopesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ScopesGrid.LayoutKey = "ClaimsMappingGrid";
			this.ScopesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 17, true);
			this.ScopesGrid.Name = "ScopesGrid";
			this.ScopesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 60, true);
			this.ScopesGrid.TabIndex = 5;
			// 
			// OIDCRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ScopesGroupBox);
			this.Controls.Add(this.ClientIdentifierGroupBox);
			this.Controls.Add(this.ClaimsMappingGroupBox);
			this.Controls.Add(this.AuthorityURLGroupBox);
			this.Controls.Add(this.OptionGroupBox);
			this.Name = "OIDCRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 401, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OptionGroupBox.ResumeLayout(false);
			this.OptionGroupBox.PerformLayout();
			this.AuthorityURLGroupBox.ResumeLayout(false);
			this.AuthorityURLGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ClaimsMappingGrid)).EndInit();
			this.ClaimsMappingGrid.ResumeLayout(false);
			this.ClaimsMappingGrid.PerformLayout();
			this.ClaimsMappingGroupBox.ResumeLayout(false);
			this.ClaimsMappingGroupBox.PerformLayout();
			this.ClientIdentifierGroupBox.ResumeLayout(false);
			this.ClientIdentifierGroupBox.PerformLayout();
			this.ScopesGroupBox.ResumeLayout(false);
			this.ScopesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ScopesGrid)).EndInit();
			this.ScopesGrid.ResumeLayout(false);
			this.ScopesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZGroupBox OptionGroupBox;
		ZArchitecture.GUI.ZGroupBox AuthorityURLGroupBox;
		ZArchitecture.ZTextBox AuthorityURLText;
		ZArchitecture.GUI.ZGroupBox ClaimsMappingGroupBox;
		ZArchitecture.ZGrid ClaimsMappingGrid;
		ZArchitecture.ZTextBox ClientIdentifierText;
		ZArchitecture.GUI.ZGroupBox ClientIdentifierGroupBox;
		ZArchitecture.GUI.ZGroupBox ScopesGroupBox;
		ZArchitecture.ZGrid ScopesGrid;
	}
}
