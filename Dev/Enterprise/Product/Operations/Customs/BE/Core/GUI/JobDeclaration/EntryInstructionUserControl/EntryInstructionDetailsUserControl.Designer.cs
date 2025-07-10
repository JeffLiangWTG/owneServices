namespace Enterprise.Customs.BE.GUI
{
	partial class EntryInstructionDetailsUserControl
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
			PreviousDocumentsTabPage.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("77b2fecb-f6c7-4164-99a0-3ebc5017908r", "[UCC 2/1] Previous documents");
			SupportingDocumentsTabPage.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("77b2fecb-f6c7-4164-99a0-3ebc5017908t", "[UCC 2/3] Supporting documents");

			this.SupplyChainActorReferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupplyChainActorReferencesUserControl = new Enterprise.Customs.EU.GUI.PlugIn.SupplyChainActorReferencesUserControl();
			this.EntryInstructionTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupplyChainActorReferencesTabPage.SuspendLayout();
			this.SupplyChainActorReferencesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// SupplyChainActorReferencesTabPage
			// 
			this.SupplyChainActorReferencesTabPage.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("507D0942-2FE7-4C84-8483-5646E8D3FC45", "Add. Supply Chain Actor");
			this.SupplyChainActorReferencesTabPage.Controls.Add(this.SupplyChainActorReferencesUserControl);
			this.SupplyChainActorReferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SupplyChainActorReferencesTabPage.Name = "SupplyChainActorReferencesTabPage";
			this.SupplyChainActorReferencesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SupplyChainActorReferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.SupplyChainActorReferencesTabPage.TabIndex = 4;
			this.SupplyChainActorReferencesTabPage.UseVisualStyleBackColor = true;
			// 
			// SupplyChainActorReferencesUserControl
			// 
			this.SupplyChainActorReferencesUserControl.AllowDrop = true;
			this.SupplyChainActorReferencesUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.SupplyChainActorReferencesUserControl, "CustomsEntryInstructions.CusSupplyChainActorReferences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.Declaration.ICusSupplyChainActorReferenceCollection<Enterprise.Customs.EU.Business.Declaration.CusSupplyChainActorReference>)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CusSupplyChainActorReferences)));
			this.SupplyChainActorReferencesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupplyChainActorReferencesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SupplyChainActorReferencesUserControl.Name = "SupplyChainActorReferencesUserControl";
			this.SupplyChainActorReferencesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1323, 399, true);
			this.SupplyChainActorReferencesUserControl.TabIndex = 1;
			// 
			// EntryInstructionDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "ImportEntryInstructionDetailsUserControl";
			this.EntryInstructionTabControl.ResumeLayout(false);
			this.EntryInstructionTabControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupplyChainActorReferencesTabPage.ResumeLayout(false);
			this.SupplyChainActorReferencesTabPage.PerformLayout();
			this.SupplyChainActorReferencesUserControl.ResumeLayout(true);
			this.SupplyChainActorReferencesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZTabPage SupplyChainActorReferencesTabPage;
		private EU.GUI.PlugIn.SupplyChainActorReferencesUserControl SupplyChainActorReferencesUserControl;
	}
}
