using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GenericTransaction
{
	[TestedType(typeof(GenericTransactionCollection))]
	public class GenericTransactionCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestDelete()
		{
			base.TestRemoveFromRelationship();
		}

		public override void TestTypedAddNew()
		{
			Assert(true);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert(true);
		}

		public void TestLoad_Customised()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var orgHeader = testObjectCreator.CreateOrgHeader("TSTORG1", false, false);
			var glAccount1 = testObjectCreator.CreateGLHeader();
			ARInvoice trnHeader = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoiceLine trnLine = Factory.NewWithValidTestData<ARInvoiceLine>();
			trnLine.AL_AH = trnHeader.PK;
			trnLine.AL_AG = glAccount1.PK;
			testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(trnLine.PK, Core.Constants.SubAccountType.Organization, orgHeader.PK);
			var glAccount2 = testObjectCreator.CreateGLHeader();
			ARInvoice trnHeader2 = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoiceLine trnLine2 = Factory.NewWithValidTestData<ARInvoiceLine>();
			trnLine2.AL_AH = trnHeader2.PK;
			trnLine2.AL_AG = glAccount2.PK;
			Factory.Save();
			trnHeader.Reload();
			trnLine.Reload();
			GenericTransactionCollection collection = new GenericTransactionCollection(Factory);
			collection.Load();
			AssertEquals("Nothing should be loaded", 0, collection.Count);
			ZQuery filter = new ZQuery(GenericTransactionSchema.VT_GC, GlbCompany.CurrentCompany.PK);
			collection.Load(filter);
			AssertEquals("2 transactions should be loaded", 2, collection.Count);
			ZGuid pK0 = collection[0].VT_FK;
			ZGuid pK1 = collection[1].VT_FK;
			int index = (pK0 == trnHeader.PK ? 0 : 1);
			GenericTransaction gT = collection[index];
			AssertEquals("VT_FK", trnHeader.PK, gT.VT_FK);
			AssertEquals("VT_IsHeader", true, gT.VT_IsHeader);
			AssertEquals("VT_Ledger", trnHeader.AH_Ledger, gT.VT_Ledger);
			AssertEquals("VT_Type", trnHeader.AH_TransactionType, gT.VT_Type);
			AssertEquals("VT_OH", trnHeader.AH_OH, gT.VT_OH);
			AssertEquals("VT_PostDate", trnHeader.AH_PostDate, gT.VT_PostDate);
			AssertEquals("VT_InvoiceDate", trnHeader.AH_InvoiceDate, gT.VT_InvoiceDate);
			AssertEquals("VT_DueDate", trnHeader.AH_DueDate, gT.VT_DueDate);
			AssertEquals("VT_Period", trnHeader.PostPeriod, gT.VT_Period);
			AssertEquals("VT_Amount", trnHeader.AH_InvoiceAmount, gT.VT_Amount);
			AssertEquals("VT_GST", trnHeader.AH_GSTAmount, gT.VT_GST);
			AssertEquals("VT_Total", trnHeader.AH_InvoiceAmount + trnHeader.AH_GSTAmount, gT.VT_Total);
			AssertEquals("VT_GLAccount", glAccount1.AG_AccountNum, gT.VT_GLAccount);
			AssertEquals("VT_GLAccountDesc", "", gT.VT_GLAccountDesc);
			AccGLHeader aRControlAccount = Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.ARControlAccount.Value);
			AssertEquals("VT_2ndGLAccount", aRControlAccount.AG_AccountNum, gT.VT_2ndGLAccount);
			AssertEquals("VT_2ndGLAccountDesc", aRControlAccount.AG_Description, gT.VT_2ndGLAccountDesc);
			AssertEquals("VT_GSTGLAccount", "", gT.VT_GSTGLAccount);
			AssertEquals("VT_GSTGLAccountDesc", "", gT.VT_GSTGLAccountDesc);
			AssertEquals("VT_TransactionNo", trnHeader.AH_TransactionNum, gT.VT_TransactionNo);
			AssertEquals("VT_TransactionDesc", trnHeader.AH_Desc, gT.VT_TransactionDesc);
			AssertEquals("VT_JH", trnHeader.AH_JH, gT.VT_JH);
			AssertEquals("VT_ChargeCode", "", gT.VT_ChargeCode);
			AssertEquals("VT_GC", trnLine.Branch.Company.PK, gT.VT_GC);
			AssertEquals("VT_GB", trnHeader.AH_GB, gT.VT_GB);
			AssertEquals("VT_GE", trnHeader.AH_GE, gT.VT_GE);
			AssertEquals("VT_ReversePeriod", 0, gT.VT_ReversePeriod);
			AssertEquals("VT_VoucherNo", trnHeader.AH_TransactionNum, gT.VT_VoucherNo);
			AssertEquals("VT_OSTotal", trnHeader.AH_OSTotal, gT.VT_OSTotal);
			AssertEquals("VT_RX_NKCurrency", trnHeader.AH_RX_NKTransactionCurrency, gT.VT_RX_NKCurrency);
			AssertEquals("VT_ExchangeRate", trnHeader.AH_ExchangeRate, gT.VT_ExchangeRate);
			AssertEquals("VT_OSAmount", Env.CurrentCompany.ExchangeRate.LocalToForeign(trnHeader.AH_InvoiceAmount, trnHeader.AH_ExchangeRate, trnHeader.AH_RX_NKTransactionCurrency), gT.VT_OSAmount);
			AssertEquals("VT_GST", Env.CurrentCompany.ExchangeRate.LocalToForeign(trnHeader.AH_GSTAmount, trnHeader.AH_ExchangeRate, trnHeader.AH_RX_NKTransactionCurrency), gT.VT_GST);
			AssertEquals("VT_SubAccounts", $"ORG: {orgHeader.OH_Code}", gT.VT_SubAccounts);
			AssertEquals("VT_AGForSubAccount", glAccount1.PK, gT.VT_AGForSubAccount);
			AssertEquals("VT_SubAccountParentPK", trnLine.PK, gT.VT_SubAccountParentPK);
		}

		public void TestLoadTop1_Customised()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var glAccount1 = testObjectCreator.CreateGLHeader();
			ARInvoice trnHeader = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoiceLine trnLine = Factory.NewWithValidTestData<ARInvoiceLine>();
			trnLine.AL_AH = trnHeader.PK;
			trnLine.AL_AG = glAccount1.PK;
			Factory.Save();
			trnHeader.Reload();
			trnLine.Reload();
			GenericTransactionCollection collection = new GenericTransactionCollection(Factory);
			collection.LoadTop1(new ZQuery());
			AssertEquals("1 transaction should be loaded", 1, collection.Count);
			GenericTransaction gT = collection[0];
			AssertEquals("VT_FK", trnHeader.PK, gT.VT_FK);
			AssertEquals("VT_IsHeader", true, gT.VT_IsHeader);
			AssertEquals("VT_Ledger", trnHeader.AH_Ledger, gT.VT_Ledger);
			AssertEquals("VT_Type", trnHeader.AH_TransactionType, gT.VT_Type);
			AssertEquals("VT_OH", trnHeader.AH_OH, gT.VT_OH);
			AssertEquals("VT_PostDate", trnHeader.AH_PostDate, gT.VT_PostDate);
			AssertEquals("VT_InvoiceDate", trnHeader.AH_InvoiceDate, gT.VT_InvoiceDate);
			AssertEquals("VT_DueDate", trnHeader.AH_DueDate, gT.VT_DueDate);
			AssertEquals("VT_Period", trnHeader.PostPeriod, gT.VT_Period);
			AssertEquals("VT_Amount", trnHeader.AH_InvoiceAmount, gT.VT_Amount);
			AssertEquals("VT_GST", trnHeader.AH_GSTAmount, gT.VT_GST);
			AssertEquals("VT_Total", trnHeader.AH_InvoiceAmount + trnHeader.AH_GSTAmount, gT.VT_Total);
			AssertEquals("VT_GLAccount", glAccount1.AG_AccountNum, gT.VT_GLAccount);
			AssertEquals("VT_GLAccountDesc", "", gT.VT_GLAccountDesc);
			AccGLHeader aRControlAccount = Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.ARControlAccount.Value);
			AssertEquals("VT_2ndGLAccount", aRControlAccount.AG_AccountNum, gT.VT_2ndGLAccount);
			AssertEquals("VT_2ndGLAccountDesc", aRControlAccount.AG_Description, gT.VT_2ndGLAccountDesc);
			AssertEquals("VT_GSTGLAccount", "", gT.VT_GSTGLAccount);
			AssertEquals("VT_GSTGLAccountDesc", "", gT.VT_GSTGLAccountDesc);
			AssertEquals("VT_TransactionNo", trnHeader.AH_TransactionNum, gT.VT_TransactionNo);
			AssertEquals("VT_TransactionDesc", trnHeader.AH_Desc, gT.VT_TransactionDesc);
			AssertEquals("VT_JH", trnHeader.AH_JH, gT.VT_JH);
			AssertEquals("VT_ChargeCode", "", gT.VT_ChargeCode);
			AssertEquals("VT_GC", trnLine.Branch.Company.PK, gT.VT_GC);
			AssertEquals("VT_GB", trnHeader.AH_GB, gT.VT_GB);
			AssertEquals("VT_GE", trnHeader.AH_GE, gT.VT_GE);
			AssertEquals("VT_ReversePeriod", 0, gT.VT_ReversePeriod);
			AssertEquals("VT_VoucherNo", trnHeader.AH_TransactionNum, gT.VT_VoucherNo);
			AssertEquals("VT_OSTotal", trnHeader.AH_OSTotal, gT.VT_OSTotal);
			AssertEquals("VT_RX_NKCurrency", trnHeader.AH_RX_NKTransactionCurrency, gT.VT_RX_NKCurrency);
			AssertEquals("VT_ExchangeRate", trnHeader.AH_ExchangeRate, gT.VT_ExchangeRate);
			AssertEquals("VT_OSAmount", Env.CurrentCompany.ExchangeRate.LocalToForeign(trnHeader.AH_InvoiceAmount, trnHeader.AH_ExchangeRate, trnHeader.AH_RX_NKTransactionCurrency), gT.VT_OSAmount);
			AssertEquals("VT_GST", Env.CurrentCompany.ExchangeRate.LocalToForeign(trnHeader.AH_GSTAmount, trnHeader.AH_ExchangeRate, trnHeader.AH_RX_NKTransactionCurrency), gT.VT_GST);
		}

		public void TestSystemCreateUser()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var header1 = CreateInvoiceData("E");
			var header2 = CreateInvoiceData("E");
			var header3 = CreateInvoiceData("UKJ");
			Factory.Save();

			var collection = new GenericTransactionCollection(Factory);
			collection.Load();
			AssertEquals("Nothing should be loaded", 0, collection.Count);

			var filter = new ZQuery(GenericTransactionSchema.VT_GC, GlbCompany.CurrentCompany.PK);
			collection.Load(filter);
			AssertEquals("3 transactions should be loaded", 3, collection.Count);

			var genericTransactionsList = collection.Cast<GenericTransaction>().ToList();
			AssertEquals("E", genericTransactionsList.FirstOrDefault(x => x.VT_FK == header1.PK).VT_SystemCreateUser);
			AssertEquals("E", genericTransactionsList.FirstOrDefault(x => x.VT_FK == header2.PK).VT_SystemCreateUser);
			AssertEquals("UKJ", genericTransactionsList.FirstOrDefault(x => x.VT_FK == header3.PK).VT_SystemCreateUser);

			filter = new ZQuery(GenericTransactionSchema.VT_SystemCreateUser, "E");
			collection.Load(filter);
			AssertEquals("2 transactions should be loaded", 2, collection.Count);

			filter = new ZQuery(GenericTransactionSchema.VT_SystemCreateUser, SQLComparisonOperator.NotEqual, "E");
			collection.Load(filter);
			AssertEquals("1 transactions should be loaded", 1, collection.Count);

			ARInvoice CreateInvoiceData(string creatingUser)
			{
				var glAccount = testObjectCreator.CreateGLHeader();
				var header = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.AUD, 1, testObjectCreator.Debtor1);
				header.AH_SystemCreateUser = creatingUser;
				var line = testObjectCreator.CreateInvoiceLine(header, 7M);
				line.AL_AG = glAccount.PK;
				return header;
			}
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GenericTransactionCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GenericTransaction(Factory);
		}
	}
}
