using System;

namespace Enterprise.DocumentEngine.DeliveryMethods.PrinterLanguages
{
	public class IbmProPrinter : Base
	{
		protected internal override CargoWise.Types.ZBlob GetNonZeroPaperFeedSequence(int twentyFourthsOfAnInchToFeed)
		{
			if (twentyFourthsOfAnInchToFeed < 0)
			{
				throw new NotImplementedException("Reverse form feed not supported");
			}
			else if (twentyFourthsOfAnInchToFeed % 24 != 0)
			{
				throw new NotImplementedException("Maximum precision for IBM commands is 1 inch");
			}
			else
			{
				byte[] result = new byte[] {
											   0x0d, 0x0d, 0x0d, 0x0d, 0x0d, 0x0d, // Driver bug leaves a half-finished escape sequence at the end of the previous job. Push some CRs down to flush it out.
											   0x1b, 0x43, 0x00, // Set page size in inches
											   0x00, // The page size
											   0x0c // Form feed
										   };

				int inches = twentyFourthsOfAnInchToFeed / 24;

				if (inches > 22)
				{
					throw new NotImplementedException("Maximum supported feed size is 22 inches");
				}

				result[9] = (byte)inches;

				return result;
			}
		}
	}
}
