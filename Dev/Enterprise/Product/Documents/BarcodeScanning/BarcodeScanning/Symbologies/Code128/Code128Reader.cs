using System;
using System.Collections;

using Enterprise.Barcode.Business.Symbologies.Generic;

namespace Enterprise.Barcode.Business.Symbologies.Code128
{
	internal abstract class Code128Reader
	{
		/// <summary>
		/// Attempt to read the given ImageData as a Code128 Barcode.
		/// </summary>
		/// <param name="imageData">ImageData to read.</param>
		/// <param name="imageHeight">Height of ImageData.</param>
		/// <param name="imageWidth">Width of ImageData.</param>
		/// <param name="desiredConfidenceLevel">
		/// The confidence level that, if reached, will cause the function to immeadiatly
		/// return with its result.
		/// </param>
		/// <param name="minimumConfidenceLevel">
		/// The minimum confidence that will be accepted as a succesfull barcode read.
		/// </param>
		/// <param name="confidence">The confidence of the barcode read.</param>
		/// <returns>Read barcode data, or empty if no barcode could be successfully read.</returns>
		public static string ReadBarcode(byte[,] imageData, int imageHeight, int imageWidth, double desiredConfidenceLevel, double minimumConfidenceLevel, out double confidence)
		{
			string result = "";
			Hashtable resultsTable = new Hashtable(imageHeight * 3);

			ReadingFunctions.PreprocessImageData(imageData, imageHeight, imageWidth);

			result = ReadBarcodeFromImageData(imageData, imageHeight, imageWidth, desiredConfidenceLevel, minimumConfidenceLevel, out confidence, resultsTable);
			if (confidence >= desiredConfidenceLevel)
			{
				return result;
			}

			ImageProcessor.HorizontalFlipImageData(imageData, imageHeight, imageWidth);
			ImageProcessor.VerticalFlipImageData(imageData, imageHeight, imageWidth);

			result = ReadBarcodeFromImageData(imageData, imageHeight, imageWidth, desiredConfidenceLevel, minimumConfidenceLevel, out confidence, resultsTable);
			if (confidence >= desiredConfidenceLevel)
			{
				return result;
			}

			string bestResult = "";
			double bestConfidence = 0;
			IDictionaryEnumerator e = resultsTable.GetEnumerator();
			for (; ; )
			{
				if (!e.MoveNext())
				{
					break;
				}

				SelectBestConfidence((string)e.Key, double.Parse(e.Value.ToString()), ref bestResult, ref bestConfidence);
			}
			confidence = Math.Min(bestConfidence, 1.00);
			return bestResult;
		}

		#region Implementation

		protected static string ReadBarcodeStripe(int[] barWidths, int offset, double desiredConfidenceLevel, double minimimConfidenceLevel, out double confidence)
		{
			return ReadBarcodeStripe(barWidths, offset, desiredConfidenceLevel, minimimConfidenceLevel, out confidence, 0);
		}

		/// <summary>
		/// Attempt to decode a row of barcode widths
		/// </summary>
		/// <remarks>
		/// The first stripe width should be the width of the first black stripe.
		/// </remarks>
		protected static string ReadBarcodeStripe(int[] barWidths, int offset, double desiredConfidenceLevel, double minimimConfidenceLevel, out double confidence, int depth)
		{
			Code128CharacterSet characterSet;

			if (!Code128Values.IsValidNumberOfStripes(barWidths.Length))
			{
				confidence = 0;
				return "";
			}

			double startCharError;
			characterSet = DetermineCodeType(barWidths, offset, out startCharError);
			if (characterSet == Code128CharacterSet.InvalidCharacterSet ||
				Code128Values.GetConfidence(startCharError) < minimimConfidenceLevel)
			{
				confidence = 0;
				return "";
			}
			if (characterSet == Code128CharacterSet.UpsideDown)
			{
				if (depth >= 1)
				{
					confidence = 0;
					return "";
				}
				else
				{
					Array.Reverse(barWidths);
					return ReadBarcodeStripe(barWidths, 0, desiredConfidenceLevel, minimimConfidenceLevel, out confidence, depth + 1);
				}
			}
			else
			{
				return ReadBarcodeStripeData(barWidths, offset, desiredConfidenceLevel, minimimConfidenceLevel, out confidence, characterSet);
			}
		}

