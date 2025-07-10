using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM493;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM493Provider
	{
		public IM493Provider(Im493 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im493 xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ImportOperation?.Lrn;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;
	}
}
