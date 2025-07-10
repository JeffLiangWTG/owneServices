using Enterprise.Customs.BE.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.NCTS.GUI;

public sealed class Phase5ArrivalNotificationDetailsLayout : IPanelLayoutProvider
{
	public Phase5ArrivalNotificationDetailsLayout()
	{
		Layout = CreateLayout();
	}

	PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	static PanelLayout CreateLayout()
	{
		var builder = new ArrivalNotificationDetailsLayoutBuilder<NctsHeader>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
		builder.Add(commonBag.LocalReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.MrnTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.DestinationCustomsOfficeCodeCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.ArrivalDateDateTimeOffsetEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.AuthorizationCodeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.NumberCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.OwnerZGuidFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.LocationOfGoodsUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.IncidentFlagDropEdit, ControlWidthClass.Long);

		builder.Add(commonBag.NationalInfoSeparatorUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(commonBag.CommunicationLanguageDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.DischargeTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.CarnetTotalPagesDropEdit, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(commonBag.DestinationTraderDocAddressControl, ControlWidthClass.Auto);

		return builder.Build();
	}
}
