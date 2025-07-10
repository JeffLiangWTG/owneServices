using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class GDMBasicLayout : IPanelLayoutProvider
	{
		PanelLayout GDMBasic { get; }

		PanelLayout IPanelLayoutProvider.Layout => GDMBasic;

		public GDMBasicLayout()
		{
			GDMBasic = CreateGDMBasicLayout();
		}

		PanelLayout CreateGDMBasicLayout()
		{
			var builder = new GDMBasicLayoutBuilder<GuidedDecisionMakingBasic>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.EffectiveDateDateEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.TariffCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.CountryOfOriginDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfDestinationDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.PreferenceDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.QuotaOrderNumberDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsFirstQuantityCalcEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Medium);

			builder.AddControlBehaviour<Universal.GUI.TariffFindBox>(commonBag.TariffCodeFindBox, (tariffCodeFindBox, gdmBasic) =>
			{
				CargoWise.Windows.UI.ControlDpiScalingHelper.SetWidth(tariffCodeFindBox, 800, true);
			});
			builder.AddControlBehaviour<ZDropEdit>(commonBag.PreferenceDropEdit, (preferenceDropEdit, gdmBasic) =>
			{
				CargoWise.Windows.UI.ControlDpiScalingHelper.SetWidth(preferenceDropEdit, 800, true);
			});

			return builder.Build();
		}
	}
}
