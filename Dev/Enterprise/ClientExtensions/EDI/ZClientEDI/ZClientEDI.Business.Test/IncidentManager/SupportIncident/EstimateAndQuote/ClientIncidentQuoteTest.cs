using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(ClientIncidentQuote))]
	public class ClientIncidentQuoteTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2014, 6, 30)]
		public void TestQuoteSentDatePopulatesOtherDates()
		{
			EDIDataRegistry.Instance.FeatureRequestQuotationAutoExpirePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 15);
			ZDateTime now = DateTime.Now;
			var quote = Factory.New<ClientIncidentQuote>();
			quote.CIQ_QuoteSentDateUTC = now;
			AssertEquals(now.AddDays(15), quote.CIQ_QuoteExpiryDateUTC);
		}

		public void TestPaymentTypePopulatesPaymentTerms()
		{
			var quote = Factory.New<ClientIncidentQuote>();
			Assert("Precondition: blank terms", quote.CIQ_PaymentTerms.IsEmpty);
			quote.CIQ_Type = PaymentTypesAndTermsList.PaymentTypes.Codes.MonthlyType;
			AssertEquals(PaymentTypesAndTermsList.PaymentTerms.Codes.MonthlyTerm, quote.CIQ_PaymentTerms);
			quote.CIQ_Type = PaymentTypesAndTermsList.PaymentTypes.Codes.OneOffType;
			AssertEquals(PaymentTypesAndTermsList.PaymentTerms.Codes.OneOffTerm, quote.CIQ_PaymentTerms);
		}

		public void TestUniqueIndexViolation()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			var estimate1 = Factory.New<ClientIncidentQuote>();
			estimate1.CIQ_IM = incident.PK;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var estimate2 = otherFactory.New<ClientIncidentQuote>();
			estimate2.CIQ_IM = incident.PK;

			try
			{
				otherFactory.Save();
				Fail("Should throw");
			}
			catch (ZSaveException ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("While you have been working, another user has made changes. Please close and reopen the form again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
