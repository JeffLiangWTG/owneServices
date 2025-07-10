namespace CargoWise.Tools.SpellCheck.GUI
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
		private void InitializeComponent()
		{
			this.listBoxSuggestions = new System.Windows.Forms.ListBox();
			this.labelSuggestions = new System.Windows.Forms.Label();
			this.richTextBoxContext = new CargoWise.Tools.SpellCheck.GUI.SpellCheckerRichTextBox();
			this.labelNotInDictionary = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonIgnoreUndo = new System.Windows.Forms.Button();
			this.buttonChange = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonIgnoreAll = new System.Windows.Forms.Button();
			this.buttonChangeAll = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// listBoxSuggestions
			// 
			this.listBoxSuggestions.FormattingEnabled = true;
			this.listBoxSuggestions.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 107, true);
			this.listBoxSuggestions.Name = "listBoxSuggestions";
			this.tableLayoutPanel1.SetRowSpan(this.listBoxSuggestions, 3);
			this.listBoxSuggestions.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 95, true);
			this.listBoxSuggestions.TabIndex = 3;
			this.listBoxSuggestions.UseTabStops = false;
			// 
			// labelSuggestions
			// 
			this.labelSuggestions.AutoSize = true;
			this.labelSuggestions.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 91, true);
			this.labelSuggestions.Name = "labelSuggestions";
			this.labelSuggestions.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 13, true);
			this.labelSuggestions.TabIndex = 2;
			this.labelSuggestions.Text = "Suggestio&ns:";
			this.labelSuggestions.UseMnemonic = true;
			// 
			// richTextBoxContext
			// 
			this.richTextBoxContext.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBoxContext.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.richTextBoxContext.Name = "richTextBoxContext";
			this.tableLayoutPanel1.SetRowSpan(this.richTextBoxContext, 3);
			this.richTextBoxContext.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 72, true);
			this.richTextBoxContext.TabIndex = 1;
			this.richTextBoxContext.Text = "";
			this.richTextBoxContext.TextChanged += new System.EventHandler(this.RichTextBoxContext_TextChanged);
			// 
			// labelNotInDictionary
			// 
			this.labelNotInDictionary.AutoSize = true;
			this.labelNotInDictionary.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.labelNotInDictionary.Name = "labelNotInDictionary";
			this.labelNotInDictionary.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 13, true);
			this.labelNotInDictionary.TabIndex = 0;
			this.labelNotInDictionary.Text = "Not in Dictionary&:";
			this.labelNotInDictionary.UseMnemonic = true;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75.62643F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24.37358F));
			this.tableLayoutPanel1.Controls.Add(this.labelNotInDictionary, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.richTextBoxContext, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelSuggestions, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.listBoxSuggestions, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.buttonIgnoreUndo, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.buttonChange, 1, 5);
			this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 1, 8);
			this.tableLayoutPanel1.Controls.Add(this.buttonIgnoreAll, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.buttonChangeAll, 1, 6);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 9;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
			this.tableLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(439, 239, true);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// buttonIgnoreUndo
			// 
			this.buttonIgnoreUndo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 16, true);
			this.buttonIgnoreUndo.Name = "buttonIgnoreUndo";
			this.buttonIgnoreUndo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 23, true);
			this.buttonIgnoreUndo.TabIndex = 4;
			this.buttonIgnoreUndo.Text = "&Ignore Once";
			this.buttonIgnoreUndo.UseVisualStyleBackColor = true;
			this.buttonIgnoreUndo.Click += new System.EventHandler(this.ButtonIgnoreUndo_Click);
			// 
			// buttonChange
			// 
			this.buttonChange.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 107, true);
			this.buttonChange.Name = "buttonChange";
			this.buttonChange.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 23, true);
			this.buttonChange.TabIndex = 5;
			this.buttonChange.Text = "&Change";
			this.buttonChange.UseVisualStyleBackColor = true;
			this.buttonChange.Click += new System.EventHandler(this.ButtonChange_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 208, true);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 23, true);
			this.buttonCancel.TabIndex = 6;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
			// 
			// buttonIgnoreAll
			// 
			this.buttonIgnoreAll.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 45, true);
			this.buttonIgnoreAll.Name = "buttonIgnoreAll";
			this.buttonIgnoreAll.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 23, true);
			this.buttonIgnoreAll.TabIndex = 7;
			this.buttonIgnoreAll.Text = "I&gnore All";
			this.buttonIgnoreAll.UseVisualStyleBackColor = true;
			this.buttonIgnoreAll.Click += new System.EventHandler(this.ButtonIgnoreAll_Click);
			// 
			// buttonChangeAll
			// 
			this.buttonChangeAll.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 136, true);
			this.buttonChangeAll.Name = "buttonChangeAll";
			this.buttonChangeAll.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 23, true);
			this.buttonChangeAll.TabIndex = 8;
			this.buttonChangeAll.Text = "Change Al&l";
			this.buttonChangeAll.UseVisualStyleBackColor = true;
			this.buttonChangeAll.Click += new System.EventHandler(this.ButtonChangeAll_Click);
			// 
			// SpellCheckerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(439, 239, true);
			this.ControlBox = false;
			this.Controls.Add(this.tableLayoutPanel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "SpellCheckerForm";
			this.Text = "Spelling";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox listBoxSuggestions;
		private System.Windows.Forms.Label labelSuggestions;
		private SpellCheckerRichTextBox richTextBoxContext;
		private System.Windows.Forms.Label labelNotInDictionary;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button buttonIgnoreUndo;
		private System.Windows.Forms.Button buttonChange;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonIgnoreAll;
		private System.Windows.Forms.Button buttonChangeAll;
	}
}
