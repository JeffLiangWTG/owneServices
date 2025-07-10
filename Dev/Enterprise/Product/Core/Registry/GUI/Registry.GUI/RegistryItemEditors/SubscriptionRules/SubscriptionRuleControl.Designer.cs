namespace Enterprise.Registry.GUI
{
	public partial class SubscriptionRuleControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.nodeGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nodeGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.SubscriptionRule);

			// 
			// nodeGrid
			// 
			this.nodeGrid.AllowNavigation = false;
			this.nodeGrid.AllowSorting = false;
			this.nodeGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.nodeGrid, "Nodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.SubscriptionRule)(null)).Nodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SubscriptionProperties)(((System.Collections.IList)(((Enterprise.Registry.Business.SubscriptionRule)(null)).Nodes)).SyncRoot)).MediaCategoryWithAll)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SubscriptionProperties)(((System.Collections.IList)(((Enterprise.Registry.Business.SubscriptionRule)(null)).Nodes)).SyncRoot)).MediaTypeWithAll)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SubscriptionProperties)(((System.Collections.IList)(((Enterprise.Registry.Business.SubscriptionRule)(null)).Nodes)).SyncRoot)).PublishedDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SubscriptionProperties)(((System.Collections.IList)(((Enterprise.Registry.Business.SubscriptionRule)(null)).Nodes)).SyncRoot)).PublishedSummary)));
			this.nodeGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("bcb083ad-d4aa-4751-bbbd-eb3e07062e34", "Media Category");
			zDropEditColumnStyleInfo1.ColumnName = "MediaCategoryWithAll";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.IsSortable = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("a145ad50-08cd-4ad3-846e-4d5928b0a355", "Media Type");
			zDropEditColumnStyleInfo2.ColumnName = "MediaTypeWithAll";
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zDropEditColumnStyleInfo2.IsSortable = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("526f9942-9726-4a34-af9d-5985dfb76a71", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "PublishedDescription";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsSortable = false;

			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0718685f-0185-441b-98bf-bb580d1cfd40", "Summary");
			zTextBoxColumnStyleInfo2.ColumnName = "PublishedSummary";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsSortable = false;

			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.nodeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.nodeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.nodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.nodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.nodeGrid.CopySelectedRowsAllowed = true;
			this.nodeGrid.GridId = "48dfc7ae-2ed3-4379-9fe2-a6fe20225752";
			this.nodeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.nodeGrid.LayoutKey = "subscriptionPublishedGrid";
			this.nodeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 6, true);
			this.nodeGrid.Name = "nodeGrid";
			this.nodeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 370, true);
			this.nodeGrid.TabIndex = 0;
			// 
			// SubscriptionRuleControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.nodeGrid);
			this.Name = "SubscriptionRuleControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 380, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nodeGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZGrid nodeGrid;
	}
}
