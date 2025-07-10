namespace Enterprise.Customs.AU.GUI
{
	partial class ConsolidatedDeclarationDetailsUserControl
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
			this.EntryStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.ConsolidatedDeclaration);
			// 
			// EntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryNumberTextBox, "DeclarationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ConsolidatedDeclaration)(null)).DeclarationNumber)));
			this.EntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryNumberTextBox.Name = "EntryNumberTextBox";
			this.EntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 15, true);
			this.EntryNumberTextBox.TabIndex = 0;
			// 
			// EntryStatusTextBox
			// 
			this.BindingSource.SetBindingMember(EntryStatusTextBox, "CustomsStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ConsolidatedDeclaration)(null)).CustomsStatusDescription)));
			this.EntryStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 0, true);
			this.EntryStatusTextBox.Name = "EntryStatusTextBox";
			this.EntryStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 15, true);
			this.EntryStatusTextBox.TabIndex = 1;
			// 
			// MessageStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "MessageStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ConsolidatedDeclaration)(null)).MessageStatusDescription)));
			this.MessageStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 0, true);
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 15, true);
			this.MessageStatusTextBox.TabIndex = 2;
			// 
			// ConsolidatedDeclarationDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.EntryNumberTextBox);
			this.Controls.Add(this.EntryStatusTextBox);
			this.Controls.Add(this.MessageStatusTextBox);
			this.Name = "ConsolidatedDeclarationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(715, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZTextBox EntryStatusTextBox;
		public ZArchitecture.ZTextBox EntryNumberTextBox;
		internal ZArchitecture.ZTextBox MessageStatusTextBox;
	}
}
