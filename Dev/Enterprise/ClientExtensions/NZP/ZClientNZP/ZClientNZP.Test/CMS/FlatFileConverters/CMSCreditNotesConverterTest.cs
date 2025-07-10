using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.NZP.CMS.Testing
{
	public class CMSCreditNotesConverterTest : CMSFlatFileConverterTestCase
	{
		public void TestExportAndTransactionCode()
		{
			CMSCreditNotesConverterTestClass converter = new CMSCreditNotesConverterTestClass(new NotificationBuffer(), Factory);
			Xsd.TxnHeader header = new Xsd.TxnHeader();
			header.TxnLines.AddNew();
			AssertNoRowsExported(header, Xsd.TxnType.OPY, "");
			AssertRowsExportAndTrasnactionCode(header, Xsd.TxnType.CRD, Core.Constants.InvoiceTerms.CashOnDelivery, Constants.CashCreditNotesTransactionCode);
			AssertRowsExportAndTrasnactionCode(header, Xsd.TxnType.CRD, Core.Constants.InvoiceTerms.FromShipmentDate, Constants.CreditNotesTransactionCode);
		}

		public void TestAmountsAreExportedAsPositiveFigures()
		{
			Xsd.TxnHeader header = new Xsd.TxnHeader();
			Xsd.TxnLine line = header.TxnLines.AddNew();
			header.TxnType = Xsd.TxnType.CRD;
			line.LocalInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)(-100.4234m), Core.Constants.CurrencyCodes.Australia);
			line.LocalInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)(-110.4234m), Core.Constants.CurrencyCodes.Australia);
			line.LocalTaxAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)(10m), Core.Constants.CurrencyCodes.Australia);
			line.ChargeCodeSalesGroup = "CARTAGE";
			line = header.TxnLines.AddNew();
			header.TxnType = Xsd.TxnType.CRD;
			line.LocalInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)(-50.4234m), Core.Constants.CurrencyCodes.Australia);
			line.LocalInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)(-55.4234m), Core.Constants.CurrencyCodes.Australia);
			line.LocalTaxAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)(5m), Core.Constants.CurrencyCodes.Australia);
			line.ChargeCodeSalesGroup = "CARTAGE";
			CMSCreditNotesConverterTestClass converter = new CMSCreditNotesConverterTestClass(new NotificationBuffer(), Factory);
			FlatFileDataRowCollection rows = converter.MapExport(header);
			AssertEquals("1 row should be exported", 1, rows.Count);
			CMSFlatFileDataRow row = (CMSFlatFileDataRow)rows[0];
			AssertEquals("Amount should be positive", 150.8468m, row.FinalPrice);
			AssertEquals("Amount should be positive", 165.8468m, row.LineTotal);
			AssertEquals("Amoutt should be positive", 15m, row.TaxTotal);
		}

#region Implementation
		protected override ICMSConverter Converter
		{
			get
			{
				if (fConverter == null)
				{
					fConverter = new CMSCreditNotesConverterTestClass(new NotificationBuffer(), Factory);
				}

				return fConverter;
			}
		}

		CMSCreditNotesConverterTestClass fConverter;
		class CMSCreditNotesConverterTestClass : CMSCreditNotesConverter, ICMSConverter
		{
			public CMSCreditNotesConverterTestClass(INotifications notify, BusinessObjectFactory factory) : base(notify, factory)
			{
			}

			public new FlatFileDataRowCollection MapExport(IValueObject valueObject)
			{
				return base.MapExport(valueObject);
			}
		}
#endregion
	}
}
