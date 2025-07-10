using System.Windows.Forms;

namespace Enterprise.UniversalCopy.GUI
{
	partial class CollectionNodeDetailsUserControl
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
			this.dropEditCopyMethod = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.calcEditOrder = new Enterprise.ZArchitecture.ZCalcEdit();
			this.tabPageElementDetails.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.dropEditCopyMethod.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabPageElementDetails
			// 
			this.tabPageElementDetails.Controls.Add(this.calcEditOrder);
			this.tabPageElementDetails.Controls.Add(this.dropEditCopyMethod);
			this.tabPageElementDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.tabPageElementDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 89, true);
			this.tabPageElementDetails.Controls.SetChildIndex(this.dropEditCopyMethod, 0);
			this.tabPageElementDetails.Controls.SetChildIndex(this.calcEditOrder, 0);
			// 
			// panelFilter
			// 
			this.panelFilter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 35, true);
			this.panelFilter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 45, true);
			// 
			// panelSort
			// 
			this.panelSort.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 26, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.UniversalCopy.Business.CollectionCopyTemplateBizo);
			// 
			// dropEditCopyMethod
			// 
			this.dropEditCopyMethod.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dropEditCopyMethod, "CopyMethodDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.UniversalCopy.Business.CollectionCopyTemplateBizo)(null)).CopyMethodDescription)));
			this.dropEditCopyMethod.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("e195e25e-f6d6-4270-8f6f-1f8574d47542", "Copy Method", "Copy method for selected element.");
			this.dropEditCopyMethod.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.dropEditCopyMethod.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 58, true);
			this.dropEditCopyMethod.Name = "dropEditCopyMethod";
			this.dropEditCopyMethod.ShowDescriptionBox = false;
			this.dropEditCopyMethod.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.dropEditCopyMethod.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 15, true);
			this.dropEditCopyMethod.TabIndex = 5;
			this.dropEditCopyMethod.UseFullWidthForCodeBox = true;
			// 
			// calcEditOrder
			// 
			this.BindingSource.SetBindingMember(this.calcEditOrder, "Order");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.UniversalCopy.Business.CollectionCopyTemplateBizo)(null)).Order)));
			this.calcEditOrder.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("0ef7b0ab-6bf8-4530-8d83-acca3a5d733c", "Order", "Processing order of current collection part");
			this.calcEditOrder.DecimalPlaces = 2;
			this.calcEditOrder.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 58, true);
			this.calcEditOrder.Name = "calcEditOrder";
			this.calcEditOrder.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 15, true);
			this.calcEditOrder.TabIndex = 6;
			this.calcEditOrder.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CollectionNodeDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "CollectionNodeDetailsUserControl";
			this.tabPageElementDetails.ResumeLayout(false);
			this.tabPageElementDetails.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.dropEditCopyMethod.ResumeLayout(true);
			this.dropEditCopyMethod.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.GUI.ZDropEdit dropEditCopyMethod;
		private Enterprise.ZArchitecture.ZCalcEdit calcEditOrder;

		#endregion
	}
}
