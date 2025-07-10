using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED871Provider : IED871
	{
		public ED871Provider(ED871D message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly ED871D message;

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public IEMCSEvent ExciseMovement => exciseMovement ?? (exciseMovement = new ED871EventProvider(message.Body.ExplanationOnReasonForShortage.ExciseMovement));
		IEMCSEvent exciseMovement;

		public ZString GlobalExplanation => message.Body.ExplanationOnReasonForShortage.Analysis?.GlobalExplanation;

		public IReadOnlyCollection<IED871BodyAnalysis> Lines => lines ?? (lines = message.Body.ExplanationOnReasonForShortage.BodyAnalysis?.Select(x => new ED871BodyAnalysisProvider(x)).ToArray<IED871BodyAnalysis>() ?? Array.Empty<IED871BodyAnalysis>());
		IReadOnlyCollection<IED871BodyAnalysis> lines;
	}

	class ED871EventProvider : IEMCSEvent
	{
		public ED871EventProvider(ED871DBodyExplanationOnReasonForShortageExciseMovement exciseMovement)
		{
			this.exciseMovement = Argument.NotNull(exciseMovement, nameof(exciseMovement));
		}
		readonly ED871DBodyExplanationOnReasonForShortageExciseMovement exciseMovement;

		public ZString AdministrativeReferenceCode => exciseMovement.AdministrativeReferenceCode;

		public ZString SequenceNumber => exciseMovement.SequenceNumber;
	}

	class ED871BodyAnalysisProvider : IED871BodyAnalysis
	{
		public ED871BodyAnalysisProvider(ED871DBodyExplanationOnReasonForShortageBodyAnalysis line)
		{
			this.line = Argument.NotNull(line, nameof(line));
		}
		readonly ED871DBodyExplanationOnReasonForShortageBodyAnalysis line;

		public ZDecimal ActualQuantity => line.ActualQuantity;

		public ZString Explanation => line.Explanation;

		public ZString LineNumber => line.BodyRecordUniqueReference;

		public ZString ExciseProductCode => line.ExciseProductCode;
	}
}
