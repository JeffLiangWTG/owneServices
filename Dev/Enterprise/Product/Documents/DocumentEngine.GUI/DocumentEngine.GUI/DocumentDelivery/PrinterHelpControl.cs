using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocumentDelivery
{
	public partial class PrinterHelpControl : ZUserControl
	{
		public PrinterHelpControl()
		{
			InitializeComponent();
			SetzLinkLabel1LinkLength();
		}

		protected internal const string printingHelpUrl = "http://myaccount.cargowise.com/my-account/Documents/UserGuides/SupportingDocs/FAQ_PrintingEmailingAndFaxing.pdf";

		void zLinkLabel1_Click(object sender, EventArgs e)
		{
			ShowHelp();
		}

		void pictureBox1_Click(object sender, EventArgs e)
		{
			ShowHelp();
		}

		void ShowHelp()
		{
			WebUrlLauncher.Launch(printingHelpUrl);
		}

		void SetzLinkLabel1LinkLength()
		{
			this.zLinkLabel1.LinkArea = new System.Windows.Forms.LinkArea(this.zLinkLabel1.CaptionResourceString.Caption.Split(':')[0].Length + 2, 45);
		}
	}
}
