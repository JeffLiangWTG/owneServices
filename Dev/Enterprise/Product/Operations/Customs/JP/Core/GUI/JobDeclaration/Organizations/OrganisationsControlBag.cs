using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public class OrganisationsControlBag : ControlBag
	{
		public OrganisationsControlBag()
		{
			DeclarationConsignorAddressControl = RegisterControl(nameof(DeclarationConsignorAddressControl));
			DeclarationConsigneeAddressControl = RegisterControl(nameof(DeclarationConsigneeAddressControl));
			CarrierGroupBox = RegisterControl(nameof(CarrierGroupBox));
			AttorneyForCustomsProcedureGroupBox = RegisterControl(nameof(AttorneyForCustomsProcedureGroupBox));
			InspectionWitnessGroupBox = RegisterControl(nameof(InspectionWitnessGroupBox));
			ExternalBrokerGroupBox = RegisterControl(nameof(ExternalBrokerGroupBox));
			AirCargoAgentGroupBox = RegisterControl(nameof(AirCargoAgentGroupBox));
			ForwarderGroupBox = RegisterControl(nameof(ForwarderGroupBox));
		}

		public ControlReference DeclarationConsignorAddressControl { get; }
		public ControlReference DeclarationConsigneeAddressControl { get; }
		public ControlReference AttorneyForCustomsProcedureGroupBox { get; }
		public ControlReference CarrierGroupBox { get; }
		public ControlReference PowerOfAttorneyTextBox { get; }
		public ControlReference InspectionWitnessGroupBox { get; }
		public ControlReference ExternalBrokerGroupBox { get; }
		public ControlReference AirCargoAgentGroupBox { get; }
		public ControlReference ForwarderGroupBox { get; }

		protected override Control CreateTemplate() => new OrganisationsUserControl();

		[ThreadStatic]
		static OrganisationsControlBag instance;

		public static OrganisationsControlBag Instance => instance ?? (instance = new OrganisationsControlBag());
	}
}
