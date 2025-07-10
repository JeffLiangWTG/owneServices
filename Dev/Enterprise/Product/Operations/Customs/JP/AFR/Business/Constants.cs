namespace Enterprise.Customs.JP.AFR.Business
{
	public static class Constants
	{
		public const string ContainerNoSeal = "NO SEAL";
		public const string JapanCustomsReceipientID = "JPCustoms";
	}

	public static class MessageDisplayConstants
	{
		public const string LineChange = "<br/>";
		public const string ShownBelow = "Shown below is a summary of relevant information received in the message.<br />";
		public const string AResponseMessage = "A response message has been received from Customs.<br />";
	}

	public static class AddInfoConstants
	{
		public static class Header
		{
			public const string VesselCallSign = "JPVesselCallSign";
			public const string CarrierCode = "JPCarrierCode";
			public const string PortOfLoadingSuffix = "JPPortOfLoadingSuffix";
			public const string PortOfDischargeSuffix = "JPPortOfDischargeSuffix";
			public const string IsDepartureFromRelaxedArea = "JPIsDepartureFromRelaxedArea";
			public const string InternalTransactionNumber = "JPInternalTransactionNumber";
			public const string MessageNumberPlaceHolder = "__(AFR MESSAGE NO)__";
			public const string VesselDetailsChanged = "JPVesselDetailsChanged";
			public const string OperationalCarrierVoyageNo = "JPOperationalCarrierVoyageNo";

			public const string CarrierCodeNew = "JPCarrierCodeNew";
			public const string VesselNameNew = "JPVesselNameNew";
			public const string VesselCallSignNew = "JPVesselCallSignNew";
			public const string VesselCountryNew = "JPVesselCountryNew";

			public const string VoyageNumberNew = "JPVoyageNumberNew";

			public const string OperationalCarrierVoyageNoNew = "JPOperationalCarrierVoyageNoNew";

			public const string PortOfLoadingSuffixNew = "JPPortOfLoadingSuffixNew";
			public const string PortOfLoadingCodeNew = "JPPortOfLoadingCodeNew";
			public const string PortOfLoadingNameNew = "JPPortOfLoadingNameNew";

			public const string IsDepartureFromRelaxedAreaNew = "JPIsDepartureFromRelaxedAreaNew";

			public const string EstimatedDateTimeOfDepartureNew = "JPEstimatedDateTimeOfDepartureNew";
			public const string BlanketChange = "JPBlanketChange";
		}

		public static class Bill
		{
			public const string HouseBillRegisterCompletion = "JPHouseBillRegisterCompletion";
			public const string Remarks = "Remarks";
			public const string NotificationForwardingPartyCode1 = "JPNotificationForwardingPartyCode1";
			public const string NotificationForwardingPartyCode2 = "JPNotificationForwardingPartyCode2";
			public const string NotificationForwardingPartyCode3 = "JPNotificationForwardingPartyCode3";
			// InBond Fields
			public const string TranshipmentArrivalPlaceCode = "JPTranshipmentArrivalPlaceCode";
			public const string TranshipmentArrivalPlaceName = "JPTranshipmentArrivalPlaceName";
			public const string TranshipmentEstimatedStartDate = "JPTranshipmentEstimatedStartDate";
			public const string TranshipmentEstimatedFinishDate = "JPTranshipmentEstimatedFinishDate";
			public const string PlaceOfDeliveryCode = "JPPlaceOfDeliveryCode";
			public const string PlaceOfDeliveryName = "JPPlaceOfDeliveryName";
			public const string TranshipmentDuration = "JPTranshipmentDuration";
			public const string TranshipmentReasonCode = "JPTranshipmentReasonCode";
			public const string TranshipmentTransportMode = "JPTranshipmentTransportMode";
			public const string OtherRelevantLawCode1 = "JPOtherRelevantLawCode1";
			public const string OtherRelevantLawCode2 = "JPOtherRelevantLawCode2";
			public const string OtherRelevantLawCode3 = "JPOtherRelevantLawCode3";
			public const string OtherRelevantLawCode4 = "JPOtherRelevantLawCode4";
			public const string OtherRelevantLawCode5 = "JPOtherRelevantLawCode5";
			// VOCC Fields
			public const string ContainerOperatorCode = "JPContainerOperatorCode";
			public const string GeneralCustomsTransitApprovalNumber = "JPGeneralCustomsTransitApprovalNumber";
			public const string MasterBillIdentifier = "JPMasterBillIdentifier";
			public const string MasterBillIdentifierValue = "M";
			// Delete Reason
			public const string DeleteReasonCode = "JPDeleteReasonCode";
			public const string DeleteReasonText = "JPDeleteReasonText";
			public const string SpecialCargoCode = "JPSPCCode";

			//BLL Function
			public const string BLLFunctionCode = "JPBLLFunctionCode";
			public const string BLLChangeReasonCode = "JPBLLChangeReasonCode";
			public const string BLLBillNumber = "BillNumber";
			public const string BLLOriginalBillNumbers = "Org";
			public const string BLLOriginalBillNumbersDescription = "Original Bill Numbers";
			public const string BLLNewBillNumbers = "New";
			public const string BLLNewBillNumbersDescription = "New Bill Numbers";
		}

		public static class Container
		{
			public const string ContainerOwnershipCode = "JPContainerOwnershipCode";
			public const string ContainerTypeOfService = "JPContainerTypeOfService";
			public const string ContainerVanningType = "JPContainerVanningType";
			public const string ContainerCCCApplicationId = "JPContainerCCCApplicationId";
			public const string ContainerSearchExclusionId = "JPContainerSearchExclusionId";
		}

		public const string True = "Y";
		public const string False = "N";
	}

	public static class JPPkgUnit
	{
		public const string Pieces = "PCS";
		public const string Tin = "TIN";
	}
}
