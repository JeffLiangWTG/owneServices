using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.China.Testing
{
	[TestedType(typeof(ChinaQueueInvoiceForTransmissionProvider))]
	public class ChinaQueueInvoiceForTransmissionProviderTest : TestCaseWithFactory
	{
		public void TestShouldShowQueueInvoiceForTransmissionMenuItem()
		{
			AssertEquals(false, QueueInvoiceForTransmissionProvider.ShouldShowQueueInvoiceForTransmissionMenuItem(LedgerTypes.AccountsReceivable));

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(true, QueueInvoiceForTransmissionProvider.ShouldShowQueueInvoiceForTransmissionMenuItem(LedgerTypes.AccountsReceivable));
				AssertEquals(false, QueueInvoiceForTransmissionProvider.ShouldShowQueueInvoiceForTransmissionMenuItem(LedgerTypes.JobCosting));
			}
		}

		public void TestShouldQueueTransactionForTransmission()
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1999", TestObjectCreator.AUD, 1.0M, TestObjectCreator.ABIGAS);
			invoice.AH_ComplianceSubType = "ABC";

			AssertEquals(false, invoice.EInvoicingProxy.IsEligibleToCreatePivot());
			AssertEquals(false, QueueInvoiceForTransmissionProvider.ShouldQueueTransactionForTransmission(invoice));
		}

		public void TestAdditionalErrorMessage()
		{
			AssertEquals(@"* Transaction Header Branch Does not have a value recorded against 'E-Invoicing Credentials' under Accounting > E-Reporting and E-Invoicing Configurations > China registry.
* Transaction Type is not INV.
* Compliance Sub Type is blank.
* E-Reporting Status is not blank.
* Compliance Number/Date is not blank.", QueueInvoiceForTransmissionProvider.GetAdditionalErrorMessage());
		}

		protected override void SetUp()
		{
			QueueInvoiceForTransmissionProvider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.China) as IInstanceProvider<IQueueInvoiceForTransmissionProvider>).Get();

			AssertNotNull(QueueInvoiceForTransmissionProvider);
		}

		IQueueInvoiceForTransmissionProvider QueueInvoiceForTransmissionProvider;

		TestObjectCreator testObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
	}
}
