namespace Enterprise.Customs.DE.GUI.Registry
{
	partial class ExportStatusRequestRecipientRegistryItemControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MessageRecipientGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageRecipientGrid)).BeginInit();
			this.MessageRecipientGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Registry.ExportStatusRequestRecipientRegistry);
			// 
			// MessageRecipientGrid
			// 
			this.MessageRecipientGrid.AllowNavigation = false;
			this.MessageRecipientGrid.AllowReadOnlyToModifyTabStop = true;
			this.MessageRecipientGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MessageRecipientGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Registry.ExportStatusRequestRecipientRegistry)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Registry.ExportStatusRequestRecipientRegistry)(null)).SystemCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Registry.ExportStatusRequestRecipientRegistry)(null)).MessageRecipient)));
			this.MessageRecipientGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("ed0ba490-1970-45a8-92f8-9892b3be0f28", "System");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "SystemCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("08141eb4-fffd-46b2-a256-0bfa1571af55", "Message Recipient");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "MessageRecipient";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(131);
			this.MessageRecipientGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessageRecipientGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessageRecipientGrid.GridId = "80C3FAFC-3E06-45F3-AA0F-89FE642E034B";
			this.MessageRecipientGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessageRecipientGrid.LayoutKey = "MessageRecipientGrid";
			this.MessageRecipientGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageRecipientGrid.Name = "MessageRecipientGrid";
			this.MessageRecipientGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 100, true);
			this.MessageRecipientGrid.TabIndex = 0;
			// 
			// ExportStatusRequestRecipientRegistryItemControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MessageRecipientGrid);
			this.Name = "ExportStatusRequestRecipientRegistryItemControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageRecipientGrid)).EndInit();
			this.MessageRecipientGrid.ResumeLayout(false);
			this.MessageRecipientGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid MessageRecipientGrid;
	}
}
