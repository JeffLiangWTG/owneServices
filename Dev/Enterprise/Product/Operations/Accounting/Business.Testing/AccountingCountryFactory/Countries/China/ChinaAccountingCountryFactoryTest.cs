using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.China.Testing
{
	public class ChinaAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestCanOverrideTransactionLineSequence()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingChina(DateTime.Now.AddDays(-30)))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1999", TestObjectCreator.AUD, 1.0M, TestObjectCreator.ABIGAS);
				var line1 = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.CC4, TestObjectCreator.AUD, 1.0M, "hello", -100);
				line1.AL_AT = TestObjectCreator.ServiceTax.PK;
				var line2 = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.CC4, TestObjectCreator.AUD, 1.0M, "hello", 200);
				line2.AL_AT = TestObjectCreator.ServiceTax.PK;

				var overrideTransactionLineSequenceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.China) as IOverrideTransactionLineSequenceProvider;
				var result = overrideTransactionLineSequenceProvider.CanOverrideTransactionLineSequence(invoice);

				Assert(result);

				line1.AL_AT = Guid.Empty;
				line2.AL_AT = Guid.Empty;
				AssertEquals("Invoice don't have line with valid AL_AT.", false, invoice.Lines.Any(x => ((InvoicingLineBase)x).AL_AT.IsValid));

				result = overrideTransactionLineSequenceProvider.CanOverrideTransactionLineSequence(invoice);
				AssertEquals("When invoice don't have line with valid AL_AT, CanOverrideTransactionLineSequence is false.", false, result);

				line1.AL_AT = TestObjectCreator.ServiceTax.PK;
				line2.AL_AT = TestObjectCreator.ServiceTax.PK;
				AssertEquals("Invoice have line with valid AL_AT.", true, invoice.Lines.Any(x => ((InvoicingLineBase)x).AL_AT.IsValid));

				line1.AL_OSExTaxAmount = 100;
				AssertEquals("Invoice don't have line with negative amount.", false, invoice.Lines.Any(x => ((InvoicingLineBase)x).AL_LineAmount < 0));

				result = overrideTransactionLineSequenceProvider.CanOverrideTransactionLineSequence(invoice);
				AssertEquals("When invoice don't have line with negative amount, CanOverrideTransactionLineSequence is false.", false, result);

				line1.AL_OSExTaxAmount = -200;
				AssertEquals("Invoice have line with negative amount.", true, invoice.Lines.Any(x => ((InvoicingLineBase)x).AL_LineAmount < 0));
				AssertEquals("Invoice amount is empty.", true, invoice.AH_LocalTotal.IsEmpty);

				result = overrideTransactionLineSequenceProvider.CanOverrideTransactionLineSequence(invoice);
				AssertEquals("When invoice amount is empty, CanOverrideTransactionLineSequence is false.", false, result);

				line1.AL_OSExTaxAmount = -100;
				AssertEquals("Invoice amount is not empty.", false, invoice.AH_LocalTotal.IsEmpty);

				using (AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, string.Empty))
				{
					AssertEquals("Credential is empty", string.Empty, AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.GetFallBackValueAtAllLevels(invoice.Company.PK.ToGuid(), invoice.Branch.PK.ToGuid(), Guid.Empty));
					result = overrideTransactionLineSequenceProvider.CanOverrideTransactionLineSequence(invoice);
					AssertEquals("When credential is empty, CanOverrideTransactionLineSequence is false.", false, result);
				}

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("EnableEInvoicingFunctionality is false", false, AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
					result = overrideTransactionLineSequenceProvider.CanOverrideTransactionLineSequence(invoice);
					AssertEquals("When EnableEInvoicingFunctionality is false, CanOverrideTransactionLineSequence is false.", false, result);
				}

				using (AccountingMasterFilesRegistry.Instance.AlwaysTransmitNegativeChargesAsDiscount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("AlwaysTransmitNegativeChargesAsDiscount is false", false, AccountingMasterFilesRegistry.Instance.AlwaysTransmitNegativeChargesAsDiscount.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
					result = overrideTransactionLineSequenceProvider.CanOverrideTransactionLineSequence(invoice);
					AssertEquals("When AlwaysTransmitNegativeChargesAsDiscount is false, CanOverrideTransactionLineSequence is false.", false, result);
				}
			}
		}

		public void TestIQueueInvoiceForTransmissionProvider()
		{
			var provider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.China) as IInstanceProvider<IQueueInvoiceForTransmissionProvider>).Get();

			AssertEquals(true, provider is ChinaQueueInvoiceForTransmissionProvider);
		}

		public void TestIEInvoicingEligibilityDecider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingEligibilityDecider>)?.Get();
			AssertNotNull(obj);
		}

		public void TestIGLJournalTypesProvider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IGLJournalTypesProvider>)?.Get();
			AssertNotNull(obj);
		}

		static IAccountingCountryFactory GetCountryFactoryForThisCountry()
			=> ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.China);

		TestObjectCreator testObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
	}
}
