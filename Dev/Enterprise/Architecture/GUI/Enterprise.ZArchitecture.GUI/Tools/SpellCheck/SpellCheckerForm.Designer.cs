namespace Enterprise.ZArchitecture.GUI.Tools.SpellCheck
{
	partial class SpellCheckerForm
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
			ItemSpellCheckCompleted = null;

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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		private new void InitializeComponent()
		{
			this.listBoxSuggestions = new Enterprise.ZArchitecture.GUI.ZListBox();
			this.labelSuggestions = new Enterprise.ZArchitecture.ZLabel();
			this.richTextBoxContext = new CargoWise.Tools.SpellCheck.GUI.SpellCheckerRichTextBox();
			this.labelNotInDictionary = new Enterprise.ZArchitecture.ZLabel();
			this.tableLayoutPanel1 = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.buttonIgnoreUndo = new Enterprise.ZArchitecture.GUI.ZButton();
			this.buttonChange = new Enterprise.ZArchitecture.GUI.ZButton();
			this.buttonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.buttonChangeAll = new Enterprise.ZArchitecture.GUI.ZButton();
			this.buttonIgnoreAll = new Enterprise.ZArchitecture.GUI.ZButton();
			this.buttonAddToDictionary = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 276, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 24, true);
			// 
			// listBoxSuggestions
			// 
			this.listBoxSuggestions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listBoxSuggestions.FormattingEnabled = true;
			this.listBoxSuggestions.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 136, true);
			this.listBoxSuggestions.Name = "listBoxSuggestions";
			this.tableLayoutPanel1.SetRowSpan(this.listBoxSuggestions, 3);
			this.listBoxSuggestions.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 95, true);
			this.listBoxSuggestions.TabIndex = 3;
			this.listBoxSuggestions.UseTabStops = false;
			// 
			// labelSuggestions
			// 
			this.labelSuggestions.AutoSize = true;
			this.labelSuggestions.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("4d2a8283-4776-4db3-80d7-965881b4d46e", "Suggestio&ns:");
			this.labelSuggestions.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.labelSuggestions.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 120, true);
			this.labelSuggestions.Name = "labelSuggestions";
			this.labelSuggestions.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 13, true);
			this.labelSuggestions.TabIndex = 2;
			this.labelSuggestions.UseMnemonic = true;
			// 
			// richTextBoxContext
			// 
			this.richTextBoxContext.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBoxContext.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.richTextBoxContext.Name = "richTextBoxContext";
			this.tableLayoutPanel1.SetRowSpan(this.richTextBoxContext, 4);
			this.richTextBoxContext.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 101, true);
			this.richTextBoxContext.TabIndex = 1;
			this.richTextBoxContext.Text = "";
			this.richTextBoxContext.TextChanged += new System.EventHandler(this.richTextBoxContext_TextChanged);
			// 
			// labelNotInDictionary
			// 
			this.labelNotInDictionary.AutoSize = true;
			this.labelNotInDictionary.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("b42e887c-a417-48f6-a88f-58b2cdbb1732", "Not in Dictionary&:");
			this.labelNotInDictionary.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.labelNotInDictionary.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.labelNotInDictionary.Name = "labelNotInDictionary";
			this.labelNotInDictionary.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 13, true);
			this.labelNotInDictionary.TabIndex = 0;
			this.labelNotInDictionary.UseMnemonic = true;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67.16637F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.83363F));
			this.tableLayoutPanel1.Controls.Add(this.labelNotInDictionary, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.richTextBoxContext, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelSuggestions, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.listBoxSuggestions, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.buttonIgnoreUndo, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.buttonChange, 1, 7);
			this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 1, 9);
			this.tableLayoutPanel1.Controls.Add(this.buttonIgnoreAll, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.buttonAddToDictionary, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.buttonChangeAll, 1, 6);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 10;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(12)));
			this.tableLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 300, true);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// buttonIgnoreUndo
			// 
			this.buttonIgnoreUndo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 16, true);
			this.buttonIgnoreUndo.Name = "buttonIgnoreUndo";
			this.buttonIgnoreUndo.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.buttonIgnoreUndo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 23, true);
			this.buttonIgnoreUndo.TabIndex = 4;
			this.buttonIgnoreUndo.ToolTipCaption = null;
			this.buttonIgnoreUndo.UseVisualStyleBackColor = true;
			this.buttonIgnoreUndo.Click += new System.EventHandler(this.buttonIgnoreUndo_Click);
			// 
			// buttonChange
			// 
			this.buttonChange.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("3a88afc1-9563-4621-a614-312e8c2f613e", "&Change");
			this.buttonChange.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 165, true);
			this.buttonChange.Name = "buttonChange";
			this.buttonChange.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.buttonChange.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 23, true);
			this.buttonChange.TabIndex = 8;
			this.buttonChange.ToolTipCaption = null;
			this.buttonChange.UseVisualStyleBackColor = true;
			this.buttonChange.Click += new System.EventHandler(this.buttonChange_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("c36ea893-5055-4deb-8c5e-b666018cdd5d", "Cancel");
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 237, true);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.buttonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 23, true);
			this.buttonCancel.TabIndex = 9;
			this.buttonCancel.ToolTipCaption = null;
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonChangeAll
			// 
			this.buttonChangeAll.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("b928b7fd-0003-40dc-900e-0241c2061ae0", "Change Al&l");
			this.buttonChangeAll.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 136, true);
			this.buttonChangeAll.Name = "buttonChangeAll";
			this.buttonChangeAll.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.buttonChangeAll.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 23, true);
			this.buttonChangeAll.TabIndex = 7;
			this.buttonChangeAll.ToolTipCaption = null;
			this.buttonChangeAll.UseVisualStyleBackColor = true;
			this.buttonChangeAll.Click += new System.EventHandler(this.buttonChangeAll_Click);
			// 
			// buttonIgnoreAll
			// 
			this.buttonIgnoreAll.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("b0ae2124-2936-4d97-8534-4d39b94d97e6", "I&gnore All");
			this.buttonIgnoreAll.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 45, true);
			this.buttonIgnoreAll.Name = "buttonIgnoreAll";
			this.buttonIgnoreAll.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.buttonIgnoreAll.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 23, true);
			this.buttonIgnoreAll.TabIndex = 5;
			this.buttonIgnoreAll.ToolTipCaption = null;
			this.buttonIgnoreAll.UseVisualStyleBackColor = true;
			this.buttonIgnoreAll.Click += new System.EventHandler(this.buttonIgnoreAll_Click);
			// 
			// buttonAddToDictionary
			// 
			this.buttonAddToDictionary.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("d64f207e-963c-4396-8453-00cd15e94e79", "Add to Dictionary");
			this.buttonAddToDictionary.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.buttonAddToDictionary.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 74, true);
			this.buttonAddToDictionary.Name = "buttonAddToDictionary";
			this.buttonAddToDictionary.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.buttonAddToDictionary.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 23, true);
			this.buttonAddToDictionary.TabIndex = 6;
			this.buttonAddToDictionary.ToolTipCaption = null;
			this.buttonAddToDictionary.UseVisualStyleBackColor = true;
			this.buttonAddToDictionary.Click += new System.EventHandler(this.buttonAddToDictionary_Click);
			// 
			// SpellCheckerForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.buttonCancel;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("1369db3b-e189-4a87-ba60-e6947262e980", "Spelling");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 300, true);
			this.ControlBox = false;
			this.Controls.Add(this.tableLayoutPanel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "SpellCheckerForm";
			this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZListBox listBoxSuggestions;
		private Enterprise.ZArchitecture.ZLabel labelSuggestions;
		private CargoWise.Tools.SpellCheck.GUI.SpellCheckerRichTextBox richTextBoxContext;
		private Enterprise.ZArchitecture.ZLabel labelNotInDictionary;
		private CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanel1;
		private Enterprise.ZArchitecture.GUI.ZButton buttonIgnoreUndo;
		private Enterprise.ZArchitecture.GUI.ZButton buttonChange;
		private Enterprise.ZArchitecture.GUI.ZButton buttonCancel;
		private Enterprise.ZArchitecture.GUI.ZButton buttonIgnoreAll;
		private Enterprise.ZArchitecture.GUI.ZButton buttonChangeAll;
		private ZButton buttonAddToDictionary;
	}
}
