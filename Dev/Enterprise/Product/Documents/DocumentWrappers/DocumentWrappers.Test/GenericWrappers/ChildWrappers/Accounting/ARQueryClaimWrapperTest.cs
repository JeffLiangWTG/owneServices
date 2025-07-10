using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(QueryClaimWrapper))]
	sealed class ARQueryClaimWrapperTest : QueryClaimWrapperTest
	{
		public override void TestAccountType()
		{
			QueryClaimWrapper wrapper = (QueryClaimWrapper)GetNewDocumentWrapper();
			AssertEquals("DEBTOR", wrapper.AccountType);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new QueryClaimWrapper(Factory.New<ARAccQueryClaim>(), Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new QueryClaimWrapper(Factory.New<ARAccQueryClaim>(), Factory);
		}

		protected override AccQueryClaim GetQueryClaim()
		{
			return Factory.New<ARAccQueryClaim>();
		}

		public void TestAmount()
		{
			ARAccQueryClaim queryClaim = Factory.New<ARAccQueryClaim>();
			queryClaim.AY_QueryClaimAmount = 225.50;
			QueryClaimWrapper wrapper = new QueryClaimWrapper(queryClaim, Factory);
			AssertEquals("Amount without currency", "225.50", wrapper.Amount);
			AccTransactionHeader transactionHeader = Factory.New<AccTransactionHeader>();
			transactionHeader.AH_RX_NKTransactionCurrency = "AUD";
			transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			queryClaim.AY_AH = transactionHeader.PK;
			AssertEquals("Amount with currency", "$225.50 AUD", wrapper.Amount);
		}

		public void TestDebtor()
		{
			ARAccQueryClaim queryClaim = Factory.New<ARAccQueryClaim>();
			OrgHeader debtor = Factory.New<OrgHeader>();
			queryClaim.AY_OH_Debtor = debtor.PK;
			QueryClaimWrapper wrapper = new QueryClaimWrapper(queryClaim, Factory);
			AssertEquals("wrapper.Debtor", debtor.PK, wrapper.Debtor.PK);
		}
	}
}
