using System;
using System.Drawing;
using System.Text;
using CargoWise.Application;
using CargoWise.BrandManager;
using Enterprise.DocumentEngine;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class FileAssociationRetrieverTest : TransactionedTestCase
	{
		public void TestGetFriendlyDocumentName()
		{
			FileAssociationRetriever retriever = new FileAssociationRetriever();

			// don't hardcode the application name in the test, because some people  
			// have different applications associated with text/word files.
			StringBuilder applicationName = new StringBuilder(256, 256);
			int bufferLength = applicationName.Capacity;
			Win32.AssocQueryString(0, Win32.ASSOCSTR_FRIENDLYDOCNAME, ".TXT", null, applicationName, ref bufferLength);
			AssertEquals(applicationName.ToString(), retriever.GetFriendlyDocumentName("TXT"));

			applicationName = new StringBuilder(256, 256);
			bufferLength = applicationName.Capacity;
			Win32.AssocQueryString(0, Win32.ASSOCSTR_FRIENDLYDOCNAME, ".DOC", null, applicationName, ref bufferLength);
			AssertEquals(applicationName.ToString(), retriever.GetFriendlyDocumentName("DOC"));

			string expectedFriendlyName;
			if (TestingState.IsVista)
			{
				expectedFriendlyName = "EDG File";
			}
			else
			{
				expectedFriendlyName = ".EDG File";
			}

			AssertEquals(expectedFriendlyName, retriever.GetFriendlyDocumentName("EDG"));

			AssertEquals($"{Core.Constants.ProductName} eDoc", retriever.GetFriendlyDocumentName("TIF"));
			AssertEquals($"{Core.Constants.ProductName} eDoc", retriever.GetFriendlyDocumentName("JPG"));
			AssertEquals($"{Core.Constants.ProductName} eDoc", retriever.GetFriendlyDocumentName("JPEG"));
		}

		public void TestGetIconForExtension()
		{
			FileAssociationRetriever retriever = new FileAssociationRetriever();
			Bitmap icon = retriever.GetIconForExtension("DOC");
			AssertNotNull("Icon shouldn't be null", icon);
			Assert("Should find an icon for word doc, so should return a bitmap size > (1,1)", icon.Size != new Size(1, 1));

			icon = retriever.GetIconForExtension("EDG");
			AssertNotNull("even with unknown extension, shouldn't return null", icon);
			Assert("Icon for the unknown extension should be whatever windows uses as default icon, shouldn't be an empty bitmap of size 1,1", icon.Size != new Size(1, 1));

			icon = retriever.GetIconForExtension("TIF");
			AssertNotNull("Icon shouldn't be null for a tif file", icon);
			AssertEquals("Icon should be the enterprise logo", Math.Min(64, ObjectFactory.Get<IDpiScalingHelper>().ScaleToCurrentDpiX(32)), icon.Size.Width);
			AssertEquals("Icon should be the enterprise logo", Math.Min(64, ObjectFactory.Get<IDpiScalingHelper>().ScaleToCurrentDpiY(32)), icon.Size.Height);

			icon = retriever.GetIconForExtension("JPG");
			AssertNotNull("Icon shouldn't be null for a jpg file", icon);
			AssertEquals("Icon should be the enterprise logo", Math.Min(64, ObjectFactory.Get<IDpiScalingHelper>().ScaleToCurrentDpiX(32)), icon.Size.Width);
			AssertEquals("Icon should be the enterprise logo", Math.Min(64, ObjectFactory.Get<IDpiScalingHelper>().ScaleToCurrentDpiY(32)), icon.Size.Height);

			icon = retriever.GetIconForExtension("JPEG");
			AssertNotNull("Icon shouldn't be null for a jpeg file", icon);
			AssertEquals("Icon should be the enterprise logo", Math.Min(64, ObjectFactory.Get<IDpiScalingHelper>().ScaleToCurrentDpiX(32)), icon.Size.Width);
			AssertEquals("Icon should be the enterprise logo", Math.Min(64, ObjectFactory.Get<IDpiScalingHelper>().ScaleToCurrentDpiY(32)), icon.Size.Height);

			Bitmap enterpriseIcon = BrandingFactory.Instance.ProductIcon.ToBitmap();
			for (int x = 0; x < icon.Width; x++)
			{
				for (int y = 0; y < icon.Height; y++)
				{
					AssertPixelColor("Icon should be the enterprise logo", enterpriseIcon.GetPixel(x, y), icon.GetPixel(x, y));
				}
			}
		}

		void AssertPixelColor(string message, Color expected, Color actual)
		{
			AssertPixelChannelWithinAllowedTolerance(message, expected.A, actual.A);
			AssertPixelChannelWithinAllowedTolerance(message, expected.R, actual.R);
			AssertPixelChannelWithinAllowedTolerance(message, expected.G, actual.G);
			AssertPixelChannelWithinAllowedTolerance(message, expected.B, actual.B);
		}

		void AssertPixelChannelWithinAllowedTolerance(string message, byte expected, byte actual)
		{
			var iconColorChannelTolerance = 2;
			Assert(message, expected + iconColorChannelTolerance >= actual);
			Assert(message, expected - iconColorChannelTolerance <= actual);
		}

		[ExpectNoExceptions()]
		public void TestGetIconForExtensionWithEmptyExtension()
		{
			FileAssociationRetriever retriever = new FileAssociationRetriever();
			Bitmap icon = retriever.GetIconForExtension(string.Empty);
			AssertNotNull("Icon shouldn't be null", icon);
		}

		[ExpectNoExceptions()]
		public void TestGetFriendlyDocumentNameWithEmptyExtension()
		{
			FileAssociationRetriever retriever = new FileAssociationRetriever();
			AssertEquals("File", retriever.GetFriendlyDocumentName(string.Empty));
		}
	}
}
