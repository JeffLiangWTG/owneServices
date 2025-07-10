using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public class ArrivalNotificationDetailsLayout : IPanelLayoutProvider
{
	public ArrivalNotificationDetailsLayout()
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
		builder.Add(commonBag.LocationOfGoodsUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.CommunicationLanguageDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ArrivalDateDateTimeOffsetEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.DestinationCustomsOfficeCodeCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.StateOfSealsDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.AdditionalTextTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.IncidentFlagDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.TransportMeansLabel, ControlWidthClass.LongNoCaption);
		builder.Add(commonBag.TransportAtArrivalTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.TransportAtArrivalIDTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.TransportNationalityCodeFindBox, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(commonBag.DestinationTraderDocAddressControl, ControlWidthClass.Auto);

		builder.SetVisibility(commonBag.MrnTextBox, x => !x.ArrivalMovementHeader.MultipleMRNIndicator);
		builder.SetVisibility(commonBag.StateOfSealsDropEdit, x => !x.ArrivalMovementHeader.MultipleMRNIndicator);
		builder.SetVisibility(commonBag.AdditionalTextTextBox, x => !x.ArrivalMovementHeader.MultipleMRNIndicator);

		return builder.Build();
	}
}
