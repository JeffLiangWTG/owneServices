using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.FR.GUI
{
	partial class UCC6EntryLineCalculationResultsControl
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
			this.CalculationResultsKSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.FeesCalculatedByCWGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FeesCalculatedByCWGrid = new Enterprise.ZArchitecture.ZGrid();
			this.FeesConfirmedByCustomsBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FeesConfirmedByCustomsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CalculationResultsKSplitContainer)).BeginInit();
			this.CalculationResultsKSplitContainer.Panel1.SuspendLayout();
			this.CalculationResultsKSplitContainer.Panel2.SuspendLayout();
			this.CalculationResultsKSplitContainer.SuspendLayout();
			this.FeesCalculatedByCWGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FeesCalculatedByCWGrid)).BeginInit();
			this.FeesCalculatedByCWGrid.SuspendLayout();
			this.FeesConfirmedByCustomsBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FeesConfirmedByCustomsGrid)).BeginInit();
			this.FeesConfirmedByCustomsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.AllCusEntryLineCollection<Enterprise.Customs.FR.Business.Declaration.CusEntryLine>);
			// 
			// CalculationResultsKSplitContainer
			// 
			this.CalculationResultsKSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CalculationResultsKSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CalculationResultsKSplitContainer.Name = "CalculationResultsKSplitContainer";
			// 
			// CalculationResultsKSplitContainer.Panel1
			// 
			this.CalculationResultsKSplitContainer.Panel1.Controls.Add(this.FeesCalculatedByCWGroupBox);
			// 
			// CalculationResultsKSplitContainer.Panel2
			// 
			this.CalculationResultsKSplitContainer.Panel2.Controls.Add(this.FeesConfirmedByCustomsBox);
			this.CalculationResultsKSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 224, true);
			this.CalculationResultsKSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.CalculationResultsKSplitContainer.SplitterWidth = 7;
			this.CalculationResultsKSplitContainer.TabIndex = 0;
			// 
			// FeesCalculatedByCWGroupBox
			// 
			this.FeesCalculatedByCWGroupBox.Controls.Add(this.FeesCalculatedByCWGrid);
			this.FeesCalculatedByCWGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FeesCalculatedByCWGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FeesCalculatedByCWGroupBox.Name = "FeesCalculatedByCWGroupBox";
			this.FeesCalculatedByCWGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 224, true);
			this.FeesCalculatedByCWGroupBox.TabIndex = 0;
			this.FeesCalculatedByCWGroupBox.TabStop = false;
			this.FeesCalculatedByCWGroupBox.Text = "Fees Calculated By CW";
			// 
			// FeesCalculatedByCWGrid
			// 
			this.FeesCalculatedByCWGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.FeesCalculatedByCWGrid, "CusEntryLineCalculatedFees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.CusEntryLine)(null)).CusEntryLineCalculatedFees)));
			this.FeesCalculatedByCWGrid.CaptionVisible = false;
			this.FeesCalculatedByCWGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FeesCalculatedByCWGrid.GridId = "43acf4d0-37d7-4b71-8e39-6c6817512a7a";
			this.FeesCalculatedByCWGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FeesCalculatedByCWGrid.LayoutKey = "FeesCalculatedByCWGrid";
			this.FeesCalculatedByCWGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.FeesCalculatedByCWGrid.Name = "FeesCalculatedByCWGrid";
			this.FeesCalculatedByCWGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.FeesCalculatedByCWGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 207, true);
			this.FeesCalculatedByCWGrid.TabIndex = 1;
			// 
			// FeesConfirmedByCustomsBox
			// 
			this.FeesConfirmedByCustomsBox.Controls.Add(this.FeesConfirmedByCustomsGrid);
			this.FeesConfirmedByCustomsBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FeesConfirmedByCustomsBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FeesConfirmedByCustomsBox.Name = "FeesConfirmedByCustomsBox";
			this.FeesConfirmedByCustomsBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 224, true);
			this.FeesConfirmedByCustomsBox.TabIndex = 0;
			this.FeesConfirmedByCustomsBox.TabStop = false;
			this.FeesConfirmedByCustomsBox.Text = "Fees Confirmed By Customs";
			// 
			// FeesConfirmedByCustomsGrid
			// 
			this.FeesConfirmedByCustomsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.FeesConfirmedByCustomsGrid, "CusEntryLineConfirmedFees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.CusEntryLine)(null)).CusEntryLineConfirmedFees)));
			this.FeesConfirmedByCustomsGrid.CaptionVisible = false;
			this.FeesConfirmedByCustomsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FeesConfirmedByCustomsGrid.GridId = "1e21619f-3df2-4e33-84d6-5ddda8124df9";
			this.FeesConfirmedByCustomsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FeesConfirmedByCustomsGrid.LayoutKey = "FeesConfirmedByCustomsGrid";
			this.FeesConfirmedByCustomsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.FeesConfirmedByCustomsGrid.Name = "FeesConfirmedByCustomsGrid";
			this.FeesConfirmedByCustomsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 207, true);
			this.FeesConfirmedByCustomsGrid.TabIndex = 1;
			// 
			// UCC6EntryLineCalculationResultsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CalculationResultsKSplitContainer);
			this.Name = "UCC6EntryLineCalculationResultsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 224, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CalculationResultsKSplitContainer.Panel1.ResumeLayout(false);
			this.CalculationResultsKSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CalculationResultsKSplitContainer)).EndInit();
			this.CalculationResultsKSplitContainer.ResumeLayout(false);
			this.CalculationResultsKSplitContainer.PerformLayout();
			this.FeesCalculatedByCWGroupBox.ResumeLayout(false);
			this.FeesCalculatedByCWGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FeesCalculatedByCWGrid)).EndInit();
			this.FeesCalculatedByCWGrid.ResumeLayout(false);
			this.FeesCalculatedByCWGrid.PerformLayout();
			this.FeesConfirmedByCustomsBox.ResumeLayout(false);
			this.FeesConfirmedByCustomsBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FeesConfirmedByCustomsGrid)).EndInit();
			this.FeesConfirmedByCustomsGrid.ResumeLayout(false);
			this.FeesConfirmedByCustomsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer CalculationResultsKSplitContainer;
		private ZArchitecture.GUI.ZGroupBox FeesCalculatedByCWGroupBox;
		private ZArchitecture.ZGrid FeesCalculatedByCWGrid;
		private ZArchitecture.GUI.ZGroupBox FeesConfirmedByCustomsBox;
		private ZArchitecture.ZGrid FeesConfirmedByCustomsGrid;
	}
}
