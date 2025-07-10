using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalNCTS5PreviousG4Wrapper : IArrivalNCTS5PreviousG4
	{
		public ArrivalNCTS5PreviousG4Wrapper(ZShort seqNum, ZString mrn)
		{
			SequenceNumber = seqNum.ToString();
			PreviousG4MRN = mrn;
		}

		public ZString SequenceNumber { get; }

		public ZString PreviousG4MRN { get; }
	}
}
