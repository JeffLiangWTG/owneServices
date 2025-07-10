using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class MessageAndCustomsStatusWithOverrideLayout : IPanelLayoutProvider
{
	public MessageAndCustomsStatusWithOverrideLayout()
	{
		entryInstuctionLayout = CreateLayout();
	}

	public PanelLayout Layout => entryInstuctionLayout;

	readonly PanelLayout entryInstuctionLayout;

	PanelLayout CreateLayout()
	{
		var builder = new MessageAndCustomsStatusWithOverrideLayoutBuilder();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.MessageAndCustomsStatusGroupBox, widthClass: ControlWidthClass.LongControl);

		return builder.Build();
	}
}
