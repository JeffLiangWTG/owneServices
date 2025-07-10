using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class OrganisationsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; } = CreateOrganisationsLayout();

	static PanelLayout CreateOrganisationsLayout()
	{
		var builder = new CommonOrganisationsLayoutBuilder<JobDeclaration>();
		var commonBag = builder.CommonBag;

		var chBag = OrganisationsControlBag.Instance;
		builder.AddControlBag(chBag);

		builder.AddColumn();
		builder.Add(commonBag.ConsigneeOrganisationGuidFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.RepresentativeAddressControl, ControlWidthClass.Long);
		builder.Add(commonBag.DeclarantOfficeAddressControl, ControlWidthClass.Long);
		builder.Add(chBag.ConsignorDocAddressControl, ControlWidthClass.Long);
		builder.Add(commonBag.ShippingOrAirLineOrganisationGuidFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.BondedWarehouseDocAddressControl, ControlWidthClass.Long);
		builder.Add(commonBag.ForwarderOrganisationGuidFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.ContainerTerminalOperatorAddressControl, ControlWidthClass.Long);
		builder.Add(commonBag.DepotAddressControl, ControlWidthClass.Long);
		builder.Add(commonBag.ContainerYardAddressControl, ControlWidthClass.Long);
		builder.Add(commonBag.ControllingAgentGuidFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.ControllingCustomerGuidFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.ExternalBrokerGuidFindBox, ControlWidthClass.Long);

		builder.SetVisibility(commonBag.RepresentativeAddressControl, h => h.IsImport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(commonBag.ConsigneeOrganisationGuidFindBox, h => h.IsImport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(chBag.ConsignorDocAddressControl, h => h.IsExportOrExportDeclarationActivation, h => h.JE_MessageTypeInfo);

		return builder.Build();
	}
}
