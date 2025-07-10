namespace Enterprise.PAVE.MENT.GUI
{
	partial class DataCollectionControl
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
			this.dataCollectionDetailsControl1 = new Enterprise.PAVE.MENT.GUI.DataCollectionDetailsControl();
			this.zGroupBox3 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.queryTextEdit = new Enterprise.ZArchitecture.GUI.ZSqlTextBox();
			this.sqlHelpLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.dataCollectionDetailsControl1.SuspendLayout();
			this.zGroupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery);
			// 
			// dataCollectionDetailsControl1
			// 
			this.dataCollectionDetailsControl1.AllowDrop = true;
			this.dataCollectionDetailsControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.dataCollectionDetailsControl1, ".");
			this.dataCollectionDetailsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 3, true);
			this.dataCollectionDetailsControl1.Name = "dataCollectionDetailsControl1";
			this.dataCollectionDetailsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(702, 178, true);
			this.dataCollectionDetailsControl1.TabIndex = 5;
			// 
			// zGroupBox3
			// 
			this.zGroupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox3.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("51746b1b-7e13-4308-a5c3-6d7d6f4685d9", "Query Text");
			this.zGroupBox3.Controls.Add(this.queryTextEdit);
			this.zGroupBox3.Controls.Add(this.sqlHelpLabel);
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 187, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 496, true);
			this.zGroupBox3.TabIndex = 4;
			this.zGroupBox3.TabStop = false;
			// 
			// queryTextEdit
			// 
			this.queryTextEdit.AcceptsReturn = true;
			this.queryTextEdit.AcceptsTab = true;
			this.queryTextEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.queryTextEdit, "MAQ_SqlText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).MAQ_SqlText)));
			this.queryTextEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.queryTextEdit.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.queryTextEdit, false);
			this.queryTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 47, true);
			this.queryTextEdit.Multiline = true;
			this.queryTextEdit.Name = "queryTextEdit";
			this.queryTextEdit.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.queryTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 443, true);
			this.queryTextEdit.TabIndex = 3;
			// 
			// sqlHelpLabel
			// 
			this.BindingSource.SetBindingMember(this.sqlHelpLabel, "SQLLabelDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).SQLLabelDescription)));
			this.sqlHelpLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.sqlHelpLabel.Name = "sqlHelpLabel";
			this.sqlHelpLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(689, 28, true);
			this.sqlHelpLabel.TabIndex = 2;
			// 
			// DataCollectionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.dataCollectionDetailsControl1);
			this.Controls.Add(this.zGroupBox3);
			this.Name = "DataCollectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 683, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.dataCollectionDetailsControl1.ResumeLayout(true);
			this.dataCollectionDetailsControl1.PerformLayout();
			this.zGroupBox3.ResumeLayout(false);
			this.zGroupBox3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox zGroupBox3;
		private ZArchitecture.ZLabel sqlHelpLabel;
		private DataCollectionDetailsControl dataCollectionDetailsControl1;
		private ZArchitecture.GUI.ZSqlTextBox queryTextEdit;
	}
}
