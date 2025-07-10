using System;
using System.Collections;
using System.Linq;

namespace Enterprise.Barcode.Business
{
	class BarcodeLocator
	{
		// All measurements in pixels
		public int QuietZoneSize = 40;
		public int MaxBarSize = 35;
		public int MinimumBarcodeHeight = 15;
		public int SkewAllowedPerRow = 5;
		public int PreprocessingVerticalBlurAmount = 5;
		public int WhiteSpaceBorder = 3;

		public int MinimumBarCount = 18;
		public int AllowedBarCountVarience = 5;
		public int VerticalRepetitionTestSamples = 2;

		public double HorizontalSanityCheckSwipeSimilarity = 0.85;
		public double BarCheckBarThreshold = 0.70;
		public double BarCheckImageThreshold = 0.70;
		public double BarCountSimilarityPercent = 0.85;
		public double WhiteSpacePurity = 0.90;
		public double BarcodeDensityThreshold = 0.36;

		// If you changed number of row skipped, you may have to change height of the document barcode doc strips
		// (Warehouse,Portrait Footers - Document Barcode and Warehouse,Landscape Footers - Document Barcode) as well.
		const int RowSkipped = 15;

		/// <summary>
		/// Return BarcodeRegions containing the location of barcodes in the
		/// given ImageData set.
		/// </summary>
		/// <returns>ArrayList of BarcodeRegions</returns>
		internal ArrayList GetBarcodeRegions(byte[,] imageData, int imageHeight, int imageWidth)
		{
			//			byte[,] WorkingData = (byte[,])ImageData.Clone();
			//			ImageProcessor.RemoveIsolatedPixels(WorkingData, ImageHeight, ImageWidth);

			int[] barcodeStart = new int[imageHeight];
			int[] barcodeEnd = new int[imageHeight];

			FindBarcodeSwipes(imageData, imageHeight, imageWidth, barcodeStart, barcodeEnd);

			ArrayList barcodeRegions;
			barcodeRegions = CalculateRegions(barcodeStart, barcodeEnd, imageHeight, imageWidth);
			barcodeRegions = SplitRegionsVertically(imageData, barcodeRegions);
			barcodeRegions = MergeDividedRegions(barcodeRegions);
			barcodeRegions = GetSaneBarcodeRegions(imageData, imageHeight, imageWidth, barcodeRegions);
			return barcodeRegions;
		}

		#region Implementation

		protected const byte BlackPixel = 0;
		protected const byte WhitePixel = 255;

		#region Barcode Region Sanity Checks

		/// <summary>
		/// Ensure that given BarcodeRegions pass sanity checks. Sanity checks are designed to
		/// remove graphics detected earlier as barcodes which are unlikely to be.
		/// </summary>
		/// <returns>New ArrayList of BarcodeRegions which pass sanity checks.</returns>
		protected ArrayList GetSaneBarcodeRegions(byte[,] imageData, int imageHeight, int imageWidth, ArrayList regions)
		{
			ArrayList result = new ArrayList();

			foreach (BarcodeRegion r in regions)
			{
				BarcodeRegion a = FixRegionBounds(r, imageHeight, imageWidth);
				if (a.Height > MinimumBarcodeHeight
					&& HasConstantNumberOfBars(imageData, a)
					&& HasReasonablePixelDensity(imageData, a))
				{
					result.Add(a);
				}
			}

			return result;
		}

		/// <summary>
		/// Check if three different horizontal 'swipes' are all vaguely similar.
		/// </summary>
		protected bool RegionRepeatsVertically(byte[,] imageData, BarcodeRegion r)
		{
			int swipeDifferences = 0;

			int swipeHeight = r.Height / (VerticalRepetitionTestSamples + 2);
			for (int i = 0; i < VerticalRepetitionTestSamples - 1; i++)
			{
				int y1 = swipeHeight * (i + 1) + r.Y;
				int y2 = swipeHeight * (i + 2) + r.Y;

				for (int x = r.X; x < r.X + r.Width; x++)
				{
					if (imageData[y1, x] != imageData[y2, x])
					{
						swipeDifferences++;
					}
				}
			}

			var percentDifferent = swipeDifferences / ((double)VerticalRepetitionTestSamples * r.Width);
			double percentSame = 1 - percentDifferent;
			return percentSame >= HorizontalSanityCheckSwipeSimilarity;
		}

