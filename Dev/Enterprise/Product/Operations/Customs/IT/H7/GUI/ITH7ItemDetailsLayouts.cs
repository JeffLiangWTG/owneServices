using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.H7.GUI;
public class ITH7ItemDetailsLayouts : IPanelLayoutProvider
{
	public ITH7ItemDetailsLayouts()
	{
		Layout = CreateLayout();
	}
	public PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	PanelLayout CreateLayout()
	{
		var builder = new EUH7ItemDetailsControlLayoutBuilder<AsycudaPackedItem>();
		var common = builder.CommonBag;

		builder.AddColumn();
		builder.Add(common.TariffFindBox, ControlWidthClass.Auto);
		builder.Add(common.IntrinsicValueConvertToLocalCurrencyControl, ControlWidthClass.Auto);
		builder.Add(common.SupplementaryDropEdit, ControlWidthClass.Auto);
		builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(common.CustomEntriesSeparatorUserControl, ControlWidthClass.Auto);
		builder.Add(common.CustomEntriesGrid, ControlWidthClass.Auto);

		builder.SetCaption(common.IntrinsicValueConvertToLocalCurrencyControl, _ => Res.GetData("5f02928a-a1a4-448c-8313-53de914bd114", "Intrinsic Value"));
		builder.SetCaption(common.SupplementaryDropEdit, _ => Res.GetData("ea0d980a-6cbe-4fa8-83fb-5e6f1b13eef0", "Suppl. Units"));

		return builder.Build();
	}
}
