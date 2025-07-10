using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

public class MessageChooser : ASYCUDA.Business.MessageChooser
{
	public MessageChooser(AsycudaManifestHeader header, IEnumerable<ISelectionItem> items, bool showStatus) : base(header, items, showStatus)
	{
	}

	public new MessageChooserItemCollection ChooserItems => (MessageChooserItemCollection)base.ChooserItems;

	protected override ASYCUDA.Business.MessageChooserItemCollection CreateNewMessageChooserItemCollection() => new MessageChooserItemCollection();
}
