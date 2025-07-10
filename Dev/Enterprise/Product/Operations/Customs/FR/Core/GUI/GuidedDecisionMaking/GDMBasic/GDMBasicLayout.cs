using Enterprise.Customs.FR.Business.GDM;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.GDM
{
	public class GDMBasicLayout : IPanelLayoutProvider
	{
		public GDMBasicLayout()
		{
			Layout = CreateGDMBasicLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateGDMBasicLayout()
		{
			var builder = new EU.GUI.GDMBasicLayoutBuilder<GuidedDecisionMakingBasic>();
			var commonBag = EU.GUI.GDMBasicControlBag.Instance;
			var frBag = GDMBasicControlBag.Instance;
			builder.AddControlBag(commonBag);
			builder.AddControlBag(frBag);

			builder.AddColumn();
			builder.Add(commonBag.EffectiveDateDateEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.TariffCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.CountryOfOriginDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfDestinationDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.PreferenceDropEdit, ControlWidthClass.Auto);
			builder.Add(frBag.RegionOrTerritoryOfDestinationDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.QuotaOrderNumberDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsFirstQuantityCalcEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.AddControlBehaviour<TariffFindBox>(commonBag.TariffCodeFindBox, (tariffCodeFindBox, gdmBasic) =>
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
