using System;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.ECU.ConsolExport.Testing
{
	public class ECUFlatFileFormatTest : FlatFileFormatTestCase
	{
		public void TestFileExtensionType()
		{
			ECUFlatFileFormat testFlatFileFormat = new ECUFlatFileFormat();
			AssertEquals("File Extension Type Should be a ECU File", FileExtensionType.ClientSpecific, testFlatFileFormat.FileExtensionForExport);
		}

		[ExpectException(typeof(NotImplementedException))]
		public void TestConvertToRowThrowsException()
		{
			ECUFlatFileFormat testFlatFileFormat = new ECUFlatFileFormat();
			testFlatFileFormat.ConvertToRow("");
		}

		[ExpectException(typeof(NotImplementedException))]
		public new void TestGetClientExtensionReturnsValueForImport()
		{
			base.TestGetClientExtensionReturnsValueForImport();
		}

		public void TestConvertToLine()
		{
			FlatFileDataRow dataRow = new FlatFileDataRow(2);
			dataRow.SetField(0, "ab");
			dataRow.SetField(1, "cd");
			ECUFlatFileFormat format = new ECUFlatFileFormat();
			AssertEquals("abcd", format.ConvertToLine(dataRow));
		}

#region Setup
		protected override FlatFileFormat GetFlatFileFormat()
		{
			return new ECUFlatFileFormat();
		}
#endregion
	}
}
