using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.NCTS.Business
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
