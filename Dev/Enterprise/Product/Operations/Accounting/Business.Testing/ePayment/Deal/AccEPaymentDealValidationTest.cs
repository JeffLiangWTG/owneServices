using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using StatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;

namespace Enterprise.Accounting.Business.Testing
{
	sealed class AccEPaymentDealValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAED_GC_Company()
		{
			deal.AED_GC_Company = ZGuid.Empty;
			AssertHasError(deal.AED_GC_CompanyInfo, "Please enter a value.");
			deal.AED_GC_Company = ZGuid.NewZGuid();
			AssertHasError(deal.AED_GC_CompanyInfo, "Deal must specify a valid Company.");
			var company = Factory.New<GlbCompany>();
			deal.AED_GC_Company = company.PK;
			AssertNoErrors(deal.AED_GC_CompanyInfo);
		}

		public void TestCheckAED_QU_Quote()
		{
			deal.AED_QU_Quote = ZGuid.Empty;
			AssertHasError(deal.AED_QU_QuoteInfo, "Please enter a value.");
			deal.AED_QU_Quote = ZGuid.NewZGuid();
			AssertHasError(deal.AED_QU_QuoteInfo, "Deal must be attached to a valid Quote.");
			var quote = Factory.New<AccEPaymentQuote>();
			deal.AED_QU_Quote = quote.PK;
			AssertNoErrors(deal.AED_QU_QuoteInfo);
		}

		public void TestCheckAED_InternalReference()
		{
			deal.AED_InternalReference = ZString.Empty;
			AssertHasError(deal.AED_InternalReferenceInfo, "Please enter a value.");
			deal.AED_InternalReference = "00001002";
			AssertNoErrors(deal.AED_InternalReferenceInfo);
		}

		public void TestCheckAED_ProviderReference()
		{
			AssertWithStatusIndicatingValidProviderReferenceeReceived(() =>
			{
				deal.AED_ProviderReference = ZString.Empty;
				AssertHasError(deal.AED_ProviderReferenceInfo, "A Response has been received from the provider but no Reference has been entered.");
				deal.AED_ProviderReference = "AAA";
				AssertNoErrors(deal.AED_ProviderReferenceInfo);
			});

			AssertWithStatusNotIndicatingProviderReferenceReceived(() =>
			{
				deal.AED_ProviderReference = ZString.Empty;
				AssertNoErrors(deal.AED_ProviderReferenceInfo);
				deal.AED_ProviderReference = "AAA";
				AssertHasError(deal.AED_ProviderReferenceInfo, "A reference cannot be entered before a response is received from the provider.");
			});
		}

		void AssertWithStatusNotIndicatingProviderReferenceReceived(Action testToRun)
		{
			foreach (var code in (new[] { StatusCodes.Queued, StatusCodes.Pending, StatusCodes.ReadyToSend, StatusCodes.Requested, StatusCodes.Cancelled, StatusCodes.SubmissionFailed, StatusCodes.Declined }))
			{
				deal.AED_Status = code;
				deal.Validation.ValidateAll();
				testToRun();
			}
		}

		void AssertWithStatusIndicatingValidProviderReferenceeReceived(Action testToRun)
		{
			foreach (var code in (new[] { StatusCodes.Accepted, StatusCodes.InProgress, StatusCodes.Paid, StatusCodes.Failed }))
			{
				deal.AED_Status = code;
				deal.Validation.ValidateAll();
				testToRun();
			}
		}

		public void TestCheckQU_LastResponseReceivedUtc()
		{
			AssertWithStatusIndicatingResponseReceived(() =>
			{
				deal.AED_LastResponseReceivedUtc = ZDateTime.Empty;
				AssertHasError(deal.AED_LastResponseReceivedUtcInfo, "Response time must be recorded if a response from the provider has been received.");
				deal.AED_SystemCreateTimeUtc = ZDateTime.Today;
				deal.AED_LastResponseReceivedUtc = ZDateTime.Today.AddDays(-1);
				AssertHasError(deal.AED_LastResponseReceivedUtcInfo, "Response time cannot be earlier than the creation time.");

				deal.AED_LastResponseReceivedUtc = ZDateTime.Today;
				AssertNoErrors(deal.AED_LastResponseReceivedUtcInfo);
				deal.AED_LastResponseReceivedUtc = ZDateTime.Today.AddDays(1);
				AssertNoErrors(deal.AED_LastResponseReceivedUtcInfo);
			});

			AssertWithStatusNotIndicatingResponseReceived(() =>
			{
				deal.AED_LastResponseReceivedUtc = ZDateTime.Today.AddDays(1);
				AssertHasError(deal.AED_LastResponseReceivedUtcInfo, "Response Time cannot be entered before a response is received from the provider.");
				deal.AED_LastResponseReceivedUtc = ZDateTime.Empty;
				AssertNoErrors(deal.AED_LastResponseReceivedUtcInfo);
			});
		}

