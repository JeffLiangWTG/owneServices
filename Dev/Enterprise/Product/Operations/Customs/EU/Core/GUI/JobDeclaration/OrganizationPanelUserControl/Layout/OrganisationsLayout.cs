using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class OrganisationsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new OrganisationsLayoutBuilder();
			var commonBag = builder.CommonBag;
			var euBag = OrganisationsControlBag.Instance;
			builder.AddControlBag(euBag);

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
			builder.Add(commonBag.DeclarantOfficeAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.RepresentativeAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.SellerAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.ManufacturerAddressControl, ControlWidthClass.Long);
			builder.Add(euBag.DefermentPartyDocAddressControl, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
