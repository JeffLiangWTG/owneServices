using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class ShipmentStatusDataForTest : LineKeyForTest, IShipmentStatusData
	{
		#region IShipmentStatusData Members
		public ZString ShipmentStatus
		{
			get
			{
				return fShipmentStatus;
			}

			set
			{
				fShipmentStatus = value;
			}
		}

		public ZString HoldReasonCode
		{
			get
			{
				return fHoldReasonCode;
			}

			set
			{
				fHoldReasonCode = value;
			}
		}

		public ZBool InspectIndicator
		{
			get
			{
				return fInspectIndicator;
			}

			set
			{
				fInspectIndicator = value;
			}
		}

		public ZBool AddressCorrectionIndicator
		{
			get
			{
				return fAddressCorrectionIndicator;
			}

			set
			{
				fAddressCorrectionIndicator = value;
			}
		}

		public ZDateTime ImportReleaseDate
		{
			get
			{
				return fImportReleaseDate;
			}

			set
			{
				fImportReleaseDate = value;
			}
		}

		public ZString CustomsRefNo
		{
			get
			{
				return fCustomsRefNo;
			}

			set
			{
				fCustomsRefNo = value;
			}
		}

		public ZString BrokerCode
		{
			get
			{
				return fBrokerCode;
			}

			set
			{
				fBrokerCode = value;
			}
		}

		public ZString Remarks
		{
			get
			{
				return fRemarks;
			}

			set
			{
				fRemarks = value;
			}
		}

		public ZString ExceptionStatusCode
		{
			get
			{
				return fExceptionStatusCode;
			}

			set
			{
				fExceptionStatusCode = value;
			}
		}

		public ZString ExceptionResolutionCode
		{
			get
			{
				return fExceptionResolutionCode;
			}

			set
			{
				fExceptionResolutionCode = value;
			}
		}

		public ShipmentStatusType ShipmentStatusType
		{
			get
			{
				return fShipmentStatusType;
			}

			set
			{
				fShipmentStatusType = value;
			}
		}

		public ZDateTime StatusChangeDate
		{
			get
			{
				return fStatusChangeDate;
			}

			set
			{
				fStatusChangeDate = value;
			}
		}

		public bool HasReasonOrResolutionCode
		{
			get
			{
				return fHasReasonOrResolutionCode;
			}

			set
			{
				fHasReasonOrResolutionCode = value;
			}
		}

		ZString fShipmentStatus;
		ZString fHoldReasonCode;
		ZBool fInspectIndicator;
		ZBool fAddressCorrectionIndicator;
		ZDateTime fImportReleaseDate;
		ZString fCustomsRefNo;
		ZString fBrokerCode;
		ZString fRemarks;
		ZString fExceptionStatusCode;
		ZString fExceptionResolutionCode;
		ShipmentStatusType fShipmentStatusType;
		ZDateTime fStatusChangeDate;
		bool fHasReasonOrResolutionCode;
		#endregion
	}
}
