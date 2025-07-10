using Enterprise.Barcode.Business.Symbologies.Code128;

namespace Enterprise.Barcode.Business
{
	internal class BarcodeReader
	{
		public double DesiredConfidenceLevel = 0.50;
		public double MinimumConfidenceLevel = 0.20;

		public BarcodeValue ReadBarcode(byte[,] imageData, int imageHeight, int imageWidth)
		{
			return GetBarcodeText(imageData, imageHeight, imageWidth);
		}

		#region Implementation

		protected void SelectBestBarcode(string newText, double newConfidence, BarcodeSymbologie newBarcodeSymbologie, ref string oldText, ref double oldConfidence, ref BarcodeSymbologie oldBarcodeSymbologie)
		{
			if (newConfidence > oldConfidence)
			{
				oldText = newText;
				oldConfidence = newConfidence;
				oldBarcodeSymbologie = newBarcodeSymbologie;
			}
		}

		public BarcodeValue GetBarcodeText(byte[,] imageData, int imageHeight, int imageWidth)
		{
			string text;
			string bestText = "";
			double bestConfidence = 0;
			double confidence;
			BarcodeSymbologie bestBarcodeSymbologie = BarcodeSymbologie.InvalidBarcode;

			text = Code128Reader.ReadBarcode(imageData, imageHeight, imageWidth, DesiredConfidenceLevel, MinimumConfidenceLevel, out confidence);
			if (confidence > DesiredConfidenceLevel)
			{
				return new BarcodeValue(text, confidence, BarcodeSymbologie.Code128);
			}

			SelectBestBarcode(text, confidence, BarcodeSymbologie.Code128, ref bestText, ref bestConfidence, ref bestBarcodeSymbologie);

			//Text = EAN13Reader.ReadBarcode(ImageData, ImageHeight, ImageWidth, DesiredConfidenceLevel, MinimumConfidenceLevel, out Confidence);
			//if (Confidence > DesiredConfidenceLevel) return new BarcodeValue(Text, Confidence, BarcodeSymbologie.EAN13);
			//SelectBestBarcode(Text, Confidence, BarcodeSymbologie.EAN13, ref BestText, ref BestConfidence, ref BestBarcodeSymbologie);

			//Text = Code39Reader.ReadBarcode(ImageData, ImageHeight, ImageWidth, DesiredConfidenceLevel, MinimumConfidenceLevel, out Confidence);
			//if (Confidence > DesiredConfidenceLevel) return new BarcodeValue(Text, Confidence, BarcodeSymbologie.Code39);
			//SelectBestBarcode(Text, Confidence, BarcodeSymbologie.Code39, ref BestText, ref BestConfidence, ref BestBarcodeSymbologie);

			// etc

			// Desired confidence level could not be reached, no matter which barcode system
			// we used. Spit out the best one we have.
			if (bestConfidence > MinimumConfidenceLevel)
			{
				return new BarcodeValue(bestText, bestConfidence, bestBarcodeSymbologie);
			}
			else
			{
				return new BarcodeValue("", 0, BarcodeSymbologie.InvalidBarcode);
			}
		}

		#endregion
	}
}
