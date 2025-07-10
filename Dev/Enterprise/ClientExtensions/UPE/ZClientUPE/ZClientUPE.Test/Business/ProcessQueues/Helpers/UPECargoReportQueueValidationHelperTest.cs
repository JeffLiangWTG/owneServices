namespace Enterprise.Client.UPE.Business.Testing
{
	internal class UPECargoReportQueueValidationHelperTest : UPECustomsQueueValidationHelperTestCase
	{
		protected override string[] QueueNamesWhichRequireReasonCode
		{
			get
			{
				return new string[] { CargoReportQueueCodeDescriptionPairList.Codes.Hold, CargoReportQueueCodeDescriptionPairList.Codes.EIR, CargoReportQueueCodeDescriptionPairList.Codes.AwaitingEvaluation, CargoReportQueueCodeDescriptionPairList.Codes.Intervention, CargoReportQueueCodeDescriptionPairList.Codes.Quarantine };
			}
		}

		protected override string[] ReasonCodesWhichRequireStatusCode
		{
			get
			{
				return new string[] { ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient, ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing, ReasonCodeDescriptionPairList.Codes.XN_PhoneNumberInvalid };
			}
		}

		protected override NonPersistentProcessQueue GetNewNonPersistentProcessQueue()
		{
			return new NonPersistentCargoReportQueue(Factory);
		}

		protected override UPEProcessQueue GetNewUPEProcessQueue()
		{
			return Factory.New<UPECargoReportQueue>();
		}

		protected override UPEProcessQueueValidationHelper GetNewProcessQueueValidationHelperForUPEProcessQueue(UPEProcessQueue queue)
		{
			return new UPECargoReportQueueValidationHelper((UPECargoReportQueue)queue);
		}

		protected override UPEProcessQueueValidationHelper GetNewProcessQueueValidationHelperForNonPersistentProcessQueue(NonPersistentProcessQueue queue)
		{
			return new UPECargoReportQueueValidationHelper((NonPersistentCargoReportQueue)queue);
		}
	}
}
