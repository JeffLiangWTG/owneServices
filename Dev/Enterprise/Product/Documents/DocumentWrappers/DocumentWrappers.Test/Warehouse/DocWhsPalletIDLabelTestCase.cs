using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsPalletIDLabel))]
	sealed class DocWhsPalletIDLabelTestCase : DocWhsLabelTest
	{
		#region Test Cases

		#region Properties

		public void TestPalletIDBarcode()
		{
			PalletIDLabelWrapper.PalletID = "12345";
			TextBarcode barcode = new TextBarcode(PalletIDLabelWrapper.PalletID);
			AssertEquals(barcode.TextAs128sFontString, PalletIDLabelWrapper.PalletIDBarcode);
		}

		public void TestPalletID()
		{
			PalletIDLabelWrapper.PalletID = "12345";
			AssertEquals("12345", PalletIDLabelWrapper.PalletID);

			PalletIDLabelWrapper.PalletID = "54321";
			AssertEquals("54321", PalletIDLabelWrapper.PalletID);
		}

		public void TestPrintDate()
		{
			ZDateTime printDate = ZDateTime.Now;
			PalletIDLabelWrapper.PrintDate = printDate;
			AssertEquals(printDate, PalletIDLabelWrapper.PrintDate);

			printDate = new ZDateTime(2008, 06, 29);
			PalletIDLabelWrapper.PrintDate = printDate;
			AssertEquals(printDate, PalletIDLabelWrapper.PrintDate);
		}

		#endregion

		#endregion

		#region Implementation

		DocWhsPalletIDLabel PalletIDLabelWrapper
		{
			get { return (DocWhsPalletIDLabel)base.labelWrapper; }
		}

		protected override DocWhsLabel GetNewDocWrapper(WhsLabel label, BusinessObjectFactory factoryToWrap)
		{
			return DocWhsPalletIDLabel.New(label, factoryToWrap);
		}

		#endregion
	}
}
