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
using static Enterprise.MasterFiles.Business.CountryCompliance.TurkeyComplianceInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	public class PricingExchangeRateTest : TestCaseWithFactory
	{
		readonly TurkeyEInvoiceTestHelper Helper = new TurkeyEInvoiceTestHelper();

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestPricingExchangeRateForeignCurrency()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.EUR, "AR001", ComplianceSubTypeCodes.EAR);
				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);
				var exportor = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					var pricingExchangeRate = eInvoice.Invoice.PricingExchangeRate;
					AssertEquals("EUR", pricingExchangeRate.SourceCurrencyCode.Value);
					AssertEquals("TRY", pricingExchangeRate.TargetCurrencyCode.Value);
					AssertEquals(6m, pricingExchangeRate.CalculationRate.Value);
					AssertEquals(Convert.ToDateTime("29/01/2020"), pricingExchangeRate.Date.Value);
				}
			}
		}

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestPricingExchangeRateLocalCurrency()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.TRY, "AR001", ComplianceSubTypeCodes.EAR);
				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);
				var exportor = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					var pricingExchangeRate = eInvoice.Invoice.PricingExchangeRate;
					AssertNull(pricingExchangeRate);
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
