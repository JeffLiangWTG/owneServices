using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM404;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1
{
	public class IM404Provider : IIM404Provider
	{
		public IM404Provider(Im404 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im404 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration?.Mrn;

		public ZDateTime AmendmentAcceptanceDateAndTime => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(xmlObject.Declaration?.AmendmentAcceptanceDate);

		public ZString PreferredPaymentMethod => xmlObject.Declaration?.PreferredPaymentMethod;

		public ZString Remarks => xmlObject.Declaration?.Remarks;

		public void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee) { }
	}
}