		/// <summary>
		/// Ensure that pictures contains mostly bars, ie, each column of pixels is either mostly
		/// black or mostly white.
		/// </summary>
		protected bool PassesBarCheck(byte[,] imageData, BarcodeRegion r)
		{
			int invalidBars = 0;

			for (int x = r.X; x < r.X + r.Width; x++)
			{
				int actives = 0;
				for (int y = r.Y; y < r.Y + r.Height; y++)
				{
					if (imageData[y, x] == 0)
					{
						actives++;
					}
				}

				var percentActive = actives / (double)r.Height;
				if (Math.Max(percentActive, 1 - percentActive) < BarCheckBarThreshold)
				{
					invalidBars++;
				}
				//System.Diagnostics.Debug.WriteLine(Math.Max(PercentActive, 1 - PercentActive));
			}

			var percentInvalid = invalidBars / (double)r.Width;
			double percentValid = 1 - percentInvalid;
			return percentValid > BarCheckImageThreshold;
		}

		/// <summary>
		/// Returns the number of bars in the given horizontal zone.
		/// </summary>
		/// <param name="x1">x position to do start detection from.</param>
		/// <param name="x2">x position to end detection at.</param>
		/// <param name="y">y coordinate to perform detection on.</param>
		protected int GetNumberOfBars(byte[,] imageData, int x1, int x2, int y)
		{
			int numberOfBars = 0;
			bool blackState = false;

			for (int x = x1; x < x2; x++)
			{
				if (imageData[y, x] == BlackPixel)
				{
					if (!blackState)
					{
						blackState = true;
						numberOfBars++;
					}
				}
				else
				{
					blackState = false;
				}
			}

			return numberOfBars;
		}

		/// <summary>
		/// Ensure that the number of bars in the given BarcodeRegion is fairly
		/// constant throughout the image.
		/// </summary>
		protected bool HasConstantNumberOfBars(byte[,] imageData, BarcodeRegion r)
		{
			int validRowNumber = r.Height - 4;
			int numberCorrectRows = 0;
			int averageNumberOfBars = 0;

			// Count bars in middle row
			int middleRow = r.Y + r.Height / 2;
			var numberOfBarsOfMiddleRow = GetNumberOfBars(imageData, r.X, r.X + r.Width, middleRow);

			// Count bars of all rows
			int[] numberOfBars = new int[validRowNumber];
			int index = 0;
			for (int y = r.Y + 2; y < r.Y + r.Height - 2; y++) // ignore the top 2 rows and bottom 2 rows to reduce the noise
			{
				int n = GetNumberOfBars(imageData, r.X, r.X + r.Width, y);
				numberOfBars[index++] = n;
				averageNumberOfBars += n;
			}

			averageNumberOfBars /= validRowNumber;
			numberCorrectRows = numberOfBars.Where(numberOfBar => Math.Abs(numberOfBar - averageNumberOfBars) <= AllowedBarCountVarience || Math.Abs(numberOfBar - numberOfBarsOfMiddleRow) <= AllowedBarCountVarience).Count();

			var percentSame = numberCorrectRows / ((double)validRowNumber);

			return (percentSame >= BarCountSimilarityPercent && averageNumberOfBars >= MinimumBarCount);
		}

		/// <summary>
		/// Ensure that the given BarcodeRegion has a ratio of black pixels to white
		/// pixels that could reasonably be a barcode.
		/// </summary>
		protected bool HasReasonablePixelDensity(byte[,] imageData, BarcodeRegion r)
		{
			int blackPixels = 0;
			int whitePixels = 0;

			for (int y = r.Y; y < r.Y + r.Height; y++)
			{
				for (int x = r.X; x < r.X + r.Width; x++)
				{
					if (imageData[y, x] == BlackPixel)
					{
						blackPixels++;
					}
					else
					{
						whitePixels++;
					}
				}
			}

			var density = blackPixels / (double)(whitePixels + blackPixels);
			return density > BarcodeDensityThreshold;
		}

