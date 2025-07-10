using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class DangerousGoodsDataProvider : IDangerousGoods
{
	public static IEnumerable<DangerousGoodsDataProvider> NewCollection(UNDGDataItemCollection undgDataItems) => undgDataItems?.Where(u => !u.UNDGSubstance?.DG_UNNO.IsEmpty ?? false).Select((item, index) => new DangerousGoodsDataProvider(item, index + 1)) ?? Enumerable.Empty<DangerousGoodsDataProvider>();

	DangerousGoodsDataProvider(UNDGDataItem undgDataItem, int sequenceNumber)
	{
		this.undgDataItem = undgDataItem;
		this.sequenceNumber = sequenceNumber;
	}
	readonly UNDGDataItem undgDataItem;
	readonly int sequenceNumber;

	public int SequenceNumber => sequenceNumber;

	public string UNNumber => undgDataItem.UNDGSubstance.DG_UNNO.PadLeft(4, '0');
}
