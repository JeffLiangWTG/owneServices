using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using StatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccEPaymentQuote))]
	public class AccEPaymentQuoteTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIEPaymentLogParentMembers()
		{
			var quote = GetNewBusinessObject() as AccEPaymentQuote;
			Factory.Save();
			var quoteAsIEPaymentLogParent = (IEPaymentLogParent)quote;
			var paymentApprovalPK = quote.PaymentApproval.PK;

			Assert("PaymentApprovalForLogging should be type PaymentApprovalBase", quoteAsIEPaymentLogParent.PaymentApprovalForLogging is PaymentApprovalBase);
			AssertEquals(paymentApprovalPK, quoteAsIEPaymentLogParent.PaymentApprovalForLogging.PK);
			AssertEquals(0, quoteAsIEPaymentLogParent.Logs.GetAllLogs().Count);

			quote.Logs.AddNew(AutoEvents.InterchangeAcknowledged);
			AssertEquals(1, quoteAsIEPaymentLogParent.Logs.GetAllLogs().Count);
		}

		public void TestIEPaymentDeliveryContextValueProviderMembers()
		{
			var quote = GetNewBusinessObject() as AccEPaymentQuote;
			Factory.Save();
			var quoteAsIEPaymentDeliveryContextDataProvider = (IEPaymentDeliveryContextValueProvider)quote;

			AssertEquals($"FX Quote Request {quote.QU_InternalReference} Sent to {quote.QU_ProviderCode}", quoteAsIEPaymentDeliveryContextDataProvider.Purpose);
			AssertEquals(GetExpectedBusinessObjectType(), quoteAsIEPaymentDeliveryContextDataProvider.EntityInfo.Type);
			AssertEquals(quote.PK, quoteAsIEPaymentDeliveryContextDataProvider.EntityInfo.InternalPK);
		}

		public void TestHasReceivedValidResponseFromProvider()
		{
			var quote = GetNewBusinessObject() as AccEPaymentQuote;
			var validResponseCodes = new[] { StatusCodes.Received, StatusCodes.Accepted, StatusCodes.Discarded, StatusCodes.Expired };
			foreach (var code in quote.Lookups.StatusCodeList.GetAllCodes())
			{
				quote.QU_Status = code;
				AssertEquals(validResponseCodes.Contains(code), quote.HasReceivedValidResponseFromProvider);
			}
		}

		public void TestCreatingUser()
		{
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_Code = "TST";
			Factory.Save();
			Assert("testUser is saved.", testUser.IsInDatabase);

			var quote = GetNewBusinessObject() as AccEPaymentQuote;
			quote.QU_SystemCreateUser = testUser.GS_Code;
			Factory.Save();
			Assert("quote is saved.", quote.IsInDatabase);

			var newFactory = new BusinessObjectFactory();
			var quoteInNewFactory = newFactory.Load<AccEPaymentQuote>(quote.PK);
			AssertNotNull("Creating user should exist.", quoteInNewFactory.CreatingUser);
			AssertEquals("Creating user should be testUser.", testUser.PK, quoteInNewFactory.CreatingUser.PK);
		}

		public void TestOrganisation()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_Code = "TST";
			var testPaymentApproval = Factory.NewWithValidTestData<AccPaymentApproval>();
			testPaymentApproval.AV_OH = testOrg.PK;

			var quote = GetNewBusinessObject() as AccEPaymentQuote;
			quote.QU_AV = testPaymentApproval.PK;

			AssertEquals("Quote Org should be payment's org.", testOrg.PK, quote.OrganisationPK);
			AssertEquals("Quote Org should be payment's org.", testOrg, quote.Organisation);
		}

		public void TestCurrencyAndExchangeRateFields()
		{
			var testQuote = Factory.New(GetExpectedBusinessObjectType()) as AccEPaymentQuote;
			var tester = new DecimalPlacesAttributeTester(testQuote);

			var list1 = new List<string>
			{
				nameof(testQuote.QU_FromAmount)
			};
			tester.CheckSetter(list1, nameof(testQuote.FromRXDecimals));

			var list2 = new List<string>
			{
				nameof(testQuote.QU_ToAmount)
			};
			tester.CheckSetter(list2, nameof(testQuote.ToRXDecimals));

			var list3 = new List<string>
			{
				nameof(testQuote.QU_FeeAmount)
			};
			tester.CheckSetter(list3, nameof(testQuote.FeeRXDecimals));

			var list4 = new List<string>
			{
				nameof(testQuote.QU_ExchangeRate),
				nameof(testQuote.QU_ExchangeRateInverted)
			};
			tester.CheckExchangeRate(list4, nameof(testQuote.ExchangeRateDecimals));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<AccEPaymentQuote>();
		}

		#endregion
	}
}
