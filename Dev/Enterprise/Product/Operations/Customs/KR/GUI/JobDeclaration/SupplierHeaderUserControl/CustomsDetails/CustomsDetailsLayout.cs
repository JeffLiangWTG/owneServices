using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class CustomsDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public CustomsDetailsLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var builder = new CustomsDetailsLayoutBuilder();
			var bag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(bag.BillZGuidDropEdit, ControlWidthClass.Auto);
			builder.Add(bag.CargoManagementNoTextBox, ControlWidthClass.Auto);
			builder.Add(bag.COStatusDropEdit, ControlWidthClass.Auto);
			builder.Add(bag.ValuationDeclarationStatusDropEdit, ControlWidthClass.Auto);
			builder.Add(bag.BlanketValuationDeclarationNoTextBox, ControlWidthClass.Auto);
			builder.Add(bag.CustomsBrokerCommentMultiTextBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(bag.SupplierZOrganisationFindBox, ControlWidthClass.Auto);
			builder.Add(bag.ShipperZAddressControl, ControlWidthClass.Auto);
			builder.Add(bag.EmptyLabel, ControlWidthClass.Auto);
			builder.Add(bag.OnlineTradeTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(bag.OnlineTradeDistributorZAddressControl, ControlWidthClass.Auto);
			builder.Add(bag.OnlineTradeSellerZAddressControl, ControlWidthClass.Auto);
			builder.Add(bag.OnlineTradeSellingAgentZOrganisationFindBox, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
