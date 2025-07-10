using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.H7.GUI;

public class MessageSendingGridColumnLayout : EU.H7.GUI.MessageSendingGridColumnLayout
{
	protected override void AddCountrySpecificColumns(GridColumnLayoutBuilder builder)
	{
		builder.AddColumn<ZDropEditColumnStyleInfo>(AutoMessageSendingObject.Schema.AmendmentReasonCode, 160);
		builder.AddColumn<ZDropEditColumnStyleInfo>(IT.H7.Business.MessageSendingObject.Schema.LegislativeReference, 140);
		builder.AddColumn<ZCalcEditColumnStyleInfo>(IT.H7.Business.MessageSendingObject.Schema.DutyAmount, 100);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(IT.H7.Business.MessageSendingObject.Schema.Currency, 100);
	}
}
