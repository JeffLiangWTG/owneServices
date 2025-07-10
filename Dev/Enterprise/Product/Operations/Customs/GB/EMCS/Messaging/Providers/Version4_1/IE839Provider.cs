using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie839;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE839Provider : IIE839
	{
		public IE839Provider(Ie839Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie839Type message;

		public ZString SendingCustomsOffice => message.Header.MessageSender;

		public ZDate IssuanceDate => new ZDate(message.Body.RefusalByCustoms.Attributes.DateAndTimeOfIssuance);

		public ZString MRN => message.Body.RefusalByCustoms.ExportDeclarationInformation?.DocumentReferenceNumber;

		public ZString LocalReferenceNumber => message.Body.RefusalByCustoms.ExportDeclarationInformation?.LocalReferenceNumber;

		public ZString MrnNumber => RejectedEads.Count > 1 ? ZString.Empty : RejectedEads.FirstOrDefault()?.AdministrativeReferenceCode ?? ZString.Empty;

		public ZString MrnNumberSequenceNumber => RejectedEads.Count > 1 ? ZString.Empty : RejectedEads.FirstOrDefault()?.SequenceNumber ?? ZString.Empty;

		public ZString RejectionReasonCode => message.Body.RefusalByCustoms.Rejection.RejectionReasonCode.XmlEnumToString();

		public IReadOnlyCollection<IEMCSEvent> RejectedEads => rejectedEads ?? (rejectedEads = message.Body.RefusalByCustoms.CEadVal.Select(x => new IE839EventProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSEvent> rejectedEads;
	}

	sealed class IE839EventProvider : IEMCSEvent
	{
		public IE839EventProvider(CEadValType rejectedEad)
		{
			this.rejectedEad = Argument.NotNull(rejectedEad, nameof(rejectedEad));
		}
		readonly CEadValType rejectedEad;

		public ZString AdministrativeReferenceCode => rejectedEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => rejectedEad.SequenceNumber;
	}
}
