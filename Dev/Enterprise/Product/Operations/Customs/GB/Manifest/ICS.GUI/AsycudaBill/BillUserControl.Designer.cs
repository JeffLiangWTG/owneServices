using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.ICS.GUI
{
	partial class BillUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SpecialMentionsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SpecialMentionsDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(EU.Manifest.Business.AsycudaBill);
			//
			// SpecialMentionsDropEdit
			//
			this.SpecialMentionsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SpecialMentionsDropEdit, "SpecialMentions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((EU.Manifest.Business.AsycudaBill)(null)).SpecialMentions)));
			this.SpecialMentionsDropEdit.CaptionResourceString = Enterprise.Customs.GB.ICS.GUI.Res.GetData("84AAB48C-D877-482C-A9C0-25A7AC781119", "Special Mentions");
			this.SpecialMentionsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 4, true);
			this.SpecialMentionsDropEdit.Name = "SpecialMentionsDropEdit";
			this.SpecialMentionsDropEdit.PreBoundMaxLength = 5;
			this.SpecialMentionsDropEdit.ShowDescriptionBox = true;
			this.SpecialMentionsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 20, true);
			this.SpecialMentionsDropEdit.TabIndex = 2;
			// 
			// SGBillSpecificUserControl
			// 
			this.Controls.Add(this.SpecialMentionsDropEdit);
			this.Name = "BillUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 55, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SpecialMentionsDropEdit.ResumeLayout(true);
			this.SpecialMentionsDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZDropEdit SpecialMentionsDropEdit;
	}
}
