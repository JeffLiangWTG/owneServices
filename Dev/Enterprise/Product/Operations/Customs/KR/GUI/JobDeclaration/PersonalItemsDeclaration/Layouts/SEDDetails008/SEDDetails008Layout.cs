using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class SEDDetails008Layout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var common = SEDDetails008ControlBag.Instance;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(130);

			layout.Include(ruler1, common.StayPeriodDropEdit);
			layout.Include(ruler1, common.CustomsOfficeCodeFindBox);
			layout.Include(ruler1, common.DepartmentCodeFindBox);
			layout.Include(ruler1, common.HasItemsDropEdit);
			layout.Include(ruler1, common.WeaponDropEdit);
			layout.Include(ruler1, common.DrugDropEdit);
			layout.Include(ruler1, common.AnimalsDropEdit);
			layout.Include(ruler1, common.EndangeredItemsDropEdit);
			layout.Include(ruler1, common.CounterfeitDropEdit);
			layout.Include(ruler1, common.CommercialUseItemsDropEdit);
			layout.Include(ruler1, common.ExcessTimeLimitItemsDropEdit);
			layout.Include(ruler1, common.PornographyDropEdit);
			layout.Include(ruler1, common.BranchCodeGuidFindBox);
			layout.Include(ruler1, common.BrokerCodeFindBox);
			layout.Include(ruler1, common.ServiceCodeFindBox);

			return layout;
		}
	}
}
