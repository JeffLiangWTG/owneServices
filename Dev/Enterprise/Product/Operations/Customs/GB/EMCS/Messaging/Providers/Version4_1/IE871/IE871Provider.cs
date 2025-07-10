using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie871;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE871Provider : IIE871
	{
		public IE871Provider(Ie871Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie871Type message;

		public ZString MessageIdentifier => message.Header.MessageIdentifier;

		public IEMCSEvent ExciseMovementEad => exciseMovementEad ?? (exciseMovementEad = new IE871EventProvider(message.Body.ExplanationOnReasonForShortage.ExciseMovement));
		IEMCSEvent exciseMovementEad;

		public ZString GlobalExplanation => message.Body.ExplanationOnReasonForShortage.Analysis?.GlobalExplanation.Value;

		public IReadOnlyCollection<IIE871BodyAnalysis> Lines => lines ?? (lines = message.Body.ExplanationOnReasonForShortage.BodyAnalysis?.Select(x => new IE871BodyAnalysisProvider(x)).ToArray<IIE871BodyAnalysis>() ?? Array.Empty<IIE871BodyAnalysis>());
		IReadOnlyCollection<IIE871BodyAnalysis> lines;

		public ZString MrnNumber => message.Body.ExplanationOnReasonForShortage.ExciseMovement.AdministrativeReferenceCode;

		public ZString MrnNumberSequenceNumber => message.Body.ExplanationOnReasonForShortage.ExciseMovement.SequenceNumber;
	}

	sealed class IE871EventProvider : IEMCSEvent
	{
		public IE871EventProvider(ExciseMovementType exciseMovementEad)
		{
			this.exciseMovementEad = Argument.NotNull(exciseMovementEad, nameof(exciseMovementEad));
		}
		readonly ExciseMovementType exciseMovementEad;

		public ZString AdministrativeReferenceCode => exciseMovementEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => exciseMovementEad.SequenceNumber;
	}
}