		/// <summary>
		/// Attempt to decode a row of barcode widths, given a known dataset.
		/// </summary>
		/// <remarks>
		/// The first stripe width should be the width of the first black stripe.
		/// </remarks>
		protected static string ReadBarcodeStripeData(int[] barWidths, int offset, double desiredConfidenceLevel, double minimimConfidenceLevel, out double confidence, Code128CharacterSet characterSet)
		{
			int checksum = CalculateChecksum(characterSet);

			int lastChar = -1;
			int length = 0;
			string result = "";
			double maxError = 0;
			double error;
			bool barcodeCorrectlyStopped = false;

			for (int i = Code128Values.BarsPerCharacter; i < barWidths.Length - Code128Values.BarsPerCharacter; i += Code128Values.BarsPerCharacter)
			{
				int readValue = ReadCharacter(barWidths, i, out error);
				if (error > maxError)
				{
					maxError = error;
				}

				if (!IsValidDataCharacter(readValue))
				{
					barcodeCorrectlyStopped = false;
					break;
				}
				if (readValue == Code128Values.StopIndex)
				{
					barcodeCorrectlyStopped = true;
					break;
				}
				if (lastChar >= 0)
				{
					result += Code128Values.CodeToChar(lastChar, ref characterSet);
				}
				checksum = CalculateChecksum(checksum, readValue, ++length);
				lastChar = readValue;
			}
			checksum = ReverseChecksum(checksum, lastChar, length);

			if (barcodeCorrectlyStopped && ChecksumPasses(checksum, lastChar))
			{
				confidence = Code128Values.GetConfidence(maxError);
				return result;
			}
			else
			{
				confidence = 0;
				return "";
			}
		}

		protected static bool IsValidDataCharacter(int value)
		{
			if (value < 0)
			{
				return false;
			}

			return !(value == Code128Values.CodeAStartIndex ||
				value == Code128Values.CodeBStartIndex ||
				value == Code128Values.CodeCStartIndex ||
				value == Code128Values.UpsideDownStartIndex);
		}
		#endregion
		#region High Level Barcode Reading Functions

		/// <summary>
		/// Attempt to read the given ImageData as a Code128 Barcode.
		/// No changes to ImageData orientation or input validation occurs.
		/// </summary>
		protected static string ReadBarcodeFromImageData(byte[,] imageData, int imageHeight, int imageWidth, double desiredConfidenceLevel, double minimumConfidenceLevel, out double confidence, Hashtable resultsTable)
		{
			string result;

			result = ReadBarcodeFromImageData_Simple(imageData, imageHeight, imageWidth, desiredConfidenceLevel, minimumConfidenceLevel, out confidence, resultsTable);
			if (confidence >= desiredConfidenceLevel)
			{
				return result;
			}

			result = ReadBarcodeFromImageData_RegionAverage(imageData, imageHeight, imageWidth, desiredConfidenceLevel, minimumConfidenceLevel, out confidence, resultsTable);
			if (confidence >= desiredConfidenceLevel)
			{
				return result;
			}

			return "";
		}

		#region ReadBarcodeFromImageData_Simple
		protected static string ReadBarcodeFromImageData_Simple(byte[,] imageData, int imageHeight, int imageWidth, double desiredConfidenceLevel, double minimumConfidenceLevel, out double confidence, Hashtable resultsTable)
		{
			int[][] barWidthData = ReadingFunctions.GetAllBarWidths(imageData, imageHeight, imageWidth);
			string result;

			foreach (int[] dataRow in barWidthData)
			{
				result = ReadBarcodeStripe(dataRow, 0, desiredConfidenceLevel, minimumConfidenceLevel, out confidence);
				if (confidence > desiredConfidenceLevel)
				{
					return result;
				}
				UpdateResultsTable(resultsTable, result, confidence);
			}

			confidence = 0;
			return "";
		}
		#endregion

