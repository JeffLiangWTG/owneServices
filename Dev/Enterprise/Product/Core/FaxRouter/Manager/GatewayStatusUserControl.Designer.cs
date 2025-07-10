namespace Enterprise.FaxRouter.Manager
{
	public partial class GatewayStatusUserControl
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
			this.GatewayControlPanel = new System.Windows.Forms.Panel();
			this.NextRunLabel = new System.Windows.Forms.Label();
			this.GatewayTimerLabel = new System.Windows.Forms.Label();
			this.ExpectDeliveryAckCheckBox = new System.Windows.Forms.CheckBox();
			this.ClearButton = new System.Windows.Forms.Button();
			this.StopButton = new System.Windows.Forms.Button();
			this.StartButton = new System.Windows.Forms.Button();
			this.StatusTextBox = new System.Windows.Forms.TextBox();
			this.GatewayTimer = new System.Windows.Forms.Timer(this.components);
			this.GatewayControlPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// GatewayControlPanel
			// 
			this.GatewayControlPanel.BackColor = System.Drawing.SystemColors.Control;
			this.GatewayControlPanel.Controls.AddRange(new System.Windows.Forms.Control[] {
																							  this.NextRunLabel,
																							  this.GatewayTimerLabel,
																							  this.ExpectDeliveryAckCheckBox,
																							  this.ClearButton,
																							  this.StopButton,
																							  this.StartButton });
			this.GatewayControlPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.GatewayControlPanel.Name = "GatewayControlPanel";
			this.GatewayControlPanel.Size = new System.Drawing.Size(752, 40);
			this.GatewayControlPanel.TabIndex = 3;
			// 
			// NextRunLabel
			// 
			this.NextRunLabel.Dock = System.Windows.Forms.DockStyle.Right;
			this.NextRunLabel.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.NextRunLabel.Location = new System.Drawing.Point(618, 0);
			this.NextRunLabel.Name = "NextRunLabel";
			this.NextRunLabel.Size = new System.Drawing.Size(62, 40);
			this.NextRunLabel.TabIndex = 5;
			this.NextRunLabel.Text = "Next Run:";
			this.NextRunLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// GatewayTimerLabel
			// 
			this.GatewayTimerLabel.Dock = System.Windows.Forms.DockStyle.Right;
			this.GatewayTimerLabel.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.GatewayTimerLabel.Location = new System.Drawing.Point(680, 0);
			this.GatewayTimerLabel.Name = "GatewayTimerLabel";
			this.GatewayTimerLabel.Size = new System.Drawing.Size(72, 40);
			this.GatewayTimerLabel.TabIndex = 4;
			this.GatewayTimerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ExpectDeliveryAckCheckBox
			// 
			this.ExpectDeliveryAckCheckBox.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.ExpectDeliveryAckCheckBox.Location = new System.Drawing.Point(266, 8);
			this.ExpectDeliveryAckCheckBox.Name = "ExpectDeliveryAckCheckBox";
			this.ExpectDeliveryAckCheckBox.Size = new System.Drawing.Size(116, 24);
			this.ExpectDeliveryAckCheckBox.TabIndex = 3;
			this.ExpectDeliveryAckCheckBox.Text = "Expect Fax Ack";
			// 
			// ClearButton
			// 
			this.ClearButton.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.ClearButton.Location = new System.Drawing.Point(16, 8);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = new System.Drawing.Size(72, 24);
			this.ClearButton.TabIndex = 2;
			this.ClearButton.Text = "Clear";
			this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
			// 
			// StopButton
			// 
			this.StopButton.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.StopButton.Location = new System.Drawing.Point(185, 8);
			this.StopButton.Name = "StopButton";
			this.StopButton.Size = new System.Drawing.Size(72, 24);
			this.StopButton.TabIndex = 1;
			this.StopButton.Text = "Stop";
			this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
			// 
			// StartButton
			// 
			this.StartButton.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.StartButton.Location = new System.Drawing.Point(98, 8);
			this.StartButton.Name = "StartButton";
			this.StartButton.Size = new System.Drawing.Size(72, 24);
			this.StartButton.TabIndex = 0;
			this.StartButton.Text = "Start";
			this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.StatusTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusTextBox.Location = new System.Drawing.Point(0, 40);
			this.StatusTextBox.Multiline = true;
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.StatusTextBox.Size = new System.Drawing.Size(752, 296);
			this.StatusTextBox.TabIndex = 4;
			this.StatusTextBox.Text = "";
			// 
			// GatewayTimer
			// 
			this.GatewayTimer.Tick += new System.EventHandler(this.GatewayTimer_Tick);
			// 
			// GatewayStatusUserControl
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.StatusTextBox,
																		  this.GatewayControlPanel });
			this.Name = "GatewayStatusUserControl";
			this.Size = new System.Drawing.Size(752, 336);
			this.GatewayControlPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

		System.Windows.Forms.Panel GatewayControlPanel;
		System.Windows.Forms.CheckBox ExpectDeliveryAckCheckBox;
		System.Windows.Forms.Button ClearButton;
		System.Windows.Forms.Button StopButton;
		System.Windows.Forms.Button StartButton;
		System.Windows.Forms.TextBox StatusTextBox;
		System.Windows.Forms.Timer GatewayTimer;
		System.Windows.Forms.Label GatewayTimerLabel;
		System.Windows.Forms.Label NextRunLabel;
		System.DateTime GatewayTimerRunAtDateTime;
	}
}
