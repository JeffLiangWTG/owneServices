using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(MatchEPaymentRecipients))]
	public class MatchEPaymentRecipientsTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_internal ?? (TestObjectCreator_internal = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_internal;

		protected override void SetUp()
		{
			base.SetUp();

			testMatchEPaymentRecipients_internalValue = (MatchEPaymentRecipients)GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = base.GetNewBusinessObject();
			result.Factory.RefreshEnabled = true;
			return result;
		}

		MatchEPaymentRecipients TestMatchEPaymentRecipients
		{
			get { return testMatchEPaymentRecipients_internalValue; }
		}
		protected MatchEPaymentRecipients testMatchEPaymentRecipients_internalValue;

		#endregion

		public void TestLoadFilteredReceipts()
		{
			var receipt1 = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			receipt1.ABF_BeneficiaryFullName = "Peter Walter";
			receipt1.ABF_BeneficiaryNickName = "Walter";
			receipt1.ABF_RX_NKAccountCurrency = "AUD";
			receipt1.ABF_RN_NKCountryCode = "AU";
			receipt1.ABF_GC_Company = GlbCompany.CurrentCompany.PK;

			var receipt2 = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			receipt2.ABF_BeneficiaryFullName = "John Smith";
			receipt2.ABF_BeneficiaryNickName = "Smith";
			receipt2.ABF_RX_NKAccountCurrency = "AUD";
			receipt2.ABF_RN_NKCountryCode = "US";
			receipt2.ABF_GC_Company = GlbCompany.CurrentCompany.PK;

			var receipt3 = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			receipt3.ABF_BeneficiaryFullName = "Jane White";
			receipt3.ABF_BeneficiaryNickName = "White";
			receipt3.ABF_RX_NKAccountCurrency = "USD";
			receipt3.ABF_RN_NKCountryCode = "US";
			receipt3.ABF_GC_Company = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var recipientNameFilter = (ModuleTextFilter)TestMatchEPaymentRecipients.RecipientsFilter["Recipient Name"];
			recipientNameFilter.IsActive = true;
			recipientNameFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			recipientNameFilter.Property = "P";

			TestMatchEPaymentRecipients.LoadFilteredRecipients();
			AssertEquals(1, TestMatchEPaymentRecipients.FilteredRecipients.Count);
			Assert(TestMatchEPaymentRecipients.FilteredRecipients.Contains(receipt1));
		}

		public void TestRetrieveLatestCreatedBeneficiaryRequest()
		{
			var request1 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request1.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			request1.ABR_SystemCreateTimeUtc = new ZDateTime(2021, 8, 15);

			var request2 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request2.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			request2.ABR_SystemCreateTimeUtc = new ZDateTime(2021, 8, 16);

			var request3 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request3.ABR_GC_Company = TestObjectCreator.NonCurrentCompany.PK;
			request3.ABR_SystemCreateTimeUtc = new ZDateTime(2021, 8, 18);

			Factory.Save();

			var latestRequest = TestMatchEPaymentRecipients.RetrieveLatestCreatedBeneficiaryRequest();
			AssertEquals(request2.PK, latestRequest.PK);
		}

		public void TestRetrieveLatestReceivedBeneficiaryRequest()
		{
			var request1 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request1.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			request1.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Received;
			request1.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 15);

			var request2 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request2.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			request2.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Received;
			request2.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 16);

			var request3 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request3.ABR_GC_Company = TestObjectCreator.NonCurrentCompany.PK;
			request3.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Received;
			request3.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 18);

			var request4 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request4.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			request4.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Error;
			request4.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 17);

			Factory.Save();

			var latestRequest = TestMatchEPaymentRecipients.RetrieveLatestReceivedBeneficiaryRequest();
			AssertEquals(request2.PK, latestRequest.PK);
		}

		public void TestPropertiesForDisplay()
		{
			var request1 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request1.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			request1.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Received;
			request1.ABR_SystemCreateTimeUtc = new ZDateTime(2021, 8, 15, 11, 0, 0);
			request1.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 15, 11, 0, 0);
			request1.ABR_SystemCreateUser = "AAA";

			var request2 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request2.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			request2.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Received;
			request2.ABR_SystemCreateTimeUtc = new ZDateTime(2021, 8, 16, 11, 0, 0);
			request2.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 16, 11, 0, 0);
			request2.ABR_SystemCreateUser = "BBB";

			var request3 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request3.ABR_GC_Company = TestObjectCreator.NonCurrentCompany.PK;
			request3.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Received;
			request3.ABR_SystemCreateTimeUtc = new ZDateTime(2021, 8, 18, 11, 0, 0);
			request3.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 18, 11, 0, 0);
			request3.ABR_SystemCreateUser = "CCC";

			var request4 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request4.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			request4.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Error;
			request4.ABR_ErrorDescription = "Some error text here";
			request4.ABR_SystemCreateTimeUtc = new ZDateTime(2021, 8, 17, 11, 0, 0);
			request4.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 17, 11, 0, 0);
			request4.ABR_SystemCreateUser = TestObjectCreator.Staff.GS_Code;

			Factory.Save();

			var accountDetails = Factory.NewWithValidTestData<AccAPAccountDetails>();
			accountDetails.A1_PaymentMethod = "EPO";
			Factory.Save();

			var matchEPaymentRecipients = new MatchEPaymentRecipients(Factory, accountDetails);
			AssertEquals("Some error text here", matchEPaymentRecipients.ErrorDescriptionForDisplay);
			AssertEquals("16 Aug 2021 21:00", matchEPaymentRecipients.RecipientListLastUpdatedDateForDisplay);
			AssertEquals("17 Aug 2021 21:00", matchEPaymentRecipients.LastRequestedDateForDisplay);
			AssertEquals("17 Aug 2021 21:00", matchEPaymentRecipients.LastResponseDateForDisplay);
			AssertEquals(EPaymentProviderCodes.Codes.OFX, matchEPaymentRecipients.ProviderCodeForDisplay);
			AssertEquals(TestObjectCreator.Staff.GS_FullName, matchEPaymentRecipients.RequestedByForDisplay);
			AssertEquals("Error", matchEPaymentRecipients.StatusForDisplay);
		}
	}
}