		#region ReadBarcodeFromImageData_RegionAverage and Helper Functions

		protected static string ReadBarcodeFromImageData_RegionAverage(byte[,] imageData, int imageHeight, int imageWidth, double desiredConfidenceLevel, double minimumConfidenceLevel, out double confidence, Hashtable resultsTable)
		{
			const int RowsToAverage = 4;

			int[][] barWidthData;
			barWidthData = ReadingFunctions.GetAllBarWidths(imageData, imageHeight, imageWidth);
			SortBarWidthDataRows(barWidthData, imageHeight, imageWidth);

			string result = "";

			int rowCount = 0;
			int rowSequenceStart = 0;
			int currentBarCount = barWidthData[0].Length;

			for (int y = 0; y < imageHeight; y++)
			{
				if (barWidthData[y].Length != currentBarCount || rowCount >= RowsToAverage)
				{
					result = ReadBarcodeFromBarAverages(barWidthData, rowSequenceStart, y - 1, currentBarCount, desiredConfidenceLevel, minimumConfidenceLevel, out confidence);
					if (confidence > desiredConfidenceLevel)
					{
						return result;
					}

					UpdateResultsTable(resultsTable, result, confidence);

					if (barWidthData[y].Length != currentBarCount)
					{
						rowCount = 1;
						rowSequenceStart = y;
						currentBarCount = barWidthData[y].Length;
					}
					else
					{
						rowSequenceStart++;
					}
				}
				else
				{
					rowCount++;
				}
			}

			confidence = 0;
			return "";
		}

		protected static void SortBarWidthDataRows(int[][] barWidthData, int imageHeight, int imageWidth)
		{
			for (int i = 0; i < imageHeight; i++)
			{
				for (int j = i + 1; j < imageHeight; j++)
				{
					if (barWidthData[i].Length > barWidthData[j].Length)
					{
						int[] swap = barWidthData[i];
						barWidthData[i] = barWidthData[j];
						barWidthData[j] = swap;
					}
				}
			}
		}

		protected static string ReadBarcodeFromBarAverages(int[][] barWidthData, int firstRow, int lastRow, int barCount, double desiredConfidenceLevel, double minimumConfidenceLevel, out double confidence)
		{
			int rowCount = lastRow - firstRow + 1;

			for (int x = 0; x < barCount; x++)
			{
				int average = 0;
				for (int i = firstRow; i <= lastRow; i++)
				{
					average += barWidthData[i][x];
				}
				average /= rowCount;
				barWidthData[lastRow][x] = average;
			}

			return ReadBarcodeStripe(barWidthData[lastRow], 0, desiredConfidenceLevel, minimumConfidenceLevel, out confidence);
		}

		#endregion

		#region Barcode Character Reading

		protected static Code128CharacterSet DetermineCodeType(int[] data, int offset, out double error)
		{
			int startCharacter = ReadCharacter(data, offset, out error);
			return Code128Values.IndexToCharacterSet(startCharacter);
		}

		/// <summary>
		/// Return the next character from the given Data of barcode widths.
		/// </summary>
		/// <param name="data">An array of barcode stripe widths.</param>
		/// <param name="offset">
		/// An offset to the position to start reading at. It is assumed that the
		/// offset is pointing to the width of a black bar.
		/// </param>
		/// <param name="error">
		/// The square of differences between expected widths of bars and the actuall
		/// widths.
		/// </param>
		/// <returns>The raw intergral value of the given data character</returns>
		/// <remarks>
		/// This function works by "smearing" all possible characters over the top
		/// of the given data and returning the one that looks best. Attempting
		/// to convert the pixel widths of the input data directly into bar widths
		/// so a direct lookup can be completed does not work, as the low resolution
		/// scans that we are working with will often give us the same pixel width for
		/// two	different bar sizes.
		/// </remarks>
		protected static int ReadCharacter(int[] data, int offset, out double error)
		{
			double moduleWidth = GetModuleWidth(data, offset);
			double[] barWidths = GetBarRatios(data, offset, moduleWidth);
			Code128Values code128Lookup = new Code128Values();

			double bestError = double.MaxValue;
			int bestCode = -1;

			for (int i = 0; i < Code128Values.DataCharacters; i++)
			{
				double currentError = 0;
				for (int j = 0; j < Code128Values.BarsPerCharacter; j++)
				{
					var barError = barWidths[j] - code128Lookup.Data[i][j];
					currentError += barError * barError;
				}
				if (currentError < bestError)
				{
					bestError = currentError;
					bestCode = i;
				}
			}

			error = bestError;
			return bestCode;
		}

