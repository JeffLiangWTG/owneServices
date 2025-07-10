namespace Enterprise.FaxRouter.Manager
{
	public partial class EDIFaxGatewayForm
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(EDIFaxGatewayForm));
			this.FaxGatewayTabControl = new System.Windows.Forms.TabControl();
			this.StatusTabPage = new System.Windows.Forms.TabPage();
			this.FaxManagementTabPage = new System.Windows.Forms.TabPage();
			this.ErrorLogTabPage = new System.Windows.Forms.TabPage();
			this.FaxGatewayTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// FaxGatewayTabControl
			// 
			this.FaxGatewayTabControl.Controls.AddRange(new System.Windows.Forms.Control[] {
																							   this.StatusTabPage,
																							   this.FaxManagementTabPage,
																							   this.ErrorLogTabPage });
			this.FaxGatewayTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FaxGatewayTabControl.Name = "FaxGatewayTabControl";
			this.FaxGatewayTabControl.SelectedIndex = 0;
			this.FaxGatewayTabControl.Size = new System.Drawing.Size(666, 368);
			this.FaxGatewayTabControl.TabIndex = 0;
			this.FaxGatewayTabControl.SelectedIndexChanged += new System.EventHandler(this.FaxGatewayTabControl_SelectedIndexChanged);
			// 
			// StatusTabPage
			// 
			this.StatusTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.StatusTabPage.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.StatusTabPage.Location = new System.Drawing.Point(4, 22);
			this.StatusTabPage.Name = "StatusTabPage";
			this.StatusTabPage.Size = new System.Drawing.Size(658, 342);
			this.StatusTabPage.TabIndex = 0;
			this.StatusTabPage.Text = "Gateway Status";
			// 
			// FaxManagementTabPage
			// 
			this.FaxManagementTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.FaxManagementTabPage.Location = new System.Drawing.Point(4, 22);
			this.FaxManagementTabPage.Name = "FaxManagementTabPage";
			this.FaxManagementTabPage.Size = new System.Drawing.Size(658, 342);
			this.FaxManagementTabPage.TabIndex = 2;
			this.FaxManagementTabPage.Text = "Fax Management";
			// 
			// ErrorLogTabPage
			// 
			this.ErrorLogTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.ErrorLogTabPage.Location = new System.Drawing.Point(4, 22);
			this.ErrorLogTabPage.Name = "ErrorLogTabPage";
			this.ErrorLogTabPage.Size = new System.Drawing.Size(658, 342);
			this.ErrorLogTabPage.TabIndex = 1;
			this.ErrorLogTabPage.Text = "Error Log";
			// 
			// EDIFaxGatewayForm
			// 

			this.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(224)), ((System.Byte)(224)), ((System.Byte)(224)));
			this.ClientSize = new System.Drawing.Size(666, 368);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.FaxGatewayTabControl });
			this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "EDIFaxGatewayForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "EDI Fax Gateway";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.EDIFaxGatewayForm_Closing);
			this.FaxGatewayTabControl.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

		System.Windows.Forms.TabPage StatusTabPage;
		System.Windows.Forms.TabControl FaxGatewayTabControl;
		System.Windows.Forms.TabPage ErrorLogTabPage;
		FaxManagementUserControl fFaxManagmentUserControl;
		System.Windows.Forms.TabPage FaxManagementTabPage;
		GatewayStatusUserControl fGatewayStatusUserControl;
	}
}