		/// <summary>
		/// Return a BarcodeRegion that correctly conforms to ImageHeight and ImageWidth
		/// restrictions by cropping parts of the regions that exceed these limitations.
		/// </summary>
		/// <returns>A new BarcodeRegion that conforms to image size restrictions.</returns>
		protected BarcodeRegion FixRegionBounds(BarcodeRegion testRegion, int imageHeight, int imageWidth)
		{
			BarcodeRegion result = new BarcodeRegion();

			result.X = (testRegion.X > 0 ? testRegion.X : 0);
			result.Y = (testRegion.Y > 0 ? testRegion.Y : 0);

			result.Width = (testRegion.X + testRegion.Width < imageWidth ? testRegion.Width : imageWidth - testRegion.X - 1);
			result.Height = (testRegion.Y + testRegion.Height < imageWidth ? testRegion.Height : imageHeight - testRegion.Y - 1);
			result.Width = testRegion.Width;
			result.Height = testRegion.Height;

			return result;
		}

		#endregion

		#region Line-Level Barcode Scanning

		/// <summary>
		/// Scan through the given ImageData, skipping some rows and locating the start and end x-coordinates of
		/// possible barcodes on each line.
		/// </summary>
		/// <remarks>
		/// If there are two possible barcodes on a given line of the ImageData, the returned
		/// BarcodeStart and BarcodeEnd x-coordinates will encompass both, also including any data
		/// between. This will be later removed by various sanity checks.
		/// </remarks>
		/// <param name="barcodeStart">Starting x-coordinate of a possible barcode on a given line</param>
		/// <param name="barcodeEnd">Finishing x-coordinate of a possible barcode on a given line</param>
		/// 

		protected void FindBarcodeSwipes(byte[,] imageData, int imageHeight, int imageWidth, int[] barcodeStart, int[] barcodeEnd)
		{
			int rowEncountered = 0, nextIndex;
			bool hitTheNextBarcode = false;

			//trying to hit the first encounter with a barcode, if any
			for (int y = 0; y < imageHeight; y += RowSkipped)
			{
				if (RowContainsBarcode(imageData, y, imageWidth, out barcodeStart[y], out barcodeEnd[y]))
				{
					rowEncountered = y;
					hitTheNextBarcode = true;
				}
				//going upward 
				for (int z = rowEncountered - 1; rowEncountered - z <= RowSkipped && z > 0; z--)
				{
					bool stillBarcode = RowContainsBarcode(imageData, z, imageWidth, out barcodeStart[z], out barcodeEnd[z]);
					if (!stillBarcode)
					{
						break;
					}
				}
				//going downward 
				for (nextIndex = rowEncountered + 1; nextIndex < imageHeight; nextIndex++)
				{
					bool stillBarcode = RowContainsBarcode(imageData, nextIndex, imageWidth, out barcodeStart[nextIndex], out barcodeEnd[nextIndex]);
					if (!stillBarcode)
					{
						break;
					}
				}
				//search for another barcode
				if (hitTheNextBarcode)
				{
					y = nextIndex;
					hitTheNextBarcode = false;
				}
			}
		}

		/// <summary>
		/// Determine if the given row of the input ImageData could contain a barcode, returning
		/// the Start and End x-coordinates of the barcode if a barcode was deteced.
		/// </summary>
		/// <remarks>
		/// If there are two possible barcodes on a given line of the ImageData, the returned
		/// BarcodeStart and BarcodeEnd x-coordinates will encompass both, also including any data
		/// between. This will be later removed by various sanity checks.
		/// </remarks>
		/// <param name="start">The starting x-coordinate of the barcode.</param>
		/// <param name="end">The ending x-coordinate of the barcode.</param>
		/// <param name="y">The row to scan.</param>
		protected bool RowContainsBarcode(byte[,] imageData, int y, int imageWidth, out int start, out int end)
		{
			const int NoBarcodeFound = -1;

			int firstBarcodeStart = NoBarcodeFound;
			int currentBarcodeStart = 0;
			int currentBarcodeEnd = 0;
			int whitePixelCount = 0;

			start = 0;
			end = 0;

			for (int x = 0; x < imageWidth; x++)
			{
				if (imageData[y, x] == BlackPixel)
				{
					whitePixelCount = 0;
				}
				else
				{
					whitePixelCount++;
					if (whitePixelCount > QuietZoneSize)
					{
						// Start considering this as a new test region. Check out
						// the old one first.
						if (IntervalContainsBarcode(imageData, y, currentBarcodeStart, x))
						{
							currentBarcodeEnd = x - QuietZoneSize;
							if (firstBarcodeStart == NoBarcodeFound)
							{
								firstBarcodeStart = currentBarcodeStart;
							}
						}
						currentBarcodeStart = x;
					}
				}
			}

			if (firstBarcodeStart != NoBarcodeFound)
			{
				start = firstBarcodeStart;
				end = currentBarcodeEnd;
				return true;
			}
			else
			{
				start = 0;
				end = 0;
				return false;
			}
		}

