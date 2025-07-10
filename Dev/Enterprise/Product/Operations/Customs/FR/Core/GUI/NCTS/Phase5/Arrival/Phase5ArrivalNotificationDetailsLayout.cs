using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
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
			var builder = new EU.NCTS.GUI.ArrivalNotificationDetailsLayoutBuilder<NctsHeader>();
			var euBag = builder.CommonBag;
			var frBag = ArrivalNotificationDetailsControlBag.Instance;
			builder.AddControlBag(frBag);
			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(euBag.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
			builder.Add(euBag.LocalReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(euBag.MrnTextBox, ControlWidthClass.Long);
			builder.Add(euBag.DestinationCustomsOfficeCodeCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.ArrivalDateDateTimeOffsetEdit, ControlWidthClass.Medium);
			builder.Add(euBag.AuthorizationCodeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.NumberCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.LocationOfGoodsUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.IncidentFlagDropEdit, ControlWidthClass.Long);
			builder.Add(frBag.ExpectedNextCustomsProcedureDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(euBag.DestinationTraderDocAddressControl, ControlWidthClass.Auto);
			return builder.Build();
		}
	}
}
