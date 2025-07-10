using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public class OrganizationsLayouts : IPanelLayoutProvider
	{
		public OrganizationsLayouts()
		{
			Organisations = CreateOrganisationsLayout();
		}

		PanelLayout Organisations { get; }

		public PanelLayout Layout => Organisations;

		PanelLayout CreateOrganisationsLayout()
		{
			var builder = new OrganizationsLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;
			var jpBag = OrganisationsControlBag.Instance;
			builder.AddControlBag(jpBag);

			builder.AddColumn();
			builder.Add(jpBag.CarrierGroupBox, ControlWidthClass.LongControl);
			builder.Add(jpBag.ForwarderGroupBox, ControlWidthClass.LongControl);
			builder.Add(commonBag.ContainerTerminalOperatorAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.DepotAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.ContainerYardAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.BondedWarehouseDocAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.ControllingAgentGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ControllingCustomerGuidFindBox, ControlWidthClass.Long);
			builder.Add(jpBag.InspectionWitnessGroupBox, ControlWidthClass.LongControl);
			builder.Add(jpBag.ExternalBrokerGroupBox, ControlWidthClass.LongControl);
			builder.Add(jpBag.DeclarationConsignorAddressControl, ControlWidthClass.Long);
			builder.Add(jpBag.DeclarationConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(jpBag.AttorneyForCustomsProcedureGroupBox, ControlWidthClass.LongControl);
			builder.Add(jpBag.AirCargoAgentGroupBox, ControlWidthClass.LongControl);

			return builder.Build();
		}
	}
}
