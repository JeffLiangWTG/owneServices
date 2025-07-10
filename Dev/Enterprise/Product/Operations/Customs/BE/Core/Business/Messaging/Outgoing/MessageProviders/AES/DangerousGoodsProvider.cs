using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business;

public class DangerousGoodsProvider : IDangerousGoods
{
	public DangerousGoodsProvider(UNDGDataItem undgDataItem, int sequenceNumber)
	{
		this.undgDataItem = Argument.NotNull(undgDataItem, nameof(undgDataItem));
		this.sequenceNumber = sequenceNumber;
	}

	protected readonly UNDGDataItem undgDataItem;
	protected readonly int sequenceNumber;

	public int SequenceNumber => sequenceNumber;

	public string UNNumber => undgDataItem.UNDGSubstance.DG_Code;
}
