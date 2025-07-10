namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	partial class ProperCaseExcludeListForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.excludeWordsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.excludeWordsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 287, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.DataMapping.ProperCaseExcludeWordCollection);
			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.AllowDrop = true;
			this.postingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 258, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.postingButtonsUserControl.TabIndex = 2;
			this.postingButtonsUserControl.TabStop = true;
			// 
			// zLabel1
			// 
			this.zLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("525698fe-551f-4076-87b1-5cd7e0774855", "Words Entered Here Will Not Be Converted To Proper Case");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 23, true);
			this.zLabel1.TabIndex = 0;
			// 
			// excludeWordsGrid
			// 
			this.excludeWordsGrid.AllowNavigation = false;
			this.excludeWordsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.excludeWordsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ProperCaseExcludeWord)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ProperCaseExcludeWord)(null)).Word)));
			this.excludeWordsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("7ea9445c-ebb7-4a99-bdeb-c806a4dd5426", "Word");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Word";
			this.excludeWordsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.excludeWordsGrid.CopySelectedRowsAllowed = true;
			this.excludeWordsGrid.GridId = "2cb38d0f-46b1-4a4e-abfc-bd0312c3fccb";
			this.excludeWordsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.excludeWordsGrid.LayoutKey = "excludeWordsGrid";
			this.excludeWordsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 35, true);
			this.excludeWordsGrid.Name = "excludeWordsGrid";
			this.excludeWordsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 217, true);
			this.excludeWordsGrid.TabIndex = 1;
			// 
			// ProperCaseExcludeListForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 311, true);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.excludeWordsGrid);
			this.Controls.Add(this.postingButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.DataMapping.ProperCaseExcludeWordCollection);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 300, true);
			this.Name = "ProperCaseExcludeListForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.postingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.excludeWordsGrid, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.excludeWordsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.Core.Forms.ZPostingButtonsUserControl postingButtonsUserControl;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private ZGrid excludeWordsGrid;
	}
}