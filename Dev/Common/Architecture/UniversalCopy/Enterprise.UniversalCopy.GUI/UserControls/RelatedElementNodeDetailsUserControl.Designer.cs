using System.Windows.Forms;

namespace Enterprise.UniversalCopy.GUI
{
	partial class RelatedElementNodeDetailsUserControl
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
			this.tabPageElementDetails.SuspendLayout();
			this.tabPageElementDetails.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.dropEditCopyMethod.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabPageElementDetails
			// 
			this.tabPageElementDetails.Controls.Add(this.dropEditCopyMethod);
			this.tabPageElementDetails.Controls.SetChildIndex(this.dropEditCopyMethod, 0);
			// 
			// panelFilter
			// 
			this.panelFilter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 35, true);
			this.panelFilter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 45, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.UniversalCopy.Business.RelatedEntityCopyTemplateBizo);
			// 
			// dropEditCopyMethod
			// 
			this.dropEditCopyMethod.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dropEditCopyMethod, "CopyMethodDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.UniversalCopy.Business.RelatedEntityCopyTemplateBizo)(null)).CopyMethodDescription)));
			this.dropEditCopyMethod.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("e195e25e-f6d6-4270-8f6f-1f8574d47542", "Copy Method", "Copy method for selected element.");
			this.dropEditCopyMethod.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 57, true);
			this.dropEditCopyMethod.Name = "dropEditCopyMethod";
			this.dropEditCopyMethod.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.dropEditCopyMethod.TabIndex = 5;
			this.dropEditCopyMethod.CharacterCasing = CharacterCasing.Normal;
			this.dropEditCopyMethod.ShowDescriptionBox = false;
			this.dropEditCopyMethod.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.dropEditCopyMethod.UseFullWidthForCodeBox = true;
			// 
			// RelatedElementNodeDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "RelatedElementNodeDetailsUserControl";
			this.tabPageElementDetails.ResumeLayout(false);
			this.tabPageElementDetails.PerformLayout();
			this.tabPageElementDetails.ResumeLayout(false);
			this.tabPageElementDetails.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.dropEditCopyMethod.ResumeLayout(true);
			this.dropEditCopyMethod.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.GUI.ZDropEdit dropEditCopyMethod;

		#endregion
	}
}
