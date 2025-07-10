
namespace Enterprise.Customs.KR.GUI
{
	partial class CustomsBrokerCommentUserControl
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
            this.CustomsBrokerCommentCode1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CustomsBrokerCommentCode2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CustomsBrokerCommentCode3 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CustomsBrokerCommentCode1.SuspendLayout();
            this.CustomsBrokerCommentCode2.SuspendLayout();
            this.CustomsBrokerCommentCode3.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // CustomsBrokerCommentCode1
            // 
            this.CustomsBrokerCommentCode1.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsBrokerCommentCode1, "CustomsBrokerCommentCode1");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsBrokerCommentCode1)));
            this.CustomsBrokerCommentCode1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 2, true);
            this.CustomsBrokerCommentCode1.Name = "CustomsBrokerCommentCode1";
            this.CustomsBrokerCommentCode1.PreBoundMaxLength = 1;
            this.CustomsBrokerCommentCode1.ShowDescriptionBox = false;
            this.CustomsBrokerCommentCode1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 17, true);
            this.CustomsBrokerCommentCode1.TabIndex = 7;
            // 
            // CustomsBrokerCommentCode2
            // 
            this.CustomsBrokerCommentCode2.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsBrokerCommentCode2, "CustomsBrokerCommentCode2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsBrokerCommentCode2)));
            this.CustomsBrokerCommentCode2.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("a9f1cdbc-3324-4505-ab96-75d5c84cd3a5", "  ");
            this.CustomsBrokerCommentCode2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 2, true);
            this.CustomsBrokerCommentCode2.Name = "CustomsBrokerCommentCode2";
            this.CustomsBrokerCommentCode2.PreBoundMaxLength = 1;
            this.CustomsBrokerCommentCode2.ShowDescriptionBox = false;
            this.CustomsBrokerCommentCode2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 17, true);
            this.CustomsBrokerCommentCode2.TabIndex = 8;
            // 
            // CustomsBrokerCommentCode3
            // 
            this.CustomsBrokerCommentCode3.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsBrokerCommentCode3, "CustomsBrokerCommentCode3");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsBrokerCommentCode3)));
            this.CustomsBrokerCommentCode3.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("982a40ef-2550-4c28-9df0-6360728f2a01", " ");
            this.CustomsBrokerCommentCode3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 2, true);
            this.CustomsBrokerCommentCode3.Name = "CustomsBrokerCommentCode3";
            this.CustomsBrokerCommentCode3.PreBoundMaxLength = 1;
            this.CustomsBrokerCommentCode3.ShowDescriptionBox = false;
            this.CustomsBrokerCommentCode3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 17, true);
            this.CustomsBrokerCommentCode3.TabIndex = 9;
            // 
            // zLabel1
            // 
            this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 2, true);
            this.zLabel1.Name = "zLabel1";
            this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 17, true);
            this.zLabel1.TabIndex = 10;
            this.zLabel1.Text = "-";
            this.zLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.zLabel1.UseMnemonic = false;
            // 
            // zLabel2
            // 
            this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 3, true);
            this.zLabel2.Name = "zLabel2";
            this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 17, true);
            this.zLabel2.TabIndex = 11;
            this.zLabel2.Text = "-";
            this.zLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.zLabel2.UseMnemonic = false;
            // 
            // CustomsBrokerCommentUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.zLabel2);
            this.Controls.Add(this.zLabel1);
            this.Controls.Add(this.CustomsBrokerCommentCode3);
            this.Controls.Add(this.CustomsBrokerCommentCode2);
            this.Controls.Add(this.CustomsBrokerCommentCode1);
            this.Name = "CustomsBrokerCommentUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 23, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CustomsBrokerCommentCode1.ResumeLayout(true);
            this.CustomsBrokerCommentCode1.PerformLayout();
            this.CustomsBrokerCommentCode2.ResumeLayout(true);
            this.CustomsBrokerCommentCode2.PerformLayout();
            this.CustomsBrokerCommentCode3.ResumeLayout(true);
            this.CustomsBrokerCommentCode3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		public ZArchitecture.GUI.ZDropEdit CustomsBrokerCommentCode1;
		public ZArchitecture.GUI.ZDropEdit CustomsBrokerCommentCode2;
		public ZArchitecture.GUI.ZDropEdit CustomsBrokerCommentCode3;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel zLabel2;
	}
}
