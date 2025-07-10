namespace Enterprise.Registry.GUI
{
	partial class ShippingPortsMessagingEHubIDControl
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
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.ShippingPortsMessagingEHubIDGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ShippingPortsMessagingEHubIDGrid)).BeginInit();
            this.ShippingPortsMessagingEHubIDGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ShippingPortsMessagingEHubIDCollection);
            // 
            // ShippingPortsMessagingEHubIDGrid
            // 
            this.ShippingPortsMessagingEHubIDGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.ShippingPortsMessagingEHubIDGrid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ShippingPortsMessagingEHubID)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ShippingPortsMessagingEHubID)(null)).Port)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ShippingPortsMessagingEHubID)(null)).Module)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ShippingPortsMessagingEHubID)(null)).RecipientID)));
            this.ShippingPortsMessagingEHubIDGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("687d67ba-cf9a-4ba5-8cad-79b64c2a986e", "Port");
            zDropEditColumnStyleInfo1.ColumnName = "Port";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("127701ec-91da-4efa-8ed4-25c79e00bafb", "Module");
            zDropEditColumnStyleInfo2.ColumnName = "Module";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("9aaa77fc-d566-4a56-941e-8f8355edcd77", "Recipient ID");
            zTextBoxColumnStyleInfo1.ColumnName = "RecipientID";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.ShippingPortsMessagingEHubIDGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.ShippingPortsMessagingEHubIDGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.ShippingPortsMessagingEHubIDGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.ShippingPortsMessagingEHubIDGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ShippingPortsMessagingEHubIDGrid.GridId = "f9be4827-e4c5-4fe6-a388-7eb1031f5ba1";
            this.ShippingPortsMessagingEHubIDGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ShippingPortsMessagingEHubIDGrid.LayoutKey = "ShippingPortsMessagingEHubIDGrid";
            this.ShippingPortsMessagingEHubIDGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ShippingPortsMessagingEHubIDGrid.Name = "ShippingPortsMessagingEHubIDGrid";
            this.ShippingPortsMessagingEHubIDGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 372, true);
            this.ShippingPortsMessagingEHubIDGrid.TabIndex = 0;
            // 
            // ShippingPortsMessagingEHubIDControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ShippingPortsMessagingEHubIDGrid);
            this.Name = "ShippingPortsMessagingEHubIDControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 372, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ShippingPortsMessagingEHubIDGrid)).EndInit();
            this.ShippingPortsMessagingEHubIDGrid.ResumeLayout(false);
            this.ShippingPortsMessagingEHubIDGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid ShippingPortsMessagingEHubIDGrid;
	}
}
