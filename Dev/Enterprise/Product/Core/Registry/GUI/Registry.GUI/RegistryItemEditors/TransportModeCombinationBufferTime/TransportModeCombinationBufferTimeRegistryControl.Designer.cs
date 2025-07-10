namespace Enterprise.Registry.GUI
{
	partial class TransportModeCombinationBufferTimeRegistryControl
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
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            this.TransportModeCombinationBufferTimeGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TransportModeCombinationBufferTimeGrid)).BeginInit();
            this.TransportModeCombinationBufferTimeGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.TransportModeCombinationBufferTimeCollection);
            // 
            // TransportModeCombinationBufferTimeGrid
            // 
            this.TransportModeCombinationBufferTimeGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.TransportModeCombinationBufferTimeGrid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.TransportModeCombinationBufferTime)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.TransportModeCombinationBufferTime)(null)).LoadTransportMode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.TransportModeCombinationBufferTime)(null)).UnloadTransportMode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.TransportModeCombinationBufferTime)(null)).BufferTimeInHours)));
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("TransportModeCombinationBufferTimeRegistryControl|cb7f7e11-2d36-4773-bdf4-24bbded7df52", "Transport mode (Unload)");
            zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo1.ColumnName = "UnloadTransportMode";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("TransportModeCombinationBufferTimeRegistryControl|3e8a48fd-6964-4bfa-9d71-1367bb7-552f7", "Transport mode (Load)");
            zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo2.ColumnName = "LoadTransportMode";
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("TransportModeCombinationBufferTimeRegistryControl|e3fa61d4-9702-4496-9d7f-6cc3a1c-1f75b", "Buffer time (Hours)");
            zCalcEditColumnStyleInfo1.ColumnName = "BufferTimeInHours";
            zCalcEditColumnStyleInfo1.Decimals = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.TransportModeCombinationBufferTimeGrid.CaptionVisible = false;
			this.TransportModeCombinationBufferTimeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TransportModeCombinationBufferTimeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.TransportModeCombinationBufferTimeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.TransportModeCombinationBufferTimeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TransportModeCombinationBufferTimeGrid.GridId = "b1774e3a-ca8d-472d-81b1-4824dee6c686";
            this.TransportModeCombinationBufferTimeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.TransportModeCombinationBufferTimeGrid.LayoutKey = "TransportModeCombinationBufferTimeGrid";
            this.TransportModeCombinationBufferTimeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.TransportModeCombinationBufferTimeGrid.Name = "TransportModeCombinationBufferTimeGrid";
            this.TransportModeCombinationBufferTimeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 400, true);
            this.TransportModeCombinationBufferTimeGrid.TabIndex = 0;
            // 
            // TransportModeCombinationBufferTimeRegistryControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoScroll = true;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.TransportModeCombinationBufferTimeGrid);
            this.Name = "TransportModeCombinationBufferTimeRegistryControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 400, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TransportModeCombinationBufferTimeGrid)).EndInit();
            this.TransportModeCombinationBufferTimeGrid.ResumeLayout(false);
            this.TransportModeCombinationBufferTimeGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid TransportModeCombinationBufferTimeGrid;
	}
}
