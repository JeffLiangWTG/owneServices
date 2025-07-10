using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class UPECommercialQueueValidationHelperTest : UPEProcessQueueValidationHelperTestCase
	{
		public void TestValidateQueueName_MovingFromQueueToQueue_FromHoldQueue()
		{
			ValidationHelperForUPEProcessQueue.Queue.QueueName = DefaultQueueCodeDescriptionPairList.Codes.Hold;
			AssertNoErrors("No errors when the queue hasn't been saved yet", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			Factory.Save();
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Hold;
			AssertNoErrors("Hold->Hold allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			AssertNoErrors("Hold->Rebill allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.AR;
			AssertNoErrors("Hold->AR allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Chase;
			AssertHasErrors("Hold->Chase NOT allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.OnFile;
			AssertHasErrors("Hold->OnFile NOT allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			AssertHasErrors("Hold->Completed NOT allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
		}

		public void TestValidateQueueName_MovingFromQueueToQueue_FromEIRQueue()
		{
			ValidationHelperForUPEProcessQueue.Queue.QueueName = DefaultQueueCodeDescriptionPairList.Codes.Hold;
			AssertNoErrors("No errors when the queue hasn't been saved yet", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			Factory.Save();
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.EIR;
			AssertNoErrors("EIR->EIR allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Finance;
			AssertNoErrors("EIR->Finance allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			AssertNoErrors("EIR->Rebill allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Hold;
			AssertNoErrors("EIR->Hold allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Chase;
			AssertHasErrors("EIR->Chase NOT allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.OnFile;
			AssertHasErrors("EIR->OnFile NOT allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			AssertHasErrors("EIR->Completed NOT allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
		}

		public void TestValidateQueueName_MovingFromQueueToQueue_FromFinanceQueue()
		{
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Finance;
			AssertNoErrors("No errors when the queue hasn't been saved yet", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			Factory.Save();
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Finance;
			AssertNoErrors("Finance->Finance allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Hold;
			AssertNoErrors("Finance->Hold allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.EIR;
			AssertNoErrors("Finance->EIR allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			AssertNoErrors("Finance->Rebill allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.AR;
			AssertNoErrors("Finance->AR allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			AssertHasErrors("Finance->Completed NOT allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
		}

		public void TestValidateQueueName_MovingFromQueueToQueue_FromARQueue()
		{
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.AR;
			AssertNoErrors("No errors when the queue hasn't been saved yet", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			Factory.Save();
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.AR;
			AssertNoErrors("AR->AR allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			AssertNoErrors("AR->Rebill allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Hold;
			AssertHasErrors("AR->Hold NOT allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			AssertHasErrors("AR->Completed NOT allowed", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
		}

		public void TestValidateQueueName_MovingToCompletedQueueWithPaymentMethod()
		{
			ValidationHelperForUPEProcessQueue.Queue.QueueName = DefaultQueueCodeDescriptionPairList.Codes.Hold;
			AssertNoErrors("No errors when the queue hasn't been saved yet", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			Factory.Save();
			Callout.Payment.PaymentMethod = UPECargoPaymentMethod.None;
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			AssertHasErrors("Hold->Completed not allowed without a payment method", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			Callout.Payment.PaymentMethod = UPECargoPaymentMethod.CreditCard;
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			AssertNoErrors("Hold->Completed allowed when a payment method is specified", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
		}

		public void TestValidateQueueName_Rebill()
		{
			UPEOrgHeader uPEOrgHeader = Factory.NewWithValidTestData<UPEOrgHeader>();
			uPEOrgHeader.CompanyData.OB_OJ_ARDebtorGroup = Factory.New(typeof(OrgDebtorGroup)).PK;
			uPEOrgHeader.CompanyData.ARDebtorGroup.OJ_Code = "7";
			uPEOrgHeader.AccountNumber = "TEST";
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			ValidationHelperForUPEProcessQueue.Queue.Status = ReasonCodeDescriptionPairList.CRBL.Codes._R0_Rebill;
			ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8 = "TEST";
			Factory.Save();
			AssertHasErrors("Should be in AR Queue", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
		}

		public void TestValidateAccountNumber()
		{
			UPEOrgHeader organisation = Factory.New<UPEOrgHeader>();
			organisation.AccountNumber = "Account";
			ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8 = "Account";
			AssertNoErrors("No warnings for valid account number", ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8Info);
			ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8 = "Invalid";
			AssertHasErrors("Warning for invalid account number", ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8Info);
			ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8 = "";
			AssertNoErrors("No warning for empty account number", ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8Info);
		}

		public void TestValidateAccountNumber_MandatoryWhenInRebillQueue()
		{
			UPEOrgHeader organisation = Factory.NewWithValidTestData<UPEOrgHeader>();
			organisation.AccountNumber = "TEST";
			ValidationHelperForUPEProcessQueue.Queue.QueueName = DefaultQueueCodeDescriptionPairList.Codes.Hold;
			ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8 = "TEST";
			AssertNoErrors("No errors not in rebill queue rebill status", ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8Info);
			ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8 = "";
			AssertNoErrors("No errors not in rebill queue rebill status", ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8Info);
			ValidationHelperForUPEProcessQueue.Queue.QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8 = "TEST";
			AssertNoErrors("No errors not in rebill queue rebill status", ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8Info);
			ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8 = "";
			AssertNoErrors("No errors not in rebill queue rebill status", ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8Info);
			ValidationHelperForUPEProcessQueue.Queue.Status = ReasonCodeDescriptionPairList.CRBL.Codes._R1_Abandon;
			ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8 = "TEST";
			AssertNoErrors("No errors not in rebill queue rebill status", ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8Info);
			ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8 = "";
			AssertNoErrors("No errors not in rebill queue rebill status", ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8Info);
			ValidationHelperForUPEProcessQueue.Queue.Status = ReasonCodeDescriptionPairList.CRBL.Codes._R0_Rebill;
			ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8 = "TEST";
			AssertNoErrors("No errors when there is an account number and in rebill queue rebill status", ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8Info);
			ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8 = "";
			AssertHasErrors("Error account number required when in rebill queue rebill status", ValidationHelperForUPEProcessQueue.Queue.P4_CustomAttrib8Info);
		}

		protected override string[] QueueNamesWhichRequireReasonCode
		{
			get
			{
				return new string[] {// not Finance because the reason may not be populated to prevent sending BY to BISI twice
				CommercialQueueCodeDescriptionPairList.Codes.Hold, CommercialQueueCodeDescriptionPairList.Codes.EIR };
			}
		}

		protected override string[] ReasonCodesWhichRequireStatusCode
		{
			get
			{
				return System.Array.Empty<string>();
			}
		}

		protected override ProcessQueueType.Enum ExpectedQueueType
		{
			get
			{
				return ProcessQueueType.Enum.Commercial;
			}
		}

		protected override NonPersistentProcessQueue GetNewNonPersistentProcessQueue()
		{
			return new NonPersistentCalloutQueue(Factory);
		}

		protected override UPEProcessQueue GetNewUPEProcessQueue()
		{
			UPECalloutQueue result = Factory.New<UPECalloutQueue>();
			result.Parent = Callout;
			return result;
		}

		protected override UPEProcessQueueValidationHelper GetNewProcessQueueValidationHelperForUPEProcessQueue(UPEProcessQueue queue)
		{
			return new UPECommercialQueueValidationHelper((UPECalloutQueue)queue);
		}

		protected override UPEProcessQueueValidationHelper GetNewProcessQueueValidationHelperForNonPersistentProcessQueue(NonPersistentProcessQueue queue)
		{
			return new UPECommercialQueueValidationHelper((NonPersistentCalloutQueue)queue);
		}

		Callout Callout
		{
			get
			{
				if (fCallout == null)
				{
					fCallout = Factory.NewWithValidTestData<Callout>();
				}

				return fCallout;
			}
		}

		Callout fCallout;
	}
}
