using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class UnloadingDetailsLayout : IPanelLayoutProvider
	{
		public UnloadingDetailsLayout()
		{
			Layout = CreateLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new UnloadingDetailsLayoutBuilder<Business.NctsArrivalMovementHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.UnloadingDateDateEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.UnloadingConformCheckBox, ControlWidthClass.Long);
			builder.Add(commonBag.StateOfSealsCheckBox, ControlWidthClass.Long);
			builder.Add(commonBag.UnloadingCompletedCheckBox, ControlWidthClass.Long);
			builder.Add(commonBag.UnloadingRemarksTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.OtherThingsToReportTextBox, ControlWidthClass.Long);
			return builder.Build();
		}
	}
}
