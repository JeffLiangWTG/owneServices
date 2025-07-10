using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicPayment.Universal;
using Enterprise.Accounting.Export.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using ProviderCodes = Enterprise.MasterFiles.Business.EPaymentProviderCodes.Codes;

namespace Enterprise.Accounting.ElectronicPayment.Testing.Universal
{
	class AccEPaymentQuoteExporterTest : TestCaseWithFactory
	{
		public void TestCreateUniversalTransaction()
		{
			var todaysDate = ZDateTime.Now;
			var testObjectCreator = new TestObjectCreator(Factory);

			var paymentApproval = testObjectCreator.CreatePaymentApproval(ReceiptTypes.Cheque, testObjectCreator.AUDBankAccount, testObjectCreator.AUDChequeBook);
			paymentApproval.AV_Amount = 1000m;
			paymentApproval.AV_RX_NKPaymentCurrency = "USD";
			paymentApproval.AV_PostDate = todaysDate.AddDays(-1);
			paymentApproval.AV_PaymentDate = todaysDate.AddDays(1);
			paymentApproval.AV_PaymentComment = "Paying FreightQuota Invoice 83942";
			paymentApproval.AV_ChequeOrReference = "00009283";
			paymentApproval.AV_GB = GlbBranch.CurrentBranch.PK;
			paymentApproval.AV_GC = GlbCompany.CurrentCompany.PK;

			var quote = testObjectCreator.CreateEPaymentQuote(paymentApproval, ProviderCodes.OFX);
			quote.QU_InternalReference = "00001234";

			var connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			var transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
			var dataAccess = new BatchExportDataAccess(connection, transaction);

			var exporter = new AccEPaymentQuoteExporter();
			var universalTransaction = exporter.CreateUniversalTransaction(dataAccess, quote);

			AssertEquals(1, universalTransaction.DataContext.DataSourceCollection.Count());
			IDataSourceDataObject dataSource = universalTransaction.DataContext.DataSourceCollection.FirstOrDefault();
			AssertEquals("00001234", dataSource.Key);
			AssertEquals("AccEPaymentQuote", dataSource.Type);

			AssertEquals("00001234", universalTransaction.TransactionReference);
			AssertEquals("AUD", universalTransaction.LocalCurrency.Code);
			AssertEquals("Australian Dollar", universalTransaction.LocalCurrency.Description);
			AssertEquals(0m, universalTransaction.LocalTotal);
			AssertEquals("USD", universalTransaction.OSCurrency.Code);
			AssertEquals("United States Dollar", universalTransaction.OSCurrency.Description);
			AssertEquals(1000m, universalTransaction.OSTotal);
			AssertEquals("AP", universalTransaction.Ledger);
			AssertEquals(todaysDate.AddDays(-1), universalTransaction.PostDate);
			AssertEquals(todaysDate.AddDays(1), universalTransaction.TransactionDate);
			AssertEquals(UniversalDataBuss.DataObjects.Accounting.TransactionType.PAY, universalTransaction.TransactionType);
			AssertEquals("Paying FreightQuota Invoice 83942", universalTransaction.Description);
			AssertEquals(UniversalDataBuss.DataObjects.Accounting.PaymentOrReceiptType.CHQ, universalTransaction.PaymentOrReceiptType);
			AssertEquals("00009283", universalTransaction.CheckNumberOrPaymentRef);
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, universalTransaction.Branch.Code);
			AssertEquals(GlbBranch.CurrentBranch.GB_BranchName, universalTransaction.Branch.Name);
			AssertEquals(testObjectCreator.AUDBankAccount.AB_Code, universalTransaction.BankAccount);
		}
	}
}