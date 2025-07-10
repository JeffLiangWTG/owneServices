using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC509C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC509CProvider
	{
		public CC509CProvider(Cc509C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc509C xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ExportOperation?.Lrn;
		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;
		public ZDateTime InvalidationDecisionDateAndTime => new ZDateTime(xmlObject.ExportOperation?.InvalidationDecisionDateAndTime);
		public ZDateTime InvalidationRequestDateAndTime => new ZDateTime(xmlObject.ExportOperation?.InvalidationRequestDateAndTime);
		public ZBool IsInvalidationInitiatedByCustoms => Extensions.IsTrueOrFalse(xmlObject.ExportOperation?.InvalidationInitiatedByCustoms);
		public ZString InvalidationJustification => xmlObject.ExportOperation?.InvalidationJustification;
	}
}
