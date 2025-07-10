using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(SummaryLine))]
	internal class SummaryLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			SummaryLine line = new SummaryLine(Factory);
			line.MainDescription = "main";
			line.AdditionalDescription = "additional";
			line.PurchasedCount = "10";
			line.UnitPrice = "32";
			line.TotalUnitCount = "42";
			line.LicenceUnits = "18";
			line.LicenceUnitsAmount = "36";
			line.UnitCount = "10";
			line.Amount = "320";

			AssertEquals("main", line.MainDescription);
			AssertEquals("additional", line.AdditionalDescription);
			AssertEquals("10", line.PurchasedCount);
			AssertEquals("32", line.UnitPrice);
			AssertEquals("42", line.TotalUnitCount);
			AssertEquals("18", line.LicenceUnits);
			AssertEquals("36", line.LicenceUnitsAmount);
			AssertEquals("10", line.UnitCount);
			AssertEquals("320", line.Amount);

			AssertColumnsMapping(line);

			line.Column1 = "Column1";
			line.Column2 = "Column2";
			line.Column3 = "Column3";
			line.Column4 = "Column4";
			line.Column5 = "Column5";
			line.Column6 = "Column6";
			line.Column7 = "Column7";
			line.Column8 = "Column8";
			line.Column9 = "Column9";
			line.Column10 = "Column10";

			AssertEquals("Column1", line.Column1);
			AssertEquals("Column2", line.Column2);
			AssertEquals("Column3", line.Column3);
			AssertEquals("Column4", line.Column4);
			AssertEquals("Column5", line.Column5);
			AssertEquals("Column6", line.Column6);
			AssertEquals("Column7", line.Column7);
			AssertEquals("Column8", line.Column8);
			AssertEquals("Column9", line.Column9);
			AssertEquals("Column10", line.Column10);
			AssertEquals("Column1,Column2,Column3,Column4,Column5,Column6,Column7,Column8,Column9,Column10", line.CodeColumns1To10);

			AssertColumnsMapping(line);
		}

		void AssertColumnsMapping(SummaryLine line)
		{
			AssertEquals("Properties should be the same", line.MainDescription, line.Column1);
			AssertEquals("Properties should be the same", line.AdditionalDescription, line.Column2);
			AssertEquals("Properties should be the same", line.UnitCount, line.Column3);
			AssertEquals("Properties should be the same", line.LicenceUnits, line.Column4);
			AssertEquals("Properties should be the same", line.LicenceUnitsAmount, line.Column5);
			AssertEquals("Properties should be the same", line.UnitPrice, line.Column6);
			AssertEquals("Properties should be the same", line.Amount, line.Column7);
			AssertEquals("Properties should be the same", line.PurchasedCount, line.Column8);
			AssertEquals("Properties should be the same", line.TotalUnitCount, line.Column9);
		}

		public void TestHeader()
		{
			SummaryLine header = new SummaryLine(Factory);
			header.MainDescription = "header";
			header.TotalAmount = "100500";
			header.TotalLicenceUnits = "123456";

			SummaryLine line = new SummaryLine(Factory);
			AssertEquals("Precondition", null, line.Header);

			line.Header = header;
			AssertEquals("Header", header, line.Header);
			AssertEquals("Header total amount", "100500", line.Header.TotalAmount);
			AssertEquals("Header total licence units", "123456", line.Header.TotalLicenceUnits);
		}

		public void TestCode()
		{
			SummaryLine header = new SummaryLine(Factory);
			header.MainDescription = "main";
			header.AdditionalDescription = "additional";
			header.UnitPrice = "32";
			header.LicenceUnits = "8";
			header.LicenceUnitsAmount = "12";
			header.PurchasedCount = "1";
			header.UnitCount = "10";
			header.TotalUnitCount = "11";
			header.Amount = "320";

			header.TopLevelDescription = "TopLevel";
			header.TotalDescription = "Total";
			header.TotalAmount = "TotalAmount";

			AssertEquals("main,additional,1,10,11,8,12,32,320,TopLevel,,,Total,TotalAmount", header.UnsortedCode);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SummaryLine(Factory);
		}

		#endregion
	}
}
