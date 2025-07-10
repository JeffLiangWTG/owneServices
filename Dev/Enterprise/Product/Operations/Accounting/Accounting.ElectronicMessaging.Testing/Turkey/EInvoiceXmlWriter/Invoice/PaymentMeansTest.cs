using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.Export.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	public class PaymentMeansTest : TestCaseWithFactory
	{
		readonly TurkeyEInvoiceTestHelper Helper = new TurkeyEInvoiceTestHelper();

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestPaymentMeans()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Helper.CreateTestARInvoiceBatch(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.KDV18.PK.ToGuid(), "A00003", 1000m, Helper.TestObjectCreator.TRY, hasPayment: true);
				var exportor = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var transactionBatch = exportor.CreateTransactionBatch(invoice, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					AssertEquals(1, eInvoice.Invoice.PaymentMeans.Length);
					var paymentMeans = eInvoice.Invoice.PaymentMeans[0];
					AssertEquals("ZZZ", paymentMeans.PaymentMeansCode.Value);
					AssertEquals(Convert.ToDateTime("29/01/2020 13:08:00.0000000"), paymentMeans.PaymentDueDate.Value);
					AssertNotNull(paymentMeans.PayeeFinancialAccount.ID.Value);
					AssertEquals("TR6211111111111111", paymentMeans.PayeeFinancialAccount.ID.Value);
					AssertNotNull(paymentMeans.PayeeFinancialAccount.PaymentNote.Value);
					AssertEquals("HSBC AUD ACCT", paymentMeans.PayeeFinancialAccount.PaymentNote.Value);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			var transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
			DataAccess = new BatchExportDataAccess(connection, transaction);
		}

		BatchExportDataAccess DataAccess;
	}
}
