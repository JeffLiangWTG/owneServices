using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM451;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class IM451Provider
	{
		public IM451Provider(Im451 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im451 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration.Mrn;

		public ZString LocalReferenceNumber => xmlObject.Declaration.Lrn25;

		public ZString DeclarationType => xmlObject.Declaration.DeclarationType11;

		public ZString AdditionalDeclarationType => xmlObject.Declaration.AdditionalDeclarationType12;

		public ZString RejectionReason => xmlObject.Declaration.RejectionReason;

		public ZString PreferredPaymentMethod => xmlObject.Declaration.PreferredPaymentMethod48;

		public ZString Remarks => xmlObject.Declaration.Remarks;
	}
}
