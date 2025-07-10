using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingIServices;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing
{
	public class AccountingNumberFountainWrapperTestCase : TestCaseWithFactory
	{
		#region Implementation

		public AccountingNumberFountainWrapperTestCase()
		{
			PeriodTestHelper = new AccountingPeriodTestHelper();
		}

		protected override void SetUp()
		{
			base.SetUp();
			PeriodTestHelper.PostPeriodsForEntireYear(2004, GlbCompany.CurrentCompany.PK);
			CargoWise.Data.Db.Connection.BeginTransaction();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		protected override void TearDown()
		{
			CargoWise.Data.Db.Connection.RollbackTransaction();
			base.TearDown();
		}

		protected AccountingPeriodTestHelper PeriodTestHelper;
		protected TestObjectCreator TestObjectCreator;

		#endregion

		#region TestGetNextForChina

		public void TestGetNextForChina()
		{
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);

			AccountingNumberFountainWrapper accFountain = new AccountingNumberFountainWrapper(Env.NumberFountains.APInvoiceNo, NumberFountainType.APInvoiceInternalReference);
			AssertEquals("GetNext for 200410", "0410001000", accFountain.Generate(new AccountingNumberFountainDataSourceForTest(Factory, new ZDateTime(2004, 4, 2), GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment)));
			AssertEquals("GetNext for 200410", "0410001001", accFountain.Generate(new AccountingNumberFountainDataSourceForTest(Factory, new ZDateTime(2004, 4, 2), GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment)));
			AssertEquals("GetNext for 200411", "0411001000", accFountain.Generate(new AccountingNumberFountainDataSourceForTest(Factory, new ZDateTime(2004, 5, 2), GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment)));
			AssertEquals("GetNext for 200412", "0412001000", accFountain.Generate(new AccountingNumberFountainDataSourceForTest(Factory, new ZDateTime(2004, 6, 2), GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment)));
			AssertEquals("GetNext for 200410", "0410001002", accFountain.Generate(new AccountingNumberFountainDataSourceForTest(Factory, new ZDateTime(2004, 4, 2), GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment)));
			AssertEquals("GetNext for 200411", "0411001001", accFountain.Generate(new AccountingNumberFountainDataSourceForTest(Factory, new ZDateTime(2004, 5, 2), GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment)));
			AssertEquals("GetNext for 200412", "0412001001", accFountain.Generate(new AccountingNumberFountainDataSourceForTest(Factory, new ZDateTime(2004, 6, 2), GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment)));

			//Another instance of the fountain SHOULD NOT continue the numbering
			accFountain = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice);
			AssertEquals("GetNext for 200412 for ARInvoiceNo should not continue the numbering", "0412001000", accFountain.Generate(new AccountingNumberFountainDataSourceForTest(Factory, new ZDateTime(2004, 6, 2), GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment)));
			AssertEquals("GetNext for 200412 for ARInvoiceNo should not continue the numbering", "0410001000", accFountain.Generate(new AccountingNumberFountainDataSourceForTest(Factory, new ZDateTime(2004, 4, 2), GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment)));

			GlbCompany.CurrentCompany.SetCountry(oldCountry);
		}

		public void TestPeekPreliminaryForChina()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);

			AccountingNumberFountainWrapper accFountain = new AccountingNumberFountainWrapper(Env.NumberFountains.APInvoiceNo, NumberFountainType.APInvoiceInternalReference);
			AssertEquals("GetNext Prelimary for 200410", "0410001000", accFountain.PeekPreliminary(Factory, "200410"));
			AssertEquals("GetNext Prelimary for 200410", "0410001000", accFountain.PeekPreliminary(Factory, "200410"));
			AssertEquals("GetNext Prelimary for 200411", "0411001000", accFountain.PeekPreliminary(Factory, "200411"));
			AssertEquals("GetNext Prelimary for 200412", "0412001000", accFountain.PeekPreliminary(Factory, "200412"));
			AssertEquals("GetNext Prelimary for 200410", "0410001000", accFountain.PeekPreliminary(Factory, "200410"));
			AssertEquals("GetNext Prelimary for 200411", "0411001000", accFountain.PeekPreliminary(Factory, "200411"));
			AssertEquals("GetNext Prelimary for 200412", "0412001000", accFountain.PeekPreliminary(Factory, "200412"));
		}

		#endregion

		#region TestGetNextForAustralia

		public void TestGetNextForAustralia()
		{
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			AccountingNumberFountainWrapper fountain1 = new AccountingNumberFountainWrapper(Env.NumberFountains.APInvoiceNo, NumberFountainType.APInvoiceInternalReference);
			AssertEquals("GetNext Fountain1", "00001000", fountain1.Generate(new AccountingNumberFountainDataSourceForTest(Factory, ZDateTime.Empty, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment)));
			AssertEquals("GetNext Fountain1", "00001001", fountain1.Generate(new AccountingNumberFountainDataSourceForTest(Factory, ZDateTime.Empty, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment)));
			AssertEquals("GetNext Fountain1", "00001002", fountain1.Generate(new AccountingNumberFountainDataSourceForTest(Factory, new ZDateTime(2004, 4, 2), GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment)));

			AccountingNumberFountainWrapper fountain2 = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.APInvoiceInternalReference);
			AssertEquals("GetNext Fountain2", "00001000", fountain2.Generate(new AccountingNumberFountainDataSourceForTest(Factory, ZDateTime.Empty, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment)));
			AssertEquals("GetNext Fountain2", "00001001", fountain2.Generate(new AccountingNumberFountainDataSourceForTest(Factory, ZDateTime.Empty, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment)));
			AssertEquals("GetNext Fountain2", "00001002", fountain2.Generate(new AccountingNumberFountainDataSourceForTest(Factory, new ZDateTime(2004, 4, 2), GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment)));
			AssertEquals("GetNext Fountain1", "00001003", fountain1.Generate(new AccountingNumberFountainDataSourceForTest(Factory, new ZDateTime(2004, 4, 2), GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment)));

			GlbCompany.CurrentCompany.SetCountry(oldCountry);
		}

		public void TestPeekPreliminaryForAustralia()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			AccountingNumberFountainWrapper fountain1 = new AccountingNumberFountainWrapper(Env.NumberFountains.APInvoiceNo, NumberFountainType.APInvoiceInternalReference);
			AssertEquals("GetNext Fountain1", "00001000", fountain1.PeekPreliminary(Factory));
			AssertEquals("GetNext Fountain1", "00001000", fountain1.PeekPreliminary(Factory));
			AssertEquals("GetNext Fountain1", "00001000", fountain1.PeekPreliminary(Factory, "200410"));
		}

		#endregion

		#region Test Generate

		[TestDate(2016, 10, 20)]
		public void TestGenerateNumber()
		{
			var testBranch = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "TST");
			var postDate = new ZDateTime(2016, 10, 20);
			PeriodTestHelper.SetupSinglePeriod(201701, new ZDateTime(2016, 10, 1), new ZDateTime(2016, 10, 30));
			Factory.Save();

			var wrapper = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice);

			var collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("00001000", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber);
			collection[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Length = 7;
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("0001001", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderBranchCode);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Include = false;
				string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("TST", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderDepartmentCode);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Include = false;
				string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("FES", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.CustomElement1);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Include = false;
				string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("ABC", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.CustomElement2);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Include = false;
				string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("XYZ", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.YearAsDigits);
			collection[TransactionNumberSequenceCustomisation.ElementNames.YearAsDigits].Code = "1";
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Include = false;
				string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("6", result);
			}
			collection[TransactionNumberSequenceCustomisation.ElementNames.YearAsDigits].Code = "2";
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Include = false;
				string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("16", result);
			}
			collection[TransactionNumberSequenceCustomisation.ElementNames.YearAsDigits].Code = "4";
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Include = false;
				string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("2016", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.YearAsLetter);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Include = false;
				string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("P", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.MonthAs2Digits);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Include = false;
				string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("10", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.MonthAsLetter);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Include = false;
				string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("J", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits);
			collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits].Code = "1";
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Include = false;
				string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("7", result);
			}
			collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits].Code = "2";
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Include = false;
				string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("17", result);
			}
			collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits].Code = "4";
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Include = false;
				string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("2017", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsLetter);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Include = false;
				string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("Q", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Include = false;
				string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("01", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.TransactionTypePrefix);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				var prefixCollection = new TransactionTypePrefixCollection();
				var prefix = prefixCollection.AddNew();
				prefix.Ledger = "AP";
				prefix.TransactionType = "DSC";
				prefix.Prefix = "XX";
				using (AccountingConfigurationRegistry.Instance.TransactionTypePrefix.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, prefixCollection))
				{
					AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Include = false;
					var apDsc = TestObjectCreator.CreateAPDiscount(20m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
					var result = wrapper.Generate(apDsc);

					AssertEquals("XX", result);
				}
			}
		}

		public void TestGenerateWithNullDataSource()
		{
			var wrapper = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice);

			AssertExceptionThrown<ArgumentNullException>(() => wrapper.Generate(null));
		}

		[TestDate(2016, 10, 20)]
		public void TestGenerateNumber_ForTaxAndNonTaxRelatedTransactions()
		{
			PeriodTestHelper.SetupSinglePeriod(201701, new ZDateTime(2016, 10, 1), new ZDateTime(2016, 10, 30));
			Factory.Save();

			var invoiceWithTax = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor, ZDateTime.Today);
			var lineWithTax = TestObjectCreator.CreateInvoiceLine(invoiceWithTax, TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			lineWithTax.AL_AT = TestObjectCreator.GST1.PK;

			var notReportableTaxRate = TestObjectCreator.CreateTaxRate("NOT", "Not reportable", AccTaxRate.Types.NotReportable, 0, ZString.Empty, 0, 1);
			var invoiceWithoutTax = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor, ZDateTime.Today);
			var lineWithoutTax = TestObjectCreator.CreateInvoiceLine(invoiceWithoutTax, TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			lineWithoutTax.AL_AT = notReportableTaxRate.PK;

			var wrapper = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice);

			ZString result = ZString.Empty;

			var collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.CustomElement1, false); //Added this so that we get a generated number fountain as opposed to the basic number fountain
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				result = wrapper.Generate(invoiceWithTax);
				AssertEquals("No customisation made for Tax/Non Tax made yet", "ABC00000001", result);

				result = wrapper.Generate(invoiceWithoutTax);
				AssertEquals("No customisation made for Tax/Non Tax made yet; same fountain shared between tax and non tax related invoice", "ABC00000002", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.TaxAndNonTax);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.TaxAndNonTax].Fountain = false;
				result = wrapper.Generate(invoiceWithTax);
				AssertEquals("1T prefix added for tax related invoice; uses same as existing number fountain", "1T00000003", result);

				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.TaxAndNonTax].Fountain = true;
				result = wrapper.Generate(invoiceWithTax);
				AssertEquals("1T prefix added for tax related invoice; uses new fountain", "1T00000001", result);

				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.TaxAndNonTax].Fountain = false;
				result = wrapper.Generate(invoiceWithoutTax);
				AssertEquals("0NT prefix added for tax related invoice; uses same as existing number fountain", "0NT00000004", result);

				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.TaxAndNonTax].Fountain = true;
				result = wrapper.Generate(invoiceWithoutTax);
				AssertEquals("0NT prefix added for tax related invoice; uses new fountain", "0NT00000001", result);
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestGenerateNumber_ForSelfBillingAndStandardTransactions()
		{
			PeriodTestHelper.SetupSinglePeriod(201701, new ZDateTime(2016, 10, 1), new ZDateTime(2016, 10, 30));
			Factory.Save();

			var standardInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor, ZDateTime.Today);
			TestObjectCreator.CreateInvoiceLine(standardInvoice, TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.GLHeader1.PK);

			var selfBillingInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor, ZDateTime.Today);
			TestObjectCreator.CreateInvoiceLine(selfBillingInvoice, TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			selfBillingInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.SelfBillingInvoice;

			var wrapper = AccountingNumberFountainWrapperFactory.Instance.SelfBillingInvoiceNo;

			ZString result = ZString.Empty;

			var collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.CustomElement1, false); //Added this so that we get a generated number fountain as opposed to the basic number fountain
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				result = wrapper.Generate(standardInvoice);
				AssertEquals("No customisation made for Self billing made yet", "ABC00000001", result);

				result = wrapper.Generate(selfBillingInvoice);
				AssertEquals("No customisation made for Self billing made yet; same fountain shared between standard and self billing invoice", "ABC00000002", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.SelfBillingAndStandard);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SelfBillingAndStandard].Fountain = false;
				result = wrapper.Generate(standardInvoice);
				AssertEquals("STD prefix added for standard invoice; uses same as existing number fountain", "STD00000003", result);

				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SelfBillingAndStandard].Fountain = true;
				result = wrapper.Generate(standardInvoice);
				AssertEquals("STD prefix added for standard invoice; uses its separate own number fountain", "STD00000001", result);

				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SelfBillingAndStandard].Fountain = false;
				result = wrapper.Generate(selfBillingInvoice);
				AssertEquals("SBI prefix added for Self billing invoice; uses same as existing fountain", "SBI00000004", result);

				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SelfBillingAndStandard].Fountain = true;
				result = wrapper.Generate(selfBillingInvoice);
				AssertEquals("SBI prefix added for Self billing invoice; uses new number fountain", "SBI00000001", result);
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestGenerateNumber_ForCorrectedAndOriginalTransactions()
		{
			PeriodTestHelper.SetupSinglePeriod(201701, new ZDateTime(2016, 10, 1), new ZDateTime(2016, 10, 30));
			Factory.Save();

			var originalInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor, ZDateTime.Today);
			TestObjectCreator.CreateInvoiceLine(originalInvoice, TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.GLHeader1.PK);

			originalInvoice.GenerateReverseTransaction(false);
			var reversedInvoice = originalInvoice.ReverseInvoice;
			reversedInvoice.AH_IsCancelled = true;
			reversedInvoice.AH_TransactionBelongsToGroup = originalInvoice.PK;

			var wrapper = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice);

			ZString result = ZString.Empty;

			var collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.CustomElement1, false); //Added this so that we get a generated number fountain as opposed to the basic number fountain
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				result = wrapper.Generate(originalInvoice);
				AssertEquals("No customisation made for Corrected/Original invoice made yet", "ABC00000001", result);

				result = wrapper.Generate(reversedInvoice);
				AssertEquals("No customisation made for Corrected/Original invoice made yet; same fountain shared between original and reversed invoice", "ABC00000002", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal].Fountain = false;
				result = wrapper.Generate(originalInvoice);
				AssertEquals("ORG prefix added for original invoice; uses same as existing fountain", "ORG00000003", result);

				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal].Fountain = true;
				result = wrapper.Generate(originalInvoice);
				AssertEquals("ORG prefix added for original invoice; uses its separate own number fountain", "ORG00000001", result);

				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal].Fountain = false;
				result = wrapper.Generate(reversedInvoice);
				AssertEquals("AMD prefix added for reversed invoice; uses same as existing fountain", "AMD00000004", result);

				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal].Fountain = true;
				result = wrapper.Generate(reversedInvoice);
				AssertEquals("AMD prefix added for reversed invoice; uses new number fountain", "AMD00000001", result);
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestGenerateNumber_ForCorrectedAndOriginalTransactions_AmendingWithInvoice()
		{
			PeriodTestHelper.SetupSinglePeriod(201701, new ZDateTime(2016, 10, 1), new ZDateTime(2016, 10, 30));
			Factory.Save();

			var originalInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor, ZDateTime.Today);
			TestObjectCreator.CreateInvoiceLine(originalInvoice, TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.GLHeader1.PK);

			var amendedInvoice = (originalInvoice as IAmending).GenerateAmendingTransaction(TransactionTypes.Invoice) as InvoicingBase;

			var wrapper = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice);

			ZString result = ZString.Empty;

			var collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.CustomElement1, false); //Added this so that we get a generated number fountain as opposed to the basic number fountain
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				result = wrapper.Generate(originalInvoice);
				AssertEquals("No customisation made for Corrected/Original invoice made yet", "ABC00000001", result);

				result = wrapper.Generate(amendedInvoice);
				AssertEquals("No customisation made for Corrected/Original invoice made yet; same fountain shared between original and reversed invoice", "ABC00000002", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal].Fountain = false;
				result = wrapper.Generate(originalInvoice);
				AssertEquals("ORG prefix added for original invoice; uses same as existing fountain", "ORG00000003", result);

				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal].Fountain = true;
				result = wrapper.Generate(originalInvoice);
				AssertEquals("ORG prefix added for original invoice; uses its separate own number fountain", "ORG00000001", result);

				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal].Fountain = false;
				result = wrapper.Generate(amendedInvoice);
				AssertEquals("AMD prefix added for reversed invoice; uses same as existing fountain", "AMD00000004", result);

				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal].Fountain = true;
				result = wrapper.Generate(amendedInvoice);
				AssertEquals("AMD prefix added for reversed invoice; uses new number fountain", "AMD00000001", result);
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestGenerateNumber_ForCorrectedAndOriginalTransactions_AmendingWithCreditNote()
		{
			PeriodTestHelper.SetupSinglePeriod(201701, new ZDateTime(2016, 10, 1), new ZDateTime(2016, 10, 30));
			Factory.Save();

			var originalInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor, ZDateTime.Today);
			TestObjectCreator.CreateInvoiceLine(originalInvoice, TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.GLHeader1.PK);

			var amendedCreditNote = (originalInvoice as IAmending).GenerateAmendingTransaction(TransactionTypes.CreditNote) as InvoicingBase;

			var wrapper = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice);

			ZString result = ZString.Empty;

			var collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.CustomElement1, false); //Added this so that we get a generated number fountain as opposed to the basic number fountain
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				result = wrapper.Generate(originalInvoice);
				AssertEquals("No customisation made for Corrected/Original invoice made yet", "ABC00000001", result);

				result = wrapper.Generate(amendedCreditNote);
				AssertEquals("No customisation made for Corrected/Original invoice made yet; same fountain shared between original and reversed invoice", "ABC00000002", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal].Fountain = false;
				result = wrapper.Generate(originalInvoice);
				AssertEquals("ORG prefix added for original invoice; uses same as existing fountain", "ORG00000003", result);

				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal].Fountain = true;
				result = wrapper.Generate(originalInvoice);
				AssertEquals("ORG prefix added for original invoice; uses its separate own number fountain", "ORG00000001", result);

				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal].Fountain = false;
				result = wrapper.Generate(amendedCreditNote);
				AssertEquals("AMD prefix added for reversed invoice; uses same as existing fountain", "AMD00000004", result);

				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal].Fountain = true;
				result = wrapper.Generate(amendedCreditNote);
				AssertEquals("AMD prefix added for reversed invoice; uses new number fountain", "AMD00000001", result);
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestGenerateNumber_CombinationOfCustomisationConfigurations()
		{
			PeriodTestHelper.SetupSinglePeriod(201701, new ZDateTime(2016, 10, 1), new ZDateTime(2016, 10, 30));
			Factory.Save();

			var originalInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor, ZDateTime.Today);
			originalInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.SelfBillingInvoice;
			var lineWithTax = TestObjectCreator.CreateInvoiceLine(originalInvoice, TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			lineWithTax.AL_AT = TestObjectCreator.GST1.PK;

			originalInvoice.GenerateReverseTransaction(false);
			var reversedInvoice = originalInvoice.ReverseInvoice;
			reversedInvoice.AH_IsCancelled = true;
			reversedInvoice.AH_TransactionBelongsToGroup = originalInvoice.PK;

			var wrapper = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice);

			ZString result = ZString.Empty;

			var collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber);
			AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.TaxAndNonTax, "1T/0NT", 3, 1, true, false);
			AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.SelfBillingAndStandard, "SBI/STD", 3, 2, true, false);
			AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal, "AMD/ORG", 3, 3, true, false);

			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.TaxAndNonTax].Fountain = true;
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SelfBillingAndStandard].Fountain = false;
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal].Fountain = false;
				result = wrapper.Generate(originalInvoice);
				AssertEquals("'1T', 'SBI', 'ORG' prefix added for 'tax related self billing original invoice'; uses new number fountain", "1TSBIORG00000001", result);

				result = wrapper.Generate(reversedInvoice);
				AssertEquals("'1T', 'SBI', 'AMD' prefix added for 'tax related self billing reversed invoice'; uses same number fountain as original invoice", "1TSBIAMD00000002", result);

				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.TaxAndNonTax].Fountain = false;
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SelfBillingAndStandard].Fountain = true;
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal].Fountain = false;
				result = wrapper.Generate(originalInvoice);
				AssertEquals("'1T', 'SBI', 'ORG' prefix added for 'tax related self billing original invoice'; uses new number fountain", "1TSBIORG00000001", result);

				result = wrapper.Generate(reversedInvoice);
				AssertEquals("'1T', 'SBI', 'AMD' prefix added for 'tax related self billing reversed invoice'; uses same number fountain as original invoice", "1TSBIAMD00000002", result);

				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.TaxAndNonTax].Fountain = false;
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.SelfBillingAndStandard].Fountain = false;
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value[TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal].Fountain = true;
				result = wrapper.Generate(originalInvoice);
				AssertEquals("'1T', 'SBI', 'ORG' prefix added for 'tax related self billing original invoice'; uses new number fountain", "1TSBIORG00000001", result);

				result = wrapper.Generate(reversedInvoice);
				AssertEquals("'1T', 'SBI', 'AMD' prefix added for 'tax related self billing reversed invoice'; reversed invoice uses its own number fountain", "1TSBIAMD00000001", result);
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestGroupedFountainForCN()
		{
			var testBranch = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "TST");
			PeriodTestHelper.SetupSinglePeriod(201603, new ZDateTime(2016, 3, 1), new ZDateTime(2016, 3, 30));
			PeriodTestHelper.SetupSinglePeriod(123, new ZDateTime(2016, 2, 1), new ZDateTime(2016, 2, 20));
			Factory.Save();

			var postDate = new ZDateTime(2016, 3, 10);

			var wrapper = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice);
			var wrapper5digits = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice, 5);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("Default China company transaction number", "1603001000", result);

				result = wrapper5digits.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("Default China company transaction number is NOT affected by priority length", "1603001001", result);

				using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.DefaultValue))
				{
					result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, new ZDateTime(2016, 4, 10), testBranch, TestObjectCreator.FESDepartment));
					AssertEquals("Post date without relative Accounting period", "001000", result);
				}

				using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.DefaultValue))
				{
					result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, new ZDateTime(2016, 2, 10), testBranch, TestObjectCreator.FESDepartment));
					AssertEquals("Test special period", "123001000", result);
				}

				var collection = new TransactionNumberSequenceCustomisationCollection();
				AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits, "2", 0, 1, true, true);
				AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits, ZString.Empty, 0, 3, true, true);
				AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber, ZString.Empty, 6, 50, true, true);
				using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				{
					result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
					AssertEquals("Fountain should continue when registry is override but with settings same as default China company trans number", "1603001002", result);
					result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
					AssertEquals("1603001003", result);
				}

				collection = new TransactionNumberSequenceCustomisationCollection();
				AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits, "2", 0, 2, true, true);
				AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits, ZString.Empty, 0, 1, true, true);
				AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber, ZString.Empty, 6, 50, true, true);
				using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				{
					result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
					AssertEquals("Sequence number should restart when fountain element order changed.", "0316000001", result);
				}

				collection = new TransactionNumberSequenceCustomisationCollection();
				AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits, "2", 0, 1, true, true);
				AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderDepartmentCode, ZString.Empty, 0, 2, true, false);
				AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits, ZString.Empty, 0, 3, true, true);
				AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber, ZString.Empty, 6, 50, true, true);
				using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				{
					result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
					AssertEquals("Sequence number should continue when add new non-fountain element.", "16FES03001004", result);
				}
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestGroupedFountain()
		{
			var testBranch = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "TST");
			PeriodTestHelper.SetupSinglePeriod(201601, new ZDateTime(2016, 1, 1), new ZDateTime(2016, 1, 30));
			PeriodTestHelper.SetupSinglePeriod(201602, new ZDateTime(2016, 2, 1), new ZDateTime(2016, 2, 20));
			PeriodTestHelper.SetupSinglePeriod(201603, new ZDateTime(2016, 3, 1), new ZDateTime(2016, 3, 30));
			PeriodTestHelper.SetupSinglePeriod(201703, new ZDateTime(2017, 3, 1), new ZDateTime(2017, 3, 30));
			PeriodTestHelper.SetupSinglePeriod(197601, new ZDateTime(1976, 1, 1), new ZDateTime(1976, 1, 30));
			PeriodTestHelper.SetupSinglePeriod(197603, new ZDateTime(1976, 3, 1), new ZDateTime(1976, 3, 30));
			PeriodTestHelper.SetupSinglePeriod(207603, new ZDateTime(2076, 3, 1), new ZDateTime(2076, 3, 30));
			Factory.Save();

			var wrapper = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice);
			var wrapper5digits = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice, 5);

			string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, new ZDateTime(2016, 2, 10), testBranch, TestObjectCreator.FESDepartment));
			AssertEquals("Default transaction number", "00001000", result);

			result = wrapper5digits.Generate(new AccountingNumberFountainDataSourceForTest(Factory, new ZDateTime(2016, 2, 10), testBranch, TestObjectCreator.FESDepartment));
			AssertEquals("Default transaction number with priority length", "01001", result);

			var collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber);
			AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.YearAsDigits, "4", 0, 9, true, true);
			AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.MonthAsLetter, ZString.Empty, 0, 15, true, true);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				var date1 = new ZDateTime(2016, 1, 10);
				var date2 = new ZDateTime(2016, 3, 10);
				var date3 = new ZDateTime(2017, 3, 10);

				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date1, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("2016A00000001", result);
				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date1, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("2016A00000002", result);
				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date2, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("2016C00000001", result);
				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date3, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("2017C00000001", result);
				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date2, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("2016C00000002", result);
				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date3, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("2017C00000002", result);
				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date1, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("2016A00000003", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber);
			AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits, "4", 0, 9, true, true);
			AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits, ZString.Empty, 0, 15, true, true);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				var date1 = new ZDateTime(2016, 1, 10);
				var date2 = new ZDateTime(2016, 3, 10);
				var date3 = new ZDateTime(2017, 3, 10);

				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date1, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("20160100000001", result);
				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date1, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("20160100000002", result);
				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date2, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("20160300000001", result);
				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date3, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("20170300000001", result);
				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date2, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("20160300000002", result);
				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date3, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("20170300000002", result);
				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date1, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("20160100000003", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber);
			AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits, "2", 0, 9, true, true);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				var date1 = new ZDateTime(1976, 1, 10);
				var date2 = new ZDateTime(1976, 3, 10);
				var date3 = new ZDateTime(2076, 3, 10);

				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date1, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("7600000001", result);
				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date2, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("7600000002", result);
				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date3, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("7600000003", result);
			}

			collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber);
			AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits, "1", 0, 9, true, true);
			using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				var date1 = new ZDateTime(1976, 1, 10);
				var date2 = new ZDateTime(1976, 3, 10);
				var date3 = new ZDateTime(2076, 3, 10);

				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date1, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("600000001", result);
				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date2, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("600000002", result);
				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, date3, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("600000003", result);
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestGetNext_SimultaneouslyEnumerateCustomisationRegistry_NoError()
		{
			AssertNoExceptionThrown("Should not throw exception: 'Collection was modified; enumeration operation may not execute'", () =>
			{
				var postDate = new ZDateTime(2016, 10, 20);
				var wrapper = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice);

				var collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber);

				using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				{
					var fountain = new AccountingNumberFountainDataSourceForTest(Factory, postDate, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);
					var thisCollection = AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value;
					foreach (var element in thisCollection)
					{
						// Ref: WI00546339, for ISS 01625069.  We are simulating calling Generate from 2 places at once -- The GetNext branch of the if/else
						// Even though we don't use "element" in this loop, it simulates the iteration inside the unfixed Generate method
						// and causes the same exception thrown in the issue (before fix).
						string result = wrapper.Generate(fountain);
					}
				}
			});
		}

		[TestDate(2016, 10, 20)]
		public void TestGenerateNumber_SimultaneouslyEnumerateCustomisationRegistry_NoError()
		{
			AssertNoExceptionThrown("Should not throw exception: 'Collection was modified; enumeration operation may not execute'", () =>
			{
				var postDate = new ZDateTime(2016, 10, 20);
				var wrapper = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice);

				var collection = PrepareTransactionNumberCustomisationCollection(TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber);
				AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits, "4", 0, 9, true, true);
				AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits, ZString.Empty, 0, 15, true, true);

				using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				{
					var fountain = new AccountingNumberFountainDataSourceForTest(Factory, postDate, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);
					var thisCollection = AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value;
					foreach (var element in thisCollection)
					{
						// Ref: WI00546339, for ISS 01625069.  We are simulating calling Generate from 2 places at once -- the GenerateNumber branch of the if/else.
						// Even though we don't use "element" in this loop, it simulates the iteration inside the unfixed Generate method
						// and causes the same exception thrown in the issue (before fix).
						string result = wrapper.Generate(fountain);
					}
				}
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity", Justification = "This test method should not be split.")]
		TransactionNumberSequenceCustomisationCollection PrepareTransactionNumberCustomisationCollection(string elementName, bool includeFountain = true)
		{
			var collection = new TransactionNumberSequenceCustomisationCollection();

			switch (elementName)
			{
				case TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber:
					AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber, ZString.Empty, 8, 50, true, includeFountain);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderBranchCode:
					AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderBranchCode, ZString.Empty, 0, 1, true, includeFountain);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderDepartmentCode:
					AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderDepartmentCode, ZString.Empty, 0, 3, true, includeFountain);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.CustomElement1:
					AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.CustomElement1, "ABC", 0, 5, true, includeFountain);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.CustomElement2:
					AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.CustomElement2, "XYZ", 0, 7, true, includeFountain);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.YearAsDigits:
					AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.YearAsDigits, "4", 0, 9, true, includeFountain);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.YearAsLetter:
					AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.YearAsLetter, ZString.Empty, 0, 11, true, includeFountain);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.MonthAs2Digits:
					AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.MonthAs2Digits, ZString.Empty, 0, 13, true, includeFountain);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.MonthAsLetter:
					AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.MonthAsLetter, ZString.Empty, 0, 15, true, includeFountain);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits:
					AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits, "4", 0, 17, true, includeFountain);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsLetter:
					AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsLetter, ZString.Empty, 0, 19, true, includeFountain);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits:
					AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits, ZString.Empty, 0, 21, true, includeFountain);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.TaxAndNonTax:
					AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.TaxAndNonTax, "1T/0NT", 3, 22, true, includeFountain);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.SelfBillingAndStandard:
					AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.SelfBillingAndStandard, "SBI/STD", 3, 23, true, includeFountain);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal:
					AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal, "AMD/ORG", 3, 24, true, includeFountain);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.TransactionTypePrefix:
					AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.TransactionTypePrefix, ZString.Empty, 0, 25, true, includeFountain);
					break;
			}

			return collection;
		}

		#endregion

		protected TransactionNumberSequenceCustomisationCollection CreateTransactionNumberSequenceCustomisationCollection()
		{
			TransactionNumberSequenceCustomisationCollection collection = new TransactionNumberSequenceCustomisationCollection();
			AddTransactionNumberSequenceCustomisation(collection, "Transaction Header Branch Code", ZString.Empty, 3, 1, false, false);
			AddTransactionNumberSequenceCustomisation(collection, "Transaction Header Department Code", ZString.Empty, 3, 2, true, true);
			AddTransactionNumberSequenceCustomisation(collection, "Custom Element 1", "AB", 2, 3, true, true);
			AddTransactionNumberSequenceCustomisation(collection, "Custom Element 2", "XY", 2, 4, true, true);
			AddTransactionNumberSequenceCustomisation(collection, "Sequence Number", ZString.Empty, 8, 5, true, true);
			return collection;
		}

		protected TransactionNumberSequenceCustomisation AddTransactionNumberSequenceCustomisation(TransactionNumberSequenceCustomisationCollection collection, ZString elementName, ZString code, ZInt length, ZByte order, ZBool include, ZBool fountain)
		{
			TransactionNumberSequenceCustomisation customisation = collection.AddNew();
			customisation.ElementName = elementName;
			customisation.Code = code;
			customisation.Include = include;
			customisation.Fountain = fountain;
			if (elementName == "Sequence Number")
			{
				customisation.Length = length;
			}
			customisation.Order = order;
			return customisation;
		}

		[TestDate(2011, 6, 30)]
		public void TestGenerate_GeneratorFountain()
		{
			AccountingNumberFountainWrapper wrapper = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice);

			TransactionNumberSequenceCustomisationCollection collection = CreateTransactionNumberSequenceCustomisationCollection();
			AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			string result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, ZDateTime.Today, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment));
			AssertEquals("BRNABXY00000001", result);

			result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, ZDateTime.Today, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment));
			AssertEquals("BRNABXY00000002", result);

			GlbCompany nonCurrentCompany = TestObjectCreator.NonCurrentCompany;
			GlbBranch nonCurrentCompanyBranch = TestObjectCreator.NonCurrentCompanyBranch;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), nonCurrentCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetValue(nonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
				result = wrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, ZDateTime.Today, nonCurrentCompanyBranch, GlbDepartment.CurrentDepartment));
				AssertEquals("BRNABXY00000001", result);
			}
		}

		public void TestGetNonUserConfigurableFountainKeyFromFountainType()
		{
			AccountingNumberFountainWrapper wrapper = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice);

			Array values = Enum.GetValues(typeof(NumberFountainType));

			foreach (NumberFountainType value in values)
			{
				if (value == NumberFountainType.None)
				{
					Assert(true);
				}
				else
				{
					AssertNotEquals(string.Format("Should not return ZString.Empty: NumberFountainType.{0}.  If this unit test fails, please add this NumberFountainType to GetNonUserConfigurableFountainKeyFromFountainType().", value.ToString()), ZString.Empty, wrapper.GetNonUserConfigurableFountainKeyFromFountainType(value));
				}
			}
		}

		[SuspendCriticalValidation]
		public void TestNumberFountainTransactionDataProviderIsUsedProperly()
		{
			var collection = AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value;
			var taxCustomisation = AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.TaxAndNonTax, "", 0, 0, false, false);
			var selfBillingCustomisation = AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.SelfBillingAndStandard, "", 0, 0, false, false);
			var correctionCustomisation = AddTransactionNumberSequenceCustomisation(collection, TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal, "", 0, 0, false, false);
			AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection);

			var wrapper = new AccountingNumberFountainWrapper(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice);

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine.AL_AT = TestObjectCreator.ExtraServiceTax.PK;

			Assert("IsTaxReportable", !invoice.IsTaxReportable);
			Assert("IsSelfBillingInvoice", !invoice.IsSelfBillingInvoice);
			Assert("IsAmendingOrReversal", !invoice.IsAmendingOrReversal);

			SetupRegistry(inclueTax: false, inclueSelfBilling: false, inclueReversal: false);
			AssertGenerate(swapTax: true, swapSelfBilling: true, swapReversal: true, isCorrect: true);

			SetupRegistry(inclueTax: true, inclueSelfBilling: false, inclueReversal: false);
			AssertGenerate(swapTax: false, swapSelfBilling: true, swapReversal: true, isCorrect: true);
			AssertGenerate(swapTax: true, swapSelfBilling: false, swapReversal: false, isCorrect: false);

			SetupRegistry(inclueTax: false, inclueSelfBilling: true, inclueReversal: false);
			AssertGenerate(swapTax: true, swapSelfBilling: false, swapReversal: true, isCorrect: true);
			AssertGenerate(swapTax: false, swapSelfBilling: true, swapReversal: false, isCorrect: false);

			SetupRegistry(inclueTax: false, inclueSelfBilling: false, inclueReversal: true);
			AssertGenerate(swapTax: true, swapSelfBilling: true, swapReversal: false, isCorrect: true);
			AssertGenerate(swapTax: false, swapSelfBilling: false, swapReversal: true, isCorrect: false);

			SetupRegistry(inclueTax: true, inclueSelfBilling: true, inclueReversal: true);
			AssertGenerate(swapTax: true, swapSelfBilling: false, swapReversal: false, isCorrect: false);
			AssertGenerate(swapTax: false, swapSelfBilling: true, swapReversal: false, isCorrect: false);
			AssertGenerate(swapTax: false, swapSelfBilling: false, swapReversal: true, isCorrect: false);
			AssertGenerate(swapTax: false, swapSelfBilling: false, swapReversal: false, isCorrect: true);

			void SetupRegistry(bool inclueTax, bool inclueSelfBilling, bool inclueReversal)
			{
				taxCustomisation.Include = inclueTax;
				if (taxCustomisation.Include)
				{
					taxCustomisation.Code = "TX/NT";
					taxCustomisation.Order = 1;
				}
				selfBillingCustomisation.Include = inclueSelfBilling;
				if (selfBillingCustomisation.Include)
				{
					selfBillingCustomisation.Code = "SB/ST";
					selfBillingCustomisation.Order = 2;
				}
				correctionCustomisation.Include = inclueReversal;
				if (correctionCustomisation.Include)
				{
					correctionCustomisation.Code = "REV/ORG";
					correctionCustomisation.Order = 3;
				}
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection);
			}

			void AssertGenerate(bool swapTax, bool swapSelfBilling, bool swapReversal, bool isCorrect)
			{
				Factory.Save();
				wrapper.Generate(invoice);
				SwapInvoiceState(swapTax, swapSelfBilling, swapReversal);
				AssertEquals("CheckDataIsCorrect", isCorrect, NumberFountainTransactionDataProvider.CheckDataIsCorrect(invoice).IsCorrect);
			}

			void SwapInvoiceState(bool swapTax, bool swapSelfBilling, bool swapReversal)
			{
				if (swapTax)
				{
					var prevIsTaxReportable = invoice.IsTaxReportable;
					invoiceLine.TaxRate.AT_Type = invoice.IsTaxReportable ? AccTaxRate.Types.NotReportable : AccTaxRate.Types.Rated;
					AssertNotEquals("SwapInvoiceState: IsTaxReportable", prevIsTaxReportable, invoice.IsTaxReportable);
				}

				if (swapSelfBilling)
				{
					var prevIsSelfBillingInvoice = invoice.IsSelfBillingInvoice;
					invoice.AH_TransactionCategory = invoice.IsSelfBillingInvoice ? TransactionCategory.Codes.Standard : TransactionCategory.Codes.SelfBilling;
					AssertNotEquals("SwapInvoiceState: IsSelfBillingInvoice", prevIsSelfBillingInvoice, invoice.IsSelfBillingInvoice);
				}

				if (swapReversal)
				{
					var prevIsAmendingOrReversal = invoice.IsAmendingOrReversal;
					invoice.AH_IsCancelled = !invoice.IsAmendingOrReversal;
					AssertNotEquals("SwapInvoiceState: IsAmendingOrReversal", prevIsAmendingOrReversal, invoice.IsAmendingOrReversal);
				}
			}
		}
	}
}
