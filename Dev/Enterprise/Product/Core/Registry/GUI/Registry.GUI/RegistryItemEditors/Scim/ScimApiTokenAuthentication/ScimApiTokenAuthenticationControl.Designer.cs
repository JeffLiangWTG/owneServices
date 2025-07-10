using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI;
partial class ScimApiTokenAuthenticationControl
{
	/// <summary> 
	/// Required designer variable.
	/// </summary>
	System.ComponentModel.IContainer components = null;

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
		this.tbApiToken = new Enterprise.ZArchitecture.ZTextBox();
		this.bGenerateNew = new Enterprise.ZArchitecture.GUI.ZButton();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(CargoWise.Types.ZString);
		// 
		// tbApiToken
		// 
		this.tbApiToken.Enabled = true;
		this.tbApiToken.ReadOnly = true;
		this.tbApiToken.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
		this.tbApiToken.Name = "tbApiToken";
		this.tbApiToken.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 17, true);
		this.tbApiToken.TabIndex = 0;
		this.tbApiToken.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("113C961D-A773-494B-91C1-06090D589763", "API Token");
		// 
		// bGenerateNew
		// 
		this.bGenerateNew.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 25, true);
		this.bGenerateNew.Name = "bGenerateNew";
		this.bGenerateNew.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 19, true);
		this.bGenerateNew.TabIndex = 1;
		this.bGenerateNew.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("21958932-7E74-4A7A-A420-B432B26D986B", "Generate New API Token");
		this.bGenerateNew.ToolTipCaption = null;
		this.bGenerateNew.UseVisualStyleBackColor = true;
		this.bGenerateNew.Click += new System.EventHandler(this.bGenerateNew_Click);
		// 
		// ScimApiTokenAuthenticationControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.bGenerateNew);
		this.Controls.Add(this.tbApiToken);
		this.Name = "ScimApiTokenAuthenticationControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 400, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	#endregion

	protected ZTextBox tbApiToken;
	private ZButton bGenerateNew;
}