		/// <summary>
		/// Return if the given interval contains a barcode.
		/// </summary>
		/// <remarks>
		/// A barcode at this level is defined as a stripe of data that contains
		/// alternating black and white bars, neither of which is considered too
		/// wide (MaxBarSize) to be part of a barcode. It is also expected that there
		/// be at least a certain number of stripes (MinimumBarCount) to be a
		/// barcode.
		/// </remarks>
		/// <param name="x1">x position to do start detection from.</param>
		/// <param name="x2">x position to end detection at.</param>
		/// <param name="y">y coordinate to perform detection on.</param>
		protected bool IntervalContainsBarcode(byte[,] imageData, int y, int x1, int x2)
		{
			bool result = true;

			// If the amount of pixels we have to work with is too small, we can not be a barcode
			if (x2 - x1 < QuietZoneSize + MinimumBarCount * 2)
			{
				return false;
			}

			// Assert there are alternating black and white pixels
			int blackPixelCount = 0;
			int barCount = 0;

			for (int i = x1; i <= x2; i++)
			{
				if (imageData[y, i] == BlackPixel)
				{
					if (blackPixelCount == 0)
					{
						barCount++;
					}
					blackPixelCount++;
					if (blackPixelCount > MaxBarSize)
					{
						result = false;
						break;
					}
				}
				else
				{
					blackPixelCount = 0;
				}
			}

			if (barCount < MinimumBarCount)
			{
				result = false;
			}

			return result;
		}

		#endregion

		#region Barcode Line Merging and Processing

		/// <summary>
		/// Convert given 1-dimensional Start and End x-coordinate data into 2-dimensional
		/// BarcodeRegion data.
		/// </summary>
		/// <param name="start">Start x-coordinate data.</param>
		/// <param name="end">End x-coordinate data.</param>
		/// <returns>
		/// An ArrayList of BarcodeRegions containing 2-dimensional data about the location
		/// of deteced barcodes.
		/// </returns>
		protected ArrayList CalculateRegions(int[] start, int[] end, int imageHeight, int imageWidth)
		{
			ArrayList results = new ArrayList();
			const int Infinity = int.MaxValue;

			int activeStart = Infinity;
			int activeEnd = 0;
			int activeTop = 0;

			for (int i = 0; i < start.Length; i++)
			{
				if (start[i] == 0 && end[i] == 0 || (activeStart != Infinity && (!SkewIsAllowable(start[i], activeStart) || !SkewIsAllowable(end[i], activeEnd))))
				{
					if (activeStart != Infinity)
					{
						// We just hit the bottom of a barcode. Process it.
						BarcodeRegion foundBarcode = new BarcodeRegion(activeStart - WhiteSpaceBorder, activeTop, activeEnd - activeStart + (WhiteSpaceBorder * 2), i - activeTop);
						results.Add(FixRegionBounds(foundBarcode, imageHeight, imageWidth));
					}
					activeStart = Infinity;
					activeEnd = 0;
					activeTop = i;
					continue;
				}
				else
				{
					if (start[i] < activeStart)
					{
						activeStart = start[i];
					}

					if (end[i] > activeEnd)
					{
						activeEnd = end[i];
					}
				}
			}

			if (activeStart != Infinity)
			{
				BarcodeRegion foundBarcode = new BarcodeRegion(activeStart - WhiteSpaceBorder, activeTop, activeEnd - activeStart + (WhiteSpaceBorder * 2), imageHeight - 1 - activeTop);
				results.Add(FixRegionBounds(foundBarcode, imageHeight, imageWidth));
			}

			return results;
		}

		/// <summary>
		/// Split given regions vertically if needed to separate distinct barcodes into
		/// seperate regions.
		/// </summary>
		protected ArrayList SplitRegionsVertically(byte[,] imageData, ArrayList regions)
		{
			ArrayList result = new ArrayList();

			foreach (BarcodeRegion r in regions)
			{
				result.AddRange(SplitRegion(r, imageData));
			}

			return result;
		}

