using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.ELG.Testing
{
	public class SagAPConverterTest : SagAccountsConverterTest
	{
		public void TestTaxRateID()
		{
			TestHelper.SetValidRegistryAll();
			Xsd.TxnHeader sourceXml = GetSource();
			FlatFileDataRowCollection expectedRows = SetExpectedRows();
			FlatFileDataRowCollection actualRows = SetActual(sourceXml);
			AssertEquals("AssertRows", Format.ConvertToLine(expectedRows[0]), Format.ConvertToLine(actualRows[0]));
			Assert("should have one", sourceXml.TxnLines.Count > 0);
			sourceXml.TxnLines[0].TaxCode = "VAT";
			actualRows = SetActual(sourceXml);
			((SagAPExternalInvoiceDataRow)expectedRows[0]).TaxAnalysisTaxRate_1 = "2";
			AssertEquals("AssertRows", Format.ConvertToLine(expectedRows[0]), Format.ConvertToLine(actualRows[0]));
			sourceXml.TxnLines[0].TaxCode = "FREEVAT";
			actualRows = SetActual(sourceXml);
			((SagAPExternalInvoiceDataRow)expectedRows[0]).TaxAnalysisTaxRate_1 = "1";
			AssertEquals("AssertRows", Format.ConvertToLine(expectedRows[0]), Format.ConvertToLine(actualRows[0]));
			sourceXml.TxnLines[0].TaxCode = "LOWVATREV";
			actualRows = SetActual(sourceXml);
			((SagAPExternalInvoiceDataRow)expectedRows[0]).TaxAnalysisTaxRate_1 = "4";
			AssertEquals("AssertRows", Format.ConvertToLine(expectedRows[0]), Format.ConvertToLine(actualRows[0]));
			sourceXml.TxnLines[0].TaxCode = "LOWVAT";
			actualRows = SetActual(sourceXml);
			((SagAPExternalInvoiceDataRow)expectedRows[0]).TaxAnalysisTaxRate_1 = "2";
			AssertEquals("AssertRows", Format.ConvertToLine(expectedRows[0]), Format.ConvertToLine(actualRows[0]));
			sourceXml.TxnLines[0].TaxCode = "MIDVATREV";
			actualRows = SetActual(sourceXml);
			((SagAPExternalInvoiceDataRow)expectedRows[0]).TaxAnalysisTaxRate_1 = "4";
			AssertEquals("AssertRows", Format.ConvertToLine(expectedRows[0]), Format.ConvertToLine(actualRows[0]));
			sourceXml.TxnLines[0].TaxCode = "MIDVAT";
			actualRows = SetActual(sourceXml);
			((SagAPExternalInvoiceDataRow)expectedRows[0]).TaxAnalysisTaxRate_1 = "2";
			AssertEquals("AssertRows", Format.ConvertToLine(expectedRows[0]), Format.ConvertToLine(actualRows[0]));
			sourceXml.TxnLines[0].TaxCode = "FREEVATREV";
			actualRows = SetActual(sourceXml);
			((SagAPExternalInvoiceDataRow)expectedRows[0]).TaxAnalysisTaxRate_1 = "7";
			AssertEquals("AssertRows", Format.ConvertToLine(expectedRows[0]), Format.ConvertToLine(actualRows[0]));
			sourceXml.TxnLines[0].TaxCode = "VATREV";
			actualRows = SetActual(sourceXml);
			((SagAPExternalInvoiceDataRow)expectedRows[0]).TaxAnalysisTaxRate_1 = "8";
			AssertEquals("AssertRows", Format.ConvertToLine(expectedRows[0]), Format.ConvertToLine(actualRows[0]));
		}

		protected override Xsd.TxnHeader GetSource()
		{
			Xsd.TxnHeader result = base.GetSource();
			result.Ledger = Xsd.TxnLedgerType.AP;
			result.TxnType = Xsd.TxnType.CRD;
			return result;
		}

		protected override FlatFileDataRowCollection SetExpectedRows()
		{
			FlatFileDataRowCollection results = new FlatFileDataRowCollection();
			SagAPExternalInvoiceDataRow result = new SagAPExternalInvoiceDataRow();
			result.AccountNumber = "ABC.AUD.AP";
			result.DueDate = new ZDateTime(2007, 5, 11);
			result.GoodsValueInAccountCurrency = 500.25m;
			result.PerControlValueInBaseCurrency = 500.25m;
			result.DocumentToBaseCurrencyRate = 1;
			result.DocumentToAccountCurrencyRate = 1;
			result.TransactionReference = "Shipment";
			result.SecondReference = "000002053";
			result.Source = 2;
			result.SYSTraderTranType = 5;
			result.TransactionDate = new ZDateTime(2007, 5, 1);
			result.TaxValue = 20.025m;
			result.NominalAnalysisTransactionValue_1 = 450.225m;
			result.NominalAnalysisNominalAccountNumber_1 = "ABC";
			result.NominalAnalysisNominalCostCentre_1 = "ABC";
			result.NominalAnalysisNominalDepartment_1 = "ASD";
			result.NominalAnalysisNominalAnalysisNarrative_1 = "Import from CargoWise One";
			result.NominalAnalysisTransactionAnalysisCode_1 = ZString.Empty;
			result.TaxAnalysisTaxRate_1 = "0";
			result.TaxAnalysisGoodsValueBeforeDiscount_1 = 450.225m;
			result.TaxAnalysisTaxOnGoodsValue_1 = 20.025m;
			results.Add(result);
			return results;
		}

		protected override FlatFileDataRowCollection SetActual(Xsd.TxnHeader xmlHeader)
		{
			return new SagAPConverterForTest(Factory, Notifications).MapExport(xmlHeader);
		}

		class SagAPConverterForTest : SagAPConverter
		{
			public SagAPConverterForTest(BusinessObjectFactory factory, NotificationBuffer notifications) : base(factory, notifications)
			{
			}

			public new FlatFileDataRowCollection MapExport(IValueObject valueObject)
			{
				return base.MapExport(valueObject);
			}
		}
	}
}
