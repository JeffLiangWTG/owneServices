using Enterprise.Customs.FR.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.H7.GUI
{
	public sealed class MessageSendingGridColumnLayout : EU.H7.GUI.MessageSendingGridColumnLayout
	{
		protected override void AddCountrySpecificColumns(GridColumnLayoutBuilder builder)
		{
			builder.AddColumn<ZMultiLineTextBoxColumnInfo>(MessageSendingObject.Schema.AmendmentInvalidationReason, 200);
			builder.AddColumn<ZDropEditColumnStyleInfo>(MessageSendingObject.Schema.Motivation, 100);
			builder.AddColumn<ZDateEditColumnStyleInfo>(MessageSendingObject.Schema.ManifestLodgementDateTime, 150);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(MessageSendingObject.Schema.FallbackReferenceNumber, 150);
		}
	}
}
