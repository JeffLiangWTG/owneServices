using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsDepartureCargoDescAttachmentPrintingSupporter : IAttachmentPrintingSupporter
{
	public NctsDepartureCargoDescAttachmentPrintingSupporter(NctsDepartureCargoDesc goodsItem)
	{
		this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
	}

	readonly NctsDepartureCargoDesc goodsItem;

	ZBool IAttachmentPrintingSupporter.RequiresAttachment
	{
		get
		{
			if (!requiresAttachment.HasValue)
			{
				requiresAttachment = !goodsItem.Remarks.IsEmpty;
			}
			return requiresAttachment.Value;
		}
	}

	bool? requiresAttachment;
}
