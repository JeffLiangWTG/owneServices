using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie837;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE837Provider : IIE837
	{
		public IE837Provider(Ie837Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie837Type message;

		public IEMCSEvent ExciseMovement => exciseMovement ?? (exciseMovement = new IE837EMCSEventProvider(message.Body.ExplanationOnDelayForDelivery.ExciseMovement));
		IEMCSEvent exciseMovement;

		public ZString MrnNumber => AdministrativeReferenceCode;

		public ZString MrnNumberSequenceNumber => ExciseMovement.SequenceNumber;

		public ZString AdministrativeReferenceCode => ExciseMovement.AdministrativeReferenceCode;

		public ZDateTime DateAndTimeOfValidationOfExplanationOnDelay => message.Body.ExplanationOnDelayForDelivery.Attributes?.DateAndTimeOfValidationOfExplanationOnDelay ?? ZDateTime.Empty;

		public ZString ExplanationCode => message.Body.ExplanationOnDelayForDelivery.Attributes?.ExplanationCode;

		public ZString MessageRole => message.Body.ExplanationOnDelayForDelivery.Attributes?.MessageRole.XmlEnumToString() ?? ZString.Empty;

		public ZString SubmitterIdentification => message.Body.ExplanationOnDelayForDelivery.Attributes?.SubmitterIdentification;

		public ZString SubmitterType => message.Body.ExplanationOnDelayForDelivery.Attributes?.SubmitterType.XmlEnumToString() ?? ZString.Empty;

		public ZString ComplementaryInformation => message.Body.ExplanationOnDelayForDelivery.Attributes?.ComplementaryInformation?.Value ?? ZString.Empty;
	}

	sealed class IE837EMCSEventProvider : IEMCSEvent
	{
		public IE837EMCSEventProvider(ExciseMovementType exciseMovement)
		{
			this.exciseMovement = Argument.NotNull(exciseMovement, nameof(exciseMovement));
		}
		readonly ExciseMovementType exciseMovement;

		public ZString AdministrativeReferenceCode => exciseMovement.AdministrativeReferenceCode;

		public ZString SequenceNumber => exciseMovement.SequenceNumber;
	}
}
