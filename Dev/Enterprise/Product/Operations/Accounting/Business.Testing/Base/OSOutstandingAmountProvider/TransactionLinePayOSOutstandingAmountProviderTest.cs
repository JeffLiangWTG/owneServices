using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public class TransactionLinePayOSOutstandingAmountProviderTest : TestCaseWithFactory
	{
		public void TestSetPaymentAmounts()
		{
			AssertSetPaymentAmounts(isRegistryEnabled: false);
			AssertSetPaymentAmounts(isRegistryEnabled: true);

			void AssertSetPaymentAmounts(bool isRegistryEnabled)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled);
				var header = GetHeader();
				var line = header.Lines.AddNew() as InvoicingLineBase;
				var linePay = line.TransLinePays.AddNew();

				TransactionLinePayOSOutstandingAmountProvider.SetPaymentAmounts(header, linePay, 100m, 200m);

				AssertEquals(100m, linePay.A7_Amount);
				AssertEquals(isRegistryEnabled ? 200m : 0m, linePay.A7_OSAmount);
			}
		}

		public void TestGetTransLinePaysTotalOSAmount()
			=> AssertTransLinePaysTotalOSAmount(false);

		public void TestGetTransLinePaysTotalOSAmount_EnableNewOSOutstandingAmountFeature()
			=> AssertTransLinePaysTotalOSAmount(true);

		void AssertTransLinePaysTotalOSAmount(bool isRegistryEnabled)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled);
			var header = GetHeader();
			var line = header.Lines.AddNew() as InvoicingLineBase;
			line.AL_OSAmount = 1200m;
			line.AL_LocalExTaxAmount = 1000m;

			var linePay = line.TransLinePays.AddNew();
			linePay.A7_Amount = 1000m;
			linePay.A7_OSAmount = 1200m;

			var lineMatching = line as ILineMatching;
			lineMatching.SetDefaultValues();

			AssertEquals(1200m, TransactionLinePayOSOutstandingAmountProvider.GetTransLinePaysTotalOSAmount(line));
		}

		InvoicingBase GetHeader()
		{
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m,
				1200m, 0m, 1000m, 0m);
			header.AH_IsOSOutstandingAmountApplicable = true;
			header.AH_OutstandingAmount = 500m;
			header.AH_OSOutstandingAmount = 1000m;
			return header;
		}

		#region Implementation

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
