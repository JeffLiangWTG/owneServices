using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class DeclarationCustomsDetailsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var common = DeclarationCustomsDetailsControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(100);
			var ruler2 = layout.CreateRuler(355);
			var ruler3 = layout.CreateRuler(223);

			layout.Include(ruler1, common.CustomsOfficeCodeFindBox, ruler2, common.DepartmentCodeFindBox);
			layout.Include(ruler1, common.DepartureCountryCodeFindBox, ruler2, common.ContainerPackDropEdit);
			layout.Include(ruler1, common.BondedAreaCodeFindBox, ruler2, common.LocationIDInBondedAreaTextBox);
			layout.Include(ruler1, common.UnderbondMovementArrivalDateEdit, ruler3, common.CustomsBrokerCommentUserControl);
			layout.Include(ruler1, common.SouthNorthTradeTypeDropEdit, ruler2, common.GoldTradeTransactionYNDropEdit);

			return layout;
		}
	}
}
