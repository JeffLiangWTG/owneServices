namespace Enterprise.Registry.GUI
{
	partial class EventsVisibilityRegistryControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.EventsVisibilityGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EventsVisibilityGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.EventVisibility);
			// 
			// EventsVisibilityGrid
			// 
			this.EventsVisibilityGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EventsVisibilityGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.EventVisibility)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.EventVisibility)(null)).EventCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.EventVisibility)(null)).EventDescription)));
			this.EventsVisibilityGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("012c45bb-47a7-4a24-a550-c36c78d68a1f", "Event");
			zDropEditColumnStyleInfo1.ColumnName = "EventCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0cc2dc79-5ba4-42ab-a2b7-12e9c125b383", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "EventDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.EventsVisibilityGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EventsVisibilityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EventsVisibilityGrid.GridId = "caf0130d-b57c-4422-8692-9874b5bb7d12";
			this.EventsVisibilityGrid.CopySelectedRowsAllowed = true;
			this.EventsVisibilityGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EventsVisibilityGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EventsVisibilityGrid.LayoutKey = "EventsVisibilityGrid";
			this.EventsVisibilityGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EventsVisibilityGrid.Name = "EventsVisibilityGrid";
			this.EventsVisibilityGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 439, true);
			this.EventsVisibilityGrid.TabIndex = 0;
			// 
			// EventsVisibilityRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EventsVisibilityGrid);
			this.Name = "EventsVisibilityRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 439, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EventsVisibilityGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected ZArchitecture.ZGrid EventsVisibilityGrid;
	}
}
