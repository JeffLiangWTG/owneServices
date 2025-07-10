using System;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class ErtsReleaseUserControl : ZUserControl
	{
		public ErtsReleaseUserControl(NonPersistentErtsReleaseOrchestrator orchestrator, Action closeMethod)
		{
			this.orchestrator = orchestrator;
			closeFormMethod = closeMethod;
			base.SetDataBinding(this.orchestrator.ErtsReleaseHelper, "");
			InitializeComponent();
			partialReleaseHelpLink.Text = Res.GetString("c9217116-eaf4-4342-982b-2020156921ce", "I want to do a Partial Release");
		}

		public void ReleaseNowButton_Click(object sender, EventArgs e)
		{
			if (orchestrator.ReleaseAndPrintRRAOnFsnOrAwb(fsnMessage: null) && closeFormMethod != null)
			{
				closeFormMethod();
			}
		}

		readonly NonPersistentErtsReleaseOrchestrator orchestrator;
		readonly Action closeFormMethod;

		void partialReleaseHelpLink_Click(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Globals.Message.ShowInformation(Res.GetString("1EE3C336-2239-4683-B97A-8FB5DB718332",
@"If you want to do a partial release, follow these steps. 

Cancel this popup, cancel the ERTS Release form, select the
receipt row for which you want to do a partial release, 
right-click in its gutter, click the DIVIDE option, and 
follow the instructions. This will divide the row into two,
allowing release of only one of them."));
		}
	}
}
