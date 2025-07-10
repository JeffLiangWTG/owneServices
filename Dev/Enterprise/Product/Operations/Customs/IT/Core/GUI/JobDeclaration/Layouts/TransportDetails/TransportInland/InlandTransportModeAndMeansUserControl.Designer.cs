namespace Enterprise.Customs.IT.GUI
{
	partial class InlandTransportModeAndMeansUserControl
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
			this.InlandModeOfTransportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InlandTransportMeansDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InlandModeOfTransportDropEdit.SuspendLayout();
			this.InlandTransportMeansDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobDeclaration);
			// 
			// InlandModeOfTransportDropEdit
			// 
			this.InlandModeOfTransportDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InlandModeOfTransportDropEdit, "JE_TransportModeInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_TransportModeInland)));
			this.InlandModeOfTransportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InlandModeOfTransportDropEdit.Name = "InlandModeOfTransportDropEdit";
			this.InlandModeOfTransportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.InlandModeOfTransportDropEdit.TabIndex = 0;
			// 
			// InlandTransportMeansDropEdit
			// 
			this.InlandTransportMeansDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InlandTransportMeansDropEdit, "JE_TransportMeans");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_TransportMeans)));
			this.InlandTransportMeansDropEdit.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("cff48c70-c3a5-44c3-9509-4ed8bf64a28e", "[18] Code", "[18] Inland Transport Code");
			this.InlandTransportMeansDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 0, true);
			this.InlandTransportMeansDropEdit.Name = "InlandTransportMeansDropEdit";
			this.InlandTransportMeansDropEdit.PreBoundMaxLength = 2;
			this.InlandTransportMeansDropEdit.ShowDescriptionBox = false;
			this.InlandTransportMeansDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.InlandTransportMeansDropEdit.TabIndex = 1;
			// 
			// InlandTransportModeAndMeansUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InlandModeOfTransportDropEdit);
			this.Controls.Add(this.InlandTransportMeansDropEdit);
			this.Name = "InlandTransportModeAndMeansUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InlandModeOfTransportDropEdit.ResumeLayout(true);
			this.InlandModeOfTransportDropEdit.PerformLayout();
			this.InlandTransportMeansDropEdit.ResumeLayout(true);
			this.InlandTransportMeansDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit InlandModeOfTransportDropEdit;
		internal ZArchitecture.GUI.ZDropEdit InlandTransportMeansDropEdit;
	}
}
