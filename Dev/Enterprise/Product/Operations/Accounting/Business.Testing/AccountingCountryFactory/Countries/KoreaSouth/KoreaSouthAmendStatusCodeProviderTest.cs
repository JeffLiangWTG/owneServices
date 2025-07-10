using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public sealed class KoreaSouthAmendStatusCodeProviderTest : TestCaseWithFactory
	{
		public void TestIsSupportAmendStatusCode()
		{
			var instanceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.KoreaSouth) as IInstanceProvider<IAmendStatusCodeProvider>;
			var provider = instanceProvider.Get();
			var arCreditNote = TestObjectCreator.CreateARCreditNote("0001", TestObjectCreator.ActiveOrg);
			var arInvoicing = TestObjectCreator.CreateARInvoice<ARInvoice>("00002", TestObjectCreator.AUD, 0.5m, TestObjectCreator.Debtor);
			var amendARInvoice = CreateAmendAR();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var isSupportAmendStatusCode = provider.IsSupportAmendStatusCode(amendARInvoice);
				AssertEquals(false, isSupportAmendStatusCode);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.KoreaSouth))
			{
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var isSupportAmendStatusCode = provider.IsSupportAmendStatusCode(amendARInvoice);
				AssertEquals(true, isSupportAmendStatusCode);

				isSupportAmendStatusCode = provider.IsSupportAmendStatusCode(arCreditNote);
				AssertEquals(true, isSupportAmendStatusCode);

				isSupportAmendStatusCode = provider.IsSupportAmendStatusCode(arInvoicing);
				AssertEquals(false, isSupportAmendStatusCode);
			}
		}

		public void TestShouldShowAmendStatusCode()
		{
			var instanceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.KoreaSouth) as IInstanceProvider<IAmendStatusCodeProvider>;
			var provider = instanceProvider.Get();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.KoreaSouth))
			{
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AssertEquals(true, provider.ShouldShowAmendStatusCode());

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertEquals(false, provider.ShouldShowAmendStatusCode());
			}
		}

		public void TestAmendStatusCodeReferenceType()
		{
			var instanceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.KoreaSouth) as IInstanceProvider<IAmendStatusCodeProvider>;
			var provider = instanceProvider.Get();
			AssertEquals(AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.KRE, provider.AmendStatusCodeReferenceType);
		}

		public void TestAmendStatusCodeList()
		{
			var instanceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.KoreaSouth) as IInstanceProvider<IAmendStatusCodeProvider>;
			var provider = instanceProvider.Get();
			var amendStatusCodeList = provider.AmendStatusCodeList;

			var statusCodeFromRegistry = AccountingConfigurationRegistry.Instance.KoreaEInvoicingAmendmentStatusCode.Value;
			for (var i = 0; i < statusCodeFromRegistry.Count; i++)
			{
				AssertEquals(statusCodeFromRegistry[i].Code, amendStatusCodeList[i].Code);
			}
			AssertEquals(statusCodeFromRegistry.Count, amendStatusCodeList.Count);
		}

		ARInvoice CreateAmendAR()
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00002", TestObjectCreator.AUD, 0.5m, TestObjectCreator.Debtor);
			var amendingAR = (invoice as IAmending).GenerateAmendingTransaction(invoice.AH_TransactionType) as ARInvoice;
			AssertEquals("PreCondition", true, amendingAR.IsAmendingTransaction);

			return amendingAR;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);

		TestObjectCreator testObjectCreator;
	}
}
