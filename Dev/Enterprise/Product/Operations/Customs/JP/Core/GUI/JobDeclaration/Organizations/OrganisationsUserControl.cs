using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class OrganisationsUserControl : ZUserControl
	{
		public OrganisationsUserControl()
		{
			InitializeComponent();

			ExternalBrokerDocAddressControl.DataSourceType = typeof(Business.JobDeclaration);
			InspectionWitnessDocAddressControl.DataSourceType = typeof(Business.JobDeclaration);
			ForwarderDocAddressControl.DataSourceType = typeof(Business.JobDeclaration);
			AirCargoAgentDocAddressControl.DataSourceType = typeof(Business.JobDeclaration);
			AttorneyForCustomsProcedureDocAddressControl.DataSourceType = typeof(Business.JobDeclaration);
		}
	}
}
