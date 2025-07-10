using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class CustomsDetailLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public CustomsDetailLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var layout = new PanelLayout();
			var bag = RefundDeclarationControlBag.Instance;
			layout.RegisterControlBag(bag);

			var ruler1 = layout.CreateRuler(100);
			layout.Include(ruler1, bag.RefundTypeDropEdit);
			layout.Include(ruler1, bag.RefundCauseDropEdit);
			layout.Include(ruler1, bag.RefundReasonDropEdit);
			layout.Include(ruler1, bag.CustomsOfficeCodeFindBox);
			layout.Include(ruler1, bag.DepartmentCodeFindBox);
			layout.Include(ruler1, bag.TaxOfficeCodeFindBox);
			layout.Include(ruler1, bag.BranchCodeGuidFindBox);
			layout.Include(ruler1, bag.BrokerCodeFindBox);
			layout.SetVisibility<CusReconDeclaration>(bag.RefundReasonDropEdit, h => h.IsRefundReasonCodeMandatory, h => h.CRD_RefundReasonCodeInfo);
			return layout;
		}
	}
}
