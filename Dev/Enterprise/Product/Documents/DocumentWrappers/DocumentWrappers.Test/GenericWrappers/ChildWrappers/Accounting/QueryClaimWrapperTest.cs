using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	public abstract class QueryClaimWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = (QueryClaimWrapper)GetNewDocumentWrapper();
			var wrappedObject = (AccQueryClaim)wrapperEmpty.WrappedObject;
			AssertEquals("wrapperEmpty.ToString()", ZString.Empty, wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.Debtor", null, wrapperEmpty.Debtor);
			AssertEquals("wrapperEmpty.Contact", ZString.Empty, wrapperEmpty.Contact.FullName);
			AssertEquals("wrapperEmpty.TransactionCurrency", null, wrapperEmpty.TransactionCurrency);
			AssertEquals("wrapperEmpty.TransactionAmount", ZDecimal.Zero, wrapperEmpty.TransactionAmount.Amount);
			AssertEquals("wrapperEmpty.JobNumber", ZString.Empty, wrapperEmpty.JobNumber);
			AssertEquals("wrapperEmpty.InvoiceNo", ZString.Empty, wrapperEmpty.InvoiceNo);
			AssertEquals("wrapperEmpty.Amount", "0.00", wrapperEmpty.Amount);
			AssertEquals("wrapperEmpty.ShortDescription", ZString.Empty, wrapperEmpty.ShortDescription);
			AssertEquals("wrapperEmpty.TypeCode", wrappedObject.AY_QueryClaimType, wrapperEmpty.TypeCode);
			AssertEquals("wrapperEmpty.TypeDescription", wrappedObject.Lookups.ClaimType.GetDescriptionFromCode(wrappedObject.AY_QueryClaimType), wrapperEmpty.TypeDescription);
			AssertEquals("wrapperEmpty.ReasonCode", wrappedObject.AY_QueryClaimReasonCode, wrapperEmpty.ReasonCode);
			AssertEquals("wrapperEmpty.ReasonDescription", wrappedObject.Lookups.ClaimReason.GetDescriptionFromCode(wrappedObject.AY_QueryClaimReasonCode), wrapperEmpty.ReasonDescription);
			AssertEquals("wrapperEmpty.StatusCode", wrappedObject.AY_QueryClaimStatus, wrapperEmpty.StatusCode);
			AssertEquals("wrapperEmpty.StatusCode", wrappedObject.Lookups.ClaimStatus.GetDescriptionFromCode(wrappedObject.AY_QueryClaimStatus), wrapperEmpty.StatusDescription);
			AssertEquals("wrapperEmpty.Details", ZString.Empty, wrapperEmpty.Details);
			AssertEquals("wrapperEmpty.NextFollowUp", ZDateTime.Now.AddDays(7).Date, wrapperEmpty.NextFollowUp.Date);
		}

		public void TestWrapperMappingFull()
		{
			AccTransactionHeader transactionHeader = Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader;
			transactionHeader.AH_RX_NKTransactionCurrency = "AUD";
			transactionHeader.AH_TransactionNum = "00001026";
			transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			QueryClaim.AY_AH = transactionHeader.PK;
			OrgContact contactOrg = Factory.New<OrgContact>();
			contactOrg.OC_ContactName = "STEVEN MCFAKE";
			ContactWrapper contact = new ContactWrapper(contactOrg, Factory);
			QueryClaim.AY_OC = contactOrg.PK;

			QueryClaim.AY_QueryClaimAmount = 58.25;
			QueryClaim.AY_QueryClaimReference = "2468";
			QueryClaim.AY_ShortDescriptionOfClaim = "THIS IS VERY DESCRIPTIVE";
			QueryClaim.AY_QueryClaimType = "CLF";
			QueryClaim.AY_QueryClaimReasonCode = "RAT";
			QueryClaim.AY_QueryClaimStatus = "OPN";
			QueryClaim.Details = "THIS IS VERY DETAILED";
			QueryClaim.AY_QueryClaimNextFollowUp = new ZDateTime(2011, 1, 4);
			GlbBranch someBranch = Factory.New<GlbBranch>();
			someBranch.GB_Code = "SYD";
			QueryClaim.AY_GB = someBranch.PK;

			QueryClaimWrapper wrapper = new QueryClaimWrapper(queryClaim, Factory);
			AssertEquals("wrapper.Contact", "STEVEN MCFAKE", wrapper.Contact.FullName);
			AssertEquals("wrapper.TransactionCurrency", "AUD", wrapper.TransactionCurrency.RX_Code);
			AssertEquals("wrapper.TransactionAmount", new ZDecimal(58.25), wrapper.TransactionAmount.Amount);
			AssertEquals("wrapper.JobNumber", "2468", wrapper.JobNumber);
			AssertEquals("wrapper.InvoiceNo", "00001026", wrapper.InvoiceNo);
			AssertEquals("wrapper.ShortDescription", "THIS IS VERY DESCRIPTIVE", wrapper.ShortDescription);
			AssertEquals("wrapper.TypeCode", "CLF", wrapper.TypeCode);
			AssertEquals("wrapper.TypeDescription", "Claim full refund", wrapper.TypeDescription);
			AssertEquals("wrapper.ReasonCode", "RAT", wrapper.ReasonCode);
			AssertEquals("wrapper.ReasonDescription", "Rating problem", wrapper.ReasonDescription);
			AssertEquals("wrapper.StatusCode", "OPN", wrapper.StatusCode);
			AssertEquals("wrapper.StatusDescription", "Added, pending evaluation", wrapper.StatusDescription);
			AssertEquals("wrapper.Details", "THIS IS VERY DETAILED", wrapper.Details);
			AssertEquals("wrapper.NextFollowUp", new ZDateTime(2011, 1, 4), wrapper.NextFollowUp);
			AssertEquals("wrapper.BranchCode", "SYD", wrapper.BranchCode);
		}

		protected AccQueryClaim QueryClaim
		{
			get { return queryClaim ?? (queryClaim = GetQueryClaim()); }
		}
		AccQueryClaim queryClaim;
		protected abstract AccQueryClaim GetQueryClaim();

		public abstract void TestAccountType();

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"Contact : 
Registry : (No Default Field Value Available on Registry)
TransactionAmount : 
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new QueryClaimWrapper(Factory.New<AccQueryClaim>(), Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Query Claim                                 (Default Field: JobNumber)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Contact                                 Contact
TransactionAmount                       Money
AccountType                             String
Amount                                  String
AssignedTo                              String
BranchCode                              String
Creator                                 String
Details                                 String
InvoiceNo                               String
JobNumber                               String
NextFollowUp                            DateTime
ReasonCode                              String
ReasonDescription                       String
ShortDescription                        String
StatusCode                              String
StatusDescription                       String
TypeCode                                String
TypeDescription                         String
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new QueryClaimWrapper(Factory.New<AccQueryClaim>(), Factory);
		}
	}
}
