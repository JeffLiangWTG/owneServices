using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED881Provider : IED881
	{
		public ED881Provider(ED881A message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly ED881A message;

		public ZString MessageSender => message.Header.MessageSender;

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public ZBool RequestAccepted => message.Body.ManualClosureResponse.Attributes.ManualClosureRequestAccepted == ED881ABodyManualClosureResponseAttributesManualClosureRequestAccepted.Item1;

		public ZString RejectionReason => message.Body.ManualClosureResponse.Attributes.ManualClosureRejectionReasonCode;

		public ZString RejectionComplement => message.Body.ManualClosureResponse.Attributes.ManualClosureRejectionComplement;

		public IEMCSEvent ResponseAttributes => responseAttributes ?? (responseAttributes = new ED881EventProvider(message.Body.ManualClosureResponse.Attributes));
		IEMCSEvent responseAttributes;
	}

	class ED881EventProvider : IEMCSEvent
	{
		public ED881EventProvider(ED881ABodyManualClosureResponseAttributes responseAttributes)
		{
			this.responseAttributes = Argument.NotNull(responseAttributes, nameof(responseAttributes));
		}
		readonly ED881ABodyManualClosureResponseAttributes responseAttributes;

		public ZString AdministrativeReferenceCode => responseAttributes.AdministrativeReferenceCode;

		public ZString SequenceNumber => responseAttributes.SequenceNumber;
	}
}
