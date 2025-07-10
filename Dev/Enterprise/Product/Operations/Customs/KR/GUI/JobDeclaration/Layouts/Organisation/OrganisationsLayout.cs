using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class OrganisationsLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public OrganisationsLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var builder = new OrganisationsLayoutBuilder();
			var common = builder.CommonBag;

			var krBag = OrganisationsControlBag.Instance;
			builder.AddControlBag(krBag);

			builder.AddColumn();
			builder.Add(krBag.SupplierAddressControl, ControlWidthClass.Long);
			builder.Add(krBag.ImporterAddressControl, ControlWidthClass.Long);
			builder.Add(krBag.PayerGuidFindBox, ControlWidthClass.Long);
			builder.Add(common.ManufacturerAddressControl, ControlWidthClass.Long);
			builder.Add(krBag.ManufacturerGuidFindBox, ControlWidthClass.Long);
			builder.Add(krBag.IndustrialParkCodeCodeFindBox, ControlWidthClass.Long);
			builder.Add(krBag.ExpoterAddressControl, ControlWidthClass.Long);
			builder.Add(krBag.ExporterGuidFindBox, ControlWidthClass.Long);
			builder.Add(common.ShippingOrAirLineOrganisationGuidFindBox, ControlWidthClass.Long);
			builder.Add(common.ForwarderOrganisationGuidFindBox, ControlWidthClass.Long);
			builder.Add(common.BondedWarehouseDocAddressControl, ControlWidthClass.Long);
			builder.Add(common.ContainerTerminalOperatorAddressControl, ControlWidthClass.Long);
			builder.Add(krBag.FinalBondedWarehouseCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.DepotAddressControl, ControlWidthClass.Long);
			builder.Add(common.ContainerYardAddressControl, ControlWidthClass.Long);
			builder.Add(common.ControllingAgentGuidFindBox, ControlWidthClass.Long);
			builder.Add(common.ControllingCustomerGuidFindBox, ControlWidthClass.Long);
			builder.Add(common.ExternalBrokerGuidFindBox, ControlWidthClass.Long);
			builder.Add(krBag.StevedoreAddressControl, ControlWidthClass.Long);
			var layout = builder.Build();

			layout.Include(krBag.AuthorGroupBox);
			layout.Include(krBag.ResponsiblePersonGroupBox);

			return layout;
		}
	}
}
