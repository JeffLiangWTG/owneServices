using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM457;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM457Provider
	{
		public IM457Provider(Im457 xmlObject) => this.xmlObject = xmlObject;
		readonly Im457 xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ImportOperation?.Lrn;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZDateTime PresentationNotificationRegistrationDateAndTime => (xmlObject.ImportOperation?.PresentationNotificationRegistrationDateAndTime).ConvertToZDateTime();
	}
}
