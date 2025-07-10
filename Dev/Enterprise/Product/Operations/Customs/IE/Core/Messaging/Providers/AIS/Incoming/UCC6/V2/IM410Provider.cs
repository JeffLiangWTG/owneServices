using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM410;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM410Provider
	{
		readonly Im410 xmlObject;

		public IM410Provider(Im410 xmlObject)
		{
			this.xmlObject = xmlObject;
		}

		public ZString LocalReferenceNumber => xmlObject.ImportOperation?.Lrn;

		public ZString CustomsRegistrationNumber => xmlObject.ImportOperation?.CustomsRegistrationNumber;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZDateTime InvalidationDecisionDateAndTime => (xmlObject.ImportOperation?.InvalidationDecisionDateAndTime).ConvertToZDateTime();

		public ZDateTime InvalidationRequestDateAndTime => (xmlObject.ImportOperation?.InvalidationRequestDateAndTime).ConvertToZDateTime();

		public ZString InvalidationInitiatedByCustoms => xmlObject.ImportOperation?.InvalidationInitiatedByCustoms;

		public ZString InvalidationJustification => xmlObject.ImportOperation?.InvalidationJustification;
	}
}
