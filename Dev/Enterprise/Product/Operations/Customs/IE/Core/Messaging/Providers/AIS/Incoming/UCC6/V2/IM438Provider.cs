using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM438;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM438Provider
	{
		public IM438Provider(Im438 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im438 xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ImportOperation?.Lrn;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZDateTime RequestDate => (xmlObject.ReminderDetails?.RequestDate).ConvertToZDateTime();

		public ZDateTime ExpirationDate => (xmlObject.ReminderDetails?.ExpirationDate).ConvertToZDateTime();
	}
}
