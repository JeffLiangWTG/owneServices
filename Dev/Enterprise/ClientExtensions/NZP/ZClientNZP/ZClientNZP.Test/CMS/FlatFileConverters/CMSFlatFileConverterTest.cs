using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.NZP.CMS.Testing
{
	public class CMSFlatFileConverterTest : CMSFlatFileConverterTestCase
	{
		[TestDate(2005, 10, 20, 9, 45, 58)]
		public void TestMapExport()
		{
			NZPDataRegistry.Instance.CurrentCMSWarehouseCode = "9F8A";
			Xsd.TxnHeader header = CreateHeader();
			string batchNumber = CMSBatchNumberFountain.New().GetGenerateFileID(Db.Connection, false);
			FlatFileDataRowCollection dataRows = Converter.MapExport(header);
			AssertEquals("Three rows should be created", 3, dataRows.Count);
			AssertRow((CMSFlatFileDataRow)dataRows[0], batchNumber, "CARTAGE");
			AssertRow((CMSFlatFileDataRow)dataRows[1], batchNumber, "FINANCE");
			AssertRow((CMSFlatFileDataRow)dataRows[2], batchNumber, ZString.Empty);
		}

		[TestDate(2005, 10, 20, 9, 45, 58)]
		public void TestUserReference1()
		{
			NZPDataRegistry.Instance.CurrentCMSWarehouseCode = "9F8A";
			Xsd.TxnHeader header = CreateHeader();
			header.JobInvoiceNo = "S00001000";
			string batchNumber = CMSBatchNumberFountain.New().GetGenerateFileID(Db.Connection, false);
			FlatFileDataRowCollection dataRows = Converter.MapExport(header);
			AssertEquals("Three rows should be created", 3, dataRows.Count);
			CMSFlatFileDataRow row = (CMSFlatFileDataRow)dataRows[0];
			AssertEquals("Should be S00001000, length is not greater 10, leading zero should not be removed", "S00001000", row.UserReference1);
			header.JobInvoiceNo = "S00001000/A";
			dataRows = Converter.MapExport(header);
			AssertEquals("Three rows should be created", 3, dataRows.Count);
			row = (CMSFlatFileDataRow)dataRows[0];
			AssertEquals("Should be S0001000/A, length is  greater 10, leading zero should  be removed", "S0001000/A", row.UserReference1);
			header.JobInvoiceNo = ZString.Empty;
			header.TxnNumber = "00001234567";
			dataRows = Converter.MapExport(header);
			AssertEquals("Three rows should be created", 3, dataRows.Count);
			row = (CMSFlatFileDataRow)dataRows[0];
			AssertEquals("Transaction number 0001234567 is populated as there is no job invoice no , length is  greater 10, leading zero should  be removed", "0001234567", row.UserReference1);
		}

		void AssertRow(CMSFlatFileDataRow row, ZString batchNumber, ZString productCode)
		{
			AssertEquals("Record Type", Constants.RecordType, row.RecordType);
			AssertEquals("Account Code", "   SJLECJI", row.AccountCode);
			AssertEquals("Warehouse Code", "9F8A", row.WarehouseCode);
			AssertEquals("Transaction Code", "CWAE", row.TransactionCode);
			AssertEquals("Product Code", productCode, row.ProductCode);
			AssertEquals("Transaction Quantity", Constants.Quantity, row.TransactionQuantity);
			AssertEquals("Transaction Date", "20-Oct-2005", row.FileGeneratedDate.ToString(Constants.DateFormat));
			AssertEquals("Delivery Code", "", row.DeliveryCode);
			AssertEquals("Exempt Number", "", row.ExemptNumber);
			AssertEquals("Tax Total", 20m, row.TaxTotal);
			AssertEquals("Line Total", 220m, row.LineTotal);
			AssertEquals("Pricing Indicator", Constants.PricingIndicator, row.PricingIndicator);
			AssertEquals("Final Price", 200m, row.FinalPrice);
			AssertEquals("Comment", "AUSYD-USLAX", row.Comment);
			AssertEquals("CONT", "", row.CONT);
			AssertEquals("Reject Code", "", row.RejectCode);
			AssertEquals("Batch Number", batchNumber, row.BatchNumber);
			AssertEquals("External ID", "", row.ExternalID);
			AssertEquals("Intermediary", "", row.Intermediary);
			AssertEquals("Reference", "00010001", row.Reference);
			AssertEquals("Line Item", Constants.LineItem, row.LineItem);
			AssertEquals("Batch Date", "20-Oct-2005", row.BatchDate.ToString(Constants.DateFormat));
			AssertEquals("Transaction Date", "28-Feb-2005", row.TransactionDate.ToString(Constants.DateFormat));
			AssertEquals("Item", "", row.Item);
			AssertEquals("Weight", 234.437m, row.Weight);
			AssertEquals("Volume", 78.92m, row.Volume);
			AssertEquals("User Reference 1", "S0006586/A", row.UserReference1);
			AssertEquals("User Reference 2", "", row.UserReference2);
			AssertEquals("Activity 1", "", row.Activity1);
			AssertEquals("Activity 2", "", row.Activity2);
			AssertEquals("Reference Number", "", row.ReferenceNumber);
			AssertEquals("Extension Reference", "", row.ExtensionReference);
			AssertEquals("Normal Price", "", row.NormalPrice);
			AssertEquals("Code", "", row.Code);
			AssertEquals("Value", "", row.Value);
			AssertEquals("Version Number", "", row.VersionNumber);
		}

		public void TestBatchNumber()
		{
			Xsd.TxnHeader header = CreateHeader();
			string batchNumber = CMSBatchNumberFountain.New().GetGenerateFileID(Db.Connection, false);
			AssertBatchNumber("Batch Number", batchNumber, header);
			AssertBatchNumber("Batch Number was not lazy loaded, lazy loaded is needed so the batch number stays the same for the current exporter/converter/file.", batchNumber, header);
		}

		void AssertBatchNumber(ZString message, ZString batchNumber, Xsd.TxnHeader header)
		{
			FlatFileDataRowCollection dataRows = Converter.MapExport(header);
			AssertEquals("Three rows should be created", 3, dataRows.Count);
			foreach (CMSFlatFileDataRow row in dataRows)
			{
				AssertEquals(message, batchNumber, row.BatchNumber);
			}
		}

#region Implementation
		protected override ICMSConverter Converter
		{
			get
			{
				if (fConverter == null)
				{
					fConverter = new CMSFlatFileConverterTestClass(new NotificationBuffer(), Factory);
				}

				return fConverter;
			}
		}

		Xsd.TxnHeader CreateHeader()
		{
			Xsd.TxnHeader header = new Xsd.TxnHeader();
			header.InvTerm = Core.Constants.InvoiceTerms.CashOnDelivery;
			header.LocalTaxAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)20m, Core.Constants.CurrencyCodes.Australia);
			header.LocalInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)220m, Core.Constants.CurrencyCodes.Australia);
			header.LocalInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)200m, Core.Constants.CurrencyCodes.Australia);
			header.TxnNumber = "90234709";
			header.TxnReference = "23OJSDAU";
			header.PostDate = new ZDateTime(2005, 2, 28, 9, 31, 12);
			header.DebtorOrCreditor.EDICode = "SJLECJI";
			header.JobInvoiceNo = "S00006586/A";
			header.TxnNumber = "00010001";
			header.Branch = GlbBranch.CurrentBranch.GB_Code;
			Xsd.TxnLine line = header.TxnLines.AddNew();
			line.OriginPortCode = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			line.DestinationPortCode = Xsd.UNLOCO.FromPortCode(Factory, "USLAX");
			line.Weight = Xsd.DimensionValue.FromAmountAndUnit((ZDecimal)234.43732m, Core.Constants.Weight.Kilograms);
			line.Volume = Xsd.DimensionValue.FromAmountAndUnit((ZDecimal)78.924m, Core.Constants.Volume.CubicMetres);
			line.ChargeCodeSalesGroup = "CARTAGE";
			line = header.TxnLines.AddNew();
			line.Weight = Xsd.DimensionValue.FromAmountAndUnit((ZDecimal)4554.1578m, Core.Constants.Weight.Kilograms);
			line.Volume = Xsd.DimensionValue.FromAmountAndUnit((ZDecimal)948.34645m, Core.Constants.Volume.CubicMetres);
			line.ChargeCodeSalesGroup = "FINANCE";
			line = header.TxnLines.AddNew();
			line.ChargeCodeSalesGroup = ZString.Empty;
			line = header.TxnLines.AddNew();
			line.ChargeCodeSalesGroup = "CARTAGE";
			line = header.TxnLines.AddNew();
			line.ChargeCodeSalesGroup = ZString.Empty;
			line = header.TxnLines.AddNew();
			line.ChargeCodeSalesGroup = "FINANCE";
			AddLineAmounts(header);
			return header;
		}

		void AddLineAmounts(Xsd.TxnHeader header)
		{
			foreach (Xsd.TxnLine line in header.TxnLines)
			{
				line.LocalTaxAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)10m, Core.Constants.CurrencyCodes.Australia);
				line.LocalInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)110m, Core.Constants.CurrencyCodes.Australia);
				line.LocalInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)100m, Core.Constants.CurrencyCodes.Australia);
			}
		}

		CMSFlatFileConverterTestClass fConverter;
		class CMSFlatFileConverterTestClass : CMSFlatFileConverter, ICMSConverter
		{
			public CMSFlatFileConverterTestClass(INotifications notify, BusinessObjectFactory factory) : base(notify, factory)
			{
			}

			public new FlatFileDataRowCollection MapExport(IValueObject valueObject)
			{
				return base.MapExport(valueObject);
			}

			protected override bool IsOkToExport(Xsd.TxnHeader header)
			{
				return true;
			}

			protected override string TransactionCode(Xsd.TxnHeader header)
			{
				return "CWAE";
			}
		}
#endregion
	}
}
