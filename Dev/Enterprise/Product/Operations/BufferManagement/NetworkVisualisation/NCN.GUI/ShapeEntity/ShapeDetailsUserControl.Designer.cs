namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	partial class ShapeDetailsUserControl
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
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ShapeTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ShapeDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.networkDiagramScaleUserControl1 = new Enterprise.BufferManagement.NetworkVisualisation.GUI.ShapePropertiesUserControl();
			this.WorkflowDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.workflowDetailsUserControl1 = new Enterprise.BufferManagement.GUI.WorkflowDetailsUserControl();
			this.BufferPenetrationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BufferPenetrationControl = new Enterprise.BufferManagement.NetworkVisualisation.GUI.BufferPenetrationUserControl();
			this.ChannelsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ChannelsControl = new Enterprise.BufferManagement.NetworkVisualisation.GUI.ShapeEntityChannelsUserControl();
			this.LevelingRulesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LevelingRulesControl = new Enterprise.BufferManagement.NetworkVisualisation.GUI.ShapeEntityLevelingRulesUserControl();
			this.ShapeNotesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ShapeNotesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShapeTabControl.SuspendLayout();
			this.ShapeDetailsTabPage.SuspendLayout();
			this.networkDiagramScaleUserControl1.SuspendLayout();
			this.WorkflowDetailsTabPage.SuspendLayout();
			this.workflowDetailsUserControl1.SuspendLayout();
			this.BufferPenetrationTabPage.SuspendLayout();
			this.BufferPenetrationControl.SuspendLayout();
			this.ChannelsTabPage.SuspendLayout();
			this.ChannelsControl.SuspendLayout();
			this.LevelingRulesTabPage.SuspendLayout();
			this.LevelingRulesControl.SuspendLayout();
			this.ShapeNotesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity);
			// 
			// ShapeTabControl
			// 
			this.ShapeTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ShapeTabControl.Controls.Add(this.ShapeDetailsTabPage);
			this.ShapeTabControl.Controls.Add(this.WorkflowDetailsTabPage);
			this.ShapeTabControl.Controls.Add(this.BufferPenetrationTabPage);
			this.ShapeTabControl.Controls.Add(this.ChannelsTabPage);
			this.ShapeTabControl.Controls.Add(this.LevelingRulesTabPage);
			this.ShapeTabControl.Controls.Add(this.ShapeNotesTabPage);
			this.ShapeTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShapeTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShapeTabControl.Name = "ShapeTabControl";
			this.ShapeTabControl.SelectedIndex = 0;
			this.ShapeTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 568, true);
			this.ShapeTabControl.TabIndex = 0;
			// 
			// ShapeDetailsTabPage
			// 
			this.ShapeDetailsTabPage.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("c1ad1473-830c-4854-85a8-b722b1a37739", "Shape Details");
			this.ShapeDetailsTabPage.Controls.Add(this.networkDiagramScaleUserControl1);
			this.ShapeDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ShapeDetailsTabPage.Name = "ShapeDetailsTabPage";
			this.ShapeDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 541, true);
			this.ShapeDetailsTabPage.TabIndex = 2;
			// 
			// networkDiagramScaleUserControl1
			// 
			this.networkDiagramScaleUserControl1.AllowDrop = true;
			this.networkDiagramScaleUserControl1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.networkDiagramScaleUserControl1, ".");
			this.networkDiagramScaleUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.networkDiagramScaleUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.networkDiagramScaleUserControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 480, true);
			this.networkDiagramScaleUserControl1.Name = "networkDiagramScaleUserControl1";
			this.networkDiagramScaleUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 541, true);
			this.networkDiagramScaleUserControl1.TabIndex = 0;
			// 
			// WorkflowDetailsTabPage
			// 
			this.WorkflowDetailsTabPage.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("2cec55f4-81c5-4834-a5ae-08e56b2112d5", "Workflow Details");
			this.WorkflowDetailsTabPage.Controls.Add(this.workflowDetailsUserControl1);
			this.WorkflowDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowDetailsTabPage.Name = "WorkflowDetailsTabPage";
			this.WorkflowDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 541, true);
			this.WorkflowDetailsTabPage.TabIndex = 0;
			this.WorkflowDetailsTabPage.UseVisualStyleBackColor = true;
			// 
			// workflowDetailsUserControl1
			// 
			this.workflowDetailsUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.workflowDetailsUserControl1, "Shape.ProcessHeader");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.BufferManagement.Business.ProcessHeader)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.ProcessHeader)));
			this.workflowDetailsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.workflowDetailsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.workflowDetailsUserControl1.Name = "workflowDetailsUserControl1";
			this.workflowDetailsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 67, true);
			this.workflowDetailsUserControl1.TabIndex = 0;
			// 
			// BufferPenetrationTabPage
			// 
			this.BufferPenetrationTabPage.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("3a1ced16-0acb-4ad7-a876-a31aa8df5399", "Buffer Penetration");
			this.BufferPenetrationTabPage.Controls.Add(this.BufferPenetrationControl);
			this.BufferPenetrationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BufferPenetrationTabPage.Name = "BufferPenetrationTabPage";
			this.BufferPenetrationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 541, true);
			this.BufferPenetrationTabPage.TabIndex = 3;
			// 
			// BufferPenetrationControl
			// 
			this.BufferPenetrationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BufferPenetrationControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.BufferPenetrationControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BufferPenetrationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BufferPenetrationControl.Name = "BufferPenetrationControl";
			this.BufferPenetrationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.BufferPenetrationControl.TabIndex = 0;
			// 
			// ChannelsTabPage
			// 
			this.ChannelsTabPage.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("782b9356-343c-445d-a53d-5eb4b2acbf22", "Channels");
			this.ChannelsTabPage.Controls.Add(this.ChannelsControl);
			this.ChannelsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ChannelsTabPage.Name = "ChannelsTabPage";
			this.ChannelsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ChannelsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 541, true);
			this.ChannelsTabPage.TabIndex = 4;
			this.ChannelsTabPage.UseVisualStyleBackColor = true;
			// 
			// ChannelsControl
			// 
			this.ChannelsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChannelsControl, "ShapeAsRootDiagram");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNRootDiagramShape)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).ShapeAsRootDiagram)));
			this.ChannelsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChannelsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ChannelsControl.Name = "ChannelsControl";
			this.ChannelsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 67, true);
			this.ChannelsControl.TabIndex = 0;
			// 
			// LevelingRulesTabPage
			// 
			this.LevelingRulesTabPage.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("89f98663-a6bc-4596-a9ed-03c3a3d0efe8", "Leveling Rules");
			this.LevelingRulesTabPage.Controls.Add(this.LevelingRulesControl);
			this.LevelingRulesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LevelingRulesTabPage.Name = "LevelingRulesTabPage";
			this.LevelingRulesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.LevelingRulesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 541, true);
			this.LevelingRulesTabPage.TabIndex = 5;
			this.LevelingRulesTabPage.UseVisualStyleBackColor = true;
			// 
			// LevelingRulesControl
			// 
			this.LevelingRulesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LevelingRulesControl, "ShapeAsRootDiagram");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNRootDiagramShape)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).ShapeAsRootDiagram)));
			this.LevelingRulesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LevelingRulesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LevelingRulesControl.Name = "LevelingRulesControl";
			this.LevelingRulesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 535, true);
			this.LevelingRulesControl.TabIndex = 0;
			// 
			// ShapeNotesTabPage
			// 
			this.ShapeNotesTabPage.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("3f2a0714-5089-4ef0-a714-a5037b926ae7", "Notes");
			this.ShapeNotesTabPage.Controls.Add(this.ShapeNotesTextBox);
			this.ShapeNotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ShapeNotesTabPage.Name = "ShapeNotesTabPage";
			this.ShapeNotesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ShapeNotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 541, true);
			this.ShapeNotesTabPage.TabIndex = 1;
			this.ShapeNotesTabPage.UseVisualStyleBackColor = true;
			// 
			// ShapeNotesTextBox
			// 
			this.ShapeNotesTextBox.AcceptsReturn = true;
			this.ShapeNotesTextBox.AcceptsTab = true;
			this.ShapeNotesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ShapeNotesTextBox, "Shape.ShapeNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.ShapeNotes)));
			this.ShapeNotesTextBox.CaptionResourceString = null;
			this.ShapeNotesTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ShapeNotesTextBox, false);
			this.ShapeNotesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.ShapeNotesTextBox.Multiline = true;
			this.ShapeNotesTextBox.Name = "ShapeNotesTextBox";
			this.ShapeNotesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 529, true);
			this.ShapeNotesTextBox.TabIndex = 1;
			// 
			// ShapeDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ShapeTabControl);
			this.Name = "ShapeDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 568, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShapeTabControl.ResumeLayout(false);
			this.ShapeTabControl.PerformLayout();
			this.ShapeDetailsTabPage.ResumeLayout(false);
			this.ShapeDetailsTabPage.PerformLayout();
			this.networkDiagramScaleUserControl1.ResumeLayout(true);
			this.networkDiagramScaleUserControl1.PerformLayout();
			this.WorkflowDetailsTabPage.ResumeLayout(false);
			this.WorkflowDetailsTabPage.PerformLayout();
			this.workflowDetailsUserControl1.ResumeLayout(true);
			this.workflowDetailsUserControl1.PerformLayout();
			this.BufferPenetrationTabPage.ResumeLayout(false);
			this.BufferPenetrationTabPage.PerformLayout();
			this.BufferPenetrationControl.ResumeLayout(true);
			this.BufferPenetrationControl.PerformLayout();
			this.ChannelsTabPage.ResumeLayout(false);
			this.ChannelsTabPage.PerformLayout();
			this.ChannelsControl.ResumeLayout(true);
			this.ChannelsControl.PerformLayout();
			this.LevelingRulesTabPage.ResumeLayout(false);
			this.LevelingRulesTabPage.PerformLayout();
			this.LevelingRulesControl.ResumeLayout(true);
			this.LevelingRulesControl.PerformLayout();
			this.ShapeNotesTabPage.ResumeLayout(false);
			this.ShapeNotesTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl ShapeTabControl;
		private ZArchitecture.GUI.ZTabPage WorkflowDetailsTabPage;
		private ZArchitecture.GUI.ZTabPage ShapeNotesTabPage;
		private BufferManagement.GUI.WorkflowDetailsUserControl workflowDetailsUserControl1;
		private ZArchitecture.ZTextBox ShapeNotesTextBox;
		private ZArchitecture.GUI.ZTabPage ShapeDetailsTabPage;
		private ShapePropertiesUserControl networkDiagramScaleUserControl1;
		private ZArchitecture.GUI.ZTabPage BufferPenetrationTabPage;
		private BufferPenetrationUserControl BufferPenetrationControl;
		private ZArchitecture.GUI.ZTabPage ChannelsTabPage;
		private ShapeEntityChannelsUserControl ChannelsControl;
		private ZArchitecture.GUI.ZTabPage LevelingRulesTabPage;
		private ShapeEntityLevelingRulesUserControl LevelingRulesControl;
	}
}
