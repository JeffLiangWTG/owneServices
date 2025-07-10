using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class CDB01EntryInstructionLayoutTemplate : ZUserControl
	{
		public CDB01EntryInstructionLayoutTemplate()
		{
			InitializeComponent();
			this.ExternalBrokerDocAddressControl.DataSourceType = typeof(Business.JobDeclaration);
			this.ForwarderDocAddressControl.DataSourceType = typeof(Business.JobDeclaration);
			this.AirCargoAgentDocAddressControl.DataSourceType = typeof(Business.JobDeclaration);
		}
	}
}
