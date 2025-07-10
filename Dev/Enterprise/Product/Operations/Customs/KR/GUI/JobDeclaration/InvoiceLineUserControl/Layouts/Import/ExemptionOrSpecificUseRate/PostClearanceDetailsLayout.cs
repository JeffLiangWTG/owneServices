using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class PostClearanceDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var common = PostClearanceDetailsControlBag.InstanceForDeclaration;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(120);
			var ruler2 = layout.CreateRuler(530);

			layout.Include(ruler1, common.PostClearanceYNDropEdit, ruler2, common.UseCodeDescriptionTextBox);
			layout.Include(ruler1, common.ProductTypeDropEdit, ruler2, common.SerialNumberTextBox);
			layout.Include(ruler1, common.CustomsOfficeCodeFindBox, ruler2, common.GoodsLocationAddressControl);

			return layout;
		}
	}
}