		/// <summary>
		/// Return the pixel width of the next six bars, or "module".
		/// </summary>
		protected static double GetModuleWidth(int[] data, int offset)
		{
			int pixelSum = 0;

			for (int i = 0; i < Code128Values.BarsPerCharacter; i++)
			{
				pixelSum += data[i + offset];
			}

			return pixelSum / (double)Code128Values.ModulesPerCharacter;
		}

		/// <summary>
		/// Return the ratios of bar widths as a percentage of the width of the module.
		/// </summary>
		protected static double[] GetBarRatios(int[] data, int offset, double moduleWidth)
		{
			double[] result = new double[Code128Values.BarsPerCharacter];
			for (int i = 0; i < Code128Values.BarsPerCharacter; i++)
			{
				result[i] = data[i + offset] / moduleWidth;
			}

			return result;
		}

		#endregion

		#region Checksumming Code

		/// <summary>
		/// Update the checksum to take into account a new character in the data stream
		/// </summary>
		/// <returns>New checksum value.</returns>
		protected static int CalculateChecksum(int oldChecksum, int newCharacterCode, int characterPosition)
		{
			int newChecksum = oldChecksum + newCharacterCode * characterPosition;
			newChecksum %= 103;
			if (newChecksum < 0)
			{
				newChecksum += 103;
			}

			return newChecksum;
		}

		/// <summary>
		/// Calculate an initial checksum value based on the Code128 Character set being used.
		/// </summary>
		protected static int CalculateChecksum(Code128CharacterSet characterSet)
		{
			switch (characterSet)
			{
				case Code128CharacterSet.CodeA:
					return 0;
				case Code128CharacterSet.CodeB:
					return 1;
				case Code128CharacterSet.CodeC:
					return 2;
			}
			return -1;
		}

		/// <summary>
		/// Remove the given character from the given checksum calculation.
		/// </summary>
		/// <returns>New checksum value.</returns>
		protected static int ReverseChecksum(int oldChecksum, int characterToRemove, int characterPosition)
		{
			int newChecksum = oldChecksum - (characterToRemove * characterPosition);
			newChecksum %= 103;
			if (newChecksum < 0)
			{
				newChecksum += 103;
			}

			return newChecksum;
		}

		protected static bool ChecksumPasses(int checksumValue, int checksumCharacter)
		{
			return checksumValue == checksumCharacter;
		}

		#endregion

		protected static void SelectBestConfidence(string result, double confidence, ref string bestResult, ref double bestConfidence)
		{
			if (confidence > bestConfidence)
			{
				bestConfidence = confidence;
				bestResult = result;
			}
		}

		/// <summary>
		/// Update the given Key of the given Hashtable to reflect a new read of the barcode
		/// with given given Confidence.
		/// </summary>
		/// <remarks>
		/// Barcode reads are stored in a Hashtable if a read is not deemed confident enougth
		/// alone. If multiple reads of low confidence are found, though, slowly the overall
		/// confidence of that value will increase.
		/// </remarks>
		protected static void UpdateResultsTable(Hashtable resultsTable, string key, double confidence)
		{
			if (string.IsNullOrEmpty(key))
			{
				return;
			}

			if (resultsTable.Contains(key))
			{
				double x = (double)resultsTable[key];
				resultsTable[key] = x + (confidence * confidence);
			}
			else
			{
				resultsTable.Add(key, (confidence * confidence));
			}
		}

		#endregion
	}
}
