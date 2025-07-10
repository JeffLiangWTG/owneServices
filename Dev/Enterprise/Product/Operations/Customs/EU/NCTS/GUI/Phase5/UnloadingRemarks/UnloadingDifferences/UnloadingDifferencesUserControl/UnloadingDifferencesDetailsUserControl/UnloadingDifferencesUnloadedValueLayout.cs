using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class UnloadingDifferencesUnloadedValueLayout : IPanelLayoutProvider
	{
		public UnloadingDifferencesUnloadedValueLayout()
		{
			Layout = CreateUnloadingDifferencesUnloadedValueLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateUnloadingDifferencesUnloadedValueLayout()
		{
			var builder = new UnloadingDifferencesDetailsLayoutBuilder<NctsArrivalMovementHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.UnloadedValueLabel, ControlWidthClass.Auto);
			builder.Add(commonBag.EffectiveGrossWeightUnloadedCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.TotalPackagesUnloadedValueCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.SeparatorLabel, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(commonBag.RecalculateTotalsButton, ControlWidthClass.LongNoCaption, commonBag.EffectiveGrossWeightUnloadedCalcEdit);

			return builder.Build();
		}
	}
}
