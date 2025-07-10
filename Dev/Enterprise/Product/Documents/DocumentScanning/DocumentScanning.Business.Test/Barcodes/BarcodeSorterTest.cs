using System;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class BarcodeSorterExposed : BarcodeSorter
	{
		public BarcodeSorterExposed(DocumentFactory masterFactory)
			: base(masterFactory)
		{
		}

		public BaseBarcode GetBarcodeTypeExposed(StringCollection enterpriseBarcodeStrings)
		{
			return base.GetBarcodeType(enterpriseBarcodeStrings);
		}
	}

	[TestedType(typeof(BarcodeSorter))]
	sealed class BarcodeSorterTest : NonPersistentBusinessObjectTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBarcodeRecognizedForNoisyPoints()
		{
			using (Bitmap page = (Bitmap)Bitmap.FromFile(Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"C00113756.tif")))
			{
				BaseBarcode barcode = Sorter.ProcessPageForBarcodes(page);
				Assert("DocType Barcode should be returned", barcode is DocTypeBarcode);
				AssertEquals("Should be CON", "CON", barcode.DocManagerCode);
				AssertEquals("Should Refer to C00113756", "C00113756", barcode.RefCode);
			}
		}

		public void TestBarcodeRecognizedForLargeFile()
		{
			Assert("Was unable to run the GC in an appropriate time for this memory heavy test. Maybe the finalizer queue is blocked?", GCHelper.RunGarbageCollector());

			using (var page = (Bitmap)Bitmap.FromFile(LargeImageWithBarcodeTifPath))
			{
				BaseBarcode barcode = Sorter.ProcessPageForBarcodes(page);
				Assert("DocType Barcode should be returned", barcode is DocTypeBarcode);
				AssertEquals("Should be SHP", "SHP", barcode.DocManagerCode);
				AssertEquals("Should Refer to S00038411", "S00038411", barcode.RefCode);
			}
		}

		public void TestGetImageFromLargeFileKeepsFileRatio()
		{
			Assert("Was unable to run the GC in an appropriate time for this memory heavy test. Maybe the finalizer queue is blocked?", GCHelper.RunGarbageCollector());

			using (var page = (Bitmap)Bitmap.FromFile(LargeImageWithBarcodeTifPath))
			{
				var originalRatio = page.Width / (double)page.Height;
				Bitmap imageAfterResize = Sorter.GetImageToUse(page);
				var afterResizeRatio = imageAfterResize.Width / (double)imageAfterResize.Height;
				Assert(imageAfterResize.Height <= BarcodeSorter.maxLongSide);
				Assert(imageAfterResize.Width <= BarcodeSorter.maxShortSide);
				Assert("Should keep when ratio of page when resizing.", Math.Abs(originalRatio - afterResizeRatio) < 0.001); // Some precision will be lost when calculating the new size of image
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessPageForBarcodesUsingDocTypeBarcode()
		{
			using (Bitmap barcodePage = (Bitmap)Bitmap.FromFile(Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"barcode_hires.tif")))
			{
				BaseBarcode barcode = Sorter.ProcessPageForBarcodes(barcodePage);
				Assert("DocType Barcode should be returned", barcode is DocTypeBarcode);
				AssertEquals("Correct info should be stored", "CIV", ((DocTypeBarcode)barcode).DocType);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessPageForBarcodesWithStandardBarcode()
		{
			using (Bitmap barcodePage = (Bitmap)Bitmap.FromFile(Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"cn1.png")))
			{
				BaseBarcode barcode = Sorter.ProcessPageForBarcodes(barcodePage);
				AssertNull("No barcode returned", barcode);

				barcode = Sorter.ProcessPageForBarcodes(barcodePage, "SHP");
				Assert("Barcode should be returned", barcode is DocTypeBarcode);
				AssertEquals("Reference PK is correct", "CN0000001", barcode.RefCode);
			}
		}

		public void TestGetBarcodeType()
		{
			StringCollection barcodeStrings = new StringCollection();
			barcodeStrings.Add("ABC");
			AssertNull("barcodes passed in not valid, so getbarcodetype() returns null", Sorter.GetBarcodeTypeExposed(barcodeStrings));

			barcodeStrings = new StringCollection();
			barcodeStrings.Add("^SHP=123456;CIV|");
			Assert("DocType barcode object is returned", Sorter.GetBarcodeTypeExposed(barcodeStrings) is DocTypeBarcode);

			barcodeStrings = new StringCollection();
			barcodeStrings.Add("[ROHCIVSYDLAXH1234567890]");
			Assert("Shipment barcode object is returned", Sorter.GetBarcodeTypeExposed(barcodeStrings) is ShipmentBarcode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BarcodeSorter(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()));
		}

		protected override void SetUp()
		{
			base.SetUp();
			MasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			Sorter = new BarcodeSorterExposed(MasterFactory);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		string LargeImageWithBarcodeTifPath
		{
			get
			{
				if (string.IsNullOrEmpty(largeImageWithBarcodeTifPath))
				{
					largeImageWithBarcodeTifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.LargeImageWithBarcode.tif");
				}
				return largeImageWithBarcodeTifPath;
			}
		}
		string largeImageWithBarcodeTifPath;

		BarcodeSorterExposed Sorter;
		DocumentFactory MasterFactory;
	}
}
