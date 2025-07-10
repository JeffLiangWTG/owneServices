using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
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
			builder.Add(commonBag.BuyerOrganisationGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.SellerAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.ManufacturerAddressControl, ControlWidthClass.Long);
			builder.Add(euBag.DefermentPartyDocAddressControl, ControlWidthClass.Long);
			builder.Add(euBag.DutyPayerGuidFindBox, ControlWidthClass.Long);

			builder.SetVisibility(commonBag.BuyerOrganisationGuidFindBox, jobDec => jobDec.IsImport, jobDec => jobDec.JE_MessageTypeInfo);
			builder.SetVisibility(commonBag.SellerAddressControl, jobDec => !jobDec.IsExport, jobDec => jobDec.JE_MessageTypeInfo);

			return builder.Build();
		}
	}
}
