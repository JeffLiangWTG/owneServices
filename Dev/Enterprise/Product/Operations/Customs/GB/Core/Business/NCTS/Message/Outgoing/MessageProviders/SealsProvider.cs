using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class SealsProvider : ISeal
	{
		public SealsProvider(string identifier, int sequenceNumber)
		{
			Identifier = identifier;
			SequenceNumber = sequenceNumber;
		}

		public int SequenceNumber { get; }

		public string Identifier { get; }
	}
}