		/// <summary>
		/// Merge BarcodeRegions that are likely to refer to the same barcode, but have
		/// been fragmented earlier due to various image processing functions.
		/// </summary>
		/// <param name="regions">Input region list containing BarcodeRegions to merge.</param>
		/// <returns>The given ArrayList of regions.</returns>
		protected ArrayList MergeDividedRegions(ArrayList regions)
		{
			for (int i = 0; i < regions.Count; i++)
			{
				for (int j = 0; j < regions.Count; j++)
				{
					if (i == j)
					{
						continue;
					}

					if (RegionsMergable((BarcodeRegion)regions[i], (BarcodeRegion)regions[j]))
					{
						regions[i] = MergeRegions((BarcodeRegion)regions[i], (BarcodeRegion)regions[j]);
						regions.RemoveAt(j);
						i--;
						break;
					}
				}
			}

			return regions;
		}

		/// <summary>
		/// Determine if the given two BarcodeRegions should be merged.
		/// </summary>
		protected bool RegionsMergable(BarcodeRegion a, BarcodeRegion b)
		{
			const int MergeBorder = 4;
			if (SkewIsAllowable(a.X, a.X + a.Width, b.X, b.X + b.Width))
			{
				if (Math.Abs(a.Y - b.Y - b.Height) <= MergeBorder)
				{
					return true;
				}

				if (Math.Abs(b.Y - a.Y - a.Height) <= MergeBorder)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Merge together two barcode regions as one which encompasses all the image
		/// data previously covered.
		/// </summary>
		protected BarcodeRegion MergeRegions(BarcodeRegion a, BarcodeRegion b)
		{
			BarcodeRegion result = new BarcodeRegion();

			result.X = Math.Min(a.X, b.X);
			result.Width = Math.Max(a.X + a.Width, b.X + b.Width) - result.X;
			result.Y = Math.Min(a.Y, b.Y);
			result.Height = Math.Max(a.Y + a.Height, b.Y + b.Height) - result.Y;

			return result;
		}

		/// <summary>
		/// Check that the amount of skew between two x values is reasonable
		/// to be part of the same barcode.
		/// </summary>
		protected bool SkewIsAllowable(int newValue, int oldValue)
		{
			return Math.Abs(newValue - oldValue) <= SkewAllowedPerRow;
		}

		/// <summary>
		/// Check if the amount of skew given is a reasonable amount,
		/// and that both sides are skewing in the same direction.
		/// </summary>
		protected bool SkewIsAllowable(int aStart, int aEnd, int bStart, int bEnd)
		{
			if (SkewIsAllowable(aStart, bStart) && SkewIsAllowable(aEnd, bEnd))
			{
				if ((aEnd - bEnd <= 0) && (aStart - bStart <= 0))
				{
					return true;
				}

				if ((aEnd - bEnd >= 0) && (aStart - bStart >= 0))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Split given regions vertically if needed to separate distinct barcodes into
		/// seperate regions.
		/// </summary>
		/// <remarks>
		/// A split is performed if there is a large vertical stripe of whitespace in the
		/// image.
		/// </remarks>
		/// <returns>A new ArrayList containing split BarcodeRegions.</returns>
		protected ArrayList SplitRegion(BarcodeRegion r, byte[,] imageData)
		{
			ArrayList result = new ArrayList();

			int whiteColumns = 0;
			int whiteRunStart = r.X;
			for (int x = r.X; x < r.X + r.Width; x++)
			{
				int blackPixels = 0;
				for (int y = r.Y; y < r.Y + r.Height; y++)
				{
					if (imageData[y, x] == BlackPixel)
					{
						blackPixels++;
					}
				}

				var percentWhiteSpace = 1 - (blackPixels / (double)r.Height);
				if (percentWhiteSpace > WhiteSpacePurity)
				{
					whiteColumns++;
				}
				else
				{
					if (whiteColumns >= QuietZoneSize)
					{
						result.AddRange(SplitRegion(new BarcodeRegion(r.X, r.Y, whiteRunStart - r.X + WhiteSpaceBorder, r.Height), imageData));
						result.AddRange(SplitRegion(new BarcodeRegion(x - WhiteSpaceBorder, r.Y, r.X + r.Width - x + WhiteSpaceBorder, r.Height), imageData));
						break;
					}
					whiteColumns = 0;
					whiteRunStart = x;
				}
			}

			if (result.Count == 0)
			{
				result.Add(r);
			}

			return result;
		}

		#endregion

		#endregion
	}
}
