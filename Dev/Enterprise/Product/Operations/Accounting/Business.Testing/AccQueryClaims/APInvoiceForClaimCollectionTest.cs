using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(APInvoiceForClaimCollection))]
	public class APInvoiceForClaimCollectionTest : TransactionHeaderCollectionForClaimTest
	{
		public void TestFilteringWithInvoiceAndCrNote()
		{
			QueryClaim = (AccQueryClaim)Factory.NewWithValidTestData(GetQueryClaimType());
			TransactionHeaderCollection transactionCollection = (TransactionHeaderCollection)GetCollectionToTest();
			transactionCollection.Load(new ZQuery());
			int existingTransactionCount = transactionCollection.Count;
			AccTransactionHeader aPInvoice = (AccTransactionHeader)Factory.NewWithValidTestData(typeof(APInvoice));
			AccTransactionHeader aPCrNote = (AccTransactionHeader)Factory.NewWithValidTestData(typeof(APCreditNote));
			aPInvoice.AH_OH = QueryClaim.AY_OH_Debtor;
			aPCrNote.AH_OH = QueryClaim.AY_OH_Debtor;
			Factory.Save();
			transactionCollection.Load(new ZQuery());
			CombineAssertions(() =>
			{
				AssertEquals("Collection should contain 1 new APInvoice", 1, transactionCollection.Count - existingTransactionCount);
				Assert("Collection should contain APInvoice", transactionCollection.Contains(aPInvoice));
				Assert("Collection should NOT contain APCrNote", !transactionCollection.Contains(aPCrNote));
			}

			);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new APInvoiceForClaimCollection(QueryClaim, new ZQuery());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(APInvoice));
		}

		protected override Type GetQueryClaimType()
		{
			return typeof(APAccQueryClaim);
		}

		protected override Type GetTransactionType()
		{
			return typeof(APInvoice);
		}
	}
}
