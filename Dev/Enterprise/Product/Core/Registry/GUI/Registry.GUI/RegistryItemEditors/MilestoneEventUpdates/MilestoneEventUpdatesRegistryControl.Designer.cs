using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class MilestoneEventUpdatesRegistryControl
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
			this.MilestoneEventUpdatesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MilestoneEventUpdatesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.MilestoneEventUpdates);
			// 
			// MilestoneEventUpdatesGrid
			// 
			this.MilestoneEventUpdatesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MilestoneEventUpdatesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.MilestoneEventUpdates)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.MilestoneEventUpdates)(null)).EventType)));
			this.MilestoneEventUpdatesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = "";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("40a20359-b701-4199-b988-1c4fed4ae1c4", "Event");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "EventType";
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MilestoneEventUpdatesGrid.CopySelectedRowsAllowed = true;
			this.MilestoneEventUpdatesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MilestoneEventUpdatesGrid.GridId = "caf0130d-b57c-4422-8692-9874b5bb7d12";
			this.MilestoneEventUpdatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MilestoneEventUpdatesGrid.LayoutKey = "MilestoneEventUpdatesGrid";
			this.MilestoneEventUpdatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MilestoneEventUpdatesGrid.Name = "MilestoneEventUpdatesGrid";
			this.MilestoneEventUpdatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 439, true);
			this.MilestoneEventUpdatesGrid.TabIndex = 0;
			// 
			// MilestoneEventUpdatesRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MilestoneEventUpdatesGrid);
			this.Name = "MilestoneEventUpdatesRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 439, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MilestoneEventUpdatesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected ZArchitecture.ZGrid MilestoneEventUpdatesGrid;

	}
}
