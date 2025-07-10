using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS
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
