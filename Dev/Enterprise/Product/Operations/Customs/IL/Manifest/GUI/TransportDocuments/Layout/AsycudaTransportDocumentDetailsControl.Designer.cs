using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public partial class AsycudaTransportDocumentDetailsControl
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
			this.typeDropEdit = new ZDropEdit();
			this.referenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.typeDropEdit.SuspendLayout();
			this.referenceTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Manifest.Business.AsycudaBill);
			// 
			// typeTextBox
			// 
			this.typeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.typeDropEdit, "TransportDocuments.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IL.Manifest.Business.AsycudaTransportDocumentInfo)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).TransportDocuments)).SyncRoot)).CSI_Code)));
			this.typeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 16, true);
			this.typeDropEdit.Name = "typeDropEdit";
			this.typeDropEdit.PreBoundMaxLength = 5;
			this.typeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.typeDropEdit.TabIndex = 0;
			// 
			// referenceTextBox
			// 
			this.referenceTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.referenceTextBox, "TransportDocuments.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IL.Manifest.Business.AsycudaTransportDocumentInfo)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).TransportDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.referenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 42, true);
			this.referenceTextBox.Name = "referenceTextBox";
			this.referenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.referenceTextBox.TabIndex = 1;
			// 
			// AsycudaTransportDocumentDetailsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.typeDropEdit);
			this.Controls.Add(this.referenceTextBox);
			this.Name = "AsycudaTransportDocumentDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1118, 421, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.referenceTextBox.ResumeLayout(true);
			this.referenceTextBox.PerformLayout();
			this.typeDropEdit.ResumeLayout(true);
			this.typeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZDropEdit typeDropEdit;
		internal ZArchitecture.ZTextBox referenceTextBox;
	}
}
