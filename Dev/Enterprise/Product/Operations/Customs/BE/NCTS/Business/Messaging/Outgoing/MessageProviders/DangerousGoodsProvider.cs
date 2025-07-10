using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class DangerousGoodsProvider : IDangerousGoods
	{
		readonly UNDGDataItem dangerousGood;
		public DangerousGoodsProvider(UNDGDataItem dangerousGood, int sequenceNumber)
		{
			this.dangerousGood = Argument.NotNull(dangerousGood, nameof(dangerousGood));
			this.SequenceNumber = sequenceNumber;
		}

		public int SequenceNumber { get; }

		public string UNNumber => dangerousGood.Substance?.DG_UNNO;
	}
}
