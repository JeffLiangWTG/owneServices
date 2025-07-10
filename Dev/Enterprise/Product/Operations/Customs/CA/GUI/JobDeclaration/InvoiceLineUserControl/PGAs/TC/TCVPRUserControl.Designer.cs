using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	partial class TCVPRUserControl
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

				foreach (ZUserControl control in subProgramControls.Values)
				{
					control.Dispose();
				}
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
			this.SubProgramDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DetailsPanel = new CargoWise.Windows.UI.KPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SubProgramDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.TCPGAHeader);
			// 
			// SubProgramDropEdit
			// 
			this.SubProgramDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SubProgramDropEdit, "CA_SubProgram");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).CA_SubProgram)));
			this.SubProgramDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d08eaaf2-78d5-4b80-9e57-1f2025a510b1", "Sub-Program");
			this.SubProgramDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 12, true);
			this.SubProgramDropEdit.Name = "SubProgramDropEdit";
			this.SubProgramDropEdit.ShouldResizeByMaxLength = true;
			this.SubProgramDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.SubProgramDropEdit.TabIndex = 2;
			// 
			// DetailsPanel
			// 
			this.DetailsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 38, true);
			this.DetailsPanel.Name = "DetailsPanel";
			this.DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1196, 535, true);
			this.DetailsPanel.TabIndex = 3;
			// 
			// TCVPRUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 400, true);
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailsPanel);
			this.Controls.Add(this.SubProgramDropEdit);
			this.Name = "TCVPRUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1202, 576, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SubProgramDropEdit.ResumeLayout(true);
			this.SubProgramDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		protected ZArchitecture.GUI.ZDropEdit SubProgramDropEdit;
		private CargoWise.Windows.UI.KPanel DetailsPanel;
	}
}
