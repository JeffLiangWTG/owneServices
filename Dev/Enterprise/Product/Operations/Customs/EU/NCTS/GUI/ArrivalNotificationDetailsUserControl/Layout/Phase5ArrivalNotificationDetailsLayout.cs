using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5ArrivalNotificationDetailsLayout : IPanelLayoutProvider
	{
		public Phase5ArrivalNotificationDetailsLayout()
		{
			Layout = CreateLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateLayout()
		{
			var builder = new ArrivalNotificationDetailsLayoutBuilder<Business.NctsHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
			builder.Add(commonBag.LocalReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.MrnTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.DestinationCustomsOfficeCodeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ArrivalDateDateTimeOffsetEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.AuthorizationCodeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.NumberCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.LocationOfGoodsUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.IncidentFlagDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.DestinationTraderDocAddressControl, ControlWidthClass.Auto);
			return builder.Build();
		}
	}
}
