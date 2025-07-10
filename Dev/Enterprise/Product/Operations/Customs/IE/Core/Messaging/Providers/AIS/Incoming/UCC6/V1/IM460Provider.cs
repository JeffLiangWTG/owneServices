using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM460;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1
{
	public class IM460Provider : IIM460Provider
	{
		public IM460Provider(Im460 xmlObject)
		{
			this.xmlObject = xmlObject;
		}

		readonly Im460 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration?.Mrn ?? ZString.Empty;

		public ZDateTime NotificationDate
		{
			get
			{
				_ = new ZString(xmlObject.Declaration?.ControlNotificationDate).TryParseToDate(out var notificationDate);
				return notificationDate;
			}
		}

		public ZDateTime TimeLimitForControl
		{
			get
			{
				_ = new ZString(xmlObject.Declaration?.TimeLimitForControl).TryParseToDate(out var limitDate);
				return limitDate;
			}
		}

		public ZString OverallControlTypeCode => xmlObject.OverallControlType?.ControlTypeCoded ?? ZString.Empty;
	}
}
