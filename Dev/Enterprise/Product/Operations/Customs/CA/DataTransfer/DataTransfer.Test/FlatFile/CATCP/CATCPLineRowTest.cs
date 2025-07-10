using NUnit.Framework;

namespace Enterprise.Customs.CA.DataTransfer.Testing
{
	[TestedType(typeof(CATCPLineRow))]
	sealed class CATCPLineRowTest : CATCPRowTest
	{
		protected override CATCPRow GetFullyPopulatedDataRow()
		{
			var row = new CATCPLineRow();
			row[CATCPLineRow.Schema.RecordIdentifier] = "03";
			row[CATCPLineRow.Schema.BusinessNumber] = "100035922RM0001";
			row[CATCPLineRow.Schema.TCPTypeCode] = "02";
			row[CATCPLineRow.Schema.TCPIdentifier] = "FENVEN";
			row[CATCPLineRow.Schema.AddressLine1] = "500 S ALTA ST";
			row[CATCPLineRow.Schema.AddressLine2] = "2ND ADD";
			row[CATCPLineRow.Schema.City] = "GONZALES";
			row[CATCPLineRow.Schema.ProvinceStateCode] = "CA";
			row[CATCPLineRow.Schema.CountryCode] = "US";
			row[CATCPLineRow.Schema.PostalZipCode] = "93926";
			row[CATCPLineRow.Schema.BusinessName] = "DOLE FRESH VEGETABLES";
			return row;
		}

		protected override string ExpectedData => "03100035922RM000102FENVEN         500 S ALTA ST                 2ND ADD                       GONZALES                      CAUS93926     DOLE FRESH VEGETABLES";
	}
}
