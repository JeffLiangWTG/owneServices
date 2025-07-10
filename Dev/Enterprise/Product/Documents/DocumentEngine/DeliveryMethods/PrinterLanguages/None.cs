using CargoWise.Types;

namespace Enterprise.DocumentEngine.DeliveryMethods.PrinterLanguages
{
	public class None : Base
	{
		protected internal override ZBlob GetNonZeroPaperFeedSequence(int twentyFourthsOfAnInchToFeed)
		{
			return null;
		}
	}
}
