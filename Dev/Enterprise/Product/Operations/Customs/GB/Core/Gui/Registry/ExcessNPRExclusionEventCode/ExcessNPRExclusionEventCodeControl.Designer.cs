using Enterprise.Customs.GB.Registry;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GB.GUI.Registry
{
	partial class ExcessNPRExclusionEventCodeControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
			this.NPRExclusionEventCodesGrid = new ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NPRExclusionEventCodesGrid)).BeginInit();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(ExcessNPRExclusionEventCodeSetting);
			//
			// NPRExclusionEventCodesGrid
			//
			this.NPRExclusionEventCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NPRExclusionEventCodesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Registry.ExcessNPRExclusionEventCodeSetting)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Registry.ExcessNPRExclusionEventCodeSetting)(null)).EventCodesList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.ExcessNPRExclusionEventCodeSetting)(null)).EventCode)));
			//zDropEditColumnStyleInfo1.BindToList = "EventCodesList";
			//zDropEditColumnStyleInfo1.Caption = "Event Code";
			//zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("def9b80d-9e1b-45de-b32c-b2a89795e668", "Event Code");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "EventCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.NPRExclusionEventCodesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.NPRExclusionEventCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NPRExclusionEventCodesGrid.GridId = "c3e6dec3-9fd8-41e6-8def-c25aab6efd60";
			this.NPRExclusionEventCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NPRExclusionEventCodesGrid.LayoutKey = "BadgeCodesGrid";
			this.NPRExclusionEventCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NPRExclusionEventCodesGrid.Name = "BadgeCodesGrid";
			this.NPRExclusionEventCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			this.NPRExclusionEventCodesGrid.TabIndex = 0;
			//
			// ExcessNPRExclusionEventCodeControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NPRExclusionEventCodesGrid);
			this.Name = "ExcessNPRExclusionEventCodeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NPRExclusionEventCodesGrid)).EndInit();
			this.NPRExclusionEventCodesGrid.ResumeLayout(false);
			this.NPRExclusionEventCodesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		}

		#endregion

		public ZGrid NPRExclusionEventCodesGrid;
	}
}
