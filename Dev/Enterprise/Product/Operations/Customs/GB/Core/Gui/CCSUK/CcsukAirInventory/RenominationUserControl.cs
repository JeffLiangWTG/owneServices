using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class RenominationUserControl : ZUserControl
	{
		public RenominationUserControl(NonPersistentRenominationOrchestrator orchestrator, VoidMethodToClose closeMethod)
		{
			this.orchestrator = orchestrator;
			closeFormMethod = closeMethod;
			base.SetDataBinding(this.orchestrator.Renomination, "");
			InitializeComponent();
#if DEBUG
			System.ComponentModel.TypeDescriptor.AddAttributes(NewAgentCodeFind, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		void ButtonGenerate_Click(object sender, System.EventArgs e)
		{
			var sentWithoutNotifications = orchestrator.RenominateToAgentAndMaybeSendGenral(new Customs.GUI.SendsMessagesToCustomsGUI());
			if (sentWithoutNotifications && closeFormMethod != null)
			{
				closeFormMethod();
			}
		}

		public delegate void VoidMethodToClose();
		readonly NonPersistentRenominationOrchestrator orchestrator;
		readonly VoidMethodToClose closeFormMethod;
	}
}
