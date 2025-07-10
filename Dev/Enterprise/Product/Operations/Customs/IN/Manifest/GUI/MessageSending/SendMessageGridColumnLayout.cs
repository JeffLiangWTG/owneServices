using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.Manifest.GUI;

sealed class SendMessageGridColumnLayout : IGridColumnLayoutProvider
{
	public SendMessageGridColumnLayout(ManifestMessageSendingObjectParent messageParent)
	{
		this.messageParent = messageParent;
	}

	IGridColumnLayout IGridColumnLayoutProvider.Layout
	{
		get
		{
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn<ZCheckBoxColumnStyleInfo>(ManifestMessageSendingObject.Schema.ShouldSend, 100, c =>
			{
				c.IsMandatory = true;
			});
			builder.AddColumn<ZDropEditColumnStyleInfo>(ManifestMessageSendingObject.Schema.MessageType, 200, c =>
			{
				c.IsMandatory = true;
				c.GroupName = Res.GetData("d4d8530e-4d18-4b28-84ea-807624ab5067", "Message Type and Description");
			});
			builder.AddColumn<ZTextBoxColumnStyleInfo>(ManifestMessageSendingObject.Schema.MessageTypeDescription, 200, c =>
			{
				c.IsMandatory = true;
				c.GroupName = Res.GetData("d4d8530e-4d18-4b28-84ea-807624ab5067", "Message Type and Description");
			});

			builder.AddColumn<ZTextBoxColumnStyleInfo>(ManifestMessageSendingObject.Schema.BillNumber, 200, c =>
			{
				c.IsMandatory = true;
				c.CaptionResourceString = messageParent.ManifestHeader.IsAir
					? Res.GetData("FF7A9F53-54B1-452A-BCCC-9A63484FFD22", "MAWB")
					: Res.GetData("6F647C20-CE09-4EE9-ABEC-DF64CCBD6656", "BOL");
			});
			return builder.Build();
		}
	}

	readonly ManifestMessageSendingObjectParent messageParent;
}
