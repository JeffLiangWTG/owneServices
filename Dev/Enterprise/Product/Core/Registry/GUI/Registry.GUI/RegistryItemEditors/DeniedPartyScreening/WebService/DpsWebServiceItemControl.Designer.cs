namespace Enterprise.Registry.GUI
{
	partial class DpsWebServiceItemControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.DpsWebServiceItemGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DpsWebServiceItemGrid)).BeginInit();
			this.DpsWebServiceItemGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.DpsWebServiceItemCollection);
			// 
			// DpsWebServiceItemGrid
			// 
			this.DpsWebServiceItemGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DpsWebServiceItemGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.DpsWebServiceItem)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DpsWebServiceItem)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DpsWebServiceItem)(null)).WebServiceUrl)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DpsWebServiceItem)(null)).Role)));
			this.DpsWebServiceItemGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("01CA5722-0C14-41B1-BBA0-1E81C6A11AD1", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AF15945D-C147-400E-9518-3218EE608412", "Web Service URL");
			zTextBoxColumnStyleInfo2.ColumnName = "WebServiceUrl";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			zDropEditColumnStyleInfo1.ColumnName = "Role";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DpsWebServiceItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DpsWebServiceItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DpsWebServiceItemGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DpsWebServiceItemGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DpsWebServiceItemGrid.GridId = "EF789ACE-79E6-4788-96C7-91FBC75325AE";
			this.DpsWebServiceItemGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DpsWebServiceItemGrid.LayoutKey = "DpsWebServiceItemGrid";
			this.DpsWebServiceItemGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DpsWebServiceItemGrid.Name = "DpsWebServiceItemGrid";
			this.DpsWebServiceItemGrid.ReadOnly = true;
			this.DpsWebServiceItemGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 400, true);
			this.DpsWebServiceItemGrid.TabIndex = 2;
			// 
			// DpsWebServiceItemControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DpsWebServiceItemGrid);
			this.Name = "DpsWebServiceItemControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 400, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DpsWebServiceItemGrid)).EndInit();
			this.DpsWebServiceItemGrid.ResumeLayout(false);
			this.DpsWebServiceItemGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal Enterprise.ZArchitecture.ZGrid DpsWebServiceItemGrid;

		#endregion
	}
}
