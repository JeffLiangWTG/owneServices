using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.NCTS.GUI
{
	sealed class Phase5MessageSendingGridColumnLayout : IGridColumnLayoutProvider
	{
		public Phase5MessageSendingGridColumnLayout(BaseMessageSendingObjectParent messageSendingObjectParent)
		{
			this.messageSendingObjectParent = Argument.NotNull(messageSendingObjectParent, nameof(messageSendingObjectParent));
		}
		readonly BaseMessageSendingObjectParent messageSendingObjectParent;

		#region IGridColumnLayoutProvider

		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		#endregion

		IGridColumnLayout CreateLayout()
		{
			var euBag = EU.NCTS.GUI.Phase5MessageSendingGridColumnBag.Instance;
			var ieBag = Phase5MessageSendingGridColumnBag.Instance;
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euBag.ShouldSendCheckBoxColumn);
			builder.AddColumn(euBag.LrnTextBoxColumn);
			builder.AddColumn(euBag.MrnTextBoxColumn);
			builder.AddColumn(euBag.MessageTypeDropEditColumn);

			if (messageSendingObjectParent is EU.NCTS.Business.NctsHeaderMessageSendingObjectParent nctsHeaderMessageSendingObjectParent)
			{
				if (nctsHeaderMessageSendingObjectParent.SupportReleaseRequest)
				{
					builder.AddColumn(euBag.ReleaseRequestDropEditColumn);
				}

				if (nctsHeaderMessageSendingObjectParent.SupportJustification)
				{
					builder.AddColumn(euBag.JustificationTextBoxColumn);
				}
			}

			builder.AddColumn(ieBag.MessageStatusTextBoxColumn);
			builder.AddColumn(ieBag.DestinationCustomsOfficeCodeFindBoxColumn);
			builder.AddColumn(ieBag.ConsigneeFindBoxColumn);
			builder.AddColumn(ieBag.TC11DeliveryDateColumn);
			builder.AddColumn(ieBag.EnquiryTextBoxColumn);
			return builder.Build();
		}
	}
}
