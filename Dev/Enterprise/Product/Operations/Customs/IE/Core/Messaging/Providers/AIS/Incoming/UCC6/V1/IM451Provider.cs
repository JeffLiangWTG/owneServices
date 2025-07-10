using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM451;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1
{
	public class IM451Provider : IIM451Provider
	{
		public IM451Provider(Im451 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im451 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration?.Mrn;

		public ZString LocalReferenceNumber => xmlObject.Declaration?.Lrn;

		public ZString AdditionalDeclarationType => xmlObject.Declaration?.AdditionalDeclarationType;

		public ZString DecisionReason => xmlObject.Declaration?.RejectionReason;

		public ZString PreferredPaymentMethod => xmlObject.Declaration?.PreferredPaymentMethod;

		public ZString Remarks => xmlObject.Declaration?.Remarks;

		public ControlResultV1Provider ControlResult => controlResult ?? (controlResult = new ControlResultV1Provider(xmlObject.Declaration.ControlResult));
		ControlResultV1Provider controlResult;
	}
}
