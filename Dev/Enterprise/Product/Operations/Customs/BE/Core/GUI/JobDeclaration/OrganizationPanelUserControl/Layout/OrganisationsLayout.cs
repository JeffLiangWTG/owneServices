using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public sealed class OrganisationsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; } = CreateLayout();

	static PanelLayout CreateLayout()
	{
		var builder = new OrganisationsLayoutBuilder();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.OrganisationsControlBag.Instance;
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
		builder.Add(commonBag.ConsigneeAddressControl, ControlWidthClass.Long);
		builder.Add(euBag.ExporterDocAddressControl, ControlWidthClass.Long);
		builder.Add(euBag.ContractualPartnerDocAddressControl, ControlWidthClass.Long);
		builder.Add(euBag.CarrierEUBorderDocAddressControl, ControlWidthClass.Long);
		builder.Add(euBag.DutyPayerGuidFindBox, ControlWidthClass.Long);

		return builder.Build();
	}
}
