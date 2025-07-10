using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Accounting.Business.MatchEPaymentRecipientsFilterBusinessObject;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(MatchEPaymentRecipientsFilterBusinessObject))]
	public class MatchEPaymentRecipientsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestWithoutFilteringShowAll()
		{
			var query = TestMatchFilterBizO.Filter;
			var result = new AccEPaymentBeneficiaryCollection(Factory, query);
			result.Load();

			AssertEquals("Expect all beneficiaries", 3, result.Count);
		}

		public void TestRecipientNameFiltering()
		{
			var recipientNameFilter = (ModuleTextFilter)TestMatchFilterBizO["Recipient Name"];
			recipientNameFilter.IsActive = true;

			recipientNameFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			recipientNameFilter.Property = "P";

			var result = new AccEPaymentBeneficiaryCollection(Factory, TestMatchFilterBizO.Filter);
			result.Load();
			AssertEquals(1, result.Count);
			Assert(result.Contains(receipt1.PK));

			recipientNameFilter.Property = "J";
			result = new AccEPaymentBeneficiaryCollection(Factory, TestMatchFilterBizO.Filter);
			result.Load();
			AssertEquals(2, result.Count);
			Assert(result.Contains(receipt2.PK));
			Assert(result.Contains(receipt3.PK));
		}

		public void TestNickNameFiltering()
		{
			var nickNameFilter = (ModuleTextFilter)TestMatchFilterBizO["Nickname"];
			nickNameFilter.IsActive = true;

			nickNameFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			nickNameFilter.Property = "W";

			var result = new AccEPaymentBeneficiaryCollection(Factory, TestMatchFilterBizO.Filter);
			result.Load();
			AssertEquals(2, result.Count);
			Assert(result.Contains(receipt1.PK));
			Assert(result.Contains(receipt3.PK));

			nickNameFilter.Property = "S";
			result = new AccEPaymentBeneficiaryCollection(Factory, TestMatchFilterBizO.Filter);
			result.Load();
			AssertEquals(1, result.Count);
			Assert(result.Contains(receipt2.PK));
		}

		public void TestCurrencyFiltering()
		{
			var currencyFilter = (ModuleNkFilter)TestMatchFilterBizO["Currency"];
			currencyFilter.IsActive = true;
			currencyFilter.Property = "AUD";

			var result = new AccEPaymentBeneficiaryCollection(Factory, TestMatchFilterBizO.Filter);
			result.Load();
			AssertEquals(2, result.Count);
			Assert(result.Contains(receipt1.PK));
			Assert(result.Contains(receipt2.PK));

			currencyFilter.Property = "USD";
			result = new AccEPaymentBeneficiaryCollection(Factory, TestMatchFilterBizO.Filter);
			result.Load();
			AssertEquals(1, result.Count);
			Assert(result.Contains(receipt3.PK));
		}

		public void TestCountryFiltering()
		{
			var countryFilter = (ModuleNkFilter)TestMatchFilterBizO["Country"];
			countryFilter.IsActive = true;
			countryFilter.Property = "AU";

			var result = new AccEPaymentBeneficiaryCollection(Factory, TestMatchFilterBizO.Filter);
			result.Load();
			AssertEquals(1, result.Count);
			Assert(result.Contains(receipt1.PK));

			countryFilter.Property = "US";
			result = new AccEPaymentBeneficiaryCollection(Factory, TestMatchFilterBizO.Filter);
			result.Load();
			AssertEquals(2, result.Count);
			Assert(result.Contains(receipt2.PK));
			Assert(result.Contains(receipt3.PK));
		}

		public void TestLastUpdatedFiltering()
		{
			var lastUpdatedFilter = (ModuleDateFilter)TestMatchFilterBizO["Last Updated"];
			lastUpdatedFilter.IsActive = true;
			lastUpdatedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			lastUpdatedFilter.Property1 = ZDateTime.Today.AddDays(-1);
			lastUpdatedFilter.Property2 = ZDateTime.Today.AddDays(1);

			var result = new AccEPaymentBeneficiaryCollection(Factory, TestMatchFilterBizO.Filter);
			result.Load();
			AssertEquals(3, result.Count);
			Assert(result.Contains(receipt1.PK));
			Assert(result.Contains(receipt2.PK));
			Assert(result.Contains(receipt3.PK));

			lastUpdatedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			lastUpdatedFilter.Property1 = ZDateTime.Today.AddDays(-3);
			lastUpdatedFilter.Property2 = ZDateTime.Today.AddDays(-2);
			result = new AccEPaymentBeneficiaryCollection(Factory, TestMatchFilterBizO.Filter);
			result.Load();
			AssertEquals(0, result.Count);
		}

		public void TestMatchedReceiptFiltering()
		{
			var matchedFilter = (ModuleTextFilter)TestMatchFilterBizO["Matched/Unmatched Recipients"];
			matchedFilter.IsActive = true;
			matchedFilter.Property = MatchRecipientsStatusTypes.MatchedOnly;

			var result = new AccEPaymentBeneficiaryCollection(Factory, TestMatchFilterBizO.Filter);
			result.Load();
			AssertEquals(1, result.Count);
			Assert(result.Contains(receipt1.PK));

			matchedFilter.Property = MatchRecipientsStatusTypes.UnmatchedOnly;
			result = new AccEPaymentBeneficiaryCollection(Factory, TestMatchFilterBizO.Filter);
			result.Load();
			AssertEquals(2, result.Count);
			Assert(result.Contains(receipt2.PK));
			Assert(result.Contains(receipt3.PK));

			matchedFilter.Property = MatchRecipientsStatusTypes.All;
			result = new AccEPaymentBeneficiaryCollection(Factory, TestMatchFilterBizO.Filter);
			result.Load();
			AssertEquals(3, result.Count);
			Assert(result.Contains(receipt1.PK));
			Assert(result.Contains(receipt2.PK));
			Assert(result.Contains(receipt3.PK));
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new MatchEPaymentRecipientsFilterBusinessObject(MatchEPaymentRecipients);
		}

		protected virtual MatchEPaymentRecipients CreateMatchEPaymentRecipients()
		{
			return new MatchEPaymentRecipients(Factory);
		}

		AccEPaymentBeneficiary receipt1, receipt2, receipt3;
		protected override void SetUp()
		{
			base.SetUp();
			MatchEPaymentRecipients = CreateMatchEPaymentRecipients();

			receipt1 = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			receipt1.ABF_BeneficiaryFullName = "Peter Walter";
			receipt1.ABF_BeneficiaryNickName = "Walter";
			receipt1.ABF_RX_NKAccountCurrency = "AUD";
			receipt1.ABF_RN_NKCountryCode = "AU";
			receipt1.ABF_GC_Company = GlbCompany.CurrentCompany.PK;

			receipt2 = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			receipt2.ABF_BeneficiaryFullName = "John Smith";
			receipt2.ABF_BeneficiaryNickName = "Smith";
			receipt2.ABF_RX_NKAccountCurrency = "AUD";
			receipt2.ABF_RN_NKCountryCode = "US";
			receipt2.ABF_GC_Company = GlbCompany.CurrentCompany.PK;

			receipt3 = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			receipt3.ABF_BeneficiaryFullName = "Jane White";
			receipt3.ABF_BeneficiaryNickName = "White";
			receipt3.ABF_RX_NKAccountCurrency = "USD";
			receipt3.ABF_RN_NKCountryCode = "US";
			receipt3.ABF_GC_Company = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var accountDetails = Factory.NewWithValidTestData<AccAPAccountDetails>();
			accountDetails.A1_PaymentMethod = "EPO";
			accountDetails.A1_EPaymentBeneficiaryId = receipt1.PK;
			Factory.Save();
		}

		protected MatchEPaymentRecipientsFilterBusinessObject TestMatchFilterBizO
		{
			get
			{
				if (fMatchEPaymentRecipientsFilterBizO == null)
				{
					fMatchEPaymentRecipientsFilterBizO = (MatchEPaymentRecipientsFilterBusinessObject)GetNewFilterStripBusinessObject();
				}
				return fMatchEPaymentRecipientsFilterBizO;
			}
		}
		MatchEPaymentRecipientsFilterBusinessObject fMatchEPaymentRecipientsFilterBizO;

		MatchEPaymentRecipients MatchEPaymentRecipients;

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		#endregion
	}
}
