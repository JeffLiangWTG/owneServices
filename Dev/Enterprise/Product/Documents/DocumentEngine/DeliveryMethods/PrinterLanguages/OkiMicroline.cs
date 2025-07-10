using System;
using System.Text;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.DeliveryMethods.PrinterLanguages
{
	public class OkiMicroline : Base
	{
		protected internal override ZBlob GetNonZeroPaperFeedSequence(int twentyFourthsOfAnInchToFeed)
		{
			if (twentyFourthsOfAnInchToFeed < 0)
			{
				throw new NotImplementedException("Reverse form feed not supported");
			}
			else if (twentyFourthsOfAnInchToFeed % 12 != 0)
			{
				throw new NotImplementedException("Maximum precision for OKI commands is 1/2 inch");
			}
			else
			{
				byte[] result = new byte[] {
									0x1b, 0x7b, 0x21, // Set emulation to ML
									0x1b, 0x18, // Reset the printer
									0x1b, 0x47, // Set page size in 1/2 inches
									0x00, 0x00, // The page size as ASCII characters, e.g. '1', '2' for 6 inches
									0x0c // Form feed
								  };

				int halfInches = twentyFourthsOfAnInchToFeed / 12;
				string halfInchesAsString = halfInches.ToString("00", System.Globalization.CultureInfo.InvariantCulture);
				byte[] halfInchesAsByteArray = Encoding.ASCII.GetBytes(halfInchesAsString);
				if (halfInchesAsByteArray.Length > 2)
				{
					throw new NotImplementedException("Maximum supported feed size is 49.5 inches");
				}

				result[7] = halfInchesAsByteArray[0];
				result[8] = halfInchesAsByteArray[1];

				return result;
			}
		}
	}
}
