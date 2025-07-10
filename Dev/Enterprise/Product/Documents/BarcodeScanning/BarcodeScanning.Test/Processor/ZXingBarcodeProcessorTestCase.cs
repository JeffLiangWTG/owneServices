using System.Drawing;
using System.Linq;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.Barcode.Business.Processor.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	abstract class ZXingBarcodeProcessorTestCase<T> : TestCase where T : IBarcodeProcessor
	{
		#region Bar Code

		#region Parse Tests

		public void TestParseControlBarCode()
		{
			// Arrange
			// Act
			var singleResult = ProcessorForTest.ParseCode(ControlBarCodeBitmap);
			// Assert
			AssertNull("The single bar code bitmap should not be parsed.", singleResult);
		}

		public void TestParseSingleBarCode()
		{
			// Arrange
			// Act
			var result = ProcessorForTest.ParseCode(SingleBarCodeBitmap);
			// Assert
			AssertNotNull("The single bar code bitmap should be parsed.", result);
			Assert("The parsed region is incorrect.", result.Region.Any());
			AssertEquals("The parsed content is incorrect.", SingleBarCodeBitmapContent, result.Text);
		}

		#endregion

		#region Implementations

		#region ProcessorForTest

		/// <summary>
		/// Called to create the processor for test.
		/// </summary>
		/// <returns>The created procesor for test.</returns>
		protected abstract T CreateProcessorForTest();

		/// <summary>
		/// Gets the processor for test.
		/// </summary>
		protected T ProcessorForTest
		{
			get
			{
				if (processorForTest == null)
				{
					processorForTest = CreateProcessorForTest();
				}
				return processorForTest;
			}
		}
		T processorForTest;

		#endregion

		#region ControlBarCodeBitmap

		/// <summary>
		/// Gets the control bar code bitmap for test.
		/// </summary>
		protected virtual Bitmap ControlBarCodeBitmap
		{
			get
			{
				using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
				{
					return new Bitmap(resourceRetriever.GetStream("qr_control.png"));
				}
			}
		}

		#endregion

		#region SingleBarCodeBitmap

		/// <summary>
		/// Gets the single bar code bitmap for test.
		/// </summary>
		protected abstract Bitmap SingleBarCodeBitmap { get; }
		/// <summary>
		/// Gets the actual content in the single bar code bitmap for test.
		/// </summary>
		protected virtual string SingleBarCodeBitmapContent { get; } = "MECARD:N:Justin Chen;TEL:+86 18888888888;EMAIL:justin.chen@wisetechglobal.com;";

		#endregion

		#endregion

		#endregion

	}
}
