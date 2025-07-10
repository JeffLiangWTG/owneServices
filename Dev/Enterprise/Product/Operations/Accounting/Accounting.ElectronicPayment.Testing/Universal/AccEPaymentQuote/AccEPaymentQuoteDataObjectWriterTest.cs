using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicPayment.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using ProviderCodes = Enterprise.MasterFiles.Business.EPaymentProviderCodes.Codes;

namespace Enterprise.Accounting.ElectronicPayment.Testing.Universal
{
	public class AccEPaymentQuoteDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestPopulateDataObject()
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

			var writer = new AccEPaymentQuoteDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, quote)));
			var quoteDataObject = writer.GetDataObject(quote);
			AssertNotNull(quoteDataObject);

			AssertEquals(1, quoteDataObject.DataContext.DataSourceCollection.Count());
			IDataSourceDataObject dataSource = quoteDataObject.DataContext.DataSourceCollection.FirstOrDefault();
			AssertEquals("00001234", dataSource.Key);
			AssertEquals("AccEPaymentQuote", dataSource.Type);

			AssertEquals("00001234", quoteDataObject.TransactionReference);
			AssertEquals("AUD", quoteDataObject.LocalCurrency.Code);
			AssertEquals("Australian Dollar", quoteDataObject.LocalCurrency.Description);
			AssertEquals(0m, quoteDataObject.LocalTotal);
			AssertEquals("USD", quoteDataObject.OSCurrency.Code);
			AssertEquals("United States Dollar", quoteDataObject.OSCurrency.Description);
			AssertEquals(1000m, quoteDataObject.OSTotal);
			AssertEquals("AP", quoteDataObject.Ledger);
			AssertEquals(todaysDate.AddDays(-1), quoteDataObject.PostDate);
			AssertEquals(todaysDate.AddDays(1), quoteDataObject.TransactionDate);
			AssertEquals(UniversalDataBuss.DataObjects.Accounting.TransactionType.PAY, quoteDataObject.TransactionType);
			AssertEquals("Paying FreightQuota Invoice 83942", quoteDataObject.Description);
			AssertEquals(UniversalDataBuss.DataObjects.Accounting.PaymentOrReceiptType.CHQ, quoteDataObject.PaymentOrReceiptType);
			AssertEquals("00009283", quoteDataObject.CheckNumberOrPaymentRef);
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, quoteDataObject.Branch.Code);
			AssertEquals(GlbBranch.CurrentBranch.GB_BranchName, quoteDataObject.Branch.Name);
			AssertEquals(testObjectCreator.AUDBankAccount.AB_Code, quoteDataObject.BankAccount);
		}
	}
}