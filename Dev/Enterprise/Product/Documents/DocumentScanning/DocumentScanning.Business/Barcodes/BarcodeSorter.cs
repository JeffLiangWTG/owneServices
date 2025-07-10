using System;
using System.Collections.Specialized;
using System.Drawing;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngine;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.Business
{
	/// <summary>
	/// Base class for Barcode parsing / recognition
	/// A page sorter will work out whether the current barcode means anything to 
	/// Enterprise, and whether the page that the barcode is on needs to be kept.
	/// </summary>
	public class BarcodeSorter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BarcodeSorter(DocumentFactory factory)
			: base(factory)
		{
			ScannedBarcodesOnPage = new StringCollection();
		}

		public DocumentFactory MasterFactory
		{
			get { return (DocumentFactory)Factory; }
		}

		/// <summary>
		/// If the page has a recognised Enterprise barcode, return that barcode type
		/// otherwise return null.
		/// Assumes that only one type of special barcode will be present on any one page.
		/// </summary>
		/// 
		public BaseBarcode ProcessPageForBarcodes(Bitmap currentPage)
		{
			return ProcessPageForBarcodes(currentPage, ZString.Empty);
		}

		public BaseBarcode ProcessPageForBarcodes(Bitmap currentPage, ZString jobType)
		{
			ScannedBarcodesOnPage.Clear();

			StringCollection enterpriseBarcodes = new StringCollection();
			Bitmap imageToUse = GetImageToUse(currentPage);
			bool shouldDispose = false;
			if (imageToUse == null)
			{
				imageToUse = currentPage;
			}
			else
			{
				shouldDispose = true;
			}

			ScannedBarcodesOnPage = new BarcodeScanner().ExtractBarcodes(imageToUse);

			foreach (string barcodeString in ScannedBarcodesOnPage)
			{
				if (BarcodeHelper.IsValidBarcodeType(barcodeString))
				{
					enterpriseBarcodes.Add(barcodeString);
				}
				else if (!jobType.IsEmpty)
				{
					enterpriseBarcodes.Add(string.Format("^{0}={1}|", jobType, barcodeString));
				}
			}
			if (shouldDispose)
			{
				imageToUse.Dispose();
			}
			return GetBarcodeType(enterpriseBarcodes);
		}

#if DEBUG
		public
#endif
		const float maxLongSide = 3000;
#if DEBUG
		public
#endif
		const float maxShortSide = 2000;

#if DEBUG
		public
#endif
		Bitmap GetImageToUse(Bitmap currentPage)
		{
			var optimalSize = GetBestSizeForScanner(currentPage.Size);
			if (optimalSize == currentPage.Size)
			{
				return null;
			}

			using (var file = TempFile.New())
			{
				currentPage.Save(file.Filename, currentPage.RawFormat);
				using (var tmp = new Bitmap(file.Filename))
				{
					return new Bitmap(tmp, optimalSize);
				}
			}
		}

		internal static Size GetBestSizeForScanner(Size suggestedSize)
		{
			var wider = suggestedSize.Width > suggestedSize.Height;

			var longSide = wider ? suggestedSize.Width : suggestedSize.Height;
			var shortSide = wider ? suggestedSize.Height : suggestedSize.Width;
			if (longSide <= maxLongSide && shortSide <= maxShortSide)
			{
				return suggestedSize;
			}

			var ratio = Math.Min(maxLongSide / (double)longSide, maxShortSide / (double)shortSide);
			var newLongSide = (int)Utilities.Round((decimal)(longSide * ratio), 0);
			var newShortSide = (int)Utilities.Round((decimal)(shortSide * ratio), 0);

			return wider ?
				ObjectFactory.Get<IDpiScalingHelper>().NewScaledSize(newLongSide, newShortSide, false) :
				ObjectFactory.Get<IDpiScalingHelper>().NewScaledSize(newShortSide, newLongSide, false);
		}

		public StringCollection ScannedBarcodesOnPage { get; private set; }

		/// <summary>
		/// Accepts a list strings found on the one page. 
		/// There SHOULD be only one string in this list - it will return the correct
		/// type of barcode that the string corresponds to.
		/// </summary>
		protected BaseBarcode GetBarcodeType(StringCollection enterpriseBarcodes)
		{
			if (enterpriseBarcodes.Count == 1)
			{
				string barcodeText = enterpriseBarcodes[0];

				if (BarcodeHelper.IsDocTypeBarcode(barcodeText))
				{
					return new DocTypeBarcode(MasterFactory, barcodeText);
				}
				else if (BarcodeHelper.IsShipmentBarcode(barcodeText))
				{
					return new ShipmentBarcode(MasterFactory, barcodeText);
				}
			}

			return null;
		}
	}
}
