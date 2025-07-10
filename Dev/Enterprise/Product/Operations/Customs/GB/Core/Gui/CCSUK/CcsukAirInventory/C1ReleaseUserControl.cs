using System;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class C1ReleaseUserControl : ZUserControl
	{
		public C1ReleaseUserControl(NonPersistentC1ReleaseOrchestrator orchestrator, Action closeMethod)
		{
			this.orchestrator = orchestrator;
			closeFormMethod = closeMethod;
			base.SetDataBinding(this.orchestrator.C1ReleaseHelper, "");
			InitializeComponent();
		}

		public void ReleaseNowButton_Click(object sender, EventArgs e)
		{
			orchestrator.ReleaseAndPrintC1OnFsn();
			if (closeFormMethod != null)
			{
				closeFormMethod();
			}
		}

		readonly NonPersistentC1ReleaseOrchestrator orchestrator;
		readonly Action closeFormMethod;
	}
}