		void AssertWithStatusNotIndicatingResponseReceived(Action testToRun)
		{
			foreach (var code in (new[] { StatusCodes.Queued, StatusCodes.Pending, StatusCodes.ReadyToSend, StatusCodes.Requested, StatusCodes.Cancelled, StatusCodes.SubmissionFailed }))
			{
				deal.AED_Status = code;
				deal.Validation.ValidateAll();
				testToRun();
			}
		}

		void AssertWithStatusIndicatingResponseReceived(Action testToRun)
		{
			foreach (var code in (new[] { StatusCodes.Accepted, StatusCodes.InProgress, StatusCodes.Paid, StatusCodes.Failed, StatusCodes.Declined }))
			{
				deal.AED_Status = code;
				deal.Validation.ValidateAll();
				testToRun();
			}
		}

		public void TestCheckAED_ProviderCode()
		{
			deal.AED_ProviderCode = ZString.Empty;
			AssertHasError(deal.AED_ProviderCodeInfo, "Please enter a value.");
			deal.AED_ProviderCode = "AAA";
			AssertHasError(deal.AED_ProviderCodeInfo, "Enter a valid selection.");
			deal.AED_ProviderCode = EPaymentProviderCodes.Codes.OFX;
			AssertNoErrors(deal.AED_ProviderCodeInfo);
		}

		public void TestCheckAED_Status()
		{
			deal.AED_Status = ZString.Empty;
			AssertHasError(deal.AED_StatusInfo, "Please enter a value.");
			deal.AED_Status = "AAA";
			AssertHasError(deal.AED_StatusInfo, "Enter a valid selection.");

			deal.AED_Status = StatusCodes.Queued;
			AssertNoErrors(deal.AED_StatusInfo);
			Factory.Save();

			var deal1 = GetNewDeal();
			deal1.AED_InternalReference = "00001002";
			deal1.AED_QU_Quote = deal.AED_QU_Quote;
			deal1.AED_Status = StatusCodes.Queued;

			AssertHasError(deal1.AED_StatusInfo, $"Cannot create deal with QUE status when a deal with QUE status already exists for Quote {deal.Quote.QU_InternalReference}.");

			deal1.Delete();
			deal.AED_Status = StatusCodes.Cancelled;
			Factory.Save();

			var deal2 = GetNewDeal();
			deal2.AED_InternalReference = "00001003";
			deal2.AED_QU_Quote = deal.AED_QU_Quote;
			deal2.AED_Status = StatusCodes.Queued;

			AssertNoErrors(deal2.AED_StatusInfo);
		}

		public void TestCheckAED_ErrorDescription()
		{
			foreach (var code in deal.Lookups.StatusCodeList.GetAllCodes())
			{
				deal.AED_Status = code;
				deal.AED_ErrorDescription = ZString.Empty;
				AssertNoErrors(deal.AED_ErrorDescriptionInfo);

				deal.AED_ErrorDescription = "Heyo an error happened";
				if (code == StatusCodes.Failed || code == StatusCodes.SubmissionFailed || code == StatusCodes.Declined)
				{
					AssertNoErrors(deal.AED_ErrorDescriptionInfo);
				}
				else
				{
					AssertHasError(deal.AED_ErrorDescriptionInfo, "Error Description should only be recorded if the status is SMF, FAL or DEC.");
				}
			}
		}

		#region Implementation

		AccEPaymentDeal deal;

		AccEPaymentDeal GetNewDeal() => Factory.NewWithValidTestData<AccEPaymentDeal>();

		protected override void SetUp()
		{
			base.SetUp();
			deal = GetNewDeal();
		}

		#endregion
	}
}
