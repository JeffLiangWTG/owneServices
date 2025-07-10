using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Export.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Accounting.Web.Business;
using Enterprise.Accounting.Web.Core;
using Enterprise.Accounting.Web.Exceptions;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Web.Testing
{
	sealed class AccountingTransactionExporterNamespace_2011_11Test : AccountingTransactionExporterBaseTest
	{
		protected override string Namespace
		{
			get { return UniversalXmlInfo.Namespace_2011_11; }
		}

		protected override string AllTransactionsExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\AllTransactionsExported.xml"; }
		}

		protected override string CashBasisVATTransactionsExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\CashBasisVATTransactionsExported.xml"; }
		}

		protected override string CashBasisVATTransactionsExportedAfterMatchingFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\CashBasisVATTransactionsExportedAfterMatching.xml"; }
		}

		protected override string CashBasisVATTransactionsExportedAgainAfterMatchingFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\CashBasisVATTransactionsExportedAgainAfterMatching.xml"; }
		}

		protected override string CashBasisVATTransactionsExportedWithVATRecoverableTaxFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\CashBasisVATTransactionsExportedWithVATRecoverableTax.xml"; }
		}

		protected override string CashBasisVATTransactionsExportedAfterMatchingWithVATRecoverableTaxFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\CashBasisVATTransactionsExportedAfterMatchingWithVATRecoverableTax.xml"; }
		}

		protected override string CashBasisVATTransactionsExportedAgainAfterMatchingWithVATRecoverableTaxFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\CashBasisVATTransactionsExportedAgainAfterMatchingWithVATRecoverableTax.xml"; }
		}

		protected override string InvCrdAdjTransactionsExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvCrdAdjTransactionsExported.xml"; }
		}

		protected override string InvCrdAdjTransactionsExportedWithVATRecoverableTaxFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvCrdAdjTransactionsExportedWithVATRecoverableTax.xml"; }
		}

		protected override string CBDirectPaymentsExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\CBDirectPaymentsExported.xml"; }
		}

		protected override string CBDirectPaymentsExportedWithVATRecoverableTaxFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\CBDirectPaymentsExportedWithVATRecoverableTax.xml"; }
		}

		protected override string TransactionWithCountryAndIBANNumberFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\TransactionWithCountryAndIBANNumber.xml"; }
		}

		protected override string InvoiceWithConsolCostExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithConsolCostExported.xml"; }
		}

		protected override string PostingWipAndAccrualExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\PostingWipAndAccrualExported.xml"; }
		}

		protected override string InvoiceWithConsolCostGovtChargeCodeExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithConsolCostGovtChargeCodeExported.xml"; }
		}

		protected override string InvoiceWithPlaceOfSupplyExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithPlaceOfSupplyExported.xml"; }
		}

		protected override string JRJRevenueAndCostExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\JRJRevenueAndCostExported.xml"; }
		}

		protected override string ReverseWipAndAccrualExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\ReverseWipAndAccrualExported.xml"; }
		}

		protected override string ARInvCrdAdjTransactionsExportedWithInvoiceRemittanceFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\ARInvCrdAdjTransactionsExportedWithInvoiceRemittance.xml"; }
		}

		protected override string APInvCrdAdjTransactionsExportedWithInvoiceRemittanceFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\APInvCrdAdjTransactionsExportedWithInvoiceRemittance.xml"; }
		}

		protected override string WipAndAccrualWithDifferentTaxDatesExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\WipAndAccrualWithDifferentTaxDatesExported.xml"; }
		}

		protected override string InvoiceWithLocalCurrencyAndHighPrecisionExchangeRateFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithLocalCurrencyAndHighPrecisionExchangeRateExported.xml"; }
		}

		protected override string TaxGroupCodeForItaly
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\TaxGroupCodeForItaly.xml"; }
		}

		protected override string TaxGroupCodeForPortugal
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\TaxGroupCodeForPortugal.xml"; }
		}

		protected override string TaxGroupCodeForArgentina
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\TaxGroupCodeForArgentina.xml"; }
		}

		protected override string WipAndAccrualWithRatingBasisFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\WipAndAccrualWithRatingBasisFileName.xml"; }
		}

		protected override string ARAPInvoicesWithRatingBasisFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\ARAPInvoicesWithRatingBasisFileName.xml"; }
		}

		protected override string APInvoiceWithConsolCostRatingBasisFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\APInvoiceWithConsolCostRatingBasisFileName.xml"; }
		}

		protected override string ChargeOSSellGSTShouldBeZeroWhenLocalSellGSTIsZero_WithoutSellInvoiceCurrencyFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\ChargeOSSellGSTShouldBeZeroWhenLocalSellGSTIsZero_WithoutSellInvoiceCurrency.xml"; }
		}

		protected override string ChargeOSSellGSTShouldBeZeroWhenLocalSellGSTIsZero_WithSellInvoiceCurrencyFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\ChargeOSSellGSTShouldBeZeroWhenLocalSellGSTIsZero_WithSellInvoiceCurrency.xml"; }
		}

		protected override string NJLExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\NJLExported.xml"; }
		}

		protected override string TaxTransactionExported_MatchingRealizationBasis => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithMatchingRealizationBasisTax.xml";

		protected override string TaxTransactionExported_PostingRealizationBasis => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithPostingRealizationBasisTax.xml";

		protected override string TaxTransactionExported_TaxTransactionCancelled_Batch1 => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithCancelledTaxTransactionBatch1.xml";

		protected override string TaxTransactionExported_TaxTransactionCancelled_Batch2 => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithCancelledTaxTransactionBatch2.xml";

		protected override string TaxTransactionExported_TaxRealized_Batch1 => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithRealizedTaxBatch1.xml";

		protected override string TaxTransactionExported_TaxRealized_Batch2 => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithRealizedTaxBatch2.xml";

		protected override string TransactionExported_AmendingReversingReason => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\TransactionExported_AmendingReversingReason.xml";

		protected override string InvoiceWithGoodsClassChargeCode => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithGoodsClassChargeCode.xml";

		protected override string InvoiceWithCancelReason => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithCancelReason.xml";

		protected override string InvoiceWithCancelReasonAndCancelledAmendment => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithCancelReasonAndCancelledAmendment.xml";

		protected override string ARInvoiceWithCashAdvanceReceivedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\ARInvoiceWithCashAdvanceReceivedFileName.xml"; }
		}

		protected override string APInvoiceWithCashAdvanceReceivedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\APInvoiceWithCashAdvanceReceivedFileName.xml"; }
		}
	}

	sealed class AccountingTransactionExporterNamespace_2012_12Test : AccountingTransactionExporterBaseTest
	{
		protected override string Namespace
		{
			get { return UniversalXmlInfo.Namespace_2012_11; }
		}

		protected override string AllTransactionsExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\AllTransactionsExported_2.xml"; }
		}

		protected override string CashBasisVATTransactionsExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\CashBasisVATTransactionsExported_2.xml"; }
		}

		protected override string CashBasisVATTransactionsExportedAfterMatchingFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\CashBasisVATTransactionsExportedAfterMatching_2.xml"; }
		}

		protected override string CashBasisVATTransactionsExportedAgainAfterMatchingFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\CashBasisVATTransactionsExportedAgainAfterMatching_2.xml"; }
		}

		protected override string CashBasisVATTransactionsExportedWithVATRecoverableTaxFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\CashBasisVATTransactionsExportedWithVATRecoverableTax_2.xml"; }
		}

		protected override string CashBasisVATTransactionsExportedAfterMatchingWithVATRecoverableTaxFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\CashBasisVATTransactionsExportedAfterMatchingWithVATRecoverableTax_2.xml"; }
		}

		protected override string CashBasisVATTransactionsExportedAgainAfterMatchingWithVATRecoverableTaxFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\CashBasisVATTransactionsExportedAgainAfterMatchingWithVATRecoverableTax_2.xml"; }
		}

		protected override string InvCrdAdjTransactionsExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvCrdAdjTransactionsExported_2.xml"; }
		}

		protected override string InvCrdAdjTransactionsExportedWithVATRecoverableTaxFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvCrdAdjTransactionsExportedWithVATRecoverableTax_2.xml"; }
		}

		protected override string CBDirectPaymentsExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\CBDirectPaymentsExported_2.xml"; }
		}

		protected override string CBDirectPaymentsExportedWithVATRecoverableTaxFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\CBDirectPaymentsExportedWithVATRecoverableTax_2.xml"; }
		}

		protected override string TransactionWithCountryAndIBANNumberFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\TransactionWithCountryAndIBANNumber_2.xml"; }
		}

		protected override string InvoiceWithConsolCostExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithConsolCostExported_2.xml"; }
		}

		protected override string PostingWipAndAccrualExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\PostingWipAndAccrualExported2.xml"; }
		}

		protected override string InvoiceWithConsolCostGovtChargeCodeExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithConsolCostGovtChargeCodeExported_2.xml"; }
		}

		protected override string InvoiceWithPlaceOfSupplyExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithPlaceOfSupplyExported_2.xml"; }
		}

		protected override string JRJRevenueAndCostExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\JRJRevenueAndCostExported_2.xml"; }
		}

		protected override string ReverseWipAndAccrualExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\ReverseWipAndAccrualExported2.xml"; }
		}

		protected override string ARInvCrdAdjTransactionsExportedWithInvoiceRemittanceFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\ARInvCrdAdjTransactionsExportedWithInvoiceRemittance_2.xml"; }
		}

		protected override string APInvCrdAdjTransactionsExportedWithInvoiceRemittanceFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\APInvCrdAdjTransactionsExportedWithInvoiceRemittance_2.xml"; }
		}

		protected override string WipAndAccrualWithDifferentTaxDatesExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\WipAndAccrualWithDifferentTaxDatesExported_2.xml"; }
		}

		protected override string InvoiceWithLocalCurrencyAndHighPrecisionExchangeRateFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithLocalCurrencyAndHighPrecisionExchangeRateExported_2.xml"; }
		}

		protected override string TaxGroupCodeForItaly
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\TaxGroupCodeForItaly2.xml"; }
		}

		protected override string TaxGroupCodeForPortugal
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\TaxGroupCodeForPortugal2.xml"; }
		}

		protected override string TaxGroupCodeForArgentina
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\TaxGroupCodeForArgentina2.xml"; }
		}

		protected override string WipAndAccrualWithRatingBasisFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\WipAndAccrualWithRatingBasisFileName2.xml"; }
		}

		protected override string ARAPInvoicesWithRatingBasisFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\ARAPInvoicesWithRatingBasisFileName2.xml"; }
		}

		protected override string APInvoiceWithConsolCostRatingBasisFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\APInvoiceWithConsolCostRatingBasisFileName2.xml"; }
		}

		protected override string ChargeOSSellGSTShouldBeZeroWhenLocalSellGSTIsZero_WithoutSellInvoiceCurrencyFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\ChargeOSSellGSTShouldBeZeroWhenLocalSellGSTIsZero_WithoutSellInvoiceCurrency2.xml"; }
		}

		protected override string ChargeOSSellGSTShouldBeZeroWhenLocalSellGSTIsZero_WithSellInvoiceCurrencyFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\ChargeOSSellGSTShouldBeZeroWhenLocalSellGSTIsZero_WithSellInvoiceCurrency2.xml"; }
		}

		protected override string NJLExportedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\NJLExported2.xml"; }
		}

		protected override string TaxTransactionExported_MatchingRealizationBasis => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithMatchingRealizationBasisTax_2.xml";

		protected override string TaxTransactionExported_PostingRealizationBasis => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithPostingRealizationBasisTax_2.xml";

		protected override string TaxTransactionExported_TaxTransactionCancelled_Batch1 => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithCancelledTaxTransactionBatch1_2.xml";

		protected override string TaxTransactionExported_TaxTransactionCancelled_Batch2 => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithCancelledTaxTransactionBatch2_2.xml";

		protected override string TaxTransactionExported_TaxRealized_Batch1 => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithRealizedTaxBatch1_2.xml";

		protected override string TaxTransactionExported_TaxRealized_Batch2 => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithRealizedTaxBatch2_2.xml";

		protected override string TransactionExported_AmendingReversingReason => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\TransactionExported_AmendingReversingReason2.xml";

		protected override string InvoiceWithGoodsClassChargeCode => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithGoodsClassChargeCode_2.xml";

		protected override string InvoiceWithCancelReason => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithCancelReason_2.xml";

		protected override string InvoiceWithCancelReasonAndCancelledAmendment => @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\InvoiceWithCancelReasonAndCancelledAmendment_2.xml";

		protected override string ARInvoiceWithCashAdvanceReceivedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\ARInvoiceWithCashAdvanceReceivedFileName_2.xml"; }
		}

		protected override string APInvoiceWithCashAdvanceReceivedFileName
		{
			get { return @"Enterprise\Product\Operations\Accounting\Web\Accounting.Web.Test\XML\APInvoiceWithCashAdvanceReceivedFileName_2.xml"; }
		}
	}

	[UseSnapshotProtection]
	sealed class AccountingTransactionExporterRealDatabaseTransactionTest : TestCase
	{
		public AccountingTransactionExporterRealDatabaseTransactionTest() : base() { }

		public void TestNoPendingDBTransactionsIfNothingExported()
		{
			AssertEquals(0, TransactionHistoryCount);
			AssertEquals(false, HasActiveTransaction);
			var rsp = CreateBatch();

			AssertEquals(1, TransactionHistoryCount);
			AssertEquals(false, HasActiveTransaction);
			AssertEquals(false, rsp.Succeeded);
			AssertEquals("Nothing to be batched.", rsp.ErrorMessage);
		}

		public void TestNoPendingDBTransactionsIfExceptionThrown()
		{
			AssertEquals(0, TransactionHistoryCount);
			AssertEquals(false, HasActiveTransaction);

			string sqlTextEDI = "EXEC AccountingTransactionExportCreateBatch 'EDI'";

			using (var otherConnection = Db.NewExtraConnectionToMainDb())
			{
				otherConnection.BeginTransaction();
				otherConnection.ExecuteNonQuery(sqlTextEDI);

				AssertEquals(0, TransactionHistoryCount);
				AssertEquals(false, HasActiveTransaction);
				var rsp = CreateBatch();

				AssertEquals(false, rsp.Succeeded);
				AssertEquals("Another process is already exporting a batch for company EDI.  You can only create one export batch for each company at a time.  Please try again later.", rsp.ErrorMessage);

				AssertEquals(1, TransactionHistoryCount);
				AssertEquals(false, HasActiveTransaction);

				otherConnection.RollbackTransaction();
			}
		}

		public void TestDBTransactionHistoryCountAndHasActiveDBTransaction()
		{
			AssertEquals(false, HasActiveTransaction);
			AssertEquals(0, TransactionHistoryCount);

			using (var txn = Connection.BeginTransaction())
			{
				AssertEquals(true, HasActiveTransaction);
				AssertEquals(1, TransactionHistoryCount);
			}

			AssertEquals(false, HasActiveTransaction);
			AssertEquals(1, TransactionHistoryCount);
		}

#pragma warning disable IDE0001 // Full name needed to identify SqlClient types
		static readonly PropertyInfo ConnectionInfoSys = typeof(System.Data.SqlClient.SqlConnection).GetProperty("InnerConnection", BindingFlags.NonPublic | BindingFlags.Instance);
#if NET
		static readonly PropertyInfo ConnectionInfoMS = typeof(Microsoft.Data.SqlClient.SqlConnection).GetProperty("InnerConnection", BindingFlags.NonPublic | BindingFlags.Instance);
#endif
		bool HasActiveTransaction
		{
			get
			{
				object internalConn = null;
				if (Connection is System.Data.SqlClient.SqlConnection)
				{
					internalConn = ConnectionInfoSys.GetValue(Connection, null);
				}
#if NET
				else if (Connection is Microsoft.Data.SqlClient.SqlConnection)
				{
					internalConn = ConnectionInfoMS.GetValue(Connection, null);
				}
#endif
				else
				{
					throw new InvalidOperationException($"Unable to determine internal connection type for transaction check. ({internalConn})");
				}

				var currentTransactionProperty = internalConn.GetType().GetProperty("CurrentTransaction", BindingFlags.NonPublic | BindingFlags.Instance);
				var currentTransaction = currentTransactionProperty.GetValue(internalConn, null);
				return currentTransaction != null;
			}
		}

		int TransactionHistoryCount
		{
			get
			{
				System.Collections.IDictionary stats = null;
				if (Connection is System.Data.SqlClient.SqlConnection sqlConnection)
				{
					stats = sqlConnection.RetrieveStatistics();
				}
#if NET
				else if (Connection is Microsoft.Data.SqlClient.SqlConnection connectionMS)
				{
					stats = connectionMS.RetrieveStatistics();
				}
#endif
				else
				{
					throw new InvalidOperationException($"Unable to determine statistics for connection type {Connection.GetType().FullName}.");
				}

				var txnCount = int.Parse(stats["Transactions"].ToString());
				return txnCount;
			}
		}

		AccountingTransactionExportResponse CreateBatch()
		{
			var dataAccess = new BatchExportDataAccess(Connection, null);
			var exporter = new AccountingTransactionWebExporter(dataAccess);
			var response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			return response;
		}

		protected override void SetUp()
		{
			var connection = Db.NewExtraConnectionToMainDb();
			disposable = connection;
			Connection = ((IDbConnectionInternals)connection).ADOConnection;

			if (Connection is System.Data.SqlClient.SqlConnection sqlConnection)
			{
				sqlConnection.StatisticsEnabled = true;
				sqlConnection.ResetStatistics();
			}
#if NET
			else if (Connection is Microsoft.Data.SqlClient.SqlConnection connectionMS)
			{
				connectionMS.StatisticsEnabled = true;
				connectionMS.ResetStatistics();
			}
#endif
			else
			{
				throw new InvalidOperationException($"Unable to determine statistics for connection type {Connection.GetType().FullName}.");
			}

			base.SetUp();
		}
#pragma warning restore IDE0001 // Full name needed to identify SqlClient types

		protected override void TearDown()
		{
			disposable.Dispose();
			base.TearDown();
		}
		IDisposable disposable;

		System.Data.Common.DbConnection Connection;
	}

	abstract class AccountingTransactionExporterBaseTest : TestCaseWithFactory
	{
		#region Implementation

		void SetupCommonData()
		{
			TestObjectCreator.AUDBankAccount.GLHeader.AG_AccountNum = "AUDAcc";
			TestObjectCreator.USDBankAccount.GLHeader.AG_AccountNum = "USDAcc";
			TestObjectCreator.GLHeader1.AG_AccountNum = "GLHeader1";
			TestObjectCreator.GLHeader2.AG_AccountNum = "GLHeader2";
			SetupOrgGroup();
		}

		void SetupOrgGroup()
		{
			var debtorGroup = TestObjectCreator.CreateDebtorGroup();
			debtorGroup.OJ_Code = "DTG";
			debtorGroup.OJ_Desc = "Debtor Group Company";
			var creditorGroup = TestObjectCreator.CreateCreditorGroup();
			creditorGroup.OG_Code = "CDG";
			creditorGroup.OG_Desc = "Creditor Group Company";
			TestObjectCreator.ABIGAS.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;
			TestObjectCreator.AALSHI.CompanyData.OB_OG_APCreditorGroup = creditorGroup.PK;
		}

		void SetupPeriods()
		{
			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
			helper.PostPeriodsForEntireYear(2008, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2009, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2010, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2011, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2012, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2013, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2014, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2015, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2016, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2017, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2018, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2019, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
		}

		void SetupAccountingWebServiceUsernamePassword()
		{
			AccountingConfigurationRegistry.Instance.AccountingWebServiceUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "paul");
			AccountingConfigurationRegistry.Instance.AccountingWebServicePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "password");
		}

		void SetupExternalDebtorCreditorCode()
		{
			TestObjectCreator.ABIGAS.CompanyData.OB_ARExternalDebtorCode = "ABIGAS_EX";
			TestObjectCreator.ABIGAS.ConfigOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ExternalCreditorAccountCode, "ABIGAS_EX1234", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			TestObjectCreator.AALSHI.ConfigOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ExternalCreditorAccountCode, "AALSHI_EX1234", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			Factory.Save();
		}

		void SetupDebtorARAccountDetails()
		{
			var accountDetails1 = TestObjectCreator.ABIGAS.CompanyData.ARAccountDetailsCollection.AddNew();
			accountDetails1.A1_IsDefaultAccount = true;
			accountDetails1.A1_RX_NKAccountCurrency = "AU";
			accountDetails1.A1_PaymentMethod = "TAX";
			accountDetails1.A1_AccountName = "Account Name A";
			accountDetails1.A1_BankName = "Bank Name A";
			accountDetails1.A1_BankSwift = "1234567";
			accountDetails1.A1_BankBsb = "060369";
			accountDetails1.A1_BankAccount = "1357924680";

			var accountDetails2 = TestObjectCreator.ABIGAS.CompanyData.ARAccountDetailsCollection.AddNew();
			accountDetails2.A1_IsDefaultAccount = false;
			accountDetails2.A1_RX_NKAccountCurrency = "AU";
			accountDetails2.A1_PaymentMethod = "TAX";
			accountDetails2.A1_AccountName = "Account Name B";
			accountDetails2.A1_BankName = "Bank Name B";
			accountDetails2.A1_BankSwift = "7654321";
			accountDetails2.A1_BankBsb = "045897";
			accountDetails2.A1_BankAccount = "987654321";

			Factory.Save();
		}

		void SetupExporterExemptionDocumentTracking()
		{
			var documentTracking = Factory.NewWithValidTestData<JobRequiredDocument>();
			documentTracking.EQ_DocPeriod = Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			documentTracking.EQ_DateReceived = new ZDateTimeOffset(2009, 06, 25);
			documentTracking.EQ_ValidToDate = documentTracking.EQ_DateReceived.ToZDateTime().AddMonths(12);
			documentTracking.EQ_DocNumber = "123456";
			documentTracking.EQ_RN_NKRelatedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			documentTracking.EQ_DocCategory = Constants.ReferenceTypes.ClientSupplierRelationship;
			documentTracking.EQ_RcvFromCustomsBroker = ZDateTime.Today.AddDays(-1);
			documentTracking.EQ_ReturnToShipper = ZDateTime.Today.AddDays(-2);
			documentTracking.EQ_SntToCustomsBroker = ZDateTime.Today.AddDays(-3);
			documentTracking.EQ_OriginalDocRequired = true;
			documentTracking.EQ_DocType = Constants.RefDocTypes.VATExporterExemption;
			documentTracking.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;

			TestObjectCreator.ABIGAS.RequiredDocuments.Add(documentTracking);

			JobRequiredDocAttrib attrib = Factory.New<JobRequiredDocAttrib>();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CostaRicaEXVDocumentType;
			attrib.D0_AttribValue = "Compras Autorizadas";
			attrib.D0_EQ = documentTracking.PK;

			attrib = Factory.New<JobRequiredDocAttrib>();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.IssuingAuthorityName;
			attrib.D0_AttribValue = "ASIC";
			attrib.D0_EQ = documentTracking.PK;

			attrib = Factory.New<JobRequiredDocAttrib>();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
			attrib.D0_AttribValue = "IMP";
			attrib.D0_EQ = documentTracking.PK;

			Factory.Save();
		}

		void SetupTaxMessages()
		{
			TaxMsg1.A9_IsShownOnDocuments = false;
			TaxMsg1.A9_IsTriggerExemptionMessage = false;

			TaxMsg2.A9_IsShownOnDocuments = false;
			TaxMsg2.A9_IsTriggerExemptionMessage = true;

			TaxMsg3.A9_IsShownOnDocuments = true;
			TaxMsg3.A9_IsTriggerExemptionMessage = false;

			TaxMsg4.A9_IsShownOnDocuments = true;
			TaxMsg4.A9_IsTriggerExemptionMessage = true;
		}

		void SetupTaxGroupCode()
		{
			var taxGroupCode = "N1";
			var testValue = new CodeDescriptionBoolRelatedItemCollection();
			testValue.Add(taxGroupCode, (NoResString)"Description N1", true, "N1.1");
			AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, testValue);

			TaxMsg2.A9_TaxGroupCode = taxGroupCode;
		}

		void SetupWithholdingTax()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_IsWHTRegistered = true;

			TestObjectCreator.ABIGAS.CompanyData.OB_ARWHTApplicable = true;

			Assert("Precondition: AALSHI AP WHT applicable", TestObjectCreator.AALSHI.CompanyData.OB_APWHTApplicable);
			Assert("Precondition: AALSHI AR WHT non-applicable", !TestObjectCreator.AALSHI.CompanyData.OB_ARWHTApplicable);

			TestObjectCreator.CC1.AC_AW_WithholdingTaxRate = TestObjectCreator.WHT1.PK;
			TestObjectCreator.RevenueChargeCode.AC_AW_WithholdingTaxRate = TestObjectCreator.WHTFREE1.PK;

			AssertEquals("Precondition", TestObjectCreator.WHT1.PK, TestObjectCreator.CC3.AC_AW_WithholdingTaxRate);
			AssertEquals("Precondition", TestObjectCreator.WHT1.PK, TestObjectCreator.CC10.AC_AW_WithholdingTaxRate);
			AssertEquals("Precondition", TestObjectCreator.WHTFREE1.PK, TestObjectCreator.RevenueNoTaxChargeCode.AC_AW_WithholdingTaxRate);
		}

		void SetupControlAccounts()
		{
			AccGLHeader aRSuspenseControlAccount = TestObjectCreator.CreateARSuspenseControlAccount();
			AccGLHeader aPSuspenseControlAccount = TestObjectCreator.CreateAPSuspenseControlAccount();
			AccGLHeader jobRevenueJournalControlAccount = TestObjectCreator.CreateJobRevenueJournalControlAccount();
			AccGLHeader cFXAccount = TestObjectCreator.CreateCFXAccount();
			AccGLHeader pendingInputTaxAccount = null;
			AccGLHeader pendingOutputTaxAccount = null;
			if (GlbCompany.CurrentCompany.GC_IsGSTCashBasis)
			{
				pendingInputTaxAccount = TestObjectCreator.CreateInputTaxReceivablePendingAccount();
				pendingOutputTaxAccount = TestObjectCreator.CreateOutputTaxPayablePendingAccount();
			}
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aRSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobRevenueJournalControlAccount.PK.ToGuid());
			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, department.PK.ToGuid(), cFXAccount.PK.ToGuid());
			if (pendingInputTaxAccount != null)
			{
				AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingInputTaxAccount.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingOutputTaxAccount.PK.ToGuid());
			}
		}

		void SetupInvoiceRemittanceConfiguration()
		{
			var collection = InvoiceRemittanceConfigurationCollectionTest.GetConfigurationCollectionForTest(Factory);
			AccountingMasterFilesRegistry.Instance.InvoiceRemittanceConfiguration.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection);
			Factory.Save();
		}

		void SetupSubAccounts(DependentTransactionLine line)
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line.PK, OrgHeaderSchema.Constants.Prefix, TestObjectCreator.ABIGAS.PK);
			testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line.PK, AccGroupsSchema.Constants.Prefix, TestObjectCreator.AR1.PK);
			testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line.PK, GlbStaffSchema.Constants.Prefix, TestObjectCreator.GS1.PK);
			testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line.PK, GlbGroupSchema.Constants.Prefix, TestObjectCreator.GG1.PK);
		}

		void SetupTaxConfigurations()
		{
			apControlAccount = TestObjectCreator.CreateAPControlAccount();
			taxExpenseAccount = TestObjectCreator.CreateTaxExpenseLinkAccount();
			taxPrepaidPendingControlAccount = TestObjectCreator.CreatePendingPrepaidTaxControlAccount();
			taxRealisedControlAccount = TestObjectCreator.CreatePrepaidAssetTaxControlAccount();
			remittanceLibilityControlAccount = TestObjectCreator.CreateRemittanceLibilityTaxControlAccount();

			taxSystemPER = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUPER", taxSuperType: TaxSuperTypeList.Perceptions.Code, name: "PERCEPTION - COMPANY LVL - STATE AUTH - OFT POS INLCUDE IN INV");
			taxSystemSLX = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSLX", taxSuperType: TaxSuperTypeList.SalesTax.Code, name: "SALES TAX - COMPANY LEVEL - NAT AUTH - TGR POS INCLUDE IN INV");
			taxSystemSPR = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code, name: "PBW - COMPANY LVL - NAT AUTH - OFT NEG");

			var taxAuthority1 = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU", "AU NATIONAL - ATO");
			var taxAuthority2 = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N02AU", "AU NATIONAL - ATO UNIT 2");

			taxConfigPER = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfigPER.ETC_Code = "AU-N01AU-AUPER-AP";
			taxConfigPER.ETC_Description = "payables percepton (MDT realisation)";
			taxConfigPER.ETC_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxConfigPER.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;
			taxConfigPER.ETC_TaxSystemCode = taxSystemPER.Code;
			taxConfigPER.ETC_TaxAuthorityCode = taxAuthority1.Code;
			taxConfigPER.ETC_AG_LedgerControlAccount = apControlAccount.PK;
			taxConfigPER.ETC_AG_TaxPendingControlAccount = taxPrepaidPendingControlAccount.PK;
			taxConfigPER.ETC_AG_TaxControlAccount = taxRealisedControlAccount.PK;

			taxConfigSLX = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfigSLX.ETC_Code = "AU-N02AU-AUSLX-AP";
			taxConfigSLX.ETC_Description = "payables sales tax expensed (PDT realisation)";
			taxConfigSLX.ETC_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxConfigSLX.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;
			taxConfigSLX.ETC_TaxSystemCode = taxSystemSLX.Code;
			taxConfigSLX.ETC_TaxAuthorityCode = taxAuthority2.Code;
			taxConfigSLX.ETC_AG_LedgerControlAccount = apControlAccount.PK;
			taxConfigSLX.ETC_AG_TaxExpenseAccount = taxExpenseAccount.PK;

			taxConfigSPR = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfigSPR.ETC_Code = "AU-N01AU-AUSPR-AP";
			taxConfigSPR.ETC_Description = "PBW payables SPR (PTM realisation)";
			taxConfigSPR.ETC_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxConfigSPR.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDateOfMatchTransaction.Code;
			taxConfigSPR.ETC_TaxSystemCode = taxSystemSPR.Code;
			taxConfigSPR.ETC_TaxAuthorityCode = taxAuthority1.Code;
			taxConfigSPR.ETC_AG_LedgerControlAccount = apControlAccount.PK;
			taxConfigSPR.ETC_AG_TaxExpenseAccount = remittanceLibilityControlAccount.PK;

			Factory.Save();

			var taxAuthorityCodeDesc_1 = new CodeDescriptionPair("N01AU", "AU NATIONAL - ATO");
			var taxAuthorityCodeDesc_2 = new CodeDescriptionPair("N02AU", "AU NATIONAL - ATO UNIT 2");
			var taxSystemCodeDesc_1 = new CodeDescriptionPair("AUPER", "PERCEPTION - COMPANY LVL - STATE AUTH - OFT POS INLCUDE IN INV");
			var taxSystemCodeDesc_2 = new CodeDescriptionPair("AUSLX", "SALES TAX - COMPANY LEVEL - NAT AUTH - TGR POS INCLUDE IN INV");
			var taxSystemCodeDesc_3 = new CodeDescriptionPair("AUSPR", "PBW - COMPANY LVL - NAT AUTH - OFT NEG");

			TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper()
							.WithGetTaxAuthorities(Constants.CountryCodes.Australia, null, new CodeDescriptionPairList { taxAuthorityCodeDesc_1, taxAuthorityCodeDesc_2 })
							.WithGetTaxSystems(Constants.CountryCodes.Australia, ZString.Empty, new CodeDescriptionPairList { taxSystemCodeDesc_1, taxSystemCodeDesc_2, taxSystemCodeDesc_3 });
		}

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator => taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory));

		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;
		AccGLHeader apControlAccount, taxExpenseAccount, taxPrepaidPendingControlAccount, taxRealisedControlAccount, remittanceLibilityControlAccount;
		AccTaxConfiguration taxConfigPER, taxConfigSLX, taxConfigSPR;
		TaxSystemsConfiguration taxSystemPER, taxSystemSLX, taxSystemSPR;

		void TakeUpLedgers()
		{
			TakeUpLedgers(GlbCompany.CurrentCompany.PK);
		}

		void TakeUpLedgers(ZGuid companyPK)
		{
			string sql = "DELETE FROM dbo.AccChargeCode WHERE AC_Code IN ('BOND', 'CLAIM')";
			TestConnection.ExecuteNonQuery(sql);

			sql = "EXEC TakeUpSubledgers @Company, 'TST'";
			using (DbCommand command = TestConnection.Command(sql))
			{
				command.AddParameter("@Company", System.Data.SqlDbType.UniqueIdentifier, companyPK.ToGuid());
				command.ExecuteNonQuery();
			}
		}

		void AddAmount(Dictionary<string, decimal> debitsCredits, string postingPeriod, string account, decimal amount)
		{
			decimal currentValue;
			string key = postingPeriod + "|" + account;
			if (debitsCredits.TryGetValue(key, out currentValue))
			{
				currentValue += amount;
				debitsCredits[key] = currentValue;
			}
			else
			{
				debitsCredits.Add(key, amount);
			}
		}

		TestObjectCreator TestObjectCreator { get; set; }

		System.Data.Common.DbConnection Connection { get; set; }
		System.Data.Common.DbTransaction Transaction { get; set; }

		protected override void SetUp()
		{
			base.SetUp();

			Connection = ((IDbConnectionInternals)TestConnection).ADOConnection;
			Transaction = ((IDbConnectionInternals)TestConnection).ADOTransaction;

			TestObjectCreator = new TestObjectCreator(Factory);

			SetupPeriods();
			SetupAccountingWebServiceUsernamePassword();

			SetupExternalDebtorCreditorCode();
			SetupDebtorARAccountDetails();
			SetupExporterExemptionDocumentTracking();
			SetupTaxMessages();
		}

		protected override void TearDown()
		{
			Connection.Close();
			base.TearDown();
		}

		protected virtual TimeSpan ExpectedHighWaterMarkBuffer
		{
			get { return new TimeSpan(48, 0, 0); }
		}

		GlbBranch NonCurrentCompanyBranch
		{
			get
			{
				if (nonCurrentCompanyBranch == null)
				{
					var query = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.GetDemoCompany(Factory.GetCachedReadOnlyFactory()).PK);
					query.AddToFilter(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
					nonCurrentCompanyBranch = Factory.LoadTop1<GlbBranch>(query);
				}

				return nonCurrentCompanyBranch;
			}
		}
		GlbBranch nonCurrentCompanyBranch;

		#endregion

		#region Transactions

		AccBankAccount BankAccountAUD
		{
			get
			{
				return TestObjectCreator.AUDBankAccount;
			}
		}

		AccInvMsg TaxMsg1 => TestObjectCreator.TaxMsg1;
		AccInvMsg TaxMsg2 => TestObjectCreator.TaxMsg2;
		AccInvMsg TaxMsg3 => TestObjectCreator.TaxMsg3;
		AccInvMsg TaxMsg4 => TestObjectCreator.TaxMsg4;

		#region Payments

		Dictionary<string, decimal> CreatePayments()
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value
			CreateARPaymentPositive();
			CreateARPaymentNegative();
			CreateAPPaymentPositive();
			CreateAPPaymentNegative();
			return debitsCredits;
		}

		void CreateARPaymentPositive()
		{
			ZDateTime date = new ZDateTime(2008, 06, 15);
			TestObjectCreator.CreateARPayment(1.0m, 100m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				TestObjectCreator.CreateARPayment(1.0m, 100m, date, date, TestObjectCreator.ABIGAS.PK, TestObjectCreator.AUDBankAccount2.PK);
				Factory.Save();
			}
		}

		void CreateARPaymentNegative()
		{
			ZDateTime date = new ZDateTime(2009, 06, 15);
			TestObjectCreator.CreateARPayment(1.0m, -10m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);

			Factory.Save();
		}

		void CreateAPPaymentPositive()
		{
			ZDateTime date = new ZDateTime(2010, 06, 15);
			TestObjectCreator.CreateAPPayment(1.0m, 10000m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);

			Factory.Save();
		}

		void CreateAPPaymentNegative()
		{
			ZDateTime date = new ZDateTime(2011, 06, 15);
			TestObjectCreator.CreateAPPayment(1.0m, -1000m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);

			Factory.Save();
		}

		#endregion

		#region Receipts

		Dictionary<string, decimal> CreateReceipts()
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value
			ZDateTime date = new ZDateTime(2008, 06, 15);
			TestObjectCreator.CreateARReceipt(1.0m, 100m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);

			Factory.Save();

			date = new ZDateTime(2009, 06, 15);
			TestObjectCreator.CreateARReceipt(1.0m, -10m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);

			Factory.Save();

			date = new ZDateTime(2010, 06, 15);
			TestObjectCreator.CreateAPReceipt(1.0m, 10000m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);

			Factory.Save();

			date = new ZDateTime(2011, 06, 15);
			TestObjectCreator.CreateAPReceipt(1.0m, -1000m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				TestObjectCreator.CreateARReceipt(1.0m, 100m, date, date, TestObjectCreator.ABIGAS.PK, TestObjectCreator.AUDBankAccount2.PK);

				Factory.Save();
			}

			return debitsCredits;
		}

		#endregion

		#region Discounts

		Dictionary<string, decimal> CreateDiscounts()
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value

			createAPDiscountPositive();

			createARDiscountNegative();

			createAPDiscountNegative();

			createARDiscountPositive();

			return debitsCredits;
		}

		void createAPDiscountPositive()
		{
			ZDateTime date = new ZDateTime(2008, 06, 15);
			APDiscount discount = TestObjectCreator.CreateAPDiscount(10m, date, TestObjectCreator.ABIGAS.PK);
			APPayment payment = TestObjectCreator.CreateAPPayment(1.0m, 100m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);
			payment.AH_OutstandingAmount = 0m;
			payment.AH_FullyPaidDate = date;
			APInvoice invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m);
			invoice.AH_PostDate = date;
			invoice.Lines[0].AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			invoice.Lines[0].AL_LocalTaxAmount = 10m;
			invoice.Lines[0].AL_GSTVAT = -10m;
			invoice.AH_GSTAmount = -10m;
			invoice.AH_OutstandingAmount = 0m;
			invoice.AH_FullyPaidDate = date;

			CreateTransactionMatchlinks(discount, invoice, payment);

			Factory.Save();
		}

		void createAPDiscountNegative()
		{
			ZDateTime date = new ZDateTime(2009, 06, 15);
			APDiscount discount = TestObjectCreator.CreateAPDiscount(-10m, date, TestObjectCreator.ABIGAS.PK);
			APPayment payment = TestObjectCreator.CreateAPPayment(1.0m, -100m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);
			payment.AH_OutstandingAmount = 0m;
			payment.AH_FullyPaidDate = date;
			APCreditNote invoice = TestObjectCreator.CreateAPCreditNote("003", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1.0m, "Desc");
			invoice.AH_PostDate = date;
			invoice.Lines.AddNew();
			invoice.Lines[0].AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			invoice.Lines[0].AL_LocalExTaxAmount = 100m;
			invoice.Lines[0].AL_LocalTaxAmount = 10m;
			invoice.AH_OutstandingAmount = 0m;
			invoice.AH_FullyPaidDate = date;

			CreateTransactionMatchlinks(discount, invoice, payment);

			Factory.Save();
		}

		void createARDiscountPositive()
		{
			ZDateTime date = new ZDateTime(2010, 06, 15);
			ARDiscount discount = TestObjectCreator.CreateARDiscount(10m, date, TestObjectCreator.ABIGAS.PK);
			ARReceipt receipt = TestObjectCreator.CreateARReceipt(1.0m, -100m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);
			receipt.AH_OutstandingAmount = 0m;
			receipt.AH_FullyPaidDate = date;
			ARCreditNote invoice = TestObjectCreator.CreateARCreditNote("004", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1.0m, "Desc");
			invoice.AH_PostDate = date;
			invoice.Lines.AddNew();
			invoice.Lines[0].AL_AC = TestObjectCreator.RevenueChargeCode.PK;
			invoice.Lines[0].AL_LocalExTaxAmount = 100m;
			invoice.Lines[0].AL_LocalTaxAmount = 10m;
			invoice.AH_OutstandingAmount = 0m;
			invoice.AH_FullyPaidDate = date;

			CreateTransactionMatchlinks(discount, invoice, receipt);

			Factory.Save();
		}

		void createARDiscountNegative()
		{
			ZDateTime date = new ZDateTime(2011, 06, 15);
			ARDiscount discount = TestObjectCreator.CreateARDiscount(-10m, date, TestObjectCreator.ABIGAS.PK);
			ARReceipt receipt = TestObjectCreator.CreateARReceipt(1.0m, 100m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);
			receipt.AH_OutstandingAmount = 0m;
			receipt.AH_FullyPaidDate = date;
			ARInvoice invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
			invoice.AH_PostDate = date;
			invoice.Lines.AddNew();
			invoice.Lines[0].AL_AC = TestObjectCreator.RevenueChargeCode.PK;
			invoice.Lines[0].AL_LocalExTaxAmount = 100m;
			invoice.Lines[0].AL_LocalTaxAmount = 10m;
			invoice.AH_OutstandingAmount = 0m;
			invoice.AH_FullyPaidDate = date;

			CreateTransactionMatchlinks(discount, invoice, receipt);

			Factory.Save();
		}

		#endregion

		#region Overpayments

		Dictionary<string, decimal> CreateOverpayments()
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value

			createAROverpaymentPositive();

			CreateAPOverpaymentPositive();

			CreateAROverpaymentNegative();

			CreateAPOverpaymentNegative();

			return debitsCredits;
		}

		void createAROverpaymentPositive()
		{
			ZDateTime date = new ZDateTime(2008, 06, 15);
			AROverpayment overpayment = TestObjectCreator.CreateOverpayment<AROverpayment>(150m, date, TestObjectCreator.ABIGAS.PK);
			ARReceipt receipt = TestObjectCreator.CreateARReceipt(1.0m, 535m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);
			receipt.AH_OutstandingAmount = 0m;
			receipt.AH_FullyPaidDate = date;
			ARInvoice invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
			invoice.AH_PostDate = date;
			invoice.Lines.AddNew();
			invoice.Lines[0].AL_AC = TestObjectCreator.RevenueChargeCode.PK;
			invoice.Lines[0].AL_LocalExTaxAmount = 350m;
			invoice.Lines[0].AL_LocalTaxAmount = 35m;
			invoice.AH_OutstandingAmount = 0m;
			invoice.AH_FullyPaidDate = date;

			CreateTransactionMatchlinks(overpayment, invoice, receipt);

			Factory.Save();
		}

		void CreateTransactionMatchlinks(TransactionHeader miscTransaction, InvoicingBase invoice, ReceiptPaymentBase receipt)
		{
			TransactionMatchLinkGroup matchLinkGroup = new TransactionMatchLinkGroup(Factory);
			TransactionMatchLink matchLink = matchLinkGroup.AddNew();
			matchLink.AP_AH = miscTransaction.PK;
			matchLink.AP_Amount = miscTransaction.AH_InvoiceAmount;
			matchLink = matchLinkGroup.AddNew();
			matchLink.AP_AH = receipt.PK;
			matchLink.AP_Amount = receipt.AH_InvoiceAmount;
			matchLink = matchLinkGroup.AddNew();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = invoice.AH_InvoiceAmount + invoice.AH_GSTAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLinkGroup);
		}

		void CreateAPOverpaymentPositive()
		{
			ZDateTime date = new ZDateTime(2009, 06, 15);
			APOverpayment overpayment = TestObjectCreator.CreateOverpayment<APOverpayment>(20m, date, TestObjectCreator.ABIGAS.PK);
			APReceipt receipt = TestObjectCreator.CreateAPReceipt(1.0m, 130m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);
			receipt.AH_OutstandingAmount = 0m;
			receipt.AH_FullyPaidDate = date;
			APCreditNote invoice = TestObjectCreator.CreateAPCreditNote("002", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1.0m, "Desc");
			invoice.AH_PostDate = date;
			invoice.Lines.AddNew();
			invoice.Lines[0].AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			invoice.Lines[0].AL_LocalExTaxAmount = 100m;
			invoice.Lines[0].AL_LocalTaxAmount = 10m;
			invoice.AH_OutstandingAmount = 0m;
			invoice.AH_FullyPaidDate = date;

			CreateTransactionMatchlinks(overpayment, invoice, receipt);

			Factory.Save();
		}

		void CreateAROverpaymentNegative()
		{
			ZDateTime date = new ZDateTime(2010, 06, 15);
			AROverpayment overpayment = TestObjectCreator.CreateOverpayment<AROverpayment>(-150m, date, TestObjectCreator.ABIGAS.PK);
			ARPayment receipt = TestObjectCreator.CreateARPayment(1.0m, 535m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);
			receipt.AH_OutstandingAmount = 0m;
			receipt.AH_FullyPaidDate = date;
			ARCreditNote invoice = TestObjectCreator.CreateARCreditNote("003", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1.0m, "Desc");
			invoice.AH_PostDate = date;
			invoice.Lines.AddNew();
			invoice.Lines[0].AL_AC = TestObjectCreator.RevenueChargeCode.PK;
			invoice.Lines[0].AL_LocalExTaxAmount = 350m;
			invoice.Lines[0].AL_LocalTaxAmount = 35m;
			invoice.AH_OutstandingAmount = 0m;
			invoice.AH_FullyPaidDate = date;

			CreateTransactionMatchlinks(overpayment, invoice, receipt);

			Factory.Save();
		}

		void CreateAPOverpaymentNegative()
		{
			ZDateTime date = new ZDateTime(2011, 06, 15);
			APOverpayment overpayment = TestObjectCreator.CreateOverpayment<APOverpayment>(-20m, date, TestObjectCreator.ABIGAS.PK);
			APPayment payment = TestObjectCreator.CreateAPPayment(1.0m, 130m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);
			payment.AH_OutstandingAmount = 0m;
			payment.AH_FullyPaidDate = date;
			APInvoice invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("004", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m);
			invoice.AH_PostDate = date;
			invoice.Lines[0].AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			invoice.Lines[0].AL_LocalExTaxAmount = 100m;
			invoice.Lines[0].AL_LocalTaxAmount = 10m;
			invoice.AH_OutstandingAmount = 0m;
			invoice.AH_FullyPaidDate = date;

			CreateTransactionMatchlinks(overpayment, invoice, payment);

			Factory.Save();
		}

		#endregion

		#region Journals

		Dictionary<string, decimal> CreateARAPJournals()
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value

			ARJournal arJournalPositive = TestObjectCreator.CreateJournal<ARJournal>(100m, new ZDateTime(2008, 6, 15), TestObjectCreator.AALSHI.PK);
			Factory.Save();

			ARJournal arJournalNegative = TestObjectCreator.CreateJournal<ARJournal>(-50m, new ZDateTime(2009, 6, 15), TestObjectCreator.AALSHI.PK);
			Factory.Save();

			APJournal apJournalPositive = TestObjectCreator.CreateJournal<APJournal>(10000m, new ZDateTime(2010, 6, 15), TestObjectCreator.ABIGAS.PK);
			Factory.Save();

			APJournal apJournalNegative = TestObjectCreator.CreateJournal<APJournal>(-5000m, new ZDateTime(2011, 6, 15), TestObjectCreator.ABIGAS.PK);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				TestObjectCreator.CreateJournal<ARJournal>(100m, new ZDateTime(2008, 6, 15), TestObjectCreator.AALSHI.PK);
				Factory.Save();
			}

			return debitsCredits;
		}

		#endregion

		#region GL Journals

		Dictionary<string, decimal> CreateGLJournals()
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value

			GLJournal glStandardJournal = TestObjectCreator.CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLStandardJournal, new ZDateTime(2008, 2, 28, 23, 59, 0), new ZDateTime(2008, 2, 15));

			var journalLine1 = TestObjectCreator.CreateGLJournalLine(glStandardJournal, 50m, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var journalLine2 = TestObjectCreator.CreateGLJournalLine(glStandardJournal, 300m, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			SetupSubAccounts(journalLine1);
			SetupSubAccounts(journalLine2);
			glStandardJournal.Balance();

			List<ITransactionParticipant> factories = new List<ITransactionParticipant>();
			factories.Add(Factory);
			AggregateWrapper aggregator = new AggregateWrapper(glStandardJournal, glStandardJournal);
			factories.Add(aggregator);
			BusinessObjectFactory.SaveTogether(factories.ToArray());

			GLJournal glAutoJournal = TestObjectCreator.CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLAutoJournal, new ZDateTime(2009, 02, 15), new ZDateTime(2009, 2, 28, 23, 59, 0), new ZDateTime(2009, 6, 30, 23, 59, 0));
			journalLine1 = TestObjectCreator.CreateGLJournalLine(glAutoJournal, 200m, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			journalLine2 = TestObjectCreator.CreateGLJournalLine(glAutoJournal, 1000m, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			SetupSubAccounts(journalLine1);
			SetupSubAccounts(journalLine2);
			glAutoJournal.Balance();

			factories = new List<ITransactionParticipant>();
			factories.Add(Factory);
			aggregator = new AggregateWrapper(glAutoJournal, glAutoJournal);
			factories.Add(aggregator);
			BusinessObjectFactory.SaveTogether(factories.ToArray());

			GLJournal glReversingJournal = TestObjectCreator.CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLReversingJournal, new ZDateTime(2010, 02, 15), new ZDateTime(2010, 2, 28, 23, 59, 0), new ZDateTime(2010, 6, 1));
			journalLine1 = TestObjectCreator.CreateGLJournalLine(glReversingJournal, 10000m, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			journalLine2 = TestObjectCreator.CreateGLJournalLine(glReversingJournal, 3000m, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			SetupSubAccounts(journalLine1);
			SetupSubAccounts(journalLine2);
			glReversingJournal.Balance();

			factories = new List<ITransactionParticipant>();
			factories.Add(Factory);
			aggregator = new AggregateWrapper(glReversingJournal, glReversingJournal);
			factories.Add(aggregator);
			BusinessObjectFactory.SaveTogether(factories.ToArray());

			return debitsCredits;
		}

		#endregion

		#region ARAP Transfers

		Dictionary<string, decimal> CreateARAPTransfers()
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value

			ARTransfer arTransferPositive = TestObjectCreator.CreateTransfer<ARTransfer>(100m, new ZDateTime(2008, 6, 15), TestObjectCreator.ABIGAS.PK, TestObjectCreator.AALSHI.PK);
			Factory.Save();

			ARTransfer arTransferNegative = TestObjectCreator.CreateTransfer<ARTransfer>(-10m, new ZDateTime(2009, 6, 15), TestObjectCreator.ABIGAS.PK, TestObjectCreator.AALSHI.PK);
			Factory.Save();

			APTransfer apTransferPositive = TestObjectCreator.CreateTransfer<APTransfer>(10000m, new ZDateTime(2010, 6, 15), TestObjectCreator.ABIGAS.PK, TestObjectCreator.AALSHI.PK);
			Factory.Save();

			APTransfer apTransferNegative = TestObjectCreator.CreateTransfer<APTransfer>(-1000m, new ZDateTime(2011, 6, 15), TestObjectCreator.ABIGAS.PK, TestObjectCreator.AALSHI.PK);
			Factory.Save();

			return debitsCredits;
		}

		#endregion

		#region Exchange Differences

		Dictionary<string, decimal> CreateARAPExchangeDifferences()
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value

			//create AR Exchange Difference Positive
			createARExchangeDifference("005", new ZDateTime(2008, 6, 15), 20m, 100m, 10m, 130m);

			//create AP Exchange Difference Positive
			createAPExchangeDifference("006", new ZDateTime(2009, 6, 15), 30m, 100m, 10m, 80m);

			//create AR Exchange Difference Negative
			createARExchangeDifference("007", new ZDateTime(2010, 6, 15), -30m, 100m, 10m, 80m);

			//create AP Exchange Difference Negative
			createAPExchangeDifference("008", new ZDateTime(2011, 6, 15), -20m, 100m, 10m, 130m);

			return debitsCredits;
		}

		void createARExchangeDifference(string transactionNumber, ZDateTime postDate, decimal exchangeDifferenceAmount, decimal invoiceAmount, decimal taxAmount, decimal receiptAmount)
		{
			ARExchangeDifference exchangeDifference = TestObjectCreator.CreateExchangeDifference<ARExchangeDifference>(-exchangeDifferenceAmount, postDate, TestObjectCreator.ABIGAS.PK);
			ARReceipt receipt = TestObjectCreator.CreateARReceipt(1.0m, receiptAmount, postDate, postDate, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);
			receipt.AH_OutstandingAmount = 0m;
			receipt.AH_FullyPaidDate = postDate;
			ARInvoice invoice = TestObjectCreator.CreateARInvoice<ARInvoice>(transactionNumber, TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
			invoice.AH_PostDate = postDate;
			invoice.Lines.AddNew();
			invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice.Lines[0].AL_LocalExTaxAmount = invoiceAmount;
			invoice.Lines[0].AL_LocalTaxAmount = taxAmount;
			invoice.AH_OutstandingAmount = 0m;
			invoice.AH_FullyPaidDate = postDate;

			CreateTransactionMatchlinks(exchangeDifference, invoice, receipt);

			Factory.Save();
		}

		void createAPExchangeDifference(string transactionNumber, ZDateTime postDate, decimal exchangeDifferenceAmount, decimal invoiceAmount, decimal taxAmount, decimal paymentAmount)
		{
			APExchangeDifference exchangeDifference = TestObjectCreator.CreateExchangeDifference<APExchangeDifference>(-exchangeDifferenceAmount, postDate, TestObjectCreator.ABIGAS.PK);
			APPayment payment = TestObjectCreator.CreateAPPayment(1.0m, paymentAmount, postDate, postDate, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);
			payment.AH_OutstandingAmount = 0m;
			payment.AH_FullyPaidDate = postDate;
			APInvoice invoice = TestObjectCreator.CreateAPInvoice<APInvoice>(transactionNumber, TestObjectCreator.AUD, 1.0m, invoiceAmount, taxAmount, 0m, invoiceAmount, taxAmount, 0m);
			invoice.AH_PostDate = postDate;
			invoice.Lines[0].AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			invoice.Lines[0].AL_LocalTaxAmount = taxAmount;
			invoice.Lines[0].AL_GSTVAT = -taxAmount;
			invoice.AH_GSTAmount = -taxAmount;
			invoice.AH_OutstandingAmount = 0m;
			invoice.AH_FullyPaidDate = postDate;

			CreateTransactionMatchlinks(exchangeDifference, invoice, payment);

			Factory.Save();
		}

		#endregion

		#region Contras

		Dictionary<string, decimal> CreateContras()
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value

			Contra contraPositive = TestObjectCreator.CreateContra(1000m, new ZDateTime(2008, 6, 15), TestObjectCreator.ABIGAS.PK, TestObjectCreator.AALSHI.PK);
			Factory.Save();

			Contra contraNegative = TestObjectCreator.CreateContra(-500m, new ZDateTime(2009, 6, 15), TestObjectCreator.ABIGAS.PK, TestObjectCreator.AALSHI.PK);
			Factory.Save();

			return debitsCredits;
		}

		#endregion

		#region Invoices

		Dictionary<string, decimal> CreateInvoices(bool doMatching = false, bool doOnlyMatching = false, bool setAPRecoverableTax = false, bool setAPDocumentReceivedDate = false)
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value

			if (doOnlyMatching)
			{
				doMatching = true;
			}

			if (setAPDocumentReceivedDate)
			{
				AccountingMasterFilesRegistry.Instance.DocumentReceivedDateDefaultingLogic.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.DocReceivedDateDefaultLogics.Code.InvoiceDate);
			}

			if (!doOnlyMatching)
			{
				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));
				apInvoicePositive = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
				apInvoicePositive.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
				apInvoicePositive.Lines[0].AL_JH = job.PK;
				apInvoicePositive.AH_PostDate = new ZDateTime(2008, 06, 15);
				apInvoicePositive.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
				apInvoicePositive.AH_GovernmentAllocatedID = "29BECD75-B227-4B55-BF95-B70EC6951C87_(Fill to Max)";
				TestObjectCreator.CreateAPInvoiceLine(apInvoicePositive, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", -10m);
				apInvoicePositive.Lines[1].AL_AC = TestObjectCreator.CC1.PK;
				TestObjectCreator.CreateCharge(apInvoicePositive.Lines[0]).JR_OSSellAmt = 0;
				TestObjectCreator.CreateCharge(apInvoicePositive.Lines[1]).JR_OSSellAmt = 0;
				if (setAPRecoverableTax)
				{
					apInvoicePositive.Lines[0].AL_Calc_InputGSTVATRecoverablePercentage = 77.77; //this is to check rounding to currency decimals
					apInvoicePositive.Lines[1].AL_Calc_InputGSTVATRecoverablePercentage = 80;
				}
				TestObjectCreator.CreateJobChargeRevRecognitionForLines(apInvoicePositive.Lines, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, new ZDateTime(2008, 06, 15));
			}
			if (doMatching)
			{
				TestObjectCreator.CreateMatchLinkToPayAPInvoice(apInvoicePositive, ZDateTime.Today.AddDays(-2), -40);
			}
			Factory.Save();

			if (!doOnlyMatching)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					var apInvoiceOtherCompany = TestObjectCreator.CreateAPInvoice<APInvoice>("1-1", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
					apInvoiceOtherCompany.Lines[0].AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
					apInvoiceOtherCompany.AH_PostDate = new ZDateTime(2008, 06, 15);
					Factory.Save();
				}
			}

			if (!doOnlyMatching)
			{
				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002002"));
				apInvoiceNegative = TestObjectCreator.CreateAPInvoice<APInvoice>("112", TestObjectCreator.AUD, 1.0m, -100m, -10m, 0m, -100m, -10m, 0m, TestObjectCreator.AALSHI);
				apInvoiceNegative.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
				apInvoiceNegative.Lines[0].AL_JH = job.PK;
				apInvoiceNegative.AH_PostDate = new ZDateTime(2008, 12, 15);
				apInvoiceNegative.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TCD;
				apInvoiceNegative.AH_GovernmentAllocatedID = "18D4F51D-5665-4B5A-8135-E62C03427A83_(Fill to Max)";
				TestObjectCreator.CreateAPInvoiceLine(apInvoiceNegative, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 10m);
				apInvoiceNegative.Lines[1].AL_AC = TestObjectCreator.CC1.PK;
				TestObjectCreator.CreateCharge(apInvoiceNegative.Lines[0]).JR_OSSellAmt = 0;
				TestObjectCreator.CreateCharge(apInvoiceNegative.Lines[1]).JR_OSSellAmt = 0;
				if (setAPRecoverableTax)
				{
					apInvoiceNegative.Lines[0].AL_Calc_InputGSTVATRecoverablePercentage = 10;
					apInvoiceNegative.Lines[1].AL_Calc_InputGSTVATRecoverablePercentage = 40;
				}
			}
			if (doMatching)
			{
				TestObjectCreator.CreateMatchLinkToPayARInvoice(apInvoiceNegative, ZDateTime.Today.AddDays(-2), 40);
			}
			Factory.Save();

			if (!doOnlyMatching)
			{
				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002003"));
				arInvoicePositive = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				arInvoicePositive.AH_PostDate = new ZDateTime(2009, 06, 15);
				arInvoicePositive.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
				var line = TestObjectCreator.CreateInvoiceLine(arInvoicePositive, TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m);
				line.AL_AC = TestObjectCreator.CC1.PK;
				line.AL_JH = job.PK;
				line.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
				line.AL_A9_VATClass = TaxMsg1.PK;
				var line2 = TestObjectCreator.CreateInvoiceLine(arInvoicePositive, TestObjectCreator.AUD, 1.0m, -50m, -5m, 0m);
				line2.AL_AC = TestObjectCreator.CC1.PK;
				line2.AL_JH = job.PK;
				line2.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
				line2.AL_A9_VATClass = TaxMsg2.PK;
				TestObjectCreator.CreateCharge(arInvoicePositive.Lines[0]).JR_OSCostAmt = 0;
				TestObjectCreator.CreateCharge(arInvoicePositive.Lines[1]).JR_OSCostAmt = 0;
				TestObjectCreator.CreateJobChargeRevRecognitionForLines(arInvoicePositive.Lines, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, new ZDateTime(2009, 06, 15));
			}
			if (doMatching)
			{
				TestObjectCreator.CreateMatchLinkToPayARInvoice(arInvoicePositive, ZDateTime.Today.AddDays(-2), 40);
			}
			Factory.Save();

			if (!doOnlyMatching)
			{
				arInvoiceNegative = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				arInvoiceNegative.AH_PostDate = new ZDateTime(2009, 12, 15);
				arInvoiceNegative.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TCD;
				var line = TestObjectCreator.CreateInvoiceLine(arInvoiceNegative, TestObjectCreator.AUD, 1.0m, -100m, -10m, 0m);
				line.AL_AC = TestObjectCreator.RevenueChargeCode.PK;
				line.AL_AT = TestObjectCreator.RevenueChargeCode.GSTRate.PK;
				line.AL_A9_VATClass = TaxMsg3.PK;
				var line2 = TestObjectCreator.CreateInvoiceLine(arInvoiceNegative, TestObjectCreator.AUD, 1.0m, 50m, 5m, 0m);
				line2.AL_AC = TestObjectCreator.RevenueChargeCode.PK;
				line2.AL_AT = TestObjectCreator.RevenueChargeCode.GSTRate.PK;
				line2.AL_A9_VATClass = TaxMsg4.PK;

				SetupSubAccounts(line);
				SetupSubAccounts(line2);
			}
			if (doMatching)
			{
				TestObjectCreator.CreateMatchLinkToPayAPInvoice(arInvoiceNegative, ZDateTime.Today.AddDays(-2), -40);
			}
			Factory.Save();

			return debitsCredits;
		}

		void CreateAPInvoicesLinkedToConsolCost()
		{
			APInvoice apInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("00000111", TestObjectCreator.AUD, 1m, 999m, 0m, 0m, 999m, 0m, 0m);
			apInvoice1.Lines.RemoveAndDeleteAll();
			var forwardingConsol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001000");
			var consolCost1 = TestObjectCreator.CreateConsolCost(forwardingConsol, TestObjectCreator.CC1, 999m);
			consolCost1.E6_AH_APInvoice = apInvoice1.PK;
			var line1 = TestObjectCreator.CreateInvoiceLine(apInvoice1, TestObjectCreator.AUD, 1.0m, 999m);
			line1.AL_AC = TestObjectCreator.CC1.PK;
			var shipment = TestObjectCreator.CreateShipment("S00001000", forwardingConsol);
			var job1 = TestObjectCreator.CreateJob(shipment, false);
			line1.AL_JH = job1.PK;
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line1.AL_GovtChargeCode = "Govt Charge Code 1";
			var charge1 = TestObjectCreator.CreateCharge(line1);
			charge1.JR_E6 = consolCost1.PK;
			apInvoice1.AH_FullyPaidDate = ZDateTime.Empty;
			apInvoice1.AH_GovernmentAllocatedID = "A55AA02A-D354-45B6-8FD6-9F78758E6AF9_(Fill to Max)";
			Factory.Save();

			var transportBookingConsol = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbBookingConsolidation>());
			transportBookingConsol[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			transportBookingConsol[DtbBookingConsolidationSchema.KB_JobID] = "CB00000001";
			APInvoice apInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("00000222", TestObjectCreator.AUD, 1m, 888m, 0m, 0m, 888m, 0m, 0m);
			apInvoice2.Lines.RemoveAndDeleteAll();
			apInvoice2.AH_GovernmentAllocatedID = "                              ";
			var shipment2 = TestObjectCreator.CreateShipment("S00001001");
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			var line2 = TestObjectCreator.CreateInvoiceLine(apInvoice2, TestObjectCreator.AUD, 1.0m, 888m);
			line2.AL_AC = TestObjectCreator.CC2.PK;
			line2.AL_JH = job2.PK;
			line2.AL_AG = TestObjectCreator.GLHeader2.PK;
			line2.AL_GovtChargeCode = "Govt Charge Code 2";
			var charge2 = TestObjectCreator.CreateCharge(line2);
			var consolCost2 = apInvoice2.ConsolCosting.ConsolCosts.AddNew();
			consolCost2.E6_AC_ChargeCode = TestObjectCreator.CC2.PK;
			consolCost2.SetE6_ParentIDAndE6_ParentTableCodeTogether(transportBookingConsol.PK, DtbBookingConsolidationSchema.Constants.Prefix);
			consolCost2.E6_AH_APInvoice = apInvoice2.PK;
			consolCost2.E6_OSCostAmount = 888m;
			charge2.JR_E6 = consolCost2.PK;
			charge2.JR_APInvoiceNum = consolCost2.E6_InvoiceNum;
			charge2.JR_APInvoiceDate = consolCost2.E6_InvoiceDate;
			charge2.JR_PaymentDate = consolCost2.E6_PaymentDate;
			Factory.Save();

			var runSheet = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbConsignmentRunSheet>());
			runSheet[DtbConsignmentRunSheetSchema.KG_RunSheetNumber] = "CG00000002";
			APInvoice apInvoice3 = TestObjectCreator.CreateAPInvoice<APInvoice>("00000333", TestObjectCreator.AUD, 1m, 777m, 0m, 0m, 777m, 0m, 0m);
			apInvoice3.Lines.RemoveAndDeleteAll();
			apInvoice3.AH_GovernmentAllocatedID = "<XMLTagTest>1234567890123456789012345</XMLTagTest>";
			var shipment3 = TestObjectCreator.CreateShipment("S00001002");
			var job3 = TestObjectCreator.CreateJob(shipment3, false);
			var line3 = TestObjectCreator.CreateInvoiceLine(apInvoice3, TestObjectCreator.AUD, 1.0m, 777m);
			line3.AL_AC = TestObjectCreator.CC3.PK;
			line3.AL_JH = job3.PK;
			line3.AL_AG = TestObjectCreator.GLHeader2.PK;
			line3.AL_GovtChargeCode = "Govt Charge Code 3";
			var charge3 = TestObjectCreator.CreateCharge(line3);
			var consolCost3 = apInvoice3.ConsolCosting.ConsolCosts.AddNew();
			consolCost3.E6_AC_ChargeCode = TestObjectCreator.CC3.PK;
			consolCost3.SetE6_ParentIDAndE6_ParentTableCodeTogether(runSheet.PK, DtbConsignmentRunSheetSchema.Constants.Prefix);
			consolCost3.E6_AH_APInvoice = apInvoice3.PK;
			consolCost3.E6_OSCostAmount = 777m;
			charge3.JR_E6 = consolCost3.PK;
			charge3.JR_APInvoiceNum = consolCost3.E6_InvoiceNum;
			charge3.JR_APInvoiceDate = consolCost3.E6_InvoiceDate;
			charge3.JR_PaymentDate = consolCost3.E6_PaymentDate;
			Factory.Save();
		}

		[TestDate(2009, 6, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvoicesWithConsolCost()
		{
			SetupCommonData();
			CreateAPInvoicesLinkedToConsolCost();
			Factory.Save();

			string xml = Export();

			string xmlFile = BaseSourcePath + InvoiceWithConsolCostExportedFileName;
			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}
		}

		[TestDate(2009, 6, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvoicesWithConsolCost_GovtChargeCode()
		{
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			try
			{
				SetupCommonData();
				CreateAPInvoicesLinkedToConsolCost();
				Factory.Save();

				string xml = Export();

				string xmlFile = BaseSourcePath + InvoiceWithConsolCostGovtChargeCodeExportedFileName;
				using (StreamReader streamReader = File.OpenText(xmlFile))
				{
					string expectedXml = streamReader.ReadToEnd();
					streamReader.Close();

					AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
				}
			}
			finally
			{
				AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			}
		}

		[TestDate(2009, 5, 25)]
		[SuspendCriticalValidation]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestJRJRevenueAndCostExported()
		{
			TestObjectCreator.GLHeader1.AG_AccountNum = "8888.88.88";
			TestObjectCreator.GLHeader1.AG_Description = "Test GL Header";

			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code, departmentCode: "FIP");
			var journal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 100M);
			var costLine = journal.Lines.Cast<JobRevenueJournalLine>().First(x => x.DebitCreditSign == DebitCreditDataEntry.DR);
			costLine.AL_LineType = TransactionLineTypes.Cost;

			Factory.Save();

			var charge = Factory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_AL_ARLine, costLine.PK));
			charge.JR_AL_ARLine = ZGuid.Empty;
			charge.JR_AL_APLine = costLine.PK;

			string xml = Export();

			string xmlFile = BaseSourcePath + JRJRevenueAndCostExportedFileName;
			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}
		}

		[TestDate(2009, 6, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvoicesWithPlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				SetupCommonData();
				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S0001001"));

				apInvoicePositive = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
				apInvoicePositive.AH_PlaceOfSupply = "JH";
				apInvoicePositive.AH_PlaceOfSupplyType = PlaceOfSupplyTypes.State.Code;

				var apLine1 = TestObjectCreator.CreateAPInvoiceLine(apInvoicePositive, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", -10m);
				apLine1.AL_PlaceOfSupply = "JH";
				apLine1.AL_PlaceOfSupplyType = PlaceOfSupplyTypes.State.Code;
				var apLine2 = TestObjectCreator.CreateAPInvoiceLine(apInvoicePositive, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", -10m);
				apLine2.AL_PlaceOfSupply = "JH";
				apLine2.AL_PlaceOfSupplyType = PlaceOfSupplyTypes.State.Code;
				var apCharge1 = TestObjectCreator.CreateCharge(apLine1);
				var apCharge2 = TestObjectCreator.CreateCharge(apLine2);

				arInvoicePositive = TestObjectCreator.CreateARInvoice<ARInvoice>("3", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				arInvoicePositive.AH_PostDate = new ZDateTime(2009, 06, 15);
				arInvoicePositive.AH_InvoiceDate = arInvoicePositive.AH_PostDate.AddDays(1);
				arInvoicePositive.AH_PlaceOfSupply = "DL";
				arInvoicePositive.AH_PlaceOfSupplyType = PlaceOfSupplyTypes.State.Code;

				var arLine = TestObjectCreator.CreateARInvoiceLine(arInvoicePositive, job, TestObjectCreator.RevenueChargeCode, TestObjectCreator.AUD, 1.0m, "desc", 100m);
				arLine.AL_PlaceOfSupply = "DL";
				arLine.AL_PlaceOfSupplyType = PlaceOfSupplyTypes.State.Code;
				var arCharge = TestObjectCreator.CreateCharge(arLine);

				Factory.Save();

				string xml = Export();

				string xmlFile = BaseSourcePath + InvoiceWithPlaceOfSupplyExportedFileName;
				using (StreamReader streamReader = File.OpenText(xmlFile))
				{
					string expectedXml = streamReader.ReadToEnd();
					streamReader.Close();

					AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
				}
			}
		}

		public AccCashAdvanceRequestLine CreatePaidARCashAdvanceLine(AccCashAdvanceRequestHeader cah, Charge charge)
		{
			var cal = TestObjectCreator.CreateCashAdvanceRequestLine(cah, charge.JR_LocalSellAmt, charge.JR_OSSellAmt);
			charge.JR_CAL_ARLine = cal.PK;
			cal.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			cal.CAL_LocalPaidAmount = charge.JR_LocalSellAmt;
			cal.CAL_OSPaidAmount = charge.JR_OSSellAmt;
			return cal;
		}

		[TestDate(2011, 2, 18)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestARInvoiceWithCashAdvance()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S0001001"));
			SetupCommonData();

			var org = TestObjectCreator.ABIGAS;
			org.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1.0m, org);
			arInvoice.AH_PostDate = new ZDateTime(2011, 6, 2);
			var line1 = TestObjectCreator.CreateARInvoiceLine(arInvoice, job, TestObjectCreator.RevenueChargeCode, TestObjectCreator.AUD, 1.0m, "invoice line with cash advance", 100m);
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			var line2 = TestObjectCreator.CreateARInvoiceLine(arInvoice, job, TestObjectCreator.RevenueChargeCode, TestObjectCreator.AUD, 1.0m, "invoice line with cash advance", 100m);
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			var cah = TestObjectCreator.CreateCashAdvanceRequestHeader(job, TestObjectCreator.Debtor, LedgerTypes.AccountsReceivable, 200M, 200M, "AUD");
			var arCharge1 = TestObjectCreator.CreateCharge(line1);
			var cal1 = CreatePaidARCashAdvanceLine(cah, arCharge1);
			var arCharge2 = TestObjectCreator.CreateCharge(line2);
			var cal2 = CreatePaidARCashAdvanceLine(cah, arCharge2);

			Factory.Save();

			string xml = Export();
			string xmlFile = BaseSourcePath + ARInvoiceWithCashAdvanceReceivedFileName;

			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}
		}

		public AccCashAdvanceRequestLine CreatePaidAPCashAdvanceLine(AccCashAdvanceRequestHeader cah, JobCharge charge)
		{
			var cal = TestObjectCreator.CreateCashAdvanceRequestLine(cah, -charge.JR_LocalCostAmt, -charge.JR_OSCostAmt);
			charge.JR_CAL_APLine = cal.PK;
			cal.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			cal.CAL_LocalPaidAmount = -charge.JR_LocalCostAmt;
			cal.CAL_OSPaidAmount = -charge.JR_OSCostAmt;
			return cal;
		}

		[TestDate(2011, 2, 18)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAPInvoiceWithCashAdvance()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S0001001"));
			SetupCommonData();

			var invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1.0m);
			invoice.AH_TransactionNum = "111";
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			InvoicingLineBase line1 = TestObjectCreator.CreateAPInvoiceLine(invoice, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", -100M);
			line1.AL_JH = job.PK;

			var charge1 = TestObjectCreator.CreateJobCharge(line1, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			var cah = TestObjectCreator.CreateCashAdvanceRequestHeader(job, TestObjectCreator.Debtor, LedgerTypes.AccountsPayable, 200M, 200M, charge1.JR_RX_NKCostCurrency);
			var cal1 = CreatePaidAPCashAdvanceLine(cah, charge1);

			InvoicingLineBase line2 = TestObjectCreator.CreateAPInvoiceLine(invoice, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", -100M);
			line2.AL_JH = job.PK;
			var charge2 = TestObjectCreator.CreateJobCharge(line2, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			var cal2 = CreatePaidAPCashAdvanceLine(cah, charge2);

			Factory.Save();

			string xml = Export();
			string xmlFile = BaseSourcePath + APInvoiceWithCashAdvanceReceivedFileName;

			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}
		}

		void CreateInvoicesSettingParentTransaction()
		{
			apInvoicePositive = TestObjectCreator.CreateAPInvoice<APInvoice>("1", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
			apInvoicePositive.Lines[0].AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			apInvoicePositive.AH_PostDate = new ZDateTime(2008, 06, 15);
			apInvoicePositive.AH_InvoiceDate = apInvoicePositive.AH_PostDate.AddDays(1);
			apInvoicePositive.AH_ConsolidatedInvoiceRef = "S1";
			apInvoicePositive.AH_TransactionReference = "ComplianceNumberAP";
			apInvoicePositive.AH_ComplianceSubType = "TXI";

			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("1-1", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
			apInvoice.Lines[0].AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			apInvoice.AH_PostDate = new ZDateTime(2008, 06, 15);
			apInvoice.AH_InvoiceDate = apInvoice.AH_PostDate.AddDays(1);
			apInvoice.AH_TransactionBelongsToGroup = apInvoicePositive.PK;

			apInvoicePositive2 = TestObjectCreator.CreateAPInvoice<APInvoice>("2", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
			apInvoicePositive2.Lines[0].AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			apInvoicePositive2.AH_PostDate = new ZDateTime(2008, 07, 15);
			apInvoicePositive2.AH_InvoiceDate = apInvoicePositive.AH_PostDate.AddDays(1);
			apInvoicePositive2.AH_ConsolidatedInvoiceRef = "S1";

			var apInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("2-1", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
			apInvoice2.Lines[0].AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			apInvoice2.AH_PostDate = new ZDateTime(2008, 07, 15);
			apInvoice2.AH_InvoiceDate = apInvoice2.AH_PostDate.AddDays(1);
			apInvoice2.AH_TransactionBelongsToGroup = apInvoicePositive2.PK;

			arInvoicePositive = TestObjectCreator.CreateARInvoice<ARInvoice>("3", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
			arInvoicePositive.AH_PostDate = new ZDateTime(2009, 06, 15);
			arInvoicePositive.AH_InvoiceDate = arInvoicePositive.AH_PostDate.AddDays(1);
			arInvoicePositive.AH_ConsolidatedInvoiceRef = "S2";
			arInvoicePositive.AH_TransactionReference = "ComplianceNumberAR";
			arInvoicePositive.AH_ComplianceSubType = "TCR";
			var line = TestObjectCreator.CreateInvoiceLine(arInvoicePositive, TestObjectCreator.AUD, 1.0m, 100m, 0m, 0m);
			line.AL_AC = TestObjectCreator.RevenueChargeCode.PK;

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("3-1", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
			arInvoice.AH_PostDate = new ZDateTime(2009, 06, 15);
			arInvoice.AH_InvoiceDate = arInvoice.AH_PostDate.AddDays(1);
			arInvoice.AH_TransactionBelongsToGroup = arInvoicePositive.PK;
			var line2 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1.0m, 100m, 0m, 0m);
			line2.AL_AC = TestObjectCreator.RevenueChargeCode.PK;

			apCreditNote = TestObjectCreator.CreateAPCreditNote("4", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1.0m, "Desc");
			apCreditNote.AH_PostDate = new ZDateTime(2008, 06, 15);
			apCreditNote.AH_InvoiceDate = apInvoice.AH_PostDate.AddDays(1);
			apCreditNote.AH_OriginalTransactionNum = "0123456789";
			apCreditNote.AH_OriginalInvoiceDate = apInvoicePositive.AH_PostDate.AddDays(1).Date;
			apCreditNote.Lines.AddNew();
			apCreditNote.Lines[0].AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			apCreditNote.Lines[0].AL_LocalExTaxAmount = 100m;
			apCreditNote.Lines[0].AL_LocalTaxAmount = 10m;

			arCreditNote = TestObjectCreator.CreateARCreditNote("5", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1.0m, "Desc");
			arCreditNote.AH_PostDate = new ZDateTime(2008, 06, 15);
			arCreditNote.AH_InvoiceDate = arInvoice.AH_PostDate.AddDays(1);
			arCreditNote.AH_OriginalTransactionNum = "9876543210";
			arCreditNote.AH_OriginalInvoiceDate = arInvoicePositive.AH_PostDate.AddDays(1).Date;
			arCreditNote.Lines.AddNew();
			arCreditNote.Lines[0].AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			arCreditNote.Lines[0].AL_LocalExTaxAmount = 100m;
			arCreditNote.Lines[0].AL_LocalTaxAmount = 10m;

			Factory.Save();
		}

		void CreateInvoicesSettingExternalDebtorAndCreditor()
		{
			OrgHeader creditor = TestObjectCreator.AALSHI;
			creditor.CompanyData.OB_APExternalCreditorCode = "CREDITORONE";
			APInvoice apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, creditor);
			apInvoice.Lines[0].AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			apInvoice.AH_PostDate = new ZDateTime(2008, 06, 15);
			Factory.Save();

			OrgHeader debtor = TestObjectCreator.ABIGAS;
			debtor.CompanyData.OB_ARExternalDebtorCode = "DEBTORONE";
			ARInvoice arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1.0m, debtor);
			arInvoice.AH_PostDate = new ZDateTime(2009, 06, 15);
			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m);
			line.AL_AC = TestObjectCreator.RevenueChargeCode.PK;
			line.AL_AT = TestObjectCreator.RevenueChargeCode.GSTRate.PK;
			Factory.Save();
		}

		APInvoice apInvoicePositive;
		APInvoice apInvoicePositive2;
		APInvoice apInvoiceNegative;
		ARInvoice arInvoicePositive;
		ARInvoice arInvoiceNegative;
		APCreditNote apCreditNote;
		ARCreditNote arCreditNote;

		#endregion

		#region Credit Notes

		Dictionary<string, decimal> CreateCreditNotes(bool doMatching = false, bool doOnlyMatching = false, bool setAPRecoverableTax = false)
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value

			if (doOnlyMatching)
			{
				doMatching = true;
			}

			if (!doOnlyMatching)
			{
				var date = new ZDateTime(2008, 6, 15);
				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001001"));
				apCreditNotePositive = TestObjectCreator.CreateAPCreditNoteWithLine("001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1.0m, "Desc", job, TestObjectCreator.CC1, 100.0m, date, false);
				TestObjectCreator.CreateAPCreditNoteLine(apCreditNotePositive, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", -5m);
				var charge = job.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_AL_APLine = apCreditNotePositive.Lines[0].PK;
				charge.SetAmountsFromLinkedLinesForTests();
				charge.JR_OSCostGSTAmt_Calc = -apCreditNotePositive.Lines[0].AL_OSGSTAmount;
				charge.JR_OSSellAmt = 0M;
				var charge2 = job.Charges.AddNew();
				charge2.JR_AC = TestObjectCreator.CC1.PK;
				charge2.JR_AL_APLine = apCreditNotePositive.Lines[1].PK;
				charge2.SetAmountsFromLinkedLinesForTests();
				charge2.JR_OSCostGSTAmt_Calc = -apCreditNotePositive.Lines[1].AL_OSGSTAmount;
				charge2.JR_OSSellAmt = 0M;
				if (setAPRecoverableTax)
				{
					apCreditNotePositive.Lines[0].AL_Calc_InputGSTVATRecoverablePercentage = 70;
					apCreditNotePositive.Lines[1].AL_Calc_InputGSTVATRecoverablePercentage = 80;
				}
			}
			if (doMatching)
			{
				TestObjectCreator.CreateMatchLinkToPayARInvoice(apCreditNotePositive, ZDateTime.Today.AddDays(-2), 40);
			}

			Factory.Save();

			if (!doOnlyMatching)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					var date = new ZDateTime(2008, 6, 15);
					var apCreditNoteOtherCompany = TestObjectCreator.CreateAPCreditNoteWithLine("001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1.0m, "Desc", null, TestObjectCreator.NonAccrualChargeCode, 100.0m, date, false);
					apCreditNoteOtherCompany.AH_PostDate = date;

					Factory.Save();
				}
			}

			if (!doOnlyMatching)
			{
				var date = new ZDateTime(2008, 12, 15);
				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001002"));
				apCreditNoteNegative = TestObjectCreator.CreateAPCreditNoteWithLine("002", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1.0m, "Desc", job, TestObjectCreator.CC1, -100.0m, date, false);
				TestObjectCreator.CreateAPCreditNoteLine(apCreditNoteNegative, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 5m);
				var charge = job.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_AL_APLine = apCreditNoteNegative.Lines[0].PK;
				charge.SetAmountsFromLinkedLinesForTests();
				charge.JR_OSCostGSTAmt_Calc = -apCreditNoteNegative.Lines[0].AL_OSGSTAmount;
				charge.JR_OSSellAmt = 0M;
				var charge2 = job.Charges.AddNew();
				charge2.JR_AC = TestObjectCreator.CC1.PK;
				charge2.JR_AL_APLine = apCreditNoteNegative.Lines[1].PK;
				charge2.SetAmountsFromLinkedLinesForTests();
				charge2.JR_OSCostGSTAmt_Calc = -apCreditNoteNegative.Lines[1].AL_OSGSTAmount;
				charge2.JR_OSSellAmt = 0M;
				if (setAPRecoverableTax)
				{
					apCreditNoteNegative.Lines[0].AL_Calc_InputGSTVATRecoverablePercentage = 10;
					apCreditNoteNegative.Lines[1].AL_Calc_InputGSTVATRecoverablePercentage = 40;
				}
			}
			if (doMatching)
			{
				TestObjectCreator.CreateMatchLinkToPayAPInvoice(apCreditNoteNegative, ZDateTime.Today.AddDays(-2), -40);
			}

			Factory.Save();

			if (!doOnlyMatching)
			{
				var date = new ZDateTime(2009, 06, 15);
				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001003"));
				arCreditNotePositive = TestObjectCreator.CreateARCreditNoteWithLine("003", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1.0m, "Desc", job, TestObjectCreator.CC1, 1000.00m, date, false);
				TestObjectCreator.CreateARCreditNoteLine(arCreditNotePositive, job, TestObjectCreator.CC1, -100.00m, TestObjectCreator.AUD, 1.0m, "Desc");
				arCreditNotePositive.AH_PostDate = date;
				arCreditNotePositive.Lines[0].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
				arCreditNotePositive.Lines[0].AL_A9_VATClass = TaxMsg1.PK;
				arCreditNotePositive.Lines[1].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
				arCreditNotePositive.Lines[1].AL_A9_VATClass = TaxMsg2.PK;
				var charge = job.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_AL_ARLine = arCreditNotePositive.Lines[0].PK;
				charge.SetAmountsFromLinkedLinesForTests();
				charge.JR_OSCostAmt = 0M;
				var charge2 = job.Charges.AddNew();
				charge2.JR_AC = TestObjectCreator.CC1.PK;
				charge2.JR_AL_ARLine = arCreditNotePositive.Lines[1].PK;
				charge2.SetAmountsFromLinkedLinesForTests();
				charge2.JR_OSCostAmt = 0M;
			}
			if (doMatching)
			{
				TestObjectCreator.CreateMatchLinkToPayAPInvoice(arCreditNotePositive, ZDateTime.Today.AddDays(-2), -40);
			}

			Factory.Save();

			if (!doOnlyMatching)
			{
				var date = new ZDateTime(2009, 12, 15);
				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001004"));
				arCreditNoteNegative = TestObjectCreator.CreateARCreditNoteWithLine("004", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1.0m, "Desc", job, TestObjectCreator.CC1, -1000.00m, date, false);
				TestObjectCreator.CreateARCreditNoteLine(arCreditNoteNegative, job, TestObjectCreator.CC1, 100.00m, TestObjectCreator.AUD, 1.0m, "Desc");
				arCreditNoteNegative.AH_PostDate = date;
				arCreditNoteNegative.Lines[0].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
				arCreditNoteNegative.Lines[0].AL_A9_VATClass = TaxMsg3.PK;
				arCreditNoteNegative.Lines[1].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
				arCreditNoteNegative.Lines[1].AL_A9_VATClass = TaxMsg4.PK;
				var charge = job.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_AL_ARLine = arCreditNoteNegative.Lines[0].PK;
				charge.SetAmountsFromLinkedLinesForTests();
				charge.JR_OSCostAmt = 0M;
				var charge2 = job.Charges.AddNew();
				charge2.JR_AC = TestObjectCreator.CC1.PK;
				charge2.JR_AL_ARLine = arCreditNoteNegative.Lines[1].PK;
				charge2.SetAmountsFromLinkedLinesForTests();
				charge2.JR_OSCostAmt = 0M;
			}
			if (doMatching)
			{
				TestObjectCreator.CreateMatchLinkToPayARInvoice(arCreditNoteNegative, ZDateTime.Today.AddDays(-2), 40);
			}

			Factory.Save();

			return debitsCredits;
		}

		APCreditNote apCreditNotePositive;
		APCreditNote apCreditNoteNegative;
		ARCreditNote arCreditNotePositive;
		ARCreditNote arCreditNoteNegative;

		#endregion

		#region Adjusment Notes

		Dictionary<string, decimal> CreateAdjustmentNotes(bool doMatching = false, bool doOnlyMatching = false, bool setAPRecoverableTax = false)
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value

			if (doOnlyMatching)
			{
				doMatching = true;
			}

			if (!doOnlyMatching)
			{
				positiveARAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("001", 90.00m, 5m, new ZDateTime(2008, 06, 15), TestObjectCreator.ABIGAS.PK);
				TestObjectCreator.CreateAdjusmentNoteLine(positiveARAdjustmentNote, TestObjectCreator.CC4.PK, 100.00m, 10m);
				TestObjectCreator.CreateAdjusmentNoteLine(positiveARAdjustmentNote, TestObjectCreator.RevenueChargeCode.PK, -10.00m, -5m);

				SetupSubAccounts(positiveARAdjustmentNote.Lines[0]);
			}
			if (doMatching)
			{
				TestObjectCreator.CreateMatchLinkToPayARInvoice(positiveARAdjustmentNote, ZDateTime.Today.AddDays(-2), 40);
			}
			Factory.Save();

			if (!doOnlyMatching)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					var positiveARAdjustmentNoteOtherCompany = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("001", 90.00m, 5m, new ZDateTime(2008, 06, 15), TestObjectCreator.ABIGAS.PK);
					var lineOtherCompany = TestObjectCreator.CreateAdjusmentNoteLine(positiveARAdjustmentNoteOtherCompany, TestObjectCreator.CC10.PK, 100.00m, 10m);

					Factory.Save();
				}
			}

			if (!doOnlyMatching)
			{
				negativeARAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("002", -90.00m, -5m, new ZDateTime(2009, 06, 15), TestObjectCreator.ABIGAS.PK);
				TestObjectCreator.CreateAdjusmentNoteLine(negativeARAdjustmentNote, TestObjectCreator.CC4.PK, -100.00m, -10m);
				TestObjectCreator.CreateAdjusmentNoteLine(negativeARAdjustmentNote, TestObjectCreator.RevenueChargeCode.PK, 10.00m, 5m);
			}
			if (doMatching)
			{
				TestObjectCreator.CreateMatchLinkToPayAPInvoice(negativeARAdjustmentNote, ZDateTime.Today.AddDays(-2), -40);
			}
			Factory.Save();

			if (!doOnlyMatching)
			{
				positiveAPAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<APAdjustmentNote>("003", 90.00m, 5m, new ZDateTime(2010, 06, 15), TestObjectCreator.AALSHI.PK);
				TestObjectCreator.CreateAdjusmentNoteLine(positiveAPAdjustmentNote, TestObjectCreator.OverheadChargeCode.PK, 100.00m, 10m);
				TestObjectCreator.CreateAdjusmentNoteLine(positiveAPAdjustmentNote, TestObjectCreator.NonAccrualChargeCode.PK, -10.00m, -5m);
				if (setAPRecoverableTax)
				{
					positiveAPAdjustmentNote.Lines[0].AL_Calc_InputGSTVATRecoverablePercentage = 10;
					positiveAPAdjustmentNote.Lines[1].AL_Calc_InputGSTVATRecoverablePercentage = 40;
				}
			}
			if (doMatching)
			{
				TestObjectCreator.CreateMatchLinkToPayAPInvoice(positiveAPAdjustmentNote, ZDateTime.Today.AddDays(-2), -40);
			}
			Factory.Save();

			if (!doOnlyMatching)
			{
				negativeAPAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<APAdjustmentNote>("004", -90.00m, -5m, new ZDateTime(2011, 06, 15), TestObjectCreator.AALSHI.PK);
				TestObjectCreator.CreateAdjusmentNoteLine(negativeAPAdjustmentNote, TestObjectCreator.OverheadChargeCode.PK, -100.00m, -10m);
				TestObjectCreator.CreateAdjusmentNoteLine(negativeAPAdjustmentNote, TestObjectCreator.NonAccrualChargeCode.PK, 10.00m, 5m);
				if (setAPRecoverableTax)
				{
					negativeAPAdjustmentNote.Lines[0].AL_Calc_InputGSTVATRecoverablePercentage = 10;
					negativeAPAdjustmentNote.Lines[1].AL_Calc_InputGSTVATRecoverablePercentage = 40;
				}
			}
			if (doMatching)
			{
				TestObjectCreator.CreateMatchLinkToPayARInvoice(negativeAPAdjustmentNote, ZDateTime.Today.AddDays(-2), 40);
			}
			Factory.Save();

			return debitsCredits;
		}
		ARAdjustmentNote positiveARAdjustmentNote;
		ARAdjustmentNote negativeARAdjustmentNote;
		APAdjustmentNote positiveAPAdjustmentNote;
		APAdjustmentNote negativeAPAdjustmentNote;

		#endregion

		#region CB Transfers

		Dictionary<string, decimal> CreateCBTransfers()
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value

			BankTransfer positiveTransfer = TestObjectCreator.CreateBankTransfer(new ZDateTime(2008, 6, 15), TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.USDBankAccount.PK, 1000m, 0.90m);
			Factory.Save();

			BankTransfer negativeTransfer = TestObjectCreator.CreateBankTransfer(new ZDateTime(2009, 6, 15), TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.USDBankAccount.PK, -5000m, 0.90m);
			Factory.Save();

			return debitsCredits;
		}

		#endregion

		#region CB Exchange Differences

		Dictionary<string, decimal> CreateCBExchangeDifferences()
		{
			AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.ExchangeGainLossControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.ExchangeGainLossControlAccount.PK.ToGuid());

			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value

			RefExchangeRate exchangeRate = Factory.NewWithValidTestData<RefExchangeRate>();
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			exchangeRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.BuyRate;
			exchangeRate.RE_SellRate = 1.234m;
			exchangeRate.RE_RX_NKExCurrency = "JPY";
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

			var cb1 = CreateCalculatedCashbookExchangeDifference(new ZDateTime(2008, 6, 15), 100m, exchangeRate);
			cb1.BankAccount.AB_AG = TestObjectCreator.CashAtBankAccount.PK;

			var cb2 = CreateCalculatedCashbookExchangeDifference(new ZDateTime(2009, 6, 15), -5000m, exchangeRate);
			cb2.BankAccount.AB_AG = TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var exchangeRateOtherCompany = Factory.NewWithValidTestData<RefExchangeRate>();
				exchangeRateOtherCompany.RE_StartDate = ZDateTime.Today.AddDays(-2);
				exchangeRateOtherCompany.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
				exchangeRateOtherCompany.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.BuyRate;
				exchangeRateOtherCompany.RE_SellRate = 1.234m;
				exchangeRateOtherCompany.RE_RX_NKExCurrency = "JPY";
				exchangeRateOtherCompany.RE_GC = GlbCompany.CurrentCompany.PK;

				var cb3 = CreateCalculatedCashbookExchangeDifference(new ZDateTime(2008, 6, 15), 100m, exchangeRateOtherCompany);
				cb3.BankAccount.AB_AG = TestObjectCreator.CashOnHandAccount.PK;
			}

			return debitsCredits;
		}

		CashbookExchangeDiff CreateCalculatedCashbookExchangeDifference(ZDateTime postDate, decimal targetAmount, RefExchangeRate exchangeRate)
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_RX_NKAccountCurrency = exchangeRate.RE_RX_NKExCurrency;
			bankAccount.AB_OpenBalance = 0;
			bankAccount.AB_OpenOSBalance = Env.CurrentCompany.IsReciprocal ? targetAmount / 1.5m : targetAmount * 1.5m;

			Factory.Save(); // Need to save bank account so CB:EXX can load it from DB.

			var exchangeDifference = Factory.New<CashbookExchangeDiff>();
			exchangeDifference.AH_InvoiceDate = postDate;
			exchangeDifference.AH_PostDate = postDate;
			exchangeDifference.AH_AB = bankAccount.PK;
			exchangeDifference.AH_Desc = "test bank currency adjustment";
			exchangeDifference.AH_ExchangeRate = 1.5m;

			Factory.Save(); // Need to save CB:EXX so it can calculate itself and draw a unique DB key from the number fountain.

			AssertEquals("Precondition: Bank account should produce CB:EXX with correct properties.", targetAmount, exchangeDifference.AH_LocalExTaxAmount);
			AssertEquals("Precondition: Bank account should produce CB:EXX with correct properties.", 1.5m, exchangeDifference.AH_ExchangeRate);
			AssertEquals("Precondition: Bank account should produce CB:EXX with correct properties.", 0m, exchangeDifference.AH_LocalTaxAmount);
			AssertEquals("Precondition: Bank account should produce CB:EXX with correct properties.", TestObjectCreator.ExchangeGainLossControlAccount.PK, exchangeDifference.AH_AG);

			return exchangeDifference;
		}

		#endregion

		#region CB Direct Payment

		Dictionary<string, decimal> CreateCBDirectPayments(bool setRecoverableTax = false)
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value

			var paymentPositive = TestObjectCreator.CreateDirectPayment(new ZDateTime(2008, 6, 15), 100m, 10m, 4000m, 400m);
			if (setRecoverableTax)
			{
				paymentPositive.Lines[0].AL_Calc_InputGSTVATRecoverablePercentage = 77.77; //this is to check rounding to currency decimals
				paymentPositive.Lines[1].AL_Calc_InputGSTVATRecoverablePercentage = 60;
			}
			SetupSubAccounts(paymentPositive.Lines[0]);
			SetupSubAccounts(paymentPositive.Lines[1]);

			Factory.Save();

			var paymentNegative = TestObjectCreator.CreateDirectPayment(new ZDateTime(2009, 6, 15), -5000m, -500m, -10000m, -1000m);
			if (setRecoverableTax)
			{
				paymentNegative.Lines[0].AL_Calc_InputGSTVATRecoverablePercentage = 20;
				paymentNegative.Lines[1].AL_Calc_InputGSTVATRecoverablePercentage = 30;
			}
			SetupSubAccounts(paymentNegative.Lines[0]);
			SetupSubAccounts(paymentNegative.Lines[1]);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				TestObjectCreator.CreateDirectPayment(new ZDateTime(2008, 6, 15), 100m, 10m, 4000m, 400m);
				Factory.Save();
			}

			return debitsCredits;
		}

		#endregion

		#region CB Direct Receipt

		Dictionary<string, decimal> CreateCBDirectReceipts()
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value

			TestObjectCreator.CreateDirectReceipt(new ZDateTime(2010, 6, 15), 100m, 10m, 4000m, 400m);
			Factory.Save();

			TestObjectCreator.CreateDirectReceipt(new ZDateTime(2011, 6, 15), -5000m, -500m, -10000m, -1000m);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				TestObjectCreator.CreateDirectReceipt(new ZDateTime(2010, 6, 15), 100m, 10m, 4000m, 400m);
				Factory.Save();
			}

			return debitsCredits;
		}

		#endregion

		#region Job Costing Journals

		Dictionary<string, decimal> CreateJobCostingJournals()
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value

			ZDateTime date = new ZDateTime(2008, 6, 15);
			JCJournalHeader journal = TestObjectCreator.CreateJCJournalHeader(date, 170m);
			JCJournalLine line = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC1, null, date, 50m);
			line = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC2, null, date, 120m);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var journalOtherComany = TestObjectCreator.CreateJCJournalHeader(date, 170m);
				TestObjectCreator.CreateJCJournalLine(journalOtherComany, TestObjectCreator.CC10, null, date, 170m);
				Factory.Save();
			}

			date = new ZDateTime(2009, 6, 15);
			journal = TestObjectCreator.CreateJCJournalHeader(date, -180m);
			line = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC1, null, date, -80m);
			line = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC2, null, date, -100m);
			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			line.AL_GE = department.PK;

			Factory.Save();

			return debitsCredits;
		}

		#endregion

		#region Job Revenue Journals

		Dictionary<string, decimal> CreateJobRevenueJournals()
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value

			Job job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code, departmentCode: "CES");
			JobRevenueJournal journal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 250m);
			((JobRevenueJournalLine)journal.Lines[0]).OSUnsignedLineAmount = 150m;  //purposely break JRJ so that we can see a result in the GL Aggregation
			Factory.Save();

			return debitsCredits;
		}

		#endregion

		#region WIPs and Accruals

		Dictionary<string, decimal> CreateWIPsAndAccruals()
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //(period|GLAccountNum) = key, amount = value

			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001006"));
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_LocalSellAmt = 25m;
			charge.JR_OSSellAmt = 25m;
			charge.JR_LocalCostAmt = 50m;
			charge.JR_OSCostAmt = 50m;
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			Factory.Save();

			charge.ARLine.AL_PostDate = new ZDateTime(2008, 6, 15);
			charge.APLine.AL_PostDate = new ZDateTime(2009, 6, 15);
			Factory.Save();

			charge.JR_LocalSellAmt = 400m;
			charge.JR_OSSellAmt = 400m;
			charge.JR_LocalCostAmt = 700m;
			charge.JR_OSCostAmt = 700m;
			Factory.Save();

			charge.ARLine.AL_PostDate = new ZDateTime(2010, 6, 15);
			charge.APLine.AL_PostDate = new ZDateTime(2011, 6, 15);
			Factory.Save();

			return debitsCredits;
		}

		#endregion

		#region Create All Transactions

		void CreateTransactions()
		{
			CreatePayments();
			CreateReceipts();
			CreateDiscounts();
			CreateOverpayments();
			CreateARAPJournals();
			CreateGLJournals();
			CreateARAPTransfers();
			CreateARAPExchangeDifferences();
			CreateContras();
			CreateInvoices();
			CreateCreditNotes();
			CreateAdjustmentNotes();
			CreateCBTransfers();
			CreateCBExchangeDifferences();
			CreateCBDirectPayments();
			CreateCBDirectReceipts();
			CreateJobCostingJournals();
			CreateJobRevenueJournals();
			CreateWIPsAndAccruals();
		}

		#endregion

		#endregion

		protected abstract string Namespace { get; }
		protected abstract string AllTransactionsExportedFileName { get; }
		protected abstract string CashBasisVATTransactionsExportedFileName { get; }
		protected abstract string CashBasisVATTransactionsExportedAfterMatchingFileName { get; }
		protected abstract string CashBasisVATTransactionsExportedAgainAfterMatchingFileName { get; }
		protected abstract string CashBasisVATTransactionsExportedWithVATRecoverableTaxFileName { get; }
		protected abstract string CashBasisVATTransactionsExportedAfterMatchingWithVATRecoverableTaxFileName { get; }
		protected abstract string CashBasisVATTransactionsExportedAgainAfterMatchingWithVATRecoverableTaxFileName { get; }
		protected abstract string InvCrdAdjTransactionsExportedFileName { get; }
		protected abstract string InvCrdAdjTransactionsExportedWithVATRecoverableTaxFileName { get; }
		protected abstract string CBDirectPaymentsExportedFileName { get; }
		protected abstract string CBDirectPaymentsExportedWithVATRecoverableTaxFileName { get; }
		protected abstract string TransactionWithCountryAndIBANNumberFileName { get; }
		protected abstract string InvoiceWithConsolCostExportedFileName { get; }
		protected abstract string PostingWipAndAccrualExportedFileName { get; }
		protected abstract string ReverseWipAndAccrualExportedFileName { get; }
		protected abstract string InvoiceWithConsolCostGovtChargeCodeExportedFileName { get; }
		protected abstract string InvoiceWithPlaceOfSupplyExportedFileName { get; }
		protected abstract string JRJRevenueAndCostExportedFileName { get; }
		protected abstract string ARInvCrdAdjTransactionsExportedWithInvoiceRemittanceFileName { get; }
		protected abstract string APInvCrdAdjTransactionsExportedWithInvoiceRemittanceFileName { get; }
		protected abstract string WipAndAccrualWithDifferentTaxDatesExportedFileName { get; }
		protected abstract string InvoiceWithLocalCurrencyAndHighPrecisionExchangeRateFileName { get; }
		protected abstract string TaxGroupCodeForItaly { get; }
		protected abstract string TaxGroupCodeForPortugal { get; }
		protected abstract string TaxGroupCodeForArgentina { get; }
		protected abstract string WipAndAccrualWithRatingBasisFileName { get; }
		protected abstract string ARAPInvoicesWithRatingBasisFileName { get; }
		protected abstract string APInvoiceWithConsolCostRatingBasisFileName { get; }
		protected abstract string ChargeOSSellGSTShouldBeZeroWhenLocalSellGSTIsZero_WithoutSellInvoiceCurrencyFileName { get; }
		protected abstract string ChargeOSSellGSTShouldBeZeroWhenLocalSellGSTIsZero_WithSellInvoiceCurrencyFileName { get; }
		protected abstract string NJLExportedFileName { get; }
		protected abstract string TaxTransactionExported_MatchingRealizationBasis { get; }
		protected abstract string TaxTransactionExported_PostingRealizationBasis { get; }
		protected abstract string TaxTransactionExported_TaxTransactionCancelled_Batch1 { get; }
		protected abstract string TaxTransactionExported_TaxTransactionCancelled_Batch2 { get; }
		protected abstract string TaxTransactionExported_TaxRealized_Batch1 { get; }
		protected abstract string TaxTransactionExported_TaxRealized_Batch2 { get; }
		protected abstract string TransactionExported_AmendingReversingReason { get; }
		protected abstract string InvoiceWithGoodsClassChargeCode { get; }
		protected abstract string InvoiceWithCancelReason { get; }
		protected abstract string InvoiceWithCancelReasonAndCancelledAmendment { get; }
		protected abstract string ARInvoiceWithCashAdvanceReceivedFileName { get; }
		protected abstract string APInvoiceWithCashAdvanceReceivedFileName { get; }

		#region Tests

		[SuspendCriticalValidation]
		[TestDate(2008, 6, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAllTransactionsExportAgainstXmlFile()
		{
			SetupCommonData();
			SetupWithholdingTax();
			SetupTaxGroupCode();
			CreateTransactions();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);

			string xmlFile = BaseSourcePath + AllTransactionsExportedFileName;

			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}
		}

		[SuspendCriticalValidation]
		[TestDate(2008, 6, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAllTransactionsExportAgainstXmlFileWithOtherCompanyBatchExport()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				SetupPeriods();
			}

			SetupCommonData();
			SetupWithholdingTax();
			SetupTaxGroupCode();
			CreateTransactions();
			Factory.Save();

			SetupControlAccounts();
			TakeUpLedgers(GlbCompany.CurrentCompany.PK);
			TakeUpLedgers(NonCurrentCompanyBranch.Company.PK);

			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionWebExporterForTest(dataAccess);
			AccountingTransactionExportResponse response = exporter.CreateBatch(NonCurrentCompanyBranch.Company.GC_Code);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("Batch Number should not be zero.", 0, response.BatchNumber);
			var responseBatchNumberOtherCompany = response.BatchNumber;

			response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("Batch Number should not be zero.", 0, response.BatchNumber);
			AssertEquals("Batch Number should not be zero.", responseBatchNumberOtherCompany, response.BatchNumber);

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("PayLoad should contain XML", string.Empty, response.PayLoad);

			string xml = response.PayLoad;

			AssertAggregation(xml);

			string xmlFile = BaseSourcePath + AllTransactionsExportedFileName;
			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}
		}

		[TestDate(2012, 12, 13)]
		public void TestCreateBatchAccountingWebReportableException()
		{
			AssertEquals("High water mark registry value should not have been set", DateTime.MinValue, AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.Value);

			var request = new AccountingTransactionCreateBatchRequestForReportableException();
			var service = new AccountingTransactionExportService();
			service.SecurityHeader = new SecuritySOAPHeader() { UserName = "username", Password = "password" };
			var response = service.CreateBatch(request);
			Assert("Fail", !response.Succeeded);
			AssertNullOrEmpty(response.PayLoad);
			AssertEquals(response.ErrorMessage, "An unexpected error occurred. Please try again later.");

			var webException = ErrorReporter.LastExceptionReported as AccountingWebReportableException;
			AssertNotNull(webException);
			AssertEquals(webException.Message, "This is the outer exception.");
			AssertEquals(ErrorReporter.LastMessageReported, "This is the outer exception.");
			AssertEquals(ErrorReporter.LastKeyReported, "Accounting.Web_AccountingTransactionExportService.CreateBatch_FileNotFoundException");

			var innerException = webException.InnerException as FileNotFoundException;
			AssertNotNull(innerException);
			AssertNotNullOrEmpty(innerException.StackTrace);
			AssertEquals(webException.Source, innerException.Source);

			AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.Inner.ClearCache();
			AssertEquals("High water mark registry value should not be set in failure case", DateTime.MinValue, AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.Value);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[SuspendCriticalValidation]
		[TestDate(2008, 6, 15)]
		public void TestAllTransactionsExportAgainstXmlFileWithHighWaterMark()
		{
			var highWaterMark = ZDateTime.UtcNow.ToDateTime().Subtract(ExpectedHighWaterMarkBuffer);
			using (AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.DataType.SuspendValidation())
			{
				AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, highWaterMark);
			}
			AssertEquals("Precondition: High water mark registry value is set", highWaterMark, AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.Value);
			TestAllTransactionsExportAgainstXmlFile();
		}

		[TestDate(2008, 6, 15)]
		public void TestNullUserContext_TaxAmountCalculations()
		{
			SetupCommonData();

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), organisation: TestObjectCreator.LocalClient);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 100);
			var postedCharge = TestObjectCreator.CreateCharge(line, job, currency: TestObjectCreator.USD);
			postedCharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.Code;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, creditor: TestObjectCreator.Creditor1, debtor: TestObjectCreator.LocalClient);
			AssertNotEquals(line.AL_RX_NKTransactionCurrency, postedCharge.JR_RX_NKSellCurrency);
			AssertNotEquals(0m, postedCharge.JR_OSSellGSTAmt_Calc);
			AssertNotEquals(0m, charge.JR_OSCostGSTAmt_Calc);
			AssertNotEquals(0m, charge.JR_OSSellGSTAmt_Calc);

			Factory.Save();

			SetupControlAccounts();
			TakeUpLedgers();

			string companyCode = GlbCompany.CurrentCompany.GC_Code;
			using (Env.SetTemporaryUserContext(null))
			{
				var response = GetExportResponse(companyCode, Namespace);
				AssertEquals("Succeeded", true, response.Succeeded);
				AssertNotEquals("PayLoad should contain XML", string.Empty, response.PayLoad);
			}
		}

		[SuspendCriticalValidation]
		[TestDate(2008, 6, 15)]
		public void TestNullUserContext()
		{
			SetupCommonData();
			CreateTransactions();
			CreateTransactionWithExtraTypeRate();

			Factory.Save();

			SetupControlAccounts();
			TakeUpLedgers();

			string companyCode = GlbCompany.CurrentCompany.GC_Code;
			using (Env.SetTemporaryUserContext(null))
			{
				var response = GetExportResponse(companyCode, Namespace);
				AssertEquals("Succeeded", true, response.Succeeded);
				AssertNotEquals("PayLoad should contain XML", string.Empty, response.PayLoad);
			}

			void CreateTransactionWithExtraTypeRate()
			{
				var taxRate = TestObjectCreator.CreateTaxRate("IVAREF", "RateWithExtraRate", AccTaxRate.Types.Rated, 16, "REF", 2, 3, "AU");
				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m);
				var charge = TestObjectCreator.CreateChargeCode("CC10", "CC10 Test", Constants.ChargeType.Revenue, 0, taxRate, null);
				TestObjectCreator.CreateInvoiceLine(invoice, null, charge, 1.38m, taxRate: taxRate);
			}
		}

		[SuspendCriticalValidation]
		[TestDate(2008, 6, 15)]
		public void TestNullNamespace()
		{
			SetupCommonData();
			CreateTransactions();
			Factory.Save();

			string companyCode = GlbCompany.CurrentCompany.GC_Code;
			using (Env.TemporarilySetNullEnvironmentInstanceForTesting())
			{
				try
				{
					var response = GetExportResponse(companyCode, null);
					Fail("Should be an exception");
				}
				catch (XmlProcessingException e)
				{
					AssertEquals("Exception Message", "Invalid namespace [(null)] - Please use a valid Universal Namespace.", e.Message);
				}
			}
		}

		[SuspendCriticalValidation]
		[TestDate(2008, 6, 15)]
		public void TestEmptyNamespace()
		{
			SetupCommonData();
			CreateTransactions();
			Factory.Save();

			string companyCode = GlbCompany.CurrentCompany.GC_Code;
			using (Env.TemporarilySetNullEnvironmentInstanceForTesting())
			{
				try
				{
					var response = GetExportResponse(companyCode, string.Empty);
					Fail("Should be an exception");
				}
				catch (XmlProcessingException e)
				{
					AssertEquals("Exception Message", "Invalid namespace [] - Please use a valid Universal Namespace.", e.Message);
				}
			}
		}

		[SuspendCriticalValidation]
		[TestDate(2008, 6, 15)]
		public void TestInvalidNamespace()
		{
			SetupCommonData();
			CreateTransactions();
			Factory.Save();

			string companyCode = GlbCompany.CurrentCompany.GC_Code;
			using (Env.TemporarilySetNullEnvironmentInstanceForTesting())
			{
				try
				{
					var response = GetExportResponse(companyCode, "blah");
					Fail("Should be an exception");
				}
				catch (XmlProcessingException e)
				{
					AssertEquals("Exception Message", "Invalid namespace [blah] - Please use a valid Universal Namespace.", e.Message);
				}
			}
		}

		[SuspendCriticalValidation]
		[TestDate(2008, 6, 15)]
		public void TestValidNamespace()
		{
			SetupCommonData();
			CreateTransactions();
			Factory.Save();

			SetupControlAccounts();
			TakeUpLedgers();

			string companyCode = GlbCompany.CurrentCompany.GC_Code;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var response = GetExportResponse(companyCode, Namespace);
				AssertEquals("Succeeded", true, response.Succeeded);
				AssertNotEquals("PayLoad should contain XML", string.Empty, response.PayLoad);
			}
		}

		public void TestEmptyCompanyCode()
		{
			SetupCommonData();
			Factory.Save();

			GetExportResponseWithErrorCompanyCode(string.Empty, Namespace);
		}

		public void TestInvalidCompanyCode()
		{
			SetupCommonData();
			Factory.Save();

			GetExportResponseWithErrorCompanyCode("XYZ", Namespace);
		}

		void GetExportResponseWithErrorCompanyCode(string companyCode, string nameSpace)
		{
			RegistryItemDictionary.Instance.PurgeAll();
			BatchExportDataAccess dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionWebExporterForTest(dataAccess);
			AccountingTransactionExportResponse response = exporter.CreateBatch(companyCode);
			AssertEquals("Succeeded", false, response.Succeeded);
			AssertEquals("Error Message", $"Unable to load company: {companyCode}", response.ErrorMessage);
		}

		AccountingTransactionExportResponse GetExportResponse(string companyCode, string nameSpace)
		{
			RegistryItemDictionary.Instance.PurgeAll();
			BatchExportDataAccess dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionWebExporterForTest(dataAccess);
			AccountingTransactionExportResponse response = exporter.CreateBatch(companyCode);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("Batch Number should not be zero.", 0, response.BatchNumber);

			return exporter.ExportBatch(companyCode, response.BatchNumber, nameSpace);
		}

		AccountingTransactionExportResponse CreateBatch(string companyCode)
		{
			RegistryItemDictionary.Instance.PurgeAll();
			BatchExportDataAccess dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionWebExporterForTest(dataAccess);
			AccountingTransactionExportResponse response = exporter.CreateBatch(companyCode);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("Batch Number should not be zero.", 0, response.BatchNumber);

			return response;
		}

		string Export(bool needInit = true)
		{
			if (needInit)
			{
				SetupControlAccounts();
			}
			TakeUpLedgers();

			var response = GetExportResponse(GlbCompany.CurrentCompany.GC_Code, Namespace);

			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("PayLoad should contain XML", string.Empty, response.PayLoad);

			return response.PayLoad;
		}

		Dictionary<string, decimal> GetDebitsCredits(string xml)
		{
			Dictionary<string, decimal> debitsCredits = new Dictionary<string, decimal>();  //postperiod|GLAccountNum, amount

			XmlDocument document = new XmlDocument();
			document.LoadXml(xml);

			XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(document.NameTable);
			xmlnsManager.AddNamespace("n", Namespace);

			XmlNodeList nodes = document.SelectNodes("/n:UniversalTransactionBatch/n:TransactionBatch/n:TransactionCollection/n:Transaction/n:PostingJournalCollection/n:PostingJournal/n:PostingJournalDetailCollection/n:PostingJournalDetail", xmlnsManager);
			AssertNotNull("nodes should not be null", nodes);
			AssertNotEquals("nodes.Count should not be zero", 0, nodes.Count);
			foreach (XmlNode node in nodes)
			{
				XmlNodeList debitNodes = node.SelectNodes("n:DebitGLAccount", xmlnsManager);
				XmlNodeList creditNodes = node.SelectNodes("n:CreditGLAccount", xmlnsManager);
				XmlNode amountNode = node.SelectSingleNode("n:PostingAmount", xmlnsManager);
				XmlNode postingDateNode = node.SelectSingleNode("n:PostingDate", xmlnsManager);
				decimal amount = Convert.ToDecimal(amountNode.InnerText);
				DateTime postingDate = Convert.ToDateTime(postingDateNode.InnerText);
				string postingPeriod = postingDate.ToString("yyyyMM");

				foreach (XmlNode debitNode in debitNodes)
				{
					string accountNum = debitNode["AccountCode"].InnerText;
					AddAmount(debitsCredits, postingPeriod, accountNum, amount);
				}

				foreach (XmlNode creditNode in creditNodes)
				{
					string accountNum = creditNode["AccountCode"].InnerText;
					AddAmount(debitsCredits, postingPeriod, accountNum, -amount);
				}
			}

			return debitsCredits;
		}

		void AssertAggregation(string xml)
		{
			Dictionary<string, decimal> debitsCredits = GetDebitsCredits(xml);

			AssertAggregation(debitsCredits);
		}

		void AssertAggregation(Dictionary<string, decimal> debitsCredits)
		{
			string sql = string.Format(@"SELECT AA_Period, AG_AccountNum, SUM(AA_Amount) AA_Amount
						FROM dbo.AccGLAggregate
						LEFT OUTER JOIN dbo.AccGLHeader
							ON AA_AG = AG_PK
						WHERE AA_GB = {0}
						GROUP BY AA_Period, AG_AccountNum", GlbBranch.CurrentBranch.PK.ToSqlGuid());

			bool aggregationRows = false;

			using (var reader = TestConnection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					aggregationRows = true;
					string period = Convert.ToString(reader["AA_Period"]);
					string accountNum = (string)reader["AG_AccountNum"];
					decimal amount = (decimal)reader["AA_Amount"];

					decimal otherAmount;
					Assert(string.Format("Amount not present for Period={0} AccountNum={1}", period, accountNum), debitsCredits.TryGetValue(period + "|" + accountNum, out otherAmount));
					//AssertEquals(string.Format("Amount not equal for Period={0} AccountNum={1}", period, accountNum), otherAmount, amount);
					debitsCredits.Remove(period + "|" + accountNum);
				}
			}

			bool allZeroAmounts = true;
			foreach (string key in debitsCredits.Keys)
			{
				if (debitsCredits[key] != 0)
				{
					allZeroAmounts = false;
					break;
				}
			}

			AssertEquals("If there are non zero debits and credits in the dictionary, there should be aggregation records in the database", false, !aggregationRows && debitsCredits.Count > 0 && !allZeroAmounts);
			AssertEquals("There are debits and credits in the dictionary that are not in the database", true, debitsCredits.Count == 0 || allZeroAmounts);
		}

		public void TestReceipts()
		{
			Dictionary<string, decimal> debitsCredits = CreateReceipts();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);
			//AssertAggregation(debitsCredits);	//TODO Add manual expected debits and credits
		}

		public void TestPayments()
		{
			Dictionary<string, decimal> debitsCredits = CreatePayments();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);
			//AssertAggregation(debitsCredits);	//TODO Add manual expected debits and credits
		}

		public void TestDiscounts()
		{
			Dictionary<string, decimal> debitsCredits = CreateDiscounts();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);
			//AssertAggregation(debitsCredits);	//TODO Add manual expected debits and credits
		}

		public void TestOverpayments()
		{
			Dictionary<string, decimal> debitsCredits = CreateOverpayments();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);
			//AssertAggregation(debitsCredits);	//TODO Add manual expected debits and credits
		}

		public void TestARAPJournals()
		{
			Dictionary<string, decimal> debitsCredits = CreateARAPJournals();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);
			//AssertAggregation(debitsCredits);	//TODO Add manual expected debits and credits
		}

		public void TestGLJournals()
		{
			Dictionary<string, decimal> debitsCredits = CreateGLJournals();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);
			//AssertAggregation(debitsCredits);	//TODO Add manual expected debits and credits
		}

		public void TestARAPTransfers()
		{
			Dictionary<string, decimal> debitsCredits = CreateARAPTransfers();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);
			//AssertAggregation(debitsCredits);	//TODO Add manual expected debits and credits
		}

		public void TestARAPExchangeDifferences()
		{
			Dictionary<string, decimal> debitsCredits = CreateARAPExchangeDifferences();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);
			//AssertAggregation(debitsCredits);	//TODO Add manual expected debits and credits
		}

		public void TestContras()
		{
			Dictionary<string, decimal> debitsCredits = CreateContras();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);
			//AssertAggregation(debitsCredits);	//TODO Add manual expected debits and credits
		}

		public void TestInvoices()
		{
			Dictionary<string, decimal> debitsCredits = CreateInvoices();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);
			//AssertAggregation(debitsCredits);	//TODO Add manual expected debits and credits
		}

		[TestDate(2008, 6, 15)]
		public void TestCreditNotes()
		{
			Dictionary<string, decimal> debitsCredits = CreateCreditNotes();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);
			//AssertAggregation(debitsCredits);	//TODO Add manual expected debits and credits
		}

		public void TestAdjustmentNotes()
		{
			Dictionary<string, decimal> debitsCredits = CreateAdjustmentNotes();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);
			//AssertAggregation(debitsCredits);	//TODO Add manual expected debits and credits
		}

		public void TestCBTransfers()
		{
			Dictionary<string, decimal> debitsCredits = CreateCBTransfers();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);
			//AssertAggregation(debitsCredits);	//TODO Add manual expected debits and credits
		}

		[SuspendCriticalValidation]
		public void TestCBExchangeDifferences()
		{
			Dictionary<string, decimal> debitsCredits = CreateCBExchangeDifferences();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);
			//AssertAggregation(debitsCredits);	//TODO Add manual expected debits and credits
		}

		public void TestCBDirectPayments()
		{
			Dictionary<string, decimal> debitsCredits = CreateCBDirectPayments();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);
			//AssertAggregation(debitsCredits);	//TODO Add manual expected debits and credits
		}

		public void TestCBDirectReceipts()
		{
			Dictionary<string, decimal> debitsCredits = CreateCBDirectReceipts();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);
			//AssertAggregation(debitsCredits);	//TODO Add manual expected debits and credits
		}

		public void TestJobCostingJournals()
		{
			Dictionary<string, decimal> debitsCredits = CreateJobCostingJournals();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);
			//AssertAggregation(debitsCredits);	//TODO Add manual expected debits and credits
		}

		[SuspendSumOfLinesEqualZeroCriticalValidation]
		[TestDate(2008, 6, 15)]
		public void TestJobRevenueJournals()
		{
			Dictionary<string, decimal> debitsCredits = CreateJobRevenueJournals();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);
			//AssertAggregation(debitsCredits);	//TODO Add manual expected debits and credits
		}

		[TestDate(2008, 5, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWIPsAndAccruals()
		{
			SetupCommonData();
			var debitsCredits = new Dictionary<string, decimal>();

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001006"));
			TestObjectCreator.SetExchangeRate(job, TestObjectCreator.USD, 3);

			var positiveCharge = job.Charges.AddNew();
			positiveCharge.JR_AC = TestObjectCreator.CC1.PK;
			positiveCharge.JR_LocalSellAmt = 65m;
			positiveCharge.JR_OSSellAmt = 65m;
			positiveCharge.JR_LocalCostAmt = 55m;
			positiveCharge.JR_OSCostAmt = 55m;
			positiveCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			positiveCharge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;

			var negativeCharge = job.Charges.AddNew();
			negativeCharge.JR_AC = TestObjectCreator.CC1.PK;
			negativeCharge.JR_LocalSellAmt = -500m;
			negativeCharge.JR_OSSellAmt = -500m;
			negativeCharge.JR_LocalCostAmt = -355m;
			negativeCharge.JR_OSCostAmt = -355m;
			negativeCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			negativeCharge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;

			var gstCharge1 = job.Charges.AddNew();
			gstCharge1.JR_AC = TestObjectCreator.CC1.PK;
			gstCharge1.JR_RX_NKCostCurrency = TestObjectCreator.USD.Code;
			gstCharge1.JR_RX_NKSellCurrency = TestObjectCreator.USD.Code;
			gstCharge1.JR_OSCostAmt = 155;
			gstCharge1.JR_OSSellAmt = 165;
			gstCharge1.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			gstCharge1.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;

			var gstCharge2 = job.Charges.AddNew();
			gstCharge2.JR_AC = TestObjectCreator.CC1.PK;
			gstCharge2.JR_RX_NKCostCurrency = TestObjectCreator.USD.Code;
			gstCharge2.JR_RX_NKSellCurrency = TestObjectCreator.USD.Code;
			gstCharge2.JR_OSCostAmt = 255;
			gstCharge2.JR_OSSellAmt = 265;
			gstCharge2.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			gstCharge2.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
			gstCharge2.JR_AT_CostGSTRate = TestObjectCreator.GSTANDQST1WithDates.PK;
			gstCharge2.JR_AT_SellGSTRate = TestObjectCreator.GSTANDQST1WithDates.PK;

			Factory.Save();

			positiveCharge.ARLine.AL_PostDate = new ZDateTime(2008, 3, 15);
			positiveCharge.APLine.AL_PostDate = new ZDateTime(2008, 4, 15);

			negativeCharge.ARLine.AL_PostDate = new ZDateTime(2008, 7, 15);
			negativeCharge.APLine.AL_PostDate = new ZDateTime(2008, 8, 15);

			gstCharge1.ARLine.AL_PostDate = new ZDateTime(2008, 9, 15);
			gstCharge1.APLine.AL_PostDate = new ZDateTime(2008, 10, 15);

			gstCharge2.ARLine.AL_PostDate = new ZDateTime(2008, 11, 15);
			gstCharge2.APLine.AL_PostDate = new ZDateTime(2008, 12, 15);

			Factory.Save();

			var postingXml = Export();

			var xmlPostingFile = BaseSourcePath + PostingWipAndAccrualExportedFileName;
			using (StreamReader streamReader = File.OpenText(xmlPostingFile))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, postingXml);
			}

			TestDateAttribute.Date = new DateTime(2009, 2, 15);

			positiveCharge.JR_LocalSellAmt = 0m;
			positiveCharge.JR_OSSellAmt = 0m;
			positiveCharge.JR_LocalCostAmt = 0m;
			positiveCharge.JR_OSCostAmt = 0m;
			negativeCharge.JR_LocalSellAmt = 0m;
			negativeCharge.JR_OSSellAmt = 0m;
			negativeCharge.JR_LocalCostAmt = 0m;
			negativeCharge.JR_OSCostAmt = 0m;
			gstCharge1.JR_OSCostAmt = 0;
			gstCharge1.JR_OSSellAmt = 0;
			gstCharge2.JR_OSCostAmt = 0;
			gstCharge2.JR_OSSellAmt = 0;
			Factory.Save();

			var reverseXml = Export(false);

			var xmlReverseFile = BaseSourcePath + ReverseWipAndAccrualExportedFileName;
			using (StreamReader streamReader = File.OpenText(xmlReverseFile))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, reverseXml);
			}
		}

		public void TestParentTransaction()
		{
			CreateInvoicesSettingParentTransaction();
			Factory.Save();

			string xml = Export();

			XmlDocument document = new XmlDocument();
			document.LoadXml(xml);

			XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(document.NameTable);
			xmlnsManager.AddNamespace("n", Namespace);

			XmlNodeList nodes = document.SelectNodes("/n:UniversalTransactionBatch/n:TransactionBatch/n:TransactionCollection/n:Transaction", xmlnsManager);
			AssertNotNull("nodes should not be null", nodes);
			AssertEquals("nodes.Count should be 8", 8, nodes.Count);
			foreach (XmlNode node in nodes)
			{
				XmlNode ledgerNode = node.SelectSingleNode("n:Ledger", xmlnsManager);
				if (ledgerNode.InnerText == "AP")
				{
					XmlNode originalReferenceNode = node.SelectSingleNode("n:OriginalReference", xmlnsManager);
					if (originalReferenceNode != null)
					{
						XmlNode originalTransactionNumberNode = originalReferenceNode.SelectSingleNode("n:OriginalTransactionNumber", xmlnsManager);
						if (originalTransactionNumberNode != null)
						{
							AssertCollectionContains(originalTransactionNumberNode.InnerText, new[] { apInvoicePositive.AH_TransactionNum, apInvoicePositive2.AH_TransactionNum, apCreditNote.AH_OriginalTransactionNum });
						}

						XmlNode originalTransactionJobInvoiceNumberNode = originalReferenceNode.SelectSingleNode("n:OriginalTransactionJobInvoiceNumber", xmlnsManager);
						if (originalTransactionJobInvoiceNumberNode != null)
						{
							AssertCollectionContains(originalTransactionJobInvoiceNumberNode.InnerText, new[] { apInvoicePositive.AH_ConsolidatedInvoiceRef, apInvoicePositive2.AH_ConsolidatedInvoiceRef });
						}

						var originalTransactionDateNode = originalReferenceNode.SelectSingleNode("n:OriginalTransactionDate", xmlnsManager);
						AssertNotNull(originalTransactionDateNode);
						DateTime result;
						Assert(DateTime.TryParse(originalTransactionDateNode.InnerText, out result));
						AssertEquals("OriginalTransactionDate", apInvoicePositive.AH_InvoiceDate, result);

						var originalTransactionReferenceNode = originalReferenceNode.SelectSingleNode("n:OriginalTransactionReference", xmlnsManager);
						var originalTransactionComplianceSubTypeNode = originalReferenceNode.SelectSingleNode("n:OriginalTransactionComplianceSubType", xmlnsManager);
						if (originalTransactionReferenceNode?.InnerText == apInvoicePositive2.AH_TransactionNum)
						{
							Assert(apInvoicePositive2.AH_TransactionReference.IsEmpty);
							AssertNull(originalTransactionReferenceNode);
							Assert(apInvoicePositive2.AH_TransactionReference.IsEmpty);
							AssertNull(originalTransactionComplianceSubTypeNode);
						}
						else if (originalTransactionReferenceNode?.InnerText == apInvoicePositive.AH_TransactionNum)
						{
							AssertNotNull(originalTransactionReferenceNode);
							AssertEquals("OriginalTransactionComplianceNumber", apInvoicePositive.AH_TransactionReference, originalTransactionReferenceNode.InnerText);
							AssertNotNull(originalTransactionComplianceSubTypeNode);
							AssertEquals("OriginalTransactionComplianceSubType", apInvoicePositive.AH_ComplianceSubType, originalTransactionComplianceSubTypeNode.InnerText);
						}
					}
				}
				else if (ledgerNode.InnerText == "AR")
				{
					XmlNode originalReferenceNode = node.SelectSingleNode("n:OriginalReference", xmlnsManager);
					if (originalReferenceNode != null)
					{
						XmlNode originalTransactionNumberNode = originalReferenceNode.SelectSingleNode("n:OriginalTransactionNumber", xmlnsManager);
						if (originalTransactionNumberNode != null)
						{
							AssertCollectionContains(originalTransactionNumberNode.InnerText, new[] { arInvoicePositive.AH_TransactionNum, arCreditNote.AH_OriginalTransactionNum });
						}

						XmlNode originalTransactionJobInvoiceNumberNode = originalReferenceNode.SelectSingleNode("n:OriginalTransactionJobInvoiceNumber", xmlnsManager);
						if (originalTransactionJobInvoiceNumberNode != null)
						{
							AssertEquals("OriginalTransactionJobInvoiceNumber", arInvoicePositive.AH_ConsolidatedInvoiceRef, originalTransactionJobInvoiceNumberNode.InnerText);
						}

						XmlNode originalTransactionDateNode = originalReferenceNode.SelectSingleNode("n:OriginalTransactionDate", xmlnsManager);
						AssertNotNull(originalTransactionDateNode);
						DateTime result;
						Assert(DateTime.TryParse(originalTransactionDateNode.InnerText, out result));
						AssertEquals("OriginalTransactionDate", arInvoicePositive.AH_InvoiceDate, result);

						var originalTransactionReferenceNode = originalReferenceNode.SelectSingleNode("n:OriginalTransactionReference", xmlnsManager);
						if (originalTransactionReferenceNode?.InnerText == arInvoicePositive.AH_TransactionNum)
						{
							AssertNotNull(originalTransactionReferenceNode);
							AssertEquals("OriginalTransactionReference", arInvoicePositive.AH_TransactionReference, originalTransactionReferenceNode.InnerText);

							var originalTransactionComplianceSubTypeNode = originalReferenceNode.SelectSingleNode("n:OriginalTransactionComplianceSubType", xmlnsManager);
							AssertNotNull(originalTransactionComplianceSubTypeNode);
							AssertEquals("OriginalTransactionComplianceSubType", arInvoicePositive.AH_ComplianceSubType, originalTransactionComplianceSubTypeNode.InnerText);
						}
					}
				}
				else
				{
					Assert("Invalid Ledger", false);
				}
			}
		}

		public void TestExternalDebtorAndCreditor()
		{
			CreateInvoicesSettingExternalDebtorAndCreditor();
			Factory.Save();

			string xml = Export();

			XmlDocument document = new XmlDocument();
			document.LoadXml(xml);

			XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(document.NameTable);
			xmlnsManager.AddNamespace("n", Namespace);

			XmlNodeList nodes = document.SelectNodes("/n:UniversalTransactionBatch/n:TransactionBatch/n:TransactionCollection/n:Transaction", xmlnsManager);
			AssertNotNull("nodes should not be null", nodes);
			AssertEquals("nodes.Count should be 2", 2, nodes.Count);
			foreach (XmlNode node in nodes)
			{
				XmlNode ledgerNode = node.SelectSingleNode("n:Ledger", xmlnsManager);
				if (ledgerNode.InnerText == "AP")
				{
					XmlNode externalCreditorNode = node.SelectSingleNode("n:ExternalCreditorCode", xmlnsManager);
					AssertEquals("ExternalCreditorCode", "CREDITORONE", externalCreditorNode.InnerText);
				}
				else if (ledgerNode.InnerText == "AR")
				{
					XmlNode externalDebtorNode = node.SelectSingleNode("n:ExternalDebtorCode", xmlnsManager);
					AssertEquals("ExternalDebtorCode", "DEBTORONE", externalDebtorNode.InnerText);
				}
				else
				{
					Assert("Invalid Ledger", false);
				}
			}
		}

		public void TestCreateBatchLocking()
		{
			string sqlTextEDI = "EXEC AccountingTransactionExportCreateBatch 'EDI'";
			string sqlTextSIN = "EXEC AccountingTransactionExportCreateBatch 'SIN'";
			TestConnection.ExecuteNonQuery(sqlTextEDI);

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				Assert("Different Connections", !Object.ReferenceEquals(TestConnection, connection));
				connection.BeginTransaction();

				try
				{
					connection.ExecuteNonQuery(sqlTextEDI);
					Assert("Expected exception", false);
				}
				catch (SqlException ex)
				{
					AssertEquals("Error Number", 50000, ex.Number);
					Assert("Error Message", ex.Message.StartsWith("Could not obtain lock - process is already running"));
				}

				connection.ExecuteNonQuery(sqlTextSIN);

				try
				{
					connection.ExecuteNonQuery(sqlTextEDI);
					Assert("Expected exception", false);
				}
				catch (SqlException ex)
				{
					AssertEquals("Error Number", 50000, ex.Number);
					Assert("Error Message", ex.Message.StartsWith("Could not obtain lock - process is already running"));
				}

				connection.RollbackTransaction();
			}
		}

		public void TestCreditNoteWithAmendingReasonCodeDIS()
		{
			ZDateTime date = new ZDateTime(2008, 6, 15);
			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001001"));
			APCreditNote apCreditNote = TestObjectCreator.CreateAPCreditNoteWithLine("001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1.0m, "Desc", job, TestObjectCreator.CC1, 100.0m, date, false);
			apCreditNote.AH_PostDate = date;
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_AL_APLine = apCreditNote.Lines[0].PK;
			charge.SetAmountsFromLinkedLinesForTests();
			charge.JR_OSCostGSTAmt_Calc = -apCreditNote.Lines[0].AL_OSGSTAmount;
			charge.JR_OSSellAmt = 0M;
			Factory.Save();

			date = new ZDateTime(2009, 06, 15);
			ARCreditNote arCreditNote = TestObjectCreator.CreateARCreditNoteWithLine("002", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1.0m, "Desc", job, TestObjectCreator.CC1, 1000.00m, date, false);
			arCreditNote.AH_PostDate = date;
			arCreditNote.Lines[0].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			charge.JR_AL_ARLine = arCreditNote.Lines[0].PK;
			charge.SetAmountsFromLinkedLinesForTests();
			charge.JR_OSCostGSTAmt_Calc = -apCreditNote.Lines[0].AL_OSGSTAmount;
			Factory.Save();

			IAmending amending = arCreditNote;
			amending.AmendingReasonCode = "DIS";
			Factory.Save();

			AssertEquals("AH_ReceiptType", "DIS", arCreditNote.AH_ReceiptType);

			string xml = Export();

			AssertEquals("String must not be null or empty", string.IsNullOrEmpty(xml), false);

			AssertAggregation(xml);
		}

		[TestDate(2012, 12, 13)]
		public void TestHighWaterMarkIsSetWhenExportSuccessful()
		{
			AssertEquals("High water mark registry value should not have been set", DateTime.MinValue, AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.Value);

			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionWebExporterForTest(dataAccess);

			SetupControlAccounts();
			TakeUpLedgers();
			CreateInvoices();
			Factory.Save();

			var response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);

			AssertEquals("Succeeded", true, response.Succeeded);
			AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.Inner.ClearCache();
			AssertEquals("High water mark registry value should have been set", ZDateTime.UtcNow.ToDateTime().Subtract(ExpectedHighWaterMarkBuffer), AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.Value);
		}

		[TestDate(2012, 12, 13)]
		public void TestHighWaterMarkIsSetWhenExportNoTransactions()
		{
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionWebExporterForTest(dataAccess);
			var response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);

			AssertEquals("Succeeded", false, response.Succeeded);
			AssertEquals("High water mark registry value should have been set, even when no transactions in batch", ZDateTime.UtcNow.ToDateTime().Subtract(ExpectedHighWaterMarkBuffer), AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.Value);
		}

		[TestDate(2012, 12, 13)]
		public void TestHighWaterMarkIsSetCorrectly()
		{
			var utcDate = DateTime.UtcNow;

			using (AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.DataType.SuspendValidation())
			{
				AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, utcDate);
			}
			var highWaterMarkAsSetByAccountingConfigurationRegistry = GetAccountingTransactionExportServiceHighWaterMark();

			using (AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.DataType.SuspendValidation())
			{
				AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.MinValue);
			}
			AssertEquals("AccountingTransactionExportServiceHighWaterMark", DateTime.MinValue, AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.Value);

			BatchExportDataAccess dataAccess = new BatchExportDataAccess(Connection, Transaction);
			dataAccess.SetAccountingTransactionExportServiceHighWaterMark(utcDate, Transaction, GlbCompany.CurrentCompany.PK.ToGuid());

			AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.Inner.ClearCache();
			AssertEquals("AccountingTransactionExportServiceHighWaterMark", SqlFormatInfo.FromSqlDateTime(SqlFormatInfo.ToSqlDateTimeString(utcDate)), AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.Value);

			AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.Inner.DeleteRecord(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			AssertNull("No AccountingTransactionExportServiceHighWaterMark exists", GetAccountingTransactionExportServiceHighWaterMark());

			dataAccess.SetAccountingTransactionExportServiceHighWaterMark(utcDate, Transaction, GlbCompany.CurrentCompany.PK.ToGuid());
			var highWaterMarkAsSetByBatchExportDataAccess = GetAccountingTransactionExportServiceHighWaterMark();

			var auditColumns = new[]
			{
				StmDataSchema.Constants.SD_SystemCreateTimeUtc, StmDataSchema.Constants.SD_SystemCreateUser,
				StmDataSchema.Constants.SD_SystemLastEditTimeUtc, StmDataSchema.Constants.SD_SystemLastEditUser,
			};
			foreach (SchemaColumn column in ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumns(StmData.Schema.TableName))
			{
				if (column.Name == StmData.Schema.PK)
				{
					AssertNotEquals("PKs should be different because a new record was created", highWaterMarkAsSetByAccountingConfigurationRegistry[column], highWaterMarkAsSetByBatchExportDataAccess[column]);
				}
				else if (!auditColumns.Contains(column.Name))
				{
					AssertEquals(column.Name, highWaterMarkAsSetByAccountingConfigurationRegistry[column], highWaterMarkAsSetByBatchExportDataAccess[column]);
				}
			}
		}

		public void TestHighWaterMarkSmartParameterSuffixes()
		{
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionWebExporterForTest(dataAccess);
			exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);

			AssertNotContains("@HighWaterMark", dataAccess.QueryForGetPotentialBatch.Item1);
			AssertNotContains("@HighWaterMark", dataAccess.QueryForGetPotentialBatch.Item2);

			AssertHighWaterMarkParameterName(DateTime.Now.AddHours(-1), "HighWaterMark_Hours");
			AssertHighWaterMarkParameterName(DateTime.Now.AddHours(-25), "HighWaterMark_Days");
			AssertHighWaterMarkParameterName(DateTime.Now.AddDays(-6), "HighWaterMark_Days");
			AssertHighWaterMarkParameterName(DateTime.Now.AddDays(-7), "HighWaterMark_Weeks");
			AssertHighWaterMarkParameterName(DateTime.Now.AddDays(-29), "HighWaterMark_Weeks");
			AssertHighWaterMarkParameterName(DateTime.Now.AddDays(-30), "HighWaterMark_Months");
			AssertHighWaterMarkParameterName(DateTime.Now.AddDays(-89), "HighWaterMark_Months");
			AssertHighWaterMarkParameterName(DateTime.Now.AddDays(-90), "HighWaterMark_Quarters");
			AssertHighWaterMarkParameterName(DateTime.Now.AddDays(-364), "HighWaterMark_Quarters");
			AssertHighWaterMarkParameterName(DateTime.Now.AddDays(-365), "HighWaterMark_Years");
			AssertHighWaterMarkParameterName(DateTime.Now.AddDays(1), "HighWaterMark");

			void AssertHighWaterMarkParameterName(DateTime highWaterMarkDate, string expectedHighWaterMarkParameterName)
			{
				dataAccess.SetAccountingTransactionExportServiceHighWaterMark(highWaterMarkDate, Transaction, GlbCompany.CurrentCompany.PK.ToGuid());
				exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);

				AssertContains($"@{expectedHighWaterMarkParameterName}", dataAccess.QueryForGetPotentialBatch.Item1);
				AssertContains($"@{expectedHighWaterMarkParameterName} = {highWaterMarkDate.ToString("d/MM/yyyy h:mm:ss tt")}", dataAccess.QueryForGetPotentialBatch.Item2);
			}
		}

		StmData GetAccountingTransactionExportServiceHighWaterMark()
		{
			return Factory.Load<StmData>(
				new ZGuid(AccountingConfigurationRegistry.Instance.AccountingTransactionExportServiceHighWaterMark.Inner.GetRegistryItemPK(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)));
		}

		[TestDate(2008, 6, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCashBasisVAT()
		{
			SetupCommonData();

			TestObjectCreator.SetupCashBasisVAT();

			CreateInvoices();
			CreateCreditNotes();
			CreateAdjustmentNotes();

			SetupControlAccounts();
			TakeUpLedgers();

			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionWebExporterForTest(dataAccess);

			var response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("Batch Number should not be zero.", 0, response.BatchNumber);

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("PayLoad should contain XML", string.Empty, response.PayLoad);
			var xml = response.PayLoad;
			AssertAggregation(xml);

			string xmlFile = BaseSourcePath + CashBasisVATTransactionsExportedFileName;
			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertMultilineASCIIEquals("Repeated export should generate the same XML", xml, response.PayLoad);

			response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", false, response.Succeeded);
			AssertEquals("Repeated CreateBatch should fail.", "Nothing to be batched.", response.ErrorMessage);

			CreateInvoices(doOnlyMatching: true);
			CreateCreditNotes(doOnlyMatching: true);
			CreateAdjustmentNotes(doOnlyMatching: true);

			response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("Batch Number should not be zero.", 1, response.BatchNumber);

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("PayLoad should contain XML", string.Empty, response.PayLoad);
			xml = response.PayLoad;
			xmlFile = BaseSourcePath + CashBasisVATTransactionsExportedAgainAfterMatchingFileName;
			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertMultilineASCIIEquals("Repeated export should generate the same XML", xml, response.PayLoad);

			response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", false, response.Succeeded);
			AssertEquals("Repeated CreateBatch should fail.", "Nothing to be batched.", response.ErrorMessage);
		}

		[TestDate(2008, 6, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCashBasisVATMatched()
		{
			SetupCommonData();

			TestObjectCreator.SetupCashBasisVAT();

			CreateInvoices(doMatching: true);
			CreateCreditNotes(true);
			CreateAdjustmentNotes(true);

			SetupControlAccounts();
			TakeUpLedgers();

			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionWebExporterForTest(dataAccess);

			var response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("Batch Number should not be zero.", 0, response.BatchNumber);

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("PayLoad should contain XML", string.Empty, response.PayLoad);
			var xml = response.PayLoad;
			AssertAggregation(xml);

			string xmlFile = BaseSourcePath + CashBasisVATTransactionsExportedAfterMatchingFileName;
			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertMultilineASCIIEquals("Repeated export should generate the same XML", xml, response.PayLoad);

			response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", false, response.Succeeded);
			AssertEquals("Repeated CreateBatch should fail.", "Nothing to be batched.", response.ErrorMessage);
		}

		[TestDate(2008, 6, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCashBasisVAT_WithVATRecoverableTax()
		{
			SetupCommonData();

			TestObjectCreator.SetupCashBasisVAT();

			CreateInvoices(setAPRecoverableTax: true);
			CreateCreditNotes(setAPRecoverableTax: true);
			CreateAdjustmentNotes(setAPRecoverableTax: true);

			SetupControlAccounts();
			TakeUpLedgers();

			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionWebExporterForTest(dataAccess);

			var response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("Batch Number should not be zero.", 0, response.BatchNumber);

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("PayLoad should contain XML", string.Empty, response.PayLoad);
			var xml = response.PayLoad;
			AssertAggregation(xml);

			string xmlFile = BaseSourcePath + CashBasisVATTransactionsExportedWithVATRecoverableTaxFileName;
			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertMultilineASCIIEquals("Repeated export should generate the same XML", xml, response.PayLoad);

			response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", false, response.Succeeded);
			AssertEquals("Repeated CreateBatch should fail.", "Nothing to be batched.", response.ErrorMessage);

			CreateInvoices(doOnlyMatching: true, setAPRecoverableTax: true);
			CreateCreditNotes(doOnlyMatching: true, setAPRecoverableTax: true);
			CreateAdjustmentNotes(doOnlyMatching: true, setAPRecoverableTax: true);

			response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("Batch Number should not be zero.", 1, response.BatchNumber);

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("PayLoad should contain XML", string.Empty, response.PayLoad);
			xml = response.PayLoad;
			xmlFile = BaseSourcePath + CashBasisVATTransactionsExportedAgainAfterMatchingWithVATRecoverableTaxFileName;
			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertMultilineASCIIEquals("Repeated export should generate the same XML", xml, response.PayLoad);

			response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", false, response.Succeeded);
			AssertEquals("Repeated CreateBatch should fail.", "Nothing to be batched.", response.ErrorMessage);
		}

		[TestDate(2008, 6, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCashBasisVATMatched_WithVATRecoverableTax()
		{
			SetupCommonData();

			TestObjectCreator.SetupCashBasisVAT();

			CreateInvoices(doMatching: true, setAPRecoverableTax: true);
			CreateCreditNotes(true, setAPRecoverableTax: true);
			CreateAdjustmentNotes(true, setAPRecoverableTax: true);

			SetupControlAccounts();
			TakeUpLedgers();

			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionWebExporterForTest(dataAccess);

			var response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("Batch Number should not be zero.", 0, response.BatchNumber);

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("PayLoad should contain XML", string.Empty, response.PayLoad);
			var xml = response.PayLoad;
			AssertAggregation(xml);

			string xmlFile = BaseSourcePath + CashBasisVATTransactionsExportedAfterMatchingWithVATRecoverableTaxFileName;
			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertMultilineASCIIEquals("Repeated export should generate the same XML", xml, response.PayLoad);

			response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", false, response.Succeeded);
			AssertEquals("Repeated CreateBatch should fail.", "Nothing to be batched.", response.ErrorMessage);
		}

		[TestDate(2008, 6, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvCrdAdjTransactions()
		{
			SetupCommonData();

			CreateInvoices(setAPDocumentReceivedDate: true);
			CreateCreditNotes();
			CreateAdjustmentNotes();

			SetupControlAccounts();
			TakeUpLedgers();

			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionWebExporterForTest(dataAccess);

			var response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("Batch Number should not be zero.", 0, response.BatchNumber);

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("PayLoad should contain XML", string.Empty, response.PayLoad);
			var xml = response.PayLoad;
			AssertAggregation(xml);

			string xmlFile = BaseSourcePath + InvCrdAdjTransactionsExportedFileName;
			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertMultilineASCIIEquals("Repeated export should generate the same XML", xml, response.PayLoad);

			response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", false, response.Succeeded);
			AssertEquals("Repeated CreateBatch should fail.", "Nothing to be batched.", response.ErrorMessage);
			AssertEquals("IsTransactionCommitted is true", true, exporter.IsTransactionCommitted);
		}

		[TestDate(2008, 6, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvCrdAdjTransactions_WithVATRecoverableTax()
		{
			SetupCommonData();

			CreateInvoices(setAPRecoverableTax: true);
			CreateCreditNotes(setAPRecoverableTax: true);
			CreateAdjustmentNotes(setAPRecoverableTax: true);

			SetupControlAccounts();
			TakeUpLedgers();

			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionWebExporterForTest(dataAccess);

			var response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("Batch Number should not be zero.", 0, response.BatchNumber);

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("PayLoad should contain XML", string.Empty, response.PayLoad);
			var xml = response.PayLoad;
			AssertAggregation(xml);

			string xmlFile = BaseSourcePath + InvCrdAdjTransactionsExportedWithVATRecoverableTaxFileName;
			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertMultilineASCIIEquals("Repeated export should generate the same XML", xml, response.PayLoad);

			response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", false, response.Succeeded);
			AssertEquals("Repeated CreateBatch should fail.", "Nothing to be batched.", response.ErrorMessage);
		}

		[TestDate(2008, 6, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCBDirectPaymentsExport()
		{
			SetupCommonData();

			CreateCBDirectPayments();

			SetupControlAccounts();
			TakeUpLedgers();

			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionWebExporterForTest(dataAccess);

			var response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("Batch Number should not be zero.", 0, response.BatchNumber);

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("PayLoad should contain XML", string.Empty, response.PayLoad);
			var xml = response.PayLoad;
			AssertAggregation(xml);

			string xmlFile = BaseSourcePath + CBDirectPaymentsExportedFileName;
			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertMultilineASCIIEquals("Repeated export should generate the same XML", xml, response.PayLoad);

			response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", false, response.Succeeded);
			AssertEquals("Repeated CreateBatch should fail.", "Nothing to be batched.", response.ErrorMessage);
		}

		[TestDate(2008, 6, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCBDirectPaymentsExport_WithVATRecoverableTax()
		{
			SetupCommonData();

			CreateCBDirectPayments(setRecoverableTax: true);

			SetupControlAccounts();
			TakeUpLedgers();

			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionWebExporterForTest(dataAccess);

			var response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("Batch Number should not be zero.", 0, response.BatchNumber);

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertEquals("Succeeded", true, response.Succeeded);
			AssertNotEquals("PayLoad should contain XML", string.Empty, response.PayLoad);
			var xml = response.PayLoad;
			AssertAggregation(xml);

			string xmlFile = BaseSourcePath + CBDirectPaymentsExportedWithVATRecoverableTaxFileName;
			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}

			response = exporter.ExportBatch(GlbCompany.CurrentCompany.GC_Code, response.BatchNumber, Namespace);
			AssertMultilineASCIIEquals("Repeated export should generate the same XML", xml, response.PayLoad);

			response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Succeeded", false, response.Succeeded);
			AssertEquals("Repeated CreateBatch should fail.", "Nothing to be batched.", response.ErrorMessage);
		}

		class TestAccount
		{
			public ZString GLAccountNum;
			public ZString LocalAccountNum;
			public ZString LocalAccountDescription;
			public ZString LocalCountry;
			public ZString LocalLanguage;
			public ZString LocalReportType;
			public bool ShouldUse;

			public TestAccount(ZString glAccountNum, ZString localAccountNum, ZString localAccountDescription, ZString country, ZString language, ZString reportType, bool shouldUse = false)
			{
				GLAccountNum = glAccountNum;
				LocalAccountNum = localAccountNum;
				LocalAccountDescription = localAccountDescription;
				LocalCountry = country;
				LocalLanguage = language;
				LocalReportType = reportType;
				ShouldUse = shouldUse;
			}
		}

		List<TestAccount> CreateTestAccounts()
		{
			var testAccounts = new List<TestAccount>();
			testAccounts.Add(new TestAccount("3510.00.00", "3510-CN-CHS", "3510-CN-CHS-Value should be used", Constants.CountryCodes.China, "ZH-CN", "COA", true));
			testAccounts.Add(new TestAccount("3510.00.00", "3510-CN-CHT", "3510-CN-CHT-Value should not be used", Constants.CountryCodes.China, "ZH-TW", "COA", false));
			testAccounts.Add(new TestAccount("3510.00.00", "3510-SG-CHS", "3510-SG-CHS-Value should not be used", Constants.CountryCodes.Singapore, "ZH-CN", "COA", false));
			testAccounts.Add(new TestAccount("3520.00.00", "3510-CN-CHT", "3520-CN-CHT-Value should be used", Constants.CountryCodes.China, "ZH-TW", "COA", true));
			testAccounts.Add(new TestAccount("3520.00.00", "3520-SG-CHS", "3520-SG-CHS-Value should not be used", Constants.CountryCodes.Singapore, "ZH-CN", "COA", false));
			testAccounts.Add(new TestAccount("3530.00.00", "3530-SG-CHS", "3530-SG-CHS-Value should be used", Constants.CountryCodes.Singapore, "ZH-CN", "COA", true));
			testAccounts.Add(new TestAccount("3540.00.00", "3540-SG-ENG", "3540-SG-ENG-Value should not be used", Constants.CountryCodes.Singapore, "EN", "COA", false));

			foreach (var testAccount in testAccounts)
			{
				var accGLHeader = TestObjectCreator.GetGLAccountFromDB(testAccount.GLAccountNum);
				if (accGLHeader == null)
				{
					accGLHeader = TestObjectCreator.CreateGLHeader();
					accGLHeader.AG_AccountNum = testAccount.GLAccountNum;
					accGLHeader.AG_Description = testAccount.GLAccountNum + "Description";
				}

				BuildMappingBetweenGLHeaderAndLocalAccount(accGLHeader, testAccount);
			}

			return testAccounts;
		}

		void BuildMappingBetweenGLHeaderAndLocalAccount(AccGLHeader glHeader, TestAccount testAccount)
		{
			var decscriptor = TestObjectCreator.CreateAccountDescriptor(glHeader.PK,
																		testAccount.LocalAccountNum,
																		testAccount.LocalReportType,
																		"",
																		testAccount.LocalLanguage,
																		testAccount.LocalAccountDescription,
																		testAccount.LocalCountry,
																		Constants.DebitCredit.Debit);

			if (testAccount.LocalReportType != AccGLAccountDescriptor.ReportTypeCOA)
			{
				TestObjectCreator.CreateGLDescriptorPivotLight(decscriptor, glHeader);
			}
		}

		public void TestGetLocalGLAccountsMappingReturnsRightLocalAccounts()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				orgProxy.OH_Language = "ZH-CN";
				var companyPK = GlbCompany.CurrentCompany.PK;

				var testAccounts = CreateTestAccounts();
				Factory.Save();

				var glAccounts = testAccounts.Select(x => x.GLAccountNum).Distinct().ToList();
				var batchExportDataAccess = new BatchExportDataAccess(Connection, Transaction);
				var retriveAccounts = batchExportDataAccess.GetLocalGLAccountsMapping(glAccounts, companyPK);
				AssertEquals("retriveAccounts should have 3 local accounts.", 3, retriveAccounts.Count);

				int count = 0;
				foreach (var testAccount in testAccounts)
				{
					bool isFind = retriveAccounts.TryGetValue(testAccount.GLAccountNum, out Tuple<ZString, ZString> localAccount);
					if (testAccount.ShouldUse)
					{
						Assert("Local account should be finded successfully.", isFind);
						AssertEquals("Local account code should be right.", testAccount.LocalAccountNum, localAccount.Item1);
						AssertEquals("Local account description should be right.", testAccount.LocalAccountDescription, localAccount.Item2);
						count++;
					}
				}
				AssertEquals("All test accounts with ShouldUse = True should be tested.", 3, count);
			}
		}

		public void TestGetLocalGLAccountsMappingReturnsCOAReportTypeLocalAccount()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				orgProxy.OH_Language = "CHS";
				var companyPK = GlbCompany.CurrentCompany.PK;

				var glHeader = TestObjectCreator.GLHeader1;
				var coaTestAccount = new TestAccount(glHeader.AG_AccountNum, "3510COA", "COADescription", "CN", "CHT", "COA", true);
				var plmTestAccount = new TestAccount(glHeader.AG_AccountNum, "3510PLM", "PLMDescription", "CN", "CHS", "PLM", false);
				BuildMappingBetweenGLHeaderAndLocalAccount(glHeader, coaTestAccount);
				BuildMappingBetweenGLHeaderAndLocalAccount(glHeader, plmTestAccount);
				Factory.Save();

				var glAccounts = new List<ZString>();
				glAccounts.Add(glHeader.AG_AccountNum);
				var batchExportDataAccess = new BatchExportDataAccess(Connection, Transaction);
				var retriveAccounts = batchExportDataAccess.GetLocalGLAccountsMapping(glAccounts, companyPK);

				AssertEquals("retriveAccounts should have only one result.", 1, retriveAccounts.Count);
				var isFind = retriveAccounts.TryGetValue(glHeader.AG_AccountNum, out Tuple<ZString, ZString> localAccount);
				Assert("Local account should be finded successfully.", isFind);
				AssertEquals("Should find the COA report type local account number.", coaTestAccount.LocalAccountNum, localAccount.Item1);
				AssertEquals("Should find the COA report type local account description.", coaTestAccount.LocalAccountDescription, localAccount.Item2);
			}
		}

		[TestDate(2011, 2, 18)]
		public void TestARInvoiceHasLocalAccountInPostingJournalDetailAccounts()
		{
			SetupControlAccounts();

			ARInvoice arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
			var glHeader = TestObjectCreator.GLHeader1;
			var line = TestObjectCreator.CreateInvoiceLine(arInvoice, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, 1.0m, 1.0m, glHeader.PK);
			var localAccount = new TestAccount(glHeader.AG_AccountNum, "TestAccount", "TestDescription", "CN", "CHS", "COA");
			BuildMappingBetweenGLHeaderAndLocalAccount(glHeader, localAccount);
			Factory.Save();

			AssertPostingJournalDetailSectionShouldHaveLocalAccount(glHeader.AG_AccountNum, localAccount, true, false, true);
		}

		[TestDate(2011, 6, 12)]
		public void TestAPInvoiceHasLocalAccountInPostingJournalDetailAccounts()
		{
			SetupControlAccounts();

			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("00004000", TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.ABIGAS);
			apInvoice.Lines[0].AL_AG = TestObjectCreator.GLHeader1.PK;
			var glHeader = TestObjectCreator.GLHeader1;
			var localAccount = new TestAccount(glHeader.AG_AccountNum, "TestAccount", "TestDescription", "CN", "CHS", "COA");
			BuildMappingBetweenGLHeaderAndLocalAccount(glHeader, localAccount);
			Factory.Save();

			AssertPostingJournalDetailSectionShouldHaveLocalAccount(glHeader.AG_AccountNum, localAccount, true, true, false);
		}

		[TestDate(2011, 6, 12)]
		public void TestDirectReceiptHasLocalAccountInPostingJournalDetailAccounts()
		{
			SetupControlAccounts();

			var directReceipt = TestObjectCreator.CreateDirectReceipt(new ZDateTime(2011, 6, 2), 1.0m, 1.0m, 1.0m, 1.0m);
			var glHeader = directReceipt.Lines[0].GLHeader;
			var localAccount = new TestAccount(glHeader.AG_AccountNum, "TestAccount", "TestDescription", "CN", "CHS", "COA");
			BuildMappingBetweenGLHeaderAndLocalAccount(glHeader, localAccount);
			Factory.Save();

			AssertPostingJournalDetailSectionShouldHaveLocalAccount(glHeader.AG_AccountNum, localAccount, true, false, true);
		}

		[TestDate(2011, 6, 12)]
		public void TestARReceiptHasLocalAccountInPostingJournalDetailAccounts()
		{
			SetupControlAccounts();

			var bankAccount = TestObjectCreator.CreateBankAccount("Test", "Desc", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			var arReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, bankAccount.PK);
			var glHeader = bankAccount.GLHeader;
			var localAccount = new TestAccount(glHeader.AG_AccountNum, "TestAccount", "TestDescription", "CN", "CHS", "COA");
			BuildMappingBetweenGLHeaderAndLocalAccount(glHeader, localAccount);
			Factory.Save();

			AssertPostingJournalDetailSectionShouldHaveLocalAccount(glHeader.AG_AccountNum, localAccount, false, true, false);
		}

		[TestDate(2011, 6, 12)]
		public void TestGLJournalHasLocalAccountInPostingJournalDetailAccounts()
		{
			SetupControlAccounts();

			var glJournal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLAutoJournal, new ZDateTime(2011, 6, 2), new ZDateTime(2011, 6, 2), new ZDateTime(2011, 6, 2));
			var lineCR = TestObjectCreator.CreateGLJournalLine(glJournal, 5m, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			var lineDR = TestObjectCreator.CreateGLJournalLine(glJournal, -5m, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			var glHeader = lineCR.GLHeader;
			var localAccount = new TestAccount(glHeader.AG_AccountNum, "TestAccount", "TestDescription", "CN", "CHS", "COA");
			BuildMappingBetweenGLHeaderAndLocalAccount(glHeader, localAccount);
			Factory.Save();

			AssertPostingJournalDetailSectionShouldHaveLocalAccount(glHeader.AG_AccountNum, localAccount, true, false, true);
		}

		[TestDate(2011, 6, 12)]
		public void TestARJournalHasLocalAccountInPostingJournalDetailAccounts()
		{
			SetupControlAccounts();

			var arJournal = Factory.NewWithValidTestData<ARJournal>();
			arJournal.AH_AG = TestObjectCreator.GLHeader1.PK;
			var glHeader = arJournal.GLHeader;
			var localAccount = new TestAccount(glHeader.AG_AccountNum, "TestAccount", "TestDescription", "CN", "CHS", "COA");
			BuildMappingBetweenGLHeaderAndLocalAccount(glHeader, localAccount);
			Factory.Save();

			AssertPostingJournalDetailSectionShouldHaveLocalAccount(glHeader.AG_AccountNum, localAccount, true, false, true);
		}

		void AssertPostingJournalDetailSectionShouldHaveLocalAccount(ZString glAccountNum, TestAccount localAccount, bool hasGLAccount, bool isTestDebitGLAccount, bool isTestCreditAccount)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				orgProxy.OH_Language = "CHS";

				var xml = Export(false);
				var document = new XmlDocument();
				document.LoadXml(xml);
				var xmlnsManager = new XmlNamespaceManager(document.NameTable);
				xmlnsManager.AddNamespace("n", Namespace);

				if (hasGLAccount)
				{
					var glAccountNodes = document.SelectNodes("/n:UniversalTransactionBatch/n:TransactionBatch/n:TransactionCollection/n:Transaction/n:PostingJournalCollection/n:PostingJournal/n:GLAccount", xmlnsManager);
					var glAccounts = glAccountNodes.Cast<XmlNode>().Where(x => x["AccountCode"].InnerText == glAccountNum);
					AssertGreaterThanOrEqualTo("PostingJournal should have at least 1 GLAccount node.", glAccounts.Count(), 1);

					foreach (XmlNode node in glAccountNodes)
					{
						if (string.Equals(node["AccountCode"].InnerText, glAccountNum.ToString()))
						{
							AssertNotNull("PostingJournal GLAccount should have local account code value.", node["LocalComplianceAccountCode"]);
							AssertNotNull("PostingJournal GLAccount should have local account description value.", node["LocalComplianceAccountDescription"]);
							AssertEquals("PostingJournal GLAccount should have a right local account code value.", localAccount.LocalAccountNum, node["LocalComplianceAccountCode"].InnerText);
							AssertEquals("PostingJournal GLAccount should have a right local account description value.", localAccount.LocalAccountDescription, node["LocalComplianceAccountDescription"].InnerText);
						}
						else
						{
							AssertNull("PostingJournal GLAccount should not have local account code value.", node["LocalComplianceAccountCode"]);
							AssertNull("PostingJournal GLAccount should not have local account description value.", node["LocalComplianceAccountDescription"]);
						}
					}
				}

				var debitGLAccountNodes = document.SelectNodes("/n:UniversalTransactionBatch/n:TransactionBatch/n:TransactionCollection/n:Transaction/n:PostingJournalCollection/n:PostingJournal/n:PostingJournalDetailCollection/n:PostingJournalDetail/n:DebitGLAccount", xmlnsManager);
				var creditGLAccountNodes = document.SelectNodes("/n:UniversalTransactionBatch/n:TransactionBatch/n:TransactionCollection/n:Transaction/n:PostingJournalCollection/n:PostingJournal/n:PostingJournalDetailCollection/n:PostingJournalDetail/n:CreditGLAccount", xmlnsManager);

				AssertNotNull("debitAndCreditNodes should not be null.", debitGLAccountNodes);
				AssertNotEquals("debitAndCreditNodes count should greater than zero", 0, debitGLAccountNodes.Count);
				AssertNotNull("debitAndCreditNodes should not be null.", creditGLAccountNodes);
				AssertNotEquals("debitAndCreditNodes count should greater than zero", 0, creditGLAccountNodes.Count);

				if (isTestDebitGLAccount)
				{
					var debitGLAccounts = debitGLAccountNodes.Cast<XmlNode>().Select(x => x).Where(x => x["AccountCode"].InnerText == glAccountNum);
					AssertGreaterThanOrEqualTo("Should have at least 1 DebitGLAccount node.", debitGLAccounts.Count(), 1);
					foreach (XmlNode debitNode in debitGLAccountNodes)
					{
						if (string.Equals(debitNode["AccountCode"].InnerText, glAccountNum))
						{
							AssertNotNull("DebitGLAccount should have local account code value.", debitNode["LocalComplianceAccountCode"]);
							AssertNotNull("DebitGLAccount should have local account description value.", debitNode["LocalComplianceAccountDescription"]);
							AssertEquals("DebitGLAccount should have a right local account code value.", localAccount.LocalAccountNum, debitNode["LocalComplianceAccountCode"].InnerText);
							AssertEquals("DebitGLAccount should have a right local account description value.", localAccount.LocalAccountDescription, debitNode["LocalComplianceAccountDescription"].InnerText);
						}
						else
						{
							AssertNull("DebitGLAccount should not have local account code value.", debitNode["LocalComplianceAccountCode"]);
							AssertNull("DebitGLAccount should not have local account description value.", debitNode["LocalComplianceAccountDescription"]);
						}
					}
				}

				if (isTestCreditAccount)
				{
					var creditGLAccounts = creditGLAccountNodes.Cast<XmlNode>().Select(x => x).Where(x => x["AccountCode"].InnerText == glAccountNum);
					AssertGreaterThanOrEqualTo("Should have at least 1 CreditGLAccount node.", creditGLAccounts.Count(), 1);
					foreach (XmlNode creditNode in creditGLAccountNodes)
					{
						if (string.Equals(creditNode["AccountCode"].InnerText, glAccountNum))
						{
							AssertNotNull("CreditGLAccount should have local account code value.", creditNode["LocalComplianceAccountCode"]);
							AssertNotNull("CreditGLAccount should have local account description value.", creditNode["LocalComplianceAccountDescription"]);
							AssertEquals("CreditGLAccount should have a right local account code value.", localAccount.LocalAccountNum, creditNode["LocalComplianceAccountCode"].InnerText);
							AssertEquals("CreditGLAccount should have a right local account description value.", localAccount.LocalAccountDescription, creditNode["LocalComplianceAccountDescription"].InnerText);
						}
						else
						{
							AssertNull("CreditGLAccount should not have local account code value.", creditNode["LocalComplianceAccountCode"]);
							AssertNull("CreditGLAccount should not have local account description value.", creditNode["LocalComplianceAccountDescription"]);
						}
					}
				}
			}
		}

		[TestDate(2011, 6, 12)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestARInvCrdAdjTransactions_WithRemittanceReference()
		{
			SetupCommonData();
			SetupInvoiceRemittanceConfiguration();

			var org = TestObjectCreator.ABIGAS;
			org.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1.0m, org);
			arInvoice.AH_PostDate = new ZDateTime(2011, 6, 2);
			var line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m);
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			var arCreditNote = TestObjectCreator.CreateARCreditNoteWithLine("002", org, TestObjectCreator.AUD, 1.0m, "Desc", null, TestObjectCreator.CC1, 1000.00m, new ZDateTime(2011, 6, 2), false);
			var arAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("003", 100m, 0m, new ZDateTime(2011, 6, 2), org.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(arAdjustmentNote, TestObjectCreator.RevenueChargeCode.PK, 100m, 0m);

			Factory.Save();

			string xml = Export();
			string xmlFile = BaseSourcePath + ARInvCrdAdjTransactionsExportedWithInvoiceRemittanceFileName;

			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}
		}

		[TestDate(2011, 6, 10)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAPInvCrdAdjTransactions_WithRemittanceReference()
		{
			SetupCommonData();

			var org = TestObjectCreator.ABIGAS;
			org.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, org);
			var creditNote = TestObjectCreator.CreateAPCreditNoteWithLine("002", org, TestObjectCreator.AUD, 1.0m, "Desc", null, TestObjectCreator.NonAccrualChargeCode, 1000.00m, new ZDateTime(2011, 6, 2), false);
			var adjustmentNote = TestObjectCreator.CreateAdjustmentNote<APAdjustmentNote>("003", 100m, 0m, new ZDateTime(2011, 6, 2), org.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(adjustmentNote, TestObjectCreator.NonAccrualChargeCode.PK, 100m, 0m);
			invoice.InvoiceRemittanceReference = "TEST001";
			creditNote.InvoiceRemittanceReference = "TEST002";
			adjustmentNote.InvoiceRemittanceReference = "TEST003";

			Factory.Save();

			string xml = Export();
			string xmlFile = BaseSourcePath + APInvCrdAdjTransactionsExportedWithInvoiceRemittanceFileName;

			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}
		}

		[TestDate(2011, 6, 10)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAPInvCrdAdjTransactions_WithRemittanceReference_WhenHavingMultipleTransactionHeaderReference()
		{
			SetupCommonData();

			var org = TestObjectCreator.ABIGAS;
			org.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, org);
			var creditNote = TestObjectCreator.CreateAPCreditNoteWithLine("002", org, TestObjectCreator.AUD, 1.0m, "Desc", null, TestObjectCreator.NonAccrualChargeCode, 1000.00m, new ZDateTime(2011, 6, 2), false);
			var adjustmentNote = TestObjectCreator.CreateAdjustmentNote<APAdjustmentNote>("003", 100m, 0m, new ZDateTime(2011, 6, 2), org.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(adjustmentNote, TestObjectCreator.NonAccrualChargeCode.PK, 100m, 0m);
			invoice.InvoiceRemittanceReference = "TEST001";
			creditNote.InvoiceRemittanceReference = "TEST002";
			adjustmentNote.InvoiceRemittanceReference = "TEST003";

			CreateAccTransactionHeaderReference(invoice, "FAK", "TEST001_F");
			CreateAccTransactionHeaderReference(creditNote, "FAK", "TEST002_F");
			CreateAccTransactionHeaderReference(adjustmentNote, "FAK", "TEST003_F");

			Factory.Save();

			CombineAssertions("PreCondition, ", () =>
			{
				AssertEquals("Invoice AccTransactionHeaderReference Count", 2, LoadAccTransactionHeaderReferenceFromDatabase(invoice).Count());
				AssertEquals("CreditNote AccTransactionHeaderReference Count", 2, LoadAccTransactionHeaderReferenceFromDatabase(creditNote).Count());
				AssertEquals("AdjustmentNote AccTransactionHeaderReference Count", 2, LoadAccTransactionHeaderReferenceFromDatabase(adjustmentNote).Count());
			});

			var xml = Export();
			var xmlFile = BaseSourcePath + APInvCrdAdjTransactionsExportedWithInvoiceRemittanceFileName;
			using (var streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}

			void CreateAccTransactionHeaderReference(AccTransactionHeader transactionHeader, string type, string value)
			{
				var reference = Factory.New<AccTransactionHeaderReference>();
				reference.AH1_AH = transactionHeader.PK;
				reference.AH1_Type = type;
				reference.AH1_Reference = value;
			}

			IEnumerable<AccTransactionHeaderReference> LoadAccTransactionHeaderReferenceFromDatabase(AccTransactionHeader transactionHeader)
			{
				var zQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				zQuery.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_AH, transactionHeader.PK);
				zQuery.FetchOnlyFromLocalCache = false;
				zQuery.IgnoreDbQueryCache = true;

				return Factory.Load<AccTransactionHeaderReference>(zQuery);
			}
		}

		[TestDate(2011, 6, 12)]
		public void TestExportBatchWithoutDisposableActionForDbConnectionError()
		{
			var request = new AccountingTransactionExportRequest();
			var service = new AccountingTransactionExportService();
			service.SecurityHeader = new SecuritySOAPHeader() { UserName = "username", Password = "password" };

			var lastError = string.Empty;

			var thread = new Thread(() =>
			{
				ErrorReporter.Clear();

				service.ExportBatch(request);

				lastError = ErrorReporter.LastMessageReported;
				ErrorReporter.Clear();
			});

			thread.Start();
			thread.Join(1000);

			// Should not be "Attempt to use Db.Connection without using Db.DisposableActionForDbConnection()"
			AssertNullOrEmpty(lastError);
		}

		[TestDate(2008, 5, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportWIPsAndAccrualsWithDifferentTaxDates()
		{
			SetupCommonData();

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001006"));
			TestObjectCreator.SetExchangeRate(job, TestObjectCreator.AUD, 1);

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "MYGST";
			taxRate.SetRate_ForTestOnly(700, 10, new ZDate(2010, 3, 16), new ZDate(9999, 1, 1));
			taxRate.SetRate_ForTestOnly(300, 10, new ZDate(1900, 1, 1), new ZDate(2010, 3, 15));

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "MyCC";
			chargeCode.AC_Desc = "My Charge Code";
			chargeCode.AC_ChargeType = "RAT";
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.AC_AG_AccrualAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_RevenueAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_WIPAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AT_GSTRate = taxRate.PK;

			Factory.Save();

			var oldWIPTaxCharge = job.Charges.AddNew();
			oldWIPTaxCharge.JR_AC = chargeCode.PK;
			oldWIPTaxCharge.JR_LocalSellAmt = 100m;
			oldWIPTaxCharge.JR_OSSellAmt = 100m;
			oldWIPTaxCharge.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
			oldWIPTaxCharge.JR_AT_SellGSTRate = taxRate.PK;
			oldWIPTaxCharge.JR_SellTaxDate = new ZDate(2008, 1, 15);

			var newWIPTaxCharge = job.Charges.AddNew();
			newWIPTaxCharge.JR_AC = chargeCode.PK;
			newWIPTaxCharge.JR_LocalSellAmt = 500m;
			newWIPTaxCharge.JR_OSSellAmt = 500m;
			newWIPTaxCharge.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
			newWIPTaxCharge.JR_AT_SellGSTRate = taxRate.PK;
			newWIPTaxCharge.JR_SellTaxDate = new ZDate(2015, 10, 15);

			var oldACRTaxCharge = job.Charges.AddNew();
			oldACRTaxCharge.JR_AC = chargeCode.PK;
			oldACRTaxCharge.JR_LocalCostAmt = 300m;
			oldACRTaxCharge.JR_OSCostAmt = 300m;
			oldACRTaxCharge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			oldACRTaxCharge.JR_AT_CostGSTRate = taxRate.PK;
			oldACRTaxCharge.JR_CostTaxDate = new ZDate(2000, 1, 15);

			var newACRTaxCharge = job.Charges.AddNew();
			newACRTaxCharge.JR_AC = chargeCode.PK;
			newACRTaxCharge.JR_LocalCostAmt = 700m;
			newACRTaxCharge.JR_OSCostAmt = 700m;
			newACRTaxCharge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			newACRTaxCharge.JR_AT_CostGSTRate = taxRate.PK;
			newACRTaxCharge.JR_CostTaxDate = new ZDate(2010, 10, 15);

			Factory.Save();

			oldWIPTaxCharge.WIP.AL_PostDate = new ZDateTime(2008, 1, 15);
			newWIPTaxCharge.WIP.AL_PostDate = new ZDate(2008, 2, 15);
			oldACRTaxCharge.Accrual.AL_PostDate = new ZDate(2008, 3, 15);
			newACRTaxCharge.Accrual.AL_PostDate = new ZDate(2008, 4, 15);
			Factory.Save();

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + WipAndAccrualWithDifferentTaxDatesExportedFileName;
			using (StreamReader streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}
		}

		[TestDate(2008, 5, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportInvoiceWithLocalCurrencyAndHighPrecisionExchangeRate()
		{
			SetupCommonData();
			var originalGC_IsReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			GlbCompany.CurrentCompany.Factory.Save();

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "TEST001", GlbCompany.CurrentCompany.LocalCurrency, 1M, 0.65M, 0.15M, 3.55M, 0.67M);
			apInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var taxRate = TestObjectCreator.CreateTaxRate("TAX", "TEST", 23);
			var line = apInvoice.Lines[0];
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_JH = job.PK;
			line.AL_RX_NKTransactionCurrency = "EUR";
			line.ExchangeRate.Rate = 4.435769M;
			line.AL_OSExTaxAmount = 0.65M;
			line.AL_AT = taxRate.PK;
			TestObjectCreator.CreateCharge(line);
			Factory.Save();

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + InvoiceWithLocalCurrencyAndHighPrecisionExchangeRateFileName;
			using (var streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}

			GlbCompany.CurrentCompany.GC_IsReciprocal = originalGC_IsReciprocal;
		}

		[TestDate(2008, 5, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportTaxGroupCodeDescriptionForItaly()
		{
			var query = new ZQuery(StmDataSchema.SD_Name, "TaxMessageGroupsManagement");
			query.AddToFilter(StmDataSchema.SD_Owner, GlbCompany.CurrentCompany.PK.ToGuid());
			var registryRecord = Factory.Load<StmData>(query);

			AssertEquals("No overridden data for registry TaxMessageGroupsManagement", 0, registryRecord.Length);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Italy);

			SetupPeriods();

			SetupCommonData();
			Factory.Save();

			var code = ItalyComplianceInfo.TaxMessageGroupCodes.N0;

			var taxMessage = TestObjectCreator.TaxMsg4;
			taxMessage.A9_TaxGroupCode = code;

			var tax = TestObjectCreator.CreateTaxRate("ZER", "Zero rated", 0, 1, Constants.CountryCodes.Italy);
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001001", TestObjectCreator.EUR, 1M, 100M, 0M, 100M, 0M);
			var line = invoice.Lines[0];
			line.AL_AT = tax.PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_A9_VATClass = taxMessage.PK;

			Factory.Save();

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + TaxGroupCodeForItaly;
			using (var streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}
		}

		[TestDate(2008, 5, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportTaxGroupCodeDescriptionForPortugal()
		{
			var query = new ZQuery(StmDataSchema.SD_Name, "TaxMessageGroupsManagement");
			query.AddToFilter(StmDataSchema.SD_Owner, GlbCompany.CurrentCompany.PK.ToGuid());
			var registryRecord = Factory.Load<StmData>(query);

			AssertEquals("No overridden data for registry TaxMessageGroupsManagement", 0, registryRecord.Length);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Portugal);

			SetupPeriods();

			SetupCommonData();
			Factory.Save();

			var code = PortugalComplianceInfo.TaxMessageGroupCodes.M19;

			var taxMessage = TestObjectCreator.TaxMsg4;
			taxMessage.A9_TaxGroupCode = code;

			var tax = TestObjectCreator.CreateTaxRate("ZER", "Zero rated", 0, 1, Constants.CountryCodes.Portugal);
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001001", TestObjectCreator.EUR, 1M, 100M, 0M, 100M, 0M);
			var line = invoice.Lines[0];
			line.AL_AT = tax.PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_A9_VATClass = taxMessage.PK;

			Factory.Save();

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + TaxGroupCodeForPortugal;
			using (var streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}
		}

		[TestDate(2020, 8, 19)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportTaxGroupCodeDescriptionForArgentina()
		{
			var query = new ZQuery(StmDataSchema.SD_Name, "TaxMessageGroupsManagement");
			query.AddToFilter(StmDataSchema.SD_Owner, GlbCompany.CurrentCompany.PK.ToGuid());
			var registryRecord = Factory.Load<StmData>(query);

			AssertEquals("No overridden data for registry TaxMessageGroupsManagement", 0, registryRecord.Length);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Argentina);

			SetupPeriods();

			SetupCommonData();
			Factory.Save();

			var code = ArgentinaComplianceInfo.TaxMessageGroupCodes.N3;

			var taxMessage = TestObjectCreator.TaxMsg4;
			taxMessage.A9_TaxGroupCode = code;

			var tax = TestObjectCreator.CreateTaxRate("IVA", "IVA rated", 0, 1, Constants.CountryCodes.Argentina);
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001001", TestObjectCreator.EUR, 1M, 100M, 0M, 100M, 0M);
			var line = invoice.Lines[0];
			line.AL_AT = tax.PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_A9_VATClass = taxMessage.PK;

			Factory.Save();

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + TaxGroupCodeForArgentina;
			using (var streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}
		}

		[TestDate(2008, 5, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportWIPsAndAccrualsWithRatingBasis()
		{
			SetupCommonData();

			var shipmentID = "S00001007";
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment(shipmentID));
			TestObjectCreator.SetExchangeRate(job, TestObjectCreator.AUD, 1);

			var jobCharge = job.Charges.AddNew();
			jobCharge.JR_AC = TestObjectCreator.CC1.PK;
			jobCharge.JR_LocalSellAmt = 100m;
			jobCharge.JR_OSSellAmt = 100m;
			jobCharge.JR_LocalCostAmt = 100m;
			jobCharge.JR_OSCostAmt = 100m;
			jobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			jobCharge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;

			TestObjectCreator.CreateJobPaymentBasis(jobCharge.PK.ToGuid(), null, true, "Shipment", shipmentID);
			TestObjectCreator.CreateJobPaymentBasis(jobCharge.PK.ToGuid(), null, false, "Shipment", shipmentID);

			Factory.Save();

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + WipAndAccrualWithRatingBasisFileName;
			using (StreamReader streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}
		}

		[TestDate(2008, 5, 15)]
		public void TestExportRatingBasisWithInvalidCurrency()
		{
			SetupCommonData();

			var shipmentID = "S00001007";
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment(shipmentID));
			TestObjectCreator.SetExchangeRate(job, TestObjectCreator.AUD, 1);

			var jobCharge = job.Charges.AddNew();
			jobCharge.JR_AC = TestObjectCreator.CC1.PK;
			jobCharge.JR_LocalSellAmt = 100m;
			jobCharge.JR_OSSellAmt = 100m;
			jobCharge.JR_LocalCostAmt = 100m;
			jobCharge.JR_OSCostAmt = 100m;
			jobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			jobCharge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;

			TestObjectCreator.CreateJobPaymentBasis(jobCharge.PK.ToGuid(), null, true, "Shipment", shipmentID, rateCurrency: "TST");
			TestObjectCreator.CreateJobPaymentBasis(jobCharge.PK.ToGuid(), null, false, "Shipment", shipmentID, rateCurrency: "TST");

			Factory.Save();

			AssertNoExceptionThrown(() => Export());
		}

		[TestDate(2008, 5, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportARAPInvoicesWithRatingBasis()
		{
			SetupCommonData();

			var consolID = "C00001000";
			var shipmentID = "S00001000";
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("001", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
			apInvoice.Lines.RemoveAndDeleteAll();
			var forwardingConsol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", consolID);
			var consolCost = TestObjectCreator.CreateConsolCost(forwardingConsol, TestObjectCreator.CC1, 100m);
			consolCost.E6_AH_APInvoice = apInvoice.PK;
			var shipment = TestObjectCreator.CreateShipment(shipmentID, forwardingConsol);
			var job = TestObjectCreator.CreateJob(shipment, false);

			var apInvoiceLine = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1.0m, 100m);
			apInvoiceLine.AL_AC = TestObjectCreator.CC1.PK;
			apInvoiceLine.AL_JH = job.PK;
			var charge1 = TestObjectCreator.CreateCharge(apInvoiceLine);
			charge1.JR_E6 = consolCost.PK;
			charge1.JR_LocalSellAmt = 0m;
			charge1.JR_OSSellAmt = 0m;

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
			var arInvoiceLine = TestObjectCreator.CreateARInvoiceLine(arInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "", 100m);

			var charge2 = TestObjectCreator.CreateCharge(arInvoiceLine);
			charge2.JR_LocalCostAmt = 0m;
			charge2.JR_OSCostAmt = 0m;

			TestObjectCreator.CreateJobPaymentBasis(null, consolCost.PK.ToGuid(), true, "Consolidation", consolID);
			TestObjectCreator.CreateJobPaymentBasis(charge2.PK.ToGuid(), null, false, "Shipment", shipmentID);

			Factory.Save();

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + ARAPInvoicesWithRatingBasisFileName;
			using (StreamReader streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}
		}

		[TestDate(2008, 5, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportAPInvoiceWithConsolCostRatingBasis()
		{
			SetupCommonData();

			var consolID = "C00001000";
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("001", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
			apInvoice.Lines.RemoveAndDeleteAll();
			var forwardingConsol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", consolID);
			var consolCost = TestObjectCreator.CreateConsolCost(forwardingConsol, TestObjectCreator.CC1, 100m);
			consolCost.E6_AH_APInvoice = apInvoice.PK;
			var shipment = TestObjectCreator.CreateShipment("S00001000", forwardingConsol);
			var job = TestObjectCreator.CreateJob(shipment, false);

			var line = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1.0m, 100m);
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_JH = job.PK;
			var charge = TestObjectCreator.CreateCharge(line);
			charge.JR_E6 = consolCost.PK;
			charge.JR_LocalSellAmt = 0m;
			charge.JR_OSSellAmt = 0m;

			TestObjectCreator.CreateJobPaymentBasis(null, consolCost.PK.ToGuid(), true, "Consolidation", consolID);

			Factory.Save();

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + APInvoiceWithConsolCostRatingBasisFileName;
			using (StreamReader streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}
		}

		[TestDate(2008, 5, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestChargeOSSellGSTShouldBeZeroWhenLocalSellGSTIsZero_WithoutSellInvoiceCurrency()
		{
			SetupCommonData();

			TestObjectCreator.USD.RX_SubUnitRatio = 0;
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00000001"));
			TestObjectCreator.SetExchangeRate(job, TestObjectCreator.USD, 14476m);
			TestObjectCreator.GST1.SetRate_ForTestOnly(1, 1);
			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_OSSellAmt = 7238m;
			charge.JR_InvoiceType = "CUR";

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + ChargeOSSellGSTShouldBeZeroWhenLocalSellGSTIsZero_WithoutSellInvoiceCurrencyFileName;
			using (StreamReader streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}
		}

		[TestDate(2008, 5, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestChargeOSSellGSTShouldBeZeroWhenLocalSellGSTIsZero_WithSellInvoiceCurrency()
		{
			SetupCommonData();

			TestObjectCreator.USD.RX_SubUnitRatio = 0;
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00000001"));
			TestObjectCreator.SetExchangeRate(job, TestObjectCreator.USD, 14476m);
			TestObjectCreator.GST1.SetRate_ForTestOnly(1, 1);
			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_OSSellAmt = 7238m;
			charge.JR_InvoiceType = "FIN";
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + ChargeOSSellGSTShouldBeZeroWhenLocalSellGSTIsZero_WithSellInvoiceCurrencyFileName;
			using (StreamReader streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}
		}

		[SuspendCriticalValidation]
		[TestDate(2008, 6, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBankAccountWithCountryAndIBANNumber()
		{
			var accountDetails = TestObjectCreator.ABIGAS.CompanyData.ARAccountDetailsCollection.AddNew();
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = "AU";
			accountDetails.A1_PaymentMethod = "TAX";
			accountDetails.A1_AccountName = "Account Name C";
			accountDetails.A1_BankName = "Bank Name C";
			accountDetails.A1_BankSwift = "1234567";
			accountDetails.A1_BankBsb = "060369";
			accountDetails.A1_BankAccount = "1357924680";
			accountDetails.A1_RN_NKCountryCode = "AT";
			accountDetails.A1_IBANNumber = "AT483200000012345864";
			Factory.Save();

			SetupCommonData();
			SetupWithholdingTax();
			SetupTaxGroupCode();
			CreateTransactions();
			Factory.Save();

			string xml = Export();

			AssertAggregation(xml);

			string xmlFile = BaseSourcePath + TransactionWithCountryAndIBANNumberFileName;

			using (StreamReader streamReader = File.OpenText(xmlFile))
			{
				string expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xml);
			}
		}

		[TestDate(2008, 5, 26, 0, 0, 0)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportBatchForNJL()
		{
			var glHeader1 = TestObjectCreator.CreateAccGLHeader("8765.43.21", "TS", "Test AR NTE", "NTE", Constants.DebitCredit.Debit);
			var glHeader2 = TestObjectCreator.CreateAccGLHeader("9876.54.32", "TS", "Test AP NTE", "NTE", Constants.DebitCredit.Credit);
			glHeader1.AG_StatisticalUnits = "KG";
			glHeader2.AG_StatisticalUnits = "KWH";

			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));
			var gLJournalSource = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLNoteJournal, ZDateTime.Today, ZDateTime.Today);
			var gLLine1 = TestObjectCreator.CreateGLJournalLine(gLJournalSource, 10m, DebitCredit.CR, glHeader1.PK);
			gLJournalSource.Lines.Add(gLLine1);
			var gLLine2 = TestObjectCreator.CreateGLJournalLine(gLJournalSource, 10m, DebitCredit.DR, glHeader2.PK);
			gLJournalSource.Lines.Add(gLLine2);
			Factory.Save();

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + NJLExportedFileName;
			using (StreamReader streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}
		}

		#region Tax Framework

		[TestDate(2010, 5, 26, 0, 0, 0)]
		[SuspendCriticalValidation]
		public void TestTaxTransaction_BatchCreatedCorrectly()
		{
			var results = Factory.Load<GenExportBatchSequence>(new ZQuery());
			AssertEquals("No batch created yet.", 0, results.Length);

			SetupCommonData();
			SetupPeriods();
			SetupControlAccounts();
			SetupTaxConfigurations();

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));

			var apInvoice1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			var line1_1 = TestObjectCreator.CreateInvoiceLine(apInvoice1, TestObjectCreator.CC1.PK, 100M, TestObjectCreator.AUD, 1M);
			line1_1.AL_JH = job.PK;
			line1_1.AL_AG = TestObjectCreator.GLHeader1.PK;
			var line1_2 = TestObjectCreator.CreateInvoiceLine(apInvoice1, TestObjectCreator.CC1.PK, 200M, TestObjectCreator.AUD, 1M);
			line1_2.AL_JH = job.PK;
			line1_2.AL_AG = TestObjectCreator.GLHeader1.PK;
			var line1_3 = TestObjectCreator.CreateInvoiceLine(apInvoice1, TestObjectCreator.CC1.PK, 700M, TestObjectCreator.AUD, 1M);
			line1_3.AL_JH = job.PK;
			line1_3.AL_AG = TestObjectCreator.GLHeader1.PK;

			var taxTransactionPER1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = apInvoice1.PK, TaxSystem = taxSystemPER, TaxConfiguration = taxConfigPER, TaxBasis = TaxBasisList.Matching.Code, RealisationDate = ZDate.Empty, OsTaxBaseAmount = 200M, LocalTaxBaseAmount = 200M, OsTaxAmount = 4M, LocalTaxAmount = 4M, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxTransactionPER1.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line1_1));
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxTransactionPER1.PK, debitAccountPK: taxPrepaidPendingControlAccount.PK, creditAccountPK: apControlAccount.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Pending.Code);
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxTransactionPER1.PK, debitAccountPK: taxRealisedControlAccount.PK, creditAccountPK: taxPrepaidPendingControlAccount.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Realised.Code);

			var taxTransactionSLX = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = apInvoice1.PK, TaxSystem = taxSystemSLX, TaxConfiguration = taxConfigSLX, TaxBasis = TaxBasisList.Posting.Code, RealisationDate = ZDate.Today, OsTaxBaseAmount = 100M, OsTaxAmount = 11M, LocalTaxBaseAmount = 100M, LocalTaxAmount = 11M, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxTransactionSLX.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line1_2));
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxTransactionSLX.PK, debitAccountPK: taxExpenseAccount.PK, creditAccountPK: apControlAccount.PK, amount: 11M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);

			var taxTransactionSPR1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = apInvoice1.PK, TaxSystem = taxSystemSPR, TaxConfiguration = taxConfigSPR, TaxBasis = TaxBasisList.PostingOnMatching.Code, RealisationDate = ZDate.Empty, OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -45M, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxTransactionSPR1.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line1_3));

			var taxTransactionSPR2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = apInvoice1.PK, TaxSystem = taxSystemSPR, TaxConfiguration = taxConfigSPR, TaxBasis = TaxBasisList.PostingOnMatching.Code, RealisationDate = ZDate.Empty, OsTaxBaseAmount = 400M, OsTaxAmount = -60M, LocalTaxBaseAmount = 400M, LocalTaxAmount = -60M, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxTransactionSPR2.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line1_3));

			Factory.Save();

			var expectedBatchNumber = 1L;
			var response = CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals(expectedBatchNumber, response.BatchNumber);

			var dataTable = GetGenExportBatchSequenceData(expectedBatchNumber);
			AssertEquals(11, dataTable.Rows.Count);

			var result = dataTable.Select($"XB_ParentTableCode='ATM'");
			AssertEquals(3, result.Length);

			result = dataTable.Select($"XB_ParentTableCode='ATT'");
			AssertEquals(2, result.Length);

			taxTransactionSPR1.IsCancelled = true;
			Factory.Save();

			response = CreateBatch(GlbCompany.CurrentCompany.GC_Code);
			expectedBatchNumber = 2L;
			AssertEquals(expectedBatchNumber, response.BatchNumber);

			dataTable = GetGenExportBatchSequenceData(expectedBatchNumber);
			result = dataTable.Select($"XB_ParentTableCode='ATT'");
			AssertEquals(1, result.Length);

			DataTable GetGenExportBatchSequenceData(long batchNumber)
			{
				string sql = string.Format(@"SELECT XB_Type, XB_ParentID, XB_ParentTableCode
						FROM dbo.GenExportBatchSequence
						WHERE XB_BatchNumber = '{0}'", batchNumber.ToString());

				return DataUtils.GetDataTableFromQuery(TestConnection, sql);
			}
		}

		[TestDate(2010, 5, 26, 0, 0, 0)]
		[SuspendCriticalValidation]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTaxTransaction_ExportTransaction_PostingOnMatchDateRealizationBasis_TaxRealized()
		{
			SetupCommonData();
			SetupPeriods();
			SetupTaxConfigurations();

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			var line = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.CC1.PK, 100M, TestObjectCreator.AUD, 1M);
			line.AL_JH = job.PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			var taxTransactionSPR = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = apInvoice.PK, TaxSystem = taxSystemSPR, TaxConfiguration = taxConfigSPR, TaxBasis = TaxBasisList.PostingOnMatching.Code, RealisationDate = ZDate.Empty, OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -45M, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxTransactionSPR.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line));

			Factory.Save();

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + TaxTransactionExported_TaxRealized_Batch1;
			using (StreamReader streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}

			var journal = Factory.NewWithValidTestData<APJournal>();
			journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.PaymentBasisWithholding;
			journal.AH_Desc = "Withholding AP Journal";
			journal.AH_TransactionType = TransactionTypes.Journal;
			journal.AH_TransactionNum = "19908";

			taxTransactionSPR.ATT_AH_MatchTransaction = journal.PK;
			taxTransactionSPR.ATT_RealisationDate = ZDate.Today;
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxTransactionSPR.PK, debitAccountPK: taxExpenseAccount.PK, creditAccountPK: apControlAccount.PK, amount: 60M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);
			Factory.Save();

			exportedXml = Export(false);
			xmlFilePath = BaseSourcePath + TaxTransactionExported_TaxRealized_Batch2;
			using (StreamReader streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}
		}

		[TestDate(2010, 5, 26, 0, 0, 0)]
		[SuspendCriticalValidation]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTaxTransaction_ExportTransaction_PostingOnMatchDateRealizationBasis_TaxTransactionCancelled()
		{
			SetupCommonData();
			SetupPeriods();
			SetupTaxConfigurations();

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			var line = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.CC1.PK, 100M, TestObjectCreator.AUD, 1M);
			line.AL_JH = job.PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			var taxTransactionSPR = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = apInvoice.PK, TaxSystem = taxSystemSPR, TaxConfiguration = taxConfigSPR, TaxBasis = TaxBasisList.PostingOnMatching.Code, RealisationDate = ZDate.Empty, OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -45M });
			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxTransactionSPR.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line));

			Factory.Save();

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + TaxTransactionExported_TaxTransactionCancelled_Batch1;
			using (StreamReader streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}

			taxTransactionSPR.IsCancelled = true;
			Factory.Save();

			exportedXml = Export(false);
			xmlFilePath = BaseSourcePath + TaxTransactionExported_TaxTransactionCancelled_Batch2;
			using (StreamReader streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}
		}

		[TestDate(2010, 5, 26, 0, 0, 0)]
		[SuspendCriticalValidation]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTaxTransaction_ExportTransaction_PostingRealizationBasis()
		{
			SetupCommonData();
			SetupPeriods();
			SetupTaxConfigurations();

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			var line = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.CC1.PK, 100M, TestObjectCreator.AUD, 1M);
			line.AL_JH = job.PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			var taxTransactionSLX = TaxFrameworkTestObjectCreator.CreateTaxTransaction(
				new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters
				{
					TransactionHeaderPK = apInvoice.PK,
					TaxSystem = taxSystemSLX,
					TaxConfiguration = taxConfigSLX,
					TaxBasis = TaxBasisList.Posting.Code,
					RealisationDate = ZDate.Today,
					ServiceCode = "100.21",
					OsTaxBaseAmount = 100M,
					OsTaxAmount = 11M,
					LocalTaxBaseAmount = 100M,
					LocalTaxAmount = 11M,
					DoesNotCreateGLMovemetsOnSaving = true
				});
			taxTransactionSLX.ATT_TaxAuthorityServiceCodeDescription = "100.21 Description";

			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxTransactionSLX.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line));
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxTransactionSLX.PK, debitAccountPK: taxExpenseAccount.PK, creditAccountPK: apControlAccount.PK, amount: 11M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);

			Factory.Save();

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + TaxTransactionExported_PostingRealizationBasis;
			using (StreamReader streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}
		}

		[TestDate(2010, 5, 26, 0, 0, 0)]
		[SuspendCriticalValidation]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTaxTransaction_ExportTransaction_MatchingRealizationBasis()
		{
			SetupCommonData();
			SetupPeriods();
			SetupTaxConfigurations();

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			var line = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.CC1.PK, 100M, TestObjectCreator.AUD, 1M);
			line.AL_JH = job.PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			var taxTransactionPER1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = apInvoice.PK, TaxSystem = taxSystemPER, TaxConfiguration = taxConfigPER, TaxBasis = TaxBasisList.Matching.Code, RealisationDate = ZDate.Empty, OsTaxBaseAmount = 200M, LocalTaxBaseAmount = 200M, OsTaxAmount = 4M, LocalTaxAmount = 4M, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxTransactionPER1.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line));
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxTransactionPER1.PK, debitAccountPK: taxPrepaidPendingControlAccount.PK, creditAccountPK: apControlAccount.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Pending.Code);
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxTransactionPER1.PK, debitAccountPK: taxRealisedControlAccount.PK, creditAccountPK: taxPrepaidPendingControlAccount.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Realised.Code);

			Factory.Save();

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + TaxTransactionExported_MatchingRealizationBasis;
			using (StreamReader streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}
		}
		#endregion

		#region OriginalTransactionAmendingReversing

		[SuspendCriticalValidation]
		[TestDate(2020, 12, 11)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTransactionExported_AmendingReversingReason()
		{
			SetupPeriods();
			SetupCommonData();

			Factory.Save();

			CreateARInvoice("IDE", "TranAmending");
			CreateARInvoice("WOR", "TranReversal", true);
			CreateARInvoice("XXX", "TranAmenNoExisCode");
			CreateARInvoice("XXX", "TranReveNoExisCode", true);
			CreateARInvoice(null, "TranNullReceiptType");

			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("1", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
			apInvoice.Lines[0].AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			apInvoice.AH_PostDate = new ZDateTime(2008, 06, 15);
			apInvoice.AH_InvoiceDate = apInvoice.AH_PostDate.AddDays(1);
			apInvoice.AH_ConsolidatedInvoiceRef = "S1";
			apInvoice.AH_TransactionReference = "TranAPWithOutReason";
			apInvoice.AH_ComplianceSubType = "TXI";

			var apInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("1-1", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
			apInvoice1.Lines[0].AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			apInvoice1.AH_PostDate = new ZDateTime(2008, 06, 15);
			apInvoice1.AH_InvoiceDate = apInvoice1.AH_PostDate.AddDays(1);
			apInvoice1.AH_TransactionBelongsToGroup = apInvoice.PK;

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + TransactionExported_AmendingReversingReason;
			using (var streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}

			void CreateARInvoice(string receiptType, string transactionReference, bool isCancelled = false)
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				arInvoice.AH_PostDate = new ZDateTime(2009, 06, 15);
				arInvoice.AH_InvoiceDate = arInvoice.AH_PostDate.AddDays(1);
				arInvoice.AH_ConsolidatedInvoiceRef = "S1";
				arInvoice.AH_TransactionReference = transactionReference;
				arInvoice.AH_ComplianceSubType = "TCR";

				var line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1.0m, 100m, 0m, 0m);
				line.AL_AC = TestObjectCreator.RevenueChargeCode.PK;

				var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("1-1", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				arInvoice1.AH_PostDate = new ZDateTime(2011, 06, 15);
				arInvoice1.AH_InvoiceDate = arInvoice1.AH_PostDate.AddDays(1);
				arInvoice1.AH_TransactionBelongsToGroup = arInvoice.PK;
				arInvoice1.AH_ReceiptType = receiptType;
				arInvoice1.AH_IsCancelled = isCancelled;

				var line2 = TestObjectCreator.CreateInvoiceLine(arInvoice1, TestObjectCreator.AUD, 1.0m, 100m, 0m, 0m);
				line2.AL_AC = TestObjectCreator.RevenueChargeCode.PK;

				Factory.Save();
			}
		}

		#endregion

		[TestDate(2008, 5, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvoiceWithGoodsClassChargeCode()
		{
			SetupCommonData();

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.GoodsClassChargeCode, "charge", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();
			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			apInvoice.Lines.Add(TestObjectCreator.CreateCostLine(charge, apInvoice.PK));
			Factory.Save();

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + InvoiceWithGoodsClassChargeCode;
			using (StreamReader streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}
		}

		[TestDate(2008, 5, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvoiceWithCancelReason()
		{
			SetupCommonData();

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			apInvoice.Lines.Add(TestObjectCreator.CreateCostLine(charge, apInvoice.PK));
			Factory.Save();

			var reverser = new ReversingFactory().NewReversing(apInvoice);
			reverser.Reverse();
			apInvoice.ReverseInvoice.AH_TransactionNum = "REV001";
			apInvoice.ReverseInvoice.AH_ReceiptType = Enterprise.Core.Constants.GenApprovalRequestReasonCode.Code.IncorrectCharges;
			Factory.Save();

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + InvoiceWithCancelReason;
			using (StreamReader streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}
		}

		[TestDate(2008, 5, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvoiceWithAmendingTransactions()
		{
			SetupCommonData();

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "1001", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK));
			Factory.Save();

			TestDateAttribute.AddDays(1);   // Here and below adding a day to avoid random order of WIPs and Accruals created on Amending and Reversing
			var amendingCreditNote = (arInvoice as IAmending)?.GenerateAmendingTransaction<ARCreditNote>();
			AssertNotNull(amendingCreditNote);
			amendingCreditNote.AH_ReceiptType = "TXT";
			AssertEquals(arInvoice.AH_OSTotal, -amendingCreditNote.AH_OSTotal);
			Factory.Save();

			TestDateAttribute.AddDays(1);
			var reverser = new ReversingFactory().NewReversing(amendingCreditNote);
			reverser.Reverse();
			amendingCreditNote.ReverseInvoice.AH_TransactionNum = "REVAMEND1001";
			amendingCreditNote.ReverseInvoice.AH_ReceiptType = "IDE";
			Factory.Save();

			TestDateAttribute.AddDays(1);
			reverser = new ReversingFactory().NewReversing(arInvoice);
			reverser.Reverse();
			arInvoice.ReverseInvoice.AH_TransactionNum = "REVINV1001";
			arInvoice.ReverseInvoice.AH_ReceiptType = "WOR";
			Factory.Save();

			var exportedXml = Export();
			var xmlFilePath = BaseSourcePath + InvoiceWithCancelReasonAndCancelledAmendment;
			using (StreamReader streamReader = File.OpenText(xmlFilePath))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			}
		}

		[TestDate(2008, 5, 15)]
		public void TestRollbackWebExporterTransaction()
		{	
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionWebExporterForTest(dataAccess, commitBehavior: () => throw new Exception());
			
			AssertExceptionThrown(typeof(Exception), () =>
			{
				var response = exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code);
				AssertEquals(false, response.Succeeded);
			});

			AssertEquals("IsTransactionRollbacked is true", true, exporter.IsTransactionRollbacked);
		}

		[TestDate(2024, 1, 15)]
		public void TestCreateBatch_WhenInvalidOperationExceptionThrown_ShouldRollbackTransaction()
		{
			using (var connection = DbAccess.NewConnection())
			using (var transaction = connection.BeginTransaction())
			{
					var dataAccess = new BatchExportDataAccess(connection, transaction);
					var exporter = new AccountingTransactionWebExporterForTest(dataAccess,
						commitBehavior: () => throw new InvalidOperationException(),
						rollbackBehavior: () => dataAccess.Transaction?.Rollback());
	
					Assert("Transaction should be active before CreateBatch is called.", transaction.Connection != null);

					var exception = AssertExceptionThrown<InvalidOperationException>(() => exporter.CreateBatch(GlbCompany.CurrentCompany.GC_Code));
					AssertEquals("IsTransactionRollbacked is true", true, exporter.IsTransactionRollbacked);
					AssertNotEquals("This SqlTransaction has completed; it is no longer usable.", exception.Message);
			}
		}

		#endregion
	}

	sealed class AccountingTransactionWebExporterForTest : AccountingTransactionWebExporter
	{
		readonly Action CommitBehavior;
		readonly Action RollbackBehavior;

		public AccountingTransactionWebExporterForTest(BatchExportDataAccess dataAccess,
				Action commitBehavior = null,
				Action rollbackBehavior = null)
			: base(dataAccess)
		{
			CommitBehavior = commitBehavior ?? (() => { IsTransactionCommitted = true; });
			RollbackBehavior = rollbackBehavior ?? (() => { IsTransactionRollbacked = true; });
		}

		protected override void CommitTransaction(System.Data.Common.DbTransaction transaction)
		{
			IsTransactionCommitted = true;
			CommitBehavior?.Invoke();
		}

		protected override (System.Data.Common.DbTransaction Transcation, IDisposable DisposeAction) CreateTransactionInfo(BatchExportDataAccess dataAccess)
		{
			return (dataAccess.Transaction, new DisposableAction(() =>
			{
				IsTransactionRollbacked = true;
				RollbackBehavior?.Invoke();
			}));
		}

		protected override DateTime GetUtcNow(System.Data.Common.DbTransaction transaction)
		{
			return (NUnit.Framework.TestDateAttribute.IsActive) ?
				NUnit.Framework.TestDateAttribute.Date : DateTime.UtcNow;
		}

		public bool IsTransactionCommitted { get; private set; }
		public bool IsTransactionRollbacked { get; private set; }
	}

	[Serializable]
	sealed class AccountingTransactionCreateBatchRequestForReportableException : AccountingTransactionCreateBatchRequest
	{
		public override string Validate()
		{
			try
			{
				var num = int.Parse("This is the inner exception.");
			}
			catch (Exception innerEx)
			{
				try
				{
					var openLog = File.Open("FileDoesNotExist", FileMode.Open);
				}
				catch
				{
					throw new FileNotFoundException("This is the outer exception.", innerEx);
				}
			}

			return string.Empty;
		}
	}
}
