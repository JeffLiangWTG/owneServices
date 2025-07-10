using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Main.Startup.MainForm
{
	partial class RefStlScriptGeneratorForm
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
		new void InitializeComponent()
		{
			this.SuspendLayout();
			this.components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 450, true);
			this.Text = "Dynamic Collector Definitions";
			this.allScripts = new CargoWise.Windows.UI.KComboBox();
			this.selectedScripts = new CargoWise.Windows.UI.KComboBox();
			this.zLableAll = new Enterprise.ZArchitecture.ZLabel();
			this.zLableSelected = new Enterprise.ZArchitecture.ZLabel();
			this.generateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.addButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.removeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.displayPropertiesButton = new Enterprise.ZArchitecture.GUI.ZButton();

			// 
			// zLableAll
			// 
			this.zLableAll.CaptionResourceString = CargoWise.Main.Res.GetData("0374340A-AFBE-4888-B5C2-A541DB2D75A9", "All: ");
			this.zLableAll.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLableAll.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 25, true);
			this.zLableAll.Name = "zLableAll";
			this.zLableAll.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 25, true);
			this.zLableAll.TabIndex = 1;
			this.zLableAll.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

			// 
			// allScripts
			// 
			this.allScripts.AllowDrop = true;
			this.allScripts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.allScripts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.allScripts.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 26, true);
			this.allScripts.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 0, true);
			this.allScripts.Name = "allScripts";
			this.allScripts.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 25, true);
			this.allScripts.TabIndex = 2;

			// 
			// addButton
			// 
			this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.addButton.CaptionResourceString = CargoWise.Main.Res.GetData("BC4F4D05-DBCC-43A3-90C7-A73255C61E04", "Add");
			this.addButton.IsCaptionOverridden = false;
			this.addButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 24, true);
			this.addButton.Name = "addButton";
			this.addButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.addButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.addButton.TabIndex = 3;
			this.addButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.addButton.ToolTipCaption = null;
			this.addButton.UseVisualStyleBackColor = true;
			this.addButton.Click += new System.EventHandler(this.addButton_Click);

			// 
			// zLableSelected
			// 
			this.zLableSelected.CaptionResourceString = CargoWise.Main.Res.GetData("3F5EF27D-9DDB-4CDD-BE05-A9CD204E810A", "Selected: ");
			this.zLableSelected.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLableSelected.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 55, true);
			this.zLableSelected.Name = "zLableSelected";
			this.zLableSelected.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 25, true);
			this.zLableSelected.TabIndex = 4;
			this.zLableSelected.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

			// 
			// selectedScripts
			// 
			this.selectedScripts.AllowDrop = true;
			this.selectedScripts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.selectedScripts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.selectedScripts.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 56, true);
			this.selectedScripts.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 0, true);
			this.selectedScripts.Name = "selectedScripts";
			this.selectedScripts.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 25, true);
			this.selectedScripts.TabIndex = 5;

			// 
			// removeButton
			// 
			this.removeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.removeButton.CaptionResourceString = CargoWise.Main.Res.GetData("60323E36-0F8A-4795-AC59-1A0787640956", "Remove");
			this.removeButton.IsCaptionOverridden = false;
			this.removeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 54, true);
			this.removeButton.Name = "removeButton";
			this.removeButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.removeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.removeButton.TabIndex = 6;
			this.removeButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.removeButton.ToolTipCaption = null;
			this.removeButton.UseVisualStyleBackColor = true;
			this.removeButton.Click += new System.EventHandler(this.removeButton_Click);

			// 
			// generateButton
			// 
			this.generateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.generateButton.CaptionResourceString = CargoWise.Main.Res.GetData("C78CA3B5-2E83-41A9-ABA2-C55AA4C033D6", "Generate SQL Script");
			this.generateButton.IsCaptionOverridden = false;
			this.generateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 84, true);
			this.generateButton.Name = "generateButton";
			this.generateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.generateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.generateButton.TabIndex = 7;
			this.generateButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.generateButton.ToolTipCaption = null;
			this.generateButton.UseVisualStyleBackColor = true;
			this.generateButton.Click += new System.EventHandler(this.generateButton_Click);

			// 
			// displayPropertiesButton
			// 
			this.displayPropertiesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.displayPropertiesButton.CaptionResourceString = CargoWise.Main.Res.GetData("C78CA3B5-2E83-41A9-ABA2-C55AA4C033D6", "Display Collector Properties");
			this.displayPropertiesButton.IsCaptionOverridden = false;
			this.displayPropertiesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 114, true);
			this.displayPropertiesButton.Name = "displayPropertiesButton";
			this.displayPropertiesButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.displayPropertiesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.displayPropertiesButton.TabIndex = 8;
			this.displayPropertiesButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.displayPropertiesButton.ToolTipCaption = null;
			this.displayPropertiesButton.UseVisualStyleBackColor = true;
			this.displayPropertiesButton.Click += new System.EventHandler(this.displayPropertiesButton_Click);

			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWise.Main.Res.GetData("1BF56582-F396-41AC-A28E-E5DDEC099B71", "Dynamic Collector Definitions");
			this.Controls.Add(this.zLableAll);
			this.Controls.Add(this.zLableSelected);
			this.Controls.Add(this.allScripts);
			this.Controls.Add(this.selectedScripts);
			this.Controls.Add(this.generateButton);
			this.Controls.Add(this.addButton);
			this.Controls.Add(this.removeButton);
			this.Controls.Add(this.displayPropertiesButton);
			this.Controls.SetChildIndex(this.zLableAll, 0);
			this.Controls.SetChildIndex(this.zLableSelected, 0);
			this.Controls.SetChildIndex(this.allScripts, 0);
			this.Controls.SetChildIndex(this.selectedScripts, 0);
			this.Controls.SetChildIndex(this.generateButton, 0);
			this.Controls.SetChildIndex(this.addButton, 0);
			this.Controls.SetChildIndex(this.removeButton, 0);
			this.Controls.SetChildIndex(this.displayPropertiesButton, 0);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected CargoWise.Windows.UI.KComboBox allScripts;
		protected CargoWise.Windows.UI.KComboBox selectedScripts;
		private ZLabel zLableAll;
		private ZLabel zLableSelected;
		private ZButton generateButton;
		private ZButton addButton;
		private ZButton removeButton;
		private ZButton displayPropertiesButton;
	}
}
