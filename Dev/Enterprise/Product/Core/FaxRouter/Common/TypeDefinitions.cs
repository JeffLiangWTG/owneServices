using System;

namespace Enterprise.FaxRouter.TypeDefinitions
{
	public enum DeliveryNotificationType
	{
		// Values are saved to DB - do not change.
		FAILED = 0,
		SUCCESS = 1,
		UNKNOWN = 2
	}

	public enum AcknowledgementParsedType
	{
		SUCCESS = 0,
		CCD_NOT_IN_EMAIL = 1,
		CCD_NOT_IN_EDIFAXDB = 2
	}

	public static class TypeTranslator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public static String DeliveryNotificationTypeToString(DeliveryNotificationType aType)
		{
			switch (aType)
			{
				case DeliveryNotificationType.SUCCESS:
					return "Success";
				case DeliveryNotificationType.FAILED:
					return "Failure";
				default:
					return "Unknown";
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public static String AcknowledgementParsedTypeToString(AcknowledgementParsedType aType, String aChargeCode)
		{
			switch (aType)
			{
				case AcknowledgementParsedType.SUCCESS:
					return " WWFax (charge code " + aChargeCode + ")";
				case AcknowledgementParsedType.CCD_NOT_IN_EMAIL:
					return " WWFax (EDI FaxRecipientId) charge code not found in ack mail";
				case AcknowledgementParsedType.CCD_NOT_IN_EDIFAXDB:
					return " WWFax (EDI FaxRecipientId) " + aChargeCode + " not found in EDIFaxDB";

				default:
					return "Unknown Ack Status code";
			}
		}
	}
}
