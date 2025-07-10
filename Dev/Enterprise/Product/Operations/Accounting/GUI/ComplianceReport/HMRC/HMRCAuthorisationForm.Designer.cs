namespace Enterprise.Accounting.GUI.ComplianceReport
{
	partial class HMRCAuthorisationForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HMRCAuthorisationForm));
			this.zLabelPrompt = new Enterprise.ZArchitecture.ZLabel();
			this.zButtonConnect = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// zLabelPrompt
			// 
			this.zLabelPrompt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zLabelPrompt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.zLabelPrompt.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelPrompt.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.zLabelPrompt.Name = "zLabelPrompt";
			this.zLabelPrompt.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(20, true);
			this.zLabelPrompt.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 120, true);
			this.zLabelPrompt.TabIndex = 0;
			this.zLabelPrompt.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("741882bb-7687-47a3-81a4-fcf13b7f5fb4", """
				CargoWise needs your permission to interact with HMRC web services on your behalf.

				Click "Connect with HMRC" button to open "Allow your software to connect with HMRC" web page in your browser.
				Once the page loads, follow the prompts to authorize CargoWise and return to this screen when done.
				""");
			this.zLabelPrompt.UseMnemonic = false;
			this.zLabelPrompt.BackColor = System.Drawing.SystemColors.Control;
			// 
			// zButtonConnect
			// 
			this.zButtonConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.zButtonConnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.zButtonConnect.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 148, true);
			this.zButtonConnect.Name = "zButtonConnect";
			this.zButtonConnect.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 23, true);
			this.zButtonConnect.TabIndex = 1;
			this.zButtonConnect.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b2d72e4b-3c4c-4434-96e5-5dfa83326075", "Connect with HMRC...");
			this.zButtonConnect.UseVisualStyleBackColor = true;
			this.zButtonConnect.Click += new System.EventHandler(this.zButtonConnect_Click);
			// 
			// zButtonCancel
			// 
			this.zButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.zButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(620, 148, true);
			this.zButtonCancel.Name = "zButtonCancel";
			this.zButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButtonCancel.TabIndex = 2;
			this.zButtonCancel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ae59d5f1-e4a4-4079-b730-7000eb066166", "Cancel");
			this.zButtonCancel.UseVisualStyleBackColor = true;
			this.zButtonCancel.Click += new System.EventHandler(this.zButtonCancel_Click);
			// 
			// HMRCAuthorisationForm
			// 
			this.AcceptButton = this.zButtonConnect;
			this.CancelButton = this.zButtonCancel;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 183, true);
			this.ControlBox = false;
			this.Controls.Add(this.zButtonCancel);
			this.Controls.Add(this.zButtonConnect);
			this.Controls.Add(this.zLabelPrompt);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "HMRCAuthorisationForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("12cc8af6-55cf-4522-9712-b6d1977c822c", "HMRC Tax Platform");
			this.Controls.SetChildIndex(this.zLabelPrompt, 0);
			this.Controls.SetChildIndex(this.zButtonConnect, 0);
			this.Controls.SetChildIndex(this.zButtonCancel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel zLabelPrompt;
		private ZArchitecture.GUI.ZButton zButtonConnect;
		private ZArchitecture.GUI.ZButton zButtonCancel;
	}
}
