using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public sealed class OrganizationsLayout : IPanelLayoutProvider
	{
		PanelLayout Organization { get; }

		public PanelLayout Layout => Organization;

		public OrganizationsLayout()
		{
			Organization = CreateOrganisationsLayout();
		}

		PanelLayout CreateOrganisationsLayout()
		{
			var builder = new CommonOrganisationsLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.ShippingOrAirLineOrganisationGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ForwarderOrganisationGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ContainerTerminalOperatorAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.DepotAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.ContainerYardAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.BondedWarehouseDocAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.ControllingAgentGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ControllingCustomerGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ExternalBrokerGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ConsigneeOrganisationGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.DeclarantOfficeAddressControl, ControlWidthClass.Long);

			builder.SetVisibility(commonBag.ConsigneeOrganisationGuidFindBox, h => h.IsImportExcludingLicense, h => h.JE_MessageTypeInfo);
			builder.SetVisibility(commonBag.DeclarantOfficeAddressControl, h => h.IsExport, h => h.JE_MessageTypeInfo);

			builder.SetCaption(commonBag.ConsigneeOrganisationGuidFindBox, h => h.ConsigneeCaption, h => h.DeclarantTypeInfo);

			return builder.Build();
		}
	}
}
