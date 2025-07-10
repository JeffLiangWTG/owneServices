using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	using System;
	using Core;
	using Enterprise.Environment;
	using NUnit.Framework;

	[TestedType(typeof(ChinaJournalCollection))]
	public class ChinaJournalCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ChinaJournalCollection>
	{
		public void TestBuildChinaJournalCollection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
				using (GlbCompany.CurrentCompany.TemporarilySetCurrency(Core.Constants.CurrencyCodes.Australia))
				{
					var testHelper = new AccountingPeriodTestHelper(Factory);
					testHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31));
					var apControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
					var arControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
					var arControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
					arControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
					arControlLocal.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
					arControlLocal.ParentGLHeaderPK = arControl.PK;
					arControlLocal.AJ_LocalAccountNumber = "ARControlAccount";
					arControlLocal.AJ_AccountDescription = "ARControlDescription";
					var apControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
					apControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
					apControlLocal.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
					apControlLocal.ParentGLHeaderPK = apControl.PK;
					apControlLocal.AJ_LocalAccountNumber = "APControlAccount";
					apControlLocal.AJ_AccountDescription = "APControlDescription";
					AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apControl.PK.ToGuid());
					AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arControl.PK.ToGuid());
					var testArInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
					testArInvoice.AH_TransactionNum = "100111";
					testArInvoice.AH_OH = new TestObjectCreator(Factory).AALSHI.PK;
					//
					testArInvoice.AH_PostDate = new ZDateTime(2006, 3, 13);
					testArInvoice.AH_DueDate = new ZDateTime(2006, 3, 18);
					testArInvoice.AH_TransactionReference = "Invoice No";
					testArInvoice.AH_ExchangeRate = 1m;
					testArInvoice.AH_InvoiceAmount = 111m;
					testArInvoice.AH_OutstandingAmount = 111m;
					testArInvoice.AH_Desc = "Invoice Desc";
					testArInvoice.AH_NumberOfSupportingDocuments = 0;
					Factory.Save();
					var collection = new ChinaJournalCollection(Factory);
					collection.AddElements(new ZDateTime(2006, 3, 1, 23, 59, 00), new ZDateTime(2006, 4, 1, 00, 00, 00), ZString.Empty);
					AssertEquals(1, collection.Count);
					ChinaJournal chinaJournal = collection[0];
					AssertEquals(chinaJournal.VoucherDate, "20060313");
					AssertEquals(chinaJournal.FinancialYear, 2006);
					AssertEquals(chinaJournal.Period, 200603);
					AssertEquals(chinaJournal.VoucherTypeNumber, "1");
					AssertEquals(chinaJournal.VoucherNumber, "ARINV0603001000");
					AssertEquals(chinaJournal.VoucherLineNumber, "1");
					AssertEquals(chinaJournal.VoucherDescription, "营业收入 - Invoice Desc");
					AssertEquals(chinaJournal.GLAccountNumber, "ARControlAccount");
					AssertEquals(chinaJournal.CurrencyCode, "AUD");
					AssertEquals(chinaJournal.DebitCurrencyAmount, 0m);
					AssertEquals(chinaJournal.DebitAmountLocalCurrency, 0m);
					AssertEquals(chinaJournal.CreditCurrencyAmount, 0m);
					AssertEquals(chinaJournal.CreditAmountLocalCurrency, 0m);
					AssertEquals(chinaJournal.ExRateTypeNumber, "1");
					AssertEquals(chinaJournal.ExRate, 1m);
					AssertEquals(chinaJournal.PaymentTypeCode, "");
					AssertEquals(chinaJournal.VoucherType, "ARINV");
					AssertEquals(chinaJournal.VoucherDocNumber, "Invoice No");
					AssertEquals(chinaJournal.VoucherDocDate, "20060313");
					AssertEquals(chinaJournal.Attachments, 0);
					AssertEquals(chinaJournal.Reviwer, "");
					AssertEquals(chinaJournal.EnteredBy, Env.CurrentUser.FullName);
					AssertEquals(chinaJournal.Cashier, "");
					AssertEquals(chinaJournal.AccountingFlag, 1);
					AssertEquals(chinaJournal.VoidFlag, 0);
				}
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ChinaJournal();
		}

		protected override ChinaJournalCollection GetCollectionToTest()
		{
			var collection = new ChinaJournalCollection(Factory);
			collection.AddElements(new ZDateTime(), new ZDateTime(), ZString.Empty);
			return collection;
		}
	}
}
