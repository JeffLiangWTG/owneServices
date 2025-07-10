using System;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TNT.OutTurnDataImport.Testing
{
	public class OutTurnFileFormatTest : FlatFileFormatTestCase
	{
		public void TestConstructor()
		{
			OutTurnFileFormat format = new OutTurnFileFormat();
			AssertEquals("FileExtensionForExport", FileExtensionType.Txt, format.FileExtensionForExport);
			AssertEquals("FileExtensionForImport", FileExtensionType.ClientSpecific, format.FileExtensionForImport);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestConvertToLine()
		{
			OutTurnFileFormat format = new OutTurnFileFormat();
			format.ConvertToLine(new FlatFileDataRow(1));
		}

		public void TestConvertToRow()
		{
			string rawRow = "||SECTOR|||||HAWB|||||";
			OutTurnFileFormat fileFormat = new OutTurnFileFormat();
			OutTurnFlatFileDataRow dataRow = fileFormat.ConvertToRow(rawRow) as OutTurnFlatFileDataRow;
			AssertNotNull("ConvertToRow should return type OutTurnFlatFileDataRow", dataRow);
			AssertEquals("Sector Information", "      SECTOR                   ", dataRow.SectorInformation);
			AssertEquals("HAWB", "HAWB", dataRow.HAWB);
			AssertEquals("ConsignmentOrigin", "", dataRow.ConsignmentOrigin);
			AssertEquals("ConsignmentDestination", "", dataRow.ConsignmentDestination);
			AssertEquals("DocumentIndicator", "", dataRow.DocumentIndicator);
			AssertEquals("ManifestPieces", 0, dataRow.ManifestPieces);
			AssertEquals("LandedPieces", 0, dataRow.LandedPieces);
		}

		public void TestGetClientSpecificFileExtension()
		{
			OutTurnFileFormatTestClass fileFormat = new OutTurnFileFormatTestClass();
			AssertEquals("Client Specific Extension", TNTConstants.OuturnFileExtension, fileFormat.GetClientSpecificFileExtension());
		}

		public void TestFileExtensionForImport()
		{
			OutTurnFileFormatTestClass fileFormat = new OutTurnFileFormatTestClass();
			AssertEquals("File Extension for Import", FileExtensionType.ClientSpecific, fileFormat.FileExtensionForImport);
		}

		public class OutTurnFileFormatTestClass : OutTurnFileFormat
		{
			public new string GetClientSpecificFileExtension()
			{
				return base.GetClientSpecificFileExtension();
			}
		}

		protected override FlatFileFormat GetFlatFileFormat()
		{
			return new OutTurnFileFormat();
		}
	}
}
