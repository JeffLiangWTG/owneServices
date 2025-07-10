using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie881;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE881Provider : IIE881
	{
		public IE881Provider(Ie881Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie881Type message;

		public ZString MessageSender => message.Header.MessageSender;

		public ZString MessageRecepient => message.Header.MessageRecipient;

		public ZDateTime DateOfPreparation => message.Header.DateOfPreparation;

		public ZDateTime TimeOfPreparation => message.Header.TimeOfPreparation;

		public ZString MessageIdentifier => message.Header.MessageIdentifier;

		public ZString MrnNumber => message.Body.ManualClosureResponse.Attributes.AdministrativeReferenceCode;

		public ZString MrnNumberSequenceNumber => message.Body.ManualClosureResponse.Attributes.SequenceNumber;

		public IIE881ManualClosureResponse ManualClosureResponse => manualClosureResponse ?? (manualClosureResponse = IE881ManualClosureResponseProvider.NewOrNull(message.Body.ManualClosureResponse));
		IIE881ManualClosureResponse manualClosureResponse;
	}

	sealed class IE881EventProvider : IEMCSEvent
	{
		public IE881EventProvider(AttributesType attributes)
		{
			this.attributes = attributes;
		}
		readonly AttributesType attributes;

		public ZString AdministrativeReferenceCode => attributes.AdministrativeReferenceCode;

		public ZString SequenceNumber => attributes.SequenceNumber;
	}
}
