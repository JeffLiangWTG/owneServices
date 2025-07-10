namespace Enterprise.Client.UPE.Business.Testing
{
	internal class UPEDeclarationQueueValidationHelperTest : UPECustomsQueueValidationHelperTestCase
	{
		public void TestRequiresStatusCode_SubmittedQueue()
		{
			ValidateQueueName(DeclarationQueueCodeDescriptionPairList.Codes.Submitted);
			ValidateReasonCode(ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing);
			ValidateStatusCode("");
			AssertMandatoryValidationError(ValidationHelperForNonPersistentQueue.Queue.SubStatusInfo, true);
			AssertMandatoryValidationError(ValidationHelperForUPEProcessQueue.Queue.SubStatusInfo, true);
		}

		protected override string[] QueueNamesWhichRequireReasonCode
		{
			get
			{
				return new string[] { DeclarationQueueCodeDescriptionPairList.Codes.BCA, DeclarationQueueCodeDescriptionPairList.Codes.BCO, DeclarationQueueCodeDescriptionPairList.Codes.EIR, DeclarationQueueCodeDescriptionPairList.Codes.Classification, DeclarationQueueCodeDescriptionPairList.Codes.Lodgement, DeclarationQueueCodeDescriptionPairList.Codes.Submitted };
			}
		}

		protected override string[] ReasonCodesWhichRequireStatusCode
		{
			get
			{
				return new string[] { ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing, ReasonCodeDescriptionPairList.Codes.XN_PhoneNumberInvalid };
			}
		}

		protected override NonPersistentProcessQueue GetNewNonPersistentProcessQueue()
		{
			return new NonPersistentDeclarationQueue(Factory);
		}

		protected override UPEProcessQueue GetNewUPEProcessQueue()
		{
			return Factory.New<UPEDeclarationQueue>();
		}

		protected override UPEProcessQueueValidationHelper GetNewProcessQueueValidationHelperForUPEProcessQueue(UPEProcessQueue queue)
		{
			return new UPEDeclarationQueueValidationHelper((UPEDeclarationQueue)queue);
		}

		protected override UPEProcessQueueValidationHelper GetNewProcessQueueValidationHelperForNonPersistentProcessQueue(NonPersistentProcessQueue queue)
		{
			return new UPEDeclarationQueueValidationHelper((NonPersistentDeclarationQueue)queue);
		}
	}
}
