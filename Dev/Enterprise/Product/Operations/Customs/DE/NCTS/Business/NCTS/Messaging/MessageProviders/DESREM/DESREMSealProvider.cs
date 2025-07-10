using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class DESREMSealProvider : IDESREMSeal
	{
		public static DESREMSealProvider NewOrNull(CusSeal seal, int sequenceNumber) => seal != null ? new DESREMSealProvider(seal, sequenceNumber) : null;

		DESREMSealProvider(CusSeal seal, int sequenceNumber)
		{
			this.seal = seal;
			SequenceNumber = sequenceNumber;
		}

		public int SequenceNumber { get; }

		public string Identifier => seal.BK_SealNumber;

		readonly CusSeal seal;
	}
}
