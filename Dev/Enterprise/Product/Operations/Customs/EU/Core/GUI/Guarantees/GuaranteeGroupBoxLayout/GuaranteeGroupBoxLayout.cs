using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class GuaranteeGroupBoxLayout : IPanelLayoutProvider
	{
		public GuaranteeGroupBoxLayout()
		{
			Layout = CreateGuaranteeGroupBoxLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateGuaranteeGroupBoxLayout()
		{
			var builder = new GuaranteeGroupBoxLayoutBuilder<CommonGuarantee>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.BondNumberCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.AmountCalcDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
