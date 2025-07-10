using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI
{
	public sealed class MessageSendingGridColumnLayout : EU.H7.GUI.MessageSendingGridColumnLayout
	{
		protected override void AddCountrySpecificColumns(GridColumnLayoutBuilder builder)
		{
			builder.AddColumn<ZDropEditColumnStyleInfo>(AutoMessageSendingObject.Schema.QueryType, 180, (info) => info.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode);
			builder.AddColumn<ZDropEditColumnStyleInfo>(AutoMessageSendingObject.Schema.AmendmentReasonCode, 100);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.AmendmentInvalidationReason, 180);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.EntryType, 150);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoMessageSendingObject.Schema.ReferenceNumber, 150);
		}
	}
}
