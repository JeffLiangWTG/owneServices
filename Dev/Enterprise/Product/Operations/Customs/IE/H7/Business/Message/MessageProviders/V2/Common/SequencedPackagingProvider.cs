using CargoWise.Customs.IE.MessageContracts.Interfaces;

namespace Enterprise.Customs.IE.H7.Business
{
	public class SequencedPackagingProvider : IPackaging
	{
		public SequencedPackagingProvider(AsycudaPack pack)
		{
			this.pack = pack;
		}

		readonly AsycudaPack pack;

		public string PackageType => null;

		public int? PackageQuantity => pack is null ? 0 : pack.APA_PackQty;

		public string ShippingMarks => pack?.APA_MarksAndNumbers;

		public string SequenceNumber => null;
	}
}
