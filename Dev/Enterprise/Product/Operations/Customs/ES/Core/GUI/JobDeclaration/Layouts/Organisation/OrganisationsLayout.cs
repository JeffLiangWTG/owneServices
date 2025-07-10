using Enterprise.Customs.ES.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public sealed class OrganisationsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public OrganisationsLayout()
		{
			Layout = CreateOrganisationsTypeLayout();
		}

		static PanelLayout CreateOrganisationsTypeLayout()
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

			builder.SetVisibility(commonBag.BuyerOrganisationGuidFindBox, jobDec => jobDec.IsImport && DeclarationConfiguration.HasImportUCC6Functionality(), jobDec => jobDec.JE_MessageTypeInfo);
			builder.SetVisibility(euBag.DutyPayerGuidFindBox, jobDec => jobDec.IsImport && DeclarationConfiguration.HasImportUCC6Functionality(), jobDec => jobDec.JE_MessageTypeInfo);

			return builder.Build();
		}
	}
}
