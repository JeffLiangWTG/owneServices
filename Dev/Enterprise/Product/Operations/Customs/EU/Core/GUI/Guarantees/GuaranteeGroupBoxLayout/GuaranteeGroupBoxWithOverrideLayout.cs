using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class GuaranteeGroupBoxWithOverrideLayout : IPanelLayoutProvider
	{
		public GuaranteeGroupBoxWithOverrideLayout()
		{
			Layout = CreateGuaranteeGroupBoxWithOverrideLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateGuaranteeGroupBoxWithOverrideLayout()
		{
			var builder = new GuaranteeGroupBoxLayoutBuilder<CommonGuarantee>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.BondNumberCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.AmountCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.OverrideCheckBox, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
