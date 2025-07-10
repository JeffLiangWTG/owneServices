using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.NZP.CMS
{
	public abstract class CMSFlatFileConverterTestCase : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			Db.Connection.BeginTransaction(); // Modifying Number Fountain must be done within a transaction.
			CMSBatchNumberFountain.Initialise();
		}

		protected override void TearDown()
		{
			Db.Connection.RollbackTransaction(); // Modifying Number Fountain must be done within a transaction.
			base.TearDown();
		}

		protected void AssertRowsExportAndTrasnactionCode(Xsd.TxnHeader header, Xsd.TxnType headerType, string invoiceTerm, string transactionCode)
		{
			header.TxnType = headerType;
			header.InvTerm = invoiceTerm;
			FlatFileDataRowCollection dataRows = Converter.MapExport(header);
			AssertEquals("one row should be created", 1, dataRows.Count);
			CMSFlatFileDataRow row = (CMSFlatFileDataRow)dataRows[0];
			AssertEquals(transactionCode, transactionCode, row.TransactionCode);
		}

		protected void AssertNoRowsExported(Xsd.TxnHeader header, Xsd.TxnType headerType, string invoiceTerm)
		{
			header.TxnType = headerType;
			header.InvTerm = invoiceTerm;
			FlatFileDataRowCollection dataRows = Converter.MapExport(header);
			AssertEquals("No row should be created", 0, dataRows.Count);
		}

		protected abstract ICMSConverter Converter { get; }

		protected interface ICMSConverter
		{
			FlatFileDataRowCollection MapExport(IValueObject valueObject);
		}
	}
}
