using System.Drawing;
using CargoWiseOne.ResourceStrings;

namespace CargoWise.NetworkVisualisation.GUI
{
	partial class SearchFinderForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            this.searchTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.searchButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.SuspendLayout();
            // 
            // searchTextBox
            // 
            this.searchTextBox.Location = new System.Drawing.Point(24, 20);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(180, 35);
            this.searchTextBox.TabIndex = 0;
			this.searchTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.searchTextBox, false);
			this.searchTextBox.TextChanged += new System.EventHandler(this.searchTextBox_TextChanged);
            this.searchTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.searchTextBox_KeyDown);
            // 
            // searchButton
            // 
            this.searchButton.Location = new System.Drawing.Point(24, 55);
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(80, 24);
            this.searchButton.TabIndex = 1;
            this.searchButton.UseVisualStyleBackColor = true;
            this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(124, 55);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(80, 24);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.CaptionResourceString = Res.GetData("5ed45713-f6a3-42df-a02d-0f3bef329eef", "Cancel");
            this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// SearchFinderForm
			//
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Res.GetData("e9ae241c-e3b1-4378-ad6b-555a64b9280a", "Search");
			this.ClientSize = new System.Drawing.Size(228, 94);
			this.BackColor = Color.White;
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.searchButton);
            this.Controls.Add(this.searchTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MinimizeBox = false;
            this.Name = "SearchFinderForm";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox searchTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton searchButton;
		private Enterprise.ZArchitecture.GUI.ZButton cancelButton;
	}
}
