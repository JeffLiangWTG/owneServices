using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccEPaymentDealLookups;
using StatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccEPaymentDeal))]
	sealed class AccEPaymentDealTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIEPaymentLogParentMembers()
		{
			var deal = GetNewBusinessObject() as AccEPaymentDeal;
			Factory.Save();
			var dealAsIEPaymentLogParent = (IEPaymentLogParent)deal;
			var paymentApprovalPK = deal.Quote.PaymentApproval.PK;

			Assert("PaymentApprovalForLogging should be type PaymentApprovalBase", dealAsIEPaymentLogParent.PaymentApprovalForLogging is PaymentApprovalBase);
			AssertEquals(paymentApprovalPK, dealAsIEPaymentLogParent.PaymentApprovalForLogging.PK);
			AssertEquals(0, dealAsIEPaymentLogParent.Logs.GetAllLogs().Count);

			deal.Logs.AddNew(AutoEvents.InterchangeAcknowledged);
			AssertEquals(1, dealAsIEPaymentLogParent.Logs.GetAllLogs().Count);
		}

		public void TestIEPaymentDeliveryContextValueProviderMembers()
		{
			var deal = GetNewBusinessObject() as AccEPaymentDeal;
			Factory.Save();
			var dealAsIEPaymentDeliveryContextDataProvider = (IEPaymentDeliveryContextValueProvider)deal;

			AssertEquals($"E-Payment Deal {deal.AED_InternalReference} submitted for processing to {deal.AED_ProviderCode}", dealAsIEPaymentDeliveryContextDataProvider.Purpose);
			AssertEquals(typeof(AccEPaymentDeal), dealAsIEPaymentDeliveryContextDataProvider.EntityInfo.Type);
			AssertEquals(deal.PK, dealAsIEPaymentDeliveryContextDataProvider.EntityInfo.InternalPK);
		}

		public void TestCreatingUser()
		{
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_Code = "TST";
			Factory.Save();
			Assert("testUser is saved.", testUser.IsInDatabase);

			var deal = GetNewBusinessObject() as AccEPaymentDeal;
			deal.AED_SystemCreateUser = testUser.GS_Code;
			Factory.Save();
			Assert("deal is saved.", deal.IsInDatabase);

			var newFactory = new BusinessObjectFactory();
			var dealInNewFactory = newFactory.Load<AccEPaymentDeal>(deal.PK);
			AssertNotNull("Creating user should exist.", dealInNewFactory.CreatingUser);
			AssertEquals("Creating user should be testUser.", testUser.PK, dealInNewFactory.CreatingUser.PK);
		}

		public void TestCancelEPayment()
		{
			var deal = GetNewBusinessObject() as AccEPaymentDeal;
			deal.AED_Status = StatusCodes.Accepted;
			deal.AED_LastResponseReceivedUtc = ZDateTime.Now;
			deal.AED_ProviderReference = "5bd7eeeb-e356-4c92-a036-bc14d7e64ced";
			deal.CancelEPayment();

			AssertEquals(StatusCodes.Cancelled, deal.AED_Status);
			AssertEquals(ZDateTime.Empty, deal.AED_LastResponseReceivedUtc);
			AssertNullOrEmpty(deal.AED_ProviderReference);
		}

		public void TestConvertOFXDealStatusToCW1()
		{
			var mappings = new Dictionary<ZString, ZString>() {
								{ OFXDealStatus.Booked, StatusCodes.Accepted },
								{ OFXDealStatus.ReceivedNotCleared, StatusCodes.InProgress },
								{ OFXDealStatus.Received, StatusCodes.InProgress },
								{ OFXDealStatus.ReadyForPayment, StatusCodes.InProgress },
								{ OFXDealStatus.Paid, StatusCodes.Paid },
			};
			var deal = GetNewBusinessObject() as AccEPaymentDeal;
			foreach (var ofxStatus in mappings.Keys)
			{
				AssertEquals(mappings[ofxStatus], deal.ConvertOFXDealStatusToCW1(ofxStatus));
			}
			deal.AED_Status = StatusCodes.Accepted;
			AssertEquals(StatusCodes.Failed, deal.ConvertOFXDealStatusToCW1(OFXDealStatus.Error));
			deal.AED_Status = StatusCodes.Requested;
			AssertEquals(StatusCodes.Declined, deal.ConvertOFXDealStatusToCW1(OFXDealStatus.Error));
			deal.AED_Status = StatusCodes.Pending;
			try
			{
				deal.ConvertOFXDealStatusToCW1(OFXDealStatus.Error);
			}
			catch (InvalidOperationException ex)
			{
				AssertEquals("Cannot convert ofx error status to CW1 when the previous status is PEN", ex.Message);
			}
			try
			{
				deal.ConvertOFXDealStatusToCW1("Other");
			}
			catch (InvalidOperationException ex)
			{
				AssertEquals("Cannot convert ofx status 'Other' to CW1", ex.Message);
			}
		}

		public void TestHasProviderSentAResponse()
		{
			var deal = GetNewBusinessObject() as AccEPaymentDeal;
			var validResponseCodes = new ZString[] { StatusCodes.Accepted, StatusCodes.InProgress, StatusCodes.Paid, StatusCodes.Failed, StatusCodes.Declined };
			foreach (var code in deal.Lookups.StatusCodeList.GetAllCodes())
			{
				deal.AED_Status = code;
				AssertEquals(validResponseCodes.Contains(code), deal.HasProviderSentAResponse);
			}
		}

		public void TestFindActiveDealAleadyInDatabase_ActiveDealIsInDatabase_TryingToCreateAnotherActiveDeal()
		{
			var activeDealStatuses = new ZString[] { StatusCodes.Queued, StatusCodes.Pending, StatusCodes.ReadyToSend, StatusCodes.Requested, StatusCodes.Accepted, StatusCodes.InProgress, StatusCodes.Paid };
			AssertIfActiveDealExists(activeDealStatuses, activeDealStatuses);
		}

		public void TestFindActiveDealAleadyInDatabase_InactiveDealIsInDatabase_TryingToCreateActiveDeal()
		{
			var activeDealStatuses = new ZString[] { StatusCodes.Queued, StatusCodes.Pending, StatusCodes.ReadyToSend, StatusCodes.Requested, StatusCodes.Accepted, StatusCodes.InProgress, StatusCodes.Paid };
			var inactiveDealStatuses = new ZString[] { StatusCodes.Cancelled, StatusCodes.SubmissionFailed, StatusCodes.Declined, StatusCodes.Failed };
			AssertIfActiveDealExists(inactiveDealStatuses, activeDealStatuses);
		}

		public void TestFindActiveDealAleadyInDatabase_InactiveDealIsInDatabase_TryingToCreateAnotherInactiveDeal()
		{
			var inactiveDealStatuses = new ZString[] { StatusCodes.Cancelled, StatusCodes.SubmissionFailed, StatusCodes.Declined, StatusCodes.Failed };
			AssertIfActiveDealExists(inactiveDealStatuses, inactiveDealStatuses);
		}

		void AssertIfActiveDealExists(ZString[] statusesForDealAlreadyInDatabase, ZString[] statusesForNewDeal)
		{
			var deal1 = GetNewBusinessObject() as AccEPaymentDeal;
			foreach (var deal1status in statusesForDealAlreadyInDatabase)
			{
				deal1.AED_Status = deal1status;
				if (deal1.HasProviderSentAResponse)
				{
					deal1.AED_LastResponseReceivedUtc = ZDateTime.UtcNow.AddHours(2);
				}
				if (deal1.HasProviderSentAReference)
				{
					deal1.AED_ProviderReference = "OFX0001";
				}
				Factory.Save();

				var deal2 = GetNewBusinessObject() as AccEPaymentDeal;
				deal2.AED_QU_Quote = deal1.AED_QU_Quote;

				foreach (var deal2status in statusesForNewDeal)
				{
					deal2.AED_Status = deal2status;
					var devMessage = "Should " + (statusesForDealAlreadyInDatabase.Contains(StatusCodes.Queued) ? "not " : "") + $"allow deal with {deal2status} status when a deal with {deal1status} status already exist.";
					var expectedResult = statusesForDealAlreadyInDatabase.Contains(StatusCodes.Queued) ? deal1status : ZString.Empty;
					AssertEquals(devMessage, expectedResult, deal2.FindActiveDealAleadyInDatabase());
				}

				deal2.Delete();
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() => Factory.NewWithValidTestData<AccEPaymentDeal>();

		#endregion
	}
}
