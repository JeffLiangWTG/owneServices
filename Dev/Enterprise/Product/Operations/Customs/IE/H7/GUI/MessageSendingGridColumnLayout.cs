using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.H7.GUI
{
	public sealed class MessageSendingGridColumnLayout : EU.H7.GUI.MessageSendingGridColumnLayout
	{
		protected override void AddCountrySpecificColumns(GridColumnLayoutBuilder builder)
		{
			builder.AddColumn<ZMultiLineTextBoxColumnInfo>(AutoMessageSendingObject.Schema.AmendmentInvalidationReason, 200, c => c.MaxLengthOverride = 512);
		}
	}
}
