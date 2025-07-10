using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class UnloadingDifferencesDeclaredValueLayout : IPanelLayoutProvider
	{
		public UnloadingDifferencesDeclaredValueLayout()
		{
			Layout = CreateUnloadingDifferencesDeclaredValueLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateUnloadingDifferencesDeclaredValueLayout()
		{
			var builder = new UnloadingDifferencesDetailsLayoutBuilder<NctsArrivalMovementHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.DeclaredValueLabel, ControlWidthClass.Auto);
			builder.Add(commonBag.TotalGrossMassDeclaredValueCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.TotalPackagesDeclaredValueCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.InlandTransportModeDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
