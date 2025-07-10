using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie905;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE905Provider : IIE905
	{
		public IE905Provider(Ie905Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie905Type message;

		public ZString MrnNumber => AdministrativeReferenceCode;

		public ZString MrnNumberSequenceNumber => message.Body.StatusResponse.Attributes?.SequenceNumber;

		public ZString AdministrativeReferenceCode => message.Body.StatusResponse.Attributes?.AdministrativeReferenceCode;

		public ZString LastReceivedMessageType => message.Body.StatusResponse.Attributes?.LastReceivedMessageType.XmlEnumToString() ?? ZString.Empty;

		public ZString Status => message.Body.StatusResponse.Attributes?.Status.XmlEnumToString() ?? ZString.Empty;
	}
}
